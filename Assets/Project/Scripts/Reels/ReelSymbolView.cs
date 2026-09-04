using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SlotGame.Data;

namespace SlotGame.Reels
{
    public class ReelSymbolView : MonoBehaviour
    {
        [SerializeField] private Image symbolImage;
        [SerializeField] private Image glowImage; // Optional: add a child image with a soft glow sprite

        private SymbolDefinitionSO currentSymbol;
        private Vector3 defaultScale;
        private Coroutine activeAnimation;

        public SymbolDefinitionSO CurrentSymbol => currentSymbol;

        private void Awake()
        {
            defaultScale = transform.localScale;
            if (glowImage != null) glowImage.enabled = false;
        }

        public void SetSymbol(SymbolDefinitionSO symbol)
        {
            currentSymbol = symbol;
            if (symbol != null && symbolImage != null)
            {
                symbolImage.sprite = symbol.DisplaySprite;
            }
        }

        public void PlaySquash()
        {
            if (activeAnimation != null) StopCoroutine(activeAnimation);
            activeAnimation = StartCoroutine(SquashRoutine());
        }

        public void PlayWinPulse()
        {
            if (activeAnimation != null) StopCoroutine(activeAnimation);
            activeAnimation = StartCoroutine(WinPulseRoutine());
        }

        public void ResetView()
        {
            if (activeAnimation != null) StopCoroutine(activeAnimation);
            transform.localScale = defaultScale;
            if (glowImage != null) glowImage.enabled = false;
        }

        private IEnumerator SquashRoutine()
        {
            // quick flat squash, then pop back to normal
            float duration = 0.15f;
            Vector3 squashed = new Vector3(defaultScale.x * 1.2f, defaultScale.y * 0.8f, defaultScale.z);

            yield return LerpScale(defaultScale, squashed, duration / 2f);
            yield return LerpScale(squashed, defaultScale, duration / 2f);
        }

        private IEnumerator WinPulseRoutine()
        {
            if (glowImage != null) glowImage.enabled = true;

            // continuous breathing effect until reset
            Vector3 enlarged = defaultScale * 1.15f;
            while (true)
            {
                yield return LerpScale(defaultScale, enlarged, 0.4f);
                yield return LerpScale(enlarged, defaultScale, 0.4f);
            }
        }

        private IEnumerator LerpScale(Vector3 from, Vector3 to, float time)
        {
            float elapsed = 0f;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(from, to, elapsed / time);
                yield return null;
            }
            transform.localScale = to;
        }
    }
}