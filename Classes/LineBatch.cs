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
            Reset(1);
        }

        public void AddLine(Line line)
        {
            _lines.Add(line);
        }

        public void Reset()
        {
            _lines.Clear();
        }

        public void SetupAndDraw(Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            var numLines = _lines.Count;
            if (numLines == 0)
            {
                return;
            }
            var numPoints = numLines * 2;
            var elementCount = numPoints * 3;
            var baseIndex = 0;
            var positionList = new float[elementCount];
            var normalList = new float[elementCount];
            var colorList = new float[elementCount];
            var uvList = new float[elementCount];
            for (var l = 0; l < numLines; l++)
            {
                var line = _lines[l];
                FillVertex(line.p1, ref baseIndex, ref positionList, ref normalList, ref colorList, ref uvList);
                FillVertex(line.p2, ref baseIndex, ref positionList, ref normalList, ref colorList, ref uvList);
            }
            var lineMesh = GetLine(0);
            lineMesh.SetData(numPoints, positionList, normalList, colorList, uvList);
            Draw(viewMatrix, projectionMatrix);
        }

        private static void FillVertex(Vector4 position, ref int baseIndex, ref float[] positionList, ref float[] normalList, ref float[] colorList, ref float[] uvList)
        {
            var index1 = baseIndex++;
            var index2 = baseIndex++;
            var index3 = baseIndex++;

            positionList[index1] = position.X;
            positionList[index2] = position.Y;
            positionList[index3] = position.Z;

            normalList[index1] = 0f;
            normalList[index2] = 0f;
            normalList[index3] = 0f;

            colorList[index1] = 1f;
            colorList[index2] = 1f;
            colorList[index3] = 1f;

            uvList[index1] = 0f;
            uvList[index2] = 0f;
            uvList[index3] = 0f;
        }

        private void Reset(int nLines)
        {
            IsValid = true;
            if (_linesMesh != null)
            {
                foreach (var mesh in _linesMesh)
                {
                    if (mesh == null)
                    {
                        continue;
                    }
                    mesh.Delete();
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

        private void Draw(Matrix4 viewMatrix, Matrix4 projectionMatrix)
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
            line.Draw();
        }
    }
}
