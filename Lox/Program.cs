using System;
using System.IO;

namespace Lox
{
    class Lox
    {
        static bool HadError = false;
        static bool HadRuntimeError = false;
        private static Interpreter _interpreter = new Interpreter();

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

            if (HadError)
            {
                System.Environment.Exit(65); //Error code?
            }

            if (HadRuntimeError)
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
                HadError = false;
            }
        }

        private static void Run(String source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            var statements = parser.Parse();

            // Stop if there was a syntax error.
            if (HadError)
            {
                return;
            }
            
            _interpreter.Interpret(statements);
            //Console.WriteLine(new AstPrinter().Print(expression));
        }

        public static void Error(int line, String message)
        {
            Report(line, "", message);
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
            HadRuntimeError = true;
        }

        public static void Error(Token token, String message)
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
            HadError = true;
        }
    }
}
