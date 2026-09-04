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
            foreach (IWinRule rule in rules)
            {
                WinResult result = rule.Evaluate(finalGrid, paytable);
                if (result.isWin)
                {
                    // just bail out and return the first valid win we find
                    return result;
                }
            }

            return new WinResult { isWin = false };
        }
    }
}