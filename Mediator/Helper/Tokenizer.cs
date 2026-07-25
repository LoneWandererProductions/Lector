/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Helper
 * FILE:        Tokenizer.cs
 * PURPOSE:     Old Tokenizer for expression parsing, soon to be replaced. But here it is a test component.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text;

namespace Mediator.Helper
{
    /// <summary>
    /// Old tokenizer for testing.
    /// </summary>
    internal static class Tokenizer
    {
        /// <summary>
        /// The multi ops
        /// </summary>
        private static readonly string[] MultiOps =
        {
            "==", "!=", ">=", "<="
        };

        /// <summary>
        /// Tokenizes the specified expr.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns>List of tokens.</returns>
        internal static IEnumerable<string> Tokenize(string? expr)
        {
            var token = new StringBuilder();
            var i = 0;

            while (i < expr.Length)
            {
                var c = expr[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // numeric or identifier
                if (char.IsLetterOrDigit(c) || c == '.')
                {
                    token.Append(c);
                    i++;
                    continue;
                }

                // Flush current token
                if (token.Length > 0)
                {
                    yield return token.ToString();

                    token.Clear();
                }

                // Multi-char operator detection
                var matchedMulti = false;
                foreach (var op in MultiOps)
                {
                    if (expr.AsSpan(i).StartsWith(op))
                    {
                        yield return op;

                        i += op.Length;
                        matchedMulti = true;
                        break;
                    }
                }

                if (matchedMulti)
                    continue;

                // Single char operator
                yield return c.ToString();

                i++;
            }

            if (token.Length > 0)
                yield return token.ToString();
        }
    }
}