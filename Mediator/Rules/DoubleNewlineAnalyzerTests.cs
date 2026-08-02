/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        DoubleNewlineAnalyzerTests.cs
 * PURPOSE:     Tests for DoubleNewlineAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class DoubleNewlineAnalyzerTests
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
        /// Analyzes the two consecutive blank lines is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_TwoConsecutiveBlankLines_IsFlagged()
        {
            const string code = "class Sample\n{\n\n\n    void Run() { }\n}\n";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DoubleNewlineAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the single blank lines only is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_SingleBlankLinesOnly_IsNotFlagged()
        {
            const string code = "class Sample\n{\n\n    void Run() { }\n\n    void Other() { }\n}\n";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DoubleNewlineAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the three consecutive blank lines flags each adjacent pair.
        /// </summary>
        [TestMethod]
        public void Analyze_ThreeConsecutiveBlankLines_FlagsEachAdjacentPair()
        {
            // Lines (blank, blank, blank) contains two overlapping adjacent pairs,
            // so this reports twice, not once - worth knowing before treating the
            // count as "number of blank-line clusters".
            const string code = "class Sample\n{\n\n\n\n}\n";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DoubleNewlineAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(2, diagnostics.Count);
        }
    }
}