using System;
using System.Collections;
using System.Windows.Forms;

// Create a sorter that implements the IComparer interface.
public class RecordSorter : IComparer
{
  // Compare the length of the strings, or the strings
  // themselves, if they are the same length.
  public int Compare(object x, object y)
  {
    int result = -1;
    try
    {
      TreeNode tx = x as TreeNode;
      TreeNode ty = y as TreeNode;

      result = string.Compare(tx.Text, ty.Text, System.StringComparison.CurrentCulture);
    }
    catch (Exception)
    {
    }

    return result;
  }
}

public class RecordSorterInvariant : IComparer
{
  // Compare the length of the strings, or the strings
  // themselves, if they are the same length.
  public int Compare(object x, object y)
  {
    int result = -1;
    try
    {
      TreeNode tx = x as TreeNode;
      TreeNode ty = y as TreeNode;

      result = string.Compare(tx.Text, ty.Text, System.StringComparison.InvariantCulture);
    }
    catch (Exception)
    {
    }

    return result;
  }
}