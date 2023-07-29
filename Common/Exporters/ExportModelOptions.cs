namespace PSXPrev.Common.Exporters
{
    public class ExportModelOptions
    {
        public bool MergeEntities { get; set; }
        public bool AttachLimbs { get; set; } = true;

        public bool ExportTextures { get; set; } = true;
        public bool ShareTextures { get; set; } = true; // Share exported textures between all models
        public bool TiledTextures { get; set; } = true;
        public bool RedrawTextures { get; set; } // Redraw textures owned by models to VRAM pages before export (HMD)
        public bool SingleTexture { get; set; } // Combine all textures into a single image

        public bool ExperimentalOBJVertexColor { get; set; } = true;

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
