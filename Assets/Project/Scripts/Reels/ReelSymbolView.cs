using UnityEngine;
using UnityEngine.UI;
using SlotGame.Data;

namespace SlotGame.Reels
{
    public class ReelSymbolView : MonoBehaviour
    {
        [SerializeField] private Image symbolImage;

        private SymbolDefinitionSO currentSymbol;

        public SymbolDefinitionSO CurrentSymbol => currentSymbol;

        public void SetSymbol(SymbolDefinitionSO symbol)
        {
            currentSymbol = symbol;

            // just updating the sprite to match our data layer
            if (symbol != null && symbolImage != null)
            {
                symbolImage.sprite = symbol.DisplaySprite;
            }
        }
    }
}