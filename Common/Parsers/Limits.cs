namespace PSXPrev.Common.Parsers
{
    public static class Limits
    {
        // Strictness settings
        public static bool IgnoreHMDVersion;
        public static bool IgnoreTIMVersion;
        public static bool IgnoreTMDVersion;

        // Sanity check values
        public static uint MaxANJoints = 512;
        public static uint MaxANFrames = 5000;

        public static ulong MaxHMDBlockCount = 1024;
        public static ulong MaxHMDCoordCount = 1024; // Same as BlockCount, because they're related
        public static ulong MaxHMDTypeCount = 1024;
        public static ulong MaxHMDDataSize = 20000;
        public static ulong MaxHMDDataCount = 5000;
        public static ulong MaxHMDPrimitiveChainLength = 512;
        public static ulong MaxHMDHeaderLength = 100;
        public static ulong MinHMDStripMeshLength = 1;
        public static ulong MaxHMDStripMeshLength = 1024;
        public static ulong MaxHMDAnimSequenceSize = 20000;
        public static ulong MaxHMDAnimSequenceCount = 1024;
        public static ulong MaxHMDAnimInstructions = ushort.MaxValue + 1; // Hard cap
        public static ulong MaxHMDAnimInterpolationTypes = 128; // Hard cap
        public static ulong MaxHMDMIMeKeys = 32;
        public static ulong MaxHMDMIMeOriginals = 100;
        public static ulong MaxHMDVertices = 5000;

        public static ulong MaxMODModels = 1000;
        public static ulong MaxMODVertices = 10000;
        public static ulong MaxMODFaces = 10000;

        public static ulong MaxPMDObjects = 4000;
        public static ulong MaxPMDPointers = 4000;
        public static ulong MaxPMDPackets = 4000;

        public static ulong MaxPSXObjectCount = 1024;

        public static ulong MaxTIMResolution = 1024;

        public static ulong MaxTMDVertices = 20000; // Also used for normals
        public static ulong MaxTMDPrimitives = 10000;
        public static ulong MaxTMDObjects = 10000;

        public static ulong MaxTODPackets = 10000;
        public static ulong MaxTODFrames = 10000;

        public static ulong MinVDFFrames = 3;
        public static ulong MaxVDFFrames = 512;
        public static ulong MaxVDFVertices = 1024;
        public static ulong MaxVDFObjects = 512;
    }
}
