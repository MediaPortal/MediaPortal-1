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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts
{
  /// <summary>
  /// A class capable of holding elementary stream information for one stream
  /// in a program map.
  /// </summary>
  public class PmtElementaryStream
  {
    #region variables

    private StreamType _streamType;
    private ushort _pid;
    private ushort _esInfoLength;
    private List<IDescriptor> _descriptors;
    private List<IDescriptor> _caDescriptors;

    private LogicalStreamType _logicalStreamType;

    #endregion

    #region properties

    /// <summary>
    /// The elementary stream's type.
    /// </summary>
    public StreamType StreamType
    {
      get
      {
        return _streamType;
      }
      internal set
      {
        _streamType = value;
      }
    }

    /// <summary>
    /// The elementary stream's PID.
    /// </summary>
    public ushort Pid
    {
      get
      {
        return _pid;
      }
      internal set
      {
        _pid = value;
      }
    }

    /// <summary>
    /// The total number of bytes in the elementary stream's descriptors.
    /// </summary>
    public ushort EsInfoLength
    {
      get
      {
        return _esInfoLength;
      }
      internal set
      {
        _esInfoLength = value;
      }
    }

    /// <summary>
    /// The elementary stream's descriptors.
    /// </summary>
    /// <remarks>
    /// Conditional access descriptors are not included.
    /// </remarks>
    public List<IDescriptor> Descriptors
    {
      get
      {
        return _descriptors;
      }
      internal set
      {
        _descriptors = value;
      }
    }

    /// <summary>
    /// The elementary stream's conditional access descriptors.
    /// </summary>
    public List<IDescriptor> CaDescriptors
    {
      get
      {
        return _caDescriptors;
      }
      internal set
      {
        _caDescriptors = value;
      }
    }

    /// <summary>
    /// The logical type or category of the elementary stream. 
    /// </summary>
    /// <remarks>
    /// This property can be used to quickly and precisely determine the stream
    /// type when the stream type would normally be indicated using one or more
    /// descriptors.
    /// </remarks>
    public LogicalStreamType LogicalStreamType
    {
      get
      {
        return _logicalStreamType;
      }
      internal set
      {
        _logicalStreamType = value;
      }
    }

    #endregion

    /// <summary>
    /// For debug use.
    /// </summary>
    public void Dump()
    {
      this.LogDebug("Elementary Stream: dump...");
      this.LogDebug("  stream type         = {0}", _streamType);
      this.LogDebug("  PID                 = {0}", _pid);
      this.LogDebug("  length              = {0}", _esInfoLength);
      this.LogDebug("  logical stream type = {0}", _logicalStreamType);
      this.LogDebug("  {0} descriptor(s)...", _descriptors.Count + _caDescriptors.Count);
      foreach (IDescriptor d in _descriptors)
      {
        d.Dump();
      }
      foreach (IDescriptor cad in _caDescriptors)
      {
        cad.Dump();
      }
    }
  }
}