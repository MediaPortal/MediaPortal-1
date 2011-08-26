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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using DirectShowLib;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// DigitalDevices CI control class
  /// It derives from GenericBDAS for DiSEqC support.
  /// </summary>
  public partial class DigitalDevices : GenericBDAS, ICiMenuActions
  {
    #region constants

    private Guid PINNAME_BDA_TRANSPORT = new Guid("{78216a81-cfa8-493e-9711-36a61c08bd9d}");

    public static List<String> VendorNames = new List<String>() {"Digital Devices", "Mystique SaTiX-S2 Dual"};

    [ComImport, SuppressUnmanagedCodeSecurity,
     Guid("28F54685-06FD-11D2-B27A-00A0C9223196"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IKsControl
    {
      [PreserveSig]
      int KsProperty(
        [In] ref KSMETHOD Property,
        [In] Int32 PropertyLength,
        [In, Out] IntPtr PropertyData,
        [In] Int32 DataLength,
        [In, Out] ref Int32 BytesReturned
        );

      [PreserveSig]
      int KsMethod(
        [In] ref KSMETHOD Method,
        [In] Int32 MethodLength,
        [In, Out] IntPtr MethodData,
        [In] Int32 DataLength,
        [In, Out] ref Int32 BytesReturned
        );

      [PreserveSig]
      int KsEvent(
        [In, Optional] ref KSMETHOD Event,
        [In] Int32 EventLength,
        [In, Out] IntPtr EventData,
        [In] Int32 DataLength,
        [In, Out] ref Int32 BytesReturned
        );
    }

    public struct KSMETHOD
    {
      public Guid Set;
      public Int32 Id;
      public Int32 Flags;

      public KSMETHOD(Guid set, Int32 id, Int32 flags)
      {
        Set = set;
        Id = (Int32)id;
        Flags = (Int32)flags;
      }
    }

    private const Int32 KSMETHOD_TYPE_SEND = 1;
    private const Int32 KSPROPERTY_TYPE_GET = 1;
    private const Int32 KSPROPERTY_TYPE_SET = 2;

    private Guid KSPROPERTYSET_DD_COMMON_INTERFACE = new Guid("0aa8a501-a240-11de-b130-000000004d56");
    private Guid KSMETHODSET_DD_CAM_CONTROL = new Guid("0aa8a511-a240-11de-b130-000000004d56");

    private enum KSPROPERTY_DD_COMMON_INTERFACE
    {
      KSPROPERTY_DD_DECRYPT_PROGRAM = 0,
      KSPROPERTY_DD_CAM_MENU_TITLE = 1,
    }

    private enum KSMETHOD_DD_CAM_CONTROL
    {
      KSMETHOD_DD_CAM_RESET,
      KSMETHOD_DD_CAM_ENTER_MENU,
      KSMETHOD_DD_CAM_CLOSE_MENU,
      KSMETHOD_DD_CAM_GET_MENU,
      KSMETHOD_DD_CAM_MENU_REPLY,
      KSMETHOD_DD_CAM_ANSWER,
    }

    private struct DD_CAM_MENU_TITLE {}

    private struct DD_CAM_MENU_REPLY
    {
      public Int32 Id;
      public Int32 Choice;
    }

    /*struct DD_CAM_TEXT_DATA
    {
      public Int32 Id;
      public Int32 Length;
      [MarshalAs(UnmanagedType.LPStr)]
      public String Data;
    }*/

    private struct DD_CAM_MENU_DATA
    {
      public Int32 Id;
      public Int32 Type;
      public Int32 NumChoices;
      public Int32 Length;
      public String Title;
      public String SubTitle;
      public String BottomText;
      public List<String> Choices;
    }

    private enum E_STATE
    {
      IDLE,
      WAIT_FOR_OPEN,
      WAIT_FOR_MENU,
      MENU_OPEN,
      LIST_OPEN,
      ENQ_OPEN,
      CAM_ERROR = 99
    } ;

    #endregion

    #region variables

    private readonly bool _isDigitalDevices;
    // CI menu related handlers
    private bool StopThread;
    private ICiMenuCallbacks ciMenuCallbacks;
    private Thread CiMenuThread;
    private IBaseFilter CiFilter;
    private Int32 _MenuId;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DigitalDevices"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public DigitalDevices(IBaseFilter tunerFilter)
      : base(tunerFilter)
    {
      _CardName = String.Empty;

      FilterInfo fInfo;
      tunerFilter.QueryFilterInfo(out fInfo);

      // remarks: the better way of detection would be to check the DevicePath for matching parts
      // but I didn't find a way to access IMoniker interface for query DevicePath from its property bag from a IFilterGraph only.
      // see also TvCardDvdBase:
      // //DD components have a common device path part. 
      //   if (!(capDevices[capIndex].DevicePath.ToLowerInvariant().Contains("fbca-11de-b16f-000000004d56") 

      // check for all vendors names to support this hardware
      foreach (String vendor in VendorNames)
      {
        if (fInfo.achName.StartsWith(vendor))
        {
          _CardName = vendor;
          break;
        }
      }
      // nothing found?
      if (_CardName == String.Empty)
      {
        _isGenericBDAS = false; // if this is no DD card, don't try to handle generic BDAS here.
        return;
      }

      IEnumerable<IBaseFilter> nextFilters = FilterGraphTools.GetAllNextFilters(tunerFilter, PINNAME_BDA_TRANSPORT,
                                                                                PinDirection.Output);
      foreach (IBaseFilter nextFilter in nextFilters)
      {
        FilterInfo filterInfo;
        nextFilter.QueryFilterInfo(out filterInfo);

        if (filterInfo.achName.ToLowerInvariant().Contains("common interface") && nextFilter is IKsControl)
        {
          CiFilter = nextFilter;
          _isDigitalDevices = true;
          Log.Log.Debug(FormatMessage(" Common Interface found!"));
          break;
        }
      }
    }

    #endregion

    #region IHardwareProvider members

    /// <summary>
    /// Gets a value indicating whether this instance supports CAM.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance supports CAM; otherwise, <c>false</c>.
    /// </value>
    public bool IsSupported
    {
      get { return _isDigitalDevices; }
    }

    #endregion

    #region ICiMenuActions Member

    /// <summary>
    /// Sends the ServiceID to CAM to start decryption.
    /// </summary>
    /// <param name="serviceId">Service ID</param>
    /// <returns>true if successful or no CI available</returns>
    public bool SendServiceIdToCam(Int32 serviceId)
    {
      IKsControl pControl = CiFilter as IKsControl;
      if (pControl == null) return true;

      KSMETHOD KsProperty = new KSMETHOD(KSPROPERTYSET_DD_COMMON_INTERFACE,
                                         (Int32)KSPROPERTY_DD_COMMON_INTERFACE.KSPROPERTY_DD_DECRYPT_PROGRAM,
                                         KSPROPERTY_TYPE_SET);
      Int32 paramSize = Marshal.SizeOf(sizeof (Int32));
      Int32 dwReturned = 0;
      // Initialize unmanged memory to hold the struct.
      IntPtr pSid = Marshal.AllocHGlobal(paramSize);
      Marshal.WriteInt32(pSid, serviceId);
      try
      {
        Int32 hr = pControl.KsProperty(ref KsProperty, Marshal.SizeOf(KsProperty),
                                       pSid, paramSize,
                                       ref dwReturned);
        Log.Log.Debug(
          FormatMessage(String.Format("--> Setting service id {0} for decrypting returned {1}", serviceId, hr)));
        return (hr == 0);
      }
      finally
      {
        // Free the unmanaged memory.
        Marshal.FreeHGlobal(pSid);
      }
    }

    /// <summary>
    /// Sets the callback handler
    /// </summary>
    /// <param name="ciMenuHandler"></param>
    /// <returns></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        ciMenuCallbacks = ciMenuHandler;
        StartCiHandlerThread();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Enters the CI menu
    /// </summary>
    /// <returns></returns>
    public bool EnterCIMenu()
    {
      IKsControl pControl = CiFilter as IKsControl;
      if (pControl == null) return false;

      KSMETHOD KsMethod = new KSMETHOD(KSMETHODSET_DD_CAM_CONTROL,
                                       (Int32)KSMETHOD_DD_CAM_CONTROL.KSMETHOD_DD_CAM_ENTER_MENU,
                                       KSMETHOD_TYPE_SEND);

      Int32 dwReturned = 0;
      Int32 hr = pControl.KsMethod(ref KsMethod, Marshal.SizeOf(KsMethod),
                                   IntPtr.Zero, 0,
                                   ref dwReturned
        );
      if (hr == 0)
      {
        _MenuId = 0; // reset
      }
      return hr == 0;
    }

    /// <summary>
    /// Resets the CAM.
    /// </summary>
    /// <returns></returns>
    public bool ResetCAM()
    {
      IKsControl pControl = CiFilter as IKsControl;
      if (pControl == null) return false;

      KSMETHOD KsMethod = new KSMETHOD(KSMETHODSET_DD_CAM_CONTROL,
                                       (Int32)KSMETHOD_DD_CAM_CONTROL.KSMETHOD_DD_CAM_RESET,
                                       KSMETHOD_TYPE_SEND);

      Int32 dwReturned = 0;
      Int32 hr = pControl.KsMethod(ref KsMethod, Marshal.SizeOf(KsMethod),
                                   IntPtr.Zero, 0,
                                   ref dwReturned
        );
      return hr == 0;
    }

    /// <summary>
    /// Closes the CI menu
    /// </summary>
    /// <returns></returns>
    public bool CloseCIMenu()
    {
      IKsControl pControl = CiFilter as IKsControl;
      if (pControl == null) return false;

      KSMETHOD KsMethod = new KSMETHOD(KSMETHODSET_DD_CAM_CONTROL,
                                       (Int32)KSMETHOD_DD_CAM_CONTROL.KSMETHOD_DD_CAM_CLOSE_MENU,
                                       KSMETHOD_TYPE_SEND);

      Int32 dwReturned = 0;
      Int32 hr = pControl.KsMethod(ref KsMethod, Marshal.SizeOf(KsMethod),
                                   IntPtr.Zero, 0,
                                   ref dwReturned
        );
      return hr == 0;
    }

    /// <summary>
    /// Selects a CI menu entry
    /// </summary>
    /// <param name="choice">choice</param>
    /// <returns></returns>
    public bool SelectMenu(byte choice)
    {
      IKsControl pControl = CiFilter as IKsControl;
      if (pControl == null) return false;

      DD_CAM_MENU_REPLY Reply;

      Reply.Id = _MenuId;
      Reply.Choice = choice;

      // Initialize unmanged memory to hold the struct.
      IntPtr pReply = Marshal.AllocHGlobal(Marshal.SizeOf(Reply));
      try
      {
        // Copy the struct to unmanaged memory.
        Marshal.StructureToPtr(Reply, pReply, false);

        KSMETHOD KsMethod = new KSMETHOD(KSMETHODSET_DD_CAM_CONTROL,
                                         (Int32)KSMETHOD_DD_CAM_CONTROL.KSMETHOD_DD_CAM_MENU_REPLY,
                                         KSMETHOD_TYPE_SEND);
        Int32 dwReturned = 0;
        Int32 hr = pControl.KsMethod(ref KsMethod, Marshal.SizeOf(KsMethod),
                                     pReply, Marshal.SizeOf(Reply),
                                     ref dwReturned
          );
        return hr == 0;
      }
      finally
      {
        // Free the unmanaged memory.
        Marshal.FreeHGlobal(pReply);
      }
    }

    /// <summary>
    /// Sends an answer after CI request
    /// </summary>
    /// <param name="Cancel">true to cancel</param>
    /// <param name="Answer">Answer string</param>
    /// <returns></returns>
    public bool SendMenuAnswer(bool Cancel, string Answer)
    {
      IKsControl pControl = CiFilter as IKsControl;
      if (pControl == null) return false;

      // Initialize unmanged memory to hold the struct.
      Int32 answerLength = (String.IsNullOrEmpty(Answer) ? 0 : Answer.Length);
      Int32 bufferSize = 8 + answerLength + 1;

      if (bufferSize < 12) bufferSize = 12;

      IntPtr pReply = Marshal.AllocHGlobal(bufferSize);
      try
      {
        // Copy the struct to unmanaged memory.
        Marshal.WriteInt32(pReply, 0, _MenuId);
        if (answerLength > 0)
        {
          Marshal.WriteInt32(pReply, 4, (int)Answer.Length);
          for (int i = 0; i < Answer.Length; i += 1)
          {
            Marshal.WriteByte(pReply, 8 + i, (byte)Answer[i]);
          }
          Marshal.WriteByte(pReply, 8 + Answer.Length, 0);
        }
        else
        {
          Marshal.WriteInt32(pReply, 4, 0);
        }

        //DVB_MMI.DumpBinary(pReply, 0, bufferSize);

        KSMETHOD KsMethod = new KSMETHOD(KSMETHODSET_DD_CAM_CONTROL,
                                         (Int32)KSMETHOD_DD_CAM_CONTROL.KSMETHOD_DD_CAM_ANSWER,
                                         KSMETHOD_TYPE_SEND);
        Int32 dwReturned = 0;
        Int32 hr = pControl.KsMethod(ref KsMethod, Marshal.SizeOf(KsMethod),
                                     pReply, bufferSize,
                                     ref dwReturned
          );
        return hr == 0;
      }
      finally
      {
        // Free the unmanaged memory.
        Marshal.FreeHGlobal(pReply);
      }
    }

    #endregion

    #region MMI reading methods

    private bool GetMenuTitle(out DD_CAM_MENU_TITLE MenuTitle)
    {
      MenuTitle = new DD_CAM_MENU_TITLE();

      IKsControl pControl = CiFilter as IKsControl;
      if (pControl == null) return false;

      KSMETHOD KsProperty = new KSMETHOD(KSPROPERTYSET_DD_COMMON_INTERFACE,
                                         (Int32)KSPROPERTY_DD_COMMON_INTERFACE.KSPROPERTY_DD_CAM_MENU_TITLE,
                                         KSPROPERTY_TYPE_GET);
      Int32 ulMenuSize = Marshal.SizeOf(MenuTitle);
      Int32 dwReturned = 0;
      // Initialize unmanged memory to hold the struct.
      IntPtr pTitle = Marshal.AllocHGlobal(Marshal.SizeOf(MenuTitle));
      try
      {
        Int32 hr = pControl.KsProperty(ref KsProperty, Marshal.SizeOf(KsProperty),
                                       pTitle, ulMenuSize,
                                       ref dwReturned
          );

        if (hr != 0 || dwReturned != ulMenuSize)
          return false;

        MenuTitle = (DD_CAM_MENU_TITLE)Marshal.PtrToStructure(pTitle, typeof (DD_CAM_MENU_TITLE));
        return true;
      }
      finally
      {
        // Free the unmanaged memory.
        Marshal.FreeHGlobal(pTitle);
      }
    }

    private bool GetCamMenuText(out DD_CAM_MENU_DATA CiMenu)
    {
      CiMenu = new DD_CAM_MENU_DATA();
      IKsControl pControl = CiFilter as IKsControl;
      if (pControl == null) return false;

      // Initialize unmanged memory to hold the struct.
      Int32 ulMenuSize = 2048;
      IntPtr pCiMenu = Marshal.AllocHGlobal(ulMenuSize);
      try
      {
        KSMETHOD KsMethod = new KSMETHOD(KSMETHODSET_DD_CAM_CONTROL,
                                         (Int32)KSMETHOD_DD_CAM_CONTROL.KSMETHOD_DD_CAM_GET_MENU,
                                         KSMETHOD_TYPE_SEND);
        Int32 dwReturned = 0;
        Int32 hr = pControl.KsMethod(ref KsMethod, Marshal.SizeOf(KsMethod),
                                     pCiMenu, ulMenuSize,
                                     ref dwReturned
          );
        if (hr == 0)
        {
          Int32 offs = 0;
          CiMenu.Id = Marshal.ReadInt32(pCiMenu, offs);
          offs += 4;
          CiMenu.Type = Marshal.ReadInt32(pCiMenu, offs);
          offs += 4;
          CiMenu.NumChoices = Marshal.ReadInt32(pCiMenu, offs);
          offs += 4;
          CiMenu.Length = Marshal.ReadInt32(pCiMenu, offs);
          offs += 4;

          // check if we already received this menu, then ignore it.
          // otherwise we would do an endless processing.
          if (CiMenu.Id == _MenuId)
            return false;

          //DVB_MMI.DumpBinary(pCiMenu, 0, dwReturned);

          CiMenu.Choices = new List<String>();
          for (int i = 0; i < CiMenu.NumChoices + 3; i++)
          {
            IntPtr newPtr = new IntPtr(pCiMenu.ToInt32() + offs);
            String choice = Marshal.PtrToStringAnsi(newPtr);
            switch (i)
            {
              case 0:
                CiMenu.Title = choice;
                break;
              case 1:
                CiMenu.SubTitle = choice;
                break;
              case 2:
                CiMenu.BottomText = choice;
                break;
              default:
                CiMenu.Choices.Add(choice);
                break;
            }
            offs += choice.Length + 1;
          }
          // remember current received menu id
          _MenuId = CiMenu.Id;
        }
        //else
        //  Marshal.ThrowExceptionForHR(hr);

        return hr == 0;
      }
      finally
      {
        // Free the unmanaged memory.
        Marshal.FreeHGlobal(pCiMenu);
      }
    }

    /// <summary>
    /// Process the MMI and do callbacks.
    /// </summary>
    /// <param name="CiMenu"></param>
    /// <returns></returns>
    private bool ProcessCamMenu(DD_CAM_MENU_DATA CiMenu)
    {
      Log.Log.Debug("Menu received (ID {0} Type {1} Choices {2})", CiMenu.Id, CiMenu.Type, CiMenu.NumChoices);
      //Log.Log.Debug(" Menu Id      = {0}", CiMenu.Id);
      //Log.Log.Debug(" Menu Type    = {0}", CiMenu.Type);
      //Log.Log.Debug(" Menu Choices = {0}", CiMenu.NumChoices);
      //Log.Log.Debug(" Menu Length  = {0}", CiMenu.Length);

      if (ciMenuCallbacks == null)
        return false;

      switch (CiMenu.Type)
      {
        case 1:
        case 2:
          ciMenuCallbacks.OnCiMenu(CiMenu.Title, CiMenu.SubTitle, CiMenu.BottomText, CiMenu.NumChoices);
          int n = 0;
          foreach (String choice in CiMenu.Choices)
          {
            ciMenuCallbacks.OnCiMenuChoice(n++, choice);
          }
          break;
        case 3:
        case 4:
          ciMenuCallbacks.OnCiRequest(false, (uint)CiMenu.NumChoices, CiMenu.Title);
          break;
        default:
          Log.Log.Debug("Unknown MMI Type {0}", CiMenu.Type);
          break;
      }
      return true;
    }

    #endregion

    #region CiMenuHandlerThread start and stop

    /// <summary>
    /// Stops CiHandler thread
    /// </summary>
    private void StopCiHandlerThread()
    {
      if (CiMenuThread != null)
      {
        CiMenuThread.Abort();
        CiMenuThread = null;
      }
    }

    /// <summary>
    /// Starts CiHandler thread
    /// </summary>
    private void StartCiHandlerThread()
    {
      // Check if the polling thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (CiMenuThread != null && !CiMenuThread.IsAlive)
      {
        CiMenuThread.Abort();
        CiMenuThread = null;
      }
      if (CiMenuThread == null)
      {
        Log.Log.Debug(FormatMessage("Starting new CI handler thread"));
        StopThread = false;
        CiMenuThread = new Thread(new ThreadStart(CiMenuHandler));
        CiMenuThread.Name = String.Format("{0} CiMenuHandler", _CardName);
        CiMenuThread.IsBackground = true;
        CiMenuThread.Priority = ThreadPriority.Lowest;
        CiMenuThread.Start();
      }
    }

    #endregion

    #region CiMenuHandlerThread for polling status and handling MMI

    /// <summary>
    /// Thread that checks for CI menu 
    /// </summary>
    private void CiMenuHandler()
    {
      Log.Log.Debug(FormatMessage("CI handler thread start polling status"));
      try
      {
        while (!StopThread)
        {
          DD_CAM_MENU_DATA ciMenu;
          if (GetCamMenuText(out ciMenu))
          {
            ProcessCamMenu(ciMenu);
          }
          Thread.Sleep(500);
        }
        ;
      }
      catch (ThreadAbortException) {}
      catch (Exception ex)
      {
        Log.Log.Debug(FormatMessage("error in CiMenuHandler thread\r\n{0}"), ex.ToString());
        return;
      }
    }

    #endregion

    #region IDisposable members

    /// <summary>
    /// Disposes COM task memory resources
    /// </summary>
    public override void Dispose()
    {
      StopCiHandlerThread();
      base.Dispose();
    }

    #endregion
  }
}