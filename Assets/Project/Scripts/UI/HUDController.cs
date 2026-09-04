using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotGame.Events;
using SlotGame.Economy;

namespace SlotGame.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI betText;
        [SerializeField] private Button spinButton;
        [SerializeField] private Button increaseBetButton;
        [SerializeField] private Button decreaseBetButton;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO spinRequestedEvent;

        private PlayerWallet wallet;

        public void Initialize(PlayerWallet activeWallet)
        {
            wallet = activeWallet;

            wallet.OnBalanceChanged += UpdateBalanceUI;
            wallet.OnBetChanged += UpdateBetUI;

            UpdateBalanceUI(wallet.Balance);
            UpdateBetUI(wallet.CurrentBet);
        }

        private void OnEnable()
        {
            spinButton.onClick.AddListener(OnSpinClicked);
        }

        private void OnDisable()
        {
            spinButton.onClick.RemoveListener(OnSpinClicked);

            if (wallet != null)
            {
                wallet.OnBalanceChanged -= UpdateBalanceUI;
                wallet.OnBetChanged -= UpdateBetUI;
            }
        }

        private void OnSpinClicked()
        {
            if (wallet.TryDeductBet())
            {
                // kill the button so they dont double-click during a spin
                SetInteractable(false);
                spinRequestedEvent?.RaiseEvent();
            }
        }

        public void SetInteractable(bool state)
        {
            // only turn the spin button back on if they can actually afford it
            spinButton.interactable = state && (wallet.Balance >= wallet.CurrentBet);
            increaseBetButton.interactable = state;
            decreaseBetButton.interactable = state;
        }

        private void UpdateBalanceUI(float newBalance)
        {
            balanceText.text = $"Balance: ${newBalance}";
            if (spinButton.interactable && newBalance < wallet.CurrentBet)
            {
                spinButton.interactable = false;
            }
        }

        private void UpdateBetUI(float newBet)
        {
            betText.text = $"Bet: ${newBet}";
        }
    }
}