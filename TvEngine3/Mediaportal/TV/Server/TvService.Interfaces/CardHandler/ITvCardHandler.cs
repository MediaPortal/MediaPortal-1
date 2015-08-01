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

using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface ITvCardHandler
  {
    IParkedUserManagement ParkedUserManagement { get; }
    IUserManagement UserManagement { get; }
    IDisEqcManagement DisEqC { get; }
    IChannelScanning Scanner { get; }
    IEpgGrabbing Epg { get; }
    
    IRecorder Recorder { get; }
    ITimeShifter TimeShifter { get; }
    ICardTuner Tuner { get; }
    IConditionalAccessMenuActions CiMenuActions { get; }
    bool CiMenuSupported { get; }

    ITuner Card { get; }    
    bool IsIdle { get; }
    Tuner DataBaseCard { get; set; }

    IChannel CurrentChannel(string userName, int idChannel);
    int CurrentDbChannel(string userName);
    string CurrentChannelName(string userName, int idChannel);    
    bool IsScrambled(string userName);
    bool IsScrambled(int subchannel);

    void StopCard();
    void Dispose();
  }
}