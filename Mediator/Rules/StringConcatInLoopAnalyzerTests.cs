/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        StringConcatInLoopAnalyzerTests.cs
 * PURPOSE:     Tests for StringConcatInLoopAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for StringConcatInLoopAnalyzer.
    /// </summary>
    [TestClass]
    public class StringConcatInLoopAnalyzerTests
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
        /// Analyzes the string concat in loop is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_StringConcatInLoop_IsFlagged()
        {
            const string code = @"
class Sample
{
    void BuildString()
    {
        string result = """";
        for (int i = 0; i < 10; i++)
        {
            result += i.ToString();
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new StringConcatInLoopAnalyzer().Analyze(path, code).ToList();

            Assert.IsTrue(diagnostics.Count >= 1);
            StringAssert.Contains(diagnostics[0].Message, "+=");
        }

        /// <summary>
        /// Analyzes the string concat outside loop is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_StringConcatOutsideLoop_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    void BuildString()
    {
        string prefix = ""Hello "";
        string name = ""World"";
        string result = prefix + name;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new StringConcatInLoopAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}