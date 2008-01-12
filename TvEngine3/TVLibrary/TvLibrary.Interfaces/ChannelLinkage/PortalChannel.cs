/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.ChannelLinkage
{
  /// <summary>
  /// class which holds all linked channels
  /// </summary>
  [Serializable]
  public class PortalChannel
  {
    #region variables
    ushort _networkId;
    ushort _transportId;
    ushort _serviceId;
    List<LinkedChannel> _linkedChannels;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="T:LinkedChannel"/> class.
    /// </summary>
    public PortalChannel()
    {
      _networkId = 0;
      _transportId = 0;
      _serviceId = 0;
      _linkedChannels = new List<LinkedChannel>();
    }
    #endregion

    #region Properties
    public ushort NetworkId
    {
      get
      {
        return _networkId;
      }
      set
      {
        _networkId = value;
      }
    }
    public ushort TransportId
    {
      get
      {
        return _transportId;
      }
      set
      {
        _transportId = value;
      }
    }
    public ushort ServiceId
    {
      get
      {
        return _serviceId;
      }
      set
      {
        _serviceId = value;
      }
    }
    public List<LinkedChannel> LinkedChannels
    {
      get
      {
        return _linkedChannels;
      }
      set
      {
        _linkedChannels = value;
      }
    }
    #endregion
  }
}
