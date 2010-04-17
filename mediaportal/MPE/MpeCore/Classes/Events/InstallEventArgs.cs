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
using MpeCore.Classes;

namespace MpeCore.Classes.Events
{
  public class InstallEventArgs
  {
    public InstallEventArgs(GroupItem groupItem, FileItem fileItem)
    {
      Group = groupItem;
      Item = fileItem;
      Description = string.Empty;
    }

    public InstallEventArgs(string description)
    {
      Group = new GroupItem();
      Item = new FileItem();
      Description = description;
    }

    /// <summary>
    /// Gets or sets the currently  intalled file item
    /// </summary>
    /// <value>The item.</value>
    public FileItem Item { get; set; }

    /// <summary>
    /// Gets or sets the currently  intalling group
    /// </summary>
    /// <value>The group.</value>
    public GroupItem Group { get; set; }

    public string Description { get; set; }
  }
}