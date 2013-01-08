using System;
using System.Collections.Generic;

namespace Interpreter.Model
{
    public class Array
    {
        private readonly int _size;
        private readonly List<Variable> _variables = new List<Variable>();

        public Array(int size, string name)
        {
            _size = size;
            for (int i = 0; i < size; i++)
            {
                _variables.Add(new Variable(name, 0));
            }
        }

        public Variable GetElement(int index)
        {
            if (index >= _size) throw new Exception("Выход за границы массива");
            return _variables[index];
        }
    }
}