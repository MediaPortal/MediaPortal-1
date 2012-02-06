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
using System.Threading;
using DirectShowLib;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Channels;
using DirectShowLib.BDA;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for NetUP tuners.
  /// </summary>
  public class NetUp : IDiSEqCController, ICiMenuActions, IDisposable
  {
    #region enums

    private enum NetUpIoControl : uint
    {
      Diseqc = 0x100000,

      CiStatus = 0x200000,

      MmiEnterMenu = 0x300000,
      MmiGetMenu = 0x310000,
      MmiAnswerMenu = 0x320000,
      MmiClose = 0x330000,

      PmtListChange = 0x400000
    }

    [Flags]
    private enum NetUpCiState
    {
      Empty = 0,
      CamPresent,
      MmiDataReady
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct CiStateInfo    // NETUP_CAM_STATUS
    {
      public NetUpCiState CiState;
      public UInt16 Manufacturer;
      public UInt16 ManufacturerCode;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String RootMenuTitle;
    }

    private struct MmiMenuEntry
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct MmiData    // NETUP_CAM_MENU
    {
      public bool IsMenu;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String Title;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String SubTitle;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxStringLength)]
      public String Footer;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCamMenuEntries)]
      public MmiMenuEntry[] Entries;
      public UInt32 EntryCount;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0x5aa642f2, 0xbf94, 0x4199, 0xa9, 0x8c, 0xc2, 0x22, 0x20, 0x91, 0xe3, 0xc3);

    private const int InstanceSize = 32;
    private const int CommandSize = 48;
    private const int CiStateInfoSize = 264 + 32;
    private const int MmiDataSize = 8 + (MaxStringLength * (3 + MaxCamMenuEntries));
    private const int MaxBufferSize = 65536;
    private const int MaxStringLength = 256;
    private const int MaxCamMenuEntries = 64;
    private const int MaxDiseqcMessageLength = 64;    // This is to match the _generalBuffer size - the driver limit is MaxBufferSize.

    #endregion

    /// <summary>
    /// This class is used to "hide" the complexity of filling the command buffer.
    /// </summary>
    private class NetUpCommand
    {
      private UInt32 _controlCode;
      private IntPtr _inBuffer;
      private Int32 _inBufferSize;
      private IntPtr _outBuffer;
      private Int32 _outBufferSize;

      public NetUpCommand(UInt32 controlCode, IntPtr inBuffer, Int32 inBufferSize, IntPtr outBuffer, Int32 outBufferSize)
      {
        _controlCode = controlCode;
        _inBuffer = inBuffer;
        _inBufferSize = inBufferSize;
        _outBuffer = outBuffer;
        _outBufferSize = outBufferSize;
      }

      public int Execute(IKsPropertySet ps, out int returnedByteCount)
      {
        returnedByteCount = 0;
        int hr = 1; // fail
        if (ps == null)
        {
          return hr;
        }

        IntPtr instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
        IntPtr commandBuffer = Marshal.AllocCoTaskMem(CommandSize);
        IntPtr returnedByteCountBuffer = Marshal.AllocCoTaskMem(sizeof(int));
        try
        {
          // Clear buffers. This is probably not actually needed, but better
          // to be safe than sorry!
          for (int i = 0; i < InstanceSize; i++)
          {
            Marshal.WriteByte(instanceBuffer, i, 0);
          }
          Marshal.WriteInt32(returnedByteCountBuffer, 0);

          /*Marshal.WriteInt32(commandBuffer, 0, (Int32)_controlCode);
          Marshal.WriteInt32(commandBuffer, 4, _inBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 8, _inBufferSize);
          Marshal.WriteInt32(commandBuffer, 12, _outBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 16, _outBufferSize);
          Marshal.WriteInt32(commandBuffer, 20, returnedByteCountBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 24, 0);
          Marshal.WriteInt32(commandBuffer, 28, 0);
          Marshal.WriteInt32(commandBuffer, 32, 0);
          Marshal.WriteInt32(commandBuffer, 36, 0);
          Marshal.WriteInt32(commandBuffer, 40, 0);
          Marshal.WriteInt32(commandBuffer, 44, 0);*/

          Marshal.WriteInt64(commandBuffer, 0, _controlCode);
          Marshal.WriteInt64(commandBuffer, 8, _inBuffer.ToInt64());
          Marshal.WriteInt64(commandBuffer, 16, _inBufferSize);
          Marshal.WriteInt64(commandBuffer, 24, _outBuffer.ToInt64());
          Marshal.WriteInt64(commandBuffer, 32, _outBufferSize);
          Marshal.WriteInt64(commandBuffer, 40, returnedByteCountBuffer.ToInt64());

          hr = ps.Set(BdaExtensionPropertySet, 0, instanceBuffer, InstanceSize, commandBuffer, CommandSize);
          if (hr == 0)
          {
            returnedByteCount = Marshal.ReadInt32(returnedByteCountBuffer);
          }
        }
        finally
        {
          Marshal.FreeCoTaskMem(instanceBuffer);
          Marshal.FreeCoTaskMem(commandBuffer);
          Marshal.FreeCoTaskMem(returnedByteCountBuffer);
        }
        return hr;
      }
    }

    #region variables

    private bool _isNetUp = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isCamReady = false;

    // Functions that are called from both the main TV service threads
    // as well as the MMI handler thread use their own local buffer to
    // avoid buffer data corruption. Otherwise functions called exclusively
    // by the MMI handler thread use the MMI buffer and other functions
    // use the general buffer.
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _mmiBuffer = IntPtr.Zero;

    private IKsPropertySet _propertySet = null;

    private Thread _mmiHandlerThread = null;
    private bool _stopMmiHandlerThread = false;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="NetUp"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public NetUp(IBaseFilter tunerFilter)
    {
      if (tunerFilter == null)
      {
        return;
      }

      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        Log.Log.Debug("NetUP: pin is not a property set");
        Release.ComObject(pin);
        return;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, 0, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("NetUP: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        Release.ComObject(pin);
      }

      Log.Log.Debug("NetUP: supported tuner detected");
      _isNetUp = true;
      _generalBuffer = Marshal.AllocCoTaskMem(MaxDiseqcMessageLength);
      _mmiBuffer = Marshal.AllocCoTaskMem(MmiDataSize);

      _isCiSlotPresent = IsCiSlotPresent();
      if (_isCiSlotPresent)
      {
        _isCamPresent = IsCamPresent();
        if (_isCamPresent)
        {
          _isCamReady = IsCamReady();
        }
      }

      StartMmiHandlerThread();
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a NetUP-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a NetUP-compatible tuner, otherwise <c>false</c></value>
    public bool IsNetUp
    {
      get
      {
        return _isNetUp;
      }
    }

    #region conditional access

    /// <summary>
    /// Gets the conditional access interface status.
    /// </summary>
    /// <param name="ciState">State of the CI slot.</param>
    /// <returns>an HRESULT indicating whether the CI status was successfully retrieved</returns>
    private int GetCiStatus(out CiStateInfo ciState)
    {
      ciState = new CiStateInfo();

      // Use a local buffer here because this function is called from the MMI
      // polling thread as well as indirectly from the main TV service thread.
      IntPtr buffer = Marshal.AllocCoTaskMem(CiStateInfoSize);
      for (int i = 0; i < CiStateInfoSize; i++)
      {
        Marshal.WriteByte(buffer, i, 0);
      }
      NetUpCommand command = new NetUpCommand((uint)NetUpIoControl.CiStatus, IntPtr.Zero, 0, buffer, CiStateInfoSize);
      int returnedByteCount;
      int hr = command.Execute(_propertySet, out returnedByteCount);
      if (hr == 0)
      {
        //DVB_MMI.DumpBinary(buffer, 0, returnedByteCount);
        ciState = (CiStateInfo)Marshal.PtrToStructure(buffer, typeof(CiStateInfo));
      }
      Marshal.FreeCoTaskMem(buffer);
      return hr;
    }

    /// <summary>
    /// Determines whether a CI slot is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CI slot is present, otherwise <c>false</c></returns>
    public bool IsCiSlotPresent()
    {
      Log.Log.Debug("NetUP: is CI slot present");

      // Both NetUP PC tuner cards support CI slots.
      Log.Log.Debug("NetUP: result = {0}", true);
      return true;
    }

    /// <summary>
    /// Determines whether a CAM is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present, otherwise <c>false</c></returns>
    public bool IsCamPresent()
    {
      Log.Log.Debug("NetUP: is CAM present");
      if (!_isCiSlotPresent)
      {
        Log.Log.Debug("NetUP: CI slot not present");
        return false;
      }

      CiStateInfo info;
      int hr = GetCiStatus(out info);
      if (hr != 0)
      {
        Log.Log.Debug("NetUP: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      Log.Log.Debug("NetUP: state = {0}", info.CiState.ToString());

      bool camPresent = false;
      if (info.CiState != NetUpCiState.Empty)
      {
        camPresent = true;
      }
      Log.Log.Debug("NetUP: result = {0}", camPresent);
      return camPresent;
    }

    /// <summary>
    /// Determines whether a CAM is present and ready for interaction.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present and ready, otherwise <c>false</c></returns>
    public bool IsCamReady()
    {
      Log.Log.Debug("NetUP: is CAM ready");
      if (!_isCamPresent)
      {
        Log.Log.Debug("NetUP: CAM not present");
        return false;
      }

      CiStateInfo info;
      int hr = GetCiStatus(out info);
      if (hr != 0)
      {
        Log.Log.Debug("NetUP: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      Log.Log.Debug("NetUP: state = {0}", info.CiState.ToString());

      bool camReady = false;
      if (info.CiState != NetUpCiState.Empty)
      {
        camReady = true;
      }
      Log.Log.Debug("NetUP: result = {0}", camReady);
      return camReady;
    }

    /// <summary>
    /// Send PMT to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="listAction">A list management action for communication with the CAM.</param>
    /// <param name="command">A decryption command directed to the CAM.</param>
    /// <param name="pmt">The PMT.</param>
    /// <param name="length">The length of the PMT in bytes.</param>
    /// <returns><c>true</c> if the service is successfully descrambled, otherwise <c>false</c></returns>
    public bool SendPmt(ListManagementType listAction, CommandIdType command, byte[] pmt, int length)
    {
      Log.Log.Debug("NetUP: send PMT to CAM, list action = {0}, command = {1}", listAction, command);
      if (!_isCamPresent)
      {
        Log.Log.Debug("NetUP: CAM not available");
        return true;
      }
      if (pmt == null || pmt.Length == 0)
      {
        Log.Log.Debug("NetUP: no PMT");
        return true;
      }
      if (listAction == ListManagementType.Add || listAction == ListManagementType.Update)
      {
        Log.Log.Debug("NetUP: list action not supported");
        return true;
      }
      if (command == CommandIdType.NotSelected)
      {
        Log.Log.Debug("NetUP: command not supported");
        return true;
      }

      // The NetUP driver accepts standard PMT and converts it to CA PMT internally.
      UInt32 code = (uint)((uint)NetUpIoControl.PmtListChange | ((byte)listAction << 8) | (uint)command);
      IntPtr buffer = Marshal.AllocCoTaskMem(length);
      Marshal.Copy(pmt, 0, buffer, length);
      DVB_MMI.DumpBinary(buffer, 0, length);
      NetUpCommand ncommand = new NetUpCommand(code, buffer, length, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = ncommand.Execute(_propertySet, out returnedByteCount);
      Marshal.FreeCoTaskMem(buffer);
      if (hr == 0)
      {
        Log.Log.Debug("NetUP: result = success");
        return true;
      }

      Log.Log.Debug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread that will handle interaction with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
      {
        _mmiHandlerThread.Abort();
        _mmiHandlerThread = null;
      }
      if (_mmiHandlerThread == null)
      {
        Log.Log.Debug("NetUP: starting new MMI handler thread");
        _stopMmiHandlerThread = false;
        _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
        _mmiHandlerThread.Name = "NetUP MMI handler";
        _mmiHandlerThread.IsBackground = true;
        _mmiHandlerThread.Priority = ThreadPriority.Lowest;
        _mmiHandlerThread.Start();
      }
    }

    /// <summary>
    /// Thread function for handling MMI responses from the CAM.
    /// </summary>
    private void MmiHandler()
    {
      Log.Log.Debug("NetUP: MMI handler thread start polling");
      NetUpCiState ciState = NetUpCiState.Empty;
      NetUpCiState prevCiState = NetUpCiState.Empty;
      try
      {
        while (!_stopMmiHandlerThread)
        {
          Thread.Sleep(500);

          CiStateInfo info;
          int hr = GetCiStatus(out info);
          if (hr != 0)
          {
            Log.Log.Debug("NetUP: failed to get CI status, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            continue;
          }

          // Handle CI slot state changes.
          ciState = info.CiState;
          if (ciState != prevCiState)
          {
            Log.Log.Debug("NetUP: CI state change");
            Log.Log.Debug("  old state    = {0}", prevCiState.ToString());
            Log.Log.Debug("  new state    = {0}", ciState.ToString());
            Log.Log.Debug("  manufacturer = 0x{0:x}", info.Manufacturer);
            Log.Log.Debug("  code         = 0x{0:x}", info.ManufacturerCode);
            Log.Log.Debug("  menu title   = {0}", info.RootMenuTitle);

            prevCiState = ciState;
            if (ciState == NetUpCiState.Empty)
            {
              _isCamPresent = false;
              _isCamReady = false;
            }
            else
            {
              _isCamPresent = true;
              _isCamReady = true;
            }
          }

          if ((ciState & NetUpCiState.MmiDataReady) != 0)
          {
            // MMI data is waiting to be retrieved, so let's get it.
            Log.Log.Debug("NetUP: get new MMI data");
            MmiData mmi;
            for (int i = 0; i < MmiDataSize; i++)
            {
              Marshal.WriteByte(_mmiBuffer, i, 0);
            }
            lock (this)
            {
              NetUpCommand command = new NetUpCommand((uint)NetUpIoControl.MmiGetMenu, IntPtr.Zero, 0, _mmiBuffer, MmiDataSize);
              int returnedByteCount;
              hr = command.Execute(_propertySet, out returnedByteCount);
              if (hr != 0 || returnedByteCount != MmiDataSize)
              {
                Log.Log.Debug("NetUP: failed to get MMI data, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                continue;
              }
              mmi = (MmiData)Marshal.PtrToStructure(_mmiBuffer, typeof(MmiData));
            }

            Log.Log.Debug("  is menu   = {0}", mmi.IsMenu);
            Log.Log.Debug("  title     = {0}", mmi.Title);
            Log.Log.Debug("  sub-title = {0}", mmi.SubTitle);
            Log.Log.Debug("  footer    = {0}", mmi.Footer);
            Log.Log.Debug("  # entries = {0}", mmi.EntryCount);
            for (int i = 0; i < mmi.EntryCount; i++)
            {
              Log.Log.Debug("  entry {0,-2}  = {1}", i + 1, mmi.Entries[i + 3].Text);
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Log.Debug("NetUP: error in MMI handler thread\r\n{0}", ex.ToString());
        return;
      }
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Sets the CAM callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        StartMmiHandlerThread();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Sends a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("NetUP: enter menu");
      int hr;
      lock (this)
      {
        NetUpCommand command = new NetUpCommand((uint)NetUpIoControl.MmiEnterMenu, IntPtr.Zero, 0, IntPtr.Zero, 0);
        int returnedByteCount;
        hr = command.Execute(_propertySet, out returnedByteCount);
      }
      if (hr == 0)
      {
        Log.Log.Debug("NetUP: result = success");
        return true;
      }

      Log.Log.Debug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Sends a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("NetUP: close menu");
      int hr;
      lock (this)
      {
        NetUpCommand command = new NetUpCommand((uint)NetUpIoControl.MmiClose, IntPtr.Zero, 0, IntPtr.Zero, 0);
        int returnedByteCount;
        hr = command.Execute(_propertySet, out returnedByteCount);
      }
      if (hr == 0)
      {
        Log.Log.Debug("NetUP: result = success");
        return true;
      }

      Log.Log.Debug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Sends a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("NetUP: select menu entry, choice = {0}", (int)choice);
      int hr;
      lock (this)
      {
        UInt32 code = (uint)((uint)NetUpIoControl.MmiAnswerMenu | choice << 8);
        NetUpCommand command = new NetUpCommand(code, IntPtr.Zero, 0, IntPtr.Zero, 0);
        int returnedByteCount;
        hr = command.Execute(_propertySet, out returnedByteCount);
      }
      if (hr == 0)
      {
        Log.Log.Debug("NetUP: result = success");
        return true;
      }

      Log.Log.Debug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Sends a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, string answer)
    {
      Log.Log.Debug("NetUP: sending a menu answer is not supported");
      return false;
    }

    #endregion

    #region IDiSEqCController members

    /// <summary>
    /// Send the appropriate DiSEqC 1.0 switch command to switch to a given channel.
    /// </summary>
    /// <param name="parameters">The scan parameters.</param>
    /// <param name="channel">The channel.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (channel.DisEqc == DisEqcType.SimpleA || channel.DisEqc == DisEqcType.SimpleB)
      {
        return false;
      }

      if (channel.DisEqc != DisEqcType.None)
      {
        bool isHighBand = BandTypeConverter.IsHiBand(channel, parameters);
        int antennaNr = BandTypeConverter.GetAntennaNr(channel);
        bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                              (channel.Polarisation == Polarisation.CircularL));
        byte command = 0xf0;
        command |= (byte)(isHighBand ? 1 : 0);
        command |= (byte)((isHorizontal) ? 2 : 0);
        command |= (byte)((antennaNr - 1) << 2);
        return SendDiSEqCCommand(new byte[4] { 0xe0, 0x10, 0x38, command });
      }

      return false;
    }

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="command">The DiSEqC command to send.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("NetUP: send DiSEqC command");

      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("NetUP: command too long, length = {0}", command.Length);
        return false;
      }

      for (int i = 0; i < command.Length; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, command[i]);
      }

      NetUpCommand ncommand = new NetUpCommand((uint)NetUpIoControl.Diseqc, _generalBuffer, command.Length, IntPtr.Zero, 0);
      int returnedByteCount;
      int hr = ncommand.Execute(_propertySet, out returnedByteCount);
      if (hr == 0)
      {
        Log.Log.Debug("NetUP: result = success");
        return true;
      }

      Log.Log.Debug("NetUP: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      Log.Log.Debug("NetUP: read DiSEqC command");
      // Not supported by the driver.
      reply = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close the conditional access interface and free unmanaged memory buffers.
    /// </summary>
    public void Dispose()
    {
      if (!_isNetUp)
      {
        return;
      }

      if (_mmiHandlerThread != null && _mmiHandlerThread.IsAlive)
      {
        _stopMmiHandlerThread = true;
        Thread.Sleep(1000);
      }
      Marshal.FreeCoTaskMem(_generalBuffer);
      Marshal.FreeCoTaskMem(_mmiBuffer);
      Release.ComObject(_propertySet);
    }

    #endregion
  }
}
