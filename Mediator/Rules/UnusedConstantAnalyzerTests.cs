/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        UnusedConstantAnalyzerTests.cs
 * PURPOSE:     Analyzer that tests UnusedConstantAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Rules;

namespace Mediator.Rules
{
    [TestClass]
    public class UnusedConstantAnalyzerTests
    {
        /// <summary>
        /// Analyzes the project constant referenced elsewhere is not flagged.
        /// </summary>
        [TestMethod]
        public void AnalyzeProject_ConstantReferencedElsewhere_IsNotFlagged()
        {
            var files = new Dictionary<string, string>
            {
                ["A.cs"] = "const int MaxRetries = 5;",
                ["B.cs"] = "var x = MaxRetries;"
            };

            var diagnostics = new UnusedConstantAnalyzer().AnalyzeProject(files).ToList();

            Assert.AreEqual(0, diagnostics.Count);
        }

        [TestMethod]
        public void AnalyzeProject_ConstantNeverReferenced_IsFlagged()
        {
            var files = new Dictionary<string, string>
            {
                ["A.cs"] = "const int NeverRead = 5;"
            };

            var diagnostics = new UnusedConstantAnalyzer().AnalyzeProject(files).ToList();

            Assert.AreEqual(1, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, "NeverRead");
        }

        [TestMethod]
        public void AnalyzeProject_NameOnlyMentionedInAComment_CountsAsUsage_FalseNegative()
        {
            // MaxRetries is genuinely never read by any real code here - "used" only
            // shows up in an unrelated comment - but whole-word text matching can't
            // tell the difference, so this dead constant is missed.
            var files = new Dictionary<string, string>
            {
                ["A.cs"] = "const int MaxRetries = 5;",
                ["B.cs"] = "// unrelated note: see MaxRetries in the config docs"
            };

            var diagnostics = new UnusedConstantAnalyzer().AnalyzeProject(files).ToList();

            Assert.AreEqual(0, diagnostics.Count,
                "documents current behavior: a comment mentioning the name counts as \"usage\"");
        }
    }
}