using System;
using OpenTK.Graphics.OpenGL4;

namespace PSXPrev.Classes.Line
{
    public class LineMesh
    {
        private readonly uint _meshId;
        private int _numElements;

        private uint _positionBuffer;
        private readonly uint[] _ids;

        public LineMesh(uint meshId)
        {
            _meshId = meshId;
            _ids = new uint[1];
            GenBuffer();
        }

        public void Delete()
        {
            GL.DeleteBuffers(1, _ids);
        }

        private void GenBuffer()
        {
            GL.GenBuffers(1, _ids);
            _positionBuffer = _ids[0];
        }

        public void Draw()
        {
            GL.BindVertexArray(_meshId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBuffer);
            GL.EnableVertexAttribArray((uint)Scene.Scene.AttributeIndexPosition);
            GL.VertexAttribPointer((uint)Scene.Scene.AttributeIndexPosition, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
            
            GL.DrawArrays(PrimitiveType.Lines, 0, _numElements);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void SetData(int numElements, float[] positionList)
        {
            _numElements = numElements;

            GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBuffer);
            BufferData(positionList);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void BufferData(float[] list)
        {
            var size = (IntPtr)(list.Length * sizeof(float));
            GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw);
        }
    }
}