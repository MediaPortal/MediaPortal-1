#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;

namespace Mediaportal.TV.Server.TVService.Interfaces.Services
{
  public interface IUser
  {
    /// <summary>
    /// Get the user's name.
    /// </summary>
    /// <value>The user's name.</value>
    [DataMember]
    string Name { get; }

    /// <summary>
    /// Get the user's type.
    /// </summary>
    /// <value>The user's type.</value>
    [DataMember]
    UserType Type { get; }

    /// <summary>
    /// Get the user's sub-channels.
    /// </summary>
    /// <remarks>
    /// The dictionary key is the sub-channel's identifier.
    /// The dictionary should not be modified.
    /// </remarks>
    /// <value>The user's sub-channels.</value>
    //[DataMember]
    //IDictionary<int, ISubChannel> SubChannels { get; }
  }
}