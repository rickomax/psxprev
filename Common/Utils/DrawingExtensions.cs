using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PSXPrev.Common.Utils
{
    public static class DrawingExtensions
    {
        // Needed to clone a bitmap while preserving its pixel format.
        public static Bitmap DeepClone(this Bitmap bitmap)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, bitmap);
                stream.Seek(0, SeekOrigin.Begin);
                return (Bitmap)formatter.Deserialize(stream);
            }
        }
    }
}
