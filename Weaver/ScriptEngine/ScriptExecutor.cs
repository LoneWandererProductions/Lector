/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ScriptExecutor.cs
 * PURPOSE:     Executes parsed script statements using Weave
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

//TODO the node exit for DoWhile is currently not implemented in the ScriptExecutor

using System.Diagnostics;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Executes a list of script statements sequentially using Weave.
    /// Supports goto labels, do{...}while(condition) loops, and if/else.
    /// Handles feedback by pausing execution until input is provided.
    /// </summary>
    public sealed class ScriptExecutor
    {
        /// <summary>
        /// The weave
        /// </summary>
        private readonly Weave _weave;

        /// <summary>
        /// The evaluator
        /// </summary>
        private readonly IEvaluator _evaluator;

        /// <summary>
        /// The statements
        /// </summary>
        private readonly List<(string Category, string)> _statements;

        /// <summary>
        /// The label positions
        /// </summary>
        private readonly Dictionary<string, int> _labelPositions;

        /// <summary>
        /// The position
        /// </summary>
        private int _position;

        /// <summary>
        /// The debug
        /// </summary>
        private readonly bool _debug;

        /// <summary>
        /// The pending feedback
        /// </summary>
        private FeedbackRequest? _pendingFeedback;

        /// <summary>
        /// Only store the start index of do-while loops
        /// </summary>
        private readonly Stack<int> _doWhileStack = new();

        //private readonly Stack<(int BodyStart, int WhileIndex)> _doWhileStack = new();


        /// <summary>
        /// Current instruction index.
        /// </summary>
        /// <value>
        /// The instruction pointer.
        /// </value>
        public int InstructionPointer => _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptExecutor" /> class.
        /// </summary>
        /// <param name="weave">The weave.</param>
        /// <param name="statements">The statements.</param>
        /// <param name="debug">if set to <c>true</c> [debug].</param>
        public ScriptExecutor(Weave weave, List<(string Category, string)> statements, bool debug = false)
        {
            //now weave also holds all the variables and evaluator commands we need.
            _weave = weave;
            _evaluator = new ExpressionEvaluator(_weave.Runtime.Variables);
            _statements = statements ?? new List<(string Category, string)>();
            _position = 0;
            _debug = debug;

            _labelPositions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < _statements.Count; i++)
            {
                if (_statements[i].Category == "Label")
                    _labelPositions[_statements[i].Item2] = i;
            }
        }

        /// <summary>
        /// True if the script has executed all statements and no feedback is pending.
        /// </summary>
        public bool IsFinished => _position >= _statements.Count && _pendingFeedback == null;

        /// <summary>
        /// Executes the next statement in the script or resumes after feedback.
        /// </summary>
        /// <param name="feedbackInput">Optional input if resuming after feedback.</param>
        /// <param name="maxIterations">Optional safety limit to prevent infinite loops.</param>
        /// <returns>CommandResult from Weave.</returns>
        public CommandResult ExecuteNext(string? feedbackInput = null, int? maxIterations = null)
        {
            if (_pendingFeedback != null)
            {
                if (feedbackInput == null)
                    throw new InvalidOperationException("Feedback input required to continue script.");

                var result = _weave.ProcessInput(feedbackInput);
                if (result.Feedback == null)
                    _pendingFeedback = null;

                return result;
            }

            var iterCount = 0;

            while (_position < _statements.Count)
            {
                if (maxIterations.HasValue && iterCount++ >= maxIterations.Value)
                    return CommandResult.Fail("Max iteration count reached.");

                var (category, stmt) = _statements[_position];

                // Only skip empty COMMANDS, never control opcodes
                if (category == "Command" && string.IsNullOrWhiteSpace(stmt))
                {
                    _position++;
                    continue;
                }

                // Debug everything
                if (_debug) DebugLine(category, stmt);

                switch (category)
                {
                    case "Goto":
                        if (_labelPositions.TryGetValue(stmt!, out var pos))
                            _position = pos + 1;
                        else
                        {
                            _position++;
                            return CommandResult.Fail($"Label '{stmt}' not found.");
                        }
                        continue;

                    case "Label":
                        _position++;
                        continue;

                    case "Do_Open":
                        // Push the index of the first statement in the loop body
                        _doWhileStack.Push(_position + 1);
                        Trace.WriteLine($"Do_Open at {_position}, push {_position + 1}");
                        _position++;
                        continue;

                    case "While_Condition":
                        if (_doWhileStack.Count == 0)
                        {
                            Trace.WriteLine($"--- Warning: While_Condition without Do_Open at Pos={_position} ---");
                            _position++;
                            continue;
                        }

                        int bodyStart = _doWhileStack.Peek();  // <- Peek, not Pop!
                        bool cond = _evaluator.Evaluate(stmt!);

                        Trace.WriteLine($"While_Condition at {_position}, bodyStart={bodyStart}, cond={cond}");

                        if (cond)
                        {
                            _position = bodyStart; // repeat loop
                        }
                        else
                        {
                            _doWhileStack.Pop();   // done with loop
                            _position++;           // move past While_Condition
                        }

                        continue;

                    case "Do_End":
                        _position++; // just move past, no stack changes
                        continue;

                    case "If_Condition":
                        bool ifcond = _evaluator.Evaluate(stmt!);
                        _position++;
                        if (!ifcond)
                        {
                            int depth = 0;
                            while (_position < _statements.Count)
                            {
                                var (cat, _) = _statements[_position];
                                if (cat == "If_Condition") { depth++; _position++; }
                                else if (cat == "If_End" && depth == 0) { _position++; break; }
                                else if (cat == "Else_Open" && depth == 0) { _position++; break; }
                                else if ((cat == "If_End" || cat == "Else_End") && depth > 0) { depth--; _position++; }
                                else { _position++; }
                            }
                        }
                        continue;

                    case "Else_Open":
                    case "If_End":
                    case "Else_End":
                        _position++;
                        continue;

                    default:
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

        /// <summary>
        /// Debugs the line.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="stmt">The statement.</param>
        private void DebugLine(string category, string stmt)
        {
            Trace.WriteLine($"{"Parser:" + category.PadRight(16)} : {stmt ?? "<null>"} | Pos={_position} | Stack=[{string.Join(",", _doWhileStack)}]");
        }
    }
}