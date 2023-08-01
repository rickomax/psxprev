using System;
using System.Collections.Generic;
using OpenTK;

namespace PSXPrev.Common.Renderer
{
    public class TriangleMeshBuilder : MeshRenderInfo
    {
        private readonly Vector3[] CubeNormals =
        {
            Vector3.UnitY,
            -Vector3.UnitY,
            -Vector3.UnitZ,
            Vector3.UnitZ,
            -Vector3.UnitX,
            Vector3.UnitX,
        };


        public bool CalculateNormals { get; set; }

        public List<Triangle> Triangles { get; } = new List<Triangle>();

        public int Count => Triangles.Count;


        public void AddTriangle(Triangle triangle)
        {
            Triangles.Add(triangle);
        }

        public void AddTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Color color)
        {
            if (!CalculateNormals)
            {
                Triangles.Add(new Triangle
                {
                    Vertices = new[] { vertex0, vertex1, vertex2 },
                    Colors = new[] { color, color, color },
                    Normals = Triangle.EmptyNormals,
                    Uv = Triangle.EmptyUv,
                });
            }
            else
            {
                var normal = GeomMath.CalculateNormal(vertex0, vertex1, vertex2);
                AddTriangle(vertex0, vertex1, vertex2, normal, color);
            }
        }

        public void AddTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 normal, Color color)
        {
            Triangles.Add(new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Colors = new[] { color, color, color },
                Normals = new[] { normal, normal, normal },
                Uv = Triangle.EmptyUv,
            });
        }

        public void AddTriangle(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Color color)
        {
            if (matrix.HasValue)
            {
                vertex0 = Vector3.TransformPosition(vertex0, matrix.Value);
                vertex1 = Vector3.TransformPosition(vertex1, matrix.Value);
                vertex2 = Vector3.TransformPosition(vertex2, matrix.Value);
            }
            AddTriangle(vertex0, vertex1, vertex2, color);
        }

        public void AddTriangle(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 normal, Color color)
        {
            if (matrix.HasValue)
            {
                vertex0 = Vector3.TransformPosition(vertex0, matrix.Value);
                vertex1 = Vector3.TransformPosition(vertex1, matrix.Value);
                vertex2 = Vector3.TransformPosition(vertex2, matrix.Value);
                normal = Vector3.TransformNormal(normal, matrix.Value);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal, color);
        }

        public void AddQuad(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Color color)
        {
            AddTriangle(vertex0, vertex1, vertex2, color);
            AddTriangle(vertex1, vertex3, vertex2, color);
        }

        public void AddQuad(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 normal, Color color)
        {
            AddTriangle(vertex0, vertex1, vertex2, normal, color);
            AddTriangle(vertex1, vertex3, vertex2, normal, color);
        }

        public void AddQuad(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Color color)
        {
            if (matrix.HasValue)
            {
                vertex0 = Vector3.TransformPosition(vertex0, matrix.Value);
                vertex1 = Vector3.TransformPosition(vertex1, matrix.Value);
                vertex2 = Vector3.TransformPosition(vertex2, matrix.Value);
                vertex3 = Vector3.TransformPosition(vertex3, matrix.Value);
            }
            AddTriangle(vertex0, vertex1, vertex2, color);
            AddTriangle(vertex1, vertex3, vertex2, color);
        }

        public void AddQuad(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 normal, Color color)
        {
            if (matrix.HasValue)
            {
                vertex0 = Vector3.TransformPosition(vertex0, matrix.Value);
                vertex1 = Vector3.TransformPosition(vertex1, matrix.Value);
                vertex2 = Vector3.TransformPosition(vertex2, matrix.Value);
                vertex3 = Vector3.TransformPosition(vertex3, matrix.Value);
                normal = Vector3.TransformNormal(normal, matrix.Value);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal, color);
            AddTriangle(vertex1, vertex3, vertex2, normal, color);
        }

        private void AddCorners(Vector3[] corners, Vector3[] normals, Color color = null)
        {
            if (color == null)
            {
                color = Color.White;
            }
            AddTriangle(corners[0], corners[3], corners[5], normals[0], color);
            AddTriangle(corners[0], corners[5], corners[1], normals[0], color);
            AddTriangle(corners[2], corners[4], corners[7], normals[1], color);
            AddTriangle(corners[2], corners[7], corners[6], normals[1], color);
            AddTriangle(corners[5], corners[3], corners[6], normals[2], color);
            AddTriangle(corners[5], corners[6], corners[7], normals[2], color);
            AddTriangle(corners[0], corners[1], corners[4], normals[3], color);
            AddTriangle(corners[0], corners[4], corners[2], normals[3], color);
            AddTriangle(corners[1], corners[5], corners[7], normals[4], color);
            AddTriangle(corners[1], corners[7], corners[4], normals[4], color);
            AddTriangle(corners[3], corners[0], corners[2], normals[5], color);
            AddTriangle(corners[3], corners[2], corners[6], normals[5], color);
        }

        public void AddBounds(BoundingBox bounds, Color color = null)
        {
            if (bounds == null)
            {
                return;
            }
            AddCorners(bounds.Corners, CubeNormals, color);
        }

        public void AddBounds(Matrix4? matrix, BoundingBox bounds, Color color = null)
        {
            if (bounds == null)
            {
                return;
            }
            var corners = bounds.Corners;
            var normals = CubeNormals;
            if (matrix.HasValue)
            {
                var newCorners = new Vector3[corners.Length];
                for (var i = 0; i < corners.Length; i++)
                {
                    newCorners[i] = Vector3.TransformPosition(corners[i], matrix.Value);
                }
                corners = newCorners;
                var newNormals = new Vector3[normals.Length];
                for (var i = 0; i < normals.Length; i++)
                {
                    newNormals[i] = Vector3.TransformNormal(normals[i], matrix.Value);
                }
                normals = newNormals;
            }
            AddCorners(corners, normals, color);
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
            bounds.AddPoint(center - size);
            bounds.AddPoint(center + size);
            AddBounds(matrix, bounds, color);
        }
    }
}
