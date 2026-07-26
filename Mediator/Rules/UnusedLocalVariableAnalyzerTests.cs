/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        UnusedConstantAnalyzerTests.cs
 * PURPOSE:     Analyzer that tests UnusedLocalVariableAnalyzerTests.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */


using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for UnusedLocalVariableAnalyzerTests
    /// </summary>
    [TestClass]
    public class UnusedLocalVariableAnalyzerTests
    {
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

        [TestMethod]
        public void Analyze_UnusedLocalVariable_IsFlagged()
        {
            const string code = @"
class Sample
{
    void Run()
    {
        int total = 0;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new UnusedLocalVariableAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "total");
        }

        /// <summary>
        /// Analyzes the local variable read later is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_LocalVariableReadLater_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    void Run()
    {
        int total = 0;
        System.Console.WriteLine(total);
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new UnusedLocalVariableAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the discard named variable is intentionally not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_DiscardNamedVariable_IsIntentionallyNotFlagged()
        {
            // The analyzer explicitly special-cases the "_" discard name, so an
            // intentionally-ignored return value doesn't get flagged as dead code.
            const string code = @"
class Sample
{
    int Compute() => 42;
    void Run()
    {
        var _ = Compute();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new UnusedLocalVariableAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}
