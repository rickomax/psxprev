using System;
using System.Collections.Generic;
using System.IO;

using OpenTK;

namespace PSXPrev
{
    public class TMDParser
    {
        private long _offset;

        public RootEntity[] LookForTmd(BinaryReader reader, string fileTitle)
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
                    if (version == 0x00000041)
                    {
                        var entity = ParseTmd(reader);
                        if (entity != null)
                        {
                            entity.EntityName = string.Format("{0}{1:X}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            entities.Add(entity);
                            Program.Logger.WriteLine("Found TMD Model at offset {0:X}", _offset);
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
            return entities.ToArray();
        }

        private RootEntity ParseTmd(BinaryReader reader)
        {
            var flags = reader.ReadUInt32();
            if (flags != 0 && flags != 1)
            {
                return null;
            }

            var nObj = reader.ReadUInt32();
            if (nObj == 0 || nObj > 5000)
            {
                return null;
            }

            var models = new List<ModelEntity>();

            var objBlocks = new ObjBlock[nObj];

            var objOffset = reader.BaseStream.Position;

            for (var o = 0; o < nObj; o++)
            {
                var vertTop = reader.ReadUInt32();
                var nVert = reader.ReadUInt32();
                var normalTop = reader.ReadUInt32();
                var nNormal = reader.ReadUInt32();
                var primitiveTop = reader.ReadUInt32();
                var nPrimitive = reader.ReadUInt32();
                var scale = reader.ReadInt32();

                if (flags == 0)
                {
                    vertTop += (uint) objOffset;
                    normalTop += (uint) objOffset;
                    primitiveTop += (uint) objOffset;
                }

                objBlocks[o] = new ObjBlock
                {
                    VertTop = vertTop,
                    NVert = nVert,
                    NormalTop = normalTop,
                    NNormal = nNormal,
                    PrimitiveTop = primitiveTop,
                    NPrimitive = nPrimitive,
                    Scale = scale
                };
            }

            for (int o = 0; o < objBlocks.Length; o++)
            {
                var objBlock = objBlocks[o];

                var vertices = new Vector3[objBlock.NVert];
                reader.BaseStream.Seek(objBlock.VertTop, SeekOrigin.Begin);
                for (var v = 0; v < objBlock.NVert; v++)
                {
                    var vx = reader.ReadInt16();
                    var vy = reader.ReadInt16();
                    var vz = reader.ReadInt16();
                    var pad = reader.ReadInt16();
                    var vertex = new Vector3
                    {
                        X = vx,
                        Y = vy,
                        Z = vz
                    };
                    vertices[v] = vertex;
                }

                var normals = new Vector3[objBlock.NNormal];
                reader.BaseStream.Seek(objBlock.NormalTop, SeekOrigin.Begin);
                for (var n = 0; n < objBlock.NNormal; n++)
                {
                    var nx = reader.ReadInt16();
                    var ny = reader.ReadInt16();
                    var nz = reader.ReadInt16();
                    var pad = reader.ReadInt16();
                    var normal = new Vector3
                    {
                        X = nx == 0 ? nx : nx/4096f,
                        Y = ny == 0 ? ny : ny/4096f,
                        Z = nz == 0 ? nz : nz/4096f
                    };
                    normals[n] = normal;
                }

                var hasNormals = false;
                var hasColors = false;
                var hasUvs = false;

                var groupedTriangles = new Dictionary<int, List<Triangle>>();
                //var missingTriangles = new List<MissingTriangle>();

                reader.BaseStream.Seek(objBlock.PrimitiveTop, SeekOrigin.Begin);
                for (var p = 0; p < objBlock.NPrimitive; p++)
                {
                    var olen = reader.ReadByte();
                    var ilen = reader.ReadByte();
                    var flag = reader.ReadByte();
                    var mode = reader.ReadByte();
                    //var option = (mode & 0x1F);

                    var offset = reader.BaseStream.Position;

                    if (olen == 0x04 && ilen == 0x03 && mode == 0x20)
                        //3 SIDED, FLAT SHADING, FLAT PIGMENT TMD_P_F3
                    {
                        //Program.Logger.WriteLine("3 SIDED, FLAT SHADING, FLAT PIGMENT");

                        var r = reader.ReadByte();
                        var g = reader.ReadByte();
                        var b = reader.ReadByte();
                        var pmode = reader.ReadByte();
                        if (pmode != mode)
                        {
                            return null;
                        }
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F3, vertices, normals,
                            vertex0, vertex1, vertex2, normal0,
                            normal0, normal0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | false;
                        hasUvs = hasUvs | false;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, 5);
                    }
                    else if (olen == 0x06 && ilen == 0x04 && mode == 0x30)
                        //3 SIDED, GOURAUD SHADING, FLAT PIGMENT TMD_P_G3
                    {
                        //Program.Logger.WriteLine("3 SIDED, GOURAUD SHADING, FLAT PIGMENT");

                        var r = reader.ReadByte();
                        var g = reader.ReadByte();
                        var b = reader.ReadByte();
                        var mode2 = reader.ReadByte();
                        if (mode2 != mode)
                        {
                            return null;
                        }
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var normal1 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var normal2 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G3, vertices, normals,
                            vertex0, vertex1, vertex2,
                            normal0, normal1, normal2, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | false;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, 5);
                    }
                    else if (olen == 0x06 && ilen == 0x05 && mode == 0x20)
                        //3 SIDED, FLAT SHADING, GRADIENT PIGMENT TMD_P_F3G
                    {
                        //Program.Logger.WriteLine("3 SIDED, FLAT SHADING, GRADIENT PIGMENT");

                        var r0 = reader.ReadByte();
                        var g0 = reader.ReadByte();
                        var b0 = reader.ReadByte();
                        var mode2 = reader.ReadByte();
                        if (mode2 != mode)
                        {
                            return null;
                        }
                        var r1 = reader.ReadByte();
                        var g1 = reader.ReadByte();
                        var b1 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var r2 = reader.ReadByte();
                        var g2 = reader.ReadByte();
                        var b2 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F3G, vertices, normals,
                            vertex0, vertex1, vertex2,
                            normal0, normal0, normal0, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0, 0, 0, 0);

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | false;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, 5);
                    }
                    else if (olen == 0x06 && ilen == 0x06 && mode == 0x30)
                        //3 SIDED, GOURAUD SHADING, GRADIENT PIGMENT TMD_P_G3G
                    {
                        //Program.Logger.WriteLine("3 SIDED, GOURAUD SHADING, GRADIENT PIGMENT");

                        var r0 = reader.ReadByte();
                        var g0 = reader.ReadByte();
                        var b0 = reader.ReadByte();
                        var mode2 = reader.ReadByte();
                        if (mode2 != mode)
                        {
                            return null;
                        }
                        var r1 = reader.ReadByte();
                        var g1 = reader.ReadByte();
                        var b1 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var r2 = reader.ReadByte();
                        var g2 = reader.ReadByte();
                        var b2 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var normal1 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var normal2 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G3G, vertices, normals,
                            vertex0, vertex1,
                            vertex2, normal0, normal1, normal2, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0,
                            0, 0, 0);

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | false;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, 5);
                    }
                    else if (olen == 0x07 && ilen == 0x05 && mode == 0x24)
                        //3 SIDED, TEXTURED, FLAT SHADING, NO PIGMENT TMD_P_TF3
                    {
                        //Program.Logger.WriteLine("3 SIDED, TEXTURED, FLAT SHADING, NO PIGMENT");

                        var u0 = reader.ReadByte();
                        var v0 = reader.ReadByte();
                        var cba = reader.ReadUInt16();
                        var u1 = reader.ReadByte();
                        var v1 = reader.ReadByte();
                        var tsb = reader.ReadUInt16();
                        var tPage = tsb & 0x1F;
                        var u2 = reader.ReadByte();
                        var v2 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var normal = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TF3, vertices, normals,
                            vertex0, vertex1,
                            vertex2, normal, normal, normal, 128, 128, 128, 128, 128, 128, 128, 128, 128, u0, v0, u1, v1,
                            u2, v2);

                        hasColors = hasColors | false;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | false;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, tPage);
                    }
                    else if (olen == 0x9 && ilen == 0x06 && mode == 0x34)
                        //3 SIDED, TEXTURED, GOURAUD SHADING, NO PIGMENT TMD_P_TG3
                    {
                        //Program.Logger.WriteLine("3 SIDED, TEXTURED, GOURAUD SHADING, NO PIGMENT");

                        var u0 = reader.ReadByte();
                        var v0 = reader.ReadByte();
                        var cba = reader.ReadUInt16();
                        var u1 = reader.ReadByte();
                        var v1 = reader.ReadByte();
                        var tsb = reader.ReadUInt16();
                        var tPage = tsb & 0x1F;
                        var u2 = reader.ReadByte();
                        var v2 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var normal1 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var normal2 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TG3, vertices, normals,
                            vertex0,
                            vertex1, vertex2, normal0, normal1, normal2, 128, 128, 128, 128, 128, 128, 128, 128, 128,
                            u0, v0, u1, v1, u2, v2);

                        hasColors = hasColors | false;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | true;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, tPage);
                    }
                    else if (olen == 0x04 && ilen == 0x03 && mode == 0x21)
                        //3 SIDED, NO SHADING, FLAT PIGMENT TMD_P_NF3
                    {
                        //Program.Logger.WriteLine("3 SIDED, NO SHADING, FLAT PIGMENT");

                        var r = reader.ReadByte();
                        var g = reader.ReadByte();
                        var b = reader.ReadByte();
                        var mode2 = reader.ReadByte();
                        if (mode2 != mode)
                        {
                            return null;
                        }
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var pad = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NF3, vertices, normals,
                            vertex0,
                            vertex1, vertex2, 0, 0, 0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0,
                            0);

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | false;
                        hasUvs = hasUvs | false;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, 5);
                    }
                    else if (olen == 0x06 && ilen == 0x05 && mode == 0x31)
                        //3 SIDED, NO SHADING, GRADIENT PIGMENT TMD_P_NG3
                    {
                        //Program.Logger.WriteLine("3 SIDED, NO SHADING, GRADIENT PIGMENT");

                        var r0 = reader.ReadByte();
                        var g0 = reader.ReadByte();
                        var b0 = reader.ReadByte();
                        var mode2 = reader.ReadByte();
                        if (mode2 != mode)
                        {
                            return null;
                        }
                        var r1 = reader.ReadByte();
                        var g1 = reader.ReadByte();
                        var b1 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var r2 = reader.ReadByte();
                        var g2 = reader.ReadByte();
                        var b2 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var pad = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NG3, vertices, normals,
                            vertex0,
                            vertex1, vertex2, 0, 0, 0, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0,
                            0, 0, 0, 0, 0);

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | false;
                        hasUvs = hasUvs | false;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, 5);
                    }
                    else if (olen == 0x07 && ilen == 0x06 && mode == 0x25)
                        //3 SIDED, TEXTURED, NO SHADING, FLAT PIGMENT TMD_P_TNF3
                    {
                        //Program.Logger.WriteLine("3 SIDED, TEXTURED, NO SHADING, FLAT PIGMENT");

                        var u0 = reader.ReadByte();
                        var v0 = reader.ReadByte();
                        var cba = reader.ReadUInt16();
                        var u1 = reader.ReadByte();
                        var v1 = reader.ReadByte();
                        var tsb = reader.ReadUInt16();
                        var tPage = tsb & 0x1F;
                        var u2 = reader.ReadByte();
                        var v2 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var r = reader.ReadByte();
                        var g = reader.ReadByte();
                        var b = reader.ReadByte();
                        var pad3 = reader.ReadByte();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var pad = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNF3, vertices, normals,
                            vertex0, vertex1, vertex2, 0, 0, 0, r, g, b, r, g, b, r, g,
                            b, u0, v0, u1, v1, u2, v2);

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | false;
                        hasUvs = hasUvs | true;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, tPage);
                    }
                    else if (olen == 0x9 && ilen == 0x08 && mode == 0x35)
                        //3 SIDED, TEXTURED, NO SHADING, GRADIENT PIGMENT TMD_P_TNG3
                    {
                        //Program.Logger.WriteLine("3 SIDED, TEXTURED, NO SHADING, GRADIENT PIGMENT");

                        var u0 = reader.ReadByte();
                        var v0 = reader.ReadByte();
                        var cba = reader.ReadUInt16();
                        var u1 = reader.ReadByte();
                        var v1 = reader.ReadByte();
                        var tsb = reader.ReadUInt16();
                        var tPage = tsb & 0x1F;
                        var u2 = reader.ReadByte();
                        var v2 = reader.ReadByte();
                        var pad1 = reader.ReadUInt16();
                        var r0 = reader.ReadByte();
                        var g0 = reader.ReadByte();
                        var b0 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var r1 = reader.ReadByte();
                        var g1 = reader.ReadByte();
                        var b1 = reader.ReadByte();
                        var pad3 = reader.ReadByte();
                        var r2 = reader.ReadByte();
                        var g2 = reader.ReadByte();
                        var b2 = reader.ReadByte();
                        var pad4 = reader.ReadByte();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var pad = reader.ReadUInt16();

                        var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNG3, vertices, normals,
                            vertex0, vertex1, vertex2, 0, 0, 0, r0, g0, b0, r1, g1,
                            b1, r2, g2, b2, u0, v0, u1, v1, u2, v2);

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | false;
                        hasUvs = hasUvs | true;

                        if (triangle == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle, tPage);
                    }
                    else if (olen == 0x05 && ilen == 0x04 && mode == 0x28)
                        //4 SIDED, Flat, No-Texture (solid) TMD_P_F4
                    {
                        //Program.Logger.WriteLine("4 SIDED, Flat, No-Texture (solid)");

                        var r = reader.ReadByte();
                        var g = reader.ReadByte();
                        var b = reader.ReadByte();
                        var pmode = reader.ReadByte();
                        if (pmode != mode)
                        {
                            return null;
                        }
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();
                        var pad = reader.ReadUInt16();

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | false;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F4, vertices, normals,
                            vertex0, vertex1, vertex2, normal0,
                            normal0, normal0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);

                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, 5);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F4, vertices, normals,
                            vertex1, vertex3, vertex2, normal0,
                            normal0, normal0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, 5);
                    }
                    else if (olen == 0x08 && ilen == 0x05 && mode == 0x38)
                        //4 SIDED, Gouraud, No-Texture (solid) TMD_P_G4
                    {
                        //Program.Logger.WriteLine("4 SIDED, Gouraud, No-Texture (solid)");

                        var r = reader.ReadByte();
                        var g = reader.ReadByte();
                        var b = reader.ReadByte();
                        var mode2 = reader.ReadByte();
                        if (mode2 != mode)
                        {
                            return null;
                        }
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var normal1 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var normal2 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var normal3 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | false;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G4, vertices, normals,
                            vertex0, vertex1, vertex2,
                            normal0, normal1, normal2, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, 5);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G4, vertices, normals,
                            vertex1, vertex3, vertex2,
                            normal1, normal3, normal2, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, 5);
                    }
                    else if (olen == 0x08 && ilen == 0x08 && mode == 0x38)
                        //4 SIDED, Gouraud, No-Texture (gradation) TMD_P_G4G
                    {
                        //Program.Logger.WriteLine("4 SIDED, Gouraud, No-Texture (gradation)");

                        var r0 = reader.ReadByte();
                        var g0 = reader.ReadByte();
                        var b0 = reader.ReadByte();
                        var mode2 = reader.ReadByte();
                        if (mode2 != mode)
                        {
                            return null;
                        }
                        var r1 = reader.ReadByte();
                        var g1 = reader.ReadByte();
                        var b1 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var r2 = reader.ReadByte();
                        var g2 = reader.ReadByte();
                        var b2 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var r3 = reader.ReadByte();
                        var g3 = reader.ReadByte();
                        var b3 = reader.ReadByte();
                        var pad3 = reader.ReadByte();
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var normal1 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var normal2 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var normal3 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | false;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G4G, vertices, normals,
                            vertex0, vertex1, vertex2,
                            normal0, normal1, normal2, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0, 0, 0, 0);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, 5);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G4G, vertices, normals,
                            vertex1, vertex3, vertex2,
                            normal1, normal3, normal2, r1, g1, b1, r3, g3, b3, r2, g2, b2, 0, 0, 0, 0, 0, 0);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, 5);
                    }
                    else if (olen == 0x08 && ilen == 0x07 && mode == 0x28)
                        //4 SIDED, Flat, No-Texture (gradation) TMD_P_F4G
                    {
                        //Program.Logger.WriteLine("4 SIDED, Flat, No-Texture (gradation)");

                        var r0 = reader.ReadByte();
                        var g0 = reader.ReadByte();
                        var b0 = reader.ReadByte();
                        var mode2 = reader.ReadByte();
                        if (mode2 != mode)
                        {
                            return null;
                        }

                        var r1 = reader.ReadByte();
                        var g1 = reader.ReadByte();
                        var b1 = reader.ReadByte();
                        var pad1 = reader.ReadByte();

                        var r2 = reader.ReadByte();
                        var g2 = reader.ReadByte();
                        var b2 = reader.ReadByte();
                        var pad2 = reader.ReadByte();

                        var r3 = reader.ReadByte();
                        var g3 = reader.ReadByte();
                        var b3 = reader.ReadByte();
                        var pad3 = reader.ReadByte();

                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();
                        var pad = reader.ReadUInt16();

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | false;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F4G, vertices, normals,
                            vertex0, vertex1, vertex2, normal0, normal0, normal0, r0, g0, b0, r1, g1,
                            b1, r2, g2, b2, 0, 0, 0, 0, 0, 0);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, 5);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F4G, vertices, normals,
                            vertex1, vertex3, vertex2, normal0, normal0, normal0, r1, g1, b1, r3, g3,
                            b3, r2, g2, b2, 0, 0, 0, 0, 0, 0);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, 5);
                    }
                    else if (olen == 0x09 && ilen == 0x07 && mode == 0x2c)
                        //4 SIDED, Flat, Texture TMD_P_TF4
                    {
                        //Program.Logger.WriteLine("4 SIDED, Flat, Texture");

                        var u0 = reader.ReadByte();
                        var v0 = reader.ReadByte();
                        var cba = reader.ReadUInt16();
                        var u1 = reader.ReadByte();
                        var v1 = reader.ReadByte();
                        var tsb = reader.ReadUInt16();
                        var tPage = tsb & 0x1F;
                        var u2 = reader.ReadByte();
                        var v2 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var u3 = reader.ReadByte();
                        var v3 = reader.ReadByte();
                        var pad3 = reader.ReadByte();
                        var pad4 = reader.ReadByte();
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();
                        var pad5 = reader.ReadUInt16();

                        hasColors = hasColors | false;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | true;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TF4, vertices, normals,
                            vertex0, vertex1, vertex2, normal0, normal0, normal0, 128, 128, 128, 128, 128, 128, 128, 128,
                            128, u0, v0, u1, v1, u2, v2);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, tPage);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TF4, vertices, normals,
                            vertex1, vertex3, vertex2, normal0, normal0, normal0, 128, 128, 128, 128, 128, 128, 128, 128,
                            128, u1, v1, u3, v3, u2, v2);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, tPage);
                    }
                    else if (olen == 0x0c && ilen == 0x08 && mode == 0x3c)
                        //4 SIDED, Goraund, Texture TMD_P_TG4
                    {
                        //Program.Logger.WriteLine("4 SIDED, Goraund, Texture");

                        var u0 = reader.ReadByte();
                        var v0 = reader.ReadByte();
                        var cba = reader.ReadUInt16();
                        var u1 = reader.ReadByte();
                        var v1 = reader.ReadByte();
                        var tsb = reader.ReadUInt16();
                        var tPage = tsb & 0x1F;
                        var u2 = reader.ReadByte();
                        var v2 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var u3 = reader.ReadByte();
                        var v3 = reader.ReadByte();
                        var pad3 = reader.ReadByte();
                        var pad4 = reader.ReadByte();
                        var normal0 = reader.ReadUInt16();
                        var vertex0 = reader.ReadUInt16();
                        var normal1 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var normal2 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var normal3 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();

                        hasColors = hasColors | false;
                        hasNormals = hasNormals | true;
                        hasUvs = hasUvs | true;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TG4, vertices, normals,
                            vertex0, vertex1, vertex2, normal0, normal1, normal2, 128, 128, 128, 128, 128, 128, 128, 128,
                            128, u0, v0, u1, v1, u2, v2);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, tPage);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TG4, vertices, normals,
                            vertex1, vertex3, vertex2, normal1, normal3, normal2, 128, 128, 128, 128, 128, 128, 128, 128,
                            128, u1, v1, u3, v3, u2, v2);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, tPage);
                    }
                    else if (olen == 0x05 && ilen == 0x03 && mode == 0x29)
                        //4 SIDED, Flat, No-Texture TMD_P_NF4
                    {
                        //Program.Logger.WriteLine("4 SIDED, Flat, No-Texture");

                        var r = reader.ReadByte();
                        var g = reader.ReadByte();
                        var b = reader.ReadByte();
                        var pmode = reader.ReadByte();
                        if (pmode != mode)
                        {
                            return null;
                        }
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | false;
                        hasUvs = hasUvs | false;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NF4, vertices, normals,
                            vertex0, vertex1, vertex2, 0,
                            0, 0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, 5);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NF4, vertices, normals,
                            vertex1, vertex3, vertex2, 0,
                            0, 0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, 5);
                    }
                    else if (olen == 0x08 && ilen == 0x06 && mode == 0x39)
                        //4 SIDED, Gradation, No-Texture TMD_P_NG4
                    {
                        //Program.Logger.WriteLine("4 SIDED, Gradation, No-Texture"); 

                        var r0 = reader.ReadByte();
                        var g0 = reader.ReadByte();
                        var b0 = reader.ReadByte();
                        var mode2 = reader.ReadByte();
                        if (mode2 != mode)
                        {
                            return null;
                        }
                        var r1 = reader.ReadByte();
                        var g1 = reader.ReadByte();
                        var b1 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var r2 = reader.ReadByte();
                        var g2 = reader.ReadByte();
                        var b2 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var r3 = reader.ReadByte();
                        var g3 = reader.ReadByte();
                        var b3 = reader.ReadByte();
                        var pad3 = reader.ReadByte();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | false;
                        hasUvs = hasUvs | false;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NG4, vertices, normals,
                            vertex0, vertex1, vertex2,
                            0, 0, 0, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0, 0, 0, 0);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, 5);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NG4, vertices, normals,
                            vertex1, vertex3, vertex2,
                            0, 0, 0, r1, g1, b1, r3, g3, b3, r2, g2, b2, 0, 0, 0, 0, 0, 0);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, 5);
                    }
                    else if (olen == 0x09 && ilen == 0x07 && mode == 0x2d)
                        //4 SIDED, Flat, Texture TMD_P_TNF4
                    {
                        //Program.Logger.WriteLine("4 SIDED, Flat, Texture");

                        var u0 = reader.ReadByte();
                        var v0 = reader.ReadByte();
                        var cba = reader.ReadUInt16();
                        var u1 = reader.ReadByte();
                        var v1 = reader.ReadByte();
                        var tsb = reader.ReadUInt16();
                        var tPage = tsb & 0x1F;
                        var u2 = reader.ReadByte();
                        var v2 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var u3 = reader.ReadByte();
                        var v3 = reader.ReadByte();
                        var pad3 = reader.ReadByte();
                        var pad4 = reader.ReadByte();
                        var r = reader.ReadByte();
                        var g = reader.ReadByte();
                        var b = reader.ReadByte();
                        var pad5 = reader.ReadByte();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | false;
                        hasUvs = hasUvs | true;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNF4, vertices, normals,
                            vertex0, vertex1, vertex2, 0, 0, 0, r, g, b, r, g,
                            b, r, g, b, u0, v0, u1, v1, u2, v2);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, tPage);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNF4, vertices, normals,
                            vertex1, vertex3, vertex2, 0, 0, 0, r, g, b, r, g,
                            b, r, g, b, u1, v1, u3, v3, u2, v2);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, tPage);
                    }
                    else if (olen == 0x0c && ilen == 0x0a && mode == 0x3d)
                        //4 SIDED, Gradation, Texture TMD_P_TNG4
                    {
                        //Program.Logger.WriteLine("4 SIDED, Gradation, Texture");

                        var u0 = reader.ReadByte();
                        var v0 = reader.ReadByte();
                        var cba = reader.ReadUInt16();
                        var u1 = reader.ReadByte();
                        var v1 = reader.ReadByte();
                        var tsb = reader.ReadUInt16();
                        var tPage = tsb & 0x1F;
                        var u2 = reader.ReadByte();
                        var v2 = reader.ReadByte();
                        var pad1 = reader.ReadByte();
                        var pad2 = reader.ReadByte();
                        var u3 = reader.ReadByte();
                        var v3 = reader.ReadByte();
                        var pad3 = reader.ReadByte();
                        var pad4 = reader.ReadByte();
                        var r0 = reader.ReadByte();
                        var g0 = reader.ReadByte();
                        var b0 = reader.ReadByte();
                        var pad5 = reader.ReadByte();
                        var r1 = reader.ReadByte();
                        var g1 = reader.ReadByte();
                        var b1 = reader.ReadByte();
                        var pad6 = reader.ReadByte();
                        var r2 = reader.ReadByte();
                        var g2 = reader.ReadByte();
                        var b2 = reader.ReadByte();
                        var pad7 = reader.ReadByte();
                        var r3 = reader.ReadByte();
                        var g3 = reader.ReadByte();
                        var b3 = reader.ReadByte();
                        var pad8 = reader.ReadByte();
                        var vertex0 = reader.ReadUInt16();
                        var vertex1 = reader.ReadUInt16();
                        var vertex2 = reader.ReadUInt16();
                        var vertex3 = reader.ReadUInt16();

                        hasColors = hasColors | true;
                        hasNormals = hasNormals | false;
                        hasUvs = hasUvs | true;

                        var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNG4, vertices, normals,
                            vertex0, vertex1, vertex2, 0, 0, 0, r0, g0, b0, r1, g1,
                            b1, r2, g2, b2, u0, v0, u1, v1, u2, v2);
                        if (triangle1 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle1, tPage);

                        var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNG4, vertices, normals,
                            vertex1, vertex3, vertex2, 0, 0, 0, r1, g1, b1, r3, g3,
                            b3, r2, g2, b2, u1, v1, u3, v3, u2, v2);
                        if (triangle2 == null)
                        {
                            goto EndModel;
                        }
                        AddTriangle(groupedTriangles, triangle2, tPage);
                    }
                    else
                    {
                        //missingTriangles.Add(new MissingTriangle
                        //{
                        //    Olen = olen,
                        //    Ilen = ilen,
                        //    Flags = flags,
                        //    Mode = mode
                        //});
                        Program.Logger.WriteLine("Unknown primitive: olen:{0:X}, ilen:{1:X}, mode:{2:X}, flags:{3:X}",
                            olen, ilen, mode, flags);
                        reader.BaseStream.Seek(offset + (ilen*4), SeekOrigin.Begin);
                        //goto EndModel;
                    }
                }

                foreach (var key in groupedTriangles.Keys)
                {
                    var triangles = groupedTriangles[key];
                    if (triangles.Count > 0)
                    {
                        var model = new ModelEntity
                        {
                            Triangles = triangles.ToArray(),
                            HasNormals = hasNormals,
                            HasColors = hasColors,
                            HasUvs = hasUvs,
                            TexturePage = key,
                            Visible = true
                        };
                        models.Add(model);
                    }
                }
            }

            EndModel:
            if (models.Count > 0)
            {
                var entity = new RootEntity
                {
                    ChildEntities = (EntityBase[]) models.ToArray()
                };
                entity.ComputeBounds();
                return entity;
            }
            return null;
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

        private Triangle TriangleFromPrimitive(Triangle.PrimitiveTypeEnum primitiveType, Vector3[] vertices, Vector3[] normals, ushort vertex0, ushort vertex1,
            ushort vertex2, ushort normal0, ushort normal1, ushort normal2, byte r0, byte g0, byte b0, byte r1, byte g1,
            byte b1, byte r2, byte g2, byte b2, byte u0, byte v0, byte u1, byte v1, byte u2, byte v2)
        {
            if (vertex0 >= vertices.Length)
            {
                return null;
            }

            if (normal0 >= normals.Length || normal1 >= normals.Length || normal2 >= normals.Length)
            {
                return null;
            }

            var ver1 = new Vector3
            {
                X = vertices[vertex0].X,
                Y = vertices[vertex0].Y,
                Z = vertices[vertex0].Z,
            };

            var ver2 = new Vector3
            {
                X = vertices[vertex1].X,
                Y = vertices[vertex1].Y,
                Z = vertices[vertex1].Z,
            };

            var ver3 = new Vector3
            {
                X = vertices[vertex2].X,
                Y = vertices[vertex2].Y,
                Z = vertices[vertex2].Z,
            };

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
                    new Vector3
                    {
                        X = normals[normal0].X,
                        Y = normals[normal0].Y,
                        Z = normals[normal0].Z
                    },
                    new Vector3
                    {
                        X = normals[normal1].X,
                        Y = normals[normal1].Y,
                        Z = normals[normal1].Z
                    },
                    new Vector3
                    {
                        X = normals[normal2].X,
                        Y = normals[normal2].Y,
                        Z = normals[normal2].Z
                    }
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
    }

    internal class ObjBlock
    {
        public uint VertTop;
        public uint NVert;
        public uint NormalTop;
        public uint NNormal;
        public uint PrimitiveTop;
        public uint NPrimitive;
        public int Scale;
    }
}
