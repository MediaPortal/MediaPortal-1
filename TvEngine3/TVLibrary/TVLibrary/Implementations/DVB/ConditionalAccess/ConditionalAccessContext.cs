/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Generic;
using TvLibrary.Channels;
using TvLibrary.Interfaces;


namespace TvLibrary.Implementations.DVB
{
  ///<summary>
  /// CA Context 
  ///</summary>
  public class ConditionalAccessContext
  {
    CamType _camType;
    DVBBaseChannel _channel;
    byte[] _PMT;
    int _pmtLength;
    int _audioPid;
    int _serviceId;
    List<ushort> _HwPids;

    /// <summary>
    /// Gets or sets the type of the cam.
    /// </summary>
    /// <value>The type of the cam.</value>
    public CamType CamType
    {
      get
      {
        return _camType;
      }
      set
      {
        _camType = value;
      }
    }
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    /// <value>The channel.</value>
    public DVBBaseChannel Channel
    {
      get
      {
        return _channel;
      }
      set
      {
        _channel = value;
      }
    }
    /// <summary>
    /// Gets or sets the PMT.
    /// </summary>
    /// <value>The PMT.</value>
    public byte[] PMT
    {
      get
      {
        return _PMT;
      }
      set
      {
        _PMT = value;
      }
    }
    /// <summary>
    /// Gets or sets the length of the PMT.
    /// </summary>
    /// <value>The length of the PMT.</value>
    public int PMTLength
    {
      get
      {
        return _pmtLength;
      }
      set
      {
        _pmtLength = value;
      }
    }
    /// <summary>
    /// Gets or sets the audio pid.
    /// </summary>
    /// <value>The audio pid.</value>
    public int AudioPid
    {
      get
      {
        return _audioPid;
      }
      set
      {
        _audioPid = value;
      }
    }
    /// <summary>
    /// Gets or sets the service id.
    /// </summary>
    /// <value>The service id.</value>
    public int ServiceId
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
    /// Gets or sets the Pid list for hardware filtering.
    /// </summary>
    /// <value>The Pid list.</value>
    public List<ushort> HwPids
    {
      get
      {
        return _HwPids;
      }
      set
      {
        _HwPids = value;
      }
    }

  }
}
