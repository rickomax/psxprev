using OpenTK;
using System.Globalization;
using System.IO;


namespace PSXPrev
{
    public class ObjExporter
    {
        public ObjExporter()
        {

        }

        public void Export(RootEntity[] entities, string selectedPath, bool experimentalVertexColor = false)
        {
            var pngExporter = new PngExporter();
            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var writer = new StreamWriter(selectedPath + "/obj" + i + ".obj");
                writer.WriteLine("mtllib mtl{0}.mtl", i);
                using (var mtlExporter = new MtlExporter(i, selectedPath))
                {
                    foreach (EntityBase childEntity in entity.ChildEntities)
                    {
                        var model = childEntity as ModelEntity;
                        if (model.Texture != null)
                        {
                            if (mtlExporter.AddMaterial(model.Texture, model.TexturePage))
                            {
                                pngExporter.Export(model.Texture, i, model.TexturePage, selectedPath);
                            }
                        }
                        foreach (var triangle in model.Triangles)
                        {
                            var vertexColor0 = string.Empty;
                            var vertexColor1 = string.Empty;
                            var vertexColor2 = string.Empty;
                            var c0 = triangle.Colors[0];
                            var c1 = triangle.Colors[1];
                            var c2 = triangle.Colors[2];
                            if (experimentalVertexColor)
                            {
                                vertexColor0 = string.Format(" {0} {1} {2}", (c0.R).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c0.G).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c0.B).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                                vertexColor1 = string.Format(" {0} {1} {2}", (c1.R).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c1.G).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c1.B).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                                vertexColor2 = string.Format(" {0} {1} {2}", (c2.R).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c2.G).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (c2.B).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                            }
                            var v0 = model.WorldMatrix * new Vector4(triangle.Vertices[0]);
                            var v1 = model.WorldMatrix * new Vector4(triangle.Vertices[1]);
                            var v2 = model.WorldMatrix * new Vector4(triangle.Vertices[2]);
                            writer.WriteLine("v {0} {1} {2} {3}", (v0.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v0.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v0.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), vertexColor0);
                            writer.WriteLine("v {0} {1} {2} {3}", (v1.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v1.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v1.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), vertexColor1);
                            writer.WriteLine("v {0} {1} {2} {3}", (v2.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v2.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-v2.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), vertexColor2);
                        }
                        foreach (var triangle in model.Triangles)
                        {
                            var n0 = (model.WorldMatrix * new Vector4(triangle.Normals[0])).Normalized();
                            var n1 = (model.WorldMatrix * new Vector4(triangle.Normals[1])).Normalized();
                            var n2 = (model.WorldMatrix * new Vector4(triangle.Normals[2])).Normalized();
                            writer.WriteLine("vn {0} {1} {2}", (n0.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n0.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n0.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                            writer.WriteLine("vn {0} {1} {2}", (n1.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n1.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n1.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                            writer.WriteLine("vn {0} {1} {2}", (n2.X).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n2.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (-n2.Z).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                        }
                        foreach (var triangle in model.Triangles)
                        {
                            var uv0 = triangle.Uv[0];
                            var uv1 = triangle.Uv[1];
                            var uv2 = triangle.Uv[2];
                            writer.WriteLine("vt {0} {1}", uv0.X.ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (1f - uv0.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                            writer.WriteLine("vt {0} {1}", uv1.X.ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (1f - uv1.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                            writer.WriteLine("vt {0} {1}", uv2.X.ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture), (1f - uv2.Y).ToString(GeomUtils.FloatFormat, CultureInfo.InvariantCulture));
                        }
                    }
                    var baseIndex = 1;
                    for (int j = 0; j < entity.ChildEntities.Length; j++)
                    {
                        var childEntity = entity.ChildEntities[j];
                        var model = childEntity as ModelEntity;
                        writer.WriteLine("g group" + j);
                        writer.WriteLine("usemtl mtl{0}", model.TexturePage);
                        for (var k = 0; k < model.Triangles.Length; k++)
                        {
                            writer.WriteLine("f {2}/{2}/{2} {1}/{1}/{1} {0}/{0}/{0}", baseIndex++, baseIndex++, baseIndex++);
                        }
                    }
                    writer.Close();
                }
            }
        }
    }
}