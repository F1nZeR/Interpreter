using System;
using Interpreter.Exceptions;
using Interpreter.Utilities;

namespace Interpreter.Model
{
    public enum LexemType
    {
        None,
        Number,
        Variable,
        Function,
        Operator,
        LeftBracket,
        RightBracket,
        EndOfExpr,
        String,
        Boolean,
        SpecialWord,
        Brace,
        LeftBrace,
        ModificatorCandidate,
        Modificator
    }

    public class Lexem
    {
        public LexemType LexemType { get; set; }
        public object Content;

        public readonly int Position;

        public Lexem(object content, LexemType tokenType, int startPos)
        {
            this.LexemType = tokenType;
            this.Content = content;
            this.Position = startPos;
        }

        public Lexem(string content)
        {
            this.LexemType = LexemType.None;
            this.Content = content;
        }

        public double ToDouble()
        {
            double res;
            if (LexemType == LexemType.Variable)
            {
                if (this.Content is Variable)
                {
                    if (double.TryParse((Content as Variable).Value.ToString(), out res))
                    {
                        return res;
                    }
                    throw new SyntaxException("Переменная является строкой", this);
                }

                if (double.TryParse(Storage.GetVariable(Content).ToString(), out res))
                {
                    return res;
                }
                throw new SyntaxException("Переменная является строкой", this);
            }

            if (LexemType == LexemType.Number) return Convert.ToDouble(Content.ToString());

            if (LexemType == LexemType.String)
            {
                try
                {
                    return Convert.ToDouble(Content.ToString());
                }
                catch (Exception)
                {
                    throw new SyntaxException("Переменная является строкой", this);
                }
            }

            throw new SyntaxException("Неверная операция", this);
        }

        public new string ToString()
        {
            if (LexemType == LexemType.Variable)
            {
                if (this.Content is Variable)
                {
                    return (Content as Variable).Value.ToString();
                }
                return Storage.GetVariable(Content.ToString()).ToString();
            }

            if (LexemType == LexemType.String || LexemType == LexemType.Number)
            {
                return Content.ToString();
            }

            throw new SyntaxException("Неверная операция", this);
        }

        public bool ToBoolean()
        {
            bool res;
            if (LexemType == LexemType.Variable)
            {
                if (this.Content is Variable)
                {
                    if (bool.TryParse((Content as Variable).Value.ToString(), out res))
                    {
                        return res;
                    }
                    throw new SyntaxException("Переменная является строкой", this);
                }

                if (bool.TryParse(Storage.GetVariable(Content.ToString()).ToString(), out res))
                {
                    return res;
                }
                throw new SyntaxException("Переменная является строкой", this);
            }

            if (LexemType == LexemType.Boolean) return Convert.ToBoolean(Content.ToString());

            throw new SyntaxException("Неверная операция", this);
        }
    }
}
