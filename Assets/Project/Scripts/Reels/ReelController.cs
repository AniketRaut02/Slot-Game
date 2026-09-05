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
        [SerializeField] private float accelDuration = 0.5f;
        [SerializeField] private float decelDuration = 1.0f;

        [Tooltip("Ease-out curve to make the stop feel natural, not linear")]
        [SerializeField] private AnimationCurve decelerationCurve;

        [Header("Settle / Snap Feel")]
        [Tooltip("Total time spent correcting from wherever the reel physically stopped into its perfect grid slot.")]
        [SerializeField] private float settleDuration = 0.22f;
        [Tooltip("Fraction of settleDuration spent falling toward the pocket before the recoil starts.")]
        [SerializeField, Range(0.1f, 0.9f)] private float settleFallFraction = 0.6f;
        [Tooltip("Max distance the reel dips past its resting slot on impact, before recoiling back.")]
        [SerializeField] private float maxOvershoot = 30f;

        [Header("Visual Replacements")]
        [SerializeField] private List<SymbolDefinitionSO> fallbackSymbols;


        public ReelSymbolView CenterView { get; private set; }

        // Fires at the exact moment the reel visually contacts its resting slot (bottom of the
        // settle dip) — this is the frame audio/camera-shake should react to, not "logic finished".
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

            // FIX: totalDuration is the caller's contract for the WHOLE spin (accel + constant + decel).
            // stateTimer resets when we entered this state, so we have to subtract the accel phase too,
            // not just decel — otherwise every spin silently runs accelDuration seconds long.
            float constantPhaseDuration = Mathf.Max(0f, totalDuration - accelDuration - decelDuration);
            if (stateTimer >= constantPhaseDuration)
            {
                stateTimer = 0f;
                InjectTargetSymbolIntoSequence();
                currentState = ReelSpinState.Decelerating;
            }
        }

        private void HandleDeceleratingState()
        {
            stateTimer += Time.deltaTime;

            float easeValue = decelerationCurve.Evaluate(stateTimer / decelDuration);
            currentSpeed = Mathf.Lerp(maxSpinSpeed, 0f, easeValue);

            MoveSymbolsCircular();

            if (stateTimer >= decelDuration)
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
            // 1. Sort the views from top to bottom based on their actual physical position
            activeSymbols.Sort((a, b) => b.transform.localPosition.y.CompareTo(a.transform.localPosition.y));

            // 2. Find where our injected target ended up in that sorted list
            int centerIdx = activeSymbols.IndexOf(CenterView);

            // 3. Assign rigid, perfect slots relative to the center view (no rounding math)
            int count = activeSymbols.Count;
            Vector3[] perfectPositions = new Vector3[count];
            Vector3[] startPositions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                int stepFromCenter = centerIdx - i;
                perfectPositions[i] = new Vector3(0, stepFromCenter * symbolHeight, 0);
                // FIX: capture each symbol's REAL current position instead of assuming a single
                // shared "dropDistance" derived only from CenterView. Every symbol corrects from
                // wherever it actually is, so there's no phantom pop and no direction-flip bug.
                startPositions[i] = activeSymbols[i].transform.localPosition;
            }

            float elapsed = 0f;
            bool impactFired = false;

            while (elapsed < settleDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / settleDuration);

                bool inFallPhase = t < settleFallFraction;
                float fallT = EaseOutCubic(Mathf.Clamp01(t / settleFallFraction));
                float recoilT = inFallPhase
                    ? 0f
                    : EaseOutBack(Mathf.Clamp01((t - settleFallFraction) / (1f - settleFallFraction)));

                for (int i = 0; i < count; i++)
                {
                    Vector3 target = perfectPositions[i];

                    // Scale the overshoot to how far THIS symbol actually had to travel, so a
                    // near-perfect stop doesn't get an oversized bounce, and a big correction does.
                    float travel = Mathf.Abs(startPositions[i].y - target.y);
                    float overshoot = Mathf.Min(maxOvershoot, travel * 0.5f + 5f);

                    Vector3 pos;
                    if (inFallPhase)
                    {
                        pos = Vector3.Lerp(startPositions[i], target, fallT);
                    }
                    else
                    {
                        Vector3 dipPoint = target + Vector3.down * overshoot;
                        pos = Vector3.Lerp(dipPoint, target, recoilT);
                    }

                    activeSymbols[i].transform.localPosition = pos;
                }

                // FIX: fire the impact exactly when the reel visually bottoms out into the pocket —
                // not at the start of the coroutine, before any motion has happened.
                if (!impactFired && !inFallPhase)
                {
                    impactFired = true;
                    OnReelImpact?.Invoke();
                }

                yield return null;
            }

            // Lock positions securely
            for (int i = 0; i < count; i++)
            {
                activeSymbols[i].transform.localPosition = perfectPositions[i];
            }

            if (CenterView != null) CenterView.PlaySquash();

            OnReelSnapped?.Invoke(this, targetSymbol);
            targetSymbol = null;
        }

        private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float x = t - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }
    }
}