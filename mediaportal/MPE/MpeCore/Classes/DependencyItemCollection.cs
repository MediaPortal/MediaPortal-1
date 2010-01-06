using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
  public class DependencyItemCollection
  {
    public List<DependencyItem> Items { get; set; }

    public DependencyItemCollection()
    {
      Items = new List<DependencyItem>();
    }

    public void Add(DependencyItem item)
    {
      Items.Add(item);
    }
  }
}