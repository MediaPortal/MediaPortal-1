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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DirectShowLib;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Interfaces;
using DirectShowLib.BDA;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which handles the conditional access modules for a tv card
  /// (CI and CAM)
  /// </summary>
  public class ConditionalAccess
  {
    #region variables
    DigitalEverywhere _digitalEveryWhere = null;
    TechnoTrend _technoTrend = null;
    Twinhan _twinhan = null;
    KNC _knc = null;
    Hauppauge _hauppauge = null;
    DiSEqCMotor _diSEqCMotor = null;
    Dictionary<int, ConditionalAccessContext> _mapSubChannels;
    GenericBDAS _genericbdas = null;
    WinTvCiModule _winTvCiModule = null;
    GenericATSC _isgenericatsc = null;
    OnAirATSC _isonairatsc = null;
    ViXSATSC _isvixsatsc = null;
    #endregion

    //ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="T:ConditionalAccess"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The capture filter.</param>
    /// <param name="winTvUsbCiFilter">The WinTV CI filter.</param>
    public ConditionalAccess(IBaseFilter tunerFilter, IBaseFilter analyzerFilter, IBaseFilter winTvUsbCiFilter)
    {
      try
      {
        _mapSubChannels = new Dictionary<int, ConditionalAccessContext>();
        if (tunerFilter == null && analyzerFilter == null) return;
        Log.Log.WriteFile("Check for KNC");
        _knc = new KNC(tunerFilter, analyzerFilter);
        if (_knc.IsKNC)
        {
          Log.Log.WriteFile("KNC card detected");
          return;
        }
        _knc = null;

        Log.Log.WriteFile("Check for Digital Everywhere");
        _digitalEveryWhere = new DigitalEverywhere(tunerFilter, analyzerFilter);
        if (_digitalEveryWhere.IsDigitalEverywhere)
        {
          Log.Log.WriteFile("Digital Everywhere card detected");
          _diSEqCMotor = new DiSEqCMotor(_digitalEveryWhere);
          //_digitalEveryWhere.ResetCAM();
          return;
        }
        _digitalEveryWhere = null;

        Log.Log.WriteFile("Check for Twinhan");
        _twinhan = new Twinhan(tunerFilter, analyzerFilter);
        if (_twinhan.IsTwinhan)
        {
          Log.Log.WriteFile("Twinhan card detected");
          _diSEqCMotor = new DiSEqCMotor(_twinhan);
          return;
        }
        _twinhan = null;

        Log.Log.WriteFile("Check for TechnoTrend");
        _technoTrend = new TechnoTrend(tunerFilter, analyzerFilter);
        if (_technoTrend.IsTechnoTrend)
        {
          Log.Log.WriteFile("TechnoTrend card detected");
          return;
        }
        _technoTrend = null;

        Log.Log.WriteFile("Check for Hauppauge");
        _hauppauge = new Hauppauge(tunerFilter, analyzerFilter);
        if (_hauppauge.IsHauppauge)
        {
          Log.Log.WriteFile("Hauppauge card detected");
          _diSEqCMotor = new DiSEqCMotor(_hauppauge);
          return;
        }
        _hauppauge = null;

        //check for ATSC
        Log.Log.WriteFile("Check for ViXS ATSC QAM card");
        _isvixsatsc = new ViXSATSC(tunerFilter, analyzerFilter);
        if (_isvixsatsc.IsViXSATSC)
        {
          Log.Log.WriteFile("ViXS ATSC QAM card detected");
          return;
        }
        _isvixsatsc = null;

        Log.Log.WriteFile("Check for OnAir ATSC QAM card");
        _isonairatsc = new OnAirATSC(tunerFilter, analyzerFilter);
        if (_isonairatsc.IsOnAirATSC)
        {
          Log.Log.WriteFile("OnAir ATSC QAM card detected");
          return;
        }
        _isonairatsc = null;

        Log.Log.WriteFile("Check for Generic ATSC QAM card");
        _isgenericatsc = new GenericATSC(tunerFilter, analyzerFilter);
        if (_isgenericatsc.IsGenericATSC)
        {
          Log.Log.WriteFile("Generic ATSC QAM card detected");
          //return;
        }
        //_isgenericatsc = null;

        Log.Log.WriteFile("Check for WinTV CI");
        if (winTvUsbCiFilter != null)
        {
          Log.Log.WriteFile("WinTV CI detected");
          _winTvCiModule = new WinTvCiModule(winTvUsbCiFilter);
          //return;
        }
        //_winTvCiModule = null;

        Log.Log.WriteFile("Check for Generic DVB-S card");
        _genericbdas = new GenericBDAS(tunerFilter, analyzerFilter);
        if (_genericbdas.IsGenericBDAS)
        {
          Log.Log.WriteFile("Generic BDA card detected");
          return;
        }
        _genericbdas = null;
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
    public void AddSubChannel(int id)
    {
      if (!_mapSubChannels.ContainsKey(id))
      {
        _mapSubChannels[id] = new ConditionalAccessContext();
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
        _mapSubChannels.Remove(id);
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
        if (_technoTrend != null)
        {
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
      get
      {
        return _diSEqCMotor;
      }
    }

    /// <summary>
    /// returns if cam is ready or not
    /// </summary>
    public bool IsCamReady()
    {
      try
      {
        if (_knc != null)
        {
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
          Log.Log.WriteFile("TechnoTrend IsCamReady(): IsCamPresent:{0}, IsCamReady:{1}", _technoTrend.IsCamPresent(), _technoTrend.IsCamReady());
          return _technoTrend.IsCamReady();
        }
        if (_winTvCiModule != null)
        {
          //How do we check this???
          int hr = _winTvCiModule.CAMReady();
          if (hr != 0)
            return false;
          Log.Log.Info("WinTVCI:  CAM is ready");
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
        if (_digitalEveryWhere != null)
        {
          _digitalEveryWhere.ResetCAM();
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }

    public bool OnRunGraph(int servicedId)
    {
      return true;
    }

    public void OnStopGraph()
    {
      if (_digitalEveryWhere != null)
      {
        _digitalEveryWhere.OnStopGraph();
      }
      if (_winTvCiModule != null)
      {
        Log.Log.Info("dvb:  Stopping WinTVCI module");
        _winTvCiModule.Shutdown();
      }
    }
    /// <summary>
    /// Gets the number of channels the card is currently decrypting.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {
        if (_mapSubChannels == null) return 0;
        if (_mapSubChannels.Count == 0) return 0;
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
                if (c.Channel.Equals(context.Channel)) exists = true;
              }
            }
            if (!exists)
            {
              filteredChannels.Add(context);
            }
          }
        }
        return filteredChannels.Count;
      }
    }

    /// <summary>
    /// Sends the PMT to the CI module
    /// </summary>
    /// <param name="subChannel">The sub channel.</param>
    /// <param name="camType">type of cam in use</param>
    /// <param name="channel">channel on which we are tuned</param>
    /// <param name="PMT">byte array containing the PMT</param>
    /// <param name="pmtLength">length of the pmt array</param>
    /// <param name="audioPid">pid of the current audio stream</param>
    /// <returns></returns>
    public bool SendPMT(int subChannel, CamType camType, DVBBaseChannel channel, byte[] PMT, int pmtLength, int audioPid)
    {
      try
      {
        if (channel.FreeToAir) return true;//no need to descramble this one...

        AddSubChannel(subChannel);
        ConditionalAccessContext context = _mapSubChannels[subChannel];
        context.CamType = camType;
        context.Channel = channel;
        context.PMT = PMT;
        context.PMTLength = pmtLength;
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
          return _knc.SendPMT(PMT, pmtLength);
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
          int videoPid = -1;
          foreach (PidInfo pmtData in info.pids)
          {
            if (pmtData.isVideo && videoPid < 0) videoPid = pmtData.pid;
            if (pmtData.isAudio && audioPid < 0) audioPid = pmtData.pid;
            if (videoPid >= 0 && audioPid >= 0) break;
          }
          int caPmtLen;
          byte[] caPmt = info.caPMT.CaPmtStruct(out caPmtLen);
          _twinhan.SendPMT(camType, (uint)videoPid, (uint)audioPid, caPmt, caPmtLen);
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
    /// sends the diseqc command to the card
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="channel">The current tv/radio channel</param>
    public void SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
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
          _hauppauge.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_genericbdas != null)
        {
          _genericbdas.SendDiseqCommand(parameters, channel);
          System.Threading.Thread.Sleep(100);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }
    /// <summary>
    /// Instructs the cam/ci module to use hardware filter and only send the pids listed in pids to the pc
    /// </summary>
    /// <param name="channel">The current tv/radio channel.</param>
    /// <param name="pids">The pids.</param>
    /// <remarks>when the pids array is empty, pid filtering is disabled and all pids are received</remarks>
    public void SendPids(DVBBaseChannel channel, ArrayList pids)
    {
      try
      {
        if (_digitalEveryWhere != null)
        {
          bool isDvbc, isDvbt, isDvbs, isAtsc;
          isDvbc = ((channel as DVBCChannel) != null);
          isDvbt = ((channel as DVBTChannel) != null);
          isDvbs = ((channel as DVBSChannel) != null);
          isAtsc = ((channel as ATSCChannel) != null);
          _digitalEveryWhere.SetHardwarePidFiltering(isDvbc, isDvbt, isDvbs, isAtsc, pids);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }
    /// <summary>
    /// Sets the DVB s2 modulation.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="channel">The channel.</param>
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
            //It seems DVB-S2 QPSK is STILL not working!
            //Test for i-loop
            if (channel.ModulationType == ModulationType.ModQpsk)
            {
              if (channel.InnerFecRate == BinaryConvolutionCodeRate.Rate9_10)
              {
                channel.ModulationType = ModulationType.Mod32Qam;
              }
              if (channel.InnerFecRate == BinaryConvolutionCodeRate.Rate8_9)
              {
                channel.ModulationType = ModulationType.Mod16Qam;
              }
              else
                channel.ModulationType = ModulationType.ModBpsk;
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
            Log.Log.WriteFile("Hauppauge DVB-S2 modulation set to:{0}", channel.ModulationType);
            Log.Log.WriteFile("Hauppauge DVB-S2 Pilot set to:{0}", channel.Pilot);
            Log.Log.WriteFile("Hauppauge DVB-S2 RollOff set to:{0}", channel.Rolloff);
            Log.Log.WriteFile("Hauppauge DVB-S2 fec set to:{0}", channel.InnerFecRate);
            _hauppauge.SetDVBS2PilotRolloff(channel);
          }
          return channel;

          //Work-around until new driver released...
          /*if (channel.ModulationType != ModulationType.ModNotSet)
          {
            //Set the alternative Hauppauge Modulation type
            if (channel.ModulationType == ModulationType.ModQpsk)
            {
              if (channel.InnerFecRate == BinaryConvolutionCodeRate.Rate9_10)
              {
                channel.ModulationType = ModulationType.Mod32Qam;
              }
              if (channel.InnerFecRate == BinaryConvolutionCodeRate.Rate8_9)
              {
                channel.ModulationType = ModulationType.Mod16Qam;
              }
              else
                channel.ModulationType = ModulationType.ModBpsk;
            }
            if (channel.ModulationType == ModulationType.Mod8psk)
            {
              if (channel.InnerFecRate == BinaryConvolutionCodeRate.Rate9_10)
              {
                channel.ModulationType = ModulationType.Mod80Qam;
              }
              if (channel.InnerFecRate == BinaryConvolutionCodeRate.Rate8_9)
              {
                channel.ModulationType = ModulationType.Mod64Qam;
              }
            }
          }
          return channel;*/
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

          DVBSChannel tuneChannel = new DVBSChannel(channel);
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
            tuneChannel.InnerFecRate = channel.InnerFecRate + _pilot + _rollOff;
          }
          Log.Log.WriteFile("DigitalEverywhere DVB-S2 modulation set to:{0}", channel.ModulationType);
          Log.Log.WriteFile("DigitalEverywhere Pilot set to:{0}", channel.Pilot);
          Log.Log.WriteFile("DigitalEverywhere RollOff set to:{0}", channel.Rolloff);
          Log.Log.WriteFile("DigitalEverywhere fec set to:{0}", (int)tuneChannel.InnerFecRate);
          return tuneChannel;
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return channel;
    }

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
          if (_isonairatsc != null)
          {
            Log.Log.Info("Setting OnAir ATSC modulation to {0}", channel.ModulationType);
            _isonairatsc.SetOnAirQam(channel);
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
  }
}
