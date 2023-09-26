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
            if (numJoints == 0 || numJoints > Limits.MaxANJoints)
            {
                return null;
            }
            var numFrames = reader.ReadUInt16();
            if (numFrames == 0 || numFrames > Limits.MaxANFrames)
            {
                return null;
            }
            if (numFrames * numJoints > Limits.MaxANTotalFrames)
            {
                return null; // Sanity check, too many key frames
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
            if (Math.Abs((long)translationTop - rotationTop) < 8)
            {
                return null; // Sanity check, transform tops overlap each other
            }
            if (translationTop < 16 || rotationTop < 16)
            {
                return null; // Sanity check, tops are less than file header size
            }

            for (uint f = 0; f < numFrames; f++)
            {
                // todo: Each seek for each joint will seek to the exact same position,
                // since it uses frame index and not joint index. Is something else missing?
                for (uint i = 0; i < numJoints; i++)
                {
                    reader.BaseStream.Seek(_offset + translationTop + 64 * f, SeekOrigin.Begin);
                    var animationObject = GetAnimationObject(i);
                    var animationFrame = GetAnimationFrame(animationObject, f);
                    // todo: Shouldn't these be signed?
                    var tx = reader.ReadUInt16();
                    var ty = reader.ReadUInt16();
                    var tz = reader.ReadUInt16();
                    var pad = reader.ReadUInt16();
                    animationFrame.Transfer = new Vector3(tx, ty, tz);
                }
                for (uint i = 0; i < numJoints; i++)
                {
                    reader.BaseStream.Seek(_offset + rotationTop + 64 * f, SeekOrigin.Begin);
                    var animationObject = GetAnimationObject(i);
                    var animationFrame = GetAnimationFrame(animationObject, f);
                    // todo: Shouldn't these be signed?
                    // todo: Shouldn't rotation be divided by 4096f? And maybe converted from radians if this format uses degrees
                    var rx = reader.ReadUInt16();
                    var ry = reader.ReadUInt16();
                    var rz = reader.ReadUInt16();
                    var pad = reader.ReadUInt16();
                    animationFrame.EulerRotation = new Vector3(rx, ry, rz);
                }
            }

            animation.AnimationType = AnimationType.Common;
            animation.FPS = frameRate;
            animation.AssignObjects(animationObjects, true, false);
            return animation;
        }
    }
}
