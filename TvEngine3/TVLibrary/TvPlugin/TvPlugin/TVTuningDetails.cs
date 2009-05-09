#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System.Collections.Generic;
using MediaPortal.GUI.Library;
using TvDatabase;

namespace TvPlugin
{
  public class TVTuningDetails : GUIWindow
  {
    public TVTuningDetails()
    {
      GetID = (int) Window.WINDOW_TV_TUNING_DETAILS;
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
          Log.Error("Error loading TuningDetails /  HasCiMenuSupport:" +ex.StackTrace);
        }

        IList<TuningDetail> details = chan.ReferringTuningDetail();
        if (details.Count > 0)
        {
          TuningDetail detail = details[0];
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Band", detail.Band.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.BandWidth", detail.Bandwidth.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.ChannelType", detail.ChannelType.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.CountryId", detail.CountryId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.FreeToAir", detail.FreeToAir.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Frequency", detail.Frequency.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.InnerFecRate", detail.InnerFecRate.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Modulation", detail.Modulation.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.NetworkId", detail.NetworkId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.PcrPid", detail.PcrPid.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.PmtPid", detail.PmtPid.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.AudioPid", detail.AudioPid.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.VideoPid", detail.VideoPid.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Polarisation", detail.Polarisation.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.Provider", detail.Provider);
          GUIPropertyManager.SetProperty("#TV.TuningDetails.ServiceId", detail.ServiceId.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.SymbolRate", detail.Symbolrate.ToString());
          GUIPropertyManager.SetProperty("#TV.TuningDetails.TransportId", detail.TransportId.ToString());
        }
      }
    }

    public override void Process()
    {
      GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalLevel", TVHome.Card.SignalLevel.ToString());
      GUIPropertyManager.SetProperty("#TV.TuningDetails.SignalQuality", TVHome.Card.SignalQuality.ToString());
    }

    #endregion
  }
}