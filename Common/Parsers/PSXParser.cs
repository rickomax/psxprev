using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using OpenTK;
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

        // Multiplied by 16 for files with HIER tagged chunk so that humanoids match the map size
        // Translation divisor is only used for reading objects
        private float _scaleDivisorTranslation = 1f;
        private float _scaleDivisor = 1f;
        private bool _useModelIndexAsObjectIndex; // Actor models use the model index to get the transform of the object
        private PSXObject[] _objects;
        private Coordinate[] _coords;
        private uint _objectCount;
        private uint _modelCount;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private uint[] _vertexJoints;
        private bool _modelIsJoint;
        private readonly Color[] _gouraudPalette = new Color[256];
        private uint _gouraudPaletteSize;
        private uint[] _textureHashes;
        private uint _textureHashCount;
        private readonly Dictionary<uint, PSXClutData> _clutDatas = new Dictionary<uint, PSXClutData>();
        private readonly Dictionary<RenderInfo, List<Triangle>> _groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();
        private readonly Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>> _groupedSprites = new Dictionary<Tuple<Vector3, RenderInfo>, List<Triangle>>();
        private readonly Dictionary<uint, Tuple<uint, Vector3>> _attachableVertices = new Dictionary<uint, Tuple<uint, Vector3>>();
        private readonly Dictionary<uint, PSXSpriteVertexData> _spriteVertices = new Dictionary<uint, PSXSpriteVertexData>();
        private readonly List<ModelEntity> _models = new List<ModelEntity>();
        // Debug:
#if DEBUG
        private readonly Dictionary<uint, List<string>> _modelDebugData = new Dictionary<uint, List<string>>();
#endif


        public PSXParser(EntityAddedAction entityAdded, TextureAddedAction textureAdded)
            : base(entityAdded: entityAdded, textureAdded: textureAdded)
        {
        }

        public override string FormatName => FormatNameConst;

        protected override void Parse(BinaryReader reader)
        {
            _scaleDivisor = _scaleDivisorTranslation = Settings.Instance.AdvancedPSXScaleDivisor;
            _coords = null;
            _modelIsJoint = false;
            _gouraudPaletteSize = 0;
            _textureHashCount = 0;
            _useModelIndexAsObjectIndex = true; // Assumed true unless we encounter a blockmap tagged chunk
            _clutDatas.Clear();
            _groupedTriangles.Clear();
            _groupedSprites.Clear();
            _attachableVertices.Clear();
            _models.Clear();
#if DEBUG
            _modelDebugData.Clear();
#endif

            if (!ReadPSX(reader))
            {
                foreach (var texture in TextureResults)
                {
                    texture.Dispose();
                }
                TextureResults.Clear();
            }
        }

        private bool ReadPSX(BinaryReader reader)
        {
            var version = reader.ReadUInt16();
            var magic   = reader.ReadUInt16();
            if (version != 0x0003 && version != 0x0004 && version != 0x0006)
            {
                return false;
            }
            if (magic != 0x0002)
            {
                return false;
            }

            // Pointer to tagged chunks, model names, textures
            var metaTop = reader.ReadUInt32();
            // Allow zero object count in-case this is a texture library
            _objectCount = reader.ReadUInt32();
            if (_objectCount > Limits.MaxPSXObjectCount)
            {
                return false;
            }

            if (_objects == null || _objects.Length < _objectCount)
            {
                _objects = new PSXObject[_objectCount];
            }
            for (uint i = 0; i < _objectCount; i++)
            {
                _objects[i] = ReadObject(reader, version, i);
                if (_objects[i].ModelIndex >= _objectCount)
                {
                    _useModelIndexAsObjectIndex = false; // Not expected, but we can't use this if we'd go out of bounds
                }
            }


            // Allow zero model count in-case this is a texture library
            _modelCount = reader.ReadUInt32();
            if ((_modelCount == 0) != (_objectCount == 0) || _modelCount > Limits.MaxPSXObjectCount)
            {
                return false; // Models or objects are zero, but not both. Or model count is too high
            }

            /*var hasDiffs = _objectCount == 0;
            var hasHigher = false;
            for (uint i = 0; i < _objectCount; i++)
            {
                hasDiffs |= (_objects[i].ModelIndex != i);
                hasHigher |= (_objects[i].ModelIndex >= _objectCount);
            }
            if (!hasDiffs && _objectCount != 0)
            {
                //return false;
            }*/


            // Read gouraud palette/vertex divisor from tagged chunks, and texture hashes before reading models
            var modelsStartPosition = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + metaTop, SeekOrigin.Begin);

            if (!ReadTaggedChunks(reader, version))
            {
                return false;
            }

            // Hashes of model names, not important
            var modelHashes = Program.Debug ? new string[_modelCount] : null;
            for (uint i = 0; i < _modelCount; i++)
            {
                var modelHash = reader.ReadUInt32();
                if (modelHashes != null)
                {
                    modelHashes[i] = $"0x{modelHash:x08}";
                }
            }
            if (modelHashes != null && _modelCount > 0)
            {
                Program.Logger.WriteLine("Model Hashes: " + string.Join(", ", modelHashes));
            }

            if (!ReadTextures(reader, version))
            {
                return false;
            }
            reader.BaseStream.Seek(modelsStartPosition, SeekOrigin.Begin);


            // We need to go through and read each model's vertices first, since we need to know joint vertex positions ahead of time
            var modelsPosition = reader.BaseStream.Position;
            for (uint i = 0; i < _modelCount; i++)
            {
                var modelTop = reader.ReadUInt32();
                var modelPosition = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + modelTop, SeekOrigin.Begin);
                if (!ReadModelJoints(reader, version, i))
                {
                    return false;
                }
                reader.BaseStream.Seek(modelPosition, SeekOrigin.Begin);
            }

            // Now that we have the joint vertex positions, we can read through each model and build lists of triangles
            reader.BaseStream.Seek(modelsPosition, SeekOrigin.Begin);
            for (uint i = 0; i < _objectCount; i++)
            {
                var psxObject = _objects[i];
                var modelIndex = _useModelIndexAsObjectIndex ? i : psxObject.ModelIndex;
                reader.BaseStream.Seek(modelsPosition + modelIndex * 4, SeekOrigin.Begin);

                var modelTop = reader.ReadUInt32();
                reader.BaseStream.Seek(_offset + modelTop, SeekOrigin.Begin);
                if (!ReadModel(reader, version, i, modelIndex))
                {
                    return false;
                }
            }
            /*reader.BaseStream.Seek(modelsPosition, SeekOrigin.Begin);
            for (uint i = 0; i < _modelCount; i++)
            {
                var modelTop = reader.ReadUInt32();
                var modelPosition = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + modelTop, SeekOrigin.Begin);
                if (!ReadModel(reader, version, i))//, ref attachmentIndex))
                {
                    return false;
                }
                reader.BaseStream.Seek(modelPosition, SeekOrigin.Begin);
            }*/

            if (_models.Count > 0)
            {
                var rootEntity = new RootEntity();
                rootEntity.ChildEntities = _models.ToArray();
                rootEntity.Coords = _coords;
                //rootEntity.OwnedTextures.AddRange(TextureResults); // todo: need to change how owned textures are handled
                foreach (var texture in TextureResults)
                {
                    texture.OwnerEntity = rootEntity;
                }
                // PrepareJoints must be called before ComputeBounds
                rootEntity.PrepareJoints(_attachableVertices.Count > 0);
                rootEntity.ComputeBounds();
                EntityResults.Add(rootEntity);
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


            uint chunkCount = 0;
            var chunkTag = reader.ReadUInt32();
            while (chunkTag != TagStop)
            {
                if (++chunkCount > Limits.MaxPSXTaggedChunks)
                {
                    return false;
                }

                var chunkLength = reader.ReadUInt32();
                var chunkPosition = reader.BaseStream.Position;

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

                var result = true;
                switch (chunkTag)
                {
                    case TagRGBs:
                        result = ReadTaggedChunkRGBs(reader, chunkLength);
                        break;
                    case TagHIER:
                        result = ReadTaggedChunkHIER(reader, chunkLength);
                        break;
                    case TagBlockMap:
                        // This probably isn't the correct way to detect this, but it works for all games tested against:
                        // (Apocalypse, Spider Man 1-2, Tony Hawk's Pro Skater 1-4)
                        _useModelIndexAsObjectIndex = false; // Not an actor model
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
            _gouraudPaletteSize = chunkLength / 4;
            if (_gouraudPaletteSize > _gouraudPalette.Length)
            {
                // Error out if palette size is too large, or should we ignore remaining colors...?
                return false;
            }
            var specialStarted = false;
            for (uint i = 0; i < _gouraudPaletteSize; i++)
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
            // We can't use Coords yet, the point of them is to handle relative position,
            // but objects already store absolute position...

            // Models with hierarchy are scaled down by x16.
            _scaleDivisor = _scaleDivisorTranslation * 16f;
            if (_coords != null)
            {
                var breakHere = 0;
            }
            _coords = new Coordinate[_objectCount];

            // Can't rely on chunk length, since it's padded to 4 bytes
            var count = chunkLength / 2;
            for (uint i = 0; i < _objectCount; i++)
            {
                var coord = new Coordinate
                {
                    Coords = _coords,
                    ID = i,
                };
                if (i < count)
                {
                    var parentIndex = reader.ReadUInt16();
                    if (parentIndex != i)
                    {
                        coord.ParentID = parentIndex;
                    }
                }
                _coords[i] = coord;
            }
            if (Coordinate.FindCircularReferences(_coords))
            {
                return false;
            }
            for (uint i = 0; i < _objectCount; i++)
            {
                var coord = _coords[i];
                var translation = _objects[i].Translation;
                if (coord.HasParent)
                {
                    // Convert absolute translation to relative translation.
                    // We only need to do this for the first-level parent,
                    // since that object's translation is also absolute.
                    translation -= _objects[coord.ParentID].Translation;
                }
                coord.OriginalLocalMatrix = Matrix4.CreateTranslation(translation);
                coord.OriginalTranslation = translation;
            }
            return true;
        }

        private bool ReadTextures(BinaryReader reader, ushort version)
        {
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
            // todo: What about stp bits for other colors? Is it effected by maskMode?
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
            var unk1 = reader.ReadUInt32();
            var unk2 = reader.ReadUInt16();
            // Index of model in the models list. If multiple objects have the same model,
            // then multiple models are created, which theoretically can be used for
            // having the same props in different positions.
            var modelIndex = reader.ReadUInt16();
            // When non-zero, observed as multiples of 2, tx and/or ty can be non-zero.
            var tx = reader.ReadInt16();
            var ty = reader.ReadInt16();
            var unk3 = reader.ReadUInt32();
            // Generally every object uses the same palette top, not sure what this palette is used for though.
            // This can also be null.
            var paletteTop = reader.ReadUInt32();

            /*_coords[objectIndex] = new Coordinate
            {
                OriginalLocalMatrix = Matrix4.CreateTranslation(x, y, z),
                OriginalTranslation = new Vector3(x, y, z),
                ID = objectIndex,
                //ParentID = Coordinate.NoID, // Assigned later
                //Coords = coords, // Assigned later on success
            };*/

#if DEBUG
            _modelDebugData[modelIndex] = new List<string> {
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
                Translation = new Vector3(x, y, z),
                ModelIndex = modelIndex,
            };
        }

        private bool ReadModelJoints(BinaryReader reader, ushort version, uint modelIndex)
        {
            var modelFlags  = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
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

            var radius = reader.ReadUInt32() / (4096f * _scaleDivisor);
            var xMax = reader.ReadInt16() / _scaleDivisor;
            var xMin = reader.ReadInt16() / _scaleDivisor;
            var yMax = reader.ReadInt16() / _scaleDivisor;
            var yMin = reader.ReadInt16() / _scaleDivisor;
            var zMax = reader.ReadInt16() / _scaleDivisor;
            var zMin = reader.ReadInt16() / _scaleDivisor;
            if (version == 0x04)
            {
                var unk2 = reader.ReadUInt32();
            }

            for (uint j = 0; j < vertexCount; j++)
            {
                // Vertex coding:
                // pad =  0: Normal vertex
                // pad =  1: Attachable vertex, file-wide counter as the index
                // pad =  2: Attached vertex,   Y is the index of the attachable (not vertex index)
                //                              X and Z are zero
                // pad =  4: Unknown            X, Y, and Z are not indexes, and may be negative
                // pad = 16: Sprite vertex,     X is a byte offset to the sprite center vertex (always 0,8,16,24)
                //                              Y is a byte offset to a vertex (usually 16,16,0,0, rarely 24,24,8,8)
                //                              Z value is the width of the sprite with some awkward behavior, not fully understood
                var x = reader.ReadInt16();
                var y = reader.ReadInt16();
                var z = reader.ReadInt16();
                var pad = reader.ReadUInt16();

                if (pad == 1)
                {
                    var vertex = new Vector3(x / _scaleDivisor, y / _scaleDivisor, z / _scaleDivisor);

                    // Note that this isn't a Joint ID, just a .PSX attachment index ID.
                    var attachmentIndex = (uint)_attachableVertices.Count;
                    _attachableVertices.Add(attachmentIndex, new Tuple<uint, Vector3>(modelIndex + 1u, vertex));
                }
            }
            return true;
        }

        private bool ReadModel(BinaryReader reader, ushort version, uint objectIndex, uint modelIndex)
        {
            // Reset model state
            _modelIsJoint = false;
            _spriteVertices.Clear();

#if DEBUG
            if (!_modelDebugData.TryGetValue(objectIndex, out var modelDebugData))
            {
                modelDebugData = new List<string>();
                _modelDebugData.Add(objectIndex, modelDebugData);
            }
#endif
            var modelFlags  = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
            var vertexCount = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
            var normalCount = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
            var faceCount   = version == 0x04 ? reader.ReadUInt16() : reader.ReadUInt32();
#if DEBUG
            modelDebugData?.Add($"modelFlags: 0x{modelFlags:x08}");
#endif

            if (vertexCount > Limits.MaxPSXVertices || normalCount > Limits.MaxPSXVertices)
            {
                return false;
            }
            if (faceCount > Limits.MaxPSXFaces)
            {
                return false;
            }

            var radius = reader.ReadUInt32() / (4096f * _scaleDivisor);
            var xMax = reader.ReadInt16() / _scaleDivisor;
            var xMin = reader.ReadInt16() / _scaleDivisor;
            var yMax = reader.ReadInt16() / _scaleDivisor;
            var yMin = reader.ReadInt16() / _scaleDivisor;
            var zMax = reader.ReadInt16() / _scaleDivisor;
            var zMin = reader.ReadInt16() / _scaleDivisor;
            if (version == 0x04)
            {
                var unk2 = reader.ReadUInt32();
#if DEBUG
                modelDebugData?.Add($"modelUnk2: 0x{unk2:x08}");
#endif
            }


            if (_vertices == null || _vertices.Length < vertexCount)
            {
                _vertices = new Vector3[vertexCount];
                _vertexJoints = new uint[vertexCount];
            }
            for (uint j = 0; j < vertexCount; j++)
            {
                // Vertex coding:
                // pad =  0: Normal vertex
                // pad =  1: Attachable vertex, file-wide counter as the index
                // pad =  2: Attached vertex,   Y is the index of the attachable (not vertex index)
                //                              X and Z are zero
                // pad =  4: Unknown            X, Y, and Z are not indexes, and may be negative
                // pad = 16: Sprite vertex,     X is a byte offset to the sprite center vertex (always 0,8,16,24)
                //                              Y is a byte offset to a vertex (usually 16,16,0,0, rarely 24,24,8,8)
                //                              Z value is the width of the sprite with some awkward behavior, not fully understood
                var x = reader.ReadInt16();
                var y = reader.ReadInt16();
                var z = reader.ReadInt16();
                var pad = reader.ReadUInt16();
                var vertex = new Vector3(x / _scaleDivisor, y / _scaleDivisor, z / _scaleDivisor);
                var jointID = Triangle.NoJoint;

                if (pad == 1)
                {
                    _modelIsJoint = true;
                }
                else if (pad == 2)
                {
                    // Note that this isn't a Joint ID, just a .PSX attachment index ID.
                    var attachmentIndex = (ushort)y;
                    if (_attachableVertices.TryGetValue(attachmentIndex, out var tuple))
                    {
                        vertex = tuple.Item2;
                        jointID = tuple.Item1;
                    }
                    else
                    {
                        var breakHere = 0;
                    }
                }
                else if (pad == 4)
                {
                    // Observed in Apocalypse, not sure what to do with this
                    var breakHere = 0;
                }
                else if (pad == 16)
                {
                    var indexB = (ushort)x / 8u;
                    var indexA = (ushort)y / 8u;
                    if ((ushort)x % 8u != 0 || (ushort)y % 8u != 0 || indexA >= vertexCount || indexB >= vertexCount)
                    {
                        return false;
                    }
                    _spriteVertices[j] = new PSXSpriteVertexData
                    {
                        IndexA = indexA,
                        IndexB = indexB,
                        Width = z / _scaleDivisor,
                    };
                }
                else if (pad != 0)
                {
                    var breakHere = 0;
                }
                _vertices[j] = vertex;
                _vertexJoints[j] = jointID;
            }

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
            //uint faceFlags  = 0;
            //uint faceLength = 0;
            //if (version == 0x03)
            //{
            //    faceFlags  = reader.ReadUInt16();
            //    faceLength = reader.ReadUInt16();
            //}
            for (uint j = 0; j < faceCount; j++)
            {
                var facePosition = reader.BaseStream.Position;

                var renderFlags = RenderFlags.None;
                var mixtureRate = MixtureRate.None;

                var faceFlags  = reader.ReadUInt16();
                var faceLength = reader.ReadUInt16();
                var quad      = (faceFlags & 0x0010) == 0;
                var gouraud   = (faceFlags & 0x0800) != 0;
                var textured  = (faceFlags & 0x0003) != 0;
                var semiTrans = (faceFlags & 0x01c0) >> 6;
                //var subdivision = (faceFlags & 0x1000) != 0;

                var i0 = version == 0x04 ? reader.ReadByte() : reader.ReadUInt16();
                var i1 = version == 0x04 ? reader.ReadByte() : reader.ReadUInt16();
                var i2 = version == 0x04 ? reader.ReadByte() : reader.ReadUInt16();
                var i3 = version == 0x04 ? reader.ReadByte() : reader.ReadUInt16();
                if (i0 >= vertexCount || i1 >= vertexCount || i2 >= vertexCount || (quad && i3 >= vertexCount))
                {
                    return false; //throw new IndexOutOfRangeException("Vertex index out of bounds");
                }
                var vertex0 = _vertices[i0];
                var vertex1 = _vertices[i1];
                var vertex2 = _vertices[i2];
                var vertex3 = quad ? _vertices[i3] : Vector3.Zero;

                Vector3? spriteCenter = null;
                var hasSpr0 = _spriteVertices.TryGetValue(i0, out var spr0);
                var hasSpr1 = _spriteVertices.TryGetValue(i1, out var spr1);
                var hasSpr2 = _spriteVertices.TryGetValue(i2, out var spr2);
                var hasSpr3 = _spriteVertices.TryGetValue(i3, out var spr3);
                if (hasSpr0 && hasSpr1 && hasSpr2 && (!quad || hasSpr3))
                {
                    // Not really sure how to properly use IndexA vs. IndexB...
                    var avertex0 = _vertices[spr0.IndexA];
                    var bvertex0 = _vertices[spr0.IndexB];
                    var avertex1 = _vertices[spr1.IndexA];
                    var bvertex1 = _vertices[spr1.IndexB];
                    var avertex2 = _vertices[spr2.IndexA];
                    var bvertex2 = _vertices[spr2.IndexB];
                    var avertex3 = quad ? _vertices[spr3.IndexA] : Vector3.Zero;
                    var bvertex3 = quad ? _vertices[spr3.IndexB] : Vector3.Zero;
                    bool Check(Vector3 va, Vector3 vb)
                    {
                        return va.X != 0f || va.Z != 0f || vb.X != 0f || vb.Z != 0f;
                    }
                    if (Check(avertex0, bvertex0) || Check(avertex1, bvertex1) || Check(avertex2, bvertex2) || (quad && Check(avertex3, bvertex3)))
                    {
                        var breakHere = 0;
                    }

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
                    vertex3 = avertex3 + new Vector3(spr3.Width * scale, 0f, 0f);

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

                var joint0 = _vertexJoints[i0];
                var joint1 = _vertexJoints[i1];
                var joint2 = _vertexJoints[i2];
                var joint3 = quad ? _vertexJoints[i3] : Triangle.NoJoint;

                Color color0, color1, color2, color3;
                var r = reader.ReadByte();
                var g = reader.ReadByte();
                var b = reader.ReadByte();
                var mode = reader.ReadByte();
                if (!gouraud)
                {
                    color0 = color1 = color2 = color3 = new Color(r / 255f, g / 255f, b / 255f);
                }
                else if (r < _gouraudPaletteSize && g < _gouraudPaletteSize && b < _gouraudPaletteSize && (!quad || mode < _gouraudPaletteSize))
                {
                    // R/G/B/Mode are treated as indices into gouraud palette table
                    color0 = _gouraudPalette[r];
                    color1 = _gouraudPalette[g];
                    color2 = _gouraudPalette[b];
                    color3 = quad ? _gouraudPalette[mode] : Color.Grey;
                }
                else
                {
                    // Gouraud but no (or too small) palette table, this shouldn't happen
                    //color0 = color1 = color2 = color3 = Color.Grey;
                    return false;
                }

                var normalIndex = reader.ReadUInt16();
                var surfFlags = reader.ReadInt16(); // How the player can interact with this surface, not important
                if (normalIndex >= normalCount)
                {
                    return false; //throw new IndexOutOfRangeException("Normal index out of bounds");
                }
                var normal0 = _normals[normalIndex];
                var normal1 = _normals[normalIndex];
                var normal2 = _normals[normalIndex];
                var normal3 = quad ? _normals[normalIndex] : Vector3.Zero;

                var invisible = false;
                switch (semiTrans)
                {
                    case 0x0: // Opaque
                        break;
                    case 0x1:
                        renderFlags |= RenderFlags.SemiTransparent;
                        mixtureRate = MixtureRate.Back50_Poly50;
                        invisible = false;
                        break;
                    case 0x2: // Invisible, likely for map triggers and behavioral objects
                        invisible = true;
                        break;
                    case 0x3:
                        renderFlags |= RenderFlags.SemiTransparent;
                        mixtureRate = MixtureRate.Back100_Poly100;
                        invisible = false;
                        break;
                    case 0x4: // Never observed, effect unknown
                        break;
                    case 0x5:
                        renderFlags |= RenderFlags.SemiTransparent;
                        mixtureRate = MixtureRate.Back100_PolyM100;
                        invisible = false;
                        break;
                    case 0x6: // Never observed, effect unknown
                        break;
                    case 0x7:
                        renderFlags |= RenderFlags.SemiTransparent;
                        mixtureRate = MixtureRate.Back100_Poly25;
                        invisible = false;
                        break;
                }

                uint tPage = 0;
                Vector2 uv0, uv1, uv2, uv3;
                if (textured)
                {
                    renderFlags |= RenderFlags.Textured;
                    var textureIndex = reader.ReadUInt32();
                    if (textureIndex < _textureHashCount)
                    {
                        tPage = _textureHashes[textureIndex];
                    }
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
                else
                {
                    uv0 = uv1 = uv2 = uv3 = Vector2.Zero;
                }

                if (!invisible || Settings.Instance.AdvancedPSXIncludeInvisible)
                {
#if DEBUG
                    var extraLength = faceLength - (reader.BaseStream.Position - facePosition);
                    var triangle1DebugData = new List<string>
                    {
                        $"faceFlags: 0x{faceFlags:x04}",
                        $"surfFlags: 0x{surfFlags:x04}",
                        $"extraLength: {extraLength}",
                        $"Position: 0x{reader.BaseStream.Position:x}",
                    };
                    var triangle2DebugData = new List<string>(triangle1DebugData);
#endif

                    var modelJointID = modelIndex + 1u;

                    var originalNormalIndices = new uint[] { normalIndex, normalIndex, normalIndex };
                    //var joints1 = Triangle.CreateJoints(joint0, joint1, joint2, modelJointID);
                    var triangle1 = new Triangle
                    {
                        Vertices = new[] { vertex0, vertex1, vertex2 },
                        Normals = new[] { normal0, normal1, normal2 },
                        Uv = new[] { uv0, uv1, uv2 },
                        Colors = new[] { color0, color1, color2 },
                        OriginalVertexIndices = new uint[] { i0, i1, i2 },
                        OriginalNormalIndices = originalNormalIndices,
                        // Use helper functions to avoid allocating an array if all are "no attachments"
                        VertexJoints = Triangle.CreateJoints(joint0, joint1, joint2, modelJointID),
                        NormalJoints = Triangle.CreateJoints(joint0, joint1, joint2, modelJointID),
#if DEBUG
                        DebugData = triangle1DebugData.ToArray(),
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
                        //var joints2 = Triangle.CreateJoints(joint1, joint3, joint2, modelJointID);
                        var triangle2 = new Triangle
                        {
                            Vertices = new[] { vertex1, vertex3, vertex2 },
                            Normals = new[] { normal1, normal3, normal2 },
                            Uv = new[] { uv1, uv3, uv2 },
                            Colors = new[] { color1, color3, color2 },
                            OriginalVertexIndices = new uint[] { i1, i3, i2 },
                            OriginalNormalIndices = originalNormalIndices,
                            // Use helper functions to avoid allocating an array if all are "no attachments"
                            VertexJoints = Triangle.CreateJoints(joint1, joint3, joint2, modelJointID),
                            NormalJoints = Triangle.CreateJoints(joint1, joint3, joint2, modelJointID),
#if DEBUG
                            DebugData = triangle2DebugData.ToArray(),
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
            }

            FlushModels(objectIndex, modelIndex);
            return true;
        }

        private void FlushModels(uint objectIndex, uint modelIndex)
        {
            var psxObject = _objects[objectIndex];
#if DEBUG
            _modelDebugData.TryGetValue(objectIndex, out var modelDebugData);
            modelDebugData?.Insert(0, $"objectIndex: {objectIndex}");
            modelDebugData?.Insert(1, $"modelIndex: {psxObject.ModelIndex}");
#endif
            //var coord = _coords[i];
            //var localMatrix = coord.WorldMatrix;
            var localMatrix = Matrix4.CreateTranslation(psxObject.Translation);

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
                    DebugData = modelDebugData?.ToArray(),
#endif
                };
                _models.Add(model);
                _modelIsJoint = false;
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
                    DebugData = modelDebugData?.ToArray(),
#endif
                };
                _models.Add(spriteModel);
            }
            if (_modelIsJoint)
            {
                var jointModel = new ModelEntity
                {
                    Triangles = new Triangle[0],
                    TexturePage = 0,
                    TMDID = objectIndex + 1u,
                    JointID = modelIndex + 1u,
                    Visible = false,
                    OriginalLocalMatrix = localMatrix,
#if DEBUG
                    DebugData = modelDebugData?.ToArray(),
#endif
                };
                _models.Add(jointModel);
                _modelIsJoint = false;
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
            public ushort ModelIndex;
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
