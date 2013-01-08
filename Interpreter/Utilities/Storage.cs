using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter.Exceptions;
using Interpreter.Model;
using Array = Interpreter.Model.Array;

namespace Interpreter.Utilities
{
    public static class Storage
    {
        public static string Output;
        private static readonly List<Variable> Variables = new List<Variable>();
 
        public static void AddVariable(Lexem nameIn, object value)
        {
            var name = nameIn.Content.ToString();
            if (Variables.Any(x => x.Name == name))
                throw new SyntaxException("Переменная с таким именем уже существует", nameIn);
            var variable = new Variable(name, value);
            Variables.Add(variable);
        }

        public static bool VariableExists(object nameIn)
        {
            string name;
            if (nameIn is Lexem) name = ((Lexem) nameIn).Content.ToString();
            else name = nameIn.ToString();
            return Variables.Any(x => x.Name == name);
        }

        public static void AddArrayVariable(Lexem nameIn, Lexem size)
        {
            var name = nameIn.Content.ToString();
            try
            {
                var arrSize = Convert.ToInt32(size.ToDouble());
                var array = new Array(arrSize, name);

                AddVariable(nameIn, array);
            }
            catch (Exception)
            {
                throw new SyntaxException("Размерность массива должна быть целочисленной", size);
            }
        }

        public static Variable GetVariable(object nameIn, Lexem pos = null)
        {
            var name = nameIn.ToString();
            if (pos == null)
            {
                return Variables.Single(x => x.Name == name);
            }

            var position = Convert.ToInt32(pos.ToDouble());
            try
            {
                return (Variables.Single(x => x.Name == name).Value as Array).GetElement(position);
            }
            catch (Exception ex)
            {
                throw new SyntaxException(ex.Message, pos);
            }
        }

        public static void SetVarConst(object variable)
        {
            if (variable is Variable)
            {
                ((Variable)variable).IsConst = true;
            }
            else
            {
                Variables.Single(x => x.Name == variable.ToString()).IsConst = true;
            }
        }

        public static void SetVariableValue(Lexem nameIn, object value)
        {
            if (nameIn.Content is Variable)
            {
                var concrVar = nameIn.Content as Variable;
                if (concrVar.IsConst)
                {
                    throw new SyntaxException("Нельзя изменить значение константы", nameIn);
                }
                concrVar.Value = value;
                return;
            }

            var name = nameIn.Content.ToString();
            var variable = Variables.Single(x => x.Name == name);
            if (variable.IsConst)
            {
                throw new SyntaxException("Невозможно изменить значение константы", nameIn);
            }
            variable.Value = value;
        }

        // -- OUTPUT --

        public static void Clear()
        {
            Variables.Clear();
            Output = string.Empty;
        }

        public static void AddToOutput(IEnumerable<Lexem> lexems, bool onNewLine)
        {
            var symbol = onNewLine ? "\r\n" : " ";
            foreach (var lexem in lexems)
            {
                if (lexem.Content is Variable)
                {
                    Output += (lexem.Content as Variable).Value + symbol;
                } 
                else Output += lexem.ToString() + symbol;
            }
            if (onNewLine) Output += symbol;
        }
    }
}
