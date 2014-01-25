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
  public sealed class AvTransportSeekMode
  {
    private readonly string _name;
    private static readonly IDictionary<string, AvTransportSeekMode> _values = new Dictionary<string, AvTransportSeekMode>();

    public static readonly AvTransportSeekMode TrackNr = new AvTransportSeekMode("TRACK_NR");
    public static readonly AvTransportSeekMode AbsTime = new AvTransportSeekMode("ABS_TIME");
    public static readonly AvTransportSeekMode RelTime = new AvTransportSeekMode("REL_TIME");
    public static readonly AvTransportSeekMode AbsCount = new AvTransportSeekMode("ABS_COUNT");
    public static readonly AvTransportSeekMode RelCount = new AvTransportSeekMode("REL_COUNT");
    public static readonly AvTransportSeekMode ChannelFreq = new AvTransportSeekMode("CHANNEL_FREQ");
    public static readonly AvTransportSeekMode TapeIndex = new AvTransportSeekMode("TAPE-INDEX");
    public static readonly AvTransportSeekMode Frame = new AvTransportSeekMode("FRAME");

    private AvTransportSeekMode(string name)
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
      AvTransportSeekMode mode = obj as AvTransportSeekMode;
      if (mode != null && this == mode)
      {
        return true;
      }
      return false;
    }

    public static ICollection<AvTransportSeekMode> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator AvTransportSeekMode(string name)
    {
      AvTransportSeekMode value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(AvTransportSeekMode mode)
    {
      return mode._name;
    }
  }
}