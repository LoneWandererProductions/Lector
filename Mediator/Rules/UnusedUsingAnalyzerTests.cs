/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        UnusedUsingAnalyzerTests.cs
 * PURPOSE:     Tests for UnusedUsingAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for UnusedUsingAnalyzer.
    /// </summary>
    [TestClass]
    public class UnusedUsingAnalyzerTests
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
        /// Analyzes the unused using directive is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UnusedUsingDirective_IsFlagged()
        {
            const string code = @"
using System.Text;

class Sample
{
    void Run()
    {
        int x = 5;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new UnusedUsingAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "System.Text");
        }

        /// <summary>
        /// Analyzes the used using directive is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UsedUsingDirective_IsNotFlagged()
        {
            const string code = @"
using System.Text;

class Sample
{
    void Run()
    {
        var sb = new StringBuilder();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new UnusedUsingAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}