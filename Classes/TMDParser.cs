using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTK;

namespace PSXPrev.Classes
{
    public class TMDParser
    {
        private long _offset;

        private readonly Action<RootEntity, long> _entityAddedAction;

        public TMDParser(Action<RootEntity, long> entityAddedAction)
        {
            _entityAddedAction = entityAddedAction;
        }

        public void LookForTmd(BinaryReader reader, string fileTitle)
        {
            if (reader == null)
            {
                throw new Exception("File must be opened");
            }

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            while (reader.BaseStream.CanRead)
            {
                var passed = false;
                try
                {
                    var version = reader.ReadUInt32();
                    if (Program.IgnoreTmdVersion || version == 0x00000041)
                    {
                        var entity = ParseTmd(reader, fileTitle);
                        if (entity != null)
                        {
                            entity.EntityName = string.Format("{0}{1:X}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            _entityAddedAction(entity, reader.BaseStream.Position);
                            Program.Logger.WritePositiveLine("Found TMD Model at offset {0:X}", _offset);
                            _offset = reader.BaseStream.Position;
                            passed = true;
                        }
                    }
                }
                catch (Exception exp)
                {
                    //if (Program.Debug)
                    //{
                    //    Program.Logger.WriteLine(exp);
                    //}
                }
                if (!passed)
                {
                    if (++_offset > reader.BaseStream.Length)
                    {
                        Program.Logger.WriteLine($"TMD - Reached file end: {fileTitle}");
                        return;
                    }
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
                }
            }
        }

        private RootEntity ParseTmd(BinaryReader reader, string fileTitle)
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
                    vertTop += (uint)objOffset;
                    normalTop += (uint)objOffset;
                    primitiveTop += (uint)objOffset;
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
                if (Program.IgnoreTmdVersion && objBlock.VertTop < _offset)
                {
                    return null;
                }
                reader.BaseStream.Seek(objBlock.VertTop, SeekOrigin.Begin);
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
                if (Program.IgnoreTmdVersion && objBlock.NormalTop < _offset)
                {
                    return null;
                }
                reader.BaseStream.Seek(objBlock.NormalTop, SeekOrigin.Begin);
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

                var groupedTriangles = new Dictionary<uint, List<Triangle>>();

                reader.BaseStream.Seek(objBlock.PrimitiveTop, SeekOrigin.Begin);
                if (Program.IgnoreTmdVersion && objBlock.PrimitiveTop < _offset)
                {
                    return null;
                }

                if (Program.Debug)
                {
                    Program.Logger.WriteLine($"Primitive count:{objBlock.NPrimitive} {fileTitle}");
                }
                for (var p = 0; p < objBlock.NPrimitive; p++)
                {
                    var olen = reader.ReadByte();
                    var ilen = reader.ReadByte();
                    var flag = reader.ReadByte();
                    var mode = reader.ReadByte();
                    var offset = reader.BaseStream.Position;
                    var packetStructure = TMDHelper.CreateTMDPacketStructure(flag, mode, reader, p);
                    if (packetStructure != null)
                    {
                        TMDHelper.AddTrianglesToGroup(groupedTriangles, packetStructure, false, delegate (uint index)
                        {
                            if (index >= vertices.Length)
                            {
                                if (Program.IgnoreTmdVersion)
                                {
                                    return new Vector3(index, 0, 0);
                                }
                                Program.Logger.WriteErrorLine("Vertex index error : " + fileTitle);
                                throw new Exception("Vertex index error: " + fileTitle);
                            }
                            return vertices[index];
                        }, delegate (uint index)
                        {
                            if (index >= normals.Length)
                            {
                                if (Program.IgnoreTmdVersion)
                                {
                                    return new Vector3(index, 0, 0);
                                }
                                Program.Logger.WriteErrorLine("Vertex index error: " + fileTitle);
                                throw new Exception("Vertex index error: " + fileTitle);
                            }
                            return normals[index];
                        });
                    }
                    var newOffset = offset + ilen * 4;
                    if (Program.IgnoreTmdVersion && newOffset < _offset)
                    {
                        return null;
                    }
                    reader.BaseStream.Seek(newOffset, SeekOrigin.Begin);
                }

                foreach (var kvp in groupedTriangles)
                {
                    var triangles = kvp.Value;
                    if (triangles.Count > 0)
                    {
                        var model = new ModelEntity
                        {
                            Triangles = triangles.ToArray(),
                            TexturePage = kvp.Key,
                            TMDID = o
                        };
                        models.Add(model);
                    }
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
    }
}