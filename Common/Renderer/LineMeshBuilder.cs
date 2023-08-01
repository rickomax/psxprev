using System;
using System.Collections.Generic;
using OpenTK;

namespace PSXPrev.Common.Renderer
{
    public class LineMeshBuilder : MeshRenderInfo
    {
        public List<Line> Lines { get; } = new List<Line>();

        public int Count => Lines.Count;


        public void AddLine(Line line)
        {
            Lines.Add(line);
        }

        public void AddLine(Vector3 vertex0, Vector3 vertex1, Color color0 = null, Color color1 = null)
        {
            Lines.Add(new Line(vertex0, vertex1, color0 ?? Color.White, color1));
        }

        public void AddLine(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Color color0, Color color1 = null)
        {
            if (matrix.HasValue)
            {
                vertex0 = Vector3.TransformPosition(vertex0, matrix.Value);
                vertex1 = Vector3.TransformPosition(vertex1, matrix.Value);
            }
            Lines.Add(new Line(vertex0, vertex1, color0, color1));
        }

        private void AddCorners(Vector3[] corners, Color color = null)
        {
            if (color == null)
            {
                color = Color.White;
            }
            AddLine(corners[0], corners[2], color);
            AddLine(corners[2], corners[4], color);
            AddLine(corners[4], corners[1], color);
            AddLine(corners[1], corners[0], color);
            AddLine(corners[6], corners[7], color);
            AddLine(corners[7], corners[5], color);
            AddLine(corners[5], corners[3], color);
            AddLine(corners[3], corners[6], color);
            AddLine(corners[4], corners[7], color);
            AddLine(corners[6], corners[2], color);
            AddLine(corners[1], corners[5], color);
            AddLine(corners[3], corners[0], color);
        }

        public void AddBounds(BoundingBox bounds, Color color = null)
        {
            if (bounds == null)
            {
                return;
            }
            AddCorners(bounds.Corners, color);
        }

        public void AddBounds(Matrix4? matrix, BoundingBox bounds, Color color = null)
        {
            if (bounds == null)
            {
                return;
            }
            var corners = bounds.Corners;
            if (matrix.HasValue)
            {
                var newCorners = new Vector3[corners.Length];
                for (var i = 0; i < corners.Length; i++)
                {
                    newCorners[i] = Vector3.TransformPosition(corners[i], matrix.Value);
                }
                corners = newCorners;
            }
            AddCorners(corners, color);
        }

        public void AddCube(Vector3 center, Vector3 size, Color color = null)
        {
            var bounds = new BoundingBox();
            bounds.AddPoint(center - size);
            bounds.AddPoint(center + size);
            AddBounds(bounds, color);
        }

        public void AddCube(Matrix4? matrix, Vector3 center, Vector3 size, Color color = null)
        {
            var bounds = new BoundingBox();
            if (matrix.HasValue)
            {
                bounds.AddPoint(Vector3.TransformPosition(center - size, matrix.Value));
                bounds.AddPoint(Vector3.TransformPosition(center + size, matrix.Value));
            }
            else
            {
                bounds.AddPoint(center - size);
                bounds.AddPoint(center + size);
            }
            AddBounds(bounds, color);
        }

        public void AddEntityBounds(EntityBase entity, Color color = null)
        {
            if (entity == null)
            {
                return;
            }
            AddBounds(entity.Bounds3D, color);
        }

        public void AddTriangleOutline(Triangle triangle, Color color = null)
        {
            if (triangle == null)
            {
                return;
            }
            if (color == null)
            {
                color = Color.White;
            }
            var vertex0 = triangle.Vertices[0];
            var vertex1 = triangle.Vertices[1];
            var vertex2 = triangle.Vertices[2];
            AddLine(vertex0, vertex1, color);
            AddLine(vertex1, vertex2, color);
            AddLine(vertex2, vertex0, color);
        }

        public void AddTriangleOutline(Matrix4? matrix, Triangle triangle, Color color = null)
        {
            if (triangle == null)
            {
                return;
            }
            if (color == null)
            {
                color = Color.White;
            }
            var vertex0 = triangle.Vertices[0];
            var vertex1 = triangle.Vertices[1];
            var vertex2 = triangle.Vertices[2];
            if (matrix.HasValue)
            {
                vertex0 = Vector3.TransformPosition(triangle.Vertices[0], matrix.Value);
                vertex1 = Vector3.TransformPosition(triangle.Vertices[1], matrix.Value);
                vertex2 = Vector3.TransformPosition(triangle.Vertices[2], matrix.Value);
            }
            AddLine(vertex0, vertex1, color);
            AddLine(vertex1, vertex2, color);
            AddLine(vertex2, vertex0, color);
        }
    }
}
