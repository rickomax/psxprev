using OpenTK;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;


namespace PSXPrev
{
    public class PlyExporter
    {
        public PlyExporter()
        {

        }

        public void Export(RootEntity[] entities, string selectedPath)
        {
            var pngExporter = new PngExporter();
            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var writer = new StreamWriter(selectedPath + "/ply" + i + ".ply");
                writer.WriteLine("ply");
                writer.WriteLine("format ascii 1.0");
                var materialsDic = new Dictionary<int, int>();
                var faceCount = 0;
                var numMaterials = 0;
                foreach (var entityBase in entity.ChildEntities)
                {
                    var model = (ModelEntity) entityBase;
                    faceCount += model.Triangles.Count();
                    var texturePage = model.TexturePage;
                    if (!materialsDic.ContainsKey(texturePage))
                    {
                        materialsDic.Add(texturePage, numMaterials++);
                        pngExporter.Export(model.Texture, i, texturePage, selectedPath);
                    }
                }
                var vertexCount = faceCount * 3;
                writer.WriteLine("element vertex {0}", vertexCount);
                writer.WriteLine("property float32 x");
                writer.WriteLine("property float32 y");
                writer.WriteLine("property float32 z");
                writer.WriteLine("property float32 nx");
                writer.WriteLine("property float32 ny");
                writer.WriteLine("property float32 nz");
                writer.WriteLine("property float32 u");
                writer.WriteLine("property float32 v");
                writer.WriteLine("property uchar red");
                writer.WriteLine("property uchar green");
                writer.WriteLine("property uchar blue");
                writer.WriteLine("property int32 material_index");
                writer.WriteLine("element face {0}", faceCount);
                writer.WriteLine("property list uint8 int32 vertex_indices");
                writer.WriteLine("element material {0}", numMaterials);
                writer.WriteLine("property uchar ambient_red");
                writer.WriteLine("property uchar ambient_green"); 
                writer.WriteLine("property uchar ambient_blue");
                writer.WriteLine("property float32 ambient_coeff");
                writer.WriteLine("property uchar diffuse_red");        
                writer.WriteLine("property uchar diffuse_green"); 
                writer.WriteLine("property uchar diffuse_blue");
                writer.WriteLine("property float32 diffuse_coeff");
                writer.WriteLine("end_header");
                foreach (var entityBase in entity.ChildEntities)
                {
                    var model = (ModelEntity) entityBase;
                    var materialIndex = materialsDic[model.TexturePage];
                    var triangles = model.Triangles;
                    foreach (var triangle in triangles)
                    {
                        var vertex0 = model.WorldMatrix * new Vector4(triangle.Vertices[0]);
                        var normal0 = triangle.Normals[0];
                        var uv0 = triangle.Uv[0];
                        var color0 = triangle.Colors[0];
                        writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                            (-vertex0.X).ToString(GeomUtils.FloatFormat), (-vertex0.Y).ToString(GeomUtils.FloatFormat), (-vertex0.Z).ToString(GeomUtils.FloatFormat),
                            (-normal0.X).ToString(GeomUtils.FloatFormat), (-normal0.Y).ToString(GeomUtils.FloatFormat), (-normal0.Z).ToString(GeomUtils.FloatFormat),
                            uv0.X.ToString(GeomUtils.FloatFormat), (1f - uv0.Y).ToString(GeomUtils.FloatFormat),
                            (color0.R * 255).ToString(GeomUtils.IntegerFormat), (color0.G * 255).ToString(GeomUtils.IntegerFormat), (color0.B * 255).ToString(GeomUtils.IntegerFormat),
                            materialIndex);
                        var vertex1 = triangle.Vertices[1];
                        var normal1 = triangle.Normals[1];
                        var uv1 = triangle.Uv[1];
                        var color1 = triangle.Colors[1];
                        writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                            (-vertex1.X).ToString(GeomUtils.FloatFormat), (-vertex1.Y).ToString(GeomUtils.FloatFormat), (-vertex1.Z).ToString(GeomUtils.FloatFormat),
                            (-normal1.X).ToString(GeomUtils.FloatFormat), (-normal1.Y).ToString(GeomUtils.FloatFormat), (-normal1.Z).ToString(GeomUtils.FloatFormat),
                            uv1.X.ToString(GeomUtils.FloatFormat), (1f - uv1.Y).ToString(GeomUtils.FloatFormat),
                            (color1.R * 255).ToString(GeomUtils.IntegerFormat), (color1.G * 255).ToString(GeomUtils.IntegerFormat), (color1.B * 255).ToString(GeomUtils.IntegerFormat),
                            materialIndex
                            );
                        var vertex2 = triangle.Vertices[2];
                        var normal2 = triangle.Normals[2];
                        var uv2 = triangle.Uv[2];
                        var color2 = triangle.Colors[2];
                        writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                            (-vertex2.X).ToString(GeomUtils.FloatFormat), (-vertex2.Y).ToString(GeomUtils.FloatFormat), (-vertex2.Z).ToString(GeomUtils.FloatFormat),
                            (-normal2.X).ToString(GeomUtils.FloatFormat), (-normal2.Y).ToString(GeomUtils.FloatFormat), (-normal2.Z).ToString(GeomUtils.FloatFormat),
                            uv2.X.ToString(GeomUtils.FloatFormat), (1f - uv2.Y).ToString(GeomUtils.FloatFormat),
                            (color2.R * 255).ToString(GeomUtils.IntegerFormat), (color2.G * 255).ToString(GeomUtils.IntegerFormat), (color2.B * 255).ToString(GeomUtils.IntegerFormat),
                            materialIndex
                            );
                    }
                }
                for (var j = 0; j < faceCount; j++)
                {
                    var faceIndex = j * 3;
                    writer.WriteLine("3 {2} {1} {0}", faceIndex, faceIndex + 1, faceIndex + 2);
                }
                for (var j = 0; j < numMaterials; j++)
                {
                    writer.WriteLine("128 128 128 1.00000 128 128 128 1.00000");
                }
                writer.Close();
            }
        }
    }
}