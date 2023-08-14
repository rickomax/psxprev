using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public class VDFParser : FileOffsetScanner
    {
        public VDFParser(AnimationAddedAction animationAdded)
            : base(animationAdded: animationAdded)
        {
        }

        public override string FormatName => "VDF";

        protected override void Parse(BinaryReader reader)
        {
            var animation = ParseVDF(reader);
            if (animation != null)
            {
                AnimationResults.Add(animation);
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

            var frameCount = reader.ReadUInt32();
            if (frameCount < Limits.MinVDFFrames || frameCount > Limits.MaxVDFFrames)
            {
                return null;
            }

            animation = new Animation();
            animationObjects = new Dictionary<uint, AnimationObject>();

            if (Program.Debug)
            {
                Program.Logger.WriteLine("VDF---------------------------------");
                Program.Logger.WriteLine("VDF frameCount:" + frameCount);
            }
            for (uint f = 0; f < frameCount; f++)
            {
                var objectId = reader.ReadUInt32();
                if (objectId > Limits.MaxVDFObjects)
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
                if (vertexCount + skippedVertices == 0 || vertexCount + skippedVertices > Limits.MaxVDFVertices)
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

            animation.AnimationType = AnimationType.VertexDiff;
            animation.FPS = 1f;
            animation.AssignObjects(animationObjects, false, false);
            return animation;
        }
    }
}
