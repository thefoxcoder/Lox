using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Lox;
using Environment = System.Environment;

namespace Lox
{
    public class Clock : ILoxCallable
    {
        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        }

        public int Arity()
        {
            return 0;
        }
     
        public override string ToString()
        {
            return "<native fn>";
        }
    }

    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        public Environment Globals;
        private Environment _environment;

        public Interpreter()
        {
            Globals = new Environment();
            Globals.Define("clock", new Clock());

            _environment = Globals;
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            var value = Evaluate(expr.Value);
            _environment.Assign(expr.Name, value);
            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }

                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.Op, "Operands must be two numbers or two strings.");
                case TokenType.GREATER:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL: return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: return IsEqual(left, right);
            }

            // Unreachable.
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            var callee = Evaluate(expr.Callee);

            if (callee is not ILoxCallable)
            {
                throw new RuntimeError(expr.Paren, "Can only call functions and classes");
            }

            var arguments = expr.Arguments
                .Select(Evaluate).ToList();

            var function = (ILoxCallable)callee;

            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(
                    expr.Paren,
                    $"Expected {function.Arity()} but got {arguments.Count}."
                );
            }

            return function.Call(this, arguments);
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double)
            {
                string text = obj.ToString();

                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2); //todo: Correct?
                }


                return text;
            }

            return obj.ToString();
        }

        public object VisitTernaryExpr(Expr.Ternary expr)
        {
            throw new NotImplementedException();
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            var left = Evaluate(expr.Left);

            if (expr.Op.Type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.Right);
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Op, right);
                    return -(double)right;
            }

            // Unreachable.
            return null;
        }


        private void CheckNumberOperand(Token @operator, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        private void CheckNumberOperand(Token @operator, object left, object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(@operator, "Operands must be numbers.");
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool)
            {
                return (bool)obj;
            }
            return true;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return _environment.Get(expr.Name);
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.Expr);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new LoxFunction(stmt, _environment);
            _environment.Define(stmt.Name.Lexeme, function);

            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            var value = Evaluate(stmt.Expr);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;

            if (stmt.Value != null)
            {
                value = Evaluate(stmt.Value);
            }

            throw new Return(value);
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;

            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            _environment.Define(stmt.Name.Lexeme, value);
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(stmt.Condition))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }

            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }

            return null;
        }


        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var previous = _environment;

            try
            {
                _environment = environment;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previous;
            }
        }
    }

    public class Return : Exception
    {
        public readonly object Value;

        public Return(object value)
        {
            Value = value;
        }
    }
}
