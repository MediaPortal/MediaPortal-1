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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.AVerMedia
{
  /// <summary>
  /// A class for handling conditional access with the AVerMedia SatGate (A706C).
  /// </summary>
  public class AVerMedia : BaseTunerExtension, IConditionalAccessMenuActions, IConditionalAccessProvider, IDisposable
  {
    #region enums

    private enum AVerMediaCiState
    {
      Empty = 0,
      CamPresent,
      CamInitialising,
      CamReady
    }

    private enum AVerMediaMmiMessageType : ushort
    {
      Menu = 2,
      List = 3,
      Enquiry = 5,
      Close = 9
    }

    #endregion

    #region delegate definitions

    /// <summary>
    /// Invoked by the CI API COM object when the common interface slot state changes.
    /// </summary>
    /// <param name="context">Internal context information maintained by the COM object.</param>
    /// <param name="stateData">Information related to the state change. Format/content is state-dependent.</param>
    /// <returns>an HRESULT to indicate whether the state change was successfully handled</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int OnAVerMediaCiStateChange(IntPtr context, IntPtr stateData);

    /// <summary>
    /// Invoked by the CI COM object when an MMI message is available.
    /// </summary>
    /// <param name="context">Internal context information maintained by the COM object.</param>
    /// <param name="message">The MMI message.</param>
    /// <returns>an HRESULT to indicate whether the message was successfully handled</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int OnAVerMediaMmiMessage(IntPtr context, ref MmiData message);

    #endregion

    #region COM imports

    [Guid("7e57c354-7b47-4ec4-8a88-d50fb19cc688"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface CiInterface
    {
      /// <summary>
      /// Set the CI slot state change call back.
      /// </summary>
      /// <remarks>
      /// The call back delegate pointer must be a pointer to a pointer to a pointer to the delegate.
      /// </remarks>
      /// <param name="callBackPtr">An indirect reference to an OnAVerMediaCiStateChange delegate.</param>
      /// <returns>an HRESULT indicating whether the call back was registered successfully</returns>
      [PreserveSig]
      int SetStateChangeCallBack(IntPtr callBackPtr);

      /// <summary>
      /// Open the interface for a specific tuner.
      /// </summary>
      /// <remarks>
      /// Only one tuner can be active at any given time.
      /// </remarks>
      /// <param name="devicePath">A section of the device path.</param>
      /// <param name="filter1">Not required.</param>
      /// <param name="filter2">Not required.</param>
      /// <returns>an HRESULT indicating whether the interface was successfully opened</returns>
      [PreserveSig]
      int OpenInterface([MarshalAs(UnmanagedType.LPStr)] string devicePath, IBaseFilter filter1, IBaseFilter filter2);

      /// <summary>
      /// Close the interface.
      /// </summary>
      /// <returns>an HRESULT indicating whether the interface was successfully closed</returns>
      [PreserveSig]
      int CloseInterface();

      /// <summary>
      /// Send PMT or CA PMT to the CAM.
      /// </summary>
      /// <remarks>
      /// This function sends the PMT command twice so although you can send CA PMT, it is not
      /// necessarily going to work. In practice I was able to decrypt two channels simultaneously
      /// using this function. If you send standard PMT the list action will be "only".
      /// </remarks>
      /// <param name="pmt">The PMT or CA PMT.</param>
      /// <param name="isNotCaPmt"><c>True</c> if the PMT is not CA PMT.</param>
      /// <returns>an HRESULT indicating whether the PMT was successfully passed to the CAM</returns>
      [PreserveSig]
      int SendPmt(ref AVerMediaPmt pmt, [MarshalAs(UnmanagedType.Bool)] bool isNotCaPmt);
    }

    [Guid("c5bd61cc-f79a-4a5e-99d0-2e763ee372b6"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface MmiInterface
    {
      /// <summary>
      /// Set the MMI message handler delegate.
      /// </summary>
      /// <remarks>
      /// The call back delegate pointer must be a pointer to a pointer to a pointer to the
      /// delegate.
      /// </remarks>
      /// <param name="callBackPtr">An indirect reference to an OnAVerMediaMmiMessage delegate.</param>
      /// <returns>an HRESULT indicating whether the call back was registered successfully</returns>
      [PreserveSig]
      int SetMessageCallBack(IntPtr callBackPtr);

      /// <summary>
      /// Open an MMI session with the CAM and request access to the CAM menu.
      /// </summary>
      /// <param name="deviceIndex">The COM object does not support multiple devices. Must be set to zero.</param>
      /// <returns>an HRESULT indicating whether the request was sent successfully</returns>
      [PreserveSig]
      int OpenMenu(byte deviceIndex);

      /// <summary>
      /// Close the CAM menu.
      /// </summary>
      /// <param name="deviceIndex">The COM object does not support multiple devices. Must be set to zero.</param>
      /// <returns>an HRESULT indicating whether the session was successfully closed</returns>
      [PreserveSig]
      int CloseMenu(byte deviceIndex);

      /// <summary>
      /// Send an answer to an enquiry from the user to the CAM.
      /// </summary>
      /// <param name="deviceIndex">The COM object does not support multiple devices. Must be set to zero.</param>
      /// <param name="answer">The user's answer to the enquiry.</param>
      /// <param name="answerLength">The length of the answer.</param>
      /// <returns>an HRESULT indicating whether the answer was successfully passed to the CAM</returns>
      [PreserveSig]
      int SendEnquiryAnswer(byte deviceIndex, [MarshalAs(UnmanagedType.LPStr)] string answer, byte answerLength);

      /// <summary>
      /// Select an entry in the CAM menu.
      /// </summary>
      /// <param name="deviceIndex">The COM object does not support multiple devices. Must be set to zero.</param>
      /// <param name="entry">The index (0..n) of the menu entry selected by the user.</param>
      /// <returns>an HRESULT indicating whether the entry was selected successfully</returns>
      [PreserveSig]
      int SelectMenuEntry(byte deviceIndex, byte entry);
    }

    [Guid("94ba71a1-3325-4eac-9df8-685921bbfa2a"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface PmtInterface
    {
      /// <summary>
      /// Send PMT to the CAM. The interface converts the PMT to CA PMT using the specified list
      /// action.
      /// </summary>
      /// <param name="pmt">The PMT.</param>
      /// <param name="listAction">A CA PMT list management action.</param>
      /// <returns>an HRESULT indicating whether the PMT was successfully passed to the CAM</returns>
      [PreserveSig]
      int SendPmtWithListAction(ref AVerMediaPmt pmt, short listAction);

      /// <summary>
      /// Send PMT to the CAM. The interface converts the PMT to CA PMT using the "only" list action.
      /// </summary>
      /// <remarks>
      /// It is not desirable to use this function because it can't be used to decrypt multiple
      /// channels simultaneously. Note that the PMT structure seems to be different to the other
      /// two PMT functions.
      /// </remarks>
      /// <param name="pmt">The PMT.</param>
      /// <returns>an HRESULT indicating whether the PMT was successfully passed to the CAM</returns>
      [PreserveSig]
      int SendPmtOnly(byte[] pmt);
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct AVerMediaPmt
    {
      private byte Zero1;
      private short Zero2;
      public short Length;
      public IntPtr PmtPtr;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MmiMenuString
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public byte[] Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MmiData
    {
      public AVerMediaMmiMessageType MessageType;
      public ushort Unknown;
      public ushort IsBlindAnswer;
      public byte StringCount;
      public byte Count;      // either the number of entries in a menu/list, or the number of characters for an enquiry
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
      public MmiMenuString[] Strings;
    }

    #endregion

    #region constants

    private static readonly Guid CI_API_CLSID = new Guid(0x68ace120, 0xe5b1, 0x48bd, 0xb8, 0x9f, 0x88, 0xdd, 0x96, 0xc2, 0x83, 0x8b);

    // From version 1.1.0.35 of the COM object.
    private static readonly string[] VALID_DEVICE_PATHS = new string[]
    {
      "ven_1131&dev_7160&subsys_1d551461",
      "ven_1131&dev_7160&subsys_26551461",  // A707 AVerTV Satellite Trinity
      "ven_1131&dev_7231&subsys_110f1461",  // H968 [pictures suggest this doesn't have a CI slot connection]
      "ven_1131&dev_7133&subsys_f01d1461",  // M135 [pictures suggest this doesn't have a CI slot connection]
      "ven_1131&dev_7133&subsys_20551461",  // A706 AVerTV Satellite Hybrid + FM
      "ven_1131&dev_7133&subsys_21551461",  // A706 AVerTV Satellite PCI
      "ven_1131&dev_7160&subsys_2e551461",  // A707 Pure DVBT CI/CA
      "vid_07ca&pid_0888",                  // R888 AVer3D Satellite TV???
      "vid_07ca&pid_c873",                  // C873 AVer MediaCenter USB Remote Controller
      "vid_07ca&pid_0889",                  // R889 AVer3D Satellite TV
      "vid_07ca&pid_1889",                  // R889 AVer3D Satellite revision???
      "ven_1131&dev_7160&subsys_71711461"   // A717 AVer3D Quadro HD
    };

    #endregion

    #region variables

    private bool _isAVerMedia = false;
    private bool _isCaInterfaceOpen = false;

    private object _ciApi = null;
    private CiInterface _ciInterface = null;
    private MmiInterface _mmiInterface = null;
    private bool _isCamReady = false;
    private AVerMediaCiState _ciState = AVerMediaCiState.Empty;

    private string _tunerDevicePath = string.Empty;

    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private object _caMenuCallBackLock = new object();

    // The interface requires the call back delegate pointers to be passed
    // with two levels of indirection. This is possibly because they have
    // classes/interfaces to encapsulate the call backs. Annoying but necessary.
    private OnAVerMediaCiStateChange _ciStateChangeDelegate = null;
    private IntPtr _ciStateChangeCallBackPtr = IntPtr.Zero;
    private IntPtr _ciStateChangeIndirectionPtr = IntPtr.Zero;
    private IntPtr _ciStateChangeInterfacePtr = IntPtr.Zero;

    private OnAVerMediaMmiMessage _mmiMessageDelegate = null;
    private IntPtr _mmiMessageCallBackPtr = IntPtr.Zero;
    private IntPtr _mmiMessageIndirectionPtr = IntPtr.Zero;
    private IntPtr _mmiMessageInterfacePtr = IntPtr.Zero;

    #endregion

    #region delegate implementations

    /// <summary>
    /// Invoked by the CI API COM object when the common interface slot state changes.
    /// </summary>
    /// <param name="context">Internal context information maintained by the COM object.</param>
    /// <param name="stateData">Information related to the state change.</param>
    /// <returns>an HRESULT to indicate whether the state change was successfully handled</returns>
    private int OnCiStateChange(IntPtr context, IntPtr stateData)
    {
      if (stateData == IntPtr.Zero)
      {
        this.LogInfo("AVerMedia: CI state change call back, no state data provided");
      }
      else
      {
        AVerMediaCiState newState = (AVerMediaCiState)Marshal.ReadByte(stateData, 0);
        this.LogInfo("AVerMedia: CI state change call back, old state = {0}, new state = {1}", _ciState, newState);
        _ciState = newState;
        _isCamReady = (_ciState == AVerMediaCiState.CamReady);

        // The other data is state dependent.
        if (_ciState == AVerMediaCiState.CamInitialising)
        {
          this.LogDebug("  menu title = {0}", DvbTextConverter.Convert(IntPtr.Add(stateData, 2)));
        }
        else if (_ciState == AVerMediaCiState.CamReady)
        {
          int casCount = Marshal.ReadInt16(stateData, 2);
          this.LogDebug("  # CAS IDs = {0}", casCount);
          for (int i = 0; i < casCount; i++)
          {
            this.LogDebug("    {0, -7} = 0x{1:x4}", i + 1, Marshal.ReadInt16(stateData, 4 + (i * 2)));
          }
        }
      }
      return (int)NativeMethods.HResult.S_OK;
    }

    /// <summary>
    /// Invoked by the CI COM object when an MMI message is available.
    /// </summary>
    /// <param name="context">Internal context information maintained by the COM object.</param>
    /// <param name="message">The MMI message.</param>
    /// <returns>an HRESULT to indicate whether the message was successfully handled</returns>
    private int OnMmiMessage(IntPtr context, ref MmiData message)
    {
      this.LogInfo("AVerMedia: MMI message call back");

      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack == null)
        {
          this.LogDebug("AVerMedia: menu call back not set");
        }

        this.LogDebug("  message type = {0}", message.MessageType);
        if (message.MessageType == AVerMediaMmiMessageType.Close)
        {
          // The field structure or meaning appears to be different in this
          // case. Example: 09 00 00 03 78 2B C2 09. All bytes except byte six
          // (0xC2) appear to be constant.
          if (_caMenuCallBack != null)
          {
            _caMenuCallBack.OnCiCloseDisplay(0);
          }
          return (int)NativeMethods.HResult.S_OK;
        }

        this.LogDebug("  unknown      = {0}", message.Unknown);
        this.LogDebug("  is blind     = {0}", message.IsBlindAnswer);
        this.LogDebug("  string count = {0}", message.StringCount);
        this.LogDebug("  count        = {0}", message.Count);

        if (message.StringCount >= 3)
        {
          string title = DvbTextConverter.Convert(message.Strings[0].Text);
          string subTitle = DvbTextConverter.Convert(message.Strings[1].Text);
          string footer = DvbTextConverter.Convert(message.Strings[2].Text);
          this.LogDebug("  title        = {0}", title);
          this.LogDebug("  sub-title    = {0}", subTitle);
          this.LogDebug("  footer       = {0}", footer);
          this.LogDebug("  # entries    = {0}", message.Count);
          if (_caMenuCallBack != null)
          {
            _caMenuCallBack.OnCiMenu(title, subTitle, footer, message.Count);
          }
          if (message.Count > 0)
          {
            for (int i = 0; i < message.Count; i++)
            {
              string entry = DvbTextConverter.Convert(message.Strings[i + 3].Text);
              this.LogDebug("    {0, -10} = {1}", i + 1, entry);
              if (_caMenuCallBack != null)
              {
                _caMenuCallBack.OnCiMenuChoice(i, entry);
              }
            }
          }
        }
        else if (message.MessageType == AVerMediaMmiMessageType.Enquiry)
        {
          string prompt = DvbTextConverter.Convert(message.Strings[0].Text);
          this.LogDebug("  prompt       = {0}", prompt);
          this.LogDebug("  length       = {0}", message.Count);
          this.LogDebug("  blind        = {0}", message.IsBlindAnswer != 0);
          if (_caMenuCallBack != null)
          {
            _caMenuCallBack.OnCiRequest(message.IsBlindAnswer != 0, message.Count, prompt);
          }
        }
        else
        {
          this.LogWarn("AVerMedia: MMI message type {0} not supported", message.MessageType);
        }
      }

      return (int)NativeMethods.HResult.S_OK;
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 50;
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
      this.LogDebug("AVerMedia: initialising");

      if (_isAVerMedia)
      {
        this.LogDebug("AVerMedia: extension already initialised");
        return true;
      }

      if (string.IsNullOrEmpty(tunerExternalId))
      {
        this.LogDebug("AVerMedia: tuner external identifier is not set");
        return false;
      }

      foreach (string validDevicePath in VALID_DEVICE_PATHS)
      {
        if (tunerExternalId.ToLowerInvariant().Contains(validDevicePath))
        {
          this.LogInfo("AVerMedia: extension supported");
          _isAVerMedia = true;
          _tunerDevicePath = tunerExternalId;
          return true;
        }
      }

      this.LogDebug("AVerMedia: tuner not supported");
      return false;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Open()
    {
      this.LogDebug("AVerMedia: open conditional access interface");

      if (!_isAVerMedia)
      {
        this.LogWarn("AVerMedia: not initialised or interface not supported");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("AVerMedia: conditional access interface is already open");
        return true;
      }

      _ciApi = null;
      try
      {
        _ciApi = Activator.CreateInstance(Type.GetTypeFromCLSID(CI_API_CLSID));
        if (_ciApi == null)
        {
          throw new NullReferenceException();
        }
      }
      catch
      {
        this.LogError("AVerMedia: failed to create CI API instance, is the AVerMedia software installed?");
        return false;
      }

      _ciInterface = _ciApi as CiInterface;
      if (_ciInterface == null)
      {
        this.LogError("AVerMedia: failed to obtain CI control interface");
        return false;
      }
      _mmiInterface = _ciApi as MmiInterface;
      if (_mmiInterface == null)
      {
        this.LogError("AVerMedia: failed to obtain MMI interface");
        return false;
      }

      _ciState = AVerMediaCiState.Empty;
      _isCamReady = false;

      // Unfortunately this pointer manipulation really is necessary.
      _ciStateChangeDelegate = OnCiStateChange;
      _ciStateChangeCallBackPtr = Marshal.GetFunctionPointerForDelegate(_ciStateChangeDelegate);
      _ciStateChangeIndirectionPtr = Marshal.AllocCoTaskMem(IntPtr.Size);
      Marshal.WriteIntPtr(_ciStateChangeIndirectionPtr, 0, _ciStateChangeCallBackPtr);
      _ciStateChangeInterfacePtr = Marshal.AllocCoTaskMem(IntPtr.Size);
      Marshal.WriteIntPtr(_ciStateChangeInterfacePtr, 0, _ciStateChangeIndirectionPtr);

      _mmiMessageDelegate = OnMmiMessage;
      _mmiMessageCallBackPtr = Marshal.GetFunctionPointerForDelegate(_mmiMessageDelegate);
      _mmiMessageIndirectionPtr = Marshal.AllocCoTaskMem(IntPtr.Size);
      Marshal.WriteIntPtr(_mmiMessageIndirectionPtr, 0, _mmiMessageCallBackPtr);
      _mmiMessageInterfacePtr = Marshal.AllocCoTaskMem(IntPtr.Size);
      Marshal.WriteIntPtr(_mmiMessageInterfacePtr, 0, _mmiMessageIndirectionPtr);

      // Trim the device path to make it like "pci#ven_1131&dev_7160&subsys_26551461&rev_03#4&2165452d&0&0008#".
      // This is important. OpenInterface() won't succeed unless you do this.
      Match m = Regex.Match(_tunerDevicePath, @"\?\\([^{]+)\{");
      string devicePathSection = _tunerDevicePath;
      if (m.Success)
      {
        devicePathSection = m.Groups[1].Value;
      }

      int hr = _ciInterface.SetStateChangeCallBack(_ciStateChangeInterfacePtr);
      hr |= _mmiInterface.SetMessageCallBack(_mmiMessageInterfacePtr);
      hr |= _ciInterface.OpenInterface(devicePathSection, null, null);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("AVerMedia: result = success");
        _isCaInterfaceOpen = true;
        return true;
      }

      this.LogError("AVerMedia: failed to open conditional access interface, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Determine if the conditional access interface is open.
    /// </summary>
    /// <value><c>true</c> if the conditional access interface is open, otherwise <c>false</c></value>
    bool IConditionalAccessProvider.IsOpen
    {
      get
      {
        return _isCaInterfaceOpen;
      }
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Close()
    {
      return CloseConditionalAccessInterface(true);
    }

    private bool CloseConditionalAccessInterface(bool isDisposing)
    {
      this.LogDebug("AVerMedia: close conditional access interface");

      if (isDisposing)
      {
        if (_ciInterface != null)
        {
          _ciInterface.SetStateChangeCallBack(IntPtr.Zero);
          _ciInterface.CloseInterface();
          _ciInterface = null;
        }
        else if (_isCaInterfaceOpen)
        {
          this.LogWarn("AVerMedia: conditional access interface is open but CI interface is null");
        }

        if (_mmiInterface != null)
        {
          _mmiInterface.SetMessageCallBack(IntPtr.Zero);
          _mmiInterface = null;
        }
        else if (_isCaInterfaceOpen)
        {
          this.LogWarn("AVerMedia: conditional access interface is open but MMI interface is null");
        }

        Release.ComObject("AVerMedia CI API", ref _ciApi);
        _mmiInterface = null;

        _ciStateChangeDelegate = null;
        _mmiMessageDelegate = null;
      }

      _ciStateChangeCallBackPtr = IntPtr.Zero;
      if (_ciStateChangeIndirectionPtr != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_ciStateChangeIndirectionPtr);
        _ciStateChangeIndirectionPtr = IntPtr.Zero;
      }
      if (_ciStateChangeInterfacePtr != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_ciStateChangeInterfacePtr);
        _ciStateChangeInterfacePtr = IntPtr.Zero;
      }

      _mmiMessageCallBackPtr = IntPtr.Zero;
      if (_mmiMessageIndirectionPtr != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiMessageIndirectionPtr);
        _mmiMessageIndirectionPtr = IntPtr.Zero;
      }
      if (_mmiMessageInterfacePtr != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiMessageInterfacePtr);
        _mmiMessageInterfacePtr = IntPtr.Zero;
      }

      _ciState = AVerMediaCiState.Empty;
      _isCamReady = false;
      _isCaInterfaceOpen = false;

      this.LogDebug("AVerMedia: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      return (this as IConditionalAccessProvider).Close() && (this as IConditionalAccessProvider).Open();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
    {
      this.LogDebug("AVerMedia: is conditional access interface ready");
      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("AVerMedia: not initialised or interface not supported");
        return false;
      }

      // The CI state is updated via call back.
      this.LogDebug("AVerMedia: result = {0}", _isCamReady);
      return _isCamReady;
    }

    /// <summary>
    /// Determine whether the conditional access interface requires access to
    /// the MPEG 2 conditional access table in order to successfully decrypt
    /// programs.
    /// </summary>
    /// <value><c>true</c> if access to the MPEG 2 conditional access table is required in order to successfully decrypt programs, otherwise <c>false</c></value>
    bool IConditionalAccessProvider.IsConditionalAccessTableRequiredForDecryption
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more programs
    ///   simultaneously. This parameter gives the interface an indication of the number of programs that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program's map table.</param>
    /// <param name="cat">The conditional access table for the program's transport stream.</param>
    /// <param name="programProvider">The program's provider.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.SendCommand(CaPmtListManagementAction listAction, CaPmtCommand command, TableProgramMap pmt, TableConditionalAccess cat, string programProvider)
    {
      this.LogDebug("AVerMedia: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("AVerMedia: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("AVerMedia: failed to send conditional access command, the CAM is not ready");
        return false;
      }
      if (pmt == null)
      {
        this.LogError("AVerMedia: failed to send conditional access command, PMT not supplied");
        return true;
      }

      int hr = (int)NativeMethods.HResult.S_OK;
      AVerMediaPmt averPmt = new AVerMediaPmt();
      try
      {
        PmtInterface pmtInterface = _ciApi as PmtInterface;
        if (pmtInterface == null)
        {
          // Use the older CI interface PMT function when the new PMT interface is not available.
          byte[] caPmt = DvbMmiHandler.CreateCaPmtRequest(pmt.GetCaPmt(listAction, command));
          averPmt.Length = (short)caPmt.Length;
          averPmt.PmtPtr = Marshal.AllocCoTaskMem(caPmt.Length);
          Marshal.Copy(caPmt, 0, averPmt.PmtPtr, caPmt.Length);
          hr = _ciInterface.SendPmt(ref averPmt, false);
        }
        else
        {
          ReadOnlyCollection<byte> rawPmt = pmt.GetRawPmt();
          averPmt.Length = (short)rawPmt.Count;
          averPmt.PmtPtr = Marshal.AllocCoTaskMem(rawPmt.Count);
          for (int i = 0; i < rawPmt.Count; i++)
          {
            Marshal.WriteByte(averPmt.PmtPtr, i, rawPmt[i]);
          }
          hr = pmtInterface.SendPmtWithListAction(ref averPmt, (short)listAction);
        }
      }
      finally
      {
        if (averPmt.PmtPtr != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(averPmt.PmtPtr);
        }
      }

      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("AVerMedia: result = success");
        return true;
      }
      this.LogError("AVerMedia: failed to send conditional access command, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    void IConditionalAccessMenuActions.SetCallBack(IConditionalAccessMenuCallBack callBack)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBack = callBack;
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Enter()
    {
      this.LogDebug("AVerMedia: enter menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("AVerMedia: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("AVerMedia: failed to enter menu, the CAM is not ready");
        return false;
      }

      int hr = _mmiInterface.OpenMenu(0);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("AVerMedia: result = success");
        return true;
      }

      this.LogError("AVerMedia: failed to enter menu, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("AVerMedia: close menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("AVerMedia: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("AVerMedia: failed to close menu, the CAM is not ready");
        return false;
      }

      int hr = _mmiInterface.CloseMenu(0);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("AVerMedia: result = success");
        return true;
      }

      this.LogError("AVerMedia: failed to close menu, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("AVerMedia: select menu entry, choice = {0}", choice);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("AVerMedia: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("AVerMedia: failed to select menu entry, the CAM is not ready");
        return false;
      }

      int hr = _mmiInterface.SelectMenuEntry(0, choice);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("AVerMedia: result = success");
        return true;
      }

      this.LogError("AVerMedia: failed to select menu entry, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.AnswerEnquiry(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = string.Empty;
      }
      this.LogDebug("AVerMedia: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("AVerMedia: not initialised or interface not supported");
        return false;
      }
      if (!_isCamReady)
      {
        this.LogError("AVerMedia: failed to answer enquiry, the CAM is not ready");
        return false;
      }

      if (cancel)
      {
        return (this as IConditionalAccessMenuActions).SelectEntry(0); // 0 means "go back to the previous menu level"
      }
      if (answer.Length > 255)
      {
        this.LogError("AVerMedia: answer too long, length = {0}", answer.Length);
        return false;
      }

      int hr = _mmiInterface.SendEnquiryAnswer(0, answer, (byte)answer.Length);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("AVerMedia: result = success");
        return true;
      }

      this.LogError("AVerMedia: failed to answer enquiry, hr = 0x{0:x}", hr);
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

    ~AVerMedia()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isAVerMedia)
      {
        CloseConditionalAccessInterface(isDisposing);
      }
      _isAVerMedia = false;
    }

    #endregion
  }
}