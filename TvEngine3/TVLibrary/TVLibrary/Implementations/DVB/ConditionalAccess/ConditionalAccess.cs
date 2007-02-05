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
using DirectShowLib;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Interfaces;


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
    Hauppauge _hauppauge = null;
    DiSEqCMotor _diSEqCMotor = null;
    Dictionary<int, ConditionalAccessContext> _mapSubChannels;
    #endregion

    //ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="T:ConditionalAccess"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The capture filter.</param>
    public ConditionalAccess(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {
      try
      {
        _mapSubChannels = new Dictionary<int, ConditionalAccessContext>();
        if (tunerFilter == null && analyzerFilter == null) return;
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

        _hauppauge = new Hauppauge(tunerFilter, analyzerFilter);
        if (_hauppauge.IsHauppauge)
        {
          Log.Log.WriteFile("Hauppauge card detected");
          _diSEqCMotor = new DiSEqCMotor(_hauppauge);
          return;
        }
        _hauppauge = null;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }

    public void AddSubChannel(int id)
    {
      if (!_mapSubChannels.ContainsKey(id))
      {
        _mapSubChannels[id] = new ConditionalAccessContext();
      }
    }
    public void FreeSubChannel(int id)
    {
      if (_mapSubChannels.ContainsKey(id))
      {
        _mapSubChannels.Remove(id);
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
        AddSubChannel(subChannel);
        ConditionalAccessContext context = _mapSubChannels[subChannel];
        context.CamType = camType;
        context.Channel = channel;
        context.PMT = PMT;
        context.PMTLength = pmtLength;
        context.AudioPid = audioPid;

        if (_digitalEveryWhere != null)
        {
          return _digitalEveryWhere.SendPMTToFireDTV(_mapSubChannels);
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
        if (_technoTrend != null)
        {
          return _technoTrend.SendPMT(channel.ServiceId);
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
    /// <param name="channel">The current tv/radio channel</param>
    public void SendDiseqcCommand(DVBSChannel channel)
    {
      try
      {
        if (_digitalEveryWhere != null)
        {
          _digitalEveryWhere.SendDiseqcCommand(channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_technoTrend != null)
        {
          _technoTrend.SendDiseqCommand(channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_twinhan != null)
        {
          _twinhan.SendDiseqCommand(channel);
          System.Threading.Thread.Sleep(100);
        }
        if (_hauppauge != null)
        {
          _hauppauge.SendDiseqCommand(channel);
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
  }
}
