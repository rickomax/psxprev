using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Collada141;
using OpenTK;

namespace PSXPrev.Common.Exporters
{
    public class DAEExporter
    {
        private PNGExporter _pngExporter;
        private Dictionary<Texture, int> _exportedTextures;
        private ModelPreparerExporter _modelPreparer;
        private ExportModelOptions _options;
        private string _baseName;

        public void Export(ExportModelOptions options, RootEntity[] entities)
        {
            _options = options?.Clone() ?? new ExportModelOptions();
            // Force any required options for this format here, before calling Validate.
            // When testing in MeshLab, only the first mesh of a given image index will
            // use the correct image, all future meshes will just use images[0].
            _options.SingleTexture = true;
            _options.Validate("dae");

            _pngExporter = new PNGExporter();
            _exportedTextures = new Dictionary<Texture, int>();
            _modelPreparer = new ModelPreparerExporter(_options);

            // Prepare the shared state for all models being exported (mainly setting up tiled textures).
            var groups = _modelPreparer.PrepareAll(entities);

            for (var i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                // Prepare the state for the current model being exported.
                var preparedEntities = _modelPreparer.PrepareCurrent(entities, group, out var preparedModels);

                ExportEntities(i, group, preparedEntities, preparedModels);
            }

            //_pngExporter.Dispose();
            _pngExporter = null;
            _exportedTextures = null;
            _modelPreparer.Dispose();
            _modelPreparer = null;
        }

        private void ExportEntities(int index, Tuple<int, long> group, RootEntity[] entities, List<ModelEntity> models)
        {
            {
                // If shared, reuse the dictionary of textures so that we only export them once.
                if (!_options.ShareTextures)
                {
                    _exportedTextures.Clear();
                }

                _baseName = _options.GetBaseName(index);//, entityIndex);
                var filePath = Path.Combine(_options.Path, $"{_baseName}.dae");


                // Find and export all textures used by this entity
                var imagesDictionary = new Dictionary<Texture, int>();
                foreach (var model in models)
                {
                    var texture = model.Texture;
                    if (NeedsTexture(model) && !imagesDictionary.ContainsKey(texture))
                    {
                        imagesDictionary.Add(texture, imagesDictionary.Count);

                        if (!_exportedTextures.TryGetValue(texture, out var exportedTextureId))
                        {
                            exportedTextureId = _exportedTextures.Count;
                            _exportedTextures.Add(texture, exportedTextureId);

                            var textureName = _options.GetTextureName(_baseName, exportedTextureId);
                            _pngExporter.Export(texture, textureName, _options.Path);
                        }
                    }
                }


                // The schema always includes dates if we define an asset, so may as well give it the right one.
                var now = DateTime.Now; // Dates have no time zone info (and no Z for UTC).
                var asset = new asset
                {
                    contributor = new[]
                    {
                        new assetContributor
                        {
                            authoring_tool = "PSXPREV",
                        },
                    },
                    created = now,
                    modified = now,
                };
                var images = new library_images
                {
                    image = new image[imagesDictionary.Count],
                };
                var materials = new library_materials
                {
                    material = new material[imagesDictionary.Count + 1], // +1 for untextured (last index)
                };
                var effects = new library_effects
                {
                    effect = new effect[imagesDictionary.Count + 1], // +1 for untextured (last index)
                };
                var geometries = new library_geometries
                {
                    geometry = new geometry[models.Count],
                };
                const string visualSceneName = "visual-scene";
                var visualSceneNodes = new node[models.Count];
                var visualScenes = new library_visual_scenes
                {
                    visual_scene = new[]
                    {
                        new visual_scene
                        {
                            id = visualSceneName,
                            node = visualSceneNodes,
                        }
                    },
                };

                #region Images, Materials, Effects Library
                const string untexturedMaterialName = "matnone";
                const string untexturedEffectName = untexturedMaterialName + "fx";

                materials.material[imagesDictionary.Count] = new material
                {
                    id = untexturedMaterialName,
                    instance_effect = new instance_effect
                    {
                        url = $"#{untexturedEffectName}",
                    },
                };
                effects.effect[imagesDictionary.Count] = new effect
                {
                    id = untexturedEffectName,
                };

                foreach (var kvp in imagesDictionary)
                {
                    var texture = kvp.Key;
                    var imageId = kvp.Value;
                    var imageName = $"image{imageId}";

                    var exportedTextureId = _exportedTextures[texture];
                    var textureName = _options.GetTextureName(_baseName, exportedTextureId);

                    images.image[imageId] = new image
                    {
                        id = imageName,
                        Item = $"{textureName}.png",
                    };

                    var imageSurfaceName = $"{imageName}-surface";
                    var imageSamplerName = $"{imageName}-sampler";
                    var materialName = $"mat{imageId}";
                    var effectName = $"{materialName}fx";

                    materials.material[imageId] = new material
                    {
                        id = materialName,
                        instance_effect = new instance_effect
                        {
                            url = $"#{effectName}",
                        },
                    };

                    effects.effect[imageId] = new effect
                    {
                        id = effectName,
                        Items = new[]
                        {
                            new effectFx_profile_abstractProfile_COMMON
                            {
                                Items = new[]
                                {
                                    new common_newparam_type
                                    {
                                        sid = imageSurfaceName,
                                        ItemElementName = ItemChoiceType.surface,
                                        Item = new fx_surface_common
                                        {
                                            type = fx_surface_type_enum.Item2D,
                                            init_from = new[]
                                            {
                                                new fx_surface_init_from_common
                                                {
                                                    Value = imageName,
                                                }
                                            },
                                            format = "A8R8G8B8",
                                        },
                                    },
                                    new common_newparam_type
                                    {
                                        sid = imageSamplerName,
                                        ItemElementName = ItemChoiceType.sampler2D,
                                        Item = new fx_sampler2D_common
                                        {
                                            source = imageSurfaceName,
                                            minfilter = fx_sampler_filter_common.NEAREST,
                                            magfilter = fx_sampler_filter_common.NEAREST,
                                            wrap_s = fx_sampler_wrap_common.WRAP,
                                            wrap_t = fx_sampler_wrap_common.WRAP,
                                        },
                                    },
                                },
                                technique = new effectFx_profile_abstractProfile_COMMONTechnique
                                {
                                    sid = "common",
                                    Item = new effectFx_profile_abstractProfile_COMMONTechniqueBlinn
                                    {
                                        diffuse = new common_color_or_texture_type
                                        {
                                            Item = new common_color_or_texture_typeTexture
                                            {
                                                texture = imageSamplerName,
                                                texcoord = $"UVSET0",
                                            },
                                        },
                                    },
                                },
                            }
                        },
                    };
                }
                #endregion


                for (var i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    var modelName = $"mesh{i}";
                    var vertexCount = model.Triangles.Length * 3;
                    var needsTexture = NeedsTexture(model);
                    var normalInVertices = !_options.VertexIndexReuse;
                    var colorInVertices = !_options.VertexIndexReuse;

                    var materialName = untexturedMaterialName;
                    var imageId = -1;
                    if (needsTexture && imagesDictionary.TryGetValue(model.Texture, out imageId))
                    {
                        materialName = materials.material[imageId].id;
                    }
                    var materialInstanceName = materialName;

                    // Parameter names (like all names) are optional, save space and don't include them.
                    var acessorParams3d = new[]
                    {
                        new param { type = "float" }, // name = "X"
                        new param { type = "float" }, // name = "Y"
                        new param { type = "float" }, // name = "Z"
                    };
                    var acessorParamsUv = new[]
                    {
                        new param { type = "float" }, // name = "S"
                        new param { type = "float" }, // name = "T"
                    };

                    #region Processing
                    var triangleIndices = new StringBuilder();
                    var positionIndices = new Dictionary<Vector3, int>();
                    var normalIndices = new Dictionary<Vector3, int>();
                    var colorIndices = new Dictionary<Vector3, int>();
                    var uvIndices = new Dictionary<Vector2, int>();
                    var positionValues = new List<double>();
                    var normalValues = new List<double>();
                    var colorValues = new List<double>();
                    var uvValues = new List<double>();
                    var baseIndex = 0;
                    foreach (var triangle in model.Triangles)
                    {
                        for (var j = 2; j >= 0; j--, baseIndex++)
                        {
                            var vertex = triangle.Vertices[j];
                            var normal = triangle.Normals[j];
                            var color = triangle.Colors[j];
                            var uv = triangle.Uv[j];

                            // Position
                            if (!_options.VertexIndexReuse || !positionIndices.TryGetValue(vertex, out var positionIndex))
                            {
                                if (_options.VertexIndexReuse)
                                {
                                    positionIndex = positionIndices.Count;
                                    positionIndices.Add(vertex, positionIndex);
                                }
                                else
                                {
                                    positionIndex = baseIndex;
                                }

                                positionValues.Add(vertex.X);
                                positionValues.Add(-vertex.Y);
                                positionValues.Add(-vertex.Z);
                            }
                            triangleIndices.Append($"{positionIndex} ");

                            // Normal
                            if (!_options.VertexIndexReuse || !normalIndices.TryGetValue(normal, out var normalIndex))
                            {
                                if (_options.VertexIndexReuse)
                                {
                                    normalIndex = normalIndices.Count;
                                    normalIndices.Add(normal, normalIndex);
                                }
                                else
                                {
                                    normalIndex = baseIndex;
                                }

                                normalValues.Add(normal.X);
                                normalValues.Add(-normal.Y);
                                normalValues.Add(-normal.Z);
                            }
                            if (!normalInVertices)
                            {
                                triangleIndices.Append($"{normalIndex} ");
                            }

                            // Color
                            if (!_options.VertexIndexReuse || !colorIndices.TryGetValue((Vector3)color, out var colorIndex))
                            {
                                if (_options.VertexIndexReuse)
                                {
                                    colorIndex = colorIndices.Count;
                                    colorIndices.Add((Vector3)color, colorIndex);
                                }
                                else
                                {
                                    colorIndex = baseIndex;
                                }

                                colorValues.Add(color.R);
                                colorValues.Add(color.G);
                                colorValues.Add(color.B);
                            }
                            if (!colorInVertices)
                            {
                                triangleIndices.Append($"{colorIndex} ");
                            }

                            // UV
                            if (needsTexture)
                            {
                                if (!_options.VertexIndexReuse || !uvIndices.TryGetValue(uv, out var uvIndex))
                                {
                                    if (_options.VertexIndexReuse)
                                    {
                                        uvIndex = uvIndices.Count;
                                        uvIndices.Add(uv, uvIndex);
                                    }
                                    else
                                    {
                                        uvIndex = baseIndex;
                                    }

                                    uvValues.Add(uv.X);
                                    uvValues.Add(1f - uv.Y);
                                }
                                triangleIndices.Append($"{uvIndex} ");
                            }
                        }
                    }

                    #endregion

                    #region Position
                    var positionName = $"{modelName}-pos";
                    var positionArrayName = $"{positionName}-array";
                    var positionSource = new source
                    {
                        id = positionName,
                        Item = new float_array
                        {
                            id = positionArrayName,
                            count = (ulong)positionValues.Count,
                            Values = positionValues.ToArray(),
                        },
                        technique_common = new sourceTechnique_common
                        {
                            accessor = new accessor
                            {
                                count = (ulong)positionValues.Count / 3,
                                offset = 0,
                                source = $"#{positionArrayName}",
                                stride = 3,
                                param = acessorParams3d,
                            },
                        },
                    };
                    #endregion

                    #region Normal
                    var normalName = $"{modelName}-norm";
                    var normalArrayName = $"{normalName}-array";
                    var normalSource = new source
                    {
                        id = normalName,
                        Item = new float_array
                        {
                            id = normalArrayName,
                            count = (ulong)normalValues.Count,
                            Values = normalValues.ToArray(),
                        },
                        technique_common = new sourceTechnique_common
                        {
                            accessor = new accessor
                            {
                                count = (ulong)normalValues.Count / 3,
                                offset = 0,
                                source = $"#{normalArrayName}",
                                stride = 3,
                                param = acessorParams3d,
                            },
                        },
                    };
                    #endregion

                    #region Color
                    var colorName = $"{modelName}-color";
                    var colorArrayName = $"{colorName}-array";
                    var colorSource = new source
                    {
                        id = colorName,
                        Item = new float_array
                        {
                            id = colorArrayName,
                            count = (ulong)colorValues.Count,
                            Values = colorValues.ToArray(),
                        },
                        technique_common = new sourceTechnique_common
                        {
                            accessor = new accessor
                            {
                                count = (ulong)colorValues.Count / 3,
                                offset = 0,
                                source = $"#{colorArrayName}",
                                stride = 3,
                                param = acessorParams3d,
                            },
                        },
                    };
                    #endregion

                    #region UV
                    var uvName = $"{modelName}-uv";
                    var uvArrayName = $"{uvName}-array";
                    source uvSource = null;
                    if (needsTexture)
                    {
                        uvSource = new source
                        {
                            id = uvName,
                            Item = new float_array
                            {
                                id = uvArrayName,
                                count = (ulong)uvValues.Count,
                                Values = uvValues.ToArray(),
                            },
                            technique_common = new sourceTechnique_common
                            {
                                accessor = new accessor
                                {
                                    count = (ulong)uvValues.Count / 2,
                                    offset = 0,
                                    source = $"#{uvArrayName}",
                                    stride = 2,
                                    param = acessorParamsUv,
                                },
                            },
                        };
                    }
                    #endregion

                    #region Vertices
                    var verticesName = $"{modelName}-vert";

                    var sources = new List<source>();
                    sources.Add(positionSource);
                    sources.Add(normalSource);
                    sources.Add(colorSource);
                    if (needsTexture)
                    {
                        sources.Add(uvSource);
                    }

                    var verticesInputs = new List<InputLocal>();
                    verticesInputs.Add(new InputLocal
                    {
                        semantic = "POSITION",
                        source = $"#{positionName}",
                    });
                    if (normalInVertices)
                    {
                        verticesInputs.Add(new InputLocal
                        {
                            semantic = "NORMAL",
                            source = $"#{normalName}",
                        });
                    }
                    if (colorInVertices)
                    {
                        verticesInputs.Add(new InputLocal
                        {
                            semantic = "COLOR",
                            source = $"#{colorName}",
                        });
                    }

                    ulong inputsOffset = 0;
                    var trianglesInputs = new List<InputLocalOffset>();
                    trianglesInputs.Add(new InputLocalOffset
                    {
                        offset = inputsOffset++,
                        semantic = "VERTEX",
                        source = $"#{verticesName}",
                    });
                    if (!normalInVertices)
                    {
                        trianglesInputs.Add(new InputLocalOffset
                        {
                            offset = inputsOffset++,
                            semantic = "NORMAL",
                            source = $"#{normalName}",
                        });
                    }
                    if (!colorInVertices)
                    {
                        trianglesInputs.Add(new InputLocalOffset
                        {
                            offset = inputsOffset++,
                            semantic = "COLOR",
                            source = $"#{colorName}",
                        });
                    }
                    if (needsTexture)
                    {
                        trianglesInputs.Add(new InputLocalOffset
                        {
                            offset = inputsOffset++,
                            semantic = "TEXCOORD",
                            source = $"#{uvName}",
                        });
                    }
                    #endregion

                    #region Geometry
                    geometries.geometry[i] = new geometry
                    {
                        id = modelName,
                        Item = new mesh
                        {
                            source = sources.ToArray(),
                            vertices = new vertices
                            {
                                id = verticesName,
                                input = verticesInputs.ToArray(),
                            },
                            Items = new[]
                            {
                                new triangles
                                {
                                    count = (ulong)model.Triangles.Length,
                                    material = materialInstanceName,
                                    input = trianglesInputs.ToArray(),
                                    p = triangleIndices.ToString(),
                                }
                            },
                        },
                    };
                    #endregion

                    #region Visual Node
                    var nodeName = $"{modelName}-node";

                    var worldMatrix = model.WorldMatrix;
                    var translation = worldMatrix.ExtractTranslation();
                    var rotation = worldMatrix.ExtractRotationSafe();
                    var scale = worldMatrix.ExtractScale();
                    translation.Y *= -1f;
                    translation.Z *= -1f;
                    rotation.Y *= -1f;
                    rotation.Z *= -1f;
                    var matrix = Matrix4.CreateScale(scale) * Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(translation);

                    visualSceneNodes[i] = new node
                    {
                        id = nodeName,
                        Items = new object[]
                        {
                            new matrix
                            {
                                Values = new double[]
                                {
                                    matrix.M11, matrix.M21, matrix.M31, matrix.M41,
                                    matrix.M12, matrix.M22, matrix.M32, matrix.M42,
                                    matrix.M13, matrix.M23, matrix.M33, matrix.M43,
                                    matrix.M14, matrix.M24, matrix.M34, matrix.M44,
                                },
                            }
                        },
                        ItemsElementName = new[]
                        {
                            ItemsChoiceType2.matrix,
                        },
                        instance_geometry = new[]
                        {
                            new instance_geometry
                            {
                                url = $"#{modelName}",
                                bind_material = new bind_material
                                {
                                    technique_common = new instance_material[]
                                    {
                                        new instance_material
                                        {
                                            symbol = materialInstanceName,
                                            target = $"#{materialName}",
                                            bind_vertex_input = needsTexture
                                                ? new[]
                                                {
                                                    new instance_materialBind_vertex_input
                                                    {
                                                        semantic = $"UVSET0",
                                                        input_semantic = "TEXCOORD",
                                                        input_set = (ulong)0,
                                                        input_setSpecified = true,
                                                    }
                                                }
                                                : null,
                                        },
                                    },
                                },
                            }
                        },
                    };
                    #endregion
                }

                var colladaItems = new List<object>();
                if (_options.ExportTextures && imagesDictionary.Count > 0)
                {
                    colladaItems.Add(images);
                }
                colladaItems.Add(materials);
                colladaItems.Add(effects);
                colladaItems.Add(geometries);
                colladaItems.Add(visualScenes);

                var collada = new COLLADA
                {
                    asset = asset,
                    Items = colladaItems.ToArray(),
                    scene = new COLLADAScene
                    {
                        instance_visual_scene = new InstanceWithExtra
                        {
                            url = $"#{visualSceneName}",
                        },
                    },
                };

                // Optional handling to output cleaner float values.
                // This modifies the auto-generated schema, but the property can be safely removed if needed.
                var oldStrictFloatFormat = COLLADA.StrictFloatFormat;
                try
                {
                    COLLADA.StrictFloatFormat = _options.StrictFloatFormat;

                    using (var stream = File.Create(filePath))
                    {
                        var xmlWriter = new XmlTextWriter(stream, Encoding.UTF8)
                        {
                            Formatting = _options.ReadableFormat ? Formatting.Indented : Formatting.None,
                            Indentation = 1,
                            IndentChar = '\t',
                        };
                        var xmlSerializer = new XmlSerializer(typeof(COLLADA));
                        xmlSerializer.Serialize(xmlWriter, collada);
                    }
                }
                finally
                {
                    COLLADA.StrictFloatFormat = oldStrictFloatFormat;
                }
            }
        }

        private bool NeedsTexture(ModelEntity model)
        {
            return _options.ExportTextures && model.HasTexture;
        }
    }
}