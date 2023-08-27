using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Parsers
{
    public class TMDParser : FileOffsetScanner
    {
        private ObjBlock[] _objBlocks;
        private Vector3[] _vertices;
        private Vector3[] _normals;

        public TMDParser(EntityAddedAction entityAdded)
            : base(entityAdded: entityAdded)
        {
        }

        public override string FormatName => "TMD";

        protected override void Parse(BinaryReader reader)
        {
            var version = reader.ReadUInt32();
            if (Limits.IgnoreTMDVersion || version == 0x00000041)
            {
                var rootEntity = ParseTmd(reader);
                if (rootEntity != null)
                {
                    EntityResults.Add(rootEntity);
                }
            }
        }

        private RootEntity ParseTmd(BinaryReader reader)
        {
            var flags = reader.ReadUInt32();
            if (flags != 0 && flags != 1)
            {
                return null;
            }

            var nObj = reader.ReadUInt32();
            if (nObj == 0 || nObj > Limits.MaxTMDObjects)
            {
                return null;
            }

            var models = new List<ModelEntity>();

            if (_objBlocks == null || _objBlocks.Length < nObj)
            {
                Array.Resize(ref _objBlocks, (int)nObj);
            }
            var objBlocks = _objBlocks;// new ObjBlock[nObj];

            var objTop = (uint)(reader.BaseStream.Position - _offset);

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
                    vertTop += objTop;
                    normalTop += objTop;
                    primitiveTop += objTop;
                }

                if (nVert > Limits.MaxTMDVertices || nNormal > Limits.MaxTMDVertices)
                {
                    return null;
                }
                if (nPrimitive > Limits.MaxTMDPrimitives)
                {
                    return null;
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

            for (uint o = 0; o < nObj; o++)
            {
                var objBlock = objBlocks[o];

                if (Limits.IgnoreTMDVersion && (int)objBlock.VertTop < 0)
                {
                    return null;
                }
                if (_vertices == null || _vertices.Length < objBlock.NVert)
                {
                    Array.Resize(ref _vertices, (int)objBlock.NVert);
                }
                var vertices = _vertices;// new Vector3[objBlock.NVert];
                reader.BaseStream.Seek(_offset + objBlock.VertTop, SeekOrigin.Begin);
                for (var v = 0; v < objBlock.NVert; v++)
                {
                    var vx = reader.ReadInt16();
                    var vy = reader.ReadInt16();
                    var vz = reader.ReadInt16();
                    var pad = reader.ReadInt16();
                    if (pad != 0)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"Found suspicious pad value of: {pad} at index:{v}");
                        }
                    }
                    vertices[v] = new Vector3(vx, vy, vz);
                }

                if (Limits.IgnoreTMDVersion && (int)objBlock.NormalTop < 0)
                {
                    return null;
                }
                if (_normals == null || _normals.Length < objBlock.NNormal)
                {
                    Array.Resize(ref _normals, (int)objBlock.NNormal);
                }
                var normals = _normals;// new Vector3[objBlock.NNormal];
                reader.BaseStream.Seek(_offset + objBlock.NormalTop, SeekOrigin.Begin);
                for (var n = 0; n < objBlock.NNormal; n++)
                {
                    var nx = TMDHelper.ConvertNormal(reader.ReadInt16());
                    var ny = TMDHelper.ConvertNormal(reader.ReadInt16());
                    var nz = TMDHelper.ConvertNormal(reader.ReadInt16());
                    var pad = reader.ReadInt16();
                    if (pad != 0)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"Found suspicious pad value of: {pad} at index:{n}");
                        }
                    }
                    normals[n] = new Vector3(nx, ny, nz).Normalized();
                }

                if (Limits.IgnoreTMDVersion && (int)objBlock.PrimitiveTop < 0)
                {
                    return null;
                }
                reader.BaseStream.Seek(_offset + objBlock.PrimitiveTop, SeekOrigin.Begin);

                if (Program.Debug)
                {
                    Program.Logger.WriteLine($"Primitive count:{objBlock.NPrimitive} {_fileTitle}");
                }

                var groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();
                var groupedSprites = new Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>>();

                Vector3 VertexCallback(uint index)
                {
                    if (index >= objBlock.NVert)
                    {
                        if (Limits.IgnoreTMDVersion)
                        {
                            return new Vector3(index, 0, 0);
                        }
                        if (!Program.ShowErrors)
                        {
                            Program.Logger.WriteErrorLine($"Vertex index error: {_fileTitle}");
                        }
                        throw new Exception($"Vertex index error: {_fileTitle}");
                    }
                    return vertices[index];
                }
                Vector3 NormalCallback(uint index)
                {
                    if (index >= objBlock.NNormal)
                    {
                        if (Limits.IgnoreTMDVersion)
                        {
                            return new Vector3(index, 0, 0);
                        }
                        if (!Program.ShowErrors)
                        {
                            Program.Logger.WriteErrorLine($"Normal index error: {_fileTitle}");
                        }
                        throw new Exception($"Normal index error: {_fileTitle}");
                    }
                    return normals[index];
                }

                for (var p = 0; p < objBlock.NPrimitive; p++)
                {
                    var olen = reader.ReadByte();
                    var ilen = reader.ReadByte();
                    var flag = reader.ReadByte();
                    var mode = reader.ReadByte();
                    var primitivePosition = reader.BaseStream.Position;
                    var packetStructure = TMDHelper.CreateTMDPacketStructure(flag, mode, reader, p);
                    if (packetStructure != null)
                    {
                        switch (packetStructure.PrimitiveType)
                        {
                            case PrimitiveType.Triangle:
                            case PrimitiveType.Quad:
                            case PrimitiveType.StraightLine:
                                TMDHelper.AddTrianglesToGroup(groupedTriangles, packetStructure, false,
                                    VertexCallback, NormalCallback);
                                break;
                            case PrimitiveType.Sprite:
                                TMDHelper.AddSpritesToGroup(groupedSprites, packetStructure,
                                    VertexCallback);
                                break;
                        }

                    }
                    reader.BaseStream.Seek(primitivePosition + ilen * 4, SeekOrigin.Begin);
                }

                var scaleValue = (float)Math.Pow(2, objBlock.Scale); // -2=0.25, -1=0.5, 0=1.0, 1=2.0, 2=4.0, ...
                var scaleMatrix = Matrix4.CreateScale(scaleValue);

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
                        TMDID = o,
                        OriginalLocalMatrix = scaleMatrix,
                    };
                    models.Add(model);
                }
                foreach (var kvp in groupedSprites)
                {
                    var spriteCenter = kvp.Key.Item1;
                    var renderInfo = kvp.Key.Item2;
                    var triangles = kvp.Value;
                    var model = new ModelEntity
                    {
                        Triangles = triangles.ToArray(),
                        TexturePage = renderInfo.TexturePage,
                        RenderFlags = renderInfo.RenderFlags,
                        MixtureRate = renderInfo.MixtureRate,
                        SpriteCenter = spriteCenter,
                        TMDID = o,
                        OriginalLocalMatrix = scaleMatrix,
                    };
                    models.Add(model);
                }
            }

            if (models.Count > 0)
            {
                var entity = new RootEntity();
                entity.ChildEntities = models.ToArray();
                entity.ComputeBounds();
                return entity;
            }
            return null;
        }

        private class ObjBlock
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
}