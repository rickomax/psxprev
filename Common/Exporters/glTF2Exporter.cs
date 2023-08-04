using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OpenTK;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Exporters.glTF2Schema;

namespace PSXPrev.Common.Exporters
{
    // todo: Model is upside down as simply changing the vertices Y and Z as we do in the viewer is screwing up the results. Figuring out what is going on here
    public class glTF2Exporter
    {
        private glTF _root;
        private BinaryWriter _binaryWriter;
        private PNGExporter _pngExporter;
        private Dictionary<Texture, int> _exportedTextures;
        private ModelPreparerExporter _modelPreparer;
        private ExportModelOptions _options;
        private string _baseName;

        public void Export(ExportModelOptions options, RootEntity[] entities, Animation[] animations, AnimationBatch animationBatch)
        {
            _options = options?.Clone() ?? new ExportModelOptions();
            // Force any required options for this format here, before calling Validate.
            _options.MergeEntities = false; // Not supported yet
            _options.Validate("gltf");

            _pngExporter = new PNGExporter();
            _exportedTextures = new Dictionary<Texture, int>();
            _modelPreparer = new ModelPreparerExporter(_options);

            // Prepare the shared state for all models being exported (mainly setting up tiled textures).
            _modelPreparer.PrepareAll(entities);

            if (!_options.MergeEntities)
            {
                for (var i = 0; i < entities.Length; i++)
                {
                    // Prepare the state for the current model being exported.
                    _modelPreparer.PrepareCurrent(entities);

                    ExportEntities(i, new[] { entities[i] }, animations, animationBatch);
                }
            }
            else // Not supported yet, disabled before Validate
            {
                // Prepare the state for the current model being exported.
                _modelPreparer.PrepareCurrent(entities);

                ExportEntities(0, entities, animations, animationBatch);
            }

            _pngExporter = null;
            _exportedTextures = null;
            _modelPreparer.Dispose();
            _modelPreparer = null;
        }

        private void ExportEntities(int index, RootEntity[] entities, Animation[] animations, AnimationBatch animationBatch)
        {
            var exportAnimations = _options.ExportAnimations && animations?.Length > 0;

            for (var entityIndex = 0; entityIndex < entities.Length; entityIndex++)
            {
                // Re-use the dictionary of textures so that we only export them once.
                if (!_options.ShareTextures)
                {
                    _exportedTextures.Clear();
                }

                _baseName = _options.GetBaseName(index, entityIndex);
                var filePath = Path.Combine(_options.Path, $"{_baseName}.gltf");
                _root = new glTF();

                // Binary buffer creation
                var binaryBufferFileName = $"{_baseName}.bin";
                _binaryWriter = new BinaryWriter(File.Create(Path.Combine(_options.Path, $"{binaryBufferFileName}")));

                var entity = _modelPreparer.GetPreparedRootEntity(entities[entityIndex], out var models);


                // Write Asset
                // "asset": {
                //  "generator": "PSXPREV",
                //  "version": "2.0"
                // }
                _root.asset = new asset
                {
                    generator = "PSXPREV",
                    version = "2.0",
                };

                _root.extensionsUsed = new List<string>();


                // Write Scenes
                // "scene": 0,
                // "scenes": [
                //  {
                //   "nodes": [ {0...models.Count-1} ]
                //  }
                // ]
                _root.scene = 0;
                _root.scenes = new List<scene>(1)
                {
                    new scene
                    {
                        nodes = new List<int>(Enumerable.Range(0, models.Count)),
                    },
                };


                // Write Buffer Views
                // "bufferViews": [
                //  // MeshBufferViews
                //  {
                //   "buffer": 0,
                //   "target": Target_ArrayBuffer,
                //   "byteOffset": {_binaryWriter.BaseStream.Position - initialOffset},
                //   "byteLength": {_binaryWriter.BaseStream.Position - offset}
                //  },
                //  //...
                //  // AnimationTimeBufferView
                //  {
                //   "buffer": 0,
                //   "byteOffset": {_binaryWriter.BaseStream.Position - initialOffset},
                //   "byteLength": {_binaryWriter.BaseStream.Position - offset}
                //  },
                //  //...
                //  // AnimationDataBufferViews
                //  {
                //   "buffer": 0,
                //   "byteOffset": {_binaryWriter.BaseStream.Position - initialOffset},
                //   "byteLength": {_binaryWriter.BaseStream.Position - offset}
                //  },
                //  //...
                // ]
                var initialOffset = _binaryWriter.BaseStream.Position;
                var offset = initialOffset;
                _root.bufferViews = new List<bufferView>();
                {
                    _root.bufferViews.Capacity = models.Count * 4;
                    // Meshes
                    foreach (var model in models)
                    {
                        WriteMeshBufferViews(model, ref offset, initialOffset);
                    }
                }
                if (exportAnimations)
                {
                    _root.bufferViews.Capacity = _root.bufferViews.Count + animations.Length * (1 + models.Count * 3);
                    // Animations
                    foreach (var animation in animations)
                    {
                        var totalTime = animation.FrameCount / animation.FPS;
                        var timeStep = 1f / animation.FPS;
                        WriteAnimationTimeBufferView(totalTime, timeStep, ref offset, initialOffset);

                        // Compute animation frames
                        var totalFrames = (int)Math.Ceiling(totalTime / timeStep) + 1; // +1 to include last frame
                        var translations = new Vector3[models.Count, totalFrames];
                        var rotations = new Quaternion[models.Count, totalFrames];
                        var scales = new Vector3[models.Count, totalFrames];
                        var frame = 0;
                        var oldLoopMode = animationBatch.LoopMode;
                        animationBatch.SetupAnimationBatch(animation, simulate: true);
                        animationBatch.LoopMode = AnimationLoopMode.Once;
                        for (var t = 0f; t < totalTime; t += timeStep, frame++)
                        {
                            animationBatch.Time = t;
                            if (animationBatch.SetupAnimationFrame(null, entity, null, simulate: true))
                            {
                                for (var j = 0; j < models.Count; j++)
                                {
                                    var model = models[j];
                                    var matrix = model.TempMatrix * model.TempLocalMatrix;
                                    translations[j, frame] = matrix.ExtractTranslation();
                                    rotations[j, frame] = matrix.ExtractRotationSafe();
                                    scales[j, frame] = matrix.ExtractScale();
                                }
                            }
                            else
                            {
                                totalFrames = frame + 1;
                            }
                        }
                        animationBatch.LoopMode = oldLoopMode;

                        // Write animation frames for each model
                        for (var j = 0; j < models.Count; j++)
                        {
                            var model = models[j];
                            WriteAnimationDataBufferViews(model, j, translations, rotations, scales, totalFrames, ref offset, initialOffset);
                        }
                    }
                }

                // Write Textures
                var imagesDictionary = new Dictionary<Texture, int>();

                if (_options.ExportTextures)
                {
                    // Write Texture Samplers
                    // "samplers": [
                    //  {
                    //   "magFilter": {sampler_filter.NEAREST},
                    //   "minFilter": {sampler_filter.NEAREST},
                    //   "wrapS": {sampler_wrap.REPEAT},
                    //   "wrapT": {sampler_wrap.REPEAT}
                    //  },
                    //  //...
                    // ]
                    _root.samplers = new List<sampler>
                    {
                        new sampler
                        {
                            magFilter = sampler_filter.NEAREST,
                            minFilter = sampler_filter.NEAREST,
                            wrapS = sampler_wrap.REPEAT,
                            wrapT = sampler_wrap.REPEAT,
                        }
                    };

                    // Write Texture Images
                    // "images": [
                    //  {
                    //   "uri": "{textureName}.png"
                    //  },
                    //  //...
                    // ]
                    _root.images = new List<image>();
                    foreach (var model in models)
                    {
                        var texture = model.Texture;
                        if (NeedsTexture(model) && !imagesDictionary.ContainsKey(texture))
                        {
                            imagesDictionary.Add(texture, imagesDictionary.Count);

                            var exported = true;
                            if (!_exportedTextures.TryGetValue(texture, out var exportedTextureId))
                            {
                                exportedTextureId = _exportedTextures.Count;
                                _exportedTextures.Add(texture, exportedTextureId);
                                exported = false;
                            }

                            var textureName = _options.GetTextureName(_baseName, exportedTextureId);
                            if (!exported)
                            {
                                _pngExporter.Export(texture, textureName, _options.Path);
                            }

                            _root.images.Add(new image
                            {
                                uri = $"{textureName}.png",
                            });
                        }
                    }

                    // Write Texture Textures
                    // "textures": [
                    //  {
                    //   "source": {imageId},
                    //   "sampler": 0
                    //  },
                    //  //...
                    // ]
                    _root.textures = new List<texture>(imagesDictionary.Count);
                    for (var imageId = 0; imageId < imagesDictionary.Count; imageId++)
                    {
                        _root.textures.Add(new texture
                        {
                            source = imageId,
                            sampler = 0,
                        });
                    }
                }


                // Write Materials
                // "materials": [
                // {
                //  "pbrMetallicRoughness": {
                // * "baseColorTexture": { "index": {imageId} }, // if textured
                //   "metallicFactor": 0.0,
                //   "roughnessFactor": 1.0
                //  },
                //  "alphaMode": "MASK"|"OPAQUE",
                //  "doubleSided": true|false,
                // *"extensions": { "KHR_materials_unlit": {} } // if RenderFlags.Unlit
                // },
                // //...
                // ]
                _root.materials = new List<material>(models.Count);
                var anyUnlit = false;
                foreach (var model in models)
                {
                    var imageId = 0; // dummy init
                    var needsTexture = NeedsTexture(model) && imagesDictionary.TryGetValue(model.Texture, out imageId);
                    _root.materials.Add(new material
                    {
                        pbrMetallicRoughness =
                            new material_pbrMetallicRoughness
                            {
                                //baseColorFactor = new[] { 1f, 1f, 1f, 1f },
                                baseColorTexture = needsTexture
                                    ? new textureInfo
                                    {
                                        index = imageId,
                                    }
                                    : null,
                                metallicFactor = 0.0f,
                                roughnessFactor = 1.0f,
                            },
                        alphaMode = needsTexture ? material_alphaMode.MASK : material_alphaMode.OPAQUE, // Hide pixels with black mask transparency (when stp bit is false)
                        doubleSided = model.RenderFlags.HasFlag(RenderFlags.DoubleSided),
                        extensions = model.RenderFlags.HasFlag(RenderFlags.Unlit)
                            ? new Dictionary<string, object>
                            {
                                { glTF.ExtensionUnlit, new object() },
                            }
                            : null,
                    });
                    if (model.RenderFlags.HasFlag(RenderFlags.Unlit))
                    {
                        if (!anyUnlit)
                        {
                            anyUnlit = true;
                            _root.extensionsUsed.Add(glTF.ExtensionUnlit);
                        }
                    }
                }


                // Write Accessors
                // "accessors": [
                //  {
                //   "bufferView": {bufferViewIndex++},
                //   "byteOffset": 0,
                //   "componentType": {accessor_componentType.FLOAT},
                //   "count": {vertexCount},
                //   "type": "VEC3"|"VEC3"|"VEC3"|"VEC2", // VEC2 if textured
                // * "min": [ {minPos} ], // first VEC3 only
                // * "max": [ {maxPos} ]  // first VEC3 only
                //  },
                //  //...
                //  {
                //   "bufferView": {bufferViewIndex++},
                //   "byteOffset": 0,
                //   "componentType": {accessor_componentType.FLOAT},
                //   "count": {stepCount},
                //   "type": "SCALAR"|"VEC3"|"VEC4"|"VEC3",
                // * "min": [ {timeMin} ], // SCALAR only
                // * "max": [ {timeMax} ]  // SCALAR only
                //  },
                //  //...
                // ]
                var bufferViewIndex = 0;
                _root.accessors = new List<accessor>();

                // Write Accessor Meshes
                {
                    _root.accessors.Capacity = models.Count * 4;
                    foreach (var model in models)
                    {
                        var triangles = model.Triangles;
                        var vertexCount = triangles.Length * 3;

                        // Compute the local min/max vertex positions. We can't use Bounds3D because that's transformed.
                        var vertMin = triangles.Length > 0 ? FixHandiness(triangles[0].Vertices[0]) : Vector3.Zero;
                        var vertMax = vertMin;
                        foreach (var triangle in triangles)
                        {
                            for (var j = 0; j < 3; j++)
                            {
                                var vertex = FixHandiness(triangle.Vertices[j]);
                                vertMin = Vector3.ComponentMin(vertMin, vertex);
                                vertMax = Vector3.ComponentMax(vertMax, vertex);
                            }
                        }
                        var boundsMin = new[] { vertMin.X, vertMin.Y, vertMin.Z };
                        var boundsMax = new[] { vertMax.X, vertMax.Y, vertMax.Z };

                        WriteAccessor(bufferViewIndex++, 0, accessor_componentType.FLOAT, vertexCount, accessor_type.VEC3, boundsMin, boundsMax); // Vertex positions
                        WriteAccessor(bufferViewIndex++, 0, accessor_componentType.FLOAT, vertexCount, accessor_type.VEC3); // Vertex normals
                        WriteAccessor(bufferViewIndex++, 0, accessor_componentType.FLOAT, vertexCount, accessor_type.VEC3); // Vertex colors
                        if (NeedsTexture(model))
                        {
                            WriteAccessor(bufferViewIndex++, 0, accessor_componentType.FLOAT, vertexCount, accessor_type.VEC2); // Vertex uvs
                        }
                    }
                }
                // Write Accessor Animations
                if (exportAnimations)
                {
                    _root.accessors.Capacity = _root.accessors.Count + animations.Length * (1 + models.Count);
                    foreach (var animation in animations)
                    {
                        var totalTime = animation.FrameCount / animation.FPS;
                        var timeStep = 1f / animation.FPS;
                        var stepCount = (int)Math.Ceiling(totalTime / timeStep);
                        var timeMin = new [] { 0f };
                        var timeMax = new [] { totalTime };
                        WriteAccessor(bufferViewIndex++, 0, accessor_componentType.FLOAT, stepCount, accessor_type.SCALAR, timeMin, timeMax); // Frame times
                        foreach (var model in models)
                        {
                            WriteAccessor(bufferViewIndex++, 0, accessor_componentType.FLOAT, stepCount, accessor_type.VEC3); // Object translation
                            WriteAccessor(bufferViewIndex++, 0, accessor_componentType.FLOAT, stepCount, accessor_type.VEC4); // Object rotation
                            WriteAccessor(bufferViewIndex++, 0, accessor_componentType.FLOAT, stepCount, accessor_type.VEC3); // Object scale
                        }
                    }
                }


                var accessorIndex = 0;

                // Write Meshes
                // "meshes": [
                //  {
                //   "name": "{model.EntityName}",
                //   "primitives": [
                //    {
                //     "attributes": {
                //      "POSITION": {accessorIndex++},
                //      "NORMAL": {accessorIndex++},
                //      "COLOR_0": {accessorIndex++},
                // *    "TEXCOORD_0": {accessorIndex++} // if textered
                //     },
                //     "material": {i},
                //     "mode": {mesh_primitive_mode.TRIANGLES}
                //    }
                //   ]
                //  },
                //  //...
                // ]
                _root.meshes = new List<mesh>(models.Count);
                for (var i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    _root.meshes.Add(new mesh
                    {
                        name = model.EntityName,
                        primitives = new List<mesh_primitive>
                        {
                            new mesh_primitive
                            {
                                attributes = new mesh_primitive_attributes
                                {
                                    POSITION = accessorIndex++,
                                    NORMAL = accessorIndex++,
                                    COLOR_0 = accessorIndex++,
                                    TEXCOORD_0 = NeedsTexture(model)
                                        ? accessorIndex++
                                        : (int?)null,
                                },
                                material = i,
                                mode = mesh_primitive_mode.TRIANGLES,
                            },
                        },
                    });
                }


                // Write Animations 
                if (exportAnimations)
                {
                    // "animations": [
                    //  {
                    //   "samplers": [
                    //    {
                    //     "input": {timeAccessorIndex},
                    //     "interpolation": "LINEAR",
                    //     "output": {accessorIndex++}
                    //    },
                    //    //...
                    //   ],
                    //   "channels": [
                    //    {
                    //     "sampler": {animationSamplerIndex++},
                    //     "target": {
                    //      "node": {j},
                    //      "path": "translation"
                    //     }
                    //    },
                    //    {
                    //     "sampler": {animationSamplerIndex++},
                    //     "target": {
                    //      "node": {j},
                    //      "path": "rotation"
                    //     }
                    //    },
                    //    {
                    //     "sampler": {animationSamplerIndex++},
                    //     "target": {
                    //      "node": {j},
                    //      "path": "scale"
                    //     }
                    //    },
                    //    //...
                    //   ]
                    //  },
                    //  //...
                    // ]
                    _root.animations = new List<animation>(animations.Length);
                    foreach (var animation in animations)
                    {
                        var animationSamplerIndex = 0;
                        var timeAccessorIndex = accessorIndex++;

                        _root.animations.Add(new animation
                        {
                            samplers = new List<animation_sampler>(models.Count * 3),
                            channels = new List<animation_channel>(models.Count * 3),
                        });
                        
                        {
                            // Samplers
                            for (var j = 0; j < models.Count; j++)
                            {
                                WriteAnimationSampler(timeAccessorIndex, animation_sampler_interpolation.LINEAR, accessorIndex++); // object translation
                                WriteAnimationSampler(timeAccessorIndex, animation_sampler_interpolation.LINEAR, accessorIndex++); // object rotation
                                WriteAnimationSampler(timeAccessorIndex, animation_sampler_interpolation.LINEAR, accessorIndex++); // object scale
                            }

                            // Channels
                            for (var j = 0; j < models.Count; j++)
                            {
                                WriteAnimationChannel(animationSamplerIndex++, animation_channel_target_path.translation, j); // object translation
                                WriteAnimationChannel(animationSamplerIndex++, animation_channel_target_path.rotation, j); // object rotation
                                WriteAnimationChannel(animationSamplerIndex++, animation_channel_target_path.scale, j); // object scale
                            }
                        }
                    }
                }


                // Write Nodes
                // "nodes:" [
                //  {
                //   "mesh": {i},
                //   "name": "{model.EntityName}",
                //   "translation": [ {model.WorldMatrix.ExtractTranslation()} ],
                //   "rotation": [ {model.WorldMatrix.ExtractRotationSafe()} ],
                //   "scale": [ {model.WorldMatrix.ExtractScale()} ]
                //  },
                //  //...
                // ]
                _root.nodes = new List<node>(models.Count);
                for (var i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    var matrix = model.WorldMatrix; //model.LocalMatrix;
                    var translation = matrix.ExtractTranslation();
                    var rotation = matrix.ExtractRotationSafe();
                    var scale = matrix.ExtractScale();
                    _root.nodes.Add(new node
                    {
                        mesh = i,
                        name = model.EntityName,
                        translation = WriteVector3(translation, true),
                        rotation = WriteQuaternion(rotation, true),
                        scale = WriteVector3(scale, false),
                    });
                }


                // Write Buffers
                // "buffers": [
                //  {
                //   "uri": "{binaryBufferFileName}",
                //   "byteLength": {_binaryWriter.BaseStream.Length}
                //  }
                // ]
                _root.buffers = new List<buffer>(1)
                {
                    new buffer
                    {
                        uri = binaryBufferFileName,
                        byteLength = _binaryWriter.BaseStream.Length,
                    }
                };


                // Don't write the extensions list if we don't have any extensions.
                if (_root.extensionsUsed.Count == 0)
                {
                    _root.extensionsUsed = null;
                }


                _binaryWriter.Dispose();
                _binaryWriter = null;

                using (var streamWriter = File.CreateText(filePath))
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.Culture = CultureInfo.InvariantCulture;
                    jsonWriter.Formatting = _options.ReadableFormat ? Formatting.Indented : Formatting.None;
                    jsonWriter.Indentation = 1;
                    jsonWriter.IndentChar = '\t';

                    var jsonSerializer = new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                    };
                    jsonSerializer.Serialize(jsonWriter, _root);
                }
            }
        }
        private void WriteAnimationChannel(int sampler, animation_channel_target_path path, int node)
        {
            //  {
            //   "sampler": {sampler},
            //   "target": {
            //    "node": {node},
            //    "path": "{path}"
            //   }
            //  }
            var animation = _root.animations[_root.animations.Count - 1];
            animation.channels.Add(new animation_channel
            {
                sampler = sampler,
                target = new animation_channel_target
                {
                    node = node,
                    path = path,
                },
            });
        }

        private void WriteAnimationSampler(int input, animation_sampler_interpolation interpolation, int output)
        {
            //  {
            //   "input": {input},
            //   "interpolation": "{interpolation}",
            //   "output": {output}
            //  }
            var animation = _root.animations[_root.animations.Count - 1];
            animation.samplers.Add(new animation_sampler
            {
                input = input,
                interpolation = interpolation,
                output = output,
            });
        }

        private void WriteAccessor(int bufferView, long byteOffset, accessor_componentType componentType, int count, accessor_type type, float[] min = null, float[] max = null)
        {
            //  {
            //   "bufferView": {bufferView},
            //   "byteOffset": {byteOffset},
            //   "componentType": {componentType},
            //   "count": {count},
            //   "type": "{type}",
            // * "min": [ {min[0]...min[N-1]} ], // if min and max != null
            // * "max": [ {max[0]...max[N-1]} ]  // if min and max != null
            //  }
            _root.accessors.Add(new accessor
            {
                bufferView = bufferView,
                byteOffset = byteOffset,
                componentType = componentType,
                count = count,
                type = type,
                min = min,
                max = max,
            });
        }

        private void WriteStartBufferView(long initialOffset, bufferView_target? target = null)
        {
            //  {
            //   "buffer": 0,
            // * "target": {target}, // if target.HasValue
            //   "byteOffset": {_binaryWriter.BaseStream.Position - initialOffset},
            _root.bufferViews.Add(new bufferView
            {
                buffer = 0,
                target = target,
                byteOffset = (_binaryWriter.BaseStream.Position - initialOffset),
            });
        }

        private void WriteEndBufferView(ref long offset)
        {
            //   "byteLength": {_binaryWriter.BaseStream.Position - offset}
            //  }
            _root.bufferViews[_root.bufferViews.Count - 1].byteLength = (_binaryWriter.BaseStream.Position - offset);

            offset = _binaryWriter.BaseStream.Position;
        }

        private void WriteAnimationTimeBufferView(float totalTime, float timeStep, ref long offset, long initialOffset)
        {
            // Write time
            WriteStartBufferView(initialOffset);
            for (var time = 0f; time < totalTime; time += timeStep)
            {
                WriteBinaryFloat(time);
            }
            WriteEndBufferView(ref offset);
        }

        private void WriteAnimationDataBufferViews(ModelEntity model, int modelIndex, Vector3[,] translations, Quaternion[,] rotations, Vector3[,] scales, int totalFrames, ref long offset, long initialOffset)
        {
            // Write position
            WriteStartBufferView(initialOffset);
            for (var frame = 0; frame < totalFrames; frame++)
            {
                var translation = translations[modelIndex, frame];
                WriteBinaryVector3(translation, true);
            }
            WriteEndBufferView(ref offset);

            // Write rotation
            WriteStartBufferView(initialOffset);
            for (var frame = 0; frame < totalFrames; frame++)
            {
                var rotation = rotations[modelIndex, frame];
                WriteBinaryQuaternion(rotation, true);
            }
            WriteEndBufferView(ref offset);

            // Write scale
            WriteStartBufferView(initialOffset);
            for (var frame = 0; frame < totalFrames; frame++)
            {
                var scale = scales[modelIndex, frame];
                WriteBinaryVector3(scale, false);
            }
            WriteEndBufferView(ref offset);
        }

        private void WriteMeshBufferViews(ModelEntity model, ref long offset, long initialOffset)
        {
            // Write vertex positions
            WriteStartBufferView(initialOffset, bufferView_target.ARRAY_BUFFER);
            foreach (var triangle in model.Triangles)
            {
                for (var j = 2; j >= 0; j--)
                {
                    WriteBinaryVector3(triangle.Vertices[j], true);
                }
            }
            WriteEndBufferView(ref offset);

            // Write vertex normals
            WriteStartBufferView(initialOffset, bufferView_target.ARRAY_BUFFER);
            foreach (var triangle in model.Triangles)
            {
                for (var j = 2; j >= 0; j--)
                {
                    WriteBinaryVector3(triangle.Normals[j], true);
                }
            }
            WriteEndBufferView(ref offset);

            // Write vertex colors
            WriteStartBufferView(initialOffset, bufferView_target.ARRAY_BUFFER);
            foreach (var triangle in model.Triangles)
            {
                for (var j = 2; j >= 0; j--)
                {
                    WriteBinaryColor(triangle.Colors[j]);
                }
            }
            WriteEndBufferView(ref offset);

            // Write vertex UVs
            if (NeedsTexture(model))
            {
                WriteStartBufferView(initialOffset, bufferView_target.ARRAY_BUFFER);
                foreach (var triangle in model.Triangles)
                {
                    for (var j = 2; j >= 0; j--)
                    {
                        WriteBinaryUV(triangle.Uv[j]);
                    }
                }
                WriteEndBufferView(ref offset);
            }
        }

        private float[] WriteVector3(Vector3 vector, bool fixHandiness = false)
        {
            if (fixHandiness)
            {
                vector = FixHandiness(vector);
            }
            return new float[] { vector.X, vector.Y, vector.Z };
        }

        private float[] WriteQuaternion(Quaternion quaternion, bool fixHandiness = false)
        {
            if (fixHandiness)
            {
                quaternion = FixHandiness(quaternion);
            }
            return new float[] { quaternion.X, quaternion.Y, quaternion.Z, quaternion.W };
        }

        private void WriteBinaryColor(Color color)
        {
            _binaryWriter.Write(color.R);
            _binaryWriter.Write(color.G);
            _binaryWriter.Write(color.B);
        }

        private void WriteBinaryFloat(float value)
        {
            _binaryWriter.Write(value);
        }

        private void WriteBinaryVector3(Vector3 vector, bool fixHandiness = false)
        {
            if (fixHandiness)
            {
                vector = FixHandiness(vector);
            }
            _binaryWriter.Write(vector.X);
            _binaryWriter.Write(vector.Y);
            _binaryWriter.Write(vector.Z);
        }

        private void WriteBinaryQuaternion(Quaternion quaternion, bool fixHandiness = false)
        {
            if (fixHandiness)
            {
                quaternion = FixHandiness(quaternion);
            }
            _binaryWriter.Write(quaternion.X);
            _binaryWriter.Write(quaternion.Y);
            _binaryWriter.Write(quaternion.Z);
            _binaryWriter.Write(quaternion.W);
        }

        private void WriteBinaryUV(Vector2 vector)
        {
            _binaryWriter.Write(vector.X);
            _binaryWriter.Write(vector.Y);
        }

        private bool NeedsTexture(ModelEntity model)
        {
            return _options.ExportTextures && model.HasTexture;
        }

        private static Vector3 FixHandiness(Vector3 vector)
        {
            return new Vector3(vector.X, -vector.Y, -vector.Z);
        }

        private static Quaternion FixHandiness(Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, -quaternion.Y, -quaternion.Z, quaternion.W);
        }
    }
}