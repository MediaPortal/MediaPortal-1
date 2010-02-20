#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Net;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Service Class that provided Authenication details for HTTP requests/transactions
  /// 
  /// Stores the NetworkCredentials for each site.
  /// </summary>
  public class HTTPAuth : IHttpAuthentication
  {
    #region Variables

    private Dictionary<string, NetworkCredential> _authList;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HTTPAuth"/> class.
    /// </summary>
    public HTTPAuth() {}

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the Network Credentials for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <returns>The Network Credentials</returns>
    public NetworkCredential Get(string site)
    {
      NetworkCredential login = null;

      if (_authList != null)
      {
        login = _authList[site];
      }

      return login;
    }

    /// <summary>
    /// Adds the Network Credentials for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <param name="login">The login.</param>
    public void Add(string site, NetworkCredential login)
    {
      if (_authList == null)
      {
        _authList = new Dictionary<string, NetworkCredential>();
      }

      _authList.Add(site, login);
    }

    #endregion
  }
}