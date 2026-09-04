using SlotGame.Data;

namespace SlotGame.RNG
{
    public interface ISlotRNG
    {
        int NextWeightedIndex(ReelStripSO strip);
        float NextFloat01();
    }
}