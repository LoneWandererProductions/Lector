/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        ScriptInput.cs
 * PURPOSE:     Script input handler with feedback support.
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

            // Tokenize and parse into ScriptNodes
            var lexer = new Lexer(script);
            var parser = new Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes(); // <-- new method that returns List<ScriptNode>

            // Flatten ScriptNodes to simple (Category, Statement) tuples
            var statements = FlattenNodes(nodes).ToList();

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

        private static IEnumerable<(string Category, string Statement)> FlattenNodes(List<ScriptNode> nodes)
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case LabelNode ln:
                        yield return ("Label", ln.Name);

                        break;
                    case GotoNode gn:
                        yield return ("Goto", gn.Target);

                        break;
                    case CommandNode cn:
                        yield return ("Command", cn.Command);

                        break;
                    case AssignmentNode an:
                        yield return ("Assignment", $"{an.Variable} = {an.Expression}");

                        break;
                    case IfNode ifn:
                        yield return ("If_Condition", ifn.Condition);

                        foreach (var child in FlattenNodes(ifn.TrueBranch))
                            yield return child;

                        if (ifn.FalseBranch != null)
                            foreach (var child in FlattenNodes(ifn.FalseBranch))
                                yield return child;

                        break;
                    case DoWhileNode dw:
                        foreach (var child in FlattenNodes(dw.Body))
                            yield return child;

                        yield return ("While_Condition", dw.Condition);

                        break;
                }
            }
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