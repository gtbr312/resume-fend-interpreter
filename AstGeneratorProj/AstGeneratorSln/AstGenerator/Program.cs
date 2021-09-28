using System;
using System.IO;
using System.Collections.Generic;

namespace AstGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> expressionTypes = new List<string>
            {
                "Literal  : Object value",
                "Unary    : Token oprtr, Expr right",
                "Call     : Expr callee, Token paren, List<Expr> args",
                "Get      : Expr objct, Token name",
                "Set      : Expr objct, Token name, Expr value",
                "This     : Token keyword",
                "Super    : Token keyword, Token method",
                "Binary   : Expr left, Token oprtr, Expr right",
                "Grouping : Expr expression",
                "Var      : Token name",
                "Assign   : Token name, Expr value",
                "Logical  : Expr left, Token oprtr, Expr right",
            };


            DefineAst("Expr", expressionTypes);
            List<string> statementTypes = new List<string>
            {
                "Expression : Expr expression",
                "Print      : Expr expression",
                "VarDec     : Token name, Expr initializer",
                "Block      : List<Stmt> statements",
                "IfStmt     : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "While      : Expr condition, Stmt body",
                "Function   : Token name, List<Token> parameters, List<Stmt> body",
                "Return     : Token keyword, Expr value",
                "Class      : Token name, Expr.Var parent, List<Stmt.Function> methods"

            };
            DefineAst("Stmt", statementTypes);
        }

        private static void DefineAst(string parentName, List<string> expressionTypes)
        {
            string outputFile = $"{parentName}.cs";
            using (StreamWriter file = new StreamWriter(@outputFile))
            {
                file.WriteLine("using System;");
                file.WriteLine("using System.Collections.Generic;");
                file.WriteLine("using FITRP;");

                file.WriteLine();
                file.WriteLine($"abstract class {parentName}");
                file.WriteLine("{");

                foreach (string type in expressionTypes)
                {
                    file.WriteLine();
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(file, parentName, className, fields);
                    file.WriteLine();
                }
                file.WriteLine();
                DefineVisitor(file, parentName, expressionTypes);

                file.WriteLine();
                file.WriteLine("    public abstract T Accept<T>(Visitor<T> visitor);");

                file.WriteLine("}");
            }
        }

        private static void DefineVisitor(StreamWriter file, string parentName, List<string> expressionTypes)
        {
            file.WriteLine("    public interface Visitor<T>");
            file.WriteLine("    {");
            file.WriteLine();
            foreach(string type in expressionTypes)
            {
                string typeName = type.Split(":")[0].Trim();
                file.WriteLine($"       T visit({typeName} {parentName.ToLower()}); ");
            }
            file.WriteLine();
            file.WriteLine("    }");
        }

        private static void DefineType(StreamWriter file, string parentName, string className, string types)
        {
            file.WriteLine($"   public class {className} : {parentName}");
            file.WriteLine("    {");


            foreach(string type in types.Split(", "))
            {
                file.WriteLine($"       public readonly {type};");
            }
            
            file.WriteLine();

            file.WriteLine($"       public {className} ({types})");
            file.WriteLine("       {");

            foreach (string type in types.Split(", "))
            {
                string name = type.Split(" ")[1];
                file.WriteLine($"       this.{name} = {name};");
            }

            file.WriteLine("       }");

            file.WriteLine();

            file.WriteLine("        public override T Accept<T>(Visitor<T> visitor)");
            file.WriteLine("        {");
            file.WriteLine("            return visitor.visit(this);");
            file.WriteLine("        }");

            file.WriteLine();

            file.WriteLine("    }");
        }
    }
}
