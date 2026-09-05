using UnityEngine;
using TMPro;
using System.Collections;

namespace SlotGame.UI
{
    public class WinPopupView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI winAmountText;
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private float rollupDuration = 0.5f;

        [Header("Audio (Optional)")]
        [SerializeField] private Audio.AudioManager audioManager;

        private void Awake()
        {
            popupPanel.SetActive(false);
        }

        public void ShowWin(float amount)
        {
            popupPanel.SetActive(true);
            StartCoroutine(RollupRoutine(amount));
        }

        private IEnumerator RollupRoutine(float targetAmount)
        {
            float time = 0f;
            float tickTimer = 0f;
            float tickInterval = 0.05f; // Play a sound every 50ms during roll-up

            while (time < rollupDuration)
            {
                time += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= tickInterval)
                {
                    tickTimer = 0f;
                    if (audioManager != null) audioManager.PlayCoinTick();
                }

                float currentVal = Mathf.Lerp(0f, targetAmount, time / rollupDuration);
                winAmountText.text = $"WIN: ${currentVal:F0}";
                yield return null;
            }

            winAmountText.text = $"WIN: ${targetAmount:F0}";

            yield return new WaitForSeconds(1.5f);
            popupPanel.SetActive(false);
        }


    }
}