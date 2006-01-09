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
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DShowNET.Helper;

namespace DShowNET
{

		[ComVisible(false)]
	public class DsUtils
	{

			const int magicConstant = -759872593;

      static public void FindFilterByClassID(IGraphBuilder m_graphBuilder, Guid classID, out IBaseFilter filterFound)
      {
        filterFound=null;
        
        if (m_graphBuilder==null) return;
				IEnumFilters ienumFilt=null;
				try
				{
					int hr=m_graphBuilder.EnumFilters(out ienumFilt);
					if (hr==0 && ienumFilt!=null)
					{
						uint iFetched;
						IBaseFilter filter;
						ienumFilt.Reset();
						do
						{
							hr=ienumFilt.Next(1,out filter,out iFetched); 
							if (hr==0 && iFetched==1)
							{
								Guid filterGuid;
								filter.GetClassID(out filterGuid);
								Marshal.ReleaseComObject(filter);
								filter=null;
								if (filterGuid == classID)
								{
									filterFound=filter;
									return;
								}
							}
						} while (iFetched==1 && hr==0);
						if (ienumFilt!=null)
							Marshal.ReleaseComObject(ienumFilt);
						ienumFilt=null;
					}
				}
				catch(Exception)
				{
				}
				finally
				{
					if (ienumFilt!=null)
						Marshal.ReleaseComObject(ienumFilt);
				}
        return ; 
      }
		static public void RemoveFilters(IGraphBuilder m_graphBuilder)
    {
			int hr;
      if (m_graphBuilder==null) return;
			for (int counter=0; counter < 100; counter++)
			{
				bool bFound=false;
				IEnumFilters ienumFilt=null;
				try
				{
					hr=m_graphBuilder.EnumFilters(out ienumFilt);
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
								int hres=Marshal.ReleaseComObject(filter);
								filter=null;
								bFound=true;
							}
						} while (iFetched==1 && hr==0);
						if (ienumFilt!=null)
							Marshal.ReleaseComObject(ienumFilt);
						ienumFilt=null;

					}
					if (!bFound) return;
				}
				catch(Exception)
				{
					return;
				}
				finally
				{
					if (ienumFilt!=null)
						hr=Marshal.ReleaseComObject(ienumFilt);
				}
			}
		}
      static public void DumpFilters(IGraphBuilder m_graphBuilder)
      {
        if (m_graphBuilder==null) return;
        Filters filters = new Filters();
        
				IEnumFilters ienumFilt=null;
        try
        {
          int iFilter=0;
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
							if (filter!=null)
								Marshal.ReleaseComObject(filter);
							filter=null;
            } while (iFetched==1 && hr==0);
						if (ienumFilt!=null)
							Marshal.ReleaseComObject(ienumFilt);
						ienumFilt=null;
					}
        }
        catch(Exception)
        {
				}
				finally
				{
					if (ienumFilt!=null)
						Marshal.ReleaseComObject(ienumFilt);
				}
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
			public static IntPtr GetUnmanagedTexture(Microsoft.DirectX.Direct3D.Texture texture) 
			{
				return texture.GetObjectByValue(magicConstant);
			}
    public static string GetFriendlyName(System.Runtime.InteropServices.ComTypes.IMoniker mon)
			{
        if (mon==null) return String.Empty;
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
      if (bld==null) return false;
      if (flt==null) return false;
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
      if (bld==null) return false;
      if (flt==null) return false;

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
      if (filter==null) return -1;
			int hr;
			IEnumPins pinEnum=null;
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
				if (pinArray[0]!=null)
					Marshal.ReleaseComObject( pinArray[0] ); 
				pinArray[0] = null;
			}
			while( hr == 0 );

			Marshal.ReleaseComObject( pinEnum ); pinEnum = null;
			return hr;
		}

			public static int GetUnconnectedPin( IBaseFilter filter, PinDirection dirrequired,  out IPin ppPin )
      {
				ppPin = null;
        if (filter==null) return -1;
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
						if (tmp!=null) Marshal.ReleaseComObject(tmp);
						tmp = null;
						if (hr != 0) 
						{
							hr = 0;
							ppPin = pins[0];
							break;
						}
					}
					if (pins[0]!=null)
						Marshal.ReleaseComObject( pins[0] ); 
					pins[0] = null;
				}
				while( true );

				Marshal.ReleaseComObject( pinEnum ); pinEnum = null;
				return hr;
			}

		public static int RenderFileToVMR9(IGraphBuilder pGB, string wFileName, 
			IBaseFilter pRenderer)
		{
      if (pGB==null) return  0;
      if (wFileName==null) return 0;
      if (pRenderer==null) return 0;
      return RenderFileToVMR9(pGB, wFileName, pRenderer, true);
		}

		public static int RenderFileToVMR9(IGraphBuilder pGB, string wFileName, 
										   IBaseFilter pRenderer, bool bRenderAudio)
		{

      if (pGB==null) return 0;
      if (wFileName==null) return 0;
      if (pRenderer==null) return 0;
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
	public struct DsPOINTClass		// POINT
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
      if (graph==null) return false;
			int hr = 0;
      System.Runtime.InteropServices.ComTypes.IRunningObjectTable rot = null;
			System.Runtime.InteropServices.ComTypes.IMoniker mk = null;
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
				
				cookie =rot.Register( ROTFLAGS_REGISTRATIONKEEPSALIVE, graph, mk);
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
			System.Runtime.InteropServices.ComTypes.IRunningObjectTable rot = null;
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
      out System.Runtime.InteropServices.ComTypes.IRunningObjectTable pprot);

		[DllImport("ole32.dll", CharSet=CharSet.Unicode, ExactSpelling=true) ]
		private static extern int CreateItemMoniker( string delim,
      string item, out System.Runtime.InteropServices.ComTypes.IMoniker ppmk);

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
