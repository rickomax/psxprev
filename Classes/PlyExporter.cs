using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenTK;

namespace PSXPrev.Classes
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
                var writer = new StreamWriter($"{selectedPath}/ply{i}.ply");
                writer.WriteLine("ply");
                writer.WriteLine("format ascii 1.0");
                var materialsDic = new Dictionary<int, int>();
                var faceCount = 0;
                var numMaterials = 0;
                foreach (ModelEntity model in entity.ChildEntities)
                {
                    faceCount += model.Triangles.Length;
                    if (model.IsTextured)
                    {
                        // todo: Handle untextured material index...
                        var texturePage = model.TexturePage;
                        if (!materialsDic.ContainsKey((int)texturePage))
                        {
                            materialsDic.Add((int)texturePage, numMaterials++);
                            pngExporter.Export(model.Texture, (int)texturePage, selectedPath);
                        }
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

                foreach (ModelEntity model in entity.ChildEntities)
                {
                    var materialIndex = materialsDic[(int) model.TexturePage];
                    var worldMatrix = model.WorldMatrix;
                    foreach (var triangle in model.Triangles)
                    {
                        // todo: Handle untextured material index...
                        for (var j = 0; j < 3; j++)
                        {
                            var vertex = Vector3.TransformPosition(triangle.Vertices[j], worldMatrix);
                            var normal = Vector3.TransformNormal(triangle.Normals[j], worldMatrix);
                            WriteVertex(writer, vertex, normal, triangle.Uv[j], triangle.Colors[j], materialIndex);
                        }
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

        private void WriteVertex(StreamWriter writer, Vector3 vertex, Vector3 normal, Vector2 uv, Color color, int materialIndex)
        {
            writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                            F(-vertex.X), F(-vertex.Y), F(-vertex.Z),
                            F(-normal.X), F(-normal.Y), F(-normal.Z),
                            F(uv.X), F(1f - uv.Y),
                            I(color.R * 255), I(color.G * 255), I(color.B * 255),
                            materialIndex);
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