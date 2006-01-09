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
#if (UseCaptureCardDefinitions)

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices; 
using System.Reflection;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.GUI.Library;

namespace DShowNET.Helper
{
	/// <summary>
	/// 
	/// </summary>
	public class VideoCaptureDevice
	{
		private ICaptureGraphBuilder2 _mCaptureGraphBuilder		 	 = null;
		private IGraphBuilder					_mGraphBuilder						 = null;
		private IBaseFilter						_mCaptureFilter						 = null;
		private IPin							    _mCapturePin							 = null;
		private IPin									_mPreviewAudioPin					 = null;
		private IPin		              _mPreviewVideoPin					 = null;
		private IPin				          _mVideoPortPin						 = null;
		private IAMStreamConfig			  _mVideoCaptureStreamConfig = null;	
		private IAMStreamConfig				_mVideoPreviewStreamConfig = null;	
		private IAMStreamConfig				_mVideoVPortStreamConfig   = null;	
		private bool							    _mIsMpeg2Card							 = false;
		private bool									_mIsMceCard								 = false;

		/// <summary>
		/// #MW#
		/// New VideoCaptureDevice.
		/// This one replaces the original one and hardly does anything anymore ;-)
		/// Has only been tested with PVR150MCEs, but might work for other MPEG2 cards too.
		/// 
		/// There is some redundancy in here, due to support of existing code and due to the
		/// fact that I cannot seem to pass TVCaptureDevice as a parameter. Using that object to
		/// access some of the variables would be easier, instead of copying for instance the IsMCECard
		/// variable everywhere ;-)
		/// 
		/// </summary>
		/// <param name="pGraphBuilder"></param>
		/// <param name="pCaptureGraphBuilder"></param>
		/// <param name="pCaptureFilter"></param>
		/// <param name="pEncoderFilter"></param>
		/// <param name="pIsMceCard"></param>
		/// <param name="pIsMpeg2Card"></param>
		public VideoCaptureDevice(
			IGraphBuilder					pGraphBuilder,
			ICaptureGraphBuilder2 pCaptureGraphBuilder,
			IBaseFilter						pCaptureFilter,
			IBaseFilter						pEncoderFilter,
			bool									pIsMceCard,
			bool									pIsMpeg2Card)
		{
			int    hr = 0;
			object o  = null;

			// Fill in the required fields.
			_mGraphBuilder        = pGraphBuilder; 
			_mCaptureGraphBuilder = pCaptureGraphBuilder;
			_mCaptureFilter       = pCaptureFilter;
			_mIsMceCard           = pIsMceCard;
			_mIsMpeg2Card					= pIsMpeg2Card;

			// Now get the output of the encoder filter and use that....
			// NOTE:
			// 1. The Encoder filter also might be the capture filter...
			// 2. I have no idea if the preview stuff should be reset to null or not to work
			//		with NON MCE devices. Aren't they needed for normal MPEG2 devices???

			_mPreviewAudioPin = null;
			_mPreviewVideoPin = null;
			_mVideoPortPin    = null;
			_mCapturePin      = DirectShowUtil.FindPinNr(pEncoderFilter, PinDirection.Output, 0);
			if (_mCapturePin!=null)
				DirectShowUtil.DebugWrite("VideoCaptureDevice: found output pin");
			DirectShowUtil.DebugWrite("VideoCaptureDevice:HW MPEG2 encoder:{0} MCE device:{1}", _mIsMpeg2Card, _mIsMceCard);

			// get video stream interfaces
			DirectShowUtil.DebugWrite("VideoCaptureDevice:get Video stream control interface (IAMStreamConfig)");
			Guid cat = PinCategory.Capture;
			Guid iid = typeof(IAMStreamConfig).GUID;
			hr = _mCaptureGraphBuilder.FindInterface(new Guid[1]{cat}, null, _mCaptureFilter, ref iid, out o);
			if ( hr == 0 )
			{
				_mVideoCaptureStreamConfig = o as IAMStreamConfig;
				DirectShowUtil.DebugWrite("VideoCaptureDevice:got IAMStreamConfig for Capture");
			}
    
			o   = null;
			cat = PinCategory.Preview;
			iid = typeof(IAMStreamConfig).GUID;
			hr  = _mCaptureGraphBuilder.FindInterface(new Guid[1]{cat}, null, _mCaptureFilter, ref iid, out o);
			if ( hr == 0 )
			{
				_mVideoPreviewStreamConfig = o as IAMStreamConfig;
				DirectShowUtil.DebugWrite("VideoCaptureDevice:got IAMStreamConfig for Preview");
			}

			o   = null;
			cat = PinCategory.VideoPort;
			iid = typeof(IAMStreamConfig).GUID;
			hr  = _mCaptureGraphBuilder.FindInterface(new Guid[1]{cat}, null, _mCaptureFilter, ref iid, out o );
			if ( hr == 0 )
			{
				_mVideoVPortStreamConfig = o as IAMStreamConfig;
				DirectShowUtil.DebugWrite("VideoCaptureDevice:got IAMStreamConfig for VPort");
			}
		}

		
		public VideoCaptureDevice(
			IGraphBuilder					pGraphBuilder, 
			ICaptureGraphBuilder2 pCaptureGraphBuilder,
			IBaseFilter						pCaptureFilter)
		{
			_mGraphBuilder        = pGraphBuilder;
			_mCaptureGraphBuilder = pCaptureGraphBuilder;
			_mCaptureFilter				= pCaptureFilter;

			DirectShowUtil.DebugWrite("VideoCaptureDevice:ctor");
			int    hr;
			object o = null;
			Guid[] medVideoTypes = new Guid[]{MediaType.Stream,
																				MediaType.Interleaved,
																				MediaType.AnalogVideo,
																				MediaType.Video};
      
			Guid[] medAudioTypes = new Guid[]{MediaType.Stream,
																				MediaType.Audio,
																				MediaType.AnalogAudio};

			for (int i=0; i < medVideoTypes.Length;++i)
			{
				if (_mPreviewVideoPin==null)
					_mPreviewVideoPin=FindPreviewPin(_mCaptureFilter,ref medVideoTypes[i]);
        
				if (_mVideoPortPin==null)
					_mVideoPortPin=FindVideoPort(_mCaptureFilter,ref medVideoTypes[i]);

				if (_mCapturePin==null)
				{
					_mCapturePin=FindCapturePin(_mCaptureFilter,ref medVideoTypes[i]);
					if (medVideoTypes[i]==MediaType.Stream && _mCapturePin!=null)
					{
						_mIsMpeg2Card=true;
					}
				}
			}
			if (_mPreviewVideoPin!=null) 
				DirectShowUtil.DebugWrite("capture:found video preview pin");
			if (_mVideoPortPin!=null) 
				DirectShowUtil.DebugWrite("capture:found videoport pin");
			if (_mCapturePin!=null) 
				DirectShowUtil.DebugWrite("capture:found video capture pin");

			//for (int i=0; i < medAudioTypes.Length;++i)
			//{
			//if (_mPreviewAudioPin==null) 
			//  _mPreviewAudioPin=FindPreviewPin(ref medAudioTypes[i]) ;
			//}
      
			//@@@TODO: replace hardcoded pin name
			if (_mPreviewAudioPin==null)
				_mPreviewAudioPin=DirectShowUtil.FindPin(_mCaptureFilter,PinDirection.Output,"Preview Audio");
			if (_mPreviewAudioPin==null)
				_mPreviewAudioPin=DirectShowUtil.FindPin(_mCaptureFilter,PinDirection.Output,"Audio");

			if (_mPreviewAudioPin!=null) 
				DirectShowUtil.DebugWrite("capture:found audio preview pin");

			//make sure the analog video&audio outputs of the crossbar are connected
			//to the video&audio inputs of the video capture device
			DsUtils.FixCrossbarRouting(_mGraphBuilder,_mCaptureGraphBuilder,_mCaptureFilter, true, false, false, false,false,false );

			if (!_mIsMpeg2Card)
			{
				// check if this is a MCE device
				/*
				 for MCE devices we need to insert the WDM Streaming Encoder Device
					[capture device     ]    [  encoder device ]     [mpeg2 demuxer]
					[         capture   ]    [          mpeg-ts]---->[             ]
					[         mpeg video] -> [mpeg video       ]     [             ]
					[         mpeg audio] -> [mpeg audio       ]     [             ]
				*/
				if (_mPreviewAudioPin==null && _mPreviewVideoPin==null)
				{
					//@@@TODO: replace hardcoded pin names
					DirectShowUtil.DebugWrite("capture:look for the MPEG Video and MPEG Audio pins");
					_mPreviewVideoPin=DirectShowUtil.FindPin(_mCaptureFilter, PinDirection.Output, "MPEG Video");
					_mPreviewAudioPin=DirectShowUtil.FindPin(_mCaptureFilter, PinDirection.Output, "MPEG Audio");
					if (_mPreviewVideoPin!=null && _mPreviewAudioPin!=null)
					{

						DirectShowUtil.DebugWrite("capture:found the MPEG Video and MPEG Audio pins");
						// looks like an MCE device
						_mIsMceCard   = true;
						_mIsMpeg2Card = true;
						//get the WDM Streaming Encoder Device  collection
						FilterCollection filters = new FilterCollection( FilterCategory.AM_KSEncoder); 
						if (filters!=null)
						{
							if (filters.Count!=0)
							{
								// add the first encoder device to the graph
								DirectShowUtil.DebugWrite("capture:Add filter:{0}", filters[0].Name);
								IBaseFilter NewFilter = (IBaseFilter) Marshal.BindToMoniker( filters[0].MonikerString );
								hr = _mGraphBuilder.AddFilter( NewFilter, filters[0].Name );
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
										hr=_mGraphBuilder.Connect(_mPreviewVideoPin,   pinIn1);
										if (hr==0) DirectShowUtil.DebugWrite("connected mpegvideo->mpegvideo");
										else DirectShowUtil.DebugWrite("capture: failed to connect mpegvideo->mpegvideo:{0:x}",hr);
									}
									else DirectShowUtil.DebugWrite("capture: could not find pin1:{0:x}",hr);
									if (pinIn2!=null)
									{
										hr=_mGraphBuilder.Connect(_mPreviewAudioPin,   pinIn2);
										if (hr==0) DirectShowUtil.DebugWrite("connected mpegaudio->mpegaudio");
										else DirectShowUtil.DebugWrite("capture: failed to connect mpegaudio->mpegaudio:{0:x}",hr);
									}
									else DirectShowUtil.DebugWrite("capture: could not find pin2:{0:x}",hr);

									// done. Now get the output of the encoder device
									// and use that....
									_mPreviewAudioPin=null;
									_mPreviewVideoPin=null;
									_mVideoPortPin=null;
									_mCapturePin=DirectShowUtil.FindPinNr(NewFilter,PinDirection.Output,0);
									if (_mCapturePin!=null)
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
			if (!_mIsMpeg2Card)
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
        
				IPin pin656=DirectShowUtil.FindPin(_mCaptureFilter, PinDirection.Output, "656");
				IPin pinI2S=DirectShowUtil.FindPin(_mCaptureFilter, PinDirection.Output, "i2s");
				if (pinI2S==null)
					pinI2S=DirectShowUtil.FindPin(_mCaptureFilter, PinDirection.Output, "I2S");

				if (pin656!=null)
				{
					_mIsMceCard=true;
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
					hr = _mGraphBuilder.AddFilter( NewFilter, name );
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
							hr=_mGraphBuilder.Connect(pin656,pinIn);
							if( hr != 0 ) 
							{
								DirectShowUtil.DebugWrite("capture:failed:unable to connect pin656->pin656:0x{0:X}",hr);
							} 
							else
							{
								//now try to find the mpeg2 output again
								_mPreviewVideoPin=null;
								_mVideoPortPin=null;
								_mCapturePin=null;
								for (int i=0; i < medVideoTypes.Length;++i)
								{
									if (_mPreviewVideoPin==null)
										_mPreviewVideoPin=FindPreviewPin(NewFilter,ref medVideoTypes[i]) ;
        
									if (_mVideoPortPin==null)
										_mVideoPortPin=FindVideoPort(NewFilter,ref medVideoTypes[i]) ;

									if (_mCapturePin==null)
									{
										_mCapturePin=FindCapturePin(NewFilter,ref medVideoTypes[i]) ;
										if (medVideoTypes[i]==MediaType.Stream && _mCapturePin!=null)
										{
											_mIsMpeg2Card=true;
										}
									}
								}
								if (_mPreviewVideoPin!=null) 
									DirectShowUtil.DebugWrite("capture:found video preview pin");
								if (_mVideoPortPin!=null) 
									DirectShowUtil.DebugWrite("capture:found videoport pin");
								if (_mCapturePin!=null) 
									DirectShowUtil.DebugWrite("capture:found video capture pin");
							}
						}
            
						if (pinI2S!=null)
						{
							DirectShowUtil.DebugWrite("capture:found input pin pinI2S");
							_mIsMceCard=true;
							//now render the videocapture i2s outpin pin -> WinFast PVR2000 encoder i2s input pin
							pinIn=DirectShowUtil.FindPinNr(NewFilter,PinDirection.Input, 1);
							if (pinIn!=null)
							{
								DirectShowUtil.DebugWrite("capture:found input pin pinI2S on encoder(WinFast PVR 2000)");
								hr=_mGraphBuilder.Connect(pinI2S, pinIn);
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
      
				if (!_mIsMpeg2Card)
				{
					DirectShowUtil.DebugWrite("capture:No MPEG Video or MPEG Audio outputs found");
				}

			}
			DirectShowUtil.DebugWrite("capture:HW MPEG2 encoder:{0} MCE device:{1}", _mIsMpeg2Card, _mIsMceCard);

			// get video stream interfaces
			DirectShowUtil.DebugWrite("capture:get Video stream control interface (IAMStreamConfig)");
			Guid cat = PinCategory.Capture;
			Guid iid = typeof(IAMStreamConfig).GUID;
			hr = _mCaptureGraphBuilder.FindInterface(new Guid[1]{cat}, null, _mCaptureFilter, ref iid, out o );
			if ( hr == 0 )
			{
				_mVideoCaptureStreamConfig = o as IAMStreamConfig;
				DirectShowUtil.DebugWrite("capture:got IAMStreamConfig for Capture");
			}
    
			o=null;
			cat = PinCategory.Preview;
			iid = typeof(IAMStreamConfig).GUID;
			hr = _mCaptureGraphBuilder.FindInterface(new Guid[1]{cat}, null, _mCaptureFilter, ref iid, out o );
			if ( hr == 0 )
			{
				_mVideoPreviewStreamConfig = o as IAMStreamConfig;
				DirectShowUtil.DebugWrite("capture:got IAMStreamConfig for Preview");
			}

			o=null;
			cat = PinCategory.VideoPort;
			iid = typeof(IAMStreamConfig).GUID;
			hr = _mCaptureGraphBuilder.FindInterface(new Guid[1]{cat}, null, _mCaptureFilter, ref iid, out o );
			if ( hr == 0 )
			{
				_mVideoVPortStreamConfig = o as IAMStreamConfig;
				DirectShowUtil.DebugWrite("capture:got IAMStreamConfig for VPort");
			}
		}

		public bool MPEG2
		{
			get { return _mIsMpeg2Card;}
		}
		public bool IsMCEDevice
		{
			get { return _mIsMceCard;}
		}

		public IPin CapturePin
		{
			get { return _mCapturePin;}
		}
		public IPin PreviewVideoPin
		{
			get { return _mPreviewVideoPin;}
		}
		public IPin PreviewAudioPin
		{
			get { return _mPreviewAudioPin;}
		}
		public IPin VideoPort
		{
			get { return _mVideoPortPin;}
		}

		public bool RenderPreview()
		{
			DirectShowUtil.DebugWrite("VideoCaptureDevice:render preview");
			int hr;
			if (null!=_mVideoPortPin)
			{
				DirectShowUtil.DebugWrite("VideoCaptureDevice:render videoport pin");
				hr=_mGraphBuilder.Render(_mVideoPortPin);
				if (hr==0) return true;
				DirectShowUtil.DebugWrite("VideoCaptureDevice:FAILED render videoport pin:0x{0:X}",hr);

			}
			if (null!=_mPreviewVideoPin)
			{
				DirectShowUtil.DebugWrite("VideoCaptureDevice:render preview pin");
				hr=_mGraphBuilder.Render(_mPreviewVideoPin);
				if (hr==0) return true;
				DirectShowUtil.DebugWrite("VideoCaptureDevice:FAILED render preview pin:0x{0:X}",hr);
			}
			if (null!=_mCapturePin)
			{
				DirectShowUtil.DebugWrite("VideoCaptureDevice:render capture pin");
				hr=_mGraphBuilder.Render(_mCapturePin);
				if (hr==0) return true;
				DirectShowUtil.DebugWrite("VideoCaptureDevice:FAILED render capture pin:0x{0:X}",hr);
			}
			return false;
		}


		IPin FindVideoPort(IBaseFilter filter,ref Guid mediaType)
		{
			IPin pPin;
			Guid cat = PinCategory.VideoPort;
			int hr = _mCaptureGraphBuilder.FindPin(filter,(int)PinDirection.Output,ref cat,ref mediaType,false,0,out pPin);
			if (hr>=0 && pPin!=null)
				DirectShowUtil.DebugWrite("VideoCaptureDevice:Found videoport pin");
			return pPin;
		}

		IPin FindPreviewPin(IBaseFilter filter,ref Guid mediaType)
		{
			IPin pPin;
			Guid cat = PinCategory.Preview;
			int hr = _mCaptureGraphBuilder.FindPin(filter,(int)PinDirection.Output,ref cat,ref mediaType,false,0,out pPin);
			if (hr>=0 && pPin!=null)
				DirectShowUtil.DebugWrite("VideoCaptureDevice:Found preview pin");
			return pPin;
		}

		IPin FindCapturePin(IBaseFilter filter,ref Guid mediaType)
		{
			IPin pPin=null;
			Guid cat = PinCategory.Capture;
			int hr = _mCaptureGraphBuilder.FindPin(filter,(int)PinDirection.Output,ref cat,ref mediaType,false,0,out pPin);
			if (hr>=0 && pPin!=null)
				DirectShowUtil.DebugWrite("VideoCaptureDevice:Found capture pin");
			return pPin;
		}


		public void CloseInterfaces()
		{
			int hr=0;
			if (_mCapturePin!=null) 
			{
				Marshal.ReleaseComObject(_mCapturePin);
				_mCapturePin=null;
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(_mCapturePin):{0}",hr);
			}

			if (_mPreviewAudioPin!=null) 
			{
				Marshal.ReleaseComObject(_mPreviewAudioPin);
				_mPreviewAudioPin=null;
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(_mPreviewAudioPin):{0}",hr);
			}
			if (_mPreviewVideoPin!=null) 
			{
				Marshal.ReleaseComObject(_mPreviewVideoPin); 
				_mPreviewVideoPin=null;
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(_mPreviewVideoPin):{0}",hr);
			}
			if (_mVideoPortPin!=null) 
			{
				hr=Marshal.ReleaseComObject(_mVideoPortPin); 
				_mVideoPortPin=null;
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(_mVideoPortPin):{0}",hr);
			}

			if (_mVideoCaptureStreamConfig!=null) 
			{
				hr=Marshal.ReleaseComObject(_mVideoCaptureStreamConfig); 
				_mVideoCaptureStreamConfig=null;
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(_mVideoCaptureStreamConfig):{0}",hr);
			}

			if (_mVideoVPortStreamConfig!=null) 
			{
				hr=Marshal.ReleaseComObject(_mVideoVPortStreamConfig); 
				_mVideoVPortStreamConfig=null;
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(_mVideoVPortStreamConfig):{0}",hr);
			}
			if (_mVideoPreviewStreamConfig!=null) 
			{
				 hr=Marshal.ReleaseComObject(_mVideoPreviewStreamConfig); 
				_mVideoPreviewStreamConfig=null;
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(_mVideoPreviewStreamConfig):{0}",hr);
			}
			_mCaptureFilter=null;
			_mCaptureGraphBuilder=null;
			_mGraphBuilder=null;
		}

		public Size GetFrameSize()
		{
			if (_mVideoCaptureStreamConfig!=null)
			{
				try
				{
					DsBITMAPINFOHEADER bmiHeader;
					object obj= getStreamConfigSetting( _mVideoCaptureStreamConfig, "BmiHeader" ) ;
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

			if (_mVideoPreviewStreamConfig!=null)
			{
				try
				{
					DsBITMAPINFOHEADER bmiHeader;
					object obj= getStreamConfigSetting( _mVideoPreviewStreamConfig, "BmiHeader" );
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

			if (_mVideoVPortStreamConfig!=null)
			{
				try
				{
					DsBITMAPINFOHEADER bmiHeader;
					object obj= getStreamConfigSetting( _mVideoVPortStreamConfig, "BmiHeader" );
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
				if (_mVideoCaptureStreamConfig!=null)
				{
					try
					{
						DsBITMAPINFOHEADER bmiHeader;
						object obj= getStreamConfigSetting( _mVideoCaptureStreamConfig, "BmiHeader" );
						if (obj!=null)
						{
							bmiHeader = (DsBITMAPINFOHEADER)obj;
							DirectShowUtil.DebugWrite("VideoCaptureDevice:change capture Framesize :{0}x{1} ->{2}x{3}",bmiHeader.Width,bmiHeader.Height, FrameSize.Width,FrameSize.Height);
							bmiHeader.Width   = FrameSize.Width;
							bmiHeader.Height = FrameSize.Height;
							setStreamConfigSetting( _mVideoCaptureStreamConfig, "BmiHeader", bmiHeader );
						}
					}
					catch(Exception)
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice:FAILED:could not set capture  Framesize to {0}x{1}!",FrameSize.Width,FrameSize.Height);
					} 
				}

				if (_mVideoPreviewStreamConfig!=null)
				{
					try
					{
						DsBITMAPINFOHEADER bmiHeader;
						object obj= getStreamConfigSetting( _mVideoPreviewStreamConfig, "BmiHeader" );
						if (obj!=null)
						{
							bmiHeader = (DsBITMAPINFOHEADER)obj;
							DirectShowUtil.DebugWrite("VideoCaptureDevice:change preview Framesize :{0}x{1} ->{2}x{3}",bmiHeader.Width,bmiHeader.Height, FrameSize.Width,FrameSize.Height);
							bmiHeader.Width   = FrameSize.Width;
							bmiHeader.Height = FrameSize.Height;
							setStreamConfigSetting( _mVideoPreviewStreamConfig, "BmiHeader", bmiHeader );
						}
					}
					catch(Exception)
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice:FAILED:could not set preview Framesize to {0}x{1}!",FrameSize.Width,FrameSize.Height);
					} 
				}

				if (_mVideoVPortStreamConfig!=null)
				{
					try
					{
						DsBITMAPINFOHEADER bmiHeader;
						object obj= getStreamConfigSetting( _mVideoVPortStreamConfig, "BmiHeader" );
						if (obj!=null)
						{
							bmiHeader = (DsBITMAPINFOHEADER)obj;
							DirectShowUtil.DebugWrite("SWGraph:change vport Framesize :{0}x{1} ->{2}x{3}",bmiHeader.Width,bmiHeader.Height, FrameSize.Width,FrameSize.Height);
							bmiHeader.Width   = FrameSize.Width;
							bmiHeader.Height = FrameSize.Height;
							setStreamConfigSetting( _mVideoVPortStreamConfig, "BmiHeader", bmiHeader );
						}
					}
					catch(Exception)
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice:FAILED:could not set vport Framesize to {0}x{1}!",FrameSize.Width,FrameSize.Height);
					} 
				}
			}
		}

		public void SetFrameRate(double FrameRate)
		{
			// set the framerate
			if (FrameRate>=1d && FrameRate<30d)
			{
				if (_mVideoCaptureStreamConfig!=null)
				{
					try
					{
						DirectShowUtil.DebugWrite("SWGraph:capture FrameRate set to {0}",FrameRate);
						long avgTimePerFrame = (long) ( 10000000d / FrameRate );
						setStreamConfigSetting( _mVideoCaptureStreamConfig, "AvgTimePerFrame", avgTimePerFrame );
						DirectShowUtil.DebugWrite("VideoCaptureDevice: capture FrameRate done :{0}", FrameRate);
					}
					catch(Exception)
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice:captureFAILED:could not set FrameRate to {0}!",FrameRate);
					}
				}

				if (_mVideoPreviewStreamConfig!=null)
				{
					try
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice:preview FrameRate set to {0}",FrameRate);
						long avgTimePerFrame = (long) ( 10000000d / FrameRate );
						setStreamConfigSetting( _mVideoPreviewStreamConfig, "AvgTimePerFrame", avgTimePerFrame );
						DirectShowUtil.DebugWrite("VideoCaptureDevice: preview FrameRate done :{0}", FrameRate);
					}
					catch(Exception)
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice:preview FAILED:could not set FrameRate to {0}!",FrameRate);
					}
				}

				if (_mVideoVPortStreamConfig!=null)
				{
					try
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice:vport FrameRate set to {0}",FrameRate);
						long avgTimePerFrame = (long) ( 10000000d / FrameRate );
						setStreamConfigSetting( _mVideoVPortStreamConfig, "AvgTimePerFrame", avgTimePerFrame );
						DirectShowUtil.DebugWrite("VideoCaptureDevice: vport FrameRate done :{0}", FrameRate);
					}
					catch(Exception)
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice:vport FAILED:could not set FrameRate to {0}!",FrameRate);
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
						DirectShowUtil.DebugWrite("VideoCaptureDevice:getStreamConfigSetting() FAILED to get:{0} (not supported)",fieldName);
						Marshal.ThrowExceptionForHR( hr );
					}
					// The formatPtr member points to different structures
					// dependingon the formatType
					object formatStruct;
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.getStreamConfigSetting() find formattype"); 
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
						DirectShowUtil.DebugWrite("VideoCaptureDevice:getStreamConfigSetting() FAILED no format returned");
						throw new NotSupportedException( "This device does not support a recognized format block." );
					}
					else
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice:getStreamConfigSetting() FAILED unknown fmt:{0} {1} {2}",mediaType.formatType,mediaType.majorType,mediaType.subType);
						throw new NotSupportedException( "This device does not support a recognized format block." );
					}
            
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.getStreamConfigSetting() get formatptr");
					// Retrieve the nested structure
					Marshal.PtrToStructure( mediaType.formatPtr, formatStruct );

					// Find the required field
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.getStreamConfigSetting() get field");
					Type structType = formatStruct.GetType();
					FieldInfo fieldInfo = structType.GetField( fieldName );
					if ( fieldInfo == null )
					{
						DirectShowUtil.DebugWrite("VideoCaptureDevice.getStreamConfigSetting() FAILED to to find member:{0}", fieldName);
						throw new NotSupportedException( "VideoCaptureDevice:FAILED to find the member '" + fieldName + "' in the format block." );
					}
            
					// Extract the field's current value
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.getStreamConfigSetting() get value");
					returnValue = fieldInfo.GetValue( formatStruct ); 
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.getStreamConfigSetting() done");	
				}
				finally
				{
					Marshal.FreeCoTaskMem( pmt );
				}
			}
			catch(Exception)
			{
				DirectShowUtil.DebugWrite("  VideoCaptureDevice.getStreamConfigSetting() FAILED ");
			}
			return( returnValue );
		}

		object setStreamConfigSetting(IAMStreamConfig streamConfig, string fieldName, object newValue)
		{
			try
			{
				object returnValue = null;
				IntPtr pmt = IntPtr.Zero;
				AMMediaTypeClass mediaType = new AMMediaTypeClass();

				try 
				{
					// Get the current format info
					int hr = streamConfig.GetFormat(out mediaType);
					if ( hr != 0 )
					{
						DirectShowUtil.DebugWrite("  VideoCaptureDevice:setStreamConfigSetting() FAILED to set:{0} (getformat) hr:{1}",fieldName,hr);
						Marshal.ThrowExceptionForHR( hr );
					}
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice:setStreamConfigSetting() get formattype");
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
						DirectShowUtil.DebugWrite("  VideoCaptureDevice:setStreamConfigSetting() FAILED no format returned");
						throw new NotSupportedException( "This device does not support a recognized format block." );
					}
					else
					{
						DirectShowUtil.DebugWrite("  VideoCaptureDevice:setStreamConfigSetting() FAILED unknown fmt");
						throw new NotSupportedException( "This device does not support a recognized format block." );
					}
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.setStreamConfigSetting() get formatptr");
					// Retrieve the nested structure
					Marshal.PtrToStructure( mediaType.formatPtr, formatStruct );

					// Find the required field
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.setStreamConfigSetting() get field");
					Type structType = formatStruct.GetType();
					FieldInfo fieldInfo = structType.GetField( fieldName );
					if ( fieldInfo == null )
					{
						DirectShowUtil.DebugWrite("  VideoCaptureDevice:setStreamConfigSetting() FAILED to to find member:{0}", fieldName);
						throw new NotSupportedException( "FAILED to find the member '" + fieldName + "' in the format block." );
					}
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.setStreamConfigSetting() set value");
					// Update the value of the field
					fieldInfo.SetValue( formatStruct, newValue );

					// PtrToStructure copies the data so we need to copy it back
					Marshal.StructureToPtr( formatStruct, mediaType.formatPtr, false ); 

					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.setStreamConfigSetting() set format");
					// Save the changes
					hr = streamConfig.SetFormat( mediaType );
					if ( hr != 0 )
					{
						DirectShowUtil.DebugWrite("  VideoCaptureDevice:setStreamConfigSetting() FAILED to set:{0} {1}",fieldName,hr);
						Marshal.ThrowExceptionForHR( hr );
					}
					//else DirectShowUtil.DebugWrite("  VideoCaptureDevice.setStreamConfigSetting() set:{0}",fieldName);
					//DirectShowUtil.DebugWrite("  VideoCaptureDevice.setStreamConfigSetting() done");
				}
				finally
				{
					Marshal.FreeCoTaskMem( pmt );
				}
				return( returnValue );
			}
			catch (Exception)
			{
				DirectShowUtil.DebugWrite("  VideoCaptureDevice.:setStreamConfigSetting() FAILED ");
			}
			return null;
		}
	}
}
#endif