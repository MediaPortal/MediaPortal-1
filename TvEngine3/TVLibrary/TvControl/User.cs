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
using System.Net;

namespace TvControl
{
  /// <summary>
  /// Class holding user credentials
  /// </summary>
  [Serializable]
  public class User
  {
    string _hostName;
    bool _isAdmin;

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    public User()
    {
      _hostName = Dns.GetHostName();
      _isAdmin = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
    public User(string name, bool isAdmin)
    {
      _hostName = name;
      _isAdmin = isAdmin;
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
      get
      {
        return _hostName;
      }
      set
      {
        _hostName = value;
      }
    }
    /// <summary>
    /// Gets or sets a value indicating whether this instance is admin.
    /// </summary>
    /// <value><c>true</c> if this instance is admin; otherwise, <c>false</c>.</value>
    public bool IsAdmin
    {
      get
      {
        return _isAdmin;
      }
      set
      {
        _isAdmin = value;
      }
    }
  }
}
