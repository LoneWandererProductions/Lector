/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Scripting
 * FILE:        LexerTests.cs
 * PURPOSE:     Full test for our lexer and expression evaluator.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Evaluate;
using Weaver.Messages;
using Weaver.Registry;
using Weaver.ScriptEngine;

namespace Mediator.Scripting
{
    /// <summary>
    /// Unit tests for the Lexer and ExpressionEvaluator components.
    /// Validates tokenization, operator handling, literals, keywords, logical expressions, and comments.
    /// </summary>
    [TestClass]
    public class LexerTests
    {
        /// <summary>
        /// Helper to create a lexer for a given input script.
        /// </summary>
        private Lexer CreateLexer(string input) => new Lexer(input);

        /// <summary>
        /// Helper to create an expression evaluator with a given variable registry.
        /// </summary>
        private ExpressionEvaluator CreateEvaluator(VariableRegistry registry) =>
            new ExpressionEvaluator(registry);

        /// <summary>
        /// Tests evaluating logical expressions using variables.
        /// Ensures that 'and', 'or', 'not' work correctly.
        /// </summary>
        [TestMethod]
        public void Test_LogicalExpressionEvaluation()
        {
            var registry = new VariableRegistry();
            registry.Set("x", 5, EnumTypes.Wint);
            registry.Set("y", 10, EnumTypes.Wint);
            registry.Set("z", false, EnumTypes.Wbool);

            var evaluator = CreateEvaluator(registry);

            // True: (5 < 10) && not false => true
            Assert.IsTrue(evaluator.Evaluate("( x < y ) && not z"));

            // False: (5 > 10) || false => false
            Assert.IsFalse(evaluator.Evaluate("( x > y ) || z"));
        }

        /// <summary>
        /// Verifies the lexer correctly tokenizes a logical expression.
        /// Ensures correct splitting and mapping of operators and identifiers.
        /// </summary>
        [TestMethod]
        public void Test_LexerTokensForLogicalExpression()
        {
            var expr = "( x < y ) && not z";
            var lexer = CreateLexer(expr);
            var tokens = lexer.Tokenize();

            var expectedLexemes = new[] { "(", "x", "<", "y", ")", "&&", "!", "z" };
            Assert.AreEqual(expectedLexemes.Length, tokens.Count);

            for (var i = 0; i < expectedLexemes.Length; i++)
            {
                Assert.AreEqual(expectedLexemes[i], tokens[i].Lexeme);
            }
        }

        /// <summary>
        /// Tests that keywords and identifiers are correctly recognized.
        /// </summary>
        [TestMethod]
        public void Test_KeywordsAndIdentifiers()
        {
            var script = "if else label goto do while myVar _testVar";
            var lexer = CreateLexer(script);
            var tokens = lexer.Tokenize();

            var expectedTypes = new[]
            {
                TokenType.KeywordIf,
                TokenType.KeywordElse,
                TokenType.Label,
                TokenType.KeywordGoto,
                TokenType.KeywordDo,
                TokenType.KeywordWhile,
                TokenType.Identifier,
                TokenType.Identifier
            };

            CollectionAssert.AreEqual(expectedTypes, tokens.Select(t => t.Type).ToArray());
        }

        /// <summary>
        /// Tests that numbers (integers and decimals) are correctly tokenized.
        /// </summary>
        [TestMethod]
        public void Test_Numbers()
        {
            var script = "123 45.67";
            var lexer = CreateLexer(script);
            var tokens = lexer.Tokenize();

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(TokenType.Number, tokens[0].Type);
            Assert.AreEqual("123", tokens[0].Lexeme);
            Assert.AreEqual(TokenType.Number, tokens[1].Type);
            Assert.AreEqual("45.67", tokens[1].Lexeme);
        }

        /// <summary>
        /// Tests that string literals are correctly tokenized (quotes removed from lexeme).
        /// </summary>
        [TestMethod]
        public void Test_Strings()
        {
            var script = "\"hello\" \"world\"";
            var lexer = CreateLexer(script);
            var tokens = lexer.Tokenize();

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(TokenType.String, tokens[0].Type);
            Assert.AreEqual("hello", tokens[0].Lexeme);
            Assert.AreEqual(TokenType.String, tokens[1].Type);
            Assert.AreEqual("world", tokens[1].Lexeme);
        }

        /// <summary>
        /// Tests recognition of single-character operators and punctuation.
        /// </summary>
        [TestMethod]
        public void Test_SingleCharOperators()
        {
            var script = "+ - * / ; ( ) { }";
            var lexer = CreateLexer(script);
            var tokens = lexer.Tokenize();

            var expectedTypes = new[]
            {
                TokenType.Plus, TokenType.Minus, TokenType.Star, TokenType.Slash,
                TokenType.Semicolon, TokenType.OpenParen, TokenType.CloseParen,
                TokenType.OpenBrace, TokenType.CloseBrace
            };

            CollectionAssert.AreEqual(expectedTypes, tokens.Select(t => t.Type).ToArray());
        }

        /// <summary>
        /// Tests recognition of double-character operators.
        /// </summary>
        [TestMethod]
        public void Test_DoubleCharOperators()
        {
            var script = "== != >= <= && ||";
            var lexer = CreateLexer(script);
            var tokens = lexer.Tokenize();

            var expectedTypes = new[]
            {
                TokenType.EqualEqual, TokenType.BangEqual,
                TokenType.GreaterEqual, TokenType.LessEqual,
                TokenType.LogicalAnd, TokenType.LogicalOr
            };

            CollectionAssert.AreEqual(expectedTypes, tokens.Select(t => t.Type).ToArray());
        }

        /// <summary>
        /// Ensures that comments are ignored during tokenization.
        /// </summary>
        [TestMethod]
        public void Test_CommentsIgnored()
        {
            var script = "a = 5; // this is a comment\nb = 6;";
            var lexer = CreateLexer(script);
            var tokens = lexer.Tokenize();

            var expectedLexemes = new[] { "a", "=", "5", ";", "b", "=", "6", ";" };
            CollectionAssert.AreEqual(expectedLexemes, tokens.Select(t => t.Lexeme).ToArray());
        }

        /// <summary>
        /// Tests label definition and goto statement tokenization.
        /// </summary>
        [TestMethod]
        public void Test_LabelAndGoto()
        {
            var script = @"
                label start;
                setValue(counter, 1, Wint);
                goto start;
            ";
            var lexer = CreateLexer(script);
            var tokens = lexer.Tokenize();

            var expectedLexemes = new[]
            {
                "label", "start", ";",
                "setValue", "(", "counter", ",", "1", ",", "Wint", ")", ";",
                "goto", "start", ";"
            };

            CollectionAssert.AreEqual(expectedLexemes, tokens.Select(t => t.Lexeme).ToArray());
        }
    }
}