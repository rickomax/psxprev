﻿using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public static class TMDHelper
    {
        public delegate Vector3 VertexNormalCallback(uint index, out uint joint);

        public static PrimitiveData CreateTMDPacketStructure(byte flag, byte mode, BinaryReader reader, uint index)
        {
            var lgtBit = ((flag >> 0) & 0x01) == 0; // Light: 0-Lit, 1-Unlit
            var fceBit = ((flag >> 1) & 0x01) == 1; // Both sides: 0-Single sided, 1-Double sided
            var grdBit = ((flag >> 2) & 0x01) == 1; // Colors: 0-Single, 1-Separate (gradation)

            var tgeBit = ((mode >> 0) & 0x01) == 0; // Brightness: 0-On, 1-Off
            var abeBit = ((mode >> 1) & 0x01) == 1; // Semi-transparency: 0-Off, 1-On
            var tmeBit = ((mode >> 2) & 0x01) == 1; // Texture: 0-Off, 1-On
            var isqBit = ((mode >> 3) & 0x01) == 1; // Polygon: 0-Triangle, 1-Quad
            var iipBit = ((mode >> 4) & 0x01) == 1; // Shading: 0-Flat, 1-Gouraud (separate colors when !lgtBit)
            var code = ((mode >> 5) & 0x07); // Code: 1-Polygon, 2-Straight Line, 3-Sprite

            // todo: This was the case with two PsyQ TMD models (mode=0x21), but it's not clear if they were actually readable...
            // Maybe the brightness bit is what actually determines if normals are present, similar to lmdBit for HMD...?
            //if (lgtBit && !tgeBit)
            //{
            //    lgtBit = false;
            //}

            var renderFlags = RenderFlags.None;
            if (!lgtBit) renderFlags |= RenderFlags.Unlit;
            if (fceBit) renderFlags |= RenderFlags.DoubleSided;
            if (abeBit) renderFlags |= RenderFlags.SemiTransparent;
            if (tmeBit) renderFlags |= RenderFlags.Textured;

            if (Program.Debug)
            {
                Program.Logger.WriteLine("[{9}] mode: {8:x2} light:{0} double-sided:{1} gradation:{2} brightness:{3} translucency:{4} texture:{5} quad:{6} gouraud:{7}",
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
            var primitiveType = PrimitiveType.None; // invalid
            var supported = false;
            switch (code)
            {
                case 1 when !isqBit:
                    primitiveType = PrimitiveType.Triangle;
                    supported = true;
                    break;
                case 1:
                    primitiveType = PrimitiveType.Quad;
                    supported = true;
                    break;
                case 2:
                    primitiveType = PrimitiveType.StraightLine;
                    supported = true; // WIP, untested
                    break;
                case 3:
                    primitiveType = PrimitiveType.Sprite;
                    supported = true; // WIP, untested
                    break;
            }

            if (!supported)
            {
                if (Program.Debug)
                {
                    Program.Logger.WriteWarningLine($"Unsupported TMD primitive code:{code}");
                }
            }

            if (primitiveType == PrimitiveType.Sprite)
            {
                var sizCode = ((mode >> 3) & 0x03);
                uint size = 0;
                switch (sizCode)
                {
                    case 0: size =  0; break; // Free size
                    case 1: size =  1; break; // 1x1
                    case 2: size =  8; break; // 8x8
                    case 3: size = 16; break; // 16x16
                }

                renderFlags |= RenderFlags.Unlit | RenderFlags.Textured | RenderFlags.Sprite;
                return ParseSpritePrimitiveData(reader, primitiveType, renderFlags, size);
            }
            else if (primitiveType == PrimitiveType.StraightLine)
            {
                renderFlags |= RenderFlags.Unlit | RenderFlags.Line; // Only use first-two vertices with Line flag
                renderFlags &= ~RenderFlags.Textured;
                return ParseLinePrimitiveData(reader, primitiveType, renderFlags, iipBit);
            }
            else
            {
                return ParsePrimitiveData(reader, primitiveType, renderFlags, false, lgtBit, iipBit, tmeBit, grdBit, false);
            }
        }

        public static PrimitiveData CreateHMDPacketStructure(uint driver, uint flag, BinaryReader reader)
        {
            var tmeBit = ((flag >> 0) & 0x01) == 1; // Texture: 0-Off, 1-On
            var colBit = ((flag >> 1) & 0x01) == 1; // Colors: 0-Single, 1-Separate
            var iipBit = ((flag >> 2) & 0x01) == 1; // Shading: 0-Flat, 1-Gouraud (separate colors when !lgtBit)
            var code   = ((flag >> 3) & 0x07);      // Polygon: 1-Triangle, 2-Quad, 3-Strip Mesh
            var lmdBit = ((flag >> 6) & 0x01) == 0; // Normals: 0-On, 1-Off
            var mipBit = ((flag >> 7) & 0x01) == 1; // MIP map: 0-Off, 1-On (not implemented by spec)
            var pstBit = ((flag >> 8) & 0x01) == 1; // Presets: 0-Off, 1-On
            var tileBit = ((flag >> 9) & 0x01) == 1; // Tiled: 0-Off, 1-On
            var mimeBit = ((flag >> 10) & 0x01) == 1; // MIMe polygon: 0-Normal, 1-MIMe (not implemented by spec)
            var quad = code == 2;

            var divBit = ((driver >> 0) & 0x01) == 1; // Subdivision: 0-Off, 1-On
            var fogBit = ((driver >> 1) & 0x01) == 1; // Fog: 0-Off, 1-On
            var lgtBit = ((driver >> 2) & 0x01) == 0; // Light: 0-Lit, 1-Unlit
            var advBit = ((driver >> 3) & 0x01) == 1; // Automatic division: 0-Off, 1-On
            var botBit = ((driver >> 4) & 0x01) == 1; // Both sides: 0-Single sided, 1-Double sided
            var stpBit = ((driver >> 5) & 0x01) == 1; // Semi-transparency: 0-Off, 1-On
            
            // Note: lmdBit should always match lgtBit (normals are only used with light source calculation).

            var renderFlags = RenderFlags.None;
            if (divBit) renderFlags |= RenderFlags.Subdivision;
            if (fogBit) renderFlags |= RenderFlags.Fog;
            if (!lgtBit) renderFlags |= RenderFlags.Unlit;
            if (advBit) renderFlags |= RenderFlags.AutomaticDivision;
            if (botBit) renderFlags |= RenderFlags.DoubleSided;
            if (stpBit) renderFlags |= RenderFlags.SemiTransparent;
            if (tmeBit) renderFlags |= RenderFlags.Textured;

            var primitiveType = PrimitiveType.None; // invalid
            var supported = false;
            switch (code)
            {
                case 1:
                    primitiveType = PrimitiveType.Triangle;
                    supported = true;
                    break;
                case 2:
                    primitiveType = PrimitiveType.Quad;
                    supported = true;
                    break;
                case 3:
                    primitiveType = PrimitiveType.StripMesh;
                    supported = true;
                    break;
            }

            //Program.Logger.WriteLine($"  div:{(divBit?1:0)} fog:{(fogBit?1:0)} lgt:{(lgtBit?1:0)} adv:{(advBit?1:0)} bot:{(botBit?1:0)} stp:{(stpBit?1:0)}");
            //Program.Logger.WriteLine($"  tme:{(tmeBit?1:0)} col:{(colBit?1:0)} iip:{(iipBit?1:0)} code:{code} lmd:{(lmdBit?1:0)} mip:{(mipBit?1:0)} pst:{(pstBit?1:0)} tile:{(tileBit?1:0)} mime:{(mimeBit?1:0)}");

            if (!supported)
            {
                if (Program.Debug)
                {
                    Program.Logger.WriteWarningLine($"Unsupported HMD primitive code:{code}");
                }
            }
            if (pstBit)
            {
                if (Program.Debug)
                {
                    Program.Logger.WriteWarningLine("Unsupported HMD primitive flag:Presets");
                }
            }

            return ParsePrimitiveData(reader, primitiveType, renderFlags, true, lgtBit, iipBit, tmeBit, colBit, tileBit);
        }

        private static PrimitiveData ParseSpritePrimitiveData(BinaryReader reader, PrimitiveType primitiveType, RenderFlags renderFlags, uint size)
        {
            var primitiveData = new PrimitiveData
            {
                PrimitiveType = primitiveType,
                RenderFlags = renderFlags,
            };

            void ReadData(PrimitiveDataType dataType)
            {
                primitiveData.ReadData(reader, 0, dataType);
            }

            ReadData(PrimitiveDataType.VERTEX0);
            ReadData(PrimitiveDataType.TSB);
            ReadData(PrimitiveDataType.U0);
            ReadData(PrimitiveDataType.V0);
            ReadData(PrimitiveDataType.CBA);
            if (size == 0)
            {
                ReadData(PrimitiveDataType.W);
                ReadData(PrimitiveDataType.H);
            }
            else
            {
                primitiveData.Add(0, PrimitiveDataType.W, size);
                primitiveData.Add(0, PrimitiveDataType.H, size);
            }

            return primitiveData;
        }

        private static PrimitiveData ParseLinePrimitiveData(BinaryReader reader, PrimitiveType primitiveType, RenderFlags renderFlags, bool gouraud)
        {
            var primitiveData = new PrimitiveData
            {
                PrimitiveType = primitiveType,
                RenderFlags = renderFlags,
            };

            void ReadData(PrimitiveDataType dataType)
            {
                primitiveData.ReadData(reader, 0, dataType);
            }

            var numVerts = 2;
            var numColors = gouraud ? numVerts : 1;

            for (var i = 0; i < numColors; i++)
            {
                ReadData(PrimitiveDataType.R0 + i);
                ReadData(PrimitiveDataType.G0 + i);
                ReadData(PrimitiveDataType.B0 + i);
                ReadData(i == 0 ? PrimitiveDataType.Mode : PrimitiveDataType.PAD1);
            }

            ReadData(PrimitiveDataType.VERTEX0);
            ReadData(PrimitiveDataType.VERTEX1);
            // Add VERTEX2 so that we can parse just like a triangle
            primitiveData.Add(0, PrimitiveDataType.VERTEX2, primitiveData[0, PrimitiveDataType.VERTEX1]);

            return primitiveData;
        }

        private static PrimitiveData ParsePrimitiveData(BinaryReader reader, PrimitiveType primitiveType, RenderFlags renderFlags, bool hmd, bool light, bool gouraud, bool texture, bool gradation, bool tiled)
        {
            var primitiveData = new PrimitiveData
            {
                PrimitiveType = primitiveType,
                RenderFlags = renderFlags,
            };

            void ReadData(int index, PrimitiveDataType dataType)
            {
                primitiveData.ReadData(reader, index, dataType);
            }

            // Mesh packet structure is the same as a triangle in most cases.
            var mesh = primitiveType == PrimitiveType.StripMesh;// || primitiveType == PrimitiveType.RoundedMesh;
            var tri  = primitiveType == PrimitiveType.Triangle || mesh;
            var quad = primitiveType == PrimitiveType.Quad;

            var hasColors = ( light && !texture) ||
                            (!light) ||
                            (hmd && gradation); // todo: Is this the same for TMD too?

            var numVerts = quad ? 4 : 3;
            var numColors = gradation ? numVerts : 1;
            // Gouraud surfaces without light have colors for each vertex (regardless of gradation).
            if (!light && gouraud)
            {
                numColors = numVerts;
            }


            // PAD2 appearing after vertices and normals.
            var vertexEndPad = ( hmd && quad &&  light && !texture && !gouraud) ||
                               ( hmd &&  tri && !light && !texture) ||
                               (!hmd && quad &&  light && !gouraud) ||
                               (!hmd &&  tri && !light);

            // HMD: Early definitions of Normal0, Vertex0, and/or Vertex1 after UV coordinates (instead of padding).
            var uv2PostData = PrimitiveDataType.PAD2;
            var uv3PostData = PrimitiveDataType.PAD2;
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
                else if (tri && !light && texture)
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

            // HMD: Meshes define a base packet, then repeat packets with 1 vertex defined per repeat.
            if (mesh)
            {
                var meshLength = reader.ReadUInt16(); // Number of triangles (including initial packet data)
                // For now, don't check the lower bounds of meshLength.
                // It's possible that 0 is treated as 1 because its not accounted for.
                // And note that SetMeshLength already ensures length is at least 1.
                //if (meshLength < Limits.MinHMDStripMeshLength)
                //{
                //    return null;
                //}
                if (meshLength > Limits.MaxHMDStripMeshLength)
                {
                    return null;
                }
                primitiveData.SetMeshLength(meshLength);
                reader.ReadUInt16(); //pad
            }

            for (var m = 0; m < primitiveData.MeshLength; m++)
            {
                // HMD: Tiled texture information.
                if (tiled)
                {
                    ReadData(m, PrimitiveDataType.TILE);
                }

                // HMD: Colors come before UVs.
                if (hmd && hasColors)
                {
                    for (var i = 0; i < numColors; i++)
                    {
                        ReadData(m, PrimitiveDataType.R0 + i);
                        ReadData(m, PrimitiveDataType.G0 + i);
                        ReadData(m, PrimitiveDataType.B0 + i);
                        ReadData(m, i == 0 ? PrimitiveDataType.Mode : PrimitiveDataType.PAD1);
                    }
                }

                if (texture)
                {
                    for (var i = 0; i < numVerts; i++)
                    {
                        ReadData(m, PrimitiveDataType.U0 + i);
                        ReadData(m, PrimitiveDataType.V0 + i);
                        switch (i)
                        {
                            case 0:
                                ReadData(m, PrimitiveDataType.CBA);
                                break;
                            case 1:
                                ReadData(m, PrimitiveDataType.TSB);
                                break;
                            case 2:
                                ReadData(m, uv2PostData); // PAD2, NORMAL0, or VERTEX0
                                break;
                            case 3:
                                ReadData(m, uv3PostData); // PAD2, NORMAL0, VERTEX0, or VERTEX1
                                break;
                        }
                    }
                }

                // TMD: Colors come after UVs.
                if (!hmd && hasColors)
                {
                    for (var i = 0; i < numColors; i++)
                    {
                        ReadData(m, PrimitiveDataType.R0 + i);
                        ReadData(m, PrimitiveDataType.G0 + i);
                        ReadData(m, PrimitiveDataType.B0 + i);
                        ReadData(m, i == 0 ? PrimitiveDataType.Mode : PrimitiveDataType.PAD1);
                    }
                }

                if (light)
                {
                    switch (numVerts)
                    {
                        case 3 when m > 0 && !gouraud: // HMD: Strip mesh repeat
                            if (!skipFirstVertexNormal)  ReadData(m, PrimitiveDataType.NORMAL0);
                            if (!skipSecondVertexNormal) ReadData(m, PrimitiveData.MESHVERTEX);
                            break;
                        case 3 when m > 0:             // HMD: Strip mesh repeat
                            if (!skipFirstVertexNormal)  ReadData(m, PrimitiveDataType.NORMAL0);
                            if (!skipSecondVertexNormal) ReadData(m, PrimitiveDataType.NORMAL1);
                            ReadData(m, PrimitiveDataType.NORMAL2);
                            ReadData(m, PrimitiveData.MESHVERTEX);
                            break;
                        case 3 when !gouraud:
                            if (!skipFirstVertexNormal)  ReadData(m, PrimitiveDataType.NORMAL0);
                            if (!skipSecondVertexNormal) ReadData(m, PrimitiveDataType.VERTEX0);
                            ReadData(m, PrimitiveDataType.VERTEX1);
                            ReadData(m, PrimitiveDataType.VERTEX2);
                            break;
                        case 3:
                            if (!skipFirstVertexNormal)  ReadData(m, PrimitiveDataType.NORMAL0);
                            if (!skipSecondVertexNormal) ReadData(m, PrimitiveDataType.VERTEX0);
                            ReadData(m, PrimitiveDataType.NORMAL1);
                            ReadData(m, PrimitiveDataType.VERTEX1);
                            ReadData(m, PrimitiveDataType.NORMAL2);
                            ReadData(m, PrimitiveDataType.VERTEX2);
                            break;
                        case 4 when !gouraud:
                            if (!skipFirstVertexNormal)  ReadData(m, PrimitiveDataType.NORMAL0);
                            if (!skipSecondVertexNormal) ReadData(m, PrimitiveDataType.VERTEX0);
                            ReadData(m, PrimitiveDataType.VERTEX1);
                            ReadData(m, PrimitiveDataType.VERTEX2);
                            ReadData(m, PrimitiveDataType.VERTEX3);
                            break;
                        case 4:
                            if (!skipFirstVertexNormal)  ReadData(m, PrimitiveDataType.NORMAL0);
                            if (!skipSecondVertexNormal) ReadData(m, PrimitiveDataType.VERTEX0);
                            ReadData(m, PrimitiveDataType.NORMAL1);
                            ReadData(m, PrimitiveDataType.VERTEX1);
                            ReadData(m, PrimitiveDataType.NORMAL2);
                            ReadData(m, PrimitiveDataType.VERTEX2);
                            ReadData(m, PrimitiveDataType.NORMAL3);
                            ReadData(m, PrimitiveDataType.VERTEX3);
                            break;
                    }
                }
                else
                {
                    switch (numVerts)
                    {
                        case 3 when m > 0: // HMD: Strip mesh repeat
                            if (!skipFirstVertexNormal)  ReadData(m, PrimitiveData.MESHVERTEX);
                            break;
                        case 3:
                            if (!skipFirstVertexNormal)  ReadData(m, PrimitiveDataType.VERTEX0);
                            if (!skipSecondVertexNormal) ReadData(m, PrimitiveDataType.VERTEX1);
                            ReadData(m, PrimitiveDataType.VERTEX2);
                            break;
                        case 4:
                            if (!skipFirstVertexNormal)  ReadData(m, PrimitiveDataType.VERTEX0);
                            if (!skipSecondVertexNormal) ReadData(m, PrimitiveDataType.VERTEX1);
                            ReadData(m, PrimitiveDataType.VERTEX2);
                            ReadData(m, PrimitiveDataType.VERTEX3);
                            break;
                    }
                }

                if (vertexEndPad)
                {
                    ReadData(m, PrimitiveDataType.PAD2); //pad
                }
            }

            return primitiveData;
        }

        public static void AddSpritesToGroup(Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>> groupedSprites, PrimitiveData primitiveData, VertexNormalCallback vertexCallback)
        {
            RenderInfo renderInfo;
            Vector3 center;

            void AddTriangle(Triangle triangle)
            {
                if (renderInfo.RenderFlags.HasFlag(RenderFlags.Textured))
                {
                    triangle.CorrectUVTearing();
                }
                var tuple = new Tuple<Vector3, RenderInfo>(center, renderInfo);
                if (!groupedSprites.TryGetValue(tuple, out var triangles))
                {
                    triangles = new List<Triangle>();
                    groupedSprites.Add(tuple, triangles);
                }
                triangles.Add(triangle);
            }

            primitiveData.TryGetValue(0, PrimitiveDataType.TSB, out var tsbValue);
            ParseTSB(tsbValue, out var tPage, out var pmode, out var mixtureRate);
            if (!primitiveData.RenderFlags.HasFlag(RenderFlags.SemiTransparent))
            {
                mixtureRate = MixtureRate.None; // No semi-transparency
            }

            renderInfo = new RenderInfo(tPage, primitiveData.RenderFlags, mixtureRate);


            if (!primitiveData.TryGetValue(0, PrimitiveDataType.VERTEX0, out var vertexIndex0))
            {
                return;
            }

            if (Program.Debug)
            {
                Program.Logger.WriteLine($"Primitive data: {primitiveData.PrintPrimitiveData()}");
            }
            
            center = vertexCallback(vertexIndex0, out var centerJoint);
            primitiveData.TryGetValue(0, PrimitiveDataType.W, out var width);
            primitiveData.TryGetValue(0, PrimitiveDataType.H, out var height);

            // Remember that Y-up is negative, so height values are negated compared to what we set for UVs.
            // Note that these vertex coordinates also assume the default orientation of the view is (0, 0, -1).
            var vertex0 = center + new Vector3(-width / 2f,  height / 2f, 0f);
            var vertex1 = center + new Vector3( width / 2f,  height / 2f, 0f);
            var vertex2 = center + new Vector3(-width / 2f, -height / 2f, 0f);
            var vertex3 = center + new Vector3( width / 2f, -height / 2f, 0f);

            var uMin = primitiveData.TryGetValue(0, PrimitiveDataType.U0, out var uValue) ? GeomMath.ConvertUV(uValue) : 0f;
            var vMin = primitiveData.TryGetValue(0, PrimitiveDataType.V0, out var vValue) ? GeomMath.ConvertUV(vValue) : 0f;
            var uMax = Math.Min(1f, GeomMath.ConvertUV(uValue + width));
            var vMax = Math.Min(1f, GeomMath.ConvertUV(vValue + height));

            var uv0 = new Vector2(uMin, vMin);
            var uv1 = new Vector2(uMax, vMin);
            var uv2 = new Vector2(uMin, vMax);
            var uv3 = new Vector2(uMax, vMax);

            var originalVertexIndices = new[] { vertexIndex0, vertexIndex0, vertexIndex0 };
            AddTriangle(new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                OriginalVertexIndices = originalVertexIndices,
                //VertexJoints = Triangle.CreateJoints(centerJoint, centerJoint, centerJoint, Triangle.NoJoint),
                Normals = Triangle.EmptyNormals,
                Colors = Triangle.EmptyColors,
                Uv = new[] { uv0, uv1, uv2 },
            });
            AddTriangle(new Triangle
            {
                Vertices = new[] { vertex1, vertex3, vertex2 },
                OriginalVertexIndices = originalVertexIndices,
                //VertexJoints = Triangle.CreateJoints(centerJoint, centerJoint, centerJoint, Triangle.NoJoint),
                Normals = Triangle.EmptyNormals,
                Colors = Triangle.EmptyColors,
                Uv = new[] { uv1, uv3, uv2 },
            });
        }

        public static void AddTrianglesToGroup(Dictionary<RenderInfo, List<Triangle>> groupedTriangles, PrimitiveData primitiveData,
                                               bool attached, uint modelJointID, VertexNormalCallback vertexCallback, VertexNormalCallback normalCallback)
        {
            RenderInfo renderInfo;

            void AddTriangle(Triangle triangle)
            {
                if (renderInfo.RenderFlags.HasFlag(RenderFlags.Textured))
                {
                    triangle.CorrectUVTearing();
                }
                if (!groupedTriangles.TryGetValue(renderInfo, out var triangles))
                {
                    triangles = new List<Triangle>();
                    groupedTriangles.Add(renderInfo, triangles);
                }
                triangles.Add(triangle);
            }

            // todo: We should cache vertex (and normal maybe?) lookups when MeshLength > 1.
            for (var m = 0; m < primitiveData.MeshLength; m++)
            {
                primitiveData.TryGetValue(m, PrimitiveDataType.TSB, out var tsbValue);
                ParseTSB(tsbValue, out var tPage, out var pmode, out var mixtureRate);
                if (!primitiveData.RenderFlags.HasFlag(RenderFlags.SemiTransparent))
                {
                    mixtureRate = MixtureRate.None; // No semi-transparency
                }

                renderInfo = new RenderInfo(tPage, primitiveData.RenderFlags, mixtureRate);


                var even = m % 2 == 0;
                var vertexOrder0 = (even ? 0 : 0);
                var vertexOrder1 = (even ? 1 : 2);
                var vertexOrder2 = (even ? 2 : 1);
                //var vertexOrder3 = (even ? 3 : 3);

                if (!primitiveData.TryGetVertex(m, vertexOrder0, out var vertexIndex0))
                {
                    return;
                }

                if (Program.Debug)
                {
                    Program.Logger.WriteLine($"Primitive data: {primitiveData.PrintPrimitiveData()}");
                }

                uint vertexJoint1 = Triangle.NoJoint, vertexJoint2 = Triangle.NoJoint;
                var vertex0 = vertexCallback(vertexIndex0, out var vertexJoint0);
                var vertex1 = primitiveData.TryGetVertex(m, vertexOrder1, out var vertexIndex1) ? vertexCallback(vertexIndex1, out vertexJoint1) : Vector3.Zero;
                var vertex2 = primitiveData.TryGetVertex(m, vertexOrder2, out var vertexIndex2) ? vertexCallback(vertexIndex2, out vertexJoint2) : Vector3.Zero;
                Vector3 normal0, normal1, normal2;
                uint normalJoint0, normalJoint1, normalJoint2;
                uint normalIndex1, normalIndex2;
                bool hasNormals;
                if (primitiveData.TryGetNormal(m, vertexOrder0, out var normalIndex0))
                {
                    hasNormals = true;
                    normal0 = normalCallback(normalIndex0, out normalJoint0);
                    normalJoint1 = normalJoint2 = normalJoint0; // Default joints to first normal unless other normals are defined
                    normal1 = primitiveData.TryGetNormal(m, vertexOrder1, out normalIndex1) ? normalCallback(normalIndex1, out normalJoint1) : normal0;
                    normal2 = primitiveData.TryGetNormal(m, vertexOrder2, out normalIndex2) ? normalCallback(normalIndex2, out normalJoint2) : normal0;
                }
                else
                {
                    hasNormals = false;
                    if (primitiveData.PrimitiveType != PrimitiveType.StraightLine)
                    {
                        normal0 = normal1 = normal2 = GeomMath.CalculateNormal(vertex0, vertex1, vertex2);
                    }
                    else
                    {
                        normal0 = normal1 = normal2 = Vector3.Zero;
                    }
                    normalIndex1 = normalIndex2 = normalIndex0;
                    normalJoint0 = normalJoint1 = normalJoint2 = Triangle.NoJoint;
                }

                // Note: It seems that dividing UVs by 256f instead of 255f fixes some texture misalignment.
                // However, this can't be used because it causes texture glitching at pixel boundaries.
                // There are a ton of other issues to consider when only changing this for TMDHelper.
                // More research is needed...
                // see: GeomMath.UVScalar

                // HMD: Tiled information for textures.
                // Default values when there's no tiled information.
                uint tumValue = 0;
                uint tvmValue = 0;
                uint tuaValue = 0;
                uint tvaValue = 0;
                var tuvm = Vector2.Zero;
                var tuva = Vector2.Zero;
                var isTiled = false;
                if (primitiveData.TryGetValue(m, PrimitiveDataType.TILE, out var tile))
                {
                    tumValue = (tile >>  0) & 0x1f;
                    tvmValue = (tile >>  5) & 0x1f;
                    tuaValue = (tile >> 10) & 0x1f;
                    tvaValue = (tile >> 15) & 0x1f;
                    // tum, tvm, tua, and tva are the upper 5 bits of a byte.
                    // tum and tvm are created by: (~(wh - 1) & 0xff) >> 3;
                    // tua and tva are created by: (xy & 0xff) >> 3;

                    // Note how width/height have 1 subtracted from them before being negated then shifted.
                    // This is because all four values are required to be multiples of 8.
                    // So to recover tum and tvm, we need to unshift, negate, and then (realistically) add 1 to get the original value.
                    // However, adding 1 right now produces gaps in the textures. We can only add 1 when FixUVAlignment == true.

                    // Confirm that tiled information is non-zero, otherwise we can just ignore it.
                    if (tumValue != 0 || tvmValue != 0) // We're not tiled if there's no wrap size.
                    {
                        isTiled = true;
                        tuvm = GeomMath.ConvertUV(((((tumValue << 3) ^ 0xff) + 1) & 0xff),
                                                  ((((tvmValue << 3) ^ 0xff) + 1) & 0xff));
                        tuva = GeomMath.ConvertUV((tuaValue << 3),
                                                  (tvaValue << 3));
                    }
                }

                // Converts UV base coordinates to tiled UV coordinates (for use with display/exporter).
                // Same as TiledUV.Convert but without float math.
                Vector2 TileUV(uint uValue, uint vValue)
                {
                    uValue = (~(tumValue << 3) & uValue) | ((tumValue << 3) & (tuaValue << 3));
                    vValue = (~(tvmValue << 3) & vValue) | ((tvmValue << 3) & (tvaValue << 3));
                    return GeomMath.ConvertUV(uValue, vValue);
                }

                var r0 = primitiveData.TryGetValue(m, PrimitiveDataType.R0, out var r0Value) ? r0Value / 255f : Color3.DefaultColorTone;
                var r1 = primitiveData.TryGetValue(m, PrimitiveDataType.R1, out var r1Value) ? r1Value / 255f : r0;
                var r2 = primitiveData.TryGetValue(m, PrimitiveDataType.R2, out var r2Value) ? r2Value / 255f : r0;
                var g0 = primitiveData.TryGetValue(m, PrimitiveDataType.G0, out var g0Value) ? g0Value / 255f : Color3.DefaultColorTone;
                var g1 = primitiveData.TryGetValue(m, PrimitiveDataType.G1, out var g1Value) ? g1Value / 255f : g0;
                var g2 = primitiveData.TryGetValue(m, PrimitiveDataType.G2, out var g2Value) ? g2Value / 255f : g0;
                var b0 = primitiveData.TryGetValue(m, PrimitiveDataType.B0, out var b0Value) ? b0Value / 255f : Color3.DefaultColorTone;
                var b1 = primitiveData.TryGetValue(m, PrimitiveDataType.B1, out var b1Value) ? b1Value / 255f : b0;
                var b2 = primitiveData.TryGetValue(m, PrimitiveDataType.B2, out var b2Value) ? b2Value / 255f : b0;
                var u0 = primitiveData.TryGetValue(m, PrimitiveDataType.U0, out var u0Value) ? GeomMath.ConvertUV(u0Value) : 0f;
                var u1 = primitiveData.TryGetValue(m, PrimitiveDataType.U1, out var u1Value) ? GeomMath.ConvertUV(u1Value) : 0f;
                var u2 = primitiveData.TryGetValue(m, PrimitiveDataType.U2, out var u2Value) ? GeomMath.ConvertUV(u2Value) : 0f;
                var v0 = primitiveData.TryGetValue(m, PrimitiveDataType.V0, out var v0Value) ? GeomMath.ConvertUV(v0Value) : 0f;
                var v1 = primitiveData.TryGetValue(m, PrimitiveDataType.V1, out var v1Value) ? GeomMath.ConvertUV(v1Value) : 0f;
                var v2 = primitiveData.TryGetValue(m, PrimitiveDataType.V2, out var v2Value) ? GeomMath.ConvertUV(v2Value) : 0f;
                var triangle1 = new Triangle
                {
                    Vertices = new[] { vertex0, vertex1, vertex2 },
                    OriginalVertexIndices = new[] { vertexIndex0, vertexIndex1, vertexIndex2 },
                    Normals = new[] { normal0, normal1, normal2 },
                    OriginalNormalIndices = new[] { normalIndex0, normalIndex1, normalIndex2 },
                    VertexJoints = Triangle.CreateJoints(vertexJoint0, vertexJoint1, vertexJoint2, modelJointID),
                    NormalJoints = Triangle.CreateJoints(normalJoint0, normalJoint1, normalJoint2, modelJointID),
                    Colors = new[] { new Color3(r0, g0, b0), new Color3(r1, g1, b1), new Color3(r2, g2, b2) },
                    Uv = new[] { new Vector2(u0, v0), new Vector2(u1, v1), new Vector2(u2, v2) },
                };
                if (isTiled)
                {
                    // Use triangle's UV as baseUv, and give the triangle a new converted UV array (for display/exporter purposes).
                    triangle1.TiledUv = new TiledUV(triangle1.Uv, tuva, tuvm);
                    triangle1.Uv = new[] { TileUV(u0Value, v0Value), TileUV(u1Value, v1Value), TileUV(u2Value, v2Value) };
                    //triangle1.Uv = triangle1.TiledUv.ConvertBaseUv();
                }
                AddTriangle(triangle1);

                // If this is a quad, then we need to handle the second triangle on the first loop.
                // Don't use TryGetVertex, since that may return the next vertex for strip meshes.
                var quad = m == 0 && primitiveData.PrimitiveType == PrimitiveType.Quad;
                //if (quad && primitiveData.TryGetVertex(m, vertexOrder3, out var vertexIndex3))
                if (quad && primitiveData.TryGetValue(m, PrimitiveDataType.VERTEX3, out var vertexIndex3))
                {
                    var vertex3 = vertexCallback(vertexIndex3, out var vertexJoint3);
                    //var normal3 = primitiveData.TryGetNormal(m, vertexOrder3, out var normalIndex3) ? normalCallback(normalIndex3) : normal0;
                    Vector3 normal3;
                    var normalJoint3 = normalJoint0; // Default joints to first normal unless other normals are defined
                    if (primitiveData.TryGetValue(m, PrimitiveDataType.NORMAL3, out var normalIndex3))
                    {
                        normal3 = normalCallback(normalIndex3, out normalJoint3);
                    }
                    else if (hasNormals)
                    {
                        normal3 = normal0;
                        normalIndex3 = normalIndex0;
                    }
                    else
                    {
                        if (primitiveData.PrimitiveType != PrimitiveType.StraightLine)
                        {
                            normal1 = normal3 = normal2 = GeomMath.CalculateNormal(vertex1, vertex3, vertex2);
                        }
                        else
                        {
                            normal3 = Vector3.Zero;
                        }
                        normalIndex3 = normalIndex0;
                    }
                    // todo: Do we need normal calculation for this triangle if !hasNormals?
                    var r3 = primitiveData.TryGetValue(m, PrimitiveDataType.R3, out var r3Value) ? r3Value / 255f : r0;
                    var g3 = primitiveData.TryGetValue(m, PrimitiveDataType.G3, out var g3Value) ? g3Value / 255f : g0;
                    var b3 = primitiveData.TryGetValue(m, PrimitiveDataType.B3, out var b3Value) ? b3Value / 255f : b0;
                    var u3 = primitiveData.TryGetValue(m, PrimitiveDataType.U3, out var u3Value) ? GeomMath.ConvertUV(u3Value) : 0f;
                    var v3 = primitiveData.TryGetValue(m, PrimitiveDataType.V3, out var v3Value) ? GeomMath.ConvertUV(v3Value) : 0f;
                    var triangle2 = new Triangle
                    {
                        Vertices = new[] { vertex1, vertex3, vertex2 },
                        OriginalVertexIndices = new[] { vertexIndex1, vertexIndex3, vertexIndex2 },
                        Normals = new[] { normal1, normal3, normal2 },
                        OriginalNormalIndices = new[] { normalIndex1, normalIndex3, normalIndex2 },
                        VertexJoints = Triangle.CreateJoints(vertexJoint1, vertexJoint3, vertexJoint2, modelJointID),
                        NormalJoints = Triangle.CreateJoints(normalJoint1, normalJoint3, normalJoint2, modelJointID),
                        Colors = new[] { new Color3(r1, g1, b1), new Color3(r3, g3, b3), new Color3(r2, g2, b2) },
                        Uv = new[] { new Vector2(u1, v1), new Vector2(u3, v3), new Vector2(u2, v2) },
                    };
                    if (isTiled)
                    {
                        // Use triangle's UV as baseUv, and give the triangle a new converted UV array (for display/exporter purposes).
                        triangle2.TiledUv = new TiledUV(triangle2.Uv, tuva, tuvm);
                        triangle2.Uv = new[] { TileUV(u1Value, v1Value), TileUV(u3Value, v3Value), TileUV(u2Value, v2Value) };
                        //triangle2.Uv = triangle2.TiledUv.ConvertBaseUv();
                    }
                    AddTriangle(triangle2);
                }
            }
        }

        public static void ParseCBA(uint cbaValue, out uint clutX, out uint clutY)
        {
            clutX = ((cbaValue >> 0) & 0x03f) << 4; // 6(10) bits
            clutY = ((cbaValue >> 6) & 0x1ff) << 0; // 9( 9) bits (not shifted to 10 bits)
            // Bit 15 is unused.
        }

        public static void ParseTSB(uint tsbValue, out uint texturePage, out uint pmode, out MixtureRate mixtureRate)
        {
            texturePage = (tsbValue >> 0) & 0x1f;
            var abr     = (tsbValue >> 5) & 0x03; // Mixture rate: 0- 50%back+50%poly, 1- 100%back+100%poly, 2- 100%back-100%poly, 3- 100%back+25%poly
            var tpf     = (tsbValue >> 7) & 0x03; // Texture pixel format: 0-4bit, 1-8bit, 2-15bit

            // Switch statement for if pmode is ever changed to an enum.
            pmode = tpf;
            /*switch (tpf)
            {
                default:
                case 0:
                    pmode = 0;// PixelFormat.Format4bppIndexed;
                    break;
                case 1:
                    pmode = 1;// PixelFormat.Format8bppIndexed;
                    break;
                case 2:
                    pmode = 2;// PixelFormat.Format16bppRgb555;
                    break;
                case 3:
                    pmode = 3;// PixelFormat.Format24bppRgb;
                    break;
            }*/

            switch (abr)
            {
                default:
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

        public static float ConvertNormal(int value)
        {
            return value == 0 ? value : value / 4096f;
        }
    }
}
