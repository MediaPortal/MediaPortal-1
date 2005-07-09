using System;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using DirectDraw = Microsoft.DirectX.DirectDraw;

namespace MediaPortal.Player
{
	/// <summary>
	/// General helper class to add the Video Mixing Render9 filter to a graph
	/// , set it to renderless mode and provide it our own allocator/presentor
	/// This will allow us to render the video to a direct3d texture
	/// which we can use to draw the transparent OSD on top of it
	/// Some classes which work together:
	///  VMR7Util								: general helper class
	///  AllocatorWrapper.cs		: implements our own allocator/presentor for VMR7 by implementing
	///                           IVMRSurfaceAllocator9 and IVMRImagePresenter9
	///  PlaneScene.cs          : class which draws the video texture onscreen and mixes it with the GUI, OSD,...                          
	/// </summary>
	public class VMR7Util
	{
		
		static public VMR7Util g_vmr7=null;
		public IBaseFilter		VMR7Filter = null;
		IQualProp quality=null;
		IVMRMixerBitmap m_mixerBitmap=null;
		DateTime repaintTimer = DateTime.Now;
		//ulong m_oldSavedBitmapCRC=0;
		bool  vmr7intialized=false;
		IGraphBuilder m_graphBuilder=null;
			//Util.CRCTool crc=new MediaPortal.Util.CRCTool();
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">
		/// key in mediaportal.xml to check if VMR7 should be enabled or not
		/// </param>
		public VMR7Util()
		{
			//crc.Init(Util.CRCTool.CRCCode.CRC32);
		}


		/// <summary>
		/// Add VMR7 filter to graph and configure it
		/// </summary>
		/// <param name="graphBuilder"></param>
		public void AddVMR7(IGraphBuilder graphBuilder)
		{
			Log.Write("VMR7Helper:AddVMR7");
			if (vmr7intialized) return;

			Type comtype = Type.GetTypeFromCLSID(Clsid.VideoMixingRenderer);
			object comobj = Activator.CreateInstance(comtype);
			VMR7Filter = (IBaseFilter)comobj; comobj = null;
			if (VMR7Filter == null)
			{
				Error.SetError("Unable to play movie", "VMR7 is not installed");
				Log.WriteFile(Log.LogType.Log, true, "VMR7Helper:Failed to get instance of VMR7 ");
				return;
			}

			int hr ;
			IVMRFilterConfig config = VMR7Filter as IVMRFilterConfig;
			if (config!=null)
			{
				hr=config.SetNumberOfStreams(1);
				if (hr != 0)
				{
					Log.WriteFile(Log.LogType.Log, true, "VMR7Helper:Failed to set number of streams:0x{0:X}",hr);
					Marshal.ReleaseComObject(VMR7Filter);
					VMR7Filter=null;
					return;
				}
			}

			hr = graphBuilder.AddFilter(VMR7Filter, "Video Mixing Renderer");
			if (hr != 0)
			{
				Error.SetError("Unable to play movie", "Unable to initialize VMR7");
				Log.WriteFile(Log.LogType.Log, true, "VMR7Helper:Failed to add VMR7 to filtergraph");
				Marshal.ReleaseComObject(VMR7Filter);
				VMR7Filter=null;
				return;
			}
			m_graphBuilder=graphBuilder;
			m_mixerBitmap=VMR7Filter as IVMRMixerBitmap;
			quality = VMR7Filter as IQualProp ;
			g_vmr7=this;
			vmr7intialized=true;
		}

		/// <summary>
		/// removes the VMR7 filter from the graph and free up all unmanaged resources
		/// </summary>
		public void RemoveVMR7()
		{
			if (vmr7intialized)
			{
//				int result;
				Log.Write("VMR7Helper:RemoveVMR7");
				//if (m_mixerBitmap != null)
				//	while ((result=Marshal.ReleaseComObject(m_mixerBitmap))>0);
				m_mixerBitmap = null;

//				if (quality != null)
//					while ((result=Marshal.ReleaseComObject(quality))>0);
				quality = null;
	
				if (VMR7Filter != null)
				{
					//while ((result=Marshal.ReleaseComObject(VMR7Filter))>0); 
					try
					{
						m_graphBuilder.RemoveFilter(VMR7Filter);
					}
					catch(Exception){}
					VMR7Filter = null;
					m_graphBuilder=null;
				}
				vmr7intialized=false;
				g_vmr7=null;
			}
		}


		/// <summary>
		/// returns a IVMRMixerBitmap interface
		/// </summary>
		public IVMRMixerBitmap MixerBitmapInterface
		{
			get{
				return m_mixerBitmap;
			}
		}

		public IQualProp Quality
		{
			get { return quality;}
		}
		public void Process()
		{
			if (!vmr7intialized) return;
			if( GUIGraphicsContext.Vmr9Active) return;
			TimeSpan ts = DateTime.Now - repaintTimer;
			if (ts.TotalMilliseconds > 1000)
			{
				repaintTimer = DateTime.Now;
				VideoRendererStatistics.Update(quality);
			}
		}
		/// <summary>
		/// This method returns true if VMR7 is enabled AND WORKING!
		/// this allows players to check if if VMR7 is working after setting up the playing graph
		/// by checking if VMR7 is possible they can for example fallback to the overlay device
		/// </summary>
		public bool IsVMR7Connected
		{
			get
			{
				// check if VMR7 is enabled and if initialized
				
				if (!vmr7intialized) return false;
				if (VMR7Filter == null)
				{
					return false;
				}

				//get the VMR7 input pin#0 is connected
				IPin pinIn, pinConnected;
				DsUtils.GetPin(VMR7Filter, PinDirection.Input, 0, out pinIn);
				if (pinIn == null)
				{
					//no input pin found, VMR7 is not possible
					return false;
				}

				//check if the input is connected to a video decoder
				pinIn.ConnectedTo(out pinConnected);
				if (pinConnected == null)
				{
					//no pin is not connected so VMR7 is not possible
					Marshal.ReleaseComObject(pinIn);
					return false;
				}
				Marshal.ReleaseComObject(pinIn);
				Marshal.ReleaseComObject(pinConnected);
				//all is ok, VMR7 is working
				return true;
			}//get {
		}//public bool IsVMR7Connected

		public bool SaveBitmap(System.Drawing.Bitmap bitmap,bool show,bool transparent,float alphaValue)
		{	
			if (!vmr7intialized) return true;
			if( GUIGraphicsContext.Vmr9Active) return true;
			if(MixerBitmapInterface==null)
				return false;


			if(VMR7Filter!=null)
			{
				int hr=0;
				
				VMRAlphaBitmap bmp=new VMRAlphaBitmap();
				
				if(show==true)
				{
					
					if(bitmap!=null)
					{
//						System.Drawing.ImageConverter conv=new ImageConverter();						
//						byte[] imgComp=new byte[0];
//						imgComp=(byte[])conv.ConvertTo(bitmap,imgComp.GetType());
						//ulong crcVal=crc.calc(imgComp);
//						if(crcVal==m_oldSavedBitmapCRC)
//							return false;

//						m_oldSavedBitmapCRC=crcVal;

						using(Bitmap n=new Bitmap(bitmap.Width,bitmap.Height))
						{
							using (Graphics g=Graphics.FromImage(n))
							{
								g.Clear(Color.Black);
								g.DrawImage(bitmap,0,0,bitmap.Width,bitmap.Height);
								IntPtr handle1=g.GetHdc();
								IntPtr hdc=Util.Win32API.CreateCompatibleDC(handle1);
								IntPtr oldBitmap=Util.Win32API.SelectObject(hdc,n.GetHbitmap());
								bmp.dwFlags=(int)VMRAlphaBitmapFlags.HDC | 8 ;
								bmp.color.blu=0;
								bmp.color.green=0;
								bmp.color.red=0;
								bmp.pDDS=IntPtr.Zero;
								bmp.HDC=hdc;
								bmp.rSrc = new DsRECT();
								bmp.rSrc.Top=0;
								bmp.rSrc.Left=0;
								bmp.rSrc.Right=bitmap.Width;
								bmp.rSrc.Bottom=bitmap.Height;
								bmp.rDest=new NormalizedRect();
								bmp.rDest.top=0.0f;
								bmp.rDest.left=0.0f;
								bmp.rDest.bottom=1.0f;
								bmp.rDest.right=1.0f;
								bmp.fAlpha=alphaValue;
								//Log.Write("SaveVMR7Bitmap() called");
							
								hr=VMR7Util.g_vmr7.MixerBitmapInterface.SetAlphaBitmap(bmp);
								//g.ReleaseHdc(ptrSrc);
								Util.Win32API.DeleteDC(hdc);
								g.ReleaseHdc(handle1);
								if(hr!=0)
								{
									Log.Write("SaveVMR7Bitmap() failed: error 0x{0:X} on SetAlphaBitmap()",hr);
									return false;
								}
							}
						}
					}
				}
				else
				{
					bmp.dwFlags=(int)VMRAlphaBitmapFlags.Disable;
					bmp.color.blu=0;
					bmp.color.green=0;
					bmp.color.red=0;
					bmp.HDC=IntPtr.Zero;
					bmp.rDest=new NormalizedRect();
					bmp.rDest.top=0.0f;
					bmp.rDest.left=0.0f;
					bmp.rDest.bottom=1.0f;
					bmp.rDest.right=1.0f;
					bmp.fAlpha=alphaValue;
					//Log.Write("SaveVMR7Bitmap() called");
					hr=VMR7Util.g_vmr7.MixerBitmapInterface.SetAlphaBitmap(bmp);
					if(hr!=0)
					{
						Log.Write("SaveVMR7Bitmap() failed: error {0:X} on SetAlphaBitmap()",hr);
						return false;
					}
				}
				// dispose
				return true;
			}
			return false;
		}// savevmr7bitmap
	}//public class VMR7Util
}//namespace MediaPortal.Player 
