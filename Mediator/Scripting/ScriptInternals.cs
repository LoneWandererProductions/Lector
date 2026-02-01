/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ediator.Scripting
 * FILE:        ScriptInternals.cs
 * PURPOSE:     Tests for general script internals.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Mediator.Helper;
using System.Diagnostics;
using Weaver.Evaluate;
using Weaver.Messages;
using Weaver.Registry;
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

            Trace.WriteLine($"Old tokens: {string.Join(", ", oldTokens)}");
            Trace.WriteLine($"New tokens: {string.Join(", ", newTokens)}");

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

            // Trace token comparisons for debug
            Trace.WriteLine("=== Token Comparison for exprTrue ===");
            Trace.WriteLine($"Expression: {exprTrue}");
            Trace.WriteLine($"Old tokens: {string.Join(", ", oldTokensTrue)}");
            Trace.WriteLine($"New tokens: {string.Join(", ", newTokensTrue)}");

            Trace.WriteLine("=== Token Comparison for exprFalse ===");
            Trace.WriteLine($"Expression: {exprFalse}");
            Trace.WriteLine($"Old tokens: {string.Join(", ", oldTokensFalse)}");
            Trace.WriteLine($"New tokens: {string.Join(", ", newTokensFalse)}");

            // Compare token streams
            CollectionAssert.AreNotEqual(oldTokensTrue, newTokensTrue, "Lexer and Tokenizer lexemes do not match for exprTrue.");
            CollectionAssert.AreNotEqual(oldTokensFalse, newTokensFalse, "Lexer and Tokenizer lexemes do not match for exprFalse.");

            // Evaluate expressions and trace results
            var resultTrue = evaluator.Evaluate(exprTrue);
            var resultFalse = evaluator.Evaluate(exprFalse);

            Trace.WriteLine($"Evaluation of '{exprTrue}': {resultTrue}");
            Trace.WriteLine($"Evaluation of '{exprFalse}': {resultFalse}");

            Assert.IsTrue(resultTrue, $"Expected '{exprTrue}' to evaluate to true.");
            Assert.IsFalse(resultFalse, $"Expected '{exprFalse}' to evaluate to false.");
        }
    }
}
