using SlotGame.Data;

namespace SlotGame.Core
{
    public interface IWinRule
    {
        WinResult Evaluate(SymbolDefinitionSO[,] grid, PaytableSO paytable);
    }
}