using System;
using System.Collections.Generic;
using System.Text;

namespace FITRP
{
    class Token
    {
        public readonly TokenType type;
        public string lexeme;
        public object literal;
        public int line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public String AsString()
        {
            return $"{type} {lexeme} {literal}";
        }
    }
}
