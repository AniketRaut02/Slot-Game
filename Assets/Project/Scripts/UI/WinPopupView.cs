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

            // animate the number from 0 to payout over a half second instead of an instant snap
            while (time < rollupDuration)
            {
                time += Time.deltaTime;
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