#define SHORT_MAP_VERTNOS

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK;


namespace PSXPrev.Classes
{
    public class BFFModelReader
    {
        private long _offset;
        private readonly Action<RootEntity, long> _entityAddedAction;

        public BFFModelReader(Action<RootEntity, long> entityAdded)
        {
            _entityAddedAction = entityAdded;
        }

        public void LookForBFF(BinaryReader reader, string fileTitle)
        {
            if (reader == null)
            {
                throw (new Exception("File must be opened"));
            }
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (reader.BaseStream.CanRead)
            {
                var passed = false;
                try
                {
                    var groupedTriangles = new Dictionary<uint, List<Triangle>>();
                    var model = ReadModels(reader, groupedTriangles);
                    if (model != null)
                    {
                        model.EntityName = string.Format("{0}{1:X}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                        _entityAddedAction(model, reader.BaseStream.Position);
                        Program.Logger.WritePositiveLine("Found BFF Model at offset {0:X}", _offset);
                        _offset = reader.BaseStream.Position;
                        passed = true;
                    }
                    //else
                    //{
                    //    return;
                    //}
                }
                catch (Exception exp)
                {
                    var x = 1;
                    //if (Program.Debug)
                    //{
                    //    Program.Logger.WriteLine(exp);
                    //}
                }
                if (!passed)
                {
                    if (++_offset > reader.BaseStream.Length)
                    {
                        Program.Logger.WriteLine($"BFF - Reached file end: {fileTitle}");
                        return;
                    }
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
                }
            }
        }

        private RootEntity ReadModels(BinaryReader reader, Dictionary<uint, List<Triangle>> groupedTriangles)
        {
            void AddTriangle(Triangle triangle, uint tPage)
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
                triangles.Add(triangle);
            }

            //var id0 = reader.ReadByte();
            //var id1 = reader.ReadByte();
            //var id2 = reader.ReadByte();
            //var id3 = reader.ReadByte();
            //if ((char)id0 != 'P' || (char)id1 != 'S' || (char)id2 != 'I' || id3 != 1)
            //{
            //    return null;
            //}
            //
            //var version = reader.ReadUInt32();
            //var flags = reader.ReadUInt32();
            //var name = Encoding.ASCII.GetString(reader.ReadBytes(32));
            //var meshNum = reader.ReadUInt32();
            //var vertNum = reader.ReadUInt32();
            //var primNum = reader.ReadUInt32();
            //var primOffset = reader.ReadUInt32();
            //var animStart = reader.ReadUInt16();
            //var animEnd = reader.ReadUInt16();
            //var animNum = reader.ReadUInt32();
            //var animSegListOffset = reader.ReadUInt32();
            //var numTex = reader.ReadUInt32();
            //var texOff = reader.ReadUInt32();
            //var firstMeshOff = reader.ReadUInt32();
            //var radius = reader.ReadUInt32();
            //var pad = reader.ReadBytes(192 - 88);
            //
            //var models = new List<ModelEntity>();
            //var position = reader.BaseStream.Position;
            //{
            //    reader.BaseStream.Seek(_offset + firstMeshOff, SeekOrigin.Begin);
            //    var vertTop = reader.ReadUInt32();
            //    var vertCount = reader.ReadUInt32();
            //    var normTop = reader.ReadUInt32();
            //    var normCount = reader.ReadUInt32();
            //    var scale = reader.ReadUInt32();
            //
            //    var meshName = Encoding.ASCII.GetString(reader.ReadBytes(16));
            //    var childTop = reader.ReadUInt32();
            //    var nextTop = reader.ReadUInt32();
            //
            //    var numScaleKeys = reader.ReadUInt16();
            //    var numMoveKeys = reader.ReadUInt16();
            //    var numRotKeys = reader.ReadUInt16();
            //    var pad1 = reader.ReadUInt16();
            //
            //    var scaleKeysTop = reader.ReadUInt32();
            //    var moveKeysTop = reader.ReadUInt32();
            //    var rotateKeysTop = reader.ReadUInt32();
            //
            //    var sortListSize0 = reader.ReadUInt16();
            //    var sortListSize1 = reader.ReadUInt16();
            //    var sortListSize2 = reader.ReadUInt16();
            //    var sortListSize3 = reader.ReadUInt16();
            //    var sortListSize4 = reader.ReadUInt16();
            //    var sortListSize5 = reader.ReadUInt16();
            //    var sortListSize6 = reader.ReadUInt16();
            //    var sortListSize7 = reader.ReadUInt16();
            //
            //    var sortListSizeTop0 = reader.ReadUInt32();
            //    var sortListSizeTop1 = reader.ReadUInt32();
            //    var sortListSizeTop2 = reader.ReadUInt32();
            //    var sortListSizeTop3 = reader.ReadUInt32();
            //    var sortListSizeTop4 = reader.ReadUInt32();
            //    var sortListSizeTop5 = reader.ReadUInt32();
            //    var sortListSizeTop6 = reader.ReadUInt32();
            //    var sortListSizeTop7 = reader.ReadUInt32();
            //
            //    var lastScaleKey = reader.ReadUInt16();
            //    var lastMoveKey = reader.ReadUInt16();
            //    var lastRotKey = reader.ReadUInt16();
            //    var pad2 = reader.ReadUInt16();
            //}

#if SHORT_MAP_VERTNOS
            var BFF_FMA_MESH_ID = 5;
            var BFF_FMA_MESH4_ID = 6;
#else
            var BFF_FMA_MESH_ID = 1;
            var BFF_FMA_MESH4_ID = 4;
#endif

            var id0 = reader.ReadByte();
            var id1 = reader.ReadByte();
            var id2 = reader.ReadByte();
            var id3 = reader.ReadByte(); //4 or 1
            if ((char)id0 != 'F' || (char)id1 != 'M' || (char)id2 != 'M' || id3 != BFF_FMA_MESH_ID && id3 != BFF_FMA_MESH4_ID)
            {
                return null;
            }

            var models = new List<ModelEntity>();

            //            // Version 1 mesh headers include flat-poly information. Like what skies used to.
            //#define BFF_FMA_MESH_ID (('F'<<0) | ('M'<<8) | ('M'<<16) | (1<<24))
            //            //#define BFF_FMA_SKYMESH_ID (('F'<<0) | ('M'<<8) | ('S'<<16) | (0<<24))
            //#define BFF_FMA_MESH4_ID (('F'<<0) | ('M'<<8) | ('M'<<16) | (4<<24))

            var length = ReadInt(reader);
            var nameCrc = ReadLong(reader);

            var minX = ReadIntP(reader);
            var minY = ReadIntP(reader);
            var minZ = ReadIntP(reader);
            var maxX = ReadIntP(reader);
            var maxY = ReadIntP(reader);
            var maxZ = ReadIntP(reader);
            //
            //var offsetX = ReadIntP(reader);
            //var offsetY = ReadIntP(reader);
            //var offsetZ = ReadIntP(reader);
            //var rotX = ReadShort(reader);
            //var rotY = ReadShort(reader);
            //var rotZ = ReadShort(reader);
            //var rotW = ReadShort(reader);

            //var dummy1 = ReadShort(reader);
            //var dummy2 = ReadShort(reader);
            var polyListTop = ReadLong(reader);
            var dummy1 = ReadShort(reader);
            var dummy2 = ReadShort(reader);

            var radius = ReadInt(reader);

            var numVerts = ReadInt(reader);
            var vertsTop = ReadLong(reader) ;
            var position = reader.BaseStream.Position;
            //var offset = reader.BaseStream.Position - _offset;
            reader.BaseStream.Seek(_offset + vertsTop , SeekOrigin.Begin);
            // reader.BaseStream.Seek(288852, SeekOrigin.Begin);
            var vertices = new Vector3[numVerts];
            var uvs = new Vector2[numVerts];
            for (var i = 0; i < numVerts; i++)
            {
                var x = ReadShort(reader);
                var y = ReadShort(reader);
                var z = ReadShort(reader);
                var tu = reader.ReadByte();
                var tv = reader.ReadByte();
                vertices[i] = new Vector3(x, y, z);
                uvs[i] = new Vector2(tu, tv);
            }

            using (var writer = File.CreateText("C:\\USERS\\RICKO\\DESKTOP\\TEST"+ nameCrc+".OBJ"))
            {
                foreach (var vertex in vertices)
                {
                    writer.WriteLine("v " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                }
            }

            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            var numGT3s = ReadInt(reader);
            var primsTop = ReadInt(reader);
            position = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + primsTop, SeekOrigin.Begin);
            if (id3 == BFF_FMA_MESH_ID)
            {
                for (var i = 0; i < numGT3s; i++) //FMA_GT3
                {
                    var r0 = reader.ReadByte();
                    var g0 = reader.ReadByte();
                    var b0 = reader.ReadByte();
                    var code = reader.ReadByte();
                    var u0 = reader.ReadByte();
                    var v0 = reader.ReadByte();
                    var clut = ReadPadData(reader);

                    var r1 = reader.ReadByte();
                    var g1 = reader.ReadByte();
                    var b1 = reader.ReadByte();
                    var pad1 = reader.ReadByte();
                    var u1 = reader.ReadByte();
                    var v1 = reader.ReadByte();
                    var tPage = ReadPadData(reader);

                    var r2 = reader.ReadByte();
                    var g2 = reader.ReadByte();
                    var b2 = reader.ReadByte();
                    var pad2 = reader.ReadByte();
                    var u2 = reader.ReadByte();
                    var v2 = reader.ReadByte();
                    var pad3 = ReadPadData(reader);

                    var vert0 = ReadIndex(reader);
                    var vert1 = ReadIndex(reader);
                    var vert2 = ReadIndex(reader);

                    var triangle = TriangleFromPrimitive(vertices,
                        vert0, vert1, vert2,
                        r0, g0, b0,
                        r1, g1, b1,
                        r2, g2, b2,
                        u0, v0,
                        u1, v1,
                        u2, v2);

                    AddTriangle(triangle, tPage);
                }
            }
            else
            {
                var vert0 = ReadIndex(reader);
                var vert1 = ReadIndex(reader);
                var vert2 = ReadIndex(reader);
                var vert3 = ReadIndex(reader);

                var triangle1 = ReadPolyGT3(reader, vertices, vert0, vert1, vert2, out var tPage1);
                //var triangle2 = ReadPolyGT3(reader, vertices, vert1, vert3, vert2, out var tPage2);
                AddTriangle(triangle1, tPage1);
                //AddTriangle(triangle2, 0);
            }

            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            var numGT4s = ReadInt(reader);
            primsTop = ReadInt(reader);
            position = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + primsTop, SeekOrigin.Begin);
            if (id3 == BFF_FMA_MESH_ID)
            {
                for (var i = 0; i < numGT4s; i++) //FMA_GT4
                {
                    var r0 = reader.ReadByte();
                    var g0 = reader.ReadByte();
                    var b0 = reader.ReadByte();
                    var code = reader.ReadByte();
                    var u0 = reader.ReadByte();
                    var v0 = reader.ReadByte();
                    var clut = ReadPadData(reader);

                    var r1 = reader.ReadByte();
                    var g1 = reader.ReadByte();
                    var b1 = reader.ReadByte();
                    var pad1 = reader.ReadByte();
                    var u1 = reader.ReadByte();
                    var v1 = reader.ReadByte();
                    var tPage = ReadPadData(reader);

                    var r2 = reader.ReadByte();
                    var g2 = reader.ReadByte();
                    var b2 = reader.ReadByte();
                    var pad2 = reader.ReadByte();
                    var u2 = reader.ReadByte();
                    var v2 = reader.ReadByte();
                    var pad3 = ReadPadData(reader);

                    var r3 = reader.ReadByte();
                    var g3 = reader.ReadByte();
                    var b3 = reader.ReadByte();
                    var pad4 = reader.ReadByte();
                    var u3 = reader.ReadByte();
                    var v3 = reader.ReadByte();
                    var pad5 = ReadPadData(reader);

                    var vert0 = ReadIndex(reader);
                    var vert1 = ReadIndex(reader);
                    var vert2 = ReadIndex(reader);
                    var vert3 = ReadIndex(reader);

                    var triangle1 = TriangleFromPrimitive(vertices,
                        vert0, vert1, vert2,
                        r0, g0, b0,
                        r1, g1, b1,
                        r2, g2, b2,
                        u0, v0,
                        u1, v1,
                        u2, v2);

                    var triangle2 = TriangleFromPrimitive(vertices,
                        vert1, vert3, vert2,
                        r1, g1, b1,
                        r3, g3, b3,
                        r2, g2, b2,
                        u1, v1,
                        u3, v3,
                        u2, v2);

                    AddTriangle(triangle1, tPage);
                    AddTriangle(triangle2, tPage);
                }
            }
            else
            {

                var vert0 = ReadIndex(reader);
                var vert1 = ReadIndex(reader);
                var vert2 = ReadIndex(reader);
                var vert3 = ReadIndex(reader);

                //two POLY_GT3
                var triangle1 = ReadPolyGT3(reader, vertices, vert0, vert1, vert2, out var tPage1);
                var triangle2 = ReadPolyGT3(reader, vertices, vert1, vert3, vert2, out var tPage2);
                AddTriangle(triangle1, tPage1);
                AddTriangle(triangle2, tPage2);
            }

            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            var numTMaps = ReadInt(reader);
            var tMapsTop = ReadLong(reader);

            var numG3s = ReadInt(reader);
            primsTop = ReadInt(reader);
            position = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + primsTop, SeekOrigin.Begin);
            for (var i = 0; i < numG3s; i++) //FMA_G3
            {
                var r0 = reader.ReadByte();
                var g0 = reader.ReadByte();
                var b0 = reader.ReadByte();
                var code = reader.ReadByte();

                var r1 = reader.ReadByte();
                var g1 = reader.ReadByte();
                var b1 = reader.ReadByte();
                var pad1 = reader.ReadByte();

                var r2 = reader.ReadByte();
                var g2 = reader.ReadByte();
                var b2 = reader.ReadByte();
                var pad2 = reader.ReadByte();

                var vert0 = ReadIndex(reader);
                var vert1 = ReadIndex(reader);
                var vert2 = ReadIndex(reader);

                var triangle = TriangleFromPrimitive(vertices,
                    vert0, vert1, vert2,
                    r0, g0, b0,
                    r1, g1, b1,
                    r2, g2, b2,
                    0, 0,
                    0, 0,
                    0, 0);

                AddTriangle(triangle, 0);
            }
            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            var numG4s = ReadInt(reader);
            primsTop = ReadInt(reader);
            position = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + primsTop, SeekOrigin.Begin);
            for (var i = 0; i < numG4s; i++) //FMA_G4
            {
                var r0 = reader.ReadByte();
                var g0 = reader.ReadByte();
                var b0 = reader.ReadByte();
                var code = reader.ReadByte();

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

                var vert0 = ReadIndex(reader);
                var vert1 = ReadIndex(reader);
                var vert2 = ReadIndex(reader);
                var vert3 = ReadIndex(reader);

                var triangle1 = TriangleFromPrimitive(vertices,
                    vert0, vert1, vert2,
                    r0, g0, b0,
                    r1, g1, b1,
                    r2, g2, b2,
                    0, 0,
                    0, 0,
                    0, 0);

                var triangle2 = TriangleFromPrimitive(vertices,
                    vert1, vert3, vert2,
                    r1, g1, b1,
                    r3, g3, b3,
                    r2, g2, b2,
                    0, 0,
                    0, 0,
                    0, 0);


                AddTriangle(triangle1, 0);
                AddTriangle(triangle2, 0);
            }
            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            foreach (var kvp in groupedTriangles)
            {
                var triangles = kvp.Value;
                if (triangles.Count > 0)
                {
                    var model = new ModelEntity
                    {
                        Triangles = triangles.ToArray(),
                        TexturePage = kvp.Key,
                        TMDID = 0
                    };
                    models.Add(model);
                }
            }

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

        private uint ReadPadData(BinaryReader reader)
        {
            return reader.ReadUInt16();
        }

        private uint ReadIndex(BinaryReader reader)
        {
#if SHORT_MAP_VERTNOS
            var n = reader.ReadUInt16();
            return (uint) n;
#else
            return reader.ReadUInt32();
#endif
        }

        private uint ReadShort(BinaryReader reader)
        {
            return reader.ReadUInt16();
        }

        private int ReadIntP(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        private uint ReadInt(BinaryReader reader)
        {
            return reader.ReadUInt32();
        }

        private uint ReadLong(BinaryReader reader)
        {
            return reader.ReadUInt32();
        }

        private Triangle ReadPolyGT3(BinaryReader reader, Vector3[] vertices, uint vert0, uint vert1, uint vert2, out uint tPage)
        {
            var tag = reader.ReadInt32();
            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var code = reader.ReadByte();
            var x0 = reader.ReadInt16();
            var y0 = reader.ReadInt16();
            var u0 = reader.ReadByte();
            var v0 = reader.ReadByte();
            var clut = reader.ReadUInt16();
            var r1 = reader.ReadByte();
            var g1 = reader.ReadByte();
            var b1 = reader.ReadByte();
            reader.ReadByte();
            var x1 = reader.ReadInt16();
            var y1 = reader.ReadInt16();
            var u1 = reader.ReadByte();
            var v1 = reader.ReadByte();
            tPage = reader.ReadUInt16();
            var r2 = reader.ReadByte();
            var g2 = reader.ReadByte();
            var b2 = reader.ReadByte();
            reader.ReadByte();
            var x2 = reader.ReadInt16();
            var y2 = reader.ReadInt16();
            var u2 = reader.ReadByte();
            var v2 = reader.ReadByte();
            reader.ReadUInt16();
            return TriangleFromPrimitive(vertices, vert0, vert1, vert2, r0, g0, b0, r1, g1, b1, r2, g2, b2, u0, v0, u1, v1, u2, v2);
        }

//# ifndef _WINDOWS
//
//#define GETX(n)( ((SHORTXY *)( (int)(tfv) +(n) ))->x )
//#define GETY(n)( ((SHORTXY *)( (int)(tfv) +(n) ))->y )
//#define GETV(n)(  *(u_long *)( (int)(tfv) +(n) ) )
//#define GETD(n)(  *(u_long *)( (int)(tfd) +(n) ) )
//#define GETN(n)( ((VERT *)( (int)(tfn) +(n<<1) )) )
//
//#else
//
//#define GETX(n)( ((SHORTXY *)( (int)(tfv) +(n<<2) ))->x )
//#define GETY(n)( ((SHORTXY *)( (int)(tfv) +(n<<2) ))->y )
//#define GETV(n)(  *(u_long *)( (int)(tfv) +(n<<2) ) )
//#define GETD(n)(  *(u_long *)( (int)(tfd) +(n<<2) ) )
//#define GETN(n)( ((VERT *)( (int)(tfn) +(n<<3) )) )
//
//#endif

        private Triangle TriangleFromPrimitive(Vector3[] vertices,
            uint vertex0, uint vertex1, uint vertex2,
            byte r0, byte g0, byte b0,
            byte r1, byte g1, byte b1,
            byte r2, byte g2, byte b2,
            byte u0, byte v0,
            byte u1, byte v1,
            byte u2, byte v2
            )
        {
            Vector3 ver1, ver2, ver3;
            if (vertex0 >= vertices.Length || vertex1 >= vertices.Length || vertex2 >= vertices.Length)
            {
                
                
                throw new Exception("Out of indices");
            }
            else
            {

                ver1 = vertices[vertex0];
                ver2 = vertices[vertex1];
                ver3 = vertices[vertex2];
            }

            var triangle = new Triangle
            {
                Colors = new[]
                {
                    new Color
                    (
                        r0/255f,
                        g0/255f,
                        b0/255f
                    ),
                    new Color
                    (
                         r1/255f,
                         g1/255f,
                         b1/255f
                    ),
                    new Color
                    (
                         r2/255f,
                         g2/255f,
                         b2/255f
                    )
                },
                Normals = new[]
                {
                    Vector3.Zero,
                    Vector3.Zero,
                    Vector3.Zero,
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
                        X = u0/255f,
                        Y = v0/255f
                    },
                    new Vector3
                    {
                        X = u1/255f,
                        Y = v1/255f
                    },
                    new Vector3
                    {
                        X = u2/255f,
                        Y = v2/255f
                    }
                },
                AttachableIndices = new[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }
            };

            return triangle;
        }
    }
}
