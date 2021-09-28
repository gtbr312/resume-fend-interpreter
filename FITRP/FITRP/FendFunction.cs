using System;
using System.Collections.Generic;
using System.Text;
using static Stmt;

namespace FITRP
{
    class FendFunction : IFendCallable
    {
        private readonly Function declaration;
        private readonly Environment closure;

        private readonly bool isInitializer;

        public FendFunction(Function declaration, Environment closure, bool isInitializer)
        {
            this.isInitializer = isInitializer;
            this.declaration = declaration;
            this.closure = closure;
        }

        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            Environment environment = new Environment(closure);

            for(int i = 0; i < declaration.parameters.Count; i++) {
                environment.Define(declaration.parameters[i].lexeme, args[i]);
            }
            try {
                interpreter.ExecuteBlock(declaration.body,environment);
            }catch(Return returnVal) {
                if (isInitializer) return closure.GetAt(0, "this");
                return returnVal.value;
            }

            if (isInitializer) return closure.GetAt(0, "this");

            return null;
        }

        public FendFunction Bind(FendInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", instance);
            return new FendFunction(declaration, environment, isInitializer);
        }

        public override string ToString()
        {
            string args = "";
            foreach(Token arg in declaration.parameters) {
                args += arg.lexeme + " ";
            }
            return $"{declaration.name.lexeme} Func <{args}>";
        }

    }
}
