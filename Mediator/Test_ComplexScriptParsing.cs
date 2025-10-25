/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        ParserTests.cs
 * PURPOSE:     Tests the Parser’s correct categorization of tokens
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weaver.ScriptEngine;
using System.Collections.Generic;

namespace Mediator
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Test_ComplexScriptParsing()
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

            var parser = new Parser(tokens);
            var lines = parser.ParseIntoCategorizedBlocks();

            // Check expected structure
            var categories = new List<string>();
            foreach (var line in lines)
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
            Assert.AreEqual("Label", lines[0].Category);
            Assert.AreEqual("Assignment", lines[3].Category);
            Assert.AreEqual("While_Condition", lines[5].Category);

            // Verify condition text
            var ifCond = lines.Find(l => l.Category == "If_Condition");
            Assert.AreEqual("x>0", ifCond.Statement);
        }
    }
}
