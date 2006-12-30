/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;


using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using DirectShowLib;
using DirectShowLib.BDA;


namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// base class for scanning DVB tv/radio channels
  /// </summary>
  public class DvbBaseScanning : IHardwarePidFiltering, IChannelScanCallback
  {
    #region enums
    /// <summary>
    /// Different stream service type
    /// </summary>
    public enum ServiceType
    {
      /// <summary>
      /// Service contains video
      /// </summary>
      Video = 1,
      /// <summary>
      /// Service contains audio
      /// </summary>
      Audio = 2,
      /// <summary>
      /// Service contains video in MPEG-4
      /// </summary>
      Mpeg4Stream = 0x11,
      /// <summary>
      /// Service contains video in H.264
      /// </summary>
      H264Stream = 0x1b,
      /// <summary>
      /// Service contains video in H.264
      /// </summary>
      Mpeg4OrH264Stream = 134
    }
    #endregion

    #region consts
    const int ScanMaxChannels = 10;
    #endregion


    #region variables
    ITsChannelScan _analyzer;
    ITVCard _card;
    List<ushort> _scanPidList = new List<ushort>();
    ManualResetEvent _event;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="T:DvbBaseScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public DvbBaseScanning(ITVCard card)
    {
      _card = card;
    }
    #endregion

    #region virtual members
    /// <summary>
    /// Sets the hw pids.
    /// </summary>
    /// <param name="pids">The pids.</param>
    protected virtual void SetHwPids(ArrayList pids)
    {
    }
    /// <summary>
    /// Gets the analyzer.
    /// </summary>
    /// <returns></returns>
    protected virtual ITsChannelScan GetAnalyzer()
    {
      return null;
    }
    /// <summary>
    /// Gets the pin analyzer SI.
    /// </summary>
    /// <value>The pin analyzer SI.</value>
    protected virtual IPin PinAnalyzerSI
    {
      get
      {
        return null;
      }
    }
    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <returns></returns>
    protected virtual IChannel CreateNewChannel(ChannelInfo info)
    {
      return null;
    }
    /// <summary>
    /// Resets the signal update.
    /// </summary>
    protected virtual void ResetSignalUpdate()
    {

    }
    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
    }
    #endregion

    #region IDisposable
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      if (_analyzer == null) return;
      //_analyzer.SetPidFilterCallback(null);
    }
    #endregion

    /// <summary>
    /// Scans the specified transponder.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public List<IChannel> Scan(IChannel channel)
    {
      _card.IsScanning = true;
      _card.TuneScan(channel);
      _analyzer = GetAnalyzer();
      if (_analyzer == null)
      {
        Log.Log.WriteFile("Scan: no analyzer interface available");
        return new List<IChannel>();
      }
      _card.IsScanning = false;
      DateTime startTime = DateTime.Now;
      ResetSignalUpdate();
      Log.Log.WriteFile("Scan: tuner locked:{0} signal:{1} quality:{2}", _card.IsTunerLocked, _card.SignalLevel , _card.SignalQuality);
      if (_card.IsTunerLocked || _card.SignalLevel > 0 || _card.SignalQuality > 0)
      {
        try
        {
          _event = new ManualResetEvent(false);

          _analyzer.SetCallBack(this);
          _analyzer.Start();
          startTime = DateTime.Now;

          _event.WaitOne(10000, true);

          short networkId;
          short transportId;
          short serviceId;
          short majorChannel;
          short minorChannel;
          short frequency;
          short EIT_schedule_flag;
          short EIT_present_following_flag;
          short runningStatus;
          short freeCAMode;
          short serviceType;
          short modulation;
          IntPtr providerName;
          IntPtr serviceName;
          short pcrPid;
          short pmtPid;
          short videoPid;
          short audio1Pid;
          short audio2Pid;
          short audio3Pid;
          short audio4Pid;
          short audio5Pid;
          short ac3Pid;
          IntPtr audioLanguage1;
          IntPtr audioLanguage2;
          IntPtr audioLanguage3;
          IntPtr audioLanguage4;
          IntPtr audioLanguage5;
          IntPtr subtitleLanguage;
          short teletextPid;
          short subtitlePid;
          string strAudioLanguage1 = "";
          string strAudioLanguage2 = "";
          string strAudioLanguage3 = "";
          string strAudioLanguage4 = "";
          string strAudioLanguage5 = "";
          short videoStreamType;
          int found = 0;
          short lcn = -1;
          short channelCount;
          _analyzer.GetCount(out channelCount);
          bool[] channelFound = new bool[channelCount];
          List<IChannel> channelsFound = new List<IChannel>();
          startTime = DateTime.Now;

          for (int i = 0; i < channelCount; ++i)
          {
            networkId = 0;
            transportId = 0;
            serviceId = 0;
            _analyzer.GetChannel((short)i,
                  out networkId, out transportId, out serviceId, out majorChannel, out minorChannel,
                  out frequency, out lcn, out EIT_schedule_flag, out EIT_present_following_flag, out runningStatus,
                  out freeCAMode, out serviceType, out modulation, out providerName, out serviceName,
                  out pcrPid, out pmtPid, out videoPid, out audio1Pid, out audio2Pid, out audio3Pid, out audio4Pid, out audio5Pid,
                  out ac3Pid, out  audioLanguage1, out audioLanguage2, out audioLanguage3, out audioLanguage4, out audioLanguage5, 
                  out teletextPid, out subtitlePid, out subtitleLanguage, out videoStreamType);
            bool isValid = ((networkId != 0 || transportId != 0 || serviceId != 0) && pmtPid != 0);
            string name = Marshal.PtrToStringAnsi(serviceName);
            //Log.Log.Write("{0}) 0x{1:X} 0x{2:X} 0x{3:X} 0x{4:X} {5} v:{6:X} a:{7:X} ac3:{8:X} type:{9:X}", 
            //  i, networkId, transportId, serviceId, pmtPid, name,videoPid,audio1Pid,ac3Pid, serviceType);
            if (videoStreamType == 0x10 || videoStreamType == 0x1b)
            {
              Log.Log.WriteFile("H264/MPEG4!");
            }
            if ((channel as ATSCChannel) != null)
            {
              isValid = (majorChannel != 0 && minorChannel != 0);
            }
            if (isValid)
            {
              found++;
              ChannelInfo info = new ChannelInfo();
              info.networkID = networkId;
              info.transportStreamID = transportId;
              info.serviceID = serviceId;
              info.majorChannel = majorChannel;
              info.minorChannel = minorChannel;
              info.freq = frequency;
              info.LCN = lcn;
              info.eitSchedule = (EIT_schedule_flag != 0);
              info.eitPreFollow = (EIT_present_following_flag != 0);
              info.serviceType = serviceType;
              info.modulation = modulation;
              info.service_provider_name = Marshal.PtrToStringAnsi(providerName);
              info.service_name = Marshal.PtrToStringAnsi(serviceName);
              info.pcr_pid = pcrPid;
              info.scrambled = (freeCAMode != 0);
              
              info.network_pmt_PID = pmtPid;

              strAudioLanguage1 = Marshal.PtrToStringAnsi(audioLanguage1);
              strAudioLanguage2 = Marshal.PtrToStringAnsi(audioLanguage2);
              strAudioLanguage3 = Marshal.PtrToStringAnsi(audioLanguage3);
              strAudioLanguage4 = Marshal.PtrToStringAnsi(audioLanguage4);
              strAudioLanguage5 = Marshal.PtrToStringAnsi(audioLanguage5);

              bool hasVideo = false;
              bool hasAudio = false;
              if (videoPid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.VideoPid(videoPid, videoStreamType);
                info.AddPid(pidInfo);
                hasVideo = true;
              }
              if (audio1Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.AudioPid(audio1Pid, strAudioLanguage1);
                info.AddPid(pidInfo);
                hasAudio = true;
              }
              if (audio2Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.AudioPid(audio2Pid, strAudioLanguage2);
                info.AddPid(pidInfo);
                hasAudio = true;
              }
              if (audio3Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.AudioPid(audio3Pid, strAudioLanguage3);
                info.AddPid(pidInfo);
                hasAudio = true;
              }
              if (audio4Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.AudioPid(audio4Pid, strAudioLanguage4);
                info.AddPid(pidInfo);
                hasAudio = true;
              }
              if (audio5Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.AudioPid(audio5Pid, strAudioLanguage5);
                info.AddPid(pidInfo);
                hasAudio = true;
              }
              if (ac3Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.Ac3Pid(ac3Pid, "");
                info.AddPid(pidInfo);
                hasAudio = true;
              }
              if (teletextPid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.TeletextPid(teletextPid);
                info.AddPid(pidInfo);
              }
              if (subtitlePid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.SubtitlePid(subtitlePid);
                info.AddPid(pidInfo);
              }
              startTime = DateTime.Now;
              bool isTvRadioChannel = false;
              if (info.serviceType == (int)ServiceType.Video || info.serviceType == (int)ServiceType.Mpeg4Stream ||
                  info.serviceType == (int)ServiceType.Audio || info.serviceType == (int)ServiceType.H264Stream ||
                  info.serviceType == (int)ServiceType.Mpeg4OrH264Stream)
              {
                IChannel dvbChannel = CreateNewChannel(info);
                if (dvbChannel != null)
                {
                  channelsFound.Add(dvbChannel);
                  isTvRadioChannel = true;
                }
              }
              else if (hasVideo || hasAudio)
              {
                if (hasVideo)
                  info.serviceType = (int)ServiceType.Video;
                else
                  info.serviceType = (int)ServiceType.Audio;
                IChannel dvbChannel = CreateNewChannel(info);
                if (dvbChannel != null)
                {
                  channelsFound.Add(dvbChannel);
                  isTvRadioChannel = true;
                }

              }
              if (!isTvRadioChannel)
              {
                Log.Log.Write("Found Unknown: {0} {1} type:{2} onid:{3:X} tsid:{4:X} sid:{5:X}",
                  info.service_provider_name, info.service_name, info.serviceType, info.networkID, info.transportStreamID, info.serviceID);
              }
            }
          }
          if (found != channelCount)
            Log.Log.Write("Scan! Got {0} from {1} channels", found, channelCount);
          else
            Log.Log.Write("Scan Got {0} from {1} channels", found, channelCount);
          return channelsFound;
        }
        finally
        {
          if (_analyzer != null)
          {
            _analyzer.SetCallBack(null);
            _analyzer.Stop();
          }
          _event.Close();
        }
      }
      else
      {
        Log.Log.WriteFile("Scan: no signal detected");
        return new List<IChannel>();
      }
    }
    /// <summary>
    /// Filters the pids.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="pids">The pids.</param>
    /// <returns></returns>
    public int FilterPids(short count, IntPtr pids)
    {
      return 0;
    }

    #region IChannelScanCallback Members

    /// <summary>
    /// Called when [scanner done].
    /// </summary>
    /// <returns></returns>
    public int OnScannerDone()
    {
      _event.Set();
      return 0;
    }

    #endregion
  }
}
