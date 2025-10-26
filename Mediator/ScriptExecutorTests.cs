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
    /// <summary>
    /// Tests for ScriptExecutor and registry logic.
    /// </summary>
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

            var lexer = new Lexer(script);
            var parser = new Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = DebugHelpers.FlattenNodes(nodes);

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            var statements = blocks
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            _executor = new ScriptExecutor(_weave, statements);
        }

        private static void DebugResult(CommandResult result, string step)
        {
            Debug.WriteLine(
                $"[{step}] Success={result.Success}, Message='{result.Message}', " +
                $"Feedback={(result.Feedback != null ? result.Feedback.Prompt : "<null>")}"
            );
        }

        [TestMethod]
        public void TestSimpleRegistryFlow()
        {
            var result1 = _executor.ExecuteNext();
            DebugResult(result1, "setValue");
            Assert.IsTrue(result1.Success);
            StringAssert.Contains(result1.Message, "Registered");

            var result2 = _executor.ExecuteNext();
            DebugResult(result2, "getValue");
            Assert.IsTrue(result2.Success);
            Assert.IsNotNull(result2.Value);
            StringAssert.Contains(result2.Value!.ToString()!, "100");

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
            StringAssert.Contains(result5.Message.ToLowerInvariant(), "empty");
        }

        [TestMethod]
        public void Test_LabelAndGoto()
        {
            const string script = @"
        label start;
        setValue(counter, 1, Wint);
        goto start;
    ";

            var lexer = new Lexer(script);
            var parser = new Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = DebugHelpers.FlattenNodes(nodes).ToList();

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            var statements = blocks
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            int safety = 0;
            while (!executor.IsFinished && safety < 10)
            {
                executor.ExecuteNext();
                safety++;
            }

            Assert.IsTrue(safety < 11, "Goto loop should not be infinite.");
        }


        [TestMethod]
        public void TestDoWhileLoop()
        {
            const string script = @"
                setValue(flag, true, Wbool);
                do
                {
                    getValue(flag);
                }
                while(flag);
            ";

            var lexer = new Lexer(script);
            var parser = new Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = DebugHelpers.FlattenNodes(nodes);

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            var statements = blocks
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            var result1 = executor.ExecuteNext();
            DebugResult(result1, "setValue");
            Assert.IsTrue(result1.Success);

            var result2 = executor.ExecuteNext();
            DebugResult(result2, "do_open");
            Assert.IsTrue(result2.Success);

            var result3 = executor.ExecuteNext();
            DebugResult(result3, "getValue");
            Assert.IsTrue(result3.Success);

            var result4 = executor.ExecuteNext();
            DebugResult(result4, "while_condition");
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
                Assert.IsTrue(step < 50, "Script should not loop infinitely.");
            }

            Assert.IsTrue(_executor.IsFinished, "Script should finish completely.");
        }
    }
}