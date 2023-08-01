using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using OpenTK;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public class HMDParser : FileOffsetScanner
    {
        public HMDParser(EntityAddedAction entityAdded, TextureAddedAction textureAdded, AnimationAddedAction animationAdded)
            : base(entityAdded, textureAdded, animationAdded)
        {
        }

        public override string FormatName => "HMD";

        protected override void Parse(BinaryReader reader)
        {
            var version = reader.ReadUInt32();
            if (Program.IgnoreHmdVersion || version == 0x00000050)
            {
                var rootEntity = ParseHMD(reader);
                if (rootEntity != null)
                {
                    EntityResults.Add(rootEntity);
                }
            }
        }

        private RootEntity ParseHMD(BinaryReader reader)
        {
            var mapFlag = reader.ReadUInt32();
            var primitiveHeaderTop = reader.ReadUInt32() * 4;
            var blockCount = reader.ReadUInt32();
            if (blockCount == 0 || blockCount > Program.MaxHMDBlockCount)
            {
                return null;
            }
            var modelEntities = new List<ModelEntity>();
            uint sharedIndex = 0;
            for (uint i = 0; i < blockCount; i++)
            {
                var primitiveSetTop = reader.ReadUInt32() * 4;
                if (primitiveSetTop == 0)
                {
                    // We may be skipping the first (pre-process) block, or last (post-process) block.
                    // This also happens if we have a coord used as a parent, but no primitives directly tied to it.
                    continue;
                }
                var blockPosition = reader.BaseStream.Position;
                ProcessPrimitiveSet(reader, modelEntities, i, primitiveSetTop, primitiveHeaderTop, blockCount, ref sharedIndex);
                reader.BaseStream.Seek(blockPosition, SeekOrigin.Begin);
            }
            RootEntity rootEntity;
            if (modelEntities.Count > 0)
            {
                rootEntity = new RootEntity();
                foreach (var modelEntity in modelEntities)
                {
                    modelEntity.ParentEntity = rootEntity;
                }
                if (TextureResults.Count > 0)
                {
                    foreach (var texture in TextureResults)
                    {
                        texture.OwnerEntity = rootEntity;
                    }
                    rootEntity.OwnedTextures.AddRange(TextureResults);
                }
                if (AnimationResults.Count > 0)
                {
                    foreach (var animation in AnimationResults)
                    {
                        animation.OwnerEntity = rootEntity;
                    }
                    rootEntity.OwnedAnimations.AddRange(AnimationResults);
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
            if (coordCount > blockCount - 2)
            {
                // Coord units start after the first (pre-process) block and end before the last (post-process) block.
                // We need to allow at least blockCount - 2 coords, so only check the max cap if we're over that.
                if (coordCount > Program.MaxHMDCoordCount)
                {
                    return null;
                }
                if (Program.Debug)
                {
                    Program.Logger.WriteLine($"coordCount {coordCount} exceeds blockCount - 2 ({blockCount - 2})");
                }
            }
            var coords = new Coordinate[coordCount];
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
            if (Coordinate.FindCircularReferences(coords))
            {
                return null; // Bad coords with parents that reference themselves.
            }
            // All coords are safe to use. Assign coords to models.
            foreach (var coord in coords)
            {
                if (coord.ID + 2 >= blockCount)
                {
                    break; // Coord units can't be assigned to post-process primitives.
                }
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

        private Coordinate ReadCoord(BinaryReader reader, uint coordTop, uint coordID, Coordinate[] coords)
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
                super /= 80; // Divide by size of Coordinate to get super ID.
                if (super == coordID || super >= coords.Length)
                {
                    return null; // Bad parent ID.
                }
            }
            else
            {
                super = Coordinate.NoID;
            }

            return new Coordinate
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
            var r00 = reader.ReadInt16() / 4096f;
            var r01 = reader.ReadInt16() / 4096f;
            var r02 = reader.ReadInt16() / 4096f;

            var r10 = reader.ReadInt16() / 4096f;
            var r11 = reader.ReadInt16() / 4096f;
            var r12 = reader.ReadInt16() / 4096f;

            var r20 = reader.ReadInt16() / 4096f;
            var r21 = reader.ReadInt16() / 4096f;
            var r22 = reader.ReadInt16() / 4096f;

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

        private void ProcessPrimitiveSet(BinaryReader reader, List<ModelEntity> modelEntities, uint primitiveIndex, uint primitiveSetTop, uint primitiveHeaderTop, uint blockCount, ref uint sharedIndex)
        {
            var groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();
            var sharedVertices = new Dictionary<uint, Vector3>();
            var sharedNormals = new Dictionary<uint, Vector3>();
            var hasSharedGeometry = false; // Signals flushing of models when shared indices are read.

            uint chainLength = 0;
            while (true)
            {
                if (++chainLength > Program.MaxHMDPrimitiveChainLength)
                {
                    return;
                }

                reader.BaseStream.Seek(_offset + primitiveSetTop, SeekOrigin.Begin);
                var nextPrimitivePointer = reader.ReadUInt32();
                var primitiveHeaderPointer = reader.ReadUInt32() * 4;
                ReadMappedValue(reader, out var typeCountMapped, out var typeCount);
                // Note: typeCount==0 is valid.
                if (typeCount > Program.MaxHMDTypeCount)
                {
                    return;
                }
                for (var j = 0; j < typeCount; j++)
                {
                    //0: Polygon data 1: Shared polygon data 2: Image data 3: Animation data 4: MIMe data 5: Ground data 6: Envmap data 7: Equipment data

                    var type = reader.ReadUInt32();
                    var developerId   = (type >> 28) &    0xf; //4
                    var category      = (type >> 24) &    0xf; //4
                    var driver        = (type >> 16) &   0xff; //8
                    var primitiveType = (type >>  0) & 0xffff; //16

                    // todo: If developerId != 0 (SCE), then ignore this type data.
                    // But currently we can't just use continue, since seeking is done at the bottom of the loop.

                    // dataSize is the remaining size of this type data in units of 4 bytes.
                    // This size includes the definition of dataSize/dataCount.
                    var typeDataPosition = reader.BaseStream.Position;

                    ReadMappedValue16(reader, out var dataSizeMapped, out var dataSize);
                    ReadMappedValue16(reader, out var dataCountMapped, out var dataCount);
                    dataSize *= 4;

                    if (dataSize == 0 || dataSize > Program.MaxHMDDataSize)
                    {
                        return;
                    }
                    if (dataCount > Program.MaxHMDDataCount)
                    {
                        return;
                    }

                    if (category > 7)
                    {
                        return;
                    }

                    if (Program.Debug)
                    {
                        Program.Logger.WriteLine($"HMD category {category}, driver 0x{driver:x02}, primitive type 0x{primitiveType:x04}");
                    }

                    if (category == 0)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Non-Shared Vertices Geometry");
                        }

                        ProcessGeometryData(groupedTriangles, reader, false, driver, primitiveType, primitiveHeaderPointer, dataCount);
                    }
                    else if (category == 1)
                    {
                        // You would expect this to be (type == 0x01000000), but examples have been found where the driver was unexpectedly 0x80.
                        var preCalculation = (primitiveType == 0);
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Shared Vertices: " + (preCalculation ? "Indices" : "Geometry"));
                        }
                        if (preCalculation)
                        {
                            // Shared indices (attachable)
                            if (hasSharedGeometry)
                            {
                                // Flush models so that previously-defined shared geometry can't reference these shared indices.
                                FlushModels(modelEntities, groupedTriangles, sharedVertices, sharedNormals, primitiveIndex, sharedIndex);
                                sharedIndex++;
                                hasSharedGeometry = false;
                            }
                            ProcessSharedIndicesData(sharedVertices, sharedNormals, reader, driver, primitiveHeaderPointer);
                        }
                        else
                        {
                            // Shared geometry (attached)
                            ProcessGeometryData(groupedTriangles, reader, true, driver, primitiveType, primitiveHeaderPointer, dataCount);
                            // If shared indices are defined after this geometry, then this geometry can't use them.
                            // So make sure the current models are flushed so that we can stop after this shared model.
                            hasSharedGeometry = true;
                        }
                    }
                    else if (category == 2)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Image Data");
                        }
                        var texture = ProcessImageData(reader, driver, primitiveType, primitiveHeaderPointer);
                        if (texture != null)
                        {
                            TextureResults.Add(texture);
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
                            var addedAnimations = ProcessAnimationData(groupedTriangles, reader, driver, primitiveType, primitiveHeaderPointer, dataCount, blockCount);
                            if (addedAnimations != null)
                            {
                                AnimationResults.AddRange(addedAnimations);
                            }
                        }
                        catch (Exception exp)
                        {
                            // Animation support is still experimental, continue reading HMD models even if we fail here.
                            if (Program.ShowErrors)
                            {
                                Program.Logger.WriteExceptionLine(exp, $"Error reading {FormatName} animation {AtOffsetString}");
                            }
                        }
                    }
                    else if (category == 4)
                    {
                        var code0 = ((primitiveType >> 4) & 0x3); // Major code: 1-Joint, 2-Vertex/Normal (docs claim it's 0,1 not 1,2)
                        var reset = ((primitiveType >> 3) & 0x1) == 1;
                        var code1 = ((primitiveType >> 0) & 0x3); // Minor code: 1:0-Axes,   1:1-RPY (Roll-pitch-yaw, docs call it "Row")
                                                                  //             2:0-Vertex, 2:1-Normal
                        if (Program.Debug)
                        {
                            var codeStr = (code0 == 1 ? (code1 == 0 ? "Joint Axes" : "Joint Roll-Pitch-Yaw")
                                                      : (code1 == 0 ? "Vertex" : "Normal"));
                            var resetStr = (reset ? " (Reset)" : "");
                            Program.Logger.WriteLine($"HMD MIMe Animation: {codeStr}{resetStr}");
                        }
                        //todo: docs are broken!
                        Animation animation = null;
                        if (code0 == 1 && !reset) // Reset not supported
                        {
                            // Joint Axes/Roll-Pitch-Yaw: Not supported yet (function doesn't return animation yet)
                            var rpy = code1 == 1;
                            animation = ProcessMIMeJointData(groupedTriangles, reader, driver, primitiveType, primitiveHeaderPointer, dataCount, rpy, reset);
                        }
                        else if (code0 == 2 && code1 == 0 && !reset) // Normal not supported, reset not supported
                        {
                            // Vertex/Normal
                            var normal = code1 == 1;
                            animation = ProcessMIMeVertexNormalData(groupedTriangles, reader, driver, primitiveType, primitiveHeaderPointer, dataCount, normal, reset);
                        }
                        if (animation != null)
                        {
                            AnimationResults.Add(animation);
                        }
                    }
                    else if (category == 5)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Ground");
                        }
                        ProcessGroundData(groupedTriangles, reader, driver, primitiveType, primitiveHeaderPointer);
                    }
                    else if (category == 6)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Environment Map"); // Envmap for short
                        }

                        var shared = ((primitiveType >> 8) & 0x1) == 1;
                        var preCalculation = shared && ((primitiveType >> 12) & 0x7) == 0;
                        if (preCalculation)
                        {
                            // Shared indices (attachable)
                            if (hasSharedGeometry)
                            {
                                // Flush models so that previously-defined shared geometry can't reference these shared indices.
                                FlushModels(modelEntities, groupedTriangles, sharedVertices, sharedNormals, primitiveIndex, sharedIndex);
                                sharedIndex++;
                                hasSharedGeometry = false;
                            }
                            ProcessSharedIndicesData(sharedVertices, sharedNormals, reader, driver, primitiveHeaderPointer);
                        }
                        else if (shared)
                        {
                            // Shared geometry (attached)
                            var flag = primitiveType & 0xeff;
                            ProcessGeometryData(groupedTriangles, reader, true, driver, flag, primitiveHeaderPointer, dataCount);
                            // If shared indices are defined after this geometry, then this geometry can't use them.
                            // So make sure the current models are flushed so that we can stop after this shared model.
                            hasSharedGeometry = true;
                        }
                        else
                        {
                            // Envmap geometry
                            ProcessEnvmapData(groupedTriangles, reader, driver, primitiveType, primitiveHeaderPointer, dataCount);
                        }
                    }
                    else if (category == 7)
                    {
                        if (Program.Debug)
                        {
                            Program.Logger.WriteLine($"HMD Equipment");
                        }
                        // Nothing is done with this data yet.
                        ProcessEquipmentData(groupedTriangles, reader, driver, primitiveType, primitiveHeaderPointer);
                    }

                    // Seek to the next type. This is necessary since not all types will fully read up to the next type (i.e. Image Data).
                    reader.BaseStream.Seek(typeDataPosition + dataSize, SeekOrigin.Begin);
                }
                if (nextPrimitivePointer != 0xFFFFFFFF)
                {
                    if (primitiveSetTop == nextPrimitivePointer * 4)
                    {
                        return; // Infinite loop
                    }
                    primitiveSetTop = nextPrimitivePointer * 4;
                    continue;
                }
                break;
            }

            FlushModels(modelEntities, groupedTriangles, sharedVertices, sharedNormals, primitiveIndex, sharedIndex);
            if (hasSharedGeometry)
            {
                sharedIndex++;
            }
        }

        // Needed to flush triangles and shared indices into models (after shared geometry and before shared indices).
        private void FlushModels(List<ModelEntity> modelEntities, Dictionary<RenderInfo, List<Triangle>> groupedTriangles, Dictionary<uint, Vector3> sharedVertices, Dictionary<uint, Vector3> sharedNormals, uint primitiveIndex, uint sharedIndex)
        {
            foreach (var kvp in groupedTriangles)
            {
                var renderInfo = kvp.Key;
                var triangles = kvp.Value;
                var model = new ModelEntity
                {
                    Triangles = triangles.ToArray(),
                    TexturePage = renderInfo.TexturePage,
                    RenderFlags = renderInfo.RenderFlags,
                    MixtureRate = renderInfo.MixtureRate,
                    TMDID = primitiveIndex, //todo
                    //PrimitiveIndex = primitiveIndex,
                    SharedID = sharedIndex,
                };
                if (sharedVertices.Count > 0 || sharedNormals.Count > 0)
                {
                    // We can add shared indices onto this existing model, instead of adding a dummy model.
                    // A model is used so that it can transform the shared vertices.
                    model.AttachableVertices = new Dictionary<uint, Vector3>(sharedVertices);
                    model.AttachableNormals = new Dictionary<uint, Vector3>(sharedNormals);
                    // Reset dictionaries so that we don't add shared indices again for this block.
                    sharedVertices.Clear();
                    sharedNormals.Clear();
                }
                modelEntities.Add(model);
            }
            if (sharedVertices.Count > 0 || sharedNormals.Count > 0)
            {
                // No models were added for this primitive index, add a dummy model.
                var sharedModel = new ModelEntity
                {
                    Triangles = new Triangle[0], // No triangles. Is it possible this could break exporters?
                    RenderFlags = RenderFlags.None, // Assign flags since None is not the default flags.
                    TMDID = primitiveIndex, //todo
                    //PrimitiveIndex = primitiveIndex,
                    SharedID = sharedIndex,
                    Visible = false,
                    AttachableVertices = new Dictionary<uint, Vector3>(sharedVertices),
                    AttachableNormals = new Dictionary<uint, Vector3>(sharedNormals),
                };
                modelEntities.Add(sharedModel);
                sharedVertices.Clear();
                sharedNormals.Clear();
            }
            groupedTriangles.Clear();
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

        private static void ReadMappedPointer(BinaryReader reader, out uint mapped, out uint pointer)
        {
            ReadMappedValue(reader, out mapped, out pointer);
            pointer *= 4;
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
            var pad = reader.ReadInt16();
            var normal = new Vector3(nx, ny, nz);
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
            return normal;
        }

        private void ProcessGeometryData(Dictionary<RenderInfo, List<Triangle>> groupedTriangles, BinaryReader reader, bool shared, uint driver, uint flag, uint primitiveHeaderPointer, uint dataCount)
        {
            var polygonIndex = reader.ReadUInt32() * 4;

            var primitivePosition = reader.BaseStream.Position;
            uint polyTop, vertTop, normTop, coordTop;
            if (!shared)
            {
                ProcessNonSharedGeometryPrimitiveHeader(reader, primitiveHeaderPointer, out polyTop, out vertTop, out normTop, out coordTop);
            }
            else
            {
                ProcessSharedGeometryPrimitiveHeader(reader, primitiveHeaderPointer, out polyTop, out vertTop, out var calcVertTop, out normTop, out var calcNormTop, out coordTop);
            }
            reader.BaseStream.Seek(_offset + polyTop + polygonIndex, SeekOrigin.Begin);

            for (var j = 0; j < dataCount; j++)
            {
                var packetStructure = TMDHelper.CreateHMDPacketStructure(driver, flag, reader);
                if (packetStructure != null)
                {
                    switch (packetStructure.PrimitiveType)
                    {
                        case PrimitiveType.Triangle:
                        case PrimitiveType.Quad:
                        case PrimitiveType.StripMesh:
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
                                });
                            break;
                    }
                }
            }
            reader.BaseStream.Seek(primitivePosition, SeekOrigin.Begin);
        }

        private void ProcessSharedIndicesData(Dictionary<uint, Vector3> sharedVertices, Dictionary<uint, Vector3> sharedNormals, BinaryReader reader, uint driver, uint primitiveHeaderPointer)
        {
            // Pre-calculation driver for shared indices data.
            var primitivePosition = reader.BaseStream.Position;
            ProcessSharedGeometryPrimitiveHeader(reader, primitiveHeaderPointer, out var polyTop, out var vertTop, out var calcVertTop, out var normTop, out var calcNormTop, out var coordTop);
            reader.BaseStream.Seek(primitivePosition, SeekOrigin.Begin);

            var vertCount = reader.ReadUInt32();
            var vertSrcOffset = reader.ReadUInt32();
            var vertDstOffset = reader.ReadUInt32();

            var normCount = reader.ReadUInt32();
            var normSrcOffset = reader.ReadUInt32();
            var normDstOffset = reader.ReadUInt32();

            // todo: If shared geometry doesn't look right, then the handling for DstOffsets here may be incorrect.
            for (uint i = 0; i < vertCount; i++)
            {
                var vertexIndex = vertSrcOffset + i;
                var lookupIndex = vertDstOffset + i; //vertexIndex;
                sharedVertices[lookupIndex] = ReadVertex(reader, vertTop, vertexIndex);
            }
            for (uint i = 0; i < normCount; i++)
            {
                var normalIndex = normSrcOffset + i;
                var lookupIndex = normDstOffset + i; //normalIndex;
                sharedNormals[lookupIndex] = ReadNormal(reader, normTop, normalIndex);
            }

            reader.BaseStream.Seek(primitivePosition, SeekOrigin.Begin);
        }

        private Texture ProcessImageData(BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer)
        {
            var hasClut = primitiveType == 1;

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
            bool[] semiTransparentPalette;
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
                palette = TIMParser.ReadPalette(reader, pmode, clutWidth, clutHeight, out semiTransparentPalette, true);
            }
            else
            {
                pmode = 2u; // 16bpp
                palette = null;
                semiTransparentPalette = null;
            }
            reader.BaseStream.Seek(_offset + imageTop + imageIndex, SeekOrigin.Begin);
            // Allow out of bounds to support HMDs with invalid image data, but valid model data.
            var texture = TIMParser.ReadTexture(reader, width, height, x, y, pmode, palette, semiTransparentPalette, true);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
            return texture;
        }

        private List<Animation> ProcessAnimationData(Dictionary<RenderInfo, List<Triangle>> groupedTriangles, BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer, uint dataCount, uint blockCount)
        {
            var primitivePosition = reader.BaseStream.Position;
            ProcessAnimationPrimitiveHeader(reader, primitiveHeaderPointer, out var interpTop, out var ctrlTop, out var paramTop, out var coordTop, out var sectionList);

            reader.BaseStream.Seek(primitivePosition, SeekOrigin.Begin);


            var animationList = new List<Animation>();
            var animationObjectsList = new List<Dictionary<uint, AnimationObject>>();

            AnimationFrame GetNextAnimationFrame(int index, AnimationObject animationObject, uint frameTime, bool assignFrame)
            {
                var animationFrames = animationObject.AnimationFrames;
                var frame = new AnimationFrame
                {
                    FrameTime = frameTime,
                    FrameDuration = 1, // Default duration
                    AnimationObject = animationObject
                };

                // We need to support overwriting frames with the same time due to tframe=0 normal instructions.
                // These instructions are used to transition between different interpolation types,
                // and also act as the start of the sequence.
                if (assignFrame)
                {
                    //animationFrames.Add(frameTime, frame);
                    animationFrames[frameTime] = frame;
                }

                return frame;
            }
            AnimationObject GetAnimationObject(int index, uint objectId, bool add)
            {
                while (index >= animationObjectsList.Count)
                {
                    animationObjectsList.Add(new Dictionary<uint, AnimationObject>());
                }
                var tmdid = objectId; // TMDID and objectId are the same.
                while (animationObjectsList[index].ContainsKey(objectId))
                {
                    if (!add)
                    {
                        return animationObjectsList[index][objectId];
                    }
                    // Another animation object exists that modifies the same TMDID.
                    // This and the other object run in parallel.
                    // We need a new objectId.
                    objectId += blockCount;
                }
                var animationObject = new AnimationObject { Animation = animationList[index], ID = objectId };
                animationObject.TMDID.Add(tmdid);
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
            if (Program.Debug && false)
            {
                HMDHelper.PrintAnimInstructions(reader, ctrlTop, paramTop, _offset);
            }
            
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
                    Program.Logger.WriteWarningLine($"Unsupported {FormatName} animation TGT {tgt}");
                }
                return null;
            }

            // Cache places we're going to be reading from multiple times.
            var descriptors = new Dictionary<uint, uint>();
            var interpTypes = new Dictionary<uint, uint>();

            // TGT = 0:
            // Coordinate update driver
            for (uint i = 0; i < dataCount; i++)
            {
                var sequencePointerPosition = reader.BaseStream.Position;

                ReadMappedValue(reader, out _, out var updateIndex);
                var updateSectionIndex = updateIndex >> 24;
                var updateOffsetInSection = (updateIndex & 0xffffff) * 4;

                if (updateOffsetInSection < 4 || (updateOffsetInSection - 4) % 80 != 0)
                {
                    if (Program.Debug)
                    {
                        Program.Logger.WriteErrorLine($"Invalid {FormatName} animation updateOffsetInSection {updateOffsetInSection}");
                    }
                    return null; // Coordinate offset starts before table, or offset is not aligned.
                }
                var tmdid = (updateOffsetInSection - 4) / 80 + 1; // Divide by size of coord uint. Coord TMDIDs are 1-indexed.

                ReadMappedValue16(reader, out _, out var sequenceSize);
                ReadMappedValue16(reader, out _, out var sequenceCount);
                sequenceSize *= 4;

                if (sequenceSize == 0 || sequenceSize > Program.MaxHMDAnimSequenceSize)
                {
                    return null;
                }
                if (sequenceCount > Program.MaxHMDAnimSequenceCount)
                {
                    return null;
                }

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
                    //Program.Logger.WriteLine($"[{i}][{j}] idx={startIdx[j]} sid={startSID[j]}");
                }

                for (var j = 0; j < sequenceCount; j++)
                {
                    // Make sure an animation is created for this sequence.
                    GetAnimation(j);
                    var animationObject = GetAnimationObject(j, tmdid, true);

                    // Speed is stored in fixed point with lowest 4 bits representing fraction.
                    // Absolute value of speed, because we already parse animation in reverse.
                    animationObject.Speed = speed == 0 ? 1f : (Math.Abs((int)speed) / 16f);


                    AnimationFrame lastAnimationFrame = null;

                    uint idx = startIdx[j]; // Current starting instruction pointer.
                    uint sid = startSID[j]; // Current Stream ID (SID), acts like a control flow variable.
                    uint time = 0; // Current frame time.
                    var executedCount = 0; // Debug information: Number of instructions encountered
                    var halted = false; // Debug information: END instruction encountered
                    var looped = false;

                    // List of encountered instructions with SIDs they were encountered with.
                    // Once we reach an instruction that we've already reached with the same SID, then we've hit an infinite loop.
                    // The lookup value is calculated as: (sid << 16 | idx)
                    var visited = new HashSet<uint>();
                    // Processed normal instructions, these are only added if they trigger adding lastAnimationFrame.
                    var processed = new HashSet<uint>();

                    // Execute instructions to find all frames (Normal instructions).
                    while (idx < ushort.MaxValue)
                    {
                        // Cache already-read instructions.
                        if (!descriptors.TryGetValue(idx, out var descriptor))
                        {
                            reader.BaseStream.Seek(_offset + ctrlTop + idx * 4, SeekOrigin.Begin);
                            descriptor = reader.ReadUInt32();
                            descriptors.Add(idx, descriptor);
                        }
                        var descriptorType = (descriptor >> 30) & 0x3;

                        var state = (sid << 16 | idx);

                        // Infinite loop check.
                        if (!visited.Add(state))
                        {
                            // If we're revisiting a normal instruction, try to process it first so that the animation loops cleanly.
                            if (!looped && (descriptorType & 0x2) == 0x0 && !processed.Contains(state))
                            {
                                // Prevent continuing more than once if we encounter normal instructions that will never get processed.
                                // todo: We could probably keep going until finding a processable normal instruction, but we'd need more safety checks.
                                looped = true;
                            }
                            else
                            {
                                // Infinite loop.
                                if (Program.Debug)
                                {
                                    Program.Logger.WriteLine($"Infinite loop in {FormatName} animation @{idx} #{sid}");
                                }
                                break;
                            }
                        }

                        executedCount++;

                        if ((descriptorType & 0x2) == 0x0) // Normal (second MSB is used by instruction)
                        {
                            // SPAGHETTI ALERT:
                            // The way frames, next frames, final parameters and interpolation is
                            // handled makes the processing of nextTFrame parameters very clunky.
                            // Especially because we need to pre-compute animation sequences.

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

                            // TFrame acts as the time until the target frame is reached.
                            // As such, we don't know the current frame's duration until we read the next normal instruction.

                            // If the first frame we encounter has TFrame!=0, then don't add to the time,
                            // because we can't start interpolation until we have a target frame (the second frame).
                            if (lastAnimationFrame != null)
                            {
                                time += nextTFrame;
                            }

                            // Create a new animation frame to read packet information into, but don't add it to the list yet.
                            // Whether or not it gets added depends on the following frame's TFrame.
                            var animationFrame = GetNextAnimationFrame(j, animationObject, time, false);

                            // Only assign the last frame after encountering a target frame where TFrame!=0.
                            if (lastAnimationFrame != null && nextTFrame != 0)
                            {
                                animationObject.AnimationFrames[lastAnimationFrame.FrameTime] = lastAnimationFrame;
                                lastAnimationFrame.FrameDuration = nextTFrame;
                                // This target frame instruction (for the current Stream ID) has been processed, don't process it again.
                                processed.Add(state);
                            }

                            // Process the target frame and assign Final* interpolation parameters to the last frame.
                            reader.BaseStream.Seek(_offset + paramTop + paramIndex * 4, SeekOrigin.Begin);
                            if (!HMDHelper.ReadAnimPacket(reader, animationType, animationFrame, lastAnimationFrame))
                            {
                                if (Program.Debug)
                                {
                                    Program.Logger.WriteWarningLine($"Invalid/unsupported {FormatName} animation type 0x{animationType:x08} @{idx} #{sid}");
                                }
                                return null; // Unsupported animation packet.
                            }

                            lastAnimationFrame = animationFrame;
                        }
                        else if (descriptorType == 0x2) // Jump
                        {
                            var seqIndex = (descriptor >>  0) & 0xffff; // Next control descriptor to jump to.
                            var cnd      = (descriptor >> 16) & 0x7f; // Stream ID conditional jump.
                            var dst      = (descriptor >> 23) & 0x7f; // Stream ID destination of jump.
                            // If condition is zero, then jump is unconditional.
                            // If condition is non-zero, then only jump if we match current Stream ID.
                            // If condition == 127, then only match Stream ID 0.
                            // Stream ID is only assigned if this is a conditional jump/or destination is non-zero.
                            if (cnd == 0 || cnd == sid || (cnd == 127 && sid == 0))
                            {
                                if (cnd != 0 || dst != 0)
                                {
                                    sid = dst;
                                }
                                idx = seqIndex;
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
                                var cnd = p1;
                                // If condition is zero, then halt is unconditional.
                                // If condition is non-zero, then only halt sequence if we match current Stream ID.
                                // If condition == 127, then only match Stream ID 0.
                                if (cnd == 0 || cnd == sid || (cnd == 127 && sid == 0))
                                {
                                    halted = true;
                                    break;
                                }
                            }
                            else if (code == 2) // Work
                            {
                                // Work area for sequence pointer required by B-Spline interpolation.
                                // p1: 127 fixed
                                // p2: Offset (in 4-byte units) in parameter section indicates work area.

                                // Currently we don't need any extra handling for this with B-Splines.
                                // HOWEVER: If an animation does something funky by changing the work area mid-setup, then it will break.
                            }
                            else
                            {
                                // Invalid code. Should we return null here?
                                return null;
                            }
                        }

                        if (speed >= 0) idx++;
                        else            idx--;
                    }
                }

                // Seek to the start of the next sequence pointer.
                reader.BaseStream.Seek(sequencePointerPosition + sequenceSize, SeekOrigin.Begin);
            }

            for (var j = 0; j < animationList.Count; j++)
            {
                var animation = animationList[j];
                var animationObjects = animationObjectsList[j];

                animation.AnimationType = AnimationType.HMD;
                // todo: Handle difference between PAL (25fps) and US (30fps) speeds.
                animation.FPS = 25f;
                animation.AssignObjects(animationObjects, true, false);
            }

            return animationList;
        }

        private Animation ProcessMIMeJointData(Dictionary<RenderInfo, List<Triangle>> groupedTriangles, BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer, uint dataCount, bool rpy, bool reset)
        {
            var position = reader.BaseStream.Position;
            ProcessMIMeJointPrimitiveHeader(reader, primitiveHeaderPointer, reset, out var coordTop, out var mimeTop, out var mimeNum, out var mimeId, out var mimeDiffTop);
            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            for (uint i = 0; i < dataCount; i++)
            {
                // Unlike Vertex/Normal, this DOES NOT differ from Reset.

                var diffIndex = reader.ReadUInt32() * 4;

                position = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + mimeDiffTop + diffIndex, SeekOrigin.Begin);

                var coordID = reader.ReadUInt16();
                var numDiffs = reader.ReadUInt16();
                if (numDiffs > Program.MaxHMDMIMEeDiffs)
                {
                    return null;
                }
                var diffKeyBits = reader.ReadUInt32(); // Keys with differences by bit index.
                var diffKeys = new List<uint>();
                for (var key = 0; key < 32; key++)
                {
                    if ((diffKeyBits & (1u << key)) != 0)
                    {
                        diffKeys.Add((uint)key);
                    }
                }

                // numDiffs of Joint MIMeDiffData packets
                for (var j = 0; j < numDiffs; j++)
                {
                    var dvx = reader.ReadInt16();
                    var dvy = reader.ReadInt16();
                    var dvz = reader.ReadInt16();
                    var dtp = reader.ReadInt16(); // bit 0: 0 if dvz-dvz are all zero, bit 1: 0 if dtx-dtz are all zero
                    var dtx = reader.ReadInt32();
                    var dty = reader.ReadInt32();
                    var dtz = reader.ReadInt32();
                }

                // 1 Reset Joint MIMeDiffData packet (this is just working data)
                if (!rpy)
                {
                    // todo: Docs claim pad uint16 is before translation, unlike with coord matrices,
                    //       But that doesn't make sense, because LABs both use MATRIX() to define the data.
                    var resetMatrix = ReadMatrix(reader, out _);
                }
                else
                {
                    var dvx = reader.ReadInt16();
                    var dvy = reader.ReadInt16();
                    var dvz = reader.ReadInt16();
                    var changed = reader.ReadUInt16();
                    var dtx = reader.ReadInt32();
                    var dty = reader.ReadInt32();
                    var dtz = reader.ReadInt32();
                }

                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }

            return null;
        }

        private Animation ProcessMIMeVertexNormalData(Dictionary<RenderInfo, List<Triangle>> groupedTriangles, BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer, uint dataCount, bool normal, bool reset)
        {
            Animation animation;
            Dictionary<uint, AnimationObject> animationObjects;

            AnimationFrame GetNextAnimationFrame(AnimationObject animationObject)
            {
                var animationFrames = animationObject.AnimationFrames;
                var frameTime = (uint)animationFrames.Count;
                var animationFrame = new AnimationFrame
                {
                    FrameTime = frameTime,
                    FrameDuration = 1,
                    AnimationObject = animationObject
                };
                animationFrames.Add(frameTime, animationFrame);

                //animation.FrameCount = Math.Max(animation.FrameCount, animationFrame.FrameEnd);
                return animationFrame;
            }
            AnimationObject GetAnimationObject(uint objectId)
            {
                if (!animationObjects.TryGetValue(objectId, out var animationObject))
                {
                    animationObject = new AnimationObject { Animation = animation, ID = objectId };
                    animationObject.TMDID.Add(objectId);
                    animationObjects.Add(objectId, animationObject);
                }
                return animationObject;
            }

            animation = new Animation();
            animationObjects = new Dictionary<uint, AnimationObject>();

            var position = reader.BaseStream.Position;
            ProcessMIMeVertexNormalPrimitiveHeader(reader, primitiveHeaderPointer, reset, out var mimeTop, out var mimeNum, out var mimeId, out var mimeDiffTop, out var mimeOrigTop, out var vertTop, out var normTop);
            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            for (uint i = 0; i < dataCount; i++)
            {
                // todo: This differs for Reset

                var diffIndex = reader.ReadUInt32() * 4;

                position = reader.BaseStream.Position;
                reader.BaseStream.Seek(_offset + mimeDiffTop + diffIndex, SeekOrigin.Begin);

                var numOriginals = reader.ReadUInt16();
                var numDiffs = reader.ReadUInt16();
                if (numDiffs > Program.MaxHMDMIMEeDiffs)
                {
                    return null;
                }
                if (numOriginals > Program.MaxHMDMIMEeOriginals)
                {
                    return null;
                }
                var diffKeyBits = reader.ReadUInt32(); // Keys with differences by bit index.
                var diffKeys = new List<uint>();
                for (var key = 0; key < 32; key++)
                {
                    if ((diffKeyBits & (1u << key)) != 0)
                    {
                        diffKeys.Add((uint)key);
                    }
                }
                var animationObject = GetAnimationObject(mimeId + 1u); // Probably not correct...
                for (uint j = 0; j < numDiffs; j++)
                {
                    var diffDataIndex = reader.ReadUInt32() * 4;

                    var diffPosition = reader.BaseStream.Position;
                    reader.BaseStream.Seek(_offset + mimeDiffTop + diffIndex + diffDataIndex, SeekOrigin.Begin);

                    var vertexStart = reader.ReadUInt32();
                    reader.ReadUInt16(); //reserved
                    var vertexCount = reader.ReadUInt16();
                    if (vertexCount + vertexStart == 0 || vertexCount + vertexStart >= Program.MaxHMDVertices)
                    {
                        return null;
                    }
                    var animationFrame = GetNextAnimationFrame(animationObject);
                    var vertices = new Vector3[vertexCount + vertexStart];
                    for (var k = 0; k < vertexCount; k++)
                    {
                        Vector3 v;
                        if (!normal)
                        {
                            var vx = reader.ReadInt16();
                            var vy = reader.ReadInt16();
                            var vz = reader.ReadInt16();
                            var pad = reader.ReadUInt16();
                            v = new Vector3(vx, vy, vz);
                        }
                        else
                        {
                            var nx = TMDHelper.ConvertNormal(reader.ReadInt16());
                            var ny = TMDHelper.ConvertNormal(reader.ReadInt16());
                            var nz = TMDHelper.ConvertNormal(reader.ReadInt16());
                            var pad = reader.ReadUInt16();
                            v = new Vector3(nx, ny, nz);
                        }
                        vertices[vertexStart + k] = v;
                    }
                    animationFrame.Vertices = vertices;
                    animationFrame.TempVertices = new Vector3[animationFrame.Vertices.Length];

                    reader.BaseStream.Seek(diffPosition, SeekOrigin.Begin);
                }
                for (uint j = 0; j < numOriginals; j++)
                {
                    var diffChangedIndex = reader.ReadUInt32() * 4;

                    var origPosition = reader.BaseStream.Position;
                    // Unlike data diffs, diffIndex isn't used here.
                    reader.BaseStream.Seek(_offset + mimeDiffTop + diffChangedIndex, SeekOrigin.Begin);

                    var changed = reader.ReadUInt16(); // Should be 0 in file
                    var vertexCount = reader.ReadUInt16();

                    reader.BaseStream.Seek(origPosition, SeekOrigin.Begin);
                }
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }

            animation.AnimationType = normal ? AnimationType.NormalDiff : AnimationType.VertexDiff;
            animation.FPS = 1f;
            animation.AssignObjects(animationObjects, false, false);
            return animation;
        }

        private void ProcessGroundData(Dictionary<RenderInfo, List<Triangle>> groupedTriangles, BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer)
        {
            var texture = primitiveType == 1;

            void AddTriangle(Triangle triangle, uint tPageNum, RenderFlags renderFlags, MixtureRate mixtureRate)
            {
                if (renderFlags.HasFlag(RenderFlags.Textured))
                {
                    triangle.CorrectUVTearing();
                }
                var renderInfo = new RenderInfo(tPageNum, renderFlags, mixtureRate);
                if (!groupedTriangles.TryGetValue(renderInfo, out var triangles))
                {
                    triangles = new List<Triangle>();
                    groupedTriangles.Add(renderInfo, triangles);
                }
                triangles.Add(triangle);
            }

            var polygonIndex = reader.ReadUInt32() * 4;
            var gridIndex = reader.ReadUInt32() * 4;
            var vertexIndex = reader.ReadUInt32() * 4;

            var position = reader.BaseStream.Position;
            ProcessGroundPrimitiveHeader(reader, primitiveHeaderPointer, out var polyTop, out var gridTop, out var vertTop, out var normTop, out var uvTop, out var coordTop);
            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            // Read polygon data
            reader.BaseStream.Seek(_offset + polyTop + polygonIndex, SeekOrigin.Begin);
            var startX = reader.ReadInt16();
            var startY = reader.ReadInt16();
            var cellWidth  = reader.ReadUInt16();
            var cellHeight = reader.ReadUInt16();
            var verticesWidth  = reader.ReadUInt16();
            var verticesHeight = reader.ReadUInt16();
            var rowCount = reader.ReadUInt16(); // size
            var baseVertex = reader.ReadUInt16(); // Not sure what to do with this...


            var gridPosition = _offset + gridTop + gridIndex;
            for (var row = 0; row < rowCount; row++)
            {
                // Continued polygon data
                var vertexStart = reader.ReadUInt16();
                var columnCount = reader.ReadUInt16();
                var polyPosition = reader.BaseStream.Position;

                for (var column = 0; column < columnCount; column++)
                {
                    uint tPage;
                    var renderFlags = RenderFlags.None;
                    var mixtureRate = MixtureRate.None;
                    Color color;
                    Vector3 normal;
                    Vector2 uv0, uv1, uv2, uv3;

                    // Read grid packet
                    reader.BaseStream.Seek(gridPosition, SeekOrigin.Begin);
                    if (!texture)
                    {
                        var r = reader.ReadByte() / 255f;
                        var g = reader.ReadByte() / 255f;
                        var b = reader.ReadByte() / 255f;
                        reader.ReadByte(); //pad
                        var normIndex = reader.ReadUInt16();
                        reader.ReadUInt16(); //pad
                        gridPosition = reader.BaseStream.Position;

                        color = new Color(r, g, b);

                        normal = ReadNormal(reader, normTop, normIndex);

                        tPage = 0;
                        uv0 = uv1 = uv2 = uv3 = Vector2.Zero;
                    }
                    else
                    {
                        renderFlags |= RenderFlags.Textured;

                        var normIndex = reader.ReadUInt16();
                        var uvIndex = reader.ReadUInt16();
                        gridPosition = reader.BaseStream.Position;

                        color = Color.Grey;

                        normal = ReadNormal(reader, normTop, normIndex);

                        // Read UV data
                        reader.BaseStream.Seek(_offset + uvTop + uvIndex * 12, SeekOrigin.Begin);
                        var u0 = reader.ReadByte();
                        var v0 = reader.ReadByte();
                        var cbaValue = reader.ReadUInt16();
                        var u1 = reader.ReadByte();
                        var v1 = reader.ReadByte();
                        var tsbValue = reader.ReadUInt16();
                        var u2 = reader.ReadByte();
                        var v2 = reader.ReadByte();
                        var u3 = reader.ReadByte();
                        var v3 = reader.ReadByte();

                        TMDHelper.ParseTSB(tsbValue, out tPage, out var pmode, out mixtureRate);
                        mixtureRate = MixtureRate.None; // No semi-transparency
                        uv0 = GeomMath.ConvertUV(u0, v0);
                        uv1 = GeomMath.ConvertUV(u1, v1);
                        uv2 = GeomMath.ConvertUV(u2, v2);
                        uv3 = GeomMath.ConvertUV(u3, v3);
                    }

                    // Read Z vertices
                    reader.BaseStream.Seek(_offset + vertTop + vertexIndex + (vertexStart + column) * 2, SeekOrigin.Begin);
                    var z0 = reader.ReadInt16();
                    //var z2 = reader.ReadInt16();
                    var z1 = reader.ReadInt16();

                    reader.BaseStream.Seek(_offset + vertTop + vertexIndex + (vertexStart + column + verticesWidth) * 2, SeekOrigin.Begin);
                    //var z1 = reader.ReadInt16();
                    var z2 = reader.ReadInt16();
                    var z3 = reader.ReadInt16();


                    var x0 = startX + cellWidth  * column;
                    var y0 = startY + cellHeight * row;
                    var x1 = x0 + cellWidth;
                    var y1 = y0 + cellHeight;

                    // The format says X,Y for cells and Z for vertices... but no sane terrain format would only allow vertical ground.
                    // To fix the polygon's vertex order, z1 and z2 have to be read opposite of the expected order...
                    //var vertex0 = new Vector3(x0, z0, y0);
                    //var vertex1 = new Vector3(x0, z1, y1);
                    //var vertex2 = new Vector3(x1, z2, y0);
                    //var vertex3 = new Vector3(x1, z3, y1);
                    var vertex0 = new Vector3(x0, y0, z0);
                    var vertex1 = new Vector3(x1, y0, z1);
                    var vertex2 = new Vector3(x0, y1, z2);
                    var vertex3 = new Vector3(x1, y1, z3);

                    var normal1 = normal;
                    var normal2 = normal;
                    // Not part of the format, but uncomment if you want to see
                    // clearer terrain lighting when all normals are the same.
                    //normal1 = GeomMath.CalculateNormal(vertex0, vertex1, vertex2);
                    //normal2 = GeomMath.CalculateNormal(vertex1, vertex3, vertex2);

                    AddTriangle(new Triangle
                    {
                        Vertices = new[] { vertex0, vertex1, vertex2 },
                        Normals = new[] { normal1, normal1, normal1 },
                        Colors = new[] { color, color, color },
                        Uv = new[] { uv0, uv1, uv2 },
                        AttachableIndices = Triangle.EmptyAttachableIndices,
                    }, tPage, renderFlags, mixtureRate);

                    AddTriangle(new Triangle
                    {
                        Vertices = new[] { vertex1, vertex3, vertex2 },
                        Normals = new[] { normal2, normal2, normal2 },
                        Uv = new[] { uv1, uv3, uv2 },
                        Colors = new[] { color, color, color },
                        AttachableIndices = Triangle.EmptyAttachableIndices,
                    }, tPage, renderFlags, mixtureRate);
                }
                reader.BaseStream.Seek(polyPosition, SeekOrigin.Begin);
            }
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessEnvmapData(Dictionary<RenderInfo, List<Triangle>> groupedTriangles, BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer, uint dataCount)
        {
            var polygonIndex = reader.ReadUInt32() * 4;

            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);
            // Read header here. Header size is 14, and is too complicated to read in another function.
            var headerSize = reader.ReadUInt32();
            ReadMappedPointer(reader, out _, out var polyTop);
            ReadMappedPointer(reader, out _, out var vertTop);
            ReadMappedPointer(reader, out _, out var normTop);
            ReadMappedPointer(reader, out _, out var envImagePointer);
            ReadMappedPointer(reader, out _, out var reflectImagePointer);
            ReadMappedPointer(reader, out _, out var reflectClutPointer);
            ReadMappedPointer(reader, out _, out var refractImagePointer);
            ReadMappedPointer(reader, out _, out var refractClutPointer);

            var envTexmode = reader.ReadByte();
            reader.ReadByte(); //pad
            var envMaterial = reader.ReadByte();
            reader.ReadByte(); //pad

            var reflectTexmode = reader.ReadByte();
            var reflectAbr = reader.ReadByte();
            var reflectRate = reader.ReadByte();
            reader.ReadByte(); //pad
            reader.ReadByte(); //pad
            var reflectR = reader.ReadByte(); // Note: RGB isn't defined for reflect, but these 4-bytes are left open, so read it as such.
            var reflectG = reader.ReadByte();
            var reflectB = reader.ReadByte();

            var refractTexmode = reader.ReadByte();
            var refractAbr = reader.ReadByte();
            var refractRate = reader.ReadByte();
            reader.ReadByte(); //pad
            reader.ReadByte(); //pad
            var refractR = reader.ReadByte();
            var refractG = reader.ReadByte();
            var refractB = reader.ReadByte();

            ReadMappedPointer(reader, out _, out var coordTop);
            // End of header


            reader.BaseStream.Seek(_offset + polyTop + polygonIndex, SeekOrigin.Begin);
            for (var j = 0; j < dataCount; j++)
            {
                var flag = primitiveType & 0xeff; // Not sure if bit 8 is *supposed* to be presets, or something else.

                var packetStructure = TMDHelper.CreateHMDPacketStructure(driver, flag, reader);
                if (packetStructure != null)
                {
                    switch (packetStructure.PrimitiveType)
                    {
                        case PrimitiveType.Triangle:
                        case PrimitiveType.Quad:
                        case PrimitiveType.StripMesh:
                            TMDHelper.AddTrianglesToGroup(groupedTriangles, packetStructure, false,
                                index => {
                                    return ReadVertex(reader, vertTop, index);
                                },
                                index => {
                                    return ReadNormal(reader, normTop, index);
                                });
                            break;
                    }
                }
            }
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessEquipmentData(Dictionary<RenderInfo, List<Triangle>> groupedTriangles, BinaryReader reader, uint driver, uint primitiveType, uint primitiveHeaderPointer)
        {
            var position = reader.BaseStream.Position;
            // Note: with coordPointers, we need to find out coordTop to get the actual coordinate indices.
            // This can be calculated ahead of time and passed to ProcessPrimitive.
            ProcessEquipmentPrimitiveHeader(reader, primitiveHeaderPointer, out var paramTop, out var posCoordPointer, out var refCoordPointer);
            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            if (primitiveType == 0x100) // Camera
            {
                // Driver: 0-Projection, 1-World camera, 2-Fix camera, 3-Aim camera

                // Read parameters
                // There is no paramIndex for cameras. The only difference is usually posCoordPointer, and refCoordPointer.
                reader.BaseStream.Seek(_offset + paramTop, SeekOrigin.Begin);
                var proj = reader.ReadInt32();
                var rot = (float)(reader.ReadInt32() / 4096.0 * (Math.PI * 2.0));
                var vx = reader.ReadInt32(); // Position of camera
                var vy = reader.ReadInt32();
                var vz = reader.ReadInt32();
                var rx = reader.ReadInt32(); // Position of target
                var ry = reader.ReadInt32();
                var rz = reader.ReadInt32();
            }
            else if (primitiveType == 0x200) // Light
            {
                // Driver: 0-Ambient color, 1-World light, 2-Fix light, 3-Aim light
                var lightID = reader.ReadUInt16();
                var paramIndex = reader.ReadUInt16();

                // Read parameters
                reader.BaseStream.Seek(_offset + paramTop + paramIndex * 4, SeekOrigin.Begin);
                var r = reader.ReadByte();
                var g = reader.ReadByte();
                var b = reader.ReadByte();
                reader.ReadByte(); //pad
                if (driver != 0) // Not ambient light
                {
                    var vx = reader.ReadInt32(); // Position of light
                    var vy = reader.ReadInt32();
                    var vz = reader.ReadInt32();
                    var rx = reader.ReadInt32(); // Position of target
                    var ry = reader.ReadInt32();
                    var rz = reader.ReadInt32();
                }
            }
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessNonSharedGeometryPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint polyTop, out uint vertTop, out uint normTop, out uint coordTop)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();

            ReadMappedPointer(reader, out var polyTopapped, out polyTop);
            ReadMappedPointer(reader, out var vertTopMapped, out vertTop);
            ReadMappedPointer(reader, out var normTopMaped, out normTop);
            ReadMappedPointer(reader, out var coordTopMapped, out coordTop);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessSharedGeometryPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint polyTop, out uint vertTop, out uint calcVertTop, out uint normTop, out uint calcNormTop, out uint coordTop)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();

            ReadMappedPointer(reader, out var polyTopMapped, out polyTop);
            ReadMappedPointer(reader, out var vertTopMapped, out vertTop);
            ReadMappedPointer(reader, out var calcVertTopMapped, out calcVertTop);
            ReadMappedPointer(reader, out var normTopMaped, out normTop);
            ReadMappedPointer(reader, out var calcNormTopMaped, out calcNormTop);
            ReadMappedPointer(reader, out var coordTopMapped, out coordTop);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessImageDataPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint imageTop, out uint clutTop)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();

            ReadMappedPointer(reader, out var imageTopMapped, out imageTop);
            ReadMappedPointer(reader, out var clutTopMapped, out clutTop);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessAnimationPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint interpTop, out uint ctrlTop, out uint paramTop, out uint coordTop, out uint[] sectionList)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();
            var animHeaderSize = reader.ReadUInt32();
            if (animHeaderSize > Program.MaxHMDHeaderLength)
            {
                // todo: We aren't setup to signal failure in this function, so just correct the issue.
                animHeaderSize = (uint)Program.MaxHMDHeaderLength;
            }
            if (animHeaderSize > headerSize)
            {
                // todo: We aren't setup to signal failure in this function.
            }

            sectionList = new uint[animHeaderSize];
            sectionList[0] = animHeaderSize; // Not a valid section index.
            for (var i = 1; i < animHeaderSize; i++)
            {
                ReadMappedPointer(reader, out _, out sectionList[i]);
            }

            interpTop = (animHeaderSize >= 2 ? sectionList[1] : 0u);
            ctrlTop   = (animHeaderSize >= 3 ? sectionList[2] : 0u);
            paramTop  = (animHeaderSize >= 4 ? sectionList[3] : 0u);
            coordTop  = (animHeaderSize >= 5 ? sectionList[4] : 0u);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessMIMeJointPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, bool reset, out uint coordTop, out uint mimeTop, out uint mimeNum, out ushort mimeId, out uint mimeDiffTop)
        {
            var position = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();

            ReadMappedPointer(reader, out var coordTopMapped, out coordTop);
            if (!reset)
            {
                ReadMappedPointer(reader, out var mimeTopMapped, out mimeTop);
                mimeNum = reader.ReadUInt32();
            }
            else
            {
                mimeTop = 0;
                mimeNum = 0;
            }
            mimeId = reader.ReadUInt16();
            reader.ReadUInt16(); //reserved
            ReadMappedPointer(reader, out var mimeAxesDiffTopMapped, out mimeDiffTop);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessMIMeVertexNormalPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, bool reset, out uint mimeTop, out uint mimeNum, out ushort mimeId, out uint mimeDiffTop, out uint mimeOrigTop, out uint vertTop, out uint normTop)
        {
            var position = reader.BaseStream.Position;
            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            var headerSize = reader.ReadUInt32();

            if (!reset)
            {
                ReadMappedPointer(reader, out var mimeTopMapped, out mimeTop);
                mimeNum = reader.ReadUInt32();
            }
            else
            {
                mimeTop = 0;
                mimeNum = 0;
            }
            mimeId = reader.ReadUInt16();
            reader.ReadUInt16(); //reserved
            ReadMappedPointer(reader, out var mimeDiffTopMapped, out mimeDiffTop);
            ReadMappedPointer(reader, out var mimeOrigTopMapped, out mimeOrigTop);
            ReadMappedPointer(reader, out var mimeVertTopMapped, out vertTop);
            ReadMappedPointer(reader, out var mimeNormTopMapped, out normTop);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessGroundPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint polyTop, out uint gridTop, out uint vertTop, out uint normTop, out uint uvTop, out uint coordTop)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            uvTop = int.MaxValue;
            coordTop = int.MaxValue;

            var headerSize = reader.ReadUInt32();

            ReadMappedPointer(reader, out var polyTopMapped, out polyTop);
            ReadMappedPointer(reader, out var gridTopMapped, out gridTop);
            ReadMappedPointer(reader, out var vertTopMapped, out vertTop);
            ReadMappedPointer(reader, out var normTopMapped, out normTop);

            if (headerSize >= 5)
            {
                ReadMappedPointer(reader, out var uvTopMapped, out uvTop);
            }
            if (headerSize >= 6)
            {
                ReadMappedPointer(reader, out var coordTopMapped, out coordTop);
            }

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private void ProcessEquipmentPrimitiveHeader(BinaryReader reader, uint primitiveHeaderPointer, out uint paramTop, out uint posCoordPointer, out uint refCoordPointer)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(_offset + primitiveHeaderPointer, SeekOrigin.Begin);

            posCoordPointer = 0;
            refCoordPointer = 0;

            var headerSize = reader.ReadUInt32();

            ReadMappedPointer(reader, out var paramTopMapped, out paramTop); // Light only

            if (headerSize >= 2)
            {
                ReadMappedPointer(reader, out var posCoordPointerMapped, out posCoordPointer); // Fix and Aim only
            }
            if (headerSize >= 3)
            {
                ReadMappedPointer(reader, out var refCoordPointerMapped, out refCoordPointer); // Aim only
            }

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }
    }
}
