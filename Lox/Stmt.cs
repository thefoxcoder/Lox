namespace Lox
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitExpressionStmt(Stmt.Expression stmt);
            R VisitPrintStmt(Stmt.Print stmt);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        public class Expression : Stmt
        {
            public Expr Expr;

            public Expression(Expr expr)
            {
                Expr = expr;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class Print : Stmt
        {
            public Expr Expr;

            public Print(Expr expr)
            {
                Expr = expr;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }
    }
}