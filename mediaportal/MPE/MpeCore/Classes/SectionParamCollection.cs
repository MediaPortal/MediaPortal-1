using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
  public class SectionParamCollection
  {
    public SectionParamCollection()
    {
      Items = new List<SectionParam>();
    }

    public SectionParamCollection(SectionParamCollection collection)
    {
      Items = new List<SectionParam>();
      foreach (SectionParam list in collection.Items)
      {
        Add(new SectionParam(list));
      }
    }

    public List<SectionParam> Items { get; set; }

    public void Add(SectionParam sectionParam)
    {
      Items.Add(sectionParam);
    }

    public SectionParam this[string indexName]
    {
      get { return GetItem(indexName); }
    }

    /// <summary>
    /// Contains the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public bool Contain(string name)
    {
      foreach (SectionParam sectionParam in Items)
      {
        if (sectionParam.Name.CompareTo(name) == 0)
          return true;
      }
      return false;
    }

    private SectionParam GetItem(string item)
    {
      foreach (SectionParam sectionParam in Items)
      {
        if (sectionParam.Name.CompareTo(item) == 0)
          return sectionParam;
      }
      return new SectionParam();
    }
  }
}