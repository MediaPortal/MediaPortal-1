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
namespace TvLibrary.Implementations.DVB
{
  public class ConditionalAccess
  {
    #region variables
    DigitalEverywhere _digitalEveryWhere;
    TechnoTrend _technoTrend;
    Twinhan _twinhan;
    #endregion

    //ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="T:ConditionalAccess"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="captureFilter">The capture filter.</param>
    public ConditionalAccess(IBaseFilter tunerFilter, IBaseFilter captureFilter)
    {
      Log.Log.WriteFile("Check for Digital Everywhere");
      _digitalEveryWhere = new DigitalEverywhere(tunerFilter, captureFilter);
      if (_digitalEveryWhere.IsDigitalEverywhere)
      {
        Log.Log.WriteFile("Digital Everywhere card detected");
        _digitalEveryWhere.ResetCAM();
        return;
      }
      _digitalEveryWhere = null;
      
      Log.Log.WriteFile("Check for Twinhan");
      _twinhan = new Twinhan(tunerFilter, captureFilter);
      if (_twinhan.IsTwinhan)
      {
        Log.Log.WriteFile("Twinhan card detected");
        return;
      }
      _twinhan = null;

      Log.Log.WriteFile("Check for TechnoTrend");
      _technoTrend = new TechnoTrend(tunerFilter, captureFilter);
      if (_technoTrend.IsTechnoTrend)
      {
        Log.Log.WriteFile("TechnoTrend card detected");
        return;
      }
      _technoTrend = null;
    }

    /// <summary>
    /// returns if cam is ready or not
    /// </summary>
    public bool IsCamReady()
    {
      if (_digitalEveryWhere != null)
      {
        _digitalEveryWhere.IsCamReady();
      }
      return true;
    }
    
    /// <summary>
    /// resets the CAM
    /// </summary>
    public void ResetCAM()
    {
      if (_digitalEveryWhere!=null)
      {
        _digitalEveryWhere.ResetCAM();
      }
    }

    /// <summary>
    /// Sends the PMT to the CI module
    /// </summary>
    /// <param name="channel">channel on which we are tuned</param>
    /// <param name="PMT">byte array containing the PMT</param>
    /// <param name="pmtLength">length of the pmt array</param>
    /// <param name="audioPid">pid of the current audio stream</param>
    /// <returns></returns>
    public bool SendPMT(DVBBaseChannel channel, byte[] PMT, int pmtLength, int audioPid)
    {
      if (_digitalEveryWhere != null)
      {
        return _digitalEveryWhere.SendPMTToFireDTV(PMT, pmtLength);
      }
      if (_twinhan != null)
      {

        ChannelInfo info = new ChannelInfo();
        info.DecodePmt(PMT); 
        int videoPid = -1;
        foreach (PidInfo pmtData in info.pids)
        {
          if (pmtData.isVideo && videoPid<0) videoPid = pmtData.pid;
          if (pmtData.isAudio && audioPid<0) audioPid = pmtData.pid;
          if (videoPid >= 0 && audioPid >= 0) break;
        }
        _twinhan.SendPMT("default", (uint)videoPid, (uint)audioPid, PMT, pmtLength);
        return true;
      }
      if (_technoTrend != null)
      {
        return _technoTrend.SendPMT(channel.ServiceId);
      }
      return true;
    }

    /// <summary>
    /// sends the diseqc command to the card
    /// </summary>
    /// <param name="channel">The current tv/radio channel</param>
    public void SendDiseqcCommand(DVBSChannel channel)
    {
      if (_digitalEveryWhere != null)
      {
        _digitalEveryWhere.SendDiseqcCommand(channel);
      }
    }
    /// <summary>
    /// Instructs the cam/ci module to use hardware filter and only send the pids listed in pids to the pc
    /// </summary>
    /// <param name="channel">The current tv/radio channel.</param>
    /// <param name="pids">The pids.</param>
    /// <remarks>when the pids array is empty, pid filtering is disabled and all pids are received</remarks>
    public void SendPids(DVBBaseChannel channel,ArrayList pids)
    {
      if (_digitalEveryWhere != null)
      {
        bool isDvbc, isDvbt, isDvbs, isAtsc;
        isDvbc = ((channel as DVBCChannel) != null);
        isDvbt = ((channel as DVBTChannel) != null);
        isDvbs = ((channel as DVBSChannel) != null);
        isAtsc = ((channel as ATSCChannel) != null);
        _digitalEveryWhere.SetHardwarePidFiltering(isDvbc,isDvbt,isDvbs,isAtsc,pids);
      }
    }
  }
}
