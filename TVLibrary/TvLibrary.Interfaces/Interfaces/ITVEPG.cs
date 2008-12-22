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
using System.Runtime.InteropServices;
using System.Text;
using TvLibrary.Epg;

namespace TvLibrary.Interfaces
{

  /// <summary>
  /// callback interface for the EPG
  /// </summary>
  [ComVisible(true), ComImport,
 Guid("FFAB5D98-2309-4d90-9C71-E4B2F490CF5A"),
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEpgCallback
  {
    /// <summary>
    /// Called when epg is received.
    /// </summary>
    /// <returns></returns>
    [PreserveSig]
    int OnEpgReceived();
  };

  /// <summary>
  /// Base class used for dvb epg-grabbing
  /// </summary>
  public abstract class BaseEpgGrabber : IEpgCallback
  {
    /// <summary>
    /// Gets called when epg has been cancelled
    /// Should be overriden by the class
    /// </summary>
    public virtual void OnEpgCancelled() 
    { 
    }
    /// <summary>
    /// Gets called when epg has been received
    /// Should be overriden by the class
    /// </summary>
    /// <returns></returns>
    public virtual int OnEpgReceived()
    {
      return 0;
    }

  }

  /// <summary>
  /// interface for dvb epg grabbing
  /// </summary>
  public interface ITVEPG
  {

    /// <summary>
    /// Starts the EPG grabber.
    /// When the epg has been received the OnEpgReceived event will be fired
    /// </summary>
    void GrabEpg(BaseEpgGrabber callback);

    /// <summary>
    /// returns a list of all epg data for each channel found.
    /// </summary>
    /// <value>The epg.</value>
    List<EpgChannel> Epg { get;}

  }
}
