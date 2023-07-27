﻿using System;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    public class LineMesh
    {
        private const int BufferCount = 5;

        private readonly uint _meshId;
        private int _numElements;

        private uint _positionBuffer;
        private uint _colorBuffer;
        private uint _normalBuffer;
        private uint _uvBuffer;
        private uint _tiledAreaBuffer;

        private readonly uint[] _ids;

        public LineMesh(uint meshId)
        {
            _meshId = meshId;
            _ids = new uint[BufferCount];
            GenBuffer();
        }

        public void Delete()
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

        public void Draw(float width = 1f)
        {
            GL.BindVertexArray(_meshId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexPosition);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexPosition, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexNormal);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexNormal, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _colorBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexColour);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexColour, 3, VertexAttribPointerType.Float, true, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _uvBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexUv);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexUv, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _tiledAreaBuffer);
            GL.EnableVertexAttribArray((uint)Scene.AttributeIndexTiledArea);
            GL.VertexAttribPointer((uint)Scene.AttributeIndexTiledArea, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.LineWidth(width);
            GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Lines, 0, _numElements);
            GL.LineWidth(1f);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void SetData(int numElements, float[] positionList, float[] normalList, float[] colorList, float[] uvList, float[] tiledAreaList = null)
        {
            _numElements = numElements;

            BufferData(_positionBuffer,  positionList,  3);
            BufferData(_normalBuffer,    normalList,    3);
            BufferData(_colorBuffer,     colorList,     3);
            BufferData(_uvBuffer,        uvList,        2);
            BufferData(_tiledAreaBuffer, tiledAreaList, 4);
        }

        //public void SetData(int numElements, float[] positionList)
        //{
        //    _numElements = numElements;
        //
        //    BufferData(_positionBuffer,  positionList,  3);
        //}

        // Passing null for list will fill the data with zeros.
        private void BufferData(uint buffer, float[] list, int elementSize)
        {
            if (list == null)
            {
                list = new float[_numElements * elementSize]; // Treat null as zeroed data.
            }
            var size = (IntPtr)(list.Length * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}