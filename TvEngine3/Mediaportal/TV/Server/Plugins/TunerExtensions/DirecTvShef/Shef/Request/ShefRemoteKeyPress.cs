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

using System.Collections.Generic;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef.Request
{
  internal sealed class ShefRemoteKeyPress
  {
    private readonly string _name;
    private static readonly IDictionary<string, ShefRemoteKeyPress> _values = new Dictionary<string, ShefRemoteKeyPress>();

    public static readonly ShefRemoteKeyPress Up = new ShefRemoteKeyPress("keyUp");
    public static readonly ShefRemoteKeyPress Down = new ShefRemoteKeyPress("keyDown");
    public static readonly ShefRemoteKeyPress Press = new ShefRemoteKeyPress("keyPress");

    private ShefRemoteKeyPress(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      ShefRemoteKeyPress keyPress = obj as ShefRemoteKeyPress;
      if (keyPress != null && this == keyPress)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static ICollection<ShefRemoteKeyPress> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator ShefRemoteKeyPress(string name)
    {
      ShefRemoteKeyPress value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(ShefRemoteKeyPress keyPress)
    {
      return keyPress._name;
    }
  }
}