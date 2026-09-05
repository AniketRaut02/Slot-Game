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
        [SerializeField] private VoidEventChannelSO bonusTriggeredEvent;
        [SerializeField] private VoidEventChannelSO bonusEndedEvent;

        private PlayerWallet wallet;
        private bool isBonusActive = false;

        private const float STARTING_BALANCE = 1000f;
        private const float DEFAULT_BET = 10f;

        private void Awake()
        {
            wallet = new PlayerWallet(STARTING_BALANCE, DEFAULT_BET);

            hudController.Initialize(wallet);
            betController.Initialize(wallet);
        }

        private void OnEnable()
        {
            if (winEvaluatedEvent != null) winEvaluatedEvent.OnEventRaised += HandleWinEvaluated;
            if (bonusTriggeredEvent != null) bonusTriggeredEvent.OnEventRaised += HandleBonusTriggered;
            if (bonusEndedEvent != null) bonusEndedEvent.OnEventRaised += HandleBonusEnded;
        }

        private void OnDisable()
        {
            if (winEvaluatedEvent != null) winEvaluatedEvent.OnEventRaised -= HandleWinEvaluated;
            if (bonusTriggeredEvent != null) bonusTriggeredEvent.OnEventRaised -= HandleBonusTriggered;
            if (bonusEndedEvent != null) bonusEndedEvent.OnEventRaised -= HandleBonusEnded;
        }

        private void HandleWinEvaluated(WinResult result)
        {
            if (result.isWin)
            {
                float payout = PayoutCalculator.CalculatePayout(result, wallet.CurrentBet);
                wallet.AddWin(payout);
                winPopup.ShowWin(payout);
            }

            // Only unlock the UI if we aren't being hijacked by the FreeSpinController
            if (!isBonusActive)
            {
                hudController.SetInteractable(true);
            }
        }

        private void HandleBonusTriggered()
        {
            isBonusActive = true;
            wallet.IsFreeSpinning = true;

            // Hard lock the UI for the duration of the bonus
            hudController.SetInteractable(false);
        }

        private void HandleBonusEnded()
        {
            isBonusActive = false;
            wallet.IsFreeSpinning = false;

            // Bonus is over, give control back to the player
            hudController.SetInteractable(true);
        }
    }
}