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

    public List<ScriptLine> ParseIntoCategorizedBlocks()
    {
        var result = new List<ScriptLine>();

        while (!IsAtEnd())
        {
            var token = Peek();

            switch (token.Type)
            {
                case TokenType.Label:
                    result.Add(new ScriptLine("Label", ReadStatementAsString()));
                    break;

                case TokenType.KeywordGoto:
                    result.Add(new ScriptLine("Goto", ReadStatementAsString()));
                    break;

                case TokenType.KeywordIf:
                    ParseIfBlock(result);
                    break;

                case TokenType.KeywordDo:
                    ParseDoWhileBlock(result);
                    break;

                case TokenType.Comment:
                    Advance(); // skip
                    break;

                default:
                    if (token.Type != TokenType.OpenBrace &&
                        token.Type != TokenType.CloseBrace &&
                        token.Type != TokenType.Semicolon)
                    {
                        result.Add(new ScriptLine("Command", ReadStatementAsString()));
                    }
                    else
                    {
                        Advance(); // skip structural tokens
                    }

                    break;
            }
        }

        return result;
    }

    private void ParseIfBlock(List<ScriptLine> output)
    {
        Advance(); // consume 'if'
        var condition = ReadCondition();
        output.Add(new ScriptLine("If_Condition", condition));

        Expect(TokenType.OpenBrace);
        output.Add(new ScriptLine("If_Open", null));
        output.AddRange(ParseBlockStatements());
        output.Add(new ScriptLine("If_End", null));

        if (!IsAtEnd() && Peek().Type == TokenType.KeywordElse)
            ParseElseBlock(output);
    }

    private void ParseElseBlock(List<ScriptLine> output)
    {
        Advance(); // consume 'else'
        Expect(TokenType.OpenBrace);
        output.Add(new ScriptLine("Else_Open", null));
        output.AddRange(ParseBlockStatements());
        output.Add(new ScriptLine("Else_End", null));
    }

    private void ParseDoWhileBlock(List<ScriptLine> output)
    {
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
            Match(TokenType.Semicolon); // optional semicolon
        }
    }

    private List<ScriptLine> ParseBlockStatements()
    {
        var statements = new List<ScriptLine>();

        while (!IsAtEnd() && Peek().Type != TokenType.CloseBrace)
        {
            var token = Peek();

            switch (token.Type)
            {
                case TokenType.KeywordIf:
                    ParseIfBlock(statements);
                    break;

                case TokenType.KeywordDo:
                    ParseDoWhileBlock(statements);
                    break;

                case TokenType.Label:
                    statements.Add(new ScriptLine("Label", ReadStatementAsString()));
                    break;

                case TokenType.KeywordGoto:
                    statements.Add(new ScriptLine("Goto", ReadStatementAsString()));
                    break;

                case TokenType.Comment:
                    Advance();
                    break;

                default:
                    if (token.Type != TokenType.OpenBrace &&
                        token.Type != TokenType.CloseBrace &&
                        token.Type != TokenType.Semicolon)
                    {
                        statements.Add(new ScriptLine("Command", ReadStatementAsString()));
                    }
                    else
                    {
                        Advance(); // skip structural tokens
                    }

                    break;
            }
        }

        Expect(TokenType.CloseBrace); // close block
        return statements;
    }

    private string ReadStatementAsString()
    {
        var sb = new System.Text.StringBuilder();
        Token? previous = null;
        bool insideParens = false;

        while (!IsAtEnd() && Peek().Type != TokenType.Semicolon)
        {
            var token = Advance();

            if (insideParens && previous != null && IsAlphanumeric(previous.Type) && IsAlphanumeric(token.Type))
                sb.Append(' ');

            sb.Append(token.Lexeme);

            if (token.Type == TokenType.OpenParen)
                insideParens = true;
            else if (token.Type == TokenType.CloseParen)
                insideParens = false;

            previous = token;
        }

        Match(TokenType.Semicolon); // consume semicolon if present
        return sb.ToString().Trim();
    }

    private string ReadCondition()
    {
        Expect(TokenType.OpenParen);
        var sb = new System.Text.StringBuilder();
        int depth = 1;

        while (!IsAtEnd() && depth > 0)
        {
            var t = Advance();
            if (t.Type == TokenType.OpenParen) depth++;
            if (t.Type == TokenType.CloseParen) depth--;
            if (depth > 0) sb.Append(t.Lexeme);
        }

        return sb.ToString().Trim();
    }

    private bool IsAlphanumeric(TokenType type) =>
        type == TokenType.Identifier || type == TokenType.Number || type == TokenType.KeywordIf ||
        type == TokenType.KeywordElse;

    private void Expect(TokenType expected)
    {
        if (IsAtEnd() || Peek().Type != expected)
            throw new ArgumentException(
                $"Expected token '{expected}' but found '{(IsAtEnd() ? "EOF" : Peek().Type.ToString())}'");

        Advance();
    }

    private bool Match(TokenType type)
    {
        if (IsAtEnd() || Peek().Type != type) return false;

        Advance();
        return true;
    }

    private Token Peek() => _tokens[_position];
    private Token Advance() => _tokens[_position++];
    private bool IsAtEnd() => _position >= _tokens.Count;
}