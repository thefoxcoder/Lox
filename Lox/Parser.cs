using System;
using System.Collections.Generic;

namespace Lox
{
    public class Parser
    {
        private class ParseError : Exception { }

        private readonly List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();

            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Expr Expression()
        {
            return AnonymousFunction();
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.FUN))
                {
                    return Function("function");
                }

                if (Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }


        private Stmt Statement()
        {
            if (Match(TokenType.FOR))
            {
                return ForStatement();
            }

            if (Match(TokenType.IF))
            {
                return IfStatement();
            }

            if (Match(TokenType.WHILE))
            {
                return WhileStatement();
            }

            if (Match(TokenType.RETURN))
            {
                return ReturnStatement();
            }

            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }

            if (Match(TokenType.LEFT_BRACE))
            {
                return new Stmt.Block(Block());
            }

            return ExpressionStatement();
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            var body = Statement();

            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt> { body, new Stmt.Expression(increment) });
            }

            if (condition == null)
            {
                condition = new Expr.Literal(true);
            }

            body = new Stmt.While(condition, body);

            if (initializer != null)
            {
                body = new Stmt.Block(new List<Stmt> { initializer, body });
            }

            return body;
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            var thenBranch = Statement();
            Stmt elseBranch = null;

            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt ReturnStatement()
        {
            var keyword = Previous();
            Expr value = null;

            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after return value.");

            return new Stmt.Return(keyword, value);
        }

        private Stmt Function(string kind)
        {
            var name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
            Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
            var parameters = new List<Token>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
            Consume(TokenType.LEFT_BRACE, $"Expect {{ before {kind} body.");
            var body = Block();

            return new Stmt.Function(name, parameters, body);
        }

        private Stmt VarDeclaration()
        {
            var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            var condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            var body = Statement();

            return new Stmt.While(condition, body);
        }

        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Expr AnonymousFunction()
        {
            if (Match(TokenType.FUN))
            {
                Consume(TokenType.LEFT_PAREN, "Expect '(' after fun.");
                var parameters = new List<Token>();

                if (!Check(TokenType.RIGHT_PAREN))
                {
                    do
                    {
                        if (parameters.Count >= 255)
                        {
                            Error(Peek(), "Can't have more than 255 parameters.");
                        }

                        parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                    } while (Match(TokenType.COMMA));
                }

                Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
                Consume(TokenType.LEFT_BRACE, $"Expect {{ before function body.");

                var body = Block();

                return new Expr.AnonymousFunction(parameters, body);
            }

            return Assignment();
        }

        private Expr Assignment()
        {
            //var expr = Ternary();
            var expr = Or();

            if (Match(TokenType.EQUAL))
            {
                var equals = Previous();
                var value = AnonymousFunction();

                if (expr is Expr.Variable)
                {
                    var name = ((Expr.Variable)expr).Name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            var expr = And();

            while (Match(TokenType.OR))
            {
                var op = Previous();
                var right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            var expr = Equality();

            while (Match(TokenType.AND))
            {
                var op = Previous();
                var right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr Ternary()
        {
            var expr = Equality();

            if (Match(TokenType.TERNARY_QUESTION_MARK))
            {
                var ifTrue = Ternary();

                if (!Match(TokenType.TERNARY_COLON))
                {
                    throw Error(Peek(), "Expected :");
                }

                var ifFalse = Ternary();

                expr = new Expr.Ternary(expr, ifTrue, ifFalse);
            }

            return expr;
        }

        private Expr Equality()
        {
            var expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                var op = Previous();
                var right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            var expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                var op = Previous();
                var right = Term();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Term()
        {
            var expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                var op = Previous();
                var right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            var expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                var op = Previous();
                var right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                var op = Previous();
                var right = Unary();
                return new Expr.Unary(op, right);
            }

            return Call();
        }

        private Expr Call()
        {
            var expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            var paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message);
        }


        private bool Check(TokenType type)
        {
            if (IsAtEnd())
            {
                return false;
            }
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                _current++;
            }
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private ParseError Error(Token token, String message)
        {
            Lox.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }

        private Token Previous()
        {
            return _tokens[_current - 1]; ;
        }
    }
}
