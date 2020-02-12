using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;

namespace PSXPrev.Classes
{
    public static class TMDHelper
    {
        public static Dictionary<PrimitiveDataType, uint> CreateTMDPacketStructure(byte flag, byte mode, BinaryReader reader)
        {
            var option = (mode & 0x1F);
            var flagMode = option | (flag << 8);

            var lgtBit = ((flag >> 0) & 0x01) == 0; //0-lit, 1-unlit
            var fceBit = ((flag >> 1) & 0x01) == 1; //1-double faced, 0-single faced
            var grdBit = ((flag >> 2) & 0x01) == 1; //0-gradation, 1-single color

            var tgeBit = ((option >> 0) & 0x01) == 0; //brightness: 0-on, 1-off
            var abeBit = ((option >> 1) & 0x01) == 1; //translucency: 0-off, 1-on
            var tmeBit = ((option >> 2) & 0x01) == 1; //texture: 0-off, 1-on
            var isqBit = ((option >> 3) & 0x01) == 1; //1-quad, 0-tri
            var iipBit = ((option >> 4) & 0x01) == 1; //shading mode: 0-flat, 1-goraund
            var code = ((mode >> 5) & 0x03);

            return ParsePrimitiveData(reader, lgtBit, iipBit, tmeBit, grdBit, isqBit);
        }

        public static Dictionary<PrimitiveDataType, uint> CreateHMDPacketStructure(uint driver, uint primitiveType, BinaryReader reader)
        {
            var tmeBit = ((primitiveType >> 0) & 0x01) == 1;
            var colBit = ((primitiveType >> 1) & 0x01) == 1;
            var iipBit = ((primitiveType >> 2) & 0x01) == 1;
            var isqBit = ((primitiveType >> 3) & 0x07) == 2;
            return ParsePrimitiveData(reader, true, iipBit, tmeBit, colBit, isqBit);
        }

        public static string ToString(Dictionary<PrimitiveDataType, uint> primitiveDataDictionary)
        {
            return primitiveDataDictionary.Keys.Aggregate("", (current, key) => current + (key + "-"));
        }

        private static Dictionary<PrimitiveDataType, uint> ParsePrimitiveData(BinaryReader reader, bool lgtBit, bool iipBit, bool tmeBit, bool grdBit, bool isqBit)
        {
            var primitiveDataDictionary = new Dictionary<PrimitiveDataType, uint>();

            var padCount = 0;

            int GetDataLength(PrimitiveDataType dataType)
            {
                switch (dataType)
                {
                    case PrimitiveDataType.S0:
                    case PrimitiveDataType.S1:
                    case PrimitiveDataType.S2:
                    case PrimitiveDataType.S3:
                    case PrimitiveDataType.T0:
                    case PrimitiveDataType.T1:
                    case PrimitiveDataType.T2:
                    case PrimitiveDataType.T3:
                    case PrimitiveDataType.R0:
                    case PrimitiveDataType.R1:
                    case PrimitiveDataType.R2:
                    case PrimitiveDataType.R3:
                    case PrimitiveDataType.G0:
                    case PrimitiveDataType.G1:
                    case PrimitiveDataType.G2:
                    case PrimitiveDataType.G3:
                    case PrimitiveDataType.B0:
                    case PrimitiveDataType.B1:
                    case PrimitiveDataType.B2:
                    case PrimitiveDataType.B3:
                    case PrimitiveDataType.PAD1:
                    case PrimitiveDataType.Mode:
                        return 1;
                        break;
                    case PrimitiveDataType.CBA:
                    case PrimitiveDataType.TSB:
                    case PrimitiveDataType.PAD2:
                    case PrimitiveDataType.VERTEX0:
                    case PrimitiveDataType.VERTEX1:
                    case PrimitiveDataType.VERTEX2:
                    case PrimitiveDataType.VERTEX3:
                    case PrimitiveDataType.NORMAL0:
                    case PrimitiveDataType.NORMAL1:
                    case PrimitiveDataType.NORMAL2:
                    case PrimitiveDataType.NORMAL3:
                        return 2;
                        break;
                }
                return 0;
            }

            void AddData(PrimitiveDataType dataType, int? dataLength = null)
            {
                uint value = 0;
                if (dataLength == null)
                {
                    switch (dataType)
                    {
                        case PrimitiveDataType.S0:
                        case PrimitiveDataType.S1:
                        case PrimitiveDataType.S2:
                        case PrimitiveDataType.S3:
                        case PrimitiveDataType.T0:
                        case PrimitiveDataType.T1:
                        case PrimitiveDataType.T2:
                        case PrimitiveDataType.T3:
                        case PrimitiveDataType.R0:
                        case PrimitiveDataType.R1:
                        case PrimitiveDataType.R2:
                        case PrimitiveDataType.R3:
                        case PrimitiveDataType.G0:
                        case PrimitiveDataType.G1:
                        case PrimitiveDataType.G2:
                        case PrimitiveDataType.G3:
                        case PrimitiveDataType.B0:
                        case PrimitiveDataType.B1:
                        case PrimitiveDataType.B2:
                        case PrimitiveDataType.B3:
                        case PrimitiveDataType.Mode:
                            value = reader.ReadByte();
                            break;
                        case PrimitiveDataType.CBA:
                        case PrimitiveDataType.TSB:
                        case PrimitiveDataType.VERTEX0:
                        case PrimitiveDataType.VERTEX1:
                        case PrimitiveDataType.VERTEX2:
                        case PrimitiveDataType.VERTEX3:
                        case PrimitiveDataType.NORMAL0:
                        case PrimitiveDataType.NORMAL1:
                        case PrimitiveDataType.NORMAL2:
                        case PrimitiveDataType.NORMAL3:
                            value = reader.ReadUInt16();
                            break;
                        case PrimitiveDataType.PAD1:
                            value = reader.ReadByte();
                            //if (value != 0)
                            //{
                            //    var xx = 1;
                            //}
                            break;
                        case PrimitiveDataType.PAD2:
                            value = reader.ReadUInt16();
                            //if (value != 0)
                            //{
                            //    var xx = 1;
                            //}
                            break;
                    }
                }
                else
                {
                    for (var i = 0; i < dataLength.Value; i++)
                    {
                        reader.ReadByte();
                    }
                    value = (uint)dataLength.Value;
                }
                var key = dataType >= PrimitiveDataType.PAD1 ? PrimitiveDataType.PAD1 + padCount++ : dataType;
                primitiveDataDictionary.Add(key, value);
            }

            //var hasGradation = grdBit;
            //var isGouraud = iipBit;
            var hasLight = lgtBit;
            var isFlat = !iipBit;
            var hasTexture = tmeBit;
            var isSolid = !grdBit;

            var hasColors = !hasLight ||
                            !hasTexture;


            var numVerts = isqBit ? 4 : 3;

            if (hasTexture)
            {
                for (var i = 0; i < numVerts; ++i)
                {
                    AddData(PrimitiveDataType.S0 + i);
                    AddData(PrimitiveDataType.T0 + i);
                    switch (i)
                    {
                        case 0:
                            AddData(PrimitiveDataType.CBA);
                            break;
                        case 1:
                            AddData(PrimitiveDataType.TSB);
                            break;
                        default:
                            AddData(PrimitiveDataType.PAD1); //pad
                            AddData(PrimitiveDataType.PAD1); //pad
                            break;
                    }
                }
            }

            if (hasColors)
            {
                var numColors = isFlat || isSolid ? 1 : numVerts;
                for (var i = 0; i < numColors; ++i)
                {
                    AddData(PrimitiveDataType.R0 + i);
                    AddData(PrimitiveDataType.G0 + i);
                    AddData(PrimitiveDataType.B0 + i);
                    AddData(primitiveDataDictionary.Count == 0 ? PrimitiveDataType.Mode : PrimitiveDataType.PAD1);
                }
            }

            if (hasLight)
            {
                switch (numVerts)
                {
                    case 3 when isFlat:
                        AddData(PrimitiveDataType.NORMAL0);
                        AddData(PrimitiveDataType.VERTEX0);
                        AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.VERTEX2);
                        break;
                    case 3:
                        AddData(PrimitiveDataType.NORMAL0);
                        AddData(PrimitiveDataType.VERTEX0);
                        AddData(PrimitiveDataType.NORMAL1);
                        AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.NORMAL2);
                        AddData(PrimitiveDataType.VERTEX2);
                        break;
                    case 4 when isFlat:
                        AddData(PrimitiveDataType.NORMAL0);
                        AddData(PrimitiveDataType.VERTEX0);
                        AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.VERTEX2);
                        AddData(PrimitiveDataType.VERTEX3);
                        AddData(PrimitiveDataType.PAD2); //pad
                        break;
                    case 4:
                        AddData(PrimitiveDataType.NORMAL0);
                        AddData(PrimitiveDataType.VERTEX0);
                        AddData(PrimitiveDataType.NORMAL1);
                        AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.NORMAL2);
                        AddData(PrimitiveDataType.VERTEX2);
                        AddData(PrimitiveDataType.NORMAL3);
                        AddData(PrimitiveDataType.VERTEX3);
                        break;
                }
            }
            else
            {
                switch (numVerts)
                {
                    case 3:
                        AddData(PrimitiveDataType.VERTEX0);
                        AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.VERTEX2);
                        AddData(PrimitiveDataType.PAD2); //pad
                        break;
                    case 4:
                        AddData(PrimitiveDataType.VERTEX0);
                        AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.VERTEX2);
                        AddData(PrimitiveDataType.VERTEX3);
                        break;
                }
            }

            //var modBytes = packetDataLength % 4;
            //if (modBytes != 0)
            //{
            //    var padding = 4 - modBytes;
            //    AddData(PrimitiveDataType.PAD1, padding);
            //}

            return primitiveDataDictionary;
        }

        public static void AddTrianglesToGroup(Dictionary<uint, List<Triangle>> groupedTriangles, Dictionary<PrimitiveDataType, uint> primitiveData, Func<uint, Vector3> vertexCallback, Func<uint, Vector3> normalCallback)
        {
            var tPage = primitiveData.TryGetValue(PrimitiveDataType.TSB, out var tsbValue) ? tsbValue & 0xF : 0;
            void AddTriangle(Triangle triangle)
            {
                List<Triangle> triangles;
                if (groupedTriangles.ContainsKey(tPage))
                {
                    triangles = groupedTriangles[tPage];
                }
                else
                {
                    triangles = new List<Triangle>();
                    groupedTriangles.Add(tPage, triangles);
                }
                foreach (var kvp in primitiveData)
                {
                    if (kvp.Key >= PrimitiveDataType.PAD1 && kvp.Value != 0)
                    {
                        triangle.ExtraPaddingData.Add(new Tuple<uint, uint>((uint)kvp.Key, kvp.Value));
                    }
                }
                triangles.Add(triangle);
            }

            if (!primitiveData.TryGetValue(PrimitiveDataType.VERTEX0, out var vertexIndex0))
            {
                return;
            }

            foreach (var kvp in primitiveData)
            {
                if (kvp.Key >= PrimitiveDataType.PAD1 && kvp.Value != 0)
                {
                    if (Program.Debug)
                    {
                        Console.WriteLine($"Primitive contains extra padding data [{kvp.Value}]{PrintPrimitiveData(primitiveData)}");
                    }
                }
            }

            var vertex0 = vertexCallback(vertexIndex0);
            var vertex1 = primitiveData.TryGetValue(PrimitiveDataType.VERTEX1, out var vertexIndex1) ? vertexCallback(vertexIndex1) : Vector3.Zero;
            var vertex2 = primitiveData.TryGetValue(PrimitiveDataType.VERTEX2, out var vertexIndex2) ? vertexCallback(vertexIndex2) : Vector3.Zero;
            Vector3 normal0;
            Vector3 normal1;
            Vector3 normal2;
            uint normalIndex1;
            uint normalIndex2;
            if (primitiveData.TryGetValue(PrimitiveDataType.NORMAL0, out var normalIndex0))
            {
                normal0 = normalCallback(normalIndex0);
                normal1 = primitiveData.TryGetValue(PrimitiveDataType.NORMAL1, out normalIndex1) ? normalCallback(normalIndex1) : normal0;
                normal2 = primitiveData.TryGetValue(PrimitiveDataType.NORMAL2, out normalIndex2) ? normalCallback(normalIndex2) : normal0;
            }
            else
            {
                normal0 = Vector3.Cross((vertex1 - vertex0).Normalized(), (vertex2 - vertex1).Normalized());
                normal1 = normal0;
                normal2 = normal0;
                normalIndex1 = normalIndex0;
                normalIndex2 = normalIndex0;
            }
            var r0 = primitiveData.TryGetValue(PrimitiveDataType.R0, out var r0Value) ? r0Value / 255f : 0.5f;
            var r1 = primitiveData.TryGetValue(PrimitiveDataType.R1, out var r1Value) ? r1Value / 255f : r0;
            var r2 = primitiveData.TryGetValue(PrimitiveDataType.R2, out var r2Value) ? r2Value / 255f : r0;
            var g0 = primitiveData.TryGetValue(PrimitiveDataType.G0, out var g0Value) ? g0Value / 255f : 0.5f;
            var g1 = primitiveData.TryGetValue(PrimitiveDataType.G1, out var g1Value) ? g1Value / 255f : g0;
            var g2 = primitiveData.TryGetValue(PrimitiveDataType.G2, out var g2Value) ? g2Value / 255f : g0;
            var b0 = primitiveData.TryGetValue(PrimitiveDataType.B0, out var b0Value) ? b0Value / 255f : 0.5f;
            var b1 = primitiveData.TryGetValue(PrimitiveDataType.B1, out var b1Value) ? b1Value / 255f : b0;
            var b2 = primitiveData.TryGetValue(PrimitiveDataType.B2, out var b2Value) ? b2Value / 255f : b0;
            var s0 = primitiveData.TryGetValue(PrimitiveDataType.S0, out var s0Value) ? s0Value / 255f : 0f;
            var s1 = primitiveData.TryGetValue(PrimitiveDataType.S1, out var s1Value) ? s1Value / 255f : 0f;
            var s2 = primitiveData.TryGetValue(PrimitiveDataType.S2, out var s2Value) ? s2Value / 255f : 0f;
            var t0 = primitiveData.TryGetValue(PrimitiveDataType.T0, out var t0Value) ? t0Value / 255f : 0f;
            var t1 = primitiveData.TryGetValue(PrimitiveDataType.T1, out var t1Value) ? t1Value / 255f : 0f;
            var t2 = primitiveData.TryGetValue(PrimitiveDataType.T2, out var t2Value) ? t2Value / 255f : 0f;
            var triangle1 = new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                OriginalVertexIndices = new[] { vertexIndex0, vertexIndex1, vertexIndex2 },
                Normals = new[] { normal0, normal1, normal2 },
                OriginalNormalIndices = new[] { normalIndex0, normalIndex1, normalIndex2 },
                Colors = new[] { new Color(r0, g0, b0), new Color(r1, g1, b1), new Color(r2, g2, b2) },
                Uv = new[] { new Vector3(s0, t0, 0f), new Vector3(s1, t1, 0f), new Vector3(s2, t2, 0f) },
                AttachableIndices = new[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }
            };
            AddTriangle(triangle1);
            if (primitiveData.TryGetValue(PrimitiveDataType.VERTEX3, out var vertexIndex3))
            {
                var vertex3 = vertexCallback(vertexIndex3);
                var normal3 = primitiveData.TryGetValue(PrimitiveDataType.NORMAL3, out var normalIndex3) ? normalCallback(normalIndex3) : normal0;
                var g3 = primitiveData.TryGetValue(PrimitiveDataType.G3, out var g3Value) ? g3Value / 255f : g0;
                var r3 = primitiveData.TryGetValue(PrimitiveDataType.R3, out var r3Value) ? r3Value / 255f : r0;
                var b3 = primitiveData.TryGetValue(PrimitiveDataType.B3, out var b3Value) ? b3Value / 255f : b0;
                var s3 = primitiveData.TryGetValue(PrimitiveDataType.S3, out var s3Value) ? s3Value / 255f : 0f;
                var t3 = primitiveData.TryGetValue(PrimitiveDataType.T3, out var t3Value) ? t3Value / 255f : 0f;
                var triangle2 = new Triangle
                {
                    Vertices = new[] { vertex1, vertex3, vertex2 },
                    OriginalVertexIndices = new[] { vertexIndex1, vertexIndex3, vertexIndex2 },
                    Normals = new[] { normal1, normal3, normal2 },
                    OriginalNormalIndices = new[] { normalIndex1, normalIndex3, normalIndex2 },
                    Colors = new[] { new Color(r1, g1, b1), new Color(r3, g3, b3), new Color(r2, g2, b2) },
                    Uv = new[] { new Vector3(s1, t1, 0f), new Vector3(s3, t3, 0f), new Vector3(s2, t2, 0f) },
                    AttachableIndices = new[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }
                };
                AddTriangle(triangle2);
            }
        }

        private static string PrintPrimitiveData(Dictionary<PrimitiveDataType, uint> primitiveData)
        {
            var value = new StringBuilder("[");
            foreach (var kvp in primitiveData)
            {
                value.Append(kvp.Key).Append(":").Append(kvp.Value).Append("|");
            }

            value.Append("]");
            return value.ToString();
        }

        public static float ConvertNormal(int value)
        {
            return value == 0 ? value : value / 4096f;
        }
    }
}