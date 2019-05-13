using System;
using System.Text;
using OpenTK;

namespace PSXPrev
{
    public static class GeomUtils
    {
        public const string FloatFormat = "0.00000";
        public const string IntegerFormat = "0";
        public static string CompleteFloatFormat = "{0:0.00000}";
        public const float Deg2Rad = (float)((Math.PI * 2f) / 360.0f);

        public static Vector3 XVector = new Vector3(1f, 0f, 0f);
        public static Vector3 YVector = new Vector3(0f, 1f, 0f);
        public static Vector3 ZVector = new Vector3(0f, 0f, 1f);

        public static float VecDistance(Vector3 a, Vector3 b)
        {
            var x = a.X - b.X;
            var y = a.Y - b.Y;
            var z = a.Z - b.Z;
            return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
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

        public static Matrix4 CreateS(float scale)
        {
            var mat = Matrix4.CreateScale(scale);
            return mat;
        }

        public static Matrix4 CreateR(Vector3 rotation)
        {
            var xRot = Matrix4.CreateRotationX(rotation.X);
            var yRot = Matrix4.CreateRotationY(rotation.Y);
            var zRot = Matrix4.CreateRotationZ(rotation.Z);
            return xRot * yRot * zRot;
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
            if (vec.W > 0.000001f || vec.W < -0.000001f)
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
            if (num < Double.Epsilon)
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
            var prod3 = prod1 / prod2;
            return rayOrigin - rayDirection * prod3;
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
    }
}