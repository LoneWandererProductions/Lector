/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Scripting
 * FILE:        ComplexScriptParsing.cs
 * PURPOSE:     Tests the Parser’s correct categorization of tokens
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.ScriptEngine;
using System.Diagnostics;

namespace Mediator.Scripting
{
    [TestClass]
    public class ComplexScriptParsing
    {
        /// <summary>
        /// Tests the complex script parsing.
        /// </summary>
        [TestMethod]
        public void TestComplexScriptParsing()
        {
            const string script = @"
                label Start;
                do {
                    command1();
                    x = 5;
                } while(condition);
                if (x > 0) {
                    command2();
                } else {
                    command3();
                }
                goto End;
                label End;
            ";
            // Tokenize (assuming you have a Lexer class)
            var lexer = new Lexer(script);
            var tokens = lexer.Tokenize();


            var parser = new Weaver.ScriptEngine.Parser(tokens);
            var lines = parser.ParseIntoNodes();

            var blocks = Lowering.ScriptLowerer(lines).ToList();

            foreach (var line in blocks)
            {
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");
            }


            // Check expected structure
            var categories = new List<string>();
            foreach (var line in blocks)
                categories.Add(line.Category);

            // Verify key blocks
            CollectionAssert.Contains(categories, "Label");
            CollectionAssert.Contains(categories, "Do_Open");
            CollectionAssert.Contains(categories, "Do_End");
            CollectionAssert.Contains(categories, "While_Condition");
            CollectionAssert.Contains(categories, "If_Condition");
            CollectionAssert.Contains(categories, "Else_Open");
            CollectionAssert.Contains(categories, "Goto");
            CollectionAssert.Contains(categories, "Command_Rewrite");

            // Verify ordering (rough check)
            Assert.AreEqual("Label", blocks[0].Category);
            Assert.AreEqual("Command_Rewrite", blocks[3].Category);
            Assert.AreEqual("While_Condition", blocks[5].Category);

            // Verify condition text
            var ifCond = blocks.Find(l => l.Category == "If_Condition");
            Assert.AreEqual("x>0", ifCond.Statement);
        }

        /// <summary>
        /// Tests the complex script parsing.
        /// </summary>
        [TestMethod]
        public void TestComplexScriptParsing_NoRewrite()
        {
            const string script = @"
                label Start;
                do {
                    command1();
                    x = 5;
                } while(condition);
                if (x > 0) {
                    command2();
                } else {
                    command3();
                }
                goto End;
                label End;
            ";
            // Tokenize (assuming you have a Lexer class)
            var lexer = new Lexer(script);
            var tokens = lexer.Tokenize();


            var parser = new Weaver.ScriptEngine.Parser(tokens);
            var lines = parser.ParseIntoNodes();

            var blocks = Lowering.ScriptLowerer(lines, null, false).ToList();

            foreach (var line in blocks)
            {
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");
            }


            // Check expected structure
            var categories = new List<string>();
            foreach (var line in blocks)
                categories.Add(line.Category);

            // Verify key blocks
            CollectionAssert.Contains(categories, "Label");
            CollectionAssert.Contains(categories, "Do_Open");
            CollectionAssert.Contains(categories, "Do_End");
            CollectionAssert.Contains(categories, "While_Condition");
            CollectionAssert.Contains(categories, "If_Condition");
            CollectionAssert.Contains(categories, "Else_Open");
            CollectionAssert.Contains(categories, "Goto");
            CollectionAssert.Contains(categories, "Assignment");

            // Verify ordering (rough check)
            Assert.AreEqual("Label", blocks[0].Category);
            Assert.AreEqual("Assignment", blocks[3].Category);
            Assert.AreEqual("While_Condition", blocks[5].Category);

            // Verify condition text
            var ifCond = blocks.Find(l => l.Category == "If_Condition");
            Assert.AreEqual("x>0", ifCond.Statement);
        }
    }
}