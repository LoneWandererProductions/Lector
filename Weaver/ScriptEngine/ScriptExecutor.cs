/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ScriptExecutor.cs
 * PURPOSE:     Executes parsed script statements using Weave
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

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
        private readonly Weave _weave;
        private readonly List<string?> _statements;
        private readonly Dictionary<string, int> _labelPositions;
        private int _position;
        private FeedbackRequest? _pendingFeedback;

        private readonly Stack<(int loopStart, string condition)> _doWhileStack = new();

        public ScriptExecutor(Weave weave, List<string?> statements)
        {
            _weave = weave;
            _statements = statements;
            _labelPositions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // pre-scan labels
            for (int i = 0; i < _statements.Count; i++)
            {
                var stmt = _statements[i];
                if (stmt.StartsWith("label ", StringComparison.OrdinalIgnoreCase))
                {
                    var label = stmt.Substring(6).Trim();
                    _labelPositions[label] = i;
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
                var stmt = _statements[_position].Trim();

                // Skip empty statements
                if (string.IsNullOrWhiteSpace(stmt))
                {
                    _position++;
                    continue;
                }

                // Handle goto
                if (stmt.StartsWith("goto ", StringComparison.OrdinalIgnoreCase))
                {
                    var label = stmt.Substring(5).Trim();
                    if (_labelPositions.TryGetValue(label, out var pos))
                    {
                        _position = pos + 1;
                        continue;
                    }
                    else
                    {
                        return CommandResult.Fail($"Label '{label}' not found.");
                    }
                }

                // Handle do { ... } while(condition)
                if (stmt.StartsWith("do", StringComparison.OrdinalIgnoreCase))
                {
                    _doWhileStack.Push((_position, string.Empty));
                    _position++;
                    continue;
                }

                if (stmt.StartsWith("while(", StringComparison.OrdinalIgnoreCase) && _doWhileStack.Count > 0)
                {
                    var top = _doWhileStack.Pop();
                    var condition = stmt.Substring(6, stmt.Length - 7).Trim(); // remove while( and )
                    _doWhileStack.Push((top.loopStart, condition));

                    // evaluate condition using Weave
                    var condResult = _weave.ProcessInput(condition);
                    if (!condResult.Success)
                        return CommandResult.Fail($"Loop condition failed: {condResult.Message}");

                    if (condResult.Message.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        _position = top.loopStart + 1; // jump to first statement inside do block
                    }
                    else
                    {
                        _doWhileStack.Pop(); // loop ends
                        _position++;
                    }

                    continue;
                }

                // Normal command execution
                var result = _weave.ProcessInput(stmt);

                // If feedback is requested, pause script
                if (result.Feedback != null)
                {
                    _pendingFeedback = result.Feedback;
                    return result;
                }

                _position++;
                return result; // return result for this statement
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