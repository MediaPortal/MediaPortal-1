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
using System.Runtime.Serialization;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations
{
  [DataContract]
  public class ScannedChannel
  {
    [DataMember]
    private IChannel _channel = null;

    [DataMember]
    private bool _isVisibleInGuide = true;

    [DataMember]
    private ushort _previousOriginalNetworkId = 0;

    [DataMember]
    private ushort _previousTransportStreamId = 0;

    [DataMember]
    private ushort _previousServiceId = 0;

    [DataMember]
    private IDictionary<ChannelGroupType, ICollection<ulong>> _groups = new Dictionary<ChannelGroupType, ICollection<ulong>>();

    public ScannedChannel(IChannel channel)
    {
      _channel = channel;
    }

    public IChannel Channel
    {
      get
      {
        return _channel;
      }
    }

    public bool IsVisibleInGuide
    {
      get
      {
        return _isVisibleInGuide;
      }
      set
      {
        _isVisibleInGuide = value;
      }
    }

    public ushort PreviousOriginalNetworkId
    {
      get
      {
        return _previousOriginalNetworkId;
      }
      set
      {
        _previousOriginalNetworkId = value;
      }
    }

    public ushort PreviousTransportStreamId
    {
      get
      {
        return _previousTransportStreamId;
      }
      set
      {
        _previousTransportStreamId = value;
      }
    }

    public ushort PreviousServiceId
    {
      get
      {
        return _previousServiceId;
      }
      set
      {
        _previousServiceId = value;
      }
    }

    public IDictionary<ChannelGroupType, ICollection<ulong>> Groups
    {
      get
      {
        return _groups;
      }
    }
  }
}