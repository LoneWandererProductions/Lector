/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        DuplicateStringLiteralAnalyzerTests.cs
 * PURPOSE:     Tests for DuplicateStringLiteralAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Reflection;
using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class DuplicateStringLiteralAnalyzerTests
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
            ResetStaticCache();
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            AnalyzerTestHelper.SafeDeleteDirectory(_tempDir);
            ResetStaticCache();
        }

        /// <summary>
        /// Resets the static cache.
        /// </summary>
        private static void ResetStaticCache()
        {
            typeof(DuplicateStringLiteralAnalyzer)
                .GetField("_cachedLiterals", BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, null);
        }

        /// <summary>
        /// Analyzes the literal repeated across two files in same project is flagged in both.
        /// </summary>
        [TestMethod]
        public void Analyze_LiteralRepeatedAcrossTwoFilesInSameProject_IsFlaggedInBoth()
        {
            const string literal = "duplicate-me";
            var pathA = AnalyzerTestHelper.CreateTempCsFile(
                $"class A {{ string S() => \"{literal}\"; }}", _tempDir);
            var pathB = AnalyzerTestHelper.CreateTempCsFile(
                $"class B {{ string S() => \"{literal}\"; }}", _tempDir);

            var analyzer = new DuplicateStringLiteralAnalyzer();

            // Both files need to exist before the *first* Analyze() call, because that
            // call is what triggers the one-time project-wide scan that gets cached.
            var diagnosticsA = analyzer.Analyze(pathA, File.ReadAllText(pathA)).ToList();
            var diagnosticsB = analyzer.Analyze(pathB, File.ReadAllText(pathB)).ToList();

            Assert.AreEqual(1, diagnosticsA.Count);
            Assert.AreEqual(1, diagnosticsB.Count);
        }

        /// <summary>
        /// Analyzes the unique literals only are not flagged.
        /// </summary>
        [TestMethod]
        public void Analyze_UniqueLiteralsOnly_AreNotFlagged()
        {
            var pathA = AnalyzerTestHelper.CreateTempCsFile(
                "class A { string S() => \"only-in-a\"; }", _tempDir);
            var pathB = AnalyzerTestHelper.CreateTempCsFile(
                "class B { string S() => \"only-in-b\"; }", _tempDir);

            var analyzer = new DuplicateStringLiteralAnalyzer();

            var diagnosticsA = analyzer.Analyze(pathA, File.ReadAllText(pathA)).ToList();
            var diagnosticsB = analyzer.Analyze(pathB, File.ReadAllText(pathB)).ToList();

            Assert.AreEqual(0, diagnosticsA.Count);
            Assert.AreEqual(0, diagnosticsB.Count);
        }

        /// <summary>
        /// Analyzes the duplicates inside project.
        /// </summary>
        [TestMethod]
        public void Analyze_Duplicates_Inside_Project()
        {
            var project1 = AnalyzerTestHelper.CreateTempDirectory();
            var project2 = AnalyzerTestHelper.CreateTempDirectory();
            try
            {
                var p1FileA = AnalyzerTestHelper.CreateTempCsFile(
                    "class A { string S() => \"project1-literal\"; }", project1);
                AnalyzerTestHelper.CreateTempCsFile(
                    "class B { string S() => \"project1-literal\"; }", project1);

                var p2FileA = AnalyzerTestHelper.CreateTempCsFile(
                    "class C { string S() => \"project2-literal\"; }", project2);
                AnalyzerTestHelper.CreateTempCsFile(
                    "class D { string S() => \"project2-literal\"; }", project2);

                var analyzer = new DuplicateStringLiteralAnalyzer();

                // Populate the cache from project1 first.
                var firstRun = analyzer.Analyze(p1FileA, File.ReadAllText(p1FileA)).ToList();
                Assert.AreEqual(1, firstRun.Count, "sanity check: project1's own duplicate is found");

                // Now analyze project2, which has its own genuine duplicate literal,
                // with no reset in between.
                var secondRun = analyzer.Analyze(p2FileA, File.ReadAllText(p2FileA)).ToList();

                Assert.AreEqual(1, secondRun.Count,
                    "Project 2's cache should be rebuilt automatically when the root directory changes, allowing it to find its own duplicate.");
            }
            finally
            {
                AnalyzerTestHelper.SafeDeleteDirectory(project1);
                AnalyzerTestHelper.SafeDeleteDirectory(project2);
            }
        }
    }
}