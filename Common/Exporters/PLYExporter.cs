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
        private Dictionary<Texture, int> _exportedTextures;
        private ModelPreparerExporter _modelPreparer;
        private ExportModelOptions _options;
        private string _baseName;

        public void Export(ExportModelOptions options, RootEntity[] entities)
        {
            _options = options?.Clone() ?? new ExportModelOptions();
            // Force any required options for this format here, before calling Validate.
            _options.SingleTexture = true; // PLY only supports one texture file
            _options.VertexIndexReuse = false; // Vertices have too much information to bother with reuse
            _options.Validate("ply");

            // todo: Different material IDs aren't handled for different root entities.
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

                    ExportEntities(i, entities[i]);
                }
            }
            else
            {
                // Prepare the state for the current model being exported.
                _modelPreparer.PrepareCurrent(entities);

                ExportEntities(0, entities);
            }

            //_pngExporter.Dispose();
            _pngExporter = null;
            _exportedTextures = null;
            _modelPreparer.Dispose();
            _modelPreparer = null;
        }

        private void ExportEntities(int index, params RootEntity[] entities)
        {
            // If shared, reuse the dictionary of textures so that we only export them once.
            if (!_options.ShareTextures)
            {
                _exportedTextures.Clear();
            }

            _baseName = _options.GetBaseName(index);
            _writer = new StreamWriter(Path.Combine(_options.Path, $"{_baseName}.ply"));

            // Count faces and export materials
            // Note that the standard Ply format does not support listing
            // texture files, even though it supports texture UVs...
            var faceCount = 0;
            Texture singleTexture = null;
            foreach (var entity in entities)
            {
                _modelPreparer.GetPreparedRootEntity(entity, out var models);
                foreach (var model in models)
                {
                    faceCount += model.Triangles.Length;

                    // Export material if we haven't already
                    var texture = model.Texture;
                    if (NeedsTexture(model))
                    {
                        singleTexture = texture;

                        if (!_exportedTextures.TryGetValue(texture, out var exportedTextureId))
                        {
                            exportedTextureId = _exportedTextures.Count; // Should always be 0 here
                            _exportedTextures.Add(texture, exportedTextureId);

                            var textureName = _options.GetTextureName(_baseName, exportedTextureId);
                            _pngExporter.Export(singleTexture, textureName, _options.Path);
                        }
                    }
                }
            }

            // Write header
            _writer.WriteLine("ply");
            _writer.WriteLine("format ascii 1.0");

            // Export single texture material
            // Note: Some formats allow defining ONE texture file, like so.
            if (singleTexture != null)
            {
                var exportedTextureId = _exportedTextures[singleTexture];
                var textureName = _options.GetTextureName(_baseName, exportedTextureId);
                _writer.WriteLine("comment TextureFile {0}.png", textureName);
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
            var materialCount = 1; // Only one material is defined
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
                    var materialIndex = 0; // Only one material is defined

                    var worldMatrix = model.WorldMatrix;
                    Matrix4.Invert(ref worldMatrix, out var invWorldMatrix);
                    foreach (var triangle in model.Triangles)
                    {
                        for (var j = 2; j >= 0; j--)
                        {
                            WriteVertex(triangle.Vertices[j], triangle.Normals[j], triangle.Uv[j], triangle.Colors[j],
                                        materialIndex, ref worldMatrix, ref invWorldMatrix);
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

        private void WriteVertex(Vector3 localVertex, Vector3 localNormal, Vector2 uv, Color color, int materialIndex, ref Matrix4 worldMatrix, ref Matrix4 invWorldMatrix)
        {
            Vector3.TransformPosition(ref localVertex, ref worldMatrix, out var vertex);
            GeomMath.TransformNormalInverseNormalized(ref localNormal, ref invWorldMatrix, out var normal);

            // Output UV (6 and 7) twice, since we're supporting two different names for it.
            // vertex X Y Z, normal X Y Z, uv U V S T, color R G B, material index
            _writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {6} {7} {8} {9} {10} {11}",
                              F(vertex.X), F(-vertex.Y), F(-vertex.Z),
                              F(normal.X), F(-normal.Y), F(-normal.Z),
                              F(uv.X), F(1f - uv.Y),
                              I(color.R * 255), I(color.G * 255), I(color.B * 255),
                              materialIndex);
        }

        private void WriteFace(int faceIndex)
        {
            var vertexIndex = faceIndex * 3;
            _writer.WriteLine("3 {0} {1} {2}", vertexIndex++, vertexIndex++, vertexIndex++);
        }

        private void WriteMaterial()
        {
            // ambient R G B Coef, diffuse R G B Coef
            _writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}",
                              128, 128, 128, F(1.0f),  // ambient
                              128, 128, 128, F(1.0f)); // diffuse
        }

        private bool NeedsTexture(ModelEntity model)
        {
            return _options.ExportTextures && model.HasTexture;
        }

        private string F(float value)
        {
            return value.ToString(_options.FloatFormat, NumberFormatInfo.InvariantInfo);
        }

        private static string I(float value)
        {
            return value.ToString(GeomMath.IntegerFormat, NumberFormatInfo.InvariantInfo);
        }
    }
}