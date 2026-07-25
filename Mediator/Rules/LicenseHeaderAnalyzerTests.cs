/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        LicenseHeaderAnalyzerTests.cs
 * PURPOSE:     Tests for LicenseHeaderAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class LicenseHeaderAnalyzerTests
    {
        private string _tempDir = null!;

        [TestInitialize]
        public void Setup() => _tempDir = AnalyzerTestHelper.CreateTempDirectory();

        [TestCleanup]
        public void Cleanup() => AnalyzerTestHelper.SafeDeleteDirectory(_tempDir);

        [TestMethod]
        public void Analyze_FileWithCopyrightBlockComment_IsNotFlagged()
        {
            // Mirrors this codebase's own header convention.
            const string code = @"/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Sample
 */
class Sample { }";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new LicenseHeaderAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        [TestMethod]
        public void Analyze_FileWithNoLeadingComment_IsFlagged()
        {
            const string code = @"class Sample { }";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new LicenseHeaderAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Missing license header");
        }

        [TestMethod]
        public void Analyze_LeadingCommentWithoutLicenseWords_IsStillFlagged()
        {
            const string code = @"// just a note to future me, nothing legal in here
class Sample { }";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new LicenseHeaderAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
        }

        [TestMethod]
        public void Analyze_LineCommentHeaderMentioningLicense_IsNotFlagged()
        {
            const string code = @"// License: MIT
class Sample { }";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new LicenseHeaderAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}
