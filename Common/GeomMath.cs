using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using OpenTK;
using OpenTK.Graphics;

namespace PSXPrev.Common
{
    public static class GeomMath
    {
        public const string FloatFormat = "0.00000";
        public const string IntegerFormat = "0";
        public const string CompleteFloatFormat = "{0:0.00000}";

        public const float One2Rad = (float)(Math.PI * 2d);
        public const float Deg2Rad = (float)((Math.PI * 2d) / 360d);
        public const float Rad2Deg = (float)(360d / (Math.PI * 2d));

        public const float Fixed12Scalar = 4096f;
        public const float Fixed16Scalar = 65536f;

        public static float UVScalar => Program.FixUVAlignment ? 256f : 255f;

        // Use Vector3.Unit(XYZ) fields instead.
        //public static Vector3 XVector = new Vector3(1f, 0f, 0f);
        //public static Vector3 YVector = new Vector3(0f, 1f, 0f);
        //public static Vector3 ZVector = new Vector3(0f, 0f, 1f);

        // Use Vector3.Distance instead.
        //public static float VecDistance(Vector3 a, Vector3 b)
        //{
        //    var x = a.X - b.X;
        //    var y = a.Y - b.Y;
        //    var z = a.Z - b.Z;
        //    return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
        //}

        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
        }

        public static Vector4 ToVector4(this Color color)
        {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static Vector4 ToVector4WithAlpha(this Color color, float alpha)
        {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, alpha);
        }

        public static Color4 ToColor4(this Color color)
        {
            return new Color4(color.R, color.G, color.B, color.A);
        }

        public static Color4 ToColor4WithAlpha(this Color color, float alpha)
        {
            return new Color4(color.R, color.G, color.B, (byte)(alpha * 255));
        }


        public static Vector3 TransformNormalNormalized(Vector3 normal, Matrix4 matrix)
        {
            TransformNormalNormalized(ref normal, ref matrix, out var result);
            return result;
        }

        public static void TransformNormalNormalized(ref Vector3 normal, ref Matrix4 matrix, out Vector3 result)
        {
            InvertSafe(ref matrix, out var invMatrix);
            TransformNormalInverseNormalized(ref normal, ref invMatrix, out result);
        }

        public static Vector3 TransformNormalInverseNormalized(Vector3 normal, Matrix4 invMatrix)
        {
            TransformNormalInverseNormalized(ref normal, ref invMatrix, out var result);
            return result;
        }

        public static void TransformNormalInverseNormalized(ref Vector3 normal, ref Matrix4 invMatrix, out Vector3 result)
        {
            Vector3.TransformNormalInverse(ref normal, ref invMatrix, out result);
            if (!result.IsZero())
            {
                result.Normalize();
            }
        }


        // One-liners for help assigning to the same value, while avoiding a struct copy of Matrix4.
        public static Vector3 TransformNormalNormalized(ref Vector3 normal, ref Matrix4 matrix)
        {
            InvertSafe(ref matrix, out var invMatrix);
            return TransformNormalInverseNormalized(ref normal, ref invMatrix);
        }

        public static Vector3 TransformNormalInverseNormalized(ref Vector3 normal, ref Matrix4 invMatrix)
        {
            TransformNormalInverseNormalized(ref normal, ref invMatrix, out var result);
            return result;
        }

        public static Vector3 TransformPosition(ref Vector3 position, ref Matrix4 matrix)
        {
            Vector3.TransformPosition(ref position, ref matrix, out var result);
            return result;
        }


        public static Matrix4 SetRotation(this Matrix4 matrix, Quaternion rotation)
        {
            matrix = matrix.ClearRotation();
            matrix *= Matrix4.CreateFromQuaternion(rotation);
            return matrix;
        }

        public static Matrix4 SetScale(this Matrix4 matrix, Vector3 scale)
        {
            matrix = matrix.ClearScale();
            matrix *= Matrix4.CreateScale(scale);
            return matrix;
        }

        public static Matrix4 SetTranslation(this Matrix4 matrix, Vector3 translation)
        {
            matrix = matrix.ClearTranslation();
            matrix *= Matrix4.CreateTranslation(translation);
            return matrix;
        }

        public static string WriteVector3(Vector3? v)
        {
            if (v != null)
            {
                var vr = v.Value;
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat(CompleteFloatFormat, vr.X).Append(", ").AppendFormat(CompleteFloatFormat, vr.Y).Append(", ").AppendFormat(CompleteFloatFormat, vr.Z);
                return stringBuilder.ToString();
            }
            return string.Empty;
        }

        public static object WriteIntArray(int[] intArray)
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < intArray.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(", ");
                }
                var item = intArray[i];
                stringBuilder.Append(item);
            }
            return stringBuilder.ToString();
        }

        public static Matrix4 CreateRotation(Vector3 rotation)
        {
            // todo: Should this actually be RotationOrder.XYZ (zRot * yRot * xRot)?
            return CreateRotation(rotation, RotationOrder.ZYX); // RotationOrder.XYZ);
        }

        public static Matrix4 CreateRotation(Vector3 rotation, RotationOrder order)
        {
            var xRot = Matrix4.CreateRotationX(rotation.X);
            var yRot = Matrix4.CreateRotationY(rotation.Y);
            var zRot = Matrix4.CreateRotationZ(rotation.Z);
            switch (order)
            {
                case RotationOrder.XYZ: return zRot * yRot * xRot;
                case RotationOrder.XZY: return yRot * zRot * xRot;
                case RotationOrder.YXZ: return zRot * xRot * yRot;
                case RotationOrder.YZX: return xRot * zRot * yRot;
                case RotationOrder.ZXY: return yRot * xRot * zRot;
                case RotationOrder.ZYX: return xRot * yRot * zRot;
            }
            return Matrix4.Identity; // Invalid rotation order
        }

        // Avoid getting NaN quaternions when any matrix scale dimension is 0.
        public static Quaternion ExtractRotationSafe(this Matrix3 matrix)
        {
            var rotation = matrix.ExtractRotation();
            return float.IsNaN(rotation.X) ? Quaternion.Identity : rotation;
        }

        public static Quaternion ExtractRotationSafe(this Matrix4 matrix)
        {
            var rotation = matrix.ExtractRotation();
            return float.IsNaN(rotation.X) ? Quaternion.Identity : rotation;
        }

        public static Matrix3 InvertSafe(Matrix3 matrix)
        {
            return matrix.Inverted(); // This is the only function where Determinant is checked
        }

        public static Matrix4 InvertSafe(Matrix4 matrix)
        {
            return matrix.Inverted(); // This is the only function where Determinant is checked
        }

        public static bool InvertSafe(ref Matrix3 matrix, out Matrix3 result)
        {
            if (matrix.Determinant != 0f)
            {
                Matrix3.Invert(ref matrix, out result);
                return true;
            }
            // Can't invert singular matrix
            result = matrix;
            return false;
        }

        public static bool InvertSafe(ref Matrix4 matrix, out Matrix4 result)
        {
            if (matrix.Determinant != 0f)
            {
                Matrix4.Invert(ref matrix, out result);
                return true;
            }
            // Can't invert singular matrix
            result = matrix;
            return false;
        }

        public static Matrix4 CreateFromQuaternionAroundOrigin(Quaternion rotation, Vector3 origin)
        {
            CreateFromQuaternionAroundOrigin(ref rotation, ref origin, out var result);
            return result;
        }

        public static void CreateFromQuaternionAroundOrigin(ref Quaternion rotation, ref Vector3 origin, out Matrix4 result)
        {
            Matrix4.CreateFromQuaternion(ref rotation, out var rotationMatrix);

            var invOrigin = -origin;
            Matrix4.CreateTranslation(ref invOrigin, out var invOriginMatrix);
            Matrix4.Mult(ref invOriginMatrix, ref rotationMatrix, out result);

            Matrix4.CreateTranslation(ref origin, out var originMatrix);
            Matrix4.Mult(ref result, ref originMatrix, out result);
        }

        public static Matrix4 CreateSpriteWorldMatrix(Matrix4 worldMatrix, Vector3 origin)
        {
            CreateSpriteWorldMatrix(ref worldMatrix, ref origin, out var result);
            return result;
        }

        public static void CreateSpriteWorldMatrix(ref Matrix4 worldMatrix, ref Vector3 origin, out Matrix4 result)
        {
            var invWorldRotation = worldMatrix.ExtractRotationSafe().Inverted();
            CreateFromQuaternionAroundOrigin(ref invWorldRotation, ref origin, out result);
            Matrix4.Mult(ref result, ref worldMatrix, out result);
        }

        public static Vector3 UnProject(this Vector3 position, Matrix4 projection, Matrix4 view, float width, float height)
        {
            // Not entirely sure if the -1 in `height - 1f - position.Y` should be there or not.
            // For now, intersections with the gizmo seem slightly more accurate with the -1.

            // OpenTK version:
            //var viewProjInv = (view * projection).Inverted();
            //position.Y = height - 1f - position.Y;
            //return Vector3.Unproject(position, 0f, 0f, width, height, 0f, 1f, viewProjInv);

            Vector4 vec;
            vec.X = 2.0f * position.X / width - 1;
            vec.Y = 2.0f * (height - 1f - position.Y) / height - 1;
            vec.Z = 2.0f * position.Z - 1; // 2.0f * position.Z / depth - 1; // Where depth=1
            vec.W = 1.0f;
            InvertSafe(ref view, out var viewInv);
            InvertSafe(ref projection, out var projInv);
            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);
            if (Math.Abs(vec.W) > 0.0000000001f) //0.000001f)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }
            return vec.Xyz;
        }

        public static Vector3 ProjectOnNormal(this Vector3 vector, Vector3 normal)
        {
            var num = normal.LengthSquared;
            if (num < float.Epsilon) // The same as num <= 0f
            {
                return Vector3.Zero;
            }
            return normal * Vector3.Dot(vector, normal) / num;
        }

        // Useful for BoxIntersect so that we can still operate on an axis-aligned box.
        public static void TransformRay(Vector3 rayOrigin, Vector3 rayDirection, Matrix4 matrix, out Vector3 resultOrigin, out Vector3 resultDirection)
        {
            InvertSafe(ref matrix, out var invMatrix);
            Vector3.TransformPosition(ref rayOrigin, ref invMatrix, out resultOrigin);
            TransformNormalInverseNormalized(ref rayDirection, ref matrix, out resultDirection);
            if (!resultDirection.IsZero())
            {
                resultDirection.Normalize();
            }
        }

        public static float BoxIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 boxMin, Vector3 boxMax)
        {
            var t1 = (boxMin.X - rayOrigin.X) / rayDirection.X;
            var t2 = (boxMax.X - rayOrigin.X) / rayDirection.X;
            var t3 = (boxMin.Y - rayOrigin.Y) / rayDirection.Y;
            var t4 = (boxMax.Y - rayOrigin.Y) / rayDirection.Y;
            var t5 = (boxMin.Z - rayOrigin.Z) / rayDirection.Z;
            var t6 = (boxMax.Z - rayOrigin.Z) / rayDirection.Z;

            var aMin = t1 < t2 ? t1 : t2;
            var bMin = t3 < t4 ? t3 : t4;
            var cMin = t5 < t6 ? t5 : t6;

            var aMax = t1 > t2 ? t1 : t2;
            var bMax = t3 > t4 ? t3 : t4;
            var cMax = t5 > t6 ? t5 : t6;

            var fMax = aMin > bMin ? aMin : bMin;
            var fMin = aMax < bMax ? aMax : bMax;

            var t7 = fMax > cMin ? fMax : cMin;
            var t8 = fMin < cMax ? fMin : cMax;

            var t9 = (t8 < 0 || t7 > t8) ? -1 : t7;

            return t9;
        }

        public static float BoxIntersect2(Vector3 rayOrigin, Vector3 rayDirection, Vector3 center, Vector3 size)
        {
            var boxMin = center - size;
            var boxMax = center + size;
            return BoxIntersect(rayOrigin, rayDirection, boxMin, boxMax);
        }

        public static float PlaneIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 planeOrigin, Vector3 planeNormal,
                                           out Vector3 intersection, bool front = true, bool back = true)
        {
            var diff = rayOrigin - planeOrigin;
            var prod1 = -Vector3.Dot(diff, planeNormal);
            var prod2 = Vector3.Dot(rayDirection, planeNormal);
            if (Math.Abs(prod2) <= 0.0000000001f) //0.000001f)
            {
                intersection = Vector3.Zero;
                return -1f; // Ray and plane are parallel.
            }
            if ((!front && prod2 > 0f) || (!back && prod2 < 0f))
            {
                intersection = Vector3.Zero;
                return -1f; // Ray intersects from a side we don't want to intersect with.
            }
            var distance = prod1 / prod2;

            intersection = rayOrigin + rayDirection * distance;
            return distance;
        }

        public static float DiscIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 planeOrigin, Vector3 planeNormal,
                                          float radius, out Vector3 intersection, bool front = true, bool back = true)
        {
            var distance = PlaneIntersect(rayOrigin, rayDirection, planeOrigin, planeNormal,
                                          out intersection, front, back);
            var o = intersection - planeOrigin;
            if (o.LengthSquared >= (radius * radius))
            {
                return -1f;
            }
            return distance;
        }

        // Untested
        public static float SphereIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 center, float radius)//, out Vector3 intersection)
        {
            // Source: <https://iquilezles.org/articles/intersectors/>
            var oc = rayOrigin - center;
            var b = Vector3.Dot(oc, rayDirection);
            var c = oc.LengthSquared - (radius * radius);
            var h = (b * b) - c;
            if (h < 0f)
            {
                //intersection = Vector3.Zero;
                return -1f;
            }
            //intersection = new Vector2(-b - h, -b + h); // ???
            return (float)Math.Sqrt(h);
        }

        // Untested
        public static float ConeIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 top, Vector3 bottom, float radius)//, out Vector3 intersection)
        {
            return CappedConeIntersect(rayOrigin, rayDirection, top, bottom, 0f, radius);//out intersection);
        }

        // Untested
        public static float CappedConeIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 top, Vector3 bottom,
                                                float topRadius, float bottomRadius)//, out Vector3 intersection)
        {
            // Source: <https://iquilezles.org/articles/intersectors/>
            var ba = bottom - top;
            var oa = rayOrigin - top;
            var ob = rayOrigin - bottom;
            var m0 = ba.LengthSquared;
            var m1 = Vector3.Dot(oa, ba);
            var m2 = Vector3.Dot(rayDirection, ba);
            var m3 = Vector3.Dot(rayDirection, oa);
            var m5 = oa.LengthSquared;
            var m9 = Vector3.Dot(ob, ba);

            // Caps
            if (m1 < 0f)
            {
                if (((oa * m2) - (rayDirection * m1)).LengthSquared < (topRadius * topRadius * m2 * m2))
                {
                    //intersection = -ba / (float)Math.Sqrt(m0);
                    return -m1 / m2;
                }
            }
            else if (m9 > 0f)
            {
                if ((ob + (rayDirection * -m9 / m2)).LengthSquared < (bottomRadius * bottomRadius))
                {
                    //intersection = ba / (float)Math.Sqrt(m0);
                    return -m9 / m2;
                }
            }

            // Body
            var rr = topRadius - bottomRadius;
            var hy = m0 + (rr * rr);
            var k2 = (m0 * m0) - (m2 * m2 * hy);
            var k1 = (m0 * m0 * m3) - (m1 * m2 * hy) + (m0 * topRadius * (rr * m2 * 1f));
            var k0 = (m0 * m0 * m5) - (m1 * m1 * hy) + (m0 * topRadius * ((rr * m1 * 2f) - (m0 * topRadius)));
            var h = (k1 * k1) - (k2 * k0);
            if (h < 0f)
            {
                //intersection = Vector3.Zero;
                return -1f;
            }
            var t = (-k1 - (float)Math.Sqrt(h)) / k2;
            var y = m1 + (t * m2);
            if (y < 0f || y > m0)
            {
                //intersection = Vector3.Zero;
                return -1f;
            }
            //intersection = (m0 * (m0 * (oa + rayDirection * t) + (rr * ba * topRadius)) - (ba * hy * y)).Normalized(); // ???
            return t;
        }

        // Untested
        // Height refers to the distance from the center to the top or bottom.
        public static float CylinderIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 center, Vector3 normal,
                                              float height, float radius, out Vector3 intersection)
        {
            var top    = center + normal * height;
            var bottom = center - normal * height;
            return CylinderIntersect(rayOrigin, rayDirection, top, bottom, radius, out intersection);
        }

        // Untested
        public static float CylinderIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 top, Vector3 bottom,
                                              float radius, out Vector3 intersection)
        {
            // Source: <https://iquilezles.org/articles/intersectors/>
            var ba = bottom - top;
            var oc = rayOrigin - top;
            var baba = ba.LengthSquared;
            var bard = Vector3.Dot(ba, rayDirection);
            var baoc = Vector3.Dot(ba, oc);

            var k2 = baba - (bard * bard);
            var k1 = baba * Vector3.Dot(oc, rayDirection) - (baoc * bard);
            var k0 = baba * oc.LengthSquared - (baoc * baoc) - (radius * radius * baba);

            var h = (k1 * k1) - (k2 * k0);
            if (h < 0f)
            {
                intersection = Vector3.Zero;
                return -1f;
            }

            h = (float)Math.Sqrt(h);
            var distance = (-k1 - h) / k2;

            // Test intersection with body
            var y = baoc + (distance * bard);
            if (y > 0f && y < baba)
            {
                intersection = ((oc + rayDirection * distance) - (ba * y / baba)) / radius;
                return distance;
            }

            // Test intersection with caps
            distance = ((y < 0f ? 0f : baba) - baoc) / bard;
            if (Math.Abs(k1 + (k2 * distance)) < h)
            {
                intersection = ba * Math.Sign(y) / (float)Math.Sqrt(baba);
                return distance;
            }

            intersection = Vector3.Zero;
            return -1f;
        }

        // Returns the intersection of a cylinder with a hole in it.
        // Height refers to the distance from the center to the top or bottom.
        // Note: The returned intersection distance and point only refer to the result of CylinderIntersect.
        public static float RingIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 center, Vector3 normal, float height,
                                          float outerRadius, float innerRadius, out Vector3 intersection)
        {
            var top = center + normal * height;
            var bottom = center - normal * height;
            var distance = CylinderIntersect(rayOrigin, rayDirection, top, bottom, outerRadius, out intersection);
            if (distance < 0f)
            {
                return -1f;
            }
            // We're intersecting with the cylinder, now make sure we aren't intersecting with both
            // the top and bottom discs of the inside of the ring. Intersecting with only zero or one
            // disc means we're still intersecting with the ring, but both means we aren't.
            if (DiscIntersect(rayOrigin, rayDirection, top, normal, innerRadius, out _) < 0f)
            {
                return distance;
            }
            if (DiscIntersect(rayOrigin, rayDirection, bottom, normal, innerRadius, out _) < 0f)
            {
                return distance;
            }
            return -1f;
        }

        public static float TriangleIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
                                              out Vector3 intersection, bool front = true, bool back = true)
        {
            // Find plane normal, intersection, and intersection distance.
            // We don't need to normalize this.
            var planeNormal = Vector3.Cross(vertex1 - vertex0, vertex2 - vertex0);

            // Find distance to intersection and intersection point.
            var distance = PlaneIntersect(rayOrigin, rayDirection, vertex0, planeNormal,
                                          out intersection, front, back);
            if (distance < 0f)
            {
                return -1f; // Triangle is behind the ray.
            }

            // Perform inside-outside test. Dot product is less than 0 if intersection lies outside of the edge.
            var edge0 = vertex1 - vertex0;
            var C0 = intersection - vertex0;
            if (Vector3.Dot(Vector3.Cross(edge0, C0), planeNormal) < 0f)
            {
                return -1f;
            }
            var edge1 = vertex2 - vertex1;
            var C1 = intersection - vertex1;
            if (Vector3.Dot(Vector3.Cross(edge1, C1), planeNormal) < 0f)
            {
                return -1f;
            }
            var edge2 = vertex0 - vertex2;
            var C2 = intersection - vertex2;
            if (Vector3.Dot(Vector3.Cross(edge2, C2), planeNormal) < 0f)
            {
                return -1f;
            }

            return distance;
        }

        // Assumes all vertices are on the same plane.
        public static float PolygonIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3[] vertices,
                                             out Vector3 intersection, bool front = true, bool back = true)
        {
            if (vertices.Length < 3)
            {
                throw new ArgumentException(nameof(vertices) + " length must be greater than or equal to 3", nameof(vertices));
            }

            // Find plane normal, intersection, and intersection distance.
            // We don't need to normalize this.
            var planeNormal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]);

            // Find distance to intersection and intersection point.
            var distance = PlaneIntersect(rayOrigin, rayDirection, vertices[0], planeNormal,
                                          out intersection, front, back);
            if (distance < 0f)
            {
                return -1f; // Polygon is behind the ray.
            }

            // Perform inside-outside test. Dot product is less than 0 if intersection lies outside of the edge.
            var length = vertices.Length;
            var vertexLast = vertices[length - 1];
            for (var i = 0; i < length; i++)
            {
                var vertex = vertices[i];
                var edge = vertex - vertexLast;
                var Ci = intersection - vertexLast;
                if (Vector3.Dot(Vector3.Cross(edge, Ci), planeNormal) < 0f)
                {
                    return -1f;
                }
                vertexLast = vertex;
            }

            return distance;
        }

        public static void GetBoxMinMax(Vector3 center, Vector3 size, out Vector3 outMin, out Vector3 outMax, Matrix4? matrix = null)
        {
            var min = new Vector3(center.X - size.X, center.Y - size.Y, center.Z - size.Z);
            var max = new Vector3(center.X + size.X, center.Y + size.Y, center.Z + size.Z);
            if (matrix.HasValue)
            {
                var matrixValue = matrix.Value;
                Vector3.TransformPosition(ref min, ref matrixValue, out outMin);
                Vector3.TransformPosition(ref max, ref matrixValue, out outMax);
            }
            else
            {
                outMin = min;
                outMax = max;
            }
        }

        public static bool IsZero(this Vector2 v)
        {
            return (v.X == 0f && v.Y == 0f);
            // Or just: return v == Vector2.Zero;
        }

        public static bool IsZero(this Vector3 v)
        {
            return (v.X == 0f && v.Y == 0f && v.Z == 0f);
            // Or just: return v == Vector3.Zero;
        }

        public static Vector3 CalculateNormal(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
        {
            var cross = Vector3.Cross(vertex2 - vertex0, vertex1 - vertex0);
            if (!cross.IsZero())
            {
                cross.Normalize();
            }
            return cross;
        }

        // Shift the components of vector by axis amount.
        // When axis = 0 (X): vector is unchanged.
        // When axis = 1 (Y): vector.X -> Y, vector.Y -> Z, vector.Z -> X.
        // When axis = 2 (Z): vector.X -> Z, vector.Y -> X, vector.Z -> Y.
        public static Vector3 SwapAxes(int axis, Vector3 vector)
        {
            switch (axis)
            {
                case 0: return vector;
                case 1: return new Vector3(vector.Z, vector.X, vector.Y);
                case 2: return new Vector3(vector.Y, vector.Z, vector.X);
            }
            throw new IndexOutOfRangeException(nameof(axis) + " must be between 0 and 2");
        }

        public static Vector3 SwapAxes(int axis, float x, float y, float z)
        {
            switch (axis)
            {
                case 0: return new Vector3(x, y, z);
                case 1: return new Vector3(z, x, y);
                case 2: return new Vector3(y, z, x);
            }
            throw new IndexOutOfRangeException(nameof(axis) + " must be between 0 and 2");
        }

        // Double is used for precision, since Math.Round already returns a double.
        public static float Snap(float x, double step)
        {
            return (float)(Math.Round(x / step) * step);
        }

        public static double Snap(double x, double step)
        {
            return (Math.Round(x / step) * step);
        }

        public static Vector3 Snap(Vector3 vector, double step)
        {
            return new Vector3(Snap(vector.X, step), Snap(vector.Y, step), Snap(vector.Z, step));
        }

        public static int PositiveModulus(int x, int m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        public static long PositiveModulus(long x, long m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        public static float PositiveModulus(float x, float m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        public static double PositiveModulus(double x, double m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        public static decimal PositiveModulus(decimal x, decimal m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        // Integer division where the result is always rounded down, instead of rounded towards zero.
        public static int FloorDiv(int x, int y)
        {
            // XOR to check if only one value is negative, modulus to avoid cases where the result is alread floored.
            if (((x < 0) ^ (y < 0)) && (x % y != 0))
            {
                return (x / y) - 1;
            }
            else
            {
                return x / y;
            }
        }

        public static long FloorDiv(long x, long y)
        {
            // XOR to check if only one value is negative, modulus to avoid cases where the result is alread floored.
            if (((x < 0) ^ (y < 0)) && (x % y != 0))
            {
                return (x / y) - 1;
            }
            else
            {
                return x / y;
            }
        }

        // Combination of FloorDiv and PositiveModulus. result is the remainder.
        public static int FloorDivRem(int x, int y, out int result)
        {
            var div = Math.DivRem(x, y, out result);
            // XOR to check if only one value is negative, non-zero to avoid cases where the result is alread floored.
            if (((x < 0) ^ (y < 0)) && result != 0)
            {
                result += y;
                div--;
            }
            return div;
        }

        public static long FloorDivRem(long x, long y, out long result)
        {
            var div = Math.DivRem(x, y, out result);
            // XOR to check if only one value is negative, non-zero to avoid cases where the result is alread floored.
            if (((x < 0) ^ (y < 0)) && result != 0)
            {
                result += y;
                div--;
            }
            return div;
        }

        // Yes, OpenTK.MathHelper.Clamp exists, but only for int, float, and double.
        // Using it when not expecting it to be missing other types is dangerous.
        // Like if using long with MathHelper.Clamp, YOU'LL GET A FLOAT OF ALL THINGS!!!
        // Clamp should not be used when a preference is needed between favoring min or max when min > max.
        public static int Clamp(int n, int min, int max)
        {
            return Math.Max(Math.Min(n, max), min);
        }

        public static uint Clamp(uint n, uint min, uint max)
        {
            return Math.Max(Math.Min(n, max), min);
        }

        public static long Clamp(long n, long min, long max)
        {
            return Math.Max(Math.Min(n, max), min);
        }

        public static ulong Clamp(ulong n, ulong min, ulong max)
        {
            return Math.Max(Math.Min(n, max), min);
        }

        public static float Clamp(float n, float min, float max)
        {
            return Math.Max(Math.Min(n, max), min);
        }

        public static double Clamp(double n, double min, double max)
        {
            return Math.Max(Math.Min(n, max), min);
        }

        public static decimal Clamp(decimal n, decimal min, decimal max)
        {
            return Math.Max(Math.Min(n, max), min);
        }

        public static int RoundUpToPower(int n, int power)
        {
            Debug.Assert(power >= 2, "RoundUpToPower must have power greater than or equal to 2");
            if (n == 0)
            {
                return 0;
            }
            var value = 1;
            var nAbs = Math.Abs(n);
            while (value < nAbs) value *= power;
            return value * Math.Sign(n);
        }

        public static uint RoundUpToPower(uint n, uint power)
        {
            Debug.Assert(power >= 2, "RoundUpToPower must have power greater than or equal to 2");
            if (n == 0)
            {
                return 0;
            }
            uint value = 1;
            while (value < n) value *= power;
            return value;
        }

        public static long RoundUpToPower(long n, long power)
        {
            Debug.Assert(power >= 2, "RoundUpToPower must have power greater than or equal to 2");
            if (n == 0)
            {
                return 0;
            }
            long value = 1;
            var nAbs = Math.Abs(n);
            while (value < nAbs) value *= power;
            return value * Math.Sign(n);
        }

        public static ulong RoundUpToPower(ulong n, ulong power)
        {
            Debug.Assert(power >= 2, "RoundUpToPower must have power greater than or equal to 2");
            if (n == 0)
            {
                return 0;
            }
            ulong value = 1;
            while (value < n) value *= power;
            return value;
        }

        public static float ConvertFixed12(int value) => value / Fixed12Scalar;

        public static float ConvertFixed16(int value) => value / Fixed16Scalar;

        public static float ConvertUV(uint uvComponent) => uvComponent / UVScalar;

        public static Vector2 ConvertUV(uint u, uint v)
        {
            var scalar = UVScalar;
            return new Vector2(u / scalar, v / scalar);
        }

        public static float InterpolateValue(float src, float dst, float delta)
        {
            // Uncomment if we want clamping. Or add bool clamp as an optional parameter.
            //if (delta <= 0f) return src;
            //if (delta >= 1f) return dst;
            return (src * (1f - delta)) + (dst * (delta));
        }

        // Just use Vector3.Lerp instead.
        /*public static Vector3 InterpolateVector(Vector3 src, Vector3 dst, float delta)
        {
            // Uncomment if we want clamping. Or add bool clamp as an optional parameter.
            //if (delta <= 0f) return src;
            //if (delta >= 1f) return dst;
            //return (src * (1f - delta)) + (dst * (delta));
            return Vector3.Lerp(src, dst, delta);
        }*/

        public static Vector3 InterpolateBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float delta)
        {
            // OpenTK's BezierCurve is only for Vector2, so we need to implement it ourselves.
            float temp;
            var r = Vector3.Zero;

            // Note: Power raised to the 0 is always 1, so we can skip those calculations.

            temp = (float)MathHelper.BinomialCoefficient(4 - 1, 0) *
                /*(float)Math.Pow(delta, 0) * */ (float)Math.Pow(1f - delta, 3);
            r += temp * p0;

            temp = (float)MathHelper.BinomialCoefficient(4 - 1, 1) *
                (float)Math.Pow(delta, 1) * (float)Math.Pow(1f - delta, 2);
            r += temp * p1;

            temp = (float)MathHelper.BinomialCoefficient(4 - 1, 2) *
                (float)Math.Pow(delta, 2) * (float)Math.Pow(1f - delta, 1);
            r += temp * p2;

            temp = (float)MathHelper.BinomialCoefficient(4 - 1, 3) *
                (float)Math.Pow(delta, 3) /* * (float)Math.Pow(1f - delta, 0)*/;
            r += temp * p3;

            return r;
        }

        public static Vector3 InterpolateBSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float delta)
        {
            // This is heavily simplified from other examples by removing knots and weights,
            // so there may be some mistakes here...

            const int NUM_POINTS = 4;
            const int DEGREE = 3;// NUM_POINTS - 1;
            const int start = DEGREE + 1; // Only constant when DEGREE == NUM_POINTS - 1

            delta = GeomMath.Clamp(delta, 0f, 1f); // Clamp delta
            var t = delta * (NUM_POINTS - DEGREE) + DEGREE;

            // This should always be 4 (3 + 1).
            //var start = GeomMath.Clamp((int)t, DEGREE, NUM_POINTS - 1) + 1;

            var points = new[] { p0, p1, p2, p3 };

            for (var lvl = 0; lvl < DEGREE; lvl++)
            {
                for (var i = start - DEGREE + lvl; i < start; i++)
                {
                    //var alpha = (t - i) / ((i + DEGREE - lvl) - i);
                    var alpha = (t - i) / (DEGREE - lvl); // simplified

                    var p = i - lvl;
                    points[p - 1] = Vector3.Lerp(points[p - 1], points[p], alpha);
                }
            }
            return points[start - DEGREE - 1];
        }
    }
}