using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using PSXPrev.Classes.Entities;
using PSXPrev.Classes.Mesh;

namespace PSXPrev.Classes.Parsers
{
    public class TMDParser
    {
        private long _offset;

        public List<RootEntity> LookForTmd(BinaryReader reader, string fileTitle)
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
                            entity.EntityName = string.Format("{0}{1:X}", fileTitle,
                                _offset > 0 ? "_" + _offset : string.Empty);
                            entities.Add(entity);
                            Program.Logger.WriteLine("Found TMD Model at offset {0:X}", _offset);
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
            return entities;
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

            for (var o = 0; o < objBlocks.Length; o++)
            {
                var objBlock = objBlocks[o];

                var vertices = new Vector3[objBlock.NVert];
                reader.BaseStream.Seek(objBlock.VertTop, SeekOrigin.Begin);
                for (var v = 0; v < objBlock.NVert; v++)
                {
                    var vx = reader.ReadInt16();
                    var vy = reader.ReadInt16();
                    var vz = reader.ReadInt16();
                    reader.ReadInt16();
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
                    reader.ReadInt16();
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
                        if (
                            !ParseTMDPF3(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x06 && ilen == 0x04 && mode == 0x30)
                        //3 SIDED, GOURAUD SHADING, FLAT PIGMENT TMD_P_G3
                    {
                        if (
                            !ParseTMDPG3(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x06 && ilen == 0x05 && mode == 0x20)
                        //3 SIDED, FLAT SHADING, GRADIENT PIGMENT TMD_P_F3G
                    {
                        if (
                            !ParseTMDPF3G(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x06 && ilen == 0x06 && mode == 0x30)
                        //3 SIDED, GOURAUD SHADING, GRADIENT PIGMENT TMD_P_G3G
                    {
                        if (
                            !ParseTMDPG3G(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x07 && ilen == 0x05 && mode == 0x24)
                        //3 SIDED, TEXTURED, FLAT SHADING, NO PIGMENT TMD_P_TF3
                    {
                        if (
                            !ParseTMDPTF3(reader, vertices, normals, groupedTriangles, ref hasColors, ref hasNormals,
                                ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x9 && ilen == 0x06 && mode == 0x34)
                        //3 SIDED, TEXTURED, GOURAUD SHADING, NO PIGMENT TMD_P_TG3
                    {
                        if (
                            !ParseTMDPTG3(reader, vertices, normals, groupedTriangles, ref hasColors, ref hasNormals,
                                ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x04 && ilen == 0x03 && mode == 0x21)
                        //3 SIDED, NO SHADING, FLAT PIGMENT TMD_P_NF3
                    {
                        if (
                            !ParseTMDPNF3(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x06 && ilen == 0x05 && mode == 0x31)
                        //3 SIDED, NO SHADING, GRADIENT PIGMENT TMD_P_NG3
                    {
                        if (
                            !ParseTMDPNG3(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x07 && ilen == 0x06 && mode == 0x25)
                        //3 SIDED, TEXTURED, NO SHADING, FLAT PIGMENT TMD_P_TNF3
                    {
                        if (
                            !ParseTMDPTNF3(reader, vertices, normals, groupedTriangles, ref hasColors, ref hasNormals,
                                ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x9 && ilen == 0x08 && mode == 0x35)
                        //3 SIDED, TEXTURED, NO SHADING, GRADIENT PIGMENT TMD_P_TNG3
                    {
                        if (
                            !ParseTMDPTNG3(reader, vertices, normals, groupedTriangles, ref hasColors, ref hasNormals,
                                ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x05 && ilen == 0x04 && mode == 0x28)
                        //4 SIDED, Flat, No-Texture (solid) TMD_P_F4
                    {
                        if (
                            !ParseTMDPF4(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x08 && ilen == 0x05 && mode == 0x38)
                        //4 SIDED, Gouraud, No-Texture (solid) TMD_P_G4
                    {
                        if (
                            !ParseTMDPG4(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x08 && ilen == 0x08 && mode == 0x38)
                        //4 SIDED, Gouraud, No-Texture (gradation) TMD_P_G4G
                    {
                        if (
                            !ParseTMDPG4G(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x08 && ilen == 0x07 && mode == 0x28)
                        //4 SIDED, Flat, No-Texture (gradation) TMD_P_F4G
                    {
                        if (
                            !ParseTMDPF4G(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x09 && ilen == 0x07 && mode == 0x2c)
                        //4 SIDED, Flat, Texture TMD_P_TF4
                    {
                        if (
                            !ParseTMDPTF4(reader, vertices, normals, groupedTriangles, ref hasColors, ref hasNormals,
                                ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x0c && ilen == 0x08 && mode == 0x3c)
                        //4 SIDED, Goraund, Texture TMD_P_TG4
                    {
                        if (
                            !ParseTMDPTG4(reader, vertices, normals, groupedTriangles, ref hasColors, ref hasNormals,
                                ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x05 && ilen == 0x03 && mode == 0x29)
                        //4 SIDED, Flat, No-Texture TMD_P_NF4
                    {
                        if (
                            !ParseTMDPNF4(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x08 && ilen == 0x06 && mode == 0x39)
                        //4 SIDED, Gradation, No-Texture TMD_P_NG4
                    {
                        if (
                            !ParseTMDPNG4(reader, mode, vertices, normals, groupedTriangles, ref hasColors,
                                ref hasNormals, ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x09 && ilen == 0x07 && mode == 0x2d)
                        //4 SIDED, Flat, Texture TMD_P_TNF4
                    {
                        if (
                            !ParseTMDPTNF4(reader, vertices, normals, groupedTriangles, ref hasColors, ref hasNormals,
                                ref hasUvs)) goto EndModel;
                    }
                    else if (olen == 0x0c && ilen == 0x0a && mode == 0x3d)
                        //4 SIDED, Gradation, Texture TMD_P_TNG4
                    {
                        if (
                            !ParseTMDPTNG4(reader, vertices, normals, groupedTriangles, ref hasColors, ref hasNormals,
                                ref hasUvs)) goto EndModel;
                    }
                    else
                    {
                        Program.Logger.WriteLine("Unknown primitive: olen:{0:X}, ilen:{1:X}, mode:{2:X}, flags:{3:X}",
                            olen, ilen, mode, flags);
                        reader.BaseStream.Seek(offset + (ilen*4), SeekOrigin.Begin);
                    }
                }

                foreach (var key in groupedTriangles.Keys)
                {
                    var triangles = groupedTriangles[key];
                    if (triangles.Count > 0)
                    {
                        for (var t = 0; t < triangles.Count; t++)
                        {
                            var triangle = triangles[t];
                            triangle.Index = t;
                        }
                        var model = new ModelEntity
                        {
                            Triangles = triangles,
                            HasNormals = hasNormals,
                            HasColors = hasColors,
                            HasUvs = hasUvs,
                            TexturePage = key,
                            Visible = true,
                            WorldMatrix = Matrix4.Identity
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
                    ChildEntities = models
                };
                entity.ComputeBounds();
                return entity;
            }
            return null;
        }

        private bool ParseTMDPTNG4(BinaryReader reader, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
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

            hasColors = true;
            hasNormals = hasNormals | false;
            hasUvs = true;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNG4, vertices, normals,
                vertex0, vertex1, vertex2, 0, 0, 0, r0, g0, b0, r1, g1,
                b1, r2, g2, b2, u0, v0, u1, v1, u2, v2);
            if (triangle1 == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle1, tPage);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNG4, vertices, normals,
                vertex1, vertex3, vertex2, 0, 0, 0, r1, g1, b1, r3, g3,
                b3, r2, g2, b2, u1, v1, u3, v3, u2, v2);
            if (triangle2 == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle2, tPage);
            return true;
        }

        private bool ParseTMDPTNF4(BinaryReader reader, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
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

            hasColors = true;
            hasNormals = hasNormals | false;
            hasUvs = true;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNF4, vertices, normals,
                vertex0, vertex1, vertex2, 0, 0, 0, r, g, b, r, g,
                b, r, g, b, u0, v0, u1, v1, u2, v2);
            if (triangle1 == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle1, tPage);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TNF4, vertices, normals,
                vertex1, vertex3, vertex2, 0, 0, 0, r, g, b, r, g,
                b, r, g, b, u1, v1, u3, v3, u2, v2);
            if (triangle2 == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle2, tPage);
            return true;
        }

        private bool ParseTMDPNG4(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("4 SIDED, Gradation, No-Texture"); 

            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var mode2 = reader.ReadByte();
            if (mode2 != mode)
            {
                return false; // return null;
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

            hasColors = true;
            hasNormals = hasNormals | false;
            hasUvs = hasUvs | false;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NG4, vertices, normals,
                vertex0, vertex1, vertex2,
                0, 0, 0, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0, 0, 0, 0);
            if (triangle1 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle1, 5);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NG4, vertices, normals,
                vertex1, vertex3, vertex2,
                0, 0, 0, r1, g1, b1, r3, g3, b3, r2, g2, b2, 0, 0, 0, 0, 0, 0);
            if (triangle2 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle2, 5);
            return true;
        }

        private bool ParseTMDPNF4(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("4 SIDED, Flat, No-Texture");

            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var pmode = reader.ReadByte();
            if (pmode != mode)
            {
                return false; // return null;
            }
            var vertex0 = reader.ReadUInt16();
            var vertex1 = reader.ReadUInt16();
            var vertex2 = reader.ReadUInt16();
            var vertex3 = reader.ReadUInt16();

            hasColors = true;
            hasNormals = hasNormals | false;
            hasUvs = hasUvs | false;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NF4, vertices, normals,
                vertex0, vertex1, vertex2, 0,
                0, 0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
            if (triangle1 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle1, 5);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NF4, vertices, normals,
                vertex1, vertex3, vertex2, 0,
                0, 0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
            if (triangle2 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle2, 5);
            return true;
        }

        private bool ParseTMDPTG4(BinaryReader reader, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
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
            hasNormals = true;
            hasUvs = true;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TG4, vertices, normals,
                vertex0, vertex1, vertex2, normal0, normal1, normal2, 128, 128, 128, 128, 128, 128, 128, 128,
                128, u0, v0, u1, v1, u2, v2);
            if (triangle1 == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle1, tPage);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TG4, vertices, normals,
                vertex1, vertex3, vertex2, normal1, normal3, normal2, 128, 128, 128, 128, 128, 128, 128, 128,
                128, u1, v1, u3, v3, u2, v2);
            if (triangle2 == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle2, tPage);
            return true;
        }

        private bool ParseTMDPTF4(BinaryReader reader, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
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
            hasNormals = true;
            hasUvs = true;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TF4, vertices, normals,
                vertex0, vertex1, vertex2, normal0, normal0, normal0, 128, 128, 128, 128, 128, 128, 128, 128,
                128, u0, v0, u1, v1, u2, v2);
            if (triangle1 == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle1, tPage);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_TF4, vertices, normals,
                vertex1, vertex3, vertex2, normal0, normal0, normal0, 128, 128, 128, 128, 128, 128, 128, 128,
                128, u1, v1, u3, v3, u2, v2);
            if (triangle2 == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle2, tPage);
            return true;
        }

        private bool ParseTMDPF4G(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("4 SIDED, Flat, No-Texture (gradation)");

            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var mode2 = reader.ReadByte();
            if (mode2 != mode)
            {
                return false; // return null;
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

            hasColors = true;
            hasNormals = true;
            hasUvs = hasUvs | false;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F4G, vertices, normals,
                vertex0, vertex1, vertex2, normal0, normal0, normal0, r0, g0, b0, r1, g1,
                b1, r2, g2, b2, 0, 0, 0, 0, 0, 0);
            if (triangle1 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle1, 5);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F4G, vertices, normals,
                vertex1, vertex3, vertex2, normal0, normal0, normal0, r1, g1, b1, r3, g3,
                b3, r2, g2, b2, 0, 0, 0, 0, 0, 0);
            if (triangle2 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle2, 5);
            return true;
        }

        private bool ParseTMDPG4G(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("4 SIDED, Gouraud, No-Texture (gradation)");

            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var mode2 = reader.ReadByte();
            if (mode2 != mode)
            {
                return false; // return null;
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

            hasColors = true;
            hasNormals = true;
            hasUvs = hasUvs | false;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G4G, vertices, normals,
                vertex0, vertex1, vertex2,
                normal0, normal1, normal2, r0, g0, b0, r1, g1, b1, r2, g2, b2, 0, 0, 0, 0, 0, 0);
            if (triangle1 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle1, 5);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G4G, vertices, normals,
                vertex1, vertex3, vertex2,
                normal1, normal3, normal2, r1, g1, b1, r3, g3, b3, r2, g2, b2, 0, 0, 0, 0, 0, 0);
            if (triangle2 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle2, 5);
            return true;
        }

        private bool ParseTMDPG4(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("4 SIDED, Gouraud, No-Texture (solid)");

            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var mode2 = reader.ReadByte();
            if (mode2 != mode)
            {
                return false; // return null;
            }
            var normal0 = reader.ReadUInt16();
            var vertex0 = reader.ReadUInt16();
            var normal1 = reader.ReadUInt16();
            var vertex1 = reader.ReadUInt16();
            var normal2 = reader.ReadUInt16();
            var vertex2 = reader.ReadUInt16();
            var normal3 = reader.ReadUInt16();
            var vertex3 = reader.ReadUInt16();

            hasColors = true;
            hasNormals = true;
            hasUvs = hasUvs | false;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G4, vertices, normals,
                vertex0, vertex1, vertex2,
                normal0, normal1, normal2, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
            if (triangle1 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle1, 5);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_G4, vertices, normals,
                vertex1, vertex3, vertex2,
                normal1, normal3, normal2, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
            if (triangle2 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle2, 5);
            return true;
        }

        private bool ParseTMDPF4(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("4 SIDED, Flat, No-Texture (solid)");

            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var pmode = reader.ReadByte();
            if (pmode != mode)
            {
                return false; // return null;
            }
            var normal0 = reader.ReadUInt16();
            var vertex0 = reader.ReadUInt16();
            var vertex1 = reader.ReadUInt16();
            var vertex2 = reader.ReadUInt16();
            var vertex3 = reader.ReadUInt16();
            var pad = reader.ReadUInt16();

            hasColors =  true;
            hasNormals =  true;
            hasUvs = hasUvs | false;

            var triangle1 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F4, vertices, normals,
                vertex0, vertex1, vertex2, normal0,
                normal0, normal0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);

            if (triangle1 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle1, 5);

            var triangle2 = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F4, vertices, normals,
                vertex1, vertex3, vertex2, normal0,
                normal0, normal0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);
            if (triangle2 == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle2, 5);
            return true;
        }

        private bool ParseTMDPTNG3(BinaryReader reader, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
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

            hasColors = true;
            hasNormals = hasNormals | false;
            hasUvs = true;

            if (triangle == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle, tPage);
            return true;
        }

        private bool ParseTMDPTNF3(BinaryReader reader, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
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

            hasColors = true;
            hasNormals = hasNormals | false;
            hasUvs = true;

            if (triangle == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle, tPage);
            return true;
        }

        private bool ParseTMDPNG3(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("3 SIDED, NO SHADING, GRADIENT PIGMENT");

            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var mode2 = reader.ReadByte();
            if (mode2 != mode)
            {
                return false; // return null;
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

            hasColors = true;
            hasNormals = hasNormals | false;
            hasUvs = hasUvs | false;

            if (triangle == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle, 5);
            return true;
        }

        private bool ParseTMDPNF3(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("3 SIDED, NO SHADING, FLAT PIGMENT");

            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var mode2 = reader.ReadByte();
            if (mode2 != mode)
            {
                return false; // return null;
            }
            var vertex0 = reader.ReadUInt16();
            var vertex1 = reader.ReadUInt16();
            var vertex2 = reader.ReadUInt16();
            var pad = reader.ReadUInt16();

            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_NF3, vertices, normals,
                vertex0,
                vertex1, vertex2, 0, 0, 0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0,
                0);

            hasColors = true;
            hasNormals = hasNormals | false;
            hasUvs = hasUvs | false;

            if (triangle == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle, 5);
            return true;
        }

        private bool ParseTMDPTG3(BinaryReader reader, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
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
            hasNormals = true;
            hasUvs = true;

            if (triangle == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle, tPage);
            return true;
        }

        private bool ParseTMDPTF3(BinaryReader reader, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles,
            ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
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
            hasNormals = true;
            hasUvs = hasUvs | false;

            if (triangle == null)
            {
                return false;
            }
            AddTriangle(groupedTriangles, triangle, tPage);
            return true;
        }

        private bool ParseTMDPG3G(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("3 SIDED, GOURAUD SHADING, GRADIENT PIGMENT");

            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var mode2 = reader.ReadByte();
            if (mode2 != mode)
            {
                return false; // return null;
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

            hasColors = true;
            hasNormals = true;
            hasUvs = hasUvs | false;

            if (triangle == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle, 5);
            return true;
        }

        private bool ParseTMDPF3G(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("3 SIDED, FLAT SHADING, GRADIENT PIGMENT");

            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var mode2 = reader.ReadByte();
            if (mode2 != mode)
            {
                return false; // return null;
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

            hasColors = true;
            hasNormals = true;
            hasUvs = hasUvs | false;

            if (triangle == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle, 5);
            return true;
        }

        private bool ParseTMDPG3(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("3 SIDED, GOURAUD SHADING, FLAT PIGMENT");

            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var mode2 = reader.ReadByte();
            if (mode2 != mode)
            {
                return false; // return null;
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

            hasColors = true;
            hasNormals = true;
            hasUvs = hasUvs | false;

            if (triangle == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle, 5);
            return true;
        }

        private bool ParseTMDPF3(BinaryReader reader, byte mode, Vector3[] vertices, Vector3[] normals,
            Dictionary<int, List<Triangle>> groupedTriangles, ref bool hasColors, ref bool hasNormals, ref bool hasUvs)
        {
            //Program.Logger.WriteLine("3 SIDED, FLAT SHADING, FLAT PIGMENT");

            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var pmode = reader.ReadByte();
            if (pmode != mode)
            {
                return false; // return null;
            }
            var normal0 = reader.ReadUInt16();
            var vertex0 = reader.ReadUInt16();
            var vertex1 = reader.ReadUInt16();
            var vertex2 = reader.ReadUInt16();

            var triangle = TriangleFromPrimitive(Triangle.PrimitiveTypeEnum.TMD_P_F3, vertices, normals,
                vertex0, vertex1, vertex2, normal0,
                normal0, normal0, r, g, b, r, g, b, r, g, b, 0, 0, 0, 0, 0, 0);

            hasColors = true;
            hasNormals = hasNormals | false;
            hasUvs = hasUvs | false;

            if (triangle == null)
            {
                return false; // goto EndModel;
            }
            AddTriangle(groupedTriangles, triangle, 5);
            return true;
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

        private Triangle TriangleFromPrimitive(Triangle.PrimitiveTypeEnum primitiveType, Vector3[] vertices,
            Vector3[] normals, ushort vertex0, ushort vertex1,
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

    internal class ObjBlock
    {
        public uint NNormal;
        public uint NPrimitive;
        public uint NVert;
        public uint NormalTop;
        public uint PrimitiveTop;
        public int Scale;
        public uint VertTop;
    }
}