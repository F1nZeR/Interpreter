namespace Interpreter.Model
{
    public class Variable
    {
        public string Name { get; private set; }
        public object Value { get; set; }
        public bool IsConst { get; set; }

        public Variable(string name, object value)
        {
            Name = name;
            Value = value;
            IsConst = false;
        }

        public new string ToString()
        {
            return Value.ToString();
        }
    }
}