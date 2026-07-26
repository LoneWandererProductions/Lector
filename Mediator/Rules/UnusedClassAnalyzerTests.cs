/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        UnusedClassAnalyzerTests.cs
 * PURPOSE:     Analyzer that tests UnusedClassAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Tests for UnusedClassAnalyzer.
    /// </summary>
    [TestClass]
    public class UnusedClassAnalyzerTests
    {
        /// <summary>
        /// Analyzes the project class referenced elsewhere is not flagged.
        /// </summary>
        [TestMethod]
        public void AnalyzeProject_ClassReferencedElsewhere_IsNotFlagged()
        {
            // B.cs deliberately doesn't declare a class of its own - just mentions
            // "Helper" as plain text - since this analyzer works on raw text, not
            // parsed/compiled C#, and introducing a second class here would itself be
            // "declared but never referenced" and complicate the count below.
            var files = new Dictionary<string, string>
            {
                ["A.cs"] = "class Helper { }",
                ["B.cs"] = "var h = new Helper();"
            };

            var diagnostics = new UnusedClassAnalyzer().AnalyzeProject(files).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        /// <summary>
        /// Analyzes the project class never referenced is flagged.
        /// </summary>
        [TestMethod]
        public void AnalyzeProject_ClassNeverReferenced_IsFlagged()
        {
            var files = new Dictionary<string, string>
            {
                ["A.cs"] = "class NeverUsed { }"
            };

            var diagnostics = new UnusedClassAnalyzer().AnalyzeProject(files).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "NeverUsed");
        }

        /// <summary>
        /// Analyzes the project name only mentioned in a comment is treated as a declaration false positive.
        /// </summary>
        [TestMethod]
        public void AnalyzeProject_NameOnlyMentionedInAComment_IsTreatedAsADeclaration_FalsePositive()
        {
            // "Ghost" is never a real class anywhere - it only ever appears inside a
            // comment - but the regex matches "class <word>" regardless of context, so
            // it's registered as a declaration, then reported as unused since that
            // comment is the only place the word "Ghost" appears anywhere in the project.
            // (Real is genuinely referenced from B.cs, so it won't also show up here and
            // muddy the point.)
            var files = new Dictionary<string, string>
            {
                ["A.cs"] = "// TODO: replace this base class Ghost implementation later\nclass Real { }",
                ["B.cs"] = "class Consumer { Real r = new Real(); }"
            };

            var diagnostics = new UnusedClassAnalyzer().AnalyzeProject(files).ToList();

            Assert.IsTrue(diagnostics.Any(d => d.Message.Contains("Ghost")),
                "documents current behavior: a word following \"class \" inside a comment is indistinguishable from a real declaration");
        }
    }
}
