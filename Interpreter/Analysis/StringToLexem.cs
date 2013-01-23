using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Interpreter.Exceptions;
using Interpreter.Model;
using Interpreter.Utilities;

namespace Interpreter.Analysis
{
    public class StringToLexem
    {
        private readonly List<string> _functions = new List<string>
                                              {
                                                  "print", "read", "scan", "rand", "println"
                                              };
        private readonly List<string> _lexemStrings = new List<string>
                {
                    "+", "-", "*", "/", "=", ")", "(", ";", "\"", "!", "{", "}", "|", "&", "]", ",", "["
                };
        private readonly List<string> _operators = new List<string>
                                                      {
                                                          "+", "-", "*", "/", "=", "]", "++", "--"
                                                      };
        private readonly List<string> _specWords = new List<string>
                                                       {
                                                           "while",
                                                           "if",
                                                           "else"
                                                       };

        public int currentPosition = 0;

        private List<Lexem> _result; 
        private readonly string _input = String.Empty;
        private string current = String.Empty;
        private Lexem _prevLexem = new Lexem(""), _curLexem;

        public StringToLexem(string input)
        {
            foreach (var boolOperator in Helper.BoolOperators)
            {
                _operators.Add(boolOperator);
                _lexemStrings.Add(boolOperator);
            }
            this._input = input;
        }

        public List<Lexem> ToLexems()
        {
            _result = new List<Lexem>();
            int index = 0;
            LexemType customLexem;

            while (currentPosition < _input.Length)
            {
                customLexem = LexemType.None;
                current = String.Empty;
                index = currentPosition;
                if (index >= _input.Length) break;

                if (Char.IsDigit(_input[index])) // числа 12345
                {
                    while (Char.IsLetterOrDigit(_input[index]) || _input[index] == '.')
                    {
                        if (Char.IsLetter(_input[index]))
                        {
                            var errLexem = new Lexem(current + _input[index], LexemType.None, currentPosition);
                            throw new SyntaxException("Введено некорректное число", errLexem);
                        }
                        current += _input[index++];
                    }
                    if (!_lexemStrings.Contains(_input[index].ToString()) && _input[index] != '\r' &&
                        _input[index] != '\n' && _input[index] != ' ')
                    {
                        var errLex = new Lexem(current + _input[index], LexemType.None, currentPosition);
                        throw new SyntaxException("Введено некорректное число", errLex);
                    }
                    current = current.Replace('.', ',');
                    if (current.Count(x => x == ',') > 1)
                    {
                        var errLexem = new Lexem(current, LexemType.None, currentPosition);
                        throw new SyntaxException("Введено некорректное число", errLexem);
                    }
                }
                else if (Char.IsLetter(_input[index]) || _input[index] == '_') // служебные слова, VAR, print
                {
                    while (Char.IsLetterOrDigit(_input[index]) || _input[index] == '_')
                    {
                        current += _input[index++];
                    }
                    if (!_lexemStrings.Contains(_input[index].ToString()) && _input[index] != '\r' &&
                        _input[index] != '\n' && _input[index] != ' ')
                    {
                        var errLex = new Lexem(_input[index].ToString(), LexemType.None, index);
                        throw new SyntaxException("Неверная лексема", errLex);
                    }
                }
                else if (_lexemStrings.Contains(_input[index].ToString())) // операторы
                {
                    current += _input[index++];
                    
                    if (current == "\"")
                    {
                        current = string.Empty;
                        while (_input[index] != '\"')
                        {
                            current += _input[index++];
                        }
                        index++;
                        customLexem = LexemType.String;
                    }
                    else if (current == "/" && _input[index] == '/') // комментарий
                    {
                        while (_input[index] != '\n')
                        {
                            index++;
                        }
                        currentPosition = index;
                        continue;
                    }
                    else if (((current == "=" || current == ">" || current == "<" || current == "!") && _input[index] == '=') ||
                        (current == "|" && _input[index] == '|') || (current == "&" && _input[index] == '&') || 
                        (current == "+" && _input[index] == '+') || (current == "-" && _input[index] == '-'))
                    {
                        current += _input[index++];
                    }

                    var next = _input[index];
                    if (current == "!" || current == "|" || current == "&" || !_lexemStrings.Contains(next.ToString()) && 
                        next != '\r' && next != '\n' && !Char.IsLetterOrDigit(next) && next != ' ' &&
                        (current != "=" || current != ">" || current != "<" || current != "+" || current != "-"))
                    {
                        var errLex = new Lexem(current + next, LexemType.None, currentPosition);
                        throw new SyntaxException("Введён некорректный оператор", errLex);
                    }
                }
                else // ненужные символы
                {
                    currentPosition = ++index;
                    continue;
                }


                _curLexem = customLexem != LexemType.None
                                     ? new Lexem(current, customLexem, index - current.Length)
                                     : new Lexem(current, TypeOfToken(current), index - current.Length);

                // Fix "," = "var"
                if (_curLexem.Content.Equals(",") && _curLexem.LexemType == LexemType.Operator)
                {
                    _curLexem.Content = "var";
                }

                _result.Add(_curLexem);
                _prevLexem = _curLexem;
                currentPosition = index;
                if (_prevLexem.LexemType == LexemType.EndOfExpr || _prevLexem.LexemType == LexemType.Brace) break;
                _curLexem = null;
            }

            // HACK: -(2+3)
            for (int i = 0; i < _result.Count; i++)
            {
                if (_result[i].Content.Equals("-"))
                {
                    if (i == 0 || (_result[i + 1].LexemType == LexemType.LeftBracket))
                    {
                        _result.Remove(_result[i]);
                        _result.Insert(i, new Lexem("-1", LexemType.Number, i-2));
                        _result.Insert(i, new Lexem("*", LexemType.Operator, i));
                    }
                }
            }

            return _result;
        }

        private LexemType TypeOfToken(string lexem)
        {
            double tmpNum;

            if (double.TryParse(lexem, out tmpNum))
                return LexemType.Number;

            if (_operators.Contains(lexem))
                return LexemType.Operator;

            if (lexem.Equals(",") && _result.Any(x => x.Content.ToString() == "var" && x.LexemType == LexemType.Operator))
            {
                lexem = "var";
            }

            if (lexem.Equals("(")) return LexemType.LeftBracket;
            if (lexem.Equals("[")) return LexemType.LeftBrace;
            if (lexem.Equals(")")) return LexemType.RightBracket;
            if (lexem.Equals(";")) return LexemType.EndOfExpr;
            if (lexem.Equals("{") || lexem.Equals("}")) return LexemType.Brace;
            if (lexem.Equals("const")) return LexemType.ModificatorCandidate;
            if (lexem.Equals("var"))
            {
                if (_prevLexem.LexemType == LexemType.ModificatorCandidate)
                {
                    _result.Last().LexemType = LexemType.Modificator;
                }
                return LexemType.Operator;
            }
            if (lexem.Equals("true") || lexem.Equals("false")) return LexemType.Boolean;
            if (_specWords.Contains(lexem)) return LexemType.SpecialWord;
            if (_functions.Contains(lexem)) return LexemType.Function;
            if (_prevLexem.Content.ToString() == "var" || Storage.VariableExists(lexem)) return LexemType.Variable;

            if (_curLexem == null) _curLexem = new Lexem(current, LexemType.None, currentPosition);
            throw new SyntaxException("Незнакомая лексема", _curLexem);
        }
    }
}