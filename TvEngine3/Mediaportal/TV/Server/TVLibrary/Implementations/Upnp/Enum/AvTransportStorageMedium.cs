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
  internal sealed class AvTransportStorageMedium
  {
    private readonly string _name;
    private static readonly IDictionary<string, AvTransportStorageMedium> _values = new Dictionary<string, AvTransportStorageMedium>();

    public static readonly AvTransportStorageMedium Unknown = new AvTransportStorageMedium("UNKNOWN");
    public static readonly AvTransportStorageMedium Dv = new AvTransportStorageMedium("DV");
    public static readonly AvTransportStorageMedium MiniDv = new AvTransportStorageMedium("MINI-DV");
    public static readonly AvTransportStorageMedium Vhs = new AvTransportStorageMedium("VHS");
    public static readonly AvTransportStorageMedium Wvhs = new AvTransportStorageMedium("W-VHS");
    public static readonly AvTransportStorageMedium Svhs = new AvTransportStorageMedium("S-VHS");
    public static readonly AvTransportStorageMedium Dvhs = new AvTransportStorageMedium("D-VHS");
    public static readonly AvTransportStorageMedium Vhsc = new AvTransportStorageMedium("VHSC");
    public static readonly AvTransportStorageMedium Video8 = new AvTransportStorageMedium("VIDEO8");
    public static readonly AvTransportStorageMedium Hi8 = new AvTransportStorageMedium("HI8");
    public static readonly AvTransportStorageMedium Cdrom = new AvTransportStorageMedium("CD-ROM");
    public static readonly AvTransportStorageMedium Cdda = new AvTransportStorageMedium("CD-DA");
    public static readonly AvTransportStorageMedium Cdr = new AvTransportStorageMedium("CD-R");
    public static readonly AvTransportStorageMedium Cdrw = new AvTransportStorageMedium("CD-RW");
    public static readonly AvTransportStorageMedium VideoCd = new AvTransportStorageMedium("VIDEO-CD");
    public static readonly AvTransportStorageMedium Sacd = new AvTransportStorageMedium("SACD");
    public static readonly AvTransportStorageMedium MdAudio = new AvTransportStorageMedium("MD-AUDIO");
    public static readonly AvTransportStorageMedium MdPicture = new AvTransportStorageMedium("MD-PICTURE");
    public static readonly AvTransportStorageMedium Dvdrom = new AvTransportStorageMedium("DVD-ROM");
    public static readonly AvTransportStorageMedium DvdVideo = new AvTransportStorageMedium("DVD-VIDEO");
    public static readonly AvTransportStorageMedium Dvdr = new AvTransportStorageMedium("DVD-R");
    public static readonly AvTransportStorageMedium DvdPlusRw = new AvTransportStorageMedium("DVD+RW");
    public static readonly AvTransportStorageMedium DvdMinusRw = new AvTransportStorageMedium("DVD-RW");
    public static readonly AvTransportStorageMedium Dvdram = new AvTransportStorageMedium("DVD-RAM");
    public static readonly AvTransportStorageMedium DvdAudio = new AvTransportStorageMedium("DVD-AUDIO");
    public static readonly AvTransportStorageMedium Dat = new AvTransportStorageMedium("DAT");
    public static readonly AvTransportStorageMedium Ld = new AvTransportStorageMedium("LD");
    public static readonly AvTransportStorageMedium Hdd = new AvTransportStorageMedium("HDD");
    public static readonly AvTransportStorageMedium MicroMv = new AvTransportStorageMedium("MICRO-MV");
    public static readonly AvTransportStorageMedium Network = new AvTransportStorageMedium("NETWORK");
    public static readonly AvTransportStorageMedium None = new AvTransportStorageMedium("NONE");
    public static readonly AvTransportStorageMedium NotImplemented = new AvTransportStorageMedium("NOT_IMPLEMENTED");

    private AvTransportStorageMedium(string name)
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
      AvTransportStorageMedium medium = obj as AvTransportStorageMedium;
      if (medium != null && this == medium)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static explicit operator AvTransportStorageMedium(string name)
    {
      AvTransportStorageMedium value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(AvTransportStorageMedium medium)
    {
      return medium._name;
    }
  }
}