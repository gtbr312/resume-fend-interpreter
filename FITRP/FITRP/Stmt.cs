using System;
using System.Collections.Generic;
using FITRP;

abstract class Stmt
{

   public class Expression : Stmt
    {
       public readonly Expr expression;

       public Expression (Expr expression)
       {
       this.expression = expression;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Print : Stmt
    {
       public readonly Expr expression;

       public Print (Expr expression)
       {
       this.expression = expression;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class VarDec : Stmt
    {
       public readonly Token name;
       public readonly Expr initializer;

       public VarDec (Token name, Expr initializer)
       {
       this.name = name;
       this.initializer = initializer;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Block : Stmt
    {
       public readonly List<Stmt> statements;

       public Block (List<Stmt> statements)
       {
       this.statements = statements;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class IfStmt : Stmt
    {
       public readonly Expr condition;
       public readonly Stmt thenBranch;
       public readonly Stmt elseBranch;

       public IfStmt (Expr condition, Stmt thenBranch, Stmt elseBranch)
       {
       this.condition = condition;
       this.thenBranch = thenBranch;
       this.elseBranch = elseBranch;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class While : Stmt
    {
       public readonly Expr condition;
       public readonly Stmt body;

       public While (Expr condition, Stmt body)
       {
       this.condition = condition;
       this.body = body;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Function : Stmt
    {
       public readonly Token name;
       public readonly List<Token> parameters;
       public readonly List<Stmt> body;

       public Function (Token name, List<Token> parameters, List<Stmt> body)
       {
       this.name = name;
       this.parameters = parameters;
       this.body = body;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Return : Stmt
    {
       public readonly Token keyword;
       public readonly Expr value;

       public Return (Token keyword, Expr value)
       {
       this.keyword = keyword;
       this.value = value;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


   public class Class : Stmt
    {
       public readonly Token name;
       public readonly Expr.Var parent;
       public readonly List<Stmt.Function> methods;

       public Class (Token name, Expr.Var parent, List<Stmt.Function> methods)
       {
       this.name = name;
       this.parent = parent;
       this.methods = methods;
       }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.visit(this);
        }

    }


    public interface Visitor<T>
    {

       T visit(Expression stmt); 
       T visit(Print stmt); 
       T visit(VarDec stmt); 
       T visit(Block stmt); 
       T visit(IfStmt stmt); 
       T visit(While stmt); 
       T visit(Function stmt); 
       T visit(Return stmt); 
       T visit(Class stmt); 

    }

    public abstract T Accept<T>(Visitor<T> visitor);
}
