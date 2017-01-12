using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using PSXPrev.Classes.Entities;
using PSXPrev.Classes.Mesh;

namespace PSXPrev.Classes.Parsers
{
    public class PMDParser
    {
        private long _offset;

        public List<RootEntity> LookForPMD(BinaryReader reader, string fileTitle)
        {
            if (reader == null)
            {
                throw (new Exception("File must be opened"));
            }

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            var entities = new List<RootEntity>();

            while (reader.BaseStream.CanRead)
            {
                try
                {
                    _offset = reader.BaseStream.Position;
                    var version = reader.ReadUInt32();
                    if (version == 0x00000042)
                    {
                        var entity = ParsePMD(reader);
                        if (entity != null)
                        {
                            entity.EntityName = string.Format("{0}{1:X}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            entities.Add(entity);
                            Program.Logger.WriteLine("Found PMD Model at offset {0:X}", _offset);
                        }
                    }
                }
                catch (Exception exp)
                {
                    if (exp is EndOfStreamException)
                    {
                        //if (checkOffset >= reader.BaseStream.Length - 4)
                        //{
                        break;
                        //}
                        //reader.BaseStream.Seek(checkOffset + 1, SeekOrigin.Begin);
                    }
                    Program.Logger.WriteLine(exp);
                }
                //Debug.WriteLine(checkOffset);
                reader.BaseStream.Seek(_offset + 1, SeekOrigin.Begin);
            }
            return entities;
        }

        private RootEntity ParsePMD(BinaryReader reader)
        {
            var primPoint = reader.ReadUInt32();
            var vertPoint = reader.ReadUInt32();
            var nObj = reader.ReadUInt32();
            if (nObj < 1 || nObj > 4000)
            {
                return null;
            }
            var models = new List<ModelEntity>();
            for (var o = 0; o < nObj; o++)
            {
                var model = new ModelEntity
                {
                    Visible = true,
                    WorldMatrix = Matrix4.Identity
                };
                var triangles = new List<Triangle>();
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
                    if (primType > 15)
                    {
                        return null;
                    }
                    for (var pk = 0; pk < nPacket; pk++)
                    {
                        switch (primType)
                        {
                            case 0x00:
                                triangles.Add(ReadPolyFT3(reader));
                                break;
                            case 0x01:
                                triangles.AddRange(ReadPolyFT4(reader));
                                break;
                            case 0x02:
                                triangles.Add(ReadPolyGT3(reader));
                                break;
                            case 0x03:
                                triangles.AddRange(ReadPolyGT4(reader));
                                break;
                            case 0x04:
                                triangles.Add(ReadPolyF3(reader));
                                break;
                            case 0x05:
                                triangles.AddRange(ReadPolyF4(reader));
                                break;
                            case 0x06:
                                triangles.Add(ReadPolyG3(reader));
                                break;
                            case 0x07:
                                triangles.AddRange(ReadPolyG4(reader));
                                break;
                            case 0x08:
                                triangles.Add(ReadPolyFT3(reader, true, _offset + vertPoint));
                                break;
                            case 0x09:
                                triangles.AddRange(ReadPolyFT4(reader, true, _offset + vertPoint));
                                break;
                            case 0x0a:
                                triangles.Add(ReadPolyGT3(reader, true, _offset + vertPoint));
                                break;
                            case 0x0b:
                                triangles.AddRange(ReadPolyGT4(reader, true, _offset + vertPoint));
                                break;
                            case 0x0c:
                                triangles.Add(ReadPolyF3(reader, true, _offset + vertPoint));
                                break;
                            case 0x0d:
                                triangles.AddRange(ReadPolyF4(reader, true, _offset + vertPoint));
                                break;
                            case 0x0e:
                                triangles.Add(ReadPolyG3(reader, true, _offset + vertPoint));
                                break;
                            case 0x0f:
                                triangles.AddRange(ReadPolyG4(reader, true, _offset + vertPoint));
                                break;
                            default:
                                goto EndObject;
                        }
                    }
                    reader.BaseStream.Seek(position + 4, SeekOrigin.Begin);
                }
            EndObject:
                for (int t = 0; t < triangles.Count; t++)
                {
                    var triangle = triangles[t];
                    triangle.Index = t;
                }
                model.Triangles = triangles;
                models.Add(model);
            }

        EndModel:
            if (models.Count > 0)
            {
                var entity = new RootEntity
                {
                    ChildEntities = models
                };
                entity.ComputeBounds();
                return entity;
            }
            return null;
        }

        private Triangle[] ReadPolyGT4(BinaryReader reader, bool sharedVertices = false, long verticesOffset = 0)
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
            ushort tPage;
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
            Int16 v0x, v0y, v0z;
            Int16 v1x, v1y, v1z;
            Int16 v2x, v2y, v2z;
            Int16 v3x, v3y, v3z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
                ReadSVector(reader, out v3x, out v3y, out v3z);
            }
            else
            {
                long vo0 = verticesOffset + reader.ReadUInt32();
                long vo1 = verticesOffset + reader.ReadUInt32();
                long vo2 = verticesOffset + reader.ReadUInt32();
                long vo3 = verticesOffset + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                ReadSharedVertices(reader, vo3, out v3x, out v3y, out v3z);
                reader.BaseStream.Position = position;
            }
            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_gt4, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r1, g1, b1, r2, g2, b2, u0, v0, u1, v1, u2, v2, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_gt4, false, false, null, null, 0, 0, 0, 0, 0,
             0, r1, g1, b1, r3, g3, b3, r2, g2, b2, u1, v1, u3, v3, u2, v2, v1x, v1y, v1z, v3x, v3y, v3z, v2x, v2y, v2z);
            return new[] { triangle1, triangle2 };
        }

        private Triangle[] ReadPolyG4(BinaryReader reader, bool sharedVertices = false, long verticesOffset = 0)
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
            Int16 v0x, v0y, v0z;
            Int16 v1x, v1y, v1z;
            Int16 v2x, v2y, v2z;
            Int16 v3x, v3y, v3z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
                ReadSVector(reader, out v3x, out v3y, out v3z);
            }
            else
            {
                long vo0 = verticesOffset + reader.ReadUInt32();
                long vo1 = verticesOffset + reader.ReadUInt32();
                long vo2 = verticesOffset + reader.ReadUInt32();
                long vo3 = verticesOffset + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                ReadSharedVertices(reader, vo3, out v3x, out v3y, out v3z);
                reader.BaseStream.Position = position;
            }

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_g4, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0, 0, 0, 0, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_g4, false, false, null, null, 0, 0, 0, 0, 0,
             0, r1, g1, b1, r3, g3, b3, r2, g2, b2, 0, 0, 0, 0, 0, 0, v1x, v1y, v1z, v3x, v3y, v3z, v2x, v2y, v2z);
            return new[] { triangle1, triangle2 };
        }

        private Triangle[] ReadPolyFT4(BinaryReader reader, bool sharedVertices = false, long verticesOffset = 0)
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
            ushort tPage;
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
            Int16 v0x, v0y, v0z;
            Int16 v1x, v1y, v1z;
            Int16 v2x, v2y, v2z;
            Int16 v3x, v3y, v3z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
                ReadSVector(reader, out v3x, out v3y, out v3z);
            }
            else
            {
                long vo0 = verticesOffset + reader.ReadUInt32();
                long vo1 = verticesOffset + reader.ReadUInt32();
                long vo2 = verticesOffset + reader.ReadUInt32();
                long vo3 = verticesOffset + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                ReadSharedVertices(reader, vo3, out v3x, out v3y, out v3z);
                reader.BaseStream.Position = position; 
            }
            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_ft4, false, false, null, null, 0, 0, 0, 0, 0,
                0, r, g, b, r, g, b, r, g, b, u0, v0, u1, v1, u2, v2, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_ft4, false, false, null, null, 0, 0, 0, 0, 0,
               0, r, g, b, r, g, b, r, g, b, u1, v1, u3, v3, u2, v2, v1x, v1y, v1z, v3x, v3y, v3z, v2x, v2y, v2z);
            return new[] { triangle1, triangle2 };
        }

        private Triangle[] ReadPolyF4(BinaryReader reader, bool sharedVertices = false, long verticesOffset = 0)
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
            Int16 v0x, v0y, v0z;
            Int16 v1x, v1y, v1z;
            Int16 v2x, v2y, v2z;
            Int16 v3x, v3y, v3z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
                ReadSVector(reader, out v3x, out v3y, out v3z);
            }
            else
            {
                long vo0 = verticesOffset + reader.ReadUInt32();
                long vo1 = verticesOffset + reader.ReadUInt32();
                long vo2 = verticesOffset + reader.ReadUInt32();
                long vo3 = verticesOffset + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                ReadSharedVertices(reader, vo3, out v3x, out v3y, out v3z);
                reader.BaseStream.Position = position;
            }
            
            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_f4, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r0, g0, b0, r0, g0, b0, 0, 0, 0, 0, 0, 0, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_f4, false, false, null, null, 0, 0, 0, 0, 0,
            0, r0, g0, b0, r0, g0, b0, r0, g0, b0, 0, 0, 0, 0, 0, 0, v1x, v1y, v1z, v3x, v3y, v3z, v2x, v2y, v2z);
            return new[] { triangle1, triangle2 };
        }

        private Triangle ReadPolyGT3(BinaryReader reader, bool sharedVertices = false, long verticesOffset = 0)
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
            ushort tPage;
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
            Int16 v0x, v0y, v0z;
            Int16 v1x, v1y, v1z;
            Int16 v2x, v2y, v2z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
            }
            else
            {
                long vo0 = verticesOffset + reader.ReadUInt32();
                long vo1 = verticesOffset + reader.ReadUInt32();
                long vo2 = verticesOffset + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                reader.BaseStream.Position = position;
            }

            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_gt3, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r1, g1, b1, r2, g2, b2, u0, v0, u1, v1, u2, v2, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            return triangle;
        }

        private Triangle ReadPolyF3(BinaryReader reader, bool sharedVertices = false, long verticesOffset = 0)
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
            Int16 v0x, v0y, v0z;
            Int16 v1x, v1y, v1z;
            Int16 v2x, v2y, v2z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
            }
            else
            {
                long vo0 = verticesOffset + reader.ReadUInt32();
                long vo1 = verticesOffset + reader.ReadUInt32();
                long vo2 = verticesOffset + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                reader.BaseStream.Position = position;
            }
            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_f3, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r0, g0, b0, r0, g0, b0, 0, 0, 0, 0, 0, 0, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            return triangle;
        }

        private Triangle ReadPolyG3(BinaryReader reader, bool sharedVertices = false, long verticesOffset = 0)
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
            Int16 v0x, v0y, v0z;
            Int16 v1x, v1y, v1z;
            Int16 v2x, v2y, v2z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
            }
            else
            {
                long vo0 = verticesOffset + reader.ReadUInt32();
                long vo1 = verticesOffset + reader.ReadUInt32();
                long vo2 = verticesOffset + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                reader.BaseStream.Position = position;
            }

            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_g3, false, false, null, null, 0, 0, 0, 0, 0,
                0, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0, 0, 0, 0, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            return triangle;
        }

        private Triangle ReadPolyFT3(BinaryReader reader, bool sharedVertices = false, long verticesOffset = 0)
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
            ushort tPage;
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

            Int16 v0x, v0y, v0z;
            Int16 v1x, v1y, v1z;
            Int16 v2x, v2y, v2z;
            if (!sharedVertices)
            {
                ReadSVector(reader, out v0x, out v0y, out v0z);
                ReadSVector(reader, out v1x, out v1y, out v1z);
                ReadSVector(reader, out v2x, out v2y, out v2z);
            }
            else
            {
                long vo0 = verticesOffset + reader.ReadUInt32();
                long vo1 = verticesOffset + reader.ReadUInt32();
                long vo2 = verticesOffset + reader.ReadUInt32();
                var position = reader.BaseStream.Position;
                ReadSharedVertices(reader, vo0, out v0x, out v0y, out v0z);
                ReadSharedVertices(reader, vo1, out v1x, out v1y, out v1z);
                ReadSharedVertices(reader, vo2, out v2x, out v2y, out v2z);
                reader.BaseStream.Position = position;
            }
            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum._poly_ft3, false, false, null, null, 0, 0, 0, 0, 0,
                0, r, g, b, r, g, b, r, g, b, u0, v0, u1, v1, u2, v2, v0x, v0y, v0z, v1x, v1y, v1z, v2x, v2y, v2z);
            return triangle;
        }

        private void ReadSVector(BinaryReader stream, out Int16 X, out Int16 Y, out Int16 Z)
        {
            X = stream.ReadInt16();
            Y = stream.ReadInt16();
            Z = stream.ReadInt16();
            stream.ReadInt16();
        }

        private void ReadSharedVertices(BinaryReader reader, long offset, out short v0X, out short v0Y, out short v0Z)
        {
            reader.BaseStream.Position = offset;
            ReadSVector(reader, out v0X, out v0Y, out v0Z);
        }

        private void AddTriangle(Dictionary<int, List<Triangle>> groupedTriangles, Triangle triangle, int p)
        {
            List<Triangle> triangles;
            if (groupedTriangles.ContainsKey(p))
            {
                triangles = groupedTriangles[p];
            }
            else
            {
                triangles = new List<Triangle>();
                groupedTriangles.Add(p, triangles);
            }
            triangles.Add(triangle);
        }

        private Triangle TriangleFromPrimitive(Triangle.PrimitiveTypeEnum primitiveType, bool sharedVertices, bool sharedNormals, Vector3[] vertices, Vector3[] normals, ushort vertex0, ushort vertex1,
            ushort vertex2, ushort normal0, ushort normal1, ushort normal2, byte r0, byte g0, byte b0, byte r1, byte g1,
            byte b1, byte r2, byte g2, byte b2, byte u0, byte v0, byte u1, byte v1, byte u2, byte v2, short p1x = 0, short p1y = 0, short p1z = 0, short p2x = 0, short p2y = 0, short p2z = 0, short p3x = 0, short p3y = 0, short p3z = 0

            , short n1x = 0, short n1y = 0, short n1z = 0, short n2x = 0, short n2y = 0, short n2z = 0, short n3x = 0, short n3y = 0, short n3z = 0

            )
        {
            if (sharedVertices)
            {
                if (vertex0 >= vertices.Length)
                {
                    return null;
                }
            }

            if (sharedNormals)
            {
                if (normal0 >= normals.Length || normal1 >= normals.Length || normal2 >= normals.Length)
                {
                    return null;
                }
            }

            Vector3 ver1, ver2, ver3;
            if (sharedVertices)
            {
                ver1 = new Vector3
                {
                    X = vertices[vertex0].X,
                    Y = vertices[vertex0].Y,
                    Z = vertices[vertex0].Z,
                };

                ver2 = new Vector3
                {
                    X = vertices[vertex1].X,
                    Y = vertices[vertex1].Y,
                    Z = vertices[vertex1].Z,
                };

                ver3 = new Vector3
                {
                    X = vertices[vertex2].X,
                    Y = vertices[vertex2].Y,
                    Z = vertices[vertex2].Z,
                };
            }
            else
            {
                ver1 = new Vector3
                {
                    X = p1x,
                    Y = p1y,
                    Z = p1z
                };
                ver2 = new Vector3
                {
                    X = p2x,
                    Y = p2y,
                    Z = p2z
                };
                ver3 = new Vector3
               {
                   X = p3x,
                   Y = p3y,
                   Z = p3z
               };
            }

            Vector3 nor1, nor2, nor3;
            if (sharedNormals)
            {
                nor1 = new Vector3
                {
                    X = normals[normal0].X,
                    Y = normals[normal0].Y,
                    Z = normals[normal0].Z
                };
                nor2 = new Vector3
                {
                    X = normals[normal1].X,
                    Y = normals[normal1].Y,
                    Z = normals[normal1].Z
                };
                nor3 = new Vector3
                {
                    X = normals[normal2].X,
                    Y = normals[normal2].Y,
                    Z = normals[normal2].Z
                };
            }
            else
            {
                nor1 = new Vector3
                {
                    X = n1x,
                    Y = n1y,
                    Z = n1z
                };
                nor2 = new Vector3
                {
                    X = n2x,
                    Y = n2y,
                    Z = n2z
                };
                nor3 = new Vector3
                {
                    X = n3x,
                    Y = n3y,
                    Z = n3z
                };
            }

            var triangle = new Triangle
            {
                PrimitiveType = primitiveType,
                Colors = new[]
                {
                    new Color.Color
                    {
                        R = r0/256f,
                        G = g0/256f,
                        B = b0/256f
                    },
                    new Color.Color
                    {
                        R = r1/256f,
                        G = g1/256f,
                        B = b1/256f
                    },
                    new Color.Color
                    {
                        R = r2/256f,
                        G = g2/256f,
                        B = b2/256f
                    }
                },
                Normals = new[]
                {
                    nor1,
                    nor2,
                    nor3
                },
                Vertices = new[]
                {
                    ver1,
                    ver2,
                    ver3
                },
                Uv = new[]
                {
                    new Vector3
                    {
                        X = u0/256f,
                        Y = v0/256f
                    },
                    new Vector3
                    {
                        X = u1/256f,
                        Y = v1/256f
                    },
                    new Vector3
                    {
                        X = u2/256f,
                        Y = v2/256f
                    }
                }
            };

            //if (unshaded)
            //{
            //    Vector3 n = glm.cross((ver1 - ver2), (ver2 - ver3));
            //    var magnitude = Math.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);
            //    if (magnitude > 0)
            //    {
            //        n = glm.normalize(n);
            //    }
            //
            //    triangle.Normals[0] = n;
            //    triangle.Normals[1] = n;
            //    triangle.Normals[2] = n;
            //}

            return triangle;
        }
    }
}
