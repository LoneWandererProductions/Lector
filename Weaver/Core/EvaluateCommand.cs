﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        EvaluateCommand.cs
 * PURPOSE:     Does double duty as internal command and extension to evaluate expressions. It can be either used as calculator or expression evaluator.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Globalization;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <summary>
    /// Simple Commmand to evaluate expressions or do simple calculations.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    internal class EvaluateCommand : ICommand
    {
        /// <inheritdoc />
        public string Name => "Evaluate";

        /// <inheritdoc />
        public string Description =>
            "In the Scriptengine this command provides an easier way to evaluate expressions or do simple calculation and expression evaluation.";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        /// <summary>
        /// The registry
        /// </summary>
        private readonly IVariableRegistry? _registry;

        /// <summary>
        /// The evaluator
        /// </summary>
        private readonly IEvaluator _evaluator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluateCommand" /> class.
        /// </summary>
        /// <param name="evaluator">The evaluator.</param>
        /// <param name="registry">The registry. Optional only relevant for the Scriptengine.</param>
        public EvaluateCommand(IEvaluator evaluator, IVariableRegistry? registry = null)
        {
            _registry = registry;
            _evaluator = evaluator;
        }

        /// <inheritdoc />
        public CommandResult Execute(string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("evaluate() requires at least 1 argument: the expression to evaluate.");

            var expression = args[0];
            var targetVar = args.Length > 1 ? args[1] : null;

            // Resolve variables from registry
            if (_registry != null)
            {
                foreach (var variable in _registry.GetAll())
                {
                    if (variable.Value.Value == null)
                        continue;

                    var key = variable.Key;
                    var val = variable.Value.Value.ToString() ?? "0";
                    expression = expression.Replace(key, val, StringComparison.OrdinalIgnoreCase);
                }
            }

            object? result;
            EnumTypes type;

            // Detect if expression looks like boolean
            var isBooleanExpr = _evaluator.IsBooleanExpression(expression);

            try
            {
                if (isBooleanExpr)
                {
                    result = _evaluator.Evaluate(expression);
                    type = EnumTypes.Wbool;
                }
                else
                {
                    result = _evaluator.EvaluateNumeric(expression);
                    type = EnumTypes.Wdouble;
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(
                    $"Cannot evaluate expression '{expression}': {ex.Message}. " +
                    "Usage: evaluate(<expression> [, <targetVar>])"
                );
            }

            // Optional target variable storage
            if (!string.IsNullOrWhiteSpace(targetVar))
            {
                if (_registry == null)
                {
                    return CommandResult.Fail(
                        "Cannot store result: registry not provided. " +
                        "Usage: evaluate(<expression>, <targetVar>) requires registry in constructor."
                    );
                }

                _registry.Set(targetVar, result!, type);
                return new CommandResult
                {
                    Success = true,
                    Message = $"Stored '{result}' in '{targetVar}'.",
                    Type = type,
                    Value = result
                };
            }

            // Return result
            var message = type switch
            {
                EnumTypes.Wbool => result?.ToString() ?? "false",
                EnumTypes.Wdouble => Convert.ToDouble(result).ToString(CultureInfo.InvariantCulture),
                _ => result?.ToString() ?? "null"
            };

            return new CommandResult
            {
                Success = true,
                Message = message,
                Type = type,
                Value = result
            };
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, string[] args)
        {
            return CommandResult.Fail("No extensions available for evaluate.");
        }
    }
}