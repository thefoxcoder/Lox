namespace Lox
{
    public abstract class Expr
    {
        public interface IVisitor<R>
        {
            R VisitAssignExpr(Expr.Assign expr);
            R VisitBinaryExpr(Expr.Binary expr);
            R VisitTernaryExpr(Expr.Ternary expr);
            R VisitGroupingExpr(Expr.Grouping expr);
            R VisitLiteralExpr(Expr.Literal expr);
            R VisitLogicalExpr(Expr.Logical expr);
            R VisitUnaryExpr(Expr.Unary expr);
            R VisitVariableExpr(Expr.Variable expr);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        public class Assign : Expr
        {
            public Token Name;
            public Expr Value;

            public Assign(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }
        }

        public class Binary : Expr
        {
            public Expr Left;
            public Token Op;
            public Expr Right;

            public Binary(Expr left, Token op, Expr right)
            {
                Left = left;
                Op = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }
        }

        public class Ternary : Expr
        {
            public Expr Condition;
            public Expr IfTrue;
            public Expr IfFalse;

            public Ternary(Expr condition, Expr ifTrue, Expr ifFalse)
            {
                Condition = condition;
                IfTrue = ifTrue;
                IfFalse = ifFalse;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitTernaryExpr(this);
            }
        }

        public class Grouping : Expr
        {
            public Expr Expression;

            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

        }

        public class Literal : Expr
        {
            public object Value;

            public Literal(object value)
            {
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }
        }

        public class Logical : Expr
        {
            public Expr Left;
            public Token Op;
            public Expr Right;

            public Logical(Expr left, Token op, Expr right)
            {
                Left = left;
                Op = op;
                Right = right;
            }


            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }
        }

        public class Unary : Expr
        {
            public Token Op;
            public Expr Right;

            public Unary(Token op, Expr right)
            {
                Op = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }
        }

        public class Variable : Expr
        {
            public Token Name;

            public Variable(Token name)
            {
                Name = name;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }
        }
    }
}