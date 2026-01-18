using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weaver.ScriptEngine;
using System.Linq;

namespace Mediator.Scripting
{
    [TestClass]
    public class LexerTests
    {
        private Lexer CreateLexer(string input) => new Lexer(input);

        private string TokensToString(Lexer lexer)
        {
            return string.Join(", ", lexer.Tokenize().Select(t => $"{t.Type}('{t.Lexeme}')"));
        }

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

        [TestMethod]
        public void Test_CommentsIgnored()
        {
            var script = "a = 5; // this is a comment\nb = 6;";
            var lexer = CreateLexer(script);
            var tokens = lexer.Tokenize();

            // Tokens should include 'a = 5 ;' and 'b = 6 ;' but not the comment
            var expectedLexemes = new[]
            {
                "a", "=", "5", ";", "b", "=", "6", ";"
            };

            CollectionAssert.AreEqual(expectedLexemes, tokens.Select(t => t.Lexeme).ToArray());
        }

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
