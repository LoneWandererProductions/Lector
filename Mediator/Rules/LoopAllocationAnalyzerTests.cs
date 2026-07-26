/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        LoopAllocationAnalyzerTests.cs
 * PURPOSE:     Tests for LoopAllocationAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Loop Allocation Analyzer Tests.
    /// </summary>
    [TestClass]
    public class LoopAllocationAnalyzerTests
    {
        private string _tempDir = null!;

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
        /// Analyzes the non escaping collection in loop is flagged for hoisting.
        /// </summary>
        [TestMethod]
        public void Analyze_NonEscapingCollectionInLoop_IsFlaggedForHoisting()
        {
            const string code = @"
using System.Collections.Generic;
class Sample
{
    void Run(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var temp = new List<int>();
            temp.Add(i);
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LoopAllocationAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "does not escape");
        }

        /// <summary>
        /// Analyzes the collection returned from inside loop is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_CollectionReturnedFromInsideLoop_IsNotFlagged()
        {
            const string code = @"
using System.Collections.Generic;
class Sample
{
    List<int> Build(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var temp = new List<int>();
            temp.Add(i);
            if (i == 5)
                return temp;
        }
        return null;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LoopAllocationAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the variable declared outside loop and reassigned inside is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_VariableDeclaredOutsideLoopAndReassignedInside_IsNotFlagged()
        {
            const string code = @"
using System.Collections.Generic;
class Sample
{
    List<int> Build(int n)
    {
        List<int> temp = null;
        for (int i = 0; i < n; i++)
        {
            temp = new List<int>();
            temp.Add(i);
        }
        return temp;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LoopAllocationAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the collection passed as argument is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_CollectionPassedAsArgument_IsNotFlagged()
        {
            const string code = @"
using System.Collections.Generic;
class Sample
{
    void Consume(List<int> items) { }
    void Run(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var temp = new List<int>();
            temp.Add(i);
            Consume(temp);
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LoopAllocationAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the non collection type allocated in loop is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_NonCollectionTypeAllocatedInLoop_IsNotFlagged()
        {
            const string code = @"
class Widget { }
class Sample
{
    void Run(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var temp = new Widget();
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new LoopAllocationAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}
