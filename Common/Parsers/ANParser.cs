using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public class ANParser : FileOffsetScanner
    {
        public ANParser(AnimationAddedAction animationAdded)
            : base(animationAdded: animationAdded)
        {
        }

        public override string FormatName => "AN";

        protected override void Parse(BinaryReader reader)
        {
            var magic = reader.ReadUInt16();
            if (magic == 0xAAAA)
            {
                var animation = ParseAN(reader);
                if (animation != null)
                {
                    AnimationResults.Add(animation);
                }
            }
        }

        private Animation ParseAN(BinaryReader reader)
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

            var version = reader.ReadByte();
            var numJoints = reader.ReadByte();
            if (numJoints == 0 || numJoints > Program.MaxANJoints)
            {
                return null;
            }
            var numFrames = reader.ReadUInt16();
            if (numFrames == 0 || numFrames > Program.MaxANFrames)
            {
                return null;
            }
            var frameRate = reader.ReadUInt16();
            if (frameRate == 0 || frameRate > 60)
            {
                return null;
            }

            animation = new Animation();
            animationObjects = new Dictionary<uint, AnimationObject>();

            var translationTop = reader.ReadUInt32();
            var rotationTop = reader.ReadUInt32();
            for (uint f = 0; f < numFrames; f++)
            {
                for (uint i = 0; i < numJoints; i++)
                {
                    reader.BaseStream.Seek(_offset + translationTop + 64 * f, SeekOrigin.Begin);
                    var animationObject = GetAnimationObject(i);
                    var animationFrame = GetAnimationFrame(animationObject, f);
                    var x = reader.ReadUInt16();
                    var y = reader.ReadUInt16();
                    var z = reader.ReadUInt16();
                    var pad = reader.ReadUInt16();
                    animationFrame.Translation = new Vector3(x,y,z);
                }
                for (uint i = 0; i < numJoints; i++)
                {
                    reader.BaseStream.Seek(_offset + rotationTop + 64 * f, SeekOrigin.Begin);
                    var animationObject = GetAnimationObject(i);
                    var animationFrame = GetAnimationFrame(animationObject, f);
                    var x = reader.ReadUInt16();
                    var y = reader.ReadUInt16();
                    var z = reader.ReadUInt16();
                    var pad = reader.ReadUInt16();
                    animationFrame.EulerRotation = new Vector3(x,y,z);
                }
            }

            animation.AnimationType = AnimationType.Common;
            animation.FPS = frameRate;
            animation.AssignObjects(animationObjects, false, false);
            return animation;
        }
    }
}
