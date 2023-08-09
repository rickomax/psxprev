using System;
using System.Collections.Generic;
using OpenTK;

namespace PSXPrev.Common.Renderer
{
    public class LineMeshBuilder : MeshRenderInfo
    {
        public List<Line> Lines { get; }

        public int Count => Lines.Count;

        public int Capacity
        {
            get => Lines.Capacity;
            set => Lines.Capacity = value;
        }


        public LineMeshBuilder(MeshRenderInfo fromRenderInfo = null)
        {
            Lines = new List<Line>();
            if (fromRenderInfo != null)
            {
                CopyFrom(fromRenderInfo);
            }
        }

        // Does NOT copy Lines list, use IEnumerable overload for that.
        public LineMeshBuilder(LineMeshBuilder fromLineBuilder)
        {
            Lines = new List<Line>();
            if (fromLineBuilder != null)
            {
                CopyFrom(fromLineBuilder);
            }
        }

        public LineMeshBuilder(int capacity, MeshRenderInfo fromRenderInfo = null)
        {
            Lines = new List<Line>(capacity);
            if (fromRenderInfo != null)
            {
                CopyFrom(fromRenderInfo);
            }
        }

        public LineMeshBuilder(int capacity, LineMeshBuilder fromLineBuilder)
        {
            Lines = new List<Line>(capacity);
            if (fromLineBuilder != null)
            {
                CopyFrom(fromLineBuilder);
            }
        }

        public LineMeshBuilder(IEnumerable<Line> lines, MeshRenderInfo fromRenderInfo = null)
        {
            Lines = new List<Line>(lines);
            if (fromRenderInfo != null)
            {
                CopyFrom(fromRenderInfo);
            }
        }

        public LineMeshBuilder(IEnumerable<Line> lines, LineMeshBuilder fromLineBuilder)
        {
            Lines = new List<Line>(lines);
            if (fromLineBuilder != null)
            {
                CopyFrom(fromLineBuilder);
            }
        }

        // Does NOT copy Lines list.
        public void CopyFrom(LineMeshBuilder lineBuilder)
        {
            base.CopyFrom(lineBuilder);
            // No extra settings to copy over yet
        }


        // Debug functions for testing built models in the PSXPrev scene viewer.
        internal ModelEntity CreateModelEntity(Matrix4? modelMatrix = null)
        {
            var triangles = new Triangle[Lines.Count];
            for (var i = 0; i < triangles.Length; i++)
            {
                triangles[i] = Lines[i].ToTriangle();
            }
            return new ModelEntity
            {
                TexturePage = TexturePage,
                RenderFlags = RenderFlags | RenderFlags.VibRibbon,
                MixtureRate = MixtureRate,
                Visible = Visible,
                DebugMeshRenderInfo = new MeshRenderInfo(this),
                Triangles = triangles,
                LocalMatrix = modelMatrix ?? Matrix4.Identity,
            };
        }

        internal RootEntity CreateRootEntity(Matrix4? modelMatrix = null, string rootEntityName = null)
        {
            var modelEntity = CreateModelEntity(modelMatrix);
            var rootEntity = new RootEntity
            {
                EntityName = rootEntityName ?? nameof(RootEntity),
                ChildEntities = new EntityBase[] { modelEntity },
            };
            return rootEntity;
        }


        public void Clear()
        {
            Lines.Clear();
        }

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
                var matrixValue = matrix.Value;
                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
            }
            Lines.Add(new Line(vertex0, vertex1, color0, color1));
        }

        private void AddCorners(Vector3[] corners, Color color = null)
        {
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

        public void AddBoundsOutline(BoundingBox bounds, Color color = null)
        {
            if (bounds == null)
            {
                return;
            }
            AddCorners(bounds.Corners, color);
        }

        public void AddBoundsOutline(Matrix4? matrix, BoundingBox bounds, Color color = null)
        {
            if (bounds == null)
            {
                return;
            }
            var corners = bounds.Corners;
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;

                var newCorners = new Vector3[BoundingBox.CornerCount];
                for (var i = 0; i < BoundingBox.CornerCount; i++)
                {
                    Vector3.TransformPosition(ref corners[i], ref matrixValue, out newCorners[i]);
                }
                corners = newCorners;
            }
            AddCorners(corners, color);
        }

        // Size refers to the distance from the center to the corners.
        public void AddCubeOutline(Vector3 center, Vector3 size, Color color = null)
        {
            var bounds = new BoundingBox();
            bounds.AddPoint(center - size);
            bounds.AddPoint(center + size);
            AddBoundsOutline(bounds, color);
        }

        public void AddCubeOutline(Matrix4? matrix, Vector3 center, Vector3 size, Color color = null)
        {
            var bounds = new BoundingBox();
            bounds.AddPoint(center - size);
            bounds.AddPoint(center + size);
            AddBoundsOutline(matrix, bounds, color);
        }

        // Size refers to the distance from the center to the corners.
        public void AddRectangleOutline(int axis, Vector3 center, Vector2 size, Color color = null)
        {
            AddRectangleOutline(null, axis, center, size, color);
        }

        public void AddRectangleOutline(Matrix4? matrix, int axis, Vector3 center, Vector2 size, Color color = null)
        {
            var vertex0 = center + GeomMath.SwapAxes(axis, 0f,  size.X,  size.Y);
            var vertex1 = center + GeomMath.SwapAxes(axis, 0f, -size.X,  size.Y);
            var vertex2 = center + GeomMath.SwapAxes(axis, 0f, -size.X, -size.Y);
            var vertex3 = center + GeomMath.SwapAxes(axis, 0f,  size.X, -size.Y);

            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                vertex3 = GeomMath.TransformPosition(ref vertex3, ref matrixValue);
            }

            AddLine(vertex0, vertex1, color);
            AddLine(vertex1, vertex2, color);
            AddLine(vertex2, vertex3, color);
            AddLine(vertex3, vertex0, color);
        }

        public void AddCircleOutline(int axis, Vector3 center, float radius, int sides, Color color = null)
        {
            AddCircleOutline(null, axis, center, radius, sides, color);
        }

        public void AddCircleOutline(Matrix4? matrix, int axis, Vector3 center, float radius, int sides, Color color = null)
        {
            var direction = GeomMath.SwapAxes(axis, 0f, 1f, 0f); // 0f, (float)Math.Cos(0f), (float)Math.Sin(0f));
            var vertexLast = center + direction * radius;

            Matrix4 matrixValue;
            Matrix4 invMatrixValue;
            if (matrix.HasValue)
            {
                matrixValue = matrix.Value;

                vertexLast = GeomMath.TransformPosition(ref vertexLast, ref matrixValue);
            }
            else
            {
                matrixValue = new Matrix4();
                invMatrixValue = new Matrix4();
            }

            for (var i = 1; i <= sides; i++)
            {
                var theta = (Math.PI * 2d) * ((double)i / sides);
                direction = GeomMath.SwapAxes(axis, 0f, (float)Math.Cos(theta), (float)Math.Sin(theta));
                var vertex = center + direction * radius;

                if (matrix.HasValue)
                {
                    vertex = GeomMath.TransformPosition(ref vertex, ref matrixValue);
                }

                AddLine(vertexLast, vertex, color);

                vertexLast = vertex;
            }
        }

        public void AddEntityBounds(EntityBase entity, Color color = null)
        {
            if (entity == null)
            {
                return;
            }
            AddBoundsOutline(entity.Bounds3D, color);
        }

        public void AddTriangleOutline(Triangle triangle, Color color = null)
        {
            if (triangle == null)
            {
                return;
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
            Vector3 vertex0, vertex1, vertex2;
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;

                Vector3.TransformPosition(ref triangle.Vertices[0], ref matrixValue, out vertex0);
                Vector3.TransformPosition(ref triangle.Vertices[1], ref matrixValue, out vertex1);
                Vector3.TransformPosition(ref triangle.Vertices[2], ref matrixValue, out vertex2);
            }
            else
            {
                vertex0 = triangle.Vertices[0];
                vertex1 = triangle.Vertices[1];
                vertex2 = triangle.Vertices[2];
            }
            AddLine(vertex0, vertex1, color);
            AddLine(vertex1, vertex2, color);
            AddLine(vertex2, vertex0, color);
        }
    }
}
