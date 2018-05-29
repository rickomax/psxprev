using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev
{
    public class HMDParser
    {
        private long _offset;

        private Action<RootEntity, long> entityAddedAction;

        public HMDParser(Action<RootEntity, long> entityAddedAction)
        {
            this.entityAddedAction = entityAddedAction;
        }


        public void LookForHMDEntities(BinaryReader reader, string fileTitle)
        {
            if (reader == null)
            {
                throw (new Exception("File must be opened"));
            }

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            //var entities = new List<RootEntity>();

            while (reader.BaseStream.CanRead)
            {
                try
                {
                    _offset = reader.BaseStream.Position;
                    var version = reader.ReadUInt32();
                    if (version == 0x00000050)
                    {
                        var entity = ParseHMDEntities(reader);
                        if (entity != null)
                        {
                            entity.EntityName = string.Format("{0}{1:X}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            //entities.Add(entity);
                            entityAddedAction(entity, reader.BaseStream.Position);
                            Program.Logger.WriteLine("Found HMD Model at offset {0:X}", _offset);
                        }
                    }
                }
                catch (Exception exp)
                {
                    if (exp is EndOfStreamException)
                    {
                        break;
                    }
                    Program.Logger.WriteLine(exp);
                }
                reader.BaseStream.Seek(_offset + 1, SeekOrigin.Begin);
            }
        }

        private RootEntity ParseHMDEntities(BinaryReader reader)
        {
            var rootEntity = new RootEntity();
            var mapFlag = reader.ReadUInt32();
            var primitiveHeaderTop = reader.ReadUInt32() * 4;
            var blockCount = reader.ReadUInt32();
            var modelEntities = new List<ModelEntity>();
            for (var i = 0; i < blockCount; i++)
            {
                var primitiveSetTop = reader.ReadUInt32() * 4;
                if (primitiveSetTop == 0)
                {
                    continue;
                }
                var blockTop = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + primitiveSetTop, SeekOrigin.Begin);
                ProccessPrimitive(reader, modelEntities, i);
                reader.BaseStream.Seek(blockTop, SeekOrigin.Begin);
            }
            var coordCount = reader.ReadUInt32();
            for (var c = 0; c < coordCount; c++)
            {
                Matrix4 worldMatrix = ReadCoord(reader);
                modelEntities[c].WorldMatrix = worldMatrix;
            }
            rootEntity.ChildEntities = modelEntities.ToArray();
            rootEntity.ComputeBounds();
            return rootEntity;
        }

        private Matrix4 ReadCoord(BinaryReader reader)
        {
            var flag = reader.ReadUInt32();
            var worldMatrix = ReadMatrix(reader);
            var workMatrix = ReadMatrix(reader);
            short rx, ry, rz;
            rx = reader.ReadInt16();
            ry = reader.ReadInt16();
            rz = reader.ReadInt16();
            var code = reader.ReadInt16();
            var super = reader.ReadUInt32() * 4;
            if (super != 0)
            {
                var top = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + super, SeekOrigin.Begin);
                var superMatrix = ReadCoord(reader);
                reader.BaseStream.Seek(top, SeekOrigin.Begin);
                return superMatrix * worldMatrix;
            }
            return worldMatrix;
        }

        private static Matrix4 ReadMatrix(BinaryReader reader)
        {
            float r00 = reader.ReadInt16() / 4096f;
            float r01 = reader.ReadInt16() / 4096f;
            float r02 = reader.ReadInt16() / 4096f;

            float r10 = reader.ReadInt16() / 4096f;
            float r11 = reader.ReadInt16() / 4096f;
            float r12 = reader.ReadInt16() / 4096f;

            float r20 = reader.ReadInt16() / 4096f;
            float r21 = reader.ReadInt16() / 4096f;
            float r22 = reader.ReadInt16() / 4096f;

            var x = reader.ReadInt32() / 4096f;
            var y = reader.ReadInt32() / 4096f;
            var z = reader.ReadInt32() / 4096f;
            var matrix = new Matrix4(
                new Vector4(r00, r10, r20, 0f),
                new Vector4(r01, r11, r21, 0f),
                new Vector4(r02, r12, r22, 0f),
                new Vector4(x, y, z, 1f)
            );
            return matrix;
        }

        private void ProccessPrimitive(BinaryReader reader, List<ModelEntity> modelEntities, int modelIndex)
        {
            var triangles = new List<Triangle>();
            var nextPrimitivePointer = reader.ReadUInt32() * 4;
            var primitiveHeaderPointer = reader.ReadUInt32() * 4;
            var typeCountMapped = reader.ReadUInt32();
            var mapped = typeCountMapped >> 0x1F & 0x1;
            var typeCount = typeCountMapped & 0x1F;
            for (var j = 0; j < typeCount; j++)
            {
                var primitiveType = reader.ReadUInt16();
                var dataType = reader.ReadUInt16();
                var developerId = (dataType & 0xF000) >> 0xC;
                var category = (dataType & 0xF00) >> 0x8;  //0: Polygon data 1: Shared polygon data 2: Image data 3: Animation data 4: MIMe data 5: Ground dat      
                var driver = dataType & 0xFF;
                var dataCountAndSize = reader.ReadUInt32();
                var flag = (dataCountAndSize & 0x80000000) >> 0x1F;
                var dataCount = (dataCountAndSize & 0x3FFF0000) >> 0x10;
                var dataSize = dataCountAndSize & 0xFFFF;
                var polygonIndex = reader.ReadUInt32() * 4;
                if (category == 0)
                {
                    ProcessNonSharedPrimitiveData(triangles, reader, driver, primitiveType, primitiveHeaderPointer, nextPrimitivePointer, polygonIndex, dataCount);
                }
            }
            var modelEntity = new ModelEntity();
            modelEntity.Triangles = triangles.ToArray();
            modelEntity.HasColors = true;
            modelEntity.HasNormals = true;
            modelEntity.HasUvs = false;
            modelEntities.Add(modelEntity);
            if (nextPrimitivePointer < 1000)
            {
                reader.BaseStream.Seek(_offset + nextPrimitivePointer, SeekOrigin.Begin);
                ProccessPrimitive(reader, modelEntities, modelIndex);
            }
        }

        private void ProcessNonSharedPrimitiveData(List<Triangle> triangles, BinaryReader reader, int driver, int primitiveType, uint primitiveHeaderPointer, uint nextPrimitivePointer, uint polygonIndex, uint dataCount)
        {
            var primitiveDataTop = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();

            var headerTop = reader.BaseStream.Position;
            var dataTopMapped = reader.ReadInt32();
            var dataMapped = (dataTopMapped & 0x80000000) >> 0x1F;
            var dataTop = (dataTopMapped & 0x7FFFFFFF) * 4;

            var vertTopMapped = reader.ReadInt32();
            var vertMapped = (vertTopMapped & 0x80000000) >> 0x1F;
            var vertTop = (vertTopMapped & 0x7FFFFFFF) * 4;

            var normTopMapped = reader.ReadInt32();
            var normMapped = (normTopMapped & 0x80000000) >> 0x1F;
            var normTop = (normTopMapped & 0x7FFFFFFF) * 4;

            var coordTopMapped = reader.ReadInt32();
            var coordMapped = (coordTopMapped & 0x80000000) >> 0x1F;
            var coordTop = (coordTopMapped & 0x7FFFFFFF) * 4;

            reader.BaseStream.Seek(_offset + dataTop + polygonIndex, SeekOrigin.Begin);
            for (var j = 0; j < dataCount; j++)
            {
                Triangle triangle1;
                Triangle triangle2;
                switch (primitiveType)
                {
                    case 0x00000008: //0x00000008; DRV(0)|PRIM_TYPE(TRI); GsUF3
                        triangle1 = ReadGsUF3(reader, vertTop, normTop);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        triangles.Add(triangle1);
                        break;
                    case 0x00000010: //  0x00000010; DRV(0) | PRIM_TYPE(QUAD); GsUF4
                        ReadGsUF4(reader, vertTop, normTop, out triangle1, out triangle2);
                        if (triangle1 == null || triangle2 == null)
                        {
                            goto EndModel;
                        }
                        triangles.Add(triangle1);
                        triangles.Add(triangle2);
                        break;
                }
            }
            EndModel:
            reader.BaseStream.Seek(primitiveDataTop, SeekOrigin.Begin);
        }

        private Triangle ReadGsUF3(BinaryReader reader, int vertTop, int normTop)
        {
            byte r, g, b;
            ReadColor(reader, out r, out g, out b);

            var ni0 = reader.ReadUInt16();
            short nx0, ny0, nz0;
            ReadXYZ(reader, normTop, ni0, out nx0, out ny0, out nz0);

            var vi0 = reader.ReadUInt16();
            short vx0, vy0, vz0;
            ReadXYZ(reader, vertTop, vi0, out vx0, out vy0, out vz0);

            var vi1 = reader.ReadUInt16();
            short vx1, vy1, vz1;
            ReadXYZ(reader, vertTop, vi1, out vx1, out vy1, out vz1);

            var vi2 = reader.ReadUInt16();
            short vx2, vy2, vz2;
            ReadXYZ(reader, vertTop, vi2, out vx2, out vy2, out vz2);

            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.GsUF3, false, false, new Vector3[] { }, new Vector3[] { }, 0, 0, 0, 0, 0, 0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0,
               vx0, vy0, vz0,
               vx1, vy1, vz1,
               vx2, vy2, vz2,
               nx0, ny0, nz0,
               nx0, ny0, nz0,
               nx0, ny0, nz0
            );
            return triangle;
        }

        private void ReadGsUF4(BinaryReader reader, int vertTop, int normTop, out Triangle triangle1, out Triangle triangle2)
        {
            byte r, g, b;
            ReadColor(reader, out r, out g, out b);

            var ni0 = reader.ReadUInt16();
            short nx0, ny0, nz0;
            ReadXYZ(reader, normTop, ni0, out nx0, out ny0, out nz0);

            var vi0 = reader.ReadUInt16();
            short vx0, vy0, vz0;
            ReadXYZ(reader, vertTop, vi0, out vx0, out vy0, out vz0);

            var vi1 = reader.ReadUInt16();
            short vx1, vy1, vz1;
            ReadXYZ(reader, vertTop, vi1, out vx1, out vy1, out vz1);

            var vi2 = reader.ReadUInt16();
            short vx2, vy2, vz2;
            ReadXYZ(reader, vertTop, vi2, out vx2, out vy2, out vz2);

            var vi3 = reader.ReadUInt16();
            short vx3, vy3, vz3;
            ReadXYZ(reader, vertTop, vi3, out vx3, out vy3, out vz3);

            triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.GsUF4, false, false, new Vector3[] { }, new Vector3[] { }, 0, 0, 0, 0, 0, 0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0,
               vx0, vy0, vz0,
               vx1, vy1, vz1,
               vx2, vy2, vz2,
               nx0, ny0, nz0,
               nx0, ny0, nz0,
               nx0, ny0, nz0
            );

            triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.GsUF4, false, false, new Vector3[] { }, new Vector3[] { }, 0, 0, 0, 0, 0, 0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0,
               vx1, vy1, vz1,
               vx3, vy3, vz3,
               vx2, vy2, vz2,
               nx0, ny0, nz0,
               nx0, ny0, nz0,
               nx0, ny0, nz0
            );
        }

        private static void ReadColor(BinaryReader reader, out byte r, out byte g, out byte b)
        {
            r = reader.ReadByte();
            g = reader.ReadByte();
            b = reader.ReadByte();
            var code = reader.ReadByte();
        }

        private void ReadXYZ(BinaryReader reader, long dataTop, int index, out short x, out short y, out short z)
        {
            var top = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + dataTop + (index * 8), SeekOrigin.Begin);
            x = reader.ReadInt16();
            y = reader.ReadInt16();
            z = reader.ReadInt16();
            var code = reader.ReadInt16();
            reader.BaseStream.Seek(top, SeekOrigin.Begin);
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
                    new Color
                    {
                        R = r0/256f,
                        G = g0/256f,
                        B = b0/256f
                    },
                    new Color
                    {
                        R = r1/256f,
                        G = g1/256f,
                        B = b1/256f
                    },
                    new Color
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

            return triangle;
        }

        //private void ReadPrimitiveHeaderPointer(BinaryReader reader, uint primitiveHeaderPointer)
        //{
        //    var offset = reader.BaseStream.Position;
        //    reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);
        //    var primitiveHeaderCount = reader.ReadUInt32();
        //    for (var i = 0; i < primitiveHeaderCount; i++)
        //    {
        //        var sectionCount = reader.ReadUInt32();
        //        for (var j = 0; j < sectionCount; j++)
        //        {
        //            var pointer = reader.ReadUInt32();
        //            var x = 1;
        //        }
        //        //reader.BaseStream.Seek(sectionCount, SeekOrigin.Current);
        //    }
        //}
    }
}
