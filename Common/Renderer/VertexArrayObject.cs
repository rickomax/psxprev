using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    public class VertexArrayObject : IDisposable, IComparable<VertexArrayObject>
    {
        private readonly int _vertexArrayObjectId; // Vertex array object
        private readonly int[] _bufferIds = new int[6];
        private readonly int _positionBufferId;
        private readonly int _normalBufferId;
        private readonly int _colorBufferId;
        private readonly int _uvBufferId;
        private readonly int _tiledAreaBufferId;
        private readonly int _jointBufferId;

        private int _elementCount; // Number of elements assigned during SetData

        public int VertexCount => _elementCount;
        public int VertexArrayObjectID => _vertexArrayObjectId;

        public int OrderIndex { get; set; }
        public bool IsInitialized { get; private set; }
        public bool IsDisposed { get; private set; }

        public VertexArrayObject(int orderIndex)
        {
            OrderIndex = orderIndex;
            _vertexArrayObjectId = GL.GenVertexArray();
            GL.GenBuffers(_bufferIds.Length, _bufferIds);
            _positionBufferId  = _bufferIds[0];
            _normalBufferId    = _bufferIds[1];
            _colorBufferId     = _bufferIds[2];
            _uvBufferId        = _bufferIds[3];
            _tiledAreaBufferId = _bufferIds[4];
            _jointBufferId     = _bufferIds[5];
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                GL.DeleteVertexArray(_vertexArrayObjectId);
                GL.DeleteBuffers(_bufferIds.Length, _bufferIds);
            }
        }

        public int CompareTo(VertexArrayObject other)
        {
            return OrderIndex.CompareTo(other.OrderIndex);
        }

        public void Bind()
        {
            GL.BindVertexArray(_vertexArrayObjectId);
        }

        public void Unbind()
        {
            GL.BindVertexArray(0);
        }

        // Initializes buffers, filling them with garbage data. Intended for use with SetSubData.
        public void InitData(int elementCount)
        {
            SetData(elementCount, null, null, null, null, null, null);
        }

        // Initializes buffers and assigns data to them. Null lists will assign garbage data.
        public void SetData(int elementCount, float[] positionList, float[] normalList, float[] colorList, float[] uvList, float[] tiledAreaList, uint[] jointList)
        {
            IsInitialized &= (_elementCount != elementCount); // We need to recreate buffers if the size differs
            _elementCount = elementCount;

            BufferAllData(0, elementCount, positionList, normalList, colorList, uvList, tiledAreaList, jointList);

            if (!IsInitialized)
            {
                BindVertexBuffers();
                IsInitialized = true;
            }
        }

        // Assigns partial data to initialized buffers. Null lists will not assign any data.
        // The first index in each list should be the first element assigned.
        public void SetSubData(int elementFirst, int elementCount, float[] positionList, float[] normalList, float[] colorList, float[] uvList, float[] tiledAreaList, uint[] jointList)
        {
            Trace.Assert(IsInitialized, "SetSubData cannot be called before data is initialized");
            Trace.Assert(elementFirst + elementCount <= _elementCount, "SetSubData element out of bounds");

            BufferAllData(elementFirst, elementCount, positionList, normalList, colorList, uvList, tiledAreaList, jointList);
        }

        private void BufferAllData(int elementFirst, int elementCount, float[] positionList, float[] normalList, float[] colorList, float[] uvList, float[] tiledAreaList, uint[] jointList)
        {
            BufferData(_positionBufferId,  positionList,  elementFirst, elementCount, 3); // Vector3
            BufferData(_normalBufferId,    normalList,    elementFirst, elementCount, 3); // Vector3
            BufferData(_colorBufferId,     colorList,     elementFirst, elementCount, 3); // Vector3 (Color3)
            BufferData(_uvBufferId,        uvList,        elementFirst, elementCount, 2); // Vector2
            BufferData(_tiledAreaBufferId, tiledAreaList, elementFirst, elementCount, 4); // Vector4
            BufferData(_jointBufferId,     jointList,     elementFirst, elementCount, 2); // uint[2]
        }

        // Passing null for list will fill the buffer with garbage data.
        private void BufferData<T>(int bufferId, T[] list, int elementFirst, int elementCount, int elementSize/*, int primitiveSize = 4*/) where T : struct
        {
            const int primitiveSize = 4;
            var length = elementCount * elementSize;
            if (list != null)
            {
                Trace.Assert(list.Length >= length, "BufferData cannot use list that's smaller than expected length");
            }
            var offset = new IntPtr(elementFirst * elementSize * primitiveSize);
            var size = new IntPtr(length * primitiveSize);

            if (!IsInitialized)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
                GL.BufferData(BufferTarget.ArrayBuffer, size, list, BufferUsageHint.StaticDraw); //.DynamicDraw);
            }
            else if (list != null) // No need to buffer sub data if its garbage data
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
                GL.BufferSubData(BufferTarget.ArrayBuffer, offset, size, list);
            }
        }

        private void BindVertexBuffers()
        {
            // Bind buffers
            GL.BindVertexArray(_vertexArrayObjectId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _positionBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_Position);
            GL.VertexAttribPointer(Shader.AttributeIndex_Position, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_Normal);
            GL.VertexAttribPointer(Shader.AttributeIndex_Normal, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _colorBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_Color);
            GL.VertexAttribPointer(Shader.AttributeIndex_Color, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _uvBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_Uv);
            GL.VertexAttribPointer(Shader.AttributeIndex_Uv, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _tiledAreaBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_TiledArea);
            GL.VertexAttribPointer(Shader.AttributeIndex_TiledArea, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _jointBufferId);
            GL.EnableVertexAttribArray(Shader.AttributeIndex_Joint);
            GL.VertexAttribIPointer(Shader.AttributeIndex_Joint, 2, VertexAttribIntegerType.UnsignedInt, 0, IntPtr.Zero);
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignVector2(float[] data, int elementIndex, ref Vector2 vector)
        {
            var index = elementIndex * 2;
            data[index++] = vector.X;
            data[index++] = vector.Y;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignVector3(float[] data, int elementIndex, ref Vector3 vector)
        {
            var index = elementIndex * 3;
            data[index++] = vector.X;
            data[index++] = vector.Y;
            data[index++] = vector.Z;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignVector4(float[] data, int elementIndex, ref Vector4 vector)
        {
            var index = elementIndex * 4;
            data[index++] = vector.X;
            data[index++] = vector.Y;
            data[index++] = vector.Z;
            data[index++] = vector.W;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignColor3(float[] data, int elementIndex, ref Color3 color)
        {
            var index = elementIndex * 3;
            data[index++] = color.R;
            data[index++] = color.G;
            data[index++] = color.B;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void AssignJoint(uint[] data, int elementIndex, uint? vertexJoint, uint? normalJoint)
        {
            var index = elementIndex * 2;
            data[index++] = (vertexJoint ?? Triangle.NoJoint) + 1u;
            data[index++] = (normalJoint ?? Triangle.NoJoint) + 1u;
        }
    }
}