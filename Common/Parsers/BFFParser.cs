using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Parsers
{
    // Blitz Games: .BFF model library format
    // This format is a series of ID/Length/Hash/Data sections, but we currently only scan by individual sections.
    public class BFFParser : FileOffsetScanner
    {
        // UVs are in texture space, and are stored in units of 2, so that 128
        // can be used to reach the last row/column of the texture.
        private const float UVConst = 2f;

        private float _scaleDivisor;
        private readonly uint[] _headerValues = new uint[13];
        private readonly uint[] _sections = new uint[6 * 2]; // count/top pairs
        private Vector3[] _vertices;
        private uint _vertexCount;
        private uint[] _textureHashes;
        private uint _textureHashCount;
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
            _scaleDivisor = Settings.Instance.AdvancedBFFScaleDivisor;
            _groupedTriangles.Clear();
            _groupedSprites.Clear();
            _models.Clear();

            ReadBFF(reader);
        }

        private bool ReadBFF(BinaryReader reader)
        {
            //var id0 = (char)reader.ReadByte();
            //var id1 = (char)reader.ReadByte();
            //var id2 = (char)reader.ReadByte();
            //var id3 = reader.ReadByte();
            //if (id0 != 'P' || id1 != 'S' || id2 != 'I' || id3 != 1)
            //{
            //    return false;
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
                return false;
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
                return false;
            }

            // // Version 1 mesh headers include flat-poly information. Like what skies used to.
            //#define BFF_FMA_MESH_ID (('F'<<0) | ('M'<<8) | ('M'<<16) | (1<<24))
            // //#define BFF_FMA_SKYMESH_ID (('F'<<0) | ('M'<<8) | ('S'<<16) | (0<<24))
            //#define BFF_FMA_MESH4_ID (('F'<<0) | ('M'<<8) | ('M'<<16) | (4<<24))

            var length = reader.ReadUInt32(); // Length from start of file
            if (_offset + length > reader.BaseStream.Length)
            {
                return false;
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
                    }
                }

                if (headerSize != 11 && headerSize != 13)
                {
                    return false; // Unexpected header size
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
            var minX = (int)_headerValues[headerExtra + 0] / _scaleDivisor;
            var minY = (int)_headerValues[headerExtra + 1] / _scaleDivisor;
            var minZ = (int)_headerValues[headerExtra + 2] / _scaleDivisor;
            var maxX = (int)_headerValues[headerExtra + 3] / _scaleDivisor;
            var maxY = (int)_headerValues[headerExtra + 4] / _scaleDivisor;
            var maxZ = (int)_headerValues[headerExtra + 5] / _scaleDivisor;

            // We only know these values are accurate for Frogger 2.
            var posX = (int)_headerValues[headerExtra + 6] / _scaleDivisor;
            var posY = (int)_headerValues[headerExtra + 7] / _scaleDivisor;
            var posZ = (int)_headerValues[headerExtra + 8] / _scaleDivisor;
            float rotX, rotY, rotZ, rotW;
            if (zeroForm)
            {
                rotX = ((short)(_headerValues[headerExtra +  9]      )) / 4096f;
                rotY = ((short)(_headerValues[headerExtra +  9] >> 16)) / 4096f;
                rotZ = ((short)(_headerValues[headerExtra + 10]      )) / 4096f;
                rotW = ((short)(_headerValues[headerExtra + 10] >> 16)) / 4096f;

                if (rotX == 0f && rotY == 0f && rotZ == 0f && rotW == 0f)
                {
                    rotW = 1f; // Change to identity
                }
            }
            else
            {
                rotX = rotY = rotZ = 0f;
                rotW = 1f;

                // Unused
                //var polyListPtr = _headerValues[headerExtra + 9];
                //var dummy3 = ((short)(_headerValues[headerExtra + 10]      ));
                //var dummy4 = ((short)(_headerValues[headerExtra + 10] >> 16));
            }

            // extraDepth used by engine for sorting in Frogger 2, radius in Chicken Run
            var radius = reader.ReadUInt32();

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
                }
            }

            int textureHashSectionIndex;
            if (zeroForm)
            {
                // This makes an assumption that texture maps always appear after the header.
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
                    return false; // Unexpected number of sections
                }
                textureHashSectionIndex = sectionCount - 1;
            }
            else
            {
                textureHashSectionIndex = 3;
            }


            _vertexCount = _sections[(sectionIndex * 2) + 0];
            var vertsTop = _sections[(sectionIndex * 2) + 1];
            sectionIndex++;
            if (_vertexCount == 0 || _vertexCount > Limits.MaxBFFVertices)
            {
                return false;
            }

            reader.BaseStream.Seek(_offset + vertsTop, SeekOrigin.Begin);
            if (_vertices == null || _vertices.Length < _vertexCount)
            {
                Array.Resize(ref _vertices, (int)_vertexCount);
            }
            for (var i = 0; i < _vertexCount; i++)
            {
                var x = reader.ReadInt16() / _scaleDivisor;
                var y = reader.ReadInt16() / _scaleDivisor;
                var z = reader.ReadInt16() / _scaleDivisor;
                var pad = reader.ReadUInt16(); //pad
                _vertices[i] = new Vector3(x, y, z);
            }


            _textureHashCount    = _sections[(textureHashSectionIndex * 2) + 0];
            var textureHashesTop = _sections[(textureHashSectionIndex * 2) + 1];
            if (_textureHashCount > Limits.MaxBFFTextureHashes)
            {
                return false;
            }

            reader.BaseStream.Seek(_offset + textureHashesTop, SeekOrigin.Begin);
            if (_textureHashes == null || _textureHashes.Length < _textureHashCount)
            {
                Array.Resize(ref _textureHashes, (int)_textureHashCount);
            }
            for (var i = 0; i < _textureHashCount; i++)
            {
                _textureHashes[i] = reader.ReadUInt32();
            }


            var gt3Count = _sections[(sectionIndex * 2) + 0];
            var gt3sTop  = _sections[(sectionIndex * 2) + 1];
            sectionIndex++;
            if (gt3Count > Limits.MaxBFFPackets)
            {
                return false;
            }

            reader.BaseStream.Seek(_offset + gt3sTop, SeekOrigin.Begin);
            if (!polyGT3Form)
            {
                for (var i = 0; i < gt3Count; i++) //FMA_GT3
                {
                    ReadFMAPacket(reader, false, true, shortForm);
                }
            }
            else
            {
                for (var i = 0; i < gt3Count; i++)
                {
                    var vertexIndex0 = ReadIndex(reader, shortForm);
                    var vertexIndex1 = ReadIndex(reader, shortForm);
                    var vertexIndex2 = ReadIndex(reader, shortForm);
                    ReadIndexPad(reader, shortForm); //pad

                    ReadPolyGT3(reader, vertexIndex0, vertexIndex1, vertexIndex2);
                }
            }


            var gt4Count = _sections[(sectionIndex * 2) + 0];
            var gt4sTop  = _sections[(sectionIndex * 2) + 1];
            sectionIndex++;
            if (gt4Count > Limits.MaxBFFPackets)
            {
                return false;
            }

            reader.BaseStream.Seek(_offset + gt4sTop, SeekOrigin.Begin);
            if (!polyGT3Form)
            {
                for (var i = 0; i < gt4Count; i++) //FMA_GT4
                {
                    ReadFMAPacket(reader, true, true, shortForm);
                }
            }
            else
            {
                for (var i = 0; i < gt4Count; i++)
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
                    var sprCount = _sections[(sectionIndex * 2) + 0];
                    var sprsTop  = _sections[(sectionIndex * 2) + 1];
                    sectionIndex++;
                    if (sprCount > Limits.MaxBFFPackets)
                    {
                        return false;
                    }

                    reader.BaseStream.Seek(_offset + sprsTop, SeekOrigin.Begin);
                    for (var i = 0; i < sprCount; i++) //FMA_SPR
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


                var g3Count = _sections[(sectionIndex * 2) + 0];
                var g3sTop  = _sections[(sectionIndex * 2) + 1];
                sectionIndex++;
                if (g3Count > Limits.MaxBFFPackets)
                {
                    return false;
                }

                reader.BaseStream.Seek(_offset + g3sTop, SeekOrigin.Begin);
                for (var i = 0; i < g3Count; i++) //FMA_G3
                {
                    ReadFMAPacket(reader, false, false, shortForm);
                }


                var g4Count = _sections[(sectionIndex * 2) + 0];
                var g4sTop  = _sections[(sectionIndex * 2) + 1];
                sectionIndex++;
                if (g4Count > Limits.MaxBFFPackets)
                {
                    return false;
                }

                reader.BaseStream.Seek(_offset + g4sTop, SeekOrigin.Begin);
                for (var i = 0; i < g4Count; i++) //FMA_G4
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
            FlushModels(0, localMatrix);

            if (_models.Count > 0)
            {
                var rootEntity = new RootEntity();
                rootEntity.ChildEntities = _models.ToArray();
                rootEntity.ComputeBounds();
                EntityResults.Add(rootEntity);
                return true;
            }

            return false;
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
                var textureIndex = reader.ReadUInt16();
                if (textureIndex < _textureHashCount)
                {
                    tPage = _textureHashes[textureIndex];
                }
                else
                {
                    var breakHere = 0;
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
                                                  u0, v0, u1, v1, u2, v2,
                                                  textured);

            AddTriangle(triangle1, null, tPage, renderFlags);

            if (quad)
            {
                var triangle2 = TriangleFromPrimitive(vertexIndex1, vertexIndex3, vertexIndex2,
                                                      r1, g1, b1, r3, g3, b3, r2, g2, b2,
                                                      u1, v1, u3, v3, u2, v2,
                                                      textured);

                AddTriangle(triangle2, null, tPage, renderFlags);
            }
        }

        private void ReadFMASPR(BinaryReader reader)
        {
            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var mode = reader.ReadByte();

            var x = reader.ReadInt16() / _scaleDivisor;
            var y = reader.ReadInt16() / _scaleDivisor;
            var z = reader.ReadInt16() / _scaleDivisor;
            var textureIndex = reader.ReadUInt16();
            uint tPage = 0;
            if (textureIndex < _textureHashCount)
            {
                tPage = _textureHashes[textureIndex];
            }
            else
            {
                var breakHere = 0;
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
            var scale = Settings.Instance.AdvancedBFFSpriteScale;// 2.5f;
            var width  = reader.ReadByte() / _scaleDivisor * scale;
            var height = reader.ReadByte() / _scaleDivisor * scale;

            var color = new Color(r / 255f, g / 255f, b / 255f);

            var uv0 = GeomMath.ConvertUV(u0, v0) * UVConst;
            var uv1 = GeomMath.ConvertUV(u1, v1) * UVConst;
            var uv2 = GeomMath.ConvertUV(u2, v2) * UVConst;
            var uv3 = GeomMath.ConvertUV(u3, v3) * UVConst;

            // Coordinates are inverted for some reason.
            var center = new Vector3(-x, -y, -z);

            // Remember that Y-up is negative, so height values are negated compared to what we set for UVs.
            // Note that these vertex coordinates also assume the default orientation of the view is (0, 0, -1).
            var vertex0 = center + new Vector3(-width,  height, 0f);
            var vertex1 = center + new Vector3( width,  height, 0f);
            var vertex2 = center + new Vector3(-width, -height, 0f);
            var vertex3 = center + new Vector3( width, -height, 0f);

            var renderFlags = RenderFlags.Textured | RenderFlags.Sprite;

            var triangle1 = new Triangle
            {
                Vertices = new[] { vertex2, vertex1, vertex0 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color, color, color },
                Uv = new[] { uv2, uv1, uv0 },
                AttachableIndices = Triangle.EmptyAttachableIndices,
            };
            triangle1.TiledUv = new TiledUV(triangle1.Uv, 0f, 0f, 1f, 1f);
            triangle1.Uv = (Vector2[])triangle1.Uv.Clone();

            AddTriangle(triangle1, center, tPage, renderFlags);

            var triangle2 = new Triangle
            {
                Vertices = new[] { vertex2, vertex3, vertex1 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color, color, color },
                Uv = new[] { uv2, uv3, uv1 },
                AttachableIndices = Triangle.EmptyAttachableIndices,
            };
            triangle2.TiledUv = new TiledUV(triangle2.Uv, 0f, 0f, 1f, 1f);
            triangle2.Uv = (Vector2[])triangle2.Uv.Clone();

            AddTriangle(triangle2, center, tPage, renderFlags);
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
            var textureIndex = reader.ReadUInt16();
            uint tPage = 0;
            if (textureIndex < _textureHashCount)
            {
                tPage = _textureHashes[textureIndex];
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
                                                 u0, v0, u1, v1, u2, v2,
                                                 true);

            AddTriangle(triangle, null, tPage, RenderFlags.Textured);
        }

        private Triangle TriangleFromPrimitive(
            uint vertexIndex0, uint vertexIndex1, uint vertexIndex2,
            byte r0, byte g0, byte b0,
            byte r1, byte g1, byte b1,
            byte r2, byte g2, byte b2,
            byte u0, byte v0,
            byte u1, byte v1,
            byte u2, byte v2,
            bool textured)
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

            var uv0 = GeomMath.ConvertUV(u0, v0) * UVConst;
            var uv1 = GeomMath.ConvertUV(u1, v1) * UVConst;
            var uv2 = GeomMath.ConvertUV(u2, v2) * UVConst;

            var triangle = new Triangle
            {
                Vertices = new[] { vertex2, vertex1, vertex0 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color2, color1, color0 },
                Uv = new[] { uv2, uv1, uv0 },
                AttachableIndices = Triangle.EmptyAttachableIndices,
            };
            if (textured)
            {
                triangle.TiledUv = new TiledUV(triangle.Uv, 0f, 0f, 1f, 1f);
                triangle.Uv = (Vector2[])triangle.Uv.Clone();
            }

            return triangle;
        }

        private void FlushModels(uint modelIndex, Matrix4 localMatrix)
        {
            foreach (var kvp in _groupedTriangles)
            {
                var renderInfo = kvp.Key;
                var triangles = kvp.Value;
                var model = new ModelEntity
                {
                    Triangles = triangles.ToArray(),
                    TexturePage = 0,
                    TextureLookup = CreateTextureLookup(renderInfo),
                    RenderFlags = renderInfo.RenderFlags,
                    MixtureRate = renderInfo.MixtureRate,
                    TMDID = modelIndex + 1u, // Only one model per BFF
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
                    TextureLookup = CreateTextureLookup(renderInfo),
                    RenderFlags = renderInfo.RenderFlags,
                    MixtureRate = renderInfo.MixtureRate,
                    SpriteCenter = spriteCenter,
                    TMDID = modelIndex + 1u, // Only one model per BFF
                    OriginalLocalMatrix = localMatrix,
                };
                _models.Add(model);
            }
            _groupedTriangles.Clear();
            _groupedSprites.Clear();
        }

        private static TextureLookup CreateTextureLookup(RenderInfo renderInfo)
        {
            if (renderInfo.RenderFlags.HasFlag(RenderFlags.Textured))
            {
                return new TextureLookup
                {
                    ID = renderInfo.TexturePage, // CRC-32 of name
                    ExpectedFormat = SPTParser.FormatNameConst,
                    UVConversion = TextureUVConversion.TextureSpace,
                    TiledAreaConversion = TextureUVConversion.TextureSpace,
                    // Real clamp seen in source assigns to 8px, which doesn't make much sense,
                    // likely an error value to make it easy to spot.
                    UVClamp = true,
                };
            }
            return null;
        }

        private void AddTriangle(Triangle triangle, Vector3? spriteCenter, uint tPage, RenderFlags renderFlags, MixtureRate mixtureRate = MixtureRate.None)
        {
            renderFlags |= RenderFlags.Unlit; // BFF has no normals, so there's no lighting
            if (!spriteCenter.HasValue)
            {
                renderFlags |= RenderFlags.DoubleSided;
            }
            if (renderFlags.HasFlag(RenderFlags.Textured))
            {
                triangle.CorrectUVTearing();
            }
            var renderInfo = new RenderInfo(tPage, renderFlags, mixtureRate);
            if (!spriteCenter.HasValue)
            {
                if (!_groupedTriangles.TryGetValue(renderInfo, out var triangles))
                {
                    triangles = new List<Triangle>();
                    _groupedTriangles.Add(renderInfo, triangles);
                }
                triangles.Add(triangle);
            }
            else
            {
                var tuple = new Tuple<Vector3, RenderInfo>(spriteCenter.Value, renderInfo);
                if (!_groupedSprites.TryGetValue(tuple, out var triangles))
                {
                    triangles = new List<Triangle>();
                    _groupedSprites.Add(tuple, triangles);
                }
                triangles.Add(triangle);
            }
        }
    }
}
