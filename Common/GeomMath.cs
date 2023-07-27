using System;
using System.Text;
using OpenTK;

namespace PSXPrev.Common
{
    public static class GeomUtils
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
            var stringBuilder = new StringBuilder();
            if (v != null)
            {
                var vr = (Vector3)v;
                stringBuilder.AppendFormat(CompleteFloatFormat, vr.X).Append(", ").AppendFormat(CompleteFloatFormat, vr.Y).Append(", ").AppendFormat(CompleteFloatFormat, vr.Z);
            }
            return stringBuilder.ToString();
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

        public static Matrix4 CreateT(Vector3 translation)
        {
            var mat = Matrix4.CreateTranslation(translation);
            return mat;
        }

        public static Matrix4 CreateS(Vector3 scale)
        {
            var mat = Matrix4.CreateScale(scale);
            return mat;
        }

        public static Matrix4 CreateR(Vector3 rotation)
        {
            // todo: Should this actually be RotationOrder.XYZ (zRot * yRot * xRot)?
            return CreateR(rotation, RotationOrder.ZYX); // RotationOrder.XYZ);
        }

        public static Matrix4 CreateR(Vector3 rotation, RotationOrder order)
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

        public static Vector3 UnProject(this Vector3 position, Matrix4 projection, Matrix4 view, float width, float height)
        {
            Vector4 vec;
            vec.X = 2.0f * position.X / width - 1;
            vec.Y = -(2.0f * position.Y / height - 1);
            vec.Z = position.Z;
            vec.W = 1.0f;
            var viewInv = Matrix4.Invert(view);
            var projInv = Matrix4.Invert(projection);
            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);
            if (Math.Abs(vec.W) > 0.000001f)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }
            return vec.Xyz;
        }

        public static Vector3 ProjectOnNormal(this Vector3 vector, Vector3 normal)
        {
            var num = Vector3.Dot(normal, normal);
            if (num < float.Epsilon)
            {
                return Vector3.Zero;
            }
            return normal * Vector3.Dot(vector, normal) / num;
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

        public static Vector3 PlaneIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 planeOrigin, Vector3 planeNormal)
        {
            var diff = rayOrigin - planeOrigin;
            var prod1 = Vector3.Dot(diff, planeNormal);
            var prod2 = Vector3.Dot(rayDirection, planeNormal);
            var intersectionDistance = -prod1 / prod2;
            return rayOrigin + rayDirection * intersectionDistance;
        }

        public static float TriangleIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, bool front = true, bool back = true)
        {
            // Find plane normal, intersection, and intersection distance.
            // We don't need to normalize this.
            var planeNormal = Vector3.Cross((vertex1 - vertex0), (vertex2 - vertex0));

            // Find distance to intersection.
            var diff = rayOrigin - vertex0;
            var prod1 = Vector3.Dot(diff, planeNormal);
            var prod2 = Vector3.Dot(rayDirection, planeNormal);
            if (Math.Abs(prod2) <= 0.000001f)
            {
                return -1f; // Ray and plane are parallel.
            }
            if ((!front && prod2 > 0f) || (!back && prod2 < 0f))
            {
                return -1f; // Ray intersects from a side we don't want to intersect with.
            }
            var intersectionDistance = -prod1 / prod2;
            if (intersectionDistance < 0f)
            {
                return -1f; // Triangle is behind the ray.
            }
            
            var planeIntersection = rayOrigin + rayDirection * intersectionDistance;


            // Perform inside-outside test. Dot product is less than 0 if planeIntersection lies outside of the edge.
            var edge0 = vertex1 - vertex0;
            var C0 = planeIntersection - vertex0;
            if (Vector3.Dot(Vector3.Cross(edge0, C0), planeNormal) < 0f)
            {
                return -1f;
            }
            var edge1 = vertex2 - vertex1;
            var C1 = planeIntersection - vertex1;
            if (Vector3.Dot(Vector3.Cross(edge1, C1), planeNormal) < 0f)
            {
                return -1f;
            }
            var edge2 = vertex0 - vertex2;
            var C2 = planeIntersection - vertex2;
            if (Vector3.Dot(Vector3.Cross(edge2, C2), planeNormal) < 0f)
            {
                return -1f;
            }

            return intersectionDistance;
        }

        public static void GetBoxMinMax(Vector3 center, Vector3 size, out Vector3 outMin, out Vector3 outMax, Matrix4? matrix = null)
        {
            var min = new Vector3(center.X - size.X, center.Y - size.Y, center.Z - size.Z);
            var max = new Vector3(center.X + size.X, center.Y + size.Y, center.Z + size.Z);
            if (matrix.HasValue)
            {
                outMin = Vector3.TransformPosition(min, matrix.Value);
                outMax = Vector3.TransformPosition(max, matrix.Value);
            }
            else
            {
                outMin = min;
                outMax = max;
            }
        }

        public static bool IsZero(this Vector3 v)
        {
            return (v.X == 0f && v.Y == 0f && v.Z == 0f);
            // Or just: return v == Vector3.Zero;
        }

        public static Vector3 CalculateNormal(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
        {
            var cross = Vector3.Cross(vertex1 - vertex0, vertex2 - vertex0);
            if (!cross.IsZero())
            {
                cross.Normalize();
            }
            return cross;
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

            delta = Math.Max(0f, Math.Min(1f, delta)); // Clamp delta
            var t = delta * (NUM_POINTS - DEGREE) + DEGREE;

            // This should always be 4 (3 + 1).
            //var start = Math.Max(DEGREE, Math.Min(NUM_POINTS - 1, (int)t)) + 1;

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