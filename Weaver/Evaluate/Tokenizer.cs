using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weaver.Evaluate
{
    internal static class Tokenizer
    {
        private static readonly string[] MultiOps =
{
            "==", "!=", ">=", "<="
        };

        internal static IEnumerable<string> Tokenize(string expr)
        {
            var token = new StringBuilder();
            int i = 0;

            while (i < expr.Length)
            {
                char c = expr[i];

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
                bool matchedMulti = false;
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
