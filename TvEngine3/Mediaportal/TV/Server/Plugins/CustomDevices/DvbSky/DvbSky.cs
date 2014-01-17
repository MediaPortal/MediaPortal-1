#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DvbSky
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for DVBSky tuners. Actually with the
  /// exception of the GUIDs, the DVBSky conditional access interface is identical to the NetUP
  /// conditional access interface, and their DiSEqC interface is identical to the Conexant
  /// interface.
  /// </summary>
  public class DvbSky : Conexant.Conexant, IConditionalAccessProvider, IConditionalAccessMenuActions
  {
    #region enums

    private enum BdaExtensionProperty
    {
      /// For sending and receiving DiSEqC messages.
      DiseqcMessage = 0,
      /// For receiving remote keypresses.
      RemoteCode,
      /// For passing blind scan parameters and commands.
      BlindScanCommand,
      /// For retrieving blind scan results.
      BlindScanData,
      /// For getting or setting DVB-C2/T2 PLP information.
      DvbPlpId,
      /// For getting the MAC address for tuner 1.
      MacAddressTuner1,
      /// For getting the MAC address for tuner 2.
      MacAddressTuner2
    }

    #endregion

    #region constants

    private static readonly Guid DVBSKY_BDA_EXTENSION_GENERAL_PROPERTY_SET = new Guid(0x03cbdcb9, 0x36dc, 0x4072, 0xac, 0x42, 0x2f, 0x94, 0xf4, 0xec, 0xa0, 0x5e);
    private static readonly Guid DVBSKY_BDA_EXTENSION_CA_PROPERTY_SET = new Guid(0x4fdc5d3a, 0x1543, 0x479e, 0x9f, 0xc3, 0xb7, 0xdb, 0xa4, 0x73, 0xfb, 0x95);

    private static readonly int MAC_ADDRESS_LENGTH = 6;

    #endregion

    #region variables

    private bool _isDvbSky = false;
    private NetUp.NetUp _netUpInterface = null;

    #endregion

    private void ReadMacAddresses()
    {
      this.LogDebug("DVBSky: read MAC addresses");

      IntPtr buffer = Marshal.AllocCoTaskMem(MAC_ADDRESS_LENGTH);
      try
      {
        BdaExtensionProperty[] properties = new BdaExtensionProperty[] { BdaExtensionProperty.MacAddressTuner1, BdaExtensionProperty.MacAddressTuner2 };
        string[] propertyNames = new string[] { "tuner 1 MAC address", "tuner 2 MAC address" };

        KSPropertySupport support;
        for (int i = 0; i < properties.Length; i++)
        {
          int hr = _propertySet.QuerySupported(_propertySetGuid, (int)BdaExtensionProperty.MacAddressTuner1, out support);
          if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Get))
          {
            this.LogWarn("DVBSky: {0} property not supported, hr = 0x{1:x} ({2})", propertyNames[i], hr, HResult.GetDXErrorString(hr));
          }
          else
          {
            for (int b = 0; b < MAC_ADDRESS_LENGTH; b++)
            {
              Marshal.WriteByte(buffer, b, 0);
            }
            int returnedByteCount;
            hr = _propertySet.Get(_propertySetGuid, (int)properties[i], _instanceBuffer, INSTANCE_SIZE, buffer, MAC_ADDRESS_LENGTH, out returnedByteCount);
            if (hr != (int)HResult.Severity.Success || returnedByteCount != MAC_ADDRESS_LENGTH)
            {
              this.LogWarn("DVBSky: result = failure, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
            }
            else
            {
              byte[] address = new byte[MAC_ADDRESS_LENGTH];
              Marshal.Copy(buffer, address, 0, MAC_ADDRESS_LENGTH);
              this.LogDebug("  {0} = {1}", propertyNames[i], BitConverter.ToString(address).ToLowerInvariant());
            }
          }
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
      }
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 75;
      }
    }

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "DVBSky";
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalIdentifier">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalIdentifier, CardType tunerType, object context)
    {
      this.LogDebug("DVBSky: initialising");

      if (_isDvbSky)
      {
        this.LogWarn("DVBSky: extension already initialised");
        return true;
      }

      this.LogDebug("DVBSky: checking base Conexant interface support");
      _propertySetGuid = DVBSKY_BDA_EXTENSION_GENERAL_PROPERTY_SET;
      if (!base.Initialise(tunerExternalIdentifier, tunerType, context))
      {
        this.LogDebug("DVBSky: base Conexant interface not supported");
        return false;
      }
      this.LogInfo("DVBSky: extension supported");
      _isDvbSky = true;
      ReadMacAddresses();

      this.LogDebug("DVBSky: checking base NetUP conditional access support");
      _netUpInterface = new NetUp.NetUp(DVBSKY_BDA_EXTENSION_CA_PROPERTY_SET);
      if (_netUpInterface.Initialise(tunerExternalIdentifier, tunerType, context))
      {
        this.LogDebug("DVBSky: conditional access interface supported");
      }
      else
      {
        this.LogDebug("DVBSky: conditional access interface not supported");
        _netUpInterface.Dispose();
        _netUpInterface = null;
      }
      return true;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface. For the interface to be opened successfully it is expected
    /// that any necessary hardware (such as a CI slot) is connected.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenInterface()
    {
      if (_netUpInterface != null)
      {
        return _netUpInterface.OpenInterface();
      }
      return false;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      if (_netUpInterface != null)
      {
        return _netUpInterface.CloseInterface();
      }
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="resetTuner">This parameter will be set to <c>true</c> if the tuner must be reset
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    public bool ResetInterface(out bool resetTuner)
    {
      resetTuner = false;
      if (_netUpInterface != null)
      {
        return _netUpInterface.ResetInterface(out resetTuner);
      }
      return false;
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsInterfaceReady()
    {
      if (_netUpInterface != null)
      {
        return _netUpInterface.IsInterfaceReady();
      }
      return false;
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="channel">The channel information associated with the service which the command relates to.</param>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more services
    ///   simultaneously. This parameter gives the interface an indication of the number of services that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program map table for the service.</param>
    /// <param name="cat">The conditional access table for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    {
      if (_netUpInterface != null)
      {
        return _netUpInterface.SendCommand(channel, listAction, command, pmt, cat);
      }
      return false;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBacks">The call back delegate.</param>
    public void SetCallBacks(IConditionalAccessMenuCallBacks callBacks)
    {
      if (_netUpInterface != null)
      {
        _netUpInterface.SetCallBacks(callBacks);
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterMenu()
    {
      if (_netUpInterface != null)
      {
        return _netUpInterface.EnterMenu();
      }
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseMenu()
    {
      if (_netUpInterface != null)
      {
        return _netUpInterface.CloseMenu();
      }
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenuEntry(byte choice)
    {
      if (_netUpInterface != null)
      {
        return _netUpInterface.SelectMenuEntry(choice);
      }
      return false;
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool AnswerEnquiry(bool cancel, string answer)
    {
      if (_netUpInterface != null)
      {
        return _netUpInterface.AnswerEnquiry(cancel, answer);
      }
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      if (_netUpInterface != null)
      {
        _netUpInterface.Dispose();
        _netUpInterface = null;
      }
      base.Dispose();
      _isDvbSky = false;
    }

    #endregion
  }
}
