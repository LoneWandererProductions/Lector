/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        LinqInLoopAnalyzerTests.cs
 * PURPOSE:     Tests for LinqInLoopAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for LinqInLoopAnalyzer.
    /// </summary>
    [TestClass]
    public class LinqInLoopAnalyzerTests
    {
        private string _tempDir = null!;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = AnalyzerTestHelper.CreateTempDirectory();
        }

        [TestCleanup]
        public void Cleanup()
        {
            AnalyzerTestHelper.SafeDeleteDirectory(_tempDir);
        }

        [TestMethod]
        public void Analyze_LinqInLoop_IsFlagged()
        {
            const string code = @"
using System.Collections.Generic;
using System.Linq;

class Sample
{
    void Process(List<int> numbers, List<List<int>> batches)
    {
        foreach (var batch in batches)
        {
            var filtered = batch.Where(x => x > 0).ToList();
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LinqInLoopAnalyzer().Analyze(path, code).ToList();

            Assert.IsTrue(diagnostics.Count >= 2, "Expected diagnostics for both Where and ToList calls.");
            Assert.IsTrue(diagnostics.Any(d => d.Message.Contains("Where")), "Expected diagnostic for 'Where'.");
            Assert.IsTrue(diagnostics.Any(d => d.Message.Contains("ToList")), "Expected diagnostic for 'ToList'.");
        }

        [TestMethod]
        public void Analyze_LinqOutsideLoop_IsNotFlagged()
        {
            const string code = @"
using System.Collections.Generic;
using System.Linq;

class Sample
{
    void Process(List<int> numbers)
    {
        var filtered = numbers.Where(x => x > 0).ToList();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LinqInLoopAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}