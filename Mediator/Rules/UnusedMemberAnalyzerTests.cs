/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        UnusedMemberAnalyzerTests.cs
 * PURPOSE:     Tests for UnusedMemberAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class UnusedMemberAnalyzerTests
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
        /// Analyzes the unused explicit private method is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UnusedExplicitPrivateMethod_IsFlagged()
        {
            const string code = @"
class Sample
{
    private void Helper() { }
    public void Run() { }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedMemberAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Helper");
        }

        /// <summary>
        /// Analyzes the private method called elsewhere in file is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_PrivateMethodCalledElsewhereInFile_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    private void Helper() { }
    public void Run() { Helper(); }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedMemberAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the unused public method is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UnusedPublicMethod_IsNotFlagged()
        {
            // Only members with an explicit `private` modifier are in scope.
            const string code = @"
class Sample
{
    public void NeverCalled() { }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedMemberAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the unused implicitly private method and ensures it is correctly flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UnusedImplicitlyPrivateMethod_IsFlagged()
        {
            // Class members with no access modifier are private by default in C#.
            // By using the SemanticModel, the analyzer correctly identifies this as private
            // even without an explicit `private` syntax token.
            const string code = @"
class Sample
{
    void Helper() { }
    public void Run() { }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedMemberAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Helper");
        }
    }
}