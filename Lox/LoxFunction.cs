using System.Collections.Generic;

namespace Lox
{
    public class LoxFunction : ILoxCallable
    {
        private readonly Stmt.Function _declaration;
        private readonly Environment _closure;

        public LoxFunction(Stmt.Function declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(_closure);

            for (var i = 0; i < _declaration.Params.Count; i++)
            {
                environment.Define(_declaration.Params[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.Value;
            }

            return null;
        }

        public int Arity()
        {
            return _declaration.Params.Count;
        }

        public override string ToString()
        {
            return $"<fn {_declaration.Name.Lexeme}>";
        }
    }
}