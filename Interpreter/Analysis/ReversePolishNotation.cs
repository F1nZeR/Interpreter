using System;
using System.Collections.Generic;
using System.Linq;
using Interpreter.Exceptions;
using Interpreter.Model;
using Interpreter.Utilities;

namespace Interpreter.Analysis
{
    public class ReversePolishNotation
    {
        private readonly Stack<Lexem> Stack = new Stack<Lexem>();
        private readonly Stack<Tuple<Lexem, Lexem>> _increaseStack = new Stack<Tuple<Lexem, Lexem>>();
        private readonly Stack<int> _whilePoses = new Stack<int>();

        private readonly List<Lexem> Output = new List<Lexem>();
        private List<Lexem> _lexems = new List<Lexem>(); 
        private LexemType _curLexType = LexemType.None;
        private bool _skipToCloseBrace, _curIfResult, _inElse;
        private StringToLexem _stToLex;
        private int _ifStateLevel = 0;
        private Random _rand = new Random();

        private void ToRPN()
        {
            var modificators = new Stack<Lexem>();
            Lexem lexem = null;
            Stack.Clear();
            int i;

            for (i = 0; i < _lexems.Count; i++)
            {
                lexem = _lexems[i];

                switch (lexem.LexemType)
                {
                    case LexemType.ModificatorCandidate:
                        throw new SyntaxException("Незнакомая лексема", lexem);

                    case LexemType.Modificator:
                        modificators.Push(lexem);
                        break;

                    case LexemType.SpecialWord:
                    case LexemType.Boolean:
                    case LexemType.String:
                    case LexemType.Number:
                    case LexemType.Variable:
                        Output.Add(lexem);
                        break;

                    case LexemType.Operator:
                        if (lexem.Content.Equals("]"))
                        {
                            var a = Stack.Peek();
                            while (Stack.Peek().LexemType != LexemType.LeftBrace)
                                Output.Add(Stack.Pop());

                            if (Stack.Peek().LexemType == LexemType.LeftBrace)
                                Stack.Pop();

                            if (Stack.Count > 0 && Stack.Peek().Content.Equals("var")) Output.Add(Stack.Pop());
                            Output.Add(lexem);
                            break;
                        }
                        while (Stack.Count > 0 && Priority(Stack.Peek().Content.ToString()) >= Priority(lexem.Content.ToString()))
                            Output.Add(Stack.Pop());
                        Stack.Push(lexem);
                        break;

                    case LexemType.LeftBracket:
                    case LexemType.LeftBrace:
                    case LexemType.Function:
                        Stack.Push(lexem);
                        break;

                    case LexemType.RightBracket:

                        while (Stack.Peek().LexemType != LexemType.LeftBracket)
                            Output.Add(Stack.Pop());

                        if (Stack.Peek().LexemType == LexemType.LeftBracket)
                            Stack.Pop();

                        if (Stack.Count > 0 && Stack.Peek().LexemType == LexemType.Function)
                            Output.Add(Stack.Pop());

                        break;

                    default:
                        break;
                }
            }

            while (Stack.Count > 0)
            {
                Output.Add(Stack.Pop());
            }

            while (modificators.Count > 0)
            {
                Output.Add(modificators.Pop());
            }

            Output.Add(lexem); // ";", "}", "{"

            return;
        }

        public void Interpret(string infix)
        {
            _stToLex = new StringToLexem(infix);

            while ((_lexems = _stToLex.ToLexems()).Count > 0 || _lexems.Any(x =>
                x.LexemType == LexemType.EndOfExpr || x.LexemType == LexemType.Brace))
            {
                if (_skipToCloseBrace)
                {
                    int opennedBraces = 0;
                    while (!_lexems.Any(x => x.LexemType == LexemType.Brace && x.Content.ToString() == "}" && opennedBraces == 0))
                    {
                        if (_lexems.Any(x => x.LexemType == LexemType.Brace))
                        {
                            var brace = _lexems.Single(x => x.LexemType == LexemType.Brace);
                            if (brace.Content.ToString() == "{") opennedBraces++; else opennedBraces--;
                        }
                        _lexems = _stToLex.ToLexems();
                    }
                }
                Output.Clear();
                ToRPN();
                Stack.Clear();
                Eval();
            }
        }

        private void Eval()
        {
            for (int i = 0; i < Output.Count; i++)
            {
                var isTaken = false;
                Lexem lex = Output[i];

                if (lex.LexemType == LexemType.SpecialWord)
                {
                    if (lex.Content.ToString() == "while")
                    {
                        _whilePoses.Push(lex.Position);
                    }
                    else if (lex.Content.ToString() == "if")
                    {
                        _ifStateLevel++;
                    }
                    else if (lex.Content.ToString() == "else")
                    {
                        _ifStateLevel++;
                        _inElse = true;
                    }
                }
                
                if (lex.LexemType == LexemType.Brace)
                {
                    if (lex.Content.ToString() == "{")
                    {
                        if (_inElse)
                        {
                            _skipToCloseBrace = _curIfResult;
                            return;
                        }

                        var conditionResult = Stack.Peek().ToBoolean();
                        if (Output.Any(x => x.LexemType == LexemType.SpecialWord && x.Content.ToString() == "if"))
                        {
                            _curIfResult = conditionResult;
                        }

                        if (conditionResult == false)
                        {
                            _skipToCloseBrace = true;
                            return;
                        }
                    }
                    else
                    {
                        if (_skipToCloseBrace)
                        {
                            _skipToCloseBrace = false;
                            if (_ifStateLevel == 0)
                            {
                                _whilePoses.Pop();
                            }
                        }
                        else
                        {
                            if (_ifStateLevel == 0)
                            {
                                isTaken = true;
                                _stToLex.currentPosition = _whilePoses.Pop();
                            }
                        }
                        if (!isTaken && _ifStateLevel > 0) _ifStateLevel--;
                        if (_inElse)
                        {
                            _inElse = false;
                            _curIfResult = false;
                        }
                        return;
                    }
                }

                if (lex.LexemType == LexemType.Variable || lex.LexemType == LexemType.Number ||
                    lex.LexemType == LexemType.Boolean || lex.LexemType == LexemType.String)
                {
                    Stack.Push(lex);
                }

                if (lex.LexemType == LexemType.Modificator)
                {
                    while (Stack.Count > 0)
                    {
                        var variable = Stack.Pop();
                        Storage.SetVarConst(variable.Content);
                    }
                }

                if (lex.LexemType == LexemType.Operator)
                {
                    if (lex.Content.Equals("++") || lex.Content.Equals("--"))
                    {
                        try
                        {
                            _increaseStack.Push(new Tuple<Lexem, Lexem>(Stack.Peek(), lex));
                            continue;
                        }
                        catch (Exception)
                        {
                            throw new SyntaxException("Ошибка выполнения операции " + lex.Content, lex);
                        }
                    }
                    else if (lex.Content.Equals("var"))
                    {
                        Lexem lexVar;
                        try
                        {
                            lexVar = Stack.Pop();
                        }
                        catch (Exception)
                        {
                            throw new SyntaxException("Пропущено название переменной", lex);
                        }
                        if (lexVar.LexemType == LexemType.Variable && (Stack.Count == 0 || Storage.VariableExists(Stack.Peek())))
                        {
                            Storage.AddVariable(lexVar, 0);
                            Stack.Push(lexVar);
                        }
                        else if (lexVar.LexemType == LexemType.Number || lexVar.LexemType == LexemType.Variable)
                        {
                            Lexem lexVarConcrete;
                            try
                            {
                                lexVarConcrete = Stack.Pop(); // имя массива
                            }
                            catch (Exception)
                            {
                                throw new SyntaxException("Пропущено имя массива", lex);
                            }
                            Storage.AddArrayVariable(lexVarConcrete, lexVar);
                            Stack.Push(lexVar);
                            if (Output[i+1].LexemType == LexemType.Operator && Output[i+1].Content.ToString() == "]")
                            {
                                Output.RemoveAt(i + 1);
                            }
                            else
                            {
                                throw new SyntaxException("Пропущена закрывающаяся скобка: ]", lexVarConcrete);
                            }
                        }

                        continue;
                    }

                    Lexem right, left;
                    try
                    {
                        right = Stack.Pop();
                        left = Stack.Pop();
                    }
                    catch (Exception)
                    {
                        throw new SyntaxException("Ошибка выполнения операции " + lex.Content, lex);
                    }

                    Stack.Push(new Lexem(ProcessOperator(lex, left, right, ref _curLexType), _curLexType, lex.Position));
                    
                }

                if (lex.LexemType == LexemType.EndOfExpr)
                {
                    while (_increaseStack.Count > 0)
                    {
                        var tuple = _increaseStack.Pop();
                        if (tuple.Item2.Content.Equals("++"))
                        {
                            Storage.SetVariableValue(tuple.Item1, tuple.Item1.ToDouble() + 1);
                        }
                        else
                        {
                            Storage.SetVariableValue(tuple.Item1, tuple.Item1.ToDouble() - 1);
                        }
                    }
                    Stack.Clear();
                    return;
                }

                if (lex.LexemType == LexemType.Function)
                {
                    ProcessFunction(lex);
                }
            }
        }

        private object ProcessOperator(Lexem oper, Lexem leftValue, Lexem rightValue, ref LexemType lt)
        {
            if (oper.Content.ToString() == "]")
            {
                lt = LexemType.Variable;
                return Storage.GetVariable(leftValue.Content.ToString(), rightValue);
            }
            return Helper.Calculate(oper, leftValue, rightValue, ref lt);
        }

        private void ProcessFunction(Lexem lex)
        {
            var args = new List<Lexem>();
            switch (lex.Content.ToString())
            {
                case "print":
                    while (Stack.Count > 0)
                    {
                        var arg = Stack.Pop();
                        args.Insert(0, arg);
                    }
                    Storage.AddToOutput(args, false);
                    break;

                case "println":
                    while (Stack.Count > 0)
                    {
                        var arg = Stack.Pop();
                        args.Insert(0, arg);
                    }
                    Storage.AddToOutput(args, true);
                    break;
                    
                case "scan":
                case "read":
                    while (Stack.Count > 0)
                    {
                        var arg = Stack.Pop();
                        args.Insert(0, arg);
                    }
                    foreach (var lexem in args.Where(x => x.LexemType == LexemType.Variable))
                    {
                        var window = new Editor.InputWindow(lexem);
                        window.ShowDialog();
                    }
                    break;

                case "rand":
                    var rand = _rand.Next(Convert.ToInt32(Stack.Pop().ToDouble()));
                    Stack.Push(new Lexem(rand, LexemType.Number, lex.Position));
                    break;
            }

            // Stack.Push(result) - если функция возвращает значение
        }

        private static int Priority(string oper)
        {
            oper = oper.Trim();

            if (oper.Equals("++") || oper.Equals("--")) return 8;
            if (oper.Equals("var")) return 7;
            if (oper.Equals("]")) return 6;
            if (oper.Equals("*") || oper.Equals("/")) return 5;
            if (oper.Equals("+") || oper.Equals("-")) return 4;

            if (Helper.BoolOperators.Contains(oper))
            {
                if (oper.Equals("||") || oper.Equals("&&")) return 2;
                return 3;
            }
            if (oper.Equals("=")) return 1;

            return 0;
        }
    }
}