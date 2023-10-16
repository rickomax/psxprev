using System;
using System.Drawing;
using OpenTK;

namespace PSXPrev.Common.Renderer
{
    public class MeshRenderInfo
    {
        public static readonly Vector3 DefaultLightDirection = -Vector3.UnitZ;
        public static readonly Color3 DefaultAmbientColor = new Color3(Color.LightGray);

        public uint TexturePage { get; set; }
        public RenderFlags RenderFlags { get; set; }
        public MixtureRate MixtureRate { get; set; }
        private float _alpha = 1f;
        public float Alpha
        {
            get => _alpha;
            set => _alpha = GeomMath.Clamp(value, 0f, 1f);
        }
        private float _thickness = 1f;
        public float Thickness
        {
            get => _thickness;
            set => _thickness = Math.Max(0f, value);
        }
        public Vector3? LightDirection { get; set; } // Overrides Scene's or MeshBatch's light direction
        public float? LightIntensity { get; set; } // Overrides Scene's or MeshBatch's light intensity
        public Color3? AmbientColor { get; set; } // Overrides Scene's or MeshBatch's ambient color
        public Color3? SolidColor { get; set; } // Overrides Mesh's vertex colors
        public Vector3 SpriteCenter { get; set; }
        public Vector2 TextureAnimation { get; set; } // Animation speed of texture in UV units per second
        public bool MissingTexture { get; set; }
        public bool Visible { get; set; } = true;


        public bool IsTextured => RenderFlags.HasFlag(RenderFlags.Textured);

        public bool IsSprite => RenderFlags.HasFlag(RenderFlags.Sprite) || RenderFlags.HasFlag(RenderFlags.SpriteNoPitch);

        public bool IsOpaque => RenderInfo.IsOpaque(RenderFlags, MixtureRate, _alpha);

        public bool IsSemiTransparent => RenderInfo.IsSemiTransparent(RenderFlags, MixtureRate, _alpha);


        public MeshRenderInfo()
        {
        }

        public MeshRenderInfo(MeshRenderInfo fromRenderInfo)
        {
            CopyFrom(fromRenderInfo);
        }

        public void CopyTo(ModelEntity modelEntity)
        {
            if (modelEntity.DebugMeshRenderInfo == null)
            {
                modelEntity.DebugMeshRenderInfo = new MeshRenderInfo();
            }
            modelEntity.DebugMeshRenderInfo.CopyFrom(this);

            // Always use the settings built into ModelEntity over MeshRenderInfo.
            modelEntity.TexturePage = TexturePage;
            modelEntity.RenderFlags = RenderFlags;
            modelEntity.MixtureRate = MixtureRate;
            modelEntity.SpriteCenter = SpriteCenter;
            modelEntity.TextureAnimation = TextureAnimation;
            //modelEntity.MissingTexture = MissingTexture; // Read-only property
            modelEntity.Visible = Visible;
        }

        public void CopyFrom(ModelEntity modelEntity)
        {
            if (modelEntity.DebugMeshRenderInfo != null)
            {
                CopyFrom(modelEntity.DebugMeshRenderInfo);
            }
            // Always use the settings built into ModelEntity over MeshRenderInfo.
            TexturePage = modelEntity.TexturePage;
            RenderFlags = modelEntity.RenderFlags;
            MixtureRate = modelEntity.MixtureRate;
            SpriteCenter = modelEntity.SpriteCenter;
            var uvConverter = modelEntity.TextureLookup;
            if (uvConverter != null)
            {
                TextureAnimation = uvConverter.ConvertUV(modelEntity.TextureAnimation, true);
            }
            else
            {
                TextureAnimation = modelEntity.TextureAnimation;
            }
            MissingTexture = modelEntity.MissingTexture;
            Visible = modelEntity.Visible;
        }

        public void CopyFrom(MeshRenderInfo renderInfo)
        {
            TexturePage = renderInfo.TexturePage;
            RenderFlags = renderInfo.RenderFlags;
            MixtureRate = renderInfo.MixtureRate;
            _alpha = renderInfo._alpha;
            _thickness = renderInfo._thickness;
            LightDirection = renderInfo.LightDirection;
            LightIntensity = renderInfo.LightIntensity;
            AmbientColor = renderInfo.AmbientColor;
            SolidColor = renderInfo.SolidColor;
            SpriteCenter = renderInfo.SpriteCenter;
            TextureAnimation = renderInfo.TextureAnimation;
            MissingTexture = renderInfo.MissingTexture;
            Visible = renderInfo.Visible;
        }
    }
}
