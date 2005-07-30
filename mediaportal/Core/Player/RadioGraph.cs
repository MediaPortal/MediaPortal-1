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
using MediaPortal.GUI.Library;
namespace MediaPortal.Player
{
	/// <summary>
	/// 
	/// </summary>

	public class RadioGraph
	{
    enum State
    { 
      None, 
      Created, 
      Listening,
    };
    State                   m_graphState = State.None;
    IGraphBuilder           m_graphBuilder = null;
    ICaptureGraphBuilder2   m_captureGraphBuilder = null;
    IBaseFilter             m_filterCaptureVideo = null;
    IAMTVTuner              m_TVTuner = null;
    int				              m_rotCookie = 0; // Cookie into the Running Object Table
    VideoCaptureDevice      m_videoCaptureDevice = null;
    IMediaControl					  m_mediaControl = null;
    string                  m_strVideoCaptureFilter = "";
    int                     m_iCurrentChannel = 28;
    int                     m_iCountryCode = 31;
    int                     m_iTuningSpace=0;
    bool                    m_bUseCable = false;
    IBaseFilter             m_audioFilter=null;
    string                  m_strAudioDevice=String.Empty;
    string                  m_strAudioInputPin=String.Empty;
    IBaseFilter             m_filterCaptureAudio = null;
    //int                     _RecordingLevel=80;

		public RadioGraph(string strDevice,string strAudioDevice,string strLineInput)
		{
      m_strAudioDevice=strAudioDevice;
      m_strAudioInputPin=strLineInput;
      m_strVideoCaptureFilter = strDevice;
    }

    public bool Create(bool cable, int tuningspace, int country)
    {
      int hr=0;
      if (m_graphState != State.None) return false;
      DirectShowUtil.DebugWrite("RadioGraph:CreateGraph()");
      m_bUseCable = cable;
      m_iTuningSpace=tuningspace;
      m_iCountryCode = country;

      Filters filters = new Filters();
      Filter filterAudioCaptureDevice = null;
      Filter filterVideoCaptureDevice = null;

      // find the audio capture device
      if (m_strAudioDevice.Length > 0)
      {
        foreach (Filter filter in filters.AudioInputDevices)
        {
          if (filter.Name.Equals(m_strAudioDevice))
          {
            filterAudioCaptureDevice = filter;
            break;
          }
        }
      }

      // find the Video capture device
      if (m_strVideoCaptureFilter.Length > 0)
      {
        foreach (Filter filter in filters.VideoInputDevices)
        {
          if (filter.Name.Equals(m_strVideoCaptureFilter))
          {
            filterVideoCaptureDevice = filter;
            break;
          }
        }
      }

      
      if (filterVideoCaptureDevice == null) 
      {
        DirectShowUtil.DebugWrite("RadioGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strVideoCaptureFilter);
        return false;
      }
      // Make a new filter graph
      DirectShowUtil.DebugWrite("RadioGraph:create new filter graph (IGraphBuilder)");
      m_graphBuilder = (IGraphBuilder) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph, true));

      // Get the Capture Graph Builder
      DirectShowUtil.DebugWrite("RadioGraph:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
      Guid clsid = Clsid.CaptureGraphBuilder2;
      Guid riid = typeof(ICaptureGraphBuilder2).GUID;
      m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance(ref clsid, ref riid);

      DirectShowUtil.DebugWrite("RadioGraph:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
      hr = m_captureGraphBuilder.SetFiltergraph(m_graphBuilder);
      if (hr < 0) 
      {
        DirectShowUtil.DebugWrite("RadioGraph:link FAILED:0x{0:X}",hr);
        return false;
      }
      DirectShowUtil.DebugWrite("RadioGraph:Add graph to ROT table");
      DsROT.AddGraphToRot(m_graphBuilder, out m_rotCookie);

      // Get the Video device and add it to the filter graph
      DirectShowUtil.DebugWrite("RadioGraph:CreateGraph() add capture device {0}",m_strVideoCaptureFilter);
      m_filterCaptureVideo = Marshal.BindToMoniker(filterVideoCaptureDevice.MonikerString) as IBaseFilter;
      if (m_filterCaptureVideo != null)
      {
        hr = m_graphBuilder.AddFilter(m_filterCaptureVideo, filterVideoCaptureDevice.Name);
        if (hr < 0) 
        {
          DirectShowUtil.DebugWrite("RadioGraph:FAILED:Add Videodevice to filtergraph:0x{0:X}",hr);
          return false;
        }
      }

/*
      // Get the audio device and add it to the filter graph
      if (filterAudioCaptureDevice != null)
      {
        // Get the audio device and add it to the filter graph
        DirectShowUtil.DebugWrite("RadioGraph:CreateGraph() add capture device {0}",m_strAudioDevice);
        m_filterCaptureAudio = Marshal.BindToMoniker(filterAudioCaptureDevice.MonikerString) as IBaseFilter;
        if (m_filterCaptureAudio != null)
        {
          hr = m_graphBuilder.AddFilter(m_filterCaptureAudio, filterAudioCaptureDevice.Name);
          if (hr < 0) 
          {
            DirectShowUtil.DebugWrite("RadioGraph:FAILED:Add audiodevice to filtergraph:0x{0:X}",hr);
            return false;
          }
        }
      }
*/

      // Retrieve TV Tuner if available
      DirectShowUtil.DebugWrite("RadioGraph:Find TV Tuner");
      object o = null;
      Guid cat = FindDirection.UpstreamOnly;
      Guid iid = typeof(IAMTVTuner).GUID;
      hr = m_captureGraphBuilder.FindInterface(new Guid[1] { cat}, null, m_filterCaptureVideo, ref iid, out o);
      if (hr == 0) 
      {
        m_TVTuner = o as IAMTVTuner;
      }
      if (m_TVTuner == null)
      {
        DirectShowUtil.DebugWrite("RadioGraph:CreateGraph() FAILED:no tuner found");
      }

      bool bAdded=false;
      m_videoCaptureDevice = new VideoCaptureDevice(m_graphBuilder, m_captureGraphBuilder, m_filterCaptureVideo);
      if (m_videoCaptureDevice.PreviewAudioPin!=null)
      {
        // add default directsound renderer
        string strDefault="@device:cm:{E0F158E1-CB04-11D0-BD4E-00A0C911CE86}";
        for (int i=0; i < filters.AudioRenderers.Count;++i)
        {
          if (filters.AudioRenderers[i].MonikerString.IndexOf(strDefault)>=0)
          {
            DirectShowUtil.DebugWrite("RadioGraph:adding {0} to graph",filters.AudioRenderers[i].Name);
						try
						{
							m_audioFilter=DirectShowUtil.AddAudioRendererToGraph( m_graphBuilder, filters.AudioRenderers[i].Name,false);
							bAdded=true;
						}
						catch(Exception)
						{ 
							Log.Write("RadioGraph:Cannot add default audio renderer to graph");
							return false;
						}
            break;
          }
        }
        if (!bAdded)
        {
          DirectShowUtil.DebugWrite("RadioGraph:FAILED could not find default directsound device");
          return false;
        }

        DirectShowUtil.DebugWrite("RadioGraph:Render audio preview pin");
        hr=m_graphBuilder.Render(m_videoCaptureDevice.PreviewAudioPin);
        if (hr!=0)
        {
          DirectShowUtil.DebugWrite("RadioGraph:FAILED could not render preview audio pin:0x{0:x}",hr);
          return false;
        }
        DirectShowUtil.DebugWrite("RadioGraph:starting graph");
      }
/*
      // select the correct audio input pin to capture
      if (m_filterCaptureAudio != null)
      {
        if (m_strAudioInputPin.Length > 0)
        {
          DirectShowUtil.DebugWrite("SWGraph:set audio input pin:{0}", m_strAudioInputPin);
          IPin pinInput = DirectShowUtil.FindPin(m_filterCaptureAudio, PinDirection.Input, m_strAudioInputPin);
          if (pinInput == null)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED audio input pin:{0} not found", m_strAudioInputPin);
          }
          else
          {
            IAMAudioInputMixer mixer = pinInput as IAMAudioInputMixer;
            if (mixer != null)
            {
              hr = mixer.put_Enable(true);
              if (hr != 0)
              {
                DirectShowUtil.DebugWrite("SWGraph:FAILED:to enable audio input pin:0x{0:X}",hr);
              }
              else
              {
                DirectShowUtil.DebugWrite("SWGraph:enabled audio input pin:{0}",m_strAudioInputPin);
              }

              double fLevel=((double)_RecordingLevel);
              fLevel /= 100.0d;
              hr = mixer.put_MixLevel(fLevel);
              if (hr != 0)
              {
                DirectShowUtil.DebugWrite("SWGraph:FAILED:to set mixing level to {0}%:0x{1:X}",_RecordingLevel,hr);
              }
              else
              {
                DirectShowUtil.DebugWrite("SWGraph:set mixing level to {0}% of pin:{1}",_RecordingLevel,m_strAudioInputPin);
              }

            }
            else
            {
              DirectShowUtil.DebugWrite("SWGraph:FAILED audio input pin:{0} does not expose an IAMAudioInputMixer", m_strAudioInputPin);
            }
          }
        }
      }
*/
      m_mediaControl = (IMediaControl) m_graphBuilder;
      m_graphState = State.Created;
      return true;
    }

    public void Tune(int channel)
    {
      if (m_graphState < State.Created) return;
      if (m_TVTuner == null) return;
      DirectShowUtil.DebugWrite("RadioGraph:Tune:{0} Hz country:{1}",channel,m_iCountryCode);
      if (m_graphState == State.Created)
      {
        m_TVTuner.put_TuningSpace(0);
        m_TVTuner.put_CountryCode(m_iCountryCode);
        m_TVTuner.put_Mode(DShowNET.AMTunerModeType.FMRadio);
        if (m_bUseCable)
        {
          DirectShowUtil.DebugWrite("RadioGraph:Cable");
          m_TVTuner.put_InputType(0, DShowNET.TunerInputType.Cable);
        }
        else
        {
          DirectShowUtil.DebugWrite("RadioGraph:Antenna");
          m_TVTuner.put_InputType(0, DShowNET.TunerInputType.Antenna);
        }

        m_TVTuner.put_Channel(channel, DShowNET.AMTunerSubChannel.Default, DShowNET.AMTunerSubChannel.Default);

        m_iCurrentChannel=channel;

        int chanmin,chanmax;
        m_TVTuner.ChannelMinMax(out chanmin, out chanmax);
        DirectShowUtil.DebugWrite("RadioGraph:minimal :{0} Hz, maximal:{1} Hz",chanmin,chanmax);
		    if (m_strAudioDevice.Length>0)
          DsUtils.FixCrossbarRouting(m_graphBuilder,m_captureGraphBuilder, m_filterCaptureVideo, true, false,false,false,false,true);
        DirectShowUtil.DebugWrite("RadioGraph:start listening");
        int hr=m_mediaControl.Run();
        if (hr!=0) DirectShowUtil.DebugWrite("RadioGraph:start listening returned :0x{0:X}",hr);
      }
      else
      { 
          m_TVTuner.put_Channel(channel, DShowNET.AMTunerSubChannel.Default, DShowNET.AMTunerSubChannel.Default);
          m_iCurrentChannel=channel;
      }
      m_graphState = State.Listening;
    }

    public void DeleteGraph()
    {
      DirectShowUtil.DebugWrite("RadioGraph:DeleteGraph");
      if (m_mediaControl != null)
      {
        m_mediaControl.Stop();
      }
      if (m_videoCaptureDevice != null)
      {
        m_videoCaptureDevice.CloseInterfaces();
        m_videoCaptureDevice = null;
      }


      if (m_TVTuner != null)
        Marshal.ReleaseComObject(m_TVTuner); m_TVTuner = null;

      if (m_audioFilter!= null)
        Marshal.ReleaseComObject(m_audioFilter); m_audioFilter= null;

      m_mediaControl = null;
      
      if (m_filterCaptureVideo != null)
        Marshal.ReleaseComObject(m_filterCaptureVideo); m_filterCaptureVideo = null;

      if (m_filterCaptureAudio != null)
        Marshal.ReleaseComObject(m_filterCaptureAudio ); m_filterCaptureAudio = null;
      
      if (m_graphBuilder!=null)
        DsUtils.RemoveFilters(m_graphBuilder);

      if (m_rotCookie != 0)
        DsROT.RemoveGraphFromRot(ref m_rotCookie);
      m_rotCookie = 0;

      if (m_captureGraphBuilder != null)
        Marshal.ReleaseComObject(m_captureGraphBuilder); m_captureGraphBuilder = null;
	
      if (m_graphBuilder != null)
        Marshal.ReleaseComObject(m_graphBuilder); m_graphBuilder = null;

      m_graphState = State.None;
      return;
    }

    public int Channel
    {
      get { return m_iCurrentChannel;}
    }

    public int AudioFrequency
    {
      get { 
        if (m_graphState != State.Listening) return 0;
        if (m_TVTuner == null) return 0;
        int iFreq=0;
        m_TVTuner.get_AudioFrequency(out iFreq);
        return iFreq; 

      }
    }
    public bool SignalPresent
    {
      get
      {
        if (m_graphState != State.Listening) return false;
        if (m_TVTuner == null) return false;
        AMTunerSignalStrength strength;
        int hr = m_TVTuner.SignalPresent( out strength );
        return ( ( (int)strength ) >=1 );
      }
    }
	}
}
