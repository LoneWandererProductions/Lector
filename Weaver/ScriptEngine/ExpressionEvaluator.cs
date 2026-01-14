/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ExpressionEvaluator.cs
 * PURPOSE:     Evaluates simple expressions (numeric, logical, registry-aware).
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Data;
using System.Text;
using Weaver.Interfaces;
using Weaver.Messages;

//TODO needs rework to support more complex expressions, parentheses, operator precedence, etc.

namespace Weaver.ScriptEngine
{
    /// <inheritdoc />
    /// <summary>
    /// Evaluates simple logical or arithmetic expressions.
    /// Supports registry variables when available.
    /// </summary>
    internal sealed class ExpressionEvaluator : IEvaluator
    {
        /// <summary>
        /// Optional variable registry.
        /// </summary>
        private readonly IVariableRegistry? _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator"/> class.
        /// </summary>
        /// <param name="registry">Optional variable registry.</param>
        public ExpressionEvaluator(IVariableRegistry? registry = null)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">Invalid or unsupported expression.</exception>
        public bool Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be empty.", nameof(expression));

            expression = expression.Trim();

            // direct boolean literals
            if (expression.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;

            if (expression.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;

            var tokens = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // (existing operator handling)
            if (tokens.Length == 3)
            {
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
                    ScriptConstants.LogicalAnd => Convert.ToBoolean(left) && Convert.ToBoolean(right),
                    ScriptConstants.LogicalOr => Convert.ToBoolean(left) || Convert.ToBoolean(right),
                    _ => throw new ArgumentException($"Unsupported operator: {op}")
                };
            }

            // unary NOT
            if (tokens.Length == 2 && tokens[0].Equals("not", StringComparison.OrdinalIgnoreCase))
            {
                var val = GetValue(tokens[1]);
                return !Convert.ToBoolean(val);
            }

            // fallback numeric
            return EvaluateNumeric(expression) != 0;
        }

        /// <inheritdoc />
        public double EvaluateNumeric(string expression)
        {
            var tokens = Tokenize(expression);
            var rpn = ToRpn(tokens);
            return EvaluateRpn(rpn);
        }
        private IEnumerable<string> Tokenize(string expr)
        {
            var token = new StringBuilder();

            foreach (var c in expr)
            {
                if (char.IsWhiteSpace(c))
                    continue;

                if (char.IsLetterOrDigit(c) || c == '.')
                {
                    token.Append(c);
                }
                else
                {
                    if (token.Length > 0)
                    {
                        yield return token.ToString();
                        token.Clear();
                    }
                    yield return c.ToString();
                }
            }

            if (token.Length > 0)
                yield return token.ToString();
        }
        private List<string> ToRpn(IEnumerable<string> tokens)
        {
            var output = new List<string>();
            var ops = new Stack<string>();

            int Precedence(string op) => op switch
            {
                "+" or "-" => 1,
                "*" or "/" => 2,
                _ => 0
            };

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out _) || IsNumericVariable(token))
                {
                    output.Add(token);
                }
                else if ("+-*/".Contains(token))
                {
                    while (ops.Count > 0 && Precedence(ops.Peek()) >= Precedence(token))
                        output.Add(ops.Pop());
                    ops.Push(token);
                }
                else if (token == "(")
                {
                    ops.Push(token);
                }
                else if (token == ")")
                {
                    while (ops.Peek() != "(")
                        output.Add(ops.Pop());
                    ops.Pop();
                }
            }

            while (ops.Count > 0)
                output.Add(ops.Pop());

            return output;
        }

        private double EvaluateRpn(List<string> rpn)
        {
            var stack = new Stack<double>();

            foreach (var token in rpn)
            {
                if (double.TryParse(token, out var num))
                {
                    stack.Push(num);
                }
                else if (IsNumericVariable(token))
                {
                    stack.Push(GetNumericValue(token)); // lookup registry
                }
                else
                {
                    var b = stack.Pop();
                    var a = stack.Pop();

                    stack.Push(token switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        _ => throw new Exception("Unknown operator")
                    });
                }
            }

            return stack.Pop();
        }

        private bool IsNumericVariable(string token)
        {
            if (_registry == null)
                return false;

            return _registry.TryGet(token, out var val, out var type)
                   && (type == EnumTypes.Wint || type == EnumTypes.Wdouble || type == EnumTypes.Wbool);
        }

        private double GetNumericValue(string token)
        {
            if (_registry != null && _registry.TryGet(token, out var val, out var type))
            {
                return val switch
                {
                    int i => i,
                    long l => l,
                    double d => d,
                    float f => f,
                    bool b => b ? 1 : 0,
                    _ => throw new InvalidOperationException($"Cannot convert value of type {val.GetType()} to number")
                };
            }

            if (double.TryParse(token, out var num))
                return num;

            throw new InvalidOperationException($"Invalid numeric token: {token}");
        }

        /// <summary>
        /// Retrieve a token value (number, variable, or literal string).
        /// </summary>
        private object GetValue(string token)
        {
            // Try numeric literal
            if (double.TryParse(token, out var num))
                return num;

            // Try registry lookup
            if (_registry != null && _registry.TryGet(token, out var val, out _))
                return val!;

            // Treat as literal string
            return token;
        }

        /// <summary>
        /// Compare two numeric values.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>Compare result.</returns>
        private static int Compare(object a, object b)
        {
            var da = Convert.ToDouble(a);
            var db = Convert.ToDouble(b);
            return da.CompareTo(db);
        }

        /// <inheritdoc />
        public bool IsBooleanExpression(string expression)
        {
            return expression.Contains(ScriptConstants.LogicalAnd, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalOr, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalNot, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.EqualEqual)
                   || expression.Contains(ScriptConstants.BangEqual)
                   || expression.Contains(ScriptConstants.GreaterEqual)
                   || expression.Contains(ScriptConstants.LessEqual)
                   || expression.Contains(ScriptConstants.Greater)
                   || expression.Contains(ScriptConstants.Less);
        }
    }
}