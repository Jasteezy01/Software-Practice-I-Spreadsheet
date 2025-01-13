using System.Text.RegularExpressions;

namespace FormulaEvaluator;

/// <summary>
/// This class evaluates provied expressions using basic arithmetic and provided variables.
/// </summary>
public static class Evaluator
{
    public delegate int Lookup(String v); //Delegate for variables

    /// <summary>
    /// This method performs the execution of the expression using variables as needed.
    /// </summary>
    /// <param name="exp"></param> The expression to be evaluated
    /// <param name="variableEvaluator"></param> A delegate that will return an int when a string is passed
    /// <returns></returns> The result of the expression
    /// <exception cref="Exception"></exception>
    public static int Evaluate(String exp, Lookup variableEvaluator)
    {
        //Stacks for vals and ops
        Stack<string> values = new Stack<string>();
        Stack<string> operators = new Stack<string>();

        //Create an array for the expression
        string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

        for (int i = 0; i < substrings.Length; i++) //Loop through the expression
        {
            string t = substrings[i];
            if (t.Equals("") || t.Equals(" "))
                continue;
            if (int.TryParse(t, out _)) // If current character in expression is an integer...
            {
                if (!values.TryPeek(out _))
                {
                    values.Push(t);
                    continue;
                }
                if (operators.Peek().Equals("*") || operators.Peek().Equals("/")) //If we are multiplying or dividing...
                {
                    int firstValue = int.Parse(values.Pop()); //Get int that is being multiplied or divided
                    string currentOperator = operators.Pop(); //Get the operation we are performing

                    int result = PerformOperation(firstValue, int.Parse(t), currentOperator); //Get result of operation
                    values.Push(result.ToString());
                }
                else
                    values.Push(t);
            }
            else if (IsPlusOrMinus(t)) //t is + or -
            {
                if (!operators.TryPeek(out _))
                {
                    operators.Push(t);
                    continue;
                }
                if (IsPlusOrMinus(operators.Peek()))
                {
                    int secondValue = int.Parse(values.Pop());
                    int firstValue = int.Parse(values.Pop());
                    string op = operators.Pop();

                    int result = PerformOperation(firstValue, secondValue, op);
                    values.Push(result.ToString());
                }
                operators.Push(t);
            }
            else if (IsMultiplyOrDivide(t)) //t is * or /
            {
                operators.Push(t);
            }
            else if (IsLeftParenthesis(t)) //t is (
            {
                operators.Push(t);
            }
            else if (IsRightParenthesis(t)) //t is )
            {
                if (IsPlusOrMinus(operators.Peek()))//If + or - is at top of operator stack, evaluate
                {
                    int secondValue = int.Parse(values.Pop());
                    int firstValue = int.Parse(values.Pop());
                    string op = operators.Pop();

                    int result = PerformOperation(firstValue, secondValue, op);
                    values.Push(result.ToString());
                }
                if (operators.Count() == 0)
                    throw new ArgumentException("'(' isn't found where expected");
                if (IsLeftParenthesis(operators.Peek()))//Now the top of operators should be a left parenthesis
                {
                    operators.Pop();//Pop it
                }
                else //If not throw an exception
                {
                    throw new ArgumentException("'(' isn't found where expected");
                }
                if (operators.Count() == 0)
                    continue;
                if (IsMultiplyOrDivide(operators.Peek()))//If multiply or divide is at top of operators, evaluate
                {
                    int secondValue = int.Parse(values.Pop());
                    int firstValue = int.Parse(values.Pop());
                    string op = operators.Pop();

                    int result = PerformOperation(firstValue, secondValue, op);
                    values.Push(result.ToString());
                }

            }

            else //t is a variable
            {
                //Ensure variable is valid
                if (!IsValidVariable(t))
                    throw new ArgumentException("Not a valid variable");

                t = variableEvaluator(t).ToString();//Get the value of the variable and push onto variable stack

                if (!operators.TryPeek(out _))
                {
                    values.Push(t);
                    continue;
                }
                if (operators.Peek().Equals("*") || operators.Peek().Equals("/")) //If we are multiplying or dividing...
                {
                    int firstValue = int.Parse(values.Pop()); //Get int that is being multiplied or divided
                    string currentOperator = operators.Pop(); //Get the operation we are performing

                    int result = PerformOperation(firstValue, int.Parse(t), currentOperator); //Get result of operation
                    values.Push(result.ToString());
                }
                else
                    values.Push(t);
            }
        }
        if(operators.TryPeek(out _) && values.Count == 2)
        {
            int secondValue = int.Parse(values.Pop());
            int firstValue = int.Parse(values.Pop());
            string op = operators.Pop();

            int result = PerformOperation(firstValue, secondValue, op);
            values.Push(result.ToString());
        }
        if (!values.TryPeek(out _))
            throw new ArgumentException("Empty expression");
        if (operators.TryPeek(out _))
            throw new ArgumentException("Too many operators");
        return int.Parse(values.Pop());//Expression finished return result
    }

    /// <summary>
    /// This method checks to see if the provided variable is "valid"
    /// That is a string consisting of one or more letters followed by one or more digits
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static bool IsValidVariable(string t)
    { 
        string pattern = @"^[A-Za-z]+[0-9]+$";

        // Use Regex.IsMatch to check if the input string matches the pattern
        return Regex.IsMatch(t, pattern);
    }

    /// <summary>
    /// This method checks to see if the provided string is a right parenthesis
    /// </summary>
    /// <param name="t"></param> The string 
    /// <returns></returns> True if was a right parenthesis
    private static bool IsRightParenthesis(string t)
    {
        return t.Equals(")");
    }

    /// <summary>
    /// This method checks to see if the provided string is a left parenthesis
    /// </summary>
    /// <param name="t"></param> The string 
    /// <returns></returns> True if was a left parenthesis
    private static bool IsLeftParenthesis(string t)
    {
        return t.Equals("(");
    }

    /// <summary>
    /// This method checks to see if the provided string is a multiplication or division symbol (* or /)
    /// </summary>
    /// <param name="t"></param> The string 
    /// <returns></returns> True if was a multiplication or division symbol
    private static bool IsMultiplyOrDivide(string t)
    {
        return t.Equals("*") || t.Equals("/");
    }

    /// <summary>
    /// This method checks to see if the provided string is an addition or subtraction symbol (+ or -)
    /// </summary>
    /// <param name="t"></param> The string 
    /// <returns></returns> True if was an addition or subtraction symbol
    private static bool IsPlusOrMinus(string t)
    {
        return t.Equals("+") || t.Equals("-");
    }

    /// <summary>
    /// This method does the basic arithmetic from two provided integers and an operator
    /// </summary>
    /// <param name="firstValue"></param> The first integer
    /// <param name="v"></param> The second integer
    /// <param name="currentOperator"></param> The operator for this operation
    /// <returns></returns>
    private static int PerformOperation(int firstValue, int v, string currentOperator)
    {
        switch (currentOperator)
        {
            case "*":
                return firstValue * v;
            case "/":
                if (v == 0)
                    throw new ArgumentException("Cannot divide by zero");
                return firstValue / v;
            case "+":
                return firstValue + v;
            default:
                return firstValue - v;
        }
    }
}

