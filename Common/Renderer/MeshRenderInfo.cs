﻿using System;
using OpenTK;

namespace PSXPrev.Common.Renderer
{
    public class MeshRenderInfo
    {
        public static readonly Vector3 DefaultLightDirection = -Vector3.UnitZ;
        public static readonly Color DefaultAmbientColor = new Color(System.Drawing.Color.LightGray);

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
        public Color AmbientColor { get; set; } // Overrides Scene's or MeshBatch's ambient color
        public Color SolidColor { get; set; } // Overrides Mesh's vertex colors
        public bool Visible { get; set; } = true;

        public bool IsTextured => RenderFlags.HasFlag(RenderFlags.Textured);

        public bool IsOpaque => RenderInfo.IsOpaque(RenderFlags, MixtureRate, _alpha);

        public bool IsSemiTransparent => RenderInfo.IsSemiTransparent(RenderFlags, MixtureRate, _alpha);


        public MeshRenderInfo()
        {
        }

        public MeshRenderInfo(MeshRenderInfo fromRenderInfo)
        {
            CopyFrom(fromRenderInfo);
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
            Visible     = modelEntity.Visible;
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
            Visible = renderInfo.Visible;
        }
    }
}
