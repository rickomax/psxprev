using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    // Represents a list of joint matrices that allow vertices to attach to other models
    public class Skin : IDisposable, IComparable<Skin>
    {
        private readonly int _skinIndex;
        private readonly int _jointMatrixBufferId;
        private int _elementCount; // Number of elements assigned during SetData

        public int JointCount => _elementCount;

        public bool IsDisposed { get; private set; }

        public Skin(int skinIndex)
        {
            _skinIndex = skinIndex;
            _jointMatrixBufferId = GL.GenBuffer();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                GL.DeleteBuffer(_jointMatrixBufferId);
            }
        }

        public void SetData(int elementCount, float[] jointMatrixList)
        {
            _elementCount = elementCount;

            BufferData(_jointMatrixBufferId, jointMatrixList, 32); // Matrix4[2]
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
            var size = (IntPtr)(length * 4);// sizeof(float));

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bufferId);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, size, list, BufferUsageHint.DynamicDraw); //.StaticDraw);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _jointMatrixBufferId);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, Shader.BufferIndex_Joints, _jointMatrixBufferId);
        }

        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            // todo: Is this necessary?
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, Shader.BufferIndex_Joints, 0);
        }

        public int CompareTo(Skin other)
        {
            return _skinIndex.CompareTo(other._skinIndex);
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignModelMatrix(float[] data, int baseIndex, ref Matrix4 matrix)
        {
            var index = baseIndex * 32 + 0;
            data[index++] = matrix.Row0.X;
            data[index++] = matrix.Row0.Y;
            data[index++] = matrix.Row0.Z;
            data[index++] = matrix.Row0.W;

            data[index++] = matrix.Row1.X;
            data[index++] = matrix.Row1.Y;
            data[index++] = matrix.Row1.Z;
            data[index++] = matrix.Row1.W;

            data[index++] = matrix.Row2.X;
            data[index++] = matrix.Row2.Y;
            data[index++] = matrix.Row2.Z;
            data[index++] = matrix.Row2.W;

            data[index++] = matrix.Row3.X;
            data[index++] = matrix.Row3.Y;
            data[index++] = matrix.Row3.Z;
            data[index++] = matrix.Row3.W;
        }

        // This function handles transposing the normal matrix.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignNormalMatrix(float[] data, int baseIndex, ref Matrix3 matrix)
        {
            var index = baseIndex * 32 + 16;
            /*data[index++] = matrix.Row0.X;
            data[index++] = matrix.Row0.Y;
            data[index++] = matrix.Row0.Z;
            index++;

            data[index++] = matrix.Row1.X;
            data[index++] = matrix.Row1.Y;
            data[index++] = matrix.Row1.Z;
            index++;

            data[index++] = matrix.Row2.X;
            data[index++] = matrix.Row2.Y;
            data[index++] = matrix.Row2.Z;*/

            // Transpose here, since it's cheaper
            data[index++] = matrix.Row0.X;
            data[index++] = matrix.Row1.X;
            data[index++] = matrix.Row2.X;
            index++;

            data[index++] = matrix.Row0.Y;
            data[index++] = matrix.Row1.Y;
            data[index++] = matrix.Row2.Y;
            index++;

            data[index++] = matrix.Row0.Z;
            data[index++] = matrix.Row1.Z;
            data[index++] = matrix.Row2.Z;
        }
    }
}
