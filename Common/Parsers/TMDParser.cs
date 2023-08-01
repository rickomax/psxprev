using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Parsers
{
    public class TMDParser : FileOffsetScanner
    {
        public TMDParser(EntityAddedAction entityAdded)
            : base(entityAdded: entityAdded)
        {
        }

        public override string FormatName => "TMD";

        protected override void Parse(BinaryReader reader)
        {
            var version = reader.ReadUInt32();
            if (Program.IgnoreTmdVersion || version == 0x00000041)
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
            if (nObj == 0 || nObj > Program.MaxTMDObjects)
            {
                return null;
            }

            var models = new List<ModelEntity>();

            var objBlocks = new ObjBlock[nObj];

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

                if (nPrimitive > Program.MaxTMDPrimitives)
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

            for (uint o = 0; o < objBlocks.Length; o++)
            {
                var objBlock = objBlocks[o];

                var vertices = new Vector3[objBlock.NVert];
                if (Program.IgnoreTmdVersion && (int)objBlock.VertTop < 0)
                {
                    return null;
                }
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
                    var vertex = new Vector3
                    {
                        X = vx,
                        Y = vy,
                        Z = vz
                    };
                    vertices[v] = vertex;
                }

                var normals = new Vector3[objBlock.NNormal];
                if (Program.IgnoreTmdVersion && (int)objBlock.NormalTop < 0)
                {
                    return null;
                }
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
                    var normal = new Vector3
                    {
                        X = nx,
                        Y = ny,
                        Z = nz
                    };
                    normals[n] = normal.Normalized();
                }

                var groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();

                if (Program.IgnoreTmdVersion && (int)objBlock.PrimitiveTop < 0)
                {
                    return null;
                }
                reader.BaseStream.Seek(_offset + objBlock.PrimitiveTop, SeekOrigin.Begin);

                if (Program.Debug)
                {
                    Program.Logger.WriteLine($"Primitive count:{objBlock.NPrimitive} {_fileTitle}");
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
                                TMDHelper.AddTrianglesToGroup(groupedTriangles, packetStructure, false,
                                    index =>
                                    {
                                        if (index >= vertices.Length)
                                        {
                                            if (Program.IgnoreTmdVersion)
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
                                    },
                                    index =>
                                    {
                                        if (index >= normals.Length)
                                        {
                                            if (Program.IgnoreTmdVersion)
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
                                    });
                                break;
                            case PrimitiveType.StraightLine:
                                break;
                            case PrimitiveType.Sprite:

                                break;
                        }

                    }
                    reader.BaseStream.Seek(primitivePosition + ilen * 4, SeekOrigin.Begin);
                }

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
                        TMDID = o
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