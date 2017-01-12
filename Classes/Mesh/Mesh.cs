using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using PSXPrev.Classes.Texture;

namespace PSXPrev.Classes.Mesh
{
    public class Mesh
    {
        public Matrix4 WorldMatrix { get; set; }
        public bool Visible { get; set; }
        public uint Texture { get; set; }

        private readonly uint _meshId;
        private int _numElements;

        private uint _positionBuffer;
        private uint _colorBuffer;
        private uint _normalBuffer;
        private uint _uvBuffer;
        private uint _indexBuffer;
        private readonly uint[] _ids;
        
        public Mesh(uint meshId)
        {
            _meshId = meshId;
            _ids = new uint[5];
            WorldMatrix = Matrix4.Identity;
            Visible = true;
            GenBuffer();
        }

        public void Delete()
        {
            GL.DeleteBuffers(5, _ids);
        }

        private void GenBuffer()
        {
            GL.GenBuffers(5, _ids);
            _positionBuffer = _ids[0];
            _colorBuffer = _ids[1];
            _normalBuffer = _ids[2];
            _uvBuffer = _ids[3];
            _indexBuffer = _ids[4];
        }

        public void Draw(TextureBinder textureBinder)
        {
            GL.BindVertexArray(_meshId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBuffer);
            GL.EnableVertexAttribArray((uint)Scene.Scene.AttributeIndexPosition);
            GL.VertexAttribPointer((uint)Scene.Scene.AttributeIndexPosition, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBuffer);
            GL.EnableVertexAttribArray((uint)Scene.Scene.AttributeIndexNormal);
            GL.VertexAttribPointer((uint)Scene.Scene.AttributeIndexNormal, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _colorBuffer);
            GL.EnableVertexAttribArray((uint)Scene.Scene.AttributeIndexColour);
            GL.VertexAttribPointer((uint)Scene.Scene.AttributeIndexColour, 3, VertexAttribPointerType.Float, true, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _uvBuffer);
            GL.EnableVertexAttribArray((uint)Scene.Scene.AttributeIndexUv);
            GL.VertexAttribPointer((uint)Scene.Scene.AttributeIndexUv, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _indexBuffer);
            GL.EnableVertexAttribArray((uint)Scene.Scene.AttributeIndexIndex);
            GL.VertexAttribIPointer((uint)Scene.Scene.AttributeIndexIndex, 3, VertexAttribIntegerType.Int, 0, IntPtr.Zero);

            if (Texture != 0)
            {
                textureBinder.BindTexture(Texture);
            }

            GL.DrawArrays(PrimitiveType.Triangles, 0, _numElements);

            if (Texture != 0)
            {
                textureBinder.Unbind();
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void SetData(int numElements, float[] positionList, float[] normalList, float[] colorList, float[] uvList, int[] indexList)
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

            GL.BindBuffer(BufferTarget.ArrayBuffer, _indexBuffer);
            BufferData(indexList);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void BufferData(float[] list)
        {
            var size = (IntPtr)(list.Length * sizeof(float));
            GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw);
        }

        public void BufferData(int[] list)
        {
            var size = (IntPtr)(list.Length * sizeof(int));
            GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw);
        }
    }
}