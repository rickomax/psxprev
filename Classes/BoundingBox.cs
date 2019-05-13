using OpenTK;

namespace PSXPrev
{
    public class BoundingBox
    {
        private readonly Vector3[] _corners = new Vector3[8];
        public Vector3[] Corners
        {
            get
            {
                _corners[0] = new Vector3(Min.X, Min.Y, Min.Z);
                _corners[1] = new Vector3(Max.X, Min.Y, Min.Z);
                _corners[2] = new Vector3(Min.X, Max.Y, Min.Z);
                _corners[3] = new Vector3(Min.X, Min.Y, Max.Z);
                _corners[4] = new Vector3(Max.X, Max.Y, Min.Z);
                _corners[5] = new Vector3(Max.X, Min.Y, Max.Z);
                _corners[6] = new Vector3(Min.X, Max.Y, Max.Z);
                _corners[7] = new Vector3(Max.X, Max.Y, Max.Z);
                return _corners;
            }
        }

        public Vector3 Center => Vector3.Lerp(Min, Max, 0.5f);

        public Vector3 Extents => Size * 0.5f;

        public Vector3 Size => Max - Min;

        public float Magnitude => Size.Length;

        public float MagnitudeFromCenter
        {
            get
            {
                var min = Min;
                var max = Max;
                GetMinMax(ref min, ref max, Vector3.Zero);
                return (max - min).Length;
            }
        }

        public Vector3 Min;

        public Vector3 Max;

        private bool _isSet;

        private void GetMinMax(ref Vector3 min, ref Vector3 max, Vector3 point)
        {
            if (point.X < min.X)
            {
                min.X = point.X;
            }
            else if (point.X > max.X)
            {
                max.X = point.X;
            }
            if (point.Y < min.Y)
            {
                min.Y = point.Y;
            }
            else if (point.Y > max.Y)
            {
                max.Y = point.Y;
            }
            if (point.Z < min.Z)
            {
                min.Z = point.Z;
            }
            else if (point.Z > max.Z)
            {
                max.Z = point.Z;
            }
        }

        public void AddPoints(Vector3[] points)
        {
            foreach (var point in points)
            {
                AddPoint(point);
            }
        }

        public void AddPoint(Vector3 point)
        {
            if (!_isSet)
            {
                Min = point;
                Max = point;
                _isSet = true;
            }
            else
            {
                GetMinMax(ref Min, ref Max, point);
            }

        }

        public override string ToString()
        {
            return $"({Min.X}, {Min.Y}, {Min.Z}) - ({Max.X}, {Max.Y}, {Max.Z})";
        }
    }
}
