namespace PSXPrev.Classes
{
    // Rotation order for Euler angles.
    public enum RotationOrder
    {
        None,
        XYZ, // zRot * yRot * xRot
        XZY, // yRot * zRot * xRot
        YXZ, // zRot * xRot * yRot
        YZX, // xRot * zRot * yRot
        ZXY, // yRot * xRot * zRot
        ZYX, // xRot * yRot * zRot
    }
}
