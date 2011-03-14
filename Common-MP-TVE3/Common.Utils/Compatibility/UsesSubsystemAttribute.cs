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

namespace MediaPortal.Common.Utils
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
  public class UsesSubsystemAttribute : Attribute
  {
    private readonly string _subsystem;
    private readonly bool _used;

    public UsesSubsystemAttribute(string subsystem, bool used)
    {
      _subsystem = subsystem;
      _used = used;
    }
    
    public UsesSubsystemAttribute(string subsystem)
      : this(subsystem, true)
    {
    }

    public string Subsystem
    {
      get { return _subsystem; }
    }

    public bool Used
    {
      get { return _used; }
    }

  }
}
