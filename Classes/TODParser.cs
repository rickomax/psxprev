using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenTK;


namespace PSXPrev
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

            ;
           //var animations = new List<Animation>();

            while (reader.BaseStream.CanRead)
            {
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
                            //animations.Add(animation);
                            entityAddedAction(animation, reader.BaseStream.Position);
                            Program.Logger.WriteLine("Found TOD Animation at offset {0:X}", _offset);
                        }
                    }
                }
                catch (Exception exp)
                {
                    if (exp is EndOfStreamException)
                    {
                        //if (checkOffset >= reader.BaseStream.Length - 4)
                        //{
                        break;
                        //}
                        //reader.BaseStream.Seek(checkOffset + 1, SeekOrigin.Begin);
                    }
                    Program.Logger.WriteLine(exp);
                }
                reader.BaseStream.Seek(_offset + 1, SeekOrigin.Begin);
            }
        }

        private Animation ParseTOD(BinaryReader reader)
        {
            var version = reader.ReadByte();
            var resolution = reader.ReadUInt16();
            var frameCount = reader.ReadUInt32();
            if (frameCount == 0 || frameCount > 10000)
            {
                return null;
            }
            var animation = new Animation();
            var rootAnimationObject = new AnimationObject();
            var animationObjects = new Dictionary<int, AnimationObject>();
            for (var f = 0; f < frameCount; f++)
            {
                //var frameTop = reader.BaseStream.Position;
                var frameSize = reader.ReadUInt16();
                var packetCount = reader.ReadUInt16();
                var frameNumber = reader.ReadUInt32();
                if (frameNumber != f)
                {
                    return null;
                }
                if (packetCount == 0)
                {
                    continue;
                }
                //if (packetCount > 50000)
                //{
                //    return null;
                //}

                for (var p = 0; p < packetCount; p++)
                {
                    var packetTop = reader.BaseStream.Position;
                    var objectId = reader.ReadUInt16();
                    var packetTypeAndFlag = reader.ReadByte();
                    var packetType = (packetTypeAndFlag & 0xF);
                    var flag = (packetTypeAndFlag & 0xF0) >> 0x4;
                    var packetLength = reader.ReadByte();
                    var animationObject = GetAnimationObject(animation, animationObjects, objectId);
                    var animationFrame = GetAnimationFrame(animationObject, f);
                    switch (packetType)
                    {
                        //case 0x00: //Attribute
                        //    var attribute1 = reader.ReadUInt32();
                        //    var attribute2 = reader.ReadUInt32();
                        //    break;
                        case 0x01: //Coordinate
                            var matrixType = (flag & 0x1);
                            var rotation = (flag & 0x2) >> 0x1;
                            var scaling = (flag & 0x4) >> 0x2;
                            var translation = (flag & 0x8) >> 0x3;
                            if (rotation != 0x00)
                            {
                                var rx = (reader.ReadInt32() / 4096f) * GeomUtils.Deg2Rad;
                                var ry = (reader.ReadInt32() / 4096f) * GeomUtils.Deg2Rad;
                                var rz = (reader.ReadInt32() / 4096f) * GeomUtils.Deg2Rad;
                                animationFrame.Rotation = new Vector3(rx, ry, rz);
                            }
                            if (scaling != 0x00)
                            {
                                var sx = reader.ReadInt16() / 4096f;
                                var sy = reader.ReadInt16() / 4096f;
                                var sz = reader.ReadInt16() / 4096f;
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
                            if ((reader.BaseStream.Position - packetTop) / 4 != packetLength)
                            {
                                return null;
                            }
                            break;
                        case 0x02: //TMD data ID
                            animationObject.TMDID = reader.ReadUInt16();
                            reader.ReadUInt16();
                            if ((reader.BaseStream.Position - packetTop) / 4 != packetLength)
                            {
                                return null;
                            }
                            break;
                        case 0x03: //Parent Object ID
                            animationObject.ParentID = reader.ReadUInt16();
                            reader.ReadUInt16();
                            if ((reader.BaseStream.Position - packetTop) / 4 != packetLength)
                            {
                                return null;
                            }
                            break;
                        case 0x04:
                            float r00 = reader.ReadInt16() / 4096f;
                            float r01 = reader.ReadInt16() / 4096f;
                            float r02 = reader.ReadInt16() / 4096f;

                            float r10 = reader.ReadInt16()/ 4096f;
                            float r11 = reader.ReadInt16()/ 4096f;
                            float r12 = reader.ReadInt16() / 4096f;

                            float r20 = reader.ReadInt16()/ 4096f;
                            float r21 = reader.ReadInt16()/ 4096f;
                            float r22 = reader.ReadInt16() / 4096f;

                            reader.ReadInt16();

                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            var z = reader.ReadInt32();

                            animationFrame.AbsoluteMatrix = true;
                            animationFrame.Matrix = new Matrix4(new Matrix3(
                                new Vector3(r00, r10, r20),
                                new Vector3(r01, r11, r21),
                                new Vector3(r02, r12, r22)
                                ));

                            if ((reader.BaseStream.Position - packetTop) / 4 != packetLength)
                            {
                                return null;
                            }
                            break;
                        //case 0x08: //object control
                        //    switch (flag)
                        //    {
                        //        case 0x00:
                        //            break;
                        //        case 0x01:
                        //            break;
                        //    }
                        //    break;
                        default:
                            reader.BaseStream.Position = packetTop + (packetLength * 4);
                            break;
                    }
                }
            }

            foreach (var animationObject in animationObjects.Values)
            {
                if (animationObjects.ContainsKey(animationObject.ParentID))
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
            animation.FPS = 1f / resolution;
            return animation;
        }

        private AnimationFrame GetAnimationFrame(AnimationObject animationObject, int frameTime)
        {
            var animationFrames = animationObject.AnimationFrames;
            if (animationFrames.ContainsKey(frameTime))
            {
                return animationFrames[frameTime];
            }
            var frame = new AnimationFrame { FrameTime = frameTime };
            animationFrames.Add(frameTime, frame);
            return frame;
        }

        private AnimationObject GetAnimationObject(Animation animation, Dictionary<int, AnimationObject> animationObjects, ushort objectId)
        {
            if (animationObjects.ContainsKey(objectId))
            {
                return animationObjects[objectId];
            }
            var animationObject = new AnimationObject { Visible = true, Animation = animation };
            animationObjects.Add(objectId, animationObject);
            return animationObject;
        }
    }
}
