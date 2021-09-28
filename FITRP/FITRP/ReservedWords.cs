using System.Collections.Generic;
using static TokenType;

class ReservedWords
{
    public static readonly Dictionary<string, TokenType> reservedWords = new Dictionary<string, TokenType>
    {
        {"and", AND },
        {"class", CLASS },
        {"else", ELSE },
        {"false", FALSE },
        {"for", FOR },
        {"func", FUNC },
        {"if", IF },
        {"null", NULL },
        {"or", OR },
        {"print", PRINT },
        {"return", RETURN },
        {"parent", SUPER },
        {"this", THIS },
        {"true", TRUE },
        {"var", VAR },
        {"while", WHILE },
    };
}