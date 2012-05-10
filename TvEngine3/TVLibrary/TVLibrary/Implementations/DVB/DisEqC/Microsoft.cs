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
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling DiSEqC for tuners that support the Microsoft
  /// IBDA_DiSEqCommand interface.
  /// </summary>
  public class Microsoft : IDiSEqCController, IDisposable
  {
    #region IBDA_DiseqCommand

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("F84E2AB0-3C6B-45E3-A0FC-8669D4B81F11"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IBDA_DiseqCommand
    {
      [PreserveSig]
      int put_EnableDiseqCommands(
        [In] int bEnable
      );

      [PreserveSig]
      int put_DiseqLNBSource(
        [In] uint ulLNBSource
      );

      [PreserveSig]
      int put_DiseqUseToneBurst(
        [In] int bUseToneBurst
      );

      [PreserveSig]
      int put_DiseqRepeats(
        [In] uint ulRepeats
      );

      [PreserveSig]
      int put_DiseqSendCommand(
        [In] uint ulRequestId, 
        [In] uint ulcbCommandLen, 
        [In] IntPtr pbCommand
      );

      [PreserveSig]
      int get_DiseqResponse(
        [In] uint ulRequestId, 
        [Out] out IntPtr pulcbResponseLen, 
        [Out] out IntPtr pbResponse
      );
    }

    #endregion

    #region variables

    private bool _isMicrosoft = false;
    private IBDA_DiseqCommand _interface = null;
    private uint _requestId = 0;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Microsoft"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Microsoft(IBaseFilter tunerFilter)
    {
      IBDA_Topology topology = tunerFilter as IBDA_Topology;
      if (topology == null)
      {
        Log.Log.Debug("Microsoft: tuner filter is not a topology");
        return;
      }

      // Get the node types on the tuner filter.
      int nodeTypeCount;
      int[] nodeTypes = new int[32];
      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      if (hr != 0)
      {
        Log.Log.Debug("Microsoft: failed to get topology node types, 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }
      else if (nodeTypeCount == 0)
      {
        Log.Log.Debug("Microsoft: no node types on filter");
        return;
      }

      // Check the GUIDs on each node.
      Guid[] interfaceGuids = new Guid[32];
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        int interfaceCount;
        hr = topology.GetNodeInterfaces(nodeTypes[i], out interfaceCount, 32, interfaceGuids);
        if (hr != 0)
        {
          Log.Log.Debug("Microsoft: failed to get interfaces for node {0}, 0x{1:x} ({2})", i, hr, HResult.GetDXErrorString(hr));
          continue;
        }

        for (int j = 0; j < interfaceCount; j++)
        {
          if (interfaceGuids[j].Equals(typeof(IBDA_DiseqCommand).GUID))
          {
            // Found the interface. Now attempt to get a reference.
            object controlNode;
            hr = topology.GetControlNode(0, 1, nodeTypes[i], out controlNode);
            _interface = controlNode as IBDA_DiseqCommand;
            if (hr != 0 || _interface == null)
            {
              Log.Log.Debug("Microsoft: failed to get the command interface for node {0}, 0x{1:x} ({2})", i, hr, HResult.GetDXErrorString(hr));
              if (controlNode != null)
              {
                Release.ComObject(controlNode);
              }
              continue;
            }

            Log.Log.WriteFile("Microsoft: supported tuner detected");
            _isMicrosoft = true;

            hr = _interface.put_DiseqRepeats(0);
            if (hr != 0)
            {
              Log.Log.Debug("Microsoft: failed to turn off repeats, 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            }
            return;
          }
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Microsoft BDA-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Microsoft BDA-compatible tuner, otherwise <c>false</c></value>
    public bool IsMicrosoft
    {
      get
      {
        return _isMicrosoft;
      }
    }

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("Microsoft: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (toneBurstState == ToneBurst.Off)
      {
        Log.Log.Debug("Microsoft: result = success");
        return true;
      }

      // First enable tone burst commands.
      int hr = _interface.put_DiseqUseToneBurst(1);
      if (hr != 0)
      {
        Log.Log.Debug("Microsoft: failed to enable tone burst, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      uint portNumber = 0;
      if (toneBurstState == ToneBurst.DataBurst)
      {
        portNumber = 1;
      }

      // Send a DiSEqC command which sends the appropriate tone burst
      // command as well.
      hr = _interface.put_DiseqLNBSource(portNumber);
      if (hr == 0)
      {
        Log.Log.Debug("Microsoft: result = success");
        return true;
      }

      Log.Log.Debug("Microsoft: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with parameters adjusted as necessary.</returns>
    public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
    {
      Log.Log.Debug("Microsoft: set tuning parameters");
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return channel;
      }

      if (ch.ModulationType == ModulationType.ModQpsk)
      {
        ch.ModulationType = ModulationType.ModNbcQpsk;
      }
      else if (ch.ModulationType == ModulationType.Mod8Psk)
      {
        ch.ModulationType = ModulationType.ModNbc8Psk;
      }
      else if (ch.ModulationType == ModulationType.ModNotSet)
      {
        ch.ModulationType = ModulationType.ModQpsk;
      }
      Log.Log.Debug("  modulation     = {0}", ch.ModulationType);

      return ch as DVBBaseChannel;
    }

    #region IDiSEqCController members

    /// <summary>
    /// Send the appropriate DiSEqC 1.0 switch command to switch to a given channel.
    /// </summary>
    /// <param name="parameters">The scan parameters.</param>
    /// <param name="channel">The channel.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      int hr;
      if (channel.DisEqc == DisEqcType.None)
      {
        hr = _interface.put_EnableDiseqCommands(0);
        if (hr == 0)
        {
          Log.Log.Debug("Microsoft: result = success");
          return true;
        }

        Log.Log.Debug("Microsoft: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      hr = _interface.put_EnableDiseqCommands(1);
      if (hr != 0)
      {
        Log.Log.Debug("Microsoft: failed to enable DiSEqC commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      bool isHighBand = BandTypeConverter.IsHiBand(channel, parameters);
      ToneBurst toneBurst = ToneBurst.Off;
      bool successDiseqc = true;
      if (channel.DisEqc == DisEqcType.SimpleA)
      {
        toneBurst = ToneBurst.ToneBurst;
      }
      else if (channel.DisEqc == DisEqcType.SimpleB)
      {
        toneBurst = ToneBurst.DataBurst;
      }
      else
      {
        hr = _interface.put_DiseqUseToneBurst(0);
        if (hr != 0)
        {
          Log.Log.Debug("Microsoft: failed to disable tone burst, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          successDiseqc = false;
        }

        int antennaNr = BandTypeConverter.GetAntennaNr(channel);
        bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                              (channel.Polarisation == Polarisation.CircularL));
        byte command = 0xf0;
        command |= (byte)(isHighBand ? 1 : 0);
        command |= (byte)((isHorizontal) ? 2 : 0);
        command |= (byte)((antennaNr - 1) << 2);
        successDiseqc = SendDiSEqCCommand(new byte[4] { 0xe0, 0x10, 0x38, command });
      }

      Tone22k tone22k = Tone22k.Off;
      if (isHighBand)
      {
        tone22k = Tone22k.On;
      }
      bool successTone = SetToneState(toneBurst, tone22k);

      return (successDiseqc && successTone);
    }

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="command">The DiSEqC command to send.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("Microsoft: send DiSEqC command");

      IntPtr buffer = Marshal.AllocCoTaskMem(command.Length);
      for (int i = 0; i < command.Length; i++)
      {
        Marshal.WriteByte(buffer, i, command[i]);
      }
      int hr = _interface.put_DiseqSendCommand(_requestId, (uint)command.Length, buffer);
      Marshal.FreeCoTaskMem(buffer);
      if (hr == 0)
      {
        Log.Log.Debug("Microsoft: result = success");
        _requestId++;
        return true;
      }

      Log.Log.Debug("Microsoft: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      Log.Log.Debug("Microsoft: read DiSEqC command");
      reply = null;

      IntPtr lengthBuffer = Marshal.AllocCoTaskMem(sizeof(UInt32));
      IntPtr replyBuffer = Marshal.AllocCoTaskMem(64);
      try
      {
        int hr = _interface.get_DiseqResponse(_requestId, out lengthBuffer, out replyBuffer);
        if (hr == 0)
        {
          Log.Log.Debug("Microsoft: result = success");
          // Copy the reply into the return array.
          int replyLength = Marshal.ReadInt32(lengthBuffer, 0);
          if (replyLength > 0)
          {
            reply = new byte[replyLength];
            for (int i = 0; i < replyLength; i++)
            {
              reply[i] = Marshal.ReadByte(replyBuffer, i);
            }
          }
          return true;
        }

        Log.Log.Debug("Microsoft: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      finally
      {
        Marshal.FreeCoTaskMem(lengthBuffer);
        Marshal.FreeCoTaskMem(replyBuffer);
      }
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release COM resources.
    /// </summary>
    public void Dispose()
    {
      if (_isMicrosoft)
      {
        Release.ComObject(_interface);
      }
    }

    #endregion
  }
}
