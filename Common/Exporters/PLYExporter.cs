using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Exporters
{
    public class PLYExporter
    {
        private StreamWriter _writer;
        private PNGExporter _pngExporter;
        private Dictionary<Texture, int> _materialsDictionary;
        private bool _untexturedMaterialExported;
        private ModelPreparerExporter _modelPreparer;
        private string _selectedPath;
        private string _baseName;
        private string _baseTextureName;
        private ExportModelOptions _options;

        public void Export(RootEntity[] entities, ExportModelOptions options = null)
        {
            _options = options?.Clone() ?? new ExportModelOptions();
            // Force any required options for this format here, before calling Validate.
            _options.SingleTexture = true; // PLY only supports one texture file
            _options.Validate();

            // todo: Different material IDs aren't handled for different root entities.
            _pngExporter = new PNGExporter();
            _materialsDictionary = new Dictionary<Texture, int>();
            _untexturedMaterialExported = false;
            _modelPreparer = new ModelPreparerExporter(_options);
            _selectedPath = _options.Path;

            // Prepare the shared state for all models being exported (mainly setting up tiled textures).
            _modelPreparer.PrepareAll(entities);

            if (!_options.MergeEntities)
            {
                for (var i = 0; i < entities.Length; i++)
                {
                    ExportEntities(i, entities[i]);
                }
            }
            else
            {
                ExportEntities(0, entities);
            }

            //_pngExporter.Dispose();
            _pngExporter = null;
            //_materialsDictionary.Clear();
            _materialsDictionary = null;
            _modelPreparer.Dispose();
            _modelPreparer = null;
        }

        private void ExportEntities(int index, params RootEntity[] entities)
        {
            if (!_options.ShareTextures)
            {
                _materialsDictionary.Clear();
            }

            _baseName = $"ply{index}";
            _baseTextureName = (_options.ShareTextures ? "plyshared" : _baseName); //+ "_";
            _writer = new StreamWriter($"{_selectedPath}/{_baseName}.ply");

            // Prepare the state for the current model being exported.
            _modelPreparer.PrepareCurrent(entities);

            // Count faces and export materials
            // Note that the standard Ply format does not support listing
            // texture files, even though it supports texture UVs...
            var faceCount = 0;
            var materialCount = 1; // change: Start at 1 because 0 is untextured
            Texture singleTexture = null;
            foreach (var entity in entities)
            {
                _modelPreparer.GetPreparedRootEntity(entity, out var models);
                foreach (var model in models)
                {
                    faceCount += model.Triangles.Length;

                    // Export material if we haven't already
                    if (NeedsTexture(model))
                    {
                        singleTexture = model.Texture;

                        if (!_materialsDictionary.TryGetValue(model.Texture, out var materialIndex))
                        {
                            // Using singleTexture, materialIndex is always 0.
                            materialIndex = 0;// _materialsDictionary.Count + 1; // 1-indexed because 0 is untextured
                            _materialsDictionary.Add(model.Texture, materialIndex);

                            //_pngExporter.Export(model.Texture, _baseTextureName + materialIndex, _selectedPath);
                        }
                        // For each later model we write, there may be more materials defined that are unused.
                        materialCount = Math.Max(materialCount, materialIndex + 1);
                    }
                }
            }

            // Export untextured material
            if (_options.ExportTextures && !_untexturedMaterialExported)
            {
                // We always need to export the untextured material, because we're 1-indexing other materials.
                _untexturedMaterialExported = true;
                // It's pointless to export an empty material, if the importer isn't going to use it.
                //_pngExporter.ExportEmpty(System.Drawing.Color.White, _baseTextureName + 0, _selectedPath);
            }

            // Write header
            _writer.WriteLine("ply");
            _writer.WriteLine("format ascii 1.0");

            // Export single texture material
            // Note: Some formats allow defining ONE texture file, like so.
            if (_options.ExportTextures && singleTexture != null)
            {
                _pngExporter.Export(singleTexture, _baseTextureName, _selectedPath);

                _writer.WriteLine("comment TextureFile {0}.png", _baseTextureName);
            }

            // Write header vertex structure
            var vertexCount = faceCount * 3;
            _writer.WriteLine("element vertex {0}", vertexCount);
            _writer.WriteLine("property float32 x");
            _writer.WriteLine("property float32 y");
            _writer.WriteLine("property float32 z");
            _writer.WriteLine("property float32 nx");
            _writer.WriteLine("property float32 ny");
            _writer.WriteLine("property float32 nz");
            // Support two different names for UVs: texture_u/v, and s/t
            _writer.WriteLine("property float32 texture_u"); // MeshLab reads texture_u/v
            _writer.WriteLine("property float32 texture_v");
            _writer.WriteLine("property float32 s"); // Blender reads s/t
            _writer.WriteLine("property float32 t");
            _writer.WriteLine("property uchar red");
            _writer.WriteLine("property uchar green");
            _writer.WriteLine("property uchar blue");
            _writer.WriteLine("property int32 material_index");
            // Write header face structure
            _writer.WriteLine("element face {0}", faceCount);
            _writer.WriteLine("property list uint8 int32 vertex_indices");
            // Write header material structure
            _writer.WriteLine("element material {0}", materialCount);
            _writer.WriteLine("property uchar ambient_red");
            _writer.WriteLine("property uchar ambient_green");
            _writer.WriteLine("property uchar ambient_blue");
            _writer.WriteLine("property float32 ambient_coeff");
            _writer.WriteLine("property uchar diffuse_red");
            _writer.WriteLine("property uchar diffuse_green");
            _writer.WriteLine("property uchar diffuse_blue");
            _writer.WriteLine("property float32 diffuse_coeff");
            // End header
            _writer.WriteLine("end_header");

            // Write vertices
            foreach (var entity in entities)
            {
                _modelPreparer.GetPreparedRootEntity(entity, out var models);
                foreach (var model in models)
                {
                    var materialIndex = 0; // Untextured
                    if (NeedsTexture(model))
                    {
                        materialIndex = _materialsDictionary[model.Texture];
                    }

                    var worldMatrix = model.WorldMatrix;
                    foreach (var triangle in model.Triangles)
                    {
                        for (var j = 0; j < 3; j++)
                        {
                            var vertex = Vector3.TransformPosition(triangle.Vertices[j], worldMatrix);
                            var normal = Vector3.TransformNormal(triangle.Normals[j], worldMatrix);
                            WriteVertex(vertex, normal, triangle.Uv[j], triangle.Colors[j], materialIndex);
                        }
                    }
                }
            }
            // Write faces
            for (var i = 0; i < faceCount; i++)
            {
                WriteFace(i);
            }
            // Write materials
            for (var i = 0; i < materialCount; i++)
            {
                WriteMaterial();
            }

            _writer.Close();
            _writer = null;
        }

        private void WriteVertex(Vector3 vertex, Vector3 normal, Vector2 uv, Color color, int materialIndex)
        {
            // Output UV (6 and 7) twice, since we're supporting two different names for it.
            _writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {6} {7} {8} {9} {10} {11}",
                              //F(-vertex.X), F(-vertex.Y), F(-vertex.Z),
                              //F(-normal.X), F(-normal.Y), F(-normal.Z),
                              F(vertex.X), F(-vertex.Y), F(-vertex.Z),
                              F(normal.X), F(-normal.Y), F(-normal.Z),
                              F(uv.X), F(1f - uv.Y),
                              I(color.R * 255), I(color.G * 255), I(color.B * 255),
                              materialIndex);
        }

        private void WriteFace(int faceIndex)
        {
            var vertexIndex = faceIndex * 3;
            //_writer.WriteLine("3 {0} {1} {2}", vertexIndex, vertexIndex + 1, vertexIndex + 2);
            _writer.WriteLine("3 {2} {1} {0}", vertexIndex, vertexIndex + 1, vertexIndex + 2);
        }

        private void WriteMaterial()
        {
            _writer.WriteLine("128 128 128 1.00000 128 128 128 1.00000");
        }

        private bool NeedsTexture(ModelEntity model)
        {
            return _options.ExportTextures && model.HasTexture;
        }

        private static string F(float value)
        {
            return value.ToString(GeomMath.FloatFormat, CultureInfo.InvariantCulture);
        }

        private static string I(float value)
        {
            return value.ToString(GeomMath.IntegerFormat, CultureInfo.InvariantCulture);
        }
    }
}