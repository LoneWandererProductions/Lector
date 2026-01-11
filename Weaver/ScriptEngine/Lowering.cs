/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        Lowering.cs
 * PURPOSE:     Converts the ScriptNode tree into a linear sequence for execution
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Script lowerer that flattens ScriptNode trees into linear sequences.
    /// </summary>
    internal static class Lowering
    {
        /// <summary>
        /// Flattens the ScriptNode tree into a linear sequence with debug categories.
        /// Includes pseudo-categories like Do_Open, Do_End, If_Open, If_End, Else_Open, Else_End.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="rewrite">A debug switch for rewrite.</param>
        /// <returns>Execution blocks.</returns>
        /// <exception cref="System.Exception">Unsupported assignment expression: '{expr}'</exception>
        public static IEnumerable<(string Category, string? Statement)> ScriptLowerer(
            IEnumerable<ScriptNode> nodes, bool? rewrite = true, string branchPath = "")
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
                        {
                            var expr = an.Expression?.Trim() ?? "";
                            var varName = an.Variable?.Trim() ?? "";

                            if (rewrite ?? true)
                            {
                                if (IsCommandCall(expr))
                                {
                                    yield return ("Command_Rewrite", $"{expr}.Store({varName})");
                                }
                                else if (IsSimpleExpression(expr))
                                {
                                    yield return ("Command_Rewrite", $"EvaluateCommand({expr}, {varName})");
                                }
                                else
                                {
                                    throw new Exception($"Unsupported assignment expression: '{expr}'");
                                }
                            }
                            else
                            {
                                yield return ("Assignment", $"{varName} = {expr}");
                            }

                            break;
                        }

                    case IfNode ifn:
                        // Emit condition
                        yield return ("If_Condition", ifn.Condition);

                        // Open true branch
                        yield return ("If_Open", branchPath + "T");

                        foreach (var child in ScriptLowerer(ifn.TrueBranch, rewrite, branchPath + "T"))
                            yield return child;

                        yield return ("If_End", branchPath + "T");

                        if (ifn.FalseBranch != null)
                        {
                            yield return ("Else_Open", branchPath + "F");

                            foreach (var child in ScriptLowerer(ifn.FalseBranch, rewrite, branchPath + "F"))
                                yield return child;

                            yield return ("Else_End", branchPath + "F");
                        }

                        break;

                    case DoWhileNode dw:
                        yield return ("Do_Open", null);

                        foreach (var child in ScriptLowerer(dw.Body, rewrite, branchPath))
                            yield return child;

                        yield return ("Do_End", null);
                        yield return ("While_Condition", dw.Condition);
                        break;
                }
            }
        }


        /// <summary>
        /// Determines whether [is command call] [the specified expr].
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns>
        ///   <c>true</c> if [is command call] [the specified expr]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCommandCall(string expr)
        {
            // Simple: starts with identifier and ends in (...)
            int paren = expr.IndexOf('(');
            return paren > 0 && expr.EndsWith(")");
        }

        /// <summary>
        /// Determines whether [is simple expression] [the specified expr].
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns>
        ///   <c>true</c> if [is simple expression] [the specified expr]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsSimpleExpression(string expr)
        {
            // no nested parentheses allowed
            if (expr.Contains("(")) return false;
            if (expr.Contains(")")) return false;

            // allow digits, identifiers, basic operators
            foreach (char c in expr)
            {
                if (!char.IsLetterOrDigit(c) && "+-*/ ".IndexOf(c) < 0)
                    return false;
            }

            return true;
        }
    }
}