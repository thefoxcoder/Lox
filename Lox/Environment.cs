using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Environment
    {
        private readonly Dictionary<string, object> _values = new();

        public void Define(string name, object value)
        {
            _values.Add(name, value);
        }

        public object Get(Token name)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                return _values[name.Lexeme];
            }

            throw new RuntimeError(name, $"Undefined variable {name.Lexeme}.");
        }
    }
}
