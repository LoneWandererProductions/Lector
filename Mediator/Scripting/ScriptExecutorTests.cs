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

namespace Mediator.Scripting
{
    /// <summary>
    /// Tests for ScriptExecutor and registry logic.
    /// </summary>
    [TestClass]
    public class ScriptExecutorTests
    {
        private Weave _weave = null!;
        private ScriptExecutor _executor = null!;

        /// <summary>
        /// Setups this instance.
        /// </summary>
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
            var parser = new Weaver.ScriptEngine.Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = Lowering.ScriptLowerer(nodes);

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            List<(string Category, string)> statements = blocks
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

        /// <summary>
        /// Tests the label and goto.
        /// </summary>
        [TestMethod]
        public void Test_LabelAndGoto()
        {
            const string script = @"
        label start;
        setValue(counter, 1, Wint);
        goto start;
    ";

            var lexer = new Lexer(script);
            var parser = new Weaver.ScriptEngine.Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = Lowering.ScriptLowerer(nodes).ToList();

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            var statements = blocks
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            var safety = 0;
            while (!executor.IsFinished && safety < 10)
            {
                executor.ExecuteNext();
                safety++;
            }

            Assert.IsTrue(safety < 11, "Goto loop should not be infinite.");
        }

        /// <summary>
        /// Tests the do while loop.
        /// </summary>
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
            var parser = new Weaver.ScriptEngine.Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = Lowering.ScriptLowerer(nodes);

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
            var step = 0;
            while (!_executor.IsFinished)
            {
                var result = _executor.ExecuteNext();
                DebugResult(result, $"Step {step}");
                step++;
                Assert.IsTrue(step < 50, "Script should not loop infinitely.");
            }

            Assert.IsTrue(_executor.IsFinished, "Script should finish completely.");
        }

        [TestMethod]
        public void TestDoWhile_ExitLoopProperly()
        {
            const string script = @"
            do
            {
                setValue(counter, 1, Wint); // just increment manually
                setValue(counter, 2, Wint);
                setValue(counter, 3, Wint);
            }
            while(counter < 3);
            getValue(counter);
            ";

            var lexer = new Lexer(script);
            var parser = new Weaver.ScriptEngine.Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = Lowering.ScriptLowerer(nodes);

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            var statements = blocks
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            CommandResult? last = null;

            // Run until finished or safety abort
            var safety = 0;
            while (!executor.IsFinished && safety++ < 50)
            {
                last = executor.ExecuteNext();
                Debug.WriteLine($"Step {safety}: {last.Message}");
            }

            Assert.IsNotNull(last, "Expected a last result but got null");
            Assert.AreEqual("3", last.Value!.ToString(), "Counter should have incremented to 3");
        }

        [TestMethod]
        public void TestIfCondition_TrueExecutesBody()
        {
            const string script = @"
        setValue(x, 0, Wint);
        if(true)
        {
            setValue(x, 42, Wint);
        }
        getValue(x);
    ";

            var lexer = new Lexer(script);
            var parser = new Weaver.ScriptEngine.Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = Lowering.ScriptLowerer(nodes);

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            var statements = blocks
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            CommandResult? last = null;
            var safety = 0;
            while (!executor.IsFinished && safety++ < 20)
                last = executor.ExecuteNext();

            Assert.IsNotNull(last);
            Assert.AreEqual("42", last.Value!.ToString(), "IF true branch should execute and set x=42");
        }

        [TestMethod]
        public void TestIfCondition_FalseSkipsBody()
        {
            const string script = @"
        setValue(x,0,Wint);
        if(false)
        {
            setValue(x,1,Wint);
        }
        getValue(x);
    ";

            var lexer = new Lexer(script);
            var parser = new Weaver.ScriptEngine.Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = Lowering.ScriptLowerer(nodes);

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            var statements = blocks
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            CommandResult? lastResult = null;

            while (!executor.IsFinished)
            {
                var result = executor.ExecuteNext();
                // Only assign lastResult if a command actually ran (non-null)
                if (result != null && result.Value != null)
                    lastResult = result;
            }

            Assert.IsNotNull(lastResult, "Expected a final CommandResult from executed command.");
            Assert.AreEqual("0", lastResult.Value!.ToString(), "Counter should remain 0 because the if-body was skipped");
        }

        [TestMethod]
        public void TestIfElse_ExecutesCorrectBranch()
        {
            const string script = @"
        setValue(x,0,Wint);
        if(false)
        {
            setValue(x,1,Wint);
        }
        else
        {
            setValue(x,2,Wint);
        }
        getValue(x);
    ";

            var lexer = new Lexer(script);
            var parser = new Weaver.ScriptEngine.Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();
            var blocks = Lowering.ScriptLowerer(nodes);

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            var statements = blocks
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            CommandResult? result = null;

            while (!executor.IsFinished)
            {
                var r = executor.ExecuteNext();

                // capture the getValue explicitly
                if (r.Message.Contains("Retrieved key 'x'") || r.Value != null)
                    result = r;
            }

            Assert.IsNotNull(result, "Expected a CommandResult from getValue(x)");
            Assert.AreEqual("2", result.Value!.ToString(), "Counter should be 2 because the else branch executed");
        }

        [TestMethod]
        public void TestGoto_MissingLabelFailsGracefully()
        {
            const string script = @"
        goto nowhere;
    ";

            var lexer = new Lexer(script);
            var parser = new Weaver.ScriptEngine.Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();

            var blocks = Lowering.ScriptLowerer(nodes);

            foreach (var line in blocks)
                Trace.WriteLine($"{line.Category.PadRight(12)} : {line.Statement}");

            var statements = blocks
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList();

            var executor = new ScriptExecutor(_weave, statements);

            CommandResult? last = executor.ExecuteNext();

            Assert.IsNotNull(last);
            Assert.IsFalse(last.Success);
            StringAssert.Contains(last.Message, "not found", "Goto to missing label should fail");
        }
    }
}