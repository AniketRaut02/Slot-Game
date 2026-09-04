using System;
using SlotGame.Data;

namespace SlotGame.RNG
{
    public class SlotRNGService : ISlotRNG
    {
        private readonly Random random;

        public SlotRNGService()
        {
            random = new Random(Environment.TickCount);
        }

        public SlotRNGService(int seed)
        {
            // Constructor overload
            random = new Random(seed);
        }

        public int NextWeightedIndex(ReelStripSO strip)
        {
            if (strip == null || strip.Symbols.Count == 0)
            {
                throw new ArgumentException("ReelStripSO is null or contains no symbols.");
            }

            int totalWeight = strip.TotalWeight;
            if (totalWeight <= 0)
            {
                throw new InvalidOperationException("ReelStripSO total weight must be greater than zero.");
            }

            // Using Random.Range(0, count) would silently make every symbol equiprobable, breaking the economy.
            int randomWeight = random.Next(totalWeight);
            int currentWeightSum = 0;

            for (int i = 0; i < strip.Symbols.Count; i++)
            {
                currentWeightSum += strip.Symbols[i].weight;
                if (randomWeight < currentWeightSum)
                {
                    return i;
                }
            }

            // Fallback safety (mathematically unreachable if weights and random generation are valid).
            return strip.Symbols.Count - 1;
        }

        public float NextFloat01()
        {
            return (float)random.NextDouble();
        }
    }
}