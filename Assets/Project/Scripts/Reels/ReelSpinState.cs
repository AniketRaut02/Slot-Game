namespace SlotGame.Reels
{
    // ditching string checks for state, using an enum so we don't get silent typo bugs
    public enum ReelSpinState
    {
        Idle,
        Accelerating,
        ConstantSpeed,
        Decelerating,
        Snapped
    }
}