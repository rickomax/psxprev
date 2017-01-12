using OpenTK;

namespace PSXPrev.Classes.BoundingBox
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

        public Vector3 Min;
        public Vector3 Max;

        public Vector3 Center
        {
            get
            {
                return new Vector3(Min.X + Max.X * 0.5f, Min.Y + Max.Y * 0.5f, Min.Z + Max.Z * 0.5f);
            }
        }

        public void AddPoint(Vector3 point)
        {
            if (point.X < Min.X)
            {
                Min.X = point.X;
            }
            else if (point.X > Max.X)
            {
                Max.X = point.X;
            }
            if (point.Y < Min.Y)
            {
                Min.Y = point.Y;
            }
            else if (point.Y > Max.Y)
            {
                Max.Y = point.Y;
            }
            if (point.Z < Min.Z)
            {
                Min.Z = point.Z;
            }
            else if (point.Z > Max.Z)
            {
                Max.Z = point.Z;
            }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}) - ({3}, {4}, {5})", Min.X, Min.Y, Min.Z, Max.X, Max.Y, Max.Z);
        }
    }
}
