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
  public class DvbBaseScanning : IHardwarePidFiltering
  {
    public enum ServiceType
    {
      Video = 1,
      Audio = 2,
      Mpeg4Stream = 0x11,
      H264Stream = 0x1b
    }
    #region consts
    const int ScanMaxChannels = 10;
    #endregion

    #region imports
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int SetupDemuxerPin(IPin pin, int pid, int elementaryStream, bool unmapOtherPins);
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int DumpMpeg2DemuxerMappings(IBaseFilter filter);

    #endregion

    #region variables
    ITsChannelScan _analyzer;
    ITVCard _card;
    List<ushort> _scanPidList = new List<ushort>();
    bool _isAtsc = false;
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
      _card.IsScanning = false;

      DateTime startTime = DateTime.Now;
      while (true)
      {
        System.Threading.Thread.Sleep(100);
        ResetSignalUpdate();
        if (_card.IsTunerLocked) break;
        TimeSpan ts = DateTime.Now - startTime;
        if (ts.TotalMilliseconds >= 2000) break;
      }
      if (_card.IsTunerLocked == false)
      {
        if ((channel as ATSCChannel) == null)
        {
          Log.Log.WriteFile("Scan! no signal detected: locked:{0} signal level:{1} signal quality:{2}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);
          return null;
        }
      }
      try
      {
        _analyzer.Start();
        if (_card.IsTunerLocked || _card.SignalQuality>0 || _card.SignalLevel>0)
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
        Log.Log.WriteFile("Tuner locked:{0} signal level:{1} signal quality:{2}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);
        startTime = DateTime.Now;
        short channelCount;
        while (true)
        {
          System.Threading.Thread.Sleep(100);
          _analyzer.GetCount(out channelCount);
          if (channelCount > 0) break;
          TimeSpan ts = DateTime.Now - startTime;
          if (ts.TotalMilliseconds > 2000) break;
        }
        if (channelCount == 0)
        {
          _analyzer.GetCount(out channelCount);
          Log.Log.WriteFile("Scan! timeout...found no channels tuner locked:{0} signal level:{1} signal quality:{2} {3}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality, channelCount);
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
        short teletextPid;
        short subtitlePid;
        string strAudioLanguage1 = "";
        string strAudioLanguage2 = "";
        string strAudioLanguage3 = "";
        int found = 0;
        short lcn = -1;
        bool[] channelFound = new bool[channelCount];
        List<IChannel> channelsFound = new List<IChannel>();
        startTime = DateTime.Now;
        while (true)
        {
          for (int i = 0; i < channelCount; ++i)
          {
            if (channelFound[i]) continue;
            networkId = 0;
            transportId = 0;
            serviceId = 0;
            _analyzer.GetChannel((short)i,
                  out networkId, out transportId, out serviceId, out majorChannel, out minorChannel,
                  out frequency, out lcn, out EIT_schedule_flag, out EIT_present_following_flag, out runningStatus,
                  out freeCAMode, out serviceType, out modulation, out providerName, out serviceName,
                  out pcrPid, out pmtPid, out videoPid, out audio1Pid, out audio2Pid, out audio3Pid,
                  out ac3Pid, out  audioLanguage1, out audioLanguage2, out audioLanguage3, out teletextPid, out subtitlePid);
            if ((networkId != 0 || transportId != 0 || serviceId != 0) && pmtPid != 0)
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
                pidInfo.VideoPid(videoPid);
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
                  info.serviceType == (int)ServiceType.Audio || info.serviceType == (int)ServiceType.H264Stream)
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
          if (found >= channelCount) break;
          TimeSpan ts = DateTime.Now - startTime;
          if (ts.TotalMilliseconds > 4000) break;
          System.Threading.Thread.Sleep(100);
        } // while true
        if (found != channelCount)
          Log.Log.Write("Scan! Got {0} from {1} channels", found, channelCount);
        else
          Log.Log.Write("Scan Got {0} from {1} channels", found, channelCount);
        return channelsFound;
      }
      finally
      {
        _analyzer.Stop();
      }
      /*
      try
      {
        if ((channel as ATSCChannel) != null)
          _isAtsc = true;

        _card.IsScanning = true;
        _scanPidList.Clear();
        _analyzer = GetAnalyzer();

        if (_analyzer == null)
        {
          Log.Log.WriteFile("No stream analyzer interface found");
          throw new TvException("No stream analyzer interface found");
        }
        _analyzer.SetPidFilterCallback(this);
        //Log.Log.WriteFile("Scan()...");
        ushort channelCount = 0;


        if (_analyzer == null)
        {
          Log.Log.WriteFile("No stream analyzer interface found");
          throw new TvException("No stream analyzer interface found");
        }
        _card.TuneScan(channel);
        SetupDemuxerPin(PinAnalyzerSI, 0, (int)MediaSampleContent.Mpeg2PSI, true);
        SetupDemuxerPin(PinAnalyzerSI, 0x10, (int)MediaSampleContent.Mpeg2PSI, false);
        SetupDemuxerPin(PinAnalyzerSI, 0x11, (int)MediaSampleContent.Mpeg2PSI, false);
        ArrayList hwpids = new ArrayList();
        hwpids.Add((ushort)0);
        hwpids.Add((ushort)0x10);
        hwpids.Add((ushort)0x11);
        if (_isAtsc)
        {
          SetupDemuxerPin(PinAnalyzerSI, 0x1ffb, (int)MediaSampleContent.Mpeg2PSI, false);
          hwpids.Add((ushort)0x1ffb);
        }
        SetHwPids(hwpids);

        DateTime startTime = DateTime.Now;
        while (true)
        {
          ResetSignalUpdate();
          if (_card.IsTunerLocked) break;
          Application.DoEvents();
          TimeSpan ts = DateTime.Now - startTime;
          if (ts.TotalMilliseconds >= 1000) break;
          System.Threading.Thread.Sleep(50);
          Application.DoEvents();
        }
        if (_card.IsTunerLocked == false)
        {
          Log.Log.WriteFile("Scan no signal detected");
          return null;
        }
        Log.Log.WriteFile("Signal detected, wait for good signal quality");
        startTime = DateTime.Now;
        while (true)
        {
          Application.DoEvents();
          ResetSignalUpdate();
          if (_card.SignalQuality >= 60) break;
          System.Threading.Thread.Sleep(50);
          Application.DoEvents();
          TimeSpan ts = DateTime.Now - startTime;
          if (ts.TotalMilliseconds >= 2000) break;
        }
        Log.Log.WriteFile("Tuner locked:{0} signal level:{1} signal quality:{2}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);

        _scanPidList.Clear();
        _analyzer.ResetParser();
        _analyzer.ResetPids();
        Application.DoEvents();

        startTime = DateTime.Now;
        while (true)
        {
          Application.DoEvents();
          TimeSpan ts = DateTime.Now - startTime;
          _analyzer.GetChannelCount(ref channelCount);
          Application.DoEvents();
          ushort newCount = 0;
          Application.DoEvents();
          _analyzer.GetChannelCount(ref newCount);
          if (channelCount > 0 && newCount == channelCount) break;
          if (ts.TotalMilliseconds > 8000) break;
          Application.DoEvents();
        }
        _analyzer.GetChannelCount(ref channelCount);
        if (channelCount == 0)
        {
          Log.Log.WriteFile("Scan timeout...found no channels tuner locked:{0} signal level:{1} signal quality:{2}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);
          return new List<IChannel>();
        }
        Log.Log.WriteFile("Identified {0} channels", channelCount);


        List<IChannel> channelsFound = new List<IChannel>();
        List<ushort> pids = new List<ushort>();
        bool[] channelFound = new bool[_scanPidList.Count];
        for (int i = 0; i < _scanPidList.Count; ++i)
        {
          pids.Add(_scanPidList[i]);
        }
        for (int i = 0; i < pids.Count; i += ScanMaxChannels)
        {
          Scan(pids, i, ref channelsFound, ref channelFound);
        }
        Log.Log.Write("Got {0} from {1} channels", channelsFound.Count, channelCount);
        return channelsFound;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw ex;
      }
      finally
      {
        if (_analyzer != null)
        {
          _analyzer.SetPidFilterCallback(null);
        }
        _card.IsScanning = false;
      }*/
    }
    /*
    void Scan(List<ushort> pids, int offset, ref List<IChannel> channelsFound, ref bool[] channelFound)
    {
      //clear all pid mappings and map pid 0x0
      int hr = SetupDemuxerPin(PinAnalyzerSI, 0, (int)MediaSampleContent.Mpeg2PSI, true);
      if (hr != 0)
      {
        Log.Log.WriteFile("map pid:{0:X} failed hr:{1:X}", 0x0, hr);
      }

      hr = SetupDemuxerPin(PinAnalyzerSI, 0x10, (int)MediaSampleContent.Mpeg2PSI, false);
      if (hr != 0)
      {
        Log.Log.WriteFile("map pid:{0:X} failed hr:{1:X}", 0x10, hr);
      }

      hr = SetupDemuxerPin(PinAnalyzerSI, 0x11, (int)MediaSampleContent.Mpeg2PSI, false);
      if (hr != 0)
      {
        Log.Log.WriteFile("map pid:{0:X} failed hr:{1:X}", 0x11, hr);
      }

      ArrayList hwpids = new ArrayList();
      hwpids.Add((ushort)0);
      hwpids.Add((ushort)0x10);
      hwpids.Add((ushort)0x11);

      if (_isAtsc)
      {
        SetupDemuxerPin(PinAnalyzerSI, 0x1ffb, (int)MediaSampleContent.Mpeg2PSI, false);
        hwpids.Add((ushort)0x1ffb);
      }

      //map 10 new pids...
      for (int i = offset; i < offset + ScanMaxChannels && i < pids.Count; ++i)
      {
        hwpids.Add((ushort)pids[i]);
        hr = SetupDemuxerPin(PinAnalyzerSI, pids[i], (int)MediaSampleContent.Mpeg2PSI, false);
        if (hr != 0)
        {
          Log.Log.WriteFile("map pid:{0} {1:X} failed hr:{2:X}", i, pids[i], hr);
        }
      }
      SetHwPids(hwpids);
      //PinInfo info;
      //PinAnalyzerSI.QueryPinInfo(out info);
      //DumpMpeg2DemuxerMappings(info.filter);
      //Marshal.ReleaseComObject(info.filter);

      int channelsFoundHere = 0;
      DateTime startTime = DateTime.Now;
      while (true)
      {
        bool allChannelsFound = true;
        for (int index = 0; index < pids.Count; index++)
        {
          Application.DoEvents();
          if (!channelFound[index])
          {
            if (_analyzer.IsChannelReady(index) == 0)
            {
              channelsFoundHere++;
              //Log.Log.Write("{0} ch found:{1} off:{2}", offset, channelsFoundHere, pids.Count);
              startTime = DateTime.Now;
              Application.DoEvents();
              channelFound[index] = true;
              ChannelInfo chi = new ChannelInfo();
              UInt16 len = 0;

              hr = _analyzer.GetCISize(ref len);
              IntPtr mmch = Marshal.AllocCoTaskMem(len);
              try
              {
                hr = _analyzer.GetChannel((UInt16)(index), mmch);
                chi.Decode(mmch);
                if (chi.serviceType == (int)ServiceType.Video || chi.serviceType == (int)ServiceType.Mpeg4Stream || chi.serviceType == (int)ServiceType.Audio || chi.serviceType == (int)ServiceType.H264Stream)
                {
                  channelsFound.Add(CreateNewChannel(chi));
                }
                else
                {
                  Log.Log.Write("Found:{0}:data type:{1} provider:{2} name:{3} {4}", index, chi.serviceType, chi.service_provider_name, chi.service_name, hr);
                }
              }
              finally
              {
                Marshal.FreeCoTaskMem(mmch);
              }
              if (channelsFoundHere == ScanMaxChannels)
              {
                //Log.Log.Write("{0} all 10 found", offset);
                return;
              }
            }//if (_analyzer.IsChannelReady(index) != 1)
            else
            {
              System.Threading.Thread.Sleep(100);
              Application.DoEvents();
              allChannelsFound = false;
            }
          }//if (!channelFound[index])
        }
        TimeSpan ts = DateTime.Now - startTime;
        if (ts.TotalMilliseconds > 8000)
        {
          //Log.Log.Write("{0} timeout", offset);
          return;
        }
        if (allChannelsFound)
        {
          //Log.Log.Write("{0} all found", offset);
          return;
        }
        System.Threading.Thread.Sleep(100);
        Application.DoEvents();
      }
    }

    public int FilterPids(short count, IntPtr pids)
    {
      try
      {
        lock (this)
        {
          string pidsText = String.Empty;
          _scanPidList = new List<ushort>();
          for (int i = 0; i < count; ++i)
          {
            ushort pid = (ushort)Marshal.ReadInt32(pids, i * 4);
            if (pid != 0 && pid != 0x10 && pid != 0x11 && pid != 0x1ffb)
            {
              bool alreadyFound = false;
              for (int x = 0; x < _scanPidList.Count; ++x)
              {
                if (_scanPidList[x] == pid)
                {
                  alreadyFound = true;
                  break;
                }
              }
              if (!alreadyFound)
              {
                _scanPidList.Add(pid);
                pidsText += String.Format("{0:X},", pid);
              }
            }
          }

          Log.Log.WriteFile("Analyzer pids to:{0}", pidsText);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return 0;
    }

    */
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
