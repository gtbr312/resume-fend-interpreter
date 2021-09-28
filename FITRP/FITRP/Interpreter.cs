using System;
using System.Collections.Generic;
using System.Text;
using static Expr;
using static TokenType;

namespace FITRP
{
    class Interpreter : Visitor<Object>, Stmt.Visitor<bool?>// this is a hack. It should be of type void
    {
        public readonly Environment globals = new Environment();
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();
        private Environment environment;

        public Interpreter()
        {
            environment = globals;

            object Call()
            {
            return (double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            }
            IFendCallable clock = new FendNativeFunction(0,Call);
            globals.Define("Clock", clock);
        }

        public void Interpret(List<Stmt> statements)
        {
            try {
                foreach (Stmt stmt in statements) {
                    Execute(stmt);
                }
            }
            catch (RuntimeError error) {
                FendInterpreter.RuntimeError(error);
            }
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public bool? visit(Stmt.Class stmt)
        {

            Object parent = null;
            if(stmt.parent != null) {
                parent = Evaluate(stmt.parent);
                if(!(parent is FendClass)) {
                    throw new RuntimeError(stmt.parent.name, "Attempt to inherit from non-class type.");
                }
            }

            environment.Define(stmt.name.lexeme, null);


            if(stmt.parent != null) {
                environment = new Environment(environment);
                environment.Define("parent", parent);
            }


            Dictionary<string, FendFunction> methods = new Dictionary<string, FendFunction>();
            foreach(Stmt.Function method in stmt.methods) {
                FendFunction function = new FendFunction(method, environment, method.name.lexeme.Equals("Init"));
                methods[method.name.lexeme] = function;
            }

            FendClass _class = new FendClass(stmt.name.lexeme, (FendClass)parent,  methods);

            if(parent != null) {
                environment = environment.enclosing;
            }

            environment.Assign(stmt.name, _class);
            return null;
        }

        public bool? visit(Stmt.Function stmt)
        {
            FendFunction function = new FendFunction(stmt, environment, false);
            environment.Define(stmt.name.lexeme,function);
            return null;
        }

        public bool? visit(Stmt.Return stmt)
        {
            Object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        public bool? visit(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.body);
            }
            return null;
        }

        public bool? visit(Stmt.IfStmt stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.thenBranch);
            } else if (stmt.elseBranch != null) {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public bool? visit(Stmt.VarDec stmt)
        {
            Object value = null;
            if (stmt.initializer != null) {
                value = Evaluate(stmt.initializer);
            }
            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public bool? visit(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public bool? visit(Stmt.Print stmt)
        {
            Object value = Evaluate(stmt.expression);
            Console.WriteLine(MakeString(value));
            return null;
        }

        public bool? visit(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            this.environment = environment;
            try {
                foreach (Stmt statement in statements) {
                    Execute(statement);
                }
            }
            finally {
                this.environment = previous;
            }
        }
        
        public object visit(This expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }

        public object visit(Super expr)
        {
            int distance = locals[expr];
            FendClass parent = (FendClass)environment.GetAt(distance, "parent");
            FendInstance objct = (FendInstance)environment.GetAt(distance - 1, "this");

            FendFunction method = parent.GetMethod(expr.method.lexeme);

            if(method == null) {
                throw new RuntimeError(expr.method, $"Attempted to access undefined property {expr.method.lexeme}");
            }

            return method.Bind(objct);
        }

        public object visit(Get expr)
        {
            Object objct = Evaluate(expr.objct);
            if(objct is FendInstance) {
                return ((FendInstance)objct).Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Atttempted to access proptery on non instance type.");
        }

        public object visit(Set expr)
        {
            Object objct = Evaluate(expr.objct);

            if(!(objct is FendInstance)) {
                throw new RuntimeError(expr.name, "Attempted to set property on non instanced type.");
            }

            object value = Evaluate(expr.value);
            ((FendInstance)objct).Set(expr.name, value);
            return value;
        }

        public object visit(Assign expr)
        {
            Object value = Evaluate(expr.value);

            if (locals.ContainsKey(expr)) {
                int distance = locals[expr];
                environment.AssignAt(distance, expr.name, value);
            } else {
                globals.Assign(expr.name, value);
            }

            environment.Assign(expr.name, value);
            return value;
        }

        public object visit(Logical expr)
        {
            Object left = Evaluate(expr.left);

            if(expr.oprtr.type == OR) {
                if (IsTruthy(left)) return left;
            } else if(expr.oprtr.type == AND){
                if (!IsTruthy(left)) return left;
            }
            return Evaluate(expr.right);

        }

        public object visit(Var expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        public object visit(Literal expr)
        {
            return expr.value;
        }

        public object visit(Unary expr)
        {
            Object right = Evaluate(expr.right);

            switch (expr.oprtr.type) {
                case BANG:
                    return !IsTruthy(right);
                case MINUS:
                    CheckIsNumber(expr.oprtr, right);
                    return -(double)right;
            }

            return null;
        }

        public object visit(Call expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach(Expr argument in expr.args) {
                arguments.Add(Evaluate(argument));
            }

            if(!(callee is IFendCallable)) {
                throw new RuntimeError(expr.paren, "Attempt to call value type other than function or class");
            }

            IFendCallable function = (IFendCallable)callee;

            if(arguments.Count != function.Arity()) {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity()} arguments but got {arguments.Count}");
            }

            return function.Call(this, arguments);
        }

        public object visit(Binary expr)
        {
            Object left = Evaluate(expr.left);
            Object right = Evaluate(expr.right);

            switch (expr.oprtr.type) {
                case PLUS:
                    if (left is double && right is double) {
                        return (double)left + (double)right;
                    }

                    if (left is string && right is string) {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.oprtr, "Operands must be either numbers or strings and of like types");
                case MINUS:
                    CheckIsNumber(expr.oprtr, left, right);
                    return (double)left - (double)right;
                case SLASH:
                    CheckIsNumber(expr.oprtr, left, right);
                    return (double)left / (double)right;
                case STAR:
                    CheckIsNumber(expr.oprtr, left, right);
                    return (double)left * (double)right;
                case GREATER:
                    CheckIsNumber(expr.oprtr, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    CheckIsNumber(expr.oprtr, left, right);
                    return (double)left >= (double)right;
                case LESS:
                    CheckIsNumber(expr.oprtr, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    CheckIsNumber(expr.oprtr, left, right);
                    return (double)left <= (double)right;
                case BANG_EQUAL: 
                    return !isEqual(left, right);
                case EQUAL_EQUAL: 
                    return isEqual(left, right);
            }

            return null;
        }

        public object visit(Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        private object Evaluate(Expr expression)
        {
            return expression.Accept(this);
        }

        private bool IsTruthy(Object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;

            return true;
        }


        private bool isEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private void CheckIsNumber(Token oprtr, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(oprtr, "Operand must be a number.");
        }

        private void CheckIsNumber(Token oprtr, object a, object b)
        {
            if (a is double && b is double) return;
            throw new RuntimeError(oprtr, "Operands must be numbers.");
        }
        private string MakeString(object value)
        {
            if (value == null) return "null";
            if(value is double) {
                string text = value.ToString();
                if (text.EndsWith(".0")) {
                    text = text.Substring(0, text.Length - 3);
                }
                return text;
            }

            return value.ToString();
        }

        public Dictionary<string, Object> GetEnv()
        {
            return environment.GetEnv();
        }

        public void Resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            if (locals.ContainsKey(expr)) {
                int distance = locals[expr];
                return environment.GetAt(distance, name.lexeme);
            } else {
                return globals.Get(name);
            }
        }
    }
}
