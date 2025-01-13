using System;
using SpreadsheetUtilities;
namespace SS;

/// <summary>
/// This class represents a cell.
/// It contains a value which by default is an empty string
/// but can be, using the constructor, set to a double, string,
/// or formula.
/// </summary>
public class Cell
{
    private object value = "";


    /// <summary>
    /// Two parameter constructor that sets the name and value for the cell.
    /// </summary>
    /// <param name="value"></param>
	public Cell(object value)
	{
        this.value = value;
	}

    /// <summary>
    /// This method returns the value that this cell contatins
    /// </summary>
    /// <returns></returns>
    public object GetContents()
    {
        return value;
    }
}

