/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        StructPaddingAnalyzerTests.cs
 * PURPOSE:     Tests for StructPaddingAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */


using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for StructPaddingAnalyzer.
    /// </summary>
    [TestClass]
    public class StructPaddingAnalyzerTests
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
        /// Analyzes the sub optimal field order is flagged with reorder suggestion.
        /// </summary>
        [TestMethod]
        public void Analyze_SubOptimalFieldOrder_IsFlaggedWithReorderSuggestion()
        {
            const string code = @"
struct Foo
{
    public bool Flag;
    public long Value;
    public int Count;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new StructPaddingAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "Value, Count, Flag");
        }

        /// <summary>
        /// Analyzes the already optimal field order is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_AlreadyOptimalFieldOrder_IsNotFlagged()
        {
            const string code = @"
struct Foo
{
    public long Value;
    public int Count;
    public bool Flag;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new StructPaddingAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the single field structure is skipped entirely.
        /// </summary>
        [TestMethod]
        public void Analyze_SingleFieldStruct_IsSkippedEntirely()
        {
            const string code = @"
struct Foo
{
    public int OnlyField;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new StructPaddingAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the decimal field recognized as16 bytes is flagged with reorder suggestion.
        /// </summary>
        [TestMethod]
        public void Analyze_DecimalFieldRecognizedAs16Bytes_IsFlaggedWithReorderSuggestion()
        {
            // With decimal correctly priced at 16 bytes, it should be placed first
            // ahead of the 8-byte long and 4-byte int.
            const string code = @"
struct Foo
{
    public long L;
    public decimal D;
    public int I;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var diagnostics = new StructPaddingAnalyzer().Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "D, L, I");
        }
    }
}