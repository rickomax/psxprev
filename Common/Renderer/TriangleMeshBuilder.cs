using System;
using System.Collections.Generic;
using OpenTK;

namespace PSXPrev.Common.Renderer
{
    public class TriangleMeshBuilder : MeshRenderInfo
    {
        private readonly Vector3[] CubeNormals =
        {
            -Vector3.UnitY,
             Vector3.UnitY,
             Vector3.UnitZ,
            -Vector3.UnitZ,
             Vector3.UnitX,
            -Vector3.UnitX,
        };
        // The order these are defined in is important.
        // Each even index has an even number of negatives.
        // Each odd  index has an odd  number of negatives.
        // This way the order we build triangle vertices in is flipped every even/odd axis.
        private static readonly Vector3[] OctahedronAxes =
        {
            new Vector3( 1f,  1f,  1f),
            new Vector3(-1f,  1f,  1f),
            new Vector3(-1f, -1f,  1f),
            new Vector3( 1f, -1f,  1f),
            new Vector3( 1f, -1f, -1f),
            new Vector3( 1f,  1f, -1f),
            new Vector3(-1f,  1f, -1f),
            new Vector3(-1f, -1f, -1f),
        };
        private static readonly Vector3 SpriteNormal = Vector3.UnitZ;


        public bool CalculateNormals { get; set; }

        // When non-null, lines will be added to this builder to outline polygons created with
        // Add shape functions (excluding Triangle and Quad). This is different from using Wireframe,
        // because this excludes triangles that are split up from larger polygons.
        public LineMeshBuilder OutlineBuilder { get; set; }

        public List<Triangle> Triangles { get; }

        public Matrix4[] JointMatrices { get; set; }

        public int Count => Triangles.Count;

        public int Capacity
        {
            get => Triangles.Capacity;
            set => Triangles.Capacity = value;
        }


        public TriangleMeshBuilder(MeshRenderInfo fromRenderInfo = null)
        {
            Triangles = new List<Triangle>();
            if (fromRenderInfo != null)
            {
                CopyFrom(fromRenderInfo);
            }
        }

        // Does NOT copy Triangles list, use IEnumerable overload for that.
        public TriangleMeshBuilder(TriangleMeshBuilder fromTriangleBuilder)
        {
            Triangles = new List<Triangle>();
            if (fromTriangleBuilder != null)
            {
                CopyFrom(fromTriangleBuilder);
            }
        }

        public TriangleMeshBuilder(int capacity, MeshRenderInfo fromRenderInfo = null)
        {
            Triangles = new List<Triangle>(capacity);
            if (fromRenderInfo != null)
            {
                CopyFrom(fromRenderInfo);
            }
        }

        public TriangleMeshBuilder(int capacity, TriangleMeshBuilder fromTriangleBuilder)
        {
            Triangles = new List<Triangle>(capacity);
            if (fromTriangleBuilder != null)
            {
                CopyFrom(fromTriangleBuilder);
            }
        }

        public TriangleMeshBuilder(IEnumerable<Triangle> triangles, MeshRenderInfo fromRenderInfo = null)
        {
            Triangles = new List<Triangle>(triangles);
            if (fromRenderInfo != null)
            {
                CopyFrom(fromRenderInfo);
            }
        }

        public TriangleMeshBuilder(IEnumerable<Triangle> triangles, TriangleMeshBuilder fromTriangleBuilder)
        {
            Triangles = new List<Triangle>(triangles);
            if (fromTriangleBuilder != null)
            {
                CopyFrom(fromTriangleBuilder);
            }
        }

        // Does NOT copy Triangles list.
        public void CopyFrom(TriangleMeshBuilder triangleBuilder)
        {
            base.CopyFrom(triangleBuilder);
            CalculateNormals = triangleBuilder.CalculateNormals;
            // todo: Should we be making a copy of this?
            OutlineBuilder = new LineMeshBuilder(triangleBuilder.OutlineBuilder);
        }


        // Debug functions for testing built models in the PSXPrev scene viewer.
        internal ModelEntity CreateModelEntity(Matrix4? modelMatrix = null)
        {
            var model = new ModelEntity
            {
                Triangles = Triangles.ToArray(),
                OriginalLocalMatrix = modelMatrix ?? Matrix4.Identity,
            };
            CopyTo(model);
            return model;
        }

        internal RootEntity CreateRootEntity(Matrix4? modelMatrix = null, string rootEntityName = null)
        {
            var modelEntity = CreateModelEntity(modelMatrix);
            var rootEntity = new RootEntity
            {
                Name = rootEntityName ?? nameof(RootEntity),
                ChildEntities = new EntityBase[] { modelEntity },
            };
            return rootEntity;
        }


        public void Clear()
        {
            Triangles.Clear();
        }

        public void AddTriangle(Triangle triangle)
        {
            Triangles.Add(triangle);
        }

        #region AddTriangle

        public void AddTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            if (!CalculateNormals)
            {
                color0 = color0 ?? Color.White;
                Triangles.Add(new Triangle
                {
                    Vertices = new[] { vertex0, vertex1, vertex2 },
                    Colors = new[] { color0, color1 ?? color0, color2 ?? color0 },
                    Normals = Triangle.EmptyNormals,
                    Uv = Triangle.EmptyUv,
                });
            }
            else
            {
                var normal = GeomMath.CalculateNormal(vertex0, vertex1, vertex2);
                AddTriangle(vertex0, vertex1, vertex2, normal, color0, color1, color2);
            }
        }

        public void AddTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 normal,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            color0 = color0 ?? Color.White;
            Triangles.Add(new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Colors = new[] { color0, color1 ?? color0, color2 ?? color0 },
                Normals = new[] { normal, normal, normal },
                Uv = Triangle.EmptyUv,
            });
        }

        public void AddTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                                Vector3 normal0, Vector3 normal1, Vector3 normal2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            color0 = color0 ?? Color.White;
            Triangles.Add(new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Colors = new[] { color0, color1 ?? color0, color2 ?? color0 },
                Normals = new[] { normal0, normal1, normal2 },
                Uv = Triangle.EmptyUv,
            });
        }

        public void AddTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                                Vector2 uv0, Vector2 uv1, Vector2 uv2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            if (!CalculateNormals)
            {
                color0 = color0 ?? Color.White;
                Triangles.Add(new Triangle
                {
                    Vertices = new[] { vertex0, vertex1, vertex2 },
                    Colors = new[] { color0, color1 ?? color0, color2 ?? color0 },
                    Normals = Triangle.EmptyNormals,
                    Uv = new[] { uv0, uv1, uv2 },
                }.CorrectUVTearing());
            }
            else
            {
                var normal = GeomMath.CalculateNormal(vertex0, vertex1, vertex2);
                AddTriangle(vertex0, vertex1, vertex2, normal, uv0, uv1, uv2, color0, color1, color2);
            }
        }

        public void AddTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 normal,
                                Vector2 uv0, Vector2 uv1, Vector2 uv2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            color0 = color0 ?? Color.White;
            Triangles.Add(new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Colors = new[] { color0, color1 ?? color0, color2 ?? color0 },
                Normals = new[] { normal, normal, normal },
                Uv = new[] { uv0, uv1, uv2 },
            }.CorrectUVTearing());
        }

        public void AddTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                                Vector3 normal0, Vector3 normal1, Vector3 normal2,
                                Vector2 uv0, Vector2 uv1, Vector2 uv2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            color0 = color0 ?? Color.White;
            Triangles.Add(new Triangle
            {
                Vertices = new[] { vertex0, vertex1, vertex2 },
                Colors = new[] { color0, color1 ?? color0, color2 ?? color0 },
                Normals = new[] { normal0, normal1, normal2 },
                Uv = new[] { uv0, uv1, uv2 },
            }.CorrectUVTearing());
        }

        #endregion

        #region AddTriangle (matrix)

        public void AddTriangle(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, color0, color1, color2);
        }

        public void AddTriangle(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 normal,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                normal = GeomMath.TransformNormalInverseNormalized(ref normal, ref invMatrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal, color0, color1, color2);
        }

        public void AddTriangle(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                                Vector3 normal0, Vector3 normal1, Vector3 normal2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                normal0 = GeomMath.TransformNormalInverseNormalized(ref normal0, ref invMatrixValue);
                normal1 = GeomMath.TransformNormalInverseNormalized(ref normal1, ref invMatrixValue);
                normal2 = GeomMath.TransformNormalInverseNormalized(ref normal2, ref invMatrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal0, normal1, normal2, color0, color1, color2);
        }

        public void AddTriangle(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                                Vector2 uv0, Vector2 uv1, Vector2 uv2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, uv0, uv1, uv2, color0, color1, color2);
        }

        public void AddTriangle(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 normal,
                                Vector2 uv0, Vector2 uv1, Vector2 uv2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                normal = GeomMath.TransformNormalInverseNormalized(ref normal, ref invMatrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal, uv0, uv1, uv2, color0, color1, color2);
        }

        public void AddTriangle(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                                Vector3 normal0, Vector3 normal1, Vector3 normal2,
                                Vector2 uv0, Vector2 uv1, Vector2 uv2,
                                Color color0 = null, Color color1 = null, Color color2 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                normal0 = GeomMath.TransformNormalInverseNormalized(ref normal0, ref invMatrixValue);
                normal1 = GeomMath.TransformNormalInverseNormalized(ref normal1, ref invMatrixValue);
                normal2 = GeomMath.TransformNormalInverseNormalized(ref normal2, ref invMatrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal0, normal1, normal2, uv0, uv1, uv2, color0, color1, color2);
        }

        #endregion

        #region AddQuad

        public void AddQuad(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            AddTriangle(vertex0, vertex1, vertex2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, color1, color3, color2);
        }

        public void AddQuad(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 normal,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            AddTriangle(vertex0, vertex1, vertex2, normal, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, normal, color1, color3, color2);
        }

        public void AddQuad(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
                            Vector3 normal0, Vector3 normal1, Vector3 normal2, Vector3 normal3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            AddTriangle(vertex0, vertex1, vertex2, normal0, normal1, normal2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, normal1, normal3, normal2, color1, color3, color2);
        }

        public void AddQuad(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
                            Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            AddTriangle(vertex0, vertex1, vertex2, uv0, uv1, uv2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, uv1, uv3, uv2, color1, color3, color2);
        }

        public void AddQuad(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 normal,
                            Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            AddTriangle(vertex0, vertex1, vertex2, normal, uv0, uv1, uv2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, normal, uv1, uv3, uv2, color1, color3, color2);
        }

        public void AddQuad(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
                            Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
                            Vector3 normal0, Vector3 normal1, Vector3 normal2, Vector3 normal3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            AddTriangle(vertex0, vertex1, vertex2, normal0, normal1, normal2, uv0, uv1, uv2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, normal1, normal3, normal2, uv1, uv3, uv2, color1, color3, color2);
        }

        #endregion

        #region AddQuad (matrix)

        public void AddQuad(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                vertex3 = GeomMath.TransformPosition(ref vertex3, ref matrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, color1, color3, color2);
        }

        public void AddQuad(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 normal,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                vertex3 = GeomMath.TransformPosition(ref vertex3, ref matrixValue);
                normal = GeomMath.TransformNormalInverseNormalized(ref normal, ref invMatrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, normal, color1, color3, color2);
        }

        public void AddQuad(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
                            Vector3 normal0, Vector3 normal1, Vector3 normal2, Vector3 normal3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                vertex3 = GeomMath.TransformPosition(ref vertex3, ref matrixValue);
                normal0 = GeomMath.TransformNormalInverseNormalized(ref normal0, ref invMatrixValue);
                normal1 = GeomMath.TransformNormalInverseNormalized(ref normal1, ref invMatrixValue);
                normal2 = GeomMath.TransformNormalInverseNormalized(ref normal2, ref invMatrixValue);
                normal3 = GeomMath.TransformNormalInverseNormalized(ref normal3, ref invMatrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal0, normal1, normal2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, normal1, normal3, normal2, color1, color3, color2);
        }

        public void AddQuad(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
                            Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                vertex3 = GeomMath.TransformPosition(ref vertex3, ref matrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, uv0, uv1, uv2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, uv1, uv3, uv2, color1, color3, color2);
        }

        public void AddQuad(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 normal,
                            Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                vertex3 = GeomMath.TransformPosition(ref vertex3, ref matrixValue);
                normal = GeomMath.TransformNormalInverseNormalized(ref normal, ref invMatrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal, uv0, uv1, uv2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, normal, uv1, uv3, uv2, color1, color3, color2);
        }

        public void AddQuad(Matrix4? matrix, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
                            Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
                            Vector3 normal0, Vector3 normal1, Vector3 normal2, Vector3 normal3,
                            Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                vertex0 = GeomMath.TransformPosition(ref vertex0, ref matrixValue);
                vertex1 = GeomMath.TransformPosition(ref vertex1, ref matrixValue);
                vertex2 = GeomMath.TransformPosition(ref vertex2, ref matrixValue);
                vertex3 = GeomMath.TransformPosition(ref vertex3, ref matrixValue);
                normal0 = GeomMath.TransformNormalInverseNormalized(ref normal0, ref invMatrixValue);
                normal1 = GeomMath.TransformNormalInverseNormalized(ref normal1, ref invMatrixValue);
                normal2 = GeomMath.TransformNormalInverseNormalized(ref normal2, ref invMatrixValue);
                normal3 = GeomMath.TransformNormalInverseNormalized(ref normal3, ref invMatrixValue);
            }
            AddTriangle(vertex0, vertex1, vertex2, normal0, normal1, normal2, uv0, uv1, uv2, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, normal1, normal3, normal2, uv1, uv3, uv2, color1, color3, color2);
        }

        #endregion

        private void AddCorners(Vector3[] corners, Vector3[] normals, Color color = null)
        {
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
            OutlineBuilder?.AddBoundsOutline(bounds);
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
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                var newCorners = new Vector3[BoundingBox.CornerCount];
                var newNormals = new Vector3[BoundingBox.CornerCount];
                for (var i = 0; i < BoundingBox.CornerCount; i++)
                {
                    Vector3.TransformPosition(ref corners[i], ref matrixValue, out newCorners[i]);
                    GeomMath.TransformNormalInverseNormalized(ref normals[i], ref invMatrixValue, out newNormals[i]);
                }
                corners = newCorners;
                normals = newNormals;
            }
            AddCorners(corners, normals, color);
            OutlineBuilder?.AddBoundsOutline(matrix, bounds);
        }

        // Size refers to the distance from the center to the corners.
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

        // Height refers to the distance from the center to the top or bottom.
        public void AddCylinder(int axis, Vector3 center, float height, float radius, int sides, bool gouraud = true, Color color = null)
        {
            AddCylinder(null, axis, center, height, radius, sides, gouraud, color);
        }

        public void AddCylinder(Matrix4? matrix, int axis, Vector3 center, float height, float radius, int sides, bool gouraud = true, Color color = null)
        {
            if (sides < 3)
            {
                throw new ArgumentException(nameof(sides) + " must be greater than or equal to 3", nameof(sides));
            }

            var normals  = new Vector3[sides];
            var vertices = new Vector3[2, sides]; // top/bottom,sides

            var heightVec = GeomMath.SwapAxes(axis, height, 0f, 0f);

            for (var i = 0; i < sides; i++)
            {
                var theta = (Math.PI * 2d) * ((double)i / sides);
                var direction = GeomMath.SwapAxes(axis, 0f, (float)Math.Cos(theta), (float)Math.Sin(theta));
                normals[i] = direction;

                var outer = center + direction * radius;
                vertices[0, i] = outer + heightVec;
                vertices[1, i] = outer - heightVec;
            }

            var topBottomNormals = new[]
            {
                GeomMath.SwapAxes(axis,  1f, 0f, 0f),
                GeomMath.SwapAxes(axis, -1f, 0f, 0f),
            };
            var topBottomVertices = new[]
            {
                center + heightVec,
                center - heightVec,
            };

            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                topBottomVertices[0] = GeomMath.TransformPosition(ref topBottomVertices[0], ref matrixValue);
                topBottomVertices[1] = GeomMath.TransformPosition(ref topBottomVertices[1], ref matrixValue);
                for (var top = 0; top < 2; top++)
                {
                    for (var i = 0; i < sides; i++)
                    {
                        vertices[top, i] = GeomMath.TransformPosition(ref vertices[top, i], ref matrixValue);
                    }
                }

                topBottomNormals[0] = GeomMath.TransformNormalInverseNormalized(ref topBottomNormals[0], ref invMatrixValue);
                topBottomNormals[1] = GeomMath.TransformNormalInverseNormalized(ref topBottomNormals[1], ref invMatrixValue);
                for (var i = 0; i < sides; i++)
                {
                    normals[i] = GeomMath.TransformNormalInverseNormalized(ref normals[i], ref invMatrixValue);
                }
            }

            // Stitch up all points
            for (var top = 0; top < 2; top++)
            {
                var order = top != 0;
                var n = topBottomNormals[top]; // Normal for all top and bottom faces 
                var vc = topBottomVertices[top]; // Center vertex for all top and bottom faces
                for (var i = 0; i < sides; i++)
                {
                    var i2 = (order ? (i + 1) : (i + sides - 1)) % sides;

                    // Add triangle slice for cylinder top and bottom faces
                    var v0 = vertices[top, i];
                    var v1 = vertices[top, i2];
                    AddTriangle(v0, v1, vc, n, color);
                }
            }
            for (var i = 0; i < sides; i++)
            {
                var i2 = (i + 1) % sides;

                // Add quad for cylinder body
                var v0 = vertices[0, i2];
                var v1 = vertices[1, i2];
                var v2 = vertices[0, i];
                var v3 = vertices[1, i];
                var n0 = normals[i2];
                var n1 = normals[i];
                if (!gouraud)
                {
                    n0 = n1 = (n0 + n1).Normalized();
                }
                AddTriangle(v0, v1, v2, n0, n0, n1, color);
                AddTriangle(v1, v3, v2, n0, n1, n1, color);
                OutlineBuilder?.AddLine(v0, v2);
                OutlineBuilder?.AddLine(v2, v3);
                OutlineBuilder?.AddLine(v3, v1);
            }
        }

        // Height refers to the distance from the center to the top or bottom.
        public void AddRing(int axis, Vector3 center, float height, float outerRadius, float innerRadius, int sides, bool gouraud = true, Color color = null)
        {
            AddRing(null, axis, center, height, outerRadius, innerRadius, sides, gouraud, color);
        }

        public void AddRing(Matrix4? matrix, int axis, Vector3 center, float height, float outerRadius, float innerRadius, int sides, bool gouraud = true, Color color = null)
        {
            if (sides < 3)
            {
                throw new ArgumentException(nameof(sides) + " must be greater than or equal to 3", nameof(sides));
            }

            var normals  = new Vector3[sides];
            var vertices = new Vector3[2, 2, sides]; // top/bottom,inner/outer,sides

            var heightVec = GeomMath.SwapAxes(axis, height, 0f, 0f);

            for (var i = 0; i < sides; i++)
            {
                var theta = (Math.PI * 2d) * ((double)i / sides);
                var direction = GeomMath.SwapAxes(axis, 0f, (float)Math.Cos(theta), (float)Math.Sin(theta));
                normals[i] = direction;

                var inner = center + direction * innerRadius;
                var outer = center + direction * outerRadius;
                vertices[0, 0, i] = inner + heightVec;
                vertices[0, 1, i] = outer + heightVec;
                vertices[1, 0, i] = inner - heightVec;
                vertices[1, 1, i] = outer - heightVec;
            }

            var topBottomNormals = new[]
            {
                GeomMath.SwapAxes(axis,  1f, 0f, 0f),
                GeomMath.SwapAxes(axis, -1f, 0f, 0f),
            };

            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                for (var top = 0; top < 2; top++)
                {
                    for (var inner = 0; inner < 2; inner++)
                    {
                        for (var i = 0; i < sides; i++)
                        {
                            vertices[top, inner, i] = GeomMath.TransformPosition(ref vertices[top, inner, i], ref matrixValue);
                        }
                    }
                }

                topBottomNormals[0] = GeomMath.TransformNormalInverseNormalized(ref topBottomNormals[0], ref invMatrixValue);
                topBottomNormals[1] = GeomMath.TransformNormalInverseNormalized(ref topBottomNormals[1], ref invMatrixValue);
                for (var i = 0; i < sides; i++)
                {
                    normals[i] = GeomMath.TransformNormalInverseNormalized(ref normals[i], ref invMatrixValue);
                }
            }

            // Stitch up all points
            for (var top = 0; top < 2; top++)
            {
                var order = top != 0;
                var inner = 1 - top;
                var n = topBottomNormals[top]; // Normal for all top and bottom faces
                for (var i = 0; i < sides; i++)
                {
                    var i2 = (order ? (i + 1) : (i + sides - 1)) % sides;
                    {
                        // Add quad for ring top and bottom faces
                        var v0 = vertices[top, 0, i];
                        var v1 = vertices[top, 1, i];
                        var v2 = vertices[top, 0, i2];
                        var v3 = vertices[top, 1, i2];
                        AddTriangle(v0, v1, v2, n, color);
                        AddTriangle(v1, v3, v2, n, color);
                    }
                    {
                        // Add quad for ring inner and outer sides
                        var v0 = vertices[0, inner, i];
                        var v1 = vertices[1, inner, i];
                        var v2 = vertices[0, inner, i2];
                        var v3 = vertices[1, inner, i2];
                        var n0 = normals[i];
                        var n1 = normals[i2];
                        if (order) // Normal directions on the inner side are reversed
                        {
                            n0 = -n0;
                            n1 = -n1;
                        }
                        if (!gouraud)
                        {
                            n0 = n1 = (n0 + n1).Normalized();
                        }
                        AddTriangle(v0, v1, v2, n0, n0, n1, color);
                        AddTriangle(v1, v3, v2, n0, n1, n1, color);
                        OutlineBuilder?.AddLine(v0, v2);
                        OutlineBuilder?.AddLine(v2, v3);
                        OutlineBuilder?.AddLine(v3, v1);
                    }
                }
            }
        }

        // Build a sphere by subdividing faces of an octahedron into smaller triangles.
        // Triangles are least dense at the center of each face, and most dense near the corners (but not by much).
        public void AddOctaSphere(Vector3 center, float radius, int subdivision, bool gouraud = true, Color color = null)
        {
            AddOctaSphere(null, center, radius, subdivision, gouraud, color);
        }

        public void AddOctaSphere(Matrix4? matrix, Vector3 center, float radius, int subdivision, bool gouraud = true, Color color = null)
        {
            if (subdivision < 1)
            {
                throw new ArgumentException(nameof(subdivision) + " must be greater than or equal to 1", nameof(subdivision));
            }

            // Split the faces of an octahedron into smaller triangles, and normalize each point to make a sphere.
            // Source: <https://stackoverflow.com/a/7687312/7517185>

            // We're only using about half of the array values, but lookup like this is much simpler.
            // At h = 0, w length == 1. At h = subdivision, w length == subdivision + 1.
            var normals  = new Vector3[8, subdivision + 1, subdivision + 1]; // axes,height,width
            var vertices = new Vector3[8, subdivision + 1, subdivision + 1];

            // Calculate directions of a single face, we can later use those for all other faces.
            // Start at h = 1 because we don't need to calculate h = 0, w = 0,
            // plus we can skip divide-by-zero checks with `w / h`.
            normals[0, 0, 0] = Vector3.UnitY;
            // h determines the height, where h = 0 is the top of the octahedron, and h = subdivision is the middle.
            // w determines the width,  where w = 0 is aligned with the X axis, and w = h is aligned with the Z axis.
            for (var h = 1; h <= subdivision; h++)
            {
                var hscalar = (float)h / subdivision;
                var y = 1f - hscalar;
                for (var w = 0; w <= h; w++)
                {
                    var wscalar = (float)w / h;
                    var x = hscalar * (1f - wscalar);
                    var z = hscalar * wscalar;
                    normals[0, h, w] = new Vector3(x, y, z).Normalized();
                }
            }

            // Convert directions to vertex positions, and apply to each face of the octahedron.
            for (var face = 0; face < 8; face++)
            {
                var axis = OctahedronAxes[face];
                for (var h = 0; h <= subdivision; h++)
                {
                    for (var w = 0; w <= h; w++)
                    {
                        normals[face, h, w] = normals[0, h, w] * axis;
                        vertices[face, h, w] = center + normals[face, h, w] * radius;
                    }
                }
            }

            // Transform points and directions (normals) if needed.
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                for (var face = 0; face < 8; face++)
                {
                    for (var h = 0; h <= subdivision; h++)
                    {
                        for (var w = 0; w <= h; w++)
                        {
                            vertices[face, h, w] = GeomMath.TransformPosition(ref vertices[face, h, w], ref matrixValue);
                            normals[face, h, w] = GeomMath.TransformNormalInverseNormalized(ref normals[face, h, w], ref invMatrixValue);
                        }
                    }
                }
            }

            // Build triangles for each face of the octahedron.
            for (var face = 0; face < 8; face++)
            {
                var order = face % 2 == 0;
                for (var h = 1; h <= subdivision; h++)
                {
                    for (var w = 1; w <= h; w++)
                    {
                        var w1 = order ? w - 1 : w;
                        var w2 = order ? w : w - 1;

                        var v0 = vertices[face, h - 1, w - 1];
                        var v1 = vertices[face, h, w1];
                        var v2 = vertices[face, h, w2];
                        var n0 = normals[face, h - 1, w - 1];
                        var n1 = normals[face, h, w1];
                        var n2 = normals[face, h, w2];
                        if (gouraud)
                        {
                            AddTriangle(v0, v1, v2, n0, n1, n2, color);
                        }
                        else
                        {
                            var n = (n0 + n1 + n2).Normalized();
                            AddTriangle(v0, v1, v2, n, color);
                        }
                        OutlineBuilder?.AddTriangleOutline(v0, v1, v2);

                        if (h < subdivision) // There are no second triangles to draw on the last loop of h
                        {
                            var v3 = vertices[face, h + 1, w];
                            var n3 = normals[face, h + 1, w];
                            if (gouraud)
                            {
                                AddTriangle(v1, v3, v2, n1, n3, n2, color);
                            }
                            else
                            {
                                var n = (n1 + n3 + n2).Normalized();
                                AddTriangle(v1, v3, v2, n, color);
                            }
                            OutlineBuilder?.AddTriangleOutline(v1, v3, v2);
                        }
                    }
                }
            }
        }

        // Build a sphere using the standard method of dividing
        // into sectors (number of horizontal sides from 0deg to 360deg)
        // and stacks (number of vertical sides from 90deg to -90deg).
        // Triangles are most dense near the top and bottom, and least dense near the equator.
        public void AddSphere(Vector3 center, float radius, int sectors, int stacks, bool gouraud = true, Color color = null)
        {
            AddSphere(null, center, radius, sectors, stacks, gouraud, color);
        }

        public void AddSphere(Matrix4? matrix, Vector3 center, float radius, int sectors, int stacks, bool gouraud = true, Color color = null)
        {
            if (sectors < 3)
            {
                throw new ArgumentException(nameof(sectors) + " must be greater than or equal to 3", nameof(sectors));
            }
            if (stacks < 2)
            {
                throw new ArgumentException(nameof(stacks) + " must be greater than or equal to 2", nameof(stacks));
            }

            var normals  = new Vector3[stacks + 1, sectors];
            var vertices = new Vector3[stacks + 1, sectors];

            // Precompute cos/sin values for sectors, since we'll be reusing them in a nested loop.
            var sectorsCosSin = new float[(sectors / 2) + 1, 2];
            for (var w = 0; w <= (sectors / 2); w++)
            {
                // From 0deg to 360deg (exclusive)
                var wtheta = (Math.PI * 2d) * ((double)w / sectors);
                sectorsCosSin[w, 0] = (float)Math.Cos(wtheta);
                sectorsCosSin[w, 1] = (float)Math.Sin(wtheta);
            }

            // Convert cosine/sine angles into directions, and convert
            // directions into translated and scaled (center + direction * radius) points.
            // We only need to calculate half of the stacks and sectors. We can just negate the other half.
            void CalcPoint(int h, int w, float sectorCos, float sectorSin, float stackCos, float stackSin, bool normalOnly = false)
            {
                var y = stackSin;
                var x = sectorCos * stackCos;
                var z = sectorSin * stackCos;
                normals[h, w] = new Vector3(x, y, z);
                if (!normalOnly)
                {
                    vertices[h, w] = center + normals[h, w] * radius;
                }
            }
            for (var h = 0; h <= (stacks / 2); h++)
            {
                // From 90deg to -90deg (inclusive)
                var htheta = (Math.PI / 2d) - Math.PI * ((double)h / stacks);
                var stackCos = (float)Math.Cos(htheta);
                var stackSin = (float)Math.Sin(htheta);

                var h2 = stacks - h;
                for (var w = 0; w <= (sectors / 2); w++)
                {
                    var sectorCos = sectorsCosSin[w, 0];
                    var sectorSin = sectorsCosSin[w, 1];
                    CalcPoint(h, w, sectorCos, sectorSin, stackCos, stackSin);
                    CalcPoint(h2, w, sectorCos, sectorSin, stackCos, -stackSin);
                    if (w > 0) // First angle is 0deg, and we're not storing a counterpart for that
                    {
                        var w2 = sectors - w;
                        CalcPoint(h, w2, sectorCos, -sectorSin, stackCos, stackSin);
                        CalcPoint(h2, w2, sectorCos, -sectorSin, stackCos, -stackSin);
                    }
                }
            }

            // Transform points and directions (normals) if needed.
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out var invMatrixValue);

                for (var h = 0; h <= stacks; h++)
                {
                    for (var w = 0; w < sectors; w++)
                    {
                        vertices[h, w] = GeomMath.TransformPosition(ref vertices[h, w], ref matrixValue);
                        normals[h, w] = GeomMath.TransformNormalInverseNormalized(ref normals[h, w], ref invMatrixValue);
                    }
                }
            }

            // Build triangles for each sector/stack.
            for (var h = 0; h < stacks; h++)
            {
                // The top and bottom have triangles that converge on the center, but not quads.
                // Either the first or second triangle needs to be excluded depending on which side we're on.
                var top = h == 0;
                var bottom = h == stacks - 1;

                for (var w = 0; w < sectors; w++)
                {
                    var w2 = (w + 1) % sectors;

                    // Add quad (or triangle if we're at the top or bottom)
                    var v0 = vertices[h,     w];
                    var v1 = vertices[h + 1, w];
                    var v2 = vertices[h,     w2];
                    var v3 = vertices[h + 1, w2];
                    var n0 = normals[h,     w];
                    var n1 = normals[h + 1, w];
                    var n2 = normals[h,     w2];
                    var n3 = normals[h + 1, w2];
                    if (!gouraud)
                    {
                        n0 = n1 = n2 = n3 = (n0 + n1 + n2 + n3).Normalized();
                    }
                    if (!top)
                    {
                        AddTriangle(v0, v1, v2, n0, n1, n2, color);
                    }
                    if (!bottom)
                    {
                        AddTriangle(v1, v3, v2, n1, n3, n2, color);
                    }
                    if (OutlineBuilder != null)
                    {
                        if (!top && !bottom)
                        {
                            OutlineBuilder.AddQuadOutline(v0, v1, v2, v3);
                        }
                        else if (!top)
                        {
                            OutlineBuilder.AddTriangleOutline(v0, v1, v2);
                        }
                        else if (!bottom)
                        {
                            OutlineBuilder.AddTriangleOutline(v1, v3, v2);
                        }
                    }
                }
            }
        }

        // Bottom refers to the center of the bottom face of the cone.
        // If smoothTop is true, then the normals for the tip of the cone will all point upwards. This fixes the fact
        // that gouraud shading cannot represent smooth normals on a cone, but the normals will not be entirely correct.
        // If flip is true, then the tip of the cone will point towards the negative direction of the axis.
        // If gouraud is false, then smoothTop is ignored.
        public void AddCone(int axis, Vector3 center, float height, float radius, int sides, bool smoothTop, bool flip = false, bool gouraud = true, Color color = null)
        {
            AddCone(null, axis, center, height, radius, sides, smoothTop, flip, gouraud, color);
        }

        public void AddCone(Matrix4? matrix, int axis, Vector3 bottom, float height, float radius, int sides, bool smoothTop, bool flip = false, bool gouraud = true, Color color = null)
        {
            // Compared to other drawing functions with an axis argument, cones
            // are a lot messier since the up/down direction of axis is important.
            var sign = flip ? -1f : 1f;
            var l = (float)Math.Sqrt((height * height) + (radius * radius));
            var normalBase = GeomMath.SwapAxes(axis, radius / l * sign, height / l, height / l);

            // Use X = 1f to preserve vertical component in normal calculation.
            var normalDirection = GeomMath.SwapAxes(axis, 1f, 1f, 0f); // 1f, (float)Math.Cos(0f), (float)Math.Sin(0f));
            var vertexDirection = GeomMath.SwapAxes(axis, 0f, 1f, 1f); // Cancels out vertical axis component from normal in vertex calculations

            var normalTop    = GeomMath.SwapAxes(axis, 1f * sign, 0f, 0f); // Used if smoothTop is true
            var normalBottom = -normalTop;
            var normalLast   = normalDirection * normalBase; // Start normal at 0deg

            var vertexTop    = bottom + normalTop * height; // Top and bottom vertices
            var vertexBottom = bottom;
            var vertexLast   = bottom + (normalDirection * vertexDirection) * radius; // Start vertex at 0deg

            Matrix4 matrixValue;
            Matrix4 invMatrixValue;
            if (matrix.HasValue)
            {
                matrixValue = matrix.Value;
                GeomMath.InvertSafe(ref matrixValue, out invMatrixValue);

                vertexTop = GeomMath.TransformPosition(ref vertexTop, ref matrixValue);
                vertexBottom = GeomMath.TransformPosition(ref vertexBottom, ref matrixValue);
                vertexLast = GeomMath.TransformPosition(ref vertexLast, ref matrixValue);

                if (smoothTop && gouraud)
                {
                    normalTop = GeomMath.TransformNormalInverseNormalized(ref normalTop, ref invMatrixValue);
                }
                normalBottom = GeomMath.TransformNormalInverseNormalized(ref normalBottom, ref invMatrixValue);
                normalLast = GeomMath.TransformNormalInverseNormalized(ref normalLast, ref invMatrixValue);
            }
            else
            {
                matrixValue = new Matrix4();
                invMatrixValue = new Matrix4();
            }

            for (var i = 1; i <= sides; i++)
            {
                // Use X = 1f to preserve vertical component in normal calculation.
                if (!smoothTop || !gouraud) // We're not reusing the same normal for all top vertices
                {
                    var halfTheta = (Math.PI * 2d) * (((double)i - 0.5d) / sides);
                    normalDirection = GeomMath.SwapAxes(axis, 1f, (float)Math.Cos(halfTheta), (float)Math.Sin(halfTheta));
                    normalTop = normalDirection * normalBase;
                }

                var theta = (Math.PI * 2d) * ((double)i / sides);
                normalDirection = GeomMath.SwapAxes(axis, 1f, (float)Math.Cos(theta), (float)Math.Sin(theta));
                var normal = normalDirection * normalBase;
                var vertex = bottom + (normalDirection * vertexDirection) * radius;

                if (matrix.HasValue)
                {
                    vertex = GeomMath.TransformPosition(ref vertex, ref matrixValue);

                    if (!smoothTop || !gouraud)
                    {
                        normalTop = GeomMath.TransformNormalInverseNormalized(ref normalTop, ref invMatrixValue);
                    }
                    normal = GeomMath.TransformNormalInverseNormalized(ref normal, ref invMatrixValue);
                }

                // Add body and bottom triangles
                if (!gouraud)
                {
                    normalLast = normal = normalTop;
                }
                if (!flip)
                {
                    AddTriangle(vertex, vertexLast, vertexTop, normal, normalLast, normalTop, color);
                    AddTriangle(vertexLast, vertex, vertexBottom, normalBottom, color);
                }
                else
                {
                    AddTriangle(vertexLast, vertex, vertexTop, normalLast, normal, normalTop, color);
                    AddTriangle(vertex, vertexLast, vertexBottom, normalBottom, color);
                }
                OutlineBuilder?.AddLine(vertexLast, vertex);
                OutlineBuilder?.AddLine(vertexLast, vertexTop);

                vertexLast = vertex;
                normalLast = normal;
            }
        }

        // Size refers to the distance from the center to the corners.
        public void AddSprite(Vector3 center, Vector2 size, Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            AddSprite(null, center, size, color0, color1, color2, color3);
        }

        public void AddSprite(Matrix4? matrix, Vector3 center, Vector2 size, Color color0 = null, Color color1 = null, Color color2 = null, Color color3 = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                center = GeomMath.TransformPosition(ref center, ref matrixValue);
            }
            // Don't transform individual vertices, since they must always face the camera
            var vertex0 = center + new Vector3(-size.X,  size.Y, 0f);
            var vertex1 = center + new Vector3( size.X,  size.Y, 0f);
            var vertex2 = center + new Vector3(-size.X, -size.Y, 0f);
            var vertex3 = center + new Vector3( size.X, -size.Y, 0f);
            AddTriangle(vertex0, vertex1, vertex2, SpriteNormal, color0, color1, color2);
            AddTriangle(vertex1, vertex3, vertex2, SpriteNormal, color1, color3, color2);
        }

        public void AddSprite(Vector3 center, Vector2 size, int u, int v, int uSize, int vSize, Color color = null)
        {
            AddSprite(null, center, size, u, v, uSize, vSize, color);
        }

        public void AddSprite(Matrix4? matrix, Vector3 center, Vector2 size, int u, int v, int uSize, int vSize, Color color = null)
        {
            var uv = GeomMath.ConvertUV((uint)u, (uint)v);
            var uvSize = GeomMath.ConvertUV((uint)uSize, (uint)vSize);
            AddSprite(null, center, size, uv, uvSize, color);
        }

        public void AddSprite(Vector3 center, Vector2 size, Vector2 uv, Vector2 uvSize, Color color = null)
        {
            AddSprite(null, center, size, uv, uvSize, color);
        }

        public void AddSprite(Matrix4? matrix, Vector3 center, Vector2 size, Vector2 uv, Vector2 uvSize, Color color = null)
        {
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                center = GeomMath.TransformPosition(ref center, ref matrixValue);
            }
            // Don't transform individual vertices, since they must always face the camera
            var vertex0 = center + new Vector3(-size.X,  size.Y, 0f);
            var vertex1 = center + new Vector3( size.X,  size.Y, 0f);
            var vertex2 = center + new Vector3(-size.X, -size.Y, 0f);
            var vertex3 = center + new Vector3( size.X, -size.Y, 0f);
            var uv0 = uv + new Vector2(      0f,       0f);
            var uv1 = uv + new Vector2(uvSize.X,       0f);
            var uv2 = uv + new Vector2(      0f, uvSize.Y);
            var uv3 = uv + new Vector2(uvSize.X, uvSize.Y);
            AddTriangle(vertex0, vertex1, vertex2, SpriteNormal, uv0, uv1, uv2, color);
            AddTriangle(vertex1, vertex3, vertex2, SpriteNormal, uv1, uv3, uv2, color);
        }
    }
}
