using System;
using System.Text.RegularExpressions;
using SpreadsheetUtilities;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SS
{
	public class Spreadsheet : AbstractSpreadsheet
	{
        private Dictionary<string, Cell> cells;
        private DependencyGraph dg = new DependencyGraph();

        /// <summary>
        /// No parameter constructor
        /// Creates an empty spreadsheet
        /// </summary>
		public Spreadsheet()
		{
            cells = new Dictionary<string, Cell>();
		}

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.
        /// The return value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            if (NameNotValid(name))
                throw new InvalidNameException();

            if (cells.TryGetValue(name, out _))
            {

                object o = cells[name].GetContents();
                return o ;
            }
            else
                return "";
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            List<string> names = new List<string>();
            foreach(var name in cells)
            {
                names.Add(name.Key);
            }
            
            return names;
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetCellContents(string name, double number)
        {
            if (NameNotValid(name))
                throw new InvalidNameException();
            if (cells.ContainsKey(name))
            {
                object toRemove = cells[name].GetContents();//If cell contained a formula before
                if (toRemove is Formula)
                {
                    IEnumerable<string> oldDependents = ((Formula)toRemove).GetVariables();
                    if (oldDependents.Count() > 0)
                        foreach (string s in oldDependents)
                            dg.RemoveDependency(s, name);
                }
                cells[name] = new Cell(number);
            }
            else
            {
                Cell cell = new Cell(number);
                cells.Add(name, cell);
            }
            dg.AddDependency(name, "");
            return GetCellsToRecalculate(name).ToList();
        }



        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetCellContents(string name, string text)
        {
            if (NameNotValid(name))
                throw new InvalidNameException();
            if (cells.ContainsKey(name))//Cell already existed
            {
                object toRemove = cells[name].GetContents();//If cell contained a formula before
                if (toRemove is Formula)
                {
                    IEnumerable<string> oldDependents = ((Formula)toRemove).GetVariables();
                    if (oldDependents.Count() > 0)
                        foreach (string s in oldDependents)
                            dg.RemoveDependency(s, name);
                }
                cells[name] = new Cell(text);
            }
            else
            {
                Cell cell = new Cell(text);
                cells.Add(name, cell);
            }
            dg.AddDependency(name, "");
            return GetCellsToRecalculate(name).ToList();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetCellContents(string name, Formula formula)
        {
            if (NameNotValid(name))
                throw new InvalidNameException();

            if (cells.ContainsKey(name))//If cell already existed
            {
                object toRemove = cells[name].GetContents();//Get dependencies that need to be removed
                if (toRemove is Formula)
                {
                    IEnumerable<string> oldDependents = ((Formula)toRemove).GetVariables();
                    if (oldDependents.Count() > 0)
                        foreach (string s in oldDependents)
                            dg.RemoveDependency(s, name);
                }
                cells[name] = new Cell(formula);
            }
            else //Cell doesn't exist
            {
                Cell cell = new Cell(formula);
                cells.Add(name, cell);
            }
            IEnumerable<string> dependents = formula.GetVariables();
            if (dependents.Count() > 0)
                foreach (string s in dependents)
                    dg.AddDependency(s, name);
            else
                dg.AddDependency(name, "");
            return GetCellsToRecalculate(name).ToList();
        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            //TODO: Is this method call correct
            return dg.GetDependents(name);
        }


        /// <summary>
        /// A string is a valid cell name if and only if:
        ///   (1) its first character is an underscore or a letter
        ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
        /// Note that this is the same as the definition of valid variable from the PS3 Formula class.
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool NameNotValid(string name)
        {
            string pattern = @"^[A-Za-z_][A-Za-z0-9_]*$";
            //string pattern = @"^[A-Za-z_]+[0-9]+$";

            // Use Regex.IsMatch to check if the input string matches the pattern
            return !Regex.IsMatch(name, pattern);
        }
    }
}

