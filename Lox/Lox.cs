using System;
using System.IO;

namespace Lox
{
    class Lox
    {
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;
        private static readonly Interpreter Interpreter = new();

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            var text = File.ReadAllText(Path.GetFullPath(path));
            Run(text);

            if (_hadError)
            {
                System.Environment.Exit(65); //Error code?
            }

            if (_hadRuntimeError)
            {
                System.Environment.Exit(70); //Error code?
            }
        }


        private static void RunPrompt()
        {
            while (true)
            {
                Console.WriteLine("> ");
                var line = Console.ReadLine();

                if (line == null)
                {
                    break;
                }
                Run(line);
                _hadError = false;
            }
        }

        private static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();

            var parser = new Parser(tokens);
            var statements = parser.Parse();

            // Stop if there was a syntax error.
            if (_hadError)
            {
                return;
            }
            
            Interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
            _hadRuntimeError = true;
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        private static void Report(int line, string where, string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            _hadError = true;
        }
    }
}
