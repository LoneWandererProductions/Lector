/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        DocCommentCoverageCommandTests.cs
 * PURPOSE:     Tests for DocCommentCoverageCommand.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class DocCommentCoverageCommandTests
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

        [TestCleanup]
        public void Cleanup()
        {
            AnalyzerTestHelper.SafeDeleteDirectory(_tempDir);
        }

        /// <summary>
        /// Analyzes the fully documented class and method is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_FullyDocumentedClassAndMethod_IsNotFlagged()
        {
            const string code = @"
/// <summary>A fully documented sample.</summary>
class Sample
{
    /// <summary>Does the thing.</summary>
    void Run() { }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new DocCommentCoverageCommand().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the type of the undocumented class with no members flags only the.
        /// </summary>
        [TestMethod]
        public void Analyze_UndocumentedClassWithNoMembers_FlagsOnlyTheType()
        {
            const string code = @"
class Sample
{
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new DocCommentCoverageCommand().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Sample");
            StringAssert.Contains(diagnostics[0].Message, "Type");
        }

        /// <summary>
        /// Analyzes the documented class with one undocumented method flags only the member.
        /// </summary>
        [TestMethod]
        public void Analyze_DocumentedClassWithOneUndocumentedMethod_FlagsOnlyTheMember()
        {
            const string code = @"
/// <summary>A fully documented sample.</summary>
class Sample
{
    void Run() { }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new DocCommentCoverageCommand().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Run");
            StringAssert.Contains(diagnostics[0].Message, "Member");
        }

        /// <summary>
        /// Analyzes the plain comment is not enough still flagged as missing XML document.
        /// </summary>
        [TestMethod]
        public void Analyze_PlainCommentIsNotEnough_StillFlaggedAsMissingXmlDoc()
        {
            const string code = @"
// This is a sample class, not XML documentation.
class Sample
{
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new DocCommentCoverageCommand().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Sample");
        }
    }
}