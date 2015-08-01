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
  internal sealed class ShefRemoteKey
  {
    private readonly string _name;
    private static readonly IDictionary<string, ShefRemoteKey> _values = new Dictionary<string, ShefRemoteKey>();

    public static readonly ShefRemoteKey Power = new ShefRemoteKey("power");
    public static readonly ShefRemoteKey PowerOn = new ShefRemoteKey("poweron");
    public static readonly ShefRemoteKey PowerOff = new ShefRemoteKey("poweroff");
    public static readonly ShefRemoteKey Format = new ShefRemoteKey("format");
    public static readonly ShefRemoteKey Pause = new ShefRemoteKey("pause");
    public static readonly ShefRemoteKey Rewind = new ShefRemoteKey("rew");
    public static readonly ShefRemoteKey Replay = new ShefRemoteKey("replay");
    public static readonly ShefRemoteKey Stop = new ShefRemoteKey("stop");
    public static readonly ShefRemoteKey Advance = new ShefRemoteKey("advance");
    public static readonly ShefRemoteKey FastForward = new ShefRemoteKey("ffwd");
    public static readonly ShefRemoteKey Record = new ShefRemoteKey("record");
    public static readonly ShefRemoteKey Play = new ShefRemoteKey("play");
    public static readonly ShefRemoteKey Guide = new ShefRemoteKey("guide");
    public static readonly ShefRemoteKey Active = new ShefRemoteKey("active");
    public static readonly ShefRemoteKey List = new ShefRemoteKey("list");
    public static readonly ShefRemoteKey Exit = new ShefRemoteKey("exit");
    public static readonly ShefRemoteKey Back = new ShefRemoteKey("back");
    public static readonly ShefRemoteKey Menu = new ShefRemoteKey("menu");
    public static readonly ShefRemoteKey Info = new ShefRemoteKey("info");
    public static readonly ShefRemoteKey Up = new ShefRemoteKey("up");
    public static readonly ShefRemoteKey Down = new ShefRemoteKey("down");
    public static readonly ShefRemoteKey Left = new ShefRemoteKey("left");
    public static readonly ShefRemoteKey Right = new ShefRemoteKey("right");
    public static readonly ShefRemoteKey Select = new ShefRemoteKey("select");
    public static readonly ShefRemoteKey Red = new ShefRemoteKey("red");
    public static readonly ShefRemoteKey Green = new ShefRemoteKey("green");
    public static readonly ShefRemoteKey Yellow = new ShefRemoteKey("yellow");
    public static readonly ShefRemoteKey Blue = new ShefRemoteKey("blue");
    public static readonly ShefRemoteKey ChannelUp = new ShefRemoteKey("chanup");
    public static readonly ShefRemoteKey ChannelDown = new ShefRemoteKey("chandown");
    public static readonly ShefRemoteKey Previous = new ShefRemoteKey("prev");
    public static readonly ShefRemoteKey Zero = new ShefRemoteKey("0");
    public static readonly ShefRemoteKey One = new ShefRemoteKey("1");
    public static readonly ShefRemoteKey Two = new ShefRemoteKey("2");
    public static readonly ShefRemoteKey Three = new ShefRemoteKey("3");
    public static readonly ShefRemoteKey Four = new ShefRemoteKey("4");
    public static readonly ShefRemoteKey Five = new ShefRemoteKey("5");
    public static readonly ShefRemoteKey Six = new ShefRemoteKey("6");
    public static readonly ShefRemoteKey Seven = new ShefRemoteKey("7");
    public static readonly ShefRemoteKey Eight = new ShefRemoteKey("8");
    public static readonly ShefRemoteKey Nine = new ShefRemoteKey("9");
    public static readonly ShefRemoteKey Dash = new ShefRemoteKey("dash");    // AKA hyphen, AKA -
    public static readonly ShefRemoteKey Enter = new ShefRemoteKey("enter");

    private ShefRemoteKey(string name)
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
      ShefRemoteKey key = obj as ShefRemoteKey;
      if (key != null && this == key)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static ICollection<ShefRemoteKey> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator ShefRemoteKey(string name)
    {
      ShefRemoteKey value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(ShefRemoteKey key)
    {
      return key._name;
    }
  }
}