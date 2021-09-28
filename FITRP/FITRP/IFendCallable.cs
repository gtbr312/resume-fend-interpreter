using System.Collections.Generic;

namespace FITRP
{
    interface IFendCallable
    {
        int Arity();
        object Call(Interpreter interpreter, List<object> args);
        string ToString();
    }
}