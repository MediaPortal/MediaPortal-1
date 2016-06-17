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
using System.Globalization;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace Mediaportal.TV.TvPlugin
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
      Channel chan = ServiceAgents.Instance.ChannelServiceAgent.GetChannel(TVHome.Navigator.Channel.Entity.IdChannel, ChannelRelation.TuningDetails);
      if (chan != null)
      {
        try
        {
          GUIPropertyManager.SetProperty("#TV.TuningDetails.HasCiMenuSupport", TVHome.Card.CiMenuSupported().ToString(CultureInfo.InvariantCulture));
        }
        catch (System.Exception ex)
        {
          this.LogError("Error loading TuningDetails /  HasCiMenuSupport:" + ex.StackTrace);
        }

        IList<TuningDetail> details = chan.TuningDetails;
        if (details.Count > 0)
        {
          // TODO This is wrong! The service could have tuned using any of the tuning details supported by the tuner.
          TuningDetail detail = details[0];

          // TODO band property is bad
          //GUIPropertyManager.SetProperty("#TV.TuningDetails.Band", detail.IdLnbType.HasValue ? detail.IdLnbType.Value.ToString() : "-1");
          GUIPropertyManager.SetProperty("#TV.TuningDetails.BandWidth", detail.Bandwidth.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.channelType", ((BroadcastStandard)detail.BroadcastStandard).GetDescription());

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

          GUIPropertyManager.SetProperty("#TV.TuningDetails.CountryId", detail.CountryId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.FreeToAir", (!detail.IsEncrypted).ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Frequency", detail.Frequency.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.InnerFecRate", detail.FecCodeRate.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Modulation", detail.Modulation.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.NetworkId", detail.OriginalNetworkId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.PmtPid", detail.PmtPid.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Polarisation", detail.Polarisation.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Provider", detail.Provider);
          GUIPropertyManager.SetProperty("#TV.TuningDetails.ServiceId", detail.ServiceId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.SymbolRate", detail.SymbolRate.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.TransportId", detail.TransportStreamId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.VideoPid", videoStreams);
          GUIPropertyManager.SetProperty("#TV.TuningDetails.AudioPid", audioStreams);
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

      bool isLocked;
      bool isPresent;
      int strength;
      int quality;
      TVHome.Card.GetSignalStatus(false, out isLocked, out isPresent, out strength, out quality);
      GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalLevel", strength.ToString());
      GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalQuality", quality.ToString());

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