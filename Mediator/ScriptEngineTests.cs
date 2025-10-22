/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        ScriptEngineTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Weaver.ScriptEngine;

namespace Mediator
{
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
            var script = @"
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
            var parser = new Parser(tokens);
            var lines = parser.ParseIntoCategorizedBlocks(); // returns List<ScriptLine>

            // Debug: show all parsed lines
            Console.WriteLine("Parsed script lines:");
            foreach (var line in lines)
            {
                Console.WriteLine($"[{line.Category}] '{line.Statement}'");
            }

            Assert.IsTrue(lines.Count > 0, "No lines parsed from script.");

            // Arrange execution context
            var executedCommands = new List<string>();
            var context = new ScriptExecutionContext();

            // Register commands
            context.RegisterCommand("command1", new DelegateCommand(() => executedCommands.Add("command1 executed")));
            context.RegisterCommand("command2", new DelegateCommand(() => executedCommands.Add("command2 executed")));
            context.RegisterCommand("command3", new DelegateCommand(() => executedCommands.Add("command3 executed")));

            // Act: Execute each command line
            Console.WriteLine("\nExecuting commands:");
            foreach (var line in lines)
            {
                if (line.Statement == null) continue;

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

    // Simple delegate command for testing
    public class DelegateCommand : System.Windows.Input.ICommand
    {
        private readonly Action _execute;
        public DelegateCommand(Action execute) => _execute = execute;
        public event EventHandler CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }

    // Dummy context for command registration
    public class ScriptExecutionContext
    {
        private readonly Dictionary<string, DelegateCommand> _commands = new();

        public void RegisterCommand(string name, DelegateCommand command)
        {
            _commands[name] = command;
        }

        public bool TryGetCommand(string? statement, out DelegateCommand? command)
        {
            if (string.IsNullOrWhiteSpace(statement))
            {
                command = null;
                return false;
            }

            // Simple matching: strip parentheses and whitespace
            var key = statement.Split('(')[0].Trim();
            return _commands.TryGetValue(key, out command);
        }
    }
}
