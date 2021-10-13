﻿using System;
using System.Text;

namespace Lox
{
    public class AstPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            throw new NotImplementedException();
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string VisitTernaryExpr(Expr.Ternary expr)
        {
            return Parenthesize("ternary", expr.Condition, expr.IfTrue, expr.IfFalse);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value?.ToString() ?? "nil";
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Right);
        }

        public string VisitVariableExpr(Expr.Variable expr)
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
