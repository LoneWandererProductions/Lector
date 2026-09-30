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