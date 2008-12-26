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

namespace TvLibrary.ChannelLinkage
{
  /// <summary>
  /// class which holds all linked channels
  /// </summary>
  [Serializable]
  public class LinkedChannel
  {
    #region variables
    ushort _networkId;
    ushort _transportId;
    ushort _serviceId;
    string _name;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedChannel"/> class.
    /// </summary>
    public LinkedChannel()
    {
      _networkId = 0;
      _transportId = 0;
      _serviceId = 0;
      _name = "";
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets/Sets the network id
    /// </summary>
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

    /// <summary>
    /// Gets/Sets the transport id
    /// </summary>
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

    /// <summary>
    /// Gets/Sets the service id
    /// </summary>
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

    /// <summary>
    /// Gets/Sets the linked channel name
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        _name = value;
      }
    }
    #endregion

  }
}
