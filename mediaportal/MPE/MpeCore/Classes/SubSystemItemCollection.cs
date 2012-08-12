#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Linq;
using System.Text;

namespace MpeCore.Classes
{
  public class SubSystemItemCollection
  {
    public List<SubSystemItem> Items { get; set; }

    public SubSystemItemCollection()
    {
      Items = new List<SubSystemItem>();
    }

    public void Add(SubSystemItem item)
    {
      Items.Add(item);
    }

    public void Sort()
    {
      Items.Sort(Compare);
    }

    static int Compare(SubSystemItem ssi1, SubSystemItem ssi2)
    {
      return ssi1.Name.CompareTo(ssi2.Name);
    }
  }
}
