/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Drawing;
using System.Runtime.InteropServices; 
using DShowNET;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.Win32;


namespace MediaPortal.TV.Recording
{

  /// <summary>
  /// Implementation of IGraph for MCE cards
  /// like the Hauppauge PVR 150MCE and the WinFast PVR 2000
  /// A graphbuilder object supports one or more TVCapture cards and
  /// contains all the code/logic necessary todo
  /// -tv viewing
  /// -tv recording
  /// -tv timeshifting
  /// </summary>	
  public class MCESinkGraph : MediaPortal.TV.Recording.SinkGraph
	{
    public MCESinkGraph(int ID,int iCountryCode,bool bCable,string strVideoCaptureFilter, Size frameSize, double frameRate,string friendlyName)
      :base(ID,iCountryCode,bCable,strVideoCaptureFilter, frameSize, frameRate,friendlyName)
    {
		}
    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public override bool CreateGraph(int Quality)
    {
        if (m_graphState!=State.None) return false;
        GUIGraphicsContext.OnGammaContrastBrightnessChanged +=new VideoGammaContrastBrightnessHandler(OnGammaContrastBrightnessChanged);
			
				Vmr9 =new VMR9Util("mytv");

        m_iPrevChannel=-1;
        DirectShowUtil.DebugWrite("MCESinkGraph:CreateGraph()");
        int hr=0;
        Filters filters = new Filters();
        Filter                  videoCaptureDeviceFilter=null;
        foreach (Filter filter in filters.VideoInputDevices)
        {
          if (filter.Name.Equals(m_strVideoCaptureFilter))
          {
            videoCaptureDeviceFilter=filter;
            break;
          }
        }

        if (videoCaptureDeviceFilter==null) 
        {
          DirectShowUtil.DebugWrite("MCESinkGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strVideoCaptureFilter);
          return false;
        }

        // Make a new filter graph
        DirectShowUtil.DebugWrite("MCESinkGraph:create new filter graph (IGraphBuilder)");
        m_graphBuilder = (IGraphBuilder) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) ); 

        // Get the Capture Graph Builder
        DirectShowUtil.DebugWrite("MCESinkGraph:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
        Guid clsid = Clsid.CaptureGraphBuilder2;
        Guid riid = typeof(ICaptureGraphBuilder2).GUID;
        m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance( ref clsid, ref riid ); 

        DirectShowUtil.DebugWrite("MCESinkGraph:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
        hr = m_captureGraphBuilder.SetFiltergraph( m_graphBuilder );
        if( hr < 0 ) 
        {
          DirectShowUtil.DebugWrite("MCESinkGraph:link FAILED");
          return false;
        }
        DirectShowUtil.DebugWrite("MCESinkGraph:Add graph to ROT table");
        DsROT.AddGraphToRot( m_graphBuilder, out m_rotCookie );

        // Get the video device and add it to the filter graph
        DirectShowUtil.DebugWrite("MCESinkGraph:CreateGraph() add capture device {0}",m_strVideoCaptureFilter);
				try
				{
					m_captureFilter = Marshal.BindToMoniker( videoCaptureDeviceFilter.MonikerString ) as IBaseFilter;
					if (m_captureFilter!=null)
					{
						hr = m_graphBuilder.AddFilter( m_captureFilter, "Video Capture Device" );
						if( hr < 0 ) 
						{
							DirectShowUtil.DebugWrite("MCESinkGraph:FAILED:Add Videodevice to filtergraph");
							return false;
						}
					}
				}
				catch(Exception)
				{
					DirectShowUtil.DebugWrite("MCESinkGraph:FAILED:Add Videodevice to filtergraph");
					return false;
				}

        // Retrieve the stream control interface for the video device
        // FindInterface will also add any required filters
        // (WDM devices in particular may need additional
        // upstream filters to function).
        DirectShowUtil.DebugWrite("MCESinkGraph:get Video stream control interface (IAMStreamConfig)");
        object o;
        Guid cat ;
        Guid iid ;

        // Retrieve TV Tuner if available
        DirectShowUtil.DebugWrite("MCESinkGraph:Find TV Tuner");
        o = null;
        cat = FindDirection.UpstreamOnly;
        iid = typeof(IAMTVTuner).GUID;
        hr = m_captureGraphBuilder.FindInterface( new Guid[1]{ cat}, null, m_captureFilter, ref iid, out o );
        if (hr==0) 
        {
          m_TVTuner = o as IAMTVTuner;
        }
        if (m_TVTuner==null)
        {
          DirectShowUtil.DebugWrite("MCESinkGraph:CreateGraph() FAILED:no tuner found");
        }

      
        // For some reason, it happens alot that the capture card can NOT be connected (pin 656 for the
        // PRV150MCE) to the encoder because for some reason the videostandard is GONE...
        // So fetch the standard from the TvTuner and define it for the capture card.

      if (m_TVTuner!=null )
      {
        InitializeTuner();

        m_IAMAnalogVideoDecoder = m_captureFilter as IAMAnalogVideoDecoder;
        if (m_IAMAnalogVideoDecoder!=null)
        {
          AnalogVideoStandard videoStandard;
          m_TVTuner.get_TVFormat(out videoStandard);
          SetVideoStandard(videoStandard);
        }
      }

        // check if all tvtuner outputs are connected
        if ( m_TVTuner!=null)
        {
          for (int ipin=0; ipin < 10; ipin++)
          {
            IPin pin=DirectShowUtil.FindPinNr( (IBaseFilter)m_TVTuner,PinDirection.Output,ipin);
            if (pin!=null)
            {
              IPin pConnectPin=null;
              hr=pin.ConnectedTo(out pConnectPin);  
              if (hr!= 0 || pConnectPin==null)
              {
                //no? then connect all tvtuner outputs
                ConnectTVTunerOutputs();
                break;
              }
            }
            else break;
          }
        }

        // create new capture device object
        // this will find all output (preview,capture,...)pins and 
        //add the encoder filter if necessary
        m_videoCaptureDevice = new VideoCaptureDevice(m_graphBuilder,m_captureGraphBuilder, m_captureFilter);
      
        m_FrameSize=m_videoCaptureDevice.GetFrameSize();

        DirectShowUtil.DebugWrite("MCESinkGraph:capturing:{0}x{1}",m_FrameSize.Width,m_FrameSize.Height);
        
        if (m_videoCaptureDevice.MPEG2)
        {
          m_mpeg2Demux = new MPEG2Demux(ref m_graphBuilder,m_FrameSize);
        }

        
        // Connect video capture->mpeg2 demuxer
        ConnectVideoCaptureToMPEG2Demuxer();
        if (m_mpeg2Demux!=null)
					m_mpeg2Demux.CreateMappings();
        m_graphState=State.Created;
        return true;
    }

    void ConnectTVTunerOutputs()
    {
      // AverMedia MCE card has a bug. It will only connect the TV Tuner->crossbar if
      // the crossbar outputs are disconnected
      // same for the winfast pvr 2000
      DirectShowUtil.DebugWrite("MCESinkGraph:ConnectTVTunerOutputs()");
      
      //find crossbar
      int  hr;
      Guid cat;
      Guid iid;
      object o=null;
      cat = FindDirection.UpstreamOnly;
      iid = typeof(IAMCrossbar).GUID;
      hr=m_captureGraphBuilder.FindInterface(new Guid[1]{cat},null,m_captureFilter, ref iid, out o);
      if (hr !=0 || o == null) 
      {
        DirectShowUtil.DebugWrite("MCESinkGraph:no crossbar found");
        return; // no crossbar found?
      }
    
      IAMCrossbar crossbar = o as IAMCrossbar;
      if (crossbar ==null) 
      {
        DirectShowUtil.DebugWrite("MCESinkGraph:no crossbar found");
        return;
      }


      //disconnect the output pins of the crossbar->video capture filter
      DirectShowUtil.DebugWrite("MCESinkGraph:disconnect crossbar outputs");
      DirectShowUtil.DisconnectOutputPins(m_graphBuilder,(IBaseFilter)crossbar);

      //connect the output pins of the tvtuner
      DirectShowUtil.DebugWrite("MCESinkGraph:connect tvtuner outputs");
      bool bAllConnected=DirectShowUtil.RenderOutputPins(m_graphBuilder,(IBaseFilter)m_TVTuner);
      if (bAllConnected)
        DirectShowUtil.DebugWrite("MCESinkGraph:all connected");
      else
        DirectShowUtil.DebugWrite("MCESinkGraph:FAILED, not all pins connected");

      //reconnect the output pins of the crossbar
      DirectShowUtil.DebugWrite("MCESinkGraph:reconnect crossbar output pins");

      bAllConnected=DirectShowUtil.RenderOutputPins(m_graphBuilder,(IBaseFilter)crossbar);
      if (bAllConnected)
        DirectShowUtil.DebugWrite("MCESinkGraph:all connected");
      else
        DirectShowUtil.DebugWrite("MCESinkGraph:FAILED, not all pins connected");
    }
		
	}
}
