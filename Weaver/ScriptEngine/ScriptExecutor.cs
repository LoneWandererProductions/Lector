/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ScriptExecutor.cs
 * PURPOSE:     Executes parsed script statements using Weave
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core;
using Weaver.Messages;

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Executes a list of script statements sequentially using Weave.
    /// Supports goto labels and do{...}while(condition) loops.
    /// Handles feedback by pausing execution until input is provided.
    /// </summary>
    public sealed class ScriptExecutor
    {
        /// <summary>
        /// The weave command executor.
        /// </summary>
        private readonly Weave _weave;

        private readonly VariableRegistry _registry;
        private readonly List<(string Category, string? Statement)> _statements;

        private readonly Dictionary<string, int> _labelPositions;
        private int _position;
        private FeedbackRequest? _pendingFeedback;

        private readonly Stack<(int loopStart, string condition)> _doWhileStack = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptExecutor"/> class.
        /// </summary>
        /// <param name="weave">The weave.</param>
        /// <param name="statements">The statements.</param>
        public ScriptExecutor(Weave weave, List<(string Category, string? Statement)> statements)
        {
            _weave = weave;
            _registry = new VariableRegistry();

            //add our custom commands for variable management
            weave.Register(new SetValue(_registry));
            weave.Register(new GetValue(_registry));
            weave.Register(new DeleteValue(_registry));
            weave.Register(new Memory(_registry));

            _statements = statements;
            _labelPositions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // pre-scan labels
            for (int i = 0; i < _statements.Count; i++)
            {
                var (category, stmt) = _statements[i];
                if (category == "Label" && !string.IsNullOrEmpty(stmt))
                {
                    _labelPositions[stmt] = i;
                }
            }
        }

        /// <summary>
        /// True if the script has executed all statements.
        /// </summary>
        public bool IsFinished => _position >= _statements.Count && _pendingFeedback == null;

        /// <summary>
        /// Executes the next statement in the script or resumes after feedback.
        /// </summary>
        /// <param name="feedbackInput">Optional input if resuming after feedback.</param>
        /// <returns>CommandResult from Weave.</returns>
        public CommandResult ExecuteNext(string? feedbackInput = null)
        {
            // Resume feedback
            if (_pendingFeedback != null)
            {
                if (feedbackInput == null)
                    throw new InvalidOperationException("Feedback input required to continue script.");

                var result = _weave.ProcessInput(feedbackInput);
                if (result.Feedback == null)
                    _pendingFeedback = null; // feedback resolved

                return result;
            }

            // Execute statements sequentially
            while (_position < _statements.Count)
            {
                var (category, stmt) = _statements[_position];

                if (string.IsNullOrWhiteSpace(stmt))
                {
                    _position++;
                    continue;
                }

                switch (category)
                {
                    case "Goto":
                        if (_labelPositions.TryGetValue(stmt!, out var pos))
                        {
                            _position = pos + 1;
                            continue;
                        }
                        return CommandResult.Fail($"Label '{stmt}' not found.");

                    case "Label":
                        _position++; // labels are just markers
                        continue;

                    case "Do_Open":
                        _doWhileStack.Push((_position, string.Empty));
                        _position++;
                        continue;

                    case "While_Condition":
                        var top = _doWhileStack.Pop();
                        var condResult = _weave.ProcessInput(stmt!);
                        if (!condResult.Success)
                            return CommandResult.Fail($"Loop condition failed: {condResult.Message}");

                        if (condResult.Message.Equals("true", StringComparison.OrdinalIgnoreCase))
                            _position = top.loopStart + 1;
                        else
                            _position++; // exit loop
                        continue;

                    default: // "Command", "Assignment", etc.
                        var result = _weave.ProcessInput(stmt!);
                        if (result.Feedback != null)
                        {
                            _pendingFeedback = result.Feedback;
                            return result;
                        }
                        _position++;
                        return result;
                }
            }

            return new CommandResult
            {
                Success = true,
                Message = "Script finished.",
                Feedback = null
            };
        }
    }
}