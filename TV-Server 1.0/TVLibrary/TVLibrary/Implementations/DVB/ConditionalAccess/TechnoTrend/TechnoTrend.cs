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
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using System.Windows.Forms;
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  public class TechnoTrend : IDisposable, IDiSEqCController
  {
    ITechnoTrend _technoTrendInterface = null;
    IntPtr ptrPmt;
    IntPtr _ptrDataInstance;
    DVBSChannel _previousChannel = null;
    /// <summary>
    /// Initializes a new instance of the <see cref="TechnoTrend"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The analyzer filter.</param>
    public TechnoTrend(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {
      _technoTrendInterface = analyzerFilter as ITechnoTrend;
      _technoTrendInterface.SetTunerFilter(tunerFilter);
      ptrPmt = Marshal.AllocCoTaskMem(1024);
      _ptrDataInstance = Marshal.AllocCoTaskMem(1024);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      _technoTrendInterface = null;
      Marshal.FreeCoTaskMem(ptrPmt);
      Marshal.FreeCoTaskMem(_ptrDataInstance);
    }

    /// <summary>
    /// Determines whether cam is ready.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam ready]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamReady()
    {
      if (_technoTrendInterface == null) return false;
      bool yesNo = false;
      _technoTrendInterface.IsCamReady(ref yesNo);
      return yesNo;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is techno trend.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is techno trend; otherwise, <c>false</c>.
    /// </value>
    public bool IsTechnoTrend
    {
      get
      {
        if (_technoTrendInterface == null) return false;
        bool yesNo = false;
        _technoTrendInterface.IsTechnoTrend(ref yesNo);
        return yesNo;
      }
    }

    /// <summary>
    /// Sends the PMT.
    /// </summary>
    /// <param name="pmt">The PMT.</param>
    /// <param name="PMTlength">The PM tlength.</param>
    /// <returns></returns>
    public bool SendPMT(byte[] pmt, int PMTlength)
    {
      if (_technoTrendInterface == null) return true;
      bool succeeded = false;
      for (int i = 0; i < PMTlength; ++i)
      {
        Marshal.WriteByte(ptrPmt, i, pmt[i]);
      }
      _technoTrendInterface.DescrambleService(ptrPmt, (short)PMTlength, ref succeeded);
      return succeeded;
    }

    /// <summary>
    /// Instructs the technotrend card to descramble all programs mentioned in subChannels.
    /// </summary>
    /// <param name="subChannels">The sub channels.</param>
    /// <returns></returns>
    public bool DescrambleMultiple(Dictionary<int, ConditionalAccessContext> subChannels)
    {
      if (_technoTrendInterface == null) return true;
      List<ConditionalAccessContext> filteredChannels = new List<ConditionalAccessContext>();
      bool succeeded = true;
      Dictionary<int, ConditionalAccessContext>.Enumerator en = subChannels.GetEnumerator();
      while (en.MoveNext())
      {
        bool exists = false;
        ConditionalAccessContext context = en.Current.Value;
        foreach (ConditionalAccessContext c in filteredChannels)
        {
          if (c.Channel.Equals(context.Channel)) exists = true;
        }
        if (!exists)
        {
          filteredChannels.Add(context);
        }
      }

      for (int i = 0; i < filteredChannels.Count; ++i)
      {
        ConditionalAccessContext context = filteredChannels[i];
        Marshal.WriteInt16(ptrPmt, 2 * i, (short)context.ServiceId);
      }
      _technoTrendInterface.DescrambleMultiple(ptrPmt, (short)filteredChannels.Count, ref succeeded);
      return succeeded;
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The scanparameters.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (_technoTrendInterface == null) return;
      if (_previousChannel != null)
      {
        if (_previousChannel.Frequency == channel.Frequency &&
            _previousChannel.DisEqc == channel.DisEqc &&
            _previousChannel.Polarisation == channel.Polarisation)
        {
          Log.Log.WriteFile("Technotrend: already tuned to diseqc:{0}, frequency:{1}, polarisation:{2}",channel.DisEqc, channel.Frequency, channel.Polarisation);
          return;
        }
      }
      _previousChannel = channel;
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);
      //"01,02,03,04,05,06,07,08,09,0a,0b,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,"	
      Marshal.WriteByte(_ptrDataInstance, 0, 0xE0);//diseqc command 1. uFraming=0xe0
      Marshal.WriteByte(_ptrDataInstance, 1, 0x10);//diseqc command 1. uAddress=0x10
      Marshal.WriteByte(_ptrDataInstance, 2, 0x38);//diseqc command 1. uCommand=0x38
      //bit 0	(1)	: 0=low band, 1 = hi band
      //bit 1 (2) : 0=vertical, 1 = horizontal
      //bit 3 (4) : 0=satellite position A, 1=satellite position B
      //bit 4 (8) : 0=switch option A, 1=switch option  B
      // LNB    option  position
      // 1        A         A
      // 2        A         B
      // 3        B         A
      // 4        B         B
      bool hiBand = BandTypeConverter.IsHiBand(channel,parameters);
      Log.Log.WriteFile("TechnoTrend SendDiseqcCommand() diseqc:{0}, antenna:{1} frequency:{2}, polarisation:{3} hiband:{4}", channel.DisEqc, antennaNr, channel.Frequency, channel.Polarisation, hiBand);
      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) || (channel.Polarisation == Polarisation.CircularL));
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);
      Marshal.WriteByte(_ptrDataInstance, 3, cmd);
      _technoTrendInterface.SetDisEqc(_ptrDataInstance, 4, 1, 0, (short)channel.Polarisation);
    }

    /// <summary>
    /// Determines whether [is cam present].
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam present]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      if (_technoTrendInterface == null) return false;
      bool yesNo = false;
      _technoTrendInterface.IsCamPresent(ref yesNo);
      return yesNo;
    }

    #region IDiSEqCController Members

    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="diSEqC">The DiSEqC command.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      if (_technoTrendInterface == null) return false;
      for (int i = 0; i < diSEqC.Length; ++i)
        Marshal.WriteByte(_ptrDataInstance, i, diSEqC[i]);
      Polarisation pol = Polarisation.LinearV;
      if (_previousChannel != null)
        pol = _previousChannel.Polarisation;
      _technoTrendInterface.SetDisEqc(_ptrDataInstance, (byte)diSEqC.Length, 1, 0, (short)pol);
      return true;
    }

    /// <summary>
    /// gets the diseqc reply
    /// </summary>
    /// <param name="reply">The reply.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      reply = null;
      return false;
    }

    #endregion
  }
}
