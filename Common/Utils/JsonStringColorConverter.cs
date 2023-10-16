using System;
using System.Drawing;
using System.Globalization;
using Newtonsoft.Json;

namespace PSXPrev.Common.Utils
{
    internal class JsonStringColorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Color color)
            {
                var str = $"#{color.R:X02}{color.G:X02}{color.B:X02}";
                if (color.A != 255)
                {
                    str += $"{color.A:X02}";
                }
                writer.WriteValue(str);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }
            else if (reader.Value is string str)
            {
                // Allow alpha channel, but ignore it.
                if (str.StartsWith("#") && str.Length == 7 || str.Length == 9)
                {
                    var channels = new byte[4];
                    channels[3] = 255;
                    for (var i = 0; i < ((str.Length - 1) / 2); i++)
                    {
                        var channelStr = str.Substring(1 + i * 2, 2);
                        if (!byte.TryParse(channelStr, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out channels[i]))
                        {
                            return existingValue; // Return invalid color
                            //throw new Exception("Invalid color string");
                        }
                    }
                    return Color.FromArgb(channels[3], channels[0], channels[1], channels[2]);
                }
            }
            return existingValue; // Return invalid color
            //throw new Exception("Invalid color string");
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override bool CanRead => true;

        public override bool CanWrite => true;
    }
}
