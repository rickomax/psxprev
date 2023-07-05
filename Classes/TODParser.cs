using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Classes
{
    public class TODParser
    {
        private long _offset;
        private Action<Animation, long> entityAddedAction;

        public TODParser(Action<Animation, long> entityAdded)
        {
            entityAddedAction = entityAdded;
        }

        public void LookForTOD(BinaryReader reader, string fileTitle)
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
                    _offset = reader.BaseStream.Position;
                    var fileID = reader.ReadByte();
                    if (fileID == 0x50)
                    {
                        var animation = ParseTOD(reader);
                        if (animation != null)
                        {
                            animation.AnimationName = string.Format("{0}{1:x}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            entityAddedAction(animation, reader.BaseStream.Position);
                            Program.Logger.WritePositiveLine("Found TOD Animation at offset {0:X}", _offset);
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
                        Program.Logger.WriteLine($"TOD - Reached file end: {fileTitle}");
                        return;
                    }
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
                }
            }
        }

        private Animation ParseTOD(BinaryReader reader)
        {
            Animation animation;
            Dictionary<uint, AnimationObject> animationObjects;
            AnimationFrame GetAnimationFrame(AnimationObject animationObject, uint frameTime)
            {
                var animationFrames = animationObject.AnimationFrames;
                if (animationFrames.ContainsKey(frameTime))
                {
                    return animationFrames[frameTime];
                }
                var frame = new AnimationFrame { FrameTime = frameTime, AnimationObject = animationObject };
                animationFrames.Add(frameTime, frame);
                return frame;
            }
            AnimationObject GetAnimationObject(ushort objectId)
            {
                if (animationObjects.ContainsKey(objectId))
                {
                    return animationObjects[objectId];
                }
                var animationObject = new AnimationObject { Animation = animation, ID = objectId, TMDID = (uint?) (objectId+1)};
                animationObjects.Add(objectId, animationObject);
                return animationObject;
            }
            var version = reader.ReadByte();
            var resolution = reader.ReadUInt16();
            if (resolution == 0)
            {
                return null;
            }
            var frameCount = reader.ReadUInt32();
            if (frameCount == 0 || frameCount > Program.MaxTODFrames)
            {
                return null;
            }
            animation = new Animation();
            var rootAnimationObject = new AnimationObject();
            animationObjects = new Dictionary<uint, AnimationObject>();
            for (var f = 0; f < frameCount; f++)
            {
                var frameTop = reader.BaseStream.Position;
                var frameSize = reader.ReadUInt16();
                var packetCount = reader.ReadUInt16();
                if (packetCount > Program.MaxTODPackets)
                {
                    return null;
                }
                var frameNumber = reader.ReadUInt32();
                //if (frameNumber != f)
                //{
                //    return null;
                //}
                if (packetCount == 0 || frameSize == 0)
                {
                    continue;
                    //reader.BaseStream.Position = frameTop + (frameSize * 4);
                }
                for (var p = 0; p < packetCount; p++)
                {
                    var packetTop = reader.BaseStream.Position;
                    var objectId = reader.ReadUInt16();
                    var packetTypeAndFlag = reader.ReadByte();
                    var packetType = (packetTypeAndFlag & 0xF);
                    var flag = (packetTypeAndFlag & 0xF0) >> 0x4;
                    var packetLength = reader.ReadByte();
                    var animationObject = GetAnimationObject(objectId);
                    var animationFrame = GetAnimationFrame(animationObject, frameNumber);
                    switch (packetType)
                    {
                        case 0x01: //Coordinate
                            var matrixType = (flag & 0x1);
                            var rotation = (flag & 0x2) >> 0x1;
                            var scaling = (flag & 0x4) >> 0x2;
                            var translation = (flag & 0x8) >> 0x3;
                            if (rotation != 0x00)
                            {
                                var rx = (reader.ReadInt32()/ 4096f) * GeomUtils.Deg2Rad;
                                var ry = (reader.ReadInt32()/ 4096f) * GeomUtils.Deg2Rad;
                                var rz = (reader.ReadInt32()/ 4096f) * GeomUtils.Deg2Rad;
                                animationFrame.EulerRotation = new Vector3(rx, ry, rz);
                            }
                            if (scaling != 0x00)
                            {
                                var sx = reader.ReadInt16()/ 4096f;
                                var sy = reader.ReadInt16()/ 4096f;
                                var sz = reader.ReadInt16()/ 4096f;
                                reader.ReadUInt16();
                                animationFrame.Scale = new Vector3(sx, sy, sz);
                            }
                            if (translation != 0x00)
                            {
                                float tx = reader.ReadInt32();
                                float ty = reader.ReadInt32();
                                float tz = reader.ReadInt32();
                                animationFrame.Translation = new Vector3(tx, ty, tz);
                            }
                            animationFrame.AbsoluteMatrix = matrixType == 0x00;
                            //if ((reader.BaseStream.Position - packetTop) / 4 != packetLength)
                            //{
                            //    return null;
                            //}
                            break;
                        case 0x02: //TMD data ID
                            animationObject.TMDID = reader.ReadUInt16();
                            reader.ReadUInt16();
                            //if ((reader.BaseStream.Position - packetTop) / 4 != packetLength)
                            //{
                            //    return null;
                            //}
                            break;
                        case 0x03: //Parent Object ID
                            animationObject.ParentID = reader.ReadUInt16();
                            reader.ReadUInt16();
                            //if ((reader.BaseStream.Position - packetTop) / 4 != packetLength)
                            //{
                            //    return null;
                            //}
                            break;
                        case 0x04:
                            var r00 = reader.ReadInt16()/ 4096f;
                            var r01 = reader.ReadInt16()/ 4096f;
                            var r02 = reader.ReadInt16()/ 4096f;

                            var r10 = reader.ReadInt16()/ 4096f;
                            var r11 = reader.ReadInt16()/ 4096f;
                            var r12 = reader.ReadInt16()/ 4096f;

                            var r20 = reader.ReadInt16()/ 4096f;
                            var r21 = reader.ReadInt16()/ 4096f;
                            var r22 = reader.ReadInt16()/ 4096f;

                            reader.ReadInt16();

                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            var z = reader.ReadInt32();

                            var matrix = new Matrix3(
                                new Vector3(r00, r01, r02),
                                new Vector3(r10, r11, r12),
                                new Vector3(r20, r21, r22)
                                );

                            animationFrame.Translation = new Vector3(x, y, z);
                            animationFrame.Rotation = matrix.ExtractRotation();
                            animationFrame.Scale = matrix.ExtractScale();
                            animationFrame.AbsoluteMatrix = true;

                            //if ((reader.BaseStream.Position - packetTop) / 4 != packetLength)
                            //{
                            //    return null;
                            //}
                            break;
                        default:
                            if (packetType <= 0xF)
                            {
                                reader.BaseStream.Position = packetTop + packetLength * 4;
                                continue;
                            }
                            return null;
                    }
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
            animation.RootAnimationObject = rootAnimationObject;
            animation.FrameCount = frameCount;
            animation.ObjectCount = animationObjects.Count;
            animation.FPS = 1f / resolution * 60f;
            if (float.IsInfinity(animation.FPS) || animation.FPS % 1 > 0)
            {
                return null;
            }
            return animation;
        }
    }
}
