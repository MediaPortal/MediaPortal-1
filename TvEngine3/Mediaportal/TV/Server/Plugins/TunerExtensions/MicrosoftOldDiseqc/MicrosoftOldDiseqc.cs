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
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftOldDiseqc
{
  /// <summary>
  /// This class provides an implementation of DiSEqC 1.0 support which is compatible with the
  /// pre-Windows-7 defacto BDA standard.
  /// </summary>
  public class MicrosoftOldDiseqc : BaseTunerExtension, IDiseqcDevice, IDisposable
  {
    #region variables

    private bool _isMicrosoftOldDiseqc = false;

    private IBDA_FrequencyFilter _interface = null;
    private IBDA_DeviceControl _deviceControl = null;

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
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
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Microsoft old DiSEqC";
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Microsoft old DiSEqC: initialising");

      if (_isMicrosoftOldDiseqc)
      {
        this.LogWarn("Microsoft old DiSEqC: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Microsoft old DiSEqC: tuner type not supported");
        return false;
      }

      _deviceControl = context as IBDA_DeviceControl;
      if (_deviceControl == null)
      {
        this.LogDebug("Microsoft old DiSEqC: context is not a device control");
        return false;
      }

      // The IBDA_FrequencyFilter.put_Range() function was the de-facto "BDA" standard
      // for DiSEqC 1.0 support prior to the introduction of IBDA_DiseqCommand in Windows
      // 7. Try to find the interface.
      IBDA_Topology topology = context as IBDA_Topology;
      if (topology == null)
      {
        this.LogDebug("Microsoft old DiSEqC: context is not a topology");
        return false;
      }

      int nodeTypeCount;
      int[] nodeTypes = new int[33];
      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Microsoft old DiSEqC: failed to get topology node types, hr = 0x{0:x}", hr);
        return false;
      }

      Guid[] interfaces = new Guid[33];
      int interfaceCount;
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        hr = topology.GetNodeInterfaces(nodeTypes[i], out interfaceCount, 32, interfaces);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          continue;
        }
        for (int j = 0; j < interfaceCount; j++)
        {
          if (interfaces[j] == typeof(IBDA_FrequencyFilter).GUID)
          {
            object controlNode;
            hr = topology.GetControlNode(0, 1, nodeTypes[i], out controlNode);
            _interface = controlNode as IBDA_FrequencyFilter;
            if (hr == (int)NativeMethods.HResult.S_OK && _interface != null)
            {
              this.LogInfo("Microsoft old DiSEqC: extension supported");
              _isMicrosoftOldDiseqc = true;
              return true;
            }
            Release.ComObject("Microsoft old DiSEqC topology control node", ref controlNode);
          }
        }
      }

      this.LogDebug("Microsoft old DiSEqC: frequency filter interface not supported");
      return false;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <remarks>
    /// Drivers don't all behave the same. There are notes about MS network
    /// providers messing up the put_Range() method when attempting to send
    /// commands before the tune request
    /// (http://www.dvbdream.org/forum/viewtopic.php?f=1&t=608&start=15). Take
    /// care if modifying this method!
    /// </remarks>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(byte[] command)
    {
      this.LogDebug("Microsoft old DiSEqC: send DiSEqC command");

      if (!_isMicrosoftOldDiseqc)
      {
        this.LogWarn("Microsoft old DiSEqC: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Microsoft old DiSEqC: DiSEqC command not supplied");
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
        this.LogError("Microsoft old DiSEqC: DiSEqC command not supported");
        Dump.DumpBinary(command);
        return false;
      }

      // Prepare the command.
      bool success = true;
      int hr = _deviceControl.StartChanges();
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Microsoft old DiSEqC: failed to start device control changes, hr = 0x{0:x}", hr);
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
        hr = _interface.put_Range(portNumber);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft old DiSEqC: failed to put range, hr = 0x{0:x}", hr);
          success = false;
        }
      }

      // Finalise (send) the command.
      if (success)
      {
        hr = _deviceControl.CheckChanges();
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft old DiSEqC: failed to check device control changes, hr = 0x{0:x}", hr);
          success = false;
        }
      }
      if (success)
      {
        hr = _deviceControl.CommitChanges();
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft old DiSEqC: failed to commit device control changes, hr = 0x{0:x}", hr);
          success = false;
        }
      }

      this.LogDebug("Microsoft old DiSEqC: result = {0}", success);
      return success;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      // Not supported.
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      // Set by tune request LNB frequency parameters.
      return true;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
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
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~MicrosoftOldDiseqc()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        Release.ComObject("Microsoft old DiSEqC interface", ref _interface);
        _deviceControl = null;
        _isMicrosoftOldDiseqc = false;
      }
    }

    #endregion
  }
}