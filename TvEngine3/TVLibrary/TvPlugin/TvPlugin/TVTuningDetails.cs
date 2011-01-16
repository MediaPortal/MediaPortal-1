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
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using TvDatabase;
using TvLibrary.Interfaces;
using TvControl;

namespace TvPlugin
{
  public class TVTuningDetails : GUIInternalWindow
  {
    public TVTuningDetails()
    {
      GetID = (int)Window.WINDOW_TV_TUNING_DETAILS;
    }

    #region Overrides

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvtuningdetails.xml");
      return bResult;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelName", TVHome.Card.ChannelName);
      GUIPropertyManager.SetProperty("#TV.TuningDetails.RTSPURL", TVHome.Card.RTSPUrl);
      Channel chan = TVHome.Navigator.Channel;
      if (chan != null)
      {
        try
        {
          GUIPropertyManager.SetProperty("#TV.TuningDetails.HasCiMenuSupport", TVHome.Card.CiMenuSupported().ToString());
        }
        catch (System.Exception ex)
        {
          Log.Error("Error loading TuningDetails /  HasCiMenuSupport:" + ex.StackTrace);
        }

        IList<TuningDetail> details = chan.ReferringTuningDetail();
        if (details.Count > 0)
        {
          TuningDetail detail = null;
          switch (TVHome.Card.Type)
          {
            case TvLibrary.Interfaces.CardType.Analog:
              foreach (TuningDetail t in details)
              {
                if (t.ChannelType == 0)
                  detail = t;
              }
              break;
            case TvLibrary.Interfaces.CardType.Atsc:
              foreach (TuningDetail t in details)
              {
                if (t.ChannelType == 1)
                  detail = t;
              }
              break;
            case TvLibrary.Interfaces.CardType.DvbC:
              foreach (TuningDetail t in details)
              {
                if (t.ChannelType == 2)
                  detail = t;
              }
              break;
            case TvLibrary.Interfaces.CardType.DvbS:
              foreach (TuningDetail t in details)
              {
                if (t.ChannelType == 3)
                  detail = t;
              }
              break;
            case TvLibrary.Interfaces.CardType.DvbT:
              foreach (TuningDetail t in details)
              {
                if (t.ChannelType == 4)
                  detail = t;
              }
              break;
            default:
              detail = details[0];
              break;
          }
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Band", detail.Band.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.BandWidth", detail.Bandwidth.ToString());
          switch (detail.ChannelType)
          {
            case 0:
              GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "Analog");
              break;
            case 1:
              GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "Atsc");
              break;
            case 2:
              GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "DVB-C");
              break;
            case 3:
              GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "DVB-S");
              break;
            case 4:
              GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "DVB-T");
              break;
          }

          IUser user = TVHome.Card.User;
          IVideoStream videoStream = TVHome.Card.GetCurrentVideoStream((User)user);
          IAudioStream[] audioStreams = TVHome.Card.AvailableAudioStreams;

          String audioPids = String.Empty;
          String videoPid = String.Empty;		  

          foreach (IAudioStream stream in audioStreams)
          {
            audioPids += stream.Pid + " (" + stream.StreamType + ") ";
          }
		  
          videoPid = videoStream.Pid.ToString() + " (" + videoStream.StreamType + ")";

          GUIPropertyManager.SetProperty("#TV.TuningDetails.CountryId", detail.CountryId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.FreeToAir", detail.FreeToAir.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Frequency", detail.Frequency.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.InnerFecRate", detail.InnerFecRate.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Modulation", detail.Modulation.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.NetworkId", detail.NetworkId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.PmtPid", detail.PmtPid.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Polarisation", detail.Polarisation.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Provider", detail.Provider);
          GUIPropertyManager.SetProperty("#TV.TuningDetails.ServiceId", detail.ServiceId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.SymbolRate", detail.Symbolrate.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.TransportId", detail.TransportId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.PcrPid", videoStream.PcrPid.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.VideoPid", videoPid);
          GUIPropertyManager.SetProperty("#TV.TuningDetails.AudioPid", audioPids);
        }
      }
    }

    private DateTime _updateTimer = DateTime.Now;
    public override void Process()
    {

      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds < 500)
      {
        return;
      }

      GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalLevel", TVHome.Card.SignalLevel.ToString());
      GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalQuality", TVHome.Card.SignalQuality.ToString());

      int totalTSpackets = 0;
      int discontinuityCounter = 0;
      TVHome.Card.GetStreamQualityCounters(out totalTSpackets, out discontinuityCounter);

      GUIPropertyManager.SetProperty("#TV.TuningDetails.TSPacketsTransferred", Convert.ToString(totalTSpackets));
      GUIPropertyManager.SetProperty("#TV.TuningDetails.Discontinuities", Convert.ToString(discontinuityCounter));

      _updateTimer = DateTime.Now;
    }

    #endregion
  }
}