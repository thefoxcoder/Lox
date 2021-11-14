using System.Collections.Generic;

namespace Lox
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitExpressionStmt(Expression stmt);
            R VisitPrintStmt(Print stmt);
            R VisitVarStmt(Var stmt);
            R VisitBlockStmt(Block stmt);
            R VisitIfStmt(If stmt);
            R VisitWhileStmt(While stmt);
            R VisitFunctionStmt(Function function);
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

        public class Var : Stmt
        {
            public Token Name;
            public Expr Initializer;

            public Var(Token name, Expr initializer)
            {
                Name = name;
                Initializer = initializer;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }

        public class While : Stmt
        {
            public Expr Condition;
            public Stmt Body;

            public While(Expr condition, Stmt body)
            {
                Condition = condition;
                Body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }
        }

        public class Block : Stmt
        {
            public List<Stmt> Statements;

            public Block(List<Stmt> statements)
            {
                Statements = statements;
            }


            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }
        }

        public class If : Stmt
        {
            public Expr Condition;
            public Stmt ThenBranch;
            public Stmt ElseBranch;

            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }
        }

        public class Function : Stmt
        {
            public Token Name;
            public List<Token> Params;
            public List<Stmt> Body;

            public Function(Token name, List<Token> @params, List<Stmt> body)
            {
                Name = name;
                Params = @params;
                Body = body;
            }


            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }
        }
    }
}