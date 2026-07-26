/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        EventHandlerAnalyzerTests.cs
 * PURPOSE:     Tests for EventHandlerAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class EventHandlerAnalyzerTests
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
        /// Analyzes the bare identifier event subscription is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_BareIdentifierEventSubscription_IsFlagged()
        {
            const string code = @"
class Sample
{
    public event System.Action MyEvent;
    void Wire()
    {
        MyEvent += Handler;
    }
    void Handler() { }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new EventHandlerAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "MyEvent");
        }

        /// <summary>
        /// Analyzes the plain numeric accumulator is also flagged known syntax limitation.
        /// </summary>
        [TestMethod]
        public void Analyze_PlainNumericAccumulator_IsAlsoFlagged_KnownSyntaxLimitation()
        {
            // Without a semantic model, a syntax-only analyzer cannot distinguish 
            // a numeric accumulator from a local event subscription using a bare identifier.
            const string code = @"
class Sample
{
    void Run()
    {
        int total = 0;
        total += 5;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new EventHandlerAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "total");
        }

        /// <summary>
        /// Analyzes the member access event subscription is successfully captured.
        /// </summary>
        [TestMethod]
        public void Analyze_MemberAccessEventSubscription_IsSuccessfullyCaptured()
        {
            const string code = @"
class Button
{
    public event System.Action Click;
}
class Sample
{
    void Wire(Button button)
    {
        button.Click += Handler;
    }
    void Handler() { }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new EventHandlerAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Click");
        }
    }
}