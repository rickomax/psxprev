using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
            var coordCount = reader.ReadUInt32();
            for (var c = 0; c < coordCount; c++)
            {
                var localMatrix = ReadCoord(reader);
                foreach (var modelEntity in modelEntities)
                {
                    if (modelEntity.TMDID == c + 1)
                    {
                        modelEntity.OriginalLocalMatrix = localMatrix;
                    }
                }
            }
            var primitiveHeaderCount = reader.ReadUInt32();
            return rootEntity;
        }

        private Matrix4 ReadCoord(BinaryReader reader)
        {
            var flag = reader.ReadUInt32();
            var worldMatrix = ReadMatrix(reader);
            var workMatrix = ReadMatrix(reader);

            short rx, ry, rz;
            rx = reader.ReadInt16();
            ry = reader.ReadInt16();
            rz = reader.ReadInt16();
            var pad = reader.ReadInt16();

            var super = reader.ReadUInt32() * 4;
            if (super != 0)
            {
                var position = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + super, SeekOrigin.Begin);
                var superMatrix = ReadCoord(reader);
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
                return worldMatrix * superMatrix;
            }
            // todo: Most matrices at coord index 0 are an identity matrix, but we should still really return the actual matrix.
            return Matrix4.Identity; //worldMatrix;
        }

        private static Matrix4 ReadMatrix(BinaryReader reader)
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
                var animationObject = new AnimationObject { Animation = animation, ID = objectId, TMDID = objectId };
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
