using System;
using System.Text;
using OpenTK;
using PSXPrev.Classes;

namespace PSXPrev
{
    public static class GeomUtils
    {
        public const string FloatFormat = "0.00000";
        public const string IntegerFormat = "0";
        public static string CompleteFloatFormat = "{0:0.00000}";

        public const float Deg2Rad = (float)((Math.PI * 2f) / 360.0f);

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

        //public static Matrix4 CreateTRS(float s, Vector3 t, Vector3 r)
        //{
            //var mat = glm.translate(new Matrix4(s), new Vector3(t.x, t.y, t.z)) *
            //          CreateR(r);
        //    return Matrix4.Identity;
        //}

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

        //public static Matrix4 CreateTRS(Vector3 translation, Vector3 rotation, Vector3 scale)
        //{
        //    var s = glm.scale(Matrix4.identity(), new Vector3(1f,1f,1f));
        //    var r = CreateR(rotation);
        //    var t = glm.translate(Matrix4.identity(), translation);
        //    return r * t * s ;
        //}
    }
}