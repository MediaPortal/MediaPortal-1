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
using System.Threading;
using DirectShowLib;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Device;
using TvLibrary.Log;

namespace TvEngine
{
  /// <summary>
  /// A class for handling conditional access for Digital Devices devices.
  /// </summary>
  public class DigitalDevices : BaseCustomDevice, IAddOnDevice, IConditionalAccessProvider, ICiMenuActions
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
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

    [StructLayout(LayoutKind.Sequential)]
    private struct MenuChoice   // DD_CAM_MENU_REPLY
    {
      public Int32 Id;
      public Int32 Choice;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
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
        CamMenuId = 0;

        // Get the filter name.
        FilterInfo filterInfo;
        int hr = filter.QueryFilterInfo(out filterInfo);
        if (filterInfo.pGraph != null)
        {
          DsUtils.ReleaseComObject(filterInfo.pGraph);
          filterInfo.pGraph = null;
        }
        if (hr == 0 && filterInfo.achName != null)
        {
          FilterName = filterInfo.achName;
        }
        else
        {
          FilterName = String.Empty;
        }
        CamMenuTitle = FilterName;
      }
    }

    #endregion

    #region constants

    private static readonly string[] ValidDeviceNamePrefixes = new string[]
    {
      "Digital Devices",
      "Mystique SaTiX-S2 Dual"
    };

    private static readonly Guid CommonInterfacePropertySet = new Guid(0x0aa8a501, 0xa240, 0x11de, 0xb1, 0x30, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x56);
    private static readonly Guid CamControlPropertySet = new Guid(0x0aa8a511, 0xa240, 0x11de, 0xb1, 0x30, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x56);

    private const String CommonDevicePathSection = "fbca-11de-b16f-000000004d56";
    private const int MenuDataSize = 2048;  // This is arbitrary - an estimate of the buffer size needed to hold the largest menu.
    private const int MenuChoiceSize = 8;
    private const int MmiHandlerThreadSleepTime = 500;   // unit = ms

    #endregion

    #region variables

    // We use this list to keep track of common interface filters that are already in use across all
    // DigitalDevices instances.
    private static List<String> _devicesInUse = new List<String>();

    private bool _isDigitalDevices = false;
    private String _name = "Digital Devices";
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
    /// Read the CAM menu title from the CAM in a specific CI slot.
    /// </summary>
    /// <param name="slot">The index of the CI context structure for the slot containing the CAM.</param>
    /// <param name="title">The CAM menu title.</param>
    /// <returns><c>true</c> if the CAM title is read successfully, otherwise <c>false</c></returns>
    private bool GetMenuTitle(int slot, out String title)
    {
      Log.WriteFile("Digital Devices: slot {0} read CAM title", slot);
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
        Log.WriteFile("  title = {0}", title);
        Log.WriteFile("Digital Devices: result = success");
        return true;
      }

      Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #region MMI handler thread

    /// <summary>
    /// Start a thread that will handle interaction with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Don't start a thread if there is no purpose for it.
      if (!_isDigitalDevices || !_isCiSlotPresent || _ciContexts == null)
      {
        return;
      }

      // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
      {
        Log.Debug("Digital Devices: aborting old MMI handler thread");
        _mmiHandlerThread.Abort();
        _mmiHandlerThread = null;
      }
      if (_mmiHandlerThread == null)
      {
        Log.Debug("Digital Devices: starting new MMI handler thread");
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
      Log.Debug("Digital Devices: MMI handler thread start polling");
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
              Log.Debug("  slot      = {0}", i + 1);
              Log.Debug("  id        = {0}", menu.Id);
              Log.Debug("  type      = {0}", menu.Type);
              Log.Debug("  length    = {0}", menu.Length);

              if (_camMessagesDisabled)
              {
                Log.Debug("Digital Devices: CAM messages are currently disabled");
              }
              else if (_ciMenuCallbacks == null)
              {
                Log.Debug("Digital Devices: menu callbacks are not set");
              }

              try
              {
                if (menu.Type == 1 || menu.Type == 2)
                {
                  Log.Debug("  title     = {0}", menu.Title);
                  Log.Debug("  sub-title = {0}", menu.SubTitle);
                  Log.Debug("  footer    = {0}", menu.Footer);
                  Log.Debug("  # entries = {0}", menu.EntryCount);

                  if (_ciMenuCallbacks != null && !_camMessagesDisabled)
                  {
                    _ciMenuCallbacks.OnCiMenu(menu.Title, menu.SubTitle, menu.Footer, menu.EntryCount);
                  }

                  for (int j = 0; j < menu.EntryCount; j++)
                  {
                    String entry = Marshal.PtrToStringAnsi(_mmiBuffer);
                    Log.Debug("  entry {0,-2}  = {1}", j + 1, entry);
                    if (_ciMenuCallbacks != null && !_camMessagesDisabled)
                    {
                      _ciMenuCallbacks.OnCiMenuChoice(j, entry);
                    }
                  }
                }
                else if (menu.Type == 3 || menu.Type == 4)
                {
                  Log.Debug("  text      = {0}", menu.Title);
                  Log.Debug("  length    = {0}", menu.EntryCount);
                  if (_ciMenuCallbacks != null && !_camMessagesDisabled)
                  {
                    _ciMenuCallbacks.OnCiRequest(false, (uint)menu.EntryCount, menu.Title);
                  }
                }
              }
              catch (Exception ex)
              {
                Log.Debug("Digital Devices: callback threw exception\r\n{0}", ex.ToString());
              }
            }
          }
          Thread.Sleep(MmiHandlerThreadSleepTime);
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        Log.Debug("Digital Devices: exception in MMI handler thread\r\n{0}", ex.ToString());
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
        Log.Debug("Digital Devices: read MMI failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      DVB_MMI.DumpBinary(_mmiBuffer, 0, returnedByteCount);

      // Is this a menu that we haven't seen before?
      menu.Id = Marshal.ReadInt32(_mmiBuffer, 0);
      if (menu.Id == _ciContexts[slot].CamMenuId)
      {
        return false;
      }
      _ciContexts[slot].CamMenuId = menu.Id;

      // Manually marshal the MMI information into our structure. We are forced
      // to do this manually because the strings are passed inline with variable lengths
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

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        return _name;
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      Log.Debug("Digital Devices: initialising device");

      // Digital Devices components have a common section in their device path.
      if (String.IsNullOrEmpty(tunerDevicePath))
      {
        Log.Debug("Digital Devices: tuner device path is not set");
        return false;
      }
      if (_isDigitalDevices)
      {
        Log.Debug("Digital Devices: device is already initialised");
        return true;
      }

      if (!tunerDevicePath.ToLowerInvariant().Contains(CommonDevicePathSection))
      {
        Log.Debug("Digital Devices: device path does not contain the Digital Devices common section");
        return false;
      }

      Log.Debug("Digital Devices: supported device detected");
      _isDigitalDevices = true;
      FilterInfo tunerFilterInfo;
      int hr = tunerFilter.QueryFilterInfo(out tunerFilterInfo);
      if (tunerFilterInfo.pGraph != null)
      {
        DsUtils.ReleaseComObject(tunerFilterInfo.pGraph);
        tunerFilterInfo.pGraph = null;
      }
      if (hr != 0 || String.IsNullOrEmpty(tunerFilterInfo.achName))
      {
        Log.Debug("Digital Devices: failed to get the tuner filter name, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        foreach (String prefix in ValidDeviceNamePrefixes)
        {
          if (tunerFilterInfo.achName.StartsWith(prefix))
          {
            Log.Debug("Digital Devices: \"{0}\", {1} variant", tunerFilterInfo.achName, prefix);
            _name = prefix;
            break;
          }
        }
      }
      return true;
    }

    #region graph state change callbacks

    /// <summary>
    /// This callback is invoked after a tune request is submitted, when the device's BDA graph is running
    /// but before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public override void OnGraphRunning(ITVCard tuner, IChannel currentChannel)
    {
      // Ensure the MMI handler thread is always running when the graph is running.
      StartMmiHandlerThread();
    }

    #endregion

    #endregion

    #region IAddOnDevice member

    /// <summary>
    /// Insert and connect the device's additional filter(s) into the BDA graph.
    /// [network provider]->[tuner]->[capture]->[...device filter(s)]->[infinite tee]->[MPEG 2 demultiplexer]->[transport information filter]->[transport stream writer]
    /// </summary>
    /// <param name="graphBuilder">The graph builder to use to insert the device filter(s).</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture/receiver filter) to
    ///   connect the [first] device filter to.</param>
    /// <returns><c>true</c> if the device was successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(ICaptureGraphBuilder2 graphBuilder, ref IBaseFilter lastFilter)
    {
      Log.Debug("Digital Devices: add to graph");

      if (!_isDigitalDevices)
      {
        Log.Debug("Digital Devices: device not initialised or interface not supported");
        return false;
      }
      if (graphBuilder == null)
      {
        Log.Debug("Digital Devices: graph builder is null");
        return false;
      }
      if (lastFilter == null)
      {
        Log.Debug("Digital Devices: upstream filter is null");
        return false;
      }
      if (_ciContexts != null && _ciContexts.Count > 0)
      {
        Log.Debug("Digital Devices: {0} device filter(s) already in graph", _ciContexts.Count);
        return true;
      }

      // We need a reference to the graph builder's graph.
      IGraphBuilder tmpGraph = null;
      int hr = graphBuilder.GetFiltergraph(out tmpGraph);
      if (hr != 0)
      {
        Log.Debug("Digital Devices: failed to get graph reference, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      _graph = tmpGraph as IFilterGraph2;
      if (_graph == null)
      {
        Log.Debug("Digital Devices: failed to get graph reference");
        return false;
      }

      // We need a demux filter to test whether we can add any further CI filters
      // to the graph.
      IBaseFilter tmpDemux = (IBaseFilter)new MPEG2Demultiplexer();
      hr = _graph.AddFilter(tmpDemux, "Temp MPEG2-Demux");
      if (hr != 0)
      {
        Log.Debug("Digital Devices: failed to add test demux to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        DsUtils.ReleaseComObject(tmpDemux);
        return false;
      }
      IPin demuxInputPin = DsFindPin.ByDirection(tmpDemux, PinDirection.Input, 0);
      if (demuxInputPin == null)
      {
        Log.Debug("Digital Devices: failed to find the demux input pin");
        _graph.RemoveFilter(tmpDemux);
        DsUtils.ReleaseComObject(tmpDemux);
        tmpDemux = null;
        return false;
      }

      // We start our work with the last filter in the graph, which we expect to be
      // either a tuner or capture filter. We need the output pin...
      IPin lastFilterOutputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
      if (lastFilterOutputPin == null)
      {
        Log.Debug("Digital Devices: upstream filter doesn't have an output pin");
        DsUtils.ReleaseComObject(demuxInputPin);
        _graph.RemoveFilter(tmpDemux);
        DsUtils.ReleaseComObject(tmpDemux);
        tmpDemux = null;
        return false;
      }

      // This will be our list of CI contexts.
      _ciContexts = new List<CiContext>();

      DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
      while (true)
      {
        // Stage 1: if connection to a demux is possible then no [further] CI slots are configured
        // for this tuner. This test removes a 30 to 45 second delay when the graphbuilder tries
        // to render [capture]->[CI]->[demux].
        if (_graph.Connect(lastFilterOutputPin, demuxInputPin) == 0)
        {
          Log.WriteFile("Digital Devices: no [more] CI filters available or configured for this tuner");
          lastFilterOutputPin.Disconnect();
          break;
        }

        // Stage 2: see if there are any more CI filters that we can add to the graph. We re-loop
        // over all capture devices because the CI filters have to be connected in a specific order
        // which is not guaranteed to be the same as the capture device array order.
        bool addedFilter = false;
        foreach (DsDevice captureDevice in captureDevices)
        {
          // We're looking for a Digital Devices common interface device that is not
          // already in the graph.
          if (captureDevice.DevicePath == null ||
            captureDevice.Name == null ||
            !captureDevice.DevicePath.ToLowerInvariant().Contains(CommonDevicePathSection) ||
            !captureDevice.Name.ToLowerInvariant().Contains("common interface"))
          {
            continue;
          }
          lock (_devicesInUse)
          {
            List<String>.Enumerator en = _devicesInUse.GetEnumerator();
            bool found = false;
            while (en.MoveNext())
            {
              if (en.Current.Equals(captureDevice.DevicePath))
              {
                found = true;
                break;
              }
            }
            if (found)
            {
              continue;
            }
            _devicesInUse.Add(captureDevice.DevicePath);
          }

          // Stage 3: okay, we've got a CI filter device. Let's try and connect it into the graph.
          Log.Debug("Digital Devices: adding filter for device \"{0}\"", captureDevice.Name);
          IBaseFilter tmpCiFilter = null;
          try
          {
            hr = _graph.AddSourceFilterForMoniker(captureDevice.Mon, null, captureDevice.Name, out tmpCiFilter);
            if (hr != 0 || tmpCiFilter == null)
            {
              Log.Debug("Digital Devices: failed to add filter to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
              continue;
            }

            // Now we've got a filter in the graph. Let's see if it will connect.
            hr = graphBuilder.RenderStream(null, null, lastFilter, null, tmpCiFilter);
            if (hr != 0)
            {
              Log.Debug("Digital Devices: failed to render stream through filter, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
              _graph.RemoveFilter(tmpCiFilter);
              DsUtils.ReleaseComObject(tmpCiFilter);
              tmpCiFilter = null;
              continue;
            }

            // Ensure that the filter has an output pin.
            IPin tmpFilterOutputPin = DsFindPin.ByDirection(tmpCiFilter, PinDirection.Output, 0);
            if (tmpFilterOutputPin == null)
            {
              Log.Debug("Digital Devices: filter doesn't have an output pin");
              _graph.RemoveFilter(tmpCiFilter);
              DsUtils.ReleaseComObject(tmpCiFilter);
              tmpCiFilter = null;
              continue;
            }
            DsUtils.ReleaseComObject(lastFilterOutputPin);
            lastFilterOutputPin = tmpFilterOutputPin;
          }
          finally
          {
            if (tmpCiFilter == null)
            {
              lock (_devicesInUse)
              {
                _devicesInUse.Remove(captureDevice.DevicePath);
              }
            }
          }

          // Excellent - CI filter successfully added!
          _ciContexts.Add(new CiContext(tmpCiFilter, captureDevice));
          Log.Debug("Digital Devices: total of {0} CI filter(s) in the graph", _ciContexts.Count);
          lastFilter = tmpCiFilter;
          addedFilter = true;
          _isCiSlotPresent = true;
        }

        // Insurance: we don't want to get stuck in an endless loop.
        if (!addedFilter)
        {
          break;
        }
      }

      DsUtils.ReleaseComObject(lastFilterOutputPin);
      lastFilterOutputPin = null;
      DsUtils.ReleaseComObject(demuxInputPin);
      demuxInputPin = null;
      _graph.RemoveFilter(tmpDemux);
      DsUtils.ReleaseComObject(tmpDemux);
      tmpDemux = null;
      return _isCiSlotPresent;
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
      Log.Debug("Digital Devices: open conditional access interface");

      if (!_isDigitalDevices)
      {
        Log.Debug("Digital Devices: device not initialised or interface not supported");
        return false;
      }
      if (_ciContexts == null)
      {
        Log.Debug("Digital Devices: device filter(s) not added to the BDA filter graph");
        return false;
      }
      if (!_isCiSlotPresent)
      {
        Log.Debug("Digital Devices: CI slot not present");
        return false;
      }
      if (_mmiBuffer != IntPtr.Zero)
      {
        Log.Debug("Digital Devices: interface is already open");
        return false;
      }

      _mmiBuffer = Marshal.AllocCoTaskMem(MenuDataSize);

      // Fill in the menu title for each CI context if possible.
      String menuTitle;
      for (byte i = 0; i < _ciContexts.Count; i++)
      {
        if (GetMenuTitle(i, out menuTitle))
        {
          _ciContexts[i].CamMenuTitle = "CI #" + (i + 1) + ": " + menuTitle;
        }
        else
        {
          _ciContexts[i].CamMenuTitle = "CI #" + (i + 1);
        }
      }

      StartMmiHandlerThread();

      Log.Debug("Digital Devices: result = success");
      return true;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      Log.Debug("Digital Devices: close conditional access interface");
      if (_mmiHandlerThread != null && _mmiHandlerThread.IsAlive)
      {
        _stopMmiHandlerThread = true;
        // In the worst case scenario it should take approximately
        // twice the thread sleep time to cleanly stop the thread.
        Thread.Sleep(MmiHandlerThreadSleepTime * 2);
        _mmiHandlerThread = null;
      }

      if (_mmiBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiBuffer);
        _mmiBuffer = IntPtr.Zero;
      }

      // We reserve the removal of the filters from the graph for when the device is disposed, otherwise
      // the interface cannot easily be re-opened.
      Log.Debug("Digital Devices: result = true");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph">This parameter will be set to <c>true</c> if the BDA graph must be rebuilt
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
    public bool ResetInterface(out bool rebuildGraph)
    {
      Log.Debug("Digital Devices: reset conditional access interface");

      rebuildGraph = false;

      if (!_isDigitalDevices || _ciContexts == null)
      {
        Log.Debug("Digital Devices: device not initialised or interface not supported");
        return false;
      }
      if (!_isCiSlotPresent)
      {
        Log.Debug("Digital Devices: CI slot not present");
        return false;
      }

      bool success = CloseInterface();

      // Reset the slot selection for menu browsing.
      _menuContext = -1;

      // We reset all the CI filters in the graph.
      int returnedByteCount;
      for (int i = 0; i < _ciContexts.Count; i++)
      {
        Log.Debug("Digital Devices: reset slot {0} \"{1}\"", i + 1, _ciContexts[i].FilterName);
        int hr = _ciContexts[i].PropertySet.Get(CamControlPropertySet, (int)CamControlProperty.Reset,
          IntPtr.Zero, 0,
          IntPtr.Zero, 0,
          out returnedByteCount
        );
        if (hr == 0)
        {
          Log.Debug("Digital Devices: result = success");
          // Reset the menu depth tracker.
          _ciContexts[i].CamMenuId = 0;
        }
        else
        {
          Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          success = false;
        }
      }
      return success && OpenInterface();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsInterfaceReady()
    {
      Log.Debug("Digital Devices: is conditional access interface ready");

      // Unfortunately We can't directly determine if the CAM(s) are ready. We could
      // attempt to read the CAM name and assume that the CAM is ready if that operation
      // were successful, however we can't be sure which CAM would actually be used
      // for the impending operation. We also can't expect that all CI slots would
      // be populated at all times. Therefore we simply return true if at least one
      // CI filter has been added to the graph.
      Log.Debug("Digital Devices: result = {0}", _isCiSlotPresent);
      return _isCiSlotPresent;
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="channel">The channel information associated with the service which the command relates to.</param>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more services
    ///   simultaneously. This parameter gives the interface an indication of the number of services that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The programme map table entry for the service.</param>
    /// <param name="cat">The conditional access table entry for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, byte[] pmt, byte[] cat)
    {
      Log.Debug("Digital Devices: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isDigitalDevices)
      {
        Log.Debug("Digital Devices: interface not supported");
        return false;
      }
      if (!_isCiSlotPresent || _ciContexts == null || _ciContexts.Count == 0)
      {
        Log.Debug("Digital Devices: CI slot not present");
        // If there are no CI slots then there is no point retrying.
        return true;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        Log.Debug("Digital Devices: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null || pmt.Length < 5)
      {
        Log.Debug("Digital Devices: PMT not supplied");
        return true;
      }

      // "Not selected" commands do nothing.
      if (command == CaPmtCommand.NotSelected)
      {
        Log.Debug("Digital Devices: result = success");
        return true;
      }

      int serviceId = (pmt[3] << 8) + pmt[4];
      Log.Debug("Digital Devices: service ID is {0} (0x{1:x})", serviceId, serviceId);

      // Disable messages from the CAM for the next 10 seconds. We do this to
      // avoid showing MMI messages which the driver tries to use unsuccessfully.
      // A better solution would be to apply requests to the CAM(s) that are able
      // to decrypt the service, however unfortunately the interface doesn't provide
      // any way to query the CAM or determine whether a decrypt command will
      // or has succeeded.
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
        Log.Debug("Digital Devices: result = success");
        return true;
      }

      Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Set the CAM menu callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        // Ensure that the MMI handler thread is running.
        StartMmiHandlerThread();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      Log.Debug("Digital Devices: enter menu");

      if (!_isDigitalDevices)
      {
        Log.Debug("Digital Devices: interface not supported");
        return false;
      }
      if (!_isCiSlotPresent || _ciContexts == null || _ciContexts.Count == 0)
      {
        Log.Debug("Digital Devices: CI slot not present");
        // If there are no CI slots then there is no point retrying.
        return true;
      }

      // If this tuner is only configured with one CI slot then enter the menu directly.
      if (_ciContexts.Count == 1)
      {
        Log.Debug("Digital Devices: there is only one CI slot present => entering menu directly");
        return EnterMenu(0);
      }

      // If there are multiple CI filters in the graph then we present the user with a
      // "fake" menu that allows them to choose which CAM they are interested in. The
      // choices are the root menu names for each of the CAMs.
      Log.Debug("Digital Devices: there are {0} CI slots present => opening root menu", _ciContexts.Count);
      if (_ciMenuCallbacks == null)
      {
        Log.Debug("Digital Devices: menu callbacks are not set");
        return false;
      }

      try
      {
        _ciMenuCallbacks.OnCiMenu("CAM Selection", "Please select a CAM.", String.Empty, _ciContexts.Count);
        for (int i = 0; i < _ciContexts.Count; i++)
        {
          _ciMenuCallbacks.OnCiMenuChoice(i, _ciContexts[i].CamMenuTitle);
          Log.Debug("  {0} = {1}", _ciContexts[i].CamMenuTitle);
        }
        _menuContext = -1;
        Log.Debug("Digital Devices: result = success");
        return true;
      }
      catch (Exception ex)
      {
        Log.Debug("Digital Devices: enter menu exception\r\n{0}", ex.ToString());
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
      Log.Debug("Digital Devices: slot {0} enter menu", slot);

      if (!_isDigitalDevices)
      {
        Log.Debug("Digital Devices: interface not supported");
        return false;
      }
      if (!_isCiSlotPresent || _ciContexts == null || _menuContext >= _ciContexts.Count)
      {
        Log.Debug("Digital Devices: CI slot not present");
        // If there are no CI slots then there is no point retrying.
        return true;
      }

      int returnedByteCount;
      int hr = _ciContexts[slot].PropertySet.Get(CamControlPropertySet, (int)CamControlProperty.EnterMenu,
        IntPtr.Zero, 0,
        IntPtr.Zero, 0,
        out returnedByteCount
      );
      if (hr == 0)
      {
        Log.Debug("Digital Devices: result = success");
        // Future menu interactions will be passed to this CI slot/CAM.
        _menuContext = slot;
        // Reset the menu depth tracker.
        _ciContexts[slot].CamMenuId = 0;
        return true;
      }

      Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      Log.Debug("Digital Devices: slot {0} close menu", _menuContext);

      if (!_isDigitalDevices)
      {
        Log.Debug("Digital Devices: interface not supported");
        return false;
      }
      if (!_isCiSlotPresent || _ciContexts == null || _menuContext >= _ciContexts.Count)
      {
        Log.Debug("Digital Devices: CI slot not present");
        // If there are no CI slots then there is no point retrying.
        return true;
      }

      int returnedByteCount;
      int hr = _ciContexts[_menuContext].PropertySet.Get(CamControlPropertySet, (int)CamControlProperty.CloseMenu,
        IntPtr.Zero, 0,
        IntPtr.Zero, 0,
        out returnedByteCount
      );
      if (hr == 0)
      {
        Log.WriteFile("Digital Devices: result = success");
        return true;
      }

      Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      // Is the user really interacting with the CAM menu, or are they interacting with
      // our "fake" root menu?
      if (_menuContext == -1)
      {
        if (choice == 0)
        {
          Log.Debug("Digital Devices: close root menu");
          try
          {
            if (_ciMenuCallbacks != null)
            {
              _ciMenuCallbacks.OnCiCloseDisplay(0);
            }
            else
            {
              Log.Debug("Digital Devices: menu callbacks are not set");
            }
            return true;
          }
          catch (Exception ex)
          {
            Log.Debug("Digital Devices: select menu exception\r\n{0}", ex.ToString());
          }
        }
        else
        {
          return EnterMenu(choice - 1);
        }
      }

      Log.Debug("Digital Devices: slot {0} select menu entry, choice = {1}", _menuContext, choice);

      if (!_isDigitalDevices)
      {
        Log.Debug("Digital Devices: interface not supported");
        return false;
      }
      if (!_isCiSlotPresent || _ciContexts == null || _menuContext >= _ciContexts.Count)
      {
        Log.Debug("Digital Devices: CI slot not present");
        // If there are no CI slots then there is no point retrying.
        return true;
      }

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
        Log.WriteFile("Digital Devices: result = success");
        return true;
      }

      Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Debug("Digital Devices: slot {0} send menu answer, answer = {1}, cancel = {2}", _menuContext, answer, cancel);

      if (!_isDigitalDevices)
      {
        Log.Debug("Digital Devices: interface not supported");
        return false;
      }
      if (!_isCiSlotPresent || _ciContexts == null || _menuContext >= _ciContexts.Count)
      {
        Log.Debug("Digital Devices: CI slot not present");
        // If there are no CI slots then there is no point retrying.
        return true;
      }

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
        Log.WriteFile("Digital Devices: result = success");
        return true;
      }

      Log.Debug("Digital Devices: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      CloseInterface();
      if (_ciContexts != null)
      {
        foreach (CiContext context in _ciContexts)
        {
          if (context.Device != null)
          {
            lock (_devicesInUse)
            {
              _devicesInUse.Remove(context.Device.DevicePath);
            }
            context.Device = null;
          }
          if (context.PropertySet != null)
          {
            _graph.RemoveFilter(context.PropertySet as IBaseFilter);
            DsUtils.ReleaseComObject(context.PropertySet);
            context.PropertySet = null;
          }
        }
      }
      _graph = null;
      _isDigitalDevices = false;
    }

    #endregion
  }
}