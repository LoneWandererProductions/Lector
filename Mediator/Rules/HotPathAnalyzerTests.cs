/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        HotPathAnalyzerTests.cs
 * PURPOSE:     Tests for HotPathAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class HotPathAnalyzerTests
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
        public void Analyze_UserMethodCallInLoop_IsFlagged()
        {
            const string code = @"
class Sample
{
    void Helper() { }
    void Run(int n)
    {
        for (int i = 0; i < n; i++)
        {
            Helper();
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new HotPathAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
        }

        [TestMethod]
        public void Analyze_CallOutsideAnyLoop_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    void Helper() { }
    void Run() { Helper(); }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new HotPathAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        [TestMethod]
        public void Analyze_UnqualifiedConsoleWriteLineInLoop_IsFlagged_FilterGap()
        {
            const string code = @"
class Sample
{
    void Run(int n)
    {
        for (int i = 0; i < n; i++)
        {
            System.Console.WriteLine(i);
        }
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine(i);
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new HotPathAnalyzer().Analyze(path, code).ToList();

            // Only the unqualified call is flagged - the fully-qualified "System." call
            // matches the exclusion prefix and is correctly skipped.
            Assert.AreEqual(1, diagnostics.Count);
        }

        [TestMethod]
        public void Analyze_CalledTwiceOnSameInstance_StatsAccumulateAcrossDirectCalls()
        {
            const string codeA =
                "class A { void Helper() { } void Run(int n) { for (int i = 0; i < n; i++) { Helper(); } } }";
            const string codeB =
                "class B { void Helper() { } void Run(int n) { for (int i = 0; i < n; i++) { Helper(); } } }";

            var pathA = AnalyzerTestHelper.CreateTempCsFile(codeA, _tempDir);
            var pathB = AnalyzerTestHelper.CreateTempCsFile(codeB, _tempDir);

            var analyzer = new HotPathAnalyzer();
            var firstFileResult = analyzer.Analyze(pathA, codeA).ToList();
            var secondFileResult = analyzer.Analyze(pathB, codeB).ToList();

            StringAssert.Contains(firstFileResult[0].Message, "Called 1 times");
            StringAssert.Contains(secondFileResult[0].Message, "Called 2 times",
                "documents current behavior: both files call a method named 'Helper', so " +
                "file B's unrelated call is counted as this instance's second sighting of " +
                "'Helper' overall when called consecutively");
        }
    }
}