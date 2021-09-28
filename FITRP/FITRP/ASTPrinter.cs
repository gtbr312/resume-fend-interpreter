using System;
using System.Collections.Generic;
using System.Text;
using static Expr;

namespace FITRP
{
    class ASTPrinter : Visitor<string>
    {
        public void Main(){
            
            Expr expression = new Binary(
                    new Expr.Unary(
                        new Token(TokenType.MINUS, "-", null, 1),
                        new Expr.Literal(123)
                    ),
                        new Token(TokenType.STAR, "*", null, 1),
                        new Grouping(
                        new Literal(23.23)
                    )
                );
            
            /*
            Expr expression = new Unary(
                new Token(TokenType.MINUS, "-", null,-1),
                new Expr.Literal(1)
                );
            */
            Console.WriteLine(new ASTPrinter().Print(expression));
        }

        public string Print(Expr expr)
        {
            return expr.Accept(this).ToString();   
        }

        public string visit(Literal expr)
        {
            if(expr.value == null)return "null";
            return expr.value.ToString();
        }

        public string visit(Unary expr)
        {
            List<Expr> exprs = new List<Expr>{
                expr.right
            };
            return Parenthesize(expr.oprtr.lexeme, exprs);
        }

        public string visit(Binary expr)
        {
            List<Expr> exprs = new List<Expr>{
                expr.left, 
                expr.right
            };
            return Parenthesize(expr.oprtr.lexeme, exprs);
        }

        public string visit(Grouping expr)
        {
            List<Expr> exprs = new List<Expr>{
                expr.expression
            };
            return Parenthesize("group", exprs);
        }

        public string visit(Var expr)
        {
            throw new NotImplementedException();
        }

        public string visit(Assign expr)
        {
            throw new NotImplementedException();
        }

        public string visit(Logical expr)
        {
            throw new NotImplementedException();
        }

        public string visit(Call expr)
        {
            throw new NotImplementedException();
        }

        public string visit(Get expr)
        {
            throw new NotImplementedException();
        }

        public string visit(Set expr)
        {
            throw new NotImplementedException();
        }

        public string visit(This expr)
        {
            throw new NotImplementedException();
        }

        public string visit(Super expr)
        {
            throw new NotImplementedException();
        }

        private string Parenthesize(string name, List<Expr> exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach(Expr expr in exprs){
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");
            return builder.ToString();
        }
    }
}
