/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices; 
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using DShowNET;



																			   

namespace DirectX.Capture
{

	/// <summary>
	///  Use the Capture class to capture audio and video to AVI files.
	/// </summary>
	/// <remarks>
	///  This is the core class of the Capture Class Library. The following 
	///  sections introduce the Capture class and how to use this library.
	///  
	/// <br/><br/>
	/// <para><b>Basic Usage</b></para>
	/// 
	/// <para>
	///  The Capture class only requires a video device and/or audio device
	///  to begin capturing. The <see cref="Filters"/> class provides
	///  lists of the installed video and audio devices. 
	/// </para>
	/// <code><div style="background-color:whitesmoke;">
	///  // Remember to add a reference to DirectX.Capture.dll
	///  using DirectX.Capture
	///  ...
	///  Capture capture = new Capture( Filters.VideoInputDevices[0], 
	///                                 Filters.AudioInputDevices[0] );
	///  capture.Start();
	///  ...
	///  capture.Stop();
	/// </div></code>
	/// <para>
	///  This will capture video and audio using the first video and audio devices
	///  installed on the system. To capture video only, pass a null as the second
	///  parameter of the constructor.
	/// </para>
	/// <para> 
	///  The class is initialized to a valid temporary file in the Windows temp
	///  folder. To capture to a different file, set the 
	///  <see cref="Capture.Filename"/> property before you begin
	///  capturing. Remember to add DirectX.Capture.dll to 
	///  your project references.
	/// </para>
	///
	/// <br/>
	/// <para><b>Setting Common Properties</b></para>
	/// 
	/// <para>
	///  The example below shows how to change video and audio settings. 
	///  Properties such as <see cref="Capture.FrameRate"/> and 
	///  <see cref="AudioSampleSize"/> allow you to programmatically adjust
	///  the capture. Use <see cref="Capture.VideoCaps"/> and 
	///  <see cref="Capture.AudioCaps"/> to determine valid values for these
	///  properties.
	/// </para>
	/// <code><div style="background-color:whitesmoke;">
	///  Capture capture = new Capture( Filters.VideoInputDevices[0], 
	///                                 Filters.AudioInputDevices[1] );
	///  capture.VideoCompressor = Filters.VideoCompressors[0];
	///  capture.AudioCompressor = Filters.AudioCompressors[0];
	///  capture.FrameRate = 29.997;
	///  capture.FrameSize = new Size( 640, 480 );
	///  capture.AudioSamplingRate = 44100;
	///  capture.AudioSampleSize = 16;
	///  capture.Filename = "C:\MyVideo.avi";
	///  capture.Start();
	///  ...
	///  capture.Stop();
	/// </div></code>
	/// <para>
	///  The example above also shows the use of video and audio compressors. In most 
	///  cases you will want to use compressors. Uncompressed video can easily
	///  consume over a 1GB of disk space per minute. Whenever possible, set 
	///  the <see cref="Capture.VideoCompressor"/> and <see cref="Capture.AudioCompressor"/>
	///  properties as early as possible. Changing them requires the internal filter
	///  graph to be rebuilt which often causes most of the other properties to
	///  be reset to default values.
	/// </para>
	///
	/// <br/>
	/// <para><b>Listing Devices</b></para>
	/// 
	/// <para>
	///  Use the <see cref="Filters.VideoInputDevices"/> collection to list
	///  video capture devices installed on the system.
	/// </para>
	/// <code><div style="background-color:whitesmoke;">
	///  foreach ( Filter f in Filters.VideoInputDevices )
	///  {
	///		Debug.WriteLine( f.Name );
	///  }
	/// </div></code>
	/// The <see cref="Filters"/> class also provides collections for audio 
	/// capture devices, video compressors and audio compressors.
	///
	/// <br/>
	/// <para><b>Preview</b></para>
	/// 
	/// <para>
	///  Video preview is controled with the <see cref="Capture.PreviewWindow"/>
	///  property. Setting this property to a visible control will immediately 
	///  begin preview. Set to null to stop the preview. 
	/// </para>
	/// <code><div style="background-color:whitesmoke;">
	///  // Enable preview
	///  capture.PreviewWindow = myPanel;
	///  // Disable preview
	///  capture.PreviewWindow = null;
	/// </div></code>
	/// <para>
	///  The control used must have a window handle (HWND), good controls to 
	///  use are the Panel or the form itself.
	/// </para>
	/// <para>
	///  Retrieving or changing video/audio settings such as FrameRate, 
	///  FrameSize, AudioSamplingRate, and AudioSampleSize will cause
	///  the preview window to flash. This is beacuse the preview must be
	///  temporarily stopped. Disable the preview if you need to access
	///  several properties at the same time.
	/// </para>
	/// 
	/// <br/>
	/// <para><b>Property Pages</b></para>
	///  
	/// <para>
	///  Property pages exposed by the devices and compressors are
	///  available through the <see cref="Capture.PropertyPages"/> 
	///  collection.
	/// </para>
	/// <code><div style="background-color:whitesmoke;">
	///  // Display the first property page
	///  capture.PropertyPages[0].Show();
	/// </div></code>
	/// <para>
	///  The property pages will often expose more settings than
	///  the Capture class does directly. Some examples are brightness, 
	///  color space, audio balance and bass boost. The disadvantage
	///  to using the property pages is the user's choices cannot be
	///  saved and later restored. The exception to this is the video
	///  and audio compressor property pages. Most compressors support
	///  the saving and restoring state, see the 
	///  <see cref="PropertyPage.State"/> property for more information.
	/// </para>
	/// <para>
	///  Changes made in the property page will be reflected 
	///  immediately in the Capture class properties (e.g. Capture.FrameSize).
	///  However, the reverse is not always true. A change made directly to
	///  FrameSize, for example, may not be reflected in the associated
	///  property page. Fortunately, the filter will use requested FrameSize
	///  even though the property page shows otherwise.
	/// </para>
	/// 
	/// <br/>
	/// <para><b>Saving and Restoring Settings</b></para>
	///  
	/// <para>
	///  To save the user's choice of devices and compressors, 
	///  save <see cref="Filter.MonikerString"/> and user it later
	///  to recreate the Filter object.
	/// </para>
	/// <para>
	///  To save a user's choices from a property page use
	///  <see cref="PropertyPage.State"/>. However, only the audio
	///  and video compressor property pages support this.
	/// </para>
	/// <para>
	///  The last items to save are the video and audio settings such
	///  as FrameSize and AudioSamplingRate. When restoring, remember
	///  to restore these properties after setting the video and audio
	///  compressors.
	/// </para>
	/// <code><div style="background-color:whitesmoke;">
	///  // Disable preview
	///  capture.PreviewWindow = null;
	///  
	///  // Save settings
	///  string videoDevice = capture.VideoDevice.MonikerString;
	///  string audioDevice = capture.AudioDevice.MonikerString;
	///  string videoCompressor = capture.VideoCompressor.MonikerString;
	///  string audioCompressor = capture.AudioCompressor.MonikerString;
	///  double frameRate = capture.FrameRate;
	///  Size frameSize = capture.FrameSize;
	///  short audioChannels = capture.AudioChannels;
	///  short audioSampleSize = capture.AudioSampleSize;
	///  int audioSamplingRate = capture.AudioSamplingRate;
	///  ArrayList pages = new ArrayList();
	///  foreach ( PropertyPage p in capture.PropertyPages )
	///  {
	///		if ( p.SupportsPersisting )
	///			pages.Add( p.State );
	///	 }
	///			
	///  
	///  // Restore settings
	///  Capture capture = new Capture( new Filter( videoDevice), 
	///				new Filter( audioDevice) );
	///  capture.VideoCompressor = new Filter( videoCompressor );
	///  capture.AudioCompressor = new Filter( audioCompressor );
	///  capture.FrameRate = frameRate;
	///  capture.FrameSize = frameSize;
	///  capture.AudioChannels = audioChannels;
	///  capture.AudioSampleSize = audioSampleSize;
	///  capture.AudioSamplingRate = audioSamplingRate;
	///  foreach ( PropertyPage p in capture.PropertyPages )
	///  {
	///		if ( p.SupportsPersisting )
	///		{
	///			p.State = (byte[]) pages[0]
	///			pages.RemoveAt( 0 );
	///		}
	///	 }
	///  // Enable preview
	///  capture.PreviewWindow = myPanel;
	/// </div></code>
	///  
	/// <br/>
	/// <para><b>TV Tuner</b></para>
	/// 
	/// <para>
	///  To access the TV Tuner, use the <see cref="Capture.Tuner"/> property.
	///  If the device does not have a TV tuner, this property will be null.
	///  See <see cref="DirectX.Capture.Tuner.Channel"/>, 
	///  <see cref="DirectX.Capture.Tuner.InputType"/> and 
	///  <see cref="DirectX.Capture.Tuner.SignalPresent"/> 
	///  for more information.
	/// </para>
	/// <code><div style="background-color:whitesmoke;">
	///  // Change to channel 5
	///  capture.Tuner.Channel = 5;
	/// </div></code>
	/// 
	/// <br/>
	/// <para><b>Troubleshooting</b></para>
	/// 
	/// <para>
	///  This class library uses COM Interop to access the full
	///  capabilities of DirectShow, so if there is another
	///  application that can successfully use a hardware device
	///  then it should be possible to modify this class library
	///  to use the device.
	/// </para>
	/// <para>
	///  Try the <b>AMCap</b> sample from the DirectX SDK 
	///  (DX9\Samples\C++\DirectShow\Bin\AMCap.exe) or 
	///  <b>Virtual VCR</b> from http://www.DigTV.ws 
	/// </para>
	/// 
	/// <br/>
	/// <para><b>Credits</b></para>
	/// 
	/// <para>
	///  This class library would not be possible without the
	///  DShowNET project by NETMaster: 
	///  http://www.codeproject.com/useritems/directshownet.asp
	/// </para>
	/// <para>
	///  Documentation is generated by nDoc available at
	///  http://ndoc.sourceforge.net
	/// </para>
	/// 
	/// <br/>
	/// <para><b>Feedback</b></para>
	/// 
	///  Feel free to send comments and questions to me at
	///  mportobello@hotmail.com. If the the topic may be of interest
	///  to others, post your question on the www.codeproject.com
	///  page for DirectX.Capture.
	/// </remarks>
  public class Capture
  {
    /*
        [Serializable]
    class Settings
        {
          public double FrameRate;
          public Size FrameSize;
          public short AudioChannels;
          public short AudioSampleSize;
          public int AudioSamplingRate;
        }
    */
    // ------------------ Private Enumerations --------------------

    /// <summary> Possible states of the interal filter graph </summary>
    protected enum GraphState
    {
      Null,			// No filter graph at all
      Created,		// Filter graph created with device filters added
      Rendered,		// Filter complete built, ready to run (possibly previewing)
      Capturing		// Filter is capturing
    }



    // ------------------ Public Properties --------------------

    /// <summary> Is the class currently capturing. Read-only. </summary>
    public bool Capturing { get { return( graphState==GraphState.Capturing ); } }

    /// <summary> Has the class been cued to begin capturing. Read-only. </summary>
    public bool Cued { get { return( isCaptureRendered && graphState==GraphState.Rendered ); } }

    /// <summary> Is the class currently stopped. Read-only. </summary>
    public bool Stopped { get { return( graphState!=GraphState.Capturing ); } }

    public bool SupportsTimeShifting 
    {
      get { 
        if (m_videoCaptureDevice==null) return false;
        return m_videoCaptureDevice.MPEG2;
      }
    }
    
    public bool IsMCECard 
    {
      get 
      { 
        if (m_videoCaptureDevice==null) return false;
        return m_videoCaptureDevice.IsMCEDevice;
      }
    }
    /// <summary> 
    ///  Name of file to capture to. Initially set to
    ///  a valid temporary file.
    /// </summary>		
    /// <remarks>
    ///  If the file does not exist, it will be created. If it does 
    ///  exist, it will be overwritten. An overwritten file will 
    ///  not be shortened if the captured data is smaller than the 
    ///  original file. The file will be valid, it will just contain 
    ///  extra, unused, data after the audio/video data. 
    /// 
    /// <para>
    ///  A future version of this class will provide a method to copy 
    ///  only the valid audio/video data to a new file. </para>
    /// 
    /// <para>
    ///  This property cannot be changed while capturing or cued. </para>
    /// </remarks> 
		
    public string Filename 
    { 
      get { return( filename ); } 
      set 
      { 
        if (!SupportsTimeShifting)
        {
          assertStopped();
          if ( Cued )
            throw new InvalidOperationException( "The Filename cannot be changed once cued. Use Stop() before changing the filename." );
          filename = value; 
          if ( fileWriterFilter != null )
          {
            string s;
            AMMediaType mt = new AMMediaType(); 
            int hr = fileWriterFilter.GetCurFile( out s, mt );
            if( hr < 0 ) Marshal.ThrowExceptionForHR( hr );
            if ( mt.formatSize > 0 )
              Marshal.FreeCoTaskMem( mt.formatPtr ); 
            hr = fileWriterFilter.SetFileName( filename, ref mt );
            if( hr < 0 ) Marshal.ThrowExceptionForHR( hr );
          }
        }
        else
        {
          filename = value; 
        }
      } 
    }


    /// <summary>
    ///  The control that will host the preview window. 
    /// </summary>
    /// <remarks>
    ///  Setting this property will begin video preview
    ///  immediately. Set this property after setting all
    ///  other properties to avoid unnecessary changes
    ///  to the internal filter graph (some properties like
    ///  FrameSize require the internal filter graph to be 
    ///  stopped and disconnected before the property
    ///  can be retrieved or set).
    ///  
    /// <para>
    ///  To stop video preview, set this property to null. </para>
    /// </remarks>
    public Control PreviewWindow
    {
      get { return( previewWindow ); }
      set
      {
        if (value != previewWindow)
        {
          
          try
          {
            assertStopped();
            derenderGraph();
          }
          catch(Exception){}
          previewWindow = value;
          wantPreviewRendered = ( ( previewWindow != null ) && ( videoDevice != null ) );

          try
          {
            renderGraph();
            startPreviewIfNeeded();
          }
          catch(Exception ex)
          {
            DirectShowUtil.DebugWrite("FAILED to preview:{0} {1}",ex.Message,ex.StackTrace);
          }
            
        }
      }
    }

    public void SetVideoPosition(Rectangle rect)
    {

      if (!SupportsTimeShifting)
      {
        DirectShowUtil.DebugWrite("Capture.SetVideoPosition:({0},{1})-({2},{3})",
          rect.Left,rect.Top,
          rect.Right,rect.Bottom);
        if (m_rect.Width<=0 || m_rect.Height<=0) return;
        try
        {
          m_rect=rect;
          if (null!=videoWindow)
          {
            //basicVideo.SetDefaultSourcePosition();
            //basicVideo.SetDestinationPosition(m_rect.Left,m_rect.Top,m_rect.Width,m_rect.Height);
            videoWindow.SetWindowPosition(m_rect.Left,m_rect.Top,m_rect.Width,m_rect.Height);
          }
        }
        catch(Exception)
        {
        }
      }
    }

    /// <summary>
    ///  The capabilities of the video device.
    /// </summary>
    /// <remarks>
    ///  It may be required to cue the capture (see <see cref="Cue"/>) 
    ///  before all capabilities are correctly reported. If you 
    ///  have such a device, the developer would be interested to
    ///  hear from you.
    /// 
    /// <para>
    ///  The information contained in this property is retrieved and
    ///  cached the first time this property is accessed. Future
    ///  calls to this property use the cached results. This was done 
    ///  for performance. </para>
    ///  
    /// <para>
    ///  However, this means <b>you may get different results depending 
    ///  on when you access this property first</b>. If you are experiencing 
    ///  problems, try accessing the property immediately after creating 
    ///  the Capture class or immediately after setting the video and 
    ///  audio compressors. Also, inform the developer. </para>
    /// </remarks>
    public VideoCapabilities VideoCaps 
    { 
      get 
      { 
        if ( videoCaps == null )
        {
          if ( videoStreamConfig != null )
          {
            try 
            { 
              videoCaps = new VideoCapabilities( videoStreamConfig ); 
            }
            catch ( Exception ex ) { Debug.WriteLine( "VideoCaps: FAILED to create videoCaps." + ex.ToString() ); }
          }
        }
        return( videoCaps ); 
      } 
    }

    /// <summary>
    ///  The capabilities of the audio device.
    /// </summary>
    /// <remarks>
    ///  It may be required to cue the capture (see <see cref="Cue"/>) 
    ///  before all capabilities are correctly reported. If you 
    ///  have such a device, the developer would be interested to
    ///  hear from you.
    /// 
    /// <para>
    ///  The information contained in this property is retrieved and
    ///  cached the first time this property is accessed. Future
    ///  calls to this property use the cached results. This was done 
    ///  for performance. </para>
    ///  
    /// <para>
    ///  However, this means <b>you may get different results depending 
    ///  on when you access this property first</b>. If you are experiencing 
    ///  problems, try accessing the property immediately after creating 
    ///  the Capture class or immediately after setting the video and 
    ///  audio compressors. Also, inform the developer. </para>
    /// </remarks>
    public AudioCapabilities AudioCaps 
    { 
      get 
      { 
        if ( audioCaps == null )
        {
          if ( audioStreamConfig != null )
          {
            try 
            { 
              audioCaps = new AudioCapabilities( audioStreamConfig ); 
            }
            catch ( Exception ex ) { Debug.WriteLine( "AudioCaps: FAILED to create audioCaps." + ex.ToString() ); }
          }
        }
        return( audioCaps ); 
      } 
    }

    /// <summary> 
    ///  The video capture device filter. Read-only. To use a different 
    ///  device, dispose of the current Capture instance and create a new 
    ///  instance with the desired device. 
    /// </summary>
    public Filter VideoDevice { get { return( videoDevice ); } }

    /// <summary> 
    ///  The audio capture device filter. Read-only. To use a different 
    ///  device, dispose of the current Capture instance and create a new 
    ///  instance with the desired device. 
    /// </summary>
    public Filter AudioDevice { get { return( audioDevice ); } }

    /// <summary> 
    ///  The video compression filter. When this property is changed 
    ///  the internal filter graph is rebuilt. This means that some properties
    ///  will be reset. Set this property as early as possible to avoid losing 
    ///  changes. This property cannot be changed while capturing.
    /// </summary>
    public Filter VideoCompressor 
    { 
      get { return( videoCompressor ); } 
      set 
      { 
        if (value!=VideoCompressor)
        {
          videoCompressor = value;
          /*
            assertStopped();
            destroyGraph();
            renderGraph();
            startPreviewIfNeeded();*/
        }
      }
    }

    /// <summary> 
    ///  The audio compression filter. 
    /// </summary>
    /// <remarks>
    ///  When this property is changed 
    ///  the internal filter graph is rebuilt. This means that some properties
    ///  will be reset. Set this property as early as possible to avoid losing 
    ///  changes. This property cannot be changed while capturing.
    /// </remarks>
    public Filter AudioCompressor 
    { 
      get { return( audioCompressor ); } 
      set 
      { 
        audioCompressor = value;
        /*
          assertStopped();
          destroyGraph();
          renderGraph();
          startPreviewIfNeeded();*/
      }
    }

    /// <summary> 
    ///  The current video source. Use Capture.VideoSources to 
    ///  list available sources. Set to null to disable all 
    ///  sources (mute).
    /// </summary>
    public Source VideoSource 
    { 
      get { return( VideoSources.CurrentSource ); } 
      set { VideoSources.CurrentSource = value; } 
    }

    /// <summary> 
    ///  The current audio source. Use Capture.AudioSources to 
    ///  list available sources. Set to null to disable all 
    ///  sources (mute).
    /// </summary>
    public Source AudioSource 
    {
      get { return( AudioSources.CurrentSource ); } 
      set { AudioSources.CurrentSource = value; }
    }

    /// <summary> 
    ///  Collection of available video sources/physical connectors 
    ///  on the current video device. 
    /// </summary>
    /// <remarks>
    ///  In most cases, if the device has only one source, 
    ///  this collection will be empty. 
    /// 
    /// <para>
    ///  The information contained in this property is retrieved and
    ///  cached the first time this property is accessed. Future
    ///  calls to this property use the cached results. This was done 
    ///  for performance. </para>
    ///  
    /// <para>
    ///  However, this means <b>you may get different results depending 
    ///  on when you access this property first</b>. If you are experiencing 
    ///  problems, try accessing the property immediately after creating 
    ///  the Capture class or immediately after setting the video and 
    ///  audio compressors. Also, inform the developer. </para>
    /// </remarks>
    public SourceCollection VideoSources
    {
      get 
      { 
        if ( (int) graphState < (int)GraphState.Created)
        {
          createGraph();
        }

        if ( videoSources == null )
        {
          try
          {
            if ( videoDevice != null )
              videoSources = new SourceCollection( captureGraphBuilder, videoDeviceFilter, true );
            else
              videoSources = new SourceCollection();
          }
          catch ( Exception ex ) { Debug.WriteLine( "VideoSources: FAILED to create VideoSources." + ex.ToString() ); }
        }
        return ( videoSources );
      } 
    }


    /// <summary> 
    ///  Collection of available audio sources/physical connectors 
    ///  on the current audio device. 
    /// </summary>
    /// <remarks>
    ///  In most cases, if the device has only one source, 
    ///  this collection will be empty. For audio
    ///  there are 2 different methods for enumerating audio sources
    ///  an audio crossbar (usually TV tuners?) or an audio mixer 
    ///  (usually sound cards?). This class will first look for an 
    ///  audio crossbar. If no sources or only one source is available
    ///  on the crossbar, this class will then look for an audio mixer.
    ///  This class does not support both methods.
    /// 
    /// <para>
    ///  The information contained in this property is retrieved and
    ///  cached the first time this property is accessed. Future
    ///  calls to this property use the cached results. This was done 
    ///  for performance. </para>
    ///  
    /// <para>
    ///  However, this means <b>you may get different results depending 
    ///  on when you access this property first</b>. If you are experiencing 
    ///  problems, try accessing the property immediately after creating 
    ///  the Capture class or immediately after setting the video and 
    ///  audio compressors. Also, inform the developer. </para>
    ///  </remarks>
    public SourceCollection AudioSources
    {
      get 
      { 
        if ( audioSources == null )
        {
          try
          {
            if ( audioDevice != null )
              audioSources = new SourceCollection( captureGraphBuilder, audioDeviceFilter, false );
            else
              audioSources = new SourceCollection();
          }
          catch ( Exception ex ) { Debug.WriteLine( "AudioSources: FAILED to create AudioSources." + ex.ToString() ); }
        }
        return ( audioSources );
      }
    }

    public IBaseFilter AudiodeviceFilter
    {
      get { return audioDeviceFilter;}
    }
      /// <summary>
      ///  Available property pages. 
      /// </summary>
      /// <remarks>
      ///  These are property pages exposed by the DirectShow filters. 
      ///  These property pages allow users modify settings on the 
      ///  filters directly. 
      /// 
      /// <para>
      ///  The information contained in this property is retrieved and
      ///  cached the first time this property is accessed. Future
      ///  calls to this property use the cached results. This was done 
      ///  for performance. </para>
      ///  
      /// <para>
      ///  However, this means <b>you may get different results depending 
      ///  on when you access this property first</b>. If you are experiencing 
      ///  problems, try accessing the property immediately after creating 
      ///  the Capture class or immediately after setting the video and 
      ///  audio compressors. Also, inform the developer. </para>
      /// </remarks>
      public PropertyPageCollection PropertyPages 
      {
        get
        {
          if ( propertyPages == null )
          {
            try 
            { 
              propertyPages = new PropertyPageCollection( captureGraphBuilder, 
                videoDeviceFilter, audioDeviceFilter, 
                videoCompressorFilter, audioCompressorFilter, 
                VideoSources, AudioSources, (IBaseFilter)Tuner.TvTuner );

            }
            catch ( Exception ex ) { Debug.WriteLine( "PropertyPages: FAILED to get property pages." + ex.ToString() ); }

          }
          return( propertyPages );
        }
      }

      public bool AudioPreview
      {
        set 
        {
          if (value==true)
          {
            createGraph();

            if (graphState >= GraphState.Created)
            {
              DirectShowUtil.DebugWrite("enable FM radio");
              if (m_videoCaptureDevice.PreviewAudioPin!=null)
              {
                
                bool bAdded=false;
                // add default directsound renderer
                IBaseFilter audioFilter=null;
                Filters filters=new Filters();
                string strDefault="@device:cm:{E0F158E1-CB04-11D0-BD4E-00A0C911CE86}";
                for (int i=0; i < filters.AudioRenderers.Count;++i)
                {
                  if (filters.AudioRenderers[i].MonikerString.IndexOf(strDefault)>=0)
                  {
                    bAdded=true;
                    DirectShowUtil.DebugWrite("adding {0} to graph",filters.AudioRenderers[i].Name);
                    audioFilter=DirectShowUtil.AddAudioRendererToGraph( graphBuilder, filters.AudioRenderers[i].Name,true);
                    break;
                  }
                }
                if (!bAdded)
                  DirectShowUtil.DebugWrite("FAILED could not find default directsound device");
                
                DirectShowUtil.DebugWrite("Render audio preview pin");
                int hr=graphBuilder.Render(m_videoCaptureDevice.PreviewAudioPin);
                if (hr!=0)
                  DirectShowUtil.DebugWrite("FAILED could not render preview audio pin:0x{0:x}",hr);

                DirectShowUtil.DebugWrite("starting graph");
                hr=mediaControl.Run();
                if (hr==0)
                  DirectShowUtil.DebugWrite("graph started");
                else
                  DirectShowUtil.DebugWrite("FAILED unable to start graph:0x{0:x}",hr);
                m_bGraphRunning=true;
              }
              else 
              {
                if (Tuner.AvailableAudioModes.FMRadio)
                {
                  Tuner.AudioMode = DirectX.Capture.Tuner.AMTunerModeType.FMRadio;
                  mediaControl.Run();
                  m_bGraphRunning=true;
                }
              }
            }
          }
        }
      }
      /// <summary>
      ///  The TV Tuner or null if the current video device 
      ///  does not have a TV Tuner.
      /// </summary>
      public Tuner Tuner { get { return( tuner ); } }

	
      /// <summary>
      ///  Gets and sets the frame rate used to capture video.
      /// </summary>
      /// <remarks>
      ///  Common frame rates: 24 fps for film, 25 for PAL, 29.997
      ///  for NTSC. Not all NTSC capture cards can capture at 
      ///  exactly 29.997 fps. Not all frame rates are supported. 
      ///  When changing the frame rate, the closest supported 
      ///  frame rate will be used. 
      ///  
      /// <para>
      ///  Not all devices support getting/setting this property.
      ///  If this property is not supported, accessing it will
      ///  throw and exception. </para>
      ///  
      /// <para>
      ///  This property cannot be changed while capturing. Changing 
      ///  this property while preview is enabled will cause some 
      ///  fickering while the internal filter graph is partially
      ///  rebuilt. Changing this property while cued will cancel the
      ///  cue. Call Cue() again to re-cue the capture. </para>
      /// </remarks>
      public double FrameRate
      {
        get
        {
          if (videoStreamConfig==null) return 25.0d;
          try
          {
            long avgTimePerFrame = (long) getStreamConfigSetting( videoStreamConfig, "AvgTimePerFrame" );
            double dFrameRate=(double) (10000000d / avgTimePerFrame );
            DirectShowUtil.DebugWrite("FrameRate is {0}",dFrameRate);
            return dFrameRate;
          }
          catch(Exception)
          {
          }
          return 25.0d;
        }
        set
        {
          if (videoStreamConfig==null) return ;
          try
          {
            DirectShowUtil.DebugWrite("Capture.FrameRate set to {0}",value);
            long avgTimePerFrame = (long) ( 10000000 / value );
            setStreamConfigSetting( videoStreamConfig, "AvgTimePerFrame", avgTimePerFrame );
            DirectShowUtil.DebugWrite("Capture.FrameRate done :{0}", FrameRate);
          }
          catch(Exception)
          {
            DirectShowUtil.DebugWrite("FrameRate:failed to set framerate to:{0}", value);
          }
        }
      }

      /// <summary>
      ///  Gets and sets the frame size used to capture video.
      /// </summary>
      /// <remarks>
      ///  To change the frame size, assign a new Size object 
      ///  to this property <code>capture.Size = new Size( w, h );</code>
      ///  rather than modifying the size in place 
      ///  (capture.Size.biWidth = w;). Not all frame
      ///  rates are supported.
      ///  
      /// <para>
      ///  Not all devices support getting/setting this property.
      ///  If this property is not supported, accessing it will
      ///  throw and exception. </para>
      /// 
      /// <para> 
      ///  This property cannot be changed while capturing. Changing 
      ///  this property while preview is enabled will cause some 
      ///  fickering while the internal filter graph is partially
      ///  rebuilt. Changing this property while cued will cancel the
      ///  cue. Call Cue() again to re-cue the capture. </para>
      /// </remarks>
      public Size FrameSize
      {
        get
        {
          if (videoStreamConfig==null) return new Size(720,576);
          try
          {
            DsBITMAPINFOHEADER bmiHeader;
            bmiHeader = (DsBITMAPINFOHEADER)getStreamConfigSetting( videoStreamConfig, "BmiHeader" ) ;
            Size size = new Size( bmiHeader.Width, bmiHeader.Height);
            DirectShowUtil.DebugWrite("Framesize returns:{0}x{1}",size.Width,size.Height);
            return( size );
          }
          catch(Exception)
          {
          }
          return  new Size(720,576);
        }
        set
        {
          if (videoStreamConfig==null) return ;
          try
          {
            //DirectShowUtil.DebugWrite("Capture.Framesize set to {0}x{1}",value.Width,value.Height);
            DsBITMAPINFOHEADER bmiHeader;
            object obj= getStreamConfigSetting( videoStreamConfig, "BmiHeader" );
            //DirectShowUtil.DebugWrite("Capture.Framesize {0} {1}",obj.GetType().ToString(), obj.ToString());
            bmiHeader = (DsBITMAPINFOHEADER)obj;

            DirectShowUtil.DebugWrite("change Framesize :{0}x{1} ->{2}x{3}",bmiHeader.Width,bmiHeader.Height, value.Width,value.Height);
            bmiHeader.Width = value.Width;
            bmiHeader.Height = value.Height;
            //DirectShowUtil.DebugWrite("set Framesize {0}x{1}",bmiHeader.Width,bmiHeader.Height);
            setStreamConfigSetting( videoStreamConfig, "BmiHeader", bmiHeader );
            Size size=FrameSize;
            if (size.Width!=value.Width || size.Height!= value.Height)
            {
              DirectShowUtil.DebugWrite("FAILED:Framesize not changed!");
            }
          }
          catch(Exception ex)
          {
            DirectShowUtil.DebugWrite("Capture.Framesize set to {0}x{1} FAILED {2} {3}",
                value.Width,value.Height, ex.Message,ex.Source);
          }
        }		
      }

      /// <summary>
      ///  Get or set the number of channels in the waveform-audio data. 
      /// </summary>
      /// <remarks>
      ///  Monaural data uses one channel and stereo data uses two channels. 
      ///  
      /// <para>
      ///  Not all devices support getting/setting this property.
      ///  If this property is not supported, accessing it will
      ///  throw and exception. </para>
      ///  
      /// <para>
      ///  This property cannot be changed while capturing. Changing 
      ///  this property while preview is enabled will cause some 
      ///  fickering while the internal filter graph is partially
      ///  rebuilt. Changing this property while cued will cancel the
      ///  cue. Call Cue() again to re-cue the capture. </para>
      /// </remarks>
      public short AudioChannels
      {
        get
        {
          if (audioStreamConfig==null) return 2;
          try
          {
            short audioChannels = (short) getStreamConfigSetting( audioStreamConfig, "nChannels" );
            return( audioChannels );
          }
          catch(Exception)
          {
            return 2;
          }
        }
        set
        {
          if (audioStreamConfig==null) return ;
          setStreamConfigSetting( audioStreamConfig, "nChannels", value );
        }
      }

      /// <summary>
      ///  Get or set the number of audio samples taken per second.
      /// </summary>
      /// <remarks>
      ///  Common sampling rates are 8.0 kHz, 11.025 kHz, 22.05 kHz, and 
      ///  44.1 kHz. Not all sampling rates are supported.
      ///  
      /// <para>
      ///  Not all devices support getting/setting this property.
      ///  If this property is not supported, accessing it will
      ///  throw and exception. </para>
      ///  
      /// <para>
      ///  This property cannot be changed while capturing. Changing 
      ///  this property while preview is enabled will cause some 
      ///  fickering while the internal filter graph is partially
      ///  rebuilt. Changing this property while cued will cancel the
      ///  cue. Call Cue() again to re-cue the capture. </para>
      /// </remarks>
      public int AudioSamplingRate
      {
        get
        {
          if (audioStreamConfig==null) return 441000;
          try
          {
            int samplingRate = (int) getStreamConfigSetting( audioStreamConfig, "nSamplesPerSec" );
            return( samplingRate );
          }
          catch(Exception)
          {
          }
          return 441000;
        }
        set
        {
          if (audioStreamConfig==null) return ;
          setStreamConfigSetting( audioStreamConfig, "nSamplesPerSec", value );
        }
      }

      /// <summary>
      ///  Get or set the number of bits recorded per sample. 
      /// </summary>
      /// <remarks>
      ///  Common sample sizes are 8 bit and 16 bit. Not all
      ///  samples sizes are supported.
      ///  
      /// <para>
      ///  Not all devices support getting/setting this property.
      ///  If this property is not supported, accessing it will
      ///  throw and exception. </para>
      ///  
      /// <para>
      ///  This property cannot be changed while capturing. Changing 
      ///  this property while preview is enabled will cause some 
      ///  fickering while the internal filter graph is partially
      ///  rebuilt. Changing this property while cued will cancel the
      ///  cue. Call Cue() again to re-cue the capture. </para>
      /// </remarks>
      public short AudioSampleSize
      {
        get
        {
          if (audioStreamConfig==null) return 16;
          try
          {
            short sampleSize = (short) getStreamConfigSetting( audioStreamConfig, "wBitsPerSample" );
            return( sampleSize );
          }
          catch(Exception)
          {
          }
          return 16;
        }
        set
        {
          if (audioStreamConfig==null) return ;
          setStreamConfigSetting( audioStreamConfig, "wBitsPerSample", value );
        }
      }


      // --------------------- Events ----------------------

      /// <summary> Fired when a capture is completed (manually or automatically). </summary>
      public event EventHandler					CaptureComplete;



      // ------------- Protected/private Properties --------------

      protected GraphState		graphState = GraphState.Null;		// State of the internal filter graph
      protected bool				isPreviewRendered = false;			// When graphState==Rendered, have we rendered the preview stream?
      protected bool				isCaptureRendered = false;			// When graphState==Rendered, have we rendered the capture stream?
      protected bool				wantPreviewRendered = false;		// Do we need the preview stream rendered (VideoDevice and PreviewWindow != null)
      protected bool				wantCaptureRendered = false;		// Do we need the capture stream rendered

      protected int				rotCookie = 0;						// Cookie into the Running Object Table
      protected Filter			videoDevice = null;					// Property Backer: Video capture device filter
      protected Filter			audioDevice = null;					// Property Backer: Audio capture device filter
      protected Filter			videoCompressor = null;				// Property Backer: Video compression filter
      protected Filter			audioCompressor = null;				// Property Backer: Audio compression filter
      protected string			filename = "";						// Property Backer: Name of file to capture to
      protected Control			previewWindow = null;				// Property Backer: Owner control for preview
      protected VideoCapabilities	videoCaps = null;					// Property Backer: capabilities of video device
      protected AudioCapabilities	audioCaps = null;					// Property Backer: capabilities of audio device
      protected SourceCollection	videoSources = null;				// Property Backer: list of physical video sources
      protected SourceCollection	audioSources = null;				// Property Backer: list of physical audio sources
      protected PropertyPageCollection propertyPages = null;			// Property Backer: list of property pages exposed by filters
      protected Tuner				tuner = null;						// Property Backer: TV Tuner

      protected IGraphBuilder		graphBuilder;						// DShow Filter: Graph builder 
      protected IStreamBufferSource streamBufferSource;						// DShow Filter: Graph builder 
      protected IMediaControl		mediaControl;						// DShow Filter: Start/Stop the filter graph -> copy of graphBuilder
      protected IVideoWindow		videoWindow;						// DShow Filter: Control preview window -> copy of graphBuilder
      private IBasicVideo2			basicVideo=null;
      protected ICaptureGraphBuilder2		captureGraphBuilder = null;	// DShow Filter: building graphs for capturing video
      protected IAMStreamConfig	videoStreamConfig = null;			// DShow Filter: configure frame rate, size
      protected IAMStreamConfig	audioStreamConfig = null;			// DShow Filter: configure sample rate, sample size
      protected IBaseFilter		videoDeviceFilter = null;			// DShow Filter: selected video device
      protected IBaseFilter		videoCompressorFilter = null;		// DShow Filter: selected video compressor
      protected IBaseFilter		audioDeviceFilter = null;			// DShow Filter: selected audio device
      protected IBaseFilter		audioCompressorFilter = null;		// DShow Filter: selected audio compressor
      protected IBaseFilter		muxFilter = null;					// DShow Filter: multiplexor (combine video and audio streams)
      protected IFileSinkFilter	fileWriterFilter = null;			// DShow Filter: file writer
      protected IBaseFilter    filewriterWinTVCap		=null;
      protected bool           m_bGraphRunning=false;
      protected IAMVideoProcAmp m_videoprocamp=null;
      VideoCaptureDevice        m_videoCaptureDevice=null;
      MPEG2Demux                m_mpeg2Demux=null;
      

      VideoProcAmp m_videoAmp=null;
      Rectangle m_rect = new Rectangle(10,10,100,100);
      // ------------- Constructors/Destructors --------------

      /// <summary> 
      ///  Create a new Capture object. 
      ///  videoDevice and audioDevice can be null if you do not 
      ///  wish to capture both audio and video. However at least
      ///  one must be a valid device. Use the <see cref="Filters"/> 
      ///  class to list available devices.
      ///  </summary>
      public Capture(Filter videoDevice, Filter audioDevice)
      {
        DirectShowUtil.DebugWrite("------------ Create new capture ------------ ");
        if ( videoDevice == null && audioDevice == null )
        {
          DirectShowUtil.DebugWrite("The VideoDevice and/or the audioDevice parameter must be set to a valid Filter." );
          throw new ArgumentException( "The videoDevice and/or the audioDevice parameter must be set to a valid Filter.\n" );
        }
        this.videoDevice = videoDevice;
        this.audioDevice = audioDevice;
        this.Filename = getTempFilename();
        if (videoDevice!=null)
        {
          DirectShowUtil.DebugWrite("Using video device:"+ videoDevice.Name);
        }
        if (audioDevice!=null)
        {
          DirectShowUtil.DebugWrite("Using audio device:"+ audioDevice.Name);
        }
      }

      /// <summary> Destructor. Dispose of resources. </summary>
      ~Capture()
      {
        DirectShowUtil.DebugWrite("~Capture");
        Dispose();
      }


      // --------------------- Public Methods -----------------------

      /// <summary>
      ///  Prepare for capturing. Use this method when capturing 
      ///  must begin as quickly as possible. 
      /// </summary>
      /// <remarks>
      ///  This will create/overwrite a zero byte file with 
      ///  the name set in the Filename property. 
      ///  
      /// <para>
      ///  This will disable preview. Preview will resume
      ///  once capture begins. This problem can be fixed
      ///  if someone is willing to make the change. </para>
      ///  
      /// <para>
      ///  This method is optional. If Cue() is not called, 
      ///  Start() will call it before capturing. This method
      ///  cannot be called while capturing. </para>
      /// </remarks>
      public void Cue()
      {
        assertStopped();

        // We want the capture stream rendered
        wantCaptureRendered = true;

        // Re-render the graph (if necessary)
        renderGraph();

        // Pause the graph
        int hr = mediaControl.Pause();
        if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr ); 
      }

      /// <summary> Begin capturing. </summary>
      public void Start()
      {
        DirectShowUtil.DebugWrite("Capture.Start");
        
        DirectShowUtil.DebugWrite(" start graph");
        assertStopped();

        // We want the capture stream rendered
        wantCaptureRendered = true;

        // Re-render the graph (if necessary)
        renderGraph();

        // Start the filter graph: begin capturing
        int hr = mediaControl.Run();
        if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr ); 
        

        // Update the state
        graphState = GraphState.Capturing;
        m_bGraphRunning=true;
      }

      /// <summary> 
      ///  Stop the current capture capture. If there is no
      ///  current capture, this method will succeed.
      /// </summary>
      public void Stop()
      {
        DirectShowUtil.DebugWrite("Capture.Stop");
        wantCaptureRendered = false;

        // Stop the graph if it is running
        // If we have a preview running we should only stop the
        // capture stream. However, if we have a preview stream
        // we need to re-render the graph anyways because we 
        // need to get rid of the capture stream. To re-render
        // we need to stop the entire graph
        if ( mediaControl != null )
        {
          if (m_bGraphRunning) mediaControl.Stop();
          m_bGraphRunning=false;
        }

        // Update the state
        if ( graphState == GraphState.Capturing )
        {
          graphState = GraphState.Rendered;
          if ( CaptureComplete != null )
            CaptureComplete( this, null );
        }

        // So we destroy the capture stream IF 
        // we need a preview stream. If we don't
        // this will leave the graph as it is.
        try { renderGraph(); } 
        catch {}
        try { startPreviewIfNeeded(); } 
        catch {}
      }

      /// <summary> 
      ///  Calls Stop, releases all references. If a capture is in progress
      ///  it will be stopped, but the CaptureComplete event will NOT fire.
      /// </summary>
      public void Dispose()
      {
        DirectShowUtil.DebugWrite("Capture.Dispose");
        wantPreviewRendered = false;
        wantCaptureRendered = false;
        CaptureComplete = null;

        try { destroyGraph(); } 
        catch {}

        if ( videoSources != null )
          videoSources.Dispose(); videoSources = null;
        if ( audioSources != null )
          audioSources.Dispose(); audioSources = null;

      }



      // --------------------- Private Methods -----------------------
		
      /// <summary> 
      ///  Create a new filter graph and add filters (devices, compressors, 
      ///  misc), but leave the filters unconnected. Call renderGraph()
      ///  to connect the filters.
      /// </summary>
      protected void createGraph()
      {
        Guid					cat;
        int						hr;

        // Ensure required properties are set
        if ( videoDevice == null && audioDevice == null )
        {
          DirectShowUtil.DebugWrite("The Video and/or audio device have not been set. Please set one or both to valid capture devices" );
          throw new ArgumentException( "The video and/or audio device have not been set. Please set one or both to valid capture devices.\n" );
        }
        // Skip if we are already created
        if ( (int)graphState < (int)GraphState.Created )
        {
        
          DirectShowUtil.DebugWrite("Capture.Create graph");
          // Garbage collect, ensure that previous filters are released
          GC.Collect();

          // Make a new filter graph
          DirectShowUtil.DebugWrite("create new filter graph (IGraphBuilder)");
          graphBuilder = (IGraphBuilder) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) ); 

          // Get the Capture Graph Builder
          DirectShowUtil.DebugWrite("Get the Capture Graph Builder (ICaptureGraphBuilder2)");
          Guid clsid = Clsid.CaptureGraphBuilder2;
          Guid riid = typeof(ICaptureGraphBuilder2).GUID;
          captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance( ref clsid, ref riid ); 

          // Link the CaptureGraphBuilder to the filter graph
          DirectShowUtil.DebugWrite("Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
          hr = captureGraphBuilder.SetFiltergraph( graphBuilder );
          if( hr < 0 ) 
          {
            DirectShowUtil.DebugWrite("link FAILED");
            Marshal.ThrowExceptionForHR( hr );
          }

          /*
                  // see
                  // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/directx9_c/directX/htm/creatingthestreambuffergraphs.asp
                  //
                  // 2. Add the Stream Buffer Source filter to the graph.
                  streamBufferSource = (IStreamBufferSource) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.StreamBufferSource, true ) ); 
                  graphBuilder.AddFilter(streamBufferSource);

                  // 3. Set the stream sink.
                  // Call IStreamBufferSource::SetStreamSink with a pointer to the sink filter's 
                  // IStreamBufferSink interface. With this method, the sink graph and the source 
                  // graph must share the same process space. 
                  streamBufferSource.SetStreamSink(StreamBufferSink);
        
        
                  //4. Enumerate the output pins on the Stream Buffer Source filter
                  //5. Call IGraphBuilder::Render on each pin to render the stream. 
                  IEnumPins pinEnum;
                  streamBufferSource.EnumPins(out pinEnum);
                  if( (hr == 0) && (pinEnum != null) )
                  {
                    pinEnum.Reset();
                    IPin[] pins = new IPin[1];
                    int f;
                    do
                    {
                      // Get the next pin
                      hr = pinEnum.Next( 1, pins, out f );
                      if( (hr == 0) && (pins[0] != null) )
                      {
                        PinDirection pinDir;
                        pins[0].QueryDirection(out pinDir);
                        if (pinDir==PinDirection.Output)
                        {
                          streamBufferSource.Render(pinDir);
                        }
                      }
                      Marshal.ReleaseComObject( pins[0] );
                    }
                  }
                  while( hr == 0 );
      

          */
          // Add the graph to the Running Object Table so it can be
          // viewed with GraphEdit
          DirectShowUtil.DebugWrite("Add graph to ROT table");
          DsROT.AddGraphToRot( graphBuilder, out rotCookie );
				
          // Get the video device and add it to the filter graph
          if ( VideoDevice != null )
          {
            DirectShowUtil.DebugWrite("Add Videodevice to filtergraph:"+VideoDevice.Name);
            videoDeviceFilter = Marshal.BindToMoniker( VideoDevice.MonikerString ) as IBaseFilter;
            if (videoDeviceFilter!=null)
            {
              hr = graphBuilder.AddFilter( videoDeviceFilter, "Video Capture Device" );
              if( hr < 0 ) 
              {
                DirectShowUtil.DebugWrite("FAILED:Add Videodevice to filtergraph");
                return;
              }
            }
            else
            {
              DirectShowUtil.DebugWrite("FAILED:Unable to add video device:{0}", VideoDevice.Name);
              return;
            }
          }

          // Get the audio device and add it to the filter graph
          if ( AudioDevice != null )
          {
            DirectShowUtil.DebugWrite("Add audiodevice to filtergraph:"+AudioDevice.Name);
            audioDeviceFilter =  Marshal.BindToMoniker( AudioDevice.MonikerString ) as IBaseFilter;
            if (audioDeviceFilter!=null)
            {
              hr = graphBuilder.AddFilter( audioDeviceFilter, "Audio Capture Device" );
              if( hr < 0 ) 
              {
                DirectShowUtil.DebugWrite("FAILED:Add audiodevice to filtergraph");
                return;
              }
            }
            else
            {
              DirectShowUtil.DebugWrite("FAILED:Unable to add audio device:{0}", AudioDevice.Name);
              return;
            }
          }

          // Get the video compressor and add it to the filter graph
          if ( VideoCompressor != null )
          {
            DirectShowUtil.DebugWrite("Add videoCompressor to filtergraph:"+VideoCompressor.Name);
            videoCompressorFilter = (IBaseFilter) Marshal.BindToMoniker( VideoCompressor.MonikerString ); 
            hr = graphBuilder.AddFilter( videoCompressorFilter, "Video Compressor" );
            if( hr < 0 ) 
            {
              DirectShowUtil.DebugWrite("FAILED:Add m_videoCompressor to filtergraph");
              Marshal.ThrowExceptionForHR( hr );
            }
          }

          // Get the audio compressor and add it to the filter graph
          if ( AudioCompressor != null )
          {
            DirectShowUtil.DebugWrite("Add AudioCompressor to filtergraph:"+AudioCompressor.Name);
            audioCompressorFilter = (IBaseFilter) Marshal.BindToMoniker( AudioCompressor.MonikerString ); 
            hr = graphBuilder.AddFilter( audioCompressorFilter, "Audio Compressor" );
            if( hr < 0 ) 
            { 
              DirectShowUtil.DebugWrite("FAILED:Add AudioCompressor to filtergraph");
              Marshal.ThrowExceptionForHR( hr );
            }
          }


          // Retrieve the stream control interface for the video device
          // FindInterface will also add any required filters
          // (WDM devices in particular may need additional
          // upstream filters to function).

          // Try looking for an interleaved media type
          DirectShowUtil.DebugWrite("get Video stream control interface (IAMStreamConfig)");
          object o;
          cat = PinCategory.Capture;
          Guid iid = typeof(IAMStreamConfig).GUID;
          hr = captureGraphBuilder.FindInterface(new Guid[1]{cat}, null, videoDeviceFilter, ref iid, out o );
          if ( hr != 0 )
          {
            o=null;
            cat = PinCategory.Preview;
            iid = typeof(IAMStreamConfig).GUID;
            hr = captureGraphBuilder.FindInterface(new Guid[1]{cat}, null, videoDeviceFilter, ref iid, out o );
            if ( hr != 0 )
            {
              o=null;
              cat = PinCategory.VideoPort;
              iid = typeof(IAMStreamConfig).GUID;
              hr = captureGraphBuilder.FindInterface(new Guid[1]{cat}, null, videoDeviceFilter, ref iid, out o );
              if ( hr != 0 )
              {
                o=null;
              }
            }
          }
          videoStreamConfig = o as IAMStreamConfig;
          if (videoStreamConfig ==null)
            DirectShowUtil.DebugWrite("FAILED:get Video stream control interface (IAMStreamConfig)");
          else
            DirectShowUtil.DebugWrite("got Video stream control interface (IAMStreamConfig)");
        
          if (audioDeviceFilter!=null)
          {
            // Retrieve the stream control interface for the audio device
            DirectShowUtil.DebugWrite("get audio stream control interface (IAMStreamConfig)");
            o = null;
            cat = PinCategory.Capture;
            iid = typeof(IAMStreamConfig).GUID;
            hr = captureGraphBuilder.FindInterface(new Guid[1]{ cat}, null, audioDeviceFilter, ref iid, out o );
            if ( hr != 0 )
            {
              o = null;
              cat = PinCategory.Preview;
              iid = typeof(IAMStreamConfig).GUID;
              hr = captureGraphBuilder.FindInterface(new Guid[1]{cat}, null, audioDeviceFilter, ref iid, out o );
              if ( hr != 0 )
              {
                o = null;
                cat = PinCategory.VideoPort;
                iid = typeof(IAMStreamConfig).GUID;
                hr = captureGraphBuilder.FindInterface(new Guid[1]{cat}, null, audioDeviceFilter, ref iid, out o );
                if ( hr != 0 )
                {
                  o=null;
                }
              }
            }

            audioStreamConfig = o as IAMStreamConfig;
            if (audioStreamConfig==null)
              DirectShowUtil.DebugWrite("FAILED:get audio stream control interface (IAMStreamConfig)");
            else
              DirectShowUtil.DebugWrite("got get audio stream control interface (IAMStreamConfig)");
          }
          else audioStreamConfig=null;

          // Retreive the media control interface (for starting/stopping graph)
          mediaControl = (IMediaControl) graphBuilder;

          // Reload any video crossbars
          if ( videoSources != null ) videoSources.Dispose(); videoSources = null;

          // Reload any audio crossbars
          if ( audioSources != null ) audioSources.Dispose(); audioSources = null;
				
          // Reload any property pages exposed by filters
          if ( propertyPages != null ) propertyPages.Dispose(); propertyPages = null;

          // Reload capabilities of video device
          videoCaps = null;

          // Reload capabilities of video device
          audioCaps = null;

          // Retrieve TV Tuner if available
          DirectShowUtil.DebugWrite("Find TV Tuner");
          o = null;
          cat = FindDirection.UpstreamOnly;
          iid = typeof(IAMTuner).GUID;
          hr = captureGraphBuilder.FindInterface( new Guid[1]{ cat}, null, videoDeviceFilter, ref iid, out o );
          if (hr==0) 
            DirectShowUtil.DebugWrite("Tuner Found (IAMTuner)");
          else
            DirectShowUtil.DebugWrite("FAILED: tuner(IAMTuner) not found:0x{0:x}",hr);

          o = null;
          cat = FindDirection.UpstreamOnly;
          iid = typeof(IAMTVTuner).GUID;
          hr = captureGraphBuilder.FindInterface( new Guid[1]{ cat}, null, videoDeviceFilter, ref iid, out o );
          if (hr!=0) 
            DirectShowUtil.DebugWrite("FAILED: tuner(IAMTVTuner) not found:0x{0:x}",hr);

          IAMTVTuner t = o as IAMTVTuner;
          if ( t != null )
          {
            DirectShowUtil.DebugWrite("TV Tuner Found");
            tuner = new Tuner( t );
          }
          else
          {
            DirectShowUtil.DebugWrite("FAILED:TV Tuner found");
          }



          m_videoprocamp=videoDeviceFilter as IAMVideoProcAmp;
          if (m_videoprocamp!=null)
          {
            m_videoAmp=new VideoProcAmp(m_videoprocamp);
          }

          m_videoCaptureDevice = new VideoCaptureDevice(graphBuilder,captureGraphBuilder, videoDeviceFilter);
          m_mpeg2Demux=null;
          if (m_videoCaptureDevice.MPEG2)
          {
          }

          // Update the state now that we are done
          graphState = GraphState.Created;
        }
      }

      /// <summary>
      ///  Connects the filters of a previously created graph 
      ///  (created by createGraph()). Once rendered the graph
      ///  is ready to be used. This method may also destroy
      ///  streams if we have streams we no longer want.
      /// </summary>
      protected void renderGraph()
      {
        Guid					cat;
        Guid					med;
        int						hr=0;
        bool					didSomething = false;
        const int WS_CHILD			= 0x40000000;	
        const int WS_CLIPCHILDREN	= 0x02000000;
        const int WS_CLIPSIBLINGS	= 0x04000000;

        DirectShowUtil.DebugWrite("Capture.renderGraph");
        assertStopped();

        // Ensure required properties set
        if ( filename == null )
        {
          DirectShowUtil.DebugWrite("capture:filename is not set");
          throw new ArgumentException( "The Filename property has not been set to a file.\n" );
        }
        // Stop the graph
        if ( mediaControl != null )
        {
          DirectShowUtil.DebugWrite("stop graph");
          if (m_bGraphRunning)
            mediaControl.Stop();
          m_bGraphRunning=false;
        }

        // Create the graph if needed (group should already be created)
        createGraph();

        // Derender the graph if we have a capture or preview stream
        // that we no longer want. We can't derender the capture and 
        // preview streams seperately. 
        // Notice the second case will leave a capture stream intact
        // even if we no longer want it. This allows the user that is
        // not using the preview to Stop() and Start() without
        // rerendering the graph.
        if ( !wantPreviewRendered && isPreviewRendered )
          derenderGraph();
        if ( !wantCaptureRendered && isCaptureRendered )
          if ( wantPreviewRendered )
            derenderGraph();


        // Render capture stream (only if necessary)
        if ( wantCaptureRendered && !isCaptureRendered )
        {
          DirectShowUtil.DebugWrite("  setup capture");
          bool bFileWriterAdded=false;
          if (Filename.Length>0 &&!bFileWriterAdded)
          {
            string strExt=System.IO.Path.GetExtension(Filename).ToLower();
            if (strExt==".avi" || strExt==".asf")
            {
              if ( strExt==".avi")
              {
                DirectShowUtil.DebugWrite("  SetOutputFileName({0}) to avi format",Filename);
                Guid mediaSubTypeAvi = MediaSubType.Avi;
                hr = captureGraphBuilder.SetOutputFileName( ref mediaSubTypeAvi, Filename, out muxFilter, out fileWriterFilter );
              }
              if (strExt==".asf")
              {
                DirectShowUtil.DebugWrite("  SetOutputFileName({0}) to asf format",Filename);
                Guid mediaSubTypeASF = MediaSubType.Asf;
                hr = captureGraphBuilder.SetOutputFileName( ref mediaSubTypeASF, Filename, out muxFilter, out fileWriterFilter );
              }
              if( hr >=0) 
              {
                DirectShowUtil.DebugWrite("  SetOutputFileName() succeeded");
              }
              if( hr < 0 ) 
              {
                // ok, try custom filter then
                DirectShowUtil.DebugWrite("  FAILED:setoutputfilename:{0}",Filename);
                Marshal.ThrowExceptionForHR( hr );
              }
            

              // Render video (video -> mux)
              if ( VideoDevice != null )
              {
                // Try interleaved first, because if the device supports it,
                // it's the only way to get audio as well as video
                DirectShowUtil.DebugWrite("  render Video stream");
                //DirectShowUtil.DebugWrite("    try interleaved");
                cat = PinCategory.Capture;
                med = MediaType.Interleaved;
								hr = captureGraphBuilder.RenderStream( new Guid[1]{ cat}, null, videoDeviceFilter, videoCompressorFilter, muxFilter ); 
								if( hr < 0 ) 
								{
									DirectShowUtil.DebugWrite("    FAILED");
								}
								else 
								{
									bFileWriterAdded=true;
									DirectShowUtil.DebugWrite("  succeeded");
								}
									/*
								hr = captureGraphBuilder.RenderStream( new Guid[1]{ cat}, new Guid[1]{ med}, videoDeviceFilter, videoCompressorFilter, muxFilter ); 
                if( hr < 0 ) 
                {
                  DirectShowUtil.DebugWrite("    FAILED,try video");
                  cat = PinCategory.Capture;
                  med = MediaType.Video;
									hr = captureGraphBuilder.RenderStream( new Guid[1]{  cat}, new Guid[1]{  med}, videoDeviceFilter, videoCompressorFilter, muxFilter ); 
                  if( hr < 0 ) 
                  {
                    DirectShowUtil.DebugWrite("    FAILED,try stream");
                    med = MediaType.Stream;
										hr = captureGraphBuilder.RenderStream( new Guid[1]{  cat}, new Guid[1]{  med}, videoDeviceFilter, videoCompressorFilter, muxFilter ); 
                    if( hr < 0 ) 
                    {
                      DirectShowUtil.DebugWrite("  FAILED:render Video stream");
                      Marshal.ThrowExceptionForHR( hr );
                    }
                    else 
                    {
                      bFileWriterAdded=true;
                      DirectShowUtil.DebugWrite("  succeeded");
                    }
                  }
                  else 
                  {
                    bFileWriterAdded=true;
                    DirectShowUtil.DebugWrite("  succeeded");
                  }
                }
                else 
                {
                  bFileWriterAdded=true;
                  DirectShowUtil.DebugWrite("  succeeded");
                }*/
              }

              // Render audio (audio -> mux)
              if ( AudioDevice != null )
              {
                DirectShowUtil.DebugWrite("  render audio stream");
                cat = PinCategory.Capture;
                med = MediaType.Audio;
								hr = captureGraphBuilder.RenderStream( new Guid[1]{ cat}, null/*new Guid[1]{  med}*/, audioDeviceFilter, audioCompressorFilter, muxFilter );
                if( hr < 0 ) 
                {
                  DirectShowUtil.DebugWrite("  FAILED:render audio stream");
                  Marshal.ThrowExceptionForHR( hr );
                }
                else
                {
                  DirectShowUtil.DebugWrite("  succeeded");
                }
              }
            }
          }

          if (Filename.Length>0 &&!bFileWriterAdded)
          {
            if (!bFileWriterAdded)
            {
              // Render the file writer portion of graph (mux -> file)
              //first add filewriter to graph
              DirectShowUtil.DebugWrite("  Add wintvcapture filewriter");
              filewriterWinTVCap= DirectShowUtil.AddFilterToGraph(graphBuilder,"WinTVCapWriter");
              if (filewriterWinTVCap!=null)
              {
                // got it, next set filename for the filewriter
                AMMediaType amtype = new AMMediaType();
                amtype.majorType=MediaType.Stream;
                amtype.subType=MediaSubType.MPEG2Program;

                DirectShowUtil.DebugWrite("  set filename:{0}", Filename);
                fileWriterFilter=(IFileSinkFilter)filewriterWinTVCap;
                fileWriterFilter.SetFileName(Filename,ref amtype);

                cat=PinCategory.Capture;
                med = MediaType.Stream;
								hr = captureGraphBuilder.RenderStream( new Guid[1]{ cat}, new Guid[1]{ med}, videoDeviceFilter, null, filewriterWinTVCap); 
                if (hr >=0)
                {
                  bFileWriterAdded=true;
                  DirectShowUtil.DebugWrite("  connect capture->filewriter ok");          
                }
                else
                {
                  DirectShowUtil.DebugWrite("  FAILED:connect capture->filewriter ");          
                }
              }
            }//if (filewriterWinTVCap!=null)
          }

          isCaptureRendered = true;
          didSomething = true;
        }

        // Render preview stream (only if necessary)
        if ( wantPreviewRendered && !isPreviewRendered )
        {
          DirectShowUtil.DebugWrite("  setup previewing");
          bool bPreviewOK=false;
          // Render preview
          if (m_videoCaptureDevice.MPEG2)
          {
            if (m_mpeg2Demux ==null)
            {
              m_mpeg2Demux = new MPEG2Demux(ref graphBuilder, new Size(720,576));
            }
            if (!m_mpeg2Demux.IsRendered)
            {
              // connect video capture pin->mpeg2 demux input
              cat = PinCategory.Capture;
              med = MediaType.Stream;
							hr = captureGraphBuilder.RenderStream( new Guid[1]{ cat}, null/*new Guid[1]{ med}*/, videoDeviceFilter, null, m_mpeg2Demux.BaseFilter); 
              if (hr==0)
              {
                m_mpeg2Demux.StartTimeshifting(Filename);
              }
            }
            bPreviewOK=true;
            isPreviewRendered = true;
            didSomething = true;
          }
          else
          {
            bPreviewOK=m_videoCaptureDevice.RenderPreview();
          

            if (bPreviewOK)
            {
              // Get the IVideoWindow interface
              videoWindow = (IVideoWindow) graphBuilder;
              basicVideo  = (IBasicVideo2) graphBuilder;
              // Set the video window to be a child of the main window
              DirectShowUtil.DebugWrite("set Video window");
              hr = videoWindow.put_Owner( previewWindow.Handle );
              if( hr < 0 ) 
              {
                DirectShowUtil.DebugWrite("FAILED:set Video window");
                Marshal.ThrowExceptionForHR( hr );
              }

              // Set Video window style
              DirectShowUtil.DebugWrite("set Video window style");
              hr = videoWindow.put_WindowStyle( WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
              if( hr < 0 ) 
              {
                DirectShowUtil.DebugWrite("FAILED:set Video window style");
                Marshal.ThrowExceptionForHR( hr );
              }

              // Position Video window in client rect of owner window
              DirectShowUtil.DebugWrite("position Videowindow :({0},{1})-({2},{3})",
                m_rect.Left,m_rect.Top,
                m_rect.Right,m_rect.Bottom);
              //previewWindow.Resize += new EventHandler( onPreviewWindowResize );
              //onPreviewWindowResize( this, null );

              //basicVideo.SetDefaultSourcePosition();
              //basicVideo.SetDefaultDestinationPosition();
              //basicVideo.SetDestinationPosition(m_rect.Left,m_rect.Top,m_rect.Width,m_rect.Height);
              videoWindow.SetWindowPosition(m_rect.Left,m_rect.Top,m_rect.Width,m_rect.Height);

              // Make the video window visible, now that it is properly positioned
              DirectShowUtil.DebugWrite("set Video window to visible");
              hr = videoWindow.put_Visible( DsHlp.OATRUE );
              if( hr < 0 ) 
              {
                DirectShowUtil.DebugWrite("FAILED: set Video window visible");
                Marshal.ThrowExceptionForHR( hr );
              }

              /*            DirectShowUtil.DebugWrite("show Video window");
                          videoWindow.put_AutoShow( DsHlp.OATRUE );
                          if( hr < 0 ) 
                          {
                            DirectShowUtil.DebugWrite("FAILED: show Video window");
                            Marshal.ThrowExceptionForHR( hr );
                          }*/
              isPreviewRendered = true;
              didSomething = true;
            }
          }
        }

        if ( didSomething )
          graphState = GraphState.Rendered;
      }

      /// <summary>
      ///  Setup and start the preview window if the user has
      ///  requested it (by setting PreviewWindow).
      /// </summary>
      protected void startPreviewIfNeeded()
      {
        DirectShowUtil.DebugWrite("Capture.startPreviewIfNeeded");
        // Render preview 
        if ( wantPreviewRendered && isPreviewRendered && !isCaptureRendered )
        {
          // Run the graph (ignore errors)
          // We can run the entire graph becuase the capture
          // stream should not be rendered (and that is enforced
          // in the if statement above)
          DirectShowUtil.DebugWrite("Run graph");
          int hr=mediaControl.Run();
          if (hr!=0)
            DirectShowUtil.DebugWrite("FAILED to Run graph");
          m_bGraphRunning=true;
        }
      }

      /// <summary>
      ///  Disconnect and remove all filters except the device
      ///  and compressor filters. This is the opposite of
      ///  renderGraph(). Soem properties such as FrameRate
      ///  can only be set when the device output pins are not
      ///  connected. 
      /// </summary>
      protected void derenderGraph()
      {
        
        DirectShowUtil.DebugWrite("Capture.derenderGraph");
        // Stop the graph if it is running (ignore errors)
        if ( mediaControl != null )
        {
          if (m_bGraphRunning)
            mediaControl.Stop();
          m_bGraphRunning=false;
        }
        // Free the preview window (ignore errors)
        if ( videoWindow != null )
        {
          videoWindow.put_Visible( DsHlp.OAFALSE );
          videoWindow.put_Owner( IntPtr.Zero );
          videoWindow = null;
        }
        basicVideo=null;


        if ( (int)graphState >= (int)GraphState.Rendered )
        {
          // Update the state
          DirectShowUtil.DebugWrite("Capture.derenderGraph rendered->created");
          graphState = GraphState.Created;
          isCaptureRendered = false;
          isPreviewRendered = false;

          // Disconnect all filters downstream of the 
          // video and audio devices. If we have a compressor
          // then disconnect it, but don't remove it
          if ( videoDeviceFilter != null )
            removeDownstream( videoDeviceFilter, (videoCompressor==null) );
          if ( audioDeviceFilter != null )
            removeDownstream( audioDeviceFilter, (audioCompressor==null) );

          // These filters should have been removed by the
          // calls above. (Is there anyway to check?)
          muxFilter = null;
          fileWriterFilter = null;
          filewriterWinTVCap=null;
          

        }
      }

      /// <summary>
      ///  Removes all filters downstream from a filter from the graph.
      ///  This is called only by derenderGraph() to remove everything
      ///  from the graph except the devices and compressors. The parameter
      ///  "removeFirstFilter" is used to keep a compressor (that should
      ///  be immediately downstream of the device) if one is begin used.
      /// </summary>
      protected void removeDownstream( IBaseFilter filter, bool removeFirstFilter )
      {
        DirectShowUtil.DebugWrite("Capture.removeDownstream");
        // Get a pin enumerator off the filter
        IEnumPins pinEnum;
        int hr = filter.EnumPins( out pinEnum );
        pinEnum.Reset();
        if( (hr == 0) && (pinEnum != null) )
        {
          // Loop through each pin
          IPin[] pins = new IPin[1];
          int f;
          do
          {
            // Get the next pin
            hr = pinEnum.Next( 1, pins, out f );
            if( (hr == 0) && (pins[0] != null) )
            {
              // Get the pin it is connected to
              IPin pinTo = null;
              pins[0].ConnectedTo( out pinTo );
              if ( pinTo != null )
              {
                // Is this an input pin?
                PinInfo info = new PinInfo();
                hr = pinTo.QueryPinInfo( out info );
                if( (hr == 0) && (info.dir == (PinDirection.Input)) )
                {
                  // Recurse down this branch
                  removeDownstream( info.filter, true );

                  // Disconnect 
                  graphBuilder.Disconnect( pinTo );
                  graphBuilder.Disconnect( pins[0] );

                  // Remove this filter
                  // but don't remove the video or audio compressors
                  if ( ( info.filter != videoCompressorFilter ) &&
                    ( info.filter != audioCompressorFilter ) )
                    graphBuilder.RemoveFilter( info.filter );
                }
                Marshal.ReleaseComObject( info.filter );
                Marshal.ReleaseComObject( pinTo );
              }
              Marshal.ReleaseComObject( pins[0] );
            }
          }
          while( hr == 0 );

          Marshal.ReleaseComObject( pinEnum ); pinEnum = null;
        }
      }

      /// <summary>
      ///  Completely tear down a filter graph and 
      ///  release all associated resources.
      /// </summary>
      protected void destroyGraph()
      {
        
        DirectShowUtil.DebugWrite("Capture.destroyGraph");
        // Derender the graph (This will stop the graph
        // and release preview window. It also destroys
        // half of the graph which is unnecessary but
        // harmless here.) (ignore errors)
        try{ derenderGraph(); } 
        catch {}

        // Update the state after derender because it
        // depends on correct status. But we also want to
        // update the state as early as possible in case
        // of error.
        graphState = GraphState.Null;
        isCaptureRendered = false;
        isPreviewRendered = false;

        // Remove graph from the ROT
        if ( rotCookie != 0 )
        {
          DsROT.RemoveGraphFromRot( ref rotCookie );
          rotCookie = 0;
        }

        // Remove filters from the graph
        // This should be unnecessary but the Nvidia WDM
        // video driver cannot be used by this application 
        // again unless we remove it. Ideally, we should
        // simply enumerate all the filters in the graph
        // and remove them. (ignore errors)
        if ( muxFilter != null )
          graphBuilder.RemoveFilter( muxFilter );
        if ( videoCompressorFilter != null )
          graphBuilder.RemoveFilter( videoCompressorFilter  );
        if ( audioCompressorFilter != null )
          graphBuilder.RemoveFilter( audioCompressorFilter  );
        if ( videoDeviceFilter != null )
          graphBuilder.RemoveFilter( videoDeviceFilter );
        if ( audioDeviceFilter != null )
          graphBuilder.RemoveFilter( audioDeviceFilter );

        // Clean up properties
        if ( videoSources != null )
          videoSources.Dispose(); videoSources = null;
        if ( audioSources != null )
          audioSources.Dispose(); audioSources = null;
        if ( propertyPages != null )
          propertyPages.Dispose(); propertyPages = null;

				DsUtils.RemoveFilters(graphBuilder);
        if ( tuner != null )
          tuner.Dispose(); tuner = null;

        if (m_videoAmp!=null)
        {
          m_videoAmp.Dispose();m_videoAmp=null;
        }

        // Cleanup
        if ( streamBufferSource != null )
          Marshal.ReleaseComObject( streamBufferSource);  streamBufferSource= null;
        if ( graphBuilder != null )
          Marshal.ReleaseComObject( graphBuilder );  graphBuilder = null;
        if ( captureGraphBuilder != null )
          Marshal.ReleaseComObject( captureGraphBuilder ); captureGraphBuilder = null;
        if ( muxFilter != null )
          Marshal.ReleaseComObject( muxFilter ); muxFilter = null;
        if ( fileWriterFilter != null )
          Marshal.ReleaseComObject( fileWriterFilter ); fileWriterFilter = null;
        if ( filewriterWinTVCap != null)
          Marshal.ReleaseComObject( filewriterWinTVCap  ); filewriterWinTVCap  = null;
        
        if ( videoDeviceFilter != null )
          Marshal.ReleaseComObject( videoDeviceFilter ); videoDeviceFilter = null;
        if ( audioDeviceFilter != null )
          Marshal.ReleaseComObject( audioDeviceFilter ); audioDeviceFilter = null;
        if ( videoCompressorFilter != null )
          Marshal.ReleaseComObject( videoCompressorFilter ); videoCompressorFilter = null;
        if ( audioCompressorFilter != null )
          Marshal.ReleaseComObject( audioCompressorFilter ); audioCompressorFilter = null;

        // These are copies of graphBuilder
        mediaControl = null;
        videoWindow = null;
        m_videoprocamp=null;

        // For unmanaged objects we haven't released explicitly
        GC.Collect();
      }

      public VideoProcAmp VideoAmp
      {
        get { return m_videoAmp;}
      }

    
  
      /// <summary> 
      ///  Get a valid temporary filename (with path). We aren't using 
      ///  Path.GetTempFileName() because it creates a 0-byte file 
      /// </summary>
      protected string getTempFilename()
      {
        string s;
        try
        {
          int count = 0;
          int i;
          Random r = new Random();
          string tempPath = Path.GetTempPath();
          do
          {
            i = r.Next();
            s = Path.Combine( tempPath, i.ToString("X") + ".avi" );
            count++;
            if ( count > 100 ) throw new InvalidOperationException( "FAILED to find temporary file." );
          } while ( File.Exists( s ) );
        }
        catch { s = "c:\temp.avi"; }
        return( s );
      }

      /// <summary>
      ///  Retrieves the value of one member of the IAMStreamConfig format block.
      ///  Helper function for several properties that expose
      ///  video/audio settings from IAMStreamConfig.GetFormat().
      ///  IAMStreamConfig.GetFormat() returns a AMMediaType struct.
      ///  AMMediaType.formatPtr points to a format block structure.
      ///  This format block structure may be one of several 
      ///  types, the type being determined by AMMediaType.formatType.
      /// </summary>
      protected object getStreamConfigSetting( IAMStreamConfig streamConfig, string fieldName)
      {
        //DirectShowUtil.DebugWrite("Capture.getStreamConfigSetting:{0}",fieldName);
        object returnValue = null;
        try
        {
          if ( streamConfig == null )
            throw new NotSupportedException();
          assertStopped();
          derenderGraph();

          IntPtr pmt = IntPtr.Zero;
          AMMediaTypeClass mediaType = new AMMediaTypeClass();

          try 
          {
            // Get the current format info
            int hr = streamConfig.GetFormat(  out mediaType);
            if ( hr != 0 )
            {
              DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() FAILED to get:{0} (not supported)",fieldName);
              Marshal.ThrowExceptionForHR( hr );
            }
            // The formatPtr member points to different structures
            // dependingon the formatType
            object formatStruct;
            //DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() find formattype"); 
            if ( mediaType.formatType == FormatType.WaveEx )
              formatStruct = new WaveFormatEx();
            else if ( mediaType.formatType == FormatType.VideoInfo )
              formatStruct = new VideoInfoHeader();
            else if ( mediaType.formatType == FormatType.VideoInfo2 )
              formatStruct = new VideoInfoHeader2();
            else if ( mediaType.formatType == FormatType.Mpeg2Video)
              formatStruct = new MPEG2VideoInfo();
            else if ( mediaType.formatType == FormatType.None)
            {
              DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() FAILED no format returned");
              throw new NotSupportedException( "This device does not support a recognized format block." );
            }
            else
            {
              DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() FAILED unknown fmt:{0} {1} {2}",mediaType.formatType,mediaType.majorType,mediaType.subType);
              throw new NotSupportedException( "This device does not support a recognized format block." );
            }
            
            //DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() get formatptr");
            // Retrieve the nested structure
            Marshal.PtrToStructure( mediaType.formatPtr, formatStruct );

            // Find the required field
            //DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() get field");
            Type structType = formatStruct.GetType();
            FieldInfo fieldInfo = structType.GetField( fieldName );
            if ( fieldInfo == null )
            {
              DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() FAILED to to find member:{0}", fieldName);
              throw new NotSupportedException( "FAILED to find the member '" + fieldName + "' in the format block." );
            }
            
            // Extract the field's current value
            //DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() get value");
            returnValue = fieldInfo.GetValue( formatStruct ); 
  					//DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() done");	
          }
          finally
          {
            Marshal.FreeCoTaskMem( pmt );
          }
          renderGraph();
          startPreviewIfNeeded();
        }
        catch(Exception)
        {
          DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() FAILED ");
        }
        return( returnValue );
      }

      /// <summary>
      ///  Set the value of one member of the IAMStreamConfig format block.
      ///  Helper function for several properties that expose
      ///  video/audio settings from IAMStreamConfig.GetFormat().
      ///  IAMStreamConfig.GetFormat() returns a AMMediaType struct.
      ///  AMMediaType.formatPtr points to a format block structure.
      ///  This format block structure may be one of several 
      ///  types, the type being determined by AMMediaType.formatType.
      /// </summary>
      protected object setStreamConfigSetting( IAMStreamConfig streamConfig, string fieldName, object newValue)
      {
        //DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting:{0}",fieldName);
        try
        {
          assertStopped();
          derenderGraph();

          object returnValue = null;
          IntPtr pmt = IntPtr.Zero;
          AMMediaTypeClass mediaType = new AMMediaTypeClass();

          try 
          {
            // Get the current format info
            int hr = streamConfig.GetFormat( out mediaType );
            if ( hr != 0 )
            {
              DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() FAILED to set:{0} (getformat) hr:{1}",fieldName,hr);
              Marshal.ThrowExceptionForHR( hr );
            }
            //DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() get formattype");
            // The formatPtr member points to different structures
            // dependingon the formatType
            object formatStruct;
            if ( mediaType.formatType == FormatType.WaveEx )
              formatStruct = new WaveFormatEx();
            else if ( mediaType.formatType == FormatType.VideoInfo )
              formatStruct = new VideoInfoHeader();
            else if ( mediaType.formatType == FormatType.VideoInfo2 )
              formatStruct = new VideoInfoHeader2();
            else if ( mediaType.formatType == FormatType.Mpeg2Video)
              formatStruct = new MPEG2VideoInfo();
            else if ( mediaType.formatType == FormatType.None)
            {
              DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() FAILED no format returned");
              throw new NotSupportedException( "This device does not support a recognized format block." );
            }
            else
            {
              DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() FAILED unknown fmt");
              throw new NotSupportedException( "This device does not support a recognized format block." );
            }
            //DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() get formatptr");
            // Retrieve the nested structure
            Marshal.PtrToStructure( mediaType.formatPtr, formatStruct );

            // Find the required field
            //DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() get field");
            Type structType = formatStruct.GetType();
            FieldInfo fieldInfo = structType.GetField( fieldName );
            if ( fieldInfo == null )
            {
              DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() FAILED to to find member:{0}", fieldName);
              throw new NotSupportedException( "FAILED to find the member '" + fieldName + "' in the format block." );
            }
            //DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() set value");
            // Update the value of the field
            fieldInfo.SetValue( formatStruct, newValue );

            // PtrToStructure copies the data so we need to copy it back
            Marshal.StructureToPtr( formatStruct, mediaType.formatPtr, false ); 

            //DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() set format");
            // Save the changes
            hr = streamConfig.SetFormat( mediaType );
            if ( hr != 0 )
            {
              DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() FAILED to set:{0} {1}",fieldName,hr);
              Marshal.ThrowExceptionForHR( hr );
            }
            //else DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() set:{0}",fieldName);
            //DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() done");
          }
          finally
          {
            Marshal.FreeCoTaskMem( pmt );
          }
          renderGraph();
          startPreviewIfNeeded();

          return( returnValue );
        }
        catch (Exception)
        {
          DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() FAILED ");
        }
        return null;
      }

      /// <summary>
      ///  Assert that the class is in a Stopped state.
      /// </summary>
      protected void assertStopped()
      {
        if ( !Stopped )
        {
          DirectShowUtil.DebugWrite("This operation not allowed while Capturing. Please Stop the current capture.");
          throw new InvalidOperationException( "This operation not allowed while Capturing. Please Stop the current capture." ); 
        }
      }
      public bool Running
      {
        get { return m_bGraphRunning;}
      }

      public void FixCrossbarRouting(bool bTunerIn)
      {
        DirectShowUtil.DebugWrite("Capture.FixCrossbarRouting");
        createGraph();

        try
        {
          if (graphState >= GraphState.Created)
          {
            int iCrossBar=0;
            DirectShowUtil.DebugWrite("Set Crossbar routing. Tuner:{0}",bTunerIn);
            IBaseFilter searchfilter=videoDeviceFilter as IBaseFilter;
            while (true)
            {
              int  hr;
              Guid cat;
              Guid iid;
              Guid med=MediaType.Stream;
              object o=null;
              cat = FindDirection.UpstreamOnly;
              iid = typeof(IAMCrossbar).GUID;
              DirectShowUtil.DebugWrite(" Find crossbar:#{0}", iCrossBar);
              hr=captureGraphBuilder.FindInterface(new Guid[1]{cat},null,searchfilter, ref iid, out o);
              if (hr ==0 && o != null)
              {
                IAMCrossbar crossbar = o as IAMCrossbar;
                searchfilter=o as IBaseFilter;
                if (crossbar!=null)
                {
                  iCrossBar++;
                  DirectShowUtil.DebugWrite("  crossbar found:{0}",iCrossBar);
                  int iOutputPinCount, iInputPinCount;
                  crossbar.get_PinCounts(out iOutputPinCount, out iInputPinCount);
                  DirectShowUtil.DebugWrite("    crossbar:{0} inputs:{1}  outputs:{2}",crossbar.ToString(),iInputPinCount,iOutputPinCount);
                
                  int                   iPinIndexRelated;
                  int                   iPinIndexRelatedIn;
                  int                   iInputPin;
                  PhysicalConnectorType PhysicalTypeOut;
                  PhysicalConnectorType PhysicalTypeIn;


                  for (int iOut=0; iOut < iOutputPinCount; ++iOut)
                  {
                    bool bReRoute=false;
                    crossbar.get_CrossbarPinInfo(false,iOut,out iPinIndexRelated, out PhysicalTypeOut);
                    crossbar.get_IsRoutedTo(iOut, out iInputPin);
                    if (iInputPin>=0)
                    {
                      crossbar.get_CrossbarPinInfo(true,iInputPin,out iPinIndexRelatedIn, out PhysicalTypeIn);
                      DirectShowUtil.DebugWrite("      output:{0}->{1} / {2}->{3}", iInputPin,iOut,PhysicalTypeIn.ToString(),PhysicalTypeOut.ToString());
                      if (bTunerIn)
                      {
                        if (PhysicalTypeIn !=PhysicalConnectorType.Audio_Tuner)
                        {
                          bReRoute=true;
                        }
                      }
                      else
                      {
                        if (PhysicalTypeIn ==PhysicalConnectorType.Audio_Tuner)
                        {
                          bReRoute=true;
                        }
                      }
                    }
                    else
                    {
                      bReRoute=true;
                      DirectShowUtil.DebugWrite("      output:{0}->? / {1}->? : -", iOut, PhysicalTypeOut.ToString() );
                    }
                 
                    if (iInputPin==-1||bReRoute)
                    {
                      for (int iIn=0; iIn < iInputPinCount; iIn++)
                      {
                        hr=crossbar.CanRoute(iOut,iIn);
                        if (hr==0)
                        {
                          crossbar.get_CrossbarPinInfo(true,iIn,out iPinIndexRelatedIn, out PhysicalTypeIn);
                          DirectShowUtil.DebugWrite("     check:{0}->{1} / {2}->{3}",iIn,iOut,PhysicalTypeIn.ToString(), PhysicalTypeOut.ToString());
                          if (bTunerIn)
                          {
                            if ( PhysicalTypeIn==PhysicalConnectorType.Audio_Tuner ) 
                            {
                              //route
                              DirectShowUtil.DebugWrite("     connect");
                              hr=crossbar.Route(iOut,iIn);
                              if (hr!=0) DirectShowUtil.DebugWrite("    connect FAILED");
                              else DirectShowUtil.DebugWrite("    connect success");
                            }
                            if ( PhysicalTypeIn==PhysicalConnectorType.Video_Tuner  )
                            {
                              //route
                              DirectShowUtil.DebugWrite("     connect");
                              hr=crossbar.Route(iOut,iIn);
                              if (hr!=0) DirectShowUtil.DebugWrite("    connect FAILED");
                              else DirectShowUtil.DebugWrite("    connect success");
                            }
                          }
                          else
                          {
                            if ( PhysicalTypeIn==PhysicalConnectorType.Audio_AUX||
                              PhysicalTypeIn==PhysicalConnectorType.Audio_Line ||
                              PhysicalTypeIn==PhysicalConnectorType.Audio_AudioDecoder) 
                            {
                              //route
                              DirectShowUtil.DebugWrite("     connect");
                              hr=crossbar.Route(iOut,iIn);
                              if (hr!=0) DirectShowUtil.DebugWrite("    connect FAILED");
                              else 
                              {
                                DirectShowUtil.DebugWrite("    connect success");
                              }
                            }
                          }
                        }//if (hr==0)
                      }//for (int iIn=0; iIn < iInputPinCount; iIn++)
                    }//if (iInputPin==-1||bReRoute)
                  }//for (int iOut=0; iOut < iOutputPinCount; ++iOut)
                  
                  if (audioDevice==null && crossbar!=null)
                  {
                    DirectShowUtil.DebugWrite("Render output pins of crossbar");
                    DirectShowUtil.RenderOutputPins(graphBuilder,(IBaseFilter)crossbar);
                    DirectShowUtil.DebugWrite("output pins rendered");
                  }
                }//if (crossbar!=null)
                else
                {
                  DirectShowUtil.DebugWrite("  no more crossbars");
                  break;
                }
              }//if (hr ==0 && o != null)
              else
              {
                DirectShowUtil.DebugWrite("  no more crossbars.");
                break;
              }
            }//while (true)
            DirectShowUtil.DebugWrite("crossbar routing done");
          }
          else
          {
            DirectShowUtil.DebugWrite("no graph is created");
          }
        }
        catch (Exception)
        {}
      }

      public void LoadSettings(int ID)
      {
        createGraph();
        FilterPersist persist = new FilterPersist(captureGraphBuilder,videoCompressorFilter,audioCompressorFilter);
        persist.LoadSettings(ID);
      }

      public void SaveSettings(int ID)
      {
        createGraph();
        FilterPersist persist = new FilterPersist(captureGraphBuilder,videoCompressorFilter,audioCompressorFilter);
        persist.SaveSettings(ID);
      }
    }
}

