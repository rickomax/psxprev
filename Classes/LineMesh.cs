using System;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Classes
{
    public class LineMesh
    {
        private const int BufferCount = 4;

        private readonly uint _meshId;
        private int _numElements;

        private uint _positionBuffer;
        private uint _colorBuffer;
        private uint _normalBuffer;
        private uint _uvBuffer;

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
            _positionBuffer = _ids[0];
            _colorBuffer = _ids[1];
            _normalBuffer = _ids[2];
            _uvBuffer = _ids[3];
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
            GL.VertexAttribPointer((uint)Scene.AttributeIndexUv, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.LineWidth(width);
            GL.DrawArrays(PrimitiveType.Lines, 0, _numElements);
            GL.LineWidth(1f);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void SetData(int numElements, float[] positionList, float[] normalList, float[] colorList, float[] uvList)
        {
            _numElements = numElements;

            GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBuffer);
            BufferData(positionList);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBuffer);
            BufferData(normalList);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _colorBuffer);
            BufferData(colorList);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _uvBuffer);
            BufferData(uvList);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        //public void SetData(int numElements, float[] positionList)
        //{
        //    _numElements = numElements;
        //
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBuffer);
        //    BufferData(positionList);
        //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //}

        public void BufferData(float[] list)
        {
            var size = (IntPtr)(list.Length * sizeof(float));
            GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw);
        }
    }
}