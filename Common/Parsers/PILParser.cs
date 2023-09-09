using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace PSXPrev.Common.Parsers
{
    // Blitz Games: .PIL model library format
    public class PILParser : FileOffsetScanner
    {
        private const float UVConst = 1f; // UV multiplier for PSI models

        private const uint GPU_COM_F3  = 0x20;
        private const uint GPU_COM_F4  = 0x28;
        private const uint GPU_COM_G3  = 0x30;
        private const uint GPU_COM_G4  = 0x38;
        private const uint GPU_COM_TF3 = 0x24;
        private const uint GPU_COM_TF4 = 0x2c;
        private const uint GPU_COM_TG3 = 0x34;
        private const uint GPU_COM_TG4 = 0x3c;
        private const uint GPU_COM_TF4SPR = 0x64;

        protected long _offset2;
        protected float _scaleDivisorTranslation = 1f;
        protected float _scaleDivisor = 1f;
        protected Vector3[] _vertices;
        protected Vector3[] _normals;
        protected uint _vertexCount;
        protected uint _normalCount;
        protected uint[] _textureHashes;
        protected uint _textureHashCount;
        private ushort[] _primitiveIndices; // Temporary buffer when parsing mesh primitives.
        // Working stack of parent/top pairs, used to avoid recursion when parsing children.
        private readonly Stack<Tuple<PSIMesh, uint>> _psiMeshStack = new Stack<Tuple<PSIMesh, uint>>();
        // Actual read mesh data.
        private readonly List<PSIMesh> _psiMeshes = new List<PSIMesh>();
        private readonly byte[] _textureNameBuffer = new byte[32]; // Temporary buffer for reading texture names to hash.
        //private readonly ushort[] _sortCounts = new ushort[8];
        //private readonly uint[] _sortTops = new uint[8];
        protected readonly Dictionary<RenderInfo, List<Triangle>> _groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();
        protected readonly Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>> _groupedSprites = new Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>>();
        protected readonly List<ModelEntity> _models = new List<ModelEntity>();

        public PILParser(EntityAddedAction entityAdded)
            : base(entityAdded: entityAdded)
        {
        }

        public override string FormatName => "PIL";

        protected override void Parse(BinaryReader reader)
        {
            ReadPIL(reader);
            //ReadPSI(reader);
        }

        #region PIL

        private bool ReadPIL(BinaryReader reader)
        {
            var entryCount = reader.ReadUInt32(); // todo: This might only be read as a byte, but occupies 4 bytes
            var maxPosition = _offset + 4 + entryCount * 4;

            {
                // Check if this is a standalone PSI file (seen in Chicken Run)
                var id0 = (char)(byte)(entryCount);
                var id1 = (char)(byte)(entryCount >> 8);
                var id2 = (char)(byte)(entryCount >> 16);
                var id3 = (byte)(entryCount >> 24);

                if (id0 == 'P' && id1 == 'S' && id2 == 'I' && id3 == 0)
                {
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
                    return ReadPSI(reader);
                }
            }

            if (entryCount == 0 || entryCount > Limits.MaxBFFEntries)
            {
                return false;
            }

            uint errorCount = 0;
            uint unknownCount = 0;
            for (uint i = 0; i < entryCount; i++)
            {
                var psiTop = reader.ReadUInt32();
                if (_offset + psiTop > reader.BaseStream.Length)
                {
                    EntityResults.Clear();
                    return false;
                }

                var psiPosition = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + psiTop, SeekOrigin.Begin);

                try
                {
                    //var name = Encoding.ASCII.GetString(reader.ReadBytes(16));
                    reader.BaseStream.Seek(16, SeekOrigin.Current);
                    var nameCRC = reader.ReadUInt32();

                    var position = reader.BaseStream.Position;
                    var id0 = (char)reader.ReadByte();
                    var id1 = (char)reader.ReadByte();
                    var id2 = (char)reader.ReadByte();
                    var id3 = reader.ReadByte();

                    if (id0 == 'P' && id1 == 'S' && id2 == 'I' && id3 == 0)
                    {
                        if (ReadPSI(reader, position, id3, nameCRC))
                        {
                            maxPosition = Math.Max(maxPosition, position + 1);
                        }
                        else
                        {
                            var breakHere = 0;
                        }
                    }
                    else
                    {
                        // todo: Should we fail here, PILs should only contain PSI files...
                        unknownCount++;
                    }
                }
                catch
                {
                    errorCount++;
                }
                reader.BaseStream.Seek(psiPosition, SeekOrigin.Begin);
            }

            if (EntityResults.Count > 0)
            {
                // Prevent detecting PIL sections as standalone PSI files
                MinOffsetIncrement = (maxPosition - _offset);
                return true;
            }

            return false;
        }

        #endregion

        #region PSI

        // Standalone function to read without the PIL container
        private bool ReadPSI(BinaryReader reader)
        {
            var position = reader.BaseStream.Position;
            var id0 = (char)reader.ReadByte();
            var id1 = (char)reader.ReadByte();
            var id2 = (char)reader.ReadByte();
            var id3 = reader.ReadByte();

            if (id0 == 'P' && id1 == 'S' && id2 == 'I' && (id3 == 0 || id3 == 1))
            {
                uint length = 0, nameCRC = 0;
                if (id3 == 1)
                {
                    // We're in a BFF file format, read the rest of the header
                    length = reader.ReadUInt32();
                    if (length <= 12 || _offset + length > reader.BaseStream.Length)
                    {
                        return false;
                    }
                    nameCRC = reader.ReadUInt32();
                }

                if (ReadPSI(reader, _offset, id3, nameCRC))
                {
                    // Format has no container, don't include "PIL/" format prefix
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

        protected bool ReadPSI(BinaryReader reader, long offset, byte id3, uint nameCRC)
        {
            // Reset state
            _scaleDivisor = _scaleDivisorTranslation = Settings.Instance.AdvancedBFFScaleDivisor;
            _psiMeshStack.Clear();
            _psiMeshes.Clear();
            _groupedTriangles.Clear();
            _groupedSprites.Clear();
            _models.Clear();
            _vertexCount = 0;
            _normalCount = 0;

            _offset2 = offset;


            var d2m = id3 == 1; // direct2mesh

            uint version = 0;
            uint flags = 0;
            if (!d2m)
            {
                version = reader.ReadUInt32();
                flags = reader.ReadUInt32();
                if (version != 1)
                {
                    var breakHere = 0;
                }
            }
            //if (d2m && (version != 0 || flags != 0))
            //{
            //    var breakHere = 0;
            //}

            // Skinned models generally look completely wrong.
            // More work is needed to handle positioning for them (if that information is even stored in this file).
            var skinned = ((flags >> 0) & 0x1) == 1;

            if (flags != 0)
            {
                var breakHere = 0;
            }
            if (skinned)
            {
                _scaleDivisor /= 10f; // Increase scale by 10 (not 0x10)
            }

            // //var name = Encoding.ASCII.GetString(reader.ReadBytes(d2m ? 24 : 32));
            // //reader.BaseStream.Seek(d2m ? 24 : 32, SeekOrigin.Current);
            //var name = Encoding.ASCII.GetString(reader.ReadBytes(32));
            reader.BaseStream.Seek(32, SeekOrigin.Current);

            var meshCount = reader.ReadUInt32();
            var totalVertexCount = reader.ReadUInt32();
            if (totalVertexCount > Limits.MaxBFFVertices)
            {
                return false;
            }
            if (_vertices == null || _vertices.Length < totalVertexCount)
            {
                Array.Resize(ref _vertices, (int)totalVertexCount);
            }
            var totalPrimitiveCount = reader.ReadUInt32();
            var primitiveTop = reader.ReadUInt32();
            var animStart = reader.ReadUInt16();
            var animEnd = reader.ReadUInt16();
            // Animation segments are a list of ushort start/end pairs
            var animCount = reader.ReadUInt32();
            var animSegmentsTop = reader.ReadUInt32();
            _textureHashCount = reader.ReadUInt32();
            if (_textureHashCount > Limits.MaxBFFTextureHashes)
            {
                return false;
            }
            var textureNameTop = reader.ReadUInt32();
            var rootMeshTop = reader.ReadUInt32(); // Pointer to first mesh in linked list
            var radius = reader.ReadUInt32() / _scaleDivisor;


            uint fg3Count = 0, fg3Top = 0;
            uint fg4Count = 0, fg4Top = 0;
            uint ft3Count = 0, ft3Top = 0;
            uint ft4Count = 0, ft4Top = 0;
            uint gt3Count = 0, gt3Top = 0;
            uint gt4Count = 0, gt4Top = 0;
            uint sprCount = 0, sprTop = 0;
            if (d2m)
            {
                fg3Count = reader.ReadUInt32();
                fg4Count = reader.ReadUInt32();
                ft3Count = reader.ReadUInt32();
                ft4Count = reader.ReadUInt32();
                gt3Count = reader.ReadUInt32();
                gt4Count = reader.ReadUInt32();
                sprCount = reader.ReadUInt32();
                fg3Top = reader.ReadUInt32();
                fg4Top = reader.ReadUInt32();
                ft3Top = reader.ReadUInt32();
                ft4Top = reader.ReadUInt32();
                gt3Top = reader.ReadUInt32();
                gt4Top = reader.ReadUInt32();
                sprTop = reader.ReadUInt32();
            }

            //reader.BaseStream.Seek(192 - (d2m ? 32 : 88), SeekOrigin.Current);


            // Textures are stored as null-terminated 32-byte strings, and must be hashed manually
            reader.BaseStream.Seek(_offset2 + textureNameTop, SeekOrigin.Begin);
            if (_textureHashes == null || _textureHashes.Length < _textureHashCount)
            {
                Array.Resize(ref _textureHashes, (int)_textureHashCount);
            }
            //var textureNames = new string[_textureHashCount];
            for (uint i = 0; i < _textureHashCount; i++)
            {
                reader.Read(_textureNameBuffer, 0, 32);
                //var textureNameString = Encoding.ASCII.GetString(_textureNameBuffer);
                //textureNames[i] = textureNameString;
                _textureHashes[i] = HashString(_textureNameBuffer);
            }


            // We need to read every mesh first so that we can construct a table of all vertices.
            // (Because primitives can reference vertices defined by any mesh.....)
            _psiMeshStack.Push(new Tuple<PSIMesh, uint>(null, rootMeshTop));
            uint modelIndex = 0;
            uint vertexIndex = 0; // Offsets into each table that the mesh writes to
            uint normalIndex = 0;
            while (_psiMeshStack.Count > 0)
            {
                if (_psiMeshes.Count + _psiMeshStack.Count > (int)Limits.MaxBFFModels)
                {
                    return false;
                }

                var tuple = _psiMeshStack.Pop();
                var parent = tuple.Item1;
                var meshTop = tuple.Item2;
                reader.BaseStream.Seek(_offset2 + meshTop, SeekOrigin.Begin);

                if (!ReadMesh(reader, version, d2m, totalVertexCount, parent, modelIndex, ref vertexIndex, ref normalIndex))
                {
                    return false;
                }
                modelIndex++;
            }

            // Now that all meshes are read, it's safe to read their primitives.
            // We can also construct coordinates in the same loop.
            Coordinate[] coords = null;
            if (!d2m)
            {
                coords = new Coordinate[_psiMeshes.Count];
                for (uint i = 0; i < _psiMeshes.Count; i++)
                {
                    var psiMesh = _psiMeshes[(int)i];
                    ReadMeshPrimitives(reader, version, d2m, primitiveTop, totalPrimitiveCount, psiMesh, i);

                    var superIndex = psiMesh.Parent?.ModelIndex ?? Coordinate.NoID;
                    var coord = new Coordinate
                    {
                        OriginalLocalMatrix = psiMesh.LocalMatrix,
                        OriginalTranslation = psiMesh.Translation,
                        //OriginalRotation = psiMesh.Rotation, // Can't use this field, since it's Euler angles
                        ID = i,
                        ParentID = superIndex,
                        Coords = coords,
                    };
                    coords[i] = coord;

                    FlushModels(i, coord);
                }
            }
            else
            {
                // Too many issues with reading based off meshes,
                // so read based off defined primitives, and add them all to a single model index.
                // Based on the game code for Chicken Run, it's entirely possible that this is the INTENDED way...

                // Note that the game code only seems to read gt3s and gt4s.
                if (fg3Count != 0 || fg4Count != 0 || ft3Count != 0 || ft4Count != 0 || sprCount != 0)
                {
                    var breakHere = 0;
                }
                // todo: Are these first two treated as flat or gouraud color?
                ReadPrimitives(reader, version, d2m, GPU_COM_G3,  fg3Top, fg3Count);
                ReadPrimitives(reader, version, d2m, GPU_COM_G4,  fg4Top, fg4Count);
                ReadPrimitives(reader, version, d2m, GPU_COM_TF3, ft3Top, ft3Count);
                ReadPrimitives(reader, version, d2m, GPU_COM_TF4, ft4Top, ft4Count);
                ReadPrimitives(reader, version, d2m, GPU_COM_TG3, gt3Top, gt3Count);
                ReadPrimitives(reader, version, d2m, GPU_COM_TG4, gt4Top, gt4Count);
                ReadPrimitives(reader, version, d2m, GPU_COM_TF4SPR, sprTop, sprCount);

                FlushModels(0, null);
            }

            if (_models.Count > 0)
            {
                var rootEntity = new RootEntity();
                rootEntity.ChildEntities = _models.ToArray();
                rootEntity.FormatName = "PSI"; // Sub-format name
                rootEntity.Coords = coords;
#if DEBUG
                rootEntity.DebugData = new[] { $"version: {version}", $"d2m: {id3}", $"flags: 0x{flags:x08}" };
#endif
                rootEntity.ComputeBounds();
                EntityResults.Add(rootEntity);
                return true;
            }

            return false;
        }

        private bool ReadMesh(BinaryReader reader, uint version, bool d2m, uint totalVertexCount, PSIMesh parent, uint modelIndex, ref uint vertexIndex, ref uint normalIndex)
        {
            // Mesh header length is 120 bytes
            var meshPosition = reader.BaseStream.Position;
            var meshTop = (uint)(meshPosition - _offset2);

            // Vertices are stored in a really weird fashion.
            // Primitives use global vertex indices, that can reference any vertex stored by any mesh.
            // But meshes read vertices as local. So basically each mesh adds vertices to a list (in depth-first order).
            var meshVertexTop   = reader.ReadUInt32();
            var meshVertexCount = reader.ReadUInt32();
            var meshNormalTop   = reader.ReadUInt32();
            var meshNormalCount = reader.ReadUInt32();
            _vertexCount += meshVertexCount;
            _normalCount += meshNormalCount;
            if (_vertexCount > totalVertexCount || _normalCount > Limits.MaxBFFVertices)
            //if (_vertexCount > Limits.MaxBFFVertices || _normalCount > Limits.MaxBFFVertices)
            {
                return false;
            }
            // Same scale factor as used by TMD (scaleValue = 2^scale)
            // A non-zero value has never been observed, so for now this isn't handled.
            var scale = reader.ReadInt32();

            //var meshName = Encoding.ASCII.GetString(reader.ReadBytes(16));
            reader.BaseStream.Seek(16, SeekOrigin.Current);

            var childTop = reader.ReadUInt32();
            var nextTop  = reader.ReadUInt32();

            var scaleKeyCount  = reader.ReadUInt16();
            var moveKeyCount   = reader.ReadUInt16();
            var rotateKeyCount = reader.ReadUInt16();
            var lastKeyOffset = reader.ReadUInt16();
            if (scaleKeyCount > Limits.MaxBFFKeys || moveKeyCount > Limits.MaxBFFKeys || rotateKeyCount > Limits.MaxBFFKeys)
            {
                return false;
            }

            var scaleKeyTop  = reader.ReadUInt32();
            var moveKeyTop   = reader.ReadUInt32();
            var rotateKeyTop = reader.ReadUInt32();

            // todo: Optimize this, since we only use the first sort count/top, we don't need an array
            var sortCounts = new ushort[8];
            var sortTops   = new uint[8];
            for (var i = 0; i < 8; i++)
            {
                var sortCount = reader.ReadUInt16();
                sortCounts[i] = sortCount;
                if (sortCount > Limits.MaxBFFPackets)
                {
                    return false;
                }
            }
            for (var i = 0; i < 8; i++)
            {
                sortTops[i] = reader.ReadUInt32();
            }

            // Center is not scaled...?
            var centerX = reader.ReadInt16() / _scaleDivisorTranslation;
            var centerY = reader.ReadInt16() / _scaleDivisorTranslation;
            var centerZ = reader.ReadInt16() / _scaleDivisorTranslation;
            var pad2 = reader.ReadUInt16();

            // This would be read instead of center
            //var lastScaleKey  = reader.ReadUInt16();
            //var lastMoveKey   = reader.ReadUInt16();
            //var lastRotateKey = reader.ReadUInt16();
            //var pad2 = reader.ReadUInt16();


            reader.BaseStream.Seek(meshPosition + meshVertexTop, SeekOrigin.Begin);
            //if (_vertices == null || _vertices.Length < _vertexCount)
            //{
            //    Array.Resize(ref _vertices, (int)_vertexCount);
            //}
            for (uint i = 0; i < meshVertexCount; i++)
            {
                var x = reader.ReadInt16() / _scaleDivisor;
                var y = reader.ReadInt16() / _scaleDivisor;
                var z = reader.ReadInt16() / _scaleDivisor;
                var pad = reader.ReadUInt16();
                _vertices[vertexIndex + i] = new Vector3(x, y, z);
                if (pad != 0)
                {
                    var breakHere = 0;
                }
            }
            vertexIndex += meshVertexCount;

            reader.BaseStream.Seek(meshPosition + meshNormalTop, SeekOrigin.Begin);
            if (_normals == null || _normals.Length < _normalCount)
            {
                Array.Resize(ref _normals, (int)_normalCount);
            }
            for (uint i = 0; i < meshNormalCount; i++)
            {
                var x = reader.ReadInt16() / 4096f;
                var y = reader.ReadInt16() / 4096f;
                var z = reader.ReadInt16() / 4096f;
                var pad = reader.ReadUInt16();
                _normals[normalIndex + i] = new Vector3(x, y, z);
                if (pad != 0)
                {
                    var breakHere = 0;
                }
            }
            normalIndex += meshNormalCount;


            // todo: Optimize this, since we only use the first key, we don't need an array
            Vector3Key[] scaleKeys = null, moveKeys = null;
            QuaternionKey[] rotateKeys = null;
            if (scaleKeyCount != 0)
            {
                reader.BaseStream.Seek(meshPosition + scaleKeyTop, SeekOrigin.Begin);
                scaleKeys = new Vector3Key[scaleKeyCount];
                for (uint i = 0; i < scaleKeyCount; i++)
                {
                    // Rare instance of scale not being 4096, scale is multiplied by 4 later in the game code
                    var x = reader.ReadInt16() / 1024f;
                    var y = reader.ReadInt16() / 1024f;
                    var z = reader.ReadInt16() / 1024f;
                    var time = reader.ReadUInt16(); //pad
                    scaleKeys[i] = new Vector3Key(x, y, z, time);
                }
            }

            if (moveKeyCount != 0)
            {
                reader.BaseStream.Seek(meshPosition + moveKeyTop, SeekOrigin.Begin);
                moveKeys = new Vector3Key[moveKeyCount];
                for (uint i = 0; i < moveKeyCount; i++)
                {
                    var x = reader.ReadInt16() / _scaleDivisor;
                    var y = reader.ReadInt16() / _scaleDivisor;
                    var z = reader.ReadInt16() / _scaleDivisor;
                    var time = reader.ReadUInt16(); //pad
                    // Y value seems to be negated, but I'm not sure if it's the same for all types of actors.
                    // I've seen other examples in game code of different inversions (i.e. -x,-y,+z)
                    moveKeys[i] = new Vector3Key(x, -y, z, time);
                }
            }

            if (rotateKeyCount != 0)
            {
                reader.BaseStream.Seek(meshPosition + rotateKeyTop, SeekOrigin.Begin);
                rotateKeys = new QuaternionKey[rotateKeyCount];
                for (uint i = 0; i < rotateKeyCount; i++)
                {
                    var x = reader.ReadInt16() / 4096f;
                    var y = reader.ReadInt16() / 4096f;
                    var z = reader.ReadInt16() / 4096f;
                    var w = reader.ReadInt16() / 4096f;
                    var time = reader.ReadUInt16(); //pad
                    rotateKeys[i] = new QuaternionKey(x, y, z, w, time);
                }
            }


            if (scale != 0)
            {
                var breakHere = 0;
            }
            var scaleValue = (float)Math.Pow(2, scale); // -2=0.25, -1=0.5, 0=1.0, 1=2.0, 2=4.0, ...
            var center = new Vector3(centerX, centerY, centerZ);

            var psiMesh = new PSIMesh
            {
                Parent = parent,
                ModelIndex = modelIndex,
                MeshTop = meshTop,
                SortTops = sortTops,
                SortCounts = sortCounts,
                ScaleValue = scaleValue, // Not sure if this is used or not...
                Center = center, // Seems to not be used for translation...
                ScaleKeys = scaleKeys,
                MoveKeys = moveKeys,
                RotateKeys = rotateKeys,
            };
            _psiMeshes.Add(psiMesh);

            // Add next first, since we parse depth-first (adding child last will mean its first out)
            if (nextTop != 0)
            {
                _psiMeshStack.Push(new Tuple<PSIMesh, uint>(parent, meshTop + nextTop));
            }
            if (childTop != 0)
            {
                _psiMeshStack.Push(new Tuple<PSIMesh, uint>(psiMesh, meshTop + childTop));
            }

            return true;
        }

        private bool ReadMeshPrimitives(BinaryReader reader, uint version, bool d2m, uint primitiveTop, uint totalPrimitiveCount, PSIMesh psiMesh, uint modelIndex)
        {
            // I think each sort count/top is a directional version of the same model.
            // Not sure how we should handle these different versions, so for now we only read the first.
            // We could maybe do a check to see if each top/count is different, and if so,
            // then create individual root models for each.
            var s = 0;
            //for (var s = 0; s < 1; s++)
            {
                // PS1\x01 tends to fail either here with the sort values (all 0 counts)
                var sortCount = psiMesh.SortCounts[s];
                var sortTop = psiMesh.SortTops[s];

                // Read all primitive indices first, so that we don't need to seek back and forth
                if (_primitiveIndices == null || _primitiveIndices.Length < sortCount)
                {
                    Array.Resize(ref _primitiveIndices, sortCount);
                }
                reader.BaseStream.Seek(_offset2 + psiMesh.MeshTop + sortTop, SeekOrigin.Begin);
                for (uint i = 0; i < sortCount; i++)
                {
                    _primitiveIndices[i] = reader.ReadUInt16();
                }

                for (uint i = 0; i < sortCount; i++)
                {
                    // Or PS1\x01 tends to fail here where the primitive indices don't align to the newer primitive format size
                    var primitiveIndex = _primitiveIndices[i] * 8u;
                    reader.BaseStream.Seek(_offset2 + primitiveTop + primitiveIndex, SeekOrigin.Begin);
                    var next = reader.ReadUInt32();
                    var olen = reader.ReadByte();
                    var ilen = reader.ReadByte();
                    var flag = reader.ReadByte();
                    var mode = reader.ReadByte();
                    var primitiveType = (mode & 0xfdu);

                    if (!ReadPrimitive(reader, version, d2m, primitiveType, flag, mode))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ReadPrimitives(BinaryReader reader, uint version, bool d2m, uint primitiveType, uint primitiveTop, uint primitiveCount)
        {
            if (primitiveCount == 0)
            {
                return true;
            }

            reader.BaseStream.Seek(_offset2 + primitiveTop, SeekOrigin.Begin);
            var primitivePosition = reader.BaseStream.Position;
            var length = GetPrimitiveLength(version, d2m, primitiveType);
            if (length == 0)
            {
                return false;
            }
            for (uint i = 0; i < primitiveCount; i++)
            {
                var next = reader.ReadUInt32();
                var olen = reader.ReadByte();
                var ilen = reader.ReadByte();
                var flag = reader.ReadByte();
                var mode = reader.ReadByte();

                if (!ReadPrimitive(reader, version, d2m, primitiveType, flag, mode))
                {
                    return false;
                }

                primitivePosition = reader.BaseStream.Seek(primitivePosition + length, SeekOrigin.Begin);
            }

            return true;
        }

        private uint GetPrimitiveLength(uint version, bool d2m, uint primitiveType)
        {
            // Length is always a multiple of 8 (since primitiveIndex is multiplied by 8)
            switch (primitiveType)
            {
                case GPU_COM_TF3: // 0x24: GPU_COM_TF3 / TMD_P_FT3I
                    return 0x20;
                case GPU_COM_TF4: // 0x2c: GPU_COM_TF4 / TMD_P_FT4I
                    return 0x28;
                case GPU_COM_TG3: // 0x34: GPU_COM_TG3 / TMD_P_GT3I
                    return (!d2m ? 0x28u : 0x30u); // +8 for normals
                case GPU_COM_TG4: // 0x3c: GPU_COM_TG4 / TMD_P_GT4I
                    return (!d2m ? 0x30u : 0x38u); // +8 for normals
                case GPU_COM_F3: // 0x20: GPU_COM_F3  / TMD_P_FG3I
                case GPU_COM_G3: // 0x30: GPU_COM_G3  / TMD_P_FG3I
                    return 0x20;
                case GPU_COM_F4: // 0x28: GPU_COM_F4  / TMD_P_FG4I
                case GPU_COM_G4: // 0x38: GPU_COM_G4  / TMD_P_FG4I
                    return 0x28;

                case GPU_COM_TF4SPR: // 0x64: GPU_COM_TF4SPR / TMD_P_FT4I (D2M_TMD_P_SP4I)
                    return 0x28;

                default:
                    return 0;
            }
        }

        private bool ReadPrimitive(BinaryReader reader, uint version, bool d2m, uint primitiveType, byte flag, byte mode)
        {
            switch (primitiveType)
            {
                case GPU_COM_TF3: // 0x24: GPU_COM_TF3 / TMD_P_FT3I
                case GPU_COM_TF4: // 0x2c: GPU_COM_TF4 / TMD_P_FT4I
                case GPU_COM_TG3: // 0x34: GPU_COM_TG3 / TMD_P_GT3I
                case GPU_COM_TG4: // 0x3c: GPU_COM_TG4 / TMD_P_GT4I
                case GPU_COM_F3: // 0x20: GPU_COM_F3  / TMD_P_FG3I
                case GPU_COM_G3: // 0x30: GPU_COM_G3  / TMD_P_FG3I
                case GPU_COM_F4: // 0x28: GPU_COM_F4  / TMD_P_FG4I
                case GPU_COM_G4: // 0x38: GPU_COM_G4  / TMD_P_FG4I
                    return ReadStandardPrimitive(reader, version, d2m, primitiveType, flag, mode);

                case GPU_COM_TF4SPR: // 0x64: GPU_COM_TF4SPR / TMD_P_FT4I (D2M_TMD_P_SP4I)
                    return ReadSpritePrimitive(reader, version, d2m, flag, mode);

                default:
                    return false;
            }
        }

        private bool ReadStandardPrimitive(BinaryReader reader, uint version, bool d2m, uint primitiveType, byte flag, byte mode)
        {
            byte u0 = 0, u1 = 0, u2 = 0, u3 = 0;
            byte v0 = 0, v1 = 0, v2 = 0, v3 = 0;
            ushort textureIndex = 0, tsb = 0;
            uint tPage = 0;
            byte r0, r1, r2, r3 = 0;
            byte g0, g1, g2, g3 = 0;
            byte b0, b1, b2, b3 = 0;
            byte rgbMode, tile = 0;
            ushort vertexIndex0, vertexIndex1 = 0, vertexIndex2, vertexIndex3 = 0;
            ushort normalIndex0 = 0, normalIndex1 = 0, normalIndex2 = 0, normalIndex3 = 0;

            var textured = ((mode >> 2) & 0x1) == 1;
            var quad     = ((mode >> 3) & 0x1) == 1;
            var gouraud  = ((mode >> 4) & 0x1) == 1;
            var bothSides = ((flag >> 0) & 0x1) == 1;
            var semiTrans = ((flag >> 1) & 0x1) == 1 && !textured;
            // Although non-direct2Mesh primitives can store a normal, these primitives are always unlit.
            var light = /*(!textured || !gouraud) ||*/ d2m;

            switch (primitiveType)
            {
                case GPU_COM_TF3: // 0x24: GPU_COM_TF3 / TMD_P_FT3I
                    // uv0-2
                    // rgb0
                    // n0
                    // v0-v2
                case GPU_COM_TF4: // 0x2c: GPU_COM_TF4 / TMD_P_FT4I
                    // uv0-3
                    // rgb0
                    // v0-v3
                    // n0
                    u0 = reader.ReadByte();
                    v0 = reader.ReadByte();
                    textureIndex = reader.ReadUInt16();

                    u1 = reader.ReadByte();
                    v1 = reader.ReadByte();
                    tsb = reader.ReadUInt16();

                    u2 = reader.ReadByte();
                    v2 = reader.ReadByte();
                    tile = reader.ReadByte();
                    reader.ReadByte(); //pad

                    if (quad)
                    {
                        u3 = reader.ReadByte();
                        v3 = reader.ReadByte();
                        reader.ReadUInt16(); //pad
                    }

                    r0 = r1 = r2 = r3 = reader.ReadByte();
                    g0 = g1 = g2 = g3 = reader.ReadByte();
                    b0 = b1 = b2 = b3 = reader.ReadByte();
                    rgbMode = reader.ReadByte();

                    if (!quad)
                    {
                        normalIndex0 = reader.ReadUInt16();
                    }
                    vertexIndex0 = reader.ReadUInt16();
                    vertexIndex1 = reader.ReadUInt16();
                    vertexIndex2 = reader.ReadUInt16();
                    if (quad)
                    {
                        vertexIndex3 = reader.ReadUInt16();
                        // For some reason, normalIndex0 appears last in this one case only
                        normalIndex0 = reader.ReadUInt16();
                        reader.ReadUInt16(); //pad
                    }
                    normalIndex1 = normalIndex2 = normalIndex3 = normalIndex0;
                    break;

                case GPU_COM_TG3: // 0x34: GPU_COM_TG3 / TMD_P_GT3I
                    // uv0-2
                    // v0
                    // rgb0-2
                    // v1-v2
                case GPU_COM_TG4: // 0x3c: GPU_COM_TG4 / TMD_P_GT4I
                    // uv0-2
                    // v0
                    // uv3
                    // v1
                    // rgb0-3
                    // v2-v3
                    u0 = reader.ReadByte();
                    v0 = reader.ReadByte();
                    textureIndex = reader.ReadUInt16();

                    u1 = reader.ReadByte();
                    v1 = reader.ReadByte();
                    tsb = reader.ReadUInt16();

                    u2 = reader.ReadByte();
                    v2 = reader.ReadByte();
                    vertexIndex0 = reader.ReadUInt16();

                    if (quad)
                    {
                        u3 = reader.ReadByte();
                        v3 = reader.ReadByte();
                        vertexIndex1 = reader.ReadUInt16();
                    }

                    r0 = reader.ReadByte();
                    g0 = reader.ReadByte();
                    b0 = reader.ReadByte();
                    rgbMode = reader.ReadByte();

                    r1 = reader.ReadByte();
                    g1 = reader.ReadByte();
                    b1 = reader.ReadByte();
                    tile = reader.ReadByte();

                    r2 = reader.ReadByte();
                    g2 = reader.ReadByte();
                    b2 = reader.ReadByte();
                    reader.ReadByte(); //pad

                    if (quad)
                    {
                        r3 = reader.ReadByte();
                        g3 = reader.ReadByte();
                        b3 = reader.ReadByte();
                        reader.ReadByte(); //pad
                    }

                    if (!quad)
                    {
                        vertexIndex1 = reader.ReadUInt16();
                    }
                    vertexIndex2 = reader.ReadUInt16();
                    if (quad)
                    {
                        vertexIndex3 = reader.ReadUInt16();
                    }
                    reader.ReadUInt32(); //avgTexCol
                    if (d2m)
                    {
                        normalIndex0 = reader.ReadUInt16();
                        normalIndex1 = reader.ReadUInt16();
                        normalIndex2 = reader.ReadUInt16();
                        if (quad)
                        {
                            normalIndex3 = reader.ReadUInt16();
                        }
                    }
                    break;

                case GPU_COM_F3: // 0x20: GPU_COM_F3  / TMD_P_FG3I
                    // rgb0-2 (only use 0)
                    // n0
                    // v0-2
                case GPU_COM_G3: // 0x30: GPU_COM_G3  / TMD_P_FG3I
                    // rgb0-2
                    // n0
                    // v0-2
                case GPU_COM_F4: // 0x28: GPU_COM_F4  / TMD_P_FG4I
                    // rgb0-3 (only use 0)
                    // n0
                    // v0-3
                case GPU_COM_G4: // 0x38: GPU_COM_G4  / TMD_P_FG4I
                    // rgb0-3
                    // n0
                    // v0-3
                    r0 = reader.ReadByte();
                    g0 = reader.ReadByte();
                    b0 = reader.ReadByte();
                    rgbMode = reader.ReadByte();

                    r1 = reader.ReadByte();
                    g1 = reader.ReadByte();
                    b1 = reader.ReadByte();
                    reader.ReadByte(); //pad

                    r2 = reader.ReadByte();
                    g2 = reader.ReadByte();
                    b2 = reader.ReadByte();
                    reader.ReadByte(); //pad

                    if (quad)
                    {
                        r3 = reader.ReadByte();
                        g3 = reader.ReadByte();
                        b3 = reader.ReadByte();
                        reader.ReadByte(); //pad
                    }

                    if (!gouraud)
                    {
                        // Only use the first rgb's
                        r1 = r2 = r3 = r0;
                        g1 = g2 = g3 = g0;
                        b1 = b2 = b3 = b0;
                    }

                    normalIndex0 = reader.ReadUInt16();
                    vertexIndex0 = reader.ReadUInt16();
                    vertexIndex1 = reader.ReadUInt16();
                    vertexIndex2 = reader.ReadUInt16();
                    if (quad)
                    {
                        vertexIndex3 = reader.ReadUInt16();
                        reader.ReadUInt16(); //pad
                    }
                    reader.ReadUInt32(); //pad
                    normalIndex1 = normalIndex2 = normalIndex3 = normalIndex0;
                    break;

                default:
                    return false;

            }

            if (vertexIndex0 >= _vertexCount || vertexIndex1 >= _vertexCount || vertexIndex2 >= _vertexCount || (quad && vertexIndex3 >= _vertexCount))
            {
                return false;
            }
            var vertex0 = _vertices[vertexIndex0];
            var vertex1 = _vertices[vertexIndex1];
            var vertex2 = _vertices[vertexIndex2];
            var vertex3 = quad ? _vertices[vertexIndex3] : Vector3.Zero;

            Vector3 normal0, normal1, normal2, normal3;
            if (light)
            {
                if (normalIndex0 >= _normalCount || normalIndex1 >= _normalCount || normalIndex2 >= _normalCount || (quad && normalIndex3 >= _normalCount))
                {
                    return false;
                }
                normal0 = _normals[normalIndex0];
                normal1 = _normals[normalIndex1];
                normal2 = _normals[normalIndex2];
                normal3 = quad ? _normals[normalIndex3] : Vector3.Zero;
            }
            else
            {
                normal0 = normal1 = normal2 = normal3 = Vector3.Zero;
            }

            Color color1, color2, color3;
            var color0 = new Color(r0 / 255f, g0 / 255f, b0 / 255f);
            if (gouraud)
            {
                color1 = new Color(r1 / 255f, g1 / 255f, b1 / 255f);
                color2 = new Color(r2 / 255f, g2 / 255f, b2 / 255f);
                color3 = quad ? new Color(r3 / 255f, g3 / 255f, b3 / 255f) : color0;
            }
            else
            {
                color1 = color2 = color3 = color0;
            }

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

            var renderFlags = RenderFlags.None;
            var mixtureRate = MixtureRate.None;
            if (textured)
            {
                renderFlags |= RenderFlags.Textured;
                if (textureIndex - 1u < _textureHashCount)
                {
                    tPage = _textureHashes[textureIndex - 1u];
                }
                else
                {
                    var breakHere = 0;
                }
            }
            if (!light)
            {
                renderFlags |= RenderFlags.Unlit;
            }
            if (bothSides)
            {
                renderFlags |= RenderFlags.DoubleSided;
            }
            if (textured)
            {
                semiTrans = (tsb != 0);
            }
            if (semiTrans)
            {
                renderFlags |= RenderFlags.SemiTransparent;
                TMDHelper.ParseTSB(tsb, out _, out _, out mixtureRate);
            }

            var triangle1 = new Triangle
            {
                Vertices = new[] { vertex2, vertex1, vertex0 },
                Normals = light ? new[] { normal2, normal1, normal0 } : Triangle.EmptyNormals,
                Uv = textured ? new[] { uv2, uv1, uv0 } : Triangle.EmptyUv,
                Colors = new[] { color2, color1, color0 },
            };
            if (textured)
            {
                triangle1.TiledUv = new TiledUV(triangle1.Uv, 0f, 0f, 1f, 1f);
                triangle1.Uv = (Vector2[])triangle1.Uv.Clone();
            }
#if DEBUG
            triangle1.DebugData = new[] { $"0x{primitiveType:x02}", (quad ? "triangle1" : "triangle") };
#endif
            AddTriangle(triangle1, null, tPage, renderFlags, mixtureRate);

            if (quad)
            {
                var triangle2 = new Triangle
                {
                    Vertices = new[] { vertex2, vertex3, vertex1 },
                    Normals = light ? new[] { normal2, normal3, normal1 } : Triangle.EmptyNormals,
                    Uv = textured ? new[] { uv2, uv3, uv1 } : Triangle.EmptyUv,
                    Colors = new[] { color2, color3, color1 },
                };
                if (textured)
                {
                    triangle2.TiledUv = new TiledUV(triangle2.Uv, 0f, 0f, 1f, 1f);
                    triangle2.Uv = (Vector2[])triangle2.Uv.Clone();
                }
#if DEBUG
                triangle2.DebugData = new[] { $"0x{primitiveType:x02}", "triangle2" };
#endif
                AddTriangle(triangle2, null, tPage, renderFlags, mixtureRate);
            }

            return true;
        }

        private bool ReadSpritePrimitive(BinaryReader reader, uint version, bool d2m, byte flag, byte mode)
        {
            // Flag only used for untextered primitives
            //var semiTrans = ((flag >> 1) & 0x1) == 1;

            // GPU_COM_TF4SPR / TMD_P_FT4I (D2M_TMD_P_SP4I)
            // uv0-3
            // rgb0
            // v0
            // v1   (pad)
            // v2-3 (hw)
            // n0   (pad)
            var u0 = reader.ReadByte();
            var v0 = reader.ReadByte();
            uint tPage = 0;
            var textureIndex = reader.ReadUInt16();
            if (textureIndex - 1u < _textureHashCount)
            {
                tPage = _textureHashes[textureIndex - 1u];
            }
            else
            {
                var breakHere = 0;
            }

            var u1 = reader.ReadByte();
            var v1 = reader.ReadByte();
            var tsb = reader.ReadUInt16();

            var u2 = reader.ReadByte();
            var v2 = reader.ReadByte();
            var tile = reader.ReadByte();
            reader.ReadByte(); //pad

            var u3 = reader.ReadByte();
            var v3 = reader.ReadByte();
            reader.ReadUInt16(); //pad

            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var rgbMode = reader.ReadByte();

            var vertexIndex0 = reader.ReadUInt16();
            reader.ReadUInt16(); //pad (vertexIndex1)
            var scale = 1f;
            var width  = reader.ReadUInt16() / _scaleDivisor * scale; // (vertexIndex2)
            var height = reader.ReadUInt16() / _scaleDivisor * scale; // (vertexIndex3)
            // For some reason, normalIndex0 appears last in this one case only
            var normalIndex0 = reader.ReadUInt16();


            if (vertexIndex0 >= _vertexCount)
            {
                return false;
            }
            var center = _vertices[vertexIndex0];

            //if (normalIndex0 >= _normalCount)
            //{
            //    return false;
            //}
            //var normal = _normals[normalIndex0];

            // Remember that Y-up is negative, so height values are negated compared to what we set for UVs.
            // Note that these vertex coordinates also assume the default orientation of the view is (0, 0, -1).
            var vertex0 = center + new Vector3(-width / 2f,  height / 2f, 0f);
            var vertex1 = center + new Vector3( width / 2f,  height / 2f, 0f);
            var vertex2 = center + new Vector3(-width / 2f, -height / 2f, 0f);
            var vertex3 = center + new Vector3( width / 2f, -height / 2f, 0f);

            var color = new Color(r / 255f, g / 255f, b / 255f);

            var uv0 = GeomMath.ConvertUV(u0, v0) * UVConst;
            var uv1 = GeomMath.ConvertUV(u1, v1) * UVConst;
            var uv2 = GeomMath.ConvertUV(u2, v2) * UVConst;
            var uv3 = GeomMath.ConvertUV(u3, v3) * UVConst;

            var renderFlags = RenderFlags.Unlit | RenderFlags.Textured | RenderFlags.Sprite;
            var mixtureRate = MixtureRate.None;
            var semiTrans = (tsb != 0);
            if (semiTrans)
            {
                renderFlags |= RenderFlags.SemiTransparent;
                TMDHelper.ParseTSB(tsb, out _, out _, out mixtureRate);
            }


            var triangle1 = new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Normals = Triangle.EmptyNormals,
                Uv = new[] { uv0, uv1, uv2 },
                Colors = new[] { color, color, color },
            };
            triangle1.TiledUv = new TiledUV(triangle1.Uv, 0f, 0f, 1f, 1f);
            triangle1.Uv = (Vector2[])triangle1.Uv.Clone();
            AddTriangle(triangle1, center, tPage, renderFlags, mixtureRate);

            var triangle2 = new Triangle
            {
                Vertices = new[] { vertex1, vertex3, vertex2 },
                Normals = Triangle.EmptyNormals,
                Uv = new[] { uv1, uv3, uv2 },
                Colors = new[] { color, color, color },
            };
            triangle2.TiledUv = new TiledUV(triangle2.Uv, 0f, 0f, 1f, 1f);
            triangle2.Uv = (Vector2[])triangle2.Uv.Clone();
            AddTriangle(triangle2, center, tPage, renderFlags, mixtureRate);

            return true;
        }

        #endregion

        private void FlushModels(uint modelIndex, Coordinate coord)
        {
            var localMatrix = coord?.WorldMatrix ?? Matrix4.Identity;

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
                    TMDID = modelIndex + 1u,
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
                    TMDID = modelIndex + 1u,
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
                    UVConversion = TextureUVConversion.Absolute,// TextureSpace,
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


        private class PSIMesh
        {
            public PSIMesh Parent;
            public uint ModelIndex;
            public uint MeshTop;
            public uint[] SortTops;
            public ushort[] SortCounts;
            public float ScaleValue;
            public Vector3 Center;

            public Vector3Key[] ScaleKeys;
            public Vector3Key[] MoveKeys;
            public QuaternionKey[] RotateKeys;

            public Vector3 Scale       => ((ScaleKeys?.Length  ?? 0) > 0) ? ScaleKeys[0].Value  : Vector3.One;
            public Vector3 Translation => ((MoveKeys?.Length   ?? 0) > 0) ? MoveKeys[0].Value   : Vector3.Zero;
            public Quaternion Rotation => ((RotateKeys?.Length ?? 0) > 0) ? RotateKeys[0].Value.Inverted() : Quaternion.Identity;

            public Matrix4 LocalMatrix
            {
                get
                {
                    var scaleMatrix = Matrix4.CreateScale(Scale);// * ScaleValue);
                    var rotationMatrix = Matrix4.CreateFromQuaternion(Rotation);
                    var translationMatrix = Matrix4.CreateTranslation(Translation);// + Center);
                    return scaleMatrix * rotationMatrix * translationMatrix;
                }
            }
        }
        
        private struct Vector3Key
        {
            public Vector3 Value;
            public ushort Time;

            public Vector3Key(float x, float y, float z, ushort time)
            {
                Value = new Vector3(x, y, z);
                Time = time;
            }
        }
        private struct QuaternionKey
        {
            public Quaternion Value;
            public ushort Time;

            public QuaternionKey(float x, float y, float z, float w, ushort time)
            {
                Value = new Quaternion(x, y, z, w);
                Time = time;
            }
        }


        // CRC-32

        private static readonly uint[] _crcTable = InitCRCTable();

        private static uint[] InitCRCTable()
        {
            const uint Polynomial = 0x04c11db7;
            var crcTable = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                var crc = (i << 24);
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 0x80000000) != 0)
                    {
                        crc = (crc << 1) ^ Polynomial;
                    }
                    else
                    {
                        crc = (crc << 1);
                    }
                }
                crcTable[i] = crc;
            }
            return crcTable;
        }

        private static uint HashString(byte[] text)
        {
            // Unlike normal forward CRC-32, the crc accumulator is not inverted
            uint crc = 0;
            // Check string length and null terminator
            for (var i = 0; i < text.Length && text[i] != 0; i++)
            {
                var index = ((crc >> 24) ^ text[i]) & 0xff;
                crc = (crc << 8) ^ _crcTable[index];
            }
            return crc;
        }
    }
}
