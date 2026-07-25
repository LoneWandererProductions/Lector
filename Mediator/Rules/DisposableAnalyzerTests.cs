/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        DisposableAnalyzerTests.cs
 * PURPOSE:     Tests for DisposableAnalyzer.
 *
 * DisposableAnalyzer's own source comment calls it a "Dummy check for demonstration":
 * it flags a variable as an undisposed IDisposable purely by checking whether its
 * *declared type name* ends in "Stream", "Reader", or "Writer" - it never checks
 * whether the type actually implements IDisposable. The last three tests below pin
 * down exactly what that trade-off costs today. If the analyzer is ever upgraded to
 * a real semantic-model check, those three assertions are the ones that should flip.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class DisposableAnalyzerTests
    {
        private string _tempDir = null!;

        [TestInitialize]
        public void Setup() => _tempDir = AnalyzerTestHelper.CreateTempDirectory();

        [TestCleanup]
        public void Cleanup() => AnalyzerTestHelper.SafeDeleteDirectory(_tempDir);

        /// <summary>
        /// Analyzes the stream declared outside using is flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_StreamDeclaredOutsideUsing_IsFlagged()
        {
            const string code = @"
using System.IO;
class Sample
{
    void Read()
    {
        FileStream stream = File.OpenRead(""x.txt"");
        stream.ReadByte();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "stream");
        }

        [TestMethod]
        public void Analyze_StreamDeclaredInsideUsing_IsNotFlagged()
        {
            const string code = @"
using System.IO;
class Sample
{
    void Read()
    {
        using FileStream stream = File.OpenRead(""x.txt"");
        stream.ReadByte();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the real disposable with non matching name false negative.
        /// </summary>
        [TestMethod]
        public void Analyze_RealDisposableWithNonMatchingName_FalseNegative()
        {
            // MyResource genuinely implements IDisposable and is never disposed here,
            // but the analyzer only pattern-matches on the name suffix, so this real
            // leak is missed.
            const string code = @"
using System;
class MyResource : IDisposable
{
    public void Dispose() { }
}
class Sample
{
    void Use()
    {
        MyResource res = new MyResource();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count, "documents current behavior: a real IDisposable leak is missed");
        }

        /// <summary>
        /// Analyzes the non disposable with matching name suffix false positive.
        /// </summary>
        [TestMethod]
        public void Analyze_NonDisposableWithMatchingNameSuffix_FalsePositive()
        {
            // TokenStream never implements IDisposable, but its name ends in "Stream",
            // which is all this heuristic checks.
            const string code = @"
class TokenStream
{
    public int Next() => 0;
}
class Sample
{
    void Use()
    {
        TokenStream stream = new TokenStream();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count, "documents current behavior: flagged on name alone");
        }

        /// <summary>
        /// Analyzes the variable declared stream not disposed false negative.
        /// </summary>
        [TestMethod]
        public void Analyze_VarDeclaredStreamNotDisposed_FalseNegative()
        {
            // This is the exact scenario the analyzer claims to target (an undisposed
            // FileStream) written the idiomatic modern way. It's still missed, because
            // the syntax-level type text for a `var` declaration is literally "var",
            // which doesn't end in Stream/Reader/Writer.
            const string code = @"
using System.IO;
class Sample
{
    void Read()
    {
        var stream = File.OpenRead(""x.txt"");
        stream.ReadByte();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count, "documents current behavior: `var` hides the type name from the check");
        }
    }
}
