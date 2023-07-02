﻿using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;

namespace PSXPrev.Classes
{
    public class ModelEntity : EntityBase
    {
        [DisplayName("VRAM Page")]
        public uint TexturePage { get; set; }
        
        [ReadOnly(true), DisplayName("Total Triangles")]
        public int TrianglesCount => Triangles.Length;

        [Browsable(false)]
        public Triangle[] Triangles { get; set; }

        [Browsable(false)]
        public Texture Texture { get; set; }
        
        [DisplayName("TMD ID")]
        public uint TMDID { get; set; }

       //[ReadOnly(true)]
       //public uint PrimitiveIndex { get; set; }

        [Browsable(false)]
        public int MeshIndex { get; set; }

        public bool Visible { get; set; } = true;

        public Matrix4 TempMatrix { get; set; } = Matrix4.Identity;
        public float Interpolator { get; set; }
        public Vector3[] InitialVertices { get; set; }
        public Vector3[] FinalVertices { get; set; }
        public Vector3[] FinalNormals { get; set; }
        public Vector3[] InitialNormals { get; set; }

        // HMD: Attachable (shared) vertices and normals that aren't tied to an existing triangle.
        [Browsable(false)]
        public Dictionary<uint, Vector3> AttachableVertices { get; set; }

        [Browsable(false)]
        public Dictionary<uint, Vector3> AttachableNormals { get; set; }

        public override void ComputeBounds()
        {
            base.ComputeBounds();
            var bounds = new BoundingBox();
            var worldMatrix = WorldMatrix;
            foreach (var triangle in Triangles)
            {
                if (triangle.Vertices != null)
                {
                    for (var i = 0; i < triangle.Vertices.Length; i++)
                    {
                        if (triangle.AttachedIndices != null)
                        {
                            if (triangle.AttachedIndices[i] != uint.MaxValue)
                            {
                                continue;
                            }
                        }
                        var vertex = triangle.Vertices[i];
                        bounds.AddPoint(Vector3.TransformPosition(vertex, worldMatrix));
                    }

                }
            }
            Bounds3D = bounds;
        }

        public override void FixConnections()
        {
            var rootEntity = GetRootEntity();
            if (rootEntity != null)
            {
                foreach (var triangle in Triangles)
                {
                    if (triangle.AttachedIndices == null)
                    {
                        continue;
                    }
                    for (var i = 0; i < 3; i++)
                    {
                        var attachedIndex = triangle.AttachedIndices[i];
                        var attachedNormalIndex = triangle.AttachedNormalIndices?[i] ?? uint.MaxValue;
                        // AttachedNormalIndices should only ever be non-null when AttachedIndices is non-null.
                        if (attachedIndex != uint.MaxValue)
                        {
                            foreach (ModelEntity subModel in rootEntity.ChildEntities)
                            {
                                if (subModel == this)
                                {
                                    continue;
                                }
                                foreach (var subTriangle in subModel.Triangles)
                                {
                                    for (var j = 0; j < subTriangle.Vertices.Length; j++)
                                    {
                                        if (subTriangle.AttachableIndices[j] == attachedIndex)
                                        {
                                            var newVertex = Vector3.TransformPosition(subTriangle.Vertices[j], subModel.WorldMatrix);
                                            newVertex = Vector3.TransformPosition(newVertex, WorldMatrix.Inverted());
                                            triangle.Vertices[i] = newVertex;
                                            break;
                                        }
                                    }
                                }

                                // HMD: Check for attachable vertices and normals that aren't associated with an existing triangle.
                                if (subModel.AttachableVertices != null && subModel.AttachableVertices.TryGetValue(attachedIndex, out var attachedVertex))
                                {
                                    var newVertex = Vector3.TransformPosition(attachedVertex, subModel.WorldMatrix);
                                    triangle.Vertices[i] = newVertex;
                                }
                                if (subModel.AttachableNormals != null && subModel.AttachableNormals.TryGetValue(attachedNormalIndex, out var attachedNormal))
                                {
                                    triangle.Normals[i] = attachedNormal;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}