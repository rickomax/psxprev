using System;
using Newtonsoft.Json;
using PSXPrev.Common.Utils;

namespace PSXPrev.Common.Exporters
{
    public enum ExportModelGrouping
    {
        Default,
        //SplitAllSubModels,
        SplitSubModelsByTMDID,
        GroupAllModels,
    }

    [JsonObject]
    public class ExportModelOptions : IEquatable<ExportModelOptions>
    {
        public const string DefaultFormat = OBJ;

        public const string OBJ = "OBJ";
        public const string PLY = "PLY";
        public const string DAE = "DAE";
        public const string GLTF2 = "glTF2";


        // Equality checking and optimization.
        [JsonIgnore]
        private string _equalityString;
        [JsonIgnore]
        private int _equalityStringHashCode;
        [JsonIgnore]
        private bool _isReadOnly;
        // Set to true to claim that fields will not be changed. Used to optimize equality checks.
        [JsonIgnore]
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                if (!_isReadOnly)
                {
                    _equalityString = null;
                    _equalityStringHashCode = 0;
                }
            }
        }

        // Optional display name to assign to history
        [JsonProperty("displayName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DisplayName { get; set; } = null;
        // Optionally prevent this from being removed from the history list
        [JsonProperty("bookmarked", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsBookmarked { get; set; } = false;


        // These settings are only present for loading and saving purposes.
        // File path:
        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        // Format:
        [JsonProperty("format")]
        public string Format { get; set; } = DefaultFormat;

        // Textures:
        [JsonProperty("exportTextures")]
        public bool ExportTextures { get; set; } = true;
        [JsonProperty("shareTextures")]
        public bool ShareTextures { get; set; } = true; // Share exported textures between all models
        [JsonProperty("tiledTextures")]
        public bool TiledTextures { get; set; } = true;
        [JsonProperty("redrawModelTextures")]
        public bool RedrawTextures { get; set; } = false; // Redraw textures owned by models to VRAM pages before export (HMD, BFF, PIL, PSX)
        [JsonProperty("singleTexture")]
        public bool SingleTexture { get; set; } = false; // Combine all textures into a single image

        // Options:
        [JsonProperty("modelGrouping"), JsonConverter(typeof(JsonStringEnumIgnoreCaseConverter))]
        public ExportModelGrouping ModelGrouping { get; set; } = ExportModelGrouping.Default;
        [JsonProperty("attachLimbs")]
        public bool AttachLimbs { get; set; } = true;
        [JsonProperty("vertexIndexReuse")]
        public bool VertexIndexReuse { get; set; } = true;
        [JsonProperty("readableFormat")]
        public bool ReadableFormat { get; set; } = true;
        [JsonProperty("strictFloatFormat")]
        public bool StrictFloatFormat { get; set; } = true;
        [JsonProperty("experimentalOBJVertexColor")]
        public bool ExperimentalOBJVertexColor { get; set; } = true;

        // Animations:
        //[JsonIgnore]
        //public bool ExportTickedAnimations { get; set; }
        [JsonProperty("exportAnimations")]
        public bool ExportAnimations { get; set; }


        // Helpers
        [JsonIgnore]
        public string FloatFormat => StrictFloatFormat ? GeomMath.FloatFormat : "G";

        public string GetBaseName(int index, int? entityIndex = null)
        {
            // {baseName}{index}{"_{entityIndex}" | ""}
            var baseName = $"{Name}{index}";
            return entityIndex.HasValue ? $"{baseName}_{entityIndex}" : baseName;
        }

        public string GetTextureName(string baseName, int textureIndex)
        {
            // {"{Name}shared" | "{baseName}"}{"_{textureIndex}" | ""}
            var baseTextureName = ShareTextures ? $"{Name}shared" : baseName;
            return !SingleTexture ? $"{baseTextureName}_{textureIndex}" : baseTextureName;
        }


        public void Validate(string defaultName)
        {
            Name = string.IsNullOrWhiteSpace(Name) ? defaultName : Name.Trim();

            if (!ExportTextures)
            {
                ShareTextures = false;
                TiledTextures = false;
                RedrawTextures = false;
                SingleTexture = false;
            }
        }

        public ExportModelOptions Clone()
        {
            return (ExportModelOptions)MemberwiseClone();
        }

        // Normalize settings for equality checks
        private void Normalize()
        {
            DisplayName = null;
            IsBookmarked = false;

            Path = Path?.ToLower();
            Name = Name?.Trim()?.ToLower();
        }

        // It's easier to manage equality by *not* having to update it every time settings are changed.
        // This is a lazy solution, but requires near-zero maintenance.
        private string GetEqualityString(out int? hashCode)
        {
            string equalityString;
            hashCode = null;
            if (_equalityString != null)
            {
                equalityString = _equalityString;
                hashCode = _equalityStringHashCode;
            }
            else
            {
                var options = Clone();
                options.Normalize();

                // Use settings to reduce the size of the string
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                };
                equalityString = JsonConvert.SerializeObject(options, jsonSettings);// Formatting.None);

                if (_isReadOnly)
                {
                    hashCode = equalityString.GetHashCode();
                    _equalityString = equalityString;
                    _equalityStringHashCode = hashCode.Value;
                }
            }
            return equalityString;
        }

        public bool Equals(ExportModelOptions other)
        {
            var str      = GetEqualityString(out var hashCode);
            var strOther = other.GetEqualityString(out var hashCodeOther);
            // If both equalities are cached, then we can compare hash codes first to save time
            return (!hashCode.HasValue || !hashCodeOther.HasValue || hashCode == hashCodeOther) && str == strOther;
        }
    }
}
