/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Parser.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

#nullable enable
using System.Text;

namespace Weaver.ScriptEngine;

internal sealed class Parser
{
     internal record ScriptLine(string Category, string? Statement);


    private readonly List<Token> _tokens;
    private int _position;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    /// <summary>
    /// Parses the into categorized blocks.
    /// </summary>
    /// <returns></returns>
    public List<ScriptLine> ParseIntoCategorizedBlocks()
    {
        var result = new List<ScriptLine>();

        while (!IsAtEnd())
        {
            var current = Peek();

            switch (current.Type)
            {
                case TokenType.KeywordIf:
                    ParseIfStatement(result);
                    break;

                case TokenType.KeywordElse:
                    ParseElseBlock(result);
                    break;

                case TokenType.Label:
                    result.Add(new ScriptLine("Label", ReadStatementAsString()));
                    break;

                case TokenType.KeywordGoto:
                    result.Add(new ScriptLine("Goto", ReadStatementAsString()));
                    break;

                case TokenType.Comment:
                    Advance(); // skip
                    break;

                default:
                    result.Add(new ScriptLine("Command", ReadStatementAsString()));
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Parses if statement.
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="commandIndex">Index of the command.</param>
    private void ParseIfStatement(List<ScriptLine> output)
    {
        Advance(); // consume 'if'
        var condition = ReadCondition();
        output.Add(new ScriptLine("If_Condition", condition));

        Expect(TokenType.OpenBrace);
        output.Add(new ScriptLine("If_Open", null));

        var statements = ParseBlockStatements();
        output.AddRange(statements);

        output.Add(new ScriptLine("If_End", null));

        if (!IsAtEnd() && Peek().Type == TokenType.KeywordElse)
        {
            ParseElseBlock(output);
        }
    }

    /// <summary>
    /// Parses the else block.
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="commandIndex">Index of the command.</param>
    private void ParseElseBlock(List<ScriptLine> output)
    {
        Advance(); // consume 'else'
        Expect(TokenType.OpenBrace);
        output.Add(new ScriptLine("Else_Open", null));

        var statements = ParseBlockStatements();
        output.AddRange(statements);

        output.Add(new ScriptLine("Else_End", null));
    }

    /// <summary>
    /// Reads the condition.
    /// </summary>
    /// <returns></returns>
    private string ReadCondition()
    {
        // Expect '('
        Expect(TokenType.OpenParen);

        var sb = new StringBuilder();
        var parenDepth = 1;

        while (!IsAtEnd() && parenDepth > 0)
        {
            var token = Advance();

            if (token.Type == TokenType.OpenParen)
            {
                parenDepth++;
            }
            else if (token.Type == TokenType.CloseParen)
            {
                parenDepth--;
            }

            if (parenDepth > 0)
            {
                sb.Append(token.Lexeme);
            }
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Parses the block statements.
    /// </summary>
    /// <returns>Tuble of Block statements</returns>
    private List<ScriptLine> ParseBlockStatements()
    {
        var statements = new List<ScriptLine>();

        while (!IsAtEnd() && Peek().Type != TokenType.CloseBrace)
        {
            var token = Peek();

            switch (token.Type)
            {
                case TokenType.KeywordIf:
                    Advance();
                    var condition = ReadCondition();
                    statements.Add(new ScriptLine("If_Condition", condition));
                    Expect(TokenType.OpenBrace);
                    statements.Add(new ScriptLine("If_Open", null));
                    statements.AddRange(ParseBlockStatements());
                    statements.Add(new ScriptLine("If_End", null));

                    if (!IsAtEnd() && Peek().Type == TokenType.KeywordElse)
                    {
                        Advance();
                        Expect(TokenType.OpenBrace);
                        statements.Add(new ScriptLine("Else_Open", null));
                        statements.AddRange(ParseBlockStatements());
                        statements.Add(new ScriptLine("Else_End", null));
                    }
                    break;

                case TokenType.Label:
                    statements.Add(new ScriptLine("Label", ReadStatementAsString()));
                    break;

                case TokenType.KeywordGoto:
                    statements.Add(new ScriptLine("Goto", ReadStatementAsString()));
                    break;

                case TokenType.Comment:
                    Advance(); // skip
                    break;

                case TokenType.KeywordDo:
                    statements.AddRange(ParseDoWhile());
                    break;

                default:
                    statements.Add(new ScriptLine("Command", ReadStatementAsString()));
                    break;
            }
        }

        Expect(TokenType.CloseBrace);
        return statements;
    }

    private List<ScriptLine> ParseDoWhile()
    {
        var output = new List<ScriptLine>();

        Advance(); // consume 'do'
        Expect(TokenType.OpenBrace);
        output.Add(new ScriptLine("Do_Open", null));

        output.AddRange(ParseBlockStatements());

        output.Add(new ScriptLine("Do_End", null));

        if (!IsAtEnd() && Peek().Type == TokenType.KeywordWhile)
        {
            Advance(); // consume 'while'
            var condition = ReadCondition();
            output.Add(new ScriptLine("While_Condition", condition));
            Match(TokenType.Semicolon);
        }

        return output;
    }



    /// <summary>
    /// Expects the specified expected.
    /// </summary>
    /// <param name="expected">The expected.</param>
    /// <exception cref="ArgumentException">Expected token '{expected}' but found '{(IsAtEnd() ? "EOF" : Peek().Type.ToString())}'.</exception>
    private void Expect(TokenType expected)
    {
        if (IsAtEnd() || Peek().Type != expected)
        {
            throw new ArgumentException(
                $"Expected token '{expected}' but found '{(IsAtEnd() ? "EOF" : Peek().Type.ToString())}'.");
        }

        Advance();
    }

    private string ReadStatementAsString()
    {
        var sb = new StringBuilder();
        var insideParens = false;
        Token? previous = null;

        while (!IsAtEnd() && Peek().Type != TokenType.Semicolon)
        {
            var current = Advance();

            if (insideParens &&
                previous != null &&
                IsAlphanumeric(previous.Type) &&
                IsAlphanumeric(current.Type))
            {
                sb.Append(' ');
            }

            sb.Append(current.Lexeme);

            if (current.Type == TokenType.OpenParen)
            {
                insideParens = true;
            }
            else if (current.Type == TokenType.CloseParen)
            {
                insideParens = false;
            }

            previous = current;
        }

        if (Match(TokenType.Semicolon))
        {
            sb.Append(';');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether the specified type is alphanumeric.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    ///   <c>true</c> if the specified type is alphanumeric; otherwise, <c>false</c>.
    /// </returns>
    private bool IsAlphanumeric(TokenType type)
    {
        return type == TokenType.Identifier ||
               type == TokenType.KeywordIf ||
               type == TokenType.KeywordElse ||
               type == TokenType.Number; // add any others you want spaced
    }

    /// <summary>
    /// Determines whether [is at end].
    /// </summary>
    /// <returns>
    ///   <c>true</c> if [is at end]; otherwise, <c>false</c>.
    /// </returns>
    private bool IsAtEnd()
    {
        return _position >= _tokens.Count;
    }

    /// <summary>
    /// Advances this instance.
    /// </summary>
    /// <returns>Next Token.</returns>
    private Token Advance()
    {
        return _tokens[_position++];
    }

    /// <summary>
    /// Peeks this instance.
    /// </summary>
    /// <returns>Token at position</returns>
    private Token Peek()
    {
        return _tokens[_position];
    }

    /// <summary>
    /// Matches the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>If type of token is matched.</returns>
    private bool Match(TokenType type)
    {
        if (IsAtEnd() || _tokens[_position].Type != type)
        {
            return false;
        }

        _position++;
        return true;
    }
}
