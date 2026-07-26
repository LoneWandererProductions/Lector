/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        LoopCacheAnalyzerTests.cs
 * PURPOSE:     Tests for LoopCacheAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// LoopCacheAnalyzer tests.
    /// </summary>
    [TestClass]
    public class LoopCacheAnalyzerTests
    {
        /// <summary>
        /// The temporary dir
        /// </summary>
        private string _tempDir = null!;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _tempDir = AnalyzerTestHelper.CreateTempDirectory();
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            AnalyzerTestHelper.SafeDeleteDirectory(_tempDir);
        }

        /// <summary>
        /// Analyzes the loop invariant call is flagged for caching.
        /// </summary>
        [TestMethod]
        public void Analyze_LoopInvariantCall_IsFlaggedForCaching()
        {
            const string code = @"
class Calculator
{
    public int Square(int x) => x * x;
}
class Sample
{
    void Run(int n)
    {
        var helper = new Calculator();
        for (int i = 0; i < n; i++)
        {
            var y = helper.Square(5);
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LoopCacheAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Square");
        }

        /// <summary>
        /// Analyzes the same invariant call on math prefixed class name is correctly analyzed.
        /// </summary>
        [TestMethod]
        public void Analyze_SameInvariantCall_OnMathPrefixedClassName_IsCorrectlyAnalyzed()
        {
            // Verifies that classes starting with "Math" in the global namespace (e.g. MathHelper)
            // are no longer incorrectly filtered out.
            const string code = @"
class MathHelper
{
    public int Square(int x) => x * x;
}
class Sample
{
    void Run(int n)
    {
        var helper = new MathHelper();
        for (int i = 0; i < n; i++)
        {
            var y = helper.Square(5);
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LoopCacheAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count,
                "Global namespace classes starting with 'Math' are successfully analyzed and flagged.");
            StringAssert.Contains(diagnostics[0].Message, "Square");
        }

        /// <summary>
        /// Analyzes the call that reads the loop variable is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_CallThatReadsTheLoopVariable_IsNotFlagged()
        {
            const string code = @"
class Calculator
{
    public int Square(int x) => x * x;
}
class Sample
{
    void Run(int n)
    {
        var helper = new Calculator();
        for (int i = 0; i < n; i++)
        {
            var y = helper.Square(i);
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LoopCacheAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}