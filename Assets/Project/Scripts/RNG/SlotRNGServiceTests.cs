using NUnit.Framework;
using UnityEngine;
using SlotGame.RNG;
using SlotGame.Data;
using System.Collections.Generic;
using System.Reflection;

namespace SlotGame.Tests
{
    public class SlotRNGServiceTests
    {
        private const int ITERATION_COUNT = 10000;
        private const float TOLERANCE_PERCENTAGE = 0.05f; // 5% acceptable variance bounds for RNG

        [Test]
        public void NextWeightedIndex_DistributionMatchesConfiguredWeights()
        {
            // Arrange
            SlotRNGService rngService = new SlotRNGService();
            ReelStripSO testStrip = ScriptableObject.CreateInstance<ReelStripSO>();

            SymbolDefinitionSO symbolA = ScriptableObject.CreateInstance<SymbolDefinitionSO>();
            SymbolDefinitionSO symbolB = ScriptableObject.CreateInstance<SymbolDefinitionSO>();
            SymbolDefinitionSO symbolC = ScriptableObject.CreateInstance<SymbolDefinitionSO>();

            // Setup a 50/30/20 probability split
            List<WeightedSymbol> testSymbols = new List<WeightedSymbol>
            {
                new WeightedSymbol { symbol = symbolA, weight = 50 },
                new WeightedSymbol { symbol = symbolB, weight = 30 },
                new WeightedSymbol { symbol = symbolC, weight = 20 }
            };

            // Use reflection to bypass [SerializeField] private encapsulation specifically for testing,
            // preserving the strict information hiding of the original Data class.
            FieldInfo fieldInfo = typeof(ReelStripSO).GetField("symbols", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(testStrip, testSymbols);

            int[] selectionCounts = new int[testSymbols.Count];

            // Act
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                int selectedIndex = rngService.NextWeightedIndex(testStrip);
                selectionCounts[selectedIndex]++;
            }

            // Assert
            float expectedRatioA = 50f / 100f;
            float expectedRatioB = 30f / 100f;
            float expectedRatioC = 20f / 100f;

            float actualRatioA = (float)selectionCounts[0] / ITERATION_COUNT;
            float actualRatioB = (float)selectionCounts[1] / ITERATION_COUNT;
            float actualRatioC = (float)selectionCounts[2] / ITERATION_COUNT;

            Assert.AreEqual(expectedRatioA, actualRatioA, TOLERANCE_PERCENTAGE, "Symbol A distribution fell outside acceptable variance.");
            Assert.AreEqual(expectedRatioB, actualRatioB, TOLERANCE_PERCENTAGE, "Symbol B distribution fell outside acceptable variance.");
            Assert.AreEqual(expectedRatioC, actualRatioC, TOLERANCE_PERCENTAGE, "Symbol C distribution fell outside acceptable variance.");
        }
    }
}