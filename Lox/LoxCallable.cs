using System.Collections.Generic;

namespace Lox
{
    public interface  ILoxCallable
    {
        public object Call(Interpreter interpreter, List<object> arguments);
        public int Arity();
    }
}