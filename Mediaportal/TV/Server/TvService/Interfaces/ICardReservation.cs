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

using Mediaportal.TV.Server.TVControl.Enums;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVDatabase.Gentle;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVService.CardManagement.CardHandler;

namespace Mediaportal.TV.Server.TVService.Interfaces
{
  public interface ICardReservation
  {
    ICardTuneReservationTicket RequestCardTuneReservation(ITvCardHandler tvcard, IChannel tuningDetail, IUser user);
    TvResult CardTune(ITvCardHandler tvcard, ref IUser user, IChannel channel, Channel dbChannel, ICardTuneReservationTicket ticket);
    TvResult Tune(ITvCardHandler tvcard, ref IUser user, IChannel channel, int idChannel, ICardTuneReservationTicket ticket);
  }
}
