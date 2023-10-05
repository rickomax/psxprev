using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    public enum MeshDataType
    {
        Triangle,
        Line,
        Point,
    }

    public class Mesh : MeshRenderInfo, IComparable<Mesh>
    {
        // Static arrays used for calls to GL.MultiDrawArrays.
        // We can make these static, since there's no way we're going to be making GL calls on multiple threads.
        // These arrays are only populated and used within the Mesh.Draw call.
        private static int[] _batchedFirsts = null;
        private static int[] _batchedCounts = null;


        private MeshDataType _meshDataType;
        private int _elementCount; // Number of elements assigned during SetData
        private int _elementFirst;

        public MeshDataType MeshDataType => SourceMesh?._meshDataType ?? _meshDataType;
        public int VertexCount => SourceMesh?._elementCount ?? _elementCount;
        public int VertexFirst => SourceMesh?._elementFirst ?? _elementFirst;
        public int PrimitiveCount => VertexCount / VerticesPerPrimitive;

        public int BatchedVertexCount
        {
            get
            {
                var count = VertexCount;
                if (BatchedMeshes != null)
                {
                    foreach (var batchedMesh in BatchedMeshes)
                    {
                        count += batchedMesh.VertexCount;
                    }
                }
                return count;
            }
        }
        public int BatchedPrimitiveCount => BatchedVertexCount / VerticesPerPrimitive;
        public int BatchedMeshCount => 1 + (BatchedMeshes?.Count ?? 0);

        public int VerticesPerPrimitive
        {
            get
            {
                switch (MeshDataType)
                {
                    case MeshDataType.Triangle:
                        return 3;
                    case MeshDataType.Line:
                        return 2;
                    case MeshDataType.Point:
                    default:
                        return 1;
                }
            }
        }

        //private static readonly int _identityMatrixHashCode = Matrix4.Identity.GetHashCode();
        private Matrix4 _worldMatrix;// = Matrix4.Identity;
        private int _worldMatrixHashCode;// = _identityMatrixHashCode;
        public Matrix4 WorldMatrix
        {
            get => _worldMatrix;
            set
            {
                _worldMatrix = value;
                _worldMatrixHashCode = value.GetHashCode();
            }
        }

        public int OrderIndex { get; set; }
        public Mesh SourceMesh { get; private set; } // Use mesh data of this mesh
        public VertexArrayObject VertexArrayObject { get; private set; }
        public Skin Skin { get; set; } // Skin used for joint matrices
        public int TextureID { get; set; } // Texture ID assinged by TextureBinder
        public bool IsBatchable { get; set; }
        public bool IsBatched => BatchedParent != null;
        public Mesh BatchedParent { get; private set; }
        public List<Mesh> BatchedMeshes { get; private set; }

        public bool HasTexture => TextureID != 0 && RenderFlags.HasFlag(RenderFlags.Textured);

        public Mesh(int orderIndex, VertexArrayObject vao)
        {
            OrderIndex = orderIndex;
            VertexArrayObject = vao;
        }

        // Share the mesh data of an existing mesh, but allow different render settings
        // SetData/SetDataSource should not be called for this mesh
        public Mesh(int orderIndex, Mesh sourceMesh)
        {
            OrderIndex = orderIndex;
            SourceMesh = sourceMesh;
            VertexArrayObject = sourceMesh.VertexArrayObject;
        }

        public int Draw(Shader shader, bool drawFaces = true, bool drawWireframe = false, bool drawVertices = false, float wireframeSize = 1f, float vertexSize = 1f)
        {
            if (SourceMesh != null)
            {
                return SourceMesh.Draw(shader, drawFaces, drawWireframe, drawVertices, wireframeSize, vertexSize);
            }

            // Setup point size and/or line width
            if (drawVertices || _meshDataType == MeshDataType.Point)
            {
                if (drawVertices && drawFaces && _meshDataType == MeshDataType.Point)
                {
                    vertexSize = Math.Max(vertexSize, Thickness);
                }
                shader.PointSize = drawVertices ? vertexSize : Thickness;
            }
            if (drawWireframe || _meshDataType == MeshDataType.Line)
            {
                if (drawWireframe && drawFaces && _meshDataType == MeshDataType.Line)
                {
                    wireframeSize = Math.Max(wireframeSize, Thickness);
                }
                shader.LineWidth = drawWireframe ? wireframeSize : Thickness;
            }

            // Check if we need to use MultiDrawArrays for batching
            var multiCount = BatchedMeshCount;
            if (multiCount > 1)
            {
                if (_batchedFirsts == null || _batchedFirsts.Length < multiCount)
                {
                    _batchedFirsts = new int[multiCount];
                    _batchedCounts = new int[multiCount];
                }
                _batchedFirsts[0] = _elementFirst;
                _batchedCounts[0] = _elementCount;
                for (var i = 0; i < BatchedMeshes.Count; i++)
                {
                    var batchedMesh = BatchedMeshes[i];
                    _batchedFirsts[i + 1] = batchedMesh._elementFirst;
                    _batchedCounts[i + 1] = batchedMesh._elementCount;
                }
            }

            var drawCalls = 0;

            // Draw geometry
            switch (_meshDataType)
            {
                case MeshDataType.Triangle:
                    if (drawFaces)
                    {
                        shader.PolygonMode = PolygonMode.Fill;
                        DrawArrays(PrimitiveType.Triangles);
                        drawCalls++;
                    }
                    if (drawWireframe)
                    {
                        shader.PolygonMode = PolygonMode.Line;
                        DrawArrays(PrimitiveType.Triangles);
                        drawCalls++;
                    }
                    if (drawVertices)
                    {
                        //shader.PolygonMode = PolygonMode.Point;
                        //DrawArrays(PrimitiveType.Triangles);
                        //drawCalls++;
                        goto case MeshDataType.Point;
                    }
                    break;

                case MeshDataType.Line:
                    if (drawFaces || drawWireframe)
                    {
                        // Note that changing PolygonMode has no effect for Lines or Points
                        DrawArrays(PrimitiveType.Lines);
                        drawCalls++;
                    }
                    if (drawVertices)
                    {
                        goto case MeshDataType.Point;
                    }
                    break;

                case MeshDataType.Point:
                    if (drawFaces || drawWireframe || drawVertices)
                    {
                        DrawArrays(PrimitiveType.Points);
                        drawCalls++;
                    }
                    break;
            }

            return drawCalls;
        }

        private void DrawArrays(PrimitiveType primitiveType)
        {
            var multiCount = BatchedMeshCount;
            if (multiCount > 1)
            {
                GL.MultiDrawArrays(primitiveType, _batchedFirsts, _batchedCounts, multiCount);
            }
            else
            {
                GL.DrawArrays(primitiveType, _elementFirst, _elementCount);
            }
        }

        // Set the data for a mesh that shares ownership of a VertexArrayObject
        // VertexArrayObject.SetData must be handled by the caller (and can be called before or after this function)
        public void SetDataSource(MeshDataType meshDataType, int elementFirst, int elementCount)
        {
            Trace.Assert(SourceMesh == null, nameof(SetDataSource) + " cannot be called for mesh with a source mesh");

            _meshDataType = meshDataType;
            _elementCount = elementCount;
            _elementFirst = elementFirst;
        }

        // Set the data for a mesh that is the sole owner of a VertexArrayObject
        // VertexArrayObject.SetData should NOT be handled by the caller
        public void SetData(MeshDataType meshDataType, int elementCount, float[] positionList, float[] normalList, float[] colorList, float[] uvList, float[] tiledAreaList, uint[] jointList)
        {
            Trace.Assert(SourceMesh == null, nameof(SetData) + " cannot be called for mesh with a source mesh");

            VertexArrayObject.SetData(elementCount, positionList, normalList, colorList, uvList, tiledAreaList, jointList);

            _meshDataType = meshDataType;
            _elementCount = elementCount;
            _elementFirst = 0;
        }

        // This is intended for Meshes created from ModelEntitys, so some render settings are ignored
        public int CompareTo(Mesh other)
        {
            // Sorting by VAO is essential to allow for more batching
            if (VertexArrayObject != other.VertexArrayObject)
            {
                return VertexArrayObject.CompareTo(other.VertexArrayObject);
            }

            // Only sort opaque meshes, since the draw order of semi-transparent meshes matters
            var semiTransparent = RenderFlags.HasFlag(RenderFlags.SemiTransparent);
            if (semiTransparent != other.RenderFlags.HasFlag(RenderFlags.SemiTransparent))
            {
                return (!semiTransparent ? -1 : 1);
            }
            else if (!semiTransparent)
            {
                if (Skin != other.Skin)
                {
                    if ((Skin == null) != (other.Skin == null))
                    {
                        return (Skin == null ? -1 : 1);
                    }
                    return Skin.CompareTo(other.Skin);
                }

                if (TextureID != other.TextureID && RenderFlags.HasFlag(RenderFlags.Textured))
                {
                    return TextureID.CompareTo(other.TextureID);
                }

                // Only sort by render flags that we support
                var supportedRenderFlags = RenderFlags & RenderInfo.SupportedFlags;
                var otherSupportedRenderFlags = other.RenderFlags & RenderInfo.SupportedFlags;
                if (supportedRenderFlags != otherSupportedRenderFlags)
                {
                    return ((uint)supportedRenderFlags).CompareTo((uint)otherSupportedRenderFlags);
                }

                // We don't sort semi-transparenct surfaces
                //if (MixtureRate != other.MixtureRate && RenderFlags.HasFlag(RenderFlags.SemiTransparent))
                //{
                //    return MixtureRate.CompareTo(other.MixtureRate);
                //}

                if (MissingTexture != other.MissingTexture && RenderFlags.HasFlag(RenderFlags.Textured))
                {
                    return MissingTexture.CompareTo(other.MissingTexture);
                }

                // This information is rarely different, and is not worth sorting by
                //if (MeshDataType != other.MeshDataType)
                //{
                //    return MeshDataType.CompareTo(other.MeshDataType);
                //}
                //if (Visible != other.Visible)
                //{
                //    return Visible.CompareTo(other.Visible);
                //}

                // Note: The benefits of sorting matrices is somewhat limited, and doesn't reduce the number of draw calls by much.
                // It probably depends on the sorting algorithm used, but putting meshes even more out of order could potentially be slower,
                // although not by a significant amount.
                if (_worldMatrixHashCode != other._worldMatrixHashCode)
                {
                    return _worldMatrixHashCode.CompareTo(other._worldMatrixHashCode);
                }
            }
            return OrderIndex.CompareTo(other.OrderIndex);
        }

        // This is intended for Meshes created from ModelEntitys, so some render settings are ignored
        // Batching is not supported for sprites
        public bool CanBatch(Mesh other)
        {
            // Ordered by most-to-least likely to be different (except Matrix4.Equals)
            return (other.IsBatchable &&
                    VertexArrayObject    == other.VertexArrayObject &&
                    Skin                 == other.Skin &&
                    _worldMatrixHashCode == other._worldMatrixHashCode &&
                    ((RenderFlags ^ other.RenderFlags) & RenderInfo.SupportedFlags) == 0 &&
                    (TextureID           == other.TextureID      || !RenderFlags.HasFlag(RenderFlags.Textured)) &&
                    (MixtureRate         == other.MixtureRate    || !RenderFlags.HasFlag(RenderFlags.SemiTransparent)) &&
                    (MissingTexture      == other.MissingTexture || !RenderFlags.HasFlag(RenderFlags.Textured)) &&
                    MeshDataType         == other.MeshDataType &&
                    Visible              == other.Visible &&
                    //TextureAnimation.Equals(other.TextureAnimation) &&
                    _worldMatrix.Equals(other._worldMatrix));
        }

        public void ResetBatch()
        {
            /*if (BatchedMeshes != null)
            {
                foreach (var batchedMesh in BatchedMeshes)
                {
                    batchedMesh.BatchedParent = null;
                }
                BatchedMeshes.Clear();
            }*/
            BatchedMeshes?.Clear();
            BatchedParent = null;
        }

        public void AddToBatch(Mesh other)
        {
            if (BatchedMeshes == null)
            {
                BatchedMeshes = new List<Mesh>();
            }
            BatchedMeshes.Add(other);
            other.BatchedParent = this; // Mark this mesh as batched, so that we don't try to draw it by itself
        }

        public bool RemoveFromBatch(Mesh other)
        {
            if (BatchedMeshes?.Remove(other) ?? false)
            {
                other.BatchedParent = null;
                return true;
            }
            return false;
        }

        public bool ContainsInBatch(Mesh other)
        {
            return BatchedMeshes?.Contains(other) ?? false;
        }
    }
}