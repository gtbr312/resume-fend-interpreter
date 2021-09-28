using System;
using System.Collections.Generic;
using static TokenType;
using static Expr;
using static Stmt;

namespace FITRP
{
    class Parser
    {
        public class ParseError : Exception { };
        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            /*
            try {
                return Expression();
            }catch(ParseError error) {
                return null;
            }
            */
            List<Stmt> statements = new List<Stmt>();
            while (!IsAtEnd()) {
                statements.Add(Declaration());
            }
            return statements;
        }

        private Stmt Declaration()
        {
            try {
                if (Match(CLASS)) return ClassDeclaration();
                if (Match(FUNC)) return Function("function");
                if (Match(VAR)) return VarDeclaration();
                return Statement();
            }catch(ParseError error) {
                Synchronize();
                return null;
            }
        }

        private Stmt ClassDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expected class name.");

            Var parent = null;
            if (Match(LESS)) {
                Consume(IDENTIFIER, "Expected superclass name.");
                parent = new Var(Previous());    
            }


            Consume(LEFT_BRACE, "Expected opening { of class body.");

            List<Function> methods = new List<Function>();
            while(!Check(RIGHT_BRACE) && !IsAtEnd()) {
                methods.Add(Function("method"));
            }

            Consume(RIGHT_BRACE, "Expected closing } of class body.");

            return new Class(name, parent, methods);
        }

        private Function Function(string type)
        {
            Token name = Consume(IDENTIFIER, $"Exprected {type} name.");
            Consume(LEFT_PAREN, $"Expected opening ( after {type} name");
            List<Token> parameters = new List<Token>();
            if (!Check(RIGHT_PAREN)) {
                do {
                    if (parameters.Count >= 255) {
                        Error(Peek(), "Cannot pass more than 255 arguments.");
                    }

                    parameters.Add(Consume(IDENTIFIER, "Expected parameter name."));
                } while (Match(COMMA));
            }
            Consume(RIGHT_PAREN, "Expected closing ) after parameters");

            Consume(LEFT_BRACE, $"Expected opening {"{"} before {type} body.");
            List<Stmt> body = Block();
            return new Function(name, parameters, body);
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expected variable name");

            Expr initializer = null;
            if (Match(EQUAL)) {
                initializer = Expression();
            }

            Consume(SEMICOLON, "Expected ; after variable declaration.");
            return new VarDec(name, initializer);
        }

        private Stmt Statement()
        {
            if (Match(FOR)) return ForStatement();
            if (Match(WHILE)) return WhileStatement();
            if (Match(IF)) return IfStatement();
            if (Match(PRINT)) return PrintStatement();
            if (Match(RETURN)) return ReturnStatement();
            if (Match(LEFT_BRACE)) return new Block(Block());

            return ExpressionStatement();
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!Check(SEMICOLON)) {
                value = Expression();
            }

            Consume(SEMICOLON, "Expected ; after return statement.");
            return new Stmt.Return(keyword, value);
        }

        private Stmt ForStatement()
        {
            Consume(LEFT_PAREN, "Expect opening ( for for statement arguments.");

            Stmt initializer;
            if (Match(SEMICOLON)) {
                initializer = null;
            } else if (Match(VAR)) {
                initializer = VarDeclaration();
            } else {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(SEMICOLON)) {
                condition = Expression();
            }
            Consume(SEMICOLON, "Expected ending ; after for loop condition");

            Expr postExecExpr = null;
            if (!Check(RIGHT_PAREN)) {
                postExecExpr = Expression();
            }
            Consume(RIGHT_PAREN, "Exprected closing ) after for loop arguments.");
            Stmt body = Statement();

            List<Stmt> internals = new List<Stmt> {
                body,
                new Expression(postExecExpr)
            };
            if (postExecExpr != null) {
                body = new Block(internals);
            }

            if (condition == null) condition = new Literal(true);
            body = new While(condition, body);

            if(initializer != null) {
                List<Stmt> sugarBlock = new List<Stmt> {
                    initializer,
                    body
                };
                body = new Block(sugarBlock);
            }

            return body;
        }

        private Stmt WhileStatement()
        {
            Consume(LEFT_PAREN, "Expected opening ( after while statement.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expected closing ) after while arguments.");
            Stmt body = Statement();

            return new While(condition, body);
        }

        private Stmt IfStatement()
        {
            Consume(LEFT_PAREN, "Expected opening ( for if statement arguments.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expected closing ) for if statement arguments.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(ELSE)) {
                elseBranch = Statement();
            }
            return new IfStmt(condition, thenBranch, elseBranch);
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!Check(RIGHT_BRACE) && !IsAtEnd()) {
                statements.Add(Declaration());
            }
            Consume(RIGHT_BRACE, "Expected closing '}' after block.");
            return statements;
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(SEMICOLON, "Exprected ; after expression statement.");
            return new Expression(expr);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(SEMICOLON, "Expected ; after print statement value.");
            return new Print(value);
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if (Match(EQUAL)) {
                Token equals = Previous();
                Expr value = Expression();

                if(expr is Var) {
                    Token name = ((Var)expr).name;
                    return new Assign(name, value);
                }else if (expr is Get) {
                    Get get = (Get)expr;
                    return new Set(get.objct, get.name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(OR)) {
                Token oprtr = Previous();
                Expr right = And();
                expr = new Logical(expr, oprtr, right);
            }
            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(AND)) {
                Token oprtr = Previous();
                Expr right = Equality();
                expr = new Logical(expr, oprtr, right);
            }
            return expr;

        }

        private Expr Equality()
        {
            List<TokenType> toMatch = new List<TokenType> 
            {
                BANG_EQUAL,
                EQUAL_EQUAL
            };
            return RecursiveBinaryExpr(Comparison, toMatch);
        }

        private Expr Comparison()
        {
            List<TokenType> toMatch = new List<TokenType>
            {
                GREATER,
                GREATER_EQUAL,
                LESS,
                LESS_EQUAL
            };
            return RecursiveBinaryExpr(Term, toMatch);
        }

        private Expr Term()
        {
            List<TokenType> toMatch = new List<TokenType>
            {
                MINUS,
                PLUS
            };
            return RecursiveBinaryExpr(Factor, toMatch);
        }

        private Expr Factor()
        {
            List<TokenType> toMatch = new List<TokenType>
            {
                SLASH,
                STAR
            };
            return RecursiveBinaryExpr(Unary, toMatch);
        }

        private Expr RecursiveBinaryExpr(Func<Expr> ExpressionType, List<TokenType> toMatch)
        {
            Expr expr = ExpressionType();
            while (Match(toMatch)) {
                Token oprtr = Previous();
                Expr right = ExpressionType();
                expr = new Binary(expr, oprtr, right);
            }
            return expr;
        }

        private Expr Unary()
        {
            List<TokenType> toMatch = new List<TokenType>
            {
                BANG,
                MINUS
            };
            if (Match(toMatch)) {
                Token oprtr = Previous();
                Expr right = Unary();
                return new Unary(oprtr, right);
            }
            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true) {
                if (Match(LEFT_PAREN)) {
                    expr = FinishCall(expr);
                }else if (Match(DOT)) {
                    Token name = Consume(IDENTIFIER, "Expected property name after . accessor.");
                    expr = new Get(expr, name);
                }else {
                    break;
                }
            }
            return expr;

        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if (!Check(RIGHT_PAREN)) {
                do {
                    if (arguments.Count >= 255) Error(Peek(), "Cannot pass more than 255 arguments.");
                    arguments.Add(Expression());
                } while (Match(COMMA));
            }
            Token paren = Consume(RIGHT_PAREN, "Expected ) after arguments.");

            return new Call(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if (Match(FALSE)) return new Literal(false);
            if (Match(TRUE)) return new Literal(true);
            if (Match(NULL)) return new Literal(null);
            if (Match(IDENTIFIER)) return new Var(Previous());
            if (Match(THIS)) return new This(Previous());
            if (Match(SUPER)) {
                Token keyword = Previous();
                Consume(DOT, "Expected method access . following parent reserved word.");
                Token method = Consume(IDENTIFIER, "Expected method name after parent access.");
                return new Super(keyword, method);
            }

            List<TokenType> numAndStringLits = new List<TokenType> { NUMBER, STRING };
            if (Match(numAndStringLits)) {
                //Previous must be called here because Match consumes the token
                return new Literal(Previous().literal);
            }

            if (Match(LEFT_PAREN)) {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Expected closing parentheses");
                return new Grouping(expr);
            }

            //Landing here means we should have encountered a terminal but did not.
            throw Error(Peek(), "Expected Expression");
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw Error(Peek(), message);
        }
        private Exception Error(Token token, string message)
        {
            FendInterpreter.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd()) {
                if (Previous().type == SEMICOLON) return;

                switch (Peek().type) {
                    case CLASS:
                    case FUNC:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }

        private bool Match(List<TokenType> toMatch)
        {
            foreach(TokenType type in toMatch) {
                if (Check(type)) {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Match(TokenType toMatch)
        {
            if (Check(toMatch)) {
                Advance();
                return true;
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().type == type;
        }

        private Token Peek()
        {
            return tokens[current];
        }

        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        private Token Previous()
        {
            return tokens[current - 1];
        }

        private bool IsAtEnd()
        {
            return Peek().type == EOF;
        }

    }

}

