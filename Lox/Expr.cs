namespace Lox
{
    public abstract class Expr
    {
        public interface IVisitor<R>
        {
            R Visit(Expr.Binary expr);
            R Visit(Expr.Ternary expr);
            R Visit(Expr.Grouping expr);
            R Visit(Expr.Literal expr);
            R Visit(Expr.Logical expr);
            R Visit(Expr.Unary expr);
            R Visit(Expr.Variable expr);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

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
                return visitor.Visit(this);
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
                return visitor.Visit(this);
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
                return visitor.Visit(this);
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
                return visitor.Visit(this);
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
                return visitor.Visit(this);
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
                return visitor.Visit(this);
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
                return visitor.Visit(this);
            }
        }
    }
}