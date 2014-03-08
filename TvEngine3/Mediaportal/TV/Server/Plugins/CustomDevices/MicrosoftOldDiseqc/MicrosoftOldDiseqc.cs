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

using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftOldDiseqc
{
  /// <summary>
  /// This class provides an implementation of DiSEqC 1.0 support which is compatible with the
  /// pre-Windows-7 defacto BDA standard.
  /// </summary>
  public class MicrosoftOldDiseqc : BaseCustomDevice, IDiseqcDevice
  {
    #region variables

    private bool _isMicrosoftOldDiseqc = false;

    private IBDA_FrequencyFilter _interface = null;
    private IBDA_DeviceControl _deviceControl = null;

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // This implementation should only be used when more specialised interfaces are not available.
        return 1;
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
        return "Microsoft old DiSEqC";
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      this.LogDebug("Microsoft old DiSEqC: initialising");

      if (_isMicrosoftOldDiseqc)
      {
        this.LogWarn("Microsoft old DiSEqC: extension already initialised");
        return true;
      }

      if (tunerType != CardType.DvbS)
      {
        this.LogDebug("Microsoft old DiSEqC: tuner type not supported");
        return false;
      }

      _deviceControl = context as IBDA_DeviceControl;
      if (_deviceControl == null)
      {
        this.LogDebug("Microsoft old DiSEqC: device control interface not supported");
        return false;
      }

      IBDA_Topology topology = context as IBDA_Topology;
      if (topology == null)
      {
        this.LogDebug("Microsoft old DiSEqC: topology interface not supported");
        return false;
      }

      // The IBDA_FrequencyFilter.put_Range() function was the de-facto "BDA" standard for DiSEqC
      // 1.0 prior to the introduction of IBDA_DiseqCommand in Windows 7.
      object controlNode;
      int hr = topology.GetControlNode(0, 1, 0, out controlNode);
      _interface = controlNode as IBDA_FrequencyFilter;
      if (hr != (int)HResult.Severity.Success || _interface == null)
      {
        this.LogDebug("Microsoft old DiSEqC: frequency filter interface not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogInfo("Microsoft old DiSEqC: extension supported");
      _isMicrosoftOldDiseqc = true;
      return true;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>
    /// The Microsoft interface does not support directly setting the 22 kHz tone state. The tuning
    /// request LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      // Not implemented.
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <remarks>
    /// Drivers don't all behave the same. There are notes about MS network providers messing up
    /// the put_Range() method when attempting to send commands before the tune request
    /// (http://www.dvbdream.org/forum/viewtopic.php?f=1&t=608&start=15). Take care if modifying
    /// this method!
    /// </remarks>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(byte[] command)
    {
      this.LogDebug("Microsoft old DiSEqC: send DiSEqC command");

      if (!_isMicrosoftOldDiseqc)
      {
        this.LogWarn("Microsoft old DiSEqC: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogError("Microsoft old DiSEqC: command not supplied");
        return true;
      }

      // Attempt to translate the raw command back into a DiSEqC 1.0 command. This interface only
      // supports DiSEqC 1.0 switch commands.
      int portNumber = -1;
      if (command.Length == 4 &&
        (command[0] == (byte)DiseqcFrame.CommandFirstTransmissionNoReply ||
        command[0] == (byte)DiseqcFrame.CommandRepeatTransmissionNoReply) &&
        command[1] == (byte)DiseqcAddress.AnySwitch &&
        command[2] == (byte)DiseqcCommand.WriteN0)
      {
        portNumber = (command[3] & 0xc) >> 2;
        this.LogDebug("Microsoft old DiSEqC: DiSEqC 1.0 command recognised for port {0}", portNumber);
      }
      else
      {
        this.LogError("Microsoft old DiSEqC: command not supported");
        return false;
      }

      // Prepare the command.
      bool success = true;
      int hr = _deviceControl.StartChanges();
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("Microsoft old DiSEqC: failed to start device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        success = false;
      }
      if (success)
      {
        // The two rightmost bytes encode option and position respectively.
        if (portNumber > 1)
        {
          portNumber -= 2;
          portNumber |= 0x100;
        }
        this.LogDebug("Microsoft old DiSEqC: range = 0x{0:x4}", portNumber);
        hr = _interface.put_Range((ulong)portNumber);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft old DiSEqC: failed to put range, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      // Finalise (send) the command.
      if (success)
      {
        hr = _deviceControl.CheckChanges();
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft old DiSEqC: failed to check device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }
      if (success)
      {
        hr = _deviceControl.CommitChanges();
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("Microsoft old DiSEqC: failed to commit device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }

      this.LogDebug("Microsoft old DiSEqC: result = {0}", success);
      return success;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadDiseqcResponse(out byte[] response)
    {
      // Not supported.
      response = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      Release.ComObject("Microsoft old DiSEqC interface", ref _interface);
      _deviceControl = null;
      _isMicrosoftOldDiseqc = false;
    }

    #endregion
  }
}