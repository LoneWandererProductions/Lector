/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        MagicNumberAnalyzerTests.cs
 * PURPOSE:     Tests for MagicNumberAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class MagicNumberAnalyzerTests
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
        /// Analyzes the magic number in method body is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_MagicNumberInMethodBody_IsFlagged()
        {
            const string code = @"
class Sample
{
    int Compute(int x) => x * 37;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new MagicNumberAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "37");
        }

        [TestMethod]
        public void Analyze_OnlySafeNumbers_AreNotFlagged()
        {
            // 0, 1, 2, -1 are the analyzer's explicit "safe" allow-list.
            const string code = @"
class Sample
{
    int Compute(int x) => (x + 1) * 2 - 1 + 0;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new MagicNumberAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the constant field declaration is not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_ConstFieldDeclaration_IsNotFlagged()
        {
            const string code = @"
class Sample
{
    private const int MaxRetries = 99;
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new MagicNumberAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the same literal twice in one method flags each occurrence.
        /// </summary>
        [TestMethod]
        public void Analyze_SameLiteralTwiceInOneMethod_FlagsEachOccurrence()
        {
            const string code = @"
class Sample
{
    int Compute(int x)
    {
        if (x > 42) return 42;
        return 0;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new MagicNumberAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            // Every occurrence is reported separately - two "42"s means two diagnostics,
            // not one de-duplicated one.
            Assert.AreEqual(2, diagnostics.Count);
        }
    }
}
