using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace PSXPrev
{
    public class Mesh
    {
        private const int BufferCount = 4;

        public Matrix4 WorldMatrix { get; set; }
        public uint Texture { get; set; }

        private readonly uint _meshId;
        private int _numElements;

        private uint _positionBuffer;
        private uint _colorBuffer;
        private uint _normalBuffer;
        private uint _uvBuffer;

        private readonly uint[] _ids;

        public Mesh(uint meshId)
        {
            _meshId = meshId;
            _ids = new uint[BufferCount];
            WorldMatrix = Matrix4.Identity;
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

        public void Draw(TextureBinder textureBinder = null, bool wireframe = false)
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

            if (textureBinder != null && Texture != 0)
            {
                textureBinder.BindTexture(Texture);
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, wireframe ? PolygonMode.Line : PolygonMode.Fill);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _numElements);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            if (textureBinder != null && Texture != 0)
            {
                textureBinder.Unbind();
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void SetData(int numElements, float[] positionList, float[] normalList, float[] colorList, float[] uvList)//, int[] indexList)
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

        private void BufferData(float[] list)
        {
            var size = (IntPtr)(list.Length * sizeof(float));
            GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw);
        }

        //private void BufferData(int[] list)
        //{
        //    var size = (IntPtr)(list.Length * sizeof(int));
        //    GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw);
        //}
    }
}