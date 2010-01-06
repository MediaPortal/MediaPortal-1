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
using System.IO;
using System.Xml.Serialization;

namespace MpeCore.Classes
{
  public class GroupItem
  {
    public GroupItem()
    {
      Reset();
    }

    public GroupItem(string name)
    {
      Reset();
      Name = name;
      DisplayName = name;
      Description = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupItem"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="b">if set to <c>true</c> the group is checked [b].</param>
    public GroupItem(string name, bool b)
    {
      Reset();
      Name = name;
      DisplayName = name;
      Description = name;
      Checked = b;
      DefaulChecked = b;
    }

    public void Reset()
    {
      DefaulChecked = true;
      Files = new FileItemCollection();
      Name = string.Empty;
      Files = new FileItemCollection();
      DisplayName = string.Empty;
      Description = string.Empty;
    }

    public void SetDestinationPath(string path)
    {
      if (!path.EndsWith(("\\")))
      {
        path += "\\";
      }
      foreach (var fileItem in Files.Items)
      {
        fileItem.DestinationFilename = path + Path.GetFileName(fileItem.LocalFileName);
      }
    }

    public string ParentGroup { get; set; }

    private string _displayName;

    /// <summary>
    /// Gets or sets the display name. This name will be displayed when the group is displayed in a section control
    /// </summary>
    /// <value>The display name.</value>
    public string DisplayName
    {
      get
      {
        if (string.IsNullOrEmpty(_displayName))
          return Name;
        return _displayName;
      }
      set { _displayName = value; }
    }


    [XmlAttribute]
    /// <summary>
      /// Gets or sets the name of the group.
      /// </summary>
      /// <value>The name.</value>
      public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [defaul checked].
    /// </summary>
    /// <value><c>true</c> if [defaul checked]; otherwise, <c>false</c>.</value>
    public bool DefaulChecked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="GroupItem"/> is checked.
    /// </summary>
    /// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
    [XmlIgnoreAttribute]
    public bool Checked { get; set; }

    private string _description;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description
    {
      get
      {
        if (string.IsNullOrEmpty(_description))
          return Name;
        return _description;
      }
      set { _description = value; }
    }

    public FileItemCollection Files { get; set; }
  }
}