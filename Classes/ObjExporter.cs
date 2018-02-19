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
                var lastIndex = 1;
                var entity = entities[i];
                var writer = new StreamWriter(selectedPath + "/obj" + i + ".obj");
                writer.WriteLine("mtllib mtl{0}.mtl", i);
                using (var mtlExporter = new MtlExporter(i, selectedPath))
                {
                    for (int j = 0; j < entity.ChildEntities.Length; j++)
                    {
                        var model = (ModelEntity)entity.ChildEntities[j];
                        writer.WriteLine("g group" + j);
                        writer.WriteLine("usemtl mtl{0}", model.TexturePage);
                        if (model.Texture != null)
                        {
                            if (mtlExporter.AddMaterial(model.Texture, model.TexturePage))
                            {
                                pngExporter.Export(model.Texture, i, model.TexturePage, selectedPath);
                            }
                        }
                        for (int k = 0; k < model.Triangles.Length; k++)
                        {
                            var triangle = model.Triangles[k];

                            var v0 = model.WorldMatrix * new Vector4(triangle.Vertices[0]);
                            var v1 = model.WorldMatrix * new Vector4(triangle.Vertices[1]);
                            var v2 = model.WorldMatrix * new Vector4(triangle.Vertices[2]);

                            var uv0 = triangle.Uv[0];
                            var uv1 = triangle.Uv[1];
                            var uv2 = triangle.Uv[2];

                            var n0 = triangle.Normals[0];
                            var n1 = triangle.Normals[1];
                            var n2 = triangle.Normals[2];

                            var c0 = triangle.Colors[0];
                            var c1 = triangle.Colors[1];
                            var c2 = triangle.Colors[2];

                            var vertexColor0 = string.Empty;
                            var vertexColor1 = string.Empty;
                            var vertexColor2 = string.Empty;
                            if (experimentalVertexColor)
                            {
                                vertexColor0 = string.Format(" {0} {1} {2}", (c0.R).ToString(GeomUtils.FloatFormat), (c0.G).ToString(GeomUtils.FloatFormat), (c0.B).ToString(GeomUtils.FloatFormat));
                                vertexColor1 = string.Format(" {0} {1} {2}", (c1.R).ToString(GeomUtils.FloatFormat), (c1.G).ToString(GeomUtils.FloatFormat), (c1.B).ToString(GeomUtils.FloatFormat));
                                vertexColor2 = string.Format(" {0} {1} {2}", (c2.R).ToString(GeomUtils.FloatFormat), (c2.G).ToString(GeomUtils.FloatFormat), (c2.B).ToString(GeomUtils.FloatFormat));
                            }

                            writer.WriteLine("v {0} {1} {2}{3}", (-v0.X).ToString(GeomUtils.FloatFormat), (-v0.Y).ToString(GeomUtils.FloatFormat), (-v0.Z).ToString(GeomUtils.FloatFormat), vertexColor0);
                            writer.WriteLine("vn {0} {1} {2}", (-n0.X).ToString(GeomUtils.FloatFormat), (-n0.Y).ToString(GeomUtils.FloatFormat), (-n0.Z).ToString(GeomUtils.FloatFormat));
                            writer.WriteLine("vt {0} {1}", uv0.X.ToString(GeomUtils.FloatFormat), (1f - uv0.Y).ToString(GeomUtils.FloatFormat));

                            writer.WriteLine("v {0} {1} {2}{3}", (-v1.X).ToString(GeomUtils.FloatFormat), (-v1.Y).ToString(GeomUtils.FloatFormat), (-v1.Z).ToString(GeomUtils.FloatFormat), vertexColor1);
                            writer.WriteLine("vn {0} {1} {2}", (-n1.X).ToString(GeomUtils.FloatFormat), (-n1.Y).ToString(GeomUtils.FloatFormat), (-n1.Z).ToString(GeomUtils.FloatFormat));
                            writer.WriteLine("vt {0} {1}", uv1.X.ToString(GeomUtils.FloatFormat), (1f - uv1.Y).ToString(GeomUtils.FloatFormat));

                            writer.WriteLine("v {0} {1} {2}{3}", (-v2.X).ToString(GeomUtils.FloatFormat), (-v2.Y).ToString(GeomUtils.FloatFormat), (-v2.Z).ToString(GeomUtils.FloatFormat), vertexColor2);
                            writer.WriteLine("vn {0} {1} {2}", (-n2.X).ToString(GeomUtils.FloatFormat), (-n2.Y).ToString(GeomUtils.FloatFormat), (-n2.Z).ToString(GeomUtils.FloatFormat));
                            writer.WriteLine("vt {0} {1}", uv2.X.ToString(GeomUtils.FloatFormat), (1f - uv2.Y).ToString(GeomUtils.FloatFormat));

                            writer.WriteLine("f {2}/{2}/{2} {1}/{1}/{1} {0}/{0}/{0}", lastIndex, lastIndex + 1, lastIndex + 2);

                            lastIndex += 3;
                        }
                    }
                    writer.Close();
                }
            }

        }
    }
}