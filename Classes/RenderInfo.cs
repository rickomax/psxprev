using System;

namespace PSXPrev.Classes
{
    [Flags]
    public enum RenderFlags
    {
        None = 0,

        DoubleSided       = (1 << 0),
        Unlit             = (1 << 1),
        SemiTransparent   = (1 << 3),
        Fog               = (1 << 4),
        // Are these even render-related?
        Subdivision       = (1 << 5),
        AutomaticDivision = (1 << 6),

        // Use this mask when separating meshes by render info.
        SupportedFlags = DoubleSided | Unlit | SemiTransparent,

        // Bits 30 and 31 are reserved for MixtureRate.
    }

    // Blending when RenderFlags.SemiTransparent is set.
    public enum MixtureRate
    {
        None,
        Back50_Poly50,    //  50% back +  50% poly
        Back100_Poly100,  // 100% back + 100% poly
        Back100_PolyM100, // 100% back - 100% poly
        Back100_Poly25,   // 100% back +  25% poly
    }
    
    // A named Tuple<uint, RenderFlags, MixtureRate> for render information used to separate models/meshes.
    public struct RenderInfo : IEquatable<RenderInfo>
    {
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
                        ((ulong)MixtureRate << 62));
            }
        }

        public RenderInfo(uint texturePage, RenderFlags renderFlags, MixtureRate mixtureRate = MixtureRate.None)
        {
            TexturePage = texturePage;
            RenderFlags = renderFlags & RenderFlags.SupportedFlags; // Ignore flags that we can't use for now
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
    }
}
