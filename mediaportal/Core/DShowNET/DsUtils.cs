/******************************************************
                  DirectShow .NET
		      netmaster@swissonline.ch
*******************************************************/
//					DsUtils
// DirectShow utility classes, partial from the SDK Common sources

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DShowNET
{

		[ComVisible(false)]
	public class DsUtils
	{

		const int magicConstant = -759872593;
		
		static public void RemoveFilters(IGraphBuilder m_graphBuilder)
		{
			while(true)
			{
				bool bFound=false;
				try
				{
					IEnumFilters ienumFilt;
					int hr=m_graphBuilder.EnumFilters(out ienumFilt);
					if (hr==0)
					{
						uint iFetched;
						IBaseFilter filter;
						ienumFilt.Reset();
						do
						{
							hr=ienumFilt.Next(1,out filter,out iFetched); 
							if (hr==0 && iFetched==1)
							{
								m_graphBuilder.RemoveFilter(filter);
								Marshal.ReleaseComObject(filter);
								bFound=true;
							}
						} while (iFetched==1 && hr==0);
					}
					if (!bFound) return;
				}
				catch(Exception)
				{
					return;
				}
			}
		}
      static public void DumpFilters(IGraphBuilder m_graphBuilder)
      {
        Filters filters = new Filters();
        
        try
        {
          int iFilter=0;
          IEnumFilters ienumFilt;
          int hr=m_graphBuilder.EnumFilters(out ienumFilt);
          if (hr==0)
          {
            uint iFetched;
            IBaseFilter filter;
            ienumFilt.Reset();
            do
            {
              hr=ienumFilt.Next(1,out filter,out iFetched); 
              if (hr==0 && iFetched==1)
              {
                FilterInfo info=new FilterInfo();
                filter.QueryFilterInfo(info);
                DirectShowUtil.DebugWrite("Filter #{0} :{1}", iFilter,info.achName);
                iFilter++;
              }
            } while (iFetched==1 && hr==0);
          }
        }
        catch(Exception)
        {
        }
      }


		static public void FixCrossbarRouting(IGraphBuilder graphbuilder, ICaptureGraphBuilder2 m_captureGraphBuilder,  IBaseFilter captureFilter, bool bTunerIn, bool bCVBS1, bool bCVBS2, bool bSVHS)
		{
			DirectShowUtil.DebugWrite("Capture.FixCrossbarRouting: tuner:{0} cvbs1:{1} cbs2:{2} svhs:{3}",bTunerIn, bCVBS1, bCVBS2, bSVHS);
			bool bCVBS= (bCVBS1 || bCVBS2);
			int iCVBSV=0;
      int iCVBSA=0;
			try
			{
				int iCrossBar=0;
				DirectShowUtil.DebugWrite("Set Crossbar routing. Tuner:{0} :{1:X}",bTunerIn,captureFilter);
				IBaseFilter searchfilter=captureFilter ;
				while (true)
				{
					// Find next crossbar
					int  hr=0;
					Guid cat;
					Guid iid;
					object o=null;
					cat = FindDirection.UpstreamOnly;
					iid = typeof(IAMCrossbar).GUID;
					DirectShowUtil.DebugWrite(" Find crossbar:#{0}", iCrossBar);
					hr=m_captureGraphBuilder.FindInterface(new Guid[1]{cat},null,searchfilter, ref iid, out o);
					if (hr ==0 && o != null)
					{
						IAMCrossbar crossbar = o as IAMCrossbar;
						searchfilter=o as IBaseFilter;
						if (crossbar!=null)
						{
							// crossbar found
							iCrossBar++;
							DirectShowUtil.DebugWrite("  crossbar found:{0}",iCrossBar);
							int iOutputPinCount, iInputPinCount;
							crossbar.get_PinCounts(out iOutputPinCount, out iInputPinCount);
							DirectShowUtil.DebugWrite("    crossbar:{0} inputs:{1}  outputs:{2}",crossbar.ToString(),iInputPinCount,iOutputPinCount);
            
							int                   iPinIndexRelated;
							int                   iPinIndexRelatedIn;
							//int                   iInputPin;
							PhysicalConnectorType PhysicalTypeOut;
							PhysicalConnectorType PhysicalTypeIn;
							iCVBSV=0;
              iCVBSA=0;

							//query all outputpins
							for (int iOut=0; iOut < iOutputPinCount; ++iOut)
							{
								crossbar.get_CrossbarPinInfo(false,iOut,out iPinIndexRelated, out PhysicalTypeOut);
                
              /*
                IPin OutPin, tmpPin;
                hr=GetPin((IBaseFilter)crossbar,PinDirection.Output,iOut,out OutPin);
                if (hr==0 && OutPin!=null)
                {
                  hr=OutPin.ConnectedTo(out tmpPin);
                  if (hr==0 && tmpPin==null)
                  {
                    DirectShowUtil.DebugWrite("crossbar:output pin:{0} type:{1}",iOut,PhysicalTypeOut.ToString());
                    if (PhysicalTypeOut == PhysicalConnectorType.Video_VideoDecoder)
                    {
                      IPin InPin;
                      GetPin(captureFilter,PinDirection.Input,iOut,out InPin);
                      hr=graphbuilder.Connect(OutPin, InPin);
                      DirectShowUtil.DebugWrite("crossbar:render video out:0x{0:Xx}",hr);
                    }

                    if (PhysicalTypeOut == PhysicalConnectorType.Audio_AudioDecoder)
                    {
                      IPin InPin;
                      GetPin(captureFilter,PinDirection.Input,iOut,out InPin);
                      if (InPin!=null)
                      {
                        hr=graphbuilder.Connect(OutPin, InPin);
                        DirectShowUtil.DebugWrite("crossbar:render audio out:0x{0:X}",hr);
                      }
                      else
                      {
                        DirectShowUtil.DebugWrite("crossbar:could not get pin in:{0} 0x{1:X}",iOut,hr);
                      }
                    }
                  }
                  else DirectShowUtil.DebugWrite("crossbar:output pin:{0} 0x{1:X}",iOut,hr);
                }
                else DirectShowUtil.DebugWrite("crossbar:unable to get output pin:{0} 0x{1:X}",iOut,hr);
*/
								for (int iIn=0; iIn < iInputPinCount; iIn++)
								{
									// check if we can make a connection between iIn -> iOut
									hr=crossbar.CanRoute(iOut, iIn);
									if (hr==0)
									{
										// yes thats possible, now get the PhysicalType of the input
										crossbar.get_CrossbarPinInfo(true,iIn,out iPinIndexRelatedIn, out PhysicalTypeIn);
										DirectShowUtil.DebugWrite("     check:{0}->{1} / {2}->{3}",iIn,iOut,PhysicalTypeIn.ToString(), PhysicalTypeOut.ToString());
										bool bRoute=false;
                  
										// video
										if (bTunerIn && PhysicalTypeIn==PhysicalConnectorType.Video_Tuner )  bRoute=true;
										if (bCVBS    && PhysicalTypeIn==PhysicalConnectorType.Video_Composite)  
										{
											iCVBSV++;
											if (iCVBSV==1 && bCVBS) bRoute=true;
											if (iCVBSV==2 && bCVBS2) bRoute=true;
										}
										if (bSVHS && PhysicalTypeIn==PhysicalConnectorType.Video_SVideo)  bRoute=true;
                    

										// audio
										if (bTunerIn)
										{
											if (PhysicalTypeIn==PhysicalConnectorType.Audio_Tuner )  bRoute=true;
										}
										else
										{
											if ( /*PhysicalTypeIn==PhysicalConnectorType.Audio_AUX||*/
												   PhysicalTypeIn==PhysicalConnectorType.Audio_Line ||
												   PhysicalTypeIn==PhysicalConnectorType.Audio_AudioDecoder) 
											{
                        iCVBSA++;
                        if (bCVBS && iCVBSA==1) bRoute=true;
                        if (iCVBSA==2 && bCVBS2) bRoute=true;
                        if (bSVHS) bRoute=true;
											}
										}

										if ( bRoute )
										{
											//route
											DirectShowUtil.DebugWrite("     connect");
											hr=crossbar.Route(iOut,iIn);
											if (hr!=0) DirectShowUtil.DebugWrite("    connect FAILED");
											else DirectShowUtil.DebugWrite("    connect success");
										}
									}//if (hr==0)
								}//for (int iIn=0; iIn < iInputPinCount; iIn++)
							}//for (int iOut=0; iOut < iOutputPinCount; ++iOut)
						}//if (crossbar!=null)
						else
						{
							DirectShowUtil.DebugWrite("  no more crossbars");
							break;
						}
					}//if (hr ==0 && o != null)
					else
					{
						DirectShowUtil.DebugWrite("  no more crossbars.:0x{0:X}",hr);
						break;
					}
				}//while (true)
				DirectShowUtil.DebugWrite("crossbar routing done");
			}
			catch (Exception)
			{}
		}



		public static bool IsCorrectDirectXVersion()
		{
			return File.Exists( Path.Combine( Environment.SystemDirectory, @"dpnhpast.dll" ) );
		}


			public static IntPtr GetUnmanagedSurface(Microsoft.DirectX.Direct3D.Surface surface) 
			{
				return surface.GetObjectByValue(magicConstant);
			}
		public static IntPtr GetUnmanagedDevice(Microsoft.DirectX.Direct3D.Device device) 
		{
			return device.GetObjectByValue(magicConstant);
		}
			public static string GetFriendlyName( UCOMIMoniker mon )
			{
				object bagObj = null;
				DShowNET.Device.IPropertyBag bag = null;
				try 
				{
					Guid bagId = typeof( DShowNET.Device.IPropertyBag ).GUID;
					mon.BindToStorage( null, null, ref bagId, out bagObj );
					bag = (DShowNET.Device.IPropertyBag) bagObj;
					object val = "";
					int hr = bag.Read( "FriendlyName", ref val, IntPtr.Zero );
					if( hr != 0 )
						Marshal.ThrowExceptionForHR( hr );
					string ret = val as string;
					if( (ret == null) || (ret.Length < 1) )
						throw new NotImplementedException( "Device FriendlyName" );
					return ret;
				}
				catch( Exception )
				{
					return null;
				}
				finally
				{
					bag = null;
					if( bagObj != null )
						Marshal.ReleaseComObject( bagObj ); bagObj = null;
				}
			}
		public static bool ShowCapPinDialog( ICaptureGraphBuilder2 bld, IBaseFilter flt, IntPtr hwnd )
		{
			int hr;
			object comObj = null;
			ISpecifyPropertyPages	spec = null;
			DsCAUUID cauuid = new DsCAUUID();

			try {
				Guid cat  = PinCategory.Capture;
				Guid iid = typeof(IAMStreamConfig).GUID;
        hr = bld.FindInterface( new Guid[1]{cat}, null, flt, ref iid, out comObj );
				if( hr != 0 )
				{
						return false;
				}
				spec = comObj as ISpecifyPropertyPages;
				if( spec == null )
					return false;

				hr = spec.GetPages( out cauuid );
				hr = OleCreatePropertyFrame( hwnd, 30, 30, null, 1,
						ref comObj, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero );
				return true;
			}
			catch( Exception ee )
			{
				Trace.WriteLine( "!Ds.NET: ShowCapPinDialog " + ee.Message );
				return false;
			}
			finally
			{
				if( cauuid.pElems != IntPtr.Zero )
					Marshal.FreeCoTaskMem( cauuid.pElems );
					
				spec = null;
				if( comObj != null )
					Marshal.ReleaseComObject( comObj ); comObj = null;
			}
		}

		public static bool ShowTunerPinDialog( ICaptureGraphBuilder2 bld, IBaseFilter flt, IntPtr hwnd )
		{
			int hr;
			object comObj = null;
			ISpecifyPropertyPages	spec = null;
			DsCAUUID cauuid = new DsCAUUID();

			try {
				Guid cat  = PinCategory.Capture;
				Guid iid = typeof(IAMTVTuner).GUID;
        hr = bld.FindInterface( new Guid[1]{cat}, null, flt, ref iid, out comObj );
				if( hr != 0 )
				{
						return false;
				}
				spec = comObj as ISpecifyPropertyPages;
				if( spec == null )
					return false;

				hr = spec.GetPages( out cauuid );
				hr = OleCreatePropertyFrame( hwnd, 30, 30, null, 1,
						ref comObj, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero );
				return true;
			}
			catch( Exception ee )
			{
				Trace.WriteLine( "!Ds.NET: ShowCapPinDialog " + ee.Message );
				return false;
			}
			finally
			{
				if( cauuid.pElems != IntPtr.Zero )
					Marshal.FreeCoTaskMem( cauuid.pElems );
					
				spec = null;
				if( comObj != null )
					Marshal.ReleaseComObject( comObj ); comObj = null;
			}
		}


		// from 'DShowUtil.cpp'
		static public int GetPin( IBaseFilter filter, PinDirection dirrequired, int num, out IPin ppPin )
		{
			ppPin = null;
			int hr;
			IEnumPins pinEnum;
			hr = filter.EnumPins( out pinEnum );
			if( (hr < 0) || (pinEnum == null) )
				return hr;

			IPin[] pins = new IPin[1];
			int f;
			PinDirection dir;
			do
			{
				IPin[] pinArray = new IPin[1];
				pinEnum.Next(1, pinArray, out f);
				if( (hr != 0) || (f<=0) )
					break;
				dir = (PinDirection) 3;
				hr = pinArray[0].QueryDirection( out dir );
				if( (hr == 0) && (dir == dirrequired) )
				{
					if( num == 0 )
					{
						ppPin = pinArray[0];
						pinArray[0] = null;
						break;
					}
					num--;
				}
				Marshal.ReleaseComObject( pinArray[0] ); pinArray[0] = null;
			}
			while( hr == 0 );

			Marshal.ReleaseComObject( pinEnum ); pinEnum = null;
			return hr;
		}

			public static int GetUnconnectedPin( IBaseFilter filter, PinDirection dirrequired,  out IPin ppPin )
			{
				ppPin = null;
				int hr;
				IEnumPins pinEnum;
				hr = filter.EnumPins( out pinEnum );
				if( (hr < 0) || (pinEnum == null) )
					return hr;

				IPin[] pins = new IPin[1];
				int f;
				PinDirection dir;
				do
				{
					//IPin[] pinArray = new IPin[1];
					pinEnum.Next(1, pins, out f);
					//pins = new IPin[f];
					
					if( (hr != 0) || (pins[0] == null) )
						break;
					dir = (PinDirection) 3;
					hr = pins[0].QueryDirection( out dir );
					if( (hr == 0) && (dir == dirrequired) )
					{
						IPin tmp;
						
						hr = pins[0].ConnectedTo(out tmp);
						tmp = null;
						if (hr != 0) 
						{
							hr = 0;
							ppPin = pins[0];
							break;
						}
					}
					Marshal.ReleaseComObject( pins[0] ); pins[0] = null;
				}
				while( true );

				Marshal.ReleaseComObject( pinEnum ); pinEnum = null;
				return hr;
			}

		public static int RenderFileToVMR9(IGraphBuilder pGB, string wFileName, 
			IBaseFilter pRenderer)
		{
			return RenderFileToVMR9(pGB, wFileName, pRenderer, true);
		}

		public static int RenderFileToVMR9(IGraphBuilder pGB, string wFileName, 
										   IBaseFilter pRenderer, bool bRenderAudio)
		{

			int hr;
			try 
			{
				hr = pGB.RenderFile(wFileName, null);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			
			//DsROT.AddGraphToRot(pGB, out hr);

			return 0;


		}


		[DllImport("olepro32.dll", CharSet=CharSet.Unicode, ExactSpelling=true) ]
		private static extern int OleCreatePropertyFrame( IntPtr hwndOwner, int x, int y,
			string lpszCaption, int cObjects,
			[In, MarshalAs(UnmanagedType.Interface)] ref object ppUnk,
			int cPages,	IntPtr pPageClsID, int lcid, int dwReserved, IntPtr pvReserved );


	}


// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct DsPOINT		// POINT
{
	public int		X;
	public int		Y;
}


// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public class DsRECT		// RECT
{
	public int		Left;
	public int		Top;
	public int		Right;
	public int		Bottom;
}


//---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential, Pack=2), ComVisible(false)]
public struct DsBITMAPINFOHEADER
	{
	public int      Size;
	public int      Width;
	public int      Height;
	public short    Planes;
	public short    BitCount;
	public int      Compression;
	public int      ImageSize;
	public int      XPelsPerMeter;
	public int      YPelsPerMeter;
	public int      ClrUsed;
	public int      ClrImportant;
	}




// ---------------------------------------------------------------------------------------

		[ComVisible(false)]
	public class DsROT
	{
		public static bool AddGraphToRot( object graph, out int cookie )
		{
			cookie = 0;
			int hr = 0;
			UCOMIRunningObjectTable rot = null;
			UCOMIMoniker mk = null;
			try {
				hr = GetRunningObjectTable( 0, out rot );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				int id = GetCurrentProcessId();
				IntPtr iuPtr = Marshal.GetIUnknownForObject( graph );
				int iuInt = (int) iuPtr;
				Marshal.Release( iuPtr );
				string item = string.Format( "FilterGraph {0} pid {1}", iuInt.ToString("x8"), id.ToString("x8") );
				hr = CreateItemMoniker( "!", item, out mk );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );
				
				rot.Register( ROTFLAGS_REGISTRATIONKEEPSALIVE, graph, mk, out cookie );
				return true;
			}
			catch( Exception )
			{
				return false;
			}
			finally
			{
				if( mk != null )
					Marshal.ReleaseComObject( mk ); mk = null;
				if( rot != null )
					Marshal.ReleaseComObject( rot ); rot = null;
			}
		}

		public static bool RemoveGraphFromRot( ref int cookie )
		{
			UCOMIRunningObjectTable rot = null;
			try {
				int hr = GetRunningObjectTable( 0, out rot );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				rot.Revoke( cookie );
				cookie = 0;
				return true;
			}
			catch( Exception )
			{
				return false;
			}
			finally
			{
				if( rot != null )
					Marshal.ReleaseComObject( rot ); rot = null;
			}
		}

		private const int ROTFLAGS_REGISTRATIONKEEPSALIVE	= 1;

		[DllImport("ole32.dll", ExactSpelling=true) ]
		private static extern int GetRunningObjectTable( int r,
			out UCOMIRunningObjectTable pprot );

		[DllImport("ole32.dll", CharSet=CharSet.Unicode, ExactSpelling=true) ]
		private static extern int CreateItemMoniker( string delim,
			string item, out UCOMIMoniker ppmk );

		[DllImport("kernel32.dll", ExactSpelling=true) ]
		private static extern int GetCurrentProcessId();
	}





// ---------------------------------- ocidl.idl ------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("B196B28B-BAB4-101A-B69C-00AA00341D07"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface ISpecifyPropertyPages
{
		[PreserveSig]
	int GetPages( out DsCAUUID pPages );
}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct DsCAUUID		// CAUUID
{
	public int		cElems;
	public IntPtr	pElems;
}

// ---------------------------------------------------------------------------------------


	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public class DsOptInt64
	{
		public DsOptInt64( long Value )
		{
			this.Value = Value;
		}
		public long		Value;
	}


	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public class DsOptIntPtr
	{
		public IntPtr	Pointer;
	}



} // namespace DShowNET
