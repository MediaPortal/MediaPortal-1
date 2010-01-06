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
  public class DependencyItem
  {
    public DependencyItem()
    {
      Type = string.Empty;
      Id = string.Empty;
      MinVersion = new VersionInfo();
      MaxVersion = new VersionInfo();
      WarnOnly = true;
      Message = string.Empty;
      Name = string.Empty;
    }

    public DependencyItem(string type)
    {
      Type = type;
      Id = string.Empty;
      MinVersion = new VersionInfo();
      MaxVersion = new VersionInfo();
      WarnOnly = true;
      Message = string.Empty;
      Name = string.Empty;
    }

    public string Type { get; set; }
    public string Id { get; set; }
    public VersionInfo MinVersion { get; set; }
    public VersionInfo MaxVersion { get; set; }
    public bool WarnOnly { get; set; }

    private string _message;

    public string Message
    {
      get
      {
        if (!string.IsNullOrEmpty(_message)
          ) return _message;
        return string.Format("Need version of {0} with version betwen {1} - {2}", Name, MinVersion, MaxVersion);
      }
      set { _message = value; }
    }

    private string _name;

    public string Name
    {
      get
      {
        if (!string.IsNullOrEmpty(_name)) return _name;
        return Type;
      }
      set { _name = value; }
    }

    public override string ToString()
    {
      return string.Format("{0}{1}({2})-({3})", Type,
                           "", MinVersion, MaxVersion);
    }
  }
}