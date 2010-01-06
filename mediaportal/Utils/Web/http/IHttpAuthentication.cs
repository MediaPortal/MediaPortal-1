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

using System.Net;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Interface for Http Authentication
  /// </summary>
  public interface IHttpAuthentication
  {
    /// <summary>
    /// Gets the NetworkCredentials for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <returns>the NetworkCredentials</returns>
    NetworkCredential Get(string site);

    /// <summary>
    /// Adds the NetworkCredentials for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <param name="login">The login.</param>
    void Add(string site, NetworkCredential login);
  }
}