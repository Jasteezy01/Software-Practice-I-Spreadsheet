using SpreadsheetUtilities;
namespace SS;

[TestClass]
public class UnitTest1
{
    /// <summary>
    /// This test ensures that there are 0 non-empty
    /// cells in an empty spreadsheet
    /// </summary>
    [TestMethod]
    public void GetNamesOfEmptySpreadsheet()
    {
        Spreadsheet s = new Spreadsheet();
        Assert.AreEqual(0, s.GetNamesOfAllNonemptyCells().Count());
    }

    /// <summary>
    /// This test uses the string parameterized SetCellContents
    /// method to add one cell and checks that the spreadsheet
    /// updated the nonempty cells
    /// </summary>
    [TestMethod]
    public void AddStringCell()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", "Test");
        Assert.AreEqual(1, s.GetNamesOfAllNonemptyCells().Count());
    }

    /// <summary>
    /// This test uses the doubl parameterized SetCellContents
    /// method to add one cell and checks that the spreadsheet
    /// updated the nonempty cells
    /// </summary>
    [TestMethod]
    public void AddDoubleCell()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", 2.0);
        Assert.AreEqual(1, s.GetNamesOfAllNonemptyCells().Count());
    }

    /// <summary>
    /// This test uses the formula parameterized SetCellContents
    /// method to add one cell and checks that the spreadsheet
    /// updated the nonempty cells
    /// </summary>
    [TestMethod]
    public void AddFormulaCell()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", new Formula("1+2"));
        Assert.AreEqual(1, s.GetNamesOfAllNonemptyCells().Count());
    }

    /// <summary>
    /// Adding a formula to the spreadsheet with variables
    /// </summary>
    [TestMethod]
    public void AddVaribaleFormulaCell()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", new Formula("5"));
        s.SetCellContents("C7", new Formula("A1 + 2"));
        Assert.AreEqual(2, s.GetNamesOfAllNonemptyCells().Count());
    }

    /// <summary>
    /// Adding a double cell with an invalid name
    /// </summary>
    [TestMethod]
    public void AddInvalidCellDouble()
    {
        Spreadsheet s = new Spreadsheet();
        Assert.ThrowsException<InvalidNameException>(() => s.SetCellContents("(aa", 1.0));

    }

    /// <summary>
    /// Adding a string cell with an invalid name
    /// </summary>
    [TestMethod]
    public void AddInvalidCellString()
    {
        Spreadsheet s = new Spreadsheet();
        Assert.ThrowsException<InvalidNameException>(() => s.SetCellContents("(aa", "Hello"));

    }

    /// <summary>
    /// Adding a formula cell with an invalid name
    /// </summary>
    [TestMethod]
    public void AddInvalidCellFormula()
    {
        Spreadsheet s = new Spreadsheet();
        Assert.ThrowsException<InvalidNameException>(() => s.SetCellContents("(aa", new Formula("2 + 2")));

    }

    /// <summary>
    /// Checks that the cell contains the correct formula
    /// </summary>
    [TestMethod]
    public void CheckCellContentsFormula()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", new Formula("5"));
        s.SetCellContents("C7", new Formula("A1 + 2"));
        Assert.AreEqual(new Formula("A1 +2"), s.GetCellContents("C7"));
    }

    /// <summary>
    /// Checks that the cell contains the correct doubl
    /// </summary>
    [TestMethod]
    public void CheckCellContentsDouble()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", 5);
        s.SetCellContents("C7", new Formula("A1 + 2"));
        Assert.AreEqual(5.0, s.GetCellContents("A1"));
    }

    /// <summary>
    /// Checks that the cell contains the correct string
    /// </summary>
    [TestMethod]
    public void CheckCellContentsString()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", "Hello");
        s.SetCellContents("C7", new Formula("A1 + 2"));
        Assert.AreEqual("Hello", s.GetCellContents("A1"));
    }

    /// <summary>
    /// This test checks to see if dependents are returned when setting cell contents
    /// </summary>
    [TestMethod]
    public void ConnectedCellContents()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("B1", new Formula("A1 *2"));
        s.SetCellContents("C1", new Formula("B1 + A1"));

        Assert.IsTrue(s.SetCellContents("A1", 7).Contains("B1"));
    }

    /// <summary>
    /// This test checks to see if indirect dependents are returned
    /// </summary>
    [TestMethod]
    public void IndirectCellContents()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("B1", new Formula("A1 *2"));
        s.SetCellContents("C1", new Formula("B1 + 9"));

        Assert.IsTrue(s.SetCellContents("A1", 7).Contains("C1"));
    }

    /// <summary>
    /// This test ensures an exception is thrown when using circular dependency
    /// </summary>
    [TestMethod]
    public void CircularDependency()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("A1", new Formula("B1"));
        s.SetCellContents("B1", new Formula("C1 + 9"));
        

        Assert.ThrowsException<CircularException>(() => s.SetCellContents("C1", new Formula("A1 -3")));
    }

    /// <summary>
    /// Checks the contents of an invalid cell name
    /// </summary>
    [TestMethod]
    public void InvalidCheckCellContents()
    {
        Spreadsheet s = new Spreadsheet();
        Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents("(aa"));
    }

    /// <summary>
    /// This test checks to see if an empty string is the default value of a cell
    /// </summary>
    [TestMethod]
    public void NonExistentCell()
    {
        Spreadsheet s = new Spreadsheet();
        Assert.AreEqual("", s.GetCellContents("A1"));
    }


    /// <summary>
    /// This test updates the value of a boolean cell
    /// </summary>
    [TestMethod]
    public void UpdateCellValueDouble()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("a1", 9.0);
        s.SetCellContents("a1", 3.5);

        Assert.AreEqual(3.5, s.GetCellContents("a1"));

    }

    /// <summary>
    /// This test updates the value of a string cell
    /// </summary>
    [TestMethod]
    public void UpdateCellValueString()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("a1", "Hello");
        s.SetCellContents("a1", "World");

        Assert.AreEqual("World", s.GetCellContents("a1"));

    }

    /// <summary>
    /// This test updates the value of a formula cell
    /// </summary>
    [TestMethod]
    public void UpdateCellValueFormula()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("b19", new Formula("9 + 2"));
        s.SetCellContents("b19", new Formula("7*12"));

        Assert.AreEqual(new Formula("7*12"), s.GetCellContents("b19"));

    }

    /// <summary>
    /// This test ensure that indirect dependents are returned after updating
    /// the value of a cell
    /// </summary>
    [TestMethod]
    public void UpdateCellValueFormulaConnected()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("a1", new Formula("17"));
        s.SetCellContents("b1", new Formula("a1 * 2"));
        s.SetCellContents("a7", new Formula("a1 + a1 - b1"));
        s.SetCellContents("c4", new Formula("a7 + 2"));
        s.SetCellContents("gg", new Formula("c4 * c4"));



        Assert.IsTrue(s.SetCellContents("a1", new Formula("23")).Contains("gg"));

    }

    /// <summary>
    /// This test ensures that indirect dependents aren't returned
    /// if values are changed to no longer connect them
    /// </summary>
    [TestMethod]
    public void UpdateCellDependents()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("a1", new Formula("17"));
        s.SetCellContents("b1", new Formula("a1 * 2"));
        s.SetCellContents("a7", new Formula("a1 + a1 - b1"));
        s.SetCellContents("c4", new Formula("a7 + 2"));
        s.SetCellContents("gg", new Formula("c4 * c4"));
        s.SetCellContents("c4", 7.0);



        Assert.IsFalse(s.SetCellContents("a1", new Formula("23")).Contains("gg"));

    }

    /// <summary>
    /// This test ensures that indirect dependents aren't returned
    /// if values are changed to no longer connect them
    /// </summary>
    [TestMethod]
    public void UpdateCellDependentsString()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("a1", new Formula("17"));
        s.SetCellContents("b1", new Formula("a1 * 2"));
        s.SetCellContents("a7", new Formula("a1 + a1 - b1"));
        s.SetCellContents("c4", new Formula("a7 + 2"));
        s.SetCellContents("gg", new Formula("c4 * c4"));
        s.SetCellContents("c4", "Hello");



        Assert.IsFalse(s.SetCellContents("a1", new Formula("23")).Contains("gg"));

    }

    /// <summary>
    /// This test ensures that indirect dependents aren't returned
    /// if values are changed to no longer connect them
    /// </summary>
    [TestMethod]
    public void UpdateCellDependentsFormula()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetCellContents("a1", new Formula("17"));
        s.SetCellContents("b1", new Formula("a1 * 2"));
        s.SetCellContents("a7", new Formula("a1 + a1 - b1"));
        s.SetCellContents("c4", new Formula("a7 + 2"));
        s.SetCellContents("gg", new Formula("c4 * c4"));
        s.SetCellContents("c4", new Formula("19 / 7"));



        Assert.IsFalse(s.SetCellContents("a1", new Formula("23")).Contains("gg"));

    }


}
