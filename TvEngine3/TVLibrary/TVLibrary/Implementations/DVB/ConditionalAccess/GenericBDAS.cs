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

/*
 * I have only tested this with Terratec Cinergy DVB-S 1200.
 * However, it should work with other Philips SAA-7146 based cards as well.
 * Use this at your own risk!!
 * /Digi
 */
using System;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{

  class GenericBDAS
  {
    #region variables

    private readonly IKsPropertySet _propertySet;
    protected IBDA_Topology _TunerDevice;
    protected bool _isGenericBDAS;
    #endregion

    #region constants

    readonly Guid guidBdaDigitalDemodulator = new Guid(0xef30f379, 0x985b, 0x4d10, 0xb6, 0x40, 0xa7, 0x9d, 0x5e, 0x4, 0xe1, 0xe0);
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericBDAS"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public GenericBDAS(IBaseFilter tunerFilter)
    {
      _TunerDevice = (IBDA_Topology)tunerFilter;
      //check if the BDA driver supports DiSEqC
      IPin pin = DsFindPin.ByName(tunerFilter, "MPEG2 Transport");
      if (pin != null)
      {
        _propertySet = pin as IKsPropertySet;
        if (_propertySet != null)
        {
          KSPropertySupport supported;
          _propertySet.QuerySupported(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
          //Log.Log.Info("GenericBDAS: QuerySupported: {0}", supported);
          if ((supported & KSPropertySupport.Set) != 0)
          {
            //Log.Log.Info("GenericBDAS: DiSEqC capable card found!");
            _isGenericBDAS = true;
          }
        }
      }
    }

    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      switch (channel.DisEqc)
      {
        case DisEqcType.Level1AA:
          Log.Log.Info("GenericBDAS:  Level1AA - SendDiSEqCCommand(0x00)");
          SendDiSEqCCommand(0x00);
          break;
        case DisEqcType.Level1AB:
          Log.Log.Info("GenericBDAS:  Level1AB - SendDiSEqCCommand(0x01)");
          SendDiSEqCCommand(0x01);
          break;
        case DisEqcType.Level1BA:
          Log.Log.Info("GenericBDAS:  Level1BA - SendDiSEqCCommand(0x0100)");
          SendDiSEqCCommand(0x0100);
          break;
        case DisEqcType.Level1BB:
          Log.Log.Info("GenericBDAS:  Level1BB - SendDiSEqCCommand(0x0101)");
          SendDiSEqCCommand(0x0101);
          break;
        case DisEqcType.SimpleA:
          Log.Log.Info("GenericBDAS:  SimpleA - SendDiSEqCCommand(0x00)");
          SendDiSEqCCommand(0x00);
          break;
        case DisEqcType.SimpleB:
          Log.Log.Info("GenericBDAS:  SimpleB - SendDiSEqCCommand(0x01)");
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
      Log.Log.Info("GenericBDAS:  SendDiSEqC Command {0}", ulRange);
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
                Log.Log.Info("GenericBDAS:  put_Range:{0} success:{1}", ulRange, hr);
                if (hr == 0)
                {
                  // did it accept the changes? 
                  hr = DecviceControl.CheckChanges();
                  if (hr == 0)
                  {
                    hr = DecviceControl.CommitChanges();
                    if (hr == 0)
                    {
                      Log.Log.Info("GenericBDAS:  CommitChanges() Succeeded");
                      return true;
                    }
                    // reset configuration
                    Log.Log.Info("GenericBDAS:  CommitChanges() Failed!");
                    DecviceControl.StartChanges();
                    DecviceControl.CommitChanges();
                    return false;
                  }
                  Log.Log.Info("GenericBDAS:  CheckChanges() Failed!");
                  return false;
                }
                Log.Log.Info("GenericBDAS:  put_Range Failed!");
                return false;
              }
            }
          }
        }
      }
      Log.Log.Info("GenericBDAS:  GetControlNode Failed!");
      return false;
    } //end SendDiSEqCCommand

    /// <summary>
    /// gets the diseqc reply
    /// </summary>
    /// <param name="pulRange">The DisEqCPort Port.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    protected bool ReadDiSEqCCommand(out ulong pulRange)
    {
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
                hr = FrequencyFilter.get_Range(out pulRange);
                Log.Log.Info("GenericBDAS:  get_Range:{0} success:{1}", pulRange, hr);
                if (hr == 0)
                {
                  return true;
                }
                Log.Log.Info("GenericBDAS:  get_Range Failed!");
                return false;
              }
            }
          }
        }
      }
      Log.Log.Info("GenericBDAS:  GetControlNode Failed!");
      pulRange = 0;
      return false;
    }//end ReadDiSEqCCommand

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
