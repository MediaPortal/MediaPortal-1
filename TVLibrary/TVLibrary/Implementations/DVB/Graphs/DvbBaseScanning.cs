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
using System;
using System.Collections.Generic;
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
      Mpeg2HDStream = 0x11,
      /// <summary>
      /// Service contains HD video in Mpeg2
      /// </summary>
      H264Stream = 0x1b,
      /// <summary>
      /// Service contains HD video
      /// </summary>
      AdvancedCodecHDVideoStream = 0x19,
      /// <summary>
      /// Service contains video in H.264
      /// </summary>
      Mpeg4OrH264Stream = 134
    }
    #endregion

    #region variables
    ITsChannelScan _analyzer;
    readonly ITVCard _card;
    ManualResetEvent _event;
    /// <summary>
    /// Enable wait for VCT indicator
    /// </summary>
    protected bool _enableWaitForVCT;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="DvbBaseScanning"/> class.
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
    protected virtual void SetHwPids(List<ushort> pids)
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
      if (_analyzer == null)
        return;
      //_analyzer.SetPidFilterCallback(null);
    }
    #endregion

    #region channel scanning
    /// <summary>
    /// Scans the specified transponder.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="settings">The settings.</param>
    /// <returns></returns>
    public List<IChannel> Scan(IChannel channel, ScanParameters settings)
    {
      try
      {
        _card.IsScanning = true;
        _card.Tune(0, channel);
        _analyzer = GetAnalyzer();
        if (_analyzer == null)
        {
          Log.Log.WriteFile("Scan: no analyzer interface available");
          return new List<IChannel>();
        }
        ResetSignalUpdate();
        if (_card.IsTunerLocked == false)
        {
          Thread.Sleep(settings.TimeOutTune * 1000);
          ResetSignalUpdate();
        }
        Log.Log.WriteFile("Scan: tuner locked:{0} signal:{1} quality:{2}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);
        if (_card.IsTunerLocked || _card.SignalLevel > 0 || _card.SignalQuality > 0)
        {
          try
          {
            _event = new ManualResetEvent(false);
            _analyzer.SetCallBack(this);
            _analyzer.Start(_enableWaitForVCT);
            _event.WaitOne(settings.TimeOutSDT * 1000, true);

            int found = 0;
            short channelCount;
            _analyzer.GetCount(out channelCount);
            List<IChannel> channelsFound = new List<IChannel>();

            for (int i = 0; i < channelCount; ++i)
            {
              int networkId;
              int transportId;
              int serviceId;
              short majorChannel;
              short minorChannel;
              short frequency;
              short freeCAMode;
              short serviceType;
              short modulation;
              IntPtr providerName;
              IntPtr serviceName;
              short pmtPid;
              short hasVideo;
              short hasAudio;
              short lcn;
              _analyzer.GetChannel((short)i,
                    out networkId, out transportId, out serviceId, out majorChannel, out minorChannel,
                    out frequency, out lcn, out freeCAMode, out serviceType, out modulation, out providerName, out serviceName,
                    out pmtPid, out hasVideo, out hasAudio);
              bool isValid = ((networkId != 0 || transportId != 0 || serviceId != 0) && pmtPid != 0);
              string name = DvbTextConverter.Convert(serviceName, "");
              Log.Log.Write("{0}) 0x{1:X} 0x{2:X} 0x{3:X} 0x{4:X} {5} type:{9:X}", i, networkId, transportId, serviceId, pmtPid, name, serviceType);
              ServiceType eServiceType = (ServiceType)serviceType;
              if (eServiceType == ServiceType.Mpeg2HDStream || eServiceType == ServiceType.H264Stream || eServiceType == ServiceType.AdvancedCodecHDVideoStream)
                Log.Log.WriteFile("HD Video ({0})!", eServiceType.ToString());

              if ((channel as ATSCChannel) != null)
              {
                //It seems with ATSC QAM the major & minor channel is not found or is not necessary (TBD)
                //isValid = (majorChannel != 0 && minorChannel != 0);
                //So we currently determine if a valid channel is found if the pmt pid is not null.
                isValid = (pmtPid != -1);
                //This is not ideal we need to look into raw ATSC transport streams
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
                info.serviceType = serviceType;
                info.modulation = modulation;
                info.service_provider_name = DvbTextConverter.Convert(providerName, "");
                info.service_name = DvbTextConverter.Convert(serviceName, "");
                info.scrambled = (freeCAMode != 0);
                info.network_pmt_PID = pmtPid;

                if (!IsKnownServiceType(info.serviceType))
                {
                  if (hasVideo == 1)
                    info.serviceType = (int)ServiceType.Video;
                  else
                    if (hasAudio == 1)
                      info.serviceType = (int)ServiceType.Audio;
                }

                if (IsKnownServiceType(info.serviceType))
                {
                  if (info.service_name.Length == 0)
                  {
                    if ((channel as ATSCChannel) != null)
                    {
                      if (((ATSCChannel)channel).Frequency > 0)
                      {
                        Log.Log.Info("DVBBaseScanning: service_name is null so now = Unknown {0}-{1}", ((ATSCChannel)channel).Frequency, info.network_pmt_PID.ToString());
                        info.service_name = String.Format("Unknown {0}-{1:X}", ((ATSCChannel)channel).Frequency, info.network_pmt_PID);
                      }
                      else
                      {
                        Log.Log.Info("DVBBaseScanning: service_name is null so now = Unknown {0}-{1}", ((ATSCChannel)channel).PhysicalChannel, info.network_pmt_PID.ToString());
                        info.service_name = String.Format("Unknown {0}-{1:X}", ((ATSCChannel)channel).PhysicalChannel, info.network_pmt_PID);
                      }
                    }
                    else
                      info.service_name = String.Format("Unknown {0:X}", info.serviceID);
                  }
                  IChannel dvbChannel = CreateNewChannel(info);
                  if (dvbChannel != null)
                  {
                    channelsFound.Add(dvbChannel);
                  }
                }
                else
                  Log.Log.Write("Found Unknown: {0} {1} type:{2} onid:{3:X} tsid:{4:X} sid:{5:X} pmt:{6:X} hasVideo:{7} hasAudio:{8}", info.service_provider_name, info.service_name, info.serviceType, info.networkID, info.transportStreamID, info.serviceID, info.network_pmt_PID, hasVideo, hasAudio);
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
      finally
      {
        _card.IsScanning = false;
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

    #endregion

    #region NIT scanning
    ///<summary>
    /// Scan NIT channel
    ///</summary>
    ///<param name="channel">Channel</param>
    ///<param name="settings">Scan Parameters</param>
    ///<returns>Found channels</returns>
    public List<IChannel> ScanNIT(IChannel channel, ScanParameters settings)
    {
      try
      {
        _card.IsScanning = true;
        _card.Tune(0, channel);
        _analyzer = GetAnalyzer();
        if (_analyzer == null)
        {
          Log.Log.WriteFile("Scan: no analyzer interface available");
          return new List<IChannel>();
        }
        _analyzer.SetCallBack(null);
        _analyzer.ScanNIT();
        Thread.Sleep(settings.TimeOutTune * 1000);
        ResetSignalUpdate();
        Log.Log.WriteFile("ScanNIT: tuner locked:{0} signal:{1} quality:{2}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);
        if (_card.IsTunerLocked || _card.SignalLevel > 0 || _card.SignalQuality > 0)
        {
          int count;

          _event = new ManualResetEvent(false);
          _event.WaitOne(16000, true);
          _event.Close();
          List<IChannel> channelsFound = new List<IChannel>();
          _analyzer.GetNITCount(out count);
          for (int i = 0; i < count; ++i)
          {
            int freq, pol, mod, symbolrate, bandwidth, innerfec, chType;
            IntPtr ptrName;
            _analyzer.GetNITChannel((short)i, out chType, out freq, out pol, out mod, out symbolrate, out bandwidth, out innerfec, out ptrName);
            string name = DvbTextConverter.Convert(ptrName, "");
            if (chType == 0)
            {
              DVBSChannel ch = new DVBSChannel();
              ch.Name = name;
              ch.Frequency = freq;
              ch.ModulationType = (ModulationType)mod;
              ch.SymbolRate = symbolrate;
              ch.InnerFecRate = (BinaryConvolutionCodeRate)innerfec;
              channelsFound.Add(ch);
            }
            else if (chType == 1)
            {
              DVBCChannel ch = new DVBCChannel();
              ch.Name = name;
              ch.Frequency = freq;
              ch.ModulationType = (ModulationType)mod;
              ch.SymbolRate = symbolrate;
              channelsFound.Add(ch);
            }
            else if (chType == 2)
            {
              DVBTChannel ch = new DVBTChannel();
              ch.Name = name;
              ch.Frequency = freq;
              ch.BandWidth = bandwidth;
              channelsFound.Add(ch);
            }
          }
          _analyzer.StopNIT();
          return channelsFound;

        }
        else
        {
          Log.Log.WriteFile("Scan: no signal detected");
          return new List<IChannel>();
        }
      }
      finally
      {
        if (_analyzer != null)
        {
          _analyzer.StopNIT();
        }
        _card.IsScanning = false;
      }
    }
    #endregion

    #region Helper
    private static bool IsKnownServiceType(int serviceType)
    {
      return (serviceType == (int)ServiceType.Video || serviceType == (int)ServiceType.Mpeg2HDStream ||
              serviceType == (int)ServiceType.Audio || serviceType == (int)ServiceType.H264Stream ||
              serviceType == (int)ServiceType.AdvancedCodecHDVideoStream || // =advanced codec HD digital television service
              serviceType == 0x8D || // = User private. his is needed to have the transponder for the 9 day dish epg guide discovered by the scan and to add the channel as TV Channel so that we can grab EPG from it
              serviceType == (int)ServiceType.Mpeg4OrH264Stream);
    }
    #endregion
  }
}
