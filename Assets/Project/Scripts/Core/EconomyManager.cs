using UnityEngine;
using SlotGame.Core;
using SlotGame.Events;
using SlotGame.UI;

namespace SlotGame.Economy
{
    public class EconomyManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HUDController hudController;
        [SerializeField] private BetController betController;
        [SerializeField] private WinPopupView winPopup;

        [Header("Events")]
        [SerializeField] private WinResultEventChannelSO winEvaluatedEvent;

        private PlayerWallet wallet;
        private const float STARTING_BALANCE = 1000f;
        private const float DEFAULT_BET = 10f;

        private void Awake()
        {
            // spin up our pure C# wallet
            wallet = new PlayerWallet(STARTING_BALANCE, DEFAULT_BET);

            hudController.Initialize(wallet);
            betController.Initialize(wallet);
        }

        private void OnEnable()
        {
            if (winEvaluatedEvent != null)
            {
                winEvaluatedEvent.OnEventRaised += HandleWinEvaluated;
            }
        }

        private void OnDisable()
        {
            if (winEvaluatedEvent != null)
            {
                winEvaluatedEvent.OnEventRaised -= HandleWinEvaluated;
            }
        }

        private void HandleWinEvaluated(WinResult result)
        {
            if (result.isWin)
            {
                float payout = PayoutCalculator.CalculatePayout(result, wallet.CurrentBet);
                wallet.AddWin(payout);
                winPopup.ShowWin(payout);
            }

            // spin is totally over, re-enable UI inputs
            hudController.SetInteractable(true);
        }
    }
}