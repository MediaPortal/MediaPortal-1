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
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using DShowNET.Helper;
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
    DsROTEntry              _rotEntry = null; // Cookie into the Running Object Table
    VideoCaptureDevice      m_videoCaptureDevice = null;
    IMediaControl					  m_mediaControl = null;
    string                  m_strVideoCaptureFilter = "";
    int                     m_iCurrentChannel = 28;
    int                     m_iCountryCode = 31;
    int                     m_iTuningSpace=0;
    bool                    m_bUseCable = false;
    IBaseFilter             m_audioFilter=null;
    string                  m_strAudioDevice=string.Empty;
    string                  m_strAudioInputPin=string.Empty;
    IBaseFilter             m_filterCaptureAudio = null;
    //int                     _RecordingLevel=80;

    private RadioGraph()
    {
    }

		public RadioGraph(string strDevice,string strAudioDevice,string strLineInput) : this()
		{
      m_strAudioDevice=strAudioDevice;
      m_strAudioInputPin=strLineInput;
      m_strVideoCaptureFilter = strDevice;
    }

    public bool Create(bool cable, int tuningspace, int country)
    {
      int hr=0;
      if (m_graphState != State.None) return false;
      Log.Info("RadioGraph:CreateGraph()");
      m_bUseCable = cable;
      m_iTuningSpace=tuningspace;
      m_iCountryCode = country;

      Filter filterAudioCaptureDevice = null;
      Filter filterVideoCaptureDevice = null;

      // find the audio capture device
      if (m_strAudioDevice.Length > 0)
      {
        foreach (Filter filter in Filters.AudioInputDevices)
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
        foreach (Filter filter in Filters.VideoInputDevices)
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
        Log.Info("RadioGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strVideoCaptureFilter);
        return false;
      }
      // Make a new filter graph
      Log.Info("RadioGraph:create new filter graph (IGraphBuilder)");
      m_graphBuilder = (IGraphBuilder)new FilterGraph();

      // Get the Capture Graph Builder
      Log.Info("RadioGraph:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
      m_captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
      Log.Info("RadioGraph:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
      hr = m_captureGraphBuilder.SetFiltergraph(m_graphBuilder);
      if (hr < 0) 
      {
        Log.Info("RadioGraph:link FAILED:0x{0:X}",hr);
        return false;
      }
      Log.Info("RadioGraph:Add graph to ROT table");
      _rotEntry = new DsROTEntry((IFilterGraph)m_graphBuilder);

      // Get the Video device and add it to the filter graph
      Log.Info("RadioGraph:CreateGraph() add capture device {0}",m_strVideoCaptureFilter);
      m_filterCaptureVideo = Marshal.BindToMoniker(filterVideoCaptureDevice.MonikerString) as IBaseFilter;
      if (m_filterCaptureVideo != null)
      {
        hr = m_graphBuilder.AddFilter(m_filterCaptureVideo, filterVideoCaptureDevice.Name);
        if (hr < 0) 
        {
          Log.Info("RadioGraph:FAILED:Add Videodevice to filtergraph:0x{0:X}",hr);
          return false;
        }
      }

/*
      // Get the audio device and add it to the filter graph
      if (filterAudioCaptureDevice != null)
      {
        // Get the audio device and add it to the filter graph
        Log.Info("RadioGraph:CreateGraph() add capture device {0}",m_strAudioDevice);
        m_filterCaptureAudio = Marshal.BindToMoniker(filterAudioCaptureDevice.MonikerString) as IBaseFilter;
        if (m_filterCaptureAudio != null)
        {
          hr = m_graphBuilder.AddFilter(m_filterCaptureAudio, filterAudioCaptureDevice.Name);
          if (hr < 0) 
          {
            Log.Info("RadioGraph:FAILED:Add audiodevice to filtergraph:0x{0:X}",hr);
            return false;
          }
        }
      }
*/

      // Retrieve TV Tuner if available
      Log.Info("RadioGraph:Find TV Tuner");
      object o = null;
      Guid iid = typeof(IAMTVTuner).GUID;
      DsGuid cat = new DsGuid(FindDirection.UpstreamOnly);

      hr = m_captureGraphBuilder.FindInterface(cat, null, m_filterCaptureVideo,  iid, out o);
      if (hr == 0) 
      {
        m_TVTuner = o as IAMTVTuner;
      }
      if (m_TVTuner == null)
      {
        Log.Info("RadioGraph:CreateGraph() FAILED:no tuner found");
      }

      bool bAdded=false;
      m_videoCaptureDevice = new VideoCaptureDevice(m_graphBuilder, m_captureGraphBuilder, m_filterCaptureVideo, m_filterCaptureVideo);
      if (m_videoCaptureDevice.PreviewAudioPin!=null)
      {
        // add default directsound renderer
        string strDefault="@device:cm:{E0F158E1-CB04-11D0-BD4E-00A0C911CE86}";
        for (int i = 0; i < Filters.AudioRenderers.Count; ++i)
        {
          if (Filters.AudioRenderers[i].MonikerString.IndexOf(strDefault) >= 0)
          {
            Log.Info("RadioGraph:adding {0} to graph", Filters.AudioRenderers[i].Name);
						try
						{
              m_audioFilter = DirectShowUtil.AddAudioRendererToGraph(m_graphBuilder, Filters.AudioRenderers[i].Name, false);
							bAdded=true;
						}
						catch(Exception)
						{ 
							Log.Info("RadioGraph:Cannot add default audio renderer to graph");
							return false;
						}
            break;
          }
        }
        if (!bAdded)
        {
          Log.Info("RadioGraph:FAILED could not find default directsound device");
          return false;
        }

        Log.Info("RadioGraph:Render audio preview pin");
        hr=m_graphBuilder.Render(m_videoCaptureDevice.PreviewAudioPin);
        if (hr!=0)
        {
          Log.Info("RadioGraph:FAILED could not render preview audio pin:0x{0:x}",hr);
          return false;
        }
        Log.Info("RadioGraph:starting graph");
      }
/*
      // select the correct audio input pin to capture
      if (m_filterCaptureAudio != null)
      {
        if (m_strAudioInputPin.Length > 0)
        {
          Log.Info("SWGraph:set audio input pin:{0}", m_strAudioInputPin);
          IPin pinInput = DirectShowUtil.FindPin(m_filterCaptureAudio, PinDirection.Input, m_strAudioInputPin);
          if (pinInput == null)
          {
            Log.Info("SWGraph:FAILED audio input pin:{0} not found", m_strAudioInputPin);
          }
          else
          {
            IAMAudioInputMixer mixer = pinInput as IAMAudioInputMixer;
            if (mixer != null)
            {
              hr = mixer.put_Enable(true);
              if (hr != 0)
              {
                Log.Info("SWGraph:FAILED:to enable audio input pin:0x{0:X}",hr);
              }
              else
              {
                Log.Info("SWGraph:enabled audio input pin:{0}",m_strAudioInputPin);
              }

              double fLevel=((double)_RecordingLevel);
              fLevel /= 100.0d;
              hr = mixer.put_MixLevel(fLevel);
              if (hr != 0)
              {
                Log.Info("SWGraph:FAILED:to set mixing level to {0}%:0x{1:X}",_RecordingLevel,hr);
              }
              else
              {
                Log.Info("SWGraph:set mixing level to {0}% of pin:{1}",_RecordingLevel,m_strAudioInputPin);
              }

            }
            else
            {
              Log.Info("SWGraph:FAILED audio input pin:{0} does not expose an IAMAudioInputMixer", m_strAudioInputPin);
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
      Log.Info("RadioGraph:Tune:{0} Hz country:{1}",channel,m_iCountryCode);
      if (m_graphState == State.Created)
      {
        m_TVTuner.put_TuningSpace(0);
        m_TVTuner.put_CountryCode(m_iCountryCode);
        m_TVTuner.put_Mode(AMTunerModeType.FMRadio);
        if (m_bUseCable)
        {
          Log.Info("RadioGraph:Cable");
          m_TVTuner.put_InputType(0, TunerInputType.Cable);
        }
        else
        {
          Log.Info("RadioGraph:Antenna");
          m_TVTuner.put_InputType(0, TunerInputType.Antenna);
        }

        m_TVTuner.put_Channel(channel, AMTunerSubChannel.Default, AMTunerSubChannel.Default);

        m_iCurrentChannel=channel;

        int chanmin,chanmax;
        m_TVTuner.ChannelMinMax(out chanmin, out chanmax);
        Log.Info("RadioGraph:minimal :{0} Hz, maximal:{1} Hz",chanmin,chanmax);
		    if (m_strAudioDevice.Length>0)
          CrossBar.Route(m_graphBuilder, m_captureGraphBuilder, m_filterCaptureVideo, true, false, false, false, false, true);
        Log.Info("RadioGraph:start listening");
        int hr=m_mediaControl.Run();
        if (hr!=0) Log.Info("RadioGraph:start listening returned :0x{0:X}",hr);
      }
      else
      { 
          m_TVTuner.put_Channel(channel, AMTunerSubChannel.Default, AMTunerSubChannel.Default);
          m_iCurrentChannel=channel;
      }
      m_graphState = State.Listening;
    }

    public void DeleteGraph()
    {
      Log.Info("RadioGraph:DeleteGraph");
      if (m_mediaControl != null)
      {
        m_mediaControl.Stop();
      }
      if (m_videoCaptureDevice != null)
      {
        m_videoCaptureDevice.Dispose();
        m_videoCaptureDevice = null;
      }


      if (m_TVTuner != null)
        DirectShowUtil.ReleaseComObject(m_TVTuner); m_TVTuner = null;

      if (m_audioFilter!= null)
        DirectShowUtil.ReleaseComObject(m_audioFilter); m_audioFilter= null;

      m_mediaControl = null;
      
      if (m_filterCaptureVideo != null)
        DirectShowUtil.ReleaseComObject(m_filterCaptureVideo); m_filterCaptureVideo = null;

      if (m_filterCaptureAudio != null)
        DirectShowUtil.ReleaseComObject(m_filterCaptureAudio ); m_filterCaptureAudio = null;
      
      if (m_graphBuilder!=null)
        DirectShowUtil.RemoveFilters(m_graphBuilder);

      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      _rotEntry = null;

      if (m_captureGraphBuilder != null)
        DirectShowUtil.ReleaseComObject(m_captureGraphBuilder); m_captureGraphBuilder = null;
	
      if (m_graphBuilder != null)
        DirectShowUtil.ReleaseComObject(m_graphBuilder); m_graphBuilder = null;

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
