/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        RethrowExceptionAnalyzerTests.cs
 * PURPOSE:     Tests for RethrowExceptionAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for RethrowExceptionAnalyzer.
    /// </summary>
    [TestClass]
    public class RethrowExceptionAnalyzerTests
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
        /// Analyzes the throw ex is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_ThrowEx_IsFlagged()
        {
            const string code = @"
using System;

class Sample
{
    void DoWork()
    {
        try
        {
            int.Parse(""invalid"");
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new RethrowExceptionAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "ex");
        }

        /// <summary>
        /// Analyzes the throw is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_Throw_IsNotFlagged()
        {
            const string code = @"
using System;

class Sample
{
    void DoWork()
    {
        try
        {
            int.Parse(""invalid"");
        }
        catch (Exception)
        {
            throw;
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new RethrowExceptionAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}