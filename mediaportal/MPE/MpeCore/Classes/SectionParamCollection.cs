#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

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