using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class AstPrinter : IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string Visit(Expr.Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string Visit(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string Visit(Expr.Literal expr)
        {
            return expr.Value?.ToString() ?? "nil";
        }

        public string Visit(Expr.Logical expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string Visit(Expr.Unary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Right);
        }

        public string Visit(Expr.Variable expr)
        {
            return Parenthesize("var", expr);
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            var builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
