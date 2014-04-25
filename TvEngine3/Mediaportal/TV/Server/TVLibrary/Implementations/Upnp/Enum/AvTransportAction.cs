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
  internal sealed class AvTransportAction
  {
    private readonly string _name;
    private static readonly IDictionary<string, AvTransportAction> _values = new Dictionary<string, AvTransportAction>();

    public static readonly AvTransportAction Play = new AvTransportAction("Play");
    public static readonly AvTransportAction Stop = new AvTransportAction("Stop");
    public static readonly AvTransportAction Pause = new AvTransportAction("Pause");
    public static readonly AvTransportAction Seek = new AvTransportAction("Seek");
    public static readonly AvTransportAction Next = new AvTransportAction("Next");
    public static readonly AvTransportAction Previous = new AvTransportAction("Previous");
    public static readonly AvTransportAction Record = new AvTransportAction("Record");

    private AvTransportAction(string name)
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
      AvTransportAction action = obj as AvTransportAction;
      if (action != null && this == action)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static explicit operator AvTransportAction(string name)
    {
      AvTransportAction value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(AvTransportAction action)
    {
      return action._name;
    }
  }
}