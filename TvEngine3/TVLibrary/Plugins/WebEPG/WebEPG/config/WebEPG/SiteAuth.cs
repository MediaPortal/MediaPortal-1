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
using System.Xml.Serialization;

namespace MediaPortal.WebEPG.Config
{
  /// <summary>
  /// The information for a Channel.
  /// </summary>
  public class SiteAuth
  {
    #region Variables

    [XmlAttribute("id")] public string id;
    [XmlAttribute("username")] public string username;
    [XmlAttribute("password")] public string password;

    #endregion

    public NetworkCredential GetCredentails()
    {
      return new NetworkCredential(username, password);
    }
  }
}