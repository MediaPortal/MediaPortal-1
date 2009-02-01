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
    protected IBDA_Topology _TunerDevice;
    DVBSChannel _previousChannel;
    /// <summary>
    /// Initializes a new instance of the <see cref="KNC"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The analyzer filter.</param>
    /// <param name="DeviceIndex">The KNC1 card hardware index (0 based)</param>
    public KNC(IBaseFilter tunerFilter, IBaseFilter analyzerFilter, int DeviceIndex)
    {
      _TunerDevice = (IBDA_Topology)tunerFilter;
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
      switch (channel.DisEqc)
      {
        case DisEqcType.Level1AA:
          Log.Log.Info("KNC:  Level1AA - SendDiSEqCCommand(0x00)");
          SendDiSEqCCommand(0x00);
          break;
        case DisEqcType.Level1AB:
          Log.Log.Info("KNC:  Level1AB - SendDiSEqCCommand(0x01)");
          SendDiSEqCCommand(0x01);
          break;
        case DisEqcType.Level1BA:
          Log.Log.Info("KNC:  Level1BA - SendDiSEqCCommand(0x0100)");
          SendDiSEqCCommand(0x0100);
          break;
        case DisEqcType.Level1BB:
          Log.Log.Info("KNC:  Level1BB - SendDiSEqCCommand(0x0101)");
          SendDiSEqCCommand(0x0101);
          break;
        case DisEqcType.SimpleA:
          Log.Log.Info("KNC:  SimpleA - SendDiSEqCCommand(0x00)");
          SendDiSEqCCommand(0x00);
          break;
        case DisEqcType.SimpleB:
          Log.Log.Info("KNC:  SimpleB - SendDiSEqCCommand(0x01)");
          SendDiSEqCCommand(0x01);
          break;
        default:
          return;
      }
    }

    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="ulRange">The DisEqCPort</param>
    /// <returns>true if succeeded, otherwise false</returns>
    protected bool SendDiSEqCCommand(ulong ulRange)
    {
      Log.Log.Info("KNC:  SendDiSEqC Command {0}", ulRange);
      // get ControlNode of tuner control node
      object ControlNode;
      int hr = _TunerDevice.GetControlNode(0, 1, 0, out ControlNode);
      if (hr == 0)
      // retrieve the BDA_DeviceControl interface 
      {
        IBDA_DeviceControl DecviceControl = (IBDA_DeviceControl)_TunerDevice;
        if (DecviceControl != null)
        {
          if (ControlNode != null)
          {
            IBDA_FrequencyFilter FrequencyFilter = ControlNode as IBDA_FrequencyFilter;
            hr = DecviceControl.StartChanges();
            if (hr == 0)
            {
              if (FrequencyFilter != null)
              {
                hr = FrequencyFilter.put_Range(ulRange);
                Log.Log.Info("KNC:  put_Range:{0} success:{1}", ulRange, hr);
                if (hr == 0)
                {
                  // did it accept the changes? 
                  hr = DecviceControl.CheckChanges();
                  if (hr == 0)
                  {
                    hr = DecviceControl.CommitChanges();
                    if (hr == 0)
                    {
                      Log.Log.Info("KNC:  CommitChanges() Succeeded");
                      return true;
                    }
                    // reset configuration
                    Log.Log.Info("KNC:  CommitChanges() Failed!");
                    DecviceControl.StartChanges();
                    DecviceControl.CommitChanges();
                    return false;
                  }
                  Log.Log.Info("KNC:  CheckChanges() Failed!");
                  return false;
                }
                Log.Log.Info("KNC:  put_Range Failed!");
                return false;
              }
            }
          }
        }
      }
      Log.Log.Info("KNC:  GetControlNode Failed!");
      return false;
    } //end SendDiSEqCCommand

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
