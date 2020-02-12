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
                    _streamWriter.WriteLine("mtllib mtl{0}.mtl", i);
                    foreach (var childEntity in entity.ChildEntities)
                    {
                        WriteModel(childEntity as ModelEntity);
                    }
                    var baseIndex = 1;
                    for (var j = 0; j < entity.ChildEntities.Length; j++)
                    {
                        var childEntity = entity.ChildEntities[j];
                        WriteGroup(j, ref baseIndex, childEntity as ModelEntity);
                    }
                    _streamWriter.Dispose();
                    _mtlExporter.Dispose();
                }
            }
            else
            {
                _mtlExporter = new MtlExporter(selectedPath, 0);
                _streamWriter = new StreamWriter($"{selectedPath}/obj0.obj");
                _streamWriter.WriteLine("mtllib mtl0.mtl");
                foreach (var entity in entities)
                {
                    foreach (EntityBase childEntity in entity.ChildEntities)
                    {
                        WriteModel(childEntity as ModelEntity);
                    }
                }
                var baseIndex = 1;
                foreach (var entity in entities)
                {
                    for (int j = 0; j < entity.ChildEntities.Length; j++)
                    {
                        var childEntity = entity.ChildEntities[j];
                        WriteGroup(j, ref baseIndex, childEntity as ModelEntity);
                    }
                }
                _streamWriter.Dispose();
                _mtlExporter.Dispose();
            }
            _mtlExporter.Dispose();
        }

        private void WriteModel(ModelEntity model)
        {
            if (model.Texture != null)
            {
                if (_mtlExporter.AddMaterial((int) model.TexturePage))
                {
                    _pngExporter.Export(model.Texture, (int) model.TexturePage, _selectedPath);
                }
            }
            var worldMatrix = model.WorldMatrix;
            foreach (var triangle in model.Triangles)
            {
                var vertexColor0 = string.Empty;
                var vertexColor1 = string.Empty;
                var vertexColor2 = string.Empty;
                var c0 = triangle.Colors[0];
                var c1 = triangle.Colors[1];
                var c2 = triangle.Colors[2];
                if (_experimentalVertexColor)
                {
                    vertexColor0 = string.Format(" {0} {1} {2}", (c0.R).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c0.G).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c0.B).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                    vertexColor1 = string.Format(" {0} {1} {2}", (c1.R).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c1.G).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c1.B).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                    vertexColor2 = string.Format(" {0} {1} {2}", (c2.R).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c2.G).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c2.B).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                }
                var v0 = Vector3.TransformPosition(triangle.Vertices[0], worldMatrix);
                var v1 = Vector3.TransformPosition(triangle.Vertices[1], worldMatrix);
                var v2 = Vector3.TransformPosition(triangle.Vertices[2], worldMatrix);
                _streamWriter.WriteLine("v {0} {1} {2} {3}", (v0.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v0.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v0.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), vertexColor0);
                _streamWriter.WriteLine("v {0} {1} {2} {3}", (v1.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v1.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v1.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), vertexColor1);
                _streamWriter.WriteLine("v {0} {1} {2} {3}", (v2.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v2.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v2.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), vertexColor2);
            }
            foreach (var triangle in model.Triangles)
            {
                var n0 = Vector3.TransformNormal(triangle.Normals[0], worldMatrix);
                var n1 = Vector3.TransformNormal(triangle.Normals[1], worldMatrix);
                var n2 = Vector3.TransformNormal(triangle.Normals[2], worldMatrix);
                _streamWriter.WriteLine("vn {0} {1} {2}", (n0.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n0.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n0.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                _streamWriter.WriteLine("vn {0} {1} {2}", (n1.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n1.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n1.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                _streamWriter.WriteLine("vn {0} {1} {2}", (n2.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n2.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n2.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
            }
            foreach (var triangle in model.Triangles)
            {
                var uv0 = triangle.Uv[0];
                var uv1 = triangle.Uv[1];
                var uv2 = triangle.Uv[2];
                _streamWriter.WriteLine("vt {0} {1}", uv0.X.ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (1f - uv0.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                _streamWriter.WriteLine("vt {0} {1}", uv1.X.ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (1f - uv1.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                _streamWriter.WriteLine("vt {0} {1}", uv2.X.ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (1f - uv2.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
            }
        }

        private void WriteGroup(int groupIndex, ref int baseIndex, ModelEntity model)
        {
            _streamWriter.WriteLine("g group" + groupIndex);
            _streamWriter.WriteLine("usemtl mtl{0}", model.TexturePage);
            for (var k = 0; k < model.Triangles.Length; k++)
            {
                _streamWriter.WriteLine("f {2}/{2}/{2} {1}/{1}/{1} {0}/{0}/{0}", baseIndex++, baseIndex++, baseIndex++);
            }
        }
    }
}