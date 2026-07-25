/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        UnusedPrivateFieldAnalyzerTests.cs
 * PURPOSE:     Tests for UnusedPrivateFieldAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Analyzer test unused field.
    /// </summary>
    [TestClass]
    public class UnusedPrivateFieldAnalyzerTests
    {
        /// <summary>
        /// The temporary dir
        /// </summary>
        private string _tempDir = null!;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup() => _tempDir = AnalyzerTestHelper.CreateTempDirectory();

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup() => AnalyzerTestHelper.SafeDeleteDirectory(_tempDir);

        /// <summary>
        /// Analyzes the unused private field is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UnusedPrivateField_IsFlagged()
        {
            const string code = @"
class Sample
{
    private int _count;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedPrivateFieldAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "_count");
        }

        [TestMethod]
        public void Analyze_PrivateFieldReadElsewhereInFile_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    private int _count;
    public int GetCount() => _count;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedPrivateFieldAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the multiple declarators on one line flags only the unused one.
        /// </summary>
        [TestMethod]
        public void Analyze_MultipleDeclaratorsOnOneLine_FlagsOnlyTheUnusedOne()
        {
            // private int _used, _unused; declares two separate VariableDeclaratorSyntax
            // nodes under one FieldDeclarationSyntax - worth pinning down that each is
            // judged independently rather than the whole declaration being treated as one unit.
            const string code = @"
class Sample
{
    private int _used, _unused;
    public int GetUsed() => _used;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new UnusedPrivateFieldAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "_unused");
        }
    }
}
