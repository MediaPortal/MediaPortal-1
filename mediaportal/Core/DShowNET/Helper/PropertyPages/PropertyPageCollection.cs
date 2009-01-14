#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace DShowNET.Helper
{
  /// <summary>
  ///  A collection of available PropertyPages in a DirectShow
  ///  filter graph. It is up to the driver manufacturer to implement
  ///  a property pages on their drivers. The list of supported 
  ///  property pages will vary from driver to driver.
  /// </summary>
  public class PropertyPageCollection : CollectionBase, IDisposable
  {
    // --------------- Constructors / Destructor ----------------

    /// <summary> Initialize collection with no property pages. </summary>
    public PropertyPageCollection()
    {
      InnerList.Capacity = 1;
    }

    /// <summary> Initialize collection with property pages from existing graph. </summary>
    public PropertyPageCollection(
      ICaptureGraphBuilder2 graphBuilder,
      IBaseFilter videoDeviceFilter, IBaseFilter audioDeviceFilter,
      IBaseFilter videoCompressorFilter, IBaseFilter audioCompressorFilter,
      SourceCollection videoSources, SourceCollection audioSources, IBaseFilter tuner)
    {
      addFromGraph(graphBuilder,
                   videoDeviceFilter, audioDeviceFilter,
                   videoCompressorFilter, audioCompressorFilter,
                   videoSources, audioSources, tuner);
    }

    public PropertyPageCollection(
      ICaptureGraphBuilder2 graphBuilder,
      IBaseFilter videoCompressorFilter, IBaseFilter audioCompressorFilter)
    {
      addFromGraph(graphBuilder,
                   null, null,
                   videoCompressorFilter, audioCompressorFilter,
                   null, null, null);
    }

    /// <summary> Destructor. Release unmanaged resources. </summary>
    ~PropertyPageCollection()
    {
      Dispose();
    }


    // ----------------- Public Properties ------------------

    /// <summary> Empty the collection. </summary>
    public new void Clear()
    {
      for (int c = 0; c < InnerList.Count; c++)
      {
        this[c].Dispose();
      }
      InnerList.Clear();
    }

    /// <summary> Release unmanaged resources </summary>
    public void Dispose()
    {
      Clear();
      InnerList.Capacity = 1;
    }


    // ---------------- Private Methods --------------------

    /// <summary> Get the filter at the specified index. </summary>
    public PropertyPage this[int index]
    {
      get { return ((PropertyPage) InnerList[index]); }
    }


    // ------------------ Public Methods --------------------

    /// <summary> Populate the collection by looking for commonly implemented property pages. </summary>
    protected void addFromGraph(
      ICaptureGraphBuilder2 graphBuilder,
      IBaseFilter videoDeviceFilter, IBaseFilter audioDeviceFilter,
      IBaseFilter videoCompressorFilter, IBaseFilter audioCompressorFilter,
      SourceCollection videoSources, SourceCollection audioSources, IBaseFilter tuner)
    {
      object filter = null;
      DsGuid cat;
      DsGuid med;
      Guid iid;
      int hr;

      Trace.Assert(graphBuilder != null);

      if (videoDeviceFilter != null)
      {
        // 1. the video capture filter
        addIfSupported(videoDeviceFilter, "Video Capture Device");

        // 2. the video capture pin
        cat = new DsGuid(PinCategory.Capture);
        iid = typeof (IAMStreamConfig).GUID;
        hr = graphBuilder.FindInterface(cat, null, videoDeviceFilter, iid, out filter);
        if (hr != 0)
        {
          filter = null;
        }
        addIfSupported(filter, "Video Capture Pin");

        // 3. the video preview pin
        cat = new DsGuid(PinCategory.Preview);
        iid = typeof (IAMStreamConfig).GUID;
        hr = graphBuilder.FindInterface(cat, null, videoDeviceFilter, iid, out filter);
        if (hr != 0)
        {
          filter = null;
        }
        addIfSupported(filter, "Video Preview Pin");
      }

      // 4. the video crossbar(s)
      int num = 1;
      ArrayList crossbars = new ArrayList();
      if (videoSources != null)
      {
        for (int c = 0; c < videoSources.Count; c++)
        {
          CrossbarSource s = videoSources[c] as CrossbarSource;
          if (s != null)
          {
            if (crossbars.IndexOf(s.Crossbar) < 0)
            {
              crossbars.Add(s.Crossbar);
              if (addIfSupported(s.Crossbar, "Video Crossbar " + (num == 1 ? "" : num.ToString())))
              {
                num++;
              }
            }
          }
        }
      }
      crossbars.Clear();

      // 5. the video compressor
      if (videoCompressorFilter != null)
      {
        addIfSupported(videoCompressorFilter, "Video Compressor");
      }
/*
      if (videoDeviceFilter!=null)
      {
        // 6. the video TV tuner
        cat = PinCategory.Capture;
        iid = typeof(IAMTVTuner).GUID;
        hr = graphBuilder.FindInterface( new Guid[1]{ cat}, null, videoDeviceFilter, ref iid, out filter );
        if ( hr != 0 )
        {
          filter = null;
        }
        addIfSupported( filter, "TV Tuner" );
      }*/

      // 7. the video compressor (VFW)
      IAMVfwCompressDialogs compressDialog = videoCompressorFilter as IAMVfwCompressDialogs;
      if (compressDialog != null)
      {
        VfwCompressorPropertyPage page = new VfwCompressorPropertyPage("Video Compressor", compressDialog);
        InnerList.Add(page);
      }

      if (audioDeviceFilter != null)
      {
        // 8. the audio capture filter
        addIfSupported(audioDeviceFilter, "Audio Capture Device");

        // 9. the audio capture pin
        cat = new DsGuid(PinCategory.Capture);
        med = new DsGuid(MediaType.Audio);
        iid = typeof (IAMStreamConfig).GUID;
        hr = graphBuilder.FindInterface(cat, med, audioDeviceFilter, iid, out filter);
        if (hr != 0)
        {
          filter = null;
        }
        addIfSupported(filter, "Audio Capture Pin");

        // 9. the audio preview pin
        cat = PinCategory.Preview;
        med = new DsGuid(MediaType.Audio);
        iid = typeof (IAMStreamConfig).GUID;
        hr = graphBuilder.FindInterface(cat, med, audioDeviceFilter, iid, out filter);
        if (hr != 0)
        {
          filter = null;
        }
        addIfSupported(filter, "Audio Preview Pin");
      }

      // 10. the audio crossbar(s)
      num = 1;
      if (audioSources != null)
      {
        for (int c = 0; c < audioSources.Count; c++)
        {
          CrossbarSource s = audioSources[c] as CrossbarSource;
          if (s != null)
          {
            if (crossbars.IndexOf(s.Crossbar) < 0)
            {
              crossbars.Add(s.Crossbar);
              if (addIfSupported(s.Crossbar, "Audio Crossbar " + (num == 1 ? "" : num.ToString())))
              {
                num++;
              }
            }
          }
        }
      }
      crossbars.Clear();

      if (audioCompressorFilter != null)
      {
        // 11. the audio compressor
        addIfSupported(audioCompressorFilter, "Audio Compressor");
      }

      if (tuner != null)
      {
        // 12. tuner
        addIfSupported(tuner, "Tuner");
      }

      // 13. crossbars
      if (videoDeviceFilter != null)
      {
        med = MediaType.Stream;
        object o;
        IBaseFilter cfilter = videoDeviceFilter;
        int iCrossBar = 0;
        while (true)
        {
          cat = new DsGuid(FindDirection.UpstreamOnly);
          iid = typeof (IAMCrossbar).GUID;
          hr = graphBuilder.FindInterface(cat, null, cfilter, iid, out o);
          if (hr == 0)
          {
            iCrossBar++;
            IAMCrossbar crossbar = o as IAMCrossbar;
            addIfSupported(crossbar, "CrossBar:" + iCrossBar.ToString());
            cfilter = (IBaseFilter) crossbar;
          }
          else
          {
            break;
          }
        }
      }
    }

    /// <summary> 
    ///  Returns the object as an ISpecificPropertyPage
    ///  if the object supports the ISpecificPropertyPage
    ///  interface and has at least one property page.
    /// </summary>
    public bool addIfSupported(object o, string name)
    {
      ISpecifyPropertyPages specifyPropertyPages = null;
      DsCAUUID cauuid = new DsCAUUID();
      bool wasAdded = false;

      // Determine if the object supports the interface
      // and has at least 1 property page
      try
      {
        specifyPropertyPages = o as ISpecifyPropertyPages;
        if (specifyPropertyPages != null)
        {
          int hr = specifyPropertyPages.GetPages(out cauuid);
          if ((hr != 0) || (cauuid.cElems <= 0))
          {
            specifyPropertyPages = null;
          }
        }
      }
      finally
      {
        if (cauuid.pElems != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(cauuid.pElems);
        }
      }

      // Add the page to the internal collection
      if (specifyPropertyPages != null)
      {
        DirectShowPropertyPage p = new DirectShowPropertyPage(name, specifyPropertyPages);
        InnerList.Add(p);
        wasAdded = true;
      }
      return (wasAdded);
    }
  }
}