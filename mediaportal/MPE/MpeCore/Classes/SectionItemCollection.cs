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
using MpeCore.Interfaces;

namespace MpeCore.Classes
{
  public class SectionItemCollection
  {
    public SectionItemCollection()
    {
      Items = new List<SectionItem>();
    }

    public List<SectionItem> Items { get; set; }

    public void Add(SectionItem sectionItem)
    {
      Items.Add(sectionItem);
    }

    public SectionItem Add(string name)
    {
      SectionItem item = new SectionItem();
      ISectionPanel panel = MpeInstaller.SectionPanels[name];
      if (panel == null)
        return null;
      item.Name = panel.DisplayName;
      item.PanelName = panel.DisplayName;
      item.Params = panel.GetDefaultParams();
      Add(item);
      return item;
    }
  }
}