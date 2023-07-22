using System.Globalization;
using System.IO;
using OpenTK;

namespace PSXPrev.Classes
{
    public class ObjExporter
    {
        private StreamWriter _streamWriter;
        private PngExporter _pngExporter;
        private MtlExporter _mtlExporter;
        private string _selectedPath;
        private bool _experimentalVertexColor;

        public void Export(RootEntity[] entities, string selectedPath, bool experimentalVertexColor = false, bool joinEntities = false)
        {
            _pngExporter = new PngExporter();
            _selectedPath = selectedPath;
            _experimentalVertexColor = experimentalVertexColor;
            if (!joinEntities)
            {
                for (var i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    _mtlExporter = new MtlExporter(selectedPath, i);
                    _streamWriter = new StreamWriter($"{selectedPath}/obj{i}.obj");
                    _streamWriter.WriteLine("mtllib {0}", _mtlExporter.FileName);
                    foreach (ModelEntity model in entity.ChildEntities)
                    {
                        WriteModel(model);
                    }
                    var baseIndex = 1;
                    for (var j = 0; j < entity.ChildEntities.Length; j++)
                    {
                        var model = (ModelEntity)entity.ChildEntities[j];
                        WriteGroup(j, ref baseIndex, model);
                    }
                    _streamWriter.Dispose();
                    _mtlExporter.Dispose();
                }
            }
            else
            {
                _mtlExporter = new MtlExporter(selectedPath, 0);
                _streamWriter = new StreamWriter($"{selectedPath}/obj0.obj");
                _streamWriter.WriteLine("mtllib {0}", _mtlExporter.FileName);
                foreach (var entity in entities)
                {
                    foreach (ModelEntity model in entity.ChildEntities)
                    {
                        WriteModel(model);
                    }
                }
                var baseIndex = 1;
                foreach (var entity in entities)
                {
                    for (var j = 0; j < entity.ChildEntities.Length; j++)
                    {
                        var model = (ModelEntity)entity.ChildEntities[j];
                        WriteGroup(j, ref baseIndex, model);
                    }
                }
                _streamWriter.Dispose();
                _mtlExporter.Dispose();
            }
            _mtlExporter.Dispose();
        }

        private void WriteModel(ModelEntity model)
        {
            if (model.Texture != null && model.IsTextured)
            {
                if (_mtlExporter.AddMaterial((int) model.TexturePage))
                {
                    _pngExporter.Export(model.Texture, (int) model.TexturePage, _selectedPath);
                }
            }
            var worldMatrix = model.WorldMatrix;
            foreach (var triangle in model.Triangles)
            {
                for (var j = 0; j < 3; j++)
                {
                    var vertex = Vector3.TransformPosition(triangle.Vertices[j], worldMatrix);
                    WriteVertex(vertex, triangle.Colors[j]);
                }
            }
            foreach (var triangle in model.Triangles)
            {
                for (var j = 0; j < 3; j++)
                {
                    var normal = Vector3.TransformNormal(triangle.Normals[j], worldMatrix);
                    WriteNormal(normal);
                }
            }
            foreach (var triangle in model.Triangles)
            {
                for (var j = 0; j < 3; j++)
                {
                    WriteUV(triangle.Uv[j]);
                }
            }
        }

        private void WriteVertex(Vector3 vertex, Color color)
        {
            var vertexColor = string.Empty;
            if (_experimentalVertexColor)
            {
                vertexColor = string.Format(" {0} {1} {2}", F(color.R), F(color.G), F(color.B));
            }
            _streamWriter.WriteLine("v {0} {1} {2}{3}", F(vertex.X), F(-vertex.Y), F(-vertex.Z), vertexColor);
        }

        private void WriteNormal(Vector3 normal)
        {
            _streamWriter.WriteLine("vn {0} {1} {2}", F(normal.X), F(-normal.Y), F(-normal.Z));
        }

        private void WriteUV(Vector2 uv)
        {
            _streamWriter.WriteLine("vt {0} {1}", F(uv.X), F(1f - uv.Y));
        }

        private void WriteGroup(int groupIndex, ref int baseIndex, ModelEntity model)
        {
            var materialName = _mtlExporter.GetMaterialName(model.IsTextured ? (int?)model.TexturePage : null);

            _streamWriter.WriteLine("g group{0}", groupIndex);
            _streamWriter.WriteLine("usemtl {0}", materialName);
            for (var k = 0; k < model.Triangles.Length; k++)
            {
                _streamWriter.WriteLine("f {2}/{2}/{2} {1}/{1}/{1} {0}/{0}/{0}", baseIndex++, baseIndex++, baseIndex++);
            }
        }

        private static string F(float value)
        {
            return value.ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture);
        }

        private static string I(float value)
        {
            return value.ToString(GeomUtils.IntegerFormat, CultureInfo.InvariantCulture);
        }
    }
}