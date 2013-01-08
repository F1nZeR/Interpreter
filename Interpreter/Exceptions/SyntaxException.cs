using System;
using Interpreter.Model;

namespace Interpreter.Exceptions
{
    public class SyntaxException : Exception
    {
        public Lexem Lexem { get; private set; }

        public SyntaxException(string message, Lexem lexem) : base(message)
        {
            Lexem = lexem;
        }
    }
}