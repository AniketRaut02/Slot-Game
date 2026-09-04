namespace SlotGame.Core
{
    // ditching scattered bool flags (like isSpinning) for a clean state machine
    // this acts as the single source of truth so we don't get double-spin bugs from impatient players
    public enum GameState
    {
        Idle,
        Spinning,
        Evaluating,
        PresentingWin
    }
}