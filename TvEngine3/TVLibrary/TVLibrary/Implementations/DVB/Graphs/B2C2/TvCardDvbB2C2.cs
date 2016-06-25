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

  #endregion

  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles the Technisat cards that use the B2C2 chip
  /// </summary>
  public class TvCardDvbB2C2 : TvCardDvbBase, IDisposable, ITVCard, IDiSEqCController
  {
    #region CLSID
    /// <summary>
    /// B2C2 Adapter GUID
    /// </summary>
    private static Guid CLSID_B2C2Adapter = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x0, 0xa0, 0xc9, 0x5d, 0x30,
                                                    0x8d);

    #endregion

    #region variables

    private IBaseFilter _filterB2C2Adapter;
    private IB2C2MPEG2DataCtrl6 _interfaceB2C2DataCtrl;
    private IB2C2MPEG2TunerCtrl4 _interfaceB2C2TunerCtrl;
    private readonly IntPtr _ptrDisEqc;
    private readonly DiSEqCMotor _disEqcMotor;
    private readonly bool _useDISEqCMotor;
    private readonly DeviceInfo _deviceInfo;
    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDvbB2C2"/> class.
    /// </summary>
    /// <param name="device">The device.</param>
    public TvCardDvbB2C2(DsDevice device, DeviceInfo deviceInfo)
      : base(device)
    {
      _deviceInfo = deviceInfo;
      _devicePath = deviceInfo.DevicePath;
      _name = deviceInfo.Name;
      GetPreloadBitAndCardId();

      _useDISEqCMotor = false;
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(_devicePath);
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
    public override ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("b2c2: Scan:{0}", channel);

      try
      {
        int pmtPid;
        if (!BeforeTune(channel, ref subChannelId, out pmtPid))
        {
          return null;
        }
        if (!DoTune())
        {
          return null;
        }
        AfterTune(subChannelId, true);

        Log.Log.WriteFile("b2c2:scan done:{0:X}", pmtPid);
        return _mapSubChannels[subChannelId];
      }
      catch (TvExceptionNoSignal)
      {
        throw;
      }
      catch (TvExceptionNoPMT)
      {
        throw;
      }
      catch (TvExceptionTuneCancelled)
      {
        throw;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw;
      }
    }

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The subchannel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public override ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("b2c2: Tune:{0}", channel);

      try
      {
        //FreePreviousChannelMDPlugs();
        int pmtPid;
        if (!BeforeTune(channel, ref subChannelId, out pmtPid))
        {
          return null;
        }
        if (!DoTune())
        {
          return null;
        }
        AfterTune(subChannelId, false);

        _previousChannel = channel;

        Log.Log.WriteFile("b2c2:tune done:{0:X}", pmtPid);
        return _mapSubChannels[subChannelId];
      }
      catch (Exception)
      {
        if (subChannelId > -1)
        {
          FreeSubChannel(subChannelId);
        }
        throw;
      }
    }

    private void AfterTune(int subChannelId, bool ignorePMT)
    {
      _interfaceB2C2TunerCtrl.CheckLock();
      _lastSignalUpdate = DateTime.MinValue;
      _mapSubChannels[subChannelId].OnAfterTune();
      try
      {
        try
        {
          RunGraph(subChannelId);
        }
        catch (TvExceptionNoPMT)
        {
          if (!ignorePMT)
          {
            throw;
          }
        }
      }
      catch (Exception)
      {
        FreeSubChannel(subChannelId);
        throw;
      }
    }

    private bool DoTune()
    {
      try
      {
      int hr = -1;
      int lockRetries = 0;
      while
        (((uint)hr == 0x90010115 || hr == -1) && lockRetries < 5)
      {
        hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
        _interfaceB2C2TunerCtrl.CheckLock();
        if (((uint)hr) == 0x90010115)
        {
          Log.Log.Info("b2c2:could not lock tuner...sleep 20ms");
          System.Threading.Thread.Sleep(20);
          lockRetries++;
        }
      }

      if (((uint)hr) == 0x90010115)
      {
        Log.Log.Info("b2c2:could not lock tuner after {0} attempts", lockRetries);
        throw new TvExceptionNoSignal("Unable to tune to channel - no signal");
      }
      if (lockRetries > 0)
      {
        Log.Log.Info("b2c2:locked tuner after {0} attempts", lockRetries);
      }

      if (hr != 0)
      {
        hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
        if (hr != 0)
          hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
        if (hr != 0)
        {
          //Log.Log.Error("b2c2:SetTunerStatus failed:0x{0:X}", hr);
          throw new TvExceptionGraphBuildingFailed("Graph building failed");
        }
      }
      return true;
    }
      finally
      {
        _cancelTune = false;
      }
      
    }

    private bool BeforeTune(IChannel channel, ref int subChannelId, out int pmtPid)
    {
      int frequency = 0;
      int symbolRate = 0;
      int modulation = (int)eModulationTAG.QAM_64;
      BandWidthType bandWidth = 0;
      LNBSelectionType lnbSelection = LNBSelectionType.Lnb0;
      const int lnbKhzTone = 22;
      const int fec = (int)FecType.Fec_Auto;
      int polarity = 0;
      B2C2DisEqcType disType = B2C2DisEqcType.None;
      int switchFreq = 0;
      pmtPid = 0;
      int satelliteIndex = 0;
      Log.Log.WriteFile("b2c2:Tune({0})", channel);
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
            return false;
          }
          if (CurrentChannel != null)
          {
            DVBSChannel oldChannels = (DVBSChannel)CurrentChannel;
            if (oldChannels.Equals(channel))
            {
              //@FIX this fails for back-2-back recordings
              //Log.Log.WriteFile("b2c2:already tuned on this channel");
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
          Log.Log.WriteFile("b2c2:  Polarity:{0} {1}", dvbsChannel.Polarisation, polarity);
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
              disType = B2C2DisEqcType.None;
              break;
            case DisEqcType.SimpleA: // Simple A
              disType = B2C2DisEqcType.Simple_A;
              break;
            case DisEqcType.SimpleB: // Simple B
              disType = B2C2DisEqcType.Simple_B;
              break;
            case DisEqcType.Level1AA: // Level 1 A/A
              disType = B2C2DisEqcType.Level_1_A_A;
              break;
            case DisEqcType.Level1BA: // Level 1 B/A
              disType = B2C2DisEqcType.Level_1_B_A;
              break;
            case DisEqcType.Level1AB: // Level 1 A/B
              disType = B2C2DisEqcType.Level_1_A_B;
              break;
            case DisEqcType.Level1BB: // Level 1 B/B
              disType = B2C2DisEqcType.Level_1_B_B;
              break;
          }
          switchFreq = lnbFrequency / 1000; //in MHz
          pmtPid = dvbsChannel.PmtPid;
          break;
        case CardType.DvbT:
          DVBTChannel dvbtChannel = channel as DVBTChannel;
          if (dvbtChannel == null)
          {
            Log.Log.Error("Channel is not a DVBT channel!!! {0}", channel.GetType().ToString());
            return false;
          }
          if (CurrentChannel != null)
          {
            DVBTChannel oldChannelt = (DVBTChannel)CurrentChannel;
            if (oldChannelt.Equals(channel))
            {
              //@FIX this fails for back-2-back recordings
              //Log.Log.WriteFile("b2c2:already tuned on this channel");
              //return _mapSubChannels[0];
            }
          }
          frequency = (int)dvbtChannel.Frequency;
          bandWidth = (BandWidthType)dvbtChannel.BandWidth;
          pmtPid = dvbtChannel.PmtPid;
          break;
        case CardType.DvbC:
          DVBCChannel dvbcChannel = channel as DVBCChannel;
          if (dvbcChannel == null)
          {
            Log.Log.Error("Channel is not a DVBC channel!!! {0}", channel.GetType().ToString());
            return false;
          }
          if (CurrentChannel != null)
          {
            DVBCChannel oldChannelc = (DVBCChannel)CurrentChannel;
            if (oldChannelc.Equals(channel))
            {
              //@FIX this fails for back-2-back recordings
              //Log.Log.WriteFile("b2c2:already tuned on this channel");
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
            return false;
          }
          if (CurrentChannel != null)
          {
            ATSCChannel oldChannela = (ATSCChannel)CurrentChannel;
            if (oldChannela.Equals(channel))
            {
              //@FIX this fails for back-2-back recordings
              //Log.Log.WriteFile("b2c2:already tuned on this channel");
              //return _mapSubChannels[0];
            }
          }
          //if modulation = 256QAM assume ATSC QAM for HD5000
          if (dvbaChannel.ModulationType == ModulationType.Mod256Qam)
          {
            Log.Log.WriteFile("b2c2:  ATSC Channel:{0} Frequency:{1}", dvbaChannel.PhysicalChannel,
                              dvbaChannel.Frequency);
            frequency = (int)dvbaChannel.Frequency;
            pmtPid = dvbaChannel.PmtPid;
          }
          else
          {
            Log.Log.WriteFile("b2c2:  ATSC Channel:{0}", dvbaChannel.PhysicalChannel);
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
            Log.Log.WriteFile("b2c2:  ATSC Frequency:{0} MHz", frequency);
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
      Log.Log.WriteFile("b2c2:  Transponder Frequency:{0} MHz", frequency);
      int hr = _interfaceB2C2TunerCtrl.SetFrequency(frequency);
      if (hr != 0)
      {
        Log.Log.Error("b2c2:SetFrequencyKHz() failed:0x{0:X}", hr);
        return false;
      }
      switch (_cardType)
      {
        case CardType.DvbC:
          Log.Log.WriteFile("b2c2:  SymbolRate:{0} KS/s", symbolRate);
          hr = _interfaceB2C2TunerCtrl.SetSymbolRate(symbolRate);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetSymbolRate() failed:0x{0:X}", hr);
            return false;
          }
          Log.Log.WriteFile("b2c2:  Modulation:{0}", ((eModulationTAG)modulation));
          hr = _interfaceB2C2TunerCtrl.SetModulation(modulation);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetModulation() failed:0x{0:X}", hr);
            return false;
          }
          break;
        case CardType.DvbT:
          Log.Log.WriteFile("b2c2:  GuardInterval:auto");
          hr = _interfaceB2C2TunerCtrl.SetGuardInterval((int)GuardIntervalType.Interval_Auto);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetGuardInterval() failed:0x{0:X}", hr);
            return false;
          }
          Log.Log.WriteFile("b2c2:  Bandwidth:{0} MHz", bandWidth);
          hr = _interfaceB2C2TunerCtrl.SetBandwidth((int)bandWidth);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetBandwidth() failed:0x{0:X}", hr);
            return false;
          }
          break;
        case CardType.DvbS:
          Log.Log.WriteFile("b2c2:  SymbolRate:{0} KS/s", symbolRate);
          hr = _interfaceB2C2TunerCtrl.SetSymbolRate(symbolRate);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetSymbolRate() failed:0x{0:X}", hr);
            return false;
          }
          Log.Log.WriteFile("b2c2:  Fec:{0} {1}", ((FecType)fec), fec);
          hr = _interfaceB2C2TunerCtrl.SetFec(fec);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetFec() failed:0x{0:X}", hr);
            return false;
          }
          hr = _interfaceB2C2TunerCtrl.SetPolarity(polarity);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetPolarity() failed:0x{0:X}", hr);
            return false;
          }
          Log.Log.WriteFile("b2c2:  Lnb:{0}", lnbSelection);
          hr = _interfaceB2C2TunerCtrl.SetLnbKHz((int)lnbSelection);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetLnbKHz() failed:0x{0:X}", hr);
            return false;
          }
          Log.Log.WriteFile("b2c2:  Diseqc:{0} {1}", disType, disType);
          hr = _interfaceB2C2TunerCtrl.SetDiseqc((int)disType);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetDiseqc() failed:0x{0:X}", hr);
            return false;
          }
          Log.Log.WriteFile("b2c2:  LNBFrequency:{0} MHz", switchFreq);
          hr = _interfaceB2C2TunerCtrl.SetLnbFrequency(switchFreq);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetLnbFrequency() failed:0x{0:X}", hr);
            return false;
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
      return true;
    }

    #endregion

    #region epg & scanning

    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        if (!CheckThreadId())
          return null;
        switch (CardType)
        {
          case CardType.DvbT:
            return new DVBTScanning(this);
          case CardType.DvbS:
            return new DVBSScanning(this);
          case CardType.DvbC:
            return new DVBCScanning(this);
          case CardType.Atsc:
            return new ATSCScanning(this);
        }
        return null;
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
    public override bool CanTune(IChannel channel)
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

    #region B2C2 specific

    ///<summary>
    /// Checks if the tuner is locked in and a sginal is present
    ///</summary>
    ///<returns>true, when the tuner is locked and a signal is present</returns>
    public override void LockInOnSignal()
    {
      bool isLocked = false;
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      while (!isLocked && ts.TotalSeconds < 2)
      {
        if (_cancelTune)
        {
          Log.Log.WriteFile("dvb:  LockInOnSignal tune cancelled");
          throw new TvExceptionTuneCancelled();
        }
        int hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
        _interfaceB2C2TunerCtrl.CheckLock();
        if (((uint)hr) == 0x90010115)
        {
          ts = DateTime.Now - timeStart;
          Log.Log.WriteFile("b2c2:  LockedInOnSignal waiting 20ms");
          System.Threading.Thread.Sleep(20);
        }
        else
        {
          isLocked = true;
        }
      }

      if (!isLocked)
      {
        Log.Log.WriteFile("b2c2:  LockedInOnSignal could not lock onto channel - no signal or bad signal");
        throw new TvExceptionNoSignal("Unable to tune to channel - no signal");
      }
      Log.Log.WriteFile("b2c2:  LockedInOnSignal ok");
    }

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
    {
      try
      {
        Log.Log.WriteFile("b2c2: build graph");
        if (_graphState != GraphState.Idle)
        {
          Log.Log.Error("b2c2: Graph already built");
          throw new TvException("Graph already built");
        }
        DevicesInUse.Instance.Add(_tunerDevice);
        _managedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _rotEntry = new DsROTEntry(_graphBuilder);
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        //=========================================================================================================
        // add the B2C2 specific filters
        //=========================================================================================================
        Log.Log.WriteFile("b2c2:CreateGraph() create B2C2 adapter");
        _filterB2C2Adapter =
          Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_B2C2Adapter, false)) as IBaseFilter;
        Log.Log.WriteFile("b2c2: Filter instance: " + _filterB2C2Adapter);
        if (_filterB2C2Adapter == null)
        {
          Log.Log.Error("b2c2:creategraph() _filterB2C2Adapter not found");
          DevicesInUse.Instance.Remove(_tunerDevice);
          return;
        }
        Log.Log.WriteFile("b2c2:creategraph() add filters to graph");
        int hr = _graphBuilder.AddFilter(_filterB2C2Adapter, "B2C2-Source");
        if (hr != 0)
        {
          Log.Log.Error("b2c2: FAILED to add B2C2-Adapter");
          DevicesInUse.Instance.Remove(_tunerDevice);
          return;
        }
        // get interfaces
        _interfaceB2C2TunerCtrl = _filterB2C2Adapter as IB2C2MPEG2TunerCtrl4;
        if (_interfaceB2C2TunerCtrl == null)
        {
          Log.Log.Error("b2c2: cannot get IB2C2MPEG2TunerCtrl4");
          DevicesInUse.Instance.Remove(_tunerDevice);
          return;
        }
        _interfaceB2C2DataCtrl = _filterB2C2Adapter as IB2C2MPEG2DataCtrl6;
        if (_interfaceB2C2DataCtrl == null)
        {
          Log.Log.Error("b2c2: cannot get IB2C2MPEG2DataCtrl6");
          DevicesInUse.Instance.Remove(_tunerDevice);
          return;
        }
        hr = _interfaceB2C2DataCtrl.SelectDevice(_deviceInfo.DeviceId);
        if (hr != 0)
        {
          Log.Log.Error("b2c2: select device failed: {0:X}", hr);
        }
        //=========================================================================================================
        // initialize B2C2 tuner
        //=========================================================================================================
        Log.Log.WriteFile("b2c2: Initialize Tuner()");
        hr = _interfaceB2C2TunerCtrl.Initialize();
        if (hr != 0)
        {
          //System.Diagnostics.Debugger.Launch();
          Log.Log.Error("b2c2: Tuner initialize failed:0x{0:X}", hr);
          // if the B2C2 card is detected as analogue, it needs a device reset 

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

          CardPresent = false;
          return;
        }
        // call checklock once, the return value dont matter
        hr = _interfaceB2C2TunerCtrl.CheckLock();
        AddTsWriterFilterToGraph();
        IBaseFilter lastFilter;
        ConnectInfTeeToB2C2(out lastFilter);
        AddMdPlugs(ref lastFilter);
        if (!ConnectTsWriter(lastFilter))
        {
          throw new TvExceptionGraphBuildingFailed("Graph building failed");
        }
        SendHwPids(new List<ushort>());
        _graphState = GraphState.Created;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw new TvExceptionGraphBuildingFailed("Graph building failed", ex);
      }
    }

    /// <summary>
    /// Connects the B2C2 filter to the infTee
    /// </summary>
    private void ConnectInfTeeToB2C2(out IBaseFilter lastFilter)
    {
      Log.Log.WriteFile("b2c2:add Inf Tee filter");
      _infTeeMain = (IBaseFilter)new InfTee();
      int hr = _graphBuilder.AddFilter(_infTeeMain, "Inf Tee");
      if (hr != 0)
      {
        Log.Log.Error("b2c2:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }

      Log.Log.WriteFile("b2c2:ConnectMainTee()");
      IPin pinOut = DsFindPin.ByDirection(_filterB2C2Adapter, PinDirection.Output, 2);
      IPin pinIn = DsFindPin.ByDirection(_infTeeMain, PinDirection.Input, 0);
      if (pinOut == null)
      {
        Log.Log.Error("b2c2:unable to find pin 2 of b2c2adapter");
        throw new TvException("unable to find pin 2 of b2c2adapter");
      }
      if (pinIn == null)
      {
        Log.Log.Error("b2c2:unable to find pin 0 of _infTeeMain");
        throw new TvException("unable to find pin 0 of _infTeeMain");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      Release.ComObject("b2c2pin2", pinOut);
      Release.ComObject("mpeg2demux pinin", pinIn);
      if (hr != 0)
      {
        Log.Log.Error("b2c2:unable to connect b2c2->_infTeeMain");
        throw new TvException("unable to connect b2c2->_infTeeMain");
      }
      lastFilter = _infTeeMain;
    }

    /// <summary>
    /// Sends the HW pids.
    /// </summary>
    /// <param name="pids">The pids.</param>
    public override void SendHwPids(List<ushort> pids)
    {
      const int PID_CAPTURE_ALL_INCLUDING_NULLS = 0x2000;
      //Enables reception of all PIDs in the transport stream including the NULL PID
      //const int PID_CAPTURE_ALL_EXCLUDING_NULLS = 0x2001;//Enables reception of all PIDs in the transport stream excluding the NULL PID.
      int pidCount = 39;
      int plOpen, plRunning;
      int[] statePids = new int[39];
      while (pidCount > 0)
      {
        int hr = _interfaceB2C2DataCtrl.GetTsState(out plOpen, out plRunning, ref pidCount, statePids);
        if (hr == 0)
        {
          if (pidCount > 0)
          {
            hr = _interfaceB2C2DataCtrl.DeletePIDsFromPin(pidCount, statePids, 0);
            if (hr != 0)
            {
              Log.Log.Error("b2c2:DeleteAllPIDs() - Step 2 - failed pid:0x2000");
            }
          }
        }
        else
        {
          Log.Log.Error("b2c2:DeleteAllPIDs() failed pid:0x2000");
        }
      }




      if (pids.Count == 0)
      {
        Log.Log.WriteFile("b2c2:hw pids:all");
        int count = 1;
        int[] pidsArray = new int[] { PID_CAPTURE_ALL_INCLUDING_NULLS };
        int hr = _interfaceB2C2DataCtrl.AddPIDsToPin(ref count, pidsArray, 0);
        if (hr != 0)
        {
          Log.Log.Error("b2c2:SetPidToPin() failed pid:0x2000");
        }
      }
      else
      {
        int maxPids;
        _interfaceB2C2DataCtrl.GetMaxPIDCount(out maxPids);
        for (int i = 0; i < pids.Count && i < maxPids; ++i)
        {
          ushort pid = pids[i];
          Log.Log.WriteFile("b2c2:hw pids:0x{0:X}", pid);
          int count = 1;
          int[] pidsArray = new int[] { PID_CAPTURE_ALL_INCLUDING_NULLS };
          int hr = _interfaceB2C2DataCtrl.AddPIDsToPin(ref count, pidsArray, 0);
          if (hr != 0)
          {
            Log.Log.Error("b2c2:SetPidToPin() failed pid:0x2000");
          }
        }
      }
      
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
      _interfaceB2C2TunerCtrl.GetSignalQuality(out quality);
      _interfaceB2C2TunerCtrl.GetSignalStrength(out level);
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
      Log.Log.WriteFile("b2c2: GetTunerCapabilities");
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graphBuilder);
      _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
      _capBuilder.SetFiltergraph(_graphBuilder);
      //=========================================================================================================
      // add the B2C2 specific filters
      //=========================================================================================================
      Log.Log.WriteFile("b2c2:GetTunerCapabilities() create B2C2 adapter");
      _filterB2C2Adapter =
        Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_B2C2Adapter, false)) as IBaseFilter;
      if (_filterB2C2Adapter == null)
      {
        Log.Log.Error("b2c2:GetTunerCapabilities() _filterB2C2Adapter not found");
        return;
      }
      _interfaceB2C2TunerCtrl = _filterB2C2Adapter as IB2C2MPEG2TunerCtrl4;
      if (_interfaceB2C2TunerCtrl == null)
      {
        Log.Log.Error("b2c2: cannot get IB2C2MPEG2TunerCtrl4");
        return;
      }

      _interfaceB2C2DataCtrl = _filterB2C2Adapter as IB2C2MPEG2DataCtrl6;
      if (_interfaceB2C2DataCtrl == null)
      {
        Log.Log.Error("b2c2: cannot get IB2C2MPEG2DataCtrl6");
        return;
      }

      int hr = _interfaceB2C2DataCtrl.SelectDevice(_deviceInfo.DeviceId);
      if (hr != 0)
      {
        Log.Log.Error("b2c2: select device failed: {0:X}", hr);
      }

      //=========================================================================================================
      // Get tuner type (DVBS, DVBC, DVBT, ATSC)
      //=========================================================================================================
      int lTunerCapSize = Marshal.SizeOf(typeof(tTunerCapabilities));
      IntPtr ptCaps = Marshal.AllocHGlobal(lTunerCapSize);
      hr = _interfaceB2C2TunerCtrl.GetTunerCapabilities(ptCaps, ref lTunerCapSize);
      if (hr != 0)
      {
        Log.Log.Error("b2c2: Tuner Type failed:0x{0:X}", hr);
        return;
      }
      tTunerCapabilities tc = (tTunerCapabilities)Marshal.PtrToStructure(ptCaps, typeof(tTunerCapabilities));
      switch (tc.eModulation)
      {
        case TunerType.ttSat:
          Log.Log.WriteFile("b2c2: Card type = DVBS");
          _cardType = CardType.DvbS;
          break;
        case TunerType.ttCable:
          Log.Log.WriteFile("b2c2: Card type = DVBC");
          _cardType = CardType.DvbC;
          break;
        case TunerType.ttTerrestrial:
          Log.Log.WriteFile("b2c2: Card type = DVBT");
          _cardType = CardType.DvbT;
          break;
        case TunerType.ttATSC:
          Log.Log.WriteFile("b2c2: Card type = ATSC");
          _cardType = CardType.Atsc;
          break;
        case TunerType.ttUnknown:
          Log.Log.WriteFile("b2c2: Card type = unknown?");
          _cardType = CardType.Unknown;
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

      base.Dispose();

      Log.Log.WriteFile("b2c2:Decompose");

      _interfaceB2C2DataCtrl = null;
      _interfaceB2C2TunerCtrl = null;

      if (_filterB2C2Adapter != null)
      {
        Release.ComObject("tuner filter", _filterB2C2Adapter);
        _filterB2C2Adapter = null;
      }
    }

    #endregion

    #region IDiSEqCController Members

    private void DisEqcGotoPosition(byte position)
    {
      _disEqcMotor.GotoPosition(position);
    }

    /// <summary>
    /// Handles DiSEqC motor operations
    /// </summary>
    public override IDiSEqCMotor DiSEqCMotor
    {
      get { return _disEqcMotor; }
    }

    /// <summary>
    /// Send the DiSEqC Command to the tuner filter
    /// </summary>
    /// <param name="diSEqC">DiSEqC command</param>
    /// <returns></returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      if (_interfaceB2C2TunerCtrl == null)
        return false;
      for (int i = 0; i < diSEqC.Length; ++i)
      {
        Marshal.WriteByte(_ptrDisEqc, i, diSEqC[i]);
      }
      _interfaceB2C2TunerCtrl.SendDiSEqCCommand(diSEqC.Length, _ptrDisEqc);
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

    protected override DVBBaseChannel CreateChannel(int networkid, int transportid, int serviceid, string name)
    {
      DVBSChannel channel = new DVBSChannel();
      channel.NetworkId = networkid;
      channel.TransportId = transportid;
      channel.ServiceId = serviceid;
      channel.Name = name;
      return channel;
    }

    public static void GetTunerInformation(out uint numberOfTuners, out List<DeviceInfo> deviceList)
    {
      numberOfTuners = 0;
      deviceList = new List<DeviceInfo>();
      //=========================================================================================================
      // add the B2C2 specific filters
      //=========================================================================================================
      IBaseFilter filterB2C2Adapter =
        Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_B2C2Adapter, false)) as IBaseFilter;
      if (filterB2C2Adapter == null)
      {
        Log.Log.Error("b2c2:GetTunerCapabilities() _filterB2C2Adapter not found");
        return;
      }
      IB2C2MPEG2DataCtrl6 interfaceB2C2DataCtrl = filterB2C2Adapter as IB2C2MPEG2DataCtrl6;
      if (interfaceB2C2DataCtrl == null)
      {
        Log.Log.Error("b2c2: cannot get IB2C2MPEG2DataCtrl6");
        return;
      }
      int deviceInfoSize = Marshal.SizeOf(typeof(tagDEVICE_INFORMATION)) * 16;
      IntPtr ptDeviceInfo = Marshal.AllocHGlobal(deviceInfoSize);
      numberOfTuners = 16;
      int hr = interfaceB2C2DataCtrl.GetDeviceList(ptDeviceInfo, ref deviceInfoSize, ref numberOfTuners);
      if (hr != 0)
      {
        Log.Log.Error("b2c2: GetDeviceList failed:0x{0:X}", hr);
      }
      Log.Log.WriteFile("Result: " + numberOfTuners);
      long currentPtr = ptDeviceInfo.ToInt64(); // Must work both on x86 and x64
      for (int i = 0; i < numberOfTuners; i++)
      {
        IntPtr RectPtr = new IntPtr(currentPtr);
        tagDEVICE_INFORMATION tc = (tagDEVICE_INFORMATION)Marshal.PtrToStructure(RectPtr, typeof(tagDEVICE_INFORMATION));
        CardType cardType = CardType.Unknown;
        switch (tc.eTunerModulation)
        {
          case TunerType.ttSat:
            cardType = CardType.DvbS;
            break;
          case TunerType.ttCable:
            cardType = CardType.DvbC;
            break;
          case TunerType.ttTerrestrial:
            cardType = CardType.DvbT;
            break;
          case TunerType.ttATSC:
            cardType = CardType.Atsc;
            break;
          case TunerType.ttUnknown:
            cardType = CardType.Unknown;
            break;
        }

        DeviceInfo device = new DeviceInfo
                              {
                                DeviceId = tc.dwDeviceID,
                                Name = tc.wsProductName + " (" + tc.wsProductRevision + ")",
                                CardType = cardType,
                                DevicePath =
                                  String.Format("device:{0}_{1}_{2}_[{3:X02}:{4:X02}:{5:X02}:{6:X02}:{7:X02}:{8:X02}]",
                                                tc.eBusInterface,
                                                tc.dwDeviceID, tc.dwProductID, tc.ucMACAddress[0], tc.ucMACAddress[1],
                                                tc.ucMACAddress[2],
                                                tc.ucMACAddress[3], tc.ucMACAddress[4], tc.ucMACAddress[5])
                              };
        deviceList.Add(device);
        currentPtr += Marshal.SizeOf(typeof(tagDEVICE_INFORMATION));
      }
      Marshal.FreeHGlobal(ptDeviceInfo);
      // Release all used object
      Release.ComObject("tuner filter", filterB2C2Adapter);
    }
  }
}