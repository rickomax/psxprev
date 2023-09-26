using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public class TODParser : FileOffsetScanner
    {
        public TODParser(AnimationAddedAction animationAdded)
            : base(animationAdded: animationAdded)
        {
        }

        public override string FormatName => "TOD";

        protected override void Parse(BinaryReader reader)
        {
            var fileID = reader.ReadByte();
            if (fileID == 0x50)
            {
                var animation = ParseTOD(reader);
                if (animation != null)
                {
                    AnimationResults.Add(animation);
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
                if (!animationFrames.TryGetValue(frameTime, out var animationFrame))
                {
                    animationFrame = new AnimationFrame
                    {
                        FrameTime = frameTime,
                        FrameDuration = 1,
                        AnimationObject = animationObject
                    };
                    animationFrames.Add(frameTime, animationFrame);
                }
                return animationFrame;
            }

            AnimationObject GetAnimationObject(ushort objectId)
            {
                if (!animationObjects.TryGetValue(objectId, out var animationObject))
                {
                    animationObject = new AnimationObject { Animation = animation, ID = objectId };
                    animationObjects.Add(objectId, animationObject);
                }
                return animationObject;
            }

            var version = reader.ReadByte();
            var resolution = reader.ReadUInt16();
            var frameCount = reader.ReadUInt32();
            if (frameCount == 0 || frameCount > Limits.MaxTODFrames)
            {
                return null;
            }

            animation = new Animation();
            animationObjects = new Dictionary<uint, AnimationObject>();

            var hasFrameTransform = false;
            for (var f = 0; f < frameCount; f++)
            {
                var framePosition = reader.BaseStream.Position;
                var frameLength = reader.ReadUInt16() * 4u;
                var packetCount = reader.ReadUInt16();
                if (packetCount > Limits.MaxTODPackets)
                {
                    return null;
                }
                if (frameLength < 8 + packetCount * 4)
                {
                    return null; // Invalid minimum frame size, must include header size of 8 and room for each packet header size of 4.
                }
                var frameNumber = reader.ReadUInt32();
                for (var pk = 0; pk < packetCount; pk++)
                {
                    var packetPosition = reader.BaseStream.Position;
                    var objectId = reader.ReadUInt16();
                    var packetTypeAndFlag = reader.ReadByte();
                    var packetType = (packetTypeAndFlag & 0xF);
                    var flag = (packetTypeAndFlag & 0xF0) >> 0x4;
                    var packetLength = reader.ReadByte() * 4u;
                    if (packetLength < 4)
                    {
                        return null; // Invalid minimum packet length, must include header size of 4.
                    }
                    var animationObject = GetAnimationObject(objectId);
                    if (animationObjects.Count > (int)Limits.MaxTODObjects)
                    {
                        return null;
                    }
                    var animationFrame = GetAnimationFrame(animationObject, frameNumber);
                    switch (packetType)
                    {
                        case 0b0001: //Coordinate
                            var isAbsolute  = ((flag     ) & 0x1) == 0;
                            var hasRotation = ((flag >> 1) & 0x1) != 0;
                            var hasScaling  = ((flag >> 2) & 0x1) != 0;
                            var hasTransfer = ((flag >> 3) & 0x1) != 0;
                            animationFrame.AbsoluteMatrix = isAbsolute;
                            if (hasRotation)
                            {
                                hasFrameTransform = true;
                                var rx = (reader.ReadInt32() / 4096f) * GeomMath.Deg2Rad;
                                var ry = (reader.ReadInt32() / 4096f) * GeomMath.Deg2Rad;
                                var rz = (reader.ReadInt32() / 4096f) * GeomMath.Deg2Rad;
                                animationFrame.EulerRotation = new Vector3(rx, ry, rz);
                            }
                            if (hasScaling)
                            {
                                hasFrameTransform = true;
                                var sx = reader.ReadInt16() / 4096f;
                                var sy = reader.ReadInt16() / 4096f;
                                var sz = reader.ReadInt16() / 4096f;
                                reader.ReadUInt16(); //pad
                                animationFrame.Scale = new Vector3(sx, sy, sz);
                            }
                            if (hasTransfer)
                            {
                                hasFrameTransform = true;
                                var tx = reader.ReadInt32();
                                var ty = reader.ReadInt32();
                                var tz = reader.ReadInt32();
                                animationFrame.Transfer = new Vector3(tx, ty, tz);
                            }
                            break;
                        case 0b0010: //TMD data ID
                            animationObject.TMDID.Add(reader.ReadUInt16());
                            reader.ReadUInt16(); //pad
                            break;
                        case 0b0011: //Parent Object ID
                            animationObject.ParentID = reader.ReadUInt16();
                            reader.ReadUInt16(); //pad
                            break;
                        case 0b0100: //Matrix
                            hasFrameTransform = true;
                            var m00 = reader.ReadInt16() / 4096f;
                            var m01 = reader.ReadInt16() / 4096f;
                            var m02 = reader.ReadInt16() / 4096f;

                            var m10 = reader.ReadInt16() / 4096f;
                            var m11 = reader.ReadInt16() / 4096f;
                            var m12 = reader.ReadInt16() / 4096f;

                            var m20 = reader.ReadInt16() / 4096f;
                            var m21 = reader.ReadInt16() / 4096f;
                            var m22 = reader.ReadInt16() / 4096f;

                            reader.ReadUInt16(); //pad

                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            var z = reader.ReadInt32();

                            animationFrame.Transfer = new Vector3(x, y, z);
                            // todo: Should this be transposed like other matrices?
                            animationFrame.Matrix = new Matrix3(m00, m01, m02,
                                                                m10, m11, m12,
                                                                m20, m21, m22);
                            break;
                        default:
                            break;
                    }
                    reader.BaseStream.Seek(packetPosition + packetLength, SeekOrigin.Begin);
                }
                reader.BaseStream.Seek(framePosition + frameLength, SeekOrigin.Begin);
            }

            if (hasFrameTransform && animationObjects.Count > 0)
            {
                animation.AnimationType = AnimationType.Common;
                animation.FPS = resolution == 0 ? 60f : 1f / resolution * 60f;
                animation.AssignObjects(animationObjects, true, false);
                return animation;
            }
            return null;
        }
    }
}
