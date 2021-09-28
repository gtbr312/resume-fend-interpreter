using System;
using System.Collections.Generic;
using System.Text;

namespace FITRP
{
    class FendNativeFunction : IFendCallable
    {
        public int arity;

        public Func<object> func;

        public FendNativeFunction(int arity, Func<object> call)
        {
            this.arity = arity;
            func = call;
        }

        public int Arity()
        {
            return arity;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            return func();
        }

    }
}
