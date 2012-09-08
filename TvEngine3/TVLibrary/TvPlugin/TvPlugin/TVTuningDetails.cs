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
using DirectShowLib.BDA;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using TvControl;
using TvDatabase;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;

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
      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\mytvtuningdetails.xml"));
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

          switch (detail.ChannelType)
          {
            case 0:
              GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "Analog");

              CountryCollection cc = new CountryCollection();
              Country c = cc.GetTunerCountryFromID(detail.CountryId);
              GUIPropertyManager.SetProperty("#TV.TuningDetails.Country", c.Name);
              GUIPropertyManager.SetProperty("#TV.TuningDetails.VideoSource", ((AnalogChannel.VideoInputType)detail.VideoSource).ToString());
              GUIPropertyManager.SetProperty("#TV.TuningDetails.AudioSource", ((AnalogChannel.AudioInputType)detail.AudioSource).ToString());
              break;
            case 1:
              if (detail.Modulation == (int)ModulationType.Mod8Vsb)
              {
                GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "ATSC");
              }
              else
              {
                GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "Cable");
              }
              GUIPropertyManager.SetProperty("#TV.TuningDetails.MajorChannelNumber", detail.MajorChannel.ToString());
              GUIPropertyManager.SetProperty("#TV.TuningDetails.MinorChannelNumber", detail.MinorChannel.ToString());
              break;
            case 2:
              GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "DVB-C");
              break;
            case 3:
              GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "DVB-S");
              GUIPropertyManager.SetProperty("#TV.TuningDetails.Polarisation", ((Polarisation)detail.Polarisation).ToString());
              GUIPropertyManager.SetProperty("#TV.TuningDetails.FecRate", ((BinaryConvolutionCodeRate)detail.InnerFecRate).ToString());
              GUIPropertyManager.SetProperty("#TV.TuningDetails.Pilot", ((Pilot)detail.Pilot).ToString());
              GUIPropertyManager.SetProperty("#TV.TuningDetails.RollOff", ((RollOff)detail.RollOff).ToString());
              break;
            case 4:
              GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", "DVB-T");
              GUIPropertyManager.SetProperty("#TV.TuningDetails.Bandwidth", detail.Bandwidth.ToString());
              break;
          }

          string videoStreams = "";
          int videoStreamCount = g_Player.VideoStreams;
          for (int i = 0; i < videoStreamCount; i++)
          {
            if (i != 0)
            {
              videoStreams += ", ";
            }
            videoStreams += g_Player.VideoType(i);
          }

          string audioStreams = "";
          int audioStreamCount = g_Player.AudioStreams;
          for (int i = 0; i < audioStreamCount; i++)
          {
            if (i != 0)
            {
              audioStreams += ", ";
            }
            audioStreams += g_Player.AudioLanguage(i) + "(" + g_Player.AudioType(i) + ")";
          }

          GUIPropertyManager.SetProperty("#TV.TuningDetails.Provider", detail.Provider);
          GUIPropertyManager.SetProperty("#TV.TuningDetails.FreeToAir", detail.FreeToAir.ToString());

          GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelNumber", detail.ChannelNumber.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Frequency", detail.Frequency.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.SymbolRate", detail.Symbolrate.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Modulation", ((ModulationType)detail.Modulation).ToString());

          GUIPropertyManager.SetProperty("#TV.TuningDetails.ServiceId", detail.ServiceId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.NetworkId", detail.NetworkId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.TransportId", detail.TransportId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.PmtPid", detail.PmtPid.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.VideoStream", videoStreams);
          GUIPropertyManager.SetProperty("#TV.TuningDetails.AudioStreams", audioStreams);
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