using System.Collections.Generic;

namespace Lox
{
    public class LoxAnonymousFunction : ILoxCallable
    {
        private readonly Expr.AnonymousFunction _function;
        private readonly Environment _closure;

        public LoxAnonymousFunction(Expr.AnonymousFunction function, Environment closure)
        {
            _function = function;
            _closure = closure;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(_closure);

            for (var i = 0; i < _function.Params.Count; i++)
            {
                environment.Define(_function.Params[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_function.Body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.Value;
            }

            return null;
        }

        public int Arity()
        {
            return _function.Params.Count;
        }

        public override string ToString()
        {
            return "<fn anonymous>";
        }
    }
}