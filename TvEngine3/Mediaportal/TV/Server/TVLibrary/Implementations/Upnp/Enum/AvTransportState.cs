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
  public sealed class AvTransportState
  {
    private readonly string _name;
    private static readonly IDictionary<string, AvTransportState> _values = new Dictionary<string, AvTransportState>();

    public static readonly AvTransportState Stopped = new AvTransportState("STOPPED");
    public static readonly AvTransportState Playing = new AvTransportState("PLAYING");
    public static readonly AvTransportState Transitioning = new AvTransportState("TRANSITIONING");
    public static readonly AvTransportState PausedPlayback = new AvTransportState("PAUSED_PLAYBACK");
    public static readonly AvTransportState PausedRecording = new AvTransportState("PAUSED_RECORDING");
    public static readonly AvTransportState Recording = new AvTransportState("RECORDING");
    public static readonly AvTransportState NoMediaPresent = new AvTransportState("NO_MEDIA_PRESENT");

    private AvTransportState(string name)
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
      AvTransportState state = obj as AvTransportState;
      if (state != null && this == state)
      {
        return true;
      }
      return false;
    }

    public static explicit operator AvTransportState(string name)
    {
      AvTransportState value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(AvTransportState state)
    {
      return state._name;
    }
  }
}