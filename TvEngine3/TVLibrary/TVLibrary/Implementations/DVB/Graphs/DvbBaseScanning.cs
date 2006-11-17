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
  public class DvbBaseScanning : IHardwarePidFiltering
  {
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
    #region consts
    const int ScanMaxChannels = 10;
    #endregion

    //#region imports
    //[DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    //protected static extern int SetupDemuxerPin(IPin pin, int pid, int elementaryStream, bool unmapOtherPins);
    //[DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    //protected static extern int DumpMpeg2DemuxerMappings(IBaseFilter filter);
    //#endregion

    #region variables
    ITsChannelScan _analyzer;
    ITVCard _card;
    List<ushort> _scanPidList = new List<ushort>();
    //bool _isAtsc = false;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="T:DvbBaseScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public DvbBaseScanning(ITVCard card)
    {
      _card = card;
    }
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


    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      if (_analyzer == null) return;
      //_analyzer.SetPidFilterCallback(null);
    }

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
      bool locked = (_card.IsTunerLocked);
      TimeSpan tsUpdate = DateTime.Now - startTime;
      Log.Log.WriteFile("Scan: signal status update took {0} msec", tsUpdate.TotalMilliseconds);
      if (tsUpdate.TotalMilliseconds > 2000)
      {
        //getting signal status takes a looong time due to card driver issue
        //so we do a simple check
        if (!_card.IsTunerLocked && _card.SignalQuality == 0 && _card.SignalLevel == 0)
        {
          System.Threading.Thread.Sleep(10000);
          ResetSignalUpdate();
        }
        if (!_card.IsTunerLocked && _card.SignalQuality == 0 && _card.SignalLevel == 0)
        {
          Log.Log.WriteFile("Scan: no signal detected");
          return new List<IChannel>();
        }
        Log.Log.WriteFile("Scan: signal detected.");
      }
      else
      {
        //getting signal status seems fast so we do a more extensive check
        startTime = DateTime.Now;
        while (true)
        {
          ResetSignalUpdate();
          if (_card.IsTunerLocked)
          {
            break;
          }
          TimeSpan ts = DateTime.Now - startTime;
          if (ts.TotalMilliseconds >= 2000) break;
          System.Threading.Thread.Sleep(100);
        }

        if (_card.IsTunerLocked == false)
        {
          ResetSignalUpdate();
          if (_card.SignalQuality == 0 && _card.SignalLevel == 0)
          {
            Log.Log.WriteFile("Scan! no signal detected: locked:{0} signal level:{1} signal quality:{2}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);
            return new List<IChannel>();
          }
        }
        if (_card.IsTunerLocked || _card.SignalQuality > 0 || _card.SignalLevel > 0)
        {
          Log.Log.WriteFile("Signal detected, wait for good signal quality");
          startTime = DateTime.Now;
          while (true)
          {
            System.Threading.Thread.Sleep(100);
            ResetSignalUpdate();
            if (_card.SignalQuality >= 30) break;
            TimeSpan ts = DateTime.Now - startTime;
            if (ts.TotalMilliseconds >= 2000) break;
          }
        }
      }

      try
      {
        _analyzer.Start();
        //Log.Log.WriteFile("Tuner locked:{0} signal level:{1} signal quality:{2}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);
        startTime = DateTime.Now;
        short channelCount;
        bool yesNo;
        while (true)
        {
          System.Threading.Thread.Sleep(100);
          _analyzer.IsReady(out yesNo);
          if (yesNo) break;
          TimeSpan ts = DateTime.Now - startTime;
          if (ts.TotalMilliseconds > 13000) break;
        }
        _analyzer.GetCount(out channelCount);
        if (channelCount == 0)
        {
          _analyzer.GetCount(out channelCount);
          Log.Log.WriteFile("Scan! timeout...found no channels");
          return new List<IChannel>();
        }
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
        short ac3Pid;
        IntPtr audioLanguage1;
        IntPtr audioLanguage2;
        IntPtr audioLanguage3;
        IntPtr subtitleLanguage;
        short teletextPid;
        short subtitlePid;
        string strAudioLanguage1 = "";
        string strAudioLanguage2 = "";
        string strAudioLanguage3 = "";
        short videoStreamType;
        int found = 0;
        short lcn = -1;
        _analyzer.GetCount(out channelCount);
        bool[] channelFound = new bool[channelCount];
        List<IChannel> channelsFound = new List<IChannel>();
        startTime = DateTime.Now;
        //while (true)
        {
          for (int i = 0; i < channelCount; ++i)
          {
            //if (channelFound[i]) continue;
            networkId = 0;
            transportId = 0;
            serviceId = 0;
            _analyzer.GetChannel((short)i,
                  out networkId, out transportId, out serviceId, out majorChannel, out minorChannel,
                  out frequency, out lcn, out EIT_schedule_flag, out EIT_present_following_flag, out runningStatus,
                  out freeCAMode, out serviceType, out modulation, out providerName, out serviceName,
                  out pcrPid, out pmtPid, out videoPid, out audio1Pid, out audio2Pid, out audio3Pid,
                  out ac3Pid, out  audioLanguage1, out audioLanguage2, out audioLanguage3, out teletextPid, out subtitlePid, out subtitleLanguage, out videoStreamType);
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
              channelFound[i] = true;
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
              info.network_pmt_PID = pmtPid;

              strAudioLanguage1 = Marshal.PtrToStringAnsi(audioLanguage1);
              strAudioLanguage2 = Marshal.PtrToStringAnsi(audioLanguage2);
              strAudioLanguage3 = Marshal.PtrToStringAnsi(audioLanguage3);

              if (videoPid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.VideoPid(videoPid, videoStreamType);
                info.AddPid(pidInfo);
              }
              if (audio1Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.AudioPid(audio1Pid, strAudioLanguage1);
                info.AddPid(pidInfo);
              }
              if (audio2Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.AudioPid(audio2Pid, strAudioLanguage2);
                info.AddPid(pidInfo);
              }
              if (audio3Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.AudioPid(audio3Pid, strAudioLanguage3);
                info.AddPid(pidInfo);
              }
              if (ac3Pid > 0)
              {
                PidInfo pidInfo = new PidInfo();
                pidInfo.Ac3Pid(ac3Pid, "");
                info.AddPid(pidInfo);
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
              if (!isTvRadioChannel)
              {
                Log.Log.Write("Found Unknown: {0} {1} type:{2} onid:{3:X} tsid:{4:X} sid:{5:X}",
                  info.service_provider_name, info.service_name, info.serviceType, info.networkID, info.transportStreamID, info.serviceID);
              }
            }
            if ((i % 10) == 0)
            {
              System.Threading.Thread.Sleep(50);
            }
          }
          //if (found >= channelCount) break;
          // TimeSpan ts = DateTime.Now - startTime;
          // if (ts.TotalMilliseconds > 4000) break;
         // System.Threading.Thread.Sleep(100);
        } // while true
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
          _analyzer.Stop();
        }
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
  }
}
