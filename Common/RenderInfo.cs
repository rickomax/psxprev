using System;

namespace PSXPrev.Common
{
    [Flags]
    public enum RenderFlags
    {
        None = 0,

        DoubleSided       = (1 << 0),
        Unlit             = (1 << 1),
        SemiTransparent   = (1 << 3),
        Fog               = (1 << 4),
        Textured          = (1 << 5),
        // todo: Are these even render-related?
        Subdivision       = (1 << 6),
        AutomaticDivision = (1 << 7),

        // Also known as VibRibbon (only use Vertex0 and Vertex1).
        Line              = (1 << 16),
        // Always face the camera (assumes direction is (0, 0, -1), so X and Y components should be used for size).
        Sprite            = (1 << 17), // WIP

        // Not PlayStation render flags
        NoAmbient         = (1 << 28),

        // Bits 29-31 are reserved for MixtureRate.
    }

    // Blending when RenderFlags.SemiTransparent is set.
    public enum MixtureRate
    {
        None             = 0,
        Back50_Poly50    = 1, //  50% back +  50% poly
        Back100_Poly100  = 2, // 100% back + 100% poly
        Back100_PolyM100 = 3, // 100% back - 100% poly
        Back100_Poly25   = 4, // 100% back +  25% poly
        Alpha            = 5, // 1-A% back +   A% poly (not a PSX mixture rate! for use with 3D visuals)
    }
    
    // A named Tuple<uint, RenderFlags, MixtureRate> for render information used to separate models/meshes.
    public struct RenderInfo : IEquatable<RenderInfo>
    {
        // Use this mask when separating meshes by render info.
        public const RenderFlags SupportedFlags = RenderFlags.DoubleSided | RenderFlags.Unlit |
                                                  RenderFlags.SemiTransparent | RenderFlags.Textured |
                                                  RenderFlags.Line | RenderFlags.Sprite;

        public uint TexturePage { get; }
        public RenderFlags RenderFlags { get; }
        public MixtureRate MixtureRate { get; }
        
        // Helper property for getting hash codes and checking for equality.
        private ulong RawValue
        {
            get
            {
                return (((ulong)TexturePage <<  0) |
                        ((ulong)RenderFlags << 32) |
                        ((ulong)MixtureRate << 61));
            }
        }

        public RenderInfo(uint texturePage, RenderFlags renderFlags, MixtureRate mixtureRate = MixtureRate.None)
        {
            TexturePage = texturePage;
            RenderFlags = renderFlags & SupportedFlags; // Ignore flags that we can't use for now
            MixtureRate = mixtureRate;
        }

        public override int GetHashCode() => RawValue.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is RenderInfo other)
            {
                return Equals(other);
            }
            return base.Equals(obj);
        }

        public bool Equals(RenderInfo other)
        {
            return RawValue.Equals(other.RawValue);
        }


        public static bool IsOpaque(RenderFlags renderFlags, MixtureRate mixtureRate, float alpha = 1f)
        {
            if (!renderFlags.HasFlag(RenderFlags.SemiTransparent) || mixtureRate == MixtureRate.None)
            {
                return true;
            }
            else if (mixtureRate == MixtureRate.Alpha && alpha >= 1f)
            {
                return true;
            }
            return false;
        }

        public static bool IsSemiTransparent(RenderFlags renderFlags, MixtureRate mixtureRate, float alpha = 1f)
        {
            if (!renderFlags.HasFlag(RenderFlags.SemiTransparent) || mixtureRate == MixtureRate.None)
            {
                return false;
            }
            else if (mixtureRate == MixtureRate.Alpha && (alpha <= 0f || alpha >= 1f))
            {
                return false;
            }
            return true;
        }
    }
}
