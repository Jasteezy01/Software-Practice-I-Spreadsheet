// Skeleton implementation by: Joe Zachary, Daniel Kopta, Travis Martin for CS 3500
// Last updated: August 2023 (small tweak to API)

namespace SpreadsheetUtilities;

/// <summary>
/// (s1,t1) is an ordered pair of strings
/// t1 depends on s1; s1 must be evaluated before t1
/// 
/// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
/// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
/// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
/// set, and the element is already in the set, the set remains unchanged.
/// 
/// Given a DependencyGraph DG:
/// 
///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///        (The set of things that depend on s)    
///        
///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///        (The set of things that s depends on) 
//
// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
//     dependents("a") = {"b", "c"}
//     dependents("b") = {"d"}
//     dependents("c") = {}
//     dependents("d") = {"d"}
//     dependees("a") = {}
//     dependees("b") = {"a"}
//     dependees("c") = {"a"}
//     dependees("d") = {"b", "d"}
/// </summary>
public class DependencyGraph
{
    //Dictionaries to hold each strings dependents and dependees
    private Dictionary<String, HashSet<String>> dependents; //Nodes in each strings list can be evaluated after it
    private Dictionary<String, HashSet<String>> dependees; //Nodes in each strings list must be evaluated before it

    //Keeping track of number of dependencies
    private int numOfDependencies;

    /// <summary>
    /// Creates an empty DependencyGraph.
    /// </summary>
    public DependencyGraph()
    {
        dependents = new Dictionary<string, HashSet<string>>();
        dependees = new Dictionary<string, HashSet<string>>();
        numOfDependencies = 0;

    }


    /// <summary>
    /// The number of ordered pairs in the DependencyGraph.
    /// This is an example of a property.
    /// </summary>
    public int NumDependencies
    {
        get { return numOfDependencies; }
    }


    /// <summary>
    /// Returns the size of dependees(s),
    /// that is, the number of things that s depends on.
    /// </summary>
    public int NumDependees(string s)
    {
        
        return dependees[s].Count();
    }


    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    public bool HasDependents(string s)
    {
        HashSet<string> temp = dependents[s];
        return temp.Count() != 0;
    }


    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    public bool HasDependees(string s)
    {
        HashSet<string> temp = dependees[s];
        return temp.Count() != 0;
    }


    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    public IEnumerable<string> GetDependents(string s)
    {
        HashSet<string> temp = dependents[s];
        return temp;
    }


    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    public IEnumerable<string> GetDependees(string s)
    {
        HashSet<string> temp = dependees[s];
        return temp;
    }


    /// <summary>
    /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
    /// 
    /// <para>This should be thought of as:</para>   
    /// 
    ///   t depends on s
    ///
    /// </summary>
    /// <param name="s"> s must be evaluated first. T depends on S</param>
    /// <param name="t"> t cannot be evaluated until s is</param>
    public void AddDependency(string s, string t)
    {
        if (!dependents.ContainsKey(s)) //If s is a new variable we must create its HashSet
        {
            /// T is a dependent of S
            dependents.Add(s, new HashSet<string>());
            dependees.Add(s, new HashSet<string>());
        }
        if (!dependees.ContainsKey(t)) //If t is a new variable we must create its HashSet
        {
            ///S is a dependee of T
            dependees.Add(t, new HashSet<string>());
            dependents.Add(t, new HashSet<string>());
        }
        if (dependents[s].Contains(t))
            return;
        else
        {
            dependents[s].Add(t);
            dependees[t].Add(s);
            numOfDependencies++;
        }
        
    }


    /// <summary>
    /// Removes the ordered pair (s,t), if it exists
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    public void RemoveDependency(string s, string t)
    {
        if (!dependents.ContainsKey(s) || !dependees.ContainsKey(t)) //Both keys need to be present
        {
            return;
        }
        dependents[s].Remove(t);
        dependees[t].Remove(s);
        numOfDependencies--;
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (s,r).  Then, for each
    /// t in newDependents, adds the ordered pair (s,t).
    /// </summary>
    public void ReplaceDependents(string s, IEnumerable<string> newDependents)
    {
        foreach(string t in dependents[s])
        {
            RemoveDependency(s, t);
        }
        foreach(string t in newDependents)
        {
            AddDependency(s, t);
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
    /// t in newDependees, adds the ordered pair (t,s).
    /// </summary>
    public void ReplaceDependees(string s, IEnumerable<string> newDependees)
    {
        foreach(string t in dependees[s])
        {
            RemoveDependency(t, s);
        }
        foreach(string t in newDependees)
        {
            AddDependency(t, s);
        }
    }
}
