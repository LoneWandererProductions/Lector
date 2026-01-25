/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Evaluate
 * FILE:        ExpressionHelpers.cs
 * PURPOSE:     Evaluation Helpers for expression evaluation. Is shared between Evaluator and Commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Globalization;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Weaver.Evaluate
{
    public static class ExpressionHelpers
    {
        /// <summary>
        /// Converts a variable object from the registry to a boolean value.
        /// </summary>
        /// <param name="vm">The vm.</param>
        /// <returns>Converts bool to a more calculateable numeric.</returns>
        public static bool ToBool(VmValue vm)
        {
            return vm.Type switch
            {
                EnumTypes.Wbool => vm.Bool,
                EnumTypes.Wint => vm.Int64 != 0,
                EnumTypes.Wdouble => vm.Double != 0.0,
                EnumTypes.Wstring => !string.IsNullOrEmpty(vm.String),
                _ => throw new ArgumentException($"Unsupported variable type: {vm.Type}")
            };
        }

        /// <summary>
        /// Replaces all variables in the expression with their literal values.
        /// Returns the replaced expression string.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="registry">The registry.</param>
        /// <returns>Expression replaced with values from registry.</returns>
        public static string ReplaceVariablesInExpression(string expression, IVariableRegistry registry)
        {
            if (registry == null || string.IsNullOrWhiteSpace(expression))
                return expression;

            foreach (var variable in registry.GetAll())
            {
                var key = variable.Key;
                var (valueObj, valueType) = variable.Value;

                if (valueObj == null)
                    continue;

                string replacement = valueType switch
                {
                    EnumTypes.Wint => Convert.ToInt64(valueObj).ToString(),
                    EnumTypes.Wdouble => Convert.ToDouble(valueObj).ToString(CultureInfo.InvariantCulture),
                    EnumTypes.Wbool => Convert.ToBoolean(valueObj) ? "1" : "0",
                    EnumTypes.Wstring => valueObj.ToString() ?? "",
                    _ => valueObj.ToString() ?? ""
                };

                expression = expression.Replace(key, replacement, StringComparison.OrdinalIgnoreCase);
            }

            return expression;
        }

        /// <summary>
        /// Attempts to get a variable as a bool, returns false if not found.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="registry">The registry.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns>Checks if variable can be found in registry.</returns>
        public static bool TryEvaluateVariableAsBool(string name, IVariableRegistry? registry, out bool value)
        {
            value = false;
            if (registry == null || string.IsNullOrWhiteSpace(name))
                return false;

            if (registry.TryGet(name, out var vm))
            {
                value = ToBool(vm);
                return true;
            }

            return false;
        }
    }
}
