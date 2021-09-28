using System;
using System.Collections.Generic;
using FITRP;

abstract class Expr
{

   public class Literal : Expr
    {
       public readonly Object value;

       public Literal (Object value)
       {
       this.value = value;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Unary : Expr
    {
       public readonly Token oprtr;
       public readonly Expr right;

       public Unary (Token oprtr, Expr right)
       {
       this.oprtr = oprtr;
       this.right = right;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Call : Expr
    {
       public readonly Expr callee;
       public readonly Token paren;
       public readonly List<Expr> args;

       public Call (Expr callee, Token paren, List<Expr> args)
       {
       this.callee = callee;
       this.paren = paren;
       this.args = args;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Get : Expr
    {
       public readonly Expr objct;
       public readonly Token name;

       public Get (Expr objct, Token name)
       {
       this.objct = objct;
       this.name = name;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Set : Expr
    {
       public readonly Expr objct;
       public readonly Token name;
       public readonly Expr value;

       public Set (Expr objct, Token name, Expr value)
       {
       this.objct = objct;
       this.name = name;
       this.value = value;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class This : Expr
    {
       public readonly Token keyword;

       public This (Token keyword)
       {
       this.keyword = keyword;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Super : Expr
    {
       public readonly Token keyword;
       public readonly Token method;

       public Super (Token keyword, Token method)
       {
       this.keyword = keyword;
       this.method = method;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Binary : Expr
    {
       public readonly Expr left;
       public readonly Token oprtr;
       public readonly Expr right;

       public Binary (Expr left, Token oprtr, Expr right)
       {
       this.left = left;
       this.oprtr = oprtr;
       this.right = right;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Grouping : Expr
    {
       public readonly Expr expression;

       public Grouping (Expr expression)
       {
       this.expression = expression;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Var : Expr
    {
       public readonly Token name;

       public Var (Token name)
       {
       this.name = name;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Assign : Expr
    {
       public readonly Token name;
       public readonly Expr value;

       public Assign (Token name, Expr value)
       {
       this.name = name;
       this.value = value;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Logical : Expr
    {
       public readonly Expr left;
       public readonly Token oprtr;
       public readonly Expr right;

       public Logical (Expr left, Token oprtr, Expr right)
       {
       this.left = left;
       this.oprtr = oprtr;
       this.right = right;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


    public interface Visitor<T>
    {

       T visit(Literal expr); 
       T visit(Unary expr); 
       T visit(Call expr); 
       T visit(Get expr); 
       T visit(Set expr); 
       T visit(This expr); 
       T visit(Super expr); 
       T visit(Binary expr); 
       T visit(Grouping expr); 
       T visit(Var expr); 
       T visit(Assign expr); 
       T visit(Logical expr); 

    }

    public abstract T Accept<T>(Visitor<T> visitor);
}
