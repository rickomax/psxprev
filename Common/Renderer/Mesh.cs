using System;
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

    public class Mesh : MeshRenderInfo, IDisposable, IComparable<Mesh>
    {
        private readonly int _meshIndex;
        private readonly int _meshId; // Vertex array object
        private readonly int[] _bufferIds = new int[Shader.JointsSupported ? 6 : 5];
        private readonly int _positionBufferId;
        private readonly int _colorBufferId;
        private readonly int _normalBufferId;
        private readonly int _uvBufferId;
        private readonly int _tiledAreaBufferId;
        private readonly int _jointBufferId;

        private MeshDataType _meshDataType;
        private int _elementCount; // Number of elements assigned during SetData

        public MeshDataType MeshDataType => SourceMesh?._meshDataType ?? _meshDataType;
        public int VertexCount => SourceMesh?._elementCount ?? _elementCount;
        public int PrimitiveCount => VertexCount / VerticesPerElement;
        public int VerticesPerElement
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

        public Mesh SourceMesh { get; private set; } // Use mesh data of this mesh
        public Mesh OwnerMesh => SourceMesh ?? this; // Mesh who owns the mesh data
        public Matrix4 WorldMatrix { get; set; } = Matrix4.Identity;
        public int TextureID { get; set; } // Texture ID assinged by TextureBinder
        public Skin Skin { get; set; } // Skin used for joint matrices
        public bool IsDisposed { get; private set; }

        public Mesh(int meshIndex, int meshId)
        {
            _meshIndex = meshIndex;
            _meshId = meshId;
            GL.GenBuffers(_bufferIds.Length, _bufferIds);
            _positionBufferId  = _bufferIds[0];
            _colorBufferId     = _bufferIds[1];
            _normalBufferId    = _bufferIds[2];
            _uvBufferId        = _bufferIds[3];
            _tiledAreaBufferId = _bufferIds[4];
            _jointBufferId     = Shader.JointsSupported ? _bufferIds[5] : 0;
        }

        // Share the mesh data of an existing mesh, but allow different render settings
        public Mesh(int meshIndex, Mesh sourceMesh)
        {
            _meshIndex = meshIndex;
            SourceMesh = sourceMesh;
            _meshId = sourceMesh._meshId; // We use this for comparison, so might as well store it locally
            //_positionBufferId  = sourceMesh._positionBufferId;
            //_colorBufferId     = sourceMesh._colorBufferId;
            //_normalBufferId    = sourceMesh._normalBufferId;
            //_uvBufferId        = sourceMesh._uvBufferId;
            //_tiledAreaBufferId = sourceMesh._tiledAreaBufferId;
            //_jointBufferId     = sourceMesh._jointBufferId;
            //_meshDataType = sourceMesh._meshDataType;
            //_elementCount = sourceMesh._elementCount;
            IsDisposed = sourceMesh.IsDisposed;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (SourceMesh == null)
                {
                    GL.DeleteBuffers(_bufferIds.Length, _bufferIds);
                }
                else
                {
                    SourceMesh = null;
                }
            }
        }

        public void Bind()
        {
            if (SourceMesh != null)
            {
                SourceMesh.Bind();
                return;
            }

            // Bind buffers
            GL.BindVertexArray(_meshId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_Position);
            GL.VertexAttribPointer(Shader.AttributeIndex_Position, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _colorBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_Color);
            GL.VertexAttribPointer(Shader.AttributeIndex_Color, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_Normal);
            GL.VertexAttribPointer(Shader.AttributeIndex_Normal, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _uvBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_Uv);
            GL.VertexAttribPointer(Shader.AttributeIndex_Uv, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _tiledAreaBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_TiledArea);
            GL.VertexAttribPointer(Shader.AttributeIndex_TiledArea, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            if (Shader.JointsSupported)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _jointBufferId);
                GL.EnableVertexAttribArray(Shader.AttributeIndex_Joint);
                GL.VertexAttribIPointer(Shader.AttributeIndex_Joint, 2, VertexAttribIntegerType.UnsignedInt, 0, IntPtr.Zero);
            }
        }

        public void Unbind()
        {
            // Unbind buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
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

            var drawCalls = 0;

            // Draw geometry
            switch (_meshDataType)
            {
                case MeshDataType.Triangle:
                    if (drawFaces)
                    {
                        shader.PolygonMode = PolygonMode.Fill;
                        GL.DrawArrays(PrimitiveType.Triangles, 0, _elementCount);
                        drawCalls++;
                    }
                    if (drawWireframe)
                    {
                        shader.PolygonMode = PolygonMode.Line;
                        GL.DrawArrays(PrimitiveType.Triangles, 0, _elementCount);
                        drawCalls++;
                    }
                    if (drawVertices)
                    {
                        //shader.PolygonMode = PolygonMode.Point;
                        //GL.DrawArrays(PrimitiveType.Triangles, 0, _elementCount);
                        //drawCalls++;
                        goto case MeshDataType.Point;
                    }
                    break;

                case MeshDataType.Line:
                    if (drawFaces || drawWireframe)
                    {
                        GL.DrawArrays(PrimitiveType.Lines, 0, _elementCount);
                        drawCalls++;
                    }
                    if (drawVertices)
                    {
                        //shader.PolygonMode = PolygonMode.Point;
                        //GL.DrawArrays(PrimitiveType.Lines, 0, _elementCount);
                        //drawCalls++;
                        goto case MeshDataType.Point;
                    }
                    break;

                case MeshDataType.Point:
                    if (drawFaces || drawWireframe || drawVertices)
                    {
                        GL.DrawArrays(PrimitiveType.Points, 0, _elementCount);
                        drawCalls++;
                    }
                    break;
            }

            return drawCalls;
        }

        public void SetData(MeshDataType meshDataType, int elementCount, float[] positionList, float[] normalList, float[] colorList, float[] uvList, float[] tiledAreaList, uint[] jointList)
        {
            Trace.Assert(SourceMesh == null, "SetData cannot be called for mesh with a source mesh");

            _meshDataType = meshDataType;
            _elementCount = elementCount;

            BufferData(_positionBufferId,  positionList,  3); // Vector3
            BufferData(_normalBufferId,    normalList,    3); // Vector3
            BufferData(_colorBufferId,     colorList,     3); // Vector3 (Color)
            BufferData(_uvBufferId,        uvList,        2); // Vector2
            BufferData(_tiledAreaBufferId, tiledAreaList, 4); // Vector4
            if (Shader.JointsSupported)
            {
                BufferData(_jointBufferId,     jointList,     2); // uint[2]
            }
        }

        // Passing null for list will fill the data with zeros.
        private void BufferData<T>(int bufferId, T[] list, int elementSize) where T : struct
        {
            var length = _elementCount * elementSize;
            if (list == null)
            {
                list = new T[length]; // Treat null as zeroed data.
            }
            else
            {
                Trace.Assert(list.Length >= length, "BufferData cannot use list that's smaller than expected length");
            }
            var size = (IntPtr)(length * 4);// sizeof(T));

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw);
        }

        public int CompareTo(Mesh other)
        {
            var semiTransparent = RenderFlags.HasFlag(RenderFlags.SemiTransparent);
            if (semiTransparent != other.RenderFlags.HasFlag(RenderFlags.SemiTransparent))
            {
                return (!semiTransparent ? -1 : 1);
            }
            else if (!semiTransparent)
            {
                // Only sort opaque meshes, since the draw order of semi-transparent meshes matters
                if (Skin != other.Skin)
                {
                    if ((Skin == null) != (other.Skin == null))
                    {
                        return (Skin == null ? -1 : 1);
                    }
                    return Skin.CompareTo(other.Skin);
                }
                //else if (MixtureRate != other.MixtureRate)
                //{
                //    return MixtureRate.CompareTo(other.MixtureRate);
                //}
                else if (TextureID != other.TextureID)
                {
                    return TextureID.CompareTo(other.TextureID);
                }
                else if (RenderFlags != other.RenderFlags)
                {
                    return RenderFlags.CompareTo(other.RenderFlags);
                }
            }
            return _meshIndex.CompareTo(other._meshIndex);
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignVector2(float[] data, int baseIndex, ref Vector2 vector)
        {
            var index = baseIndex * 2;
            data[index++] = vector.X;
            data[index++] = vector.Y;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignVector3(float[] data, int baseIndex, ref Vector3 vector)
        {
            var index = baseIndex * 3;
            data[index++] = vector.X;
            data[index++] = vector.Y;
            data[index++] = vector.Z;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignVector4(float[] data, int baseIndex, ref Vector4 vector)
        {
            var index = baseIndex * 4;
            data[index++] = vector.X;
            data[index++] = vector.Y;
            data[index++] = vector.Z;
            data[index++] = vector.W;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignColor(float[] data, int baseIndex, Color color)
        {
            var index = baseIndex * 3;
            data[index++] = color.R;
            data[index++] = color.G;
            data[index++] = color.B;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignJoint(uint[] data, int baseIndex, uint? vertexJoint, uint? normalJoint)
        {
            var index = baseIndex * 2;
            data[index++] = (vertexJoint ?? Triangle.NoJoint) + 1u;
            data[index++] = (normalJoint ?? Triangle.NoJoint) + 1u;
        }
    }
}