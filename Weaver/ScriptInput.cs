/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        ScriptInput.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Weaver
{
    /// <summary>
    /// Handles executing a full script string with feedback support.
    /// </summary>
    public sealed class ScriptInput
    {
        private readonly ScriptExecutor _executor;
        private readonly IScriptIo _io;

        public ScriptInput(Weave weave, string script, IScriptIo? io = null)
        {
            _io = io ?? new ConsoleScriptIo();

            // Tokenize and parse
            var lexer = new Lexer(script);
            var parser = new Parser(lexer.Tokenize());
            var blocks = parser.ParseIntoCategorizedBlocks();
            var statements = blocks
                .Select(line => line.Statement) // get the string
                .Where(s => s != null) // optional: skip nulls
                .ToList();

            _executor = new ScriptExecutor(weave, statements);
        }

        /// <summary>
        /// Runs the script to completion.
        /// </summary>
        public void Run()
        {
            while (!_executor.IsFinished)
            {
                var result = _executor.ExecuteNext();

                if (!string.IsNullOrEmpty(result.Message))
                    _io.WriteOutput(result.Message);

                if (result.Feedback != null)
                {
                    var input = _io.ReadInput(result.Feedback.Prompt);
                    _executor.ExecuteNext(input);
                }
            }
        }

        /// <summary>
        /// Executes the script step by step, returning the last result.
        /// Useful if you want to control each step externally.
        /// </summary>
        /// <returns>CommandResult for this step</returns>
        public CommandResult Step()
        {
            var result = _executor.ExecuteNext();

            if (!string.IsNullOrEmpty(result.Message))
                _io.WriteOutput(result.Message);

            if (result.Feedback != null)
            {
                var input = _io.ReadInput(result.Feedback.Prompt);
                result = _executor.ExecuteNext(input);
            }

            return result;
        }
    }

    /// <summary>
    /// Provides a simple console-based IO device for scripts.
    /// </summary>
    public sealed class ConsoleScriptIo : IScriptIo
    {
        public string ReadInput(string prompt)
        {
            Console.Write(prompt + " ");
            return Console.ReadLine() ?? string.Empty;
        }

        public void WriteOutput(string message)
        {
            Console.WriteLine(message);
        }
    }
}