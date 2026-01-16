/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Scripting
 * FILE:        ScriptEngineTests.cs
 * PURPOSE:     Simple Script Engine tests.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Mediator.Helper;
using Weaver.ScriptEngine;

namespace Mediator.Scripting
{
    /// <summary>
    /// basic tests for the Script Engine.
    /// </summary>
    [TestClass]
    public class ScriptEngineTests
    {
        /// <summary>
        /// Tests parsing a simple script with commands, labels, if/else, and do/while.
        /// Includes debug logging of parsed lines and executed commands.
        /// </summary>
        [TestMethod]
        public void TestScriptParsingAndExecution()
        {
            // Arrange: simple script
            const string script = @"
                label Start;
                do {
                    command1();
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

            // Parse
            var parser = new Weaver.ScriptEngine.Parser(tokens);
            var lines = parser.ParseIntoNodes(); // returns List<ScriptLine>

            var blocks = Lowering.ScriptLowerer(lines).ToList();

            // Debug: show all parsed lines
            Console.WriteLine("Parsed script lines:");
            foreach (var line in blocks)
            {
                Console.WriteLine($"[{line.Category}] '{line.Statement}'");
            }

            Assert.IsTrue(lines.Count > 0, "No lines parsed from script.");

            // Arrange execution context
            var executedCommands = new List<string>();
            var context = new ScriptContext();

            // Register commands
            context.RegisterCommand("command1", new DelegateCommand(() => executedCommands.Add("command1 executed")));
            context.RegisterCommand("command2", new DelegateCommand(() => executedCommands.Add("command2 executed")));
            context.RegisterCommand("command3", new DelegateCommand(() => executedCommands.Add("command3 executed")));

            // Act: Execute each command line
            Console.WriteLine("\nExecuting commands:");
            foreach (var line in blocks)
            {
                if (context.TryGetCommand(line.Statement, out var cmd))
                {
                    Console.WriteLine($"Executing: '{line.Statement}'");
                    cmd.Execute(null);
                }
                else
                {
                    Console.WriteLine($"Skipping non-command line: [{line.Category}] '{line.Statement}'");
                }
            }

            // Assert execution
            Assert.IsTrue(executedCommands.Contains("command1 executed"), "command1 was not executed");
            Assert.IsTrue(executedCommands.Contains("command2 executed"), "command2 was not executed");
            Assert.IsTrue(executedCommands.Contains("command3 executed"), "command3 was not executed");

            // Debug: executed commands summary
            Console.WriteLine("\nExecuted commands summary:");
            foreach (var cmd in executedCommands)
            {
                Console.WriteLine(cmd);
            }
        }
    }
}