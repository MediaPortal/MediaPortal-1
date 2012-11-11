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

using System;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Scheduler;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardReservation.Implementations
{
  public class CardReservationRec : CardReservationBase
  {
 

    private CardDetail _cardInfo;
    private RecordingDetail _recDetail;


    protected override bool IsTunedToTransponder(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      return tvcard.Tuner.IsTunedToTransponder(tuningDetail);
    }

    public CardDetail CardInfo
    {
      get { return _cardInfo; }
      set { _cardInfo = value; }
    }

    public RecordingDetail RecDetail
    {
      get { return _recDetail; }
      set { _recDetail = value; }
    }  
    
    protected override bool OnStartTune(ITvCardHandler tvcard, IUser user, int idChannel)
    {
      bool startRecordingOnDisc = true;
      if (ServiceManager.Instance.InternalControllerService.SupportsSubChannels(_cardInfo.Card.IdCard) == false)
      {        
        this.LogDebug("Scheduler : record, now start timeshift");
        string timeshiftFileName = String.Format(@"{0}\live{1}-{2}.ts", _cardInfo.Card.TimeshiftingFolder, _cardInfo.Id,
                                                 tvcard.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel));
        startRecordingOnDisc = (TvResult.Succeeded == ServiceManager.Instance.InternalControllerService.StartTimeShifting(ref user, ref timeshiftFileName, idChannel));
      }

      if (startRecordingOnDisc)
      {
        _recDetail.MakeFileName(_cardInfo.Card.RecordingFolder);
        _recDetail.CardInfo = _cardInfo;
        this.LogDebug("Scheduler : record to {0}", _recDetail.FileName);
        string fileName = _recDetail.FileName;
        startRecordingOnDisc = (TvResult.Succeeded == ServiceManager.Instance.InternalControllerService.StartRecording(user.Name, user.CardId, out user, ref fileName));

        if (startRecordingOnDisc)
        {
          _recDetail.FileName = fileName;
          _recDetail.RecordingStartDateTime = DateTime.Now;
        }
      }
      if (!startRecordingOnDisc && ServiceManager.Instance.InternalControllerService.AllCardsIdle)
      {
        ServiceManager.Instance.InternalControllerService.EpgGrabberEnabled = true;
      }

      return startRecordingOnDisc;
    }

  }
}
