/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        UnusedParameterAnalyzerTests.cs
 * PURPOSE:     Tests for UnusedParameterAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class UnusedParameterAnalyzerTests
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
        /// Analyzes the unused parameter is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UnusedParameter_IsFlagged()
        {
            const string code = @"
class Sample
{
    void Run(int value)
    {
        System.Console.WriteLine(""hi"");
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedParameterAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "value");
        }

        /// <summary>
        /// Analyzes the used parameter is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UsedParameter_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    void Run(int value)
    {
        System.Console.WriteLine(value);
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedParameterAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the abstract method with no body is skipped entirely.
        /// </summary>
        [TestMethod]
        public void Analyze_AbstractMethodWithNoBody_IsSkippedEntirely()
        {
            const string code = @"
abstract class Sample
{
    public abstract void Run(int value);
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedParameterAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            // No Body/ExpressionBody to check means the analyzer skips it outright,
            // rather than false-flagging every parameter in an abstract signature.
            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the two parameters one unused flags only that one.
        /// </summary>
        [TestMethod]
        public void Analyze_TwoParametersOneUnused_FlagsOnlyThatOne()
        {
            const string code = @"
class Sample
{
    int Run(int used, int unused)
    {
        return used;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedParameterAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "unused");
        }
    }
}
