using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Classes
{
    public class CrocModelReader
    {
        private long _offset;
        private readonly Action<RootEntity, long> _entityAddedAction;

        public CrocModelReader(Action<RootEntity, long> entityAdded)
        {
            _entityAddedAction = entityAdded;
        }

        public void LookForCrocModel(BinaryReader reader, string fileTitle)
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
                    var model = ReadModels(reader);
                    if (model != null)
                    {
                        model.EntityName = string.Format("{0}{1:X}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                        _entityAddedAction(model, _offset);
                        Program.Logger.WritePositiveLine("Found Croc Model at offset {0:X}", _offset);
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
                    //if (Program.Debug)
                    //{
                    //    Program.Logger.WriteLine(exp);
                    //}
                }
                if (!passed)
                {
                    if (++_offset > reader.BaseStream.Length)
                    {
                        Program.Logger.WriteLine($"Croc - Reached file end: {fileTitle}");
                        return;
                    }
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
                }
            }
        }

        private static RootEntity ReadModels(BinaryReader reader)
        {
            var count = BRenderHelper.ReadU16BE(reader.BaseStream);
            if (count == 0 || count > 10000)
            {
                return null;
            }
            var flags = BRenderHelper.ReadU16BE(reader.BaseStream);
            var models = new List<ModelEntity>();
            for (var i = 0; i < count; i++)
            {
                var modelEntity = new ModelEntity();
                var radius = (int)BRenderHelper.ReadU32BE(reader.BaseStream);
                for (var j = 0; j < 9; j++)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        BRenderHelper.ReadU16BE(reader.BaseStream);
                    }
                }
                var countVerts = BRenderHelper.ReadU32BE(reader.BaseStream);
                if (countVerts == 0 || countVerts > 1000)
                {
                    return null;
                }
                var vertices = new Vector3[countVerts];
                var normals = new Vector3[countVerts];
                for (var j = 0; j < countVerts; j++)
                {
                    var x = (short)BRenderHelper.ReadU16BE(reader.BaseStream) / 16f;
                    var z = (short)BRenderHelper.ReadU16BE(reader.BaseStream) / 16f;
                    var y = -(short)BRenderHelper.ReadU16BE(reader.BaseStream) / 16f;
                    BRenderHelper.ReadU16BE(reader.BaseStream);
                    vertices[j] = new Vector3(x, y, z);
                }
                for (var j = 0; j < countVerts; j++)
                {
                    var x = (short)BRenderHelper.ReadU16BE(reader.BaseStream) / 16f;
                    var z = (short)BRenderHelper.ReadU16BE(reader.BaseStream) / 16f;
                    var y = -(short)BRenderHelper.ReadU16BE(reader.BaseStream) / 16f;
                    BRenderHelper.ReadU16BE(reader.BaseStream);
                    normals[j] = new Vector3(x, y, z);
                }
                var countFaces = BRenderHelper.ReadU32BE(reader.BaseStream);
                if (countFaces == 0)
                {
                    return null;
                }
                var triangles = new List<Triangle>();
                //var header = Encoding.ASCII.GetString(reader.ReadBytes(64));
                for (var j = 0; j < countFaces; j++)
                {
                    var unk1 = reader.BaseStream.ReadByte();
                    var unk2 = reader.BaseStream.ReadByte();
                    var unk3 = reader.BaseStream.ReadByte();
                    var unk4 = reader.BaseStream.ReadByte();
                    var unk5 = reader.BaseStream.ReadByte();
                    var unk6 = reader.BaseStream.ReadByte();
                    var unk7 = reader.BaseStream.ReadByte();
                    var unk8 = reader.BaseStream.ReadByte();
                    var f0 = BRenderHelper.ReadU16BE(reader.BaseStream);
                    var f1 = BRenderHelper.ReadU16BE(reader.BaseStream);
                    var f2 = BRenderHelper.ReadU16BE(reader.BaseStream);
                    var f3 = BRenderHelper.ReadU16BE(reader.BaseStream);
                    if (f0 >= vertices.Length || f1 >= vertices.Length || f2 >= vertices.Length || f3 >= vertices.Length)
                    {
                        return null;
                    }
                    var unk9 = BRenderHelper.ReadU16BE(reader.BaseStream);
                    var unk10 = reader.BaseStream.ReadByte();
                    var primFlags = reader.BaseStream.ReadByte();
                    var vertex0 = vertices[f0];
                    var vertex1 = vertices[f1];
                    var vertex2 = vertices[f2];
                    var normal0 = normals[f0];
                    var normal1 = normals[f1];
                    var normal2 = normals[f2];
                    //if (Math.Abs((normal0 + normal1 + normal2).Length) <= 0.0f)
                    //{
                        normal0 = Vector3.Cross(vertex1 - vertex0, vertex2 - vertex0).Normalized();
                        normal1 = normal0;
                        normal2 = normal0;
                    //}
                    triangles.Add(new Triangle
                    {
                        Vertices = new[] { vertex0, vertex1, vertex2 },
                        Normals = new[] { normal0, normal1, normal2 },
                        Colors = new[] { Color.Grey, Color.Grey, Color.Grey },
                        Uv = new[] { Vector3.Zero, Vector3.Zero, Vector3.Zero },
                        AttachableIndices = new [] {uint.MaxValue, uint.MaxValue, uint.MaxValue}
                    });
                    if ((primFlags & 0x8) != 0)
                    {
                        var vertex3 = vertices[f3];
                        var normal3 = normals[f3];
                        //if (Math.Abs((normal1 + normal3 + normal2).Length) <= 0.0f)
                        {
                            normal3 = Vector3.Cross(vertex3 - vertex1, vertex2 - vertex1).Normalized();
                            normal1 = normal3;
                            normal2 = normal3;
                        }
                        triangles.Add(new Triangle
                        {
                            Vertices = new[] { vertex1, vertex3, vertex2 },
                            Normals = new[] { normal1, normal3, normal2 },
                            Colors = new[] { Color.Grey, Color.Grey, Color.Grey },
                            Uv = new[] { Vector3.Zero, Vector3.Zero, Vector3.Zero },
                            AttachableIndices = new[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }
                        });
                    }
                }
                modelEntity.Triangles = triangles.ToArray();
                models.Add(modelEntity);
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
