/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Evaluate
 * FILE:        ExpressionEvaluator.cs
 * PURPOSE:     Evaluates simple expressions (numeric, logical, registry-aware).
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ScriptEngine;

//TODO needs rework to support more complex expressions, parentheses, operator precedence, etc.

namespace Weaver.Evaluate
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
        /// The operators
        /// </summary>
        private static readonly Dictionary<string, (int precedence, bool rightAssociative, int arity)> Operators = new()
        {
            ["!"] = (5, true, 1),

            ["*"] = (4, false, 2),
            ["/"] = (4, false, 2),

            ["+"] = (3, false, 2),
            ["-"] = (3, false, 2),

            [">"] = (2, false, 2),
            ["<"] = (2, false, 2),
            [">="] = (2, false, 2),
            ["<="] = (2, false, 2),
            ["=="] = (2, false, 2),
            ["!="] = (2, false, 2),

            ["&&"] = (1, false, 2),
            ["||"] = (0, false, 2),
        };

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

            // single-variable evaluation
            if (ExpressionHelpers.TryEvaluateVariableAsBool(expression, _registry, out var single))
                return single;

            // try to interpret as a variable
            if(_registry != null) expression = ExpressionHelpers.ReplaceVariablesInExpression(expression, _registry);

            // --- comparison operators ---
            //var parts = Tokenizer.Tokenize(expression).ToArray();
            var parts = Lexer.Tokenize(expression).ToArray();

            if (parts.Length == 2 && parts[0] == ScriptConstants.LogicalNotSymbol) // "!"
            {
                var val = GetValue(parts[1]);
                return !Convert.ToBoolean(val);
            }

            if (parts.Length == 3)
            {
                var left = GetValue(parts[0]);
                var op = parts[1];
                var right = GetValue(parts[2]);

                return op switch
                {
                    ScriptConstants.EqualEqual => Equals(left, right),
                    ScriptConstants.BangEqual => !Equals(left, right),
                    ScriptConstants.Greater => Compare(left, right) > 0,
                    ScriptConstants.Less => Compare(left, right) < 0,
                    ScriptConstants.GreaterEqual => Compare(left, right) >= 0,
                    ScriptConstants.LessEqual => Compare(left, right) <= 0,
                    ScriptConstants.LogicalAndSymbol => Convert.ToBoolean(left) && Convert.ToBoolean(right),
                    ScriptConstants.LogicalOrSymbol => Convert.ToBoolean(left) || Convert.ToBoolean(right),
                    _ => throw new ArgumentException($"Unsupported operator: {op}")
                };
            }

            // unary NOT with tokens
            if (parts.Length == 2 && parts[0].Equals("not", StringComparison.OrdinalIgnoreCase))
            {
                var val = GetValue(parts[1]);
                return !Convert.ToBoolean(val);
            }

            // fallback numeric
            return EvaluateNumeric(expression) != 0;
        }

        /// <inheritdoc />
        public double EvaluateNumeric(string expression)
        {
            //var tokens = Tokenizer.Tokenize(expression);

            var tokens = Lexer.Tokenize(expression);
            var rpn = ToRpn(tokens);
            return EvaluateRpn(rpn);
        }

        private List<string> ToRpn(IEnumerable<string> tokens)
        {
            var output = new List<string>();
            var ops = new Stack<string>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out _) || IsNumericVariable(token))
                {
                    output.Add(token);
                    continue;
                }

                if (token == "(")
                {
                    ops.Push(token);
                    continue;
                }

                if (token == ")")
                {
                    while (ops.Count > 0 && ops.Peek() != "(")
                        output.Add(ops.Pop());

                    ops.Pop(); // remove "("
                    continue;
                }

                if (!Operators.TryGetValue(token, out var op1))
                    continue;

                while (ops.Count > 0 && Operators.TryGetValue(ops.Peek(), out var op2))
                {
                    if ((op1.rightAssociative && op1.precedence < op2.precedence) ||
                        (!op1.rightAssociative && op1.precedence <= op2.precedence))
                    {
                        output.Add(ops.Pop());
                    }
                    else break;
                }

                ops.Push(token);
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
                    stack.Push(GetNumericValue(token));
                }
                else if (token == ScriptConstants.LogicalNotSymbol) // "!"
                {
                    var val = stack.Pop();
                    stack.Push(val == 0 ? 1 : 0);
                }
                else
                {
                    // binary operators
                    var b = stack.Pop();
                    var a = stack.Pop();

                    stack.Push(token switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        "&&" => (a != 0 && b != 0) ? 1 : 0,
                        "||" => (a != 0 || b != 0) ? 1 : 0,
                        "==" => a == b ? 1 : 0,
                        "!=" => a != b ? 1 : 0,
                        ">" => a > b ? 1 : 0,
                        "<" => a < b ? 1 : 0,
                        ">=" => a >= b ? 1 : 0,
                        "<=" => a <= b ? 1 : 0,
                        _ => throw new Exception($"Unknown operator in RPN: {token}")
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
            return expression.Contains(ScriptConstants.LogicalAndSymbol, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalOrSymbol, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalNotSymbol, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalAndWord, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalOrWord, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalNotWord, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.EqualEqual)
                   || expression.Contains(ScriptConstants.BangEqual)
                   || expression.Contains(ScriptConstants.GreaterEqual)
                   || expression.Contains(ScriptConstants.LessEqual)
                   || expression.Contains(ScriptConstants.Greater)
                   || expression.Contains(ScriptConstants.Less);
        }

    }
}