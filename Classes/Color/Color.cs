namespace PSXPrev.Classes.Color
{
    public class Color
    {
        public float R { get; set; }

        public float G { get; set; }

        public float B { get; set; }

        public override string ToString()
        {
            return R + "|" + G + "|" + B;
        }
    }
}