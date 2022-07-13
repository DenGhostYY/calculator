using System;
using System.Collections.Generic;

namespace SimpleCalculator
{
    public static class Calculation
    {
        // 1-низкий приоритет, 3-высокий
        // унарные операции еще выше
        private static Dictionary<string, int> priotities =
            new Dictionary<string, int>() {
            {"+", 1},
            {"-", 1}, // бинарный минус
            {"*", 2},
            {"/", 2},
            {"^", 3}, // степень 3^4=81
            {"n√", 3}, //корень 4√81=3
            {"(", 0},
            {")", 0},
            {"+-", 0}, // унарный минус
            {"√", 0} // квадратный корень √25=5
        };
        class Term
        {
            public enum TERM { NUMBER, OPERATION };

            public TERM type;
            public string operation;
            public double number;
            public Term()
            {
                type = TERM.NUMBER;
                operation = "";
                number = 1.0;
            }
            public Term(double _number)
            {
                type = TERM.NUMBER;
                number = _number;
            }
            public Term(string op)
            {
                type = TERM.OPERATION;
                operation = op;
            }
        }

        public enum TypesErros { NoError, Lexical, Syntax, DivideByZero, RootNegative };

        public static string Calculate(string expression, out TypesErros error)
        {
            List<Term> terms = ReadTerms(expression, out error);
            if (error != TypesErros.NoError)
            {
                return "";
            }

            List<Term> poliz = ToPoliz(terms, out error);
            if (error != TypesErros.NoError)
            {
                return "";
            }

            double answer = Calc(poliz, out error);
            if (error != TypesErros.NoError)
            {
                return "";
            }
            return answer.ToString();
        }
        private static double Calc(List<Term> poliz, out TypesErros error)
        {
            // вычисление значения по обратной польской записи
            error = TypesErros.NoError;
            Stack<double> stack = new Stack<double>();
            foreach (Term term in poliz)
            {
                if (term.type == Term.TERM.NUMBER)
                {
                    stack.Push(term.number);
                }
                else if (IsUnaryOperation(term))
                {
                    if (stack.Count == 0)
                    {
                        error = TypesErros.Syntax;
                        return Double.NaN;
                    }

                    double top = stack.Pop();
                    double resultOperation = Operate(top, term.operation, out error);
                    if (error != TypesErros.NoError)
                    {
                        return Double.NaN;
                    }
                    stack.Push(resultOperation);
                }
                else
                {
                    if (stack.Count < 2)
                    {
                        error = TypesErros.Syntax;
                        return Double.NaN;
                    }
                    double secondOperand = stack.Pop();
                    double firstOperand = stack.Pop();
                    
                    double resultOperation = Operate(firstOperand, term.operation,
                            secondOperand, out error);
                    if (error != TypesErros.NoError)
                    {
                        return Double.NaN;
                    }
                    stack.Push(resultOperation);
                }
            }
            if (stack.Count != 1)
            {
                error = TypesErros.Syntax;
                return Double.NaN;
            }
            return stack.Peek();
        }

        private static double Operate(double operand, string operation, out TypesErros error)
        {
            // унарная операция
            error = TypesErros.NoError;
            switch (operation)
            {
                case "+-":
                    return -operand;
                case "√":
                    if (operand < Double.Epsilon)
                    {
                        error = TypesErros.RootNegative;
                        return Double.NaN;
                    }
                    return Math.Sqrt(operand);
                default:
                    error = TypesErros.Syntax;
                    return Double.NaN;
            }
        }

        private static double Operate(double firstOperand, string operation, double secondOperand, out TypesErros error)
        {
            // бинарная операция
            error = TypesErros.NoError;
            switch (operation)
            {
                case "+":
                    return firstOperand + secondOperand;
                case "-":
                    return firstOperand - secondOperand;
                case "*":
                    return firstOperand * secondOperand;
                case "/":
                    if (Math.Abs(secondOperand) < Double.Epsilon)
                    {
                        error = TypesErros.DivideByZero;
                        return Double.NaN;
                    }
                    return firstOperand / secondOperand;
                case "^":
                    return Math.Pow(firstOperand, secondOperand);
                case "n√":
                    if (firstOperand < Double.Epsilon)
                    {
                        error = TypesErros.RootNegative;
                        return Double.NaN;
                    }
                    if (secondOperand < Double.Epsilon)
                    {
                        int tr = (int)Math.Truncate(firstOperand);
                        if (tr % 2 != 0 && Math.Abs(tr - firstOperand) < Double.Epsilon)
                        {
                            return -Math.Pow(-secondOperand, 1/firstOperand);
                        }
                    }
                    return Math.Pow(secondOperand, 1/firstOperand);
                default:
                    error = TypesErros.Syntax;
                    return Double.NaN;
            }
        }

        private static List<Term> ToPoliz(List<Term> terms, out TypesErros error)
        {
            // преобразование из инфиксной нотации в
            // обратную польскую
            error = TypesErros.NoError;
            List<Term> poliz = new List<Term>();
            Stack<Term> stack = new Stack<Term>();
            foreach (Term term in terms)
            {
                if (term.type == Term.TERM.NUMBER)
                {
                    poliz.Add(term);
                }
                else if (IsUnaryOperation(term) || term.operation == "(")
                {
                    stack.Push(term);
                }
                else if (term.operation == ")")
                {
                    bool isOpenParanthes = false;
                    while (stack.Count != 0)
                    {
                        if (stack.Peek().operation == "(")
                        {
                            isOpenParanthes = true;
                            stack.Pop();
                            break;
                        }
                        poliz.Add(stack.Pop());
                    }
                    if (!isOpenParanthes)
                    {
                        error = TypesErros.Syntax;
                        return null;
                    }
                }
                else
                {
                    while (stack.Count != 0 && (IsUnaryOperation(stack.Peek()) ||
                            priotities[stack.Peek().operation] > priotities[term.operation] ||
                            priotities[stack.Peek().operation] == priotities[term.operation] &&
                                IsLeftAssoc(stack.Peek())))
                    {
                        poliz.Add(stack.Pop());
                    }
                    stack.Push(term);
                }
            }
            while (stack.Count != 0) {
                if (stack.Peek().operation == "(" ||
                    stack.Peek().operation == ")")
                {
                    error = TypesErros.Syntax;
                    return null;
                }
                poliz.Add(stack.Pop());
            }
            return poliz;
        }

        private static bool IsUnaryOperation(Term t)
        {
            return t.operation == "+-" || t.operation == "√";
        }

        private static bool IsLeftAssoc(Term t)
        {
            return t.operation == "+" || t.operation == "-" ||
                t.operation == "*" || t.operation == "/";
        }

        private static List<Term> ReadTerms(string expression, out TypesErros error)
        {
            // разбор исходного выражения на лексемы-токены
            error = TypesErros.NoError;
            List<Term> terms = new List<Term>();
            bool isNumber = false;
            string word = "";
            for (int i = 0; i < expression.Length; i++)
            {
                if (Char.IsDigit(expression[i]) || expression[i] == ',')
                {
                    isNumber = true;
                    word += expression[i];
                }
                else
                {
                    if (isNumber)
                    {
                        double number;
                        if (Double.TryParse(word, out number))
                        {
                            terms.Add(new Term(number));
                            word = "";
                            isNumber = false;
                        }
                        else
                        {
                            error = TypesErros.Lexical;
                            return null;
                        }
                    }
                    int t;
                    word += expression[i];
                    if (priotities.TryGetValue(word, out t))
                    {
                        if (word == "-" && (terms.Count == 0 ||
                                terms[terms.Count - 1].type == Term.TERM.OPERATION &&
                                terms[terms.Count - 1].operation != ")"))
                        {
                            word = "+-";
                        }
                        if (word == "√" && terms.Count != 0 &&
                                (terms[terms.Count - 1].type == Term.TERM.NUMBER ||
                                terms[terms.Count - 1].operation == ")"))
                        {
                            word = "n√";
                        }
                        terms.Add(new Term(word));
                        word = "";
                    }
                    else
                    {
                        error = TypesErros.Lexical;
                        return null;
                    }
                }
            }
            if (isNumber)
            {
                double number;
                if (Double.TryParse(word, out number))
                {
                    terms.Add(new Term(number));
                }
                else
                {
                    error = TypesErros.Lexical;
                    return null;
                }
            }
            return terms;
        }
    }
}