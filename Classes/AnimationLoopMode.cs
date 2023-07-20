namespace PSXPrev.Classes
{
    public enum AnimationLoopMode
    {
        Once,         // Animation stops after reaching the end.
        Loop,         // Animation repeats, but allows objects with unsynced frame counts to continue at their own pace.
        UnsyncedLoop, // Animation repeats, and resets even if objects have unsynced frame counts.
        MirrorOnce,   // Animation plays forwards and backwards, and then stops.
        MirrorLoop,   // Animation plays forwards and backwards, and then repeats.
    }

    public static class AnimationLoopModeExtensions
    {
        public static bool IsLooping(this AnimationLoopMode loopMode)
        {
            return loopMode != AnimationLoopMode.Once && loopMode != AnimationLoopMode.MirrorOnce;
        }

        public static bool IsMirrored(this AnimationLoopMode loopMode)
        {
            return loopMode == AnimationLoopMode.MirrorOnce || loopMode == AnimationLoopMode.MirrorLoop;
        }
    }
}
