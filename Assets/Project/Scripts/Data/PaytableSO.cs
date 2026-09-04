using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotGame.Data
{
    [Serializable]
    public struct PaytableEntry
    {
        public SymbolDefinitionSO symbol;
        [Min(0f)] public float payoutMultiplier;
    }

    [CreateAssetMenu(fileName = "NewPaytable", menuName = "Slot Game/Data/Paytable")]
    public class PaytableSO : ScriptableObject
    {
        [SerializeField] private List<PaytableEntry> entries;

        public float GetMultiplier(SymbolDefinitionSO targetSymbol)
        {
            // A linear search over a list replaces a native Dictionary. 
            // Unity does not serialize Dictionaries natively, and the dataset is small enough (<20 items) 
            // that linear search overhead is negligible compared to custom serialization logic.
            foreach (PaytableEntry entry in entries)
            {
                if (entry.symbol == targetSymbol)
                {
                    return entry.payoutMultiplier;
                }
            }

            return 0f;
        }
    }
}