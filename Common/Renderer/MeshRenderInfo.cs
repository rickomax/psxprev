using OpenTK;

namespace PSXPrev.Common.Renderer
{
    public class MeshRenderInfo
    {
        public uint TexturePage { get; set; }
        public RenderFlags RenderFlags { get; set; }
        public MixtureRate MixtureRate { get; set; }
        private float _alpha = 1f;
        public float Alpha
        {
            get => _alpha;
            set => _alpha = MathHelper.Clamp(value, 0f, 1f);
        }
        public float Thickness { get; set; } = 1f;
        public Color SolidColor { get; set; }
        public bool Visible { get; set; } = true;

        public bool IsTextured => RenderFlags.HasFlag(RenderFlags.Textured);

        public bool IsOpaque => RenderInfo.IsOpaque(RenderFlags, MixtureRate, Alpha);

        public bool IsSemiTransparent => RenderInfo.IsSemiTransparent(RenderFlags, MixtureRate, Alpha);
    }
}
