using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Parsers
{
    public class BFFParser : FileOffsetScanner
    {
        private readonly uint[] _headerValues = new uint[13];
        private readonly uint[] _sections = new uint[6 * 2]; // count/top pairs
        private Vector3[] _vertices;
        private uint _vertexCount;
        private uint[] _textureMapCRCs;
        private uint _textureMapCount;
        private readonly Dictionary<RenderInfo, List<Triangle>> _groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();
        private readonly Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>> _groupedSprites = new Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>>();
        private readonly List<ModelEntity> _models = new List<ModelEntity>();

        public BFFParser(EntityAddedAction entityAdded)
            : base(entityAdded: entityAdded)
        {
        }

        public override string FormatName => "BFF";

        protected override void Parse(BinaryReader reader)
        {
            _groupedTriangles.Clear();
            _groupedSprites.Clear();
            _models.Clear();

            var rootEntity = ReadModels(reader);
            if (rootEntity != null)
            {
                EntityResults.Add(rootEntity);
            }
        }

        private RootEntity ReadModels(BinaryReader reader)
        {
            //var id0 = (char)reader.ReadByte();
            //var id1 = (char)reader.ReadByte();
            //var id2 = (char)reader.ReadByte();
            //var id3 = reader.ReadByte();
            //if (id0 != 'P' || id1 != 'S' || id2 != 'I' || id3 != 1)
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
            //var numTex = reader.ReadUInt32();R
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

            const int BFF_FMA_MESH_ZERO_ID   = 0; // Frogger 2
            const int BFF_FMA_MESH_SHORT_ID  = 5; // Chicken Run
            const int BFF_FMA_MESH4_SHORT_ID = 6;
            const int BFF_FMA_MESH_LONG_ID   = 1; // Long form IDs noted to be used for PC versions of files
            const int BFF_FMA_MESH4_LONG_ID  = 4;

            var id0 = (char)reader.ReadByte();
            var id1 = (char)reader.ReadByte();
            var id2 = (char)reader.ReadByte();
            var id3 = reader.ReadByte();
            if (id0 != 'F' || id1 != 'M' || id2 != 'M')
            {
                return null;
            }

            var zeroForm = false; // slightly different structure, and needs guesswork to determine format
            var shortForm = true; // indices are read as a uint16 instead of a uint32
            var polyGT3Form = false; // GT3s and GT4s are read using the same POLY_GT3 structure as PMD
            if (id3 == BFF_FMA_MESH_ZERO_ID)
            {
                zeroForm = true;
                //shortForm = ; // determined later by header size
                polyGT3Form = false;
            }
            else if (id3 == BFF_FMA_MESH_SHORT_ID || id3 == BFF_FMA_MESH4_SHORT_ID)
            {
                zeroForm = false;
                shortForm = true;
                polyGT3Form = id3 == BFF_FMA_MESH4_SHORT_ID;
            }
            else if (id3 == BFF_FMA_MESH_LONG_ID || id3 == BFF_FMA_MESH4_LONG_ID)
            {
                zeroForm = false;
                shortForm = false;
                polyGT3Form = id3 == BFF_FMA_MESH4_LONG_ID;
            }
            else
            {
                return null;
            }

            // // Version 1 mesh headers include flat-poly information. Like what skies used to.
            //#define BFF_FMA_MESH_ID (('F'<<0) | ('M'<<8) | ('M'<<16) | (1<<24))
            // //#define BFF_FMA_SKYMESH_ID (('F'<<0) | ('M'<<8) | ('S'<<16) | (0<<24))
            //#define BFF_FMA_MESH4_ID (('F'<<0) | ('M'<<8) | ('M'<<16) | (4<<24))

            var length = reader.ReadUInt32(); // Length from start of file
            if (_offset + length > reader.BaseStream.Length)
            {
                return null;
            }
            var nameCRC = reader.ReadUInt32();

            var headerSize = 0; // Size including stop value in zeroForm
            if (zeroForm)
            {
                // Header uses following format: (may exclude unk0 and unk1)
                // uint32 flags
                // int32 shift
                // int32 minX
                // int32 minY
                // int32 minZ
                // int32 maxX
                // int32 maxY
                // int32 maxZ
                // int32 posX = 0
                // int32 posY = 0
                // int32 posZ = 0
                // int16 rotX = 0
                // int16 rotY = 0
                // int16 rotZ = 0
                // int16 rotW = 0x1000 (stop = 0x10000000)
                for (var i = 0; i < 13; i++)
                {
                    var headerValue = reader.ReadUInt32();
                    _headerValues[i] = headerValue;
                    // todo: This relies entirely on rotW being 1f, and will break otherwise.
                    // See if there's another way to determine why there are different-sized headers.
                    if (headerValue == 0x10000000)
                    {
                        headerSize = i + 1;
                        break;
                    }
                }

                if (headerSize != 11 && headerSize != 13)
                {
                    return null; // Unexpected header size
                }
            }
            else
            {
                headerSize = 11;

                for (var i = 0; i < 11; i++)
                {
                    _headerValues[i] = reader.ReadUInt32();
                }

                /*//var dummy1 = reader.ReadUInt16();
                //var dummy2 = reader.ReadUInt16();
                var polyListTop = reader.ReadUInt32();
                var dummy1 = reader.ReadUInt16();
                var dummy2 = reader.ReadUInt16();*/
            }
            var headerExtra = headerSize - 11;

            // Bounding box
            var minX = (int)_headerValues[headerExtra + 0];
            var minY = (int)_headerValues[headerExtra + 1];
            var minZ = (int)_headerValues[headerExtra + 2];
            var maxX = (int)_headerValues[headerExtra + 3];
            var maxY = (int)_headerValues[headerExtra + 4];
            var maxZ = (int)_headerValues[headerExtra + 5];

            // We only know these values are accurate for Frogger 2.
            var posX = (int)_headerValues[headerExtra + 6];
            var posY = (int)_headerValues[headerExtra + 7];
            var posZ = (int)_headerValues[headerExtra + 8];
            var rotX = ((short)(_headerValues[headerExtra +  9] >>  0)) / 4096f;
            var rotY = ((short)(_headerValues[headerExtra +  9] >> 16)) / 4096f;
            var rotZ = ((short)(_headerValues[headerExtra + 10] >>  0)) / 4096f;
            var rotW = ((short)(_headerValues[headerExtra + 10] >> 16)) / 4096f;

            if (rotX == 0f && rotY == 0f && rotZ == 0f && rotW == 0f)
            {
                rotW = 1f; // Change to identity
            }

            var radius = reader.ReadUInt32(); // extraDepth used by engine for sorting

            var sectionIndex = 0;
            var sectionCount = 0;
            for (var i = 0; i < 6; i++)
            {
                var count = reader.ReadUInt32();
                var top = reader.ReadUInt32();
                _sections[(i * 2) + 0] = count;
                _sections[(i * 2) + 1] = top;
                if (_offset + top == reader.BaseStream.Position)
                {
                    sectionCount = i + 1;
                    break;
                }
            }

            int textureMapSectionIndex;
            if (zeroForm)
            {
                if (sectionCount == 4)
                {
                    shortForm = false;
                }
                else if (sectionCount == 5)
                {
                    shortForm = true;
                }
                else
                {
                    return null; // Unexpected number of sections
                }
                textureMapSectionIndex = sectionCount - 1;
            }
            else
            {
                textureMapSectionIndex = 3;
            }


            _vertexCount = _sections[(sectionIndex * 2) + 0];
            var vertsTop = _sections[(sectionIndex * 2) + 1];
            sectionIndex++;
            if (_vertexCount == 0 || _vertexCount > Limits.MaxBFFVertices)
            {
                return null;
            }

            reader.BaseStream.Seek(_offset + vertsTop, SeekOrigin.Begin);
            if (_vertices == null || _vertices.Length < _vertexCount)
            {
                Array.Resize(ref _vertices, (int)_vertexCount);
            }
            var vertices = _vertices;// new Vector3[_vertexCount];
            for (var i = 0; i < _vertexCount; i++)
            {
                var x = reader.ReadInt16();
                var y = reader.ReadInt16();
                var z = reader.ReadInt16();
                var pad = reader.ReadUInt16(); //pad
                vertices[i] = new Vector3(x, y, z);
            }


            _textureMapCount  = _sections[(textureMapSectionIndex * 2) + 0];
            var textureMapTop = _sections[(textureMapSectionIndex * 2) + 1];
            if (_textureMapCount > Limits.MaxBFFTextureMaps)
            {
                return null;
            }

            reader.BaseStream.Seek(_offset + textureMapTop, SeekOrigin.Begin);
            if (_textureMapCRCs == null || _textureMapCRCs.Length < _textureMapCount)
            {
                Array.Resize(ref _textureMapCRCs, (int)_textureMapCount);
            }
            var textureMapCRCs = _textureMapCRCs;// new uint[_textureMapCount];
            for (var i = 0; i < _textureMapCount; i++)
            {
                textureMapCRCs[i] = reader.ReadUInt32();
            }


            var numGT3s = _sections[(sectionIndex * 2) + 0];
            var gt3Top  = _sections[(sectionIndex * 2) + 1];
            sectionIndex++;
            if (numGT3s > Limits.MaxBFFPackets)
            {
                return null;
            }

            reader.BaseStream.Seek(_offset + gt3Top, SeekOrigin.Begin);
            if (!polyGT3Form)
            {
                for (var i = 0; i < numGT3s; i++) //FMA_GT3
                {
                    ReadFMAPacket(reader, false, true, shortForm);
                }
            }
            else
            {
                for (var i = 0; i < numGT3s; i++)
                {
                    var vertexIndex0 = ReadIndex(reader, shortForm);
                    var vertexIndex1 = ReadIndex(reader, shortForm);
                    var vertexIndex2 = ReadIndex(reader, shortForm);
                    ReadIndexPad(reader, shortForm); //pad

                    ReadPolyGT3(reader, vertexIndex0, vertexIndex1, vertexIndex2);
                }
            }


            var numGT4s = _sections[(sectionIndex * 2) + 0];
            var gt4Top  = _sections[(sectionIndex * 2) + 1];
            sectionIndex++;
            if (numGT4s > Limits.MaxBFFPackets)
            {
                return null;
            }

            reader.BaseStream.Seek(_offset + gt4Top, SeekOrigin.Begin);
            if (!polyGT3Form)
            {
                for (var i = 0; i < numGT4s; i++) //FMA_GT4
                {
                    ReadFMAPacket(reader, true, true, shortForm);
                }
            }
            else
            {
                for (var i = 0; i < numGT4s; i++)
                {
                    var vertexIndex0 = ReadIndex(reader, shortForm);
                    var vertexIndex1 = ReadIndex(reader, shortForm);
                    var vertexIndex2 = ReadIndex(reader, shortForm);
                    var vertexIndex3 = ReadIndex(reader, shortForm);

                    //two POLY_GT3
                    ReadPolyGT3(reader, vertexIndex0, vertexIndex1, vertexIndex2);
                    ReadPolyGT3(reader, vertexIndex1, vertexIndex3, vertexIndex2);
                }
            }


            if (zeroForm)
            {
                if (shortForm)
                {
                    var numSprs = _sections[(sectionIndex * 2) + 0];
                    var sprTop  = _sections[(sectionIndex * 2) + 1];
                    sectionIndex++;
                    if (numSprs > Limits.MaxBFFPackets)
                    {
                        return null;
                    }

                    reader.BaseStream.Seek(_offset + sprTop, SeekOrigin.Begin);
                    for (var i = 0; i < numSprs; i++) //FMA_SPR
                    {
                        ReadFMASPR(reader);
                    }
                }


                // Skip texture maps, we already read those.
                sectionIndex++;
            }
            else
            {
                // Skip texture maps, we already read those.
                sectionIndex++;


                var numG3s = _sections[(sectionIndex * 2) + 0];
                var g3Top  = _sections[(sectionIndex * 2) + 1];
                sectionIndex++;
                if (numG3s > Limits.MaxBFFPackets)
                {
                    return null;
                }

                reader.BaseStream.Seek(_offset + g3Top, SeekOrigin.Begin);
                for (var i = 0; i < numG3s; i++) //FMA_G3
                {
                    ReadFMAPacket(reader, false, false, shortForm);
                }


                var numG4s = _sections[(sectionIndex * 2) + 0];
                var g4Top  = _sections[(sectionIndex * 2) + 1];
                sectionIndex++;
                if (numG4s > Limits.MaxBFFPackets)
                {
                    return null;
                }

                reader.BaseStream.Seek(_offset + g4Top, SeekOrigin.Begin);
                for (var i = 0; i < numG4s; i++) //FMA_G4
                {
                    ReadFMAPacket(reader, true, false, shortForm);
                }
            }

            var localMatrix = Matrix4.Identity;
            // todo: Not sure if this is the same for non-zeroForms
            //if (zeroForm)
            {
                localMatrix = Matrix4.CreateFromQuaternion(new Quaternion(rotX, rotY, rotZ, rotW)) *
                              Matrix4.CreateTranslation(posX, posY, posZ);
            }

            foreach (var kvp in _groupedTriangles)
            {
                var renderInfo = kvp.Key;
                var triangles = kvp.Value;
                var model = new ModelEntity
                {
                    Triangles = triangles.ToArray(),
                    TexturePage = 0,
                    TextureLookup = renderInfo.RenderFlags.HasFlag(RenderFlags.Textured)
                        ? new TextureLookup
                        {
                            ID = renderInfo.TexturePage, // CRC-32 of name
                            ExpectedFormat = SPTParser.FormatNameConst,
                        }
                        : null,
                    RenderFlags = renderInfo.RenderFlags,
                    MixtureRate = renderInfo.MixtureRate,
                    TMDID = 0, // Only one model per BFF
                    OriginalLocalMatrix = localMatrix,
                };
                _models.Add(model);
            }
            foreach (var kvp in _groupedSprites)
            {
                var spriteCenter = kvp.Key.Item1;
                var renderInfo = kvp.Key.Item2;
                var triangles = kvp.Value;
                var model = new ModelEntity
                {
                    Triangles = triangles.ToArray(),
                    TexturePage = 0,
                    TextureLookup = renderInfo.RenderFlags.HasFlag(RenderFlags.Textured)
                        ? new TextureLookup
                        {
                            ID = renderInfo.TexturePage, // CRC-32 of name
                            ExpectedFormat = SPTParser.FormatNameConst,
                        }
                        : null,
                    RenderFlags = renderInfo.RenderFlags,
                    MixtureRate = renderInfo.MixtureRate,
                    SpriteCenter = spriteCenter,
                    TMDID = 0,
                    OriginalLocalMatrix = localMatrix,
                };
                _models.Add(model);
            }

            if (_models.Count > 0)
            {
                var entity = new RootEntity();
                entity.ChildEntities = _models.ToArray();
                entity.ComputeBounds();
                return entity;
            }

            return null;
        }

        private static uint ReadIndex(BinaryReader reader, bool shortForm)
        {
            var value = shortForm ? reader.ReadUInt16() : reader.ReadUInt32();
            if (value % 4 != 0)
            {
                throw new Exception("Unexpected vertex index");
            }
            return value / 4u;
        }

        private static uint ReadIndexPad(BinaryReader reader, bool shortForm)
        {
            var value = shortForm ? reader.ReadUInt16() : 0u;
            return value;
        }

        private void ReadFMAPacket(BinaryReader reader, bool quad, bool textured, bool shortForm)
        {
            var renderFlags = RenderFlags.None;
            if (textured)
            {
                renderFlags |= RenderFlags.Textured;
            }

            byte u0 = 0, v0 = 0, u1 = 0, v1 = 0, u2 = 0, v2 = 0, u3 = 0, v3 = 0;
            uint tPage = 0;//, clutX = 0, clutY = 0;

            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var mode = reader.ReadByte();
            if (textured)
            {
                u0 = reader.ReadByte();
                v0 = reader.ReadByte();
                var cba = reader.ReadUInt16();
                //TMDHelper.ParseCBA(cba, out clutX, out clutY);
            }

            var r1 = reader.ReadByte();
            var g1 = reader.ReadByte();
            var b1 = reader.ReadByte();
            var pad1 = reader.ReadByte(); //pad
            if (textured)
            {
                u1 = reader.ReadByte();
                v1 = reader.ReadByte();
                var tMapIndex = reader.ReadUInt16();
                if (tMapIndex < _textureMapCount)
                {
                    tPage = _textureMapCRCs[tMapIndex];
                }
            }

            var r2 = reader.ReadByte();
            var g2 = reader.ReadByte();
            var b2 = reader.ReadByte();
            var pad2 = reader.ReadByte(); //pad
            if (textured)
            {
                u2 = reader.ReadByte();
                v2 = reader.ReadByte();
                var pad3 = reader.ReadUInt16(); //pad
            }

            byte r3 = 0, g3 = 0, b3 = 0;
            if (quad)
            {
                r3 = reader.ReadByte();
                g3 = reader.ReadByte();
                b3 = reader.ReadByte();
                var pad4 = reader.ReadByte(); //pad
                if (textured)
                {
                    u3 = reader.ReadByte();
                    v3 = reader.ReadByte();
                    var pad5 = reader.ReadUInt16(); //pad
                }
            }

            var vertexIndex0 = ReadIndex(reader, shortForm);
            var vertexIndex1 = ReadIndex(reader, shortForm);
            var vertexIndex2 = ReadIndex(reader, shortForm);
            var vertexIndex3 = quad ? ReadIndex(reader, shortForm) : ReadIndexPad(reader, shortForm);

            var triangle1 = TriangleFromPrimitive(vertexIndex0, vertexIndex1, vertexIndex2,
                                                  r0, g0, b0, r1, g1, b1, r2, g2, b2,
                                                  u0, v0, u1, v1, u2, v2);

            AddTriangle(triangle1, tPage, renderFlags);

            if (quad)
            {
                var triangle2 = TriangleFromPrimitive(vertexIndex1, vertexIndex3, vertexIndex2,
                                                      r1, g1, b1, r3, g3, b3, r2, g2, b2,
                                                      u1, v1, u3, v3, u2, v2);

                AddTriangle(triangle2, tPage, renderFlags);
            }
        }

        private void ReadFMASPR(BinaryReader reader)
        {
            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var mode = reader.ReadByte();

            var x = reader.ReadInt16();
            var y = reader.ReadInt16();
            var z = reader.ReadInt16();
            var tMapIndex = reader.ReadUInt16();
            uint tPage = 0;
            if (tMapIndex < _textureMapCount)
            {
                tPage = _textureMapCRCs[tMapIndex];
            }

            var u0 = reader.ReadByte();
            var v0 = reader.ReadByte();
            var u1 = reader.ReadByte();
            var v1 = reader.ReadByte();
            var u2 = reader.ReadByte();
            var v2 = reader.ReadByte();
            var u3 = reader.ReadByte();
            var v3 = reader.ReadByte();

            var cba = reader.ReadUInt16();
            //TMDHelper.ParseCBA(cba, out var clutX, out var clutY);

            // Not sure on the exact value for this, but 2.0-2.5 seems to look correct with Frogger 2.
            var scale = Settings.Instance.DebugBFFSpriteScale;// 2.5f;
            var width  = reader.ReadByte() * scale;
            var height = reader.ReadByte() * scale;

            var uv0 = GeomMath.ConvertUV(u0, v0);
            var uv1 = GeomMath.ConvertUV(u1, v1);
            var uv2 = GeomMath.ConvertUV(u2, v2);
            var uv3 = GeomMath.ConvertUV(u3, v3);

            var color = new Color(r0 / 255f, g0 / 255f, b0 / 255f);
            // Coordinates are inverted for some reason.
            var center = new Vector3(-x, -y, -z);

            // Remember that Y-up is negative, so height values are negated compared to what we set for UVs.
            // Note that these vertex coordinates also assume the default orientation of the view is (0, 0, -1).
            var vertex0 = center + new Vector3(-width,  height, 0f);
            var vertex1 = center + new Vector3( width,  height, 0f);
            var vertex2 = center + new Vector3(-width, -height, 0f);
            var vertex3 = center + new Vector3( width, -height, 0f);

            var renderFlags = RenderFlags.Textured | RenderFlags.Sprite;

            AddSpriteTriangle(new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color, color, color },
                Uv = new[] { uv0, uv1, uv2 },
                AttachableIndices = Triangle.EmptyAttachableIndices,
            }, center, tPage, renderFlags);

            AddSpriteTriangle(new Triangle
            {
                Vertices = new[] { vertex1, vertex3, vertex2 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color, color, color },
                Uv = new[] { uv1, uv3, uv2 },
                AttachableIndices = Triangle.EmptyAttachableIndices,
            }, center, tPage, renderFlags);
        }

        private void ReadPolyGT3(BinaryReader reader, uint vertexIndex0, uint vertexIndex1, uint vertexIndex2)
        {
            // Same structure as seen in PMD format, but not repeated twice
            var tag = reader.ReadInt32();

            var r0 = reader.ReadByte();
            var g0 = reader.ReadByte();
            var b0 = reader.ReadByte();
            var mode = reader.ReadByte();

            var x0 = reader.ReadInt16();
            var y0 = reader.ReadInt16();
            var u0 = reader.ReadByte();
            var v0 = reader.ReadByte();
            var cba = reader.ReadUInt16();
            //TMDHelper.ParseCBA(cba, out var clutX, out var clutY);

            var r1 = reader.ReadByte();
            var g1 = reader.ReadByte();
            var b1 = reader.ReadByte();
            var pad1 = reader.ReadByte(); //pad

            var x1 = reader.ReadInt16();
            var y1 = reader.ReadInt16();
            var u1 = reader.ReadByte();
            var v1 = reader.ReadByte();
            var tMapIndex = reader.ReadUInt16();
            uint tPage = 0;
            if (tMapIndex < _textureMapCount)
            {
                tPage = _textureMapCRCs[tMapIndex];
            }

            var r2 = reader.ReadByte();
            var g2 = reader.ReadByte();
            var b2 = reader.ReadByte();
            var pad2 = reader.ReadByte(); //pad

            var x2 = reader.ReadInt16();
            var y2 = reader.ReadInt16();
            var u2 = reader.ReadByte();
            var v2 = reader.ReadByte();
            var pad3 = reader.ReadUInt16(); //pad

            var triangle = TriangleFromPrimitive(vertexIndex0, vertexIndex1, vertexIndex2,
                                                 r0, g0, b0, r1, g1, b1, r2, g2, b2,
                                                 u0, v0, u1, v1, u2, v2);

            AddTriangle(triangle, tPage, RenderFlags.Textured);
        }

        private Triangle TriangleFromPrimitive(
            uint vertexIndex0, uint vertexIndex1, uint vertexIndex2,
            byte r0, byte g0, byte b0,
            byte r1, byte g1, byte b1,
            byte r2, byte g2, byte b2,
            byte u0, byte v0,
            byte u1, byte v1,
            byte u2, byte v2
            )
        {
            if (vertexIndex0 >= _vertexCount || vertexIndex1 >= _vertexCount || vertexIndex2 >= _vertexCount)
            {
                throw new Exception("Out of indices");
            }

            var vertex0 = _vertices[vertexIndex0];
            var vertex1 = _vertices[vertexIndex1];
            var vertex2 = _vertices[vertexIndex2];

            var color0 = new Color(r0 / 255f, g0 / 255f, b0 / 255f);
            var color1 = new Color(r1 / 255f, g1 / 255f, b1 / 255f);
            var color2 = new Color(r2 / 255f, g2 / 255f, b2 / 255f);

            var uv0 = GeomMath.ConvertUV(u0, v0);
            var uv1 = GeomMath.ConvertUV(u1, v1);
            var uv2 = GeomMath.ConvertUV(u2, v2);

            var triangle = new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color0, color1, color2 },
                Uv = new[] { uv0, uv1, uv2 },
                AttachableIndices = Triangle.EmptyAttachableIndices,
            };

            return triangle;
        }

        private void AddTriangle(Triangle triangle, uint tPage, RenderFlags renderFlags)
        {
            renderFlags |= RenderFlags.Unlit | RenderFlags.DoubleSided;
            if (renderFlags.HasFlag(RenderFlags.Textured))
            {
                triangle.CorrectUVTearing();
            }
            var renderInfo = new RenderInfo(tPage, renderFlags);
            if (!_groupedTriangles.TryGetValue(renderInfo, out var triangles))
            {
                triangles = new List<Triangle>();
                _groupedTriangles.Add(renderInfo, triangles);
            }
            triangles.Add(triangle);
        }

        private void AddSpriteTriangle(Triangle triangle, Vector3 center, uint tPage, RenderFlags renderFlags)
        {
            renderFlags |= RenderFlags.Unlit;
            if (renderFlags.HasFlag(RenderFlags.Textured))
            {
                triangle.CorrectUVTearing();
            }
            var renderInfo = new RenderInfo(tPage, renderFlags);
            var tuple = new Tuple<Vector3, RenderInfo>(center, renderInfo);
            if (!_groupedSprites.TryGetValue(tuple, out var triangles))
            {
                triangles = new List<Triangle>();
                _groupedSprites.Add(tuple, triangles);
            }
            triangles.Add(triangle);
        }
    }
}
