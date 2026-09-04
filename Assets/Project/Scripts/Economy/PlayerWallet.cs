using System;

namespace SlotGame.Economy
{
    public class PlayerWallet
    {
        public float Balance { get; private set; }
        public float CurrentBet { get; private set; }

        // keeping state changes hidden and broadcasting them out so UI can just listen
        public event Action<float> OnBalanceChanged;
        public event Action<float> OnBetChanged;

        public PlayerWallet(float initialBalance, float defaultBet)
        {
            Balance = initialBalance;
            CurrentBet = defaultBet;
        }

        public bool TryDeductBet()
        {
            if (Balance < CurrentBet) return false;

            Balance -= CurrentBet;
            OnBalanceChanged?.Invoke(Balance);
            return true;
        }

        public void AddWin(float amount)
        {
            Balance += amount;
            OnBalanceChanged?.Invoke(Balance);
        }

        public void SetBet(float newBet)
        {
            CurrentBet = newBet;
            OnBetChanged?.Invoke(CurrentBet);
        }
    }
}