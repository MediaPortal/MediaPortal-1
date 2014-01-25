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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Enum
{
  public sealed class ConnectionDirection
  {
    private readonly string _name;
    private static readonly IDictionary<string, ConnectionDirection> _values = new Dictionary<string, ConnectionDirection>();

    public static readonly ConnectionDirection Output = new ConnectionDirection("Output");
    public static readonly ConnectionDirection Input = new ConnectionDirection("Input");

    private ConnectionDirection(string name)
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
      ConnectionDirection direction = obj as ConnectionDirection;
      if (direction != null && this == direction)
      {
        return true;
      }
      return false;
    }

    public static ICollection<ConnectionDirection> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator ConnectionDirection(string name)
    {
      ConnectionDirection value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(ConnectionDirection direction)
    {
      return direction._name;
    }
  }
}