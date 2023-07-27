using System.Collections.Generic;
using OpenTK;

namespace PSXPrev.Common
{
    public class BoundingBox
    {
        public Vector3 Min;
        public Vector3 Max;
        public bool IsSet { get; private set; } // True if at least one point has been added.

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

        public float MagnitudeFromCenter => Size.Length * 0.5f;

        public float MagnitudeFromOrigin => MagnitudeFromPosition(Vector3.Zero);

        public float MagnitudeFromPosition(Vector3 position)
        {
            var min = Vector3.ComponentMin(Min, position);
            var max = Vector3.ComponentMax(Max, position);
            return (max - min).Length;
        }


        public BoundingBox()
        {
        }

        public BoundingBox(BoundingBox fromBoundingBox)
        {
            Min = fromBoundingBox.Min;
            Max = fromBoundingBox.Max;
            IsSet = fromBoundingBox.IsSet;
        }


        public void Reset()
        {
            Min = Max = Vector3.Zero;
            IsSet = false;
        }

        public void AddBounds(BoundingBox boundingBox)
        {
            if (boundingBox.IsSet)
            {
                // This works even if Min/Max have higher/lower values (due to direction modification of fields).
                //AddPoint(boundingBox.Min);
                //AddPoint(boundingBox.Max);
                // This does not, but it's faster. :)
                if (!IsSet)
                {
                    Min = boundingBox.Min;
                    Max = boundingBox.Max;
                    IsSet = true;
                }
                else
                {
                    Min = Vector3.ComponentMin(Min, boundingBox.Min);
                    Max = Vector3.ComponentMax(Max, boundingBox.Max);
                }
            }
        }

        public void AddPoints(IEnumerable<Vector3> points)
        {
            foreach (var point in points)
            {
                AddPoint(point);
            }
        }

        public void AddPoint(Vector3 point)
        {
            if (!IsSet)
            {
                Min = Max = point;
                IsSet = true;
            }
            else
            {
                Min = Vector3.ComponentMin(Min, point);
                Max = Vector3.ComponentMax(Max, point);
            }
        }

        public override string ToString()
        {
            return $"{Min} - {Max}";
        }
    }
}
