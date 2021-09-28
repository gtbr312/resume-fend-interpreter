using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FITRP
{
    class Resolver : Expr.Visitor<bool>, Stmt.Visitor<bool>
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;
        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public bool visit(Stmt.Class stmt) 
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.name);
            Define(stmt.name);


            if(stmt.parent != null && stmt.name.lexeme == stmt.parent.name.lexeme) {
                FendInterpreter.Error(stmt.parent.name, "Attempt to inherit from same class.");
            }

            if(stmt.parent != null) {
                currentClass = ClassType.SUBCLASS;
                Resolve(stmt.parent);
            }

            if(stmt.parent != null) {
                BeginScope();
                scopes.Peek().Add("parent", true);
            }

            BeginScope();
            scopes.Peek()["this"] = true;

            foreach(Stmt.Function method in stmt.methods) {
                FunctionType declaration = FunctionType.METHOD;
                if (method.name.lexeme.Equals("Init")) {
                    declaration = FunctionType.INITIALIZER;
                }
                ResolveFunction(method, declaration);
            }

            EndScope();

            if(stmt.parent != null) {
                EndScope();
            }

            currentClass = enclosingClass;

            return false;
        }

        public bool visit(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return false;
        }

        public bool visit(Stmt.VarDec stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null) {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return false;
        }

        public bool visit(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return false;

        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();

            foreach (Token param in function.parameters) {
                Declare(param);
                Define(param);
            }
            Resolve(function.body);
            EndScope();

            currentFunction = enclosingFunction;
        }

        public bool visit(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return false;
        }

        public bool visit(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return false;
        }

        public bool visit(Stmt.Return stmt)
        {
            if(currentFunction == FunctionType.NONE) {
                FendInterpreter.Error(stmt.keyword, "Cannot return from global scope.");
            }

            if(stmt.value != null) {

                if(currentFunction == FunctionType.INITIALIZER) {
                    FendInterpreter.Error(stmt.keyword, "Cannot return value from initializer");
                }

                Resolve(stmt.value);
            }

            return false;
        }

        public bool visit(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return false;
        }

        public bool visit(Stmt.IfStmt stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if(stmt.elseBranch != null) {
                Resolve(stmt.elseBranch);
            }
            return false;
        }

        public bool visit(Expr.This expr)
        {
            if (currentClass == ClassType.NONE) {
                FendInterpreter.Error(expr.keyword, "Attempted to use this keyword outside of class");
                return false;
            }

            ResolveLocal(expr, expr.keyword);
            return false;
        }

        public bool visit(Expr.Super expr)
        {
            if(currentClass == ClassType.NONE) {
                FendInterpreter.Error(expr.keyword, "Cannot use keyword parent outside of a class;");
            }else if (currentClass != ClassType.SUBCLASS) {
                FendInterpreter.Error(expr.keyword, "Cannot use keyword parent in class that does not inherit from another class.");
            }

            ResolveLocal(expr, expr.keyword);
            return false;
        }

        public bool visit(Expr.Get expr)
        {
            Resolve(expr.objct);
            return false;
        }

        public bool visit(Expr.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr.objct);
            return false;
        }

        public bool visit(Expr.Var expr)
        {
            if (scopes.Count > 0 && scopes.Peek().ContainsKey(expr.name.lexeme) && scopes.Peek()[expr.name.lexeme] == false) {
                FendInterpreter.Error(expr.name, "Can't read local variable in its own initializer.");
            }
            ResolveLocal(expr, expr.name);
            return false;
        }

        public bool visit(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return false;
        }

        public bool visit(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return false;
        }

        public bool visit(Expr.Call expr)
        {
            Resolve(expr.callee);

            foreach(Expr arg in expr.args) {
                Resolve(arg);
            }

            return false;
        }

        public bool visit(Expr.Grouping expr)
        {
            Resolve(expr);
            return false;
        }

        public bool visit(Expr.Literal expr)
        {
            return false;
        }

        public bool visit(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return false;
        }

        public bool visit(Expr.Unary expr)
        {
            Resolve(expr.right);
            return false;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for(int i = 0; i < scopes.Count; i++) {
                if (scopes.ElementAt(i).ContainsKey(name.lexeme)) {
                    interpreter.Resolve(expr, i);
                    return;
                }
            }
        }

        private void Define(Token name)
        {
            if (scopes.Count < 1) return;
            scopes.Peek()[name.lexeme] = true;
        }

        private void Declare(Token name)
        {
            if (scopes.Count < 1) return;
            Dictionary<string, bool> scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme)) {
                FendInterpreter.Error(name, "Attemped to redeclare variable in local scope.");
            }
            scope[name.lexeme] = false;
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach(Stmt stmt in statements) {
                Resolve(stmt);
            }
        }

        public void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }
    }
}
