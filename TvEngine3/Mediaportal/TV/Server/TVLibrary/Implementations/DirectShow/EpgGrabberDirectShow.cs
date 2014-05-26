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

using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  /// <summary>
  /// An implementation of <see cref="IEpgGrabber"/> for tuners that utilise the MediaPortal TS
  /// writer/analyser DirectShow filter.
  /// </summary>
  internal class EpgGrabberDirectShow : IEpgGrabber, IEpgCallBack
  {
    #region variables

    /// <summary>
    /// Indicator: is the grabber grabbing electronic programme guide data?
    /// </summary>
    private bool _isEpgGrabbing = false;

    /// <summary>
    /// Transponders often carry data for all provider channels, but it may
    /// only be now/next data. Should we store that extended data?
    /// </summary>
    private bool _storeOnlyDataForCurrentTransponder = false;

    /// <summary>
    /// A delegate to notify about EPG grabbing progress.
    /// </summary>
    private IEpgGrabberCallBack _epgGrabberCallBack = null;

    /// <summary>
    /// The TS writer/analyser EPG scanning interface.
    /// </summary>
    private ITsEpgScanner _epgScanner = null;

    /// <summary>
    /// The current transponder/multiplex tuning details.
    /// </summary>
    private IChannel _currentTuningDetail = null;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="EpgGrabberDirectShow"/> class.
    /// </summary>
    /// <param name="epgScanner">The TS writer/analyser EPG scanner interface.</param>
    public EpgGrabberDirectShow(ITsEpgScanner epgScanner)
    {
      if (epgScanner == null)
      {
        throw new ArgumentException("EPG scanner is null");
      }
      _epgScanner = epgScanner;
      ReloadConfiguration();
    }

    private static int HexDigitsToDecimal(byte val)
    {
      // Each nibble (4 bits) in the byte is a digit.
      if ((val & 0xf0) >= 0xf0 || (val & 0xf) >= 0xa)
      {
        return 0;
      }
      return (((val & 0xf0) >> 4) * 10) + (val & 0xf);
    }

    /// <summary>
    /// Retrieve, translate and collate the raw EPG data into a standard format.
    /// </summary>
    private IList<EpgChannel> CollectEpgData()
    {
      try
      {
        bool dvbReady, mhwReady;
        _epgScanner.IsEPGReady(out dvbReady);
        _epgScanner.IsMHWReady(out mhwReady);
        if (dvbReady == false || mhwReady == false)
        {
          // This should never happen!
          this.LogWarn("DirectShow EPG: not ready");
          return null;
        }
        uint titleCount;
        uint channelCount;
        _epgScanner.GetMHWTitleCount(out titleCount);
        mhwReady = titleCount > 10;
        _epgScanner.GetEPGChannelCount(out channelCount);
        dvbReady = channelCount > 0;
        List<EpgChannel> epgChannels = new List<EpgChannel>();
        this.LogDebug("DirectShow EPG: ready, EIT channel(s) = {0}, MHW title(s) = {1}", channelCount, titleCount);
        if (mhwReady)
        {
          _epgScanner.GetMHWTitleCount(out titleCount);
          for (int i = 0; i < titleCount; ++i)
          {
            uint id = 0;
            uint programid = 0;
            uint transportid = 0, networkid = 0, channelnr = 0, channelid = 0, themeid = 0, PPV = 0, duration = 0;
            byte summaries = 0;
            uint datestart = 0, timestart = 0;
            uint tmp1 = 0, tmp2 = 0;
            IntPtr ptrTitle, ptrProgramName;
            IntPtr ptrChannelName, ptrSummary, ptrTheme;
            _epgScanner.GetMHWTitle((ushort)i, ref id, ref tmp1, ref tmp2, ref channelnr, ref programid,
                                              ref themeid, ref PPV, ref summaries, ref duration, ref datestart,
                                              ref timestart, out ptrTitle, out ptrProgramName);
            _epgScanner.GetMHWChannel(channelnr, ref channelid, ref networkid, ref transportid,
                                                out ptrChannelName);
            _epgScanner.GetMHWSummary(programid, out ptrSummary);
            _epgScanner.GetMHWTheme(themeid, out ptrTheme);
            string channelName = DvbTextConverter.Convert(ptrChannelName);
            string title = DvbTextConverter.Convert(ptrTitle);
            string summary = DvbTextConverter.Convert(ptrSummary);
            string theme = DvbTextConverter.Convert(ptrTheme);
            if (channelName == null)
              channelName = string.Empty;
            if (title == null)
              title = string.Empty;
            if (summary == null)
              summary = string.Empty;
            if (theme == null)
              theme = string.Empty;
            channelName = channelName.Trim();
            title = title.Trim();
            summary = summary.Trim();
            theme = theme.Trim();
            EpgChannel epgChannel = null;
            foreach (EpgChannel chan in epgChannels)
            {
              DVBBaseChannel dvbChan = (DVBBaseChannel)chan.Channel;
              if (dvbChan.NetworkId == networkid && dvbChan.TransportId == transportid &&
                  dvbChan.ServiceId == channelid)
              {
                epgChannel = chan;
                break;
              }
            }
            if (epgChannel == null)
            {
              // We need to use a matching channel type per card, because tuning details will be looked up with cardtype as filter.
              DVBBaseChannel channel = (DVBBaseChannel)_currentTuningDetail.Clone();
              channel.NetworkId = (int)networkid;
              channel.TransportId = (int)transportid;
              channel.ServiceId = (int)channelid;
              epgChannel = new EpgChannel { Channel = channel };
              //this.LogInfo("dvb: start filtering channel NID {0} TID {1} SID{2}", dvbChan.NetworkId, dvbChan.TransportId, dvbChan.ServiceId);
              if (FilterOutEPGChannel((ushort)networkid, (ushort)transportid, (ushort)channelid) == false)
              {
                //this.LogInfo("dvb: Not Filtered channel NID {0} TID {1} SID{2}", dvbChan.NetworkId, dvbChan.TransportId, dvbChan.ServiceId);
                epgChannels.Add(epgChannel);
              }
            }
            uint d1 = datestart;
            uint m = timestart & 0xff;
            uint h1 = (timestart >> 16) & 0xff;
            DateTime dayStart = DateTime.Now;
            dayStart =
              dayStart.Subtract(new TimeSpan(1, dayStart.Hour, dayStart.Minute, dayStart.Second, dayStart.Millisecond));
            int day = (int)dayStart.DayOfWeek;
            DateTime programStartTime = dayStart;
            int minVal = (int)((d1 - day) * 86400 + h1 * 3600 + m * 60);
            if (minVal < 21600)
              minVal += 604800;
            programStartTime = programStartTime.AddSeconds(minVal);
            EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));
            EpgLanguageText epgLang = new EpgLanguageText("ALL", title, summary, theme, 0, string.Empty, -1);
            program.Text.Add(epgLang);
            epgChannel.Programs.Add(program);
          }
          for (int i = 0; i < epgChannels.Count; ++i)
          {
            epgChannels[i].Sort();
          }
          return epgChannels;
        }

        if (dvbReady)
        {
          ushort networkid = 0;
          ushort transportid = 0;
          ushort serviceid = 0;
          for (uint x = 0; x < channelCount; ++x)
          {
            _epgScanner.GetEPGChannel(x, ref networkid, ref transportid, ref serviceid);
            // We need to use a matching channel type per card, because tuning details will be looked up with cardtype as filter.
            DVBBaseChannel channel = (DVBBaseChannel)_currentTuningDetail.Clone();
            channel.NetworkId = networkid;
            channel.TransportId = transportid;
            channel.ServiceId = serviceid;
            EpgChannel epgChannel = new EpgChannel { Channel = channel };
            uint eventCount;
            _epgScanner.GetEPGEventCount(x, out eventCount);
            for (uint i = 0; i < eventCount; ++i)
            {
              uint start_time_MJD, start_time_UTC, duration, languageCount;
              string title, description;
              IntPtr ptrGenre;
              int starRating;
              IntPtr ptrClassification;

              _epgScanner.GetEPGEvent(x, i, out languageCount, out start_time_MJD, out start_time_UTC,
                                                out duration, out ptrGenre, out starRating, out ptrClassification);
              string genre = DvbTextConverter.Convert(ptrGenre);
              string classification = DvbTextConverter.Convert(ptrClassification);

              if (starRating < 1 || starRating > 7)
                starRating = 0;

              int duration_hh = HexDigitsToDecimal((byte)((duration >> 16) & 0xff));
              int duration_mm = HexDigitsToDecimal((byte)((duration >> 8) & 0xff));
              int duration_ss = 0; //HexDigitsToDecimal((byte)(duration & 0xff));
              int starttime_hh = HexDigitsToDecimal((byte)((start_time_UTC >> 16) & 0xff));
              int starttime_mm = HexDigitsToDecimal((byte)((start_time_UTC >> 8) & 0xff));
              int starttime_ss = 0; //HexDigitsToDecimal((byte)(start_time_UTC & 0xff));

              if (starttime_hh > 23)
                starttime_hh = 23;
              if (starttime_mm > 59)
                starttime_mm = 59;
              if (starttime_ss > 59)
                starttime_ss = 59;

              // DON'T ENABLE THIS. Some entries can be indeed >23 Hours !!!
              //if (duration_hh > 23) duration_hh = 23;
              if (duration_mm > 59)
                duration_mm = 59;
              if (duration_ss > 59)
                duration_ss = 59;

              // convert the julian date
              int year = (int)((start_time_MJD - 15078.2) / 365.25);
              int month = (int)((start_time_MJD - 14956.1 - (int)(year * 365.25)) / 30.6001);
              int day = (int)(start_time_MJD - 14956 - (int)(year * 365.25) - (int)(month * 30.6001));
              int k = (month == 14 || month == 15) ? 1 : 0;
              year += 1900 + k; // start from year 1900, so add that here
              month = month - 1 - k * 12;
              int starttime_y = year;
              int starttime_m = month;
              int starttime_d = day;
              if (year < 2000)
                continue;

              try
              {
                DateTime dtUTC = new DateTime(starttime_y, starttime_m, starttime_d, starttime_hh, starttime_mm,
                                              starttime_ss, 0);
                DateTime dtStart = dtUTC.ToLocalTime();
                if (dtStart < DateTime.Now.AddDays(-1) || dtStart > DateTime.Now.AddMonths(2))
                  continue;
                DateTime dtEnd = dtStart.AddHours(duration_hh);
                dtEnd = dtEnd.AddMinutes(duration_mm);
                dtEnd = dtEnd.AddSeconds(duration_ss);
                EpgProgram epgProgram = new EpgProgram(dtStart, dtEnd);
                //EPGEvent newEvent = new EPGEvent(genre, dtStart, dtEnd);
                for (int z = 0; z < languageCount; ++z)
                {
                  uint languageId;
                  IntPtr ptrTitle;
                  IntPtr ptrDesc;
                  int parentalRating;
                  _epgScanner.GetEPGLanguage(x, i, (uint)z, out languageId, out ptrTitle, out ptrDesc,
                                                      out parentalRating);
                  string language = string.Empty;
                  language += (char)((languageId >> 16) & 0xff);
                  language += (char)((languageId >> 8) & 0xff);
                  language += (char)((languageId) & 0xff);
                  title = DvbTextConverter.Convert(ptrTitle);
                  description = DvbTextConverter.Convert(ptrDesc);
                  if (title == null)
                    title = string.Empty;
                  if (description == null)
                    description = string.Empty;
                  if (string.IsNullOrEmpty(language))
                    language = string.Empty;
                  if (genre == null)
                    genre = string.Empty;
                  if (classification == null)
                    classification = string.Empty;
                  title = title.Trim();
                  description = description.Trim();
                  language = language.Trim();
                  genre = genre.Trim();
                  EpgLanguageText epgLangague = new EpgLanguageText(language, title, description, genre, starRating,
                                                                    classification, parentalRating);
                  epgProgram.Text.Add(epgLangague);
                }
                epgChannel.Programs.Add(epgProgram);
              }
              catch (Exception ex)
              {
                this.LogError(ex);
              }
            } //for (uint i = 0; i < eventCount; ++i)
            if (epgChannel.Programs.Count > 0)
            {
              epgChannel.Sort();
              //this.LogInfo("dvb: start filtering channel NID {0} TID {1} SID{2}", chan.NetworkId, chan.TransportId, chan.ServiceId);
              if (FilterOutEPGChannel(networkid, transportid, serviceid) == false)
              {
                //this.LogInfo("dvb: Not Filtered channel NID {0} TID {1} SID{2}", chan.NetworkId, chan.TransportId, chan.ServiceId);
                epgChannels.Add(epgChannel);
              }
            }
          } //for (uint x = 0; x < channelCount; ++x)
        }
        return epgChannels;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        return new List<EpgChannel>();
      }
      finally
      {
        // free the epg infos in TsWriter so that the mem used gets released 
        _epgScanner.Reset();
      }
    }

    /// <summary>
    /// Check if the EPG data found in a scan should not be kept.
    /// </summary>
    /// <remarks>
    /// This function implements the logic to filter out data for services that are not on the same transponder.
    /// </remarks>
    /// <value><c>false</c> if the data should be kept, otherwise <c>true</c></value>
    private bool FilterOutEPGChannel(ushort networkId, ushort transportStreamId, ushort serviceId)
    {
      if (!_storeOnlyDataForCurrentTransponder)
      {
        return false;
      }

      // The following code attempts to find a tuning detail for the tuner type (eg. a DVB-T tuning detail for
      // a DVB-T tuner), and check if that tuning detail corresponds with the same transponder that the EPG was
      // collected from (ie. the transponder that the tuner is currently tuned to). This logic will potentially
      // fail for people that merge HD and SD tuning details that happen to be for the same tuner type.
      Channel dbchannel = ChannelManagement.GetChannelByTuningDetail(networkId, transportStreamId, serviceId);
      if (dbchannel == null)
      {
        return false;
      }
      foreach (TuningDetail detail in dbchannel.TuningDetails)
      {
        IChannel channel = ChannelManagement.GetTuningChannel(detail);
        if (_currentTuningDetail.GetType() == channel.GetType() && !_currentTuningDetail.IsDifferentTransponder(channel))
        {
          return false;
        }
      }
      return true;
    }

    #region IEpgCallBack member

    /// <summary>
    /// TS writer grab completion call back.
    /// </summary>
    /// <returns></returns>
    public int OnEpgReceived()
    {
      this.LogDebug("DirectShow EPG: EPG received");
      if (_epgGrabberCallBack != null)
      {
        // TODO Start thread to collect EPG data => avoid timeshifting EPG grabber glitch?
        _epgGrabberCallBack.OnEpgReceived(CollectEpgData());
      }
      _isEpgGrabbing = false;
      return 0;
    }

    #endregion

    #region IEpgGrabber members

    /// <summary>
    /// Reload the controller's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("DirectShow EPG: reload configuration");
      _storeOnlyDataForCurrentTransponder = SettingsManagement.GetValue("generalGrapOnlyForSameTransponder", false);
    }

    /// <summary>
    /// Start grabbing electronic programme guide data.
    /// </summary>
    /// <param name="tuningDetail">The current transponder/multiplex tuning details.</param>
    /// <param name="callBack">The delegate to notify when grabbing is complete or canceled.</param>
    public void GrabEpg(IChannel tuningDetail, IEpgGrabberCallBack callBack)
    {
      this.LogDebug("DirectShow EPG: grab EPG");
      _currentTuningDetail = tuningDetail;
      _epgGrabberCallBack = callBack;
      _epgScanner.Reset();
      _epgScanner.SetCallBack(this);
      _epgScanner.GrabEPG();
      _epgScanner.GrabMHW();
      _isEpgGrabbing = true;
    }

    /// <summary>
    /// Get the grabber's current status.
    /// </summary>
    /// <value><c>true</c> if the grabber is grabbing, otherwise <c>false</c></value>
    public bool IsEpgGrabbing
    {
      get
      {
        return _isEpgGrabbing;
      }
    }

    /// <summary>
    /// Abort grabbing electronic programme guide data.
    /// </summary>
    public void AbortGrabbing()
    {
      this.LogDebug("DirectShow EPG: abort grabbing");
      _epgScanner.AbortGrabbing();
      if (_epgGrabberCallBack != null)
      {
        _epgGrabberCallBack.OnEpgCancelled();
      }
      _isEpgGrabbing = false;
    }

    #endregion
  }
}