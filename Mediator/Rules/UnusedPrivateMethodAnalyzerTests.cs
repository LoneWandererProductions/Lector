/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        UnusedPrivateMethodAnalyzerTests.cs
 * PURPOSE:     Tests for UnusedPrivateMethodAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for UnusedPrivateMethodAnalyzer.
    /// </summary>
    [TestClass]
    public class UnusedPrivateMethodAnalyzerTests
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

        /// <summary>
        /// Analyzes the unused private method is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UnusedPrivateMethod_IsFlagged()
        {
            const string code = @"
class Sample
{
    private void UnusedInternalMethod()
    {
    }

    public void PublicMethod()
    {
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new UnusedPrivateMethodAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "UnusedInternalMethod");
        }

        /// <summary>
        /// Analyzes the used private method is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UsedPrivateMethod_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    private void Helper()
    {
    }

    public void PublicMethod()
    {
        Helper();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new UnusedPrivateMethodAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the public method is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_PublicMethod_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    public void PublicMethod()
    {
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new UnusedPrivateMethodAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}