/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
//#define FORM
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Implementations.Helper;
using TvDatabase;

namespace TvLibrary.Implementations.DVB
{
  #region enums
  enum TunerType
  {
    ttSat = 0,
    ttCable = 1,
    ttTerrestrial = 2,
    ttATSC = 3,
    ttUnknown = -1
  }
  enum eModulationTAG
  {
    QAM_4 = 2,
    QAM_16,
    QAM_32,
    QAM_64,
    QAM_128,
    QAM_256,
    MODE_UNKNOWN = -1
  }
  enum GuardIntervalType
  {
    Interval_1_32 = 0,
    Interval_1_16,
    Interval_1_8,
    Interval_1_4,
    Interval_Auto
  }
  enum BandWidthType
  {
    MHz_6 = 6,
    MHz_7 = 7,
    MHz_8 = 8,
  }
  enum SS2DisEqcType
  {
    None = 0,
    Simple_A,
    Simple_B,
    Level_1_A_A,
    Level_1_A_B,
    Level_1_B_A,
    Level_1_B_B
  }
  enum FecType
  {
    Fec_1_2 = 1,
    Fec_2_3,
    Fec_3_4,
    Fec_5_6,
    Fec_7_8,
    Fec_Auto
  }
  enum LNBSelectionType
  {
    Lnb0 = 0,
    Lnb22kHz,
    Lnb33kHz,
    Lnb44kHz,
  }
  enum PolarityType
  {
    Horizontal = 0,
    Vertical,
  }
  #endregion

  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles the SkyStar 2 DVB-S card
  /// </summary>
  public class TvCardDvbSS2 : TvCardDvbBase, IDisposable, ITVCard, IDiSEqCController
  {

    #region private consts

    //private const string DEVICE_DRIVER_NAME = "TechniSat DVB-PC TV Star PCI";

    #endregion

    #region imports
    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    class MpTsAnalyzer { }
    #endregion

    #region Structs
    //
    //	Structure completedy by GetTunerCapabilities() to return tuner capabilities
    //
#pragma warning disable 169, 649
    private struct tTunerCapabilities
    {
#pragma warning disable 649 // All fields are used by the Marshal.PtrToStructure function
      public TunerType eModulation;
      public int dwConstellationSupported;       // Show if SetModulation() is supported
      public int dwFECSupported;                 // Show if SetFec() is suppoted
      public int dwMinTransponderFreqInKHz;
      public int dwMaxTransponderFreqInKHz;
      public int dwMinTunerFreqInKHz;
      public int dwMaxTunerFreqInKHz;
      public int dwMinSymbolRateInBaud;
      public int dwMaxSymbolRateInBaud;
      public int bAutoSymbolRate;				// Obsolte		
      public int dwPerformanceMonitoring;        // See bitmask definitions below
      public int dwLockTimeInMilliSecond;		// lock time in millisecond
      public int dwKernelLockTimeInMilliSecond;	// lock time for kernel
      public int dwAcquisitionCapabilities;
#pragma warning restore 649
    }
#pragma warning restore 169,649
    #endregion

    #region variables
    IBaseFilter _filterB2C2Adapter;
    DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 _interfaceB2C2DataCtrl;
    DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2 _interfaceB2C2TunerCtrl;
    readonly IntPtr _ptrDisEqc;
    readonly DiSEqCMotor _disEqcMotor;
    readonly bool _useDISEqCMotor;

    #endregion

    #region imports
    [DllImport("advapi32", CharSet = CharSet.Auto)]
    static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    static extern int SetPidToPin(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl, UInt16 pin, UInt16 pid);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool DeleteAllPIDs(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl, UInt16 pin);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetSNR(DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2 tunerCtrl, [Out] out int a, [Out] out int b);
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDvbSS2"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface.</param>
    /// <param name="device">The device.</param>
    public TvCardDvbSS2(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _useDISEqCMotor = false;
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(device.DevicePath);
      if (card != null)
      {
        Setting setting = layer.GetSetting("dvbs" + card.IdCard + "motorEnabled", "no");
        if (setting.Value == "yes")
          _useDISEqCMotor = true;
      }
      _conditionalAccess = new ConditionalAccess(null, null, null, this);
      _ptrDisEqc = Marshal.AllocCoTaskMem(20);
      _disEqcMotor = new DiSEqCMotor(this);
      GetTunerCapabilities();
    }
    #endregion

    #region tuning & recording
    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The subchannel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      int frequency = 0;
      int symbolRate = 0;
      int modulation = (int)eModulationTAG.QAM_64;
      int bandWidth = 0;
      LNBSelectionType lnbSelection = LNBSelectionType.Lnb0;
      const int lnbKhzTone = 22;
      const int fec = (int)FecType.Fec_Auto;
      int polarity = 0;
      SS2DisEqcType disType = SS2DisEqcType.None;
      int switchFreq = 0;
      int pmtPid = 0;
      int satelliteIndex = 0;
      Log.Log.WriteFile("ss2:Tune({0})", channel);
      if (_epgGrabbing)
      {
        _epgGrabbing = false;
        if (_epgGrabberCallback != null && _epgGrabbing)
        {
          _epgGrabberCallback.OnEpgCancelled();
        }
      }
      switch (_cardType)
      {
        case CardType.DvbS:
          DVBSChannel dvbsChannel = channel as DVBSChannel;
          if (dvbsChannel == null)
          {
            Log.Log.Error("Channel is not a DVBS channel!!! {0}", channel.GetType().ToString());
            return null;
          }
          if (CurrentChannel != null)
          {
            DVBSChannel oldChannels = (DVBSChannel)CurrentChannel;
            if (oldChannels.Equals(channel))
            {
              //@FIX this fails for back-2-back recordings
              //Log.Log.WriteFile("ss2:already tuned on this channel");
              //return _mapSubChannels[0];
            }
          }
          frequency = (int)dvbsChannel.Frequency;
          symbolRate = dvbsChannel.SymbolRate;
          satelliteIndex = dvbsChannel.SatelliteIndex;
          bool hiBand = BandTypeConverter.IsHiBand(dvbsChannel, Parameters);
          int lof1, lof2, sw;
          BandTypeConverter.GetDefaultLnbSetup(Parameters, dvbsChannel.BandType, out lof1, out lof2, out sw);
          int lnbFrequency;
          if (BandTypeConverter.IsHiBand(dvbsChannel, Parameters))
            lnbFrequency = lof2 * 1000;
          else
            lnbFrequency = lof1 * 1000;
          //0=horizontal or left, 1=vertical or right
          polarity = 0;
          if (dvbsChannel.Polarisation == Polarisation.LinearV)
            polarity = 1;
          if (dvbsChannel.Polarisation == Polarisation.CircularR)
            polarity = 1;
          Log.Log.WriteFile("ss2:  Polarity:{0} {1}", dvbsChannel.Polarisation, polarity);
          lnbSelection = LNBSelectionType.Lnb0;
          if (dvbsChannel.BandType == BandType.Universal)
          {
            //only set the LNB (22,33,44) Khz tone when we use ku-band and are in hi-band
            switch (lnbKhzTone)
            {
              case 22:
                lnbSelection = LNBSelectionType.Lnb22kHz;
                break;
            }
            if (hiBand == false)
            {
              lnbSelection = LNBSelectionType.Lnb0;
            }
          }
          switch (dvbsChannel.DisEqc)
          {
            case DisEqcType.None: // none
              disType = SS2DisEqcType.None;
              break;
            case DisEqcType.SimpleA: // Simple A
              disType = SS2DisEqcType.Simple_A;
              break;
            case DisEqcType.SimpleB: // Simple B
              disType = SS2DisEqcType.Simple_B;
              break;
            case DisEqcType.Level1AA: // Level 1 A/A
              disType = SS2DisEqcType.Level_1_A_A;
              break;
            case DisEqcType.Level1BA: // Level 1 B/A
              disType = SS2DisEqcType.Level_1_B_A;
              break;
            case DisEqcType.Level1AB: // Level 1 A/B
              disType = SS2DisEqcType.Level_1_A_B;
              break;
            case DisEqcType.Level1BB: // Level 1 B/B
              disType = SS2DisEqcType.Level_1_B_B;
              break;
          }
          switchFreq = lnbFrequency / 1000;//in MHz
          pmtPid = dvbsChannel.PmtPid;
          break;
        case CardType.DvbT:
          DVBTChannel dvbtChannel = channel as DVBTChannel;
          if (dvbtChannel == null)
          {
            Log.Log.Error("Channel is not a DVBT channel!!! {0}", channel.GetType().ToString());
            return null;
          }
          if (CurrentChannel != null)
          {
            DVBTChannel oldChannelt = (DVBTChannel)CurrentChannel;
            if (oldChannelt.Equals(channel))
            {
              //@FIX this fails for back-2-back recordings
              //Log.Log.WriteFile("ss2:already tuned on this channel");
              //return _mapSubChannels[0];
            }
          }
          frequency = (int)dvbtChannel.Frequency;
          bandWidth = dvbtChannel.BandWidth;
          pmtPid = dvbtChannel.PmtPid;
          break;
        case CardType.DvbC:
          DVBCChannel dvbcChannel = channel as DVBCChannel;
          if (dvbcChannel == null)
          {
            Log.Log.Error("Channel is not a DVBC channel!!! {0}", channel.GetType().ToString());
            return null;
          }
          if (CurrentChannel != null)
          {
            DVBCChannel oldChannelc = (DVBCChannel)CurrentChannel;
            if (oldChannelc.Equals(channel))
            {
              //@FIX this fails for back-2-back recordings
              //Log.Log.WriteFile("ss2:already tuned on this channel");
              //return _mapSubChannels[0];
            }
          }
          frequency = (int)dvbcChannel.Frequency;
          symbolRate = dvbcChannel.SymbolRate;
          switch (dvbcChannel.ModulationType)
          {
            case ModulationType.Mod16Qam:
              modulation = (int)eModulationTAG.QAM_16;
              break;
            case ModulationType.Mod32Qam:
              modulation = (int)eModulationTAG.QAM_32;
              break;
            case ModulationType.Mod64Qam:
              modulation = (int)eModulationTAG.QAM_64;
              break;
            case ModulationType.Mod128Qam:
              modulation = (int)eModulationTAG.QAM_128;
              break;
            case ModulationType.Mod256Qam:
              modulation = (int)eModulationTAG.QAM_256;
              break;
          }
          pmtPid = dvbcChannel.PmtPid;
          break;
        case CardType.Atsc:
          ATSCChannel dvbaChannel = channel as ATSCChannel;
          if (dvbaChannel == null)
          {
            Log.Log.Error("Channel is not a ATSC channel!!! {0}", channel.GetType().ToString());
            return null;
          }
          if (CurrentChannel != null)
          {
            ATSCChannel oldChannela = (ATSCChannel)CurrentChannel;
            if (oldChannela.Equals(channel))
            {
              //@FIX this fails for back-2-back recordings
              //Log.Log.WriteFile("ss2:already tuned on this channel");
              //return _mapSubChannels[0];
            }
          }
          //if modulation = 256QAM assume ATSC QAM for HD5000
          if (dvbaChannel.ModulationType == ModulationType.Mod256Qam)
          {
            Log.Log.WriteFile("DVBGraphB2C2:  ATSC Channel:{0} Frequency:{1}", dvbaChannel.PhysicalChannel, dvbaChannel.Frequency);
            frequency = (int)dvbaChannel.Frequency;
            pmtPid = dvbaChannel.PmtPid;
          }
          else
          {
            Log.Log.WriteFile("DVBGraphSkyStar2:  ATSC Channel:{0}", dvbaChannel.PhysicalChannel);
            //#DM B2C2 SDK says ATSC is tuned by frequency. Here we work the OTA frequency by channel number#
            int atscfreq = 0;
            if (dvbaChannel.PhysicalChannel <= 6)
              atscfreq = 45 + (dvbaChannel.PhysicalChannel * 6);
            if (dvbaChannel.PhysicalChannel >= 7 && dvbaChannel.PhysicalChannel <= 13)
              atscfreq = 177 + ((dvbaChannel.PhysicalChannel - 7) * 6);
            if (dvbaChannel.PhysicalChannel >= 14)
              atscfreq = 473 + ((dvbaChannel.PhysicalChannel - 14) * 6);
            //#DM changed tuning parameter from physical channel to calculated frequency above.
            frequency = atscfreq;
            Log.Log.WriteFile("ss2:  ATSC Frequency:{0} MHz", frequency);
            pmtPid = dvbaChannel.PmtPid;
          }
          break;
      }
      if (_graphState == GraphState.Idle)
      {
        BuildGraph();
      }
      if (_mapSubChannels.ContainsKey(subChannelId) == false)
      {
        subChannelId = GetNewSubChannel(channel);
      }
      _mapSubChannels[subChannelId].CurrentChannel = channel;
      _mapSubChannels[subChannelId].OnBeforeTune();
      if (_interfaceEpgGrabber != null)
      {
        _interfaceEpgGrabber.Reset();
      }
      if (frequency > 13000)
        frequency /= 1000;
      Log.Log.WriteFile("ss2:  Transponder Frequency:{0} MHz", frequency);
      int hr = _interfaceB2C2TunerCtrl.SetFrequency(frequency);
      if (hr != 0)
      {
        Log.Log.Error("ss2:SetFrequencyKHz() failed:0x{0:X}", hr);
        return null;
      }
      switch (_cardType)
      {
        case CardType.DvbC:
          Log.Log.WriteFile("ss2:  SymbolRate:{0} KS/s", symbolRate);
          hr = _interfaceB2C2TunerCtrl.SetSymbolRate(symbolRate);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetSymbolRate() failed:0x{0:X}", hr);
            return null;
          }
          Log.Log.WriteFile("ss2:  Modulation:{0}", ((eModulationTAG)modulation));
          hr = _interfaceB2C2TunerCtrl.SetModulation(modulation);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetModulation() failed:0x{0:X}", hr);
            return null;
          }
          break;
        case CardType.DvbT:
          Log.Log.WriteFile("ss2:  GuardInterval:auto");
          hr = _interfaceB2C2TunerCtrl.SetGuardInterval((int)GuardIntervalType.Interval_Auto);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetGuardInterval() failed:0x{0:X}", hr);
            return null;
          }
          Log.Log.WriteFile("ss2:  Bandwidth:{0} MHz", bandWidth);
          //hr = _interfaceB2C2TunerCtrl.SetBandwidth((int)dvbtChannel.BandWidth);
          // Set Channel Bandwidth (NOTE: Temporarily use polarity function to avoid having to 
          // change SDK interface for SetBandwidth)
          // from Technisat SDK 03/2006
          hr = _interfaceB2C2TunerCtrl.SetPolarity(bandWidth);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetBandwidth() failed:0x{0:X}", hr);
            return null;
          }
          break;
        case CardType.DvbS:
          Log.Log.WriteFile("ss2:  SymbolRate:{0} KS/s", symbolRate);
          hr = _interfaceB2C2TunerCtrl.SetSymbolRate(symbolRate);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetSymbolRate() failed:0x{0:X}", hr);
            return null;
          }
          Log.Log.WriteFile("ss2:  Fec:{0} {1}", ((FecType)fec), fec);
          hr = _interfaceB2C2TunerCtrl.SetFec(fec);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetFec() failed:0x{0:X}", hr);
            return null;
          }
          hr = _interfaceB2C2TunerCtrl.SetPolarity(polarity);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetPolarity() failed:0x{0:X}", hr);
            return null;
          }
          Log.Log.WriteFile("ss2:  Lnb:{0}", lnbSelection);
          hr = _interfaceB2C2TunerCtrl.SetLnbKHz((int)lnbSelection);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetLnbKHz() failed:0x{0:X}", hr);
            return null;
          }
          Log.Log.WriteFile("ss2:  Diseqc:{0} {1}", disType, disType);
          hr = _interfaceB2C2TunerCtrl.SetDiseqc((int)disType);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetDiseqc() failed:0x{0:X}", hr);
            return null;
          }
          Log.Log.WriteFile("ss2:  LNBFrequency:{0} MHz", switchFreq);
          hr = _interfaceB2C2TunerCtrl.SetLnbFrequency(switchFreq);
          if (hr != 0)
          {
            Log.Log.Error("ss2:SetLnbFrequency() failed:0x{0:X}", hr);
            return null;
          }
          if (_useDISEqCMotor)
          {
            if (satelliteIndex > 0)
            {
              DisEqcGotoPosition((byte)satelliteIndex);
            }
          }
          break;
      }
      hr = -1;
      int lockRetries = 0;
      while
          (((uint)hr == 0x90010115 || hr == -1) && lockRetries < 5)
      {
        hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
        _interfaceB2C2TunerCtrl.CheckLock();
        if (((uint)hr) == 0x90010115)
        {
          Log.Log.Info("ss2:could not lock tuner...sleep 20ms");
          System.Threading.Thread.Sleep(20);
          lockRetries++;
        }
      }

      if (((uint)hr) == 0x90010115)
      {
        Log.Log.Info("ss2:could not lock tuner after {0} attempts", lockRetries);
        return null;
      }
      if (lockRetries > 0)
      {
        Log.Log.Info("ss2:locked tuner after {0} attempts", lockRetries);
      }

      if (hr != 0)
      {
        hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
        if (hr != 0)
          hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
        if (hr != 0)
        {
          //Log.Log.Error("ss2:SetTunerStatus failed:0x{0:X}", hr);
          return null;
        }
      }
      _interfaceB2C2TunerCtrl.CheckLock();
      _lastSignalUpdate = DateTime.MinValue;
      SendHwPids(new List<ushort>());
      _mapSubChannels[subChannelId].OnAfterTune();
      RunGraph(subChannelId);
      Log.Log.WriteFile("ss2:tune done:{0:X}", pmtPid);
      return _mapSubChannels[subChannelId];
    }
    #endregion

    #region epg & scanning
    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    public ITVScanning ScanningInterface
    {
      get
      {
        if (!CheckThreadId())
          return null;
        return new DVBSS2canning(this);
      }
    }
    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _tunerDevice.Name;
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {
      if (_cardType == CardType.DvbS)
      {
        if ((channel as DVBSChannel) == null)
          return false;
        return true;
      }
      if (_cardType == CardType.DvbT)
      {
        if ((channel as DVBTChannel) == null)
          return false;
        return true;
      }
      if (_cardType == CardType.DvbC)
      {
        if ((channel as DVBCChannel) == null)
          return false;
        return true;
      }
      if (_cardType == CardType.Atsc)
      {
        if ((channel as ATSCChannel) == null)
          return false;
        return true;
      }
      return false;
    }

    #region SS2 specific

    ///<summary>
    /// Checks if the tuner is locked in and a sginal is present
    ///</summary>
    ///<returns>true, when the tuner is locked and a signal is present</returns>
    public override bool LockedInOnSignal()
    {
      bool isLocked = false;
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      while (!isLocked && ts.TotalSeconds < 2)
      {
        int hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
        _interfaceB2C2TunerCtrl.CheckLock();
        if (((uint)hr) == 0x90010115)
        {
          ts = DateTime.Now - timeStart;
          Log.Log.WriteFile("dvb-s ss2:  LockedInOnSignal waiting 20ms");
          System.Threading.Thread.Sleep(20);
        }
        else
        {
          isLocked = true;
        }
      }

      if (!isLocked)
      {
        Log.Log.WriteFile("dvb-s ss2:  LockedInOnSignal could not lock onto channel - no signal or bad signal");
      }
      else
      {
        Log.Log.WriteFile("dvb-s ss2:  LockedInOnSignal ok");
      }
      return isLocked;
    }

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
    {
      Log.Log.WriteFile("ss2: build graph");
      if (_graphState != GraphState.Idle)
      {
        Log.Log.Error("ss2: Graph already built");
        throw new TvException("Graph already built");
      }
      DevicesInUse.Instance.Add(_tunerDevice);
      _managedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graphBuilder);
      _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
      _capBuilder.SetFiltergraph(_graphBuilder);
      //=========================================================================================================
      // add the skystar 2 specific filters
      //=========================================================================================================
      Log.Log.WriteFile("ss2:CreateGraph() create B2C2 adapter");
      _filterB2C2Adapter = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(DVBSkyStar2Helper.CLSID_B2C2Adapter, false));
      if (_filterB2C2Adapter == null)
      {
        Log.Log.Error("ss2:creategraph() _filterB2C2Adapter not found");
        DevicesInUse.Instance.Remove(_tunerDevice);
        return;
      }
      Log.Log.WriteFile("ss2:creategraph() add filters to graph");
      int hr = _graphBuilder.AddFilter(_filterB2C2Adapter, "B2C2-Source");
      if (hr != 0)
      {
        Log.Log.Error("ss2: FAILED to add B2C2-Adapter");
        DevicesInUse.Instance.Remove(_tunerDevice);
        return;
      }
      // get interfaces
      _interfaceB2C2DataCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3;
      if (_interfaceB2C2DataCtrl == null)
      {
        Log.Log.Error("ss2: cannot get IB2C2MPEG2DataCtrl3");
        DevicesInUse.Instance.Remove(_tunerDevice);
        return;
      }
      _interfaceB2C2TunerCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2;
      if (_interfaceB2C2TunerCtrl == null)
      {
        Log.Log.Error("ss2: cannot get IB2C2MPEG2TunerCtrl3");
        DevicesInUse.Instance.Remove(_tunerDevice);
        return;
      }
      //=========================================================================================================
      // initialize skystar 2 tuner
      //=========================================================================================================
      Log.Log.WriteFile("ss2: Initialize Tuner()");
      hr = _interfaceB2C2TunerCtrl.Initialize();
      if (hr != 0)
      {
        //System.Diagnostics.Debugger.Launch();
        Log.Log.Error("ss2: Tuner initialize failed:0x{0:X}", hr);
        // if the skystar2 card is detected as analogue, it needs a device reset 

        ((IMediaControl)_graphBuilder).Stop();
        FreeAllSubChannels();
        FilterGraphTools.RemoveAllFilters(_graphBuilder);

        if (_graphBuilder != null)
        {
          Release.ComObject("graph builder", _graphBuilder);
          _graphBuilder = null;
        }

        if (_capBuilder != null)
        {
          Release.ComObject("capBuilder", _capBuilder);
          _capBuilder = null;
        }

        DevicesInUse.Instance.Remove(_tunerDevice);

        /*
        if (initResetTries == 0)
        {
          Log.Log.Error("ss2: resetting driver");
          HardwareHelperLib.HH_Lib hwHelper = new HardwareHelperLib.HH_Lib();
          string[] deviceDriverName = new string[1];
          deviceDriverName[0] = DEVICE_DRIVER_NAME;
          hwHelper.SetDeviceState(deviceDriverName, false);
          hwHelper.SetDeviceState(deviceDriverName, true);
          initResetTries++;          

          BuildGraph();
        }
        else
        {
          Log.Log.Error("ss2: resetting driver did not help");          
          CardPresent = false;
        }    
        */
        CardPresent = false;
        return;
      }
      // call checklock once, the return value dont matter
      _interfaceB2C2TunerCtrl.CheckLock();
      AddMpeg2DemuxerToGraph();
      ConnectInfTeeToSS2();
      ConnectMpeg2DemuxToInfTee();
      AddTsWriterFilterToGraph();
      SendHwPids(new List<ushort>());
      _graphState = GraphState.Created;
    }

    /// <summary>
    /// Connects the SS2 filter to the infTee
    /// </summary>
    void ConnectInfTeeToSS2()
    {
      Log.Log.WriteFile("ss2:ConnectMainTee()");
      IPin pinOut = DsFindPin.ByDirection(_filterB2C2Adapter, PinDirection.Output, 2);
      IPin pinIn = DsFindPin.ByDirection(_infTeeMain, PinDirection.Input, 0);
      if (pinOut == null)
      {
        Log.Log.Error("ss2:unable to find pin 2 of b2c2adapter");
        throw new TvException("unable to find pin 2 of b2c2adapter");
      }
      if (pinIn == null)
      {
        Log.Log.Error("ss2:unable to find pin 0 of _infTeeMain");
        throw new TvException("unable to find pin 0 of _infTeeMain");
      }
      int hr = _graphBuilder.Connect(pinOut, pinIn);
      Release.ComObject("b2c2pin2", pinOut);
      Release.ComObject("mpeg2demux pinin", pinIn);
      if (hr != 0)
      {
        Log.Log.Error("ss2:unable to connect b2c2->_infTeeMain");
        throw new TvException("unable to connect b2c2->_infTeeMain");
      }
    }
    /// <summary>
    /// Sends the HW pids.
    /// </summary>
    /// <param name="pids">The pids.</param>
    public override void SendHwPids(List<ushort> pids)
    {
      const int PID_CAPTURE_ALL_INCLUDING_NULLS = 0x2000;//Enables reception of all PIDs in the transport stream including the NULL PID
      //const int PID_CAPTURE_ALL_EXCLUDING_NULLS = 0x2001;//Enables reception of all PIDs in the transport stream excluding the NULL PID.

      if (!DeleteAllPIDs(_interfaceB2C2DataCtrl, 0))
      {
        Log.Log.Error("ss2:DeleteAllPIDs() failed pid:0x2000");
      }
      /*if (pids.Count == 0 || true)*/
      {
        Log.Log.WriteFile("ss2:hw pids:all");
        int added = SetPidToPin(_interfaceB2C2DataCtrl, 0, PID_CAPTURE_ALL_INCLUDING_NULLS);
        if (added != 1)
        {
          Log.Log.Error("ss2:SetPidToPin() failed pid:0x2000");
        }
      }/* unreachable
      else
      {
        int maxPids;
        _interfaceB2C2DataCtrl.GetMaxPIDCount(out maxPids);
        for (int i = 0; i < pids.Count && i < maxPids; ++i)
        {
          ushort pid = (ushort)pids[i];
          Log.Log.WriteFile("ss2:hw pids:0x{0:X}", pid);
          SetPidToPin(_interfaceB2C2DataCtrl, 0, pid);
        }
      }
      */
    }

    /// <summary>
    /// updates the signal quality/level and tuner locked statusses
    /// </summary>
    protected override void UpdateSignalQuality(bool force)
    {
      TimeSpan ts = DateTime.Now - _lastSignalUpdate;
      if (ts.TotalMilliseconds < 5000)
        return;
      if (GraphRunning() == false)
      {
        _tunerLocked = false;
        _signalLevel = 0;
        _signalPresent = false;
        _signalQuality = 0;
        return;
      }
      if (CurrentChannel == null)
      {
        _tunerLocked = false;
        _signalLevel = 0;
        _signalPresent = false;
        _signalQuality = 0;
        return;
      }
      if (_graphState == GraphState.Idle || _interfaceB2C2TunerCtrl == null)
      {
        _tunerLocked = false;
        _signalQuality = 0;
        _signalLevel = 0;
        return;
      }
      int level, quality;
      _tunerLocked = (_interfaceB2C2TunerCtrl.CheckLock() == 0);
      GetSNR(_interfaceB2C2TunerCtrl, out level, out quality);
      if (level < 0)
        level = 0;
      if (level > 100)
        level = 100;
      if (quality < 0)
        quality = 0;
      if (quality > 100)
        quality = 100;
      _signalQuality = quality;
      _signalLevel = level;
      _lastSignalUpdate = DateTime.Now;
    }

    /// <summary>
    /// updates the signal quality/level and tuner locked statusses
    /// </summary>
    protected override void UpdateSignalQuality()
    {
      UpdateSignalQuality(false);
    }

    private void GetTunerCapabilities()
    {
      Log.Log.WriteFile("ss2: GetTunerCapabilities");
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graphBuilder);
      _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
      _capBuilder.SetFiltergraph(_graphBuilder);
      //=========================================================================================================
      // add the skystar 2 specific filters
      //=========================================================================================================
      Log.Log.WriteFile("ss2:GetTunerCapabilities() create B2C2 adapter");
      _filterB2C2Adapter = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(DVBSkyStar2Helper.CLSID_B2C2Adapter, false));
      if (_filterB2C2Adapter == null)
      {
        Log.Log.Error("ss2:GetTunerCapabilities() _filterB2C2Adapter not found");
        return;
      }
      _interfaceB2C2TunerCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2;
      if (_interfaceB2C2TunerCtrl == null)
      {
        Log.Log.Error("ss2: cannot get IB2C2MPEG2TunerCtrl3");
        return;
      }
      //=========================================================================================================
      // initialize skystar 2 tuner
      //=========================================================================================================
      /* Not necessary for query-only application
       
      Log.Log.WriteFile("ss2: Initialize Tuner()");
      hr = _interfaceB2C2TunerCtrl.Initialize();
      if (hr != 0)
      {
        Log.Log.Error("ss2: Tuner initialize failed:0x{0:X}", hr);
        //return;
      }*/
      //=========================================================================================================
      // Get tuner type (DVBS, DVBC, DVBT, ATSC)
      //=========================================================================================================
      int lTunerCapSize = Marshal.SizeOf(typeof(tTunerCapabilities));
      IntPtr ptCaps = Marshal.AllocHGlobal(lTunerCapSize);
      int hr = _interfaceB2C2TunerCtrl.GetTunerCapabilities(ptCaps, ref lTunerCapSize);
      if (hr != 0)
      {
        Log.Log.Error("ss2: Tuner Type failed:0x{0:X}", hr);
        return;
      }
      tTunerCapabilities tc = (tTunerCapabilities)Marshal.PtrToStructure(ptCaps, typeof(tTunerCapabilities));
      switch (tc.eModulation)
      {
        case TunerType.ttSat:
          Log.Log.WriteFile("ss2: Card type = DVBS");
          _cardType = CardType.DvbS;
          break;
        case TunerType.ttCable:
          Log.Log.WriteFile("ss2: Card type = DVBC");
          _cardType = CardType.DvbC;
          break;
        case TunerType.ttTerrestrial:
          Log.Log.WriteFile("ss2: Card type = DVBT");
          _cardType = CardType.DvbT;
          break;
        case TunerType.ttATSC:
          Log.Log.WriteFile("ss2: Card type = ATSC");
          _cardType = CardType.Atsc;
          break;
        case TunerType.ttUnknown:
          Log.Log.WriteFile("ss2: Card type = unknown?");
          _cardType = CardType.DvbS;
          break;
      }
      Marshal.FreeHGlobal(ptCaps);
      // Release all used object
      if (_filterB2C2Adapter != null)
      {
        Release.ComObject("tuner filter", _filterB2C2Adapter);
        _filterB2C2Adapter = null;
      }
      _rotEntry.Dispose();
      if (_capBuilder != null)
      {
        Release.ComObject("capture builder", _capBuilder);
        _capBuilder = null;
      }
      if (_graphBuilder != null)
      {
        Release.ComObject("graph builder", _graphBuilder);
        _graphBuilder = null;
      }
    }
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public override void Dispose()
    {
      if (_graphBuilder == null)
        return;
      if (!CheckThreadId())
        return;

      if (_epgGrabbing)
      {
        _epgGrabbing = false;
        if (_epgGrabberCallback != null && _epgGrabbing)
        {
          _epgGrabberCallback.OnEpgCancelled();
        }
      }
      Log.Log.WriteFile("ss2:Decompose");
      // Decompose the graph
      //hr = (_graphBuilder as IMediaControl).StopWhenReady();
      ((IMediaControl)_graphBuilder).Stop();
      FreeAllSubChannels();
      FilterGraphTools.RemoveAllFilters(_graphBuilder);
      _interfaceChannelScan = null;
      _interfaceEpgGrabber = null;
      _interfaceB2C2DataCtrl = null;
      _interfaceB2C2TunerCtrl = null;
      if (_filterMpeg2DemuxTif != null)
      {
        Release.ComObject("MPEG2 demux filter", _filterMpeg2DemuxTif);
        _filterMpeg2DemuxTif = null;
      }
      if (_filterB2C2Adapter != null)
      {
        Release.ComObject("tuner filter", _filterB2C2Adapter);
        _filterB2C2Adapter = null;
      }
      if (_filterTIF != null)
      {
        Release.ComObject("TIF filter", _filterTIF);
        _filterTIF = null;
      }
      //if (_filterSectionsAndTables != null)
      //{
      //  Release.ComObject("secions&tables filter", _filterSectionsAndTables); _filterSectionsAndTables = null;
      //}
      if (_filterTsWriter != null)
      {
        Release.ComObject("_filterMpTsAnalyzer", _filterTsWriter);
        _filterTsWriter = null;
      }
      if (_infTeeMain != null)
      {
        Release.ComObject("_infTeeMain", _infTeeMain);
        _infTeeMain = null;
      }
      _rotEntry.Dispose();
      if (_capBuilder != null)
      {
        Release.ComObject("capture builder", _capBuilder);
        _capBuilder = null;
      }
      if (_graphBuilder != null)
      {
        Release.ComObject("graph builder", _graphBuilder);
        _graphBuilder = null;
      }
      if (_tunerDevice != null)
      {
        DevicesInUse.Instance.Remove(_tunerDevice);
        _tunerDevice = null;
      }
    }
    #endregion

    #region IDiSEqCController Members
    void DisEqcGotoPosition(byte position)
    {
      _disEqcMotor.GotoPosition(position);
    }
    /// <summary>
    /// Handles DiSEqC motor operations
    /// </summary>
    public override IDiSEqCMotor DiSEqCMotor
    {
      get
      {
        return _disEqcMotor;
      }
    }

    /// <summary>
    /// Send the DiSEqC Command to the tuner filter
    /// </summary>
    /// <param name="diSEqC">DiSEqC command</param>
    /// <returns></returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl4 tuner4 = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl4;
      if (tuner4 == null)
        return false;
      for (int i = 0; i < diSEqC.Length; ++i)
      {
        Marshal.WriteByte(_ptrDisEqc, i, diSEqC[i]);
      }
      tuner4.SendDiSEqCCommand(diSEqC.Length, _ptrDisEqc);
      return true;
    }

    /// <summary>
    /// Reads the DiSEqC Command from the tuner filter
    /// </summary>
    /// <param name="reply">gets the DiSEqC command</param>
    /// <returns></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      reply = new byte[1];
      return false;
    }
    #endregion
  }
}
