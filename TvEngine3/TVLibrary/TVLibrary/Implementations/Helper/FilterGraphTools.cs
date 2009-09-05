/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;

using DirectShowLib;

#if !USING_NET11
using System.Runtime.InteropServices.ComTypes;
#endif

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A collection of methods to do common DirectShow tasks.
  /// </summary>

  public static class FilterGraphTools
  {
    #region structs
    static readonly byte[] Mpeg2ProgramVideo = 
    {
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.left
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.top
          0xd0, 0x02, 0x00, 0x00,							//  .hdr.rcSource.right
          0x40, 0x02, 0x00, 0x00,							//  .hdr.rcSource.bottom
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.left
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.top
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.right
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.bottom
          0xc0, 0xe1, 0xe4, 0x00,							//  .hdr.dwBitRate
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwBitErrorRate
          0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwInterlaceFlags
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwCopyProtectFlags
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioX
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioY
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved1
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved2
          0x28, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSize
          0xd0, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biWidth
          0x40, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biHeight
          0x00, 0x00,										//  .hdr.bmiHeader.biPlanes
          0x00, 0x00,										//  .hdr.bmiHeader.biBitCount
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biCompression
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSizeImage
          0xd0, 0x07, 0x00, 0x00,							//  .hdr.bmiHeader.biXPelsPerMeter
          0x42, 0xd8, 0x00, 0x00,							//  .hdr.bmiHeader.biYPelsPerMeter
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrUsed
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrImportant
          0x00, 0x00, 0x00, 0x00,							//  .dwStartTimeCode
          0x4c, 0x00, 0x00, 0x00,							//  .cbSequenceHeader
          0x00, 0x00, 0x00, 0x00,							//  .dwProfile
          0x00, 0x00, 0x00, 0x00,							//  .dwLevel
          0x00, 0x00, 0x00, 0x00,							//  .Flags
					                        //  .dwSequenceHeader [1]
          0x00, 0x00, 0x01, 0xb3, 0x2d, 0x02, 0x40, 0x33, 
          0x24, 0x9f, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
          0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
          0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
          0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
          0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
          0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
          0x1a, 0x1a, 0x1a, 0x1a, 0x19, 0x1b, 0x1b, 0x1b, 
          0x1b, 0x1b, 0x1c, 0x1c, 0x1c, 0x1c, 0x1e, 0x1e, 
          0x1e, 0x1f, 0x1f, 0x21, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

#pragma warning disable 169
    static byte[] H264VideoFormat = {
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcSource.left              = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcSource.top               = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcSource.right             = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcSource.bottom            = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcTarget.left              = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcTarget.top               = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcTarget.right             = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.rcTarget.bottom            = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.dwBitRate                  = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.dwBitErrorRate             = 0x00000000
    //	0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame            = 0x0000000000061a80
	    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame            = 0x0000000000061a80
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biSize           = 0x00000028
	    0xD0, 0x02, 0x00, 0x00,                         //  .hdr.bmiHeader.biWidth          = 0x000002d0
	    0x40, 0x02, 0x00, 0x00,                         //  .hdr.bmiHeader.biHeight         = 0x00000240
	    0x00, 0x00,                                     //  .hdr.bmiHeader.biPlanes         = 0x0001
	    0x00, 0x00,                                     //  .hdr.bmiHeader.biBitCount       = 0x0018
	    0x48, 0x32, 0x36, 0x34,                         //  .hdr.bmiHeader.biCompression    = "H264"
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biSizeImage      = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biXPelsPerMeter  = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biYPelsPerMeter  = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biClrUsed        = 0x00000000
	    0x00, 0x00, 0x00, 0x00,                         //  .hdr.bmiHeader.biClrImportant   = 0x00000000
    };
#pragma warning restore 169

    static readonly byte[] MPEG2AudioFormat =
      {	
        0x50, 0x00,				//wFormatTag
	      0x02, 0x00,				//nChannels
	      0x80, 0xbb, 0x00, 0x00, //nSamplesPerSec
	      0x00, 0x7d, 0x00, 0x00, //nAvgBytesPerSec
	      0x01, 0x00,				//nBlockAlign
	      0x00, 0x00,				//wBitsPerSample
	      0x16, 0x00,				//cbSize
	      0x02, 0x00,				//wValidBitsPerSample
	      0x00, 0xe8,				//wSamplesPerBlock
	      0x03, 0x00,				//wReserved
	      0x01, 0x00, 0x01, 0x00, //dwChannelMask
	      0x01, 0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      };


    static readonly byte[] MPEG1AudioFormat = 
      {
	      0x50, 0x00,				//wFormatTag
	      0x02, 0x00,				//nChannels
	      0x80, 0xBB,	0x00, 0x00, //nSamplesPerSec
	      0x00, 0x7D,	0x00, 0x00, //nAvgBytesPerSec
	      0x00, 0x03,				//nBlockAlign
	      0x00, 0x00,				//wBitsPerSample
	      0x16, 0x00,				//cbSize
	      0x02, 0x00,				//wValidBitsPerSample
	      0x00, 0xE8,				//wSamplesPerBlock
	      0x03, 0x00,				//wReserved
	      0x01, 0x00,	0x01,0x00,  //dwChannelMask
	      0x01, 0x00,	0x1C, 0x00, 0x00, 0x00,	0x00, 0x00, 0x00, 0x00, 0x00, 0x00

      };
    static readonly byte[] LPCMAudioFormat =
        {
          0x00, 0x00,             // format type      = 0x0000=WAVE_FORMAT_UNKNOWN
          0x02, 0x00,             // channels
          0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
          0x00, 0x7D, 0x00, 0x00, // nAvgBytesPerSec  = 0x00007d00=32000
          0x00, 0x03,             // nBlockAlign      = 0x0300 = 768
          0x10, 0x00,             // wBitsPerSample   = 16
          0x16, 0x00,             // extra size       = 0x0016 = 22 bytes
          };

#pragma warning disable 169
    static byte[] AC3AudioFormat = {
      0x00, 0x20,				//wFormatTag
      0x06, 0x00,				//nChannels
      0x80, 0xBB, 0x00, 0x00, //nSamplesPerSec
      0xC0, 0x5D, 0x00, 0x00, //nAvgBytesPerSec
      0x00, 0x03,				//nBlockAlign
      0x00, 0x00,				//wBitsPerSample
      0x00, 0x00				//cbSize
    };
#pragma warning restore 169

    static readonly byte[] AACAudioFormat = {
      0xFF, 0x00,				//wFormatTag
      0x02, 0x00,				//nChannels
      0x80, 0xBB, 0x00, 0x00, //nSamplesPerSec
      0xCE, 0x3E, 0x00, 0x00, //nAvgBytesPerSec
      0xAE, 0x02,				//nBlockAlign
      0x00, 0x00,				//wBitsPerSample
      0x02, 0x00,				//cbSize
      0x11, 0x90
    };

#pragma warning disable 169
    static byte[] AACAudioFormat2 = {
	    0xFF, 0x00,				//wFormatTag
	    0x02, 0x00,				//nChannels
	    0x80, 0xBB, 0x00, 0x00, //nSamplesPerSec
	    0x9F, 0x24, 0x00, 0x00, //nAvgBytesPerSec
	    0x90, 0x01,				//nBlockAlign
	    0x00, 0x00,				//wBitsPerSample
	    0x02, 0x00,				//cbSize
	    0x11, 0x90
    };
#pragma warning restore 169

    #endregion

    /// <summary>
    /// Add a filter to a DirectShow Graph using its CLSID
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="clsid">a valid CLSID. This object must implement IBaseFilter</param>
    /// <param name="name">the name used in the graph (may be null)</param>
    /// <returns>an instance of the filter if the method successfully created it, null if not</returns>
    /// <remarks>
    /// You can use <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> to check is the CLSID is valid before calling this method
    /// </remarks>
    /// <example>This sample shows how to programmatically add a NVIDIA Video decoder filter to a graph
    /// <code>
    /// Guid nvidiaVideoDecClsid = new Guid("71E4616A-DB5E-452B-8CA5-71D9CC7805E9");
    /// 
    /// if (FilterGraphTools.IsThisComObjectInstalled(nvidiaVideoDecClsid))
    /// {
    ///   filter = FilterGraphTools.AddFilterFromClsid(graphBuilder, nvidiaVideoDecClsid, "NVIDIA Video Decoder");
    /// }
    /// else
    /// {
    ///   // use another filter...
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IsThisComObjectInstalled"/>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur when the filter is add to the graph</exception>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static IBaseFilter AddFilterFromClsid(IGraphBuilder graphBuilder, Guid clsid, string name)
    {
      IBaseFilter filter = null;

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      try
      {
        Type type = Type.GetTypeFromCLSID(clsid);
        filter = (IBaseFilter)Activator.CreateInstance(type);

        int hr = graphBuilder.AddFilter(filter, name);
        DsError.ThrowExceptionForHR(hr);
      } catch
      {
        if (filter != null)
        {
          Marshal.ReleaseComObject(filter);
          filter = null;
        }
      }

      return filter;
    }

    /// <summary>
    /// Add a filter to a DirectShow Graph using its name
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="deviceCategory">the filter category (see DirectShowLib.FilterCategory)</param>
    /// <param name="friendlyName">the filter name (case-sensitive)</param>
    /// <returns>an instance of the filter if the method successfully created it, null if not</returns>
    /// <example>This sample shows how to programmatically add a NVIDIA Video decoder filter to a graph
    /// <code>
    /// filter = FilterGraphTools.AddFilterByName(graphBuilder, FilterCategory.LegacyAmFilterCategory, "NVIDIA Video Decoder");
    /// </code>
    /// </example>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur when the filter is add to the graph</exception>
    public static IBaseFilter AddFilterByName(IGraphBuilder graphBuilder, Guid deviceCategory, string friendlyName)
    {
      IBaseFilter filter = null;

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      DsDevice[] devices = DsDevice.GetDevicesOfCat(deviceCategory);

      for (int i = 0; i < devices.Length; i++)
      {
        if (!devices[i].Name.Equals(friendlyName))
          continue;

        int hr = ((IFilterGraph2)graphBuilder).AddSourceFilterForMoniker(devices[i].Mon, null, friendlyName, out filter);
        DsError.ThrowExceptionForHR(hr);

        break;
      }

      return filter;
    }

    /// <summary>
    /// Add a filter to a DirectShow Graph using its Moniker's device path
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="devicePath">a moniker path</param>
    /// <param name="name">the name to use for the filter in the graph</param>
    /// <returns>an instance of the filter if the method successfully creates it, null if not</returns>
    /// <example>This sample shows how to programmatically add a NVIDIA Video decoder filter to a graph
    /// <code>
    /// string devicePath = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{71E4616A-DB5E-452B-8CA5-71D9CC7805E9}";
    /// filter = FilterGraphTools.AddFilterByDevicePath(graphBuilder, devicePath, "NVIDIA Video Decoder");
    /// </code>
    /// </example>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occur when the filter is add to the graph</exception>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static IBaseFilter AddFilterByDevicePath(IGraphBuilder graphBuilder, string devicePath, string name)
    {
      IBaseFilter filter = null;
#if USING_NET11
			UCOMIBindCtx bindCtx = null;
			UCOMIMoniker moniker = null;
#else
      IBindCtx bindCtx = null;
      IMoniker moniker = null;
#endif

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      try
      {
        int hr = NativeMethods.CreateBindCtx(0, out bindCtx);
        Marshal.ThrowExceptionForHR(hr);
        int eaten;

        hr = NativeMethods.MkParseDisplayName(bindCtx, devicePath, out eaten, out moniker);
        Marshal.ThrowExceptionForHR(hr);

        hr = ((IFilterGraph2)graphBuilder).AddSourceFilterForMoniker(moniker, bindCtx, name, out filter);
        DsError.ThrowExceptionForHR(hr);
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Error occured while adding filter by device path: ", ex);
        // An error occur. Just returning null...
      }
      finally
      {
        if (bindCtx != null)
          Marshal.ReleaseComObject(bindCtx);
        if (moniker != null)
          Marshal.ReleaseComObject(moniker);
      }

      return filter;
    }

    /// <summary>
    /// Find a filter in a DirectShow Graph using its name
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="filterName">the filter name to find (case-sensitive)</param>
    /// <returns>an instance of the filter if found, null if not</returns>
    /// <seealso cref="FindFilterByClsid"/>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static IBaseFilter FindFilterByName(IGraphBuilder graphBuilder, string filterName)
    {

      IBaseFilter filter = null;
      IEnumFilters enumFilters;

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      int hr = graphBuilder.EnumFilters(out enumFilters);
      if (hr == 0)
      {
        IBaseFilter[] filters = new IBaseFilter[1];
        int fetched;

        while (enumFilters.Next(filters.Length, filters, out fetched) == 0)
        {
          if (GetFilterName(filters[0]).Equals(filterName))
          {
            filter = filters[0];
            break;
          }
        }
        Marshal.ReleaseComObject(enumFilters);
      }

      return filter;
    }

    /// <summary>
    /// Find a filter in a DirectShow Graph using its CLSID
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="filterClsid">the CLSID to find</param>
    /// <returns>an instance of the filter if found, null if not</returns>
    /// <seealso cref="FindFilterByName"/>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static IBaseFilter FindFilterByClsid(IGraphBuilder graphBuilder, Guid filterClsid)
    {
      IBaseFilter filter = null;
      IEnumFilters enumFilters;

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      int hr = graphBuilder.EnumFilters(out enumFilters);
      if (hr == 0)
      {
        IBaseFilter[] filters = new IBaseFilter[1];
        int fetched;

        while (enumFilters.Next(filters.Length, filters, out fetched) == 0)
        {
          Guid clsid;

          hr = filters[0].GetClassID(out clsid);

          if ((hr == 0) && (clsid == filterClsid))
          {
            filter = filters[0];
            break;
          }

          Marshal.ReleaseComObject(filters[0]);
        }
        Marshal.ReleaseComObject(enumFilters);
      }

      return filter;
    }

    /// <summary>
    /// Render a filter's pin in a DirectShow Graph
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="source">the filter containing the pin to render</param>
    /// <param name="pinName">the pin name</param>
    /// <returns>true if rendering is a success, false if not</returns>
    /// <example>
    /// <code>
    /// hr = graphBuilder.AddSourceFilter(@"foo.avi", "Source Filter", out filter);
    /// DsError.ThrowExceptionForHR(hr);
    /// 
    /// if (!FilterGraphTools.RenderPin(graphBuilder, filter, "Output"))
    /// {
    ///   // Something went wrong...
    /// }
    /// </code>
    /// </example>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder or source is null</exception>
    /// <remarks>This method assumes that the filter is part of the given graph</remarks>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static bool RenderPin(IGraphBuilder graphBuilder, IBaseFilter source, string pinName)
    {

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      if (source == null)
        throw new ArgumentNullException("source");

      IPin pin = DsFindPin.ByName(source, pinName);

      if (pin != null)
      {
        int hr = graphBuilder.Render(pin);
        Marshal.ReleaseComObject(pin);

        return (hr >= 0);
      }

      return false;
    }

    /// <summary>
    /// Find a pin on the filter specified
    /// which can supplies the mediatype and mediasubtype specified
    /// if found the pin is returned
    /// </summary>
    /// <param name="filter">The filter to find the pin on.</param>
    /// <param name="mediaType">Type of the media.</param>
    /// <param name="mediaSubtype">The media subtype.</param>
    /// <param name="direction">The direction of the pin</param>
    /// <param name="index">Index of the pin</param>
    public static IPin FindMediaPin(IBaseFilter filter, Guid mediaType, Guid mediaSubtype, PinDirection direction, out int index)
    {

      IEnumPins enumPins;
      filter.EnumPins(out enumPins);
      // loop through all pins
      index = -1;
      while (true)
      {
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1)
          break;
        //first check if the pindirection matches
        PinDirection pinDirection;
        pins[0].QueryDirection(out pinDirection);
        if (pinDirection != direction)
          continue;
        index++;
        //next check if the pin supports the media type requested
        IEnumMediaTypes enumMedia;
        AMMediaType[] media = new AMMediaType[2];
        pins[0].EnumMediaTypes(out enumMedia);
        while (true)
        {
          int fetchedMedia;
          enumMedia.Next(1, media, out fetchedMedia);
          if (fetchedMedia != 1)
            break;

          if (media[0].majorType == mediaType)
          {
            if (media[0].subType == mediaSubtype || mediaSubtype == MediaSubType.Null)
            {
              DsUtils.FreeAMMediaType(media[0]);
              return pins[0];
            }
          }
          DsUtils.FreeAMMediaType(media[0]);
        }
        Release.ComObject("Find media pin", pins[0]);
      }
      return null;
    }

    /// <summary>
    /// Scans a filter's pins looking for a pin with the specified name
    /// </summary>
    /// <param name="vSource">The filter to scan</param>
    /// <param name="vPinName">The pin name to find</param>
    /// <param name="vDir">The direction of the pin</param>
    /// <param name="pinIndex">The index of the pin</param>
    /// <returns>The matching pin, or null if not found</returns>
    public static IPin GetPinByName(IBaseFilter vSource, string vPinName, PinDirection vDir, out int pinIndex)
    {
      IEnumPins ppEnum;
      PinInfo ppinfo;
      IPin pRet = null;
      IPin[] pPins = new IPin[1];
      pinIndex = -1;
      if (vSource == null)
      {
        return null;
      }

      // Get the pin enumerator
      int hr = vSource.EnumPins(out ppEnum);
      DsError.ThrowExceptionForHR(hr);

      try
      {
        int lFetched;
        // Walk the pins looking for a match
        while ((ppEnum.Next(1, pPins, out lFetched) >= 0) && (lFetched == 1))
        {
          PinDirection ppindir;
          // Read the info
          hr = pPins[0].QueryPinInfo(out ppinfo);
          DsError.ThrowExceptionForHR(hr);

          hr = pPins[0].QueryDirection(out ppindir);
          DsError.ThrowExceptionForHR(hr);

          // Is it the right direction?
          if (ppindir == vDir)
          {
            pinIndex++;
            // Is it the right name?
            if (ppinfo.name == vPinName)
            {
              DsUtils.FreePinInfo(ppinfo);
              pRet = pPins[0];
              break;
            }
          }
          Marshal.ReleaseComObject(pPins[0]);
          DsUtils.FreePinInfo(ppinfo);
        }
      }
      finally
      {
        Marshal.ReleaseComObject(ppEnum);
      }

      return pRet;
    }

    /// <summary>
    /// Scan's a filter's pins looking for a pin with the specified category and direction
    /// </summary>
    /// <param name="vSource">The filter to scan</param>
    /// <param name="PinCategory">The guid from PinCategory to scan for</param>
    /// <param name="iIndex">Zero based index (ie 2 will return the third pin of the specified category)</param>
    /// <param name="vDir">The direction of the pin</param>
    /// <param name="pinIndex">The index of the pin</param>
    /// <returns>The matching pin, or null if not found</returns>
    public static IPin GetPinByCategoryAndDirection(IBaseFilter vSource, Guid PinCategory, int iIndex, PinDirection vDir, out int pinIndex)
    {
      IEnumPins ppEnum;
      IPin pRet = null;
      IPin[] pPins = new IPin[1];
      pinIndex = -1;
      if (vSource == null)
      {
        return null;
      }

      // Get the pin enumerator
      int hr = vSource.EnumPins(out ppEnum);
      DsError.ThrowExceptionForHR(hr);

      try
      {
        int lFetched;
        // Walk the pins looking for a match
        while ((ppEnum.Next(1, pPins, out lFetched) >= 0) && (lFetched == 1))
        {
          PinDirection ppindir;
          // Read the direction
          hr = pPins[0].QueryDirection(out ppindir);
          DsError.ThrowExceptionForHR(hr);

          if (ppindir == vDir)
          {
            pinIndex++;
            // Is it the right category?
            if (DsUtils.GetPinCategory(pPins[0]) == PinCategory)
            {
              // Is is the right index?
              if (iIndex == 0)
              {
                pRet = pPins[0];
                break;
              }
              iIndex--;
            }
          }
          Marshal.ReleaseComObject(pPins[0]);
        }
      }
      finally
      {
        Marshal.ReleaseComObject(ppEnum);
      }

      return pRet;
    }

    /// <summary>
    /// Disconnect all pins on a given filter
    /// </summary>
    /// <param name="filter">the filter on which to disconnect all the pins</param>
    /// <exception cref="System.ArgumentNullException">Thrown if filter is null</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if errors occured during the disconnection process</exception>
    /// <remarks>Both input and output pins are disconnected</remarks>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static void DisconnectPins(IBaseFilter filter)
    {

      if (filter == null)
        throw new ArgumentNullException("filter");

      IEnumPins enumPins;
      IPin[] pins = new IPin[1];
      int hr = filter.EnumPins(out enumPins);
      DsError.ThrowExceptionForHR(hr);

      try
      {
        int fetched;
        while (enumPins.Next(pins.Length, pins, out fetched) == 0)
        {
          try
          {
            hr = pins[0].Disconnect();
            DsError.ThrowExceptionForHR(hr);
          }
          finally
          {
            Marshal.ReleaseComObject(pins[0]);
          }
        }
      }
      finally
      {
        Marshal.ReleaseComObject(enumPins);
      }
    }

    /// <summary>
    /// Disconnect pins of all the filters in a DirectShow Graph
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if the method can't enumerate its filters</exception>
    /// <remarks>This method doesn't throw an exception if an error occurs during pin disconnections</remarks>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static void DisconnectAllPins(IGraphBuilder graphBuilder)
    {
      IEnumFilters enumFilters;

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      int hr = graphBuilder.EnumFilters(out enumFilters);
      DsError.ThrowExceptionForHR(hr);

      try
      {
        IBaseFilter[] filters = new IBaseFilter[1];
        int fetched;

        while (enumFilters.Next(filters.Length, filters, out fetched) == 0)
        {
          try
          {
            DisconnectPins(filters[0]);
          } catch (Exception ex) { Log.Log.WriteFile("Error while disconnecting all pins: ", ex); }
          Marshal.ReleaseComObject(filters[0]);
        }
      }
      finally
      {
        Marshal.ReleaseComObject(enumFilters);
      }
    }

    /// <summary>
    /// Remove and release all filters from a DirectShow Graph
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder is null</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if the method can't enumerate its filters</exception>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static void RemoveAllFilters(IGraphBuilder graphBuilder)
    {
      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      IEnumFilters enumFilters = null;
      System.Collections.ArrayList filtersArray = new System.Collections.ArrayList();
      
      try
      {
        int hr = graphBuilder.EnumFilters(out enumFilters);
        DsError.ThrowExceptionForHR(hr);

        IBaseFilter[] filters = new IBaseFilter[1];
        int fetched;

        while (enumFilters.Next(filters.Length, filters, out fetched) == 0)
        {
          filtersArray.Add(filters[0]);
        }
        foreach (IBaseFilter filter in filtersArray)
        {
          FilterInfo info;
          filter.QueryFilterInfo(out info);
          Log.Log.Write("Remove filter from graph: {0}", info.achName);
          graphBuilder.RemoveFilter(filter);
          while(Marshal.ReleaseComObject(filter) > 0) ;
        }
      }
      catch (Exception)
      {
        Log.Log.Write("Remove filter error!");
        return;
      }
      finally
      {
        if (enumFilters != null)
        {
          Marshal.ReleaseComObject(enumFilters);
        }
      }      
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
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
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
            fileName,
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
        if (stream != null)
          Marshal.ReleaseComObject(stream);
        if (storage != null)
          Marshal.ReleaseComObject(storage);
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
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
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
        if (stream != null)
          Marshal.ReleaseComObject(stream);
        if (storage != null)
          Marshal.ReleaseComObject(storage);
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
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
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

        if (filterInfo.pGraph != null)
          Marshal.ReleaseComObject(filterInfo.pGraph);

        hr = ((ISpecifyPropertyPages)filter).GetPages(out caGuid);
        DsError.ThrowExceptionForHR(hr);

        try
        {
          objs = new object[1];
          objs[0] = filter;

          NativeMethods.OleCreatePropertyFrame(
              parent, 0, 0,
              filterInfo.achName,
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
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static bool IsThisComObjectInstalled(Guid clsid)
    {
      bool retval = false;

      try
      {
        Type type = Type.GetTypeFromCLSID(clsid);
        object o = Activator.CreateInstance(type);
        retval = true;
        Marshal.ReleaseComObject(o);
      }
      catch { }
      return retval;
    }

    /// <summary>
    /// Check if the Video Mixing Renderer 9 Filter is available
    /// <seealso cref="IsThisComObjectInstalled"/>
    /// </summary>
    /// <remarks>
    /// This method uses <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> internally
    /// </remarks>
    /// <returns>true if VMR9 is present, false if not</returns>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static bool IsVMR9Present()
    {
      return IsThisComObjectInstalled(typeof(VideoMixingRenderer9).GUID);
    }

    /// <summary>
    /// Check if the Video Mixing Renderer 7 Filter is available
    /// <seealso cref="IsThisComObjectInstalled"/>
    /// </summary>
    /// <remarks>
    /// This method uses <see cref="IsThisComObjectInstalled">IsThisComObjectInstalled</see> internally
    /// </remarks>
    /// <returns>true if VMR7 is present, false if not</returns>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static bool IsVMR7Present()
    {
      return IsThisComObjectInstalled(typeof(VideoMixingRenderer).GUID);
    }

    /// <summary>
    /// Connect pins from two filters
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="upFilter">the upstream filter</param>
    /// <param name="sourcePinName">the upstream filter pin name</param>
    /// <param name="downFilter">the downstream filter</param>
    /// <param name="destPinName">the downstream filter pin name</param>
    /// <param name="useIntelligentConnect">indicate if the method should use DirectShow's Intelligent Connect</param>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder, upFilter or downFilter are null</exception>
    /// <exception cref="System.ArgumentException">Thrown if pin names are not found in filters</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if pins can't connect</exception>
    /// <remarks>
    /// If useIntelligentConnect is true, this method can add missing filters between the two pins.<br/>
    /// If useIntelligentConnect is false, this method works only if the two media types are compatible.
    /// </remarks>
    public static void ConnectFilters(IGraphBuilder graphBuilder, IBaseFilter upFilter, string sourcePinName, IBaseFilter downFilter, string destPinName, bool useIntelligentConnect)
    {
      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      if (upFilter == null)
        throw new ArgumentNullException("upFilter");

      if (downFilter == null)
        throw new ArgumentNullException("downFilter");

      IPin sourcePin = DsFindPin.ByName(upFilter, sourcePinName);
      if (sourcePin == null)
        throw new ArgumentException("The source filter has no pin called : " + sourcePinName, sourcePinName);

      IPin destPin = DsFindPin.ByName(downFilter, destPinName);
      if (destPin == null)
        throw new ArgumentException("The downstream filter has no pin called : " + destPinName, destPinName);

      try
      {
        ConnectFilters(graphBuilder, sourcePin, destPin, useIntelligentConnect);
      }
      finally
      {
        Marshal.ReleaseComObject(sourcePin);
        Marshal.ReleaseComObject(destPin);
      }
    }

    /// <summary>
    /// Connect pins from two filters
    /// </summary>
    /// <param name="graphBuilder">the IGraphBuilder interface of the graph</param>
    /// <param name="sourcePin">the source (upstream / output) pin</param>
    /// <param name="destPin">the destination (downstream / input) pin</param>
    /// <param name="useIntelligentConnect">indicates if the method should use DirectShow's Intelligent Connect</param>
    /// <exception cref="System.ArgumentNullException">Thrown if graphBuilder, sourcePin or destPin are null</exception>
    /// <exception cref="System.Runtime.InteropServices.COMException">Thrown if pins can't connect</exception>
    /// <remarks>
    /// If useIntelligentConnect is true, this method can add missing filters between the two pins.<br/>
    /// If useIntelligentConnect is false, this method works only if the two media types are compatible.
    /// </remarks>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static void ConnectFilters(IGraphBuilder graphBuilder, IPin sourcePin, IPin destPin, bool useIntelligentConnect)
    {
      int hr;

      if (graphBuilder == null)
        throw new ArgumentNullException("graphBuilder");

      if (sourcePin == null)
        throw new ArgumentNullException("sourcePin");

      if (destPin == null)
        throw new ArgumentNullException("destPin");

      if (useIntelligentConnect)
      {
        hr = graphBuilder.Connect(sourcePin, destPin);
        DsError.ThrowExceptionForHR(hr);
      }
      else
      {
        hr = graphBuilder.ConnectDirect(sourcePin, destPin, null);
        DsError.ThrowExceptionForHR(hr);
      }
    }

    /// <summary>
    /// helper function to connect 2 filters
    /// </summary>
    /// <param name="graphBuilder">graph builder interface</param>
    /// <param name="pinSource">souce pin</param>
    /// <param name="filterDest">destination filter</param>
    /// <param name="destPinIndex">input pin index</param>
    public static bool ConnectPin(IGraphBuilder graphBuilder, IPin pinSource, IBaseFilter filterDest, int destPinIndex)
    {
      IPin pin;
      pinSource.ConnectedTo(out pin);
      if (pin != null)
      {
        Release.ComObject("Connect Pin", pin);
        return false;
      }
      IPin pinDest = DsFindPin.ByDirection(filterDest, PinDirection.Input, destPinIndex);
      if (pinDest == null)
        return false;

      int hr = graphBuilder.Connect(pinSource, pinDest);
      if (hr != 0)
      {
        Release.ComObject("Connect Pin", pinDest);
        return false;
      }
      Release.ComObject("Connect Pin", pinDest);
      return true;
    }

    /// <summary>
    /// helper function to connect 2 filters
    /// </summary>
    /// <param name="graphBuilder">The graph builder.</param>
    /// <param name="pinSource">The ping source.</param>
    /// <param name="destinationFilter">The destination filter.</param>
    /// <returns></returns>
    static public bool ConnectFilter(IGraphBuilder graphBuilder, IPin pinSource, IBaseFilter destinationFilter)
    {
      //Log.Log.WriteFile("analog: ConnectFilter()");
      Log.Log.WriteFile("analog:  PinSource:{0}", LogPinInfo(pinSource));
      for (int i = 0; i <= 10; ++i)
      {
        IPin pinIn = DsFindPin.ByDirection(destinationFilter, PinDirection.Input, i);
        if (pinIn == null)
          return false;
        IPin connectedToPin;
        if (pinIn.ConnectedTo(out connectedToPin) != 0)
          connectedToPin = null;

        if (connectedToPin == null)
        {
          Log.Log.WriteFile("analog:  pinDest {0}:{1}", i, LogPinInfo(pinIn));
          int hr = graphBuilder.Connect(pinSource, pinIn);
          if (hr == 0)
          {
            Log.Log.WriteFile("analog:  pins connected");
            Release.ComObject("pindest" + i, pinIn);
            return true;
          }
        }else
        {
          Release.ComObject("connectedToPin",connectedToPin);
        }
        Release.ComObject("pindest" + i, pinIn);
      }
      return false;
    }

    /// <summary>
    /// helper function to connect 2 filters
    /// </summary>
    /// <param name="graphBuilder">The graph builder.</param>
    /// <param name="pinSource">The ping source.</param>
    /// <param name="destinationFilter">The destination filter.</param>
    /// <param name="destinationPinIndex">The index of the destination pin</param>
    /// <returns></returns>
    static public bool ConnectFilter(IGraphBuilder graphBuilder, IPin pinSource, IBaseFilter destinationFilter, out int destinationPinIndex)
    {
      //Log.Log.WriteFile("analog: ConnectFilter()");
      Log.Log.WriteFile("analog:  PinSource:{0}", LogPinInfo(pinSource));
      destinationPinIndex = -1;
      for (int i = 0; i <= 10; ++i)
      {
        IPin pinIn = DsFindPin.ByDirection(destinationFilter, PinDirection.Input, i);
        if (pinIn == null)
          return false;
        destinationPinIndex++;
        IPin connectedToPin;
        if (pinIn.ConnectedTo(out connectedToPin) != 0)
          connectedToPin = null;

        if (connectedToPin == null)
        {
          Log.Log.WriteFile("analog:  pinDest {0}:{1}", i, LogPinInfo(pinIn));
          int hr = graphBuilder.Connect(pinSource, pinIn);
          if (hr == 0)
          {
            Log.Log.WriteFile("analog:  pins connected");
            Release.ComObject("pindest" + i, pinIn);
            return true;
          }
        }
        else
        {
          Release.ComObject("connectedToPin", connectedToPin);
        }
        Release.ComObject("pindest" + i, pinIn);
      }
      return false;
    }

    /// <summary>
    /// helper function to connect 2 filters
    /// </summary>
    /// <param name="graphBuilder">The graph builder.</param>
    /// <param name="sourceFilter">The source filter.</param>
    /// <param name="pinDestination">The pin destination.</param>
    /// <returns></returns>
    static public bool ConnectFilter(IGraphBuilder graphBuilder, IBaseFilter sourceFilter, IPin pinDestination)
    {
      //Log.Log.WriteFile("analog: ConnectFilter()");
      Log.Log.WriteFile("analog:  PinDest:{0}", LogPinInfo(pinDestination));
      for (int i = 0; i <= 10; ++i)
      {
        IPin pinOut = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, i);
        if (pinOut == null)
          return false;
        Log.Log.WriteFile("analog:  pinSource {0}:{1}", i, LogPinInfo(pinOut));
        int hr = graphBuilder.Connect(pinOut, pinDestination);
        if (hr == 0)
        {
          Log.Log.WriteFile("analog:  pins connected");
          Release.ComObject("pindest" + i, pinOut);
          return true;
        }
        Release.ComObject("pindest" + i, pinOut);
      }
      return false;
    }

    /// <summary>
    /// helper function to connect 2 filters
    /// </summary>
    /// <param name="graphBuilder">The graph builder.</param>
    /// <param name="sourceFilter">The source filter.</param>
    /// <param name="pinDestination">The pin destination.</param>
    /// <param name="sourcePinIndex">The determined output index of the source filter</param>
    /// <returns></returns>
    static public bool ConnectFilter(IGraphBuilder graphBuilder, IBaseFilter sourceFilter, IPin pinDestination, out int sourcePinIndex)
    {
      //Log.Log.WriteFile("analog: ConnectFilter()");
      Log.Log.WriteFile("analog:  PinDest:{0}", LogPinInfo(pinDestination));
      sourcePinIndex = -1;
      for (int i = 0; i <= 10; ++i)
      {
        IPin pinOut = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, i);
        if (pinOut == null)
          return false;
        Log.Log.WriteFile("analog:  pinSource {0}:{1}", i, LogPinInfo(pinOut));
        int hr = graphBuilder.Connect(pinOut, pinDestination);
        if (hr == 0)
        {
          Log.Log.WriteFile("analog:  pins connected");
          Release.ComObject("pindest" + i, pinOut);
          sourcePinIndex = i;
          return true;
        }
        Release.ComObject("pindest" + i, pinOut);
      }
      return false;
    }

    /// <summary>
    /// helper function to connect 2 filters
    /// </summary>
    /// <param name="graphBuilder">The graph builder.</param>
    /// <param name="sourceFilter">The source filter.</param>
    /// <param name="destinationFilter">The destination filter.</param>
    /// <returns></returns>
    static public bool ConnectFilter(IGraphBuilder graphBuilder, IBaseFilter sourceFilter, IBaseFilter destinationFilter)
    {
      //Log.Log.WriteFile("analog: ConnectFilter()");
      IPin pinIn = DsFindPin.ByDirection(destinationFilter, PinDirection.Input, 0);
      Log.Log.WriteFile("analog:  PinDest:{0}", LogPinInfo(pinIn));
      for (int i = 0; i <= 10; ++i)
      {
        IPin pinOut = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, i);
        Log.Log.WriteFile("analog:  pinSource {0}:{1}", i, LogPinInfo(pinOut));
        int hr = graphBuilder.Connect(pinOut, pinIn);
        if (hr == 0)
        {
          Log.Log.WriteFile("analog:  pins connected");
          Release.ComObject("pinIn", pinIn);
          Release.ComObject("pinOut", pinOut);
          return true;
        }
        Release.ComObject("pinOut", pinOut);
      }
      Release.ComObject("pinIn", pinIn);
      return false;
    }

    /// <summary>
    /// Connects two filters to one another
    /// </summary>
    /// <param name="graphBuilder">current graph reference</param>
    /// <param name="sourceFilter">source filter</param>
    /// <param name="destinationFilter">destination filetr</param>
    /// <param name="deviceName">filter name</param>
    /// <returns></returns>
    static public bool ConnectFilter(IGraphBuilder graphBuilder, IBaseFilter sourceFilter, IBaseFilter destinationFilter, string deviceName)
    {
      //Log.Log.WriteFile("analog: ConnectFilter()");
      IPin pinIn = DsFindPin.ByDirection(destinationFilter, PinDirection.Input, 0);
      Log.Log.WriteFile("analog:  PinDest:{0}", LogPinInfo(pinIn));
      for (int i = 0; i <= 10; ++i)
      {
        IPin pinOut = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, i);
        if (pinOut == null)
          return false;
        Log.Log.WriteFile("analog:  pinSource {0}:{1}", i, LogPinInfo(pinOut));

        //Hauppauge hack - sorry, i attempted to do this right, but hauppauge drivers report incorrect values
        //and it takes a very long time to reject the audio to video connection - diehard2

        int hr = -1;
        IPin testPin = null;
        try
        {
          pinOut.ConnectedTo(out testPin);
        } catch (Exception ex) { Log.Log.WriteFile("Error while connecting a filter: ", ex); }
        if (testPin != null)
        {
          Release.ComObject("outPin", pinOut);
          Release.ComObject("testPin", testPin);
          Log.Log.WriteFile("analog: skipping pin");
          continue;
        }
        if (deviceName.Contains("Hauppauge") &&
          (LogPinInfo(pinOut).Contains("Audio") || LogPinInfo(pinIn).Contains("Audio")))
        {
          if (LogPinInfo(pinOut).Contains("Audio") && LogPinInfo(pinIn).Contains("Audio"))
            hr = graphBuilder.Connect(pinOut, pinIn);
        }
        else
          hr = graphBuilder.Connect(pinOut, pinIn);

        if (hr == 0)
        {
          Log.Log.WriteFile("analog:  pins connected");
          Release.ComObject("pinIn", pinIn);
          Release.ComObject("pinOut", pinOut);
          return true;
        }
        Release.ComObject("pinOut", pinOut);
        Release.ComObject("testPin", testPin);
      }
      Release.ComObject("pinIn", pinIn);
      return false;
    }

    /// <summary>
    /// increment the ComObject referencecount of the object.
    /// </summary>
    /// <param name="o">The object.</param>
    static private object incRefCountCOM(object o)
    {
      IntPtr pUnk = Marshal.GetIUnknownForObject(o);
      object oCom = Marshal.GetObjectForIUnknown(pUnk);
      Marshal.Release(pUnk);
      return oCom;
    }

    /// <summary>
    /// get the referencecount of the object.
    /// </summary>
    /// <param name="o">The object.</param>
    /// <returns>referencecount</returns>
    static public int getRefCount(object o)
    {
      if (o == null)
        return -1;
      int refcount = Marshal.Release(Marshal.GetIUnknownForObject(o));
      return refcount;
    }

    /// <summary>
    /// get the ComObject referencecount of the filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>referencecount</returns>
    static public int getRefCountCOM(object filter)
    {
      if (filter == null)
        return -1;
      object o = incRefCountCOM(filter);
      int refcount = Marshal.ReleaseComObject(o);
      return refcount;
    }

    /// <summary>
    /// Logs the pin info.
    /// </summary>
    /// <param name="pin">The pin.</param>
    /// <returns></returns>
    static public string LogPinInfo(IPin pin)
    {
      if (pin == null)
        return " pin==null ";
      PinInfo pinInfo;
      IPin connectedToPin;
      if (pin.ConnectedTo(out connectedToPin) != 0)
        connectedToPin = null;

      bool connected = connectedToPin != null;
      if (connected)
        Marshal.ReleaseComObject(connectedToPin);

      int hr = pin.QueryPinInfo(out pinInfo);
      if (hr == 0)
      {
        if (pinInfo.filter != null)
          Marshal.ReleaseComObject(pinInfo.filter);
      }
      return String.Format("name:{0} [{3}/{4}] Direction:{1} Connected:{2}", pinInfo.name, pinInfo.dir, connected, getRefCount(pin), getRefCountCOM(pin));
    }

    /// <summary>
    /// Logs the filter info.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns></returns>
    static public string LogFilterInfo(IBaseFilter filter)
    {
      if (filter == null)
        return " filter==null ";
      return String.Format("name:{0} [{1}/{2}]", GetFilterName(filter), getRefCount(filter), getRefCountCOM(filter));
    }

    /// <summary>
    /// Gets the filter name.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>FilterName</returns>
    static public string GetFilterName(IBaseFilter filter)
    {
      FilterInfo filterInfo;
      int hr = filter.QueryFilterInfo(out filterInfo);
      if (hr == 0)
      {
        if (filterInfo.pGraph != null)
          Marshal.ReleaseComObject(filterInfo.pGraph);
      }
      return String.Format(filterInfo.achName);
    }

    /// <summary>
    /// Gets the pin name.
    /// </summary>
    /// <param name="pin">The pin.</param>
    /// <returns>PinName</returns>
    static public string GetPinName(IPin pin)
    {
      if (pin == null)
        return "";
      PinInfo pinInfo;
      int hr = pin.QueryPinInfo(out pinInfo);
      if (hr == 0)
      {
        if (pinInfo.filter != null)
          Marshal.ReleaseComObject(pinInfo.filter);
      }
      return String.Format(pinInfo.name);
    }

    /// <summary>
    /// Gets the audio MPG2 media.
    /// </summary>
    /// <returns></returns>
    static public AMMediaType GetAudioMpg2Media()
    {
      AMMediaType mediaAudio = new AMMediaType();
      mediaAudio.majorType = MediaType.Audio;
      mediaAudio.subType = MediaSubType.Mpeg2Audio;
      mediaAudio.formatType = FormatType.WaveEx;
      mediaAudio.formatPtr = IntPtr.Zero;
      mediaAudio.sampleSize = 1;
      mediaAudio.temporalCompression = false;
      mediaAudio.fixedSizeSamples = true;
      mediaAudio.unkPtr = IntPtr.Zero;
      mediaAudio.formatType = FormatType.WaveEx;
      mediaAudio.formatSize = MPEG2AudioFormat.GetLength(0);
      mediaAudio.formatPtr = Marshal.AllocCoTaskMem(mediaAudio.formatSize);
      Marshal.Copy(MPEG2AudioFormat, 0, mediaAudio.formatPtr, mediaAudio.formatSize);
      return mediaAudio;
    }

    /// <summary>
    /// Gets the audio MPG1 media.
    /// </summary>
    /// <returns></returns>
    static public AMMediaType GetAudioMpg1Media()
    {
      AMMediaType mediaAudio = new AMMediaType();
      mediaAudio.majorType = MediaType.Audio;
      mediaAudio.subType = MediaSubType.MPEG1Payload;
      mediaAudio.formatType = FormatType.WaveEx;
      mediaAudio.formatPtr = IntPtr.Zero;
      mediaAudio.sampleSize = 1;
      mediaAudio.temporalCompression = false;
      mediaAudio.fixedSizeSamples = true;
      mediaAudio.unkPtr = IntPtr.Zero;
      mediaAudio.formatType = FormatType.WaveEx;
      mediaAudio.formatSize = MPEG1AudioFormat.GetLength(0);
      mediaAudio.formatPtr = Marshal.AllocCoTaskMem(mediaAudio.formatSize);
      Marshal.Copy(MPEG1AudioFormat, 0, mediaAudio.formatPtr, mediaAudio.formatSize);
      return mediaAudio;
    }

    /// <summary>
    /// Gets the video MPG2 media.
    /// </summary>
    /// <returns></returns>
    static public AMMediaType GetVideoMpg2Media()
    {
      AMMediaType mediaVideo = new AMMediaType();
      mediaVideo.majorType = MediaType.Video;
      mediaVideo.subType = MediaSubType.Mpeg2Video;
      mediaVideo.formatType = FormatType.Mpeg2Video;
      mediaVideo.unkPtr = IntPtr.Zero;
      mediaVideo.sampleSize = 1;
      mediaVideo.temporalCompression = false;
      mediaVideo.fixedSizeSamples = true;
      mediaVideo.formatSize = Mpeg2ProgramVideo.GetLength(0);
      mediaVideo.formatPtr = Marshal.AllocCoTaskMem(mediaVideo.formatSize);
      Marshal.Copy(Mpeg2ProgramVideo, 0, mediaVideo.formatPtr, mediaVideo.formatSize);
      return mediaVideo;
    }

    /// <summary>
    /// Gets the audio ac3 media type
    /// </summary>
    /// <returns></returns>
    static public AMMediaType GetAudioAc3()
    {
      AMMediaType mediaAc3 = new AMMediaType();
      mediaAc3.majorType = MediaType.Audio;
      mediaAc3.subType = MediaSubType.DolbyAC3;
      mediaAc3.formatType = FormatType.WaveEx;
      mediaAc3.unkPtr = IntPtr.Zero;
      mediaAc3.sampleSize = 1;
      mediaAc3.temporalCompression = false;
      mediaAc3.fixedSizeSamples = true;
      mediaAc3.formatSize = MPEG1AudioFormat.GetLength(0);
      mediaAc3.formatPtr = Marshal.AllocCoTaskMem(mediaAc3.formatSize);
      Marshal.Copy(MPEG1AudioFormat, 0, mediaAc3.formatPtr, mediaAc3.formatSize);
      return mediaAc3;
    }

    /// <summary>
    /// Gets the audio AAC media type
    /// </summary>
    /// <returns></returns>
    static public AMMediaType GetAudioAAC()
    {
      AMMediaType mediaAac = new AMMediaType();
      mediaAac.majorType = MediaType.Audio;
      mediaAac.subType = MediaSubType.AAC;
      mediaAac.formatType = FormatType.WaveEx;
      mediaAac.unkPtr = IntPtr.Zero;
      mediaAac.sampleSize = 1;
      mediaAac.temporalCompression = false;
      mediaAac.fixedSizeSamples = true;
      mediaAac.formatSize = AACAudioFormat.GetLength(0);
      mediaAac.formatPtr = Marshal.AllocCoTaskMem(mediaAac.formatSize);
      Marshal.Copy(AACAudioFormat, 0, mediaAac.formatPtr, mediaAac.formatSize);
      return mediaAac;
    }

    /// <summary>
    /// Gets the audio LATM AAC media type
    /// </summary>
    /// <returns></returns>
    static public AMMediaType GetAudioLATMAAC()
    {
      AMMediaType mediaLATMAAC = new AMMediaType();
      mediaLATMAAC.majorType = MediaType.Audio;
      mediaLATMAAC.subType = MediaSubType.LATMAAC;
      mediaLATMAAC.formatType = FormatType.WaveEx;
      mediaLATMAAC.unkPtr = IntPtr.Zero;
      mediaLATMAAC.sampleSize = 1;
      mediaLATMAAC.temporalCompression = false;
      mediaLATMAAC.fixedSizeSamples = true;
      mediaLATMAAC.formatSize = AACAudioFormat.GetLength(0);
      mediaLATMAAC.formatPtr = Marshal.AllocCoTaskMem(mediaLATMAAC.formatSize);
      Marshal.Copy(AACAudioFormat, 0, mediaLATMAAC.formatPtr, mediaLATMAAC.formatSize);
      return mediaLATMAAC;
    }

    /// <summary>
    /// Gets the audio LPCM media type
    /// </summary>
    /// <returns></returns>
    static public AMMediaType GetAudioLPCMMedia()
    {
      AMMediaType mediaLPCM = new AMMediaType();
      mediaLPCM.majorType = MediaType.Audio;
      mediaLPCM.subType = MediaSubType.DVD_LPCM_AUDIO;
      mediaLPCM.formatType = FormatType.WaveEx;
      mediaLPCM.unkPtr = IntPtr.Zero;
      mediaLPCM.sampleSize = 1;
      mediaLPCM.temporalCompression = false;
      mediaLPCM.fixedSizeSamples = true;
      mediaLPCM.formatSize = LPCMAudioFormat.GetLength(0);
      mediaLPCM.formatPtr = Marshal.AllocCoTaskMem(mediaLPCM.formatSize);
      Marshal.Copy(LPCMAudioFormat, 0, mediaLPCM.formatPtr, mediaLPCM.formatSize);
      return mediaLPCM;
    }

    /// <summary>
    /// Gets the transport stream media type
    /// </summary>
    /// <returns></returns>
    static public AMMediaType GetTransportStreamMedia()
    {
      AMMediaType mediaTS = new AMMediaType();
      mediaTS.majorType = MediaType.Stream;
      mediaTS.subType = MediaSubType.Mpeg2Transport;
      mediaTS.formatType = FormatType.Null;
      mediaTS.formatSize = 0;
      mediaTS.formatPtr = IntPtr.Zero;
      return mediaTS;
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
    public static extern int MkParseDisplayName(IBindCtx pcb, [MarshalAs(UnmanagedType.LPWStr)] string szUserName, out int pchEaten, out IMoniker ppmk);
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
