using System;
using Newtonsoft.Json;

namespace PSXPrev.Common.Utils
{
    internal sealed class JsonStringEnumIgnoreCaseConverter : JsonStringEnumConverter
    {
        public JsonStringEnumIgnoreCaseConverter()
        {
            IgnoreCase = true;
        }
    }

    internal class JsonStringEnumConverter : JsonConverter
    {
        protected bool IgnoreCase { get; set; } = false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }
            else if (reader.Value is string str)
            {
                var enumType = Nullable.GetUnderlyingType(objectType) ?? objectType;
                var comparison = IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
                foreach (var name in Enum.GetNames(enumType))
                {
                    if (string.Equals(str, name, comparison))
                    {
                        return Enum.Parse(enumType, name);
                    }
                }
            }
            return existingValue;
            //throw new Exception("Unknown enum name");
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override bool CanRead => true;

        public override bool CanWrite => true;
    }
}
