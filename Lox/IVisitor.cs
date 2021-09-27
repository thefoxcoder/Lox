namespace Lox
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
}