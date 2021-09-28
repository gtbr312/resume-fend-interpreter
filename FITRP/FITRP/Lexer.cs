using System;
using System.Collections.Generic;
using static TokenType;
using static ReservedWords;

namespace FITRP
{
    class Lexer
    {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 0;

        public Lexer(string source)
        {
            this.source = source;
        }

        public List<Token> AnalyzeTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;
                case '}': AddToken(RIGHT_BRACE); break;
                case ',': AddToken(COMMA); break;
                case '.': AddToken(DOT); break;
                case '-': AddToken(MINUS); break;
                case '+': AddToken(PLUS); break;
                case ';': AddToken(SEMICOLON); break;
                case '*': AddToken(STAR); break;
                case '!':
                    AddToken(Match('=') ? BANG_EQUAL : BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? LESS_EQUAL : LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }else if (Match('*'))
                    {
                        CommentBlock();
                    }
                    else
                    {
                        AddToken(SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    break;
                case '"':
                    StringLiteral();
                    break;
                default:
                    if (IsDigit(c)) {
                        NumberLiteral();
                    } else if (IsAlpha(c)) {
                        Identifier();
                    } else
                        FendInterpreter.Error(line, "Invalid token");
                    break;
            }
        }

        private int CommentBlock()
        {
            int nestedCodeBlockEnd = 0;
            while (!(Peek() == '*' && PeekNext() == '/'))
            {
                if (IsAtEnd())
                {
                    if (nestedCodeBlockEnd != 0)
                    {
                        current = nestedCodeBlockEnd;
                        break;
                    }
                    return -1;
                }
                if (Peek() == '/' && PeekNext() == '*')
                {
                    Advance();
                    Advance();
                    int end = CommentBlock();
                    if(end != -1)
                    {
                        nestedCodeBlockEnd = end;
                    }
                }
                else
                {
                    if (Peek() == '\n') line++;
                    Advance();
                }
            }

            /*/**/
            
            //These advances consume the closing */
            Advance();
            Advance();
            return current;
            
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            String identifier = source.Substring(start, current - start);
            if (!reservedWords.TryGetValue(identifier, out TokenType type))
            {
                type = IDENTIFIER;
            }
            AddToken(type);
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);    
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private void NumberLiteral()
        {
            while (IsDigit(Peek())) Advance();
            
            //Find decimal point
            if(Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek())) Advance();

            }
            Double val = Convert.ToDouble(source.Substring(start, current - start));
            AddToken(NUMBER, val);
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void StringLiteral()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd())
            {
                //Console.WriteLine($"Unclosed string literal at line {line}");
                FendInterpreter.Error(line, $"Unclosed string literal.");
                return;
            }

            //This advance is consuming the closing "
            Advance();
            string value = source.Substring(start+1, (current-start)-2);
            AddToken(STRING, value);
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private bool Match(char expectedChar)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expectedChar) return false;

            current++;
            return true;

        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, Object literal)
        {
            String text = source.Substring(start, current-start);
            tokens.Add(new Token(type, text, literal, line));
        }

        private char Advance()
        {
            current++;
            return source[current-1];
        }

        private bool IsAtEnd()
        {
            return current >= source.Length;
        }
    }
}
