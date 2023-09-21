namespace PSXPrev.Common.Renderer
{
    public enum GizmoType
    {
        None,
        Translate,
        Rotate,
        Scale,
    }

    public enum GizmoId
    {
        None,
        AxisX,
        AxisY,
        AxisZ,
        Uniform, // For Scale only
    }

    public enum EntitySelectionMode
    {
        None,
        Bounds,
        Triangle,
    }
}
