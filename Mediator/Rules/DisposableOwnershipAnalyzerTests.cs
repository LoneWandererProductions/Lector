/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        DisposableOwnershipAnalyzerTests.cs
 * PURPOSE:     Tests for DisposableOwnershipAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Enums;
using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Disposable Ownership Tests
    /// </summary>
    [TestClass]
    public class DisposableOwnershipAnalyzerTests
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
        /// Mirrors DetailCompareView._btmOne = btm; overwriting a live field with no
        /// disposal of the previous value - the exact gap CA2000 does not check.
        /// </summary>
        [TestMethod]
        public void Analyze_FieldOverwrittenWithoutDispose_IsFlagged()
        {
            const string code = @"
using System;
class MyBitmap : IDisposable
{
    public void Dispose() { }
}
class Sample
{
    private MyBitmap _current;

    void Open(MyBitmap next)
    {
        _current = next;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableOwnershipAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "_current");
            StringAssert.Contains(diagnostics[0].Message, "CA2000");
        }

        /// <summary>
        /// Same shape, but the previous value is disposed first - should be silent.
        /// </summary>
        [TestMethod]
        public void Analyze_FieldDisposedBeforeOverwrite_IsNotFlagged()
        {
            const string code = @"
using System;
class MyBitmap : IDisposable
{
    public void Dispose() { }
}
class Sample
{
    private MyBitmap _current;

    void Open(MyBitmap next)
    {
        _current?.Dispose();
        _current = next;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableOwnershipAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Mirrors AnalysisProcessing.cs: a local disposable is reassigned to a new
        /// instance before the first instance is disposed.
        /// </summary>
        [TestMethod]
        public void Analyze_LocalReassignedWithoutDispose_IsFlagged()
        {
            const string code = @"
using System;
class Wrapper : IDisposable
{
    public static Wrapper GetInstance() => new Wrapper();
    public void Dispose() { }
}
class Sample
{
    void Process()
    {
        var dbm = Wrapper.GetInstance();
        dbm.ToString();
        dbm = Wrapper.GetInstance();
        dbm.Dispose();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableOwnershipAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            StringAssert.Contains(diagnostics[0].Message, "reassigned");
        }

        /// <summary>
        /// Proves the fix for DisposableAnalyzer's documented false negative: a real
        /// IDisposable whose name doesn't hint at it (no "Stream"/"Reader"/"Writer") is
        /// still caught here because the type is resolved semantically.
        /// </summary>
        [TestMethod]
        public void Analyze_RealDisposableWithNonMatchingName_IsFlagged()
        {
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
            var analyzer = new DisposableOwnershipAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count,
                "unlike DisposableAnalyzer, this must catch a real IDisposable regardless of its name");
        }

        /// <summary>
        /// A value that is returned out of the method is not flagged as "never disposed" -
        /// ownership plausibly transfers to the caller.
        /// </summary>
        [TestMethod]
        public void Analyze_DisposableReturned_IsNotFlagged()
        {
            const string code = @"
using System;
class MyResource : IDisposable
{
    public void Dispose() { }
}
class Sample
{
    MyResource Create()
    {
        var res = new MyResource();
        return res;
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableOwnershipAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// The same never-disposed shape inside a loop is escalated from Warning to
        /// Error - the severity dimension CA2000 has no equivalent for.
        /// </summary>
        [TestMethod]
        public void Analyze_LeakInsideLoop_IsEscalatedToError()
        {
            const string code = @"
using System;
class MyResource : IDisposable
{
    public void Dispose() { }
}
class Sample
{
    void ScanFiles(string[] paths)
    {
        foreach (var path in paths)
        {
            var res = new MyResource();
            res.ToString();
        }
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableOwnershipAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.IsTrue(diagnostics.Count >= 1);
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[0].Severity);
        }

        /// <summary>
        /// The same shape outside any loop stays a Warning.
        /// </summary>
        [TestMethod]
        public void Analyze_LeakOutsideLoop_StaysWarning()
        {
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
        var res = new MyResource();
        res.ToString();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableOwnershipAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[0].Severity);
        }

        /// <summary>
        /// 'using' declarations remain silent, as with DisposableAnalyzer.
        /// </summary>
        [TestMethod]
        public void Analyze_UsingDeclaration_IsNotFlagged()
        {
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
        using var res = new MyResource();
        res.ToString();
    }
}";
            var path = AnalyzerTestHelper.CreateTempCsFile(code, _tempDir);
            var analyzer = new DisposableOwnershipAnalyzer();

            var diagnostics = analyzer.Analyze(path, code).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }
    }
}
