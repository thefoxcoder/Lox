using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Interpreter : Expr.IVisitor<Object>, Stmt.IVisitor<Object>
    {
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
        
        public object Visit(Expr.Binary expr)
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

                    if (left is string && right is string) {
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

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double) {
                string text = obj.ToString();
                
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2); //todo: Correct?
                }


                return text;
            }

            return obj.ToString();
        }

        public object Visit(Expr.Ternary expr)
        {
            throw new NotImplementedException();
        }

        public object Visit(Expr.Grouping expr)
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

        public object Visit(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object Visit(Expr.Logical expr)
        {
            throw new NotImplementedException();
        }

        public object Visit(Expr.Unary expr)
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

        public object Visit(Expr.Variable expr)
        {
            throw new NotImplementedException();
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.Expr);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            var value = Evaluate(stmt.Expr);
            Console.WriteLine(Stringify(value));
            return null;
        }
    }
}
