using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Classes
{
    public class TODParser : FileOffsetScanner
    {
        public TODParser(AnimationAddedAction animationAdded)
            : base(animationAdded: animationAdded)
        {
        }

        public override string FormatName => "TOD";

        protected override void Parse(BinaryReader reader, string fileTitle, out List<RootEntity> entities, out List<Animation> animations, out List<Texture> textures)
        {
            entities = null;
            animations = null;
            textures = null;

            var fileID = reader.ReadByte();
            if (fileID == 0x50)
            {
                var animation = ParseTOD(reader);
                if (animation != null)
                {
                    animations = new List<Animation> { animation };
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

                    //animation.FrameCount = Math.Max(animation.FrameCount, animationFrame.FrameEnd);
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
            if (frameCount == 0 || frameCount > Program.MaxTODFrames)
            {
                return null;
            }

            animation = new Animation();
            animationObjects = new Dictionary<uint, AnimationObject>();

            for (var f = 0; f < frameCount; f++)
            {
                var framePosition = reader.BaseStream.Position;
                var frameSize = reader.ReadUInt16();
                var packetCount = reader.ReadUInt16();
                if (packetCount > Program.MaxTODPackets)
                {
                    return null;
                }
                var frameNumber = reader.ReadUInt32();
                if (packetCount == 0 || frameSize == 0)
                {
                    continue;
                }
                for (var p = 0; p < packetCount; p++)
                {
                    var packetPosition = reader.BaseStream.Position;
                    var objectId = reader.ReadUInt16();
                    var packetTypeAndFlag = reader.ReadByte();
                    var packetType = (packetTypeAndFlag & 0xF);
                    var flag = (packetTypeAndFlag & 0xF0) >> 0x4;
                    var packetLength = reader.ReadByte();
                    var animationObject = GetAnimationObject(objectId);
                    var animationFrame = GetAnimationFrame(animationObject, frameNumber);
                    switch (packetType)
                    {
                        case 0b0001: //Coordinate
                            var matrixType = (flag & 0x1);
                            var rotation = (flag & 0x2) >> 0x1;
                            var scaling = (flag & 0x4) >> 0x2;
                            var transfer = (flag & 0x8) >> 0x3;
                            if (rotation != 0x00)
                            {
                                var rx = (reader.ReadInt32() / 4096f) * GeomUtils.Deg2Rad;
                                var ry = (reader.ReadInt32() / 4096f) * GeomUtils.Deg2Rad;
                                var rz = (reader.ReadInt32() / 4096f) * GeomUtils.Deg2Rad;
                                animationFrame.EulerRotation = new Vector3(rx, ry, rz);
                            }
                            if (scaling != 0x00)
                            {
                                var sx = reader.ReadInt16() / 4096f;
                                var sy = reader.ReadInt16() / 4096f;
                                var sz = reader.ReadInt16() / 4096f;
                                reader.ReadUInt16();
                                animationFrame.Scale = new Vector3(sx, sy, sz);
                            }
                            if (transfer != 0x00)
                            {
                                float tx = reader.ReadInt32();
                                float ty = reader.ReadInt32();
                                float tz = reader.ReadInt32();
                                animationFrame.Transfer = new Vector3(tx, ty, tz);
                            }
                            animationFrame.AbsoluteMatrix = matrixType == 0x00;
                            break;
                        case 0b0010: //TMD data ID
                            animationObject.TMDID.Add(reader.ReadUInt16());
                            reader.ReadUInt16();
                            break;
                        case 0b0011: //Parent Object ID
                            animationObject.ParentID = reader.ReadUInt16();
                            reader.ReadUInt16();
                            break;
                        case 0b0100: //Matrix
                            var r00 = reader.ReadInt16() / 4096f;
                            var r01 = reader.ReadInt16() / 4096f;
                            var r02 = reader.ReadInt16() / 4096f;

                            var r10 = reader.ReadInt16() / 4096f;
                            var r11 = reader.ReadInt16() / 4096f;
                            var r12 = reader.ReadInt16() / 4096f;

                            var r20 = reader.ReadInt16() / 4096f;
                            var r21 = reader.ReadInt16() / 4096f;
                            var r22 = reader.ReadInt16() / 4096f;

                            reader.ReadInt16();

                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            var z = reader.ReadInt32();

                            var matrix = new Matrix3(new Vector3(r00, r01, r02), new Vector3(r10, r11, r12), new Vector3(r20, r21, r22));
                            animationFrame.Transfer = new Vector3(x, y, z);
                            animationFrame.Matrix = matrix;
                            animationFrame.AbsoluteMatrix = true;
                            break;
                        default:
                            reader.BaseStream.Seek(packetPosition + packetLength * 4, SeekOrigin.Begin);
                            break;
                    }
                }
            }
            animation.AnimationType = AnimationType.Common;
            animation.FPS = resolution == 0 ? 60f : 1f / resolution * 60f;
            animation.AssignObjects(animationObjects, false, false);
            return animation;
        }
    }
}
