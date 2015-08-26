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

namespace Mediaportal.TV.Server.SetupTV.Sections.Helpers
{
  internal class Codec
  {
    #region constants

    private static readonly Guid CLSID_LAV_VIDEO = new Guid(0xee30215d, 0x164f, 0x4a92, 0xa4, 0xeb, 0x9d, 0x4c, 0x13, 0x39, 0x0f, 0x9f);
    private static readonly Guid CLSID_LAV_AUDIO = new Guid(0xe8e73b6b, 0x4cb3, 0x44a4, 0xbe, 0x99, 0x4f, 0x7b, 0xcb, 0x96, 0xe4, 0x91);

    public static readonly Codec DEFAULT_VIDEO = new Codec("LAV Video Decoder", CLSID_LAV_VIDEO);
    public static readonly Codec DEFAULT_AUDIO = new Codec("LAV Audio Decoder", CLSID_LAV_AUDIO);

    #endregion

    public string Name;
    public Guid ClassId;

    public Codec(string name, Guid classId)
    {
      Name = name;
      ClassId = classId;
    }

    #region object overrides

    public override bool Equals(object obj)
    {
      Codec other = obj as Codec;
      if (
        obj == null ||
        !string.Equals(Name, other.Name) ||
        !Guid.Equals(ClassId, other.ClassId)
      )
      {
        return false;
      }
      return true;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode() ^ Name.GetHashCode() ^ ClassId.GetHashCode();
    }

    public override string ToString()
    {
      return Name;
    }

    #endregion

    #region serialisation

    public string Serialise()
    {
      return string.Format("{0}|{1}", ClassId, Name ?? string.Empty);
    }

    public static Codec Deserialise(string serialCodec)
    {
      try
      {
        string[] details = (serialCodec ?? string.Empty).Split('|');
        if (details.Length != 2)
        {
          return null;
        }
        return new Codec(details[1], Guid.Parse(details[0]));
      }
      catch
      {
        return null;
      }
    }

    #endregion
  }
}