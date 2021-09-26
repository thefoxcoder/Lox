using System;
using System.IO;

namespace Lox
{
    class Lox
    {
        static bool HadError = false;

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
                Environment.Exit(65); //Error code?
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
            var expression = parser.Parse();

            // Stop if there was a syntax error.
            if (HadError)
            {
                return;
            }

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        public static void Error(int line, String message)
        {
            Report(line, "", message);
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
