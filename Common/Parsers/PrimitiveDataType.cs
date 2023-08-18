namespace PSXPrev.Common.Parsers
{
    public enum PrimitiveDataType
    {
        // 1 byte:
        U0,
        U1,
        U2,
        U3,
        V0,
        V1,
        V2,
        V3,
        R0,
        R1,
        R2,
        R3,
        G0,
        G1,
        G2,
        G3,
        B0,
        B1,
        B2,
        B3,
        Mode,
        // 2 bytes:
        CBA,
        TSB,
        W,
        H,
        VERTEX0,
        VERTEX1,
        VERTEX2,
        VERTEX3,
        NORMAL0,
        NORMAL1,
        NORMAL2,
        NORMAL3,
        // 4 bytes:
        TILE,
        // padding:
        PAD1,
        PAD2,
    }
}