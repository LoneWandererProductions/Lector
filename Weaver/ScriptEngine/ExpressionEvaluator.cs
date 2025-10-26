/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ExpressionEvaluator.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.ScriptEngine
{
    internal sealed class ExpressionEvaluator
    {
        private readonly VariableRegistry _registry;

        public ExpressionEvaluator(VariableRegistry registry)
        {
            _registry = registry;
        }

        public bool Evaluate(string expression)
        {
            // simple parser for expressions like "x > 0", "score == 100"
            var tokens = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 3)
                throw new ArgumentException($"Invalid expression: {expression}");

            var left = GetValue(tokens[0]);
            var op = tokens[1];
            var right = GetValue(tokens[2]);

            return op switch
            {
                ScriptConstants.EqualEqual => Equals(left, right),
                ScriptConstants.BangEqual => !Equals(left, right),
                ScriptConstants.Greater => Compare(left, right) > 0,
                ScriptConstants.Less => Compare(left, right) < 0,
                ScriptConstants.GreaterEqual => Compare(left, right) >= 0,
                ScriptConstants.LessEqual => Compare(left, right) <= 0,
                _ => throw new ArgumentException($"Unsupported operator: {op}")
            };
        }

        private object GetValue(string token)
        {
            if (double.TryParse(token, out var num)) return num;
            if (_registry.TryGet(token, out var val, out _)) return val!;

            return token; // treat as literal string
        }

        /// <summary>
        /// Compares the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>If both values are equal</returns>
        private static int Compare(object a, object b)
        {
            var da = Convert.ToDouble(a);
            var db = Convert.ToDouble(b);
            return da.CompareTo(db);
        }
    }
}