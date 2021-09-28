using System;
using System.Collections.Generic;

namespace FITRP
{
    class FendClass : IFendCallable
    {
        public readonly string name;
        public readonly FendClass parent;
        public readonly Dictionary<string, FendFunction> methods;

        public FendClass(string name, FendClass parent, Dictionary<string, FendFunction> methods)
        {
            this.name = name;
            this.parent = parent;
            this.methods = methods;
        }

        public int Arity()
        {
            FendFunction initializer = GetMethod("Init");
            if (initializer == null) return 0;
            return initializer.Arity();
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            FendInstance instance = new FendInstance(this);

            FendFunction initializer = GetMethod("Init");
            if(initializer != null) {
                initializer.Bind(instance).Call(interpreter, args);
            }

            return instance;
        }

        public override string ToString()
        {
            return name;
        }

        public FendFunction GetMethod(string name)
        {
            if (methods.ContainsKey(name)) {
                return methods[name];
            }

            if(parent != null) {
                return parent.GetMethod(name);
            }

            return null;
        }
    }
}