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
using TvLibrary.Implementations.DVB.Structures;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access for Digital Devices tuners.
  /// </summary>
  public class DigitalDevices : ICiMenuActions, IDisposable
  {
    #region enums

    private enum CommonInterfaceProperty
    {
      DecryptProgram = 0,
      CamMenuTitle,
    }

    private enum CamControlProperty
    {
      Reset = 0,
      EnterMenu,
      CloseMenu,
      GetMenu,
      MenuReply,    // Select a menu entry.
      CamAnswer,    // Send an answer to a CAM enquiry.
    }

    private enum DecryptChainingRestriction : uint
    {
      None = 0,
      NoForwardChaining = 0x80000000,
      NoBackwardChaining = 0x40000000
    }

    #endregion

    #region structs

    private struct MenuData   // DD_CAM_MENU_DATA
    {
      public Int32 Id;
      public Int32 Type;
      public Int32 EntryCount;
      public Int32 Length;
      // The following strings are passed back as an inline array of
      // variable length NULL terminated strings. This makes it
      // impossible to unmarshal the struct automatically.
      public String Title;
      public String SubTitle;
      public String Footer;
      public List<String> Entries;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct MenuChoice   // DD_CAM_MENU_REPLY
    {
      public Int32 Id;
      public Int32 Choice;
    }

    private struct MenuAnswer   // DD_CAM_TEXT_DATA
    {
#pragma warning disable 0649
      public Int32 Id;
      public Int32 Length;
      // The following string is passed back as an inline variable
      // length NULL terminated string. This makes it impossible to
      // unmarshal the struct automatically.
      public String Answer;
#pragma warning restore 0649
    }

    private struct MenuTitle    // DD_CAM_MENU_TITLE
    {
      // The following string is passed back as an inline variable
      // length NULL terminated string. This makes it impossible to
      // unmarshal the struct automatically.
#pragma warning disable 0649
      public String Title;
#pragma warning restore 0649
    }

    private class CiContext
    {
      public IKsPropertySet PropertySet;
      public DsDevice Device;
      public String FilterName;
      public String CamMenuTitle;
      public Int32 CamMenuId;

      public CiContext(IBaseFilter filter, DsDevice device)
      {
        PropertySet = filter as IKsPropertySet;
        Device = device;
        FilterName = FilterGraphTools.GetFilterName(filter);
        CamMenuTitle = FilterName;
        CamMenuId = 0;
      }
    }

    #endregion

    #region constants

    private static readonly string[] ValidTunerNamePrefixes = new string[]
    {
      "Digital Devices",
      "Mystique SaTiX-S2 Dual"
    };

    private static readonly Guid CommonInterfacePropertySet = new Guid(0x0aa8a501, 0xa240, 0x11de, 0xb1, 0x30, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x56);
    private static readonly Guid CamControlPropertySet = new Guid(0x0aa8a511, 0xa240, 0x11de, 0xb1, 0x30, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x56);

    private const String CommonDevicePathSection = "fbca-11de-b16f-000000004d56";
    private const int MenuDataSize = 2048;  // This is arbitrary - an estimate of the buffer size needed to hold the largest menu.
    private const int MenuChoiceSize = 8;

    #endregion

    #region variables

    private bool _isDigitalDevices = false;
    private bool _isCiSlotPresent = false;

    private List<CiContext> _ciContexts = null;
    private IFilterGraph2 _graph = null;
    private int _menuContext = -1;

    private bool _camMessagesDisabled = false;
    private DateTime _camMessageEnableTs = DateTime.MinValue;

    private IntPtr _mmiBuffer = IntPtr.Zero;

    private ICiMenuCallbacks _ciMenuCallbacks = null;
    private bool _stopMmiHandlerThread = false;
    private Thread _mmiHandlerThread = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="DigitalDevices"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="tunerDevicePath">The tuner device path.</param>
    public DigitalDevices(IBaseFilter tunerFilter, String tunerDevicePath)
    {
      // Digital Devices components have a common section in their device path.
      if (tunerDevicePath == null || !tunerDevicePath.ToLowerInvariant().Contains(CommonDevicePathSection))
      {
        return;
      }

      Log.Log.Debug("Digital Devices: supported tuner detected");
      _isDigitalDevices = true;
      _ciContexts = new List<CiContext>();
      _mmiBuffer = Marshal.AllocCoTaskMem(MenuDataSize);
    }

    /// <summary>
    /// Insert and connect the add-on device into the graph.
    /// </summary>
    /// <param name="graphBuilder">The graph builder to use to insert the device.</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture filter) to connect the device to.</param>
    /// <returns><c>true</c> if the device was successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(ref ICaptureGraphBuilder2 graphBuilder, ref IBaseFilter lastFilter)
    {
      Log.Log.Debug("Digital Devices: add filter to graph");
      if (graphBuilder == null)
      {
        Log.Log.Debug("Digital Devices: graph builder is null");
        return false;
      }
      if (lastFilter == null)
      {
        Log.Log.Debug("Digital Devices: upstream filter is null");
        return false;
      }

      // We need a reference to the graph builder's graph.
      IGraphBuilder tmpGraph = null;
      int hr = graphBuilder.GetFiltergraph(out tmpGraph);
      if (hr != 0)
      {
        Log.Log.Debug("Digital Devices: couldn't get graph reference, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      _graph = tmpGraph as IFilterGraph2;
      if (_graph == null)
      {
        Log.Log.Debug("Digital Devices: couldn't get graph reference");
        return false;
      }

      // We need a demux filter to test whether we can add any further CI filters
      // to the graph.
      IBaseFilter tmpDemux = (IBaseFilter)new MPEG2Demultiplexer();
      hr = _graph.AddFilter(tmpDemux, "Temp MPEG2-Demux");
      if (hr != 0)
      {
        Log.Log.Debug("Digital Devices: failed to add test demux to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        Release.ComObject(tmpDemux);
        return false;
      }
      IPin demuxInputPin = DsFindPin.ByDirection(tmpDemux, PinDirection.Input, 0);
      if (demuxInputPin == null)
      {
        Log.Log.Debug("Digital Devices: couldn't find the demux input pin");
        _graph.RemoveFilter(tmpDemux);
        Release.ComObject(tmpDemux);
        return false;
      }

      // We start our work with the last filter in the graph, which we expect to be
      // either a tuner or capture filter. We need the output pin...
      IPin lastFilterOutputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
      if (lastFilterOutputPin == null)
      {
        Log.Log.Debug("Digital Devices: upstream filter doesn't have an output pin");
        Release.ComObject(demuxInputPin);
        _graph.RemoveFilter(tmpDemux);
        Release.ComObject(tmpDemux);
        return false;
      }
      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);

      while (true)
      {
        // Stage 1: if connection to a demux is possible then no [further] CI slots are configured
        // for this tuner. This test removes a 30 to 45 second delay when the graphbuilder tries
        // to render [capture]->[CI]->[demux].
        if (_graph.Connect(lastFilterOutputPin, demuxInputPin) == 0)
        {
          Log.Log.WriteFile("Digital Devices: no [more] CI filters available or configured for this tuner");
          lastFilterOutputPin.Disconnect();
          break;
        }

        // Stage 2: see if there are any more CI filters that we can add to the graph. We re-loop
        // over all capture devices because the CI filters have to be connected in a specific order
        // which is not guaranteed to be the same as the capture device array order.
        bool addedFilter = false;
        for (int i = 0; i < captureDevices.Length; i++)
        {
          // We're looking for a Digital Devices common interface device that is not
          // already in the graph.
          if (!captureDevices[i].DevicePath.ToLowerInvariant().Contains(CommonDevicePathSection) ||
            !captureDevices[i].Name.ToLowerInvariant().Contains("common interface") ||
            DevicesInUse.Instance.IsUsed(captureDevices[i]))
          {
            continue;
          }

          // Okay, we've got a device. Let's try and connect it into the graph.
          Log.Log.Debug("Digital Devices: adding filter for device \"{0}\"", captureDevices[i].Name);
          IBaseFilter tmpCiFilter = null;
          hr = _graph.AddSourceFilterForMoniker(captureDevices[i].Mon, null, captureDevices[i].Name, out tmpCiFilter);
          if (hr != 0 || tmpCiFilter == null)
          {
            Log.Log.Debug("Digital Devices: failed to add filter to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            continue;
          }

          // Now we've got a filter in the graph. Let's see if it will connect.
          hr = graphBuilder.RenderStream(null, null, lastFilter, null, tmpCiFilter);
          if (hr != 0)
          {
            Log.Log.Debug("Digital Devices: failed to render stream through filter, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            _graph.RemoveFilter(tmpCiFilter);
            Release.ComObject(tmpCiFilter);
            continue;
          }

          // Ensure that the filter has an output pin.
          Release.ComObject(lastFilterOutputPin);
          lastFilterOutputPin = DsFindPin.ByDirection(tmpCiFilter, PinDirection.Output, 0);
          if (lastFilterOutputPin == null)
          {
            Log.Log.Debug("Digital Devices: filter doesn't have an output pin");
            _graph.RemoveFilter(tmpCiFilter);
            Release.ComObject(tmpCiFilter);
            continue;
          }

          // Excellent - CI filter successfully added!
          _ciContexts.Add(new CiContext(tmpCiFilter, captureDevices[i]));
          Log.Log.Debug("Digital Devices: total of {0} CI filter(s) in the graph", _ciContexts.Count);
          DevicesInUse.Instance.Add(captureDevices[i]);
          lastFilter = tmpCiFilter;
          addedFilter = true;
          _isCiSlotPresent = true;

          // Fill in the menu title if possible.
          int index = _ciContexts.Count - 1;
          String menuTitle;
          if (GetMenuTitle(index, out menuTitle))
          {
            _ciContexts[index].CamMenuTitle = menuTitle;
          }
        }

        if (!addedFilter)
        {
          break;
        }
      }

      Release.ComObject(lastFilterOutputPin);
      Release.ComObject(demuxInputPin);
      _graph.RemoveFilter(tmpDemux);
      Release.ComObject(tmpDemux);
      return _isCiSlotPresent;
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Digital Devices compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Digital Devices compatible tuner, otherwise <c>false</c></value>
    public bool IsDigitalDevices
    {
      get
      {
        return _isDigitalDevices;
      }
    }

    #region conditional access

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is reset successfully, otherwise <c>false</c></returns>
    public bool ResetCi()
    {
      Log.Log.Debug("Digital Devices: reset CI");

      // Reset the slot selection for menu browsing.
      _menuContext = -1;

      if (!_isCiSlotPresent)
      {
        Log.Log.Debug("Digital Devices: no interface to reset");
        return false;
      }
      int returnedByteCount;
      bool success = true;

      // We reset all the CI filters in the graph.
      for (int i = 0; i < _ciContexts.Count; i++)
      {
        Log.Log.Debug("Digital Devices: reset slot {0} \"{1}\"", i + 1, _ciContexts[i].FilterName);
        int hr = _ciContexts[i].PropertySet.Get(CamControlPropertySet, (int)CamControlProperty.Reset,
          IntPtr.Zero, 0,
          IntPtr.Zero, 0,
          out returnedByteCount
        );
        if (hr == 0)
        {
          Log.Log.WriteFile("Digital Devices: result = success");
          _ciContexts[i].CamMenuId = 0;
        }
        else
        {
          Log.Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }
      return success;
    }

    /// <summary>
    /// Read the CAM menu title from the CAM in a specific CI slot.
    /// </summary>
    /// <param name="slot">The index of the CI context structure for the slot containing the CAM.</param>
    /// <param name="title">The CAM menu title.</param>
    /// <returns><c>true</c> if the CAM title is read successfully, otherwise <c>false</c></returns>
    private bool GetMenuTitle(int slot, out String title)
    {
      Log.Log.WriteFile("Digital Devices: slot {0} read CAM title", slot);
      title = String.Empty;

      for (int i = 0; i < MenuDataSize; i++)
      {
        Marshal.WriteByte(_mmiBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _ciContexts[slot].PropertySet.Get(CommonInterfacePropertySet, (int)CommonInterfaceProperty.CamMenuTitle,
        _mmiBuffer, MenuDataSize,
        _mmiBuffer, MenuDataSize,
        out returnedByteCount
      );
      if (hr == 0)
      {
        title = Marshal.PtrToStringAnsi(_mmiBuffer, returnedByteCount).TrimEnd();
        Log.Log.WriteFile("  title = {0}", title);
        Log.Log.WriteFile("Digital Devices: result = success");
        return true;
      }

      Log.Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send PMT to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="listAction">A list management action for communication with the CAM.</param>
    /// <param name="command">A decryption command directed to the CAM.</param>
    /// <param name="pmt">The PMT.</param>
    /// <param name="length">The length of the PMT in bytes.</param>
    /// <returns><c>true</c> if the request is sent successfully, otherwise <c>false</c></returns>
    public bool SendPmt(ListManagementType listAction, CommandIdType command, byte[] pmt, int length)
    {
      Log.Log.Debug("Digital Devices: send PMT to CAM, list action = {0}, command = {1}", listAction, command);
      if (!_isCiSlotPresent)
      {
        Log.Log.Debug("Digital Devices: CAM not available");
        return true;    // Don't retry.
      }
      if (command == CommandIdType.MMI || command == CommandIdType.Query)
      {
        Log.Log.Debug("Digital Devices: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null || pmt.Length == 0)
      {
        Log.Log.Debug("Digital Devices: no PMT");
        return true;
      }

      int serviceId = (pmt[3] << 8) + pmt[4];
      Log.Log.Debug("Digital Devices: new service ID is {0} (0x{1:x})", serviceId, serviceId);

      // Disable messages from the CAM for the next 10 seconds.
      if (_ciContexts.Count > 1)
      {
        _camMessagesDisabled = true;
        _camMessageEnableTs = DateTime.Now.AddSeconds(10);
      }

      // We apply the request to the first CI filter in the graph and the
      // driver will automatically try all of filters to see if any of them
      // can decrypt the service.
      int paramSize = sizeof(Int32);
      IntPtr buffer = Marshal.AllocCoTaskMem(paramSize);
      Marshal.WriteInt32(buffer, serviceId | (int)DecryptChainingRestriction.None);
      int hr = _ciContexts[0].PropertySet.Set(CommonInterfacePropertySet, (int)CommonInterfaceProperty.DecryptProgram,
        buffer, paramSize,
        buffer, paramSize
      );
      Marshal.FreeCoTaskMem(buffer);
      if (hr == 0)
      {
        Log.Log.Debug("Digital Devices: result = success");
        return true;
      }

      Log.Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
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
      if (!_isCiSlotPresent)
      {
        return false;
      }
      Log.Log.Debug("Digital Devices: enter menu");


      // If this tuner is only configured with one CI slot then enter the menu directly.
      if (_ciContexts.Count == 1)
      {
        Log.Log.Debug("Digital Devices: there is only one CI slot present => entering menu directly");
        return EnterMenu(0);
      }

      // If there are multiple CI filters in the graph then we present the user with a
      // "fake" menu that allows them to choose which CAM they are interested in. The
      // choices are the root menu names for each of the CAMs.
      Log.Log.Debug("Digital Devices: there are {0} CI slots present => opening root menu", _ciContexts.Count);
      if (_ciMenuCallbacks == null)
      {
        Log.Log.Debug("Digital Devices: callbacks are not available");
        return false;
      }

      try
      {
        _ciMenuCallbacks.OnCiMenu("CAM Selection", "Please select a CAM.", String.Empty, _ciContexts.Count);
        for (int i = 0; i < _ciContexts.Count; i++)
        {
          _ciMenuCallbacks.OnCiMenuChoice(i, _ciContexts[i].CamMenuTitle);
          Log.Log.Debug("  {0} = {1}", _ciContexts[i].CamMenuTitle);
        }
        _menuContext = -1;
        return true;
      }
      catch (Exception ex)
      {
        Log.Log.Debug("Digital Devices: enter menu exception\r\n{0}", ex.ToString());
      }
      return false;
    }

    /// <summary>
    /// Enter the menu for a specific CAM/slot.
    /// </summary>
    /// <param name="slot">The index of the CI context structure for the slot containing the CAM.</param>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    private bool EnterMenu(int slot)
    {
      Log.Log.Debug("Digital Devices: slot {0} enter menu", slot);
      int returnedByteCount;
      int hr = _ciContexts[slot].PropertySet.Get(CamControlPropertySet, (int)CamControlProperty.EnterMenu,
        IntPtr.Zero, 0,
        IntPtr.Zero, 0,
        out returnedByteCount
      );
      if (hr == 0)
      {
        Log.Log.Debug("Digital Devices: result = success");
        _menuContext = slot;
        // Reset the menu depth tracker.
        _ciContexts[slot].CamMenuId = 0;
        return true;
      }

      Log.Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Sends a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      if (!_isCiSlotPresent)
      {
        return false;
      }
      Log.Log.Debug("Digital Devices: slot {0} close menu", _menuContext);
      int returnedByteCount;
      int hr = _ciContexts[_menuContext].PropertySet.Get(CamControlPropertySet, (int)CamControlProperty.CloseMenu,
        IntPtr.Zero, 0,
        IntPtr.Zero, 0,
        out returnedByteCount
      );
      if (hr == 0)
      {
        Log.Log.WriteFile("Digital Devices: result = success");
        return true;
      }

      Log.Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Sends a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      if (!_isCiSlotPresent)
      {
        return false;
      }

      // Is the user really interacting with the CAM menu, or are they interacting with
      // our "fake" root menu?
      if (_menuContext == -1)
      {
        if (choice == 0)
        {
          Log.Log.Debug("Digital Devices: close root menu");
          try
          {
            if (_ciMenuCallbacks != null)
            {
              _ciMenuCallbacks.OnCiCloseDisplay(0);
            }
            return true;
          }
          catch (Exception ex)
          {
            Log.Log.Debug("Digital Devices: select menu exception\r\n{0}", ex.ToString());
          }
        }
        else
        {
          return EnterMenu(choice - 1);
        }
      }

      Log.Log.Debug("Digital Devices: slot {0} select menu entry, choice = {1}", _menuContext, choice);
      MenuChoice reply;
      reply.Id = _ciContexts[_menuContext].CamMenuId;
      reply.Choice = choice;
      Marshal.StructureToPtr(reply, _mmiBuffer, true);

      int returnedByteCount;
      int hr = _ciContexts[_menuContext].PropertySet.Get(CamControlPropertySet, (int)CamControlProperty.MenuReply,
        _mmiBuffer, MenuChoiceSize,
        _mmiBuffer, MenuChoiceSize,
        out returnedByteCount
      );
      if (hr == 0)
      {
        Log.Log.WriteFile("Digital Devices: result = success");
        return true;
      }

      Log.Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Sends a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (!_isCiSlotPresent)
      {
        return false;
      }
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Log.Debug("Digital Devices: slot {0} send menu answer, answer = {1}, cancel = {2}", _menuContext, answer, cancel);

      Marshal.WriteInt32(_mmiBuffer, 0, _ciContexts[_menuContext].CamMenuId);
      Marshal.WriteInt32(_mmiBuffer, 4, answer.Length);
      Marshal.WriteInt32(_mmiBuffer, 8, 0);
      for (int i = 0; i < answer.Length; i++)
      {
        Marshal.WriteByte(_mmiBuffer, 8 + i, (byte)answer[i]);
      }
      // NULL terminate the string.
      Marshal.WriteByte(_mmiBuffer, 8 + answer.Length, 0);

      int bufferSize = 8 + Math.Max(4, answer.Length + 1);
      DVB_MMI.DumpBinary(_mmiBuffer, 0, bufferSize);

      int returnedByteCount;
      int hr = _ciContexts[_menuContext].PropertySet.Get(CamControlPropertySet, (int)CamControlProperty.CamAnswer,
        _mmiBuffer, bufferSize,
        _mmiBuffer, bufferSize,
        out returnedByteCount
      );
      if (hr == 0)
      {
        Log.Log.WriteFile("Digital Devices: result = success");
        return true;
      }

      Log.Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
        Log.Log.Debug("Digital Devices: starting new MMI handler thread");
        _stopMmiHandlerThread = false;
        _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
        _mmiHandlerThread.Name = "Digital Devices MMI handler";
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
      Log.Log.Debug("Digital Devices: MMI handler thread start polling");
      _camMessagesDisabled = false;
      try
      {
        while (!_stopMmiHandlerThread)
        {
          // If CAM messages are currently disabled then check if
          // we can re-enable them now.
          if (_camMessagesDisabled && _camMessageEnableTs < DateTime.Now)
          {
            _camMessagesDisabled = false;
          }

          for (int i = 0; i < _ciContexts.Count; i++)
          {
            MenuData menu;
            if (ReadMmi(i, out menu))
            {
              Log.Log.Debug("  id        = {0}", menu.Id);
              Log.Log.Debug("  type      = {0}", menu.Type);
              Log.Log.Debug("  length    = {0}", menu.Length);

              try
              {
                if (menu.Type == 1 || menu.Type == 2)
                {
                  Log.Log.Debug("  title     = {0}", menu.Title);
                  Log.Log.Debug("  sub-title = {0}", menu.SubTitle);
                  Log.Log.Debug("  footer    = {0}", menu.Footer);
                  Log.Log.Debug("  # entries = {0}", menu.EntryCount);

                  if (_ciMenuCallbacks != null && !_camMessagesDisabled)
                  {
                    _ciMenuCallbacks.OnCiMenu(menu.Title, menu.SubTitle, menu.Footer, menu.EntryCount);
                  }

                  for (int j = 0; j < menu.EntryCount; j++)
                  {
                    String entry = Marshal.PtrToStringAnsi(_mmiBuffer);
                    Log.Log.Debug("  entry {0,-2}  = {1}", j + 1, entry);
                    if (_ciMenuCallbacks != null && !_camMessagesDisabled)
                    {
                      _ciMenuCallbacks.OnCiMenuChoice(j, entry);
                    }
                  }
                }
                else if (menu.Type == 3 || menu.Type == 4)
                {
                  Log.Log.Debug("  text      = {0}", menu.Title);
                  Log.Log.Debug("  length    = {0}", menu.EntryCount);
                  if (_ciMenuCallbacks != null && !_camMessagesDisabled)
                  {
                    _ciMenuCallbacks.OnCiRequest(false, (uint)menu.EntryCount, menu.Title);
                  }
                }
              }
              catch (Exception ex)
              {
                Log.Log.Debug("Digital Devices: callback threw exception\r\n{0}", ex.ToString());
              }
            }
          }
          Thread.Sleep(500);
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Log.Debug("Digital Devices: error in MMI handler thread\r\n{0}", ex.ToString());
        return;
      }
    }

    /// <summary>
    /// Read and parse an MMI response from the CAM into a MenuData object.
    /// </summary>
    /// <param name="slot">The index of the CI context structure for the slot containing the CAM.</param>
    /// <param name="menu">The parsed response from the CAM.</param>
    /// <returns><c>true</c> if the response from the CAM was successfully parsed, otherwise <c>false</c></returns>
    private bool ReadMmi(int slot, out MenuData menu)
    {
      menu = new MenuData();
      for (int i = 0; i < MenuDataSize; i++)
      {
        Marshal.WriteByte(_mmiBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _ciContexts[slot].PropertySet.Get(CamControlPropertySet, (int)CamControlProperty.GetMenu,
        _mmiBuffer, MenuDataSize,
        _mmiBuffer, MenuDataSize,
        out returnedByteCount
      );
      if (hr != 0)
      {
        Log.Log.Debug("Digital Devices: read MMI failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      DVB_MMI.DumpBinary(_mmiBuffer, 0, returnedByteCount);

      menu.Id = Marshal.ReadInt32(_mmiBuffer, 0);
      // Is this a menu that we haven't seen before?
      if (menu.Id == _ciContexts[slot].CamMenuId)
      {
        return false;
      }
      _ciContexts[slot].CamMenuId = menu.Id;
      menu.Type = Marshal.ReadInt32(_mmiBuffer, 4);
      menu.EntryCount = Marshal.ReadInt32(_mmiBuffer, 8);
      menu.Length = Marshal.ReadInt32(_mmiBuffer, 12);
      menu.Entries = new List<String>();
      int offset = 16;
      for (int i = 0; i < menu.EntryCount + 3; i++)
      {
        IntPtr stringPtr = new IntPtr(_mmiBuffer.ToInt32() + offset);
        String entry = Marshal.PtrToStringAnsi(stringPtr);
        switch (i)
        {
          case 0:
            menu.Title = entry;
            break;
          case 1:
            menu.SubTitle = entry;
            break;
          case 2:
            menu.Footer = entry;
            break;
          default:
            menu.Entries.Add(entry);
            break;
        }
        offset += entry.Length + 1;
      }
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Free unmanaged memory buffers and release COM objects.
    /// </summary>
    public void Dispose()
    {
      if (!_isDigitalDevices)
      {
        return;
      }

      if (_isCiSlotPresent)
      {
        _stopMmiHandlerThread = true;
        Thread.Sleep(1000);

        foreach (CiContext context in _ciContexts)
        {
          DevicesInUse.Instance.Remove(context.Device);
          context.Device = null;
          _graph.RemoveFilter(context.PropertySet as IBaseFilter);
          Release.ComObject(context.PropertySet);
        }
      }
      Marshal.FreeCoTaskMem(_mmiBuffer);
      _isDigitalDevices = false;
    }

    #endregion
  }
}