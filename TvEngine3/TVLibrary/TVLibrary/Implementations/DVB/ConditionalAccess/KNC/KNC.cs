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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// KNC CI control class
  /// </summary>
  public class KNC : IDisposable
  {
    IKNC _KNCInterface;
    readonly IntPtr ptrPmt;
    readonly IntPtr _ptrDataInstance;
    DVBSChannel _previousChannel;
    /// <summary>
    /// Initializes a new instance of the <see cref="KNC"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The analyzer filter.</param>
    /// <param name="DeviceIndex">The KNC1 card hardware index (0 based)</param>
    public KNC(IBaseFilter tunerFilter, IBaseFilter analyzerFilter, int DeviceIndex)
    {
      _KNCInterface = analyzerFilter as IKNC;
      if (_KNCInterface != null)
      {
        _KNCInterface.SetTunerFilter(tunerFilter, DeviceIndex);
      }
      ptrPmt = Marshal.AllocCoTaskMem(1024);
      _ptrDataInstance = Marshal.AllocCoTaskMem(1024);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      _KNCInterface = null;
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
      if (_KNCInterface == null)
        return false;
      bool yesNo = false;
      _KNCInterface.IsCamReady(ref yesNo);
      Log.Log.Info("KNC: IsCAMReady {0}", yesNo);
      return yesNo;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is a KNC card.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is a KNC card; otherwise, <c>false</c>.
    /// </value>
    public bool IsKNC
    {
      get
      {
        if (_KNCInterface == null)
          return false;
        bool yesNo = false;
        _KNCInterface.IsKNC(ref yesNo);
        Log.Log.Info("KNC: IsKNC {0}", yesNo);
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
      if (_KNCInterface == null)
        return true;
      bool succeeded = false;
      for (int i = 0; i < PMTlength; ++i)
      {
        Marshal.WriteByte(ptrPmt, i, pmt[i]);
      }
      _KNCInterface.DescrambleService(ptrPmt, (short)PMTlength, ref succeeded);
      Log.Log.Info("KNC: SendPMT success = {0}", succeeded);
      return succeeded;
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The scanparameters.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (_KNCInterface == null)
        return;
      if (_previousChannel != null)
      {
        if (_previousChannel.Frequency == channel.Frequency &&
            _previousChannel.DisEqc == channel.DisEqc &&
            _previousChannel.Polarisation == channel.Polarisation)
        {
          Log.Log.WriteFile("KNC: already tuned to diseqc:{0}, frequency:{1}, polarisation:{2}", channel.DisEqc, channel.Frequency, channel.Polarisation);
          return;
        }
      }
      _previousChannel = channel;
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);
      Marshal.WriteByte(_ptrDataInstance, 0, 0xE0);//diseqc command 1. uFraming=0xe0
      Marshal.WriteByte(_ptrDataInstance, 1, 0x10);//diseqc command 1. uAddress=0x10
      Marshal.WriteByte(_ptrDataInstance, 2, 0x38);//diseqc command 1. uCommand=0x38
      bool hiBand = BandTypeConverter.IsHiBand(channel, parameters);
      Log.Log.WriteFile("KNC: SendDiseqcCommand() diseqc:{0}, antenna:{1} frequency:{2}, polarisation:{3} hiband:{4}", channel.DisEqc, antennaNr, channel.Frequency, channel.Polarisation, hiBand);
      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) || (channel.Polarisation == Polarisation.CircularL));
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);
      Marshal.WriteByte(_ptrDataInstance, 3, cmd);
      _KNCInterface.SetDisEqc(_ptrDataInstance, 4, 1);
    }

    /// <summary>
    /// Determines whether [is cam present].
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam present]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      if (_KNCInterface == null)
        return false;
      bool yesNo = false;
      _KNCInterface.IsCIAvailable(ref yesNo);
      Log.Log.Info("KNC: IsCIAvailable {0}", yesNo);
      return yesNo;
    }
  }
}
