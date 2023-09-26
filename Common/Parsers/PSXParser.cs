using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using PSXPrev.Common.Animator;
using PSXPrev.Common.Utils;

//Translated from:
//https://gist.github.com/iamgreaser/2a67f7473d9c48a70946018b73fa1e40

namespace PSXPrev.Common.Parsers
{
    // Neversoft's Big Guns engine: .PSX model and texture library format
    public class PSXParser : FileOffsetScanner
    {
        public const string FormatNameConst = "PSX";

        private static readonly ushort MaskMagenta = TexturePalette.FromComponents(248, 0, 248, false);

        // Used in-place of _lodDepths when we're not including all LOD levels
        private static readonly short[] LODMinDepthArray = new[] { short.MinValue };
        private const short LODMaxDepth = short.MaxValue; // Max depth away from the camera, a mesh with this depth will always be included
        private const ushort NoMeshIndex = ushort.MaxValue; // Used for LOD to state there is no lower-quality LOD mesh after this

        // _scaleDivisor is multiplied by 16 for files with HIER tagged chunk so that humanoids
        // match the map size. Translation divisor is only used for reading objects.
        private float _scaleDivisorTranslation = 1f;
        private float _scaleDivisor = 1f;
        private bool _useObjectIndexAsModelIndex; // Actor models use the mesh index to get the transform of the object
        private PSXObject[] _psxObjects;
        private Coordinate[] _coords;
        private PSXMesh[] _psxMeshes;
        private uint _objectCount;
        private uint _meshCount;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private uint[] _vertexJoints; // Joints for both vertices and normals, but always uses vertex index for lookup
        private uint _vertexCount;    // Total vertex count of all meshes
        private uint _normalCount;    // Total normal count of all meshes
        private uint[] _tempSections; // Temporary top/count/etc. tuples used for tagged chunks
        private readonly Color[] _gouraudPalette = new Color[256];
        private uint _gouraudCount; // Number of entries read into _gouraudPalette
        private uint[] _textureHashes;
        private uint _textureHashCount;
        private readonly Dictionary<uint, PSXClutData> _clutDatas = new Dictionary<uint, PSXClutData>();
        private readonly HashSet<short> _lodDepths = new HashSet<short>();
        private readonly Dictionary<uint, Tuple<uint, Vector3>> _attachableVertices = new Dictionary<uint, Tuple<uint, Vector3>>();
        private readonly Dictionary<uint, PSXSpriteVertexData> _spriteVertices = new Dictionary<uint, PSXSpriteVertexData>();
        private readonly Dictionary<RenderInfo, List<Triangle>> _groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();
        private readonly Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>> _groupedSprites = new Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>>();
        private readonly List<ModelEntity> _models = new List<ModelEntity>();
        // Debug:
#if DEBUG
        private readonly Dictionary<uint, List<string>> _objectDebugData = new Dictionary<uint, List<string>>();
#endif


        public PSXParser(EntityAddedAction entityAdded, TextureAddedAction textureAdded, AnimationAddedAction animationAdded)
            : base(entityAdded: entityAdded, textureAdded: textureAdded, animationAdded: animationAdded)
        {
        }

        public override string FormatName => FormatNameConst;

        protected override void Parse(BinaryReader reader)
        {
            var version = reader.ReadUInt16();
            if (version == 0x0003 || version == 0x0004 || version == 0x0006)
            {
                var magic = reader.ReadUInt16();
                if (magic == 0x0002)
                {
                    if (!ReadPSX(reader, version))
                    {
                        foreach (var texture in TextureResults)
                        {
                            texture.Dispose();
                        }
                        EntityResults.Clear();
                        TextureResults.Clear();
                        AnimationResults.Clear();
                    }
                }
            }
        }

        private bool ReadPSX(BinaryReader reader, ushort version)
        {
            // Reset state
            _scaleDivisor = _scaleDivisorTranslation = Settings.Instance.AdvancedPSXScaleDivisor;
            _vertexCount = 0;
            _normalCount = 0;
            _gouraudCount = 0;
            _useObjectIndexAsModelIndex = true; // Assumed true unless we encounter a blockmap tagged chunk
            _lodDepths.Clear();
            _attachableVertices.Clear();
#if DEBUG
            _objectDebugData.Clear();
#endif


            // Pointer to tagged chunks, mesh names, textures
            var metaTop = reader.ReadUInt32();
            // Allow zero object count in-case this is a texture library
            _objectCount = reader.ReadUInt32();
            if (_objectCount > Limits.MaxPSXObjectCount)
            {
                return false;
            }

            // Coords are assigned during ReadObject (we need an exact-sized array to assign to each Coordinate)
            _coords = _objectCount > 0 ? new Coordinate[_objectCount] : null;

            if (_psxObjects == null || _psxObjects.Length < _objectCount)
            {
                _psxObjects = new PSXObject[_objectCount];
            }
            for (uint i = 0; i < _objectCount; i++)
            {
                _psxObjects[i] = ReadObject(reader, version, i);
                if (_psxObjects[i].MeshIndex >= _objectCount)
                {
                    _useObjectIndexAsModelIndex = false; // Not expected, but we can't use this if we'd go out of bounds
                }
            }


            // Allow zero mesh count in-case this is a texture library
            _meshCount = reader.ReadUInt32();
            if ((_meshCount == 0) != (_objectCount == 0) || _meshCount > Limits.MaxPSXObjectCount)
            {
                return false; // Meshes or objects are zero, but not both. Or mesh count is too high
            }
            if (_psxMeshes == null || _psxMeshes.Length < _meshCount)
            {
                _psxMeshes = new PSXMesh[_meshCount];
            }

            // Debug:
            /*if (_objectCount > 0)
            {
                var hasDiffs = false;
                var hasHigher = false;
                for (uint i = 0; i < _objectCount; i++)
                {
                    hasDiffs |= (_psxObjects[i].MeshIndex != i);
                    hasHigher |= (_psxObjects[i].MeshIndex >= _objectCount);
                }
                if (!hasDiffs)
                {
                    //return false;
                }
            }*/


            // Read gouraud palette/vertex divisor from tagged chunks, and texture hashes before reading meshes
            var meshesStartPosition = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + metaTop, SeekOrigin.Begin);

            if (!ReadTaggedChunks(reader, version))
            {
                return false;
            }

            // Hashes of mesh names, not important
            var meshHashes = Program.Debug ? new string[_meshCount] : null;
            for (uint i = 0; i < _meshCount; i++)
            {
                var meshHash = reader.ReadUInt32();
                if (meshHashes != null)
                {
                    meshHashes[i] = $"0x{meshHash:x08}";
                }
            }
            if (meshHashes != null && _meshCount > 0)
            {
                Program.Logger.WriteLine("Mesh Hashes: " + string.Join(", ", meshHashes));
            }

            if (!ReadTextures(reader, version))
            {
                return false;
            }
            reader.BaseStream.Seek(meshesStartPosition, SeekOrigin.Begin);


            // We need to go through and read each mesh's attachable vertices first,
            // since we need to know joint vertex positions ahead of time
            for (uint i = 0; i < _meshCount; i++)
            {
                var meshTop = reader.ReadUInt32();
                var meshPosition = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + meshTop, SeekOrigin.Begin);
                if (!ReadMesh(reader, version, i))
                {
                    return false;
                }
                reader.BaseStream.Seek(meshPosition, SeekOrigin.Begin);
            }

            // Now that we have the joint vertex positions, we can read through each mesh and build lists of triangles
            // PSX format features swappable LOD meshes for each object,
            // by default we only use the first-listed mesh for highest quality.
            IEnumerable<short> lodDepths = LODMinDepthArray; // Single depth that won't use LOD (since no LOD can be less than this)
            if (Settings.Instance.AdvancedPSXIncludeLODLevels && _lodDepths.Count > 1)
            {
                lodDepths = _lodDepths.OrderBy(d => d); // Order quality from highest to lowest
            }

            foreach (var lodDepth in lodDepths)
            {
                // Reset model state
                _groupedTriangles.Clear();
                _groupedSprites.Clear();
                _models.Clear();

                for (uint i = 0; i < _objectCount; i++)
                {
                    var psxObject = _psxObjects[i];
                    var modelIndex = _useObjectIndexAsModelIndex ? i : psxObject.MeshIndex;
                    var psxMesh = _psxMeshes[modelIndex];

                    // Find mesh index for the given LOD depth
                    uint lodLevel = 0; // Used to sanity-check recursion
                    while (modelIndex != NoMeshIndex && psxMesh.LODDepth < lodDepth)
                    {
                        if (++lodLevel > Limits.MaxPSXLODLevel)
                        {
                            return false;
                        }
                        modelIndex = psxMesh.LODNextMeshIndex;
                        if (modelIndex != NoMeshIndex)
                        {
                            psxMesh = _psxMeshes[modelIndex];
                        }
                    }
                    if (modelIndex == NoMeshIndex)
                    {
                        continue; // Mesh is hidden at this LOD depth
                    }

                    reader.BaseStream.Seek(_offset + psxMesh.MeshTop, SeekOrigin.Begin);
                    if (!ReadMeshPrimitives(reader, version, i, modelIndex))
                    {
                        return false;
                    }
                }


                //if (_models.Count > 0)
                if (_models.Any(m => m.Triangles.Length > 0))
                {
                    var rootEntity = new RootEntity();
                    rootEntity.ChildEntities = _models.ToArray();
                    // We can use the original instance of coords for the first entity, but clone for all remaining entities.
                    rootEntity.Coords = EntityResults.Count == 0 ? _coords : Coordinate.CloneCoordiantes(_coords);
                    //rootEntity.OwnedTextures.AddRange(TextureResults); // todo: need to change how owned textures are handled
                    rootEntity.OwnedAnimations.AddRange(AnimationResults);
                    if (EntityResults.Count == 0)
                    {
                        // Set the owner as the highest-quality LOD model
                        foreach (var texture in TextureResults)
                        {
                            texture.OwnerEntity = rootEntity;
                        }
                        foreach (var animation in AnimationResults)
                        {
                            animation.OwnerEntity = rootEntity;
                        }
                    }
                    // PrepareJoints must be called before ComputeBounds
                    rootEntity.PrepareJoints(_attachableVertices.Count > 0);
                    rootEntity.ComputeBounds();
                    EntityResults.Add(rootEntity);
                }
            }

            return true;
        }

        private bool ReadTaggedChunks(BinaryReader reader, ushort version)
        {
            const uint TagStop = 0xffffffff;
            // Gouraud palette
            const uint TagRGBs = 'R' | (((uint)'G') << 8) | (((uint)'B') << 16) | (((uint)'s') << 24);
            // Hierarchy
            const uint TagHIER = 'H' | (((uint)'I') << 8) | (((uint)'E') << 16) | (((uint)'R') << 24);
            // Unknown
            const uint TagChnk = 'C' | (((uint)'h') << 8) | (((uint)'n') << 16) | (((uint)'k') << 24);
            const uint TagUnknown6  = 0x00000006; //     Very small
            const uint TagUnknown7  = 0x00000007; //     Medium
            const uint TagBlockMap  = 0x0000000a; //     Very large (level physics)
            const uint TagUnknown2C = 0x0000002c; // "," Very large (positional/animation information?)
            const uint TagUnknown2A = 0x0000002a; // "*" Very small to large (positional/animation information?)
            const uint TagBits      = 0x00000045; // "E" Medium (Used for bits.psx, aka fonts and common symbols/images/hud/etc.)


            // Start by checking chunks for parser behavior changes, then parse the chunks for real after that
            // This is needed since the HIER tagged chunk changes the scale divisor that's used for animations
            var chunksStartPosition = reader.BaseStream.Position;
            uint chunkCount = 0;
            var chunkTag = reader.ReadUInt32();
            while (chunkTag != TagStop)
            {
                if (++chunkCount > Limits.MaxPSXTaggedChunks)
                {
                    return false;
                }
                var chunkLength = reader.ReadUInt32();

                if (Program.Debug)
                {
                    var tagStr = new StringBuilder(4);
                    for (var i = 0; i < 4; i++)
                    {
                        var c = (char)(byte)(chunkTag >> (i * 8));
                        tagStr.Append(char.IsControl(c) ? ' ' : c);
                    }
                    Program.Logger.WriteLine($"chunk[{chunkCount-1}]: \"{tagStr}\" 0x{chunkTag:x08} {chunkLength} : {_fileTitle}_{_offset:X} @ 0x{reader.BaseStream.Position:x}");
                }

                switch (chunkTag)
                {
                    case TagHIER:
                        // Models with hierarchy are scaled down by x16.
                        _scaleDivisor = _scaleDivisorTranslation * 16f;
                        break;
                    case TagBlockMap:
                        // This probably isn't the correct way to detect this, but it works for all games tested against:
                        // (Apocalypse, Spider Man 1-2, Tony Hawk's Pro Skater 1-4)
                        _useObjectIndexAsModelIndex = false; // Not an actor model
                        break;
                }

                reader.BaseStream.Seek(reader.BaseStream.Position + chunkLength, SeekOrigin.Begin);
                chunkTag = reader.ReadUInt32();
            }

            reader.BaseStream.Seek(chunksStartPosition, SeekOrigin.Begin);
            chunkTag = reader.ReadUInt32();
            while (chunkTag != TagStop)
            {
                var chunkLength = reader.ReadUInt32();
                var chunkPosition = reader.BaseStream.Position;

                var result = true;
                switch (chunkTag)
                {
                    case TagRGBs:
                        result = ReadTaggedChunkRGBs(reader, chunkLength);
                        break;
                    case TagHIER:
                        result = ReadTaggedChunkHIER(reader, chunkLength);
                        break;
                    case TagUnknown2C:
                        result = ReadTaggedChunk0x2C(reader, version, chunkLength);
                        break;
                    case TagUnknown2A:
                        result = ReadTaggedChunk0x2A(reader, version, chunkLength);
                        break;
                }
                if (!result)
                {
                    return false;
                }

                reader.BaseStream.Seek(chunkPosition + chunkLength, SeekOrigin.Begin);
                chunkTag = reader.ReadUInt32();
            }

            return true;
        }

        private bool ReadTaggedChunkRGBs(BinaryReader reader, uint chunkLength)
        {
            // In order to support gouraud color without changing the face structure, RGBMode are changed into
            // indices that look up a color in the table defined in this chunk.
            // There is some weird behavior that hasn't quite been nailed down yet. Mainly, if the color black repeats itself,
            // then the surface has some special color properties (maybe reflectivity/refraction/etc.).
            // For now, the color is just being changed to gray so the face's texture is visible, but this isn't correct.

            // Palette size can be smaller than 256
            _gouraudCount = chunkLength / 4;
            if (_gouraudCount > _gouraudPalette.Length)
            {
                // Error out if palette size is too large, or should we ignore remaining colors...?
                return false;
            }
            var specialStarted = false;
            for (uint i = 0; i < _gouraudCount; i++)
            {
                var r = reader.ReadByte();
                var g = reader.ReadByte();
                var b = reader.ReadByte();
                var pad = reader.ReadByte(); //pad
                if (pad != 0)
                {
                    var breakHere = 0;
                }
                Color color;
                if (r == 0 && g == 0 && b == 0)
                {
                    if (!specialStarted)
                    {
                        specialStarted = true;
                        color = Color.Black;
                    }
                    else
                    {
                        // This is NOT correct, however it's better than using black
                        color = Color.Grey;
                    }
                    //color = new Color(i / 255f, i / 255f, i / 255f);
                }
                else
                {
                    color = new Color(r / 255f, g / 255f, b / 255f);
                }
                _gouraudPalette[i] = color;
            }
            return true;
        }

        private bool ReadTaggedChunkHIER(BinaryReader reader, uint chunkLength)
        {
            // Can't rely on chunk length, since it's padded to 4 bytes
            var count = Math.Min(chunkLength / 2, _objectCount);
            for (uint i = 0; i < count; i++)
            {
                var parentIndex = reader.ReadUInt16();
                if (parentIndex != i)
                {
                    _coords[i].ParentID = parentIndex;
                }
            }
            if (Coordinate.FindCircularReferences(_coords))
            {
                return false;
            }
            return true;
        }

#if DEBUG
        private class SectionSizes
        {
            public class Range1d
            {
                public double Min = double.NaN;
                public double Max = double.NaN;
                public void Update(double value)
                {
                    Min = (!double.IsNaN(Min) && Min < value) ? Min : value;
                    Max = (!double.IsNaN(Max) && Max > value) ? Max : value;
                }
                public override string ToString() => $"{Min}, {Max}";
            }

            public class Range1u
            {
                public uint Min = uint.MaxValue;
                public uint Max = uint.MinValue;
                public void Update(uint value)
                {
                    Min = Math.Min(Min, value);
                    Max = Math.Max(Max, value);
                }
                public override string ToString() => $"{Min}, {Max}";
            }

            public Range1u Count = new Range1u();
            public Range1u CountMesh = new Range1u();
            public Range1u CountObject = new Range1u();
            public Range1u Length = new Range1u();

            public Range1d SizeCount = new Range1d();
            public Range1d SizeCountXMesh = new Range1d();
            public Range1d SizeCountXObject = new Range1d();
            public Range1d SizeMesh = new Range1d();
            public Range1d SizeObject = new Range1d();

            public readonly HashSet<string> FileNames = new HashSet<string>();
            public uint Occurrences = 0;

            public void Update(string fileTitle, uint count, uint objectCount, uint meshCount, uint length)
            {
                Count.Update(count);
                CountMesh.Update(meshCount);
                CountObject.Update(objectCount);
                Length.Update(length);

                if (count > 0)
                {
                    SizeCount.Update((double)length / count);
                }
                if ((count * meshCount) > 0)
                {
                    SizeCountXMesh.Update((double)length / (count * meshCount));
                }
                if ((count * objectCount) > 0)
                {
                    SizeCountXObject.Update((double)length / (count * objectCount));
                }
                if (meshCount > 0)
                {
                    SizeMesh.Update((double)length / meshCount);
                }
                if (objectCount > 0)
                {
                    SizeObject.Update((double)length / objectCount);
                }

                FileNames.Add(fileTitle);
                Occurrences++;
            }

            public override string ToString()
            {
                return $"({Occurrences}): {SizeCount.Min}, {SizeObject.Min}, {SizeCountXObject.Min}";
            }
        }

        private static readonly Dictionary<ushort, SectionSizes> ChunkSectionSizes = new Dictionary<ushort, SectionSizes>();
#endif

        // Supposedly an animation format
        private bool ReadTaggedChunk0x2C(BinaryReader reader, ushort version, uint chunkLength)
        {
            var chunkPosition = reader.BaseStream.Position;
            var sectionCount = reader.ReadUInt32();
            if (sectionCount > Limits.MaxPSXAnimations)
            {
                return false;
            }
            if (_tempSections == null || _tempSections.Length < sectionCount * 2)
            {
                _tempSections = new uint[sectionCount * 2];
            }
            for (uint i = 0; i < sectionCount; i++) // top/count(?) pairs
            {
                _tempSections[(i * 2) + 0] = reader.ReadUInt32();
                _tempSections[(i * 2) + 1] = reader.ReadUInt32();
            }
            for (uint i = 0; i < sectionCount; i++)
            {
                var top   = _tempSections[(i * 2) + 0];
                var count = _tempSections[(i * 2) + 1]; // This number is small, usually between 1 and 90
                // count * _objectCount can be larger than the length of a section
                var length = (i + 1 < sectionCount ? _tempSections[((i + 1) * 2) + 0] : chunkLength) - top;
                if (count > Limits.MaxPSXAnimationFrames)
                {
                    return false;
                }
                if (count > ushort.MaxValue)
                {
                    var breakHere = 0;
                }

                reader.BaseStream.Seek(chunkPosition + top, SeekOrigin.Begin);
#if DEBUG
                var sectionID = (ushort)((0x2C << 8));
                if (!ChunkSectionSizes.TryGetValue(sectionID, out var sectionSizes))
                {
                    sectionSizes = new SectionSizes();
                    ChunkSectionSizes.Add(sectionID, sectionSizes);
                }
                sectionSizes.Update(_fileTitle, count, _objectCount, _meshCount, length);
#endif

                if (count > length)
                {
                    var breakHere = 0;
                }
                if (_objectCount > length)
                {
                    var breakHere = 0;
                }
            }
            return true;
        }

        // Various animation formats (based on subtypes)
        private bool ReadTaggedChunk0x2A(BinaryReader reader, ushort version, uint chunkLength)
        {
            var chunkPosition = reader.BaseStream.Position;
            var sectionCount = reader.ReadUInt32();
            if (sectionCount > Limits.MaxPSXAnimations)
            {
                return false;
            }
            if (_tempSections == null || _tempSections.Length < sectionCount * 3)
            {
                _tempSections = new uint[sectionCount * 3];
            }
            for (uint i = 0; i < sectionCount; i++) // top/count(?)/type(?) tuples
            {
                _tempSections[(i * 3) + 0] = reader.ReadUInt32();
                _tempSections[(i * 3) + 1] = reader.ReadUInt16();
                _tempSections[(i * 3) + 2] = reader.ReadUInt16();
            }
            for (uint i = 0; i < sectionCount; i++)
            {
                var top     = _tempSections[(i * 3) + 0];
                var count   = _tempSections[(i * 3) + 1]; // Seems to be number of key frames (can be 1)
                var subtype = _tempSections[(i * 3) + 2]; // Describes the format of the animation data
                var length = (i + 1 < sectionCount ? _tempSections[((i + 1) * 3) + 0] : chunkLength) - top;
                if (count > Limits.MaxPSXAnimationFrames)
                {
                    return false;
                }

                reader.BaseStream.Seek(chunkPosition + top, SeekOrigin.Begin);
#if DEBUG
                var sectionID = (ushort)((0x2A << 8) | subtype);
                if (!ChunkSectionSizes.TryGetValue(sectionID, out var sectionSizes))
                {
                    sectionSizes = new SectionSizes();
                    ChunkSectionSizes.Add(sectionID, sectionSizes);
                }
                sectionSizes.Update(_fileTitle, count, _objectCount, _meshCount, length);
#endif

                switch (subtype)
                {
                    case 0x0: // Matrix (Matrix3 + Translation) key frames
                        // Always (count * _objectCount * 24) bytes in length
                        if (!ReadTaggedChunk0x2ASubtype0(reader, version, count, length))
                        {
                            return false;
                        }
                        break;
                    case 0x1:
                        // Always at least (count * _objectCount * 12) bytes in length
                        break; // Apocalypse
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break; // Spider Man 1 (henchman.psx)
                    case 0x5:
                        break; // Spider Man 1
                    case 0x8:
                        break; // Spider Man 2
                    case 0x9:
                        break; // Apocalypse (demon.psx, prisoner.psx, punk.psx)
                    case 0xd:
                        break; // Spider Man 2
                    case 0x12:
                        break; // Spider Man 2
                    case 0x62:
                        break; // Spider Man 1 (chopper.psx)
                    default:
                        break;
                }
            }
            return true;
        }

        private bool ReadTaggedChunk0x2ASubtype0(BinaryReader reader, ushort version, uint count, uint length)
        {
            //if ((count * _objectCount * 24) > length)
            //{
            //    return false; // Section is too small
            //}
            // Stricter sanity-check
            //if (length / (_objectCount * count) != 24 || length % (_objectCount * count) != 0)
            //{
            //    return false;
            //}
            var animation = new Animation();
            var animationObjects = new Dictionary<uint, AnimationObject>();
            for (uint i = 0; i < count; i++)
            {
                for (uint o = 0; o < _objectCount; o++)
                {
                    var objectId = o + 1u;
                    if (!animationObjects.TryGetValue(objectId, out var animationObject))
                    {
                        animationObject = new AnimationObject
                        {
                            Animation = animation,
                            ID = objectId,
                        };
                        animationObject.TMDID.Add(objectId);
                        animationObjects.Add(objectId, animationObject);
                    }

                    var m00 = reader.ReadInt16() / 4096f;
                    var m01 = reader.ReadInt16() / 4096f;
                    var m02 = reader.ReadInt16() / 4096f;
                    var m10 = reader.ReadInt16() / 4096f;
                    var m11 = reader.ReadInt16() / 4096f;
                    var m12 = reader.ReadInt16() / 4096f;
                    var m20 = reader.ReadInt16() / 4096f;
                    var m21 = reader.ReadInt16() / 4096f;
                    var m22 = reader.ReadInt16() / 4096f;

                    var tx = reader.ReadInt16() / _scaleDivisor;
                    var ty = reader.ReadInt16() / _scaleDivisor;
                    var tz = reader.ReadInt16() / _scaleDivisor;

                    var matrix3 = new Matrix3(
                        m00, m10, m20,
                        m01, m11, m21,
                        m02, m12, m22);

                    var translation = new Vector3(tx, ty, tz);
                    var rotation = matrix3.ExtractRotationSafe();
                    var scale = matrix3.ExtractScale();

                    if (i > 0 && animationObject.AnimationFrames.TryGetValue(i - 1u, out var lastAnimationFrame))
                    {
                        lastAnimationFrame.FinalTranslation = translation;
                        lastAnimationFrame.FinalRotation = rotation;
                        lastAnimationFrame.FinalScale = scale;
                    }

                    if (i + 1 < count || count == 1)
                    {
                        var frameTime = i;
                        var animationFrame = new AnimationFrame
                        {
                            FrameTime = frameTime,
                            FrameDuration = 1,
                            AnimationObject = animationObject,
                        };
                        animationObject.AnimationFrames.Add(frameTime, animationFrame);

                        animationFrame.TranslationType = InterpolationType.Linear;
                        animationFrame.RotationType = InterpolationType.Linear;
                        animationFrame.ScaleType = InterpolationType.Linear;
                        animationFrame.Translation = translation;
                        animationFrame.Rotation = rotation;
                        animationFrame.Scale = scale;

                        if (count == 1)
                        {
                            animationFrame.FinalTranslation = translation;
                            animationFrame.FinalRotation = rotation;
                            animationFrame.FinalScale = scale;
                        }
                    }
                }
            }

            if (animationObjects.Count > 0 && count > 0)
            {
                // Apocalypse/Spider Man seems to use 25-30FPS,
                // while THPS uses 1FPS (but it only has one identical animation used by all characters).
                animation.AnimationType = AnimationType.PSI;
                animation.FPS = 25f;
                animation.AssignObjects(animationObjects, true, false);
                AnimationResults.Add(animation);
            }
            return true;
        }

        private bool ReadTextures(BinaryReader reader, ushort version)
        {
            // Reset textures state
            _clutDatas.Clear();

            _textureHashCount = reader.ReadUInt32();
            if (_textureHashCount > Limits.MaxPSXTextureHashes)
            {
                return false;
            }
            if (_textureHashes == null || _textureHashes.Length < _textureHashCount)
            {
                _textureHashes = new uint[_textureHashCount];
            }
            for (uint i = 0; i < _textureHashCount; i++)
            {
                _textureHashes[i] = reader.ReadUInt32();
            }

            var clut16Count = reader.ReadUInt32();
            if (clut16Count > Limits.MaxPSXTextures)
            {
                return false;
            }
            for (uint i = 0; i < clut16Count; i++)
            {
                var clutHash = reader.ReadUInt32();
                _clutDatas[clutHash] = new PSXClutData
                {
                    ClutTop = (uint)(reader.BaseStream.Position - _offset),
                    IsClut256 = false,
                };
                // Skip clut data for now
                reader.BaseStream.Seek(2 * 16, SeekOrigin.Current);
            }

            var clut256Count = reader.ReadUInt32();
            if (clut256Count > Limits.MaxPSXTextures)
            {
                return false;
            }
            for (uint i = 0; i < clut256Count; i++)
            {
                var clutHash = reader.ReadUInt32();
                _clutDatas[clutHash] = new PSXClutData
                {
                    ClutTop = (uint)(reader.BaseStream.Position - _offset),
                    IsClut256 = true,
                };
                // Skip clut data for now
                reader.BaseStream.Seek(2 * 256, SeekOrigin.Current);
            }

            var textureDataCount = reader.ReadUInt32();
            if (textureDataCount > Limits.MaxPSXTextures)
            {
                return false;
            }
            for (uint i = 0; i < textureDataCount; i++)
            {
                var textureDataTop = reader.ReadUInt32();
                var textureDataPosition = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + textureDataTop, SeekOrigin.Begin);
                if (!ReadTextureData(reader, version))
                {
                    return false;
                }
                reader.BaseStream.Seek(textureDataPosition, SeekOrigin.Begin);
            }

            return true;
        }

        private bool ReadTextureData(BinaryReader reader, ushort version)
        {
            var maskMode = reader.ReadUInt32();
            if (maskMode > 1)
            {
                var breakHere = 0;
            }
            var paletteSize = reader.ReadUInt32();
            var clutHash = reader.ReadUInt32();
            var textureIndex = reader.ReadUInt32();
            if (textureIndex >= _textureHashCount)
            {
                return false;
            }
            var width  = reader.ReadUInt16();
            var height = reader.ReadUInt16();

            int bpp;
            uint pixelFormat = 0, size = 0;
            switch (paletteSize)
            {
                case 16:
                    bpp = 4;
                    break;
                case 256:
                    bpp = 8;
                    break;
                case 0x10000:
                    bpp = 16;
                    // todo: Magenta masking only assumed for this bit depth
                    pixelFormat = reader.ReadUInt32();
                    size        = reader.ReadUInt32();
                    break;

                default:
                    return false;
            }

            ushort[][] palettes = null, origPalettes = null;
            bool? hasSemiTransparency = null;
            Func<ushort, ushort> maskPixel16 = null;
            if (paletteSize <= 256)
            {
                var imagePosition = reader.BaseStream.Position;

                if (!_clutDatas.TryGetValue(clutHash, out var clutData))
                {
                    return false;
                }
                if ((paletteSize == 256) != clutData.IsClut256)
                {
                    return false;
                }
                var clutWidth = (ushort)paletteSize;
                reader.BaseStream.Seek(_offset + clutData.ClutTop, SeekOrigin.Begin);
                palettes = TIMParser.ReadPalettes(reader, bpp, clutWidth, 1, out hasSemiTransparency, true, false);
                if (palettes == null)
                {
                    return false;
                }

                origPalettes = MaskPalette(maskMode == 0, palettes, ref hasSemiTransparency);

                reader.BaseStream.Seek(imagePosition, SeekOrigin.Begin);
            }
            else
            {
                maskPixel16 = MaskPixel;
            }

            var texture = TIMParser.ReadTexturePacked(reader, bpp, width, height, 0, palettes, hasSemiTransparency, true, maskPixel16);
            if (texture == null)
            {
                return false;
            }
            texture.OriginalPalettes = origPalettes;
            texture.LookupID = _textureHashes[textureIndex];
#if DEBUG
            texture.DebugData = new[] { $"{maskMode}" };
#endif
            TextureResults.Add(texture);
            return true;
        }

        private static ushort MaskPixel(ushort color)
        {
            if (TexturePalette.Equals(color, MaskMagenta, true))
            {
                color = TexturePalette.Transparent;
            }
            else if (color == TexturePalette.Transparent)
            {
                color = TexturePalette.SetStp(color, true);
            }
            // todo: What about stp bits for other colors? Is it affected by maskMode?
            return color;
        }

        private static ushort[][] MaskPalette(bool blackMaskMode, ushort[][] palettes, ref bool? hasSemiTransparency)
        {
            // Stp bits and masking:
            // So this format decided it needed some overly-complex rules to handle turning the stp bit on,
            // because the developers couldn't be bothered to do that when saving the image data.
            //
            // 1. The color magenta rgb(248,0,248) is ALWAYS masked. It's assumed that stp bit is ignored when checking
            //    for magenta. It's also assumed that every appearance is masked.
            //
            // 2. The color transparent is never masked, except for the one exception in rule #5.
            //
            // 3. If magenta is in the palette, then all entries after the first appearance will have their stp bit set.
            //
            // 4. If magenta is not in the palette and !blackMaskMode, then ALL entries have their stp bit set.
            //
            // 5. If magenta is not in the palette and blackMaskMode, then stp bits will be set after the first appearance
            //    of transparent. However, index 255 will not have its stp bit set if the color is transparent.
            var palette = palettes[0];
            var paletteSize = palette.Length;
            ushort[][] origPalettes = null;
            var hasMagenta = false;
            for (var c = 0; c < paletteSize; c++)
            {
                if (TexturePalette.Equals(palette[c], MaskMagenta, true))
                {
                    hasMagenta = true;
                    break;
                }
            }

            // All stp bits are set if magenta is missing and !blackMaskMode.
            var stpStarted = !blackMaskMode && !hasMagenta;
            for (var c = 0; c < paletteSize; c++)
            {
                var color = palette[c];
                var newColor = color;
                if (TexturePalette.Equals(color, MaskMagenta, true))
                {
                    // Magenta marks the start of stp bit setting
                    newColor = TexturePalette.Transparent;
                    stpStarted = true;
                }
                else if (color == TexturePalette.Transparent)
                {
                    // Transparent can mark the start when blackMaskMode and there's no magenta
                    if (blackMaskMode && !hasMagenta)
                    {
                        stpStarted = true;
                    }
                    // Transparent always has stp bit set, with one exception:
                    // When blackMaskMode and !hasMagenta for last color in palette (but only for clut256)
                    if (!blackMaskMode || hasMagenta || c != 255)
                    {
                        newColor = TexturePalette.SetStp(color, true);
                        hasSemiTransparency = true;
                    }
                }
                else if (stpStarted)
                {
                    // Everything after the masking color has the stp bit set
                    newColor = TexturePalette.SetStp(color, true);
                    hasSemiTransparency = true;
                }

                if (color != newColor)
                {
                    if (origPalettes == null)
                    {
                        origPalettes = new ushort[][] { (ushort[])palette.Clone() };
                    }
                    palette[c] = newColor;
                }
            }

            return origPalettes ?? palettes;
        }

        private PSXObject ReadObject(BinaryReader reader, ushort version, uint objectIndex)
        {
            var objectFlags = reader.ReadUInt32();
            // Translation is in absolute world coordinates, not coordinates relative to hierarchy.
            var x = reader.ReadInt32() / (4096f * _scaleDivisorTranslation);
            var y = reader.ReadInt32() / (4096f * _scaleDivisorTranslation);
            var z = reader.ReadInt32() / (4096f * _scaleDivisorTranslation);
            var translation = new Vector3(x, y, z);
            var unk1 = reader.ReadUInt32();
            var unk2 = reader.ReadUInt16();
            // Index of mesh in the meshes list. If multiple objects have the same mesh,
            // then multiple meshes are created, which theoretically can be used for
            // having the same props in different positions.
            var meshIndex = reader.ReadUInt16();
            // When non-zero, observed as multiples of 2, tx and/or ty can be non-zero.
            var tx = reader.ReadInt16();
            var ty = reader.ReadInt16();
            var unk3 = reader.ReadUInt32();
            // Generally every object uses the same palette top, not sure what this palette is used for though.
            // This can also be null.
            var paletteTop = reader.ReadUInt32();

            _coords[objectIndex] = new Coordinate
            {
                Coords = _coords,
                ID = objectIndex,
                IsAbsolute = true, // Transforms are absolute, parents don't affect WorldMatrix
                OriginalLocalMatrix = Matrix4.CreateTranslation(translation),
                OriginalTranslation = translation,
            };

#if DEBUG
            _objectDebugData[objectIndex] = new List<string>
            {
                $"objectFlags: 0x{objectFlags:x08}",
                $"unk1: 0x{unk1:x08}",
                $"unk2: 0x{unk2:x04}",
                $"unk3: 0x{unk3:x08}",
                $"tx,ty: {tx}, {ty}",
                $"paletteTop: 0x{paletteTop:x08}",
            };
#endif
            return new PSXObject
            {
                Translation = translation,
                MeshIndex = meshIndex,
            };
        }

        private bool ReadMesh(BinaryReader reader, ushort version, uint modelIndex)
        {
            var meshTop = (uint)(reader.BaseStream.Position - _offset);

            var meshFlags   = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
            var vertexCount = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
            var normalCount = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
            var faceCount   = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();

            if (vertexCount > Limits.MaxPSXVertices || normalCount > Limits.MaxPSXVertices)
            {
                return false;
            }
            if (faceCount > Limits.MaxPSXFaces)
            {
                return false;
            }

            // Skip radius, xyzMaxMin
            reader.BaseStream.Seek(16, SeekOrigin.Current);
            //reader.BaseStream.Seek(version == 0x04 ? 18 : 20, SeekOrigin.Current); // Includes faceCount in skip

            var lodDepth = LODMaxDepth;
            var lodNextMeshIndex = NoMeshIndex;
            if (version == 0x04)
            {
                lodDepth = reader.ReadInt16();
                lodNextMeshIndex = reader.ReadUInt16();
            }

            var modelIsJoint = false;

            // We need to read through vertices twice, first to load all attachables, and the second time
            // to load vertices for the current mesh. Tests were done to compare load times of storing all
            // vertices at once, and loading vertices per-mesh is faster by a decent amount.
            for (uint j = 0; j < vertexCount; j++)
            {
                // Vertex coding:
                // It's possible that type is actually flags, since all encountered types are powers of 2.
                // However most types wouldn't make sense to use together.
                // type =  0: Normal vertex
                // type =  1: Attachable vertex, file-wide counter as the index
                // type =  2: Attached vertex,   Y is the index of the attachable (not vertex index)
                //                               X and Z are zero
                // type =  4: Unknown            X, Y, and Z are not indexes, and may be negative
                // type = 16: Sprite vertex,     X is a byte offset to the sprite center vertex (always 0,8,16,24)
                //                               Y is a byte offset to a vertex (usually 16,16,0,0, rarely 24,24,8,8)
                //                               Z value is the width of the sprite with some awkward behavior, not fully understood
                var x = reader.ReadInt16();
                var y = reader.ReadInt16();
                var z = reader.ReadInt16();
                var type = reader.ReadUInt16();

                if (type == 1)
                {
                    var vertex = new Vector3(x / _scaleDivisor, y / _scaleDivisor, z / _scaleDivisor);

                    // Note that this isn't a Joint ID, just a .PSX attachment index ID.
                    var attachmentIndex = (uint)_attachableVertices.Count;
                    var modelJointID = modelIndex + 1u;
                    _attachableVertices.Add(attachmentIndex, new Tuple<uint, Vector3>(modelJointID, vertex));
                    modelIsJoint = true;
                }
            }

            _psxMeshes[modelIndex] = new PSXMesh
            {
                MeshTop = meshTop,
                VertexStart = _vertexCount,
                NormalStart = _normalCount,
                IsJoint = modelIsJoint,
                LODDepth = lodDepth,
                LODNextMeshIndex = lodNextMeshIndex,
            };
            _vertexCount += vertexCount;
            _normalCount += normalCount;
            if (Settings.Instance.AdvancedPSXIncludeLODLevels)
            {
                _lodDepths.Add(lodDepth);
            }

            return true;
        }

        private bool ReadMeshPrimitives(BinaryReader reader, ushort version, uint objectIndex, uint modelIndex)
        {
            // Reset mesh state
            _spriteVertices.Clear();

#if DEBUG
            if (!_objectDebugData.TryGetValue(objectIndex, out var objectDebugData))
            {
                objectDebugData = new List<string>();
                _objectDebugData.Add(objectIndex, objectDebugData);
            }
#endif
            var meshFlags   = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32(); // Always 0x8 or 0xa
            var vertexCount = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
            var normalCount = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
            var faceCount   = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
#if DEBUG
            objectDebugData?.Add($"meshFlags: 0x{meshFlags:x08}");
#endif

            // We already sanity-checked vertexCount, normalCount, and faceCount in ReadMesh

            // Read bounds information
            var radius = reader.ReadUInt32() / (4096f * _scaleDivisor);
            var xMax = reader.ReadInt16() / _scaleDivisor;
            var xMin = reader.ReadInt16() / _scaleDivisor;
            var yMax = reader.ReadInt16() / _scaleDivisor;
            var yMin = reader.ReadInt16() / _scaleDivisor;
            var zMax = reader.ReadInt16() / _scaleDivisor;
            var zMin = reader.ReadInt16() / _scaleDivisor;
            if (version == 0x04)
            {
                var lodDepth = reader.ReadInt16(); // Just a guess, maybe the range at which this LOD level shows
                var lodNextMeshIndex = reader.ReadUInt16();
#if DEBUG
                var lodDebugDepth = lodDepth != LODMaxDepth ? lodDepth.ToString() : "max";
                var lodDebugNextMeshIndex = lodNextMeshIndex != NoMeshIndex ? lodNextMeshIndex.ToString() : "none";
                objectDebugData?.Add($"lodMesh: {lodDebugNextMeshIndex}, {lodDebugDepth}");
#endif
            }


            // Read mesh vertices (and lookup attached vertices, now that attachables are all loaded)
            if (_vertices == null || _vertices.Length < vertexCount)
            {
                _vertices = new Vector3[vertexCount];
                _vertexJoints = new uint[vertexCount];
            }
            for (uint j = 0; j < vertexCount; j++)
            {
                // Vertex coding:
                // It's possible that type is actually flags, since all encountered types are powers of 2.
                // However most types wouldn't make sense to use together.
                // type =  0: Normal vertex
                // type =  1: Attachable vertex, file-wide counter as the index
                // type =  2: Attached vertex,   Y is the index of the attachable (not vertex index)
                //                               X and Z are zero
                // type =  4: Unknown            X, Y, and Z are not indexes, and may be negative
                // type = 16: Sprite vertex,     X is a byte offset to the sprite center vertex (always 0,8,16,24)
                //                               Y is a byte offset to a vertex (usually 16,16,0,0, rarely 24,24,8,8)
                //                               Z value is the width of the sprite with some awkward behavior, not fully understood
                var x = reader.ReadInt16();
                var y = reader.ReadInt16();
                var z = reader.ReadInt16();
                var type = reader.ReadUInt16();
                _vertices[j] = new Vector3(x / _scaleDivisor, y / _scaleDivisor, z / _scaleDivisor);
                _vertexJoints[j] = Triangle.NoJoint;

                if (type == 1)
                {
                    // Already handled in ReadMesh
                }
                else if (type == 2)
                {
                    // Note that this isn't a Joint ID, just a .PSX attachment index ID.
                    var attachmentIndex = (ushort)y;
                    if (_attachableVertices.TryGetValue(attachmentIndex, out var tuple))
                    {
                        _vertices[j] = tuple.Item2;
                        _vertexJoints[j] = tuple.Item1;
                    }
                    else
                    {
                        var breakHere = 0;
                    }
                }
                else if (type == 4)
                {
                    // Observed in Apocalypse, not sure what to do with this
                    var breakHere = 0;
                }
                else if (type == 16)
                {
                    var indexB = (ushort)x / 8u; // 8 is the byte length of each vertex
                    var indexA = (ushort)y / 8u;
                    if ((ushort)x % 8u != 0 || (ushort)y % 8u != 0 || indexA >= vertexCount || indexB >= vertexCount)
                    {
                        return false;
                    }
                    _spriteVertices.Add(j, new PSXSpriteVertexData
                    {
                        IndexA = indexA,
                        IndexB = indexB,
                        Width = z / _scaleDivisor,
                    });
                }
                else if (type != 0)
                {
                    var breakHere = 0;
                }
            }


            // Read mesh normals
            if (_normals == null || _normals.Length < normalCount)
            {
                _normals = new Vector3[normalCount];
            }
            for (uint j = 0; j < normalCount; j++)
            {
                var x = reader.ReadInt16() / 4096f;
                var y = reader.ReadInt16() / 4096f;
                var z = reader.ReadInt16() / 4096f;
                var pad = reader.ReadUInt16(); //pad
                _normals[j] = new Vector3(x, y, z);
                if (pad != 0)
                {
                    var breakHere = 0;
                }
            }

            // I've seen version 0x03 in Apocalypse, and there didn't seem to be fields like this before the faces.
            // Leaving in-case this exists somewhere else...
            //ushort faceFlags  = 0;
            //ushort faceLength = 0;
            //if (version == 0x03)
            //{
            //    faceFlags  = reader.ReadUInt16();
            //    faceLength = reader.ReadUInt16();
            //}
            for (uint j = 0; j < faceCount; j++)
            {
                if (!ReadPrimitive(reader, version, modelIndex, vertexCount, normalCount))
                {
                    return false;
                }
            }

            FlushModels(objectIndex, modelIndex);
            return true;
        }

        private bool ReadPrimitive(BinaryReader reader, ushort version, uint modelIndex, uint vertexCount, uint normalCount)
        {
            var facePosition = reader.BaseStream.Position;

            var renderFlags = RenderFlags.None;
            var mixtureRate = MixtureRate.None;
            var invisible = false;

            var faceFlags  = reader.ReadUInt16();
            var faceLength = reader.ReadUInt16();

            const ushort KnownFlags = 0x19d3;
            var texCoord  = (faceFlags & 0x0001) != 0; // +12 extra bytes (textureIndex, uv0-3)
            var texLookup = (faceFlags & 0x0002) != 0;
            var textured  = texCoord || texLookup;
            //var flag0004  = (faceFlags & 0x0004) != 0;
            var flag0008  = (faceFlags & 0x0008) != 0; // +8 extra bytes (ushort[4], always set with 0x4, seems to be used for actor models)
            var quad      = (faceFlags & 0x0010) == 0;
            var flag0020  = (faceFlags & 0x0020) != 0; // +4 extra bytes (extra bytes are always zero... maybe a working field?)
            var semiTrans = (faceFlags & 0x0040) != 0;
            var transMode = (faceFlags & 0x0180) >> 7; // Effect depends on semiTrans
            //var flag0200  = (faceFlags & 0x0200) != 0;
            //var flag0400  = (faceFlags & 0x0400) != 0;
            var gouraud   = (faceFlags & 0x0800) != 0;
            //var subdiv    = (faceFlags & 0x1000) != 0;
            //var flag2000  = (faceFlags & 0x2000) != 0;
            //var flag4000  = (faceFlags & 0x4000) != 0;


            uint i0 = version == 0x04 ? reader.ReadByte() : reader.ReadUInt16();
            uint i1 = version == 0x04 ? reader.ReadByte() : reader.ReadUInt16();
            uint i2 = version == 0x04 ? reader.ReadByte() : reader.ReadUInt16();
            uint i3 = version == 0x04 ? reader.ReadByte() : reader.ReadUInt16();
            if (i0 >= vertexCount || i1 >= vertexCount || i2 >= vertexCount || (quad && i3 >= vertexCount))
            {
                return false;
            }

            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var mode = reader.ReadByte();
            if (gouraud && (r >= _gouraudCount || g >= _gouraudCount || b >= _gouraudCount || (quad && mode >= _gouraudCount)))
            {
                return false;
            }

            uint normalIndex = reader.ReadUInt16();
            var surfFlags = reader.ReadInt16(); // How the player can interact with this surface, not important
            if (normalIndex >= normalCount)
            {
                return false;
            }

            uint textureIndex = 0;
            byte u0 = 0, v0 = 0, u1 = 0, v1 = 0, u2 = 0, v2 = 0, u3 = 0, v3 = 0;
            if (texCoord)
            {
                textureIndex = reader.ReadUInt32();
                u0 = reader.ReadByte();
                v0 = reader.ReadByte();
                u1 = reader.ReadByte();
                v1 = reader.ReadByte();
                u2 = reader.ReadByte();
                v2 = reader.ReadByte();
                u3 = reader.ReadByte();
                v3 = reader.ReadByte();
            }

            ushort f8_0 = 0, f8_1 = 0, f8_2 = 0, f8_3 = 0;
            if (flag0008)
            {
                // These are definitely ushorts. For example:
                // f8_0 was observed as a non-zero non-minus-one value, while all other values were -1.
                // Often times all of these values are zero, but otherwise they can be under a byte's max value,
                // or over a short's max value.
                f8_0 = reader.ReadUInt16();
                f8_1 = reader.ReadUInt16();
                f8_2 = reader.ReadUInt16();
                f8_3 = reader.ReadUInt16();
            }

            uint f20 = 0;
            if (texCoord && flag0020)
            {
                // Only ever observed as zero, this may be a working field added to aid in whatever this flag actually does.
                // In Apocalyse pest_obj.psx where texCoord wasn't set, this field was skipped in faceLength.
                f20 = reader.ReadUInt32();
            }


            var vertex0 = _vertices[i0];
            var vertex1 = _vertices[i1];
            var vertex2 = _vertices[i2];
            var vertex3 = quad ? _vertices[i3] : Vector3.Zero;

            var joint0 = _vertexJoints[i0];
            var joint1 = _vertexJoints[i1];
            var joint2 = _vertexJoints[i2];
            var joint3 = quad ? _vertexJoints[i3] : Triangle.NoJoint;

            Vector3? spriteCenter = null;
            var hasSpr0 = _spriteVertices.TryGetValue(i0, out var spr0);
            var hasSpr1 = _spriteVertices.TryGetValue(i1, out var spr1);
            var hasSpr2 = _spriteVertices.TryGetValue(i2, out var spr2);
            var hasSpr3 = _spriteVertices.TryGetValue(i3, out var spr3);
            if (hasSpr0 && hasSpr1 && hasSpr2 && (!quad || hasSpr3))
            {
                if (!quad)
                {
                    var breakHere = 0;
                }
                // Not really sure how to properly use IndexA vs. IndexB...
                var ia0 = spr0.IndexA;
                var ib0 = spr0.IndexB;
                var ia1 = spr1.IndexA;
                var ib1 = spr1.IndexB;
                var ia2 = spr2.IndexA;
                var ib2 = spr2.IndexB;
                var ia3 = spr3.IndexA;
                var ib3 = spr3.IndexB;
                if (ia0 >= vertexCount || ia1 >= vertexCount || ia2 >= vertexCount || (quad && ia3 >= vertexCount))
                {
                    return false;
                }
                if (ib0 >= vertexCount || ib1 >= vertexCount || ib2 >= vertexCount || (quad && ib3 >= vertexCount))
                {
                    return false;
                }
                var avertex0 = _vertices[ia0];
                var bvertex0 = _vertices[ib0];
                var avertex1 = _vertices[ia1];
                var bvertex1 = _vertices[ib1];
                var avertex2 = _vertices[ia2];
                var bvertex2 = _vertices[ib2];
                var avertex3 = quad ? _vertices[ia3] : Vector3.Zero;
                var bvertex3 = quad ? _vertices[ib3] : Vector3.Zero;
#if DEBUG
                bool Check(Vector3 va, Vector3 vb)
                {
                    return va.X != 0f || va.Z != 0f || vb.X != 0f || vb.Z != 0f;
                }
                if (Check(avertex0, bvertex0) || Check(avertex1, bvertex1) || Check(avertex2, bvertex2) || (quad && Check(avertex3, bvertex3)))
                {
                    var breakHere = 0;
                }
#endif

                spriteCenter = (avertex0 + avertex1 + avertex2 + (quad ? avertex3 : Vector3.Zero));
                spriteCenter /= (quad ? 4 : 3);

                var scale = 1.5f;
                // Remember that Y-up is negative, so height values are negated compared to what we set for UVs.
                // Note that these vertex coordinates also assume the default orientation of the view is (0, 0, -1).
                // These numbers are kind of fudged, but it's the only way I
                // could get the sprites working purely from the values given.
                vertex0 = avertex0 + new Vector3(-spr0.Width * scale, 0f, 0f);
                vertex1 = avertex1 + new Vector3(-spr1.Width * scale, 0f, 0f);
                vertex2 = avertex2 + new Vector3(spr2.Width * scale, 0f, 0f);
                vertex3 = avertex3 + new Vector3(spr3.Width * scale, 0f, 0f); // todo: What about when we're not a quad?

                // todo: Double-sided sprites until this is less jank.
                // I've encountered sprites that were backwards.
                // Known places where this fails without double-sided:
                // * THPS2, CD.WAD_1B36000 courtyard trees
                renderFlags |= RenderFlags.SpriteNoPitch | RenderFlags.DoubleSided;
            }
            else if (hasSpr0 || hasSpr1 || hasSpr2 || (quad && hasSpr3))
            {
                var breakHere = 0;
            }

            var normal = _normals[normalIndex];

            Color color0, color1, color2, color3;
            if (!gouraud)
            {
                color0 = color1 = color2 = color3 = new Color(r / 255f, g / 255f, b / 255f);
            }
            else
            {
                // R/G/B/Mode are treated as indices into gouraud palette table
                color0 = _gouraudPalette[r];
                color1 = _gouraudPalette[g];
                color2 = _gouraudPalette[b];
                color3 = quad ? _gouraudPalette[mode] : Color.Grey;
            }

            uint tPage = 0;
            Vector2 uv0, uv1, uv2, uv3;
            if (textured)
            {
                renderFlags |= RenderFlags.Textured;
                if (!texCoord)
                {
                    tPage = uint.MaxValue; // Arbitrary value that hopefully doesn't exist
                }
                else if (textureIndex < _textureHashCount)
                {
                    tPage = _textureHashes[textureIndex];
                }
                uv0 = GeomMath.ConvertUV(u0, v0);
                uv1 = GeomMath.ConvertUV(u1, v1);
                uv2 = GeomMath.ConvertUV(u2, v2);
                uv3 = quad ? GeomMath.ConvertUV(u3, v3) : Vector2.Zero;
            }
            else
            {
                uv0 = uv1 = uv2 = uv3 = Vector2.Zero;
            }

            // semiTrans: 0-Normal, 1-Semi-transparent
            // transMode: 0-Opaque, 1-Invisible, 2-unused(?), 3-unused(?)
            //            0-50% back + 50% poly, 1-100% back + 100% poly, 2-100% back - 100% poly, 3-100% back + 25% poly
            if (!semiTrans)
            {
                switch (transMode)
                {
                    case 0: break; // Opaque
                    case 1: invisible = true; break; // Invisible
                    case 2:
                        break; // Probably unused
                    case 3:
                        break; // Probably unused
                }
            }
            else
            {
                renderFlags |= RenderFlags.SemiTransparent;
                switch (transMode)
                {
                    case 0: mixtureRate = MixtureRate.Back50_Poly50; break;
                    case 1: mixtureRate = MixtureRate.Back100_Poly100; break;
                    case 2: mixtureRate = MixtureRate.Back100_PolyM100; break;
                    case 3: mixtureRate = MixtureRate.Back100_Poly25; break;
                }
            }


            if (!invisible || Settings.Instance.AdvancedPSXIncludeInvisible)
            {
#if DEBUG
                var extraLength = faceLength - (reader.BaseStream.Position - facePosition);
                var triangleDebugData = new[]
                {
                    $"faceFlags: 0x{faceFlags:x04}",
                    $"surfFlags: 0x{surfFlags:x04}",
                    $"extraLength: {extraLength}",
                    $"Position: 0x{reader.BaseStream.Position:x}",
                };
#endif

                var modelJointID = modelIndex + 1u;

                var originalNormalIndices = new[] { normalIndex, normalIndex, normalIndex };
                var joints1 = Triangle.CreateJoints(joint0, joint1, joint2, modelJointID);
                var triangle1 = new Triangle
                {
                    Vertices = new[] { vertex0, vertex1, vertex2 },
                    Normals = new[] { normal, normal, normal },
                    Uv = new[] { uv0, uv1, uv2 },
                    Colors = new[] { color0, color1, color2 },
                    OriginalVertexIndices = new[] { i0, i1, i2 },
                    OriginalNormalIndices = originalNormalIndices,
                    VertexJoints = joints1,
                    NormalJoints = (uint[])joints1?.Clone(),
#if DEBUG
                    DebugData = triangleDebugData,
#endif
                };
                if (textured)
                {
                    triangle1.TiledUv = new TiledUV(triangle1.Uv, 0f, 0f, 1f, 1f);
                    triangle1.Uv = (Vector2[])triangle1.Uv.Clone();
                }
                AddTriangle(triangle1, spriteCenter, tPage, renderFlags, mixtureRate);

                if (quad)
                {
                    var joints2 = Triangle.CreateJoints(joint1, joint3, joint2, modelJointID);
                    var triangle2 = new Triangle
                    {
                        Vertices = new[] { vertex1, vertex3, vertex2 },
                        Normals = new[] { normal, normal, normal },
                        Uv = new[] { uv1, uv3, uv2 },
                        Colors = new[] { color1, color3, color2 },
                        OriginalVertexIndices = new[] { i1, i3, i2 },
                        OriginalNormalIndices = originalNormalIndices,
                        VertexJoints = joints2,
                        NormalJoints = (uint[])joints2?.Clone(),
#if DEBUG
                        DebugData = triangleDebugData,
#endif
                    };
                    if (textured)
                    {
                        triangle2.TiledUv = new TiledUV(triangle2.Uv, 0f, 0f, 1f, 1f);
                        triangle2.Uv = (Vector2[])triangle2.Uv.Clone();
                    }
                    AddTriangle(triangle2, spriteCenter, tPage, renderFlags, mixtureRate);
                }
            }

            reader.BaseStream.Seek(facePosition + faceLength, SeekOrigin.Begin);
            return true;
        }

        private void FlushModels(uint objectIndex, uint modelIndex)
        {
            var psxObject = _psxObjects[objectIndex];
            var psxMesh = _psxMeshes[modelIndex];
            var modelIsJoint = psxMesh.IsJoint;
#if DEBUG
            _objectDebugData.TryGetValue(objectIndex, out var objectDebugData);
            objectDebugData?.Insert(0, $"objectIndex: {objectIndex}");
            objectDebugData?.Insert(1, $"modelIndex: {psxObject.MeshIndex}");
#endif

            var localMatrix = _coords[objectIndex].WorldMatrix;
            //var localMatrix = Matrix4.CreateTranslation(psxObject.Translation);

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
                    TMDID = objectIndex + 1u,
                    JointID = modelIndex + 1u,
                    OriginalLocalMatrix = localMatrix,
#if DEBUG
                    DebugData = objectDebugData?.ToArray(),
#endif
                };
                _models.Add(model);
                // We can add attachable indices onto this existing model, instead of adding a dummy model.
                // A model is used so that it can transform the attachable vertices.
                modelIsJoint = false;
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
                    TMDID = objectIndex + 1u,
                    OriginalLocalMatrix = localMatrix,
#if DEBUG
                    DebugData = objectDebugData?.ToArray(),
#endif
                };
                _models.Add(spriteModel);
            }
            if (modelIsJoint)
            {
                // No models were added for this model index, add a dummy model to serve as the joint transform.
                var jointModel = new ModelEntity
                {
                    Triangles = new Triangle[0],
                    TexturePage = 0,
                    TMDID = objectIndex + 1u,
                    JointID = modelIndex + 1u,
                    Visible = false,
                    OriginalLocalMatrix = localMatrix,
#if DEBUG
                    DebugData = objectDebugData?.ToArray(),
#endif
                };
                _models.Add(jointModel);
                modelIsJoint = false;
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
                    ID = renderInfo.TexturePage, // Unknown-type hash of name
                    ExpectedFormat = PSXParser.FormatNameConst,
                    UVConversion = TextureUVConversion.Absolute,
                    TiledAreaConversion = TextureUVConversion.TextureSpace,
                };
            }
            return null;
        }

        private void AddTriangle(Triangle triangle, Vector3? spriteCenter, uint tPage, RenderFlags renderFlags, MixtureRate mixtureRate)
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


        private struct PSXObject
        {
            public Vector3 Translation;
            public ushort MeshIndex;
        }

        private class PSXMesh
        {
            public uint MeshTop;
            public uint VertexStart;
            public uint NormalStart;
            public bool IsJoint;
            public ushort LODNextMeshIndex;
            public short LODDepth;
        }

        private struct PSXClutData
        {
            public uint ClutTop;
            public bool IsClut256;
        }

        private struct PSXSpriteVertexData
        {
            public uint IndexA;
            public uint IndexB;
            public float Width;
        }
    }
}
