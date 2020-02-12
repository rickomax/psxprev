using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Classes
{
    public class ANParser
    {
        private long _offset;
        private readonly Action<Animation, long> _entityAddedAction;

        public ANParser(Action<Animation, long> entityAdded)
        {
            _entityAddedAction = entityAdded;
        }

        public void LookForAN(BinaryReader reader, string fileTitle)
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
                    var magic = reader.ReadUInt16();
                    if (magic == 0xAAAA)
                    {
                        var animation = ParseAN(reader);
                        if (animation != null)
                        {
                            animation.AnimationName = string.Format("{0}{1:x}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            _entityAddedAction(animation, reader.BaseStream.Position);
                            Program.Logger.WriteLine("Found AN Animation at offset {0:X}", _offset);
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
                        Program.Logger.WriteLine("Reached file end");
                        return;
                    }
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
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
                if (!animationObject.AnimationFrames.TryGetValue(frameTime, out var animationFrame))
                {
                    animationFrame = new AnimationFrame { FrameTime = frameTime, AnimationObject = animationObject };
                    animationFrames.Add(frameTime, animationFrame);
                }
                if (frameTime >= animation.FrameCount)
                {
                    animation.FrameCount = frameTime + 1;
                }
                return animationFrame;
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
            var rootAnimationObject = new AnimationObject();
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
            animation.AnimationType = AnimationType.Common;
            animation.RootAnimationObject = rootAnimationObject;
            animation.ObjectCount = animationObjects.Count;
            animation.FPS = frameRate;
            return animation;
        }
    }
}
