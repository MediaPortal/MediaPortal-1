using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices; 
using System.Reflection;
using DShowNET;

namespace DShowNET
{
	/// <summary>
	/// 
	/// </summary>
	public class VideoCaptureDevice
	{
    ICaptureGraphBuilder2 m_captureGraphBuilder=null;
    IGraphBuilder     m_graphBuilder=null;
    IBaseFilter       m_captureDevice=null;
    IPin              m_pinCapture=null;
    IPin              m_pinPreviewAudio=null;
    IPin              m_pinPreviewVideo=null;
    IPin              m_pinVideoPort=null;
    IAMStreamConfig	  m_videoStreamConfigCapture = null;	
    IAMStreamConfig	  m_videoStreamConfigPreview = null;	
    IAMStreamConfig	  m_videoStreamConfigVPort   = null;	
    bool              m_bMPEG2=false;
    bool              m_bMCE=false;

		public VideoCaptureDevice(IGraphBuilder graphBuilder, ICaptureGraphBuilder2 captureGraphBuilder,IBaseFilter captureDevice)
		{
      m_graphBuilder=graphBuilder;
      m_captureGraphBuilder=captureGraphBuilder;
      m_captureDevice=captureDevice;

      DirectShowUtil.DebugWrite("VideoCaptureDevice:ctor");
      int hr;
      object o=null;
      Guid[] medVideoTypes = new Guid[] { MediaType.Stream,
                                          MediaType.Interleaved,
                                          MediaType.AnalogVideo,
                                          MediaType.Video
                                         };
      
      Guid[] medAudioTypes = new Guid[] { MediaType.Stream,
                                          MediaType.Audio,
                                          MediaType.AnalogAudio
                                        };
      for (int i=0; i < medVideoTypes.Length;++i)
      {
        if (m_pinPreviewVideo==null)
          m_pinPreviewVideo=FindPreviewPin(m_captureDevice,ref medVideoTypes[i]) ;
        
        if (m_pinVideoPort==null)
          m_pinVideoPort=FindVideoPort(m_captureDevice,ref medVideoTypes[i]) ;

        if (m_pinCapture==null)
        {
          m_pinCapture=FindCapturePin(m_captureDevice,ref medVideoTypes[i]) ;
          if (medVideoTypes[i]==MediaType.Stream && m_pinCapture!=null)
          {
            m_bMPEG2=true;
          }
        }
      }
      if (m_pinPreviewVideo!=null) 
        DirectShowUtil.DebugWrite("capture:found video preview pin");
      if (m_pinVideoPort!=null) 
        DirectShowUtil.DebugWrite("capture:found videoport pin");
      if (m_pinCapture!=null) 
        DirectShowUtil.DebugWrite("capture:found video capture pin");

      //for (int i=0; i < medAudioTypes.Length;++i)
      //{
        //if (m_pinPreviewAudio==null) 
        //  m_pinPreviewAudio=FindPreviewPin(ref medAudioTypes[i]) ;
      //}
      
      //@@@TODO: replace hardcoded pin name
      if (m_pinPreviewAudio==null)
        m_pinPreviewAudio=DirectShowUtil.FindPin(m_captureDevice,PinDirection.Output,"Preview Audio");
      if (m_pinPreviewAudio==null)
        m_pinPreviewAudio=DirectShowUtil.FindPin(m_captureDevice,PinDirection.Output,"Audio");

      if (m_pinPreviewAudio!=null) 
        DirectShowUtil.DebugWrite("capture:found audio preview pin");

      //make sure the analog video&audio outputs of the crossbar are connected
      //to the video&audio inputs of the video capture device
      DsUtils.FixCrossbarRouting(m_graphBuilder,m_captureGraphBuilder,m_captureDevice, true, false, false, false );

      if (!m_bMPEG2)
      {
        // check if this is a MCE device
        /*
         for MCE devices we need to insert the WDM Streaming Encoder Device
          [capture device     ]    [  encoder device ]     [mpeg2 demuxer]
          [         capture   ]    [          mpeg-ts]---->[             ]
          [         mpeg video] -> [mpeg video       ]     [             ]
          [         mpeg audio] -> [mpeg audio       ]     [             ]
        */
        if (m_pinPreviewAudio==null && m_pinPreviewVideo==null)
        {
          //@@@TODO: replace hardcoded pin names
          DirectShowUtil.DebugWrite("capture:look for the MPEG Video and MPEG Audio pins");
          m_pinPreviewVideo=DirectShowUtil.FindPin(m_captureDevice, PinDirection.Output, "MPEG Video");
          m_pinPreviewAudio=DirectShowUtil.FindPin(m_captureDevice, PinDirection.Output, "MPEG Audio");
          if (m_pinPreviewVideo!=null && m_pinPreviewAudio!=null)
          {

            DirectShowUtil.DebugWrite("capture:found the MPEG Video and MPEG Audio pins");
            // looks like an MCE device
            m_bMCE=true;
            m_bMPEG2=true;
            //get the WDM Streaming Encoder Device  collection
            FilterCollection filters = new FilterCollection( FilterCategory.AM_KSEncoder); 
            if (filters!=null)
            {
              if (filters.Count!=0)
              {
                // add the first encoder device to the graph
                DirectShowUtil.DebugWrite("capture:Add filter:{0}", filters[0].Name);
                IBaseFilter NewFilter = (IBaseFilter) Marshal.BindToMoniker( filters[0].MonikerString );
                hr = graphBuilder.AddFilter( NewFilter, filters[0].Name );
                if( hr < 0 ) 
                {
                  DirectShowUtil.DebugWrite("capture:failed:unable to add filter:{0} to graph", filters[0].Name);
                }
                else
                {
                  // filter added. now connect video capture device->encoder device
                  IPin pinIn1 = DirectShowUtil.FindPinNr(NewFilter, PinDirection.Input,0);
                  IPin pinIn2 = DirectShowUtil.FindPinNr(NewFilter, PinDirection.Input,1);
                  if (pinIn1!=null)
                  {
                    hr=m_graphBuilder.Connect(m_pinPreviewVideo,   pinIn1);
                    if (hr==0) DirectShowUtil.DebugWrite("connected mpegvideo->mpegvideo");
                    else DirectShowUtil.DebugWrite("capture: failed to connect mpegvideo->mpegvideo:{0:x}",hr);
                  }
                  else DirectShowUtil.DebugWrite("capture: could not find pin1:{0:x}",hr);
                  if (pinIn2!=null)
                  {
                    hr=m_graphBuilder.Connect(m_pinPreviewAudio,   pinIn2);
                    if (hr==0) DirectShowUtil.DebugWrite("connected mpegaudio->mpegaudio");
                    else DirectShowUtil.DebugWrite("capture: failed to connect mpegaudio->mpegaudio:{0:x}",hr);
                  }
                  else DirectShowUtil.DebugWrite("capture: could not find pin2:{0:x}",hr);

                  // done. Now get the output of the encoder device
                  // and use that....
                  m_pinPreviewAudio=null;
                  m_pinPreviewVideo=null;
                  m_pinVideoPort=null;
                  m_pinCapture=DirectShowUtil.FindPinNr(NewFilter,PinDirection.Output,0);
                  if (m_pinCapture!=null)
                    DirectShowUtil.DebugWrite("capture: found output pin");
                  else DirectShowUtil.DebugWrite("capture: could not find output pin");
                }
              }
              else DirectShowUtil.DebugWrite("capture:No WDM Streaming Encoder devices");
            }
            else DirectShowUtil.DebugWrite("capture:No WDM Streaming Encoder devices");
          }
        }
      }
      if (!m_bMPEG2)
      {
        //@@@TODO:replace hardcoded pinnames
        //@@@TODO:fails if we got different 2 mce cards like an hauppauge pvr150mce and
        //        a winfast pvr2000


        // for Hauppauge PVR 150
        // [ video capture ]     [ hauppauge pvr ii encoder ]
        // [     inspelning]     [                          ]
        // [     audio out ]     [                          ]
        // [           vbi ]     [                          ]
        // [         pin656]---->[pin656              mpeg2 ] --->
        // [               ]     [                          ]

        // for Winfast PVR2000
        // [ video capture ]     [ WinFast PVR2000 encoder  ]
        // [       Capture ]     [                          ]
        // [     audio out ]     [                          ]
        // [         pin656]---->[pin656              mpeg2 ] --->
        // [          i2s  ]---->[i2s                       ]
 
        DirectShowUtil.DebugWrite("capture:look for pin 656 (PVR150)");
        DirectShowUtil.DebugWrite("capture:look for pin I2S (WinFast PVR2000)");
        
        IPin pin656=DirectShowUtil.FindPin(m_captureDevice, PinDirection.Output, "656");
        IPin pinI2S=DirectShowUtil.FindPin(m_captureDevice, PinDirection.Output, "i2s");
        if (pinI2S==null)
          pinI2S=DirectShowUtil.FindPin(m_captureDevice, PinDirection.Output, "I2S");

        if (pin656!=null)
        {
          m_bMCE=true;
          DirectShowUtil.DebugWrite("capture:found output pin 656 (PVR150)");
          DirectShowUtil.DebugWrite("capture:adding Encoder filter");

          // hauppauge PVR II encoder
          string HaupPaugeMonikerString1 =@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_80030070&rev_01#3&61aaa01&0&50#{19689bf6-c384-48fd-ad51-90e58c79f70b}\{03688831-8667-4c61-b5d6-4a361f025d2d}";
          string HaupPaugeMonikerString2 =@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_88010070&rev_01#3&267a616a&0&60#{19689bf6-c384-48fd-ad51-90e58c79f70b}\{03688831-8667-4c61-b5d6-4a361f025d2d}";
          // winfast pvr2000 encoder
          string WinFastPVR2000MonikerString =@"@device:pnp:\\?\pci#ven_14f1&dev_8802&subsys_663c107d&rev_05#3&61aaa01&0&52#{19689bf6-c384-48fd-ad51-90e58c79f70b}\global";
          string name="Hauppauge PVR II Encoder";
          IBaseFilter NewFilter=null ;
          try
          {
            NewFilter = Marshal.BindToMoniker( HaupPaugeMonikerString1 ) as IBaseFilter;
          }
          catch(Exception){}
          try
          {
            NewFilter = Marshal.BindToMoniker( HaupPaugeMonikerString2 ) as IBaseFilter;
          }
          catch(Exception){}
          try
          {
            if (NewFilter==null)
            {
              name="WinFast PVR 2000 Encoder";
              NewFilter = Marshal.BindToMoniker( WinFastPVR2000MonikerString ) as IBaseFilter;
            }
          }
          catch(Exception){}
          if (NewFilter==null)
          {
            Filters filters = new Filters();
            if (filters.WDMEncoders.Count>0)
            {
              Filter filter=filters.WDMEncoders[0];
              name=filter.Name;
              try
              {
                NewFilter = Marshal.BindToMoniker( filter.MonikerString ) as IBaseFilter;
              }
              catch (Exception){}
            }
          }
          if (NewFilter==null)
          {
            DirectShowUtil.DebugWrite("capture:failed:unable to create Encoder ");
            return;
          }

          DirectShowUtil.DebugWrite("capture:adding {0} to graph",name);
          hr = graphBuilder.AddFilter( NewFilter, name );
          if( hr != 0 ) 
          {
            DirectShowUtil.DebugWrite("capture:failed:unable to add Encoder filter to graph:{0:X}",hr);
            return;
          }        
          else
          {
            //now render the videocapture pin656 outpin pin -> hauppauge pvrII encoder pin656 input pin
            IPin pinIn=DirectShowUtil.FindPinNr(NewFilter,PinDirection.Input,0);
            if (pinIn!=null)
            {
              DirectShowUtil.DebugWrite("capture:found input pin 656 (PVR150)");
              hr=m_graphBuilder.Connect(pin656,pinIn);
              if( hr != 0 ) 
              {
                DirectShowUtil.DebugWrite("capture:failed:unable to connect pin656->pin656:0x{0:X}",hr);
              } 
              else
              {
                //now try to find the mpeg2 output again
                m_pinPreviewVideo=null;
                m_pinVideoPort=null;
                m_pinCapture=null;
                for (int i=0; i < medVideoTypes.Length;++i)
                {
                  if (m_pinPreviewVideo==null)
                    m_pinPreviewVideo=FindPreviewPin(NewFilter,ref medVideoTypes[i]) ;
        
                  if (m_pinVideoPort==null)
                    m_pinVideoPort=FindVideoPort(NewFilter,ref medVideoTypes[i]) ;

                  if (m_pinCapture==null)
                  {
                    m_pinCapture=FindCapturePin(NewFilter,ref medVideoTypes[i]) ;
                    if (medVideoTypes[i]==MediaType.Stream && m_pinCapture!=null)
                    {
                      m_bMPEG2=true;
                    }
                  }
                }
                if (m_pinPreviewVideo!=null) 
                  DirectShowUtil.DebugWrite("capture:found video preview pin");
                if (m_pinVideoPort!=null) 
                  DirectShowUtil.DebugWrite("capture:found videoport pin");
                if (m_pinCapture!=null) 
                  DirectShowUtil.DebugWrite("capture:found video capture pin");
              }
            }
            
            if (pinI2S!=null)
            {
              DirectShowUtil.DebugWrite("capture:found input pin pinI2S");
              m_bMCE=true;
              //now render the videocapture i2s outpin pin -> WinFast PVR2000 encoder i2s input pin
              pinIn=DirectShowUtil.FindPinNr(NewFilter,PinDirection.Input, 1);
              if (pinIn!=null)
              {
                DirectShowUtil.DebugWrite("capture:found input pin pinI2S on encoder(WinFast PVR 2000)");
                hr=m_graphBuilder.Connect(pinI2S, pinIn);
                if( hr != 0 ) 
                {
                  DirectShowUtil.DebugWrite("capture:failed:unable to connect pinI2S->pinI2S:0x{0:X}",hr);
                } 
              }

              else
              {
                DirectShowUtil.DebugWrite("capture:FAILED unable to find pin656 on hauppauge encoder filter");
              }
            }
          }
        }
      
        if (!m_bMPEG2)
        {
          DirectShowUtil.DebugWrite("capture:No MPEG Video or MPEG Audio outputs found");
        }

      }
      DirectShowUtil.DebugWrite("capture:HW MPEG2 encoder:{0} MCE device:{1}", m_bMPEG2, m_bMCE);

      // get video stream interfaces
      DirectShowUtil.DebugWrite("capture:get Video stream control interface (IAMStreamConfig)");
      Guid cat = PinCategory.Capture;
      Guid iid = typeof(IAMStreamConfig).GUID;
      hr = captureGraphBuilder.FindInterface(new Guid[1]{cat}, null, m_captureDevice, ref iid, out o );
      if ( hr == 0 )
      {
        m_videoStreamConfigCapture = o as IAMStreamConfig;
        DirectShowUtil.DebugWrite("capture:got IAMStreamConfig for Capture");
      }
    
      o=null;
      cat = PinCategory.Preview;
      iid = typeof(IAMStreamConfig).GUID;
      hr = captureGraphBuilder.FindInterface(new Guid[1]{cat}, null, m_captureDevice, ref iid, out o );
      if ( hr == 0 )
      {
        m_videoStreamConfigPreview = o as IAMStreamConfig;
        DirectShowUtil.DebugWrite("capture:got IAMStreamConfig for Preview");
      }

      o=null;
      cat = PinCategory.VideoPort;
      iid = typeof(IAMStreamConfig).GUID;
      hr = captureGraphBuilder.FindInterface(new Guid[1]{cat}, null, m_captureDevice, ref iid, out o );
      if ( hr == 0 )
      {
        m_videoStreamConfigVPort = o as IAMStreamConfig;
        DirectShowUtil.DebugWrite("capture:got IAMStreamConfig for VPort");
      }
    }

    public bool MPEG2
    {
      get { return m_bMPEG2;}
    }
    public bool IsMCEDevice
    {
      get { return m_bMCE;}
    }

    public IPin CapturePin
    {
      get { return m_pinCapture;}
    }
    public IPin PreviewVideoPin
    {
      get { return m_pinPreviewVideo;}
    }
    public IPin PreviewAudioPin
    {
      get { return m_pinPreviewAudio;}
    }
    public IPin VideoPort
    {
      get { return m_pinVideoPort;}
    }

    public bool RenderPreview()
    {
      DirectShowUtil.DebugWrite("capture:render preview");
      int hr;
      if (null!=m_pinVideoPort)
      {
          DirectShowUtil.DebugWrite("capture:render videoport pin");
          hr=m_graphBuilder.Render(m_pinVideoPort);
          if (hr==0) return true;
          DirectShowUtil.DebugWrite("capture:FAILED render videoport pin:0x{0:X}",hr);

      }
      if (null!=m_pinPreviewVideo)
      {
        DirectShowUtil.DebugWrite("capture:render preview pin");
        hr=m_graphBuilder.Render(m_pinPreviewVideo);
        if (hr==0) return true;
        DirectShowUtil.DebugWrite("capture:FAILED render preview pin:0x{0:X}",hr);
      }
      if (null!=m_pinCapture)
      {
        DirectShowUtil.DebugWrite("capture:render capture pin");
        hr=m_graphBuilder.Render(m_pinCapture);
        if (hr==0) return true;
        DirectShowUtil.DebugWrite("capture:FAILED render capture pin:0x{0:X}",hr);
      }
      return false;
    }


    IPin FindVideoPort(IBaseFilter filter,ref Guid mediaType)
    {
      IPin pPin;
      Guid cat = PinCategory.VideoPort;
      int hr = m_captureGraphBuilder.FindPin(filter,(int)PinDirection.Output,ref cat,ref mediaType,false,0,out pPin);
      if (hr>=0 && pPin!=null)
        DirectShowUtil.DebugWrite("capture:Found videoport pin");
      return pPin;
    }

    IPin FindPreviewPin(IBaseFilter filter,ref Guid mediaType)
    {
      IPin pPin;
      Guid cat = PinCategory.Preview;
      int hr = m_captureGraphBuilder.FindPin(filter,(int)PinDirection.Output,ref cat,ref mediaType,false,0,out pPin);
      if (hr>=0 && pPin!=null)
        DirectShowUtil.DebugWrite("capture:Found preview pin");
      return pPin;
    }

    IPin FindCapturePin(IBaseFilter filter,ref Guid mediaType)
    {
      IPin pPin=null;
      Guid cat = PinCategory.Capture;
      int hr = m_captureGraphBuilder.FindPin(filter,(int)PinDirection.Output,ref cat,ref mediaType,false,0,out pPin);
      if (hr>=0 && pPin!=null)
        DirectShowUtil.DebugWrite("capture:Found capture pin");
      return pPin;
    }


		public void CloseInterfaces()
		{
			if (m_pinCapture!=null) Marshal.ReleaseComObject(m_pinCapture); m_pinCapture=null;
			if (m_pinPreviewAudio!=null) Marshal.ReleaseComObject(m_pinPreviewAudio); m_pinPreviewAudio=null;
			if (m_pinPreviewVideo!=null) Marshal.ReleaseComObject(m_pinPreviewVideo); m_pinPreviewVideo=null;
      if (m_pinVideoPort!=null) Marshal.ReleaseComObject(m_pinVideoPort); m_pinVideoPort=null;
      if (m_videoStreamConfigCapture!=null) Marshal.ReleaseComObject(m_videoStreamConfigCapture); m_videoStreamConfigCapture=null;
      if (m_videoStreamConfigVPort!=null) Marshal.ReleaseComObject(m_videoStreamConfigVPort); m_videoStreamConfigVPort=null;
      if (m_videoStreamConfigPreview!=null) Marshal.ReleaseComObject(m_videoStreamConfigPreview); m_videoStreamConfigPreview=null;
      if (m_captureDevice!=null) Marshal.ReleaseComObject(m_captureDevice); m_captureDevice=null;
			m_captureGraphBuilder=null;
			m_graphBuilder=null;
		}

    public Size GetFrameSize()
    {
      if (m_videoStreamConfigCapture!=null)
      {
        try
        {
          DsBITMAPINFOHEADER bmiHeader;
          object obj= getStreamConfigSetting( m_videoStreamConfigCapture, "BmiHeader" ) ;
          if (obj!=null)
          {
            bmiHeader = (DsBITMAPINFOHEADER)obj;
            return new Size(bmiHeader.Width,bmiHeader.Height);
          }
        }
        catch(Exception)
        {
        } 
      }

      if (m_videoStreamConfigPreview!=null)
      {
        try
        {
          DsBITMAPINFOHEADER bmiHeader;
          object obj= getStreamConfigSetting( m_videoStreamConfigPreview, "BmiHeader" );
          if (obj!=null)
          {
            bmiHeader = (DsBITMAPINFOHEADER)obj;
            bmiHeader = (DsBITMAPINFOHEADER)obj;
            return new Size(bmiHeader.Width,bmiHeader.Height);
          }
        }
        catch(Exception)
        {
        } 
      }

      if (m_videoStreamConfigVPort!=null)
      {
        try
        {
          DsBITMAPINFOHEADER bmiHeader;
          object obj= getStreamConfigSetting( m_videoStreamConfigVPort, "BmiHeader" );
          if (obj!=null)
          {
            bmiHeader = (DsBITMAPINFOHEADER)obj;
            return new Size(bmiHeader.Width,bmiHeader.Height);
          }
        }
        catch(Exception)
        {
        } 
      }
      return new Size(720,576);
    }

    
    
    public void SetFrameSize(Size FrameSize)
    {
      if (FrameSize.Width>0 && FrameSize.Height>0)
      {
        if (m_videoStreamConfigCapture!=null)
        {
          try
          {
            DsBITMAPINFOHEADER bmiHeader;
            object obj= getStreamConfigSetting( m_videoStreamConfigCapture, "BmiHeader" );
            if (obj!=null)
            {
              bmiHeader = (DsBITMAPINFOHEADER)obj;
              DirectShowUtil.DebugWrite("SWGraph:change capture Framesize :{0}x{1} ->{2}x{3}",bmiHeader.Width,bmiHeader.Height, FrameSize.Width,FrameSize.Height);
              bmiHeader.Width   = FrameSize.Width;
              bmiHeader.Height = FrameSize.Height;
              setStreamConfigSetting( m_videoStreamConfigCapture, "BmiHeader", bmiHeader );
            }
          }
          catch(Exception)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED:could not set capture  Framesize to {0}x{1}!",FrameSize.Width,FrameSize.Height);
          } 
        }

        if (m_videoStreamConfigPreview!=null)
        {
          try
          {
            DsBITMAPINFOHEADER bmiHeader;
            object obj= getStreamConfigSetting( m_videoStreamConfigPreview, "BmiHeader" );
            if (obj!=null)
            {
              bmiHeader = (DsBITMAPINFOHEADER)obj;
              DirectShowUtil.DebugWrite("SWGraph:change preview Framesize :{0}x{1} ->{2}x{3}",bmiHeader.Width,bmiHeader.Height, FrameSize.Width,FrameSize.Height);
              bmiHeader.Width   = FrameSize.Width;
              bmiHeader.Height = FrameSize.Height;
              setStreamConfigSetting( m_videoStreamConfigPreview, "BmiHeader", bmiHeader );
            }
          }
          catch(Exception)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED:could not set preview Framesize to {0}x{1}!",FrameSize.Width,FrameSize.Height);
          } 
        }

        if (m_videoStreamConfigVPort!=null)
        {
          try
          {
            DsBITMAPINFOHEADER bmiHeader;
            object obj= getStreamConfigSetting( m_videoStreamConfigVPort, "BmiHeader" );
            if (obj!=null)
            {
              bmiHeader = (DsBITMAPINFOHEADER)obj;
              DirectShowUtil.DebugWrite("SWGraph:change vport Framesize :{0}x{1} ->{2}x{3}",bmiHeader.Width,bmiHeader.Height, FrameSize.Width,FrameSize.Height);
              bmiHeader.Width   = FrameSize.Width;
              bmiHeader.Height = FrameSize.Height;
              setStreamConfigSetting( m_videoStreamConfigVPort, "BmiHeader", bmiHeader );
            }
          }
          catch(Exception)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED:could not set vport Framesize to {0}x{1}!",FrameSize.Width,FrameSize.Height);
          } 
        }
      }
    }

    public void SetFrameRate(double FrameRate)
    {
      // set the framerate
      if (FrameRate>=1d && FrameRate<30d)
      {
        if (m_videoStreamConfigCapture!=null)
        {
          try
          {
            DirectShowUtil.DebugWrite("SWGraph:capture FrameRate set to {0}",FrameRate);
            long avgTimePerFrame = (long) ( 10000000d / FrameRate );
            setStreamConfigSetting( m_videoStreamConfigCapture, "AvgTimePerFrame", avgTimePerFrame );
            DirectShowUtil.DebugWrite("SWGraph: capture FrameRate done :{0}", FrameRate);
          }
          catch(Exception)
          {
            DirectShowUtil.DebugWrite("SWGraph:captureFAILED:could not set FrameRate to {0}!",FrameRate);
          }
        }

        if (m_videoStreamConfigPreview!=null)
        {
          try
          {
            DirectShowUtil.DebugWrite("SWGraph:preview FrameRate set to {0}",FrameRate);
            long avgTimePerFrame = (long) ( 10000000d / FrameRate );
            setStreamConfigSetting( m_videoStreamConfigPreview, "AvgTimePerFrame", avgTimePerFrame );
            DirectShowUtil.DebugWrite("SWGraph: preview FrameRate done :{0}", FrameRate);
          }
          catch(Exception)
          {
            DirectShowUtil.DebugWrite("SWGraph:preview FAILED:could not set FrameRate to {0}!",FrameRate);
          }
        }

        if (m_videoStreamConfigVPort!=null)
        {
          try
          {
            DirectShowUtil.DebugWrite("SWGraph:vport FrameRate set to {0}",FrameRate);
            long avgTimePerFrame = (long) ( 10000000d / FrameRate );
            setStreamConfigSetting( m_videoStreamConfigVPort, "AvgTimePerFrame", avgTimePerFrame );
            DirectShowUtil.DebugWrite("SWGraph: vport FrameRate done :{0}", FrameRate);
          }
          catch(Exception)
          {
            DirectShowUtil.DebugWrite("SWGraph:vport FAILED:could not set FrameRate to {0}!",FrameRate);
          }
        }
      }
    }
    
    
    object getStreamConfigSetting( IAMStreamConfig streamConfig, string fieldName)
    {
      object returnValue = null;
      try
      {
        if ( streamConfig == null )
          throw new NotSupportedException();

        IntPtr pmt = IntPtr.Zero;
        AMMediaTypeClass mediaType = new AMMediaTypeClass();

        try 
        {
          // Get the current format info
          mediaType.formatType=FormatType.VideoInfo2 ;
          int hr = streamConfig.GetFormat(  out mediaType);
          if ( hr != 0 )
          {
            DirectShowUtil.DebugWrite("Capture:getStreamConfigSetting() FAILED to get:{0} (not supported)",fieldName);
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
            DirectShowUtil.DebugWrite("Capture:getStreamConfigSetting() FAILED no format returned");
            throw new NotSupportedException( "This device does not support a recognized format block." );
          }
          else
          {
            DirectShowUtil.DebugWrite("Capture:getStreamConfigSetting() FAILED unknown fmt:{0} {1} {2}",mediaType.formatType,mediaType.majorType,mediaType.subType);
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
            DirectShowUtil.DebugWrite("Capture.getStreamConfigSetting() FAILED to to find member:{0}", fieldName);
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
      }
      catch(Exception)
      {
        DirectShowUtil.DebugWrite("  Capture.getStreamConfigSetting() FAILED ");
      }
      return( returnValue );
    }

    object setStreamConfigSetting( IAMStreamConfig streamConfig, string fieldName, object newValue)
    {
       try
       {
        object returnValue = null;
        IntPtr pmt = IntPtr.Zero;
        AMMediaTypeClass mediaType = new AMMediaTypeClass();

        try 
        {
          // Get the current format info
          int hr = streamConfig.GetFormat( out mediaType );
          if ( hr != 0 )
          {
            DirectShowUtil.DebugWrite("  Capture:setStreamConfigSetting() FAILED to set:{0} (getformat) hr:{1}",fieldName,hr);
            Marshal.ThrowExceptionForHR( hr );
          }
          //DirectShowUtil.DebugWrite("  Capture:setStreamConfigSetting() get formattype");
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
            DirectShowUtil.DebugWrite("  Capture:setStreamConfigSetting() FAILED no format returned");
            throw new NotSupportedException( "This device does not support a recognized format block." );
          }
          else
          {
            DirectShowUtil.DebugWrite("  Capture:setStreamConfigSetting() FAILED unknown fmt");
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
            DirectShowUtil.DebugWrite("  Capture:setStreamConfigSetting() FAILED to to find member:{0}", fieldName);
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
            DirectShowUtil.DebugWrite("  Capture:setStreamConfigSetting() FAILED to set:{0} {1}",fieldName,hr);
            Marshal.ThrowExceptionForHR( hr );
          }
          //else DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() set:{0}",fieldName);
          //DirectShowUtil.DebugWrite("  Capture.setStreamConfigSetting() done");
        }
        finally
        {
          Marshal.FreeCoTaskMem( pmt );
        }
        return( returnValue );
      }
      catch (Exception)
      {
        DirectShowUtil.DebugWrite("  Capture.:setStreamConfigSetting() FAILED ");
      }
      return null;
    }
	}
}
