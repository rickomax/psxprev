using System.Collections.Generic;
using System.IO;
using PSXPrev.Classes.Entities;
using PSXPrev.Classes.Mesh;
using PSXPrev.Classes.Utils;

namespace PSXPrev.Classes.Exporters
{
    public class ObjExporter
    {
        public void Export(List<RootEntity> entities, string selectedPath, bool experimentalVertexColor = false)
        {
            var pngExporter = new PngExporter();
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var fileTitle = DialogUtils.SafeFileName(entity.EntityName);
                var fileName = string.Format("{0}/{1}.obj", selectedPath, fileTitle);
                var writer = new StreamWriter(fileName);
                using (var mtlExporter = new MtlExporter(i, fileTitle, selectedPath))
                {
                    writer.WriteLine("mtllib {0}", mtlExporter.FileTitle);
                    foreach (ModelEntity model in entity.ChildEntities)
                    {
                        foreach (var triangle in model.Triangles)
                        {
                            var v0 = triangle.Vertices[0];
                            var v1 = triangle.Vertices[1];
                            var v2 = triangle.Vertices[2];
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
                            writer.WriteLine("v {0} {1} {2}{3}", (v0.X).ToString(GeomUtils.FloatFormat), (-v0.Y).ToString(GeomUtils.FloatFormat), (-v0.Z).ToString(GeomUtils.FloatFormat), vertexColor0);
                            writer.WriteLine("v {0} {1} {2}{3}", (v1.X).ToString(GeomUtils.FloatFormat), (-v1.Y).ToString(GeomUtils.FloatFormat), (-v1.Z).ToString(GeomUtils.FloatFormat), vertexColor1);
                            writer.WriteLine("v {0} {1} {2}{3}", (v2.X).ToString(GeomUtils.FloatFormat), (-v2.Y).ToString(GeomUtils.FloatFormat), (-v2.Z).ToString(GeomUtils.FloatFormat), vertexColor2);
                        }
                        foreach (var triangle in model.Triangles)
                        {
                            var n0 = triangle.Normals[0];
                            var n1 = triangle.Normals[1];
                            var n2 = triangle.Normals[2]; 
                            writer.WriteLine("vn {0} {1} {2}", (n0.X).ToString(GeomUtils.FloatFormat), (-n0.Y).ToString(GeomUtils.FloatFormat), (-n0.Z).ToString(GeomUtils.FloatFormat));
                            writer.WriteLine("vn {0} {1} {2}", (n1.X).ToString(GeomUtils.FloatFormat), (-n1.Y).ToString(GeomUtils.FloatFormat), (-n1.Z).ToString(GeomUtils.FloatFormat));
                            writer.WriteLine("vn {0} {1} {2}", (n2.X).ToString(GeomUtils.FloatFormat), (-n2.Y).ToString(GeomUtils.FloatFormat), (-n2.Z).ToString(GeomUtils.FloatFormat));
                        }
                        foreach (var triangle in model.Triangles)
                        {
                            var uv0 = triangle.Uv[0];
                            var uv1 = triangle.Uv[1];
                            var uv2 = triangle.Uv[2];
                            writer.WriteLine("vt {0} {1}", uv0.X.ToString(GeomUtils.FloatFormat), (1f - uv0.Y).ToString(GeomUtils.FloatFormat));
                            writer.WriteLine("vt {0} {1}", uv1.X.ToString(GeomUtils.FloatFormat), (1f - uv1.Y).ToString(GeomUtils.FloatFormat));
                            writer.WriteLine("vt {0} {1}", uv2.X.ToString(GeomUtils.FloatFormat), (1f - uv2.Y).ToString(GeomUtils.FloatFormat));
                        }
                    }
                    var lastIndex = 1;
                    for (var m = 0; m < entity.ChildEntities.Count; m++)
                    {
                        var model = entity.ChildEntities[m];
                        if (model.Texture != null)
                        {
                            if (mtlExporter.AddMaterial(model.Texture, model.TexturePage))
                            {
                                pngExporter.Export(model.Texture, i, model.TexturePage, selectedPath);
                            }
                            writer.WriteLine("g group" + m);
                            writer.WriteLine("usemtl mtl{0}", model.TexturePage);
                            for (var k = 0; k < model.Triangles.Count; k++)
                            {
                                writer.WriteLine("f {2}/{2}/{2} {1}/{1}/{1} {0}/{0}/{0}", lastIndex++, lastIndex++,
                                    lastIndex++);
                            }
                            writer.Close();
                        }
                    }
                }
            }

        }
    }
}