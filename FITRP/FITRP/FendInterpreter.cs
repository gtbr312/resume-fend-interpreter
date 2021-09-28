using System;
using System.IO;
using System.Collections.Generic;

namespace FITRP
{
    class FendInterpreter
    {
        private static readonly Interpreter interpreter = new Interpreter();
        private static readonly ASTPrinter printer = new ASTPrinter();

        static bool hadError = false;
        static bool hadRuntimeError;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Argument quantity exceeded. Accepted Format: FITRP HelloWorld.fend");
            }else if(args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
            if (hadError) Console.WriteLine("Errors found in file.");
            if (hadRuntimeError) Console.WriteLine("Error occurred at runtime.");
        }

        private static void RunFile(string file)
        {
            try {
                if (!File.Exists(file))
                {
                throw new ArgumentException("File could not be found");
                }

                using (StreamReader sr = new StreamReader(file))
                {
                    string str = sr.ReadToEnd();
                    Run(str);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Could not read file");
                Console.WriteLine(e);
            }
        }

        private static void RunPrompt()
        {
            Console.WriteLine("\nFend REPL: Enter a line of fend to begin.\n");
            string line;
            while ((line = Console.ReadLine()).Length > 0)
            {
                if (line == "cls") {
                    Console.Clear();
                    Console.WriteLine("\nFend REPL: Enter a line of fend to begin.\n");
                    Console.WriteLine();
                } else if (line == "env") {
                    Dictionary<string, Object> envVals = interpreter.GetEnv();
                    foreach(var variable in envVals) {
                        Console.WriteLine($"{variable.Key} : {variable.Value}");
                    }
                    Console.WriteLine();
                } else {
                    hadError = false;
                    Run(line);
                }
            }

            Console.WriteLine("\nPress any key to exit\n");
            Console.ReadLine();
        }

        private static void Run(string source)
        {
            Lexer lexer = new Lexer(source);
            List<Token> tokens = lexer.AnalyzeTokens();

            Parser parser = new Parser(tokens);
            List<Stmt> statements= parser.Parse();

            
            //prints all tokens
            /*
            foreach(Token token in tokens)
            {
                Console.Write($"{ token.type} ");
            }
            */
            Console.WriteLine();

            if (hadError) return;
            //Console.WriteLine(printer.Print(expression));

            Resolver resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            if (hadError) return;

            interpreter.Interpret(statements);
        }

        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF) {
                Report(token.line, "at end", message);
            } else {
                Report(token.line, $"at '{token.lexeme}'", message);
            }
        }
        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine($"Runtime error on line {error.token.line + 1} at {error.token.lexeme}, {error.Message}");
            hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"line {line} : Error {where}: {message}");
            hadError = true;
        }
    }
}
