using NUnit.Framework;
using UnityEngine;
using SlotGame.Core;
using SlotGame.Data;
using System.Collections.Generic;
using System.Reflection;

namespace SlotGame.Tests
{
    public class WinEvaluatorTests
    {
        private WinEvaluator evaluator;
        private PaytableSO mockPaytable;
        private SymbolDefinitionSO cherry;
        private SymbolDefinitionSO bell;
        private SymbolDefinitionSO wild;

        [SetUp]
        public void Setup()
        {
            evaluator = new WinEvaluator();

            cherry = ScriptableObject.CreateInstance<SymbolDefinitionSO>();
            bell = ScriptableObject.CreateInstance<SymbolDefinitionSO>();
            wild = ScriptableObject.CreateInstance<SymbolDefinitionSO>();

            // force the isWild flag to true using reflection so we don't have to break encapsulation in the main script
            typeof(SymbolDefinitionSO).GetField("isWild", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(wild, true);

            mockPaytable = ScriptableObject.CreateInstance<PaytableSO>();
            List<PaytableEntry> mockEntries = new List<PaytableEntry>
            {
                new PaytableEntry { symbol = cherry, payoutMultiplier = 5f },
                new PaytableEntry { symbol = bell, payoutMultiplier = 10f }
            };
            typeof(PaytableSO).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mockPaytable, mockEntries);
        }

        [Test]
        public void Evaluate_AllMatch_ReturnsWin()
        {
            // Arrange a 3x3 grid (x, y) where y=1 is the middle row
            SymbolDefinitionSO[,] grid = new SymbolDefinitionSO[3, 3];
            grid[0, 1] = cherry;
            grid[1, 1] = cherry;
            grid[2, 1] = cherry;

            // Act
            WinResult result = evaluator.Evaluate(grid, mockPaytable);

            // Assert
            Assert.IsTrue(result.isWin);
            Assert.AreEqual(cherry, result.matchedSymbol);
            Assert.AreEqual(5f, result.payoutMultiplier);
        }

        [Test]
        public void Evaluate_NoMatch_ReturnsLoss()
        {
            SymbolDefinitionSO[,] grid = new SymbolDefinitionSO[3, 3];
            grid[0, 1] = cherry;
            grid[1, 1] = bell;
            grid[2, 1] = cherry;

            WinResult result = evaluator.Evaluate(grid, mockPaytable);

            Assert.IsFalse(result.isWin);
        }

        [Test]
        public void Evaluate_WithWild_ReturnsWin()
        {
            SymbolDefinitionSO[,] grid = new SymbolDefinitionSO[3, 3];
            grid[0, 1] = bell;
            grid[1, 1] = wild;
            grid[2, 1] = bell;

            WinResult result = evaluator.Evaluate(grid, mockPaytable);

            Assert.IsTrue(result.isWin);
            Assert.AreEqual(bell, result.matchedSymbol);
            Assert.AreEqual(10f, result.payoutMultiplier);
        }
    }
}