using UnityEngine;
using SlotGame.Data;

namespace SlotGame.Economy
{
    public class BetController : MonoBehaviour
    {
        [SerializeField] private SlotMachineConfigSO config;

        // holding a reference to the pure C# wallet to pipe inputs into it
        private PlayerWallet wallet;

        public void Initialize(PlayerWallet activeWallet)
        {
            wallet = activeWallet;
            wallet.SetBet(config.DefaultBet);
        }

        public void IncreaseBet()
        {
            if (wallet == null) return;

            // clamped so they cant bet the house beyond config limits
            float newBet = Mathf.Min(wallet.CurrentBet + 10f, config.MaxBet);
            wallet.SetBet(newBet);
        }

        public void DecreaseBet()
        {
            if (wallet == null) return;

            float newBet = Mathf.Max(wallet.CurrentBet - 10f, config.MinBet);
            wallet.SetBet(newBet);
        }
    }
}