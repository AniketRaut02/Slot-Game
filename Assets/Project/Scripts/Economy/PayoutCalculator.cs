using SlotGame.Core;

namespace SlotGame.Economy
{
    public static class PayoutCalculator
    {
        public static float CalculatePayout(WinResult result, float currentBet)
        {
            if (!result.isWin) return 0f;
            return currentBet * result.payoutMultiplier;
        }
    }
}