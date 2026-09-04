using UnityEngine;
using SlotGame.Data;

namespace SlotGame.Reels
{
    public class ReelTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ReelController reelToTest;
        [SerializeField] private SymbolDefinitionSO targetSymbol;

        [Header("Test Settings")]
        [SerializeField] private float testDuration = 2.5f;
        [SerializeField] private float testStartDelay = 0f;

        // Using ContextMenu lets us trigger this function directly from the 
        // Unity inspector by right-clicking the component, no UI button needed.
        [ContextMenu("Trigger Test Spin")]
        private void TriggerTestSpin()
        {
            if (reelToTest == null || targetSymbol == null)
            {
                Debug.LogWarning("Dude, assign the references in the inspector first.");
                return;
            }

            reelToTest.SpinToward(targetSymbol, testDuration, testStartDelay);
        }
    }
}