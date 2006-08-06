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
    /// <returns></returns>
    public bool SendPMT(DVBBaseChannel channel, byte[] PMT, int pmtLength)
    {
      if (_digitalEveryWhere != null)
      {
        return _digitalEveryWhere.SendPMTToFireDTV(PMT, pmtLength);
      }
      if (_twinhan != null)
      {

        ChannelInfo info = new ChannelInfo();
        info.DecodePmt(PMT);
        int audioPid = -1;
        int videoPid = -1;
        foreach (PidInfo pmtData in info.pids)
        {
          if (pmtData.isVideo) videoPid = pmtData.pid;
          if (pmtData.isAudio) audioPid = pmtData.pid;
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
    /// <param name="channel"></param>
    public void SendDiseqcCommand(DVBSChannel channel)
    {
      if (_digitalEveryWhere != null)
      {
        _digitalEveryWhere.SendDiseqcCommand(channel);
      }
    }
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
