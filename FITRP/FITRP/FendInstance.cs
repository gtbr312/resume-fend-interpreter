using System;
using System.Collections.Generic;

namespace FITRP
{
    class FendInstance
    {
        private FendClass _class;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public FendInstance(FendClass _class)
        {
            this._class = _class;
        }

        public override string ToString()
        {
            return $"Instance of {_class.name}";
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.lexeme)) {
                return fields[name.lexeme];
            }

            FendFunction method = _class.GetMethod(name.lexeme);
            if (method != null) return method.Bind(this);

            throw new RuntimeError(name, $"Attempted to access undeclared property {name.lexeme} on instance {_class.name}");
        }

        internal void Set(Token name, object value)
        {
            fields[name.lexeme] = value;
        }
    }
}