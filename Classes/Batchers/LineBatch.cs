using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using PSXPrev.Classes.Line;

namespace PSXPrev.Classes.Batchers
{
    public class LineBatch
    {
        private uint[] _ids;
        private LineMesh[] _linesMesh;
        private readonly List<Line.Line> _lines;

        public bool IsValid { get; private set; }

        public LineBatch()
        {
            _lines = new List<Line.Line>(100);
            Reset(1);
        }

        public void AddLine(Line.Line line)
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
            var numElements = numPoints * 3;
            var lineArray = new float[numElements];
            var lastIndex = 0;
            for (int l = 0; l < numLines; l++)
            {
                var line = _lines[l];
                lineArray[lastIndex++] = line.p1.X;
                lineArray[lastIndex++] = line.p1.Y;
                lineArray[lastIndex++] = line.p1.Z;
                lineArray[lastIndex++] = line.p2.X;
                lineArray[lastIndex++] = line.p2.Y;
                lineArray[lastIndex++] = line.p2.Z;
            }
            var lineMesh = GetLine(0);
            lineMesh.SetData(numPoints, lineArray);
            Draw(viewMatrix, projectionMatrix);
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
            GL.UniformMatrix4(Scene.Scene.UniformIndexMvp, false, ref mvpMatrix);
            line.Draw();
        }
    }
}
