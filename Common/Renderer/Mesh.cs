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

    public class Mesh : MeshRenderInfo, IDisposable
    {
        private readonly uint _meshId;
        private readonly uint[] _bufferIds = new uint[Scene.JointsSupported ? 6 : 5];
        private readonly uint _positionBufferId;
        private readonly uint _colorBufferId;
        private readonly uint _normalBufferId;
        private readonly uint _uvBufferId;
        private readonly uint _tiledAreaBufferId;
        private readonly uint _jointBufferId;

        private MeshDataType _meshDataType;
        private int _elementCount; // Number of elements assigned during SetData

        public MeshDataType MeshDataType => _meshDataType;
        public int VertexCount => _elementCount;
        public int PrimitiveCount => _elementCount / VerticesPerElement;
        public int VerticesPerElement
        {
            get
            {
                switch (_meshDataType)
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

        public Matrix4 WorldMatrix { get; set; } = Matrix4.Identity;
        public uint TextureID { get; set; } // Texture ID assinged by TextureBinder
        public Skin Skin { get; set; } // Skin used for joint matrices

        public Mesh(uint meshId)
        {
            _meshId = meshId;
            GL.GenBuffers(_bufferIds.Length, _bufferIds);
            _positionBufferId  = _bufferIds[0];
            _colorBufferId     = _bufferIds[1];
            _normalBufferId    = _bufferIds[2];
            _uvBufferId        = _bufferIds[3];
            _tiledAreaBufferId = _bufferIds[4];
            _jointBufferId     = Scene.JointsSupported ? _bufferIds[5] : 0u;
        }

        public void Dispose()
        {
            GL.DeleteBuffers(_bufferIds.Length, _bufferIds);
        }

        public int Draw(TextureBinder textureBinder = null, bool drawFaces = true, bool drawWireframe = false, bool drawVertices = false, float wireframeSize = 1f, float vertexSize = 1f)
        {
            // Bind buffers
            GL.BindVertexArray(_meshId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBufferId);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexPosition);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexPosition, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBufferId);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexNormal);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexNormal, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            // True argument normalizes normals
            GL.BindBuffer(BufferTarget.ArrayBuffer, _colorBufferId);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexColor);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexColor, 3, VertexAttribPointerType.Float, true, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _uvBufferId);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexUv);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexUv, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _tiledAreaBufferId);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexTiledArea);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexTiledArea, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            if (Scene.JointsSupported)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _jointBufferId);
                GL.EnableVertexAttribArray((uint)Scene.AttributeIndexJoint);
                GL.VertexAttribIPointer((uint)Scene.AttributeIndexJoint, 2, VertexAttribIntegerType.UnsignedInt, 0, IntPtr.Zero);
            }

            // Bind joint matrices
            if (Scene.JointsSupported && Skin != null)
            {
                Skin.Bind();
            }

            // Bind texture
            if (textureBinder != null && TextureID != 0)
            {
                textureBinder.BindTexture(TextureID);
            }

            // Setup point size and/or line width
            if (drawVertices || _meshDataType == MeshDataType.Point)
            {
                if (drawVertices && drawFaces && _meshDataType == MeshDataType.Point)
                {
                    vertexSize = Math.Max(vertexSize, Thickness);
                }
                GL.PointSize(drawVertices ? vertexSize : Thickness);
            }
            if (drawWireframe || _meshDataType == MeshDataType.Line)
            {
                if (drawWireframe && drawFaces && _meshDataType == MeshDataType.Line)
                {
                    wireframeSize = Math.Max(wireframeSize, Thickness);
                }
                GL.LineWidth(drawWireframe ? wireframeSize : Thickness);
            }

            var drawCalls = 0;

            // Draw geometry
            switch (_meshDataType)
            {
                case MeshDataType.Triangle:
                    if (drawFaces)
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                        GL.DrawArrays(PrimitiveType.Triangles, 0, _elementCount);
                        drawCalls++;
                    }
                    if (drawWireframe)
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                        GL.DrawArrays(PrimitiveType.Triangles, 0, _elementCount);
                        drawCalls++;
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    }
                    if (drawVertices)
                    {
                        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                        //GL.DrawArrays(PrimitiveType.Triangles, 0, _elementCount);
                        //drawCalls++;
                        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
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
                        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                        //GL.DrawArrays(PrimitiveType.Lines, 0, _elementCount);
                        //drawCalls++;
                        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
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

            // Restore point size and/or line width
            if (drawVertices || _meshDataType == MeshDataType.Point)
            {
                GL.PointSize(1f);
            }
            if (drawWireframe || _meshDataType == MeshDataType.Line)
            {
                GL.LineWidth(1f);
            }

            // Unbind texture
            if (textureBinder != null && TextureID != 0)
            {
                textureBinder.Unbind();
            }

            // Unbind joint matrices
            if (Scene.JointsSupported && Skin != null)
            {
                Skin.Unbind();
            }

            // Unbind buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            return drawCalls;
        }

        public void SetData(MeshDataType meshDataType, int elementCount, float[] positionList, float[] normalList, float[] colorList, float[] uvList, float[] tiledAreaList, uint[] jointList)
        {
            _meshDataType = meshDataType;
            _elementCount = elementCount;

            BufferData(_positionBufferId,  positionList,  3); // Vector3
            BufferData(_normalBufferId,    normalList,    3); // Vector3
            BufferData(_colorBufferId,     colorList,     3); // Vector3 (Color)
            BufferData(_uvBufferId,        uvList,        2); // Vector2
            BufferData(_tiledAreaBufferId, tiledAreaList, 4); // Vector4
            if (Scene.JointsSupported)
            {
                BufferData(_jointBufferId,     jointList,     2); // uint[2]
            }
        }

        // Passing null for list will fill the data with zeros.
        private void BufferData<T>(uint bufferId, T[] list, int elementSize) where T : struct
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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