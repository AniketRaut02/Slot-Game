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

        private ReelSpinState currentState = ReelSpinState.Idle;
        private float currentSpeed;
        private float stateTimer;
        private float spinDelay;
        private float totalDuration;
        private SymbolDefinitionSO targetSymbol;

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
            if (fallbackSymbols == null || fallbackSymbols.Count == 0) return;

            foreach (ReelSymbolView view in activeSymbols)
            {
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

            // grab the curve value so the slow down doesn't look robotic
            float easeValue = decelerationCurve.Evaluate(stateTimer / 1.0f);
            currentSpeed = Mathf.Lerp(maxSpinSpeed, 0f, easeValue);

            MoveSymbolsCircular();

            if (stateTimer >= 1.0f)
            {
                SnapToGrid();
                currentState = ReelSpinState.Snapped;

                // tell the orchestrator we finished our visual job
                OnReelSnapped?.Invoke(this, targetSymbol);
                targetSymbol = null;
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

                    if (fallbackSymbols != null && fallbackSymbols.Count > 0)
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

            if (targetSymbol != null)
            {
                highestView.SetSymbol(targetSymbol);
            }
        }

        private void SnapToGrid()
        {
            currentSpeed = 0f;

            float closestDistance = float.MaxValue;
            float offsetToCenter = 0f;
            ReelSymbolView centerView = null;

            foreach (ReelSymbolView view in activeSymbols)
            {
                float distance = Mathf.Abs(view.transform.localPosition.y);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    offsetToCenter = view.transform.localPosition.y;
                    centerView = view;
                }
            }

            if (centerView != null && targetSymbol != null)
            {
                centerView.SetSymbol(targetSymbol);
            }

            foreach (ReelSymbolView view in activeSymbols)
            {
                view.transform.localPosition -= new Vector3(0, offsetToCenter, 0);
            }
        }
    }
}