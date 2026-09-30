/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        AsyncVoidAnalyzerTests.cs
 * PURPOSE:     Tests for AsyncVoidAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for AsyncVoidAnalyzer.
    /// </summary>
    [TestClass]
    public class AsyncVoidAnalyzerTests
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
        public void Analyze_AsyncVoidMethod_IsFlagged()
        {
            const string code = @"
using System.Threading.Tasks;

class Sample
{
    private async void DoDangerousWork()
    {
        await Task.Delay(10);
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new AsyncVoidAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "DoDangerousWork");
        }

        [TestMethod]
        public void Analyze_AsyncTaskMethod_IsNotFlagged()
        {
            const string code = @"
using System.Threading.Tasks;

class Sample
{
    private async Task DoSafeWorkAsync()
    {
        await Task.Delay(10);
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new AsyncVoidAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        [TestMethod]
        public void Analyze_AsyncVoidEventHandler_IsNotFlagged()
        {
            const string code = @"
using System;
using System.Threading.Tasks;

class Sample
{
    private async void OnButtonClicked(object sender, EventArgs e)
    {
        await Task.Delay(10);
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new AsyncVoidAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}