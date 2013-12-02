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
using System.Security.Permissions;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TvLibrary.Utils.Util;

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
    /// <param name="clsid">The class ID (CLSID) for the filter class. The class must expose the IBaseFilter interface.</param>
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
        HResult.ThrowException(hr, "Failed to add the new filter to the graph.");
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
    /// <param name="clsid">The class ID (CLSID) for the filter class. The class must expose the IBaseFilter interface.</param>
    /// <param name="filterName">The name or label to use for the filter.</param>
    /// <returns>the instance of the filter if the method successfully created it, otherwise <c>null</c></returns>
    public static IBaseFilter AddFilterFromFile(IFilterGraph2 graph, string fileName, Guid clsid, string filterName)
    {
      IBaseFilter filter = null;
      try
      {
        filter = ComHelper.LoadComObjectFromFile(fileName, clsid, typeof(IBaseFilter).GUID) as IBaseFilter;

        int hr = graph.AddFilter(filter, filterName);
        HResult.ThrowException(hr, "Failed to add the new filter to the graph.");
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
    /// <param name="device">The device.</param>
    /// <param name="name">The name or label to use for the filter.</param>
    /// <returns>the instance of the filter if the method successfully created it, otherwise <c>null</c></returns>
    public static IBaseFilter AddFilterFromDevice(IFilterGraph2 graph, DsDevice device)
    {
      if (device == null || device.Mon == null)
      {
        throw new TvException("Failed to add filter by device, device or moniker is null.");
      }

      IBaseFilter filter = null;
      try
      {
        int hr = graph.AddSourceFilterForMoniker(device.Mon, null, device.Name, out filter);
        HResult.ThrowException(hr, "Failed to add the new filter to the graph.");
      }
      catch
      {
        Release.ComObject("filter graph tools add-filter-from-device filter", ref filter);
        throw;
      }
      return filter;
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
        int hr = NativeMethods.StgCreateDocfile(
          Path.Combine(PathManager.GetDataPath, FileUtils.MakeFileName(fileName)),
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
      catch { }
      return retval;
    }

    /// <summary>
    /// increment the ComObject referencecount of the object.
    /// </summary>
    /// <param name="o">The object.</param>
    private static object incRefCountCOM(object o)
    {
      IntPtr pUnk = Marshal.GetIUnknownForObject(o);
      object oCom = Marshal.GetObjectForIUnknown(pUnk);
      Marshal.Release(pUnk);
      return oCom;
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
      HResult.ThrowException(hr, "Failed to query filter information.");
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
      HResult.ThrowException(hr, "Failed to query pin information.");
      Release.ComObject("filter graph tools pin name filter", ref pinInfo.filter);
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
    public static void AddAndConnectFilterIntoGraph(IFilterGraph2 graph, IBaseFilter newFilter, string filterName, IBaseFilter upstreamFilter, int upstreamFilterOutputPinIndex, int newFilterInputPinIndex)
    {
      if (upstreamFilter == null || newFilter == null)
      {
        throw new TvException("Failed to add and connect filter, upstream or new filter are null.");
      }
      if (upstreamFilterOutputPinIndex < 0 || newFilterInputPinIndex < 0)
      {
        throw new TvException("Failed to add and connect filter, one or both pin indicies are invalid.");
      }

      int hr = graph.AddFilter(newFilter, filterName);
      HResult.ThrowException(hr, "Failed to add the new filter to the graph.");

      IPin upstreamOutputPin = DsFindPin.ByDirection(upstreamFilter, PinDirection.Output, upstreamFilterOutputPinIndex);
      try
      {
        IPin newFilterInputPin = DsFindPin.ByDirection(newFilter, PinDirection.Input, newFilterInputPinIndex);
        try
        {
          hr = graph.ConnectDirect(upstreamOutputPin, newFilterInputPin, null);
          HResult.ThrowException(hr, "Failed to connect the new filter into the graph.");
        }
        finally
        {
          Release.ComObject("filter graph tools add-and-connect-filter-into-graph new filter input pin", ref newFilterInputPin);
        }
      }
      catch
      {
        graph.RemoveFilter(newFilter);
        throw;
      }
      finally
      {
        Release.ComObject("filter graph tools add-and-connect-filter-into-graph upstream filter output pin", ref upstreamOutputPin);
      }
    }

    /// <summary>
    /// Add and connect a filter into a DirectShow graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="newFilter">The filter to add and connect.</param>
    /// <param name="filterName">The name or label to use for the filter.</param>
    /// <param name="upstreamFilter">The upstream filter to connect the new filter to. This filter should already be present in the graph.</param>
    /// <param name="graphBuilder">The graph builder, used to render-connect the filters.</param>
    public static void AddAndConnectFilterIntoGraph(IFilterGraph2 graph, IBaseFilter newFilter, string filterName, IBaseFilter upstreamFilter, ICaptureGraphBuilder2 graphBuilder)
    {
      if (upstreamFilter == null || newFilter == null)
      {
        throw new TvException("Failed to add and connect filter, upstream or new filter are null.");
      }

      int hr = graph.AddFilter(newFilter, filterName);
      HResult.ThrowException(hr, "Failed to add the new filter to the graph.");

      try
      {
        hr = graphBuilder.RenderStream(null, null, upstreamFilter, null, newFilter);
        HResult.ThrowException(hr, "Failed to render into the new filter.");
      }
      catch
      {
        graph.RemoveFilter(newFilter);
        throw;
      }
    }

    /// <summary>
    /// Add and connect a filter into a DirectShow graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="device">A DsDevice instance that wraps an IMoniker instance. The filter to add and connect will be instantiated from the IMoniker.</param>
    /// <param name="upstreamFilter">The upstream filter to connect the new filter to. This filter should already be present in the graph.</param>
    /// <param name="graphBuilder">The graph builder, used to render-connect the filters.</param>
    /// <remarks>the filter that was instanciated, added to, and connected into the graph</remarks>
    public static IBaseFilter AddAndConnectFilterIntoGraph(IFilterGraph2 graph, DsDevice device, IBaseFilter upstreamFilter, ICaptureGraphBuilder2 graphBuilder)
    {
      if (upstreamFilter == null || device == null || device.Mon == null)
      {
        throw new TvException("Failed to add and connect filter, upstream filter or moniker are null.");
      }

      IBaseFilter newFilter = AddFilterFromDevice(graph, device);

      try
      {
        int hr = graphBuilder.RenderStream(null, null, upstreamFilter, null, newFilter);
        HResult.ThrowException(hr, "Failed to render into the new filter.");
      }
      catch
      {
        graph.RemoveFilter(newFilter);
        Release.ComObject("filter graph tools add-and-connect-filter-into-graph new filter", ref newFilter);
        throw;
      }
      return newFilter;
    }
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

  internal static class NativeMethods
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