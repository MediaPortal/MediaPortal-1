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
  internal sealed class AvTransportRecordMediumWriteStatus
  {
    private readonly string _name;
    private static readonly IDictionary<string, AvTransportRecordMediumWriteStatus> _values = new Dictionary<string, AvTransportRecordMediumWriteStatus>();

    public static readonly AvTransportRecordMediumWriteStatus Writable = new AvTransportRecordMediumWriteStatus("WRITABLE");
    public static readonly AvTransportRecordMediumWriteStatus Protected = new AvTransportRecordMediumWriteStatus("PROTECTED");
    public static readonly AvTransportRecordMediumWriteStatus NotWritable = new AvTransportRecordMediumWriteStatus("NOT_WRITABLE");
    public static readonly AvTransportRecordMediumWriteStatus Unknown = new AvTransportRecordMediumWriteStatus("UNKNOWN");
    public static readonly AvTransportRecordMediumWriteStatus NotImplemented = new AvTransportRecordMediumWriteStatus("NOT_IMPLEMENTED");

    private AvTransportRecordMediumWriteStatus(string name)
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
      AvTransportRecordMediumWriteStatus status = obj as AvTransportRecordMediumWriteStatus;
      if (status != null && this == status)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static explicit operator AvTransportRecordMediumWriteStatus(string name)
    {
      AvTransportRecordMediumWriteStatus value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(AvTransportRecordMediumWriteStatus status)
    {
      return status._name;
    }
  }
}