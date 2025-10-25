/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        ScriptExecutorTests.cs
 * PURPOSE:     Tests ScriptExecutor and registry commands with debug output
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using Weaver;
using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Mediator
{
    [TestClass]
    public class ScriptExecutorTests
    {
        private Weave _weave = null!;
        private ScriptExecutor _executor = null!;

        [TestInitialize]
        public void Setup()
        {
            _weave = new Weave();

            const string script = @"
        setValue(score, 100, Wint);
        getValue(score);
        memory();
        deleteValue(score);
        memory();
    ";

            // Tokenize and parse
            var lexer = new Lexer(script);
            var parser = new Parser(lexer.Tokenize());
            var blocks = parser.ParseIntoCategorizedBlocks();

            // Pass full (Category, Statement) tuples to ScriptExecutor
            var statements = blocks
                .Where(line => line.Statement != null)   // skip null statements
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            _executor = new ScriptExecutor(_weave, statements);
        }


        private void DebugResult(CommandResult result, string step)
        {
            Debug.WriteLine($"[{step}] Success={result.Success}, Message='{result.Message}', Feedback={(result.Feedback != null ? result.Feedback.Prompt : "<null>")}");
        }

        [TestMethod]
        public void Test_SimpleRegistryFlow()
        {
            var result1 = _executor.ExecuteNext();
            DebugResult(result1, "setValue");
            Assert.IsTrue(result1.Success);
            StringAssert.Contains(result1.Message, "registered");

            var result2 = _executor.ExecuteNext();
            DebugResult(result2, "getValue");
            Assert.IsTrue(result2.Success);
            StringAssert.Contains(result2.Message, "100");

            var result3 = _executor.ExecuteNext();
            DebugResult(result3, "memory1");
            Assert.IsTrue(result3.Success);
            StringAssert.Contains(result3.Message, "score");

            var result4 = _executor.ExecuteNext();
            DebugResult(result4, "deleteValue");
            Assert.IsTrue(result4.Success);
            StringAssert.Contains(result4.Message, "Deleted");

            var result5 = _executor.ExecuteNext();
            DebugResult(result5, "memory2");
            Assert.IsTrue(result5.Success);
            StringAssert.Contains(result5.Message, "empty");
        }

        [TestMethod]
        public void Test_LabelAndGoto()
        {
        const string script = @"
        label start;
        setValue(counter, 1, Wint);
        goto start;";

        // Tokenize and parse
        var lexer = new Lexer(script);
            var parser = new Parser(lexer.Tokenize());
            var blocks = parser.ParseIntoCategorizedBlocks();

            // Pass full (Category, Statement) tuples to ScriptExecutor
            var statements = blocks
                .Where(line => line.Statement != null)   // skip null statements
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            var result1 = executor.ExecuteNext();
            DebugResult(result1, "label");
            Assert.IsTrue(result1.Success);

            var result2 = executor.ExecuteNext();
            DebugResult(result2, "setValue");
            Assert.IsTrue(result2.Success);

            var result3 = executor.ExecuteNext();
            DebugResult(result3, "goto");
            Assert.IsTrue(result3.Success);
        }

        [TestMethod]
        public void Test_DoWhileLoop()
        {
            const string script = @"
            setValue(flag, true, Wbool);
            do;
            getValue(flag);
            while(getValue(flag))";

            // Tokenize and parse
            var lexer = new Lexer(script);
            var parser = new Parser(lexer.Tokenize());
            var blocks = parser.ParseIntoCategorizedBlocks();

            // Pass full (Category, Statement) tuples to ScriptExecutor
            var statements = blocks
                .Where(line => line.Statement != null)   // skip null statements
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            var result1 = executor.ExecuteNext();
            DebugResult(result1, "setValue");
            Assert.IsTrue(result1.Success);

            var result2 = executor.ExecuteNext();
            DebugResult(result2, "do");
            Assert.IsTrue(result2.Success);

            var result3 = executor.ExecuteNext();
            DebugResult(result3, "getValue");
            Assert.IsTrue(result3.Success);

            var result4 = executor.ExecuteNext();
            DebugResult(result4, "while");
            Assert.IsTrue(result4.Success);
        }

        [TestMethod]
        public void Test_ScriptFinish()
        {
            int step = 0;
            while (!_executor.IsFinished)
            {
                var result = _executor.ExecuteNext();
                DebugResult(result, $"Step {step}");
                step++;
            }

            Assert.IsTrue(_executor.IsFinished, "Script should finish completely.");
        }
    }
}
