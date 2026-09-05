using UnityEngine;
using TMPro;
using System.Collections;
using SlotGame.Core;
using SlotGame.Events;

namespace SlotGame.Bonus
{
    public class FreeSpinController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int freeSpinsAwarded = 5;
        [SerializeField] private int scatterTriggerCount = 3;

        [Header("UI")]
        [SerializeField] private GameObject bonusOverlayPanel;
        [SerializeField] private TextMeshProUGUI spinsRemainingText;

        [Header("Events")]
        [SerializeField] private WinResultEventChannelSO winEvaluatedEvent;
        [SerializeField] private VoidEventChannelSO spinRequestedEvent;
        [SerializeField] private VoidEventChannelSO bonusTriggeredEvent;
        [SerializeField] private VoidEventChannelSO bonusEndedEvent;

        private int remainingSpins = 0;
        private bool isBonusActive = false;

        private void Awake()
        {
            bonusOverlayPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (winEvaluatedEvent != null)
                winEvaluatedEvent.OnEventRaised += HandleWinEvaluated;
        }

        private void OnDisable()
        {
            if (winEvaluatedEvent != null)
                winEvaluatedEvent.OnEventRaised -= HandleWinEvaluated;
        }

        private void HandleWinEvaluated(WinResult result)
        {
            if (isBonusActive)
            {
                remainingSpins--;
                UpdateUI();

                if (remainingSpins > 0)
                {
                    // Wait a moment for the win presentation to finish, then spin again
                    StartCoroutine(AutoSpinRoutine());
                }
                else
                {
                    EndBonus();
                }
            }
            else if (result.scatterCount >= scatterTriggerCount)
            {
                StartBonus();
            }
        }

        private void StartBonus()
        {
            isBonusActive = true;
            remainingSpins = freeSpinsAwarded;

            bonusOverlayPanel.SetActive(true);
            UpdateUI();

            bonusTriggeredEvent?.RaiseEvent();

            // Kick off the first free spin
            StartCoroutine(AutoSpinRoutine());
        }

        private void EndBonus()
        {
            isBonusActive = false;
            bonusOverlayPanel.SetActive(false);
            bonusEndedEvent?.RaiseEvent();
        }

        private void UpdateUI()
        {
            if (spinsRemainingText != null)
            {
                spinsRemainingText.text = $"FREE SPINS: {remainingSpins}";
            }
        }

        private IEnumerator AutoSpinRoutine()
        {
            // Give the player a second to breathe and see their last win/trigger before auto-spinning
            yield return new WaitForSeconds(2.0f);
            spinRequestedEvent?.RaiseEvent();
        }
    }
}