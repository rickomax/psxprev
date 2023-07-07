using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using OpenTK;

namespace PSXPrev.Classes
{
    public class HMDParser
    {
        private long _offset;

        private readonly Action<RootEntity, long> _entityAddedAction;
        private readonly Action<Animation, long> _animationAddedAction;
        private readonly Action<Texture, long> _textureAddedAction;

        public HMDParser(Action<RootEntity, long> entityAddedAction, Action<Animation, long> animationAddedAction, Action<Texture, long> textureAddedAction)
        {
            _entityAddedAction = entityAddedAction;
            _animationAddedAction = animationAddedAction;
            _textureAddedAction = textureAddedAction;
        }

        public void LookForHMDEntities(BinaryReader reader, string fileTitle)
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
                    var version = reader.ReadUInt32();
                    if (version == 0x00000050)
                    {
                        var rootEntity = ParseHMD(reader, out var animations, out var textures);
                        if (rootEntity != null)
                        {
                            rootEntity.EntityName = string.Format("{0}{1:X}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            _entityAddedAction(rootEntity, reader.BaseStream.Position);
                            Program.Logger.WritePositiveLine("Found HMD Model at offset {0:X}", _offset);
                            _offset = reader.BaseStream.Position;
                            passed = true;
                        }
                        foreach (var animation in animations)
                        {
                            animation.AnimationName = string.Format("{0}{1:x}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            _animationAddedAction(animation, reader.BaseStream.Position);
                            Program.Logger.WritePositiveLine("Found HMD Animation at offset {0:X}", _offset);
                            _offset = reader.BaseStream.Position;
                            passed = true;
                        }

                        foreach (var texture in textures)
                        {
                            texture.TextureName = string.Format("{0}{1:x}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            _textureAddedAction(texture, reader.BaseStream.Position);
                            Program.Logger.WritePositiveLine("Found HMD Image at offset {0:X}", _offset);
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
                        Program.Logger.WriteLine($"HMD - Reached file end: {fileTitle}");
                        return;
                    }
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
                }
            }
        }

        private RootEntity ParseHMD(BinaryReader reader, out List<Animation> animations, out List<Texture> textures)
        {
            animations = new List<Animation>();
            textures = new List<Texture>();
            var mapFlag = reader.ReadUInt32();
            var primitiveHeaderTop = reader.ReadUInt32() * 4;
            var blockCount = reader.ReadUInt32();
            if (blockCount == 0 || blockCount > Program.MaxHMDBlockCount)
            {
                return null;
            }
            var modelEntities = new List<ModelEntity>();
            for (uint i = 0; i < blockCount; i++)
            {
                var primitiveSetTop = reader.ReadUInt32() * 4;
                if (primitiveSetTop == 0)
                {
                    continue;
                }
                var blockTop = reader.BaseStream.Position;
                ProccessPrimitive(reader, modelEntities, animations, textures, i, primitiveSetTop, primitiveHeaderTop);
                reader.BaseStream.Seek(blockTop, SeekOrigin.Begin);
            }
            RootEntity rootEntity;
            if (modelEntities.Count > 0)
            {
                rootEntity = new RootEntity();
                foreach (var modelEntity in modelEntities)
                {
                    modelEntity.ParentEntity = rootEntity;
                }
                rootEntity.ChildEntities = modelEntities.ToArray();
                rootEntity.ComputeBounds();
            }
            else
            {
                rootEntity = null;
            }
            // Read hierarchical coordinates table.
            var coordTop = (uint)(reader.BaseStream.Position - _offset);
            var coordCount = reader.ReadUInt32();
            var coords = new CoordUnit[coordCount];
            for (uint c = 0; c < coordCount; c++)
            {
                var coord = ReadCoord(reader, coordTop, c, coords);
                if (coord == null)
                {
                    return null; // Bad coord unit.
                }
                coords[c] = coord;
            }
            // Now that the table is fully read, ensure no circular references in coord parents.
            foreach (var coord in coords)
            {
                if (coord.HasCircularReference())
                {
                    return null; // Bad coord with parents that reference themselves.
                }
            }
            // All coords are safe to use. Assign coords to models.
            foreach (var coord in coords)
            {
                var localMatrix = coord.WorldMatrix;
                foreach (var modelEntity in modelEntities)
                {
                    if (modelEntity.TMDID == coord.TMDID)
                    {
                        modelEntity.OriginalLocalMatrix = localMatrix;
                    }
                }
            }
            // Assign coords table to root entity so that they can be used in animations.
            if (rootEntity != null)
            {
                rootEntity.Coords = coords;
            }
            var primitiveHeaderCount = reader.ReadUInt32();
            return rootEntity;
        }

        private CoordUnit ReadCoord(BinaryReader reader, uint coordTop, uint coordID, CoordUnit[] coords)
        {
            var flag = reader.ReadUInt32();
            var localMatrix = ReadMatrix(reader, out var translation);
            var workMatrix = ReadMatrix(reader, out _);
            
            var rx = (float)(reader.ReadInt16() / 4096.0 * (Math.PI * 2.0));
            var ry = (float)(reader.ReadInt16() / 4096.0 * (Math.PI * 2.0));
            var rz = (float)(reader.ReadInt16() / 4096.0 * (Math.PI * 2.0));
            var pad = reader.ReadInt16();
            var rotation = new Vector3(rx, ry, rz);

            var super = reader.ReadUInt32() * 4;
            if (super != 0)
            {
                if (super < coordTop + 4)
                {
                    return null; // Before coord table.
                }
                super -= coordTop + 4; // Change to start of coord table.
                if (super % 80 != 0)
                {
                    return null; // Not aligned to coord table.
                }
                super /= 80; // Divide by size of CoordUnit to get super ID.
                if (super == coordID || super >= coords.Length)
                {
                    return null; // Bad parent ID.
                }
            }
            else
            {
                super = CoordUnit.NoID;
            }

            return new CoordUnit
            {
                OriginalLocalMatrix = localMatrix,
                OriginalTranslation = translation,
                OriginalRotation = rotation,
                ID = coordID,
                ParentID = super,
                Coords = coords,
            };
        }

        private static Matrix4 ReadMatrix(BinaryReader reader, out Vector3 translation)
        {
            float r00 = reader.ReadInt16() / 4096f;
            float r01 = reader.ReadInt16() / 4096f;
            float r02 = reader.ReadInt16() / 4096f;

            float r10 = reader.ReadInt16() / 4096f;
            float r11 = reader.ReadInt16() / 4096f;
            float r12 = reader.ReadInt16() / 4096f;

            float r20 = reader.ReadInt16() / 4096f;
            float r21 = reader.ReadInt16() / 4096f;
            float r22 = reader.ReadInt16() / 4096f;

            var x = reader.ReadInt32() / 65536f;
            var y = reader.ReadInt32() / 65536f;
            var z = reader.ReadInt32() / 65536f;
            translation = new Vector3(x, y, z);

            var matrix = new Matrix4(
                new Vector4(r00, r10, r20, 0f),
                new Vector4(r01, r11, r21, 0f),
                new Vector4(r02, r12, r22, 0f),
                new Vector4(x, y, z, 1f)
            );
            // It's strange that padding comes after the int32s. Would have expected the int32s to get aligned instead.
            var pad = reader.ReadInt16();
            return matrix;
        }

        private void ProccessPrimitive(BinaryReader reader, List<ModelEntity> modelEntities, List<Animation> animations, List<Texture> textures, uint primitiveIndex, uint primitiveSetTop, uint primitiveHeaderTop)
        {
            var groupedTriangles = new Dictionary<uint, List<Triangle>>();
            var sharedVertices = new Dictionary<uint, Vector3>();
            var sharedNormals = new Dictionary<uint, Vector3>();
            while (true)
            {
                reader.BaseStream.Seek(_offset + primitiveSetTop, SeekOrigin.Begin);
                var nextPrimitivePointer = reader.ReadUInt32();
                var primitiveHeaderPointer = reader.ReadUInt32() * 4;
                ReadMappedValue(reader, out var typeCountMapped, out var typeCount);
                if (typeCount > Program.MaxHMDTypeCount)
                {
                    return;
                }
                for (var j = 0; j < typeCount; j++)
                {
                    //0: Polygon data 1: Shared polygon data 2: Image data 3: Animation data 4: MIMe data 5: Ground data  

                    var type = reader.ReadUInt32();
                    var developerId = (type >> 27) & 0b00001111; //4
                    var category = (type >> 24) & 0b00001111; //4  
                    var driver = (type >> 16) & 0b11111111; //8
                    var primitiveType = type & 0xFFFF; //16
                    
                    // dataSize is the remaining size of this type data in units of 4 bytes.
                    // This size includes the definition of dataSize/dataCount.
                    var typeDataPosition = reader.BaseStream.Position;

                    ReadMappedValue16(reader, out var dataSizeMapped, out var dataSize);
                    ReadMappedValue16(reader, out var dataCountMapped, out var dataCount);

                    if (dataSize > Program.MaxHMDDataSize)
                    {
                        return;
                    }
                    if (dataCount > Program.MaxHMDDataSize)
                    {
                        return;
                    }

                    if (category > 5)
                    {
                        return;
                    }

                    if (Program.Debug)
                    {
                        Program.Logger.WriteLine($"HMD Type: {type} of category {category} and primitive type {primitiveType}");
                    }

                    if (Program.Debug)
                    {
                        Program.Logger.WriteLine("Primitive type bits:" + new BitArray(BitConverter.GetBytes(primitiveType)).ToBitString());
                    }

                    if (category == 0)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Non-Shared Vertices Geometry");
                        }

                        var polygonIndex = reader.ReadUInt32() * 4;
                        ProcessNonSharedGeometryData(groupedTriangles, reader, false, driver, primitiveType, primitiveHeaderPointer, nextPrimitivePointer, polygonIndex, dataCount);
                    }
                    else if (category == 1)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Shared Vertices Geometry");
                        }

                        // You would expect this to be (type == 0x01000000), but examples have been found where the driver was unexpectedly 0x80.
                        var preCalculation = (primitiveType == 0);
                        if (preCalculation)
                        {
                            ProcessSharedGeometryData(sharedVertices, sharedNormals, reader, driver, primitiveHeaderPointer, nextPrimitivePointer);
                        }
                        else
                        {
                            var polygonIndex = reader.ReadUInt32() * 4;
                            ProcessNonSharedGeometryData(groupedTriangles, reader, true, driver, primitiveType, primitiveHeaderPointer, nextPrimitivePointer, polygonIndex, dataCount);
                        }
                    }
                    else if (category == 2)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Image Data");
                        }
                        var hasClut = primitiveType == 1;
                        var texture = ProcessImageData(reader, driver, hasClut, primitiveHeaderPointer, nextPrimitivePointer);
                        if (texture != null)
                        {
                            textures.Add(texture);
                        }
                    }
                    else if (category == 3)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Animation");
                        }
                        try
                        {
                            var addedAnimations = ProcessAnimationData(groupedTriangles, reader, driver, primitiveType, primitiveHeaderPointer, nextPrimitivePointer, dataCount);
                            if (addedAnimations != null)
                            {
                                foreach (var animation in addedAnimations)
                                {
                                    animations.Add(animation);
                                }
                            }
                        }
                        catch (Exception exp)
                        {
                            // Animation support is still experimental, continue reading HMD models even if we fail here.
                            //if (Program.Debug)
                            //{
                            //    Program.Logger.WriteLine(exp);
                            //}
                        }
                    }
                    else if (category == 4)
                    {
                        var code1 = (primitiveType & 0b11100000) > 0;
                        var rst =   (primitiveType & 0b00010000) > 0;
                        var code0 = (primitiveType & 0b00001110) > 0;
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Mime Animation: {code1}|{rst}|{code0}");
                        }
                        //todo: docs are broken!
                        if (!code0)
                        {
                            var diffTop = reader.ReadUInt32() * 4;
                            Animation animation = null;
                            if (code1)
                            {
                                animation = ProcessMimeVertexData(groupedTriangles, reader, driver, primitiveType, primitiveHeaderPointer, nextPrimitivePointer, diffTop, dataCount, rst);
                            }
                            if (animation != null)
                            {
                                animations.Add(animation);
                            }
                        }
                    }
                    else if (category == 5)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Grid");
                        }
                        var polygonIndex = reader.ReadUInt32() * 4;
                        var gridIndex = reader.ReadUInt32() * 4;
                        var vertexIndex = reader.ReadUInt32() * 4;
                        // TODO: Is this actually supposed to pass dataCount and not dataSize?
                        // Originally dataSize/dataCount were read in the wrong order, so what was assumed to be the dataCount variable was used here (but was really dataSize).
                        // Is the divide-by-4 here because the value needed to be transformed into something that passed the tests?
                        ProcessGroundData(groupedTriangles, reader, driver, primitiveType, primitiveHeaderPointer, nextPrimitivePointer, polygonIndex, dataSize / 4, gridIndex, vertexIndex);
                    }

                    // Seek to the next type. This is necessary since not all types will fully read up to the next type (i.e. Image Data).
                    reader.BaseStream.Seek(typeDataPosition + dataSize * 4, SeekOrigin.Begin);
                }
                if (nextPrimitivePointer != 0xFFFFFFFF)
                {
                    primitiveSetTop = nextPrimitivePointer * 4;
                    continue;
                }
                break;
            }
            foreach (var key in groupedTriangles.Keys)
            {
                var triangles = groupedTriangles[key];
                var model = new ModelEntity
                {
                    Triangles = triangles.ToArray(),
                    TexturePage = key,
                    TMDID = primitiveIndex, //todo
                    //PrimitiveIndex = primitiveIndex
                };
                if (sharedVertices.Count > 0 || sharedNormals.Count > 0)
                {
                    // We can add shared geometry onto this existing model, instead of adding a dummy model.
                    // A model is used so that it can transform the shared vertices.
                    model.AttachableVertices = sharedVertices;
                    model.AttachableNormals = sharedNormals;
                    // Reset dictionaries so that we don't add shared geometry again for this block.
                    sharedVertices = new Dictionary<uint, Vector3>();
                    sharedNormals = new Dictionary<uint, Vector3>();
                }
                modelEntities.Add(model);
            }
            if (sharedVertices.Count > 0 || sharedNormals.Count > 0)
            {
                // No models were added for this primitive index, add a dummy model.
                var sharedModel = new ModelEntity
                {
                    Triangles = new Triangle[0], // No triangles. Is it possible this could break exporters?
                    TMDID = primitiveIndex, //todo
                    //PrimitiveIndex = primitiveIndex
                    Visible = false,
                    AttachableVertices = sharedVertices,
                    AttachableNormals = sharedNormals,
                };
                modelEntities.Add(sharedModel);
            }
        }

        private Texture ProcessImageData(BinaryReader reader, uint driver, bool hasClut, uint primitiveHeaderPointer, uint nextPrimitivePointer)
        {
            var position = reader.BaseStream.Position;
            ProcessImageDataPrimitiveHeader(reader, primitiveHeaderPointer, out var imageTop, out var clutTop);
            reader.BaseStream.Seek(position, SeekOrigin.Begin); // Seek is redundant, but follows same convention as other Process functions.
            
            var x = reader.ReadUInt16();
            var y = reader.ReadUInt16();
            var width = reader.ReadUInt16();
            var height = reader.ReadUInt16();
            if (width == 0 || height == 0 || width > 256 || height > 256)
            {
                return null;
            }

            var imageIndex = reader.ReadUInt32() * 4;
            uint pmode;
            System.Drawing.Color[] palette;
            if (hasClut)
            {
                var clutX = reader.ReadUInt16();
                var clutY = reader.ReadUInt16();
                var clutWidth = reader.ReadUInt16();
                var clutHeight = reader.ReadUInt16();
                var clutIndex = reader.ReadUInt32() * 4;

                // NOTE: Width*height always seems to be 16 or 256.
                //       Specifically width was 16 or 256 and height was 1.
                //       With that, it's safe to assume the dimensions tell us the color count.
                //       Because this data could potentionally give us something other than 16 or 256,
                //       assume anything greater than 16 will allocate a 256clut and only read w*h colors.
                pmode = (clutWidth * clutHeight <= 16) ? 0u : 1u; // 16clut or 256clut
                
                reader.BaseStream.Seek(_offset + clutTop + clutIndex, SeekOrigin.Begin);
                // Allow out of bounds to support HMDs with invalid image data, but valid model data.
                palette = TIMParser.ReadPalette(reader, pmode, clutWidth, clutHeight, true);
            }
            else
            {
                pmode = 3u; // 24bpp
                palette = null;
            }
            reader.BaseStream.Seek(_offset + imageTop + imageIndex, SeekOrigin.Begin);
            // Allow out of bounds to support HMDs with invalid image data, but valid model data.
            var texture = TIMParser.ReadTexture(reader, width, height, x, y, pmode, palette, true);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
            return texture;
        }

        private static void ReadMappedValue(BinaryReader reader, out uint mapped, out uint value)
        {
            var valueMapped = reader.ReadUInt32();
            mapped = (valueMapped >> 31) & 0b00000001;
            value = valueMapped & 0b01111111111111111111111111111111;
        }

        private static void ReadMappedValue16(BinaryReader reader, out uint mapped, out uint value)
        {
            var valueMapped = reader.ReadUInt16();
            mapped = (uint)((valueMapped >> 15) & 0b00000001);
            value = (uint)(valueMapped & 0b0111111111111111);
        }

        private Vector3 ReadVertex(BinaryReader reader, uint vertTop, uint index)
        {
            var position = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + vertTop + index * 8, SeekOrigin.Begin);
            var x = reader.ReadInt16();
            var y = reader.ReadInt16();
            var z = reader.ReadInt16();
            var pad = reader.ReadInt16();
            var vertex = new Vector3(x, y, z);
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
            return vertex;
        }

        private Vector3 ReadNormal(BinaryReader reader, uint normTop, uint index)
        {
            var position = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + normTop + index * 8, SeekOrigin.Begin);
            var nx = TMDHelper.ConvertNormal(reader.ReadInt16());
            var ny = TMDHelper.ConvertNormal(reader.ReadInt16());
            var nz = TMDHelper.ConvertNormal(reader.ReadInt16());
            var pad = FInt.Create(reader.ReadInt16());
            var normal = new Vector3(nx, ny, nz);
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
            return normal;
        }

        private void ProcessNonSharedGeometryData(Dictionary<uint, List<Triangle>> groupedTriangles, BinaryReader reader, bool shared, uint driver, uint primitiveType, uint primitiveHeaderPointer, uint nextPrimitivePointer, uint polygonIndex, uint dataCount)
        {
            var primitivePosition = reader.BaseStream.Position;
            uint dataTop, vertTop, normTop, coordTop;
            if (!shared)
            {
                ProcessGeometryPrimitiveHeader(reader, primitiveHeaderPointer, out vertTop, out normTop, out coordTop, out dataTop);
            }
            else
            {
                // Post-processing driver for shared geometry data.
                ProcessSharedGeometryPrimitiveHeader(reader, primitiveHeaderPointer, out dataTop, out vertTop, out var calcVertTop, out normTop, out var calcNormTop, out coordTop);
            }
            reader.BaseStream.Seek(_offset + dataTop + polygonIndex, SeekOrigin.Begin);
            for (var j = 0; j < dataCount; j++)
            {
                var packetStructure = TMDHelper.CreateHMDPacketStructure(driver, primitiveType, reader);
                //var offset = reader.BaseStream.Position;
                if (packetStructure != null)
                {
                    TMDHelper.AddTrianglesToGroup(groupedTriangles, packetStructure, shared,
                        index =>
                        {
                            if (shared)
                            {
                                return Vector3.Zero; // This is an attached vertex.
                            }
                            return ReadVertex(reader, vertTop, index);
                        },
                        index =>
                        {
                            if (shared)
                            {
                                return Vector3.UnitZ; // This is an attached normal. Return Unit vector in-case it somehow gets used in a calculation.
                            }
                            return ReadNormal(reader, normTop, index);
                        }
                    );
                }
            }
            reader.BaseStream.Seek(primitivePosition, SeekOrigin.Begin);
        }

        private void ProcessSharedGeometryData(Dictionary<uint, Vector3> sharedVertices, Dictionary<uint, Vector3> sharedNormals, BinaryReader reader, uint driver, uint primitiveHeaderPointer, uint nextPrimitivePointer)
        {
            // Pre-calculation driver for shared geometry data.
            var primitivePosition = reader.BaseStream.Position;
            ProcessSharedGeometryPrimitiveHeader(reader, primitiveHeaderPointer, out var dataTop, out var vertTop, out var calcVertTop, out var normTop, out var calcNormTop, out var coordTop);
            reader.BaseStream.Seek(primitivePosition, SeekOrigin.Begin);

            // todo: Figure out what to do when dst != src. Is dst the lookup index?
            var vertCount = reader.ReadUInt32();
            var vertSrcOffset = reader.ReadUInt32();
            var vertDstOffset = reader.ReadUInt32();

            var normCount = reader.ReadUInt32();
            var normSrcOffset = reader.ReadUInt32();
            var normDstOffset = reader.ReadUInt32();

            for (uint i = 0; i < vertCount; i++)
            {
                var index = vertSrcOffset + i;
                sharedVertices[index] = ReadVertex(reader, vertTop, index);
            }
            for (uint i = 0; i < normCount; i++)
            {
                var index = normSrcOffset + i;
                sharedNormals[index] = ReadNormal(reader, normTop, index);
            }

            reader.BaseStream.Seek(primitivePosition, SeekOrigin.Begin);
        }

        private List<Animation> ProcessAnimationData(Dictionary<uint, List<Triangle>> groupedTriangles, BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer, uint nextPrimitivePointer, uint dataCount)
        {
            var primitivePosition = reader.BaseStream.Position;
            ProcessAnimationPrimitiveHeader(reader, primitiveHeaderPointer, out var interpTop, out var ctrlTop, out var paramTop, out var coordTop, out var sectionList);

            reader.BaseStream.Seek(primitivePosition, SeekOrigin.Begin);


            var animationList = new List<Animation>();
            var animationObjectsList = new List<Dictionary<uint, AnimationObject>>();

            AnimationFrame GetNextAnimationFrame(int index, AnimationObject animationObject, uint frameTime, bool assignFrame)
            {
                var animationFrames = animationObject.AnimationFrames;
                var frame = new AnimationFrame { FrameTime = frameTime, AnimationObject = animationObject };

                // We need to support overwriting frames with the same time due to tframe=0 normal instructions.
                // These instructions are used to transition between different interpolation types,
                // and also act as the start of the sequence.
                if (assignFrame)
                {
                    //animationFrames.Add(frameTime, frame);
                    animationFrames[frameTime] = frame;
                }

                if (frameTime >= animationList[index].FrameCount)
                {
                    animationList[index].FrameCount = frameTime + 1;
                }
                return frame;
            }
            AnimationObject GetAnimationObject(int index, uint objectId)
            {
                while (index >= animationObjectsList.Count)
                {
                    animationObjectsList.Add(new Dictionary<uint, AnimationObject>());
                }
                if (animationObjectsList[index].ContainsKey(objectId))
                {
                    return animationObjectsList[index][objectId];
                }
                var animationObject = new AnimationObject { Animation = animationList[index], ID = objectId };
                animationObject.TMDID.Add(objectId); // TMDID and objectId are the same.
                animationObjectsList[index].Add(objectId, animationObject);
                return animationObject;
            }
            Animation GetAnimation(int index)
            {
                while (index >= animationList.Count)
                {
                    animationList.Add(new Animation());
                }
                return animationList[index];
            }


            // Debug: Print instruction set. Programmatically disabled by default since it wastes a lot of time.
#if false
            if (Program.Debug)
            {
                HMDHelper.PrintAnimInstructions(reader, ctrlTop, paramTop, _offset);
            }
#endif
            
            var tgt = ((driver     ) & 0xf);
            var cat = ((driver >> 4) & 0x7);
            var ini = ((driver >> 7) & 0x1) == 1;

            if (tgt != 0)
            {
                // TGT = 1:
                // General update driver
                // Not supported yet. This would be updates to individual vertices or normals.
                if (Program.Debug)
                {
                    Program.Logger.WriteLine($"Unsupported animation TGT {tgt}");
                }
                return null;
            }

            // Cache places we're going to be reading from multiple times.
            var descriptors = new Dictionary<uint, uint>();
            var interpTypes = new Dictionary<uint, uint>();
            // Debug: Track if we can support the model's speed.
            var speeds = new HashSet<int>();

            // TGT = 0:
            // Coordinate update driver
            for (uint i = 0; i < dataCount; i++)
            {
                var sequencePointerPosition = reader.BaseStream.Position;

                ReadMappedValue(reader, out _, out var updateIndex);
                var updateSectionOffset = updateIndex >> 24;
                var updateOffsetInSection = (updateIndex & 0xffffff) * 4;

                if (updateOffsetInSection < 4 || (updateOffsetInSection - 4) % 80 != 0)
                {
                    if (Program.Debug)
                    {
                        Program.Logger.WriteLine($"Invalid animation updateOffsetInSection {updateOffsetInSection}");
                    }
                    return null; // Coordinate offset starts before table, or offset is not aligned.
                }
                var tmdid = (updateOffsetInSection - 4) / 80 + 1; // Divide by size of coord uint. Coord TMDIDs are 1-indexed.

                ReadMappedValue16(reader, out _, out var sequenceSize);
                ReadMappedValue16(reader, out _, out var sequenceCount);
                
                var interpIdx = reader.ReadUInt16();
                var aframe = reader.ReadUInt16();
                
                var streamID = reader.ReadByte();
                var speed = reader.ReadSByte();
                var srcInterpIdx = reader.ReadUInt16();
                
                var rframe = reader.ReadUInt16();
                var tframe = reader.ReadUInt16();
                
                var ctrIdx = reader.ReadUInt16();
                var tctrIdx = reader.ReadUInt16();
                
                var startIdx = new ushort[sequenceCount];
                var startSID = new byte[sequenceCount];
                var traveling = new byte[sequenceCount]; // Note: Only the first index is used

                for (var j = 0; j < sequenceCount; j++)
                {
                    startIdx[j] = reader.ReadUInt16();
                    startSID[j] = reader.ReadByte();
                    traveling[j] = reader.ReadByte();
                }

                // Track if multiple speeds are defined for different objects, we can't handle that yet.
                speeds.Add(Math.Abs((int)speed));
                
                for (var j = 0; j < sequenceCount; j++)
                {
                    // This is the first animation object for this sequence, rely on this for frame rate.
                    if (j >= animationList.Count) // i == 0)
                    {
                        var animation = GetAnimation(j);
                        // Speed is stored in fixed point with lowest 4 bits representing fraction.
                        // todo: Handle difference between PAL (25fps) and US (30fps) speeds.
                        animation.FPS = 25f * (Math.Abs(speed) / 16f); // todo: Should we really rely on this?
                    }

                    var animationObject = GetAnimationObject(j, tmdid);

                    AnimationFrame lastAnimationFrame = null;
                    uint lastTFrame = 0;

                    uint idx = startIdx[j]; // Current starting instruction pointer.
                    uint sid = startSID[j]; // Current StreamID, acts like a control flow variable.
                    uint time = 0; // Current frame time.
                    var processedCount = 0; // Debug information: Number of instructions encountered
                    var halted = false; // Debug information: END instruction encountered

                    // List of encountered instructions with SIDs they were encountered with.
                    // Once we reach an instruction that we've already reached with the same SID, then we've hit an infinite loop.
                    var visited = new Dictionary<uint, HashSet<uint>>();

                    // Execute instructions to find all frames (Normal instructions).
                    while (idx < ushort.MaxValue)
                    {
                        // Infinite loop check.
                        if (!visited.TryGetValue(idx, out var sidSet))
                        {
                            sidSet = new HashSet<uint>();
                            visited.Add(idx, sidSet);
                        }
                        if (!sidSet.Add(sid))
                        {
                            // Infinite loop.
                            // todo: Animation looping might rely on the first frame of the infinite loop,
                            //       so we may need to parse one more normal frame.
                            // todo: Handle animation loops that don't have the same frame lengths for different objects.
                            if (Program.Debug)
                            {
                                Program.Logger.WriteLine($"Infinite loop in animation @{idx} #{sid}");
                            }
                            break;
                        }

                        // Cache already-read instructions.
                        if (!descriptors.TryGetValue(idx, out var descriptor))
                        {
                            reader.BaseStream.Seek(_offset + ctrlTop + idx * 4, SeekOrigin.Begin);
                            descriptor = reader.ReadUInt32();
                            descriptors.Add(idx, descriptor);
                        }
                        var descriptorType = (descriptor >> 30) & 0x3;

                        if ((descriptorType & 0x2) == 0x0) // Normal (second MSB is used by instruction)
                        {
                            // SPAGHETTI ALERT:
                            // The way frames, next frames, final parameters and interpolation is
                            // handled makes the processing of nextTFrame parameters very clunky.
                            // Especially because we need to pre-compute animations.

                            // Standard instruction used for interpolation.
                            var paramIndex  = (descriptor >>  0) & 0xffff; // Index to parameter data for key frame referred to by sequence descriptor.
                            var nextTFrame  = (descriptor >> 16) & 0xff; // Frame number of next sequence descriptor (int).
                            var interpIndex = (descriptor >> 24) & 0x7f; // Index into interpolation table. Specifies function to be used.
                            
                            // Cache already-read interpolation (animation) types.
                            if (!interpTypes.TryGetValue(interpIndex, out var animationType))
                            {
                                // +4 to skip type count
                                reader.BaseStream.Seek(_offset + interpTop + 4 + interpIndex * 4, SeekOrigin.Begin);
                                animationType = reader.ReadUInt32();
                                interpTypes.Add(interpIndex, animationType);
                            }
                            
                            // We don't know if we want to assign this frame until we run into an instruction where TFrame!=0.
                            // Make a special exception for the first animation frame.
                            var assignFrame = (animationObject.AnimationFrames.Count == 0 || nextTFrame != 0);
                            var animationFrame = GetNextAnimationFrame(j, animationObject, time + nextTFrame, assignFrame);

                            // Don't overwrite the last animation frame when we run into a chain of TFrame==0 instructions.
                            // Only overwrite once we encounter an instruction where TFrame!=0.
                            if (lastTFrame == 0 && nextTFrame != 0)
                            {
                                animationObject.AnimationFrames[time] = lastAnimationFrame;
                            }

                            // Don't assign the Final* interpolation parameters to lastAnimationFrame if TFrame==0.
                            // This frame is a dummy frame used to transition to the next interpolation type. Or is used in some other incorrect way.
                            var argLastAnimationFrame = (nextTFrame != 0 ? lastAnimationFrame : null);

                            reader.BaseStream.Seek(_offset + paramTop + paramIndex * 4, SeekOrigin.Begin);
                            if (!HMDHelper.ReadAnimPacket(reader, animationType, animationFrame, argLastAnimationFrame))
                            {
                                if (Program.Debug)
                                {
                                    Program.Logger.WriteLine($"Invalid/unsupported animation type 0x{animationType:x08} @{idx} #{sid}");
                                }
                                return null; // Unsupported animation packet.
                            }
                            lastAnimationFrame = animationFrame;
                            lastTFrame = nextTFrame;
                            time += nextTFrame;
                        }
                        else if (descriptorType == 0x2) // Jump
                        {
                            var seqIndex = (descriptor >>  0) & 0xffff; // Next control descriptor to jump to.
                            var cnd      = (descriptor >> 16) & 0x7f; // Stream ID conditional jump.
                            var dst      = (descriptor >> 23) & 0x7f; // Stream ID destination of jump.
                            // todo: How to handle SID 127, is this for sid, or cnd???
                            if (cnd == 0 || cnd == sid || (cnd == 127 && sid == 0))
                            {
                                if (cnd != 0 || dst != 0)
                                {
                                    sid = dst;
                                }
                                idx = seqIndex;
                                processedCount++;
                                continue;
                            }
                        }
                        else if (descriptorType == 0x3) // Control
                        {
                            var code = (descriptor >> 23) & 0x7f;
                            var p1   = (descriptor >> 16) & 0x7f;
                            var p2   = (descriptor >>  0) & 0xffff;
                            if (code == 1) // End
                            {
                                // If P1 matches current Stream ID, sequence is halted.
                                // todo: Is the SID 127 condition supported here too?
                                if (p1 == 0 || p1 == sid || (p1 == 127 && sid == 0))
                                {
                                    halted = true;
                                    processedCount++;
                                    break;
                                }
                            }
                            else if (code == 2) // Work
                            {
                                // Work area for sequence pointer required by B-Spline interpolation.
                                // p1: 127 fixed
                                // p2: Offset (in 4-byte units) in parameter section indicates work area.
                                // B-Splines arent't supported yet.
                            }
                            else
                            {
                                // Invalid code. Should we return null here?
                                return null;
                            }
                        }

                        if (speed >= 0) idx++;
                        else            idx--;
                        processedCount++;
                    }
                }

                // Seek to the start of the next sequence pointer.
                reader.BaseStream.Seek(sequencePointerPosition + sequenceSize * 4, SeekOrigin.Begin);
            }

            if (speeds.Count > 1)
            {
                if (Program.Debug)
                {
                    Program.Logger.WriteLine($"Unsupported multiple speeds defined for different animation objects");
                }
            }

            for (var j = 0; j < animationList.Count; j++)
            {
                var animation = animationList[j];
                var animationObjects = animationObjectsList[j];
                var rootAnimationObject = new AnimationObject();
                foreach (var animationObject in animationObjects.Values)
                {
                    // Assign frame durations.
                    var animationFrames = animationObject.AnimationFrames;
                    AnimationFrame lastAnimationFrame = null;
                    foreach (var animationFrame in animationFrames.Values.OrderBy(af => af.FrameTime))
                    {
                        if (lastAnimationFrame != null)
                        {
                            lastAnimationFrame.FrameDuration = animationFrame.FrameTime - lastAnimationFrame.FrameTime;
                        }
                        lastAnimationFrame = animationFrame;
                    }

                    // No parenting.
                    /*if (animationObject.ParentID != 0 && animationObjects.ContainsKey(animationObject.ParentID))
                    {
                        var parent = animationObjects[animationObject.ParentID];
                        animationObject.Parent = parent;
                        parent.Children.Add(animationObject);
                        continue;
                    }*/
                    animationObject.Parent = rootAnimationObject;
                    rootAnimationObject.Children.Add(animationObject);
                }
                animation.AnimationType = AnimationType.HMD;
                animation.RootAnimationObject = rootAnimationObject;
                animation.ObjectCount = animationObjects.Count;
            }

            return animationList;
        }

        private Animation ProcessMimeVertexData(Dictionary<uint, List<Triangle>> groupedTriangles, BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer, uint nextPrimitivePointer, uint diffTop, uint dataCount, bool rst)
        {
            Animation animation;
            Dictionary<uint, AnimationObject> animationObjects;
            AnimationFrame GetNextAnimationFrame(AnimationObject animationObject)
            {
                var animationFrames = animationObject.AnimationFrames;
                var frameTime = (uint)animationFrames.Count;
                var frame = new AnimationFrame { FrameTime = frameTime, AnimationObject = animationObject };
                animationFrames.Add(frameTime, frame);
                if (frameTime >= animation.FrameCount)
                {
                    animation.FrameCount = frameTime + 1;
                }
                return frame;
            }
            AnimationObject GetAnimationObject(uint objectId)
            {
                if (animationObjects.ContainsKey(objectId))
                {
                    return animationObjects[objectId];
                }
                var animationObject = new AnimationObject { Animation = animation, ID = objectId };
                animationObject.TMDID.Add(objectId);
                animationObjects.Add(objectId, animationObject);
                return animationObject;
            }
            animation = new Animation();
            var rootAnimationObject = new AnimationObject();
            animationObjects = new Dictionary<uint, AnimationObject>();
            var primitiveDataTop = reader.BaseStream.Position;
            ProcessMimeVertexPrimitiveHeader(reader, primitiveHeaderPointer, out var coordTop, out var mimeDiffTop, out var mimeOrgTop, out var mimeVertTop, out var mimeNormTop, out var mimeTop);
            reader.BaseStream.Seek(primitiveDataTop, SeekOrigin.Begin);
            for (uint i = 0; i < dataCount; i++)
            {
                reader.BaseStream.Seek(_offset + mimeDiffTop, SeekOrigin.Begin);
                var oNum = reader.ReadUInt16();
                var numDiffs = reader.ReadUInt16();
                if (numDiffs > Program.MaxHMDMimeDiffs)
                {
                    return null;
                }
                var flags = reader.ReadUInt32();
                var animationObject = GetAnimationObject(oNum);
                for (uint j = 0; j < numDiffs; j++)
                {
                    var position = reader.BaseStream.Position;
                    var diffDataTop = reader.ReadUInt32() * 4;
                    reader.BaseStream.Seek(_offset + mimeDiffTop + diffTop + diffDataTop, SeekOrigin.Begin);
                    var vertexStart = reader.ReadUInt32();
                    var res = reader.ReadUInt16();
                    var vertexCount = reader.ReadUInt16();
                    if (vertexCount + vertexStart == 0 || vertexCount + vertexStart >= Program.MaxHMDVertCount)
                    {
                        return null;
                    }
                    var animationFrame = GetNextAnimationFrame(animationObject);
                    var vertices = new Vector3[vertexCount + vertexStart];
                    for (var k = 0; k < vertexCount; k++)
                    {
                        var x = reader.ReadInt16();
                        var y = reader.ReadInt16();
                        var z = reader.ReadInt16();
                        var pad = reader.ReadUInt16();
                        vertices[vertexStart + k] = new Vector3(x, y, z);
                    }
                    animationFrame.Vertices = vertices;
                    animationFrame.TempVertices = new Vector3[animationFrame.Vertices.Length];
                    reader.BaseStream.Seek(position, SeekOrigin.Begin);
                }
                if (flags == 1)
                {
                    var resetOffset = reader.ReadUInt32() * 4;
                }
            }
            foreach (var animationObject in animationObjects.Values)
            {
                if (animationObject.ParentID != 0 && animationObjects.ContainsKey(animationObject.ParentID))
                {
                    var parent = animationObjects[animationObject.ParentID];
                    animationObject.Parent = parent;
                    parent.Children.Add(animationObject);
                    continue;
                }
                animationObject.Parent = rootAnimationObject;
                rootAnimationObject.Children.Add(animationObject);
            }
            animation.AnimationType = AnimationType.VertexDiff;
            animation.RootAnimationObject = rootAnimationObject;
            animation.ObjectCount = animationObjects.Count;
            animation.FPS = 1f;
            return animation;
        }

        private void ProcessGroundData(Dictionary<uint, List<Triangle>> groupedTriangles, BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer, uint nextPrimitivePointer, uint polygonIndex, uint dataCount, uint gridIndex, uint vertexIndex)
        {
            void AddTriangle(Triangle triangle, uint tPageNum)
            {
                List<Triangle> triangles;
                if (groupedTriangles.ContainsKey(tPageNum))
                {
                    triangles = groupedTriangles[tPageNum];
                }
                else
                {
                    triangles = new List<Triangle>();
                    groupedTriangles.Add(tPageNum, triangles);
                }
                triangles.Add(triangle);
            }

            ProcessGroundPrimitiveHeader(reader, primitiveHeaderPointer, primitiveType, polygonIndex, out var vertTop, out var normTop, out var polyTop, out var uvTop, out var gridTop, out var coordTop);

            for (var j = 0; j < dataCount; j++)
            {
                //polygon
                reader.BaseStream.Seek(_offset + polyTop + polygonIndex, SeekOrigin.Begin);
                var x0 = reader.ReadInt16();
                var y0 = reader.ReadInt16();
                var w = reader.ReadUInt16();
                var h = reader.ReadUInt16();
                var m = reader.ReadUInt16();
                var n = reader.ReadUInt16();
                var size = reader.ReadUInt16();
                var @base = reader.ReadUInt16();
                var position = reader.BaseStream.Position;
                var gridItemSize = primitiveType == 1 ? 32 : 16;
                for (var row = 0; row < size; row++)
                {
                    var itemVertexIndex = reader.ReadUInt16();
                    var itemGridCount = reader.ReadUInt16();
                    var rowPosition = position;
                    for (var itemGridIndex = 0; itemGridIndex < itemGridCount; itemGridIndex++)
                    {
                        reader.BaseStream.Seek(_offset + gridTop + gridIndex + itemGridIndex * gridItemSize, SeekOrigin.Begin);

                        uint tPage;
                        Color color;
                        Vector3 n0, n1, n2, n3;
                        Vector3 uv0, uv1, uv2, uv3;

                        if (primitiveType == 0)
                        {
                            var r = reader.ReadByte() / 255f;
                            var g = reader.ReadByte() / 255f;
                            var b = reader.ReadByte() / 255f;
                            color = new Color(r, g, b);
                            reader.ReadByte();
                            var normIndex = reader.ReadUInt16();

                            //todo
                            n0 = n1 = n2 = n3 = Vector3.Zero;

                            reader.ReadUInt16();
                            tPage = 0;
                            uv0 = uv1 = uv2 = uv3 = Vector3.Zero;
                        }
                        else
                        {
                            var normIndex = reader.ReadUInt16();
                            var uvIndex = reader.ReadUInt16();

                            //todo
                            tPage = 0;
                            uv0 = uv1 = uv2 = uv3 = Vector3.Zero;

                            color = Color.Grey;
                            n0 = n1 = n2 = n3 = Vector3.Zero;
                        }

                        var columnPosition = position;
                        reader.BaseStream.Seek(_offset + vertTop + vertexIndex + itemVertexIndex * 4, SeekOrigin.Begin);
                        var z0 = reader.ReadInt16();
                        //var z1 = reader.ReadInt16();
                        //var z2 = reader.ReadInt16();
                        //var z3 = reader.ReadInt16();
                        var z1 = z0;
                        var z2 = z0;
                        var z3 = z0;
                        reader.BaseStream.Seek(columnPosition, SeekOrigin.Begin);

                        var vertex0 = new Vector3(x0 + w * row, y0 + h * itemGridIndex, z0);
                        var vertex1 = new Vector3(x0 + w * (row + 1), y0 + h * itemGridIndex, z1);
                        var vertex2 = new Vector3(x0 + w * (row + 1), y0 + h * (itemGridIndex + 1), z2);
                        var vertex3 = new Vector3(x0 + w * row, y0 + h * (itemGridIndex + 1), z3);

                        AddTriangle(new Triangle
                        {
                            Vertices = new[] { vertex0, vertex1, vertex2 },
                            Normals = new[] { n0, n1, n2 },
                            Colors = new[] { color, color, color },
                            Uv = new[] { uv0, uv1, uv2 },
                            AttachableIndices = new[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }
                        }, tPage);

                        AddTriangle(new Triangle
                        {
                            Vertices = new[] { vertex2, vertex3, vertex0 },
                            Normals = new[] { n2, n3, n0 },
                            Colors = new[] { color, color, color },
                            Uv = new[] { uv2, uv3, uv0 },
                            AttachableIndices = new[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }
                        }, tPage);
                    }
                    reader.BaseStream.Seek(rowPosition, SeekOrigin.Begin);

                }
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
        }

        private void ProcessGeometryPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint vertTop, out uint normTop, out uint coordTop, out uint dataTop)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();

            ReadMappedValue(reader, out var dataTopMapped, out dataTop);
            ReadMappedValue(reader, out var vertTopMapped, out vertTop);
            ReadMappedValue(reader, out var normTopMaped, out normTop);
            ReadMappedValue(reader, out var coordTopMapped, out coordTop);

            dataTop *= 4;
            vertTop *= 4;
            normTop *= 4;
            coordTop *= 4;

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessSharedGeometryPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint dataTop, out uint vertTop, out uint calcVertTop, out uint normTop, out uint calcNormTop, out uint coordTop)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();

            ReadMappedValue(reader, out var dataTopMapped, out dataTop);
            ReadMappedValue(reader, out var vertTopMapped, out vertTop);
            ReadMappedValue(reader, out var calcVertTopMapped, out calcVertTop);
            ReadMappedValue(reader, out var normTopMaped, out normTop);
            ReadMappedValue(reader, out var calcNormTopMaped, out calcNormTop);
            ReadMappedValue(reader, out var coordTopMapped, out coordTop);

            dataTop *= 4;
            vertTop *= 4;
            calcVertTop *= 4;
            normTop *= 4;
            calcNormTop *= 4;
            coordTop *= 4;

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessImageDataPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint imageTop, out uint clutTop)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();

            ReadMappedValue(reader, out var imageTopMapped, out imageTop);
            ReadMappedValue(reader, out var clutTopMapped, out clutTop);

            imageTop *= 4;
            clutTop *= 4;

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessAnimationPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint interpTop, out uint ctrlTop, out uint paramTop, out uint coordTop, out uint[] sectionList)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();
            var animHeaderSize = reader.ReadUInt32();

            sectionList = new uint[animHeaderSize];
            sectionList[0] = animHeaderSize; // Not a valid section index.
            for (var i = 1; i < animHeaderSize; i++)
            {
                ReadMappedValue(reader, out _, out sectionList[i]);
                sectionList[i] *= 4;
            }

            interpTop = (headerSize >= 2 ? sectionList[1] : 0u);
            ctrlTop   = (headerSize >= 3 ? sectionList[2] : 0u);
            paramTop  = (headerSize >= 4 ? sectionList[3] : 0u);
            coordTop  = (headerSize >= 5 ? sectionList[4] : 0u);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessMimeVertexPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint coordTop, out uint mimeDiffTop, out uint mimeOrgTop, out uint mimeVertTop, out uint mimeNormTop, out uint mimeTop)
        {
            //7; /* header size */
            //M(MIMePr_ptr / 4);
            //MIMe_num;
            //H(MIMeID); H(0 /* reserved */);
            //M(MIMeDiffSect / 4);
            //M(MIMeOrgsVNSect / 4);
            //M(VertSect / 4);
            //M(NormSect / 4);

            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headLen = reader.ReadUInt32();

            //ReadMappedValue(reader, out var coordTopMapped, out coordTop);
            //coordTop *= 4;

            coordTop = 0;

            ReadMappedValue(reader, out var mimeTopMapped, out mimeTop);
            mimeTop *= 4;

            var mimeNum = reader.ReadUInt32();
            var mimeId = reader.ReadUInt16();
            reader.ReadUInt16();

            ReadMappedValue(reader, out var mimeDiffTopMapped, out mimeDiffTop);
            ReadMappedValue(reader, out var mimeOrgTopMapped, out mimeOrgTop);
            ReadMappedValue(reader, out var mimeVertTopMapped, out mimeVertTop);
            ReadMappedValue(reader, out var mimeNormTopMapped, out mimeNormTop);

            mimeDiffTop *= 4;
            mimeOrgTop *= 4;
            mimeVertTop *= 4;
            mimeNormTop *= 4;

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessGroundPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, uint primitiveType, uint polygonIndex, out uint vertTop, out uint normTop, out uint polyTop, out uint uvTop, out uint gridTop, out uint coordTop)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            uvTop = int.MaxValue;
            coordTop = int.MaxValue;

            var headerSize = reader.ReadUInt32();

            ReadMappedValue(reader, out var polyTopMapped, out polyTop);
            ReadMappedValue(reader, out var gridTopMapped, out gridTop);
            ReadMappedValue(reader, out var vertTopMapped, out vertTop);
            ReadMappedValue(reader, out var normTopMapped, out normTop);
            polyTop *= 4;
            gridTop *= 4;
            vertTop *= 4;
            normTop *= 4;

            if (headerSize >= 5)
            {
                ReadMappedValue(reader, out var uvTopMapped, out uvTop);
                uvTop *= 4;
            }
            if (headerSize >= 6)
            {
                ReadMappedValue(reader, out var coordTopMapped, out coordTop);
                coordTop *= 4;
            }

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }
    }
}
