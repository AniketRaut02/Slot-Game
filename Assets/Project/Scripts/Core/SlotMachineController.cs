using System.Collections.Generic;
using UnityEngine;
using SlotGame.Data;
using SlotGame.Reels;
using SlotGame.RNG;
using SlotGame.Events;

namespace SlotGame.Core
{
    public class SlotMachineController : MonoBehaviour
    {
        [Header("Data & Configuration")]
        [SerializeField] private SlotMachineConfigSO config;
        [SerializeField] private PaytableSO paytable;
        [SerializeField] private List<ReelStripSO> reelStrips;

        [Header("Scene References")]
        [SerializeField] private List<ReelController> reels;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO spinRequestedEvent;
        [SerializeField] private WinResultEventChannelSO winEvaluatedEvent;

        private GameState currentState = GameState.Idle;
        private ISlotRNG rngService;
        private WinEvaluator winEvaluator;

        private int reelsStoppedCount;
        private SymbolDefinitionSO[,] finalGrid;

        private void Awake()
        {
            // spinning up our pure C# logic systems 
            rngService = new SlotRNGService();
            winEvaluator = new WinEvaluator();

            // prep the grid array based on our config SO
            finalGrid = new SymbolDefinitionSO[config.ReelCount, config.VisibleRows];
        }

        private void OnEnable()
        {
            if (spinRequestedEvent != null)
            {
                spinRequestedEvent.OnEventRaised += HandleSpinRequested;
            }

            foreach (ReelController reel in reels)
            {
                reel.OnReelSnapped += HandleReelStopped;
            }
        }

        private void OnDisable()
        {
            if (spinRequestedEvent != null)
            {
                spinRequestedEvent.OnEventRaised -= HandleSpinRequested;
            }

            foreach (ReelController reel in reels)
            {
                reel.OnReelSnapped -= HandleReelStopped;
            }
        }

        private void HandleSpinRequested()
        {
            if (currentState != GameState.Idle) return;

            currentState = GameState.Spinning;
            reelsStoppedCount = 0;

            // Clear previous win animations
            foreach (ReelController reel in reels)
            {
                if (reel.CenterView != null) reel.CenterView.ResetView();
            }

            float baseDuration = UnityEngine.Random.Range(config.MinSpinDuration, config.MaxSpinDuration);

            for (int i = 0; i < reels.Count; i++)
            {
                int targetIndex = rngService.NextWeightedIndex(reelStrips[i]);
                SymbolDefinitionSO targetSymbol = reelStrips[i].Symbols[targetIndex].symbol;
                finalGrid[i, 1] = targetSymbol;

                float startDelay = i * config.PerReelStopStagger;
                float duration = baseDuration;

                // Classic slot tension trick: add an extra half second to the final reel
                if (i == reels.Count - 1)
                {
                    duration += 0.5f;
                }

                reels[i].SpinToward(targetSymbol, duration, startDelay);
            }
        }

        private void HandleReelStopped(ReelController reel, SymbolDefinitionSO landedSymbol)
        {
            reelsStoppedCount++;

            // wait for the final reel to snap into place before grading the homework
            if (reelsStoppedCount >= reels.Count)
            {
                currentState = GameState.Evaluating;
                EvaluateWin();
            }
        }

        private void EvaluateWin()
        {
            WinResult result = winEvaluator.Evaluate(finalGrid, paytable);
            currentState = GameState.PresentingWin;

            // Trigger the visual glow on the specific winning symbols
            if (result.isWin && result.winningCells != null)
            {
                foreach (Vector2Int cell in result.winningCells)
                {
                    // cell.x maps perfectly to our reels list index
                    reels[cell.x].CenterView.PlayWinPulse();
                }
            }

            if (winEvaluatedEvent != null)
            {
                winEvaluatedEvent.RaiseEvent(result);
            }

            currentState = GameState.Idle;
        }
    }
}