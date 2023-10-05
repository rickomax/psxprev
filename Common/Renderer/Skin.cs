using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    // Represents a list of joint matrices that allow vertices to attach to other models
    public class Skin : IDisposable, IComparable<Skin>
    {
        private readonly int _jointMatrixBufferId;
        private int _elementCount; // Number of elements assigned during SetData

        private BufferTarget SupportedBufferTarget => Shader.SSBOSupported ? BufferTarget.ShaderStorageBuffer : BufferTarget.UniformBuffer;
        private BufferRangeTarget SupportedBufferRangeTarget => Shader.SSBOSupported ? BufferRangeTarget.ShaderStorageBuffer : BufferRangeTarget.UniformBuffer;

        public int JointCount => _elementCount;

        public int OrderIndex { get; set; }
        public bool IsInitialized { get; private set; }
        public bool IsDisposed { get; private set; }

        public Skin(int orderIndex)
        {
            OrderIndex = orderIndex;
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

        public int CompareTo(Skin other)
        {
            return OrderIndex.CompareTo(other.OrderIndex);
        }

        public void Bind()
        {
            GL.BindBufferBase(SupportedBufferRangeTarget, Shader.BufferIndex_JointTransforms, _jointMatrixBufferId);
        }

        public void Unbind()
        {
            GL.BindBufferBase(SupportedBufferRangeTarget, Shader.BufferIndex_JointTransforms, 0);
        }

        // Initialize buffers, but only fill them with garbage data, intended for using SetSubData after this call.
        public void InitData(int elementCount)
        {
            SetData(elementCount, null);
        }

        public void SetData(int elementCount, float[] jointMatrixList)
        {
            IsInitialized &= (_elementCount != elementCount); // We need to recreate buffers if the size differs
            _elementCount = elementCount;

            BufferAllData(0, elementCount, jointMatrixList);

            IsInitialized = true;
        }

        // Assign partial data to the buffers, the first index in each list should be the first element assigned.
        public void SetSubData(int elementFirst, int elementCount, float[] jointMatrixList)
        {
            Trace.Assert(IsInitialized, nameof(SetSubData) + " cannot be called before data is initialized");
            Trace.Assert(elementFirst + elementCount <= _elementCount, nameof(SetSubData) + " element out of bounds");

            BufferAllData(elementFirst, elementCount, jointMatrixList);
        }

        private void BufferAllData(int elementFirst, int elementCount, float[] jointMatrixList)
        {
            BufferData(_jointMatrixBufferId, jointMatrixList, elementFirst, elementCount, 32); // Matrix4[2]
        }

        // Passing null for list will fill the data with zeros.
        private void BufferData<T>(int bufferId, T[] list, int elementFirst, int elementCount, int elementSize/*, int primitiveSize = 4*/) where T : struct
        {
            const int primitiveSize = 4;
            var length = elementCount * elementSize;
            if (list != null)
            {
                Trace.Assert(list.Length >= length, nameof(BufferData) + " cannot use list that's smaller than expected length");
            }
            var offset = new IntPtr(elementFirst * elementSize * primitiveSize);
            var size = new IntPtr(length * primitiveSize);

            if (!IsInitialized)
            {
                GL.BindBuffer(SupportedBufferTarget, bufferId);
                GL.BufferData(SupportedBufferTarget, size, list, BufferUsageHint.DynamicDraw); //.StaticDraw);
            }
            else if (list != null) // No need to buffer sub data if its garbage data
            {
                GL.BindBuffer(SupportedBufferTarget, bufferId);
                GL.BufferSubData(SupportedBufferTarget, offset, size, list);
            }
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignModelMatrix(float[] data, int elementIndex, ref Matrix4 matrix)
        {
            var index = elementIndex * 32 + 0;
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
        public static void AssignNormalMatrix(float[] data, int elementIndex, ref Matrix3 matrix)
        {
            var index = elementIndex * 32 + 16;
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
