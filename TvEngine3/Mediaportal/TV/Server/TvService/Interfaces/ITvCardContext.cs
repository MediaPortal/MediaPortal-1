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
using Mediaportal.TV.Server.TVService.CardManagement.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces
{
  public interface ITvCardContext
  {
    /// <summary>
    ///   Sets the owner.
    /// </summary>
    /// <value>The owner.</value>
    OwnerSubChannel OwnerSubChannel { get; set; }

    /// <summary>
    ///   Gets the users.
    /// </summary>
    /// <value>The users.</value>
    IDictionary<string, IUser> Users { get; }
    IDictionary<string, ParkedUser> ParkedUsers { get; }
    IDictionary<string, IUser> UsersHistory { get; }
  }
}