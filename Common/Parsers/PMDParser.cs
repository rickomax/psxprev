using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public class PMDParser : FileOffsetScanner
    {
        public PMDParser(EntityAddedAction entityAdded)
            : base(entityAdded: entityAdded)
        {
        }

        public override string FormatName => "PMD";

        protected override void Parse(BinaryReader reader, string fileTitle, out List<RootEntity> entities, out List<Animation> animations, out List<Texture> textures)
        {
            entities = null;
            animations = null;
            textures = null;

            var version = reader.ReadUInt32();
            if (version == 0x00000042)
            {
                var entity = ParsePMD(reader);
                if (entity != null)
                {
                    entities = new List<RootEntity> { entity };
                }
            }
        }

        private RootEntity ParsePMD(BinaryReader reader)
        {
            var groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();

            void AddTriangle(Triangle triangle, uint tPage, RenderFlags renderFlags)
            {
                if (renderFlags.HasFlag(RenderFlags.Textured))
                {
                    triangle.CorrectUVTearing();
                }
                var renderInfo = new RenderInfo(tPage, renderFlags);
                if (!groupedTriangles.TryGetValue(renderInfo, out var triangles))
                {
                    triangles = new List<Triangle>();
                    groupedTriangles.Add(renderInfo, triangles);
                }
                triangles.Add(triangle);
            }

            void AddTriangles(Triangle[] triangles, uint tPage, RenderFlags renderFlags)
            {
                foreach (var triangle in triangles)
                {
                    AddTriangle(triangle, tPage, renderFlags);
                }
            }

            var primPoint = reader.ReadUInt32();
            var vertPoint = reader.ReadUInt32();
            var nObj = reader.ReadUInt32();
            if (nObj < 1 || nObj > 4000)
            {
                return null;
            }
            var models = new List<ModelEntity>();
            for (uint o = 0; o < nObj; o++)
            {
                var nPointers = reader.ReadUInt32();
                if (nPointers < 1 || nPointers > 4000)
                {
                    return null;
                }
                for (var p = 0; p < nPointers; p++)
                {
                    var position = reader.BaseStream.Position;
                    var pointer = reader.ReadUInt32();
                    reader.BaseStream.Seek(_offset + pointer, SeekOrigin.Begin);
                    var nPacket = reader.ReadUInt16();
                    if (nPacket > 4000)
                    {
                        return null;
                    }
                    var primType = reader.ReadUInt16();

                    var quad   = ((primType >> 0) & 0x1) == 1; // Polygon: 0-Triangle, 1-Quad
                    var iipBit = ((primType >> 1) & 0x1) == 1; // Shading: 0-Flat, 1-Gouraud (separate colors when !lgtBit)
                    var tmeBit = ((primType >> 2) & 0x1) == 0; // Texture: 1-On, 0-Off
                    var shared = ((primType >> 3) & 0x1) == 1; // Vertex: 0-Independent, 1-Shared
                    var lgtBit = ((primType >> 4) & 0x1) == 1; // Light: 0-Unlit, 1-Lit
                    var botBit = ((primType >> 5) & 0x1) == 1; // Both sides: 0-Single sided, 1-Double sided

                    var renderFlags = RenderFlags.None;
                    if (tmeBit) renderFlags |= RenderFlags.Textured;
                    if (!lgtBit) renderFlags |= RenderFlags.Unlit;
                    if (botBit) renderFlags |= RenderFlags.DoubleSided;

                    var primTypeSwitch = primType & ~0x30; // These two bits don't effect packet structure
                    if (primTypeSwitch > 15)
                    {
                        return null;
                    }

                    for (var pk = 0; pk < nPacket; pk++)
                    {
                        uint tPage;
                        switch (primTypeSwitch)
                        {
                            case 0x00:
                                AddTriangle(ReadPolyFT3(reader, out tPage), tPage, renderFlags);
                                break;
                            case 0x01:
                                AddTriangles(ReadPolyFT4(reader, out tPage), tPage, renderFlags);
                                break;
                            case 0x02:
                                AddTriangle(ReadPolyGT3(reader, out tPage), tPage, renderFlags);
                                break;
                            case 0x03:
                                AddTriangles(ReadPolyGT4(reader, out tPage), tPage, renderFlags);
                                break;
                            case 0x04:
                                AddTriangle(ReadPolyF3(reader), 0, renderFlags);
                                break;
                            case 0x05:
                                AddTriangles(ReadPolyF4(reader), 0, renderFlags);
                                break;
                            case 0x06:
                                AddTriangle(ReadPolyG3(reader), 0, renderFlags);
                                break;
                            case 0x07:
                                AddTriangles(ReadPolyG4(reader), 0, renderFlags);
                                break;
                            case 0x08:
                                AddTriangle(ReadPolyFT3(reader, out tPage, true, vertPoint), tPage, renderFlags);
                                break;
                            case 0x09:
                                AddTriangles(ReadPolyFT4(reader, out tPage, true, vertPoint), tPage, renderFlags);
                                break;
                            case 0x0a:
                                AddTriangle(ReadPolyGT3(reader, out tPage, true, vertPoint), tPage, renderFlags);
                                break;
                            case 0x0b:
                                AddTriangles(ReadPolyGT4(reader, out tPage, true, vertPoint), tPage, renderFlags);
                                break;
                            case 0x0c:
                                AddTriangle(ReadPolyF3(reader, true, vertPoint), 0, renderFlags);
                                break;
                            case 0x0d:
                                AddTriangles(ReadPolyF4(reader, true, vertPoint), 0, renderFlags);
                                break;
                            case 0x0e:
                                AddTriangle(ReadPolyG3(reader, true, vertPoint), 0, renderFlags);
                                break;
                            case 0x0f:
                                AddTriangles(ReadPolyG4(reader, true, vertPoint), 0, renderFlags);
                                break;
                            default:
                                if (Program.Debug)
                                {
                                    Program.Logger.WriteErrorLine($"Unknown primitive:{primType}");
                                }
                                goto EndObject;
                        }
                    }
                    reader.BaseStream.Seek(position + 4, SeekOrigin.Begin);
                }
                EndObject:
                foreach (var kvp in groupedTriangles)
                {
                    var renderInfo = kvp.Key;
                    var triangles = kvp.Value;
                    var model = new ModelEntity
                    {
                        Triangles = triangles.ToArray(),
                        TexturePage = renderInfo.TexturePage,
                        RenderFlags = renderInfo.RenderFlags,
                        MixtureRate = renderInfo.MixtureRate,
                        TMDID = o + 1, //todo
                    };
                    models.Add(model);
                }
                groupedTriangles.Clear();
            }

            EndModel:
            if (models.Count > 0)
            {
                var entity = new RootEntity();
                foreach (var model in models)
                {
                    model.ParentEntity = entity;
                }
                entity.ChildEntities = models.ToArray();
                entity.ComputeBounds();
                return entity;
            }
            return null;
        }

        private Triangle[] ReadPolyGT4(BinaryReader reader, out uint tPage, bool sharedVertices = false, uint vertPoint = 0)
        {
            int tag;
            byte r0 = 0, g0 = 0, b0 = 0;
            byte r1 = 0, g1 = 0, b1 = 0;
            byte r2 = 0, g2 = 0, b2 = 0;
            byte r3 = 0, g3 = 0, b3 = 0;
            short x0, y0;
            short x1, y1;
            short x2, y2;
            short x3, y3;
            byte u0 = 0, v0 = 0;
            byte u1 = 0, v1 = 0;
            byte u2 = 0, v2 = 0;
            byte u3 = 0, v3 = 0;
            ushort clut;
            tPage = 0;
            byte code;
            for (var i = 0; i < 2; i++)
            {
                tag = reader.ReadInt32();
                r0 = reader.ReadByte();
                g0 = reader.ReadByte();
                b0 = reader.ReadByte();
                code = reader.ReadByte();
                x0 = reader.ReadInt16();
                y0 = reader.ReadInt16();
                u0 = reader.ReadByte();
                v0 = reader.ReadByte();
                clut = reader.ReadUInt16();
                r1 = reader.ReadByte();
                g1 = reader.ReadByte();
                b1 = reader.ReadByte();
                reader.ReadByte();
                x1 = reader.ReadInt16();
                y1 = reader.ReadInt16();
                u1 = reader.ReadByte();
                v1 = reader.ReadByte();
                tPage = reader.ReadUInt16();
                r2 = reader.ReadByte();
                g2 = reader.ReadByte();
                b2 = reader.ReadByte();
                reader.ReadByte();
                x2 = reader.ReadInt16();
                y2 = reader.ReadInt16();
                u2 = reader.ReadByte();
                v2 = reader.ReadByte();
                reader.ReadUInt16();
                r3 = reader.ReadByte();
                g3 = reader.ReadByte();
                b3 = reader.ReadByte();
                reader.ReadByte();
                x3 = reader.ReadInt16();
                y3 = reader.ReadInt16();
                u3 = reader.ReadByte();
                v3 = reader.ReadByte();
                reader.ReadUInt16();
            }
            short v0x, v0y, v0z;
            short v1x, v1y, v1z;
            short v2x, v2y, v2z;
            short v3x, v3y, v3z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
                ReadSVector(reader, out v3x, out v3y, out v3z);
            }
            else
            {
                var vo0 = vertPoint + reader.ReadUInt32();
                var vo1 = vertPoint + reader.ReadUInt32();
                var vo2 = vertPoint + reader.ReadUInt32();
                var vo3 = vertPoint + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                ReadSharedVertices(reader, vo3, out v3x, out v3y, out v3z);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_gt4, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r1, g1, b1, r2, g2, b2, u0, v0, u1, v1, u2, v2, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_gt4, false, false, null, null, 0, 0, 0, 0, 0,
             0, r1, g1, b1, r3, g3, b3, r2, g2, b2, u1, v1, u3, v3, u2, v2, v1x, v1y, v1z, v3x, v3y, v3z, v2x, v2y, v2z);
            return new[] { triangle1, triangle2 };
        }

        private Triangle[] ReadPolyG4(BinaryReader reader, bool sharedVertices = false, uint vertPoint = 0)
        {
            long tag;
            byte r0 = 0, g0 = 0, b0 = 0;
            byte r1 = 0, g1 = 0, b1 = 0;
            byte r2 = 0, g2 = 0, b2 = 0;
            byte r3 = 0, g3 = 0, b3 = 0;
            byte code;
            short x0, y0;
            short x1, y1;
            short x2, y2;
            short x3, y3;
            for (var i = 0; i < 2; i++)
            {
                tag = reader.ReadInt32();
                r0 = reader.ReadByte();
                g0 = reader.ReadByte();
                b0 = reader.ReadByte();
                code = reader.ReadByte();
                x0 = reader.ReadInt16();
                y0 = reader.ReadInt16();
                r1 = reader.ReadByte();
                g1 = reader.ReadByte();
                b1 = reader.ReadByte();
                reader.ReadByte();
                x1 = reader.ReadInt16();
                y1 = reader.ReadInt16();
                r2 = reader.ReadByte();
                g2 = reader.ReadByte();
                b2 = reader.ReadByte();
                reader.ReadByte();
                x2 = reader.ReadInt16();
                y2 = reader.ReadInt16();
                r3 = reader.ReadByte();
                g3 = reader.ReadByte();
                b3 = reader.ReadByte();
                reader.ReadByte();
                x3 = reader.ReadInt16();
                y3 = reader.ReadInt16();
            }
            short v0x, v0y, v0z;
            short v1x, v1y, v1z;
            short v2x, v2y, v2z;
            short v3x, v3y, v3z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
                ReadSVector(reader, out v3x, out v3y, out v3z);
            }
            else
            {
                var vo0 = vertPoint + reader.ReadUInt32();
                var vo1 = vertPoint + reader.ReadUInt32();
                var vo2 = vertPoint + reader.ReadUInt32();
                var vo3 = vertPoint + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                ReadSharedVertices(reader, vo3, out v3x, out v3y, out v3z);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_g4, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0, 0, 0, 0, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_g4, false, false, null, null, 0, 0, 0, 0, 0,
             0, r1, g1, b1, r3, g3, b3, r2, g2, b2, 0, 0, 0, 0, 0, 0, v1x, v1y, v1z, v3x, v3y, v3z, v2x, v2y, v2z);
            return new[] { triangle1, triangle2 };
        }

        private Triangle[] ReadPolyFT4(BinaryReader reader, out uint tPage, bool sharedVertices = false, uint vertPoint = 0)
        {
            long tag;
            byte r = 0, g = 0, b = 0;
            byte code;
            short x0, y0;
            short x1, y1;
            short x2, y2;
            short x3, y3;
            byte u0 = 0, v0 = 0;
            byte u1 = 0, v1 = 0;
            byte u2 = 0, v2 = 0;
            byte u3 = 0, v3 = 0;
            ushort clutId;
            tPage = 0;
            for (var i = 0; i < 2; i++)
            {
                tag = reader.ReadUInt32();
                r = reader.ReadByte();
                g = reader.ReadByte();
                b = reader.ReadByte();
                code = reader.ReadByte();
                x0 = reader.ReadInt16();
                y0 = reader.ReadInt16();
                u0 = reader.ReadByte();
                v0 = reader.ReadByte();
                clutId = reader.ReadUInt16();
                x1 = reader.ReadInt16();
                y1 = reader.ReadInt16();
                u1 = reader.ReadByte();
                v1 = reader.ReadByte();
                tPage = reader.ReadUInt16();
                x2 = reader.ReadInt16();
                y2 = reader.ReadInt16();
                u2 = reader.ReadByte();
                v2 = reader.ReadByte();
                reader.ReadUInt16();
                x3 = reader.ReadInt16();
                y3 = reader.ReadInt16();
                u3 = reader.ReadByte();
                v3 = reader.ReadByte();
                reader.ReadUInt16();
            }
            short v0x, v0y, v0z;
            short v1x, v1y, v1z;
            short v2x, v2y, v2z;
            short v3x, v3y, v3z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
                ReadSVector(reader, out v3x, out v3y, out v3z);
            }
            else
            {
                var vo0 = vertPoint + reader.ReadUInt32();
                var vo1 = vertPoint + reader.ReadUInt32();
                var vo2 = vertPoint + reader.ReadUInt32();
                var vo3 = vertPoint + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                ReadSharedVertices(reader, vo3, out v3x, out v3y, out v3z);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_ft4, false, false, null, null, 0, 0, 0, 0, 0,
                0, r, g, b, r, g, b, r, g, b, u0, v0, u1, v1, u2, v2, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_ft4, false, false, null, null, 0, 0, 0, 0, 0,
               0, r, g, b, r, g, b, r, g, b, u1, v1, u3, v3, u2, v2, v1x, v1y, v1z, v3x, v3y, v3z, v2x, v2y, v2z);
            return new[] { triangle1, triangle2 };
        }

        private Triangle[] ReadPolyF4(BinaryReader reader, bool sharedVertices = false, uint vertPoint = 0)
        {
            int tag;
            byte r0 = 0, g0 = 0, b0 = 0;
            short x0, y0;
            short x1, y1;
            short x2, y2;
            short x3, y3;
            byte code;
            for (var i = 0; i < 2; i++)
            {
                tag = reader.ReadInt32();
                r0 = reader.ReadByte();
                g0 = reader.ReadByte();
                b0 = reader.ReadByte();
                code = reader.ReadByte();
                x0 = reader.ReadInt16();
                y0 = reader.ReadInt16();
                x1 = reader.ReadInt16();
                y1 = reader.ReadInt16();
                x2 = reader.ReadInt16();
                y2 = reader.ReadInt16();
                x3 = reader.ReadInt16();
                y3 = reader.ReadInt16();
            }
            short v0x, v0y, v0z;
            short v1x, v1y, v1z;
            short v2x, v2y, v2z;
            short v3x, v3y, v3z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
                ReadSVector(reader, out v3x, out v3y, out v3z);
            }
            else
            {
                var vo0 = vertPoint + reader.ReadUInt32();
                var vo1 = vertPoint + reader.ReadUInt32();
                var vo2 = vertPoint + reader.ReadUInt32();
                var vo3 = vertPoint + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                ReadSharedVertices(reader, vo3, out v3x, out v3y, out v3z);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_f4, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r0, g0, b0, r0, g0, b0, 0, 0, 0, 0, 0, 0, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_f4, false, false, null, null, 0, 0, 0, 0, 0,
            0, r0, g0, b0, r0, g0, b0, r0, g0, b0, 0, 0, 0, 0, 0, 0, v1x, v1y, v1z, v3x, v3y, v3z, v2x, v2y, v2z);
            return new[] { triangle1, triangle2 };
        }

        private Triangle ReadPolyGT3(BinaryReader reader, out uint tPage, bool sharedVertices = false, uint vertPoint = 0)
        {
            int tag;
            byte r0 = 0, g0 = 0, b0 = 0;
            byte r1 = 0, g1 = 0, b1 = 0;
            byte r2 = 0, g2 = 0, b2 = 0;
            short x0, y0;
            short x1, y1;
            short x2, y2;
            byte u0 = 0, v0 = 0;
            byte u1 = 0, v1 = 0;
            byte u2 = 0, v2 = 0;
            ushort clut;
            tPage = 0;
            byte code;
            for (var i = 0; i < 2; i++)
            {
                tag = reader.ReadInt32();
                r0 = reader.ReadByte();
                g0 = reader.ReadByte();
                b0 = reader.ReadByte();
                code = reader.ReadByte();
                x0 = reader.ReadInt16();
                y0 = reader.ReadInt16();
                u0 = reader.ReadByte();
                v0 = reader.ReadByte();
                clut = reader.ReadUInt16();
                r1 = reader.ReadByte();
                g1 = reader.ReadByte();
                b1 = reader.ReadByte();
                reader.ReadByte();
                x1 = reader.ReadInt16();
                y1 = reader.ReadInt16();
                u1 = reader.ReadByte();
                v1 = reader.ReadByte();
                tPage = reader.ReadUInt16();
                r2 = reader.ReadByte();
                g2 = reader.ReadByte();
                b2 = reader.ReadByte();
                reader.ReadByte();
                x2 = reader.ReadInt16();
                y2 = reader.ReadInt16();
                u2 = reader.ReadByte();
                v2 = reader.ReadByte();
                reader.ReadUInt16();
            }
            short v0x, v0y, v0z;
            short v1x, v1y, v1z;
            short v2x, v2y, v2z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
            }
            else
            {
                var vo0 = vertPoint + reader.ReadUInt32();
                var vo1 = vertPoint + reader.ReadUInt32();
                var vo2 = vertPoint + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }

            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_gt3, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r1, g1, b1, r2, g2, b2, u0, v0, u1, v1, u2, v2, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            return triangle;
        }

        private Triangle ReadPolyF3(BinaryReader reader, bool sharedVertices = false, uint vertPoint = 0)
        {
            int tag;
            byte r0 = 0, g0 = 0, b0 = 0;
            short x0, y0;
            short x1, y1;
            short x2, y2;
            byte code;
            for (var i = 0; i < 2; i++)
            {
                tag = reader.ReadInt32();
                r0 = reader.ReadByte();
                g0 = reader.ReadByte();
                b0 = reader.ReadByte();
                code = reader.ReadByte();
                x0 = reader.ReadInt16();
                y0 = reader.ReadInt16();
                x1 = reader.ReadInt16();
                y1 = reader.ReadInt16();
                x2 = reader.ReadInt16();
                y2 = reader.ReadInt16();
            }
            short v0x, v0y, v0z;
            short v1x, v1y, v1z;
            short v2x, v2y, v2z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
            }
            else
            {
                var vo0 = vertPoint + reader.ReadUInt32();
                var vo1 = vertPoint + reader.ReadUInt32();
                var vo2 = vertPoint + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_f3, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r0, g0, b0, r0, g0, b0, 0, 0, 0, 0, 0, 0, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            return triangle;
        }

        private Triangle ReadPolyG3(BinaryReader reader, bool sharedVertices = false, uint vertPoint = 0)
        {
            long tag;
            byte r0 = 0, g0 = 0, b0 = 0;
            byte r1 = 0, g1 = 0, b1 = 0;
            byte r2 = 0, g2 = 0, b2 = 0;
            byte code;
            short x0, y0;
            short x1, y1;
            short x2, y2;
            for (var i = 0; i < 2; i++)
            {
                tag = reader.ReadInt32();
                r0 = reader.ReadByte();
                g0 = reader.ReadByte();
                b0 = reader.ReadByte();
                code = reader.ReadByte();
                x0 = reader.ReadInt16();
                y0 = reader.ReadInt16();
                r1 = reader.ReadByte();
                g1 = reader.ReadByte();
                b1 = reader.ReadByte();
                reader.ReadByte();
                x1 = reader.ReadInt16();
                y1 = reader.ReadInt16();
                r2 = reader.ReadByte();
                g2 = reader.ReadByte();
                b2 = reader.ReadByte();
                reader.ReadByte();
                x2 = reader.ReadInt16();
                y2 = reader.ReadInt16();
            }
            short v0x, v0y, v0z;
            short v1x, v1y, v1z;
            short v2x, v2y, v2z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
            }
            else
            {
                var vo0 = vertPoint + reader.ReadUInt32();
                var vo1 = vertPoint + reader.ReadUInt32();
                var vo2 = vertPoint + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }

            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_g3, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0, 0, 0, 0, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            return triangle;
        }

        private Triangle ReadPolyFT3(BinaryReader reader, out uint tPage, bool sharedVertices = false, uint vertPoint = 0)
        {
            long tag;
            byte r = 0, g = 0, b = 0;
            byte code;
            short x0, y0;
            short x1, y1;
            short x2, y2;
            byte u0 = 0, v0 = 0;
            byte u1 = 0, v1 = 0;
            byte u2 = 0, v2 = 0;
            ushort clutId;
            tPage = 0;
            for (var i = 0; i < 2; i++)
            {
                tag = reader.ReadUInt32();
                r = reader.ReadByte();
                g = reader.ReadByte();
                b = reader.ReadByte();
                code = reader.ReadByte();
                x0 = reader.ReadInt16();
                y0 = reader.ReadInt16();
                u0 = reader.ReadByte();
                v0 = reader.ReadByte();
                clutId = reader.ReadUInt16();
                x1 = reader.ReadInt16();
                y1 = reader.ReadInt16();
                u1 = reader.ReadByte();
                v1 = reader.ReadByte();
                tPage = reader.ReadUInt16();
                x2 = reader.ReadInt16();
                y2 = reader.ReadInt16();
                u2 = reader.ReadByte();
                v2 = reader.ReadByte();
                reader.ReadUInt16();
            }

            short v0x, v0y, v0z;
            short v1x, v1y, v1z;
            short v2x, v2y, v2z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
            }
            else
            {
                var vo0 = vertPoint + reader.ReadUInt32();
                var vo1 = vertPoint + reader.ReadUInt32();
                var vo2 = vertPoint + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_ft3, false, false, null, null, 0, 0, 0, 0, 0,
                0, r, g, b, r, g, b, r, g, b, u0, v0, u1, v1, u2, v2, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            return triangle;
        }

        private void ReadSVector(BinaryReader stream, out short x, out short y, out short z)
        {
            x = stream.ReadInt16();
            y = stream.ReadInt16();
            z = stream.ReadInt16();
            stream.ReadInt16();
        }

        private void ReadSharedVertices(BinaryReader reader, uint vertOffset, out short v0X, out short v0Y, out short v0Z)
        {
            reader.BaseStream.Seek(_offset + vertOffset, SeekOrigin.Begin);
            ReadSVector(reader, out v0X, out v0Y, out v0Z);
        }

        private Triangle TriangleFromPrimitive(Triangle.PrimitiveTypeEnum primitiveType, bool sharedVertices, bool sharedNormals, Vector3[] vertices, Vector3[] normals,
            ushort vertexIndex0, ushort vertexIndex1, ushort vertexIndex2, ushort normalIndex0, ushort normalIndex1, ushort normalIndex2,
            byte r0, byte g0, byte b0, byte r1, byte g1, byte b1, byte r2, byte g2, byte b2, byte u0, byte v0, byte u1, byte v1, byte u2, byte v2,
            short p0x = 0, short p0y = 0, short p0z = 0, short p1x = 0, short p1y = 0, short p1z = 0, short p2x = 0, short p2y = 0, short p2z = 0,
            short n0x = 0, short n0y = 0, short n0z = 0, short n1x = 0, short n1y = 0, short n1z = 0, short n2x = 0, short n2y = 0, short n2z = 0
            )
        {
            if (sharedVertices)
            {
                if (vertexIndex0 >= vertices.Length)// || vertexIndex1 >= vertices.Length || vertexIndex2 >= vertices.Length)
                {
                    return null;
                }
            }

            if (sharedNormals)
            {
                if (normalIndex0 >= normals.Length || normalIndex1 >= normals.Length || normalIndex2 >= normals.Length)
                {
                    return null;
                }
            }

            var vertex0 = sharedVertices ? vertices[vertexIndex0] : new Vector3(p0x, p0y, p0z);
            var vertex1 = sharedVertices ? vertices[vertexIndex1] : new Vector3(p1x, p1y, p1z);
            var vertex2 = sharedVertices ? vertices[vertexIndex2] : new Vector3(p2x, p2y, p2z);

            var normal0 = sharedNormals ? normals[normalIndex0] : new Vector3(n0x, n0y, n0z);
            var normal1 = sharedNormals ? normals[normalIndex1] : new Vector3(n1x, n1y, n1z);
            var normal2 = sharedNormals ? normals[normalIndex2] : new Vector3(n2x, n2y, n2z);

            var color0 = new Color(r0/255f, g0/255f, b0/255f);
            var color1 = new Color(r1/255f, g1/255f, b1/255f);
            var color2 = new Color(r2/255f, g2/255f, b2/255f);

            var uv0 = GeomMath.ConvertUV(u0, v0);
            var uv1 = GeomMath.ConvertUV(u1, v1);
            var uv2 = GeomMath.ConvertUV(u2, v2);

            var triangle = new Triangle
            {
                //PrimitiveType = primitiveType,
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Normals = new[] { normal0, normal1, normal2 },
                Colors = new[] { color0, color1, color2 },
                Uv = new[] { uv0, uv1, uv2 },
                AttachableIndices = new[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }
            };

            return triangle;
        }
    }
}
