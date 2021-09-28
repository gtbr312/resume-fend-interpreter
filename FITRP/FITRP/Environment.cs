using System;
using System.Collections.Generic;
using System.Text;

namespace FITRP
{
    class Environment
    {
        public readonly Environment enclosing;

        private readonly Dictionary<string, Object> values = new Dictionary<string, object>();

        public Environment()
        {
            enclosing = null;
        }
        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, Object value)
        {
            values[name] = value;
        }
        
        public Object Get(Token name)
        {
            if (values.ContainsKey(name.lexeme)) {
                return values[name.lexeme];
            }

            if (enclosing != null) return enclosing.Get(name);

            throw new RuntimeError(name, $"Attempt to access undeclared variable, {name.lexeme}.");
        }

        internal void Assign(Token name, object value)
        {

            if (values.ContainsKey(name.lexeme)) {
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null) 
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Attempted assignment to undeclared variable , {name}");
        }

        public Dictionary<string, Object> GetEnv()
        {
            return values;
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance).values[name];
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).values[name.lexeme] = value;
        }

        private Environment Ancestor(int distance)
        {
            Environment environment = this;
            for(int i = 0; i < distance; i++) {
                environment = environment.enclosing;
            }
            return environment;
        }
    }
}
