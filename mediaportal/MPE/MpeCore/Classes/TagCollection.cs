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
  public class TagCollection
  {
    public TagCollection()
    {
      Tags = new List<string>();
    }

    public TagCollection(string tags)
    {
      Tags = new List<string>();
      Pharse(tags);
    }

    public List<string> Tags { get; set; }

    public void Add(TagCollection collection)
    {
      foreach (string s in collection.Tags)
      {
        Add(s);
      }
    }

    public void Add(string tag)
    {
      tag = tag.ToLower().Trim();
      if (string.IsNullOrEmpty(tag))
        return;
      if (!Tags.Contains(tag))
        Tags.Add(tag);
    }

    public void Pharse(string tags)
    {
      string[] list = tags.Split(',');
      foreach (string s in list)
      {
        Add(s);
      }
    }
  }
}