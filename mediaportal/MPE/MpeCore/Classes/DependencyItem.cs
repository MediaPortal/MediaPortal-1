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
using System.Text;

namespace MpeCore.Classes
{
  public class DependencyItem
  {
    private string _name;
    private string _message;
    private VersionInfo _MinVersion = new VersionInfo();
    private VersionInfo _MaxVersion = new VersionInfo();

    public DependencyItem()
    {
      Type = string.Empty;
      Id = string.Empty;
      WarnOnly = true;
      Message = string.Empty;
      Name = string.Empty;
    }

    public DependencyItem(string type)
    {
      Type = type;
      Id = string.Empty;
      WarnOnly = true;
      Message = string.Empty;
      Name = string.Empty;
    }

    public string Type { get; set; }

    public string Id { get; set; }

    public VersionInfo MinVersion
    {
      get
      {
        return this._MinVersion;
      }
      set
      {
        //Check old MP versioning
        if (this.Type != "MediaPortal" || value.Major != "1" || value.Minor != "1" || value.Build != "6" || value.Revision != "27644")
        {
          this._MinVersion = value;
          this._message = null; //force to reload the message
        }
      }
    }

    public VersionInfo MaxVersion
    {
      get
      {
        return this._MaxVersion;
      }
      set
      {
        //Check old MP versioning
        if (this.Type != "MediaPortal" || value.Major != "1" || value.Minor != "1" || value.Build != "6" || value.Revision != "27644")
        {
          this._MaxVersion = value;
          this._message = null; //force to reload the message
        }
      }
    }
    
    public bool WarnOnly { get; set; }
    
    public string Message
    {
      get
      {
        if (this._message == null)
        {
          if (!this.MinVersion.IsAnyVersion && this.MaxVersion.IsAnyVersion)
          {
            this._message = string.Format("Requires {0} {1} or higher!",
              this.Name,
              this.MinVersion);
          }
          else if (this.MinVersion.IsAnyVersion && !this.MaxVersion.IsAnyVersion)
          {
            this._message = string.Format("Requires {0} {1} or lower!",
              this.Name,
              this.MaxVersion);
          }
          else if (!this.MinVersion.IsAnyVersion && !this.MaxVersion.IsAnyVersion)
          {
            if (this.MinVersion.ToString().Equals(this.MaxVersion.ToString()))
              this._message = string.Format("Requires {0} {1}!",
              this.Name,
              this.MinVersion);
            else
              this._message = string.Format("Requires {0} from {1} to {2}!",
                this.Name,
                this.MinVersion,
                this.MaxVersion);
          }
          else
            this._message = string.Empty;
        }

        return this._message;
      }
      set { _message = value; }
    }

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
      return string.Format("{0}{1} ({2})-({3})",
        Type,
        string.Empty,
        MinVersion,
        MaxVersion);
    }
  }
}