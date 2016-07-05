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
using DirectShowLib;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;
using DirectShowLib.BDA;
using TvDatabase;
using TvLibrary.Hardware;
using TvLibrary.Implementations.Dri;
using TvLibrary.Implementations.Dri.Service;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which handles the conditional access modules for a tv card
  /// (CI and CAM)
  /// </summary>
  public class ConditionalAccess : IDisposable
  {
    #region variables

    private readonly bool _useCam;

    /// <summary>
    /// CA decryption limit, 0 for disable CA
    /// </summary>
    private readonly int _decryptLimit;

    private readonly CamType _CamType = CamType.Default;
    private readonly DigitalEverywhere _digitalEveryWhere;
    private readonly TechnoTrendAPI _technoTrend;
    private readonly Twinhan _twinhan;
    private readonly KNCAPI _knc;
    private readonly Hauppauge _hauppauge;
    private readonly ProfRed _profred;
    private readonly DiSEqCMotor _diSEqCMotor;
    private readonly Dictionary<int, ConditionalAccessContext> _mapSubChannels;
    private readonly GenericBDAS _genericbdas;
    private readonly WinTvCiModule _winTvCiModule;
    private readonly GenericATSC _isgenericatsc;
    private readonly ViXSATSC _isvixsatsc;
    private readonly ConexantBDA _conexant;
    private readonly GenPixBDA _genpix;
    private readonly TeVii _TeVii;
    private readonly DigitalDevices _DigitalDevices;
    private readonly TunerDri _tunerDri;
    private readonly TvCardDvbB2C2 _b2c2Card;
    private readonly IHardwareProvider _HWProvider;

    private readonly ICiMenuActions _ciMenu;


    /// <summary>
    /// Accessor for CI Menu handler
    /// </summary>
    public ICiMenuActions CiMenu
    {
      get { return _ciMenu; }
    }

    /// <summary>
    /// Accessor for hardware providers.
    /// Used only for non-standard methods (i.e. vendor APIs)
    /// </summary>
    public IHardwareProvider HWProvider
    {
      get { return _HWProvider; }
    }

    //anysee _anysee = null;

    #endregion

    //ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalAccess"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The capture filter.</param>
    /// <param name="winTvUsbCiFilter">The WinTV CI filter.</param>
    /// <param name="card">Determines the type of TV card</param>    
    public ConditionalAccess(IBaseFilter tunerFilter, IBaseFilter analyzerFilter, IBaseFilter winTvUsbCiFilter,
                             TvCardBase card)
    {
      try
      {
        //System.Diagnostics.Debugger.Launch();        
        if (card != null && card.DevicePath != null)
        {
          //fetch decrypt limit from DB and apply it.
          TvBusinessLayer layer = new TvBusinessLayer();
          Card c = layer.GetCardByDevicePath(card.DevicePath);
          _decryptLimit = c.DecryptLimit;
          _useCam = c.CAM;
          _CamType = (CamType)c.CamType;
          Log.Log.WriteFile("CAM is {0} model", _CamType);
        }
        if (card is TvCardDvbB2C2)
        {
          Log.Log.WriteFile("B2C2 card detected in ConditionalAccess");
          _b2c2Card = card as TvCardDvbB2C2;
        }

        _mapSubChannels = new Dictionary<int, ConditionalAccessContext>();
        TunerDri driTuner = card as TunerDri;
        if (driTuner != null)
        {
          _tunerDri = driTuner;
          _ciMenu = driTuner;
        }
        if (tunerFilter == null && analyzerFilter == null)
          return;
        //DVB checks. Conditional Access & DiSEqC etc.
        bool isDVBS = (card is TvCardDVBS);
        bool isDVBT = (card is TvCardDVBT);
        bool isDVBC = (card is TvCardDVBC);

        if (isDVBC || isDVBS || isDVBT)
        {
           Log.Log.WriteFile("Check for KNC");
          // Lookup device index of current card. only counting KNC cards by device path
          int DeviceIndex = KNCDeviceLookup.GetDeviceIndex(card);
          _knc = new KNCAPI(tunerFilter, (uint)DeviceIndex);
          if (_knc.IsKNC)
          {
            //if (_knc.IsCamReady()) 
            _ciMenu = _knc; // Register KNC CI Menu capabilities when CAM detected and ready
            Log.Log.WriteFile("KNC card detected");
            return;
          }
          Release.DisposeToNull(ref _knc);

          Log.Log.WriteFile("Check for Digital Everywhere");
          _digitalEveryWhere = new DigitalEverywhere(tunerFilter);
          if (_digitalEveryWhere.IsDigitalEverywhere)
          {
            Log.Log.WriteFile("Digital Everywhere card detected");
            _diSEqCMotor = new DiSEqCMotor(_digitalEveryWhere);

            if (_digitalEveryWhere.IsCamReady())
            {
              Log.Log.WriteFile("Digital Everywhere registering CI menu capabilities");
              _ciMenu = _digitalEveryWhere; // Register FireDTV CI Menu capabilities when CAM detected and ready
            }
            //_digitalEveryWhere.ResetCAM();
            return;
          }
          Release.DisposeToNull(ref _digitalEveryWhere);

          Log.Log.WriteFile("Check for Twinhan");
          _twinhan = new Twinhan(tunerFilter);
          if (_twinhan.IsTwinhan)
          {
            Log.Log.WriteFile("Twinhan card detected");
            _diSEqCMotor = new DiSEqCMotor(_twinhan);
            Log.Log.WriteFile("Twinhan registering CI menu capabilities");
            _ciMenu = _twinhan; // Register Twinhan CI Menu capabilities when CAM detected and ready
            return;
          }
          Release.DisposeToNull(ref _twinhan);

          Log.Log.WriteFile("Check for TechnoTrend");
          _technoTrend = new TechnoTrendAPI(tunerFilter);
          if (_technoTrend.IsTechnoTrend)
          {
            ////if (_technoTrend.IsCamPresent()) 
            _ciMenu = _technoTrend; // Register Technotrend CI Menu capabilities
            Log.Log.WriteFile("TechnoTrend card detected");
            return;
          }
          Release.DisposeToNull(ref _technoTrend);

          Log.Log.WriteFile("Check for Hauppauge");
          _hauppauge = new Hauppauge(tunerFilter);
          if (_hauppauge.IsHauppauge)
          {
            Log.Log.WriteFile("Hauppauge card detected");
            Log.Log.WriteFile("Check for Hauppauge WinTV CI");
            if (winTvUsbCiFilter != null)
            {
              Log.Log.WriteFile("WinTV CI detected in graph - using capabilities...");
              _winTvCiModule = new WinTvCiModule(winTvUsbCiFilter);

              Log.Log.WriteFile("WinTV CI registering CI menu capabilities");
              _ciMenu = _winTvCiModule; // WinTv CI Menu capabilities 
            }
            _diSEqCMotor = new DiSEqCMotor(_hauppauge);
            return;
          }
          Release.DisposeToNull(ref _hauppauge);
          Release.DisposeToNull(ref _winTvCiModule);

          /*Log.Log.Info("Check for anysee");
          _anysee = new anysee(tunerFilter, analyzerFilter);
          if (_anysee.Isanysee)
          {
            Log.Log.Info("anysee device detected");
            return;
          }*/

          Log.Log.WriteFile("Check for ProfRed");
          _profred = new ProfRed(tunerFilter);
          if (_profred.IsProfRed)
          {
            Log.Log.WriteFile("ProfRed card detected");
            _diSEqCMotor = new DiSEqCMotor(_profred);
            return;
          }
          Release.DisposeToNull(ref _profred);

          // TeVii support
          _TeVii = new TeVii();
          _TeVii.Init(tunerFilter);
          _TeVii.DevicePath = card.DevicePath;
          Log.Log.WriteFile("Check for {0}", _TeVii.Provider);
          _TeVii.CheckAndOpen();
          if (_TeVii.IsSupported)
          {
            _diSEqCMotor = new DiSEqCMotor(_TeVii);
            _HWProvider = _TeVii;
            Log.Log.WriteFile("Check for Hauppauge WinTV CI");
            if (winTvUsbCiFilter != null)
            {
              Log.Log.WriteFile("WinTV CI detected in graph - using capabilities...");
              _winTvCiModule = new WinTvCiModule(winTvUsbCiFilter);
            }
            return;
          }
          Release.DisposeToNull(ref _TeVii);

          // DigitalDevices support
          _DigitalDevices = new DigitalDevices(tunerFilter);
          if (_DigitalDevices.IsGenericBDAS)
          {
            _genericbdas = _DigitalDevices;
            if (_DigitalDevices.IsSupported)
            {
              _ciMenu = _DigitalDevices;
            }
            return; // detected
          }
          Release.DisposeToNull(ref _DigitalDevices);

          Log.Log.WriteFile("Check for Conexant based card");
          _conexant = new ConexantBDA(tunerFilter);
          if (_conexant.IsConexant)
          {
            Log.Log.WriteFile("Conexant BDA card detected");
            Log.Log.WriteFile("Check for Hauppauge WinTV CI");
            if (winTvUsbCiFilter != null)
            {
              Log.Log.WriteFile("WinTV CI detected in graph - using capabilities...");
              _winTvCiModule = new WinTvCiModule(winTvUsbCiFilter);
            }
            return;
          }
          Release.DisposeToNull(ref _conexant);
          Release.DisposeToNull(ref _winTvCiModule);

          Log.Log.WriteFile("Check for GenPix BDA based card");
          _genpix = new GenPixBDA(tunerFilter);
          if (_genpix.IsGenPix)
          {
            Log.Log.WriteFile("GenPix BDA card detected");
            Log.Log.WriteFile("Check for Hauppauge WinTV CI");
            if (winTvUsbCiFilter != null)
            {
              Log.Log.WriteFile("WinTV CI detected in graph - using capabilities...");
              _winTvCiModule = new WinTvCiModule(winTvUsbCiFilter);
            }
            return;
          }
          Release.DisposeToNull(ref _genpix);
          Release.DisposeToNull(ref _winTvCiModule);

          Log.Log.WriteFile("Check for Generic DVB-S card");
          _genericbdas = new GenericBDAS(tunerFilter);
          if (_genericbdas.IsGenericBDAS)
          {
            Log.Log.WriteFile("Generic BDA card detected");
            Log.Log.WriteFile("Check for Hauppauge WinTV CI");
            if (winTvUsbCiFilter != null)
            {
              Log.Log.WriteFile("WinTV CI detected in graph - using capabilities...");
              _winTvCiModule = new WinTvCiModule(winTvUsbCiFilter);
            }
            return;
          }
          Release.DisposeToNull(ref _genericbdas);

          //Final WinTV-CI check for DVB-T hybrid cards
          Log.Log.WriteFile("Check for Hauppauge WinTV CI");
          if (winTvUsbCiFilter != null)
          {
            Log.Log.WriteFile("WinTV CI detected in graph - using capabilities...");
            _winTvCiModule = new WinTvCiModule(winTvUsbCiFilter);
            return;
          }
          Release.DisposeToNull(ref _winTvCiModule);
        }

        //ATSC checks
        bool isATSC = (card is TvCardATSC);
        if (isATSC)
        {
          Log.Log.WriteFile("Check for ViXS ATSC QAM card");
          _isvixsatsc = new ViXSATSC(tunerFilter);
          if (_isvixsatsc.IsViXSATSC)
          {
            Log.Log.WriteFile("ViXS ATSC QAM card detected");
            return;
          }
          Release.DisposeToNull(ref _isvixsatsc);

          Log.Log.WriteFile("Check for Generic ATSC QAM card");
          _isgenericatsc = new GenericATSC(tunerFilter);
          if (_isgenericatsc.IsGenericATSC)
          {
            Log.Log.WriteFile("Generic ATSC QAM card detected");
            return;
          }
          Release.DisposeToNull(ref _isgenericatsc);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }

    /// <summary>
    /// Adds the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="channel">The channel</param>
    public void AddSubChannel(int id, IChannel channel)
    {
      if (!_mapSubChannels.ContainsKey(id))
      {
        _mapSubChannels[id] = new ConditionalAccessContext();
        if (channel is DVBBaseChannel)
        {
          _mapSubChannels[id].Channel = (DVBBaseChannel)channel;
        }
      }
    }

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    public void FreeSubChannel(int id)
    {
      if (_mapSubChannels.ContainsKey(id))
      {
        Log.Log.WriteFile("FreeSubChannel CA: freeing sub channel : {0}", id);
        _mapSubChannels.Remove(id);
      }
      else
      {
        Log.Log.WriteFile("FreeSubChannel CA: tried to free non existing sub channel : {0}", id);
      }
    }

    /// <summary>
    /// Gets a value indicating whether we are allowed to stop the graph
    /// Some devices like the technotrend cards have a very long start up time
    /// Stopping/starting graphs would mean using these cards is not very userfriendly
    /// </summary>
    /// <value><c>true</c> if allowed to stop graph; otherwise, <c>false</c>.</value>
    public bool AllowedToStopGraph
    {
      get
      {
        //if (_technoTrend != null)
        //{
        //  return false;
        //}
        if (_twinhan != null)
        {
          if (_twinhan.IsCamPresent())
            return false;
        }

        return true;
      }
    }

    /// <summary>
    /// Gets the interface for controlling the DiSeQC motor.
    /// </summary>
    /// <value>IDiSEqCMotor.</value>
    public IDiSEqCMotor DiSEqCMotor
    {
      get { return _diSEqCMotor; }
    }

    /// <summary>
    /// returns if cam is ready or not
    /// </summary>
    public bool IsCamReady()
    {
      try
      {
        if (!_useCam)
          return true;

        if (_tunerDri != null)
        {
          return _tunerDri.CardStatus == DriCasCardStatus.Inserted;
        }

        if (_knc != null)
        {
          //Log.Log.WriteFile("KNC IsCamReady(): IsCamPresent:{0}, IsCamReady:{1}", _knc.IsCamPresent(), _knc.IsCamReady());
          return _knc.IsCamReady();
        }
        if (_digitalEveryWhere != null)
        {
          _digitalEveryWhere.IsCamReady();
        }
        if (_twinhan != null)
        {
          return _twinhan.IsCamReady();
        }
        if (_technoTrend != null)
        {
          Log.Log.WriteFile("TechnoTrend IsCamReady(): IsCamPresent:{0}, IsCamReady:{1}", _technoTrend.IsCamPresent(),
                            _technoTrend.IsCamReady());
          if (_technoTrend.IsCamPresent() == false)
          {
            return _technoTrend.IsCamPresent();
          }
          return _technoTrend.IsCamReady();
        }
        /*if (_anysee != null)
        {
          return _anysee.IsCamReady();
        }*/
        if (_winTvCiModule != null)
        {
          int hr = _winTvCiModule.Init();
          if (hr != 0)
            return false;
          Log.Log.Info("WinTVCI:  CAM initialized");
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return true;
    }

    /// <summary>
    /// resets the CAM
    /// </summary>
    public void ResetCAM()
    {
      try
      {
        if (!_useCam)
          return;
        if (_digitalEveryWhere != null)
        {
          _digitalEveryWhere.ResetCAM();
        }
        if (_technoTrend != null)
        {
          _technoTrend.ResetCI();
        }
        if (_knc != null)
        {
          _knc.ResetCI();
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }

    ///<summary>
    /// Called when the graph is started
    ///</summary>
    ///<param name="servicedId">The service id</param>
    ///<returns></returns>
    public bool OnRunGraph(int servicedId)
    {
      if (_digitalEveryWhere != null)
      {
        _digitalEveryWhere.OnStartGraph();
      }
      return true;
    }

    /// <summary>
    /// Called when the graph is stopped
    /// </summary>
    public void OnStopGraph()
    {
      if (_digitalEveryWhere != null)
      {
        _digitalEveryWhere.OnStopGraph();
      }
    }


    /// <summary>
    /// CA enabled or disabled ?
    /// </summary>
    /// <value>Is CA enabled or disabled</value>
    public bool UseCA
    {
      get { return _useCam; }
    }

    /// <summary>
    /// CA decryption limit, 0 for unlimited
    /// </summary>
    /// <value>The number of channels decrypting that are able to decrypt.</value>
    public int DecryptLimit
    {
      get { return _decryptLimit; }
    }

    /// <summary>
    /// Gets the number of channels the card is currently decrypting.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {
        if (_mapSubChannels == null)
          return 0;
        if (_mapSubChannels.Count == 0)
          return 0;
        if (_decryptLimit == 0)
          return 0; //CA disabled, so no channels are decrypting.

        List<ConditionalAccessContext> filteredChannels = new List<ConditionalAccessContext>();

        Dictionary<int, ConditionalAccessContext>.Enumerator en = _mapSubChannels.GetEnumerator();

        while (en.MoveNext())
        {
          bool exists = false;
          ConditionalAccessContext context = en.Current.Value;
          if (context != null)
          {
            foreach (ConditionalAccessContext c in filteredChannels)
            {
              if (c.Channel != null && context.Channel != null)
              {
                if (c.Channel.Equals(context.Channel))
                {
                  exists = true;
                  break;
                }
              }
            }
            if (!exists)
            {
              if (context.Channel != null && !context.Channel.FreeToAir)
              {
                filteredChannels.Add(context);
              }
            }
          }
        }
        return filteredChannels.Count;
      }
    }

    /// <summary>
    /// Patches the PMT to force standard AC3 header.
    /// </summary>
    /// <param name="PMT">byte array containing the PMT</param>
    /// <param name="pmtLength">length of the pmt array</param>
    /// <param name="newPmtLength">The new PMT length</param>
    /// <returns></returns>
    private static byte[] PatchPMT_AstonCrypt2(byte[] PMT, int pmtLength, out int newPmtLength)
    {
      byte[] newPMT = new byte[pmtLength]; // create a new array.

      Log.Log.Info("Conditional Access:  PatchPMT_AstonCrypt2 : pmtLength {0}", pmtLength);

      int ps = 0;
      int pd = 0;

      for (int i = 0; i < 12; ++i)
        newPMT[pd++] = PMT[ps++];
      for (int i = 0; i < PMT[11]; ++i)
        newPMT[pd++] = PMT[ps++];

      // Need to patch audio AC3 channels 0x06, , , , ,0x6A in real AC3 descriptor 0x81, .... for ( at least !) ASTONCRYPT CAM module
      while ((ps + 5 < pmtLength) && (pd < pmtLength))
      {
        int len = PMT[ps + 4] + 5;
        for (int i = 0; i < len; ++i)
        {
          if (pd >= pmtLength)
            break;
          if ((i == 0) && (PMT[ps] == 0x06) && (PMT[ps + 5] == 0x6A))
          {
            newPMT[pd++] = 0x81;
            ps++;
          }
          else
            newPMT[pd++] = PMT[ps++];
        }
      }

      newPmtLength = pd;
      return newPMT;
    }

    /// <summary>
    /// Sends the PMT to the CI module
    /// </summary>
    /// <param name="subChannel">The sub channel.</param>
    /// <param name="channel">channel on which we are tuned</param>
    /// <param name="PMT">byte array containing the PMT</param>
    /// <param name="pmtLength">length of the pmt array</param>
    /// <param name="audioPid">pid of the current audio stream</param>
    /// <returns></returns>
    public bool SendPMT(int subChannel, DVBBaseChannel channel, byte[] PMT, int pmtLength, int audioPid)
    {
      try
      {
        if (!_useCam)
          return true;
        if (channel.FreeToAir)
          return true; //no need to descramble this one...

        AddSubChannel(subChannel, channel);
        ConditionalAccessContext context = _mapSubChannels[subChannel];
        context.CamType = _CamType;
        context.Channel = channel;
        if (_CamType == CamType.Astoncrypt2)
        {
          int newLength;
          context.PMT = PatchPMT_AstonCrypt2(PMT, pmtLength, out newLength);
          context.PMTLength = newLength;
        }
        else
        {
          context.PMT = PMT;
          context.PMTLength = pmtLength;
        }
        context.AudioPid = audioPid;
        context.ServiceId = channel.ServiceId;

        if (_winTvCiModule != null)
        {
          int hr = _winTvCiModule.SendPMT(PMT, pmtLength);
          if (hr != 0)
          {
            Log.Log.Info("Conditional Access:  sendPMT to WinTVCI failed");
            return false;
          }
          Log.Log.Info("Conditional Access:  sendPMT to WinTVCI succeeded");
          return true;
        }
        if (_knc != null)
        {
          ChannelInfo info = new ChannelInfo();
          info.DecodePmt(PMT);
          int caPmtLen;
          byte[] caPmt = info.caPMT.CaPmtStruct(out caPmtLen);
          return _knc.SendPMT(caPmt, caPmtLen);
        }
        if (_DigitalDevices != null)
        {
          return _DigitalDevices.SendServiceIdToCam(channel.ServiceId);
        }
        if (_digitalEveryWhere != null)
        {
          return _digitalEveryWhere.SendPMTToFireDTV(_mapSubChannels);
        }
        if (_technoTrend != null)
        {
          return _technoTrend.DescrambleMultiple(_mapSubChannels);
          // return _technoTrend.SendPMT(PMT, pmtLength);
        }
        if (_twinhan != null)
        {
          ChannelInfo info = new ChannelInfo();
          info.DecodePmt(PMT);

          int caPmtLen;
          byte[] caPmt = info.caPMT.CaPmtStruct(out caPmtLen);
          return _twinhan.SendPMT(caPmt, caPmtLen);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return true;
    }

    /// <summary>
    /// sends the diseqc command to the card
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="channel">The current tv/radio channel</param>
    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      bool succeeded = true;
      try
      {
        if (_knc != null)
        {
          _knc.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_digitalEveryWhere != null)
        {
          _digitalEveryWhere.SendDiseqcCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_technoTrend != null)
        {
          _technoTrend.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_twinhan != null)
        {
          _twinhan.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_hauppauge != null)
        {
          succeeded = _hauppauge.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_genericbdas != null)
        {
          _genericbdas.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_conexant != null)
        {
          _conexant.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_profred != null)
        {
          _profred.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_genpix != null)
        {
          _genpix.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_TeVii != null)
        {
          _TeVii.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return succeeded;
    }

    /// <summary>
    /// Instructs the cam/ci module to use hardware filter and only send the pids listed in pids to the pc
    /// </summary>
    /// <param name="subChannel">The sub channel id</param>
    /// <param name="channel">The current tv/radio channel.</param>
    /// <param name="pids">The pids.</param>
    /// <remarks>when the pids array is empty, pid filtering is disabled and all pids are received</remarks>
    public void SendPids(int subChannel, DVBBaseChannel channel, List<ushort> pids)
    {
      try
      {
        List<ushort> HwPids = new List<ushort>();

        _mapSubChannels[subChannel].HwPids = pids;

        Dictionary<int, ConditionalAccessContext>.Enumerator enSubch = _mapSubChannels.GetEnumerator();
        while (enSubch.MoveNext())
        {
          List<ushort> enPid = enSubch.Current.Value.HwPids;
          if (enPid != null)
          {
            for (int i = 0; i < enPid.Count; ++i)
            {
              if (!HwPids.Contains(enPid[i]))
                HwPids.Add(enPid[i]);
            }
          }
        }
        if(_b2c2Card!=null)
        {
          _b2c2Card.SendHwPids(HwPids);
          /*
          bool isDvbs = ((channel as DVBSChannel) != null);
          if (isDvbs &&
              (((DVBSChannel)channel).ModulationType == ModulationType.Mod8Psk ||
              ((DVBSChannel)channel).ModulationType == ModulationType.Mod16Apsk ||
              ((DVBSChannel)channel).ModulationType == ModulationType.Mod32Apsk)
          )
          {
            _b2c2Card.SendHwPids(HwPids);
          }else
          {
            _b2c2Card.SendHwPids(new List<ushort>());
          }
           */
        }
        if (_digitalEveryWhere != null)
        {
          bool isDvbc = ((channel as DVBCChannel) != null);
          bool isDvbt = ((channel as DVBTChannel) != null);
          bool isDvbs = ((channel as DVBSChannel) != null);
          bool isAtsc = ((channel as ATSCChannel) != null);

          // It is not ideal to have to enable hardware PID filtering because
          // doing so can limit the number of channels that can be viewed/recorded
          // simultaneously. However, it does seem that there is a need for filtering
          // on transponders with high data rates. Problems have been observed with
          // transponders on Thor 5/6, Intelsat 10-02 (0.8W) if the filter is not enabled:
          //   Symbol Rate: 27500, Modulation: 8 PSK, FEC rate: 5/6, Pilot: On, Roll-Off: 0.35
          //   Symbol Rate: 30000, Modulation: 8 PSK, FEC rate: 3/4, Pilot: On, Roll-Off: 0.35
          if (pids.Count != 0 && isDvbs &&
              (((DVBSChannel)channel).ModulationType == ModulationType.Mod8Psk ||
              ((DVBSChannel)channel).ModulationType == ModulationType.Mod16Apsk ||
              ((DVBSChannel)channel).ModulationType == ModulationType.Mod32Apsk)
          )
          {
            for (int i = 0; i < HwPids.Count; ++i)
            {
              Log.Log.Info("FireDTV: HW Filtered Pid : 0x{0:X}", HwPids[i]);
            }
            _digitalEveryWhere.SetHardwarePidFiltering(isDvbc, isDvbt, true, isAtsc, HwPids);
          }
          else
          {
            pids.Clear();
            Log.Log.Info("FireDTV: HW Filtering disabled.");
            _digitalEveryWhere.SetHardwarePidFiltering(isDvbc, isDvbt, isDvbs, isAtsc, pids);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }

    /// <summary>
    /// Sets the DVB-S2 parameters such as modulation, roll-off, pilot etc.
    /// </summary>
    /// <param name="parameters">The LNB parameters.</param>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with DVB-S2 parameters set.</returns>
    public DVBSChannel SetDVBS2Modulation(ScanParameters parameters, DVBSChannel channel)
    {
      //Log.Log.WriteFile("Trying to set DVB-S2 modulation...");
      try
      {
        if (_twinhan != null)
        {
          //DVB-S2 modulation parameters for Twinhan
          if (channel.ModulationType == ModulationType.ModQpsk)
          {
            channel.ModulationType = ModulationType.Mod8Vsb;
          }
          if (channel.ModulationType == ModulationType.Mod8Psk)
          {
            channel.ModulationType = ModulationType.Mod8Vsb;
          }
          if (channel.ModulationType == ModulationType.Mod16Apsk)
          {
            channel.ModulationType = ModulationType.Mod16Vsb;
          }
          if (channel.ModulationType == ModulationType.Mod32Apsk)
          {
            channel.ModulationType = ModulationType.ModOqpsk;
          }
          Log.Log.WriteFile("Twinhan DVB-S2 modulation set to:{0}", channel.ModulationType);
          Log.Log.WriteFile("Twinhan DVB-S2 Pilot set to:{0}", channel.Pilot);
          Log.Log.WriteFile("Twinhan DVB-S2 RollOff set to:{0}", channel.Rolloff);
          Log.Log.WriteFile("Twinhan DVB-S2 fec set to:{0}", channel.InnerFecRate);
          return channel;
        }
        if (_hauppauge != null)
        {
          //Set Hauppauge pilot, roll-off settings but only if DVB-S2
          //We assume if the modulation is set then a DVB-S2 tuning request has been requested
          if (channel.ModulationType != ModulationType.ModNotSet)
          {
            //Set the alternative Hauppauge Modulation type
            if (channel.ModulationType == ModulationType.ModQpsk)
            {
              if (channel.InnerFecRate == BinaryConvolutionCodeRate.Rate9_10)
              {
                channel.ModulationType = ModulationType.Mod32Qam;
              }
              channel.ModulationType = channel.InnerFecRate == BinaryConvolutionCodeRate.Rate8_9
                                         ? ModulationType.Mod16Qam
                                         : ModulationType.ModBpsk;
            }
            //Set the Hauppauge Modulation type
            /*if (channel.ModulationType == ModulationType.ModQpsk)
            {
              channel.ModulationType = ModulationType.ModQpsk2;
            }*/
            if (channel.ModulationType == ModulationType.Mod8Psk)
            {
              channel.ModulationType = ModulationType.ModNbc8Psk;
            }
            if (channel.SymbolRate == 30000)
            {
              channel.Pilot = Pilot.Off;
            }
            Log.Log.WriteFile("Hauppauge DVB-S2 modulation set to:{0}", channel.ModulationType);
            Log.Log.WriteFile("Hauppauge DVB-S2 Pilot set to:{0}", channel.Pilot);
            Log.Log.WriteFile("Hauppauge DVB-S2 RollOff set to:{0}", channel.Rolloff);
            Log.Log.WriteFile("Hauppauge DVB-S2 fec set to:{0}", channel.InnerFecRate);
            _hauppauge.SetDVBS2PilotRolloff(channel);
          }
          return channel;
        }
        if (_profred != null)
        {
          //Set ProfRed pilot, roll-off settings
          if (channel.ModulationType == ModulationType.ModNotSet)
          {
            channel.ModulationType = ModulationType.ModNotDefined;
          }
          if (channel.ModulationType == ModulationType.Mod8Psk)
          {
            channel.ModulationType = ModulationType.ModBpsk;
          }
          Log.Log.WriteFile("ProfRed DVB-S2 modulation set to:{0}", channel.ModulationType);
          Log.Log.WriteFile("ProfRed DVB-S2 Pilot set to:{0}", channel.Pilot);
          Log.Log.WriteFile("ProfRed DVB-S2 RollOff set to:{0}", channel.Rolloff);
          Log.Log.WriteFile("ProfRed DVB-S2 fec set to:{0}", channel.InnerFecRate);
          //}
          return channel;
        }
        if (_technoTrend != null)
        {
          //Set TechnoTrend modulation tuning settings
          if (channel.ModulationType == ModulationType.ModQpsk)
          {
            channel.ModulationType = ModulationType.Mod8Vsb;
          }
          if (channel.ModulationType == ModulationType.Mod8Psk)
          {
            channel.ModulationType = ModulationType.Mod8Vsb;
          }
          if (channel.ModulationType == ModulationType.Mod16Apsk)
          {
            channel.ModulationType = ModulationType.Mod16Vsb;
          }
          if (channel.ModulationType == ModulationType.Mod32Apsk)
          {
            channel.ModulationType = ModulationType.ModOqpsk;
          }
          Log.Log.WriteFile("Technotrend DVB-S2 modulation set to:{0}", channel.ModulationType);
          Log.Log.WriteFile("Technotrend DVB-S2 Pilot set to:{0}", channel.Pilot);
          Log.Log.WriteFile("Technotrend DVB-S2 RollOff set to:{0}", channel.Rolloff);
          Log.Log.WriteFile("Technotrend DVB-S2 fec set to:{0}", channel.InnerFecRate);
          return channel;
        }
        if (_knc != null)
        {
          //Set KNC modulation tuning settings
          if (channel.ModulationType == ModulationType.ModQpsk)
          {
            channel.ModulationType = ModulationType.Mod8Vsb;
          }
          if (channel.ModulationType == ModulationType.Mod8Psk)
          {
            channel.ModulationType = ModulationType.Mod8Vsb;
          }
          if (channel.ModulationType == ModulationType.Mod16Apsk)
          {
            channel.ModulationType = ModulationType.Mod16Vsb;
          }
          if (channel.ModulationType == ModulationType.Mod32Apsk)
          {
            channel.ModulationType = ModulationType.ModOqpsk;
          }
          Log.Log.WriteFile("KNC DVB-S2 modulation set to:{0}", channel.ModulationType);
          Log.Log.WriteFile("KNC DVB-S2 Pilot set to:{0}", channel.Pilot);
          Log.Log.WriteFile("KNC DVB-S2 RollOff set to:{0}", channel.Rolloff);
          Log.Log.WriteFile("KNC DVB-S2 fec set to:{0}", channel.InnerFecRate);
          return channel;
        }
        if (_digitalEveryWhere != null)
        {
          if (channel.ModulationType == ModulationType.ModQpsk)
          {
            channel.ModulationType = ModulationType.ModNbcQpsk;
          }
          if (channel.ModulationType == ModulationType.Mod8Psk)
          {
            channel.ModulationType = ModulationType.ModNbc8Psk;
          }
          //Check if DVB-S channel if not turn off Pilot & Roll-off regardless
          if (channel.ModulationType == ModulationType.ModNotSet)
          {
            channel.Pilot = Pilot.NotSet;
            channel.Rolloff = RollOff.NotSet;
            //Log.Log.WriteFile("DigitalEverywhere: we're tuning DVB-S, pilot & roll-off are now not set");
          }

          if (channel.InnerFecRate != BinaryConvolutionCodeRate.RateNotSet)
          {
            //Set the DigitalEverywhere binary values for Pilot & Roll-off
            int _pilot = 0;
            int _rollOff = 0;
            if (channel.Pilot == Pilot.On)
              _pilot = 128;
            if (channel.Pilot == Pilot.Off)
              _pilot = 64;
            if (channel.Rolloff == RollOff.Twenty)
              _rollOff = 16;
            if (channel.Rolloff == RollOff.TwentyFive)
              _rollOff = 32;
            if (channel.Rolloff == RollOff.ThirtyFive)
              _rollOff = 48;
            //The binary values get added to the current InnerFECRate - done!
            BinaryConvolutionCodeRate r = channel.InnerFecRate + _pilot + _rollOff;
            channel.InnerFecRate = r;
          }
          Log.Log.WriteFile("DigitalEverywhere DVB-S2 modulation set to:{0}", channel.ModulationType);
          Log.Log.WriteFile("DigitalEverywhere Pilot set to:{0}", channel.Pilot);
          Log.Log.WriteFile("DigitalEverywhere RollOff set to:{0}", channel.Rolloff);
          Log.Log.WriteFile("DigitalEverywhere fec set to:{0}", (int)channel.InnerFecRate);
          return channel;
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return channel;
    }

    /// <summary>
    /// check if the card are ATSC QAM capable cards.
    /// If so sets the QAM modulation for those specific ATSC cards.
    /// </summary>
    public ATSCChannel CheckATSCQAM(ATSCChannel channel)
    {
      try
      {
        if (channel.ModulationType == ModulationType.Mod64Qam || channel.ModulationType == ModulationType.Mod256Qam)
        {
          if (_isgenericatsc != null)
          {
            Log.Log.Info("Setting Generic ATSC modulation to {0}", channel.ModulationType);
            _isgenericatsc.SetXPATSCQam(channel);
          }
          if (_isvixsatsc != null)
          {
            Log.Log.Info("Setting ViXS ATSC BDA modulation to {0}", channel.ModulationType);
            _isvixsatsc.SetViXSQam(channel);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return channel;
    }

    /// <summary>
    /// gets the QAM modulation for ViXS ATSC cards under XP
    /// </summary>
    public ATSCChannel CheckVIXSQAM(ATSCChannel channel)
    {
      try
      {
        if (_isvixsatsc != null)
        {
          _isvixsatsc.GetViXSQam(channel);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return channel;
    }

    /// <summary>
    /// Property to set CI Menu Handler 
    /// </summary>
    public ICiMenuCallbacks CiMenuHandler
    {
      set
      {
        if (_ciMenu != null)
        {
          _ciMenu.SetCiMenuHandler(value);
        }
      }
    }

    #region IDisposable Member

    /// <summary>
    /// Disposing CI and API resources
    /// </summary>
    public void Dispose()
    {
      Release.Dispose(_knc);
      Release.Dispose(_technoTrend);
      Release.Dispose(_digitalEveryWhere);
      Release.Dispose(_hauppauge);
      Release.Dispose(_conexant);
      Release.Dispose(_genericbdas);
      Release.Dispose(_isgenericatsc);
      Release.Dispose(_isvixsatsc);
      Release.Dispose(_genpix);
      Release.Dispose(_winTvCiModule);
      Release.Dispose(_twinhan);
      Release.Dispose(_profred);
      Release.Dispose(_TeVii);
    }

    #endregion
  }
}