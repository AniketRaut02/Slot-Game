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
            // lock input if we aren't chilling in the idle state
            if (currentState != GameState.Idle) return;

            currentState = GameState.Spinning;
            reelsStoppedCount = 0;

            for (int i = 0; i < reels.Count; i++)
            {
                // 1. ask RNG for the exact final outcome before any graphics move
                int targetIndex = rngService.NextWeightedIndex(reelStrips[i]);
                SymbolDefinitionSO targetSymbol = reelStrips[i].Symbols[targetIndex].symbol;

                // Validate our data instantly so we aren't guessing where a null came from
                if (targetSymbol == null)
                {
                    Debug.LogError($"[SlotMachineController] ReelStrip '{reelStrips[i].name}' has an empty symbol slot at index {targetIndex}. Assign it in the Inspector!");
                }

                // 2. cache it in our invisible grid for the win evaluator to check later.
                finalGrid[i, 1] = targetSymbol;

                // 3. calculate the stagger so they stop one after another, classic slot tension trick
                float startDelay = i * config.PerReelStopStagger;
                float randomDuration = UnityEngine.Random.Range(config.MinSpinDuration, config.MaxSpinDuration);

                // 4. tell the visual reel to go do its thing
                reels[i].SpinToward(targetSymbol, randomDuration, startDelay);
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
            // pass the pre-calculated grid to our pure logic evaluator
            WinResult result = winEvaluator.Evaluate(finalGrid, paytable);

            currentState = GameState.PresentingWin;
            Debug.Log($"Win status: {result.isWin}");
            // broadcast the result out to the void. 
            // UI, Audio, and Economy will pick this up on their own without tight coupling.
            if (winEvaluatedEvent != null)
            {
                winEvaluatedEvent.RaiseEvent(result);
            }

            // popping back to idle. in a full game we'd wait for a "presentation finished" event,
            // but this keeps the loop unbroken for now.
            currentState = GameState.Idle;
        }
    }
}