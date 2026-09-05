using System;
using System.Collections.Generic;
using UnityEngine;
using SlotGame.Data;

namespace SlotGame.Reels
{
    public class ReelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform symbolsContainer;
        [SerializeField] private List<ReelSymbolView> activeSymbols;

        [Header("Spin Settings")]
        [SerializeField] private float maxSpinSpeed = 2500f;
        [SerializeField] private float symbolHeight = 250f; // distance between each pooled symbol

        [Tooltip("Ease-out curve to make the stop feel natural, not linear")]
        [SerializeField] private AnimationCurve decelerationCurve;

        [Header("Visual Replacements")]
        [SerializeField] private List<SymbolDefinitionSO> fallbackSymbols;


        public ReelSymbolView CenterView { get; private set; }
        public System.Action OnReelImpact;

        private ReelSpinState currentState = ReelSpinState.Idle;
        private float currentSpeed;
        private float stateTimer;
        private float spinDelay;
        private float totalDuration;
        private SymbolDefinitionSO targetSymbol;

        private ReelSymbolView injectedTargetView;

        // clean event for the orchestrator to listen to, no tight coupling here
        public Action<ReelController, SymbolDefinitionSO> OnReelSnapped;

        // the only public method. notice how it doesn't ask the RNG for anything.
        public void SpinToward(SymbolDefinitionSO target, float duration, float startDelay)
        {
            targetSymbol = target;
            totalDuration = duration;
            spinDelay = startDelay;

            stateTimer = 0f;
            currentSpeed = 0f;
            currentState = ReelSpinState.Idle; // wait for our staggered delay
        }

        private void Start()
        {
            // Force perfect spacing at startup so we don't rely on human precision in the Inspector
            for (int i = 0; i < activeSymbols.Count; i++)
            {
                // If Center is index 1, this places them at +1, 0, -1, -2 heights
                float startY = (1 - i) * symbolHeight;
                activeSymbols[i].transform.localPosition = new Vector3(0, startY, 0);
            }

            if (fallbackSymbols == null || fallbackSymbols.Count == 0) return;

            foreach (ReelSymbolView view in activeSymbols)
            {
                // Fallback to Code Monkey rule: avoid magic strings/indexes, use count
                SymbolDefinitionSO randomSymbol = fallbackSymbols[UnityEngine.Random.Range(0, fallbackSymbols.Count)];
                view.SetSymbol(randomSymbol);
            }
        }
        private void Update()
        {
            switch (currentState)
            {
                case ReelSpinState.Idle:
                    HandleIdleState();
                    break;
                case ReelSpinState.Accelerating:
                    HandleAcceleratingState();
                    break;
                case ReelSpinState.ConstantSpeed:
                    HandleConstantSpeedState();
                    break;
                case ReelSpinState.Decelerating:
                    HandleDeceleratingState();
                    break;
                case ReelSpinState.Snapped:
                    // we're locked in, nothing to do
                    break;
            }
        }

        private void HandleIdleState()
        {
            if (targetSymbol == null) return;

            stateTimer += Time.deltaTime;
            if (stateTimer >= spinDelay)
            {
                stateTimer = 0f;
                currentState = ReelSpinState.Accelerating;
            }
        }

        private void HandleAcceleratingState()
        {
            stateTimer += Time.deltaTime;

            // quick linear acceleration for the first half second to get up to speed
            float accelDuration = 0.5f;
            currentSpeed = Mathf.Lerp(0f, maxSpinSpeed, stateTimer / accelDuration);

            MoveSymbolsCircular();

            if (stateTimer >= accelDuration)
            {
                stateTimer = 0f;
                currentState = ReelSpinState.ConstantSpeed;
            }
        }

        private void HandleConstantSpeedState()
        {
            stateTimer += Time.deltaTime;
            currentSpeed = maxSpinSpeed;

            MoveSymbolsCircular();

            // leave exactly 1 second at the end for the deceleration curve
            float timeUntilDecel = totalDuration - 1.0f;
            if (stateTimer >= timeUntilDecel)
            {
                stateTimer = 0f;
                InjectTargetSymbolIntoSequence();
                currentState = ReelSpinState.Decelerating;
            }
        }

        private void HandleDeceleratingState()
        {
            stateTimer += Time.deltaTime;

            float easeValue = decelerationCurve.Evaluate(stateTimer / 1.0f);
            currentSpeed = Mathf.Lerp(maxSpinSpeed, 0f, easeValue);

            MoveSymbolsCircular();

            if (stateTimer >= 1.0f)
            {
                // Push it to a transition state so Update stops calling this
                currentState = ReelSpinState.Snapped;
                SnapToGrid();
            }
        }

        private void MoveSymbolsCircular()
        {
            foreach (ReelSymbolView view in activeSymbols)
            {
                view.transform.localPosition += Vector3.down * (currentSpeed * Time.deltaTime);

                if (view.transform.localPosition.y <= -symbolHeight * 2)
                {
                    view.transform.localPosition += new Vector3(0, symbolHeight * activeSymbols.Count, 0);

                    // FIX: Only swap to random fallbacks if we aren't currently trying to lock in the final result.
                    // This prevents the target symbol from being accidentally overwritten at the last second.
                    if (fallbackSymbols.Count > 0 && currentState != ReelSpinState.Decelerating)
                    {
                        SymbolDefinitionSO randomSymbol = fallbackSymbols[UnityEngine.Random.Range(0, fallbackSymbols.Count)];
                        view.SetSymbol(randomSymbol);
                    }
                }
            }
        }

        private void InjectTargetSymbolIntoSequence()
        {
            ReelSymbolView highestView = activeSymbols[0];
            float maxY = float.MinValue;

            foreach (ReelSymbolView view in activeSymbols)
            {
                if (view.transform.localPosition.y > maxY)
                {
                    maxY = view.transform.localPosition.y;
                    highestView = view;
                }
            }

            // Cache the exact view we injected so we never accidentally swap to a different one
            injectedTargetView = highestView;

            if (targetSymbol != null)
            {
                injectedTargetView.SetSymbol(targetSymbol);
            }
        }

        private void SnapToGrid()
        {
            currentSpeed = 0f;

            // We strictly use the view we injected, ignoring whatever is technically "closest"
            CenterView = injectedTargetView;

            StartCoroutine(SettleRoutine());
        }

        private System.Collections.IEnumerator SettleRoutine()
        {
            OnReelImpact?.Invoke();

            // 1. Sort the views from top to bottom based on their actual physical position
            activeSymbols.Sort((a, b) => b.transform.localPosition.y.CompareTo(a.transform.localPosition.y));

            // 2. Find where our injected target ended up in that sorted list
            int centerIdx = activeSymbols.IndexOf(CenterView);

            // 3. Assign rigid, perfect slots relative to the center view (no rounding math)
            Vector3[] perfectPositions = new Vector3[activeSymbols.Count];
            for (int i = 0; i < activeSymbols.Count; i++)
            {
                int stepFromCenter = centerIdx - i;
                perfectPositions[i] = new Vector3(0, stepFromCenter * symbolHeight, 0);
            }

            float currentY = CenterView.transform.localPosition.y;
            float dropDistance = Mathf.Max(currentY, 75f);

            float elapsed = 0f;
            float duration = 0.18f;
            float overshootAmount = 30f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float currentOffset;

                if (t < 0.6f)
                {
                    currentOffset = Mathf.Lerp(dropDistance, -overshootAmount, t / 0.6f);
                }
                else
                {
                    currentOffset = Mathf.Lerp(-overshootAmount, 0f, (t - 0.6f) / 0.4f);
                }

                for (int i = 0; i < activeSymbols.Count; i++)
                {
                    activeSymbols[i].transform.localPosition = perfectPositions[i] + new Vector3(0, currentOffset, 0);
                }
                yield return null;
            }

            // Lock positions securely
            for (int i = 0; i < activeSymbols.Count; i++)
            {
                activeSymbols[i].transform.localPosition = perfectPositions[i];
            }

            if (CenterView != null) CenterView.PlaySquash();

            OnReelSnapped?.Invoke(this, targetSymbol);
            targetSymbol = null;
        }
    }
}