/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        AllocationAnalyzerTests.cs
 * PURPOSE:     Tests for AllocationAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Allocation Analyzer tests.
    /// </summary>
    [TestClass]
    public class AllocationAnalyzerTests
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
        /// Analyzes the allocation in constant bounded loop flags with risk10.
        /// </summary>
        [TestMethod]
        public void Analyze_AllocationInConstantBoundedLoop_FlagsWithRisk10()
        {
            const string code = @"
class Sample
{
    void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var item = new object();
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new AllocationAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "risk 10");
        }

        [TestMethod]
        public void Analyze_AllocationInVariableBoundedLoop_FlagsWithRisk20()
        {
            const string code = @"
class Sample
{
    void Run(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var item = new object();
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new AllocationAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "risk 20");
        }

        [TestMethod]
        public void Analyze_AllocationInNestedLoop_FlagsWithRisk50()
        {
            const string code = @"
class Sample
{
    void Run(int n)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                var item = new object();
            }
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new AllocationAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "risk 50");
        }

        /// <summary>
        /// Analyzes the allocation outside any loop is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_AllocationOutsideAnyLoop_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    void Run()
    {
        var item = new object();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new AllocationAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the capitalized string type is skipped.
        /// </summary>
        [TestMethod]
        public void Analyze_CapitalizedStringType_IsSkipped()
        {
            const string code = @"
class Sample
{
    void Run(char[] chars)
    {
        for (int i = 0; i < 10; i++)
        {
            var s = new string(chars);
        }
        for (int i = 0; i < 10; i++)
        {
            var s = new String(chars);
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new AllocationAnalyzer().Analyze(path, code).ToList();
            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the called twice on same instance stats accumulate across direct calls.
        /// </summary>
        [TestMethod]
        public void Analyze_CalledTwiceOnSameInstance_StatsAccumulateAcrossDirectCalls()
        {
            const string codeA = @"
class A
{
    void Run()
    {
        for (int i = 0; i < 10; i++) { var item = new object(); }
    }
}";
            const string codeB = @"
class B
{
    void Run()
    {
        for (int i = 0; i < 10; i++) { var item = new object(); }
    }
}";
            var pathA = AnalyzerTestHelper.CreateTempCsFile(codeA, _tempDir);
            var pathB = AnalyzerTestHelper.CreateTempCsFile(codeB, _tempDir);

            var analyzer = new AllocationAnalyzer();
            var firstFileResult = analyzer.Analyze(pathA, codeA).ToList();
            var secondFileResult = analyzer.Analyze(pathB, codeB).ToList();

            StringAssert.Contains(firstFileResult[0].Message, "Called 1 times");
            // Documents behavior when Analyze() is invoked directly on the same instance consecutively
            StringAssert.Contains(secondFileResult[0].Message, "Called 2 times",
                "if this ever reads 'Called 1 times', direct instance state isolation has been updated");
        }
    }
}