/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        EmptyCatchBlockAnalyzerTests.cs
 * PURPOSE:     Tests for EmptyCatchBlockAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for EmptyCatchBlockAnalyzer.
    /// </summary>
    [TestClass]
    public class EmptyCatchBlockAnalyzerTests
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
        public void Analyze_EmptyCatchBlock_IsFlagged()
        {
            const string code = @"
using System;

class Sample
{
    void DoWork()
    {
        try
        {
            int x = 0;
            int y = 1 / x;
        }
        catch (Exception)
        {
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new EmptyCatchBlockAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Empty catch block");
        }

        [TestMethod]
        public void Analyze_NonEmptyCatchBlock_IsNotFlagged()
        {
            const string code = @"
using System;

class Sample
{
    void DoWork()
    {
        try
        {
            int x = 0;
            int y = 1 / x;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new EmptyCatchBlockAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}