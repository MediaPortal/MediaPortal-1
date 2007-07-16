/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

/*
 * I have only tested this with Terratec Cinergy DVB-S 1200.
 * However, it should work with other Philips SAA-7146 based cards as well.
 * Use this at your own risk!!
 * /Digi
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using System.Windows.Forms;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{

  class GenericBDAS
  {
    protected IBDA_Topology _TunerDevice;
    protected bool _isGenericBDAS = false;

    public GenericBDAS(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {

      _TunerDevice = (IBDA_Topology)tunerFilter;
      _isGenericBDAS = true;
      /*
       * It should probaly be implemented a function which handle a list of DBA devices
       * that is compatible with "put_Range" function.
       * I have only tested this with Terratec Cinergy DVB-S 1200.
       * However, it should work with other Philips SAA-7146 based cards as well.
       * Use this at your own risk!!
       * /Digi
       * 
       */


    }

    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {

      switch (channel.DisEqc)
      {
        case DisEqcType.Level1AA: SendDiSEqCCommand(0x00); break;
        case DisEqcType.Level1AB: SendDiSEqCCommand(0x01); break;
        case DisEqcType.Level1BA: SendDiSEqCCommand(0x0100); break;
        case DisEqcType.Level1BB: SendDiSEqCCommand(0x0101); break;
        case DisEqcType.SimpleA: SendDiSEqCCommand(0x00); break;
        case DisEqcType.SimpleB: SendDiSEqCCommand(0x01); break;
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
      int hr = 0;

      // get ControlNode of tuner control node
      object ControlNode = null;
      hr = _TunerDevice.GetControlNode(0, 1, 0, out ControlNode);
      if (hr == 0)
      // retrieve the BDA_DeviceControl interface 
      {
        IBDA_DeviceControl DecviceControl = (IBDA_DeviceControl)_TunerDevice;
        if (DecviceControl != null)
        {
          if (ControlNode != null)
          {
            IBDA_FrequencyFilter FrequencyFilter = (IBDA_FrequencyFilter)ControlNode;
            hr = DecviceControl.StartChanges();
            if (hr == 0)
            {
              if (FrequencyFilter != null)
              {
                hr = FrequencyFilter.put_Range(ulRange);
                if (hr == 0)
                {
                  // did it accept the changes? 
                  hr = DecviceControl.CheckChanges();
                  if (hr == 0)
                  {
                    hr = DecviceControl.CommitChanges();
                    if (hr == 0)
                      return true;
                    else
                    {
                      // reset configuration 
                      DecviceControl.StartChanges();
                      DecviceControl.CommitChanges();
                      return false;
                    }
                  }
                }
              }
            }
          }
        }
      }
      return false;
    } //end SendDiSEqCCommand

    /// <summary>
    /// gets the diseqc reply
    /// </summary>
    /// <param name="ulRange">The DisEqCPort Port.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    protected bool ReadDiSEqCCommand(out ulong ulRange)
    {
      int hr = 0;
      ulRange = 0;
      // get ControlNode of tuner control node
      object ControlNode = null;
      hr = _TunerDevice.GetControlNode(0, 1, 0, out ControlNode);
      if (hr == 0)
      // retrieve the BDA_DeviceControl interface 
      {
        IBDA_DeviceControl DecviceControl = (IBDA_DeviceControl)_TunerDevice;
        if (DecviceControl != null)
        {
          if (ControlNode != null)
          {
            IBDA_FrequencyFilter FrequencyFilter = (IBDA_FrequencyFilter)ControlNode;
            if (FrequencyFilter != null)
            {
              hr = FrequencyFilter.get_Range(out ulRange);
              if (hr == 0)
              {
                return true;
              }
            }
          }
        }
      }
      return false;
    } //end ReadDiSEqCCommand

    /// <summary>
    /// Determines whether [is cam present].
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam present]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      return false;
    }

    public bool IsGenericBDAS
    {
      get
      {
        return _isGenericBDAS;
      }
    }
  }
}
