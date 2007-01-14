/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using DirectShowLib.SBE;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvEngine;
using TvDatabase;
using TvEngine.Events;

namespace TvService
{
  /// <summary>
  /// Class which holds the context for a specific card
  /// </summary>
  public class TvCardContext
  {
    #region variables
    User _user;
    int _idChannel;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardContext"/> class.
    /// </summary>
    public TvCardContext()
    {
      _user = null;
      _idChannel = -1;
    }
    #endregion

    #region public methods
    /// <summary>
    /// Locks the card for the user specifies
    /// </summary>
    /// <param name="user">The user.</param>
    public void Lock(User user)
    {
      _user = user;
    }

    /// <summary>
    /// Unlocks this card.
    /// </summary>
    public void Unlock()
    {
      _user = null;
    }

    /// <summary>
    /// Determines whether the the card is locked and ifso returns by which used.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified user is locked; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLocked(out User user)
    {
      user = _user;
      return (_user != null);
    }

    /// <summary>
    /// Gets or sets the database id of the tv/radio channel we are currently tuned on
    /// </summary>
    /// <value>The id channel.</value>
    public int IdChannel
    {
      get
      {
        return _idChannel;
      }
      set
      {
        _idChannel = value;
      }
    }
    #endregion

  }
}
