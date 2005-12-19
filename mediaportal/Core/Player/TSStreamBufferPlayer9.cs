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
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectX.Capture;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using DShowNET;

namespace MediaPortal.Player 
{
  public class TStreamBufferPlayer9 : BaseTStreamBufferPlayer
  {

		[ComImport, Guid("AFB6C280-2C41-11D3-8A60-0000F81E0E4A")]
			class MPEG2Demultiplexer {}
		VMR9Util _vmr9 = null;
		IBaseFilter _filter;
		IFileSourceFilter _fileSource ;
    public TStreamBufferPlayer9()
    {
    }
		protected override void OnInitialized()
		{
			Log.Write("TStreamBufferPlayer9: OnInitialized()");
			if (_vmr9!=null)
			{
				_vmr9.Enable(true);
				_updateNeeded=true;
				SetVideoWindow();
			}
		}
		public override void SetVideoWindow()
		{
			if (GUIGraphicsContext.IsFullScreenVideo!= _fullScreen)
			{
				_fullScreen=GUIGraphicsContext.IsFullScreenVideo;
				_updateNeeded=true;
			}

			if (!_updateNeeded) return;
      
			_updateNeeded=false;
			_started=true;

		}


    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces(string filename)
    {
		  Speed=1;	
			Log.Write("TStreamBufferPlayer9: GetInterfaces()");
      Type comtype = null;
      object comobj = null;
      
      //switch back to directx fullscreen mode
			
	//		Log.Write("TStreamBufferPlayer9: switch to fullscreen mode");
      GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
      GUIWindowManager.SendMessage(msg);
//Log.Write("TStreamBufferPlayer9: build graph");

      try 
      {
        comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
        if( comtype == null )
        {
          Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer9:DirectX 9 not installed");
          return false;
        }
        comobj = Activator.CreateInstance( comtype );
        _graphBuilder = (IGraphBuilder) comobj; comobj = null;
				_vmr9= new VMR9Util("mytv");
				_vmr9.AddVMR9(_graphBuilder);			
				_vmr9.Enable(false);	
				int hr;

				MPEG2Demultiplexer m_MPEG2Demuxer=null;
				IBaseFilter m_mpeg2Multiplexer=null;
				Log.WriteFile(Log.LogType.Capture,"TStreamBufferPlayer9:add new MPEG2 Demultiplexer to graph");
				try
				{
					m_MPEG2Demuxer=new MPEG2Demultiplexer();
					m_mpeg2Multiplexer = (IBaseFilter) m_MPEG2Demuxer;
				}
				catch(Exception){}
				//m_mpeg2Multiplexer = DirectShowUtil.AddFilterToGraph(m_graphBuilder,"MPEG-2 Demultiplexer");
				if (m_mpeg2Multiplexer==null) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"TStreamBufferPlayer9:FAILED to create mpeg2 demuxer");
					return false;
				}
				 hr=_graphBuilder.AddFilter(m_mpeg2Multiplexer,"MPEG-2 Demultiplexer");
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"TStreamBufferPlayer9:FAILED to add mpeg2 demuxer to graph:0x{0:X}",hr);
					return false;
				}

				Log.WriteFile(Log.LogType.Capture,"TStreamBufferPlayer9:add new TS reader to graph");

				_bufferSource = new TsFileSource();
				_filter = (IBaseFilter) _bufferSource;
				_graphBuilder.AddFilter(_filter, "TsFileSource");
		
				Log.WriteFile(Log.LogType.Capture,"TStreamBufferPlayer9:open file");
				_fileSource = (IFileSourceFilter) _bufferSource;
				 hr = _fileSource.Load(filename, IntPtr.Zero);

				Log.WriteFile(Log.LogType.Capture,"TStreamBufferPlayer9:add codecs");

				// add preferred video & audio codecs
				string strVideoCodec=String.Empty;
        string strAudioCodec=String.Empty;
				string strAudioRenderer=String.Empty;
        bool   bAddFFDshow=false;
				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          bAddFFDshow=xmlreader.GetValueAsBool("mytv","ffdshow",false);
					strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec",String.Empty);
					strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec",String.Empty);
					strAudioRenderer=xmlreader.GetValueAsString("mytv","audiorenderer",String.Empty);
					string strValue=xmlreader.GetValueAsString("mytv","defaultar","normal");
					if (strValue.Equals("zoom")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
					if (strValue.Equals("stretch")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
					if (strValue.Equals("normal")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
					if (strValue.Equals("original")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
					if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
					if (strValue.Equals("panscan")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;

				}
				IBaseFilter videoCodec=null;
				if (strVideoCodec.Length>0) 
					videoCodec=DirectShowUtil.AddFilterToGraph(_graphBuilder,strVideoCodec);
				if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(_graphBuilder,strAudioCodec);
				if (strAudioRenderer.Length==0) 
					strAudioRenderer="Default DirectSound Device";
        if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(_graphBuilder,"ffdshow raw video filter");

				_audioRendererFilter=DirectShowUtil.AddAudioRendererToGraph(_graphBuilder,strAudioRenderer,false);
				// render output pins of SBE
				IPin pin=DirectShowUtil.FindPinNr(_filter,PinDirection.Output,0);
				_graphBuilder.Render(pin);
				Marshal.ReleaseComObject(pin);

        _mediaCtrl	= (IMediaControl)  _graphBuilder;
        _mediaEvt	= (IMediaEventEx)  _graphBuilder;
				_mediaSeeking = _graphBuilder as IMediaSeeking;

				if (_audioRendererFilter!=null)
				{
					IMediaFilter mp				= _graphBuilder as IMediaFilter;
					IReferenceClock clock = _audioRendererFilter as IReferenceClock;
					hr=mp.SetSyncSource(clock);
				}
				        
				_hasVideo=true;
				if ( !_vmr9.IsVMR9Connected )
				{
					_hasVideo=false;
					if (videoCodec!=null)
						_graphBuilder.RemoveFilter(videoCodec);
					if (_vmr9!=null)
						_vmr9.RemoveVMR9();
					_vmr9=null;
				}

				return true;
      }
      catch( Exception  ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer9:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
        return false;
      }
      finally
      {
        if( comobj != null )
          Marshal.ReleaseComObject( comobj ); comobj = null;
      }
    }




    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
		{
			Cleanup();
		}

		void Cleanup()
		{
				if (_graphBuilder==null) return;

        int hr;
        //Log.Write("TStreamBufferPlayer9:cleanup DShow graph {0}",GUIGraphicsContext.InVmr9Render);
        try 
        {
					if(_vmr9!=null)
						_vmr9.Enable(false);

					int counter=0;
					while (GUIGraphicsContext.InVmr9Render)
					{
						counter++;
						System.Threading.Thread.Sleep(1);
						if (counter >200) break;
					}

          if( _mediaCtrl != null )
          {
            hr = _mediaCtrl.Stop();
						_mediaCtrl=null;
          }
          
					_mediaEvt = null;

					if(_vmr9!=null)
					{
						_vmr9.RemoveVMR9();
						_vmr9.Release();
						_vmr9=null;
					}

					if (_filter!=null)
					{
						int x=Marshal.ReleaseComObject(_filter);
						Log.Write("Release filter:{0}", x);
						_filter=null;
					}					
					if (_fileSource!=null)
					{
						int x=Marshal.ReleaseComObject(_fileSource);
						Log.Write("Release filesource:{0}", x);
						_fileSource=null;
					}
					if (_audioRendererFilter!=null)
						Marshal.ReleaseComObject( _audioRendererFilter );
					_audioRendererFilter	= null;

					_basicAudio	= null;
					_basicVideo	= null;
					_mediaSeeking=null;
					_bufferSource=null;
					_videoWin=null;

					DsUtils.RemoveFilters(_graphBuilder);

					if( _rotCookie != 0 )
						DsROT.RemoveGraphFromRot( ref _rotCookie );
					_rotCookie=0;

					if( _graphBuilder != null )
					{
						while ((hr=Marshal.ReleaseComObject( _graphBuilder ))>0); 
						_graphBuilder = null;
					}

				GUIGraphicsContext.form.Invalidate(true);
				_state = PlayState.Init;
				GC.Collect();GC.Collect();GC.Collect();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer9:exception while cleaning DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }

//Log.Write("TStreamBufferPlayer9:switch");
			//switch back to directx windowed mode

      if (!GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }

			Log.Write("TStreamBufferPlayer9:cleanup done");
		}

    protected override void OnProcess()
		{
			if (_vmr9!=null)
			{
				_videoWidth=_vmr9.VideoWidth;
				_videoHeight=_vmr9.VideoHeight;
				
			}
    }

		public override bool IsRadio
		{
			get
			{
				return false;
			}
		}

  }
}
