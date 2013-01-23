using System;
using System.Collections.Generic;
using Interpreter.Exceptions;
using Interpreter.Model;

namespace Interpreter.Utilities
{
    public static class Helper
    {
        public static readonly List<string> BoolOperators = new List<string>
                                                      {
                                                          ">", "<", "==", "!=", ">=", "<=", "||", "&&"
                                                      };

        private static TypeCode PickCorrectType(Lexem leftValue, Lexem rightValue)
        {
            LexemType lType = leftValue.LexemType, rType = rightValue.LexemType;

            if (lType == LexemType.Number && rType == LexemType.Number)
            {
                return TypeCode.Double;
            }

            if (lType == LexemType.String && rType == LexemType.String)
            {
                return TypeCode.String;
            }

            if (lType == LexemType.Variable && rType == LexemType.Number ||
                lType == LexemType.String && rType == LexemType.Number)
            {
                try
                {
                    leftValue.ToDouble();
                    return TypeCode.Double;
                }
                catch (Exception)
                {
                    return TypeCode.String;
                }
            }

            if (lType == LexemType.Number && rType == LexemType.Variable ||
                lType == LexemType.Number && rType == LexemType.String)
            {
                try
                {
                    rightValue.ToDouble();
                    return TypeCode.Double;
                }
                catch (Exception)
                {
                    return TypeCode.String;
                }
            }



            if (lType == LexemType.Variable && rType == LexemType.String ||
                lType == LexemType.String && rType == LexemType.Variable)
            {
                return TypeCode.String;
            }

            if (lType == LexemType.Variable && rType == LexemType.Boolean ||
                lType == LexemType.Boolean && rType == LexemType.Variable)
            {
                return TypeCode.Boolean;
            }

            if (lType == LexemType.Boolean && rType == LexemType.Boolean)
            {
                return TypeCode.Boolean;
            }

            if (lType == LexemType.Variable && rType == LexemType.Variable)
            {
                try
                {
                    leftValue.ToDouble();
                    return TypeCode.Double;
                }
                catch (Exception)
                {
                    try
                    {
                        leftValue.ToBoolean();
                        return TypeCode.Boolean;
                    }
                    catch (Exception)
                    {
                        return TypeCode.String;
                    }
                }
            }

            throw new SyntaxException("Не удалось определить общий тип", leftValue);
        }

        public static object Calculate(Lexem oper, Lexem leftValue, Lexem rightValue, ref LexemType lt)
        {
            var t = PickCorrectType(leftValue, rightValue);

            switch (t)
            {
                case TypeCode.Double:
                    lt = LexemType.Number;
                    switch (oper.Content.ToString())
                    {
                        case "*":
                            return leftValue.ToDouble() * rightValue.ToDouble();

                        case "/":
                            return leftValue.ToDouble() / rightValue.ToDouble();

                        case "+":
                            return leftValue.ToDouble() + rightValue.ToDouble();

                        case "-":
                            return leftValue.ToDouble() - rightValue.ToDouble();

                        case ">":
                            lt = LexemType.Boolean;
                            return leftValue.ToDouble() > rightValue.ToDouble();

                        case "<":
                            lt = LexemType.Boolean;
                            return leftValue.ToDouble() < rightValue.ToDouble();

                        case "!=":
                            lt = LexemType.Boolean;
                            return leftValue.ToDouble() != rightValue.ToDouble();

                        case "==":
                            lt = LexemType.Boolean;
                            return leftValue.ToDouble() == rightValue.ToDouble();

                        case ">=":
                            lt = LexemType.Boolean;
                            return leftValue.ToDouble() >= rightValue.ToDouble();

                        case "<=":
                            lt = LexemType.Boolean;
                            return leftValue.ToDouble() <= rightValue.ToDouble();

                        case "=":
                            Storage.SetVariableValue(leftValue, rightValue.ToDouble());
                            if (leftValue.Content is Variable) return (leftValue.Content as Variable).Value;
                            return Storage.GetVariable(leftValue.Content.ToString());
                    }
                    break;

                case TypeCode.String:
                    lt = LexemType.String;
                    switch (oper.Content.ToString())
                    {
                        case "+":
                            return leftValue.ToString() + rightValue.ToString();
                        
                        case "!=":
                            lt = LexemType.Boolean;
                            return leftValue.ToString() != rightValue.ToString();

                        case "==":
                            lt = LexemType.Boolean;
                            return leftValue.ToString() == rightValue.ToString();

                        case "=":
                            Storage.SetVariableValue(leftValue, rightValue.ToString());
                            if (leftValue.Content is Variable) return (leftValue.Content as Variable).Value;
                            return Storage.GetVariable(leftValue.Content.ToString());

                        default:
                            throw new SyntaxException("Неверная операция", oper);
                    }

                case TypeCode.Boolean:
                    lt = LexemType.Boolean;
                    switch (oper.Content.ToString())
                    {
                        case "||":
                            return leftValue.ToBoolean() || rightValue.ToBoolean();

                        case "&&":
                            return leftValue.ToBoolean() && rightValue.ToBoolean();

                        case "==":
                            return leftValue.ToBoolean() == rightValue.ToBoolean();

                        case "!=":
                            return leftValue.ToBoolean() != rightValue.ToBoolean();

                        case "=":
                            Storage.SetVariableValue(leftValue, rightValue.ToBoolean());
                            if (leftValue.Content is Variable) return (leftValue.Content as Variable).Value;
                            return Storage.GetVariable(leftValue.Content.ToString());

                        default:
                            throw new SyntaxException("Неверная операция", oper);
                    }
            }

            throw new SyntaxException("Ошибка вычисления оператора", oper);
        }
    }
}
