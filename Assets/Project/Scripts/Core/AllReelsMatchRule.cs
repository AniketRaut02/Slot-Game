using System.Collections.Generic;
using UnityEngine;
using SlotGame.Data;

namespace SlotGame.Core
{
    public class AllReelsMatchRule : IWinRule
    {
        // strictly checking the middle row for now per the base assignment spec
        private const int CENTER_ROW = 1;

        public WinResult Evaluate(SymbolDefinitionSO[,] grid, PaytableSO paytable)
        {
            int reelCount = grid.GetLength(0);
            SymbolDefinitionSO firstNonWild = null;
            List<Vector2Int> matchedCells = new List<Vector2Int>();

            // loop through the middle row of every reel to see what we landed on
            for (int x = 0; x < reelCount; x++)
            {
                SymbolDefinitionSO currentSymbol = grid[x, CENTER_ROW];
                matchedCells.Add(new Vector2Int(x, CENTER_ROW));

                // if it's a wild, just keep going since it matches anything
                if (currentSymbol.IsWild)
                {
                    continue;
                }

                // lock in the first normal symbol we see
                if (firstNonWild == null)
                {
                    firstNonWild = currentSymbol;
                }
                // if we find a different symbol later, the streak is broken
                else if (firstNonWild != currentSymbol)
                {
                    return new WinResult { isWin = false };
                }
            }

            // if the whole line was just wilds, use the wild symbol itself for the payout lookup
            SymbolDefinitionSO winningSymbol = firstNonWild != null ? firstNonWild : grid[0, CENTER_ROW];
            float multiplier = paytable.GetMultiplier(winningSymbol);

            return new WinResult
            {
                isWin = true,
                matchedSymbol = winningSymbol,
                payoutMultiplier = multiplier,
                winningCells = matchedCells
            };
        }
    }
}