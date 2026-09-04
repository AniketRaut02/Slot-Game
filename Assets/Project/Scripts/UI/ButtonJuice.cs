using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace SlotGame.UI
{
    public class ButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float punchScale = 0.9f;
        [SerializeField] private float punchDuration = 0.1f;

        private Vector3 originalScale;
        private Coroutine scaleCoroutine;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ScaleTo(originalScale * punchScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ScaleTo(originalScale);
        }

        private void ScaleTo(Vector3 target)
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(ScaleRoutine(target));
        }

        private IEnumerator ScaleRoutine(Vector3 target)
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < punchDuration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, target, elapsed / punchDuration);
                yield return null;
            }

            transform.localScale = target;
        }
    }
}