using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Classes
{
    public class VDFParser
    {
        private long _offset;
        private readonly Action<Animation, long> _animationAddedAction;

        public VDFParser(Action<Animation, long> animationAdded)
        {
            _animationAddedAction = animationAdded;
        }

        public void LookForVDF(BinaryReader reader, string fileTitle)
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
                    var animation = ParseVDF(reader);
                    if (animation != null)
                    {
                        animation.AnimationName = string.Format("{0}{1:x}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                        _animationAddedAction(animation, _offset);
                        Program.Logger.WritePositiveLine("Found VDF Animation at offset {0:X}", _offset);
                        _offset = reader.BaseStream.Position;
                        passed = true;
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
                        Program.Logger.WriteLine($"VDF - Reached file end: {fileTitle}");
                        return;
                    }
                    reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
                }
            }
        }

        private static Animation ParseVDF(BinaryReader reader)
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
                var animationObject = new AnimationObject { Animation = animation, ID = objectId};
                animationObject.TMDID.Add(objectId);
                animationObjects.Add(objectId, animationObject);
                return animationObject;
            }
            var frameCount = reader.ReadUInt32();
            if (frameCount < Program.MinVDFFrames || frameCount > Program.MaxVDFFrames)
            {
                return null;
            }
            animation = new Animation();
            var rootAnimationObject = new AnimationObject();
            animationObjects = new Dictionary<uint, AnimationObject>();
            if (Program.Debug)
            {
                Program.Logger.WriteLine("VDF---------------------------------");
                Program.Logger.WriteLine("VDF frameCount:" + frameCount);
            }
            for (uint f = 0; f < frameCount; f++)
            {
                var objectId = reader.ReadUInt32();
                if (objectId > Program.MaxVDFObjects)
                {
                    return null;
                }
                if (Program.Debug)
                {
                    Program.Logger.WriteLine("  VDF objectId:" + objectId);
                }
                var vertexOffset = reader.ReadUInt32();
                var skippedVertices = vertexOffset / 8;
                var vertexCount = reader.ReadUInt32();
                if (vertexCount + skippedVertices == 0 || vertexCount + skippedVertices > Program.MaxVDFVertices)
                {
                    return null;
                }
                if (Program.Debug)
                {
                    Program.Logger.WriteLine("  VDF skippedVertices:" + skippedVertices);
                }
                if (Program.Debug)
                {
                    Program.Logger.WriteLine("  VDF vertexCount:" + vertexCount);
                }
                var animationObject = GetAnimationObject(objectId);
                //if (f == 0)
                //{
                //    var firstAnimationFrame = GetAnimationFrame(animationObject, 0);
                //    var firstVertices = new Vector3[vertexCount + skippedVertices];
                //    firstAnimationFrame.Vertices = firstVertices;
                //    firstAnimationFrame.TempVertices = new Vector3[firstAnimationFrame.Vertices.Length];
                //}
                var animationFrame = GetNextAnimationFrame(animationObject);
                var vertices = new Vector3[vertexCount + skippedVertices];
                for (var i = 0; i < vertexCount; i++)
                {
                    var vx = reader.ReadInt16();
                    var vy = reader.ReadInt16();
                    var vz = reader.ReadInt16();
                    reader.ReadInt16();
                    var vertex = new Vector3(vx, vy, vz);
                    vertices[i + skippedVertices] = vertex;
                    //if (Program.Debug)
                    //{
                    //    Program.Logger.WriteLine("      VDF vertex:" + vertex);
                    //}
                }
                animationFrame.Vertices = vertices;
                animationFrame.TempVertices = new Vector3[animationFrame.Vertices.Length];

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
    }
}
