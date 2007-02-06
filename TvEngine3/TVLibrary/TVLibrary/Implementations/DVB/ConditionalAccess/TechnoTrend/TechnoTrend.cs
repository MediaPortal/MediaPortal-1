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
using System.Runtime.InteropServices;

using DirectShowLib;
using DirectShowLib.BDA;
using System.Windows.Forms;
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  public class TechnoTrend : IDisposable
  {
    ITechnoTrend _technoTrendInterface = null;
    IntPtr ptrPmt;
    /// <summary>
    /// Initializes a new instance of the <see cref="TechnoTrend"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="captureFilter">The capture filter.</param>
    public TechnoTrend(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {
      _technoTrendInterface = analyzerFilter as ITechnoTrend;
      _technoTrendInterface.SetTunerFilter(tunerFilter);
      ptrPmt = Marshal.AllocCoTaskMem(1024);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      _technoTrendInterface = null;
      Marshal.FreeCoTaskMem(ptrPmt);
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
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public void SendDiseqCommand(DVBSChannel channel)
    {
      if (_technoTrendInterface == null) return;
      short isHiBand = 0;

      switch (channel.BandType)
      {
        case BandType.Universal:
          if (channel.Frequency >= 11700000)
          {
            isHiBand = 1;
          }
          else
          {
            isHiBand = 0;
          }
          break;
      }
      short isVertical = 0;
      if (channel.Polarisation == Polarisation.LinearV) isVertical = 1;
      if (channel.Polarisation == Polarisation.CircularR) isVertical = 1;
      _technoTrendInterface.SetDisEqc((short)channel.DisEqc, isHiBand, isVertical);
    }

    /// <summary>
    /// Determines whether [is cam present].
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam present]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      return true;
    }
  }
}
