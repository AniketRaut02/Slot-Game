using UnityEngine;

namespace SlotGame.Data
{
    [CreateAssetMenu(fileName = "NewSymbolDefinition", menuName = "Slot Game/Data/Symbol Definition")]
    public class SymbolDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string symbolId;

        [Header("Visuals")]
        [SerializeField] private Sprite displaySprite;

        [Header("Rules")]
        [SerializeField] private float payoutMultiplier;
        [SerializeField] private bool isWild;
        [SerializeField] private bool isScatter;

        // Core logic should compare the ScriptableObject references directly rather than strings.
        // The symbolId remains exposed strictly for debugging or save-state serialization.
        public string SymbolId => symbolId;
        public Sprite DisplaySprite => displaySprite;
        public float PayoutMultiplier => payoutMultiplier;
        public bool IsWild => isWild;
        public bool IsScatter => isScatter;
    }
}