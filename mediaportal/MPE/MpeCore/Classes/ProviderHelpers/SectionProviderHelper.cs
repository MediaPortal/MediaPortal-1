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