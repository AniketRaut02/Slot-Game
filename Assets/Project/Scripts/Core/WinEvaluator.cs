using System.Collections.Generic;
using SlotGame.Data;

namespace SlotGame.Core
{
    public class WinEvaluator
    {
        private readonly List<IWinRule> rules;

        public WinEvaluator()
        {
            // setting this up as a list so it's super easy to add diagonal rules or paylines later if we want
            rules = new List<IWinRule>
            {
                new AllReelsMatchRule()
            };
        }

        public WinResult Evaluate(SymbolDefinitionSO[,] finalGrid, PaytableSO paytable)
        {
            int scatters = 0;

            // count scatters across the entire grid, not just the center line
            foreach (SymbolDefinitionSO symbol in finalGrid)
            {
                if (symbol != null && symbol.IsScatter)
                {
                    scatters++;
                }
            }

            // run our standard line win rules
            foreach (IWinRule rule in rules)
            {
                WinResult result = rule.Evaluate(finalGrid, paytable);
                if (result.isWin)
                {
                    result.scatterCount = scatters; // attach our scatter count to the winning packet
                    return result;
                }
            }

            // even if they lost the line bet, we still need to pass the scatter count out 
            // so the FreeSpinController can check if a bonus was triggered
            return new WinResult { isWin = false, scatterCount = scatters };
        }
    }
}