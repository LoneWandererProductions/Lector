using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weaver.Evaluate;
using Weaver.ScriptEngine;

namespace Mediator.Scripting
{
    [TestClass]
    public class ScriptInternals
    {
        [TestMethod]
        public void CompareTokenizers()
        {
            string expr = "1 + 2.5 >= 3";

            var oldTokens = Tokenizer.Tokenize(expr).ToList();
            var newTokens = new Lexer(expr).Tokenize().Select(t => t.Lexeme).ToList();

            CollectionAssert.AreEqual(oldTokens, newTokens);
        }

    }
}
