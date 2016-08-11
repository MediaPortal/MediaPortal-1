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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using HResult = MediaPortal.Common.Utils.NativeMethods.HResult;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Helper
{
  /// <summary>
  /// A collection of methods to do common DirectShow tasks.
  /// </summary>
  public static class FilterGraphTools
  {
    /// <summary>
    /// Add a filter to a DirectShow graph using its CLSID.
    /// </summary>
    /// <remarks>
    /// The filter class must be registered with Windows using regsvr32.
    /// You can use <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> to check if the CLSID is valid before calling this method.
    /// </remarks>
    /// <param name="graph">The graph.</param>
    /// <param name="clsid">The filter's class ID (CLSID). The class must expose the IBaseFilter interface.</param>
    /// <param name="name">The name or label to use for the filter.</param>
    /// <returns>the instance of the filter if the method successfully created it, otherwise <c>null</c></returns>
    public static IBaseFilter AddFilterFromRegisteredClsid(IFilterGraph2 graph, Guid clsid, string name)
    {
      IBaseFilter filter = null;
      try
      {
        Type type = Type.GetTypeFromCLSID(clsid);
        filter = Activator.CreateInstance(type) as IBaseFilter;

        int hr = graph.AddFilter(filter, name);
        TvExceptionDirectShowError.Throw(hr, "Failed to add the new filter to the graph.");
      }
      catch
      {
        Release.ComObject("filter graph tools add-filter-from-registered-CLSID filter", ref filter);
        throw;
      }

      return filter;
    }

    /// <summary>
    /// Add a filter implemented in a known file to a DirectShow graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="fileName">The name of the file containing the filter implementation.</param>
    /// <param name="clsid">The filter's class ID (CLSID). The class must expose the IBaseFilter interface.</param>
    /// <param name="filterName">The name or label to use for the filter.</param>
    /// <returns>the instance of the filter if the method successfully created it, otherwise <c>null</c></returns>
    public static IBaseFilter AddFilterFromFile(IFilterGraph2 graph, string fileName, Guid clsid, string filterName)
    {
      IBaseFilter filter = null;
      try
      {
        filter = ComHelper.LoadComObjectFromFile(fileName, clsid, typeof(IBaseFilter).GUID) as IBaseFilter;

        int hr = graph.AddFilter(filter, filterName);
        TvExceptionDirectShowError.Throw(hr, "Failed to add the new filter to the graph.");
      }
      catch
      {
        Release.ComObject("filter graph tools add-filter-from-file filter", ref filter);
        throw;
      }

      return filter;
    }

    /// <summary>
    /// Add a filter to a DirectShow graph using its corresponding <see cref="DsDevice"/> (<see cref="IMoniker"/> wrapper).
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="device">The device instance associated with the filter.</param>
    /// <param name="name">The name or label to use for the filter.</param>
    /// <returns>the instance of the filter if the method successfully created it, otherwise <c>null</c></returns>
    public static IBaseFilter AddFilterFromDevice(IFilterGraph2 graph, DsDevice device, string name = null)
    {
      if (device == null || device.Mon == null)
      {
        throw new TvException("Failed to add filter from device, device or moniker is null.");
      }

      IBaseFilter filter = null;
      try
      {
        int hr = graph.AddSourceFilterForMoniker(device.Mon, null, name ?? device.Name, out filter);
        TvExceptionDirectShowError.Throw(hr, "Failed to add the new filter to the graph.");
      }
      catch
      {
        Release.ComObject("filter graph tools add-filter-from-device filter", ref filter);
        throw;
      }
      return filter;
    }

    public delegate bool DeviceSelectorDelegate(DsDevice device);

    /// <summary>
    /// Add a filter to a DirectShow graph using it's category identifier and name.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="categoryId">The identifier of the category which the filter is associated with.</param>
    /// <param name="deviceCheckDelegate">A delegate for selecting the target device/filter.</param>
    /// <returns>the instance of the filter if the method successfully created it, otherwise <c>null</c></returns>
    public static IBaseFilter AddFilterFromCategory(IFilterGraph2 graph, Guid categoryId, DeviceSelectorDelegate deviceSelectorDelegate)
    {
      DsDevice[] devices = DsDevice.GetDevicesOfCat(categoryId);
      if (devices == null)
      {
        return null;
      }
      try
      {
        for (int i = 0; i < devices.Length; i++)
        {
          DsDevice device = devices[i];
          if (device == null || device.Name == null)
          {
            continue;
          }
          if (deviceSelectorDelegate(device))
          {
            return AddFilterFromDevice(graph, device);
          }
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          d.Dispose();
        }
      }
      return null;
    }

    /// <summary>
    /// Save a DirectShow Graph to a GRF file
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="fileName">the file to be saved</param>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur during the file creation</exception>
    /// <seealso cref="LoadGraphFile"/>
    /// <remarks>This method overwrites any existing file</remarks>
    public static void SaveGraphFile(IGraphBuilder graphBuilder, string fileName)
    {
      IStorage storage = null;
#if USING_NET11
            UCOMIStream stream = null;
#else
      IStream stream = null;
#endif

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      try
      {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
          fileName = fileName.Replace(c, '_');
        }
        int hr = NativeMethods.StgCreateDocfile(
          Path.Combine(PathManager.GetDataPath, fileName),
          STGM.Create | STGM.Transacted | STGM.ReadWrite | STGM.ShareExclusive,
          0,
          out storage
          );

        Marshal.ThrowExceptionForHR(hr);

        hr = storage.CreateStream(
          @"ActiveMovieGraph",
          STGM.Write | STGM.Create | STGM.ShareExclusive,
          0,
          0,
          out stream
          );

        Marshal.ThrowExceptionForHR(hr);

        hr = ((IPersistStream)graphBuilder).Save(stream, true);
        Marshal.ThrowExceptionForHR(hr);

        hr = storage.Commit(STGC.Default);
        Marshal.ThrowExceptionForHR(hr);
      }
      finally
      {
        Release.ComObject("filter graph tools save-graph-file stream", ref stream);
        Release.ComObject("filter graph tools save-graph-file storage", ref storage);
      }
    }

    /// <summary>
    /// Load a DirectShow Graph from a file
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="fileName">the file to be loaded</param>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
    /// <exception cref="System.ArgumentException">Thrown if the given file is not a valid graph file</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur during loading</exception>
    /// <seealso cref="SaveGraphFile"/>
    public static void LoadGraphFile(IGraphBuilder graphBuilder, string fileName)
    {
      IStorage storage = null;
#if USING_NET11
			UCOMIStream stream = null;
#else
      IStream stream = null;
#endif

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      try
      {
        if (NativeMethods.StgIsStorageFile(fileName) != 0)
          throw new ArgumentException();

        int hr = NativeMethods.StgOpenStorage(
          fileName,
          null,
          STGM.Transacted | STGM.Read | STGM.ShareDenyWrite,
          IntPtr.Zero,
          0,
          out storage
          );

        Marshal.ThrowExceptionForHR(hr);

        hr = storage.OpenStream(
          @"ActiveMovieGraph",
          IntPtr.Zero,
          STGM.Read | STGM.ShareExclusive,
          0,
          out stream
          );

        Marshal.ThrowExceptionForHR(hr);

        hr = ((IPersistStream)graphBuilder).Load(stream);
        Marshal.ThrowExceptionForHR(hr);
      }
      finally
      {
        Release.ComObject("filter graph tools load-graph-file stream", ref stream);
        Release.ComObject("filter graph tools load-graph-file storage", ref storage);
      }
    }

    /// <summary>
    /// Check if a DirectShow filter can display Property Pages
    /// </summary>
    /// <param name="filter">A DirectShow Filter</param>
    /// <exception cref="System.ArgumentNullException">Thrown if filter is null</exception>
    /// <seealso cref="ShowFilterPropertyPage"/>
    /// <returns>true if the filter has Property Pages, false if not</returns>
    /// <remarks>
    /// This method is intended to be used with <see cref="ShowFilterPropertyPage">ShowFilterPropertyPage</see>
    /// </remarks>
    public static bool HasPropertyPages(IBaseFilter filter)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      return ((filter as ISpecifyPropertyPages) != null);
    }

    /// <summary>
    /// Display Property Pages of a given DirectShow filter
    /// </summary>
    /// <param name="filter">A DirectShow Filter</param>
    /// <param name="parent">A hwnd handle of a window to contain the pages</param>
    /// <exception cref="System.ArgumentNullException">Thrown if filter is null</exception>
    /// <seealso cref="HasPropertyPages"/>
    /// <remarks>
    /// You can check if a filter supports Property Pages with the <see cref="HasPropertyPages">HasPropertyPages</see> method.<br/>
    /// <strong>Warning</strong> : This method is blocking. It only returns when the Property Pages are closed.
    /// </remarks>
    /// <example>This sample shows how to check if a filter supports Property Pages and displays them
    /// <code>
    /// if (FilterGraphTools.HasPropertyPages(myFilter))
    /// {
    ///   FilterGraphTools.ShowFilterPropertyPage(myFilter, myForm.Handle);
    /// }
    /// </code>
    /// </example>
    public static void ShowFilterPropertyPage(IBaseFilter filter, IntPtr parent)
    {
      FilterInfo filterInfo;
      DsCAUUID caGuid;
      object[] objs;

      if (filter == null)
        throw new ArgumentNullException("filter");

      if (HasPropertyPages(filter))
      {
        int hr = filter.QueryFilterInfo(out filterInfo);
        DsError.ThrowExceptionForHR(hr);
        string filterName = filterInfo.achName;
        Release.FilterInfo(ref filterInfo);

        hr = ((ISpecifyPropertyPages)filter).GetPages(out caGuid);
        DsError.ThrowExceptionForHR(hr);

        try
        {
          objs = new object[1];
          objs[0] = filter;

          NativeMethods.OleCreatePropertyFrame(
            parent, 0, 0,
            filterName,
            objs.Length, objs,
            caGuid.cElems, caGuid.pElems,
            0, 0,
            IntPtr.Zero
            );
        }
        finally
        {
          Marshal.FreeCoTaskMem(caGuid.pElems);
        }
      }
    }

    /// <summary>
    /// Check if a COM Object is available
    /// </summary>
    /// <param name="clsid">The CLSID of this object</param>
    /// <example>This sample shows how to check if the MPEG-2 Demultiplexer filter is available
    /// <code>
    /// if (FilterGraphTools.IsThisComObjectInstalled(typeof(MPEG2Demultiplexer).GUID))
    /// {
    ///   // Use it...
    /// }
    /// </code>
    /// </example>
    /// <returns>true if the object is available, false if not</returns>
    public static bool IsThisComObjectInstalled(Guid clsid)
    {
      bool retval = false;

      try
      {
        Type type = Type.GetTypeFromCLSID(clsid);
        object o = Activator.CreateInstance(type);
        retval = true;
        Release.ComObject("filter graph tools is-this-com-object-installed instance", ref o);
      }
      catch
      {
      }
      return retval;
    }

    /// <summary>
    /// Get the name of a filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>the name of the filter</returns>
    public static string GetFilterName(IBaseFilter filter)
    {
      FilterInfo filterInfo;
      int hr = filter.QueryFilterInfo(out filterInfo);
      TvExceptionDirectShowError.Throw(hr, "Failed to query filter information.");
      Release.FilterInfo(ref filterInfo);
      return filterInfo.achName;
    }

    /// <summary>
    /// Get the name of a pin.
    /// </summary>
    /// <param name="pin">The pin.</param>
    /// <returns>the name of the pin</returns>
    public static string GetPinName(IPin pin)
    {
      PinInfo pinInfo;
      int hr = pin.QueryPinInfo(out pinInfo);
      TvExceptionDirectShowError.Throw(hr, "Failed to query pin information.");
      Release.PinInfo(ref pinInfo);
      return pinInfo.name;
    }

    /// <summary>
    /// Add and connect a filter into a DirectShow graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="newFilter">The filter to add and connect.</param>
    /// <param name="filterName">The name or label to use for the filter.</param>
    /// <param name="upstreamFilter">The upstream filter to connect the new filter to. This filter should already be present in the graph.</param>
    /// <param name="upstreamFilterOutputPinIndex">The zero based index of the upstream filter output pin to connect to the new filter.</param>
    /// <param name="newFilterInputPinIndex">The zero based index of the new filter input pin to connect to the upstream filter.</param>
    public static void AddAndConnectFilterIntoGraph(IFilterGraph2 graph, IBaseFilter newFilter, string filterName, IBaseFilter upstreamFilter, int upstreamFilterOutputPinIndex = 0, int newFilterInputPinIndex = 0)
    {
      if (graph == null || upstreamFilter == null || newFilter == null)
      {
        throw new TvException("Failed to add and connect filter, graph, upstream filter or new filter are null.");
      }

      int hr = graph.AddFilter(newFilter, filterName);
      TvExceptionDirectShowError.Throw(hr, "Failed to add the new filter to the graph.");

      try
      {
        ConnectFilters(graph, upstreamFilter, upstreamFilterOutputPinIndex, newFilter, newFilterInputPinIndex);
      }
      catch
      {
        graph.RemoveFilter(newFilter);
        throw;
      }
    }

    /// <summary>
    /// Get the mediums for a pin.
    /// </summary>
    /// <param name="pin">The pin.</param>
    /// <returns>the pin's mediums (if any); <c>null</c> if the pin is not a KS pin</returns>
    public static ICollection<RegPinMedium> GetPinMediums(IPin pin)
    {
      IKsPin ksPin = pin as IKsPin;
      if (ksPin == null)
      {
        return new List<RegPinMedium>(0);
      }

      IntPtr ksMultiplePtr = IntPtr.Zero;
      int hr = ksPin.KsQueryMediums(out ksMultiplePtr);
      // Can return 1 (S_FALSE) for non-error scenarios.
      if (hr < (int)HResult.S_OK)
      {
        TvExceptionDirectShowError.Throw(hr, "Failed to query pin mediums.");
      }
      try
      {
        KSMultipleItem ksMultipleItem = (KSMultipleItem)Marshal.PtrToStructure(ksMultiplePtr, typeof(KSMultipleItem));
        List<RegPinMedium> mediums = new List<RegPinMedium>(ksMultipleItem.Count);
        IntPtr mediumPtr = IntPtr.Add(ksMultiplePtr, Marshal.SizeOf(typeof(KSMultipleItem)));
        int regPinMediumSize = Marshal.SizeOf(typeof(RegPinMedium));
        for (int i = 0; i < ksMultipleItem.Count; i++)
        {
          RegPinMedium m = (RegPinMedium)Marshal.PtrToStructure(mediumPtr, typeof(RegPinMedium));
          // Exclude invalid and non-meaningful mediums.
          if (m.clsMedium != Guid.Empty && m.clsMedium != TveGuid.KS_MEDIUM_SET_ID_STANDARD)
          {
            mediums.Add(m);
          }
          mediumPtr = IntPtr.Add(mediumPtr, regPinMediumSize);
        }
        return mediums;
      }
      finally
      {
        Marshal.FreeCoTaskMem(ksMultiplePtr);
      }
    }

    /// <summary>
    /// Connect two filters in a DirectShow graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="upstreamFilter">The upstream filter.</param>
    /// <param name="upstreamPinIndex">The pin index of the output pin on the upstream filter that should be connected.</param>
    /// <param name="downstreamFilter">The downstream filter.</param>
    /// <param name="downstreamPinIndex">The pin index of the input pin on the downstream filter that should be connected.</param>
    public static void ConnectFilters(IFilterGraph2 graph, IBaseFilter upstreamFilter, int upstreamPinIndex, IBaseFilter downstreamFilter, int downstreamPinIndex)
    {
      if (graph == null)
      {
        throw new TvException("Failed to connect filters, graph is null.");
      }
      if (upstreamFilter == null)
      {
        throw new TvException("Failed to connect filters, upstream filter is null.");
      }
      if (downstreamFilter == null)
      {
        throw new TvException("Failed to connect filters, downstream filter is null.");
      }

      IPin upstreamPin = DsFindPin.ByDirection(upstreamFilter, PinDirection.Output, upstreamPinIndex);
      if (upstreamPin == null)
      {
        throw new TvException("Failed to connect filters, upstream filter does not have {0} output pin(s).", upstreamPinIndex + 1);
      }
      try
      {
        IPin downstreamPin = DsFindPin.ByDirection(downstreamFilter, PinDirection.Input, downstreamPinIndex);
        if (downstreamPin == null)
        {
          throw new TvException("Failed to connect filters, downstream filter does not have {0} input pin(s).", downstreamPinIndex + 1);
        }
        try
        {
          int hr = graph.ConnectDirect(upstreamPin, downstreamPin, null);
          TvExceptionDirectShowError.Throw(hr, "Failed to connect filters, pins won't connect.");
        }
        finally
        {
          Release.ComObject("filter graph tools connect-filters downstream pin", ref downstreamPin);
        }
      }
      finally
      {
        Release.ComObject("filter graph tools connect-filters upstream pin", ref upstreamPin);
      }
    }

    /// <summary>
    /// Attempt to find a medium on a hardware filter's input or output pins.
    /// </summary>
    /// <param name="device">The hardware device.</param>
    /// <param name="mediums">The medium(s) to find. Any one of these is acceptable.</param>
    /// <param name="direction">The direction of the pins to check.</param>
    /// <param name="filter">The filter instance if the medium is found, otherwise <c>null</c>.</param>
    /// <param name="pin">The pin instance on which the medium is found, or <c>null</c> if the medium is not found.</param>
    /// <returns><c>true</c> if the medium is found, otherwise <c>false</c></returns>
    public static bool FindMediumOnHardwareFilter(DsDevice device, ICollection<RegPinMedium> mediums, PinDirection direction, out IBaseFilter filter, out IPin pin)
    {
      filter = null;
      pin = null;
      if (device == null || mediums == null)
      {
        throw new TvException("Failed to find medium on hardware filter, device or mediums are null.");
      }

      Guid filterIid = typeof(IBaseFilter).GUID;
      object obj;
      try
      {
        device.Mon.BindToObject(null, null, ref filterIid, out obj);
      }
      catch (Exception ex)
      {
        throw new TvException(ex, "Failed to find medium on hardware filter, can't instanciate filter from moniker.");
      }

      filter = obj as IBaseFilter;
      try
      {
        IEnumPins pinEnum;
        int hr = filter.EnumPins(out pinEnum);
        TvExceptionDirectShowError.Throw(hr, "Failed to find medium on hardware filter, can't get filter pin enumerator.");

        IPin[] pins = new IPin[2];
        int pinCount;
        while (pinEnum.Next(1, pins, out pinCount) == (int)HResult.S_OK && pinCount == 1)
        {
          IPin pinToCheck = pins[0];
          try
          {
            PinDirection d;
            hr = pinToCheck.QueryDirection(out d);
            TvExceptionDirectShowError.Throw(hr, "Failed to find medium on hardware filter, can't get pin direction.");

            if (d != direction)
            {
              continue;
            }

            ICollection<RegPinMedium> tempMediums = GetPinMediums(pinToCheck);
            foreach (RegPinMedium m1 in mediums)
            {
              foreach (RegPinMedium m2 in tempMediums)
              {
                if (m1.clsMedium == m2.clsMedium && m1.dw1 == m2.dw1 && m1.dw2 == m2.dw2)
                {
                  pin = pinToCheck;
                  return true;
                }
              }
            }
          }
          finally
          {
            if (pin == null)
            {
              Release.ComObject("filter graph tools find-medium-on-hardware-filter pin", ref pinToCheck);
            }
          }
        }

        Release.ComObject("filter graph tools find-medium-on-hardware-filter filter", ref filter);
      }
      catch
      {
        Release.ComObject("filter graph tools find-medium-on-hardware-filter filter", ref filter);
        throw;
      }
      return false;
    }

    /// <summary>
    /// Add and connect a filter for a hardware device from a given category to a DirectShow graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="pinToConnect">The upstream or downstream filter pin that <paramref name="filter"/> must connect to.</param>
    /// <param name="filterCategory">The category that the filter/device must be a member of.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="device">The device.</param>
    /// <param name="productInstanceId">A preferred device product instance identifier.</param>
    /// <param name="pinToConnectDirection">The direction - input or output - of <paramref name="pinToConnect"/>.</param>
    /// <param name="deviceBlacklist">A list of device names to ignore.</param>
    /// <returns><c>true</c> if a filter is successfully added and connected to <paramref name="pinToConnect"/>, otherwise <c>false</c></returns>
    public static bool AddAndConnectHardwareFilterByCategoryAndMedium(IFilterGraph2 graph, IPin pinToConnect, Guid filterCategory, out IBaseFilter filter, out DsDevice device, string productInstanceId = null, PinDirection pinToConnectDirection = PinDirection.Output, ICollection<string> deviceBlacklist = null)
    {
      filter = null;
      device = null;

      ICollection<RegPinMedium> mediums = GetPinMediums(pinToConnect);
      if (mediums == null || mediums.Count == 0)
      {
        throw new TvException("Failed to find medium on pin.");
      }

      PinDirection connectDirection = PinDirection.Input;
      if (pinToConnectDirection == PinDirection.Input)
      {
        connectDirection = PinDirection.Output;
      }

      DsDevice[] devices = DsDevice.GetDevicesOfCat(filterCategory);
      Log.Debug("FGT: add and connect hardware filter by category and medium, category = {0}, product instance ID = {1}, device count = {2}", filterCategory, productInstanceId ?? "[null]", devices.Length);
      try
      {
        if (!string.IsNullOrEmpty(productInstanceId))
        {
          Array.Sort(devices, delegate(DsDevice d1, DsDevice d2)
          {
            bool d1Result = productInstanceId.Equals(d1.ProductInstanceIdentifier);
            bool d2Result = productInstanceId.Equals(d2.ProductInstanceIdentifier);
            if (d1Result && !d2Result)
            {
              return -1;
            }
            if (!d1Result && d2Result)
            {
              return 1;
            }
            return 0;
          });
        }

        for (int i = 0; i < devices.Length; i++)
        {
          DsDevice d = devices[i];
          string devicePath = d.DevicePath;
          string deviceName = d.Name;
          Log.Debug("FGT:   try {0} {1} {2}", deviceName ?? "[null]", d.ProductInstanceIdentifier ?? "[null]", devicePath ?? "[null]");
          if (devicePath == null || deviceName == null || devicePath.Contains("root#system#") ||
            (productInstanceId != null && !productInstanceId.Equals(d.ProductInstanceIdentifier)) ||
            (deviceBlacklist != null && deviceBlacklist.Contains(deviceName)) ||
            !DevicesInUse.Instance.Add(d)
          )
          {
            Log.Debug("FGT:     invalid, system, different product instance, blacklisted, or in use");
            continue;
          }

          try
          {
            IPin pin2;
            if (!FilterGraphTools.FindMediumOnHardwareFilter(d, mediums, connectDirection, out filter, out pin2))
            {
              Log.Debug("FGT:     medium not found");
              continue;
            }

            try
            {
              int hr = graph.AddFilter(filter, deviceName);
              if (hr != 0x4022d)  // VFW_S_DUPLICATE_NAME
              {
                TvExceptionDirectShowError.Throw(hr, "Failed to add matching filter {0} {1} into the graph.", deviceName, devicePath);
              }
              if (pinToConnectDirection == PinDirection.Input)
              {
                hr = graph.ConnectDirect(pin2, pinToConnect, null);
              }
              else
              {
                hr = graph.ConnectDirect(pinToConnect, pin2, null);
              }
              TvExceptionDirectShowError.Throw(hr, "Failed to connect matching filter {0} {1} into the graph.", deviceName, devicePath);
            }
            catch
            {
              Release.ComObject("filter graph tools add-and-connect-hardware-filter-by-category-and-medium filter", ref filter);
              throw;
            }
            finally
            {
              Release.ComObject("filter graph tools add-and-connect-hardware-filter-by-category-and-medium pin", ref pin2);
            }

            Log.Debug("FGT:     connected!");
            device = d;
            return true;
          }
          finally
          {
            if (device == null)
            {
              DevicesInUse.Instance.Remove(d);
            }
          }
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          if (d != device)
          {
            d.Dispose();
          }
        }
      }

      return false;
    }

    /// <summary>
    /// Connect a filter pin of known direction with any one of the pins on another filter.
    /// </summary>
    /// <param name="graph">The graph containing the two filters.</param>
    /// <param name="pinToConnect">The upstream or downstream filter pin that <paramref name="filter"/> must connect to.</param>
    /// <param name="pinToConnectDirection">The direction - input or output - of <paramref name="pinToConnect"/>.</param>
    /// <param name="filter">The filter to connect to <paramref name="pinToConnect"/>.</param>
    /// <returns><c>true</c> if <paramref name="filter"/> was successfully connected to <paramref name="pinToConnect"/>, otherwise <c>false</c></returns>
    public static bool ConnectFilterWithPin(IFilterGraph2 graph, IPin pinToConnect, PinDirection pinToConnectDirection, IBaseFilter filter)
    {
      Log.Debug("FGT: connect filter with pin");
      int hr = (int)HResult.S_OK;
      int pinCount = 0;
      int pinIndex = 0;

      ICollection<RegPinMedium> mediumsPinToConnect = FilterGraphTools.GetPinMediums(pinToConnect);
      if (mediumsPinToConnect != null && mediumsPinToConnect.Count > 0)
      {
        Log.Debug("FGT:   checking for medium");
      }

      IEnumPins pinEnum = null;
      hr = filter.EnumPins(out pinEnum);
      TvExceptionDirectShowError.Throw(hr, "Failed to obtain pin enumerator for filter.");
      try
      {
        IPin[] pinsTemp = new IPin[2];
        while (pinEnum.Next(1, pinsTemp, out pinCount) == (int)HResult.S_OK && pinCount == 1)
        {
          IPin pinToTry = pinsTemp[0];
          try
          {
            // We're not interested in pins unless they're the right direction.
            PinDirection direction;
            hr = pinToTry.QueryDirection(out direction);
            TvExceptionDirectShowError.Throw(hr, "Failed to query pin direction for filter pin.");
            if (direction == pinToConnectDirection)
            {
              Log.Debug("FGT:   pin {0} is the wrong direction", pinIndex++);
              continue;
            }

            // We can't use pins that are already connected.
            IPin tempPin = null;
            hr = pinToTry.ConnectedTo(out tempPin);
            if (hr == (int)HResult.S_OK && tempPin != null)
            {
              Log.Debug("FGT:   pin {0} is already connected", pinIndex++);
              Release.ComObject("filter graph tools connect-filter-with-pin filter connected pin", ref tempPin);
              continue;
            }

            // Check for the required medium.
            if (mediumsPinToConnect != null && mediumsPinToConnect.Count > 0)
            {
              bool foundMedium = false;
              ICollection<RegPinMedium> mediumsPinToTry = FilterGraphTools.GetPinMediums(pinToTry);
              if (mediumsPinToTry != null)
              {
                foreach (RegPinMedium m1 in mediumsPinToConnect)
                {
                  foreach (RegPinMedium m2 in mediumsPinToTry)
                  {
                    if (m1.clsMedium == m2.clsMedium && m1.dw1 == m2.dw1 && m1.dw2 == m2.dw2)
                    {
                      foundMedium = true;
                      break;
                    }
                  }
                  if (foundMedium)
                  {
                    break;
                  }
                }
              }
              if (!foundMedium)
              {
                Log.Debug("FGT:   pin {0} doesn't have the required medium", pinIndex++);
                continue;
              }
            }

            try
            {
              if (pinToConnectDirection == PinDirection.Input)
              {
                hr = graph.ConnectDirect(pinToTry, pinToConnect, null);
              }
              else
              {
                hr = graph.ConnectDirect(pinToConnect, pinToTry, null);
              }
              TvExceptionDirectShowError.Throw(hr, "Failed to connect pins.");
              Log.Debug("FGT:   pin {0} connected!", pinIndex);
              return true;
            }
            catch
            {
              // Connection failed, move on to next upstream pin.
            }
            pinIndex++;
          }
          finally
          {
            Release.ComObject("filter graph tools connect-filter-with-pin filter pin", ref pinToTry);
          }
        }
      }
      finally
      {
        Release.ComObject("filter graph tools connect-filter-with-pin filter pin enumerator", ref pinEnum);
      }

      return false;
    }

    #region Unmanaged Code declarations

    [Flags]
    internal enum STGM
    {
      Read = 0x00000000,
      Write = 0x00000001,
      ReadWrite = 0x00000002,
      ShareDenyNone = 0x00000040,
      ShareDenyRead = 0x00000030,
      ShareDenyWrite = 0x00000020,
      ShareExclusive = 0x00000010,
      Priority = 0x00040000,
      Create = 0x00001000,
      Convert = 0x00020000,
      FailIfThere = 0x00000000,
      Direct = 0x00000000,
      Transacted = 0x00010000,
      NoScratch = 0x00100000,
      NoSnapShot = 0x00200000,
      Simple = 0x08000000,
      DirectSWMR = 0x00400000,
      DeleteOnRelease = 0x04000000,
    }

    [Flags]
    internal enum STGC
    {
      Default = 0,
      Overwrite = 1,
      OnlyIfCurrent = 2,
      DangerouslyCommitMerelyToDiskCache = 4,
      Consolidate = 8
    }

    [Guid("0000000b-0000-0000-C000-000000000046"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IStorage
    {
      [PreserveSig]
      int CreateStream(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
        [In] STGM grfMode,
        [In] int reserved1,
        [In] int reserved2,
#if USING_NET11
			[Out] out UCOMIStream ppstm
#else
 [Out] out IStream ppstm
#endif
);

      [PreserveSig]
      int OpenStream(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
        [In] IntPtr reserved1,
        [In] STGM grfMode,
        [In] int reserved2,
#if USING_NET11
			[Out] out UCOMIStream ppstm
#else
 [Out] out IStream ppstm
#endif
);

      [PreserveSig]
      int CreateStorage(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
        [In] STGM grfMode,
        [In] int reserved1,
        [In] int reserved2,
        [Out] out IStorage ppstg
        );

      [PreserveSig]
      int OpenStorage(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
        [In] IStorage pstgPriority,
        [In] STGM grfMode,
        [In] int snbExclude,
        [In] int reserved,
        [Out] out IStorage ppstg
        );

      [PreserveSig]
      int CopyTo(
        [In] int ciidExclude,
        [In] Guid[] rgiidExclude,
        [In] string[] snbExclude,
        [In] IStorage pstgDest
        );

      [PreserveSig]
      int MoveElementTo(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
        [In] IStorage pstgDest,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsNewName,
        [In] STGM grfFlags
        );

      [PreserveSig]
      int Commit([In] STGC grfCommitFlags);

      [PreserveSig]
      int Revert();

      [PreserveSig]
      int EnumElements(
        [In] int reserved1,
        [In] IntPtr reserved2,
        [In] int reserved3,
        [Out, MarshalAs(UnmanagedType.Interface)] out object ppenum
        );

      [PreserveSig]
      int DestroyElement([In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName);

      [PreserveSig]
      int RenameElement(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsOldName,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsNewName
        );

      [PreserveSig]
      int SetElementTimes(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
#if USING_NET11
			[In] FILETIME pctime,
			[In] FILETIME patime,
			[In] FILETIME pmtime
#else
 [In] System.Runtime.InteropServices.ComTypes.FILETIME pctime,
 [In] System.Runtime.InteropServices.ComTypes.FILETIME patime,
 [In] System.Runtime.InteropServices.ComTypes.FILETIME pmtime
#endif
);

      [PreserveSig]
      int SetClass([In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid);

      [PreserveSig]
      int SetStateBits(
        [In] int grfStateBits,
        [In] int grfMask
        );

      [PreserveSig]
      int Stat(
#if USING_NET11
			[Out] out STATSTG pStatStg,
#else
[Out] out System.Runtime.InteropServices.ComTypes.STATSTG pStatStg,
#endif
 [In] int grfStatFlag
 );
    }

    private static class NativeMethods
    {
      [DllImport("ole32.dll")]
#if USING_NET11
		public static extern int CreateBindCtx(int reserved, out UCOMIBindCtx ppbc);
#else
      public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);
#endif

      [DllImport("ole32.dll")]
#if USING_NET11
		public static extern int MkParseDisplayName(UCOMIBindCtx pcb, [MarshalAs(UnmanagedType.LPWStr)] string szUserName, out int pchEaten, out UCOMIMoniker ppmk);
#else
      public static extern int MkParseDisplayName(IBindCtx pcb, [MarshalAs(UnmanagedType.LPWStr)] string szUserName,
                                                  out int pchEaten, out IMoniker ppmk);
#endif

      [DllImport("olepro32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
      public static extern int OleCreatePropertyFrame(
        [In] IntPtr hwndOwner,
        [In] int x,
        [In] int y,
        [In, MarshalAs(UnmanagedType.LPWStr)] string lpszCaption,
        [In] int cObjects,
        [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown)] object[] ppUnk,
        [In] int cPages,
        [In] IntPtr pPageClsID,
        [In] int lcid,
        [In] int dwReserved,
        [In] IntPtr pvReserved
        );

      [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
      public static extern int StgCreateDocfile(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
        [In] STGM grfMode,
        [In] int reserved,
        [Out] out IStorage ppstgOpen
        );

      [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
      public static extern int StgIsStorageFile([In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName);

      [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
      public static extern int StgOpenStorage(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwcsName,
        [In] IStorage pstgPriority,
        [In] STGM grfMode,
        [In] IntPtr snbExclude,
        [In] int reserved,
        [Out] out IStorage ppstgOpen
        );
    }

    #endregion
  }
}