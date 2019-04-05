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

using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl
{
  [DataContract]
  public class User : IUser
  {
    #region variables

    [DataMember]
    private string _name;

    [DataMember] 
    private UserType _type;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">The user's name.</param>
    /// <param name="type">The user's type.</param>
    public User(string name, UserType type = UserType.Normal)
    {
      _name = name;
      _type = type;
    }

    #region properties

    /// <summary>
    /// Get the user's name.
    /// </summary>
    /// <value>The user's name.</value>
    public string Name
    {
      get { return _name; }
    }

    /// <summary>
    /// Get the user's type.
    /// </summary>
    /// <value>The user's type.</value>
    public UserType Type
    {
      get { return _type; }
    }

    #endregion
  }
}