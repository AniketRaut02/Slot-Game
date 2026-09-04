using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotGame.Data
{
    [Serializable]
    public struct WeightedSymbol
    {
        public SymbolDefinitionSO symbol;
        [Min(0)] public int weight;
    }

    [CreateAssetMenu(fileName = "NewReelStrip", menuName = "Slot Game/Data/Reel Strip")]
    public class ReelStripSO : ScriptableObject
    {
        [SerializeField] private List<WeightedSymbol> symbols;

        public IReadOnlyList<WeightedSymbol> Symbols => symbols;

        private int cachedTotalWeight = -1;

        public int TotalWeight
        {
            get
            {
                if (cachedTotalWeight < 0)
                {
                    CalculateTotalWeight();
                }
                return cachedTotalWeight;
            }
        }

        // Ensures the weight is recalculatable in the editor if tweaks values.
        private void OnValidate()
        {
            CalculateTotalWeight();
        }

        private void CalculateTotalWeight()
        {
            cachedTotalWeight = 0;
            if (symbols == null) return;

            foreach (WeightedSymbol entry in symbols)
            {
                cachedTotalWeight += entry.weight;
            }
        }
    }
}