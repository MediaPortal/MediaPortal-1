using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
  public class GroupItemCollection
  {
    public GroupItemCollection()
    {
      Items = new List<GroupItem>();
    }


    public GroupItem this[string indexName]
    {
      get { return GetItem(indexName); }
    }

    public List<GroupItem> Items { get; set; }

    /// <summary>
    /// Adds the specified group.
    /// </summary>
    /// <param name="item">The group item.</param>
    public void Add(GroupItem item)
    {
      Items.Add(item);
    }

    private GroupItem GetItem(string item)
    {
      foreach (GroupItem sectionParam in Items)
      {
        if (sectionParam.Name.CompareTo(item) == 0)
          return sectionParam;
      }
      return null;
    }
  }
}