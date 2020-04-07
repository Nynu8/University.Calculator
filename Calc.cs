using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Calculator
{
    class Calc
    {
        private decimal Result = 0;
        public string GetResult { 
            get 
            {
                string stringValue = Result.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                var decimalPlaces = 0;
                if (stringValue.Contains("."))
                {
                    decimalPlaces = stringValue.Length - stringValue.IndexOf(".") - 1;
                }
                var format = new NumberFormatInfo { NumberGroupSeparator = " " };
                string output;
                if (Result.ToString().Length > 13)
                {
                    output = Result.ToString("E", format);
                }
                else
                {
                    output = Result.ToString($"F{decimalPlaces}", format);
                }
                return output;
            } 
            private set { } }

        public static readonly List<char> Operators = new List<char>()
        {
            '+', '-', '*', '/', '^', '(', ')'
        };

        private readonly List<string> AdjecentOperators = new List<string>()
        {
            "*-", "(-", "-(", "+(", "*(", "/(", "((", ")+", ")-", ")*", ")/", "^-", "^(", ")^"
        };

        public bool CanInsertOperator(string op1, string op2)
        {
            if(int.TryParse(op1, out int _))
            {
                if(op2 == "(")
                {
                    return false;
                }
            }

            string combination = op1 + op2;
            if (AdjecentOperators.Contains(combination))
            {
                return true;
            }

            return false;
        }

        public void Reset()
        {
            Result = 0;
        }

        public void EvaluateExpression(string expression)
        {
            Stack<decimal> values = new Stack<decimal>();
            Stack<string> operators = new Stack<string>();
            if (expression.StartsWith("-"))
            {
                //algorithm can only evaluate "a operator b" expressions so we're adding 0 to have 0-stuff
                expression = "0" + expression;
            }

            int i = 0;
            while(i < expression.Length)
            {
                string nextCharacter = GetNextExpressionCharacter(expression, i);
                if(nextCharacter.Length >= decimal.MaxValue.ToString().Length)
                {
                    throw new ArithmeticException();
                }

                if(decimal.TryParse(nextCharacter, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"), out var num))
                {
                    values.Push(num);
                }
                else
                {
                    if(nextCharacter == "(")
                    {
                        operators.Push(nextCharacter);
                    }
                    else if(nextCharacter == ")")
                    {
                        while (operators.Count > 0 && operators.Peek() != "(")
                        {
                            var op = operators.Pop();
                            var val2 = values.Pop();
                            var val1 = values.Pop();
                            values.Push(ApplyOperator(val1, val2, op));
                        }

                        operators.Pop();
                    }
                    else if (Operators.Contains(nextCharacter[0]))
                    {
                        while(operators.Count > 0 && Precedence(operators.Peek()) >= Precedence(nextCharacter))
                        {
                            var op = operators.Pop();
                            var val2 = values.Pop();
                            var val1 = values.Pop();
                            values.Push(ApplyOperator(val1, val2, op));
                        }

                        operators.Push(nextCharacter);
                    }
                }

                i += nextCharacter.Length;
            }

            if(operators.Count + 1 != values.Count)
            {
                //expression isn't complete, we don't want to do anything
                throw new Exception();
            }

            while (operators.Count > 0)
            {
                var op = operators.Pop();
                var val2 = values.Pop();
                var val1 = values.Pop();
                values.Push(ApplyOperator(val1, val2, op));
            }

            Result = values.Pop();
        }

        private decimal ApplyOperator(decimal val1, decimal val2, string op)
        {
            switch (op)
            {
                case "+": return val1 + val2;
                case "-": return val1 - val2;
                case "*": return val1 * val2;
                case "/": return val1 / val2;
                case "^": return (decimal)Math.Pow((double)val1, (double)val2);
                default: throw new Exception("Invalid operator");
            }
        }

        private string GetNextExpressionCharacter(string expression, int startIndex)
        {
            var currentCharacter = expression[startIndex];
            if (Operators.Contains(currentCharacter) && (currentCharacter != '-' || (currentCharacter == '-' && (!Operators.Contains(expression[startIndex-1]) || expression[startIndex-1] == ')'))))
            {
                return currentCharacter.ToString();
            }
            else
            {
                string result = expression[startIndex++].ToString();
                while (startIndex < expression.Length && (!Operators.Contains(expression[startIndex]) || Operators.Contains(expression[startIndex]) && expression[startIndex-1] == 'E')) 
                {
                    result += expression[startIndex];
                    startIndex++;
                }

                return result;
            }
        }

        private int Precedence(string op)
        {
            if (op == "+" || op == "-")
                return 1;
            if (op == "*" || op == "/")
                return 2;
            if (op == "^")
                return 3;
            return 0;
        }
    }
}
