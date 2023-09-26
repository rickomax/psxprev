using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Parsers
{
    public class PMDParser : FileOffsetScanner
    {
        private readonly Dictionary<RenderInfo, List<Triangle>> _groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();
        private readonly List<ModelEntity> _models = new List<ModelEntity>();

        public PMDParser(EntityAddedAction entityAdded)
            : base(entityAdded: entityAdded)
        {
        }

        public override string FormatName => "PMD";

        protected override void Parse(BinaryReader reader)
        {
            var version = reader.ReadUInt32();
            if (version == 0x00000042)
            {
                ParsePMD(reader);
            }
        }

        private bool ParsePMD(BinaryReader reader)
        {
            // Reset state
            _groupedTriangles.Clear();
            _models.Clear();


            var primitiveTop = reader.ReadUInt32();
            var vertTop = reader.ReadUInt32();
            var objectCount = reader.ReadUInt32();
            if (objectCount == 0 || objectCount > Limits.MaxPMDObjects)
            {
                return false;
            }

            for (uint o = 0; o < objectCount; o++)
            {
                // Number of different primitive types used in the object
                var primitiveGroupCount = reader.ReadUInt32();
                // todo: Is it possible that an object may be empty, and define zero for its primitiveGroupCount?
                if (primitiveGroupCount == 0 || primitiveGroupCount > Limits.MaxPMDPointers)
                {
                    return false;
                }
                for (uint pg = 0; pg < primitiveGroupCount; pg++)
                {
                    // Pointer to primitives of the same type
                    var primitiveGroupTop = reader.ReadUInt32();
                    var primitiveGroupPosition = reader.BaseStream.Position;
                    // Strange that primitiveTop isn't used at all
                    reader.BaseStream.Seek(_offset + primitiveGroupTop, SeekOrigin.Begin);

                    var primitiveCount = reader.ReadUInt16();
                    // todo: Is a primitiveCount of zero valid?
                    if (primitiveCount > Limits.MaxPMDPackets)
                    {
                        return false;
                    }
                    var primType = reader.ReadUInt16();

                    var quad      = ((primType >> 0) & 0x1) != 0; // Polygon: 0-Triangle, 1-Quad
                    var gouraud   = ((primType >> 1) & 0x1) != 0; // Shading: 0-Flat, 1-Gouraud
                    var textured  = ((primType >> 2) & 0x1) == 0; // Texture: 0-On, 1-Off
                    var shared    = ((primType >> 3) & 0x1) != 0; // Vertex: 0-Independent, 1-Shared
                    var light     = ((primType >> 4) & 0x1) != 0; // Light: 0-Unlit, 1-Lit
                    var bothSides = ((primType >> 5) & 0x1) != 0; // Both sides: 0-Single sided, 1-Double sided

                    var renderFlags = RenderFlags.None;
                    if (textured)  renderFlags |= RenderFlags.Textured;
                    if (!light)    renderFlags |= RenderFlags.Unlit;
                    if (bothSides) renderFlags |= RenderFlags.DoubleSided;

                    var primTypeSwitch = primType & ~0x30; // These two bits don't effect packet structure
                    if (primTypeSwitch > 0xf)
                    {
                        return false;
                        // Alt:
                        //if (Program.Debug)
                        //{
                        //    Program.Logger.WriteErrorLine($"Unknown primitive:0x{primTypeSwitch:x}");
                        //}
                        //break;
                    }

                    for (uint pk = 0; pk < primitiveCount; pk++)
                    {
                        // Independent,   Shared
                        // 0x0: POLY_FT3, 0x8: POLY_FT3
                        // 0x1: POLY_FT4, 0x9: POLY_FT4
                        // 0x2: POLY_GT3, 0xa: POLY_GT3
                        // 0x3: POLY_GT4, 0xb: POLY_GT4
                        // 0x4: POLY_F3,  0xc: POLY_F3
                        // 0x5: POLY_F4,  0xd: POLY_F4
                        // 0x6: POLY_G3,  0xe: POLY_G3
                        // 0x7: POLY_G4,  0xf: POLY_G4
                        ReadPrimitive(reader, renderFlags, quad, gouraud, textured, light, shared, vertTop);
                    }

                    reader.BaseStream.Seek(primitiveGroupPosition, SeekOrigin.Begin);
                }

                FlushModels(o);
            }

            if (_models.Count > 0)
            {
                var rootEntity = new RootEntity();
                rootEntity.ChildEntities = _models.ToArray();
                rootEntity.ComputeBounds();
                EntityResults.Add(rootEntity);
                return true;
            }
            return false;
        }

        private void ReadPrimitive(BinaryReader reader, RenderFlags renderFlags, bool quad, bool gouraud, bool textured, bool light, bool shared, uint vertTop)
        {
            var packetPosition = reader.BaseStream.Position;

            // PMD packets contain two sets of the same POLY_* struct, so that one can be used as working data at runtime.
            // Skip the first POLY_* struct, and only use the second one's values. Both structs should have identical values.
            // tag + rgb0 + xy0-2 + (xy3) + (rgb1-2 + (rgb3)) + (uv0-2 + (uv3))
            var polyLength = (quad ? 24 : 20) + (gouraud ? (quad ? 12 : 8) : 0) + (textured ? (quad ? 16 : 12) : 0);
            reader.BaseStream.Seek(polyLength, SeekOrigin.Current);

            byte r1 = 0, g1 = 0, b1 = 0, r2 = 0, g2 = 0, b2 = 0, r3 = 0, g3 = 0, b3 = 0;
            byte u0 = 0, v0 = 0, u1 = 0, v1 = 0, u2 = 0, v2 = 0, u3 = 0, v3 = 0;
            ushort cba = 0, tsb = 0;

            var tag = reader.ReadUInt32();

            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var code = reader.ReadByte();
            var x0 = reader.ReadInt16();
            var y0 = reader.ReadInt16();
            if (textured)
            {
                u0 = reader.ReadByte();
                v0 = reader.ReadByte();
                cba = reader.ReadUInt16();
            }

            if (gouraud)
            {
                r1 = reader.ReadByte();
                g1 = reader.ReadByte();
                b1 = reader.ReadByte();
                reader.ReadByte(); //pad
            }
            var x1 = reader.ReadInt16();
            var y1 = reader.ReadInt16();
            if (textured)
            {
                u1 = reader.ReadByte();
                v1 = reader.ReadByte();
                tsb = reader.ReadUInt16();
            }

            if (gouraud)
            {
                r2 = reader.ReadByte();
                g2 = reader.ReadByte();
                b2 = reader.ReadByte();
                reader.ReadByte(); //pad
            }
            var x2 = reader.ReadInt16();
            var y2 = reader.ReadInt16();
            if (textured)
            {
                u2 = reader.ReadByte();
                v2 = reader.ReadByte();
                reader.ReadUInt16(); //pad
            }

            if (quad)
            {
                if (gouraud)
                {
                    r3 = reader.ReadByte();
                    g3 = reader.ReadByte();
                    b3 = reader.ReadByte();
                    reader.ReadByte(); //pad
                }
                var x3 = reader.ReadInt16();
                var y3 = reader.ReadInt16();
                if (textured)
                {
                    u3 = reader.ReadByte();
                    v3 = reader.ReadByte();
                    reader.ReadUInt16(); //pad
                }
            }


            Vector3 vertex0, vertex1, vertex2, vertex3;
            uint vertexIndex0, vertexIndex1, vertexIndex2, vertexIndex3;
            if (!shared)
            {
                vertexIndex0 = vertexIndex1 = vertexIndex2 = vertexIndex3 = 0;
                vertex0 = ReadVertex(reader);
                vertex1 = ReadVertex(reader);
                vertex2 = ReadVertex(reader);
                vertex3 = quad ? ReadVertex(reader) : Vector3.Zero;
            }
            else
            {
                vertexIndex0 = reader.ReadUInt32();
                vertexIndex1 = reader.ReadUInt32();
                vertexIndex2 = reader.ReadUInt32();
                vertexIndex3 = quad ? reader.ReadUInt32() : 0;
                var position = reader.BaseStream.Position;
                vertex0 = ReadSharedVertex(reader, vertTop + vertexIndex0);
                vertex1 = ReadSharedVertex(reader, vertTop + vertexIndex1);
                vertex2 = ReadSharedVertex(reader, vertTop + vertexIndex2);
                vertex3 = quad ? ReadSharedVertex(reader, vertTop + vertexIndex3) : Vector3.Zero;
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }


            Color color1, color2, color3;
            var color0 = new Color(r0 / 255f, g0 / 255f, b0 / 255f);
            if (gouraud)
            {
                color1 = new Color(r1 / 255f, g1 / 255f, b1 / 255f);
                color2 = new Color(r2 / 255f, g2 / 255f, b2 / 255f);
                color3 = quad ? new Color(r3 / 255f, g3 / 255f, b3 / 255f) : null;
            }
            else
            {
                color1 = color2 = color3 = color0;
            }

            Vector2 uv0, uv1, uv2, uv3;
            if (textured)
            {
                uv0 = GeomMath.ConvertUV(u0, v0);
                uv1 = GeomMath.ConvertUV(u1, v1);
                uv2 = GeomMath.ConvertUV(u2, v2);
                uv3 = quad ? GeomMath.ConvertUV(u3, v3) : Vector2.Zero;
            }
            else
            {
                uv0 = uv1 = uv2 = uv3 = Vector2.Zero;
            }

            var semiTrans = ((code >> 1) & 0x1) != 0;
            if (semiTrans)
            {
                renderFlags |= RenderFlags.SemiTransparent;
            }

            TMDHelper.ParseTSB(tsb, out var tPage, out _, out var mixtureRate);
            if (!semiTrans)
            {
                mixtureRate = MixtureRate.None;
            }


            var normal1 = light ? GeomMath.CalculateNormal(vertex0, vertex1, vertex2) : Vector3.Zero;
            var normal2 = light && quad ? GeomMath.CalculateNormal(vertex1, vertex3, vertex2) : Vector3.Zero;

            var triangleDebugData = new[] { $"packetPosition: 0x{packetPosition:X}" };
            var triangle1 = new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Normals = light ? new[] { normal1, normal1, normal1 } : Triangle.EmptyNormals,
                OriginalVertexIndices = shared ? new[] { vertexIndex0 / 8, vertexIndex1 / 8, vertexIndex2 / 8 } : null,
                Colors = new[] { color0, color1, color2 },
                Uv = textured ? new[] { uv0, uv1, uv2 } : Triangle.EmptyUv,
                DebugData = triangleDebugData,
            };
            AddTriangle(triangle1, tPage, renderFlags, mixtureRate);

            if (quad)
            {
                var triangle2 = new Triangle
                {
                    Vertices = new[] { vertex1, vertex3, vertex2 },
                    Normals = light ? new[] { normal2, normal2, normal2 } : Triangle.EmptyNormals,
                    OriginalVertexIndices = shared ? new[] { vertexIndex1 / 8, vertexIndex3 / 8, vertexIndex2 / 8 } : null,
                    Colors = new[] { color1, color3, color2 },
                    Uv = textured ? new[] { uv1, uv3, uv2 } : Triangle.EmptyUv,
                    DebugData = triangleDebugData,
                };
                AddTriangle(triangle2, tPage, renderFlags, mixtureRate);
            }
        }

        private static Vector3 ReadVertex(BinaryReader reader)
        {
            var x = reader.ReadInt16();
            var y = reader.ReadInt16();
            var z = reader.ReadInt16();
            reader.ReadUInt16(); //pad
            return new Vector3(x, y, z);
        }

        private Vector3 ReadSharedVertex(BinaryReader reader, uint vertOffset)
        {
            reader.BaseStream.Seek(_offset + vertOffset, SeekOrigin.Begin);
            return ReadVertex(reader);
        }

        private void FlushModels(uint objectIndex)
        {
            foreach (var kvp in _groupedTriangles)
            {
                var renderInfo = kvp.Key;
                var triangles = kvp.Value;
                var model = new ModelEntity
                {
                    Triangles = triangles.ToArray(),
                    TexturePage = renderInfo.TexturePage,
                    RenderFlags = renderInfo.RenderFlags,
                    MixtureRate = renderInfo.MixtureRate,
                    TMDID = objectIndex + 1u,
                };
                _models.Add(model);
            }
            _groupedTriangles.Clear();
        }

        private void AddTriangle(Triangle triangle, uint tPage, RenderFlags renderFlags, MixtureRate mixtureRate)
        {
            if (renderFlags.HasFlag(RenderFlags.Textured))
            {
                triangle.CorrectUVTearing();
            }
            var renderInfo = new RenderInfo(tPage, renderFlags, mixtureRate);
            if (!_groupedTriangles.TryGetValue(renderInfo, out var triangles))
            {
                triangles = new List<Triangle>();
                _groupedTriangles.Add(renderInfo, triangles);
            }
            triangles.Add(triangle);
        }
    }
}
