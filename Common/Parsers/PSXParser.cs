using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using OpenTK;

//Translated from:
//https://gist.github.com/iamgreaser/2a67f7473d9c48a70946018b73fa1e40

namespace PSXPrev.Common.Parsers
{
    public class PSXParser : FileOffsetScanner
    {
        private PSXModel[] _objectModels;
        private Vector3[] _vertices;
        private Vector3[] _normals;

        public PSXParser(EntityAddedAction entityAdded)
            : base(entityAdded: entityAdded)
        {
        }

        public override string FormatName => "PSX";

        protected override void Parse(BinaryReader reader)
        {
            var rootEntity = ReadModels(reader);
            if (rootEntity != null)
            {
                EntityResults.Add(rootEntity);
            }
        }

        private RootEntity ReadModels(BinaryReader reader)
        {
            // (modelIndex, RenderInfo)
            var groupedTriangles = new Dictionary<Tuple<uint, RenderInfo>, List<Triangle>>();

            void AddTriangle(Triangle triangle, uint modelIndex, uint tPage, RenderFlags renderFlags)
            {
                renderFlags |= RenderFlags.DoubleSided; //todo
                if (renderFlags.HasFlag(RenderFlags.Textured))
                {
                    triangle.CorrectUVTearing();
                }
                var renderInfo = new RenderInfo(tPage, renderFlags);
                var tuple = new Tuple<uint, RenderInfo>(modelIndex, renderInfo);
                if (!groupedTriangles.TryGetValue(tuple, out var triangles))
                {
                    triangles = new List<Triangle>();
                    groupedTriangles.Add(tuple, triangles);
                }
                triangles.Add(triangle);
            }

            var version = reader.ReadUInt16();
            var magic   = reader.ReadUInt16();
            if (version != 0x0004 && version != 0x0003 && version != 0x0006)
            {
                return null;
            }
            if (magic != 0x0002)
            {
                return null;
            }

            var metaPtr = reader.ReadUInt32(); //todo: read meta
            var objectCount = reader.ReadUInt32();
            if (objectCount == 0 || objectCount > Limits.MaxPSXObjectCount)
            {
                return null;
            }
            if (_objectModels == null || _objectModels.Length < objectCount)
            {
                Array.Resize(ref _objectModels, (int)objectCount);
            }
            var objectModels = _objectModels;// new PSXModel[objectCount];
            for (var i = 0; i < objectCount; i++)
            {
                var flags = reader.ReadUInt32();
                var x = (reader.ReadInt32() / 4096f) / 2.25f;// 4096f;
                var y = (reader.ReadInt32() / 4096f) / 2.25f;// 4096f;
                var z = (reader.ReadInt32() / 4096f) / 2.25f;// 4096f;
                var unk1 = reader.ReadUInt32();
                var unk2 = reader.ReadUInt16();
                var modelIndex = reader.ReadUInt16();
                var tx = reader.ReadUInt16();
                var ty = reader.ReadUInt16();
                var unk3 = reader.ReadUInt32();
                var palTop = reader.ReadUInt32();
                objectModels[i] = new PSXModel(x, y, z, modelIndex);
            }

            var modelCount = reader.ReadUInt32();
            if (modelCount == 0 || modelCount > Limits.MaxPSXObjectCount)
            {
                return null;
            }

            var modelEntities = new List<ModelEntity>();
            var attachmentIndex = 0;
            for (uint i = 0; i < modelCount; i++)
            {
                var modelTop = reader.ReadUInt32();
                var modelPosition = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + modelTop, SeekOrigin.Begin);
                var flags = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
                var vertexCount = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
                var planeCount = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
                var faceCount = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
                var radius = reader.ReadUInt32();
                var xMax = reader.ReadUInt16();
                var xMin = reader.ReadUInt16();
                var yMax = reader.ReadUInt16();
                var yMin = reader.ReadUInt16();
                var zMax = reader.ReadUInt16();
                var zMin = reader.ReadUInt16();
                if (version == 0x04)
                {
                    var unk2 = reader.ReadUInt32();
                }

                var attachedIndices = new Dictionary<uint, uint>();
                var attachableIndices = new Dictionary<uint, uint>();

                if (_vertices == null || _vertices.Length < vertexCount)
                {
                    Array.Resize(ref _vertices, (int)vertexCount);
                }
                var vertices = _vertices;// new Vector3[vertexCount];
                for (uint j = 0; j < vertexCount; j++)
                {
                    var x = reader.ReadInt16();
                    var y = reader.ReadInt16();
                    var z = reader.ReadInt16();
                    var pad = reader.ReadUInt16();
                    vertices[j] = new Vector3(x / 2.25f, y / 2.25f, z / 2.25f);

                    if (pad == 1)
                    {
                        attachableIndices.Add(j, (uint)attachmentIndex++);
                    }
                    else if (pad == 2)
                    {
                        attachedIndices.Add(j, (uint)y);
                    }
                }

                if (_normals == null || _normals.Length < planeCount)
                {
                    Array.Resize(ref _normals, (int)planeCount);
                }
                var normals = _normals;// new Vector3[planeCount];
                for (uint j = 0; j < planeCount; j++)
                {
                    var x = reader.ReadInt16() / 4096f;
                    var y = reader.ReadInt16() / 4096f;
                    var z = reader.ReadInt16() / 4096f;
                    reader.ReadUInt16(); //pad
                    normals[j] = new Vector3(x, y, z);
                }

                //uint faceFlags;
                //uint faceLength;
                //if (version == 0x03)
                //{
                //    faceFlags = reader.ReadUInt16();
                //    faceLength = reader.ReadUInt16();
                //}
                //else
                //{
                //    faceFlags = 0;
                //    faceLength = 0;
                //}
                for (uint j = 0; j < faceCount; j++)
                {
                    if (version == 0x04)
                    {
                        var facePosition = reader.BaseStream.Position;
                        var faceFlags = reader.ReadUInt16();
                        var faceLength = reader.ReadUInt16();
                        var quad = (faceFlags & 0x0010) == 0;
                        var gouraud = (faceFlags & 0x0800) != 0;
                        var textured = (faceFlags & 0x0003) != 0;
                        var invisible = (faceFlags & 0x0080) != 0;
                        var i0 = reader.ReadByte();
                        var i1 = reader.ReadByte();
                        var i2 = reader.ReadByte();
                        var i3 = reader.ReadByte();
                        if (i0 >= vertexCount || i1 >= vertexCount || i2 >= vertexCount || i3 >= vertexCount)
                        {
                            throw new IndexOutOfRangeException("Vertex index out of bounds");
                        }
                        var vertex0 = vertices[i0];
                        var vertex1 = vertices[i1];
                        var vertex2 = vertices[i2];
                        var vertex3 = vertices[i3];
                        Color color0;
                        Color color1;
                        Color color2;
                        Color color3;
                        var r0 = reader.ReadByte() / 255f;
                        var g0 = reader.ReadByte() / 255f;
                        var b0 = reader.ReadByte() / 255f;
                        var command = reader.ReadByte();
                        var attachedIndex0 = attachedIndices.TryGetValue(i0, out var index0) ? index0 : Triangle.NoAttachment;
                        var attachedIndex1 = attachedIndices.TryGetValue(i1, out var index1) ? index1 : Triangle.NoAttachment;
                        var attachedIndex2 = attachedIndices.TryGetValue(i2, out var index2) ? index2 : Triangle.NoAttachment;
                        var attachedIndex3 = attachedIndices.TryGetValue(i3, out var index3) ? index3 : Triangle.NoAttachment;
                        var attachableIndex0 = attachableIndices.TryGetValue(i0, out var attIndex0) ? attIndex0 : Triangle.NoAttachment;
                        var attachableIndex1 = attachableIndices.TryGetValue(i1, out var attIndex1) ? attIndex1 : Triangle.NoAttachment;
                        var attachableIndex2 = attachableIndices.TryGetValue(i2, out var attIndex2) ? attIndex2 : Triangle.NoAttachment;
                        var attachableIndex3 = attachableIndices.TryGetValue(i3, out var attIndex3) ? attIndex3 : Triangle.NoAttachment;
                        if (gouraud)
                        {
                            color0 = color1 = color2 = color3 = Color.Grey; //todo
                        }
                        else
                        {
                            color0 = color1 = color2 = color3 = new Color(r0, g0, b0);
                        }
                        //todo
                        var planeIndex = reader.ReadUInt16();
                        var surfFlags = reader.ReadInt16();
                        if (planeIndex >= planeCount)
                        {
                            throw new IndexOutOfRangeException("Normal index out of bounds");
                        }
                        var normal0 = normals[planeIndex];
                        var normal1 = normals[planeIndex];
                        var normal2 = normals[planeIndex];
                        var normal3 = normals[planeIndex];
                        var uv0 = Vector2.Zero;
                        var uv1 = Vector2.Zero;
                        var uv2 = Vector2.Zero;
                        var uv3 = Vector2.Zero;
                        uint tPage = 0;
                        var renderFlags = RenderFlags.None;
                        if (textured)
                        {
                            renderFlags |= RenderFlags.Textured;
                            tPage = reader.ReadUInt32(); //todo
                            var u0 = reader.ReadByte();
                            var v0 = reader.ReadByte();
                            var u1 = reader.ReadByte();
                            var v1 = reader.ReadByte();
                            var u2 = reader.ReadByte();
                            var v2 = reader.ReadByte();
                            var u3 = reader.ReadByte();
                            var v3 = reader.ReadByte();
                            uv0 = GeomMath.ConvertUV(u0, v0);
                            uv1 = GeomMath.ConvertUV(u1, v1);
                            uv2 = GeomMath.ConvertUV(u2, v2);
                            uv3 = GeomMath.ConvertUV(u3, v3);
                        }
                        if (!invisible)
                        {
                            AddTriangle(new Triangle
                            {
                                Vertices = new[] { vertex2, vertex1, vertex0 },
                                Normals = new[] { normal2, normal1, normal0 },
                                Uv = new[] { uv2, uv1, uv0 },
                                Colors = new[] { color2, color1, color0 },
                                OriginalVertexIndices = new uint[] { i2, i1, i0 },
                                AttachedIndices = new[] { attachedIndex2, attachedIndex1, attachedIndex0 },
                                AttachableIndices = new[] { attachableIndex2, attachableIndex1, attachableIndex0 }
                            }, i, tPage, renderFlags);
                            if (quad)
                            {
                                AddTriangle(new Triangle
                                {
                                    Vertices = new[] { vertex1, vertex2, vertex3 },
                                    Normals = new[] { normal1, normal2, normal3 },
                                    Uv = new[] { uv1, uv2, uv3 },
                                    Colors = new[] { color1, color2, color3 },
                                    OriginalVertexIndices = new uint[] { i1, i2, i3 },
                                    AttachedIndices = new[] { attachedIndex1, attachedIndex2, attachedIndex3 },
                                    AttachableIndices = new[] { attachableIndex1, attachableIndex2, attachableIndex3 }
                                }, i, tPage, renderFlags);
                            }
                        }
                        reader.BaseStream.Seek(facePosition + faceLength, SeekOrigin.Begin);
                    }
                    else
                    {
                        var facePosition = reader.BaseStream.Position;
                        var faceFlags = reader.ReadUInt16();
                        var faceLength = reader.ReadUInt16();
                        var quad = (faceFlags & 0x0010) == 0;
                        var gouraud = (faceFlags & 0x0800) != 0;
                        var textured = (faceFlags & 0x0003) != 0;
                        var invisible = (faceFlags & 0x0080) != 0;
                        var i0 = reader.ReadUInt16();
                        var i1 = reader.ReadUInt16();
                        var i2 = reader.ReadUInt16();
                        var i3 = reader.ReadUInt16();
                        if (i0 >= vertexCount || i1 >= vertexCount || i2 >= vertexCount || i3 >= vertexCount)
                        {
                            throw new IndexOutOfRangeException("Vertex index out of bounds");
                        }
                        var vertex0 = vertices[i0];
                        var vertex1 = vertices[i1];
                        var vertex2 = vertices[i2];
                        var vertex3 = vertices[i3];
                        Color color0;
                        Color color1;
                        Color color2;
                        Color color3;
                        var r0 = reader.ReadByte() / 255f;
                        var g0 = reader.ReadByte() / 255f;
                        var b0 = reader.ReadByte() / 255f;
                        var command = reader.ReadByte();
                        var attachedIndex0 = attachedIndices.TryGetValue(i0, out var index0) ? index0 : Triangle.NoAttachment;
                        var attachedIndex1 = attachedIndices.TryGetValue(i1, out var index1) ? index1 : Triangle.NoAttachment;
                        var attachedIndex2 = attachedIndices.TryGetValue(i2, out var index2) ? index2 : Triangle.NoAttachment;
                        var attachedIndex3 = attachedIndices.TryGetValue(i3, out var index3) ? index3 : Triangle.NoAttachment;
                        var attachableIndex0 = attachableIndices.TryGetValue(i0, out var attIndex0) ? attIndex0 : Triangle.NoAttachment;
                        var attachableIndex1 = attachableIndices.TryGetValue(i1, out var attIndex1) ? attIndex1 : Triangle.NoAttachment;
                        var attachableIndex2 = attachableIndices.TryGetValue(i2, out var attIndex2) ? attIndex2 : Triangle.NoAttachment;
                        var attachableIndex3 = attachableIndices.TryGetValue(i3, out var attIndex3) ? attIndex3 : Triangle.NoAttachment;
                        if (gouraud)
                        {
                            color0 = color1 = color2 = color3 = Color.Grey; //todo
                        }
                        else
                        {
                            color0 = color1 = color2 = color3 = new Color(r0, g0, b0);
                        }
                        //todo
                        var planeIndex = reader.ReadUInt16();
                        var surfFlags = reader.ReadInt16();
                        if (planeIndex >= planeCount)
                        {
                            throw new IndexOutOfRangeException("Normal index out of bounds");
                        }
                        var normal0 = normals[planeIndex];
                        var normal1 = normals[planeIndex];
                        var normal2 = normals[planeIndex];
                        var normal3 = normals[planeIndex];
                        var uv0 = Vector2.Zero;
                        var uv1 = Vector2.Zero;
                        var uv2 = Vector2.Zero;
                        var uv3 = Vector2.Zero;
                        uint tPage = 0;
                        var renderFlags = RenderFlags.None;
                        if (textured)
                        {
                            renderFlags |= RenderFlags.Textured;
                            tPage = reader.ReadUInt32(); //todo
                            var u0 = reader.ReadByte();
                            var v0 = reader.ReadByte();
                            var u1 = reader.ReadByte();
                            var v1 = reader.ReadByte();
                            var u2 = reader.ReadByte();
                            var v2 = reader.ReadByte();
                            var u3 = reader.ReadByte();
                            var v3 = reader.ReadByte();
                            uv0 = GeomMath.ConvertUV(u0, v0);
                            uv1 = GeomMath.ConvertUV(u1, v1);
                            uv2 = GeomMath.ConvertUV(u2, v2);
                            uv3 = GeomMath.ConvertUV(u3, v3);
                        }
                        if (!invisible)
                        {
                            AddTriangle(new Triangle
                            {
                                Vertices = new[] { vertex2, vertex1, vertex0 },
                                Normals = new[] { normal2, normal1, normal0 },
                                Uv = new[] { uv2, uv1, uv0 },
                                Colors = new[] { color2, color1, color0 },
                                OriginalVertexIndices = new uint[] { i2, i1, i0 },
                                AttachedIndices = new[] { attachedIndex2, attachedIndex1, attachedIndex0 },
                                AttachableIndices = new[] { attachableIndex2, attachableIndex1, attachableIndex0 }
                            }, i, tPage, renderFlags);
                            if (quad)
                            {
                                AddTriangle(new Triangle
                                {
                                    Vertices = new[] { vertex1, vertex2, vertex3 },
                                    Normals = new[] { normal1, normal2, normal3 },
                                    Uv = new[] { uv1, uv2, uv3 },
                                    Colors = new[] { color1, color2, color3 },
                                    OriginalVertexIndices = new uint[] { i1, i2, i3 },
                                    AttachedIndices = new[] { attachedIndex1, attachedIndex2, attachedIndex3 },
                                    AttachableIndices = new[] { attachableIndex1, attachableIndex2, attachableIndex3 }
                                }, i, tPage, renderFlags);
                            }
                        }
                        reader.BaseStream.Seek(facePosition + faceLength, SeekOrigin.Begin);
                    }
                }
                reader.BaseStream.Seek(modelPosition, SeekOrigin.Begin);
            }

            //var position = reader.BaseStream.Position;
            //reader.BaseStream.Seek(metaPtr, SeekOrigin.Begin);
            //for (; ; )
            //{
            //    var chunkId = reader.ReadBytes(4);
            //    if (chunkId[0] == 0xFF && chunkId[1] == 0xFF && chunkId[2] == 0xFF && chunkId[3] == 0xFF)
            //    {
            //        break;
            //    }
            //    var chunkString = Encoding.ASCII.GetString(chunkId);
            //    var chunkLength = reader.ReadUInt32();
            //    var chunkData = reader.ReadBytes((int)chunkLength);
            //};
            //reader.BaseStream.Seek(position, SeekOrigin.Begin);

            for (var i = 0; i < objectCount; i++)
            {
                var psxModel = objectModels[i];
                foreach (var kvp in groupedTriangles)
                {
                    if (kvp.Key.Item1 == psxModel.ModelIndex)
                    {
                        var triangles = kvp.Value;
                        var renderInfo = kvp.Key.Item2;
                        var model = new ModelEntity
                        {
                            Triangles = triangles.ToArray(),
                            TexturePage = renderInfo.TexturePage,
                            RenderFlags = renderInfo.RenderFlags,
                            MixtureRate = renderInfo.MixtureRate,
                            TMDID = psxModel.ModelIndex, //todo
                            OriginalLocalMatrix = Matrix4.CreateTranslation(psxModel.X, psxModel.Y, psxModel.Z)
                        };
                        model.ComputeAttached();
                        modelEntities.Add(model);
                    }
                }
            }
            RootEntity rootEntity;
            if (modelEntities.Count > 0)
            {
                rootEntity = new RootEntity();
                rootEntity.ChildEntities = modelEntities.ToArray();
                rootEntity.ComputeBounds();
            }
            else
            {
                rootEntity = null;
            }
            return rootEntity;
        }

        private struct PSXModel
        {
            public float X { get; }
            public float Y { get; }
            public float Z { get; }
            public ushort ModelIndex { get; }

            public PSXModel(float x, float y, float z, ushort modelIndex)
            {
                X = x;
                Y = y;
                Z = z;
                ModelIndex = modelIndex;
            }
        }
    }
}
