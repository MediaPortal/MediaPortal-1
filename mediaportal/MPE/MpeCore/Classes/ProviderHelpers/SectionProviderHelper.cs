using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MpeCore.Interfaces;

namespace MpeCore.Classes.ProviderHelpers
{
  public class SectionProviderHelper
  {
    public SectionProviderHelper()
    {
      Items = new Dictionary<string, Type>();
    }

    public ISectionPanel this[string index]
    {
      get
      {
        /* return the specified index here */
        return (ISectionPanel)Activator.CreateInstance(Items[index]);
      }
    }

    public void Add(string name, object obj)
    {
      Items.Add(name, obj.GetType());
    }

    public Dictionary<string, Type> Items { get; set; }
  }
}