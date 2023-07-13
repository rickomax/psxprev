using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Classes
{
    public class LineBatch
    {
        private uint[] _ids;
        private LineMesh[] _linesMesh;
        private readonly List<Line> _lines;

        public bool IsValid { get; private set; }

        public LineBatch()
        {
            _lines = new List<Line>(100);
            ResetMeshes(1);
        }

        public void AddLine(Vector3 p1, Vector3 p2, Color color)
        {
            _lines.Add(new Line(new Vector4(p1), new Vector4(p2), color));
        }

        public void Reset()
        {
            _lines.Clear();
        }

        public void SetupAndDraw(Matrix4 viewMatrix, Matrix4 projectionMatrix, float width = 1f)
        {
            var numLines = _lines.Count;
            if (numLines == 0)
            {
                return;
            }
            var numElements = numLines * 2;
            var baseIndex = 0;
            var positionList = new float[numElements * 3]; // Vector3
            var colorList    = new float[numElements * 3]; // Vector3 (Color)
            for (var l = 0; l < numLines; l++)
            {
                var line = _lines[l];
                FillVertex(line.P1, line.Color, ref baseIndex, ref positionList, ref colorList);
                FillVertex(line.P2, line.Color, ref baseIndex, ref positionList, ref colorList);
            }
            var lineMesh = GetLine(0);
            lineMesh.SetData(numElements, positionList, null, colorList, null);
            Draw(viewMatrix, projectionMatrix, width);
        }

        private static void FillVertex(Vector4 position, Color color, ref int baseIndex, ref float[] positionList, ref float[] colorList)
        {
            var index3d = baseIndex * 3;
            baseIndex++;

            positionList[index3d + 0] = position.X;
            positionList[index3d + 1] = position.Y;
            positionList[index3d + 2] = position.Z;

            // Normals are all 0f (passing null will default to a zeroed list).

            colorList[index3d + 0] = color.R;
            colorList[index3d + 1] = color.G;
            colorList[index3d + 2] = color.B;

            // UVs are all 0f (passing null will default to a zeroed list).
        }

        private void ResetMeshes(int nLines)
        {
            IsValid = true;
            if (_linesMesh != null)
            {
                foreach (var mesh in _linesMesh)
                {
                    mesh?.Delete();
                }
            }
            _linesMesh = new LineMesh[nLines];
            _ids = new uint[nLines];
            GL.GenVertexArrays(nLines, _ids);
        }

        private LineMesh GetLine(int index)
        {
            if (_linesMesh[index] == null)
            {
                _linesMesh[index] = new LineMesh(_ids[index]);
            }
            return _linesMesh[index];
        }

        private void Draw(Matrix4 viewMatrix, Matrix4 projectionMatrix, float width = 1f)
        {
            if (!IsValid)
            {
                return;
            }
            var line = _linesMesh[0];
            if (line == null)
            {
                return;
            }
            var modelMatrix = Matrix4.Identity;
            var mvpMatrix = modelMatrix * viewMatrix * projectionMatrix;
            GL.UniformMatrix4(Scene.UniformIndexMVP, false, ref mvpMatrix);
            line.Draw(width);
        }

        public void SetupEntityBounds(EntityBase entity)
        {
            if (entity == null)
            {
                return;
            }
            var corners = entity.Bounds3D.Corners;
            AddLine(corners[0], corners[2], Color.White);
            AddLine(corners[2], corners[4], Color.White);
            AddLine(corners[4], corners[1], Color.White);
            AddLine(corners[1], corners[0], Color.White);
            AddLine(corners[6], corners[7], Color.White);
            AddLine(corners[7], corners[5], Color.White);
            AddLine(corners[5], corners[3], Color.White);
            AddLine(corners[3], corners[6], Color.White);
            AddLine(corners[4], corners[7], Color.White);
            AddLine(corners[6], corners[2], Color.White);
            AddLine(corners[1], corners[5], Color.White);
            AddLine(corners[3], corners[0], Color.White);
        }

        public void SetupTriangleOutline(Triangle triangle, Matrix4 worldMatrix)
        {
            var vertex0 = Vector3.TransformPosition(triangle.Vertices[0], worldMatrix);
            var vertex1 = Vector3.TransformPosition(triangle.Vertices[1], worldMatrix);
            var vertex2 = Vector3.TransformPosition(triangle.Vertices[2], worldMatrix);
            AddLine(vertex0, vertex1, Color.White);
            AddLine(vertex1, vertex2, Color.White);
            AddLine(vertex2, vertex0, Color.White);
        }
    }
}
