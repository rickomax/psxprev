using Newtonsoft.Json;

namespace PSXPrev.Common.Exporters
{
    [JsonObject]
    public class ExportModelOptions
    {
        public const string DefaultFormat = OBJ;

        public const string OBJ = "OBJ";
        public const string PLY = "PLY";
        public const string GLTF2 = "glTF2";

        // These settings are only present for loading and saving purposes.
        // File path:
        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;
        // Format:
        [JsonProperty("format")]
        public string Format { get; set; } = OBJ;

        // Textures:
        [JsonProperty("exportTextures")]
        public bool ExportTextures { get; set; } = true;
        [JsonProperty("shareTextures")]
        public bool ShareTextures { get; set; } = true; // Share exported textures between all models
        [JsonProperty("tiledTextures")]
        public bool TiledTextures { get; set; } = true;
        [JsonProperty("redrawModelTextures")]
        public bool RedrawTextures { get; set; } = false; // Redraw textures owned by models to VRAM pages before export (HMD)
        [JsonProperty("singleTexture")]
        public bool SingleTexture { get; set; } = false; // Combine all textures into a single image

        // Options:
        [JsonProperty("mergeModels")]
        public bool MergeEntities { get; set; } = false;
        [JsonProperty("attachLimbs")]
        public bool AttachLimbs { get; set; } = true;
        [JsonProperty("experimentalOBJVertexColor")]
        public bool ExperimentalOBJVertexColor { get; set; } = true;

        // Animations:
        [JsonIgnore]
        public bool ExportTickedAnimations { get; set; }
        [JsonProperty("exportAnimations")]
        public bool ExportAnimations { get; set; }


        public void Validate()
        {
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
    }
}
