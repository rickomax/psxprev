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
        public static Dictionary<PrimitiveDataType, uint> CreateTMDPacketStructure(byte flag, byte mode, BinaryReader reader, int index, out RenderFlags renderFlags, out PrimitiveType primitiveType)
        {
            var option = (mode & 0x1F);

            var lgtBit = ((flag >> 0) & 0x01) == 0; //0-lit, 1-unlit
            var fceBit = ((flag >> 1) & 0x01) == 1; //1-double faced, 0-single faced
            var grdBit = ((flag >> 2) & 0x01) == 1 || mode == 0x35 || mode == 0x31 || mode == 0x39 || mode == 0x3d || mode == 0x3b || mode == 0x3f; //0-gradation, 1-single color

            var tgeBit = ((option >> 0) & 0x01) == 0; //brightness: 0-on, 1-off
            var abeBit = ((option >> 1) & 0x01) == 1; //translucency: 0-off, 1-on
            var tmeBit = ((option >> 2) & 0x01) == 1; //texture: 0-off, 1-on
            var isqBit = ((option >> 3) & 0x01) == 1; //1-quad, 0-tri
            var iipBit = ((option >> 4) & 0x01) == 1; //shading mode: 0-flat, 1-goraund
            var code = ((mode >> 5) & 0x03);

            renderFlags = RenderFlags.None;
            if (!lgtBit) renderFlags |= RenderFlags.Unlit;
            if (fceBit) renderFlags |= RenderFlags.DoubleSided;
            if (abeBit) renderFlags |= RenderFlags.SemiTransparent;

            if (Program.Debug)
            {
                Program.Logger.WriteLine("[{9}] mode: {8:x2} light:{0} double-faced:{1} gradation:{2} brightness:{3} translucency:{4} texture:{5} quad:{6} gouraud:{7}",
                    lgtBit ? 1 : 0,
                    fceBit ? 1 : 0,
                    grdBit ? 1 : 0, 
                    tgeBit ? 1 : 0,
                    abeBit ? 1 : 0, 
                    tmeBit ? 1 : 0, 
                    isqBit ? 1 : 0,
                    iipBit ? 1 : 0,
                    mode,
                    index);
            }
            primitiveType = (PrimitiveType)code;
            if (primitiveType != PrimitiveType.Polygon)
            {
                if (Program.Debug)
                {
                    Program.Logger.WriteErrorLine($"Unsupported primitive code:{primitiveType}");
                }
            }
            return ParsePrimitiveData(reader, false, lgtBit, iipBit, tmeBit, grdBit, isqBit, abeBit, false);
        }

        public static Dictionary<PrimitiveDataType, uint> CreateHMDPacketStructure(uint driver, uint flag, BinaryReader reader, out RenderFlags renderFlags, out PrimitiveType primitiveType)
        {
            var tmeBit = ((flag >> 0) & 0x01) == 1; // Texture: 0-Off, 1-On
            var colBit = ((flag >> 1) & 0x01) == 1; // Color: 0-Single, 1-Separate
            var iipBit = ((flag >> 2) & 0x01) == 1; // Shading: 0-Flat, 1-Gouraud
            var code   = ((flag >> 3) & 0x07); // Polygon: 1-Triangle, 2-Quad, 3-Mesh
            var lmdBit = ((flag >> 6) & 0x01) == 1; // Normal: 0-Off, 1-On
            var tileBit = ((flag >> 9) & 0x01) == 1; // Tile: 0-Off, 1-On
            var quad = code == 2;

            var divBit = ((driver >> 0) & 0x01) == 1; // Subdivision: 0-Off, 1-On
            var fogBit = ((driver >> 1) & 0x01) == 1; // Fog: 0-Off, 1-On
            var lgtBit = ((driver >> 2) & 0x01) == 0; // Light: 0-Lit, 1-Unlit
            var advBit = ((driver >> 3) & 0x01) == 1; // Automatic division: 0-Off, 1-On
            var botBit = ((driver >> 4) & 0x01) == 1; // Both sides: 0-Single sided, 1-Double sided
            var stpBit = ((driver >> 5) & 0x01) == 1; // Semi-transparency: 0-Preserve, 1-On
            
            renderFlags = RenderFlags.None;
            if (divBit) renderFlags |= RenderFlags.Subdivision;
            if (fogBit) renderFlags |= RenderFlags.Fog;
            if (!lgtBit) renderFlags |= RenderFlags.Unlit;
            if (advBit) renderFlags |= RenderFlags.AutomaticDivision;
            if (botBit) renderFlags |= RenderFlags.DoubleSided;
            if (stpBit) renderFlags |= RenderFlags.SemiTransparent;

            primitiveType = (PrimitiveType)code;
            if (primitiveType != PrimitiveType.Polygon)
            {
                if (Program.Debug)
                {
                    Program.Logger.WriteErrorLine($"Unsupported primitive code:{primitiveType}");
                }
            }

            return ParsePrimitiveData(reader, true, lgtBit, iipBit, tmeBit, colBit, quad, stpBit, tileBit);
        }

        public static string ToString(Dictionary<PrimitiveDataType, uint> primitiveDataDictionary)
        {
            return primitiveDataDictionary.Keys.Aggregate("", (current, key) => current + (key + "-"));
        }

        private static Dictionary<PrimitiveDataType, uint> ParsePrimitiveData(BinaryReader reader, bool hmd, bool light, bool gouraud, bool texture, bool gradation, bool quad, bool translucency, bool tiled)
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
                    case PrimitiveDataType.TILE:
                        return 4;
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
                        case PrimitiveDataType.TILE:
                            value = reader.ReadUInt32();
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

            var hasColors = (!quad &&  light && !texture) ||
                            ( quad &&  light && !texture) ||
                            (!quad && !light) ||
                            ( quad && !light) ||
                            (hmd && gradation);

            var numVerts = quad ? 4 : 3;


            // HMD: PAD2 appearing after vertices. Handle this differently from TMD since it's more complex.
            var hmdVertexEndPad = hmd && (( quad &&  light && !texture && !gouraud) ||
                                          (!quad && !light && !texture));

            // HMD: Early definitions of Normal0, Vertex0, and/or Vertex1 after UV coordinates (instead of padding).
            var uv2PostData = PrimitiveDataType.PAD1;
            var uv3PostData = PrimitiveDataType.PAD1;
            var skipFirstVertexNormal = false;
            var skipSecondVertexNormal = false;
            if (hmd)
            {
                if (quad && light && texture && !gouraud)
                {
                    uv3PostData = PrimitiveDataType.NORMAL0;
                    skipFirstVertexNormal = true;
                }
                else if (quad && light && texture && gouraud)
                {
                    uv2PostData = PrimitiveDataType.NORMAL0;
                    uv3PostData = PrimitiveDataType.VERTEX0;
                    skipFirstVertexNormal = true;
                    skipSecondVertexNormal = true;
                }
                else if (!quad && !light && texture)
                {
                    uv2PostData = PrimitiveDataType.VERTEX0;
                    skipFirstVertexNormal = true;
                }
                else if (quad && !light && texture)
                {
                    uv2PostData = PrimitiveDataType.VERTEX0;
                    uv3PostData = PrimitiveDataType.VERTEX1;
                    skipFirstVertexNormal = true;
                    skipSecondVertexNormal = true;
                }
            }
            
            
            // HMD: Tiled texture information.
            if (tiled)
            {
                AddData(PrimitiveDataType.TILE);
            }

            // HMD: Colors come before UVs.
            if (hmd && hasColors)
            {
                var numColors = gradation ? numVerts : 1;
                for (var i = 0; i < numColors; ++i)
                {
                    AddData(PrimitiveDataType.R0 + i);
                    AddData(PrimitiveDataType.G0 + i);
                    AddData(PrimitiveDataType.B0 + i);
                    AddData(primitiveDataDictionary.Count == 0 ? PrimitiveDataType.Mode : PrimitiveDataType.PAD1);
                }
            }

            if (texture)
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
                        case 2:
                            if (uv2PostData < PrimitiveDataType.PAD1)
                            {
                                AddData(uv2PostData); // NORMAL0 or VERTEX0
                            }
                            else
                            {
                                AddData(PrimitiveDataType.PAD1); //pad
                                AddData(PrimitiveDataType.PAD1); //pad
                            }
                            break;
                        case 3:
                            if (uv3PostData < PrimitiveDataType.PAD1)
                            {
                                AddData(uv3PostData); // NORMAL0, VERTEX0, or VERTEX1
                            }
                            else
                            {
                                AddData(PrimitiveDataType.PAD1); //pad
                                AddData(PrimitiveDataType.PAD1); //pad
                            }
                            break;
                    }
                }
            }

            // TMD: Colors come after UVs.
            if (!hmd && hasColors)
            {
                var numColors = gradation ? numVerts : 1;
                for (var i = 0; i < numColors; ++i)
                {
                    AddData(PrimitiveDataType.R0 + i);
                    AddData(PrimitiveDataType.G0 + i);
                    AddData(PrimitiveDataType.B0 + i);
                    AddData(primitiveDataDictionary.Count == 0 ? PrimitiveDataType.Mode : PrimitiveDataType.PAD1);
                }
            }

            if (light)
            {
                switch (numVerts)
                {
                    case 3 when !gouraud:
                        if (!skipFirstVertexNormal)  AddData(PrimitiveDataType.NORMAL0);
                        if (!skipSecondVertexNormal) AddData(PrimitiveDataType.VERTEX0);
                        AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.VERTEX2);
                        break;
                    case 3:
                        if (!skipFirstVertexNormal)  AddData(PrimitiveDataType.NORMAL0);
                        if (!skipSecondVertexNormal) AddData(PrimitiveDataType.VERTEX0);
                        AddData(PrimitiveDataType.NORMAL1);
                        AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.NORMAL2);
                        AddData(PrimitiveDataType.VERTEX2);
                        break;
                    case 4 when !gouraud:
                        if (!skipFirstVertexNormal)  AddData(PrimitiveDataType.NORMAL0);
                        if (!skipSecondVertexNormal) AddData(PrimitiveDataType.VERTEX0);
                        AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.VERTEX2);
                        AddData(PrimitiveDataType.VERTEX3);
                        if (!hmd)
                        {
                            AddData(PrimitiveDataType.PAD2); //pad
                        }
                        break;
                    case 4:
                        if (!skipFirstVertexNormal)  AddData(PrimitiveDataType.NORMAL0);
                        if (!skipSecondVertexNormal) AddData(PrimitiveDataType.VERTEX0);
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
                        if (!skipFirstVertexNormal)  AddData(PrimitiveDataType.VERTEX0);
                        if (!skipSecondVertexNormal) AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.VERTEX2);
                        if (!hmd)
                        {
                            AddData(PrimitiveDataType.PAD2); //pad
                        }
                        break;
                    case 4:
                        if (!skipFirstVertexNormal)  AddData(PrimitiveDataType.VERTEX0);
                        if (!skipSecondVertexNormal) AddData(PrimitiveDataType.VERTEX1);
                        AddData(PrimitiveDataType.VERTEX2);
                        AddData(PrimitiveDataType.VERTEX3);
                        break;
                }
            }

            if (hmdVertexEndPad)
            {
                AddData(PrimitiveDataType.PAD2); //pad
            }

            //var modBytes = packetDataLength % 4;
            //if (modBytes != 0)
            //{
            //    var padding = 4 - modBytes;
            //    AddData(PrimitiveDataType.PAD1, padding);
            //}

            return primitiveDataDictionary;
        }

        public static HashSet<MixtureRate> mixtureRates = new HashSet<MixtureRate>();

        public static void AddTrianglesToGroup(PrimitiveType primitiveType, Dictionary<RenderInfo, List<Triangle>> groupedTriangles, Dictionary<PrimitiveDataType, uint> primitiveData, RenderFlags renderFlags, bool attached, Func<uint, Vector3> vertexCallback, Func<uint, Vector3> normalCallback)
        {
            var tPage = primitiveData.TryGetValue(PrimitiveDataType.TSB, out var tsbValue) ? tsbValue & 0x1F : 0;

            var abr = (tsbValue >> 5) & 0x3; // Mixture rate: 0- 50%back+50%poly, 1- 100%back+100%poly, 2- 100%back-100%poly, 3- 100%back+25%poly
            //var tpf = (tsbValue >> 7) & 0x3; // Color mode: 0-4bit, 1-8bit, 2-15bit

            var mixtureRate = MixtureRate.None;
            if (renderFlags.HasFlag(RenderFlags.SemiTransparent))
            {
                switch (abr)
                {
                    case 0:
                        mixtureRate = MixtureRate.Back50_Poly50;
                        break;
                    case 1:
                        mixtureRate = MixtureRate.Back100_Poly100;
                        break;
                    case 2:
                        mixtureRate = MixtureRate.Back100_PolyM100;
                        break;
                    case 3:
                        mixtureRate = MixtureRate.Back100_Poly25;
                        break;
                }
            }

            var renderInfo = new RenderInfo(tPage, renderFlags, mixtureRate);

            void AddTriangle(Triangle triangle)
            {
                if (!groupedTriangles.TryGetValue(renderInfo, out var triangles))
                {
                    triangles = new List<Triangle>();
                    groupedTriangles.Add(renderInfo, triangles);
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

            if (Program.Debug)
            {
                Program.Logger.WriteLine($"Primitive data: {PrintPrimitiveData(primitiveData)}");
            }

            var vertex0 = vertexCallback(vertexIndex0);
            var vertex1 = primitiveData.TryGetValue(PrimitiveDataType.VERTEX1, out var vertexIndex1) ? vertexCallback(vertexIndex1) : Vector3.Zero;
            var vertex2 = primitiveData.TryGetValue(PrimitiveDataType.VERTEX2, out var vertexIndex2) ? vertexCallback(vertexIndex2) : Vector3.Zero;
            bool hasNormals;
            Vector3 normal0;
            Vector3 normal1;
            Vector3 normal2;
            uint normalIndex1;
            uint normalIndex2;
            if (primitiveData.TryGetValue(PrimitiveDataType.NORMAL0, out var normalIndex0))
            {
                hasNormals = true;
                normal0 = normalCallback(normalIndex0);
                normal1 = primitiveData.TryGetValue(PrimitiveDataType.NORMAL1, out normalIndex1) ? normalCallback(normalIndex1) : normal0;
                normal2 = primitiveData.TryGetValue(PrimitiveDataType.NORMAL2, out normalIndex2) ? normalCallback(normalIndex2) : normal0;
            }
            else
            {
                hasNormals = false;
                normal0 = Vector3.Cross((vertex1 - vertex0).Normalized(), (vertex2 - vertex1).Normalized());
                normal1 = normal0;
                normal2 = normal0;
                normalIndex1 = normalIndex0;
                normalIndex2 = normalIndex0;
            }

            // HMD: Tiled information for textures.
            // Default values when there's no tiled information.
            uint tum = 0;
            uint tvm = 0;
            uint tua = 0;
            uint tva = 0;
            if (primitiveData.TryGetValue(PrimitiveDataType.TILE, out var tile))
            {
                tum = (tile >>  0) & 0x1f;
                tvm = (tile >>  5) & 0x1f;
                tua = (tile >> 10) & 0x1f;
                tva = (tile >> 15) & 0x1f;
            }
            // We can wrap these around all S/T getters since these functions
            // will not change S/T values if there is no tiled information.
            uint TileS(uint sValue)
            {
                return (~(tum << 3) & sValue) | ((tum << 3) & (tua << 3));
            }
            uint TileT(uint tValue)
            {
                return (~(tvm << 3) & tValue) | ((tvm << 3) & (tva << 3));
            }

            var r0 = primitiveData.TryGetValue(PrimitiveDataType.R0, out var r0Value) ? r0Value / 255f : Color.DefaultColorTone;
            var r1 = primitiveData.TryGetValue(PrimitiveDataType.R1, out var r1Value) ? r1Value / 255f : r0;
            var r2 = primitiveData.TryGetValue(PrimitiveDataType.R2, out var r2Value) ? r2Value / 255f : r0;
            var g0 = primitiveData.TryGetValue(PrimitiveDataType.G0, out var g0Value) ? g0Value / 255f : Color.DefaultColorTone;
            var g1 = primitiveData.TryGetValue(PrimitiveDataType.G1, out var g1Value) ? g1Value / 255f : g0;
            var g2 = primitiveData.TryGetValue(PrimitiveDataType.G2, out var g2Value) ? g2Value / 255f : g0;
            var b0 = primitiveData.TryGetValue(PrimitiveDataType.B0, out var b0Value) ? b0Value / 255f : Color.DefaultColorTone;
            var b1 = primitiveData.TryGetValue(PrimitiveDataType.B1, out var b1Value) ? b1Value / 255f : b0;
            var b2 = primitiveData.TryGetValue(PrimitiveDataType.B2, out var b2Value) ? b2Value / 255f : b0;
            var s0 = primitiveData.TryGetValue(PrimitiveDataType.S0, out var s0Value) ? TileS(s0Value) / 255f : 0f;
            var s1 = primitiveData.TryGetValue(PrimitiveDataType.S1, out var s1Value) ? TileS(s1Value) / 255f : 0f;
            var s2 = primitiveData.TryGetValue(PrimitiveDataType.S2, out var s2Value) ? TileS(s2Value) / 255f : 0f;
            var t0 = primitiveData.TryGetValue(PrimitiveDataType.T0, out var t0Value) ? TileT(t0Value) / 255f : 0f;
            var t1 = primitiveData.TryGetValue(PrimitiveDataType.T1, out var t1Value) ? TileT(t1Value) / 255f : 0f;
            var t2 = primitiveData.TryGetValue(PrimitiveDataType.T2, out var t2Value) ? TileT(t2Value) / 255f : 0f;
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
            if (attached)
            {
                // HMD: Attached (shared) indices from other model entities.
                triangle1.AttachedIndices = (uint[])triangle1.OriginalVertexIndices.Clone();
                if (hasNormals)
                {
                    triangle1.AttachedNormalIndices = (uint[])triangle1.OriginalNormalIndices.Clone();
                }
            }
            AddTriangle(triangle1);
            if (primitiveData.TryGetValue(PrimitiveDataType.VERTEX3, out var vertexIndex3))
            {
                var vertex3 = vertexCallback(vertexIndex3);
                var normal3 = primitiveData.TryGetValue(PrimitiveDataType.NORMAL3, out var normalIndex3) ? normalCallback(normalIndex3) : normal0;
                var g3 = primitiveData.TryGetValue(PrimitiveDataType.G3, out var g3Value) ? g3Value / 255f : g0;
                var r3 = primitiveData.TryGetValue(PrimitiveDataType.R3, out var r3Value) ? r3Value / 255f : r0;
                var b3 = primitiveData.TryGetValue(PrimitiveDataType.B3, out var b3Value) ? b3Value / 255f : b0;
                var s3 = primitiveData.TryGetValue(PrimitiveDataType.S3, out var s3Value) ? TileS(s3Value) / 255f : 0f;
                var t3 = primitiveData.TryGetValue(PrimitiveDataType.T3, out var t3Value) ? TileT(t3Value) / 255f : 0f;
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
                if (attached)
                {
                    // HMD: Attached (shared) indices from other model entities.
                    triangle2.AttachedIndices = (uint[])triangle2.OriginalVertexIndices.Clone();
                    if (hasNormals)
                    {
                        triangle2.AttachedNormalIndices = (uint[])triangle2.OriginalNormalIndices.Clone();
                    }
                }
                AddTriangle(triangle2);
            }
        }

        private static string PrintPrimitiveData(Dictionary<PrimitiveDataType, uint> primitiveData)
        {
            var value = new StringBuilder();
            foreach (var kvp in primitiveData)
            {
                value.Append("\t").Append(kvp.Key).Append(":").Append(kvp.Value);
            }
            return value.ToString();
        }

        public static float ConvertNormal(int value)
        {
            return value == 0 ? value : value / 4096f;
        }
    }
}
