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
using System.Collections.Generic;
using System.Text;
using TvControl;
using TvLibrary.Interfaces;

namespace TvEngine.Events
{
  public class TvServerEventArgs : EventArgs
  {
    #region variables
    User _user;
    VirtualCard _card;
    IChannel _channel;
    IController _controller;
    #endregion

    #region properties
    /// <summary>
    /// Gets the controller.
    /// </summary>
    /// <value>The controller.</value>
    public IController Controller
    {
      get
      {
        return _controller;
      }
    }
    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <value>The user.</value>
    public User User
    {
      get
      {
        return _user;
      }
    }

    /// <summary>
    /// Gets the card.
    /// </summary>
    /// <value>The card.</value>
    public VirtualCard Card
    {
      get
      {
        return _card;
      }
    }

    /// <summary>
    /// Gets the channel.
    /// </summary>
    /// <value>The channel.</value>
    public IChannel channel
    {
      get
      {
        return _channel;
      }
    }
    #endregion
  }
}
