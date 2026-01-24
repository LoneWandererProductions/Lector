using Mediator.Helper;
using Weaver.Evaluate;
using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Mediator.Scripting
{
    [TestClass]
    public class ScriptInternals
    {
        /// <summary>
        /// Compares the tokenizers.
        /// </summary>
        [TestMethod]
        public void CompareTokenizers()
        {
            string expr = "1 + 2.5 >= 3";

            var oldTokens = Tokenizer.Tokenize(expr).ToList();
            var newTokens = new Lexer(expr).Tokenize().Select(t => t.Lexeme).ToList();

            CollectionAssert.AreEqual(oldTokens, newTokens);
        }

        /// <summary>
        /// Evaluates the logical expressions.
        /// </summary>
        [TestMethod]
        public void EvaluateLogicalExpressions()
        {
            var registry = new VariableRegistry();
            registry.Set("x", 5, EnumTypes.Wint);
            registry.Set("y", 10, EnumTypes.Wint);
            registry.Set("z", false, EnumTypes.Wbool);

            var evaluator = new ExpressionEvaluator(registry);

            // Expressions to test
            string exprTrue = "( x < y ) && not z";
            string exprFalse = "( x > y ) || z";

            // Old tokenizer produces List<Token>
            var oldTokensTrue = Tokenizer.Tokenize(exprTrue).ToList();
            var oldTokensFalse = Tokenizer.Tokenize(exprFalse).ToList();

            // New lexer produces List<string> of lexemes
            var newTokensTrue = new Lexer(exprTrue).Tokenize().Select(t => t.Lexeme).ToList();
            var newTokensFalse = new Lexer(exprFalse).Tokenize().Select(t => t.Lexeme).ToList();

            // Optionally: compare token streams for consistency
            CollectionAssert.AreEqual(oldTokensTrue, newTokensTrue, "Lexer and Tokenizer lexemes do not match for exprTrue.");
            CollectionAssert.AreEqual(oldTokensFalse, newTokensFalse, "Lexer and Tokenizer lexemes do not match for exprFalse.");

            // Evaluate expressions
            Assert.IsTrue(evaluator.Evaluate(exprTrue), $"Expected '{exprTrue}' to evaluate to true.");
            Assert.IsFalse(evaluator.Evaluate(exprFalse), $"Expected '{exprFalse}' to evaluate to false.");
        }
    }
}
