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

    public class Mesh : MeshRenderInfo, IDisposable
    {
        private const int BufferCount = 5;

        private readonly uint _meshId;
        private readonly uint[] _ids = new uint[BufferCount];

        private uint _positionBuffer;
        private uint _colorBuffer;
        private uint _normalBuffer;
        private uint _uvBuffer;
        private uint _tiledAreaBuffer;

        private MeshDataType _meshDataType;
        private int _numElements; // Number of elements assigned during SetData

        private int VerticesPerElement
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
        public uint Texture { get; set; } // Texture assinged by TextureBinder

        public Mesh(uint meshId)
        {
            _meshId = meshId;
            GenBuffer();
        }

        public void Dispose()
        {
            GL.DeleteBuffers(BufferCount, _ids);
        }

        private void GenBuffer()
        {
            GL.GenBuffers(BufferCount, _ids);
            _positionBuffer  = _ids[0];
            _colorBuffer     = _ids[1];
            _normalBuffer    = _ids[2];
            _uvBuffer        = _ids[3];
            _tiledAreaBuffer = _ids[4];
        }


        public void Draw(TextureBinder textureBinder = null, bool wireframe = false, bool verticesOnly = false, float wireframeSize = 1f, float vertexSize = 1f)
        {
            // Bind buffers
            GL.BindVertexArray(_meshId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexPosition);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexPosition, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexNormal);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexNormal, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _colorBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexColor);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexColor, 3, VertexAttribPointerType.Float, true, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _uvBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexUv);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexUv, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _tiledAreaBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexTiledArea);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexTiledArea, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            // Bind texture
            if (textureBinder != null && Texture != 0)
            {
                textureBinder.BindTexture(Texture);
            }

            // Setup point size or line width
            if (verticesOnly || _meshDataType == MeshDataType.Point)
            {
                GL.PointSize(verticesOnly ? vertexSize : Thickness);
            }
            else if (wireframe || _meshDataType == MeshDataType.Line)
            {
                GL.LineWidth(wireframe ? wireframeSize : Thickness);
            }

            // Draw geometry
            switch (_meshDataType)
            {
                case MeshDataType.Triangle:
                    GL.PolygonMode(MaterialFace.FrontAndBack, wireframe ? PolygonMode.Line : PolygonMode.Fill);
                    GL.DrawArrays(verticesOnly ? PrimitiveType.Points : PrimitiveType.Triangles, 0, _numElements);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    break;

                case MeshDataType.Line:
                    GL.DrawArrays(verticesOnly ? PrimitiveType.Points : PrimitiveType.Lines, 0, _numElements);
                    break;

                case MeshDataType.Point:
                    GL.DrawArrays(PrimitiveType.Points, 0, _numElements);
                    break;
            }

            // Restore point size or line width
            if (verticesOnly || _meshDataType == MeshDataType.Point)
            {
                GL.PointSize(1f);
            }
            else if (wireframe || _meshDataType == MeshDataType.Line)
            {
                GL.LineWidth(1f);
            }

            // Unbind texture
            if (textureBinder != null && Texture != 0)
            {
                textureBinder.Unbind();
            }

            // Unbind buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void SetData(MeshDataType meshType, int numElements, float[] positionList, float[] normalList, float[] colorList, float[] uvList, float[] tiledAreaList = null)
        {
            _meshDataType = meshType;
            _numElements = numElements;

            BufferData(_positionBuffer,  positionList,  3);
            BufferData(_normalBuffer,    normalList,    3);
            BufferData(_colorBuffer,     colorList,     3);
            BufferData(_uvBuffer,        uvList,        2);
            BufferData(_tiledAreaBuffer, tiledAreaList, 4);
        }

        // Passing null for list will fill the data with zeros.
        private void BufferData(uint buffer, float[] list, int elementSize)
        {
            var length = _numElements * elementSize;
            if (list == null)
            {
                list = new float[length]; // Treat null as zeroed data.
            }
            else
            {
                //Debug.Assert(list.Length >= length, "BufferData cannot use list that's smaller than expected length");
            }
            var size = (IntPtr)(length * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}