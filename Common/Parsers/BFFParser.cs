using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Parsers
{
    // Blitz Games: .BFF model library format
    // This format is a series of ID/Length/Hash/Data sections, ~~but we currently only scan by individual sections.~~
    // We extend PILParser so that we can share the ReadPSI function (without having to construct a separate instance).
    public class BFFParser : PILParser
    {
        // UVs are in texture space, and are stored in units of 2, so that 128
        // can be used to reach the last row/column of the texture.
        private const float UVConst = 2f; // UV multiplier for FMM models

        //private long _offset2;
        //private float _scaleDivisor;
        private readonly uint[] _headerValues = new uint[13];
        private readonly uint[] _sections = new uint[6 * 2]; // count/top pairs
        //private Vector3[] _vertices;
        //private uint _vertexCount;
        //private uint[] _textureHashes;
        //private uint _textureHashCount;
        //private readonly Dictionary<RenderInfo, List<Triangle>> _groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();
        //private readonly Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>> _groupedSprites = new Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>>();
        //private readonly List<ModelEntity> _models = new List<ModelEntity>();
        private readonly Dictionary<uint, List<ModelEntity>> _modelHashes = new Dictionary<uint, List<ModelEntity>>();
        private readonly HashSet<uint> _usedModelHashes = new HashSet<uint>();
        private readonly List<long> _postProcessPositions = new List<long>();

        public BFFParser(EntityAddedAction entityAdded, AnimationAddedAction animationAdded)
            : base(entityAdded: entityAdded, animationAdded: animationAdded)
        {
        }

        public override string FormatName => "BFF";

        protected override void Parse(BinaryReader reader)
        {
            ReadBFF(reader);
            //ReadFMM(reader);
        }

        #region BFF

        private bool ReadBFF(BinaryReader reader)
        {
            // Reset library state
            _modelHashes.Clear();
            _usedModelHashes.Clear();
            _postProcessPositions.Clear();

            uint modelIndex = 0;
            uint entryCount = 0;
            uint errorCount = 0;
            uint unknownCount = 0;
            var position = reader.BaseStream.Position;
            while (position + 12 < reader.BaseStream.Length)
            {
                if (++entryCount > Limits.MaxBFFEntries)
                {
                    EntityResults.Clear();
                    return false;
                }

                var id0 = (char)reader.ReadByte();
                var id1 = (char)reader.ReadByte();
                var id2 = (char)reader.ReadByte();
                var id3 = reader.ReadByte();
                var length = reader.ReadUInt32(); // Length from start of BFF header
                var nameCRC = reader.ReadUInt32();

                if (!IsBFFIDChar(id0) || !IsBFFIDChar(id1) || !IsBFFIDChar(id2) || !IsBFFIDByte(id3))
                {
                    // All BFF IDs start with three letters (we're also checking for digits for sanity).
                    // But anything other than these characters can be assumed to not be a BFF header.
                    // This detection is essential, because we have no way to detect the real end of a BFF file.
                    // And WE NEED to be able to detect the end, to avoid skipping over unrelated files when we set MinIncrement.
                    break;
                }
                else if (length < 12 || position + length > reader.BaseStream.Length)
                {
                    break; // Invalid length
                }

                try
                {
                    if (id0 == 'P' && id1 == 'S' && id2 == 'I' && id3 == 1) // Only id3 1 is contained in BFF files
                    {
                        // Standalone actor model, these are added as-is, and not attached to any world.
                        if (!ReadPSI(reader, position, id3, nameCRC))
                        {
                            var breakHere = 0;
                        }
                    }
                    else if (id0 == 'F' && id1 == 'M' && id2 == 'M' && (id3 == 0 || id3 == 1 || id3 == 4 || id3 == 5 || id3 == 6))
                    {
                        // Models are added to lists by hash. After we've read the entire BFF file,
                        // then we read each world and add those models to those worlds.
                        if (ReadFMM(reader, position, id3, nameCRC, modelIndex))
                        {
                            modelIndex++;
                        }
                        else
                        {
                            var breakHere = 0;
                        }
                    }
                    else if (id0 == 'F' && id1 == 'M' && id2 == 'W' && (id3 == 0 || id3 == 2))
                    {
                        // World file that groups FMM models together.
                        // Parse these last, so that we have all the models ready.
                        _postProcessPositions.Add(position);
                    }
                    else
                    {
                        unknownCount++;
                    }
                }
                catch
                {
                    errorCount++;
                }

                position = reader.BaseStream.Seek(position + length, SeekOrigin.Begin);
            }

            var endPosition = position;


            // Combine models that are all used in the same worlds (we don't want to have to manually check 100+ models)
            uint worldIndex = 0;
            foreach (var postProcessPosition in _postProcessPositions)
            {
                reader.BaseStream.Seek(postProcessPosition, SeekOrigin.Begin);

                var id0 = (char)reader.ReadByte();
                var id1 = (char)reader.ReadByte();
                var id2 = (char)reader.ReadByte();
                var id3 = reader.ReadByte();
                var length = reader.ReadUInt32(); // Length from start of BFF header
                var nameCRC = reader.ReadUInt32();

                if (id0 == 'F' && id1 == 'M' && id2 == 'W' && (id3 == 0 || id3 == 2))
                {
                    // World file that groups FMM models together.
                    // Parse these last, so that we have all the models ready.
                    if (ReadFMW(reader, postProcessPosition, id3, nameCRC, worldIndex))
                    {
                        worldIndex++;
                    }
                    else
                    {
                        var breakHere = 0;
                    }
                }
            }

            // Find any models that weren't included in world files, and add those as individual models.
            foreach (var kvp in _modelHashes)
            {
                var nameCRC = kvp.Key;
                if (!_usedModelHashes.Contains(nameCRC))
                {
                    // This model was not referenced by any world file, so include it as-is.
                    var models = kvp.Value;
                    if (models.Count > 0)
                    {
                        var rootEntity = new RootEntity();
                        rootEntity.ChildEntities = models.ToArray();
                        rootEntity.FormatName = "FMM"; // Sub-format name
                        rootEntity.ComputeBounds();
                        EntityResults.Add(rootEntity);
                    }
                }
            }

            if (EntityResults.Count > 0)
            {
                // Prevent capturing following BFF headers as another start of file
                MinOffsetIncrement = (endPosition - _offset);
                return true;
            }

            return false;
        }

        private static bool IsBFFIDChar(char c)
        {
            // Only uppercase letters have been observed in the first 3 id chars.
            // But we're checking these other characters for sanity's sake.
            // Space used to be used for "VH ".
            return (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ' ';// || (c >= 'a' && c <= 'z') || c == '_' || c == '-';
        }

        private static bool IsBFFIDByte(byte b)
        {
            return (b >= 0 && b <= 10) || (b >= (byte)'0' && b <= (byte)'9');
        }

        #endregion

        #region FMW (FMA World)

        private bool ReadFMW(BinaryReader reader, long offset, byte id3, uint nameCRC, uint worldIndex)
        {
            // Reset world state
            _scaleDivisor = _scaleDivisorTranslation = Settings.Instance.AdvancedBFFScaleDivisor;
            _models.Clear();

            _offset2 = offset;

            var modelCount = reader.ReadUInt32();
            uint missingCount = 0;
            if (modelCount == 0 || modelCount > Limits.MaxBFFEntries)
            {
                return false;
            }
            _models.Clear();
            if (id3 == 0)
            {
                for (uint i = 0; i < modelCount; i++)
                {
                    var modelCRC = reader.ReadUInt32();
                    if (!_modelHashes.TryGetValue(modelCRC, out var models))
                    {
                        missingCount++;
                        continue;
                    }
                    _usedModelHashes.Add(modelCRC);

                    foreach (var model in models)
                    {
                        var newModel = CloneModel(model);
                        newModel.TMDID = i + 1u;
                        _models.Add(newModel);
                    }
                }
            }
            else if (id3 == 2)
            {
                for (uint i = 0; i < modelCount; i++)
                {
                    var posX = reader.ReadInt32() / _scaleDivisor;
                    var posY = reader.ReadInt32() / _scaleDivisor;
                    var posZ = reader.ReadInt32() / _scaleDivisor;
                    var rotX = reader.ReadInt16() / 4096f;
                    var rotY = reader.ReadInt16() / 4096f;
                    var rotZ = reader.ReadInt16() / 4096f;
                    var rotW = reader.ReadInt16() / 4096f;
                    var radius = reader.ReadUInt32() / _scaleDivisor;

                    var terrain = reader.ReadUInt16();
                    var enabled = reader.ReadByte();
                    var pad = reader.ReadByte(); //pad

                    var tag = reader.ReadUInt32();
                    var sortDepth = reader.ReadUInt32();

                    var modelCRC = reader.ReadUInt32();
                    // Basically next nodes in a linked list of entries that all perform the same behavior.
                    // i.e. Set enabled to true for this entry, then subdiv and each following subdiv do the same.
                    var subdivModelIndex = reader.ReadUInt32();

                    if (!_modelHashes.TryGetValue(modelCRC, out var models))
                    {
                        missingCount++;
                        continue;
                    }
                    _usedModelHashes.Add(modelCRC);

                    foreach (var model in models)
                    {
                        var newModel = CloneModel(model);
                        var translation = new Vector3(posX, posY, posZ) + model.Translation;
                        var localMatrix = Matrix4.CreateFromQuaternion(new Quaternion(rotX, rotY, rotZ, rotW)) *
                                          Matrix4.CreateTranslation(translation);
                        newModel.OriginalLocalMatrix = localMatrix;
                        newModel.TMDID = i + 1u;
                        _models.Add(newModel);
                    }
                }
            }

            if (_models.Count > 0)
            {
                var rootEntity = new RootEntity();
                rootEntity.ChildEntities = _models.ToArray();
                //rootEntity.FormatName = CombineFormatName("FMW", "FMM"); // Sub-format names
                rootEntity.FormatName = "FMW"; // Sub-format name
                rootEntity.ComputeBounds();
                EntityResults.Add(rootEntity);
                return true;
            }

            return false;
        }

        private static ModelEntity CloneModel(ModelEntity model)
        {
            var newTriangles = new Triangle[model.Triangles.Length];
            for (var i = 0; i < newTriangles.Length; i++)
            {
                newTriangles[i] = new Triangle(model.Triangles[i]);
            }
            return new ModelEntity(model, newTriangles);
        }

        #endregion

        #region FMM (FMA Mesh)

        // Standalone function to read without the BFF container
        private bool ReadFMM(BinaryReader reader)
        {
            var id0 = (char)reader.ReadByte();
            var id1 = (char)reader.ReadByte();
            var id2 = (char)reader.ReadByte();
            var id3 = reader.ReadByte();
            var length = reader.ReadUInt32(); // Length from start of BFF header
            var nameCRC = reader.ReadUInt32();

            if (length <= 12 || _offset + length > reader.BaseStream.Length)
            {
                return false;
            }

            if (id0 == 'F' && id1 == 'M' && id2 == 'M' && (id3 == 0 || id3 == 1 || id3 == 4 || id3 == 5 || id3 == 6))
            {
                if (ReadFMM(reader, _offset, id3, nameCRC, 0))
                {
                    // Format has no container, don't include "BFF/" format prefix
                    //var rootEntity = EntityResults[EntityResults.Count - 1];
                    //rootEntity.FormatName = AbsoluteFormatName(rootEntity.FormatName);
                    return true;
                }
                else
                {
                    var breakHere = 0;
                }
            }
            return false;
        }

        private bool ReadFMM(BinaryReader reader, long offset, byte id3, uint nameCRC, uint modelIndex)
        {
            // Reset model state
            _scaleDivisor = _scaleDivisorTranslation = Settings.Instance.AdvancedBFFScaleDivisor;
            _groupedTriangles.Clear();
            _groupedSprites.Clear();

            _offset2 = offset;


            const int BFF_FMA_MESH_ZERO_ID   = 0; // Frogger 2
            const int BFF_FMA_MESH_SHORT_ID  = 5; // Chicken Run
            const int BFF_FMA_MESH4_SHORT_ID = 6;
            const int BFF_FMA_MESH_LONG_ID   = 1; // Long form IDs noted to be used for PC versions of files
            const int BFF_FMA_MESH4_LONG_ID  = 4;

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

            var headerSize = 0; // Size including stop value in zeroForm
            if (zeroForm)
            {
                // Header uses following format: (may exclude flags and shift)
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
                if (_offset2 + top == reader.BaseStream.Position)
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

            reader.BaseStream.Seek(_offset2 + vertsTop, SeekOrigin.Begin);
            if (_vertices == null || _vertices.Length < _vertexCount)
            {
                _vertices = new Vector3[_vertexCount];
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

            reader.BaseStream.Seek(_offset2 + textureHashesTop, SeekOrigin.Begin);
            if (_textureHashes == null || _textureHashes.Length < _textureHashCount)
            {
                _textureHashes = new uint[_textureHashCount];
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

            reader.BaseStream.Seek(_offset2 + gt3sTop, SeekOrigin.Begin);
            if (!polyGT3Form)
            {
                for (var i = 0; i < gt3Count; i++) //FMA_GT3
                {
                    if (!ReadFMAPacket(reader, false, true, shortForm))
                    {
                        return false;
                    }
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

                    if (!ReadPolyGT3(reader, vertexIndex0, vertexIndex1, vertexIndex2))
                    {
                        return false;
                    }
                }
            }


            var gt4Count = _sections[(sectionIndex * 2) + 0];
            var gt4sTop  = _sections[(sectionIndex * 2) + 1];
            sectionIndex++;
            if (gt4Count > Limits.MaxBFFPackets)
            {
                return false;
            }

            reader.BaseStream.Seek(_offset2 + gt4sTop, SeekOrigin.Begin);
            if (!polyGT3Form)
            {
                for (var i = 0; i < gt4Count; i++) //FMA_GT4
                {
                    if (!ReadFMAPacket(reader, true, true, shortForm))
                    {
                        return false;
                    }
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
                    if (!ReadPolyGT3(reader, vertexIndex0, vertexIndex1, vertexIndex2))
                    {
                        return false;
                    }
                    if (!ReadPolyGT3(reader, vertexIndex1, vertexIndex3, vertexIndex2))
                    {
                        return false;
                    }
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

                    reader.BaseStream.Seek(_offset2 + sprsTop, SeekOrigin.Begin);
                    for (var i = 0; i < sprCount; i++) //FMA_SPR
                    {
                        if (!ReadFMASPR(reader))
                        {
                            return false;
                        }
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

                reader.BaseStream.Seek(_offset2 + g3sTop, SeekOrigin.Begin);
                for (var i = 0; i < g3Count; i++) //FMA_G3
                {
                    if (!ReadFMAPacket(reader, false, false, shortForm))
                    {
                        return false;
                    }
                }


                var g4Count = _sections[(sectionIndex * 2) + 0];
                var g4sTop  = _sections[(sectionIndex * 2) + 1];
                sectionIndex++;
                if (g4Count > Limits.MaxBFFPackets)
                {
                    return false;
                }

                reader.BaseStream.Seek(_offset2 + g4sTop, SeekOrigin.Begin);
                for (var i = 0; i < g4Count; i++) //FMA_G4
                {
                    if (!ReadFMAPacket(reader, true, false, shortForm))
                    {
                        return false;
                    }
                }
            }

            var localMatrix = Matrix4.CreateTranslation(posX, posY, posZ);
            if (zeroForm)
            {
                var rotationMatrix = Matrix4.CreateFromQuaternion(new Quaternion(rotX, rotY, rotZ, rotW));
                localMatrix = rotationMatrix * localMatrix;
            }
            FlushModels(modelIndex, nameCRC, localMatrix);

            return true;
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

        private bool ReadFMAPacket(BinaryReader reader, bool quad, bool textured, bool shortForm)
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


            if (vertexIndex0 >= _vertexCount || vertexIndex1 >= _vertexCount || vertexIndex2 >= _vertexCount || (quad && vertexIndex3 >= _vertexCount))
            {
                return false;
            }
            var vertex0 = _vertices[vertexIndex0];
            var vertex1 = _vertices[vertexIndex1];
            var vertex2 = _vertices[vertexIndex2];
            var vertex3 = quad ? _vertices[vertexIndex3] : Vector3.Zero;

            var color0 = new Color3(r0, g0, b0);
            var color1 = new Color3(r1, g1, b1);
            var color2 = new Color3(r2, g2, b2);
            var color3 = quad ? new Color3(r3, g3, b3) : Color3.Black;

            Vector2 uv0, uv1, uv2, uv3;
            if (textured)
            {
                uv0 = GeomMath.ConvertUV(u0, v0) * UVConst;
                uv1 = GeomMath.ConvertUV(u1, v1) * UVConst;
                uv2 = GeomMath.ConvertUV(u2, v2) * UVConst;
                uv3 = quad ? GeomMath.ConvertUV(u3, v3) * UVConst : Vector2.Zero;
            }
            else
            {
                uv0 = uv1 = uv2 = uv3 = Vector2.Zero;
            }

            // Note that vertex order is reversed for FMAPackets
            var triangle1 = new Triangle
            {
                Vertices = new[] { vertex2, vertex1, vertex0 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color2, color1, color0 },
                Uv = new[] { uv2, uv1, uv0 },
            };
            if (textured)
            {
                triangle1.TiledUv = new TiledUV(triangle1.Uv, 0f, 0f, 1f, 1f);
                triangle1.Uv = (Vector2[])triangle1.Uv.Clone();
            }

            AddTriangle(triangle1, null, tPage, renderFlags);

            if (quad)
            {
                var triangle2 = new Triangle
                {
                    Vertices = new[] { vertex2, vertex3, vertex1 },
                    Normals = Triangle.EmptyNormals,
                    Colors = new[] { color2, color3, color1 },
                    Uv = new[] { uv2, uv3, uv1 },
                };
                if (textured)
                {
                    triangle2.TiledUv = new TiledUV(triangle2.Uv, 0f, 0f, 1f, 1f);
                    triangle2.Uv = (Vector2[])triangle2.Uv.Clone();
                }

                AddTriangle(triangle2, null, tPage, renderFlags);
            }
            return true;
        }

        private bool ReadFMASPR(BinaryReader reader)
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

            var color = new Color3(r, g, b);

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

            // Todo: Is vertex order different for FMASPR, just like it is for FMAPackets?
            var triangle1 = new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color, color, color },
                Uv = new[] { uv0, uv1, uv2 },
            };
            triangle1.TiledUv = new TiledUV(triangle1.Uv, 0f, 0f, 1f, 1f);
            triangle1.Uv = (Vector2[])triangle1.Uv.Clone();

            AddTriangle(triangle1, center, tPage, renderFlags);

            var triangle2 = new Triangle
            {
                Vertices = new[] { vertex1, vertex3, vertex2 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color, color, color },
                Uv = new[] { uv1, uv3, uv2 },
            };
            triangle2.TiledUv = new TiledUV(triangle2.Uv, 0f, 0f, 1f, 1f);
            triangle2.Uv = (Vector2[])triangle2.Uv.Clone();

            AddTriangle(triangle2, center, tPage, renderFlags);
            return true;
        }

        private bool ReadPolyGT3(BinaryReader reader, uint vertexIndex0, uint vertexIndex1, uint vertexIndex2)
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


            if (vertexIndex0 >= _vertexCount || vertexIndex1 >= _vertexCount || vertexIndex2 >= _vertexCount)
            {
                return false;
            }
            var vertex0 = _vertices[vertexIndex0];
            var vertex1 = _vertices[vertexIndex1];
            var vertex2 = _vertices[vertexIndex2];

            var color0 = new Color3(r0, g0, b0);
            var color1 = new Color3(r1, g1, b1);
            var color2 = new Color3(r2, g2, b2);

            var uv0 = GeomMath.ConvertUV(u0, v0) * UVConst;
            var uv1 = GeomMath.ConvertUV(u1, v1) * UVConst;
            var uv2 = GeomMath.ConvertUV(u2, v2) * UVConst;

            // Todo: Is vertex order different for PolyGT3, just like it is for FMAPackets?
            var triangle = new Triangle
            {
                Vertices = new[] { vertex2, vertex1, vertex0 },
                Normals = Triangle.EmptyNormals,
                Colors = new[] { color2, color1, color0 },
                Uv = new[] { uv2, uv1, uv0 },
            };
            triangle.TiledUv = new TiledUV(triangle.Uv, 0f, 0f, 1f, 1f);
            triangle.Uv = (Vector2[])triangle.Uv.Clone();

            AddTriangle(triangle, null, tPage, RenderFlags.Textured);
            return true;
        }

        #endregion

        private void FlushModels(uint modelIndex, uint nameCRC, Matrix4 localMatrix)
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
                if (!_modelHashes.TryGetValue(nameCRC, out var models))
                {
                    models = new List<ModelEntity>();
                    _modelHashes.Add(nameCRC, models);
                }
                models.Add(model);
            }
            foreach (var kvp in _groupedSprites)
            {
                var spriteCenter = kvp.Key.Item1;
                var renderInfo = kvp.Key.Item2;
                var triangles = kvp.Value;
                var spriteModel = new ModelEntity
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
                if (!_modelHashes.TryGetValue(nameCRC, out var models))
                {
                    models = new List<ModelEntity>();
                    _modelHashes.Add(nameCRC, models);
                }
                models.Add(spriteModel);
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
            /*if (!spriteCenter.HasValue)
            {
                renderFlags |= RenderFlags.DoubleSided;
            }*/
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
