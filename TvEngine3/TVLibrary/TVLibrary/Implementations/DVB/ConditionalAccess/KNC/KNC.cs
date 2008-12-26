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
    /// <summary>
    /// Initializes a new instance of the <see cref="KNC"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The analyzer filter.</param>
    public KNC(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {
      _KNCInterface = analyzerFilter as IKNC;
      if (_KNCInterface != null)
        _KNCInterface.SetTunerFilter(tunerFilter);
      ptrPmt = Marshal.AllocCoTaskMem(1024);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      _KNCInterface = null;
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
      if (_KNCInterface == null)
        return false;
      bool yesNo = false;
      _KNCInterface.IsCamReady(ref yesNo);
      Log.Log.Info("KNC: IsCAMReady {0}", yesNo);
      return yesNo;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is techno trend.
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
      Log.Log.Info("KNC: SendPMT {0}", succeeded);
      return succeeded;
    }


    /// <summary>
    /// Instructs the KNC card to descramble all programs mentioned in subChannels.
    /// </summary>
    /// <param name="subChannels">The sub channels.</param>
    /// <returns></returns>
    public bool DescrambleMultiple(Dictionary<int, ConditionalAccessContext> subChannels)
    {
      if (_KNCInterface == null)
        return true;
      List<ConditionalAccessContext> filteredChannels = new List<ConditionalAccessContext>();
      bool succeeded = true;
      Dictionary<int, ConditionalAccessContext>.Enumerator en = subChannels.GetEnumerator();
      while (en.MoveNext())
      {
        bool exists = false;
        ConditionalAccessContext context = en.Current.Value;
        foreach (ConditionalAccessContext c in filteredChannels)
        {
          if (c.Channel.Equals(context.Channel))
            exists = true;
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
      _KNCInterface.DescrambleMultiple(ptrPmt, (short)filteredChannels.Count, ref succeeded);
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

      short isHiBand = (short)(BandTypeConverter.IsHiBand(channel, parameters) ? 1 : 0);

      short isVertical = 0;
      if (channel.Polarisation == Polarisation.LinearV)
        isVertical = 1;
      if (channel.Polarisation == Polarisation.CircularR)
        isVertical = 1;
      _KNCInterface.SetDisEqc((short)channel.DisEqc, isHiBand, isVertical);
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
