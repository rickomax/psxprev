using System.Globalization;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Exporters
{
    public class OBJExporter
    {
        private StreamWriter _writer;
        private PNGExporter _pngExporter;
        private MTLExporter _mtlExporter;
        private MTLExporter.MaterialDictionary _mtlDictionary;
        private ModelPreparerExporter _modelPreparer;
        private string _selectedPath;
        private bool _experimentalVertexColor;
        private bool _exportTextures;

        public void Export(RootEntity[] entities, string selectedPath, bool joinEntities = false, bool experimentalVertexColor = false, bool exportTextures = true)
        {
            _pngExporter = new PNGExporter();
            _mtlDictionary = new MTLExporter.MaterialDictionary();
            _modelPreparer = new ModelPreparerExporter(tiledTextures: true, singleTexture: false);
            _selectedPath = selectedPath;
            _experimentalVertexColor = experimentalVertexColor;
            _exportTextures = exportTextures;

            // Prepare the shared state for all models being exported (mainly setting up tiled textures).
            foreach (var entity in entities)
            {
                _modelPreparer.AddRootEntity(entity);
            }
            _modelPreparer.PrepareAll();

            if (!joinEntities)
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
            //_mtlDictionary.Clear();
            _mtlDictionary = null;
            _modelPreparer.Dispose();
            _modelPreparer = null;
        }

        private void ExportEntities(int index, params RootEntity[] entities)
        {
            // Re-use the dictionary of materials so that we only export them once,
            // and so that different textures aren't assigned the same ID.
            // We're using a separate mtl file for each model so that unused materials aren't added.
            _mtlExporter = new MTLExporter(_selectedPath, index, _mtlDictionary);
            _writer = new StreamWriter($"{_selectedPath}/obj{index}.obj");

            // Prepare the state for the current model being exported.
            _modelPreparer.PrepareCurrent(entities);

            // Write mtl file reference
            _writer.WriteLine("mtllib {0}", _mtlExporter.FileName);

            // Write vertices and export materials
            foreach (var entity in entities)
            {
                foreach (var model in _modelPreparer.GetModels(entity))
                {
                    WriteModel(model);
                }
            }

            // Write groups and their faces
            var baseIndex = 1; // Obj format is 1-indexed I guess...
            foreach (var entity in entities)
            {
                var models = _modelPreparer.GetModels(entity);
                // todo: Should we really be restarting j (groupIndex) from 0 for each root entity?
                for (var j = 0; j < models.Count; j++)
                {
                    var model = models[j];
                    WriteGroup(j, ref baseIndex, model);
                }
            }

            _mtlExporter.Dispose();
            _mtlExporter = null;
            _writer.Dispose();
            _writer = null;
        }

        private void WriteModel(ModelEntity model)
        {
            // Export material if we haven't already
            if (_exportTextures && model.Texture != null && model.IsTextured)
            {
                if (_mtlExporter.AddMaterial(model.Texture, out var materialId))
                {
                    _pngExporter.Export(model.Texture, materialId, _selectedPath);
                }
            }

            var worldMatrix = model.WorldMatrix;
            // Write vertex positions (and colors if experimental)
            foreach (var triangle in model.Triangles)
            {
                for (var j = 0; j < 3; j++)
                {
                    var vertex = Vector3.TransformPosition(triangle.Vertices[j], worldMatrix);
                    WriteVertexPosition(vertex, triangle.Colors[j]);
                }
            }
            // Write vertex normals
            foreach (var triangle in model.Triangles)
            {
                for (var j = 0; j < 3; j++)
                {
                    var normal = Vector3.TransformNormal(triangle.Normals[j], worldMatrix);
                    WriteNormal(normal);
                }
            }
            // Write vertex UVs
            foreach (var triangle in model.Triangles)
            {
                for (var j = 0; j < 3; j++)
                {
                    WriteUV(triangle.Uv[j]);
                }
            }
        }

        private void WriteVertexPosition(Vector3 vertex, Color color)
        {
            var vertexColor = string.Empty;
            if (_experimentalVertexColor)
            {
                vertexColor = string.Format(" {0} {1} {2}", F(color.R), F(color.G), F(color.B));
            }
            _writer.WriteLine("v {0} {1} {2}{3}", F(vertex.X), F(-vertex.Y), F(-vertex.Z), vertexColor);
        }

        private void WriteNormal(Vector3 normal)
        {
            _writer.WriteLine("vn {0} {1} {2}", F(normal.X), F(-normal.Y), F(-normal.Z));
        }

        private void WriteUV(Vector2 uv)
        {
            _writer.WriteLine("vt {0} {1}", F(uv.X), F(1f - uv.Y));
        }

        private void WriteGroup(int groupIndex, ref int baseIndex, ModelEntity model)
        {
            var materialName = _mtlExporter.GetMaterialName(_exportTextures ? model.Texture : null);

            // Write group header
            _writer.WriteLine("g group{0}", groupIndex);
            _writer.WriteLine("usemtl {0}", materialName);
            // Write group faces
            for (var k = 0; k < model.Triangles.Length; k++)
            {
                // v/vt/vn
                _writer.WriteLine("f {2}/{2}/{2} {1}/{1}/{1} {0}/{0}/{0}", baseIndex++, baseIndex++, baseIndex++);
            }
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