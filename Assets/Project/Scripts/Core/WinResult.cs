using System.Collections.Generic;
using UnityEngine;
using SlotGame.Data;

namespace SlotGame.Core
{
    public struct WinResult
    {
        public bool isWin;
        public SymbolDefinitionSO matchedSymbol;
        public float payoutMultiplier;
        public List<Vector2Int> winningCells;
        public int scatterCount; // New field for the bonus system to read
    }
}