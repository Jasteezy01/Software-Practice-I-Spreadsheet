// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!
// Last updated: August 2023 (small tweak to API)

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    private string[] substrings;
    private Func<string, string> normalize;
    private List<string> variables;
    

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true)
    {
    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        this.normalize = normalize;
        variables = new List<string>();

        //Ensure that formula is valid
        //Create an array for the expression
        string[] substrings = Regex.Split(formula, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
        foreach (string s in substrings)
        {
            if (s.Equals("") || s.Equals(" "))
                continue;
            //Skip all operators and digits
            if(IsPlusOrMinus(s) || IsMultiplyOrDivide(s) || double.TryParse(s, out _) || IsRightParenthesis(s) || IsLeftParenthesis(s))
            {
                continue;
            }
            //Normalize the variable
            string variable = normalize(s);
            variable = variable.Trim();
            //Ensure variable has valid syntax
            if (!IsValidVariable(variable))
                throw new ArgumentException("Invalid variable: " + variable);

            //Check to see if variable is valid according to extra restrictions
            if (!isValid(variable))
                throw new FormulaFormatException("Invalid Variable: " + variable);
            variables.Add(variable);
        }

        //Formula is valid, field can be set
        this.substrings = substrings;
    }

    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        //Stacks for vals and ops
        Stack<string> values = new Stack<string>();
        Stack<string> operators = new Stack<string>();

        for (int i = 0; i < substrings.Length; i++) //Loop through the expression
        {
            string t = substrings[i];
            if (t.Equals("") || t.Equals(" "))
                continue;
            if (double.TryParse(t, out _)) // If current character in expression is an integer...
            {
                if (!values.TryPeek(out _))// If values stack is empty
                {
                    values.Push(t);//Add value to top of stack
                    continue;
                }
                if (operators.Peek().Equals("*") || operators.Peek().Equals("/")) //If we are multiplying or dividing...
                {
                    double firstValue = double.Parse(values.Pop()); //Get int that is being multiplied or divided
                    string currentOperator = operators.Pop(); //Get the operation we are performing

                    if (CheckDivideByZero(currentOperator, int.Parse(t)))
                        return new FormulaError("Division by zero occured");

                    double result = PerformOperation(firstValue, double.Parse(t), currentOperator); //Get result of operation
                    values.Push(result.ToString()); //Add result to top of stack
                }
                else
                    values.Push(t); //Add value to top of stack
            }
            else if (IsPlusOrMinus(t)) //t is + or -
            {
                if (!operators.TryPeek(out _)) //If operator stack is empty 
                {
                    operators.Push(t); //Push operator to top of stack
                    continue;
                }
                if (IsPlusOrMinus(operators.Peek())) //If next operator is a plus or minus
                {
                    //Evaluate the previous two values with their appropriate operator
                    double secondValue = double.Parse(values.Pop());
                    double firstValue = double.Parse(values.Pop());
                    string op = operators.Pop();
                    if (CheckDivideByZero(op, secondValue))
                        return new FormulaError("Division by zero occured");
                    double result = PerformOperation(firstValue, secondValue, op);
                    values.Push(result.ToString()); //Push result to top of stack
                }
                operators.Push(t);//Add original operator to top of stack
            }
            else if (IsMultiplyOrDivide(t)) //t is * or /
            {
                operators.Push(t);//Push it to top of stack
            }
            else if (IsLeftParenthesis(t)) //t is (
            {
                operators.Push(t);//Push it to top of stack
            }
            else if (IsRightParenthesis(t)) //t is )
            {
                if (IsPlusOrMinus(operators.Peek()))//If + or - is at top of operator stack, evaluate
                {
                    double secondValue = double.Parse(values.Pop());
                    double firstValue = double.Parse(values.Pop());
                    string op = operators.Pop();
                    if (CheckDivideByZero(op, secondValue))
                        return new FormulaError("Division by zero occured");
                    double result = PerformOperation(firstValue, secondValue, op);
                    values.Push(result.ToString());
                }
                if (operators.Count() == 0)
                    return new FormulaError("'(' isn't found where expected");
                if (IsLeftParenthesis(operators.Peek()))//Now the top of operators should be a left parenthesis
                {
                    operators.Pop();//Pop it
                }
                else //If not throw an exception
                {
                    return new FormulaError("'(' isn't found where expected");
                }
                if (operators.Count() == 0)
                    continue;
                if (IsMultiplyOrDivide(operators.Peek()))//If multiply or divide is at top of operators, evaluate
                {
                    double secondValue = double.Parse(values.Pop());
                    double firstValue = double.Parse(values.Pop());
                    string op = operators.Pop();
                    if (CheckDivideByZero(op, secondValue))
                        return new FormulaError("Division by zero occured");
                    double result = PerformOperation(firstValue, secondValue, op);
                    values.Push(result.ToString());
                }

            }

            else //t is a variable
            {
                
                t = lookup(normalize(t)).ToString();//Get the value of the variable and push onto variable stack

                if (!operators.TryPeek(out _))
                {
                    values.Push(t);
                    continue;
                }
                if (operators.Peek().Equals("*") || operators.Peek().Equals("/")) //If we are multiplying or dividing...
                {
                    double firstValue = double.Parse(values.Pop()); //Get int that is being multiplied or divided
                    string currentOperator = operators.Pop(); //Get the operation we are performing

                    if (CheckDivideByZero(currentOperator, int.Parse(t)))
                        return new FormulaError("Division by zero occured");
                    double result = PerformOperation(firstValue, int.Parse(t), currentOperator); //Get result of operation
                    values.Push(result.ToString());
                }
                else
                    values.Push(t);
            }
        }
        if (operators.TryPeek(out _) && values.Count == 2)
        {
            double secondValue = double.Parse(values.Pop());
            double firstValue = double.Parse(values.Pop());
            string op = operators.Pop();
            if (CheckDivideByZero(op, secondValue))
                return new FormulaError("Division by zero occured");
            double result = PerformOperation(firstValue, secondValue, op);
            values.Push(result.ToString());
        }
        if (!values.TryPeek(out _))
            return new FormulaError("Empty expression");
        if (operators.TryPeek(out _))
            return new FormulaError("Too many operators");
        return double.Parse(values.Pop());//Expression finished return result
    }

    private bool CheckDivideByZero(string op, double secondValue)
    {
        if (op.Equals("/") && secondValue == 0)
            return true;
        return false;
    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables()
    {
        return variables;
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        string ret = "";

        for (int i = 0; i < substrings.Length; i++)
        {
            string t = substrings[i];
            if (t.Equals("") || t.Equals(" "))
                continue;
            if (IsPlusOrMinus(t) || IsMultiplyOrDivide(t) || double.TryParse(t, out _) || IsRightParenthesis(t) || IsLeftParenthesis(t))
            {
                t = t.Trim();
                ret = ret + t;
            }
            else
            {
                t = t.Trim();
                ret = ret + normalize(t);
            }
        }
        return ret;
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokens are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is Formula))
            return false;

        Formula otherFormula = (Formula)obj;

        string[] param = GetTokens(otherFormula.ToString()).ToArray();
        if (param.Length != substrings.Length)
            return false;
        for (int i = 0; i < substrings.Length; i++)
        {
            //If token at 'i' is an operator
            if (IsPlusOrMinus(substrings[i]) || IsMultiplyOrDivide(substrings[i]) || IsRightParenthesis(substrings[i]) || IsLeftParenthesis(substrings[i]))
            {
                if (!substrings[i].Equals(param[i])) //Compare the strings
                    return false;
            }
            else if (double.TryParse(substrings[i], out _)) //If token at 'i' is a double
            {
                double a = double.Parse(substrings[i]);
                double b = double.Parse(param[i]);
                if (a != b)
                    return false;
            }
            else
            {
                string a = normalize(substrings[i]);
                a = a.Trim();
                string b = normalize(param[i]);
                b = b.Trim();
                if (!a.Equals(b))
                    return false;
            }

        }
        return true;
    }

    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.Equals(f2);
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !(f1 == f2);
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        int j= 0;
        for (int i = 0; i < substrings.Length; i++)
            j = j + substrings[i].GetHashCode();
        
        return j;
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula)
    {
        // Patterns for individual tokens
        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }

    }

    /// <summary>
    /// This method checks to see if the provided variable is "valid"
    /// That is a string consisting of one or more letters or underscores followed by one or more digits
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static bool IsValidVariable(string t)
    {
        string pattern = @"^[A-Za-z_]+[0-9]+$";

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
    private static double PerformOperation(double firstValue, double v, string currentOperator)
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

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

