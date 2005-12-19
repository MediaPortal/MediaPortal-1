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
using System.Collections;
using System.Runtime.InteropServices;

namespace DShowNET.Device
{

		[ComVisible(false)]
	public class DsDev
	{

		public static bool GetDevicesOfCat( Guid cat, out ArrayList devs )
		{
			devs = null;
			int hr;
			object comObj = null;
			ICreateDevEnum enumDev = null;
			System.Runtime.InteropServices.ComTypes.IEnumMoniker enumMon = null;
			System.Runtime.InteropServices.ComTypes.IMoniker[] mon = new System.Runtime.InteropServices.ComTypes.IMoniker[1];
			try {
				Type	srvType = Type.GetTypeFromCLSID( Clsid.SystemDeviceEnum );
				if( srvType == null )
					throw new NotImplementedException( "System Device Enumerator" );

				comObj = Activator.CreateInstance( srvType );
				enumDev = (ICreateDevEnum) comObj;
				hr = enumDev.CreateClassEnumerator( ref cat, out enumMon, 0 );
				if( hr != 0 )
					throw new NotSupportedException( "No devices of the category" );

        IntPtr f = Marshal.AllocCoTaskMem(sizeof(int));
				int  count = 0;
				do
				{
					hr = enumMon.Next( 1, mon, f );
					if( (hr != 0) || (mon[0] == null) )
						break;
					DsDevice dev = new DsDevice();
					dev.Name = GetFriendlyName( mon[0] );
					if( devs == null )
						devs = new ArrayList();
					dev.Mon = mon[0]; mon[0] = null;
					devs.Add( dev ); dev = null;
					count++;
				}
				while(true);
        Marshal.FreeCoTaskMem(f);

				return count > 0;
			}
			catch( Exception )
			{
				if( devs != null )
				{
					foreach( DsDevice d in devs )
						d.Dispose();
					devs = null;
				}
				return false;
			}
			finally
			{
				enumDev = null;
				if( mon[0] != null )
					Marshal.ReleaseComObject( mon[0] ); mon[0] = null;
				if( enumMon != null )
					Marshal.ReleaseComObject( enumMon ); enumMon = null;
				if( comObj != null )
					Marshal.ReleaseComObject( comObj ); comObj = null;
			}

		}

    public static string GetFriendlyName(System.Runtime.InteropServices.ComTypes.IMoniker mon)
		{
			object bagObj = null;
			IPropertyBag bag = null;
			try {
				Guid bagId = typeof( IPropertyBag ).GUID;
				mon.BindToStorage( null, null, ref bagId, out bagObj );
				bag = (IPropertyBag) bagObj;
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
	}


		[ComVisible(false)]
	public class DsDevice : IDisposable
	{
		public string			Name;
    public System.Runtime.InteropServices.ComTypes.IMoniker Mon;
		
		public void Dispose()
		{
			if( Mon != null )
				Marshal.ReleaseComObject( Mon ); Mon = null;
		}
	}







	[ComVisible(true), ComImport,
	Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface ICreateDevEnum
{
		[PreserveSig]
	int CreateClassEnumerator(
		[In]											ref Guid			pType,
   [Out]										out System.Runtime.InteropServices.ComTypes.IEnumMoniker ppEnumMoniker,
		[In]											int					dwFlags );
}



	[ComVisible(true), ComImport,
	Guid("55272A00-42CB-11CE-8135-00AA004BB851"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface IPropertyBag
{
		[PreserveSig]
	int Read(
		[In, MarshalAs(UnmanagedType.LPWStr)]			string			pszPropName,
		[In, Out, MarshalAs(UnmanagedType.Struct)]	ref	object			pVar,
		IntPtr pErrorLog );

		[PreserveSig]
	int Write(
		[In, MarshalAs(UnmanagedType.LPWStr)]			string			pszPropName,
		[In, MarshalAs(UnmanagedType.Struct)]		ref	object			pVar );
}



	[StructLayout(LayoutKind.Sequential)]
	public class GuidCouple 
	{
		public System.Guid type;
		public System.Guid subtype;
	}

	[ComVisible(true), ComImport,
	Guid("b79bb0b0-33c1-11d1-abe1-00a0c905f375"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IFilterMapper2
	{
		[PreserveSig]
		int CreateCategory ( System.Guid category, UInt32 merit, [MarshalAs(UnmanagedType.LPStr)] string catName);

		[PreserveSig]
		int UnregisterFilter ( IntPtr p1, IntPtr p2, IntPtr p3);

		[PreserveSig]
		int RegisterFilter ( IntPtr p1, IntPtr p2, ref IntPtr p3, IntPtr p4, IntPtr p5, IntPtr p6);

		//the previous interface are dummy, do not use them
		//use only this one instead
	
		[PreserveSig]
		int EnumMatchingFilters
      (out System.Runtime.InteropServices.ComTypes.IEnumMoniker ppEnumMoniker    // enumerator returned
			,       Int32 dwFlags                   // 0
			,  [MarshalAs(UnmanagedType.Bool)]    bool bExactMatch                // don't match wildcards
			,       Int32 dwMerit                   // at least this merit needed
			,  [MarshalAs(UnmanagedType.Bool)]     bool  bInputNeeded              // need at least one input pin
			,       Int32 cInputTypes               // Number of input types to match
			// Any match is OK
			,  [MarshalAs(UnmanagedType.LPArray)] Guid[] pInputTypes // input major+subtype pair array
			,  IntPtr pMedIn      // input medium
			,   IntPtr pPinCategoryIn     // input pin category
			,  [MarshalAs(UnmanagedType.Bool)] bool  bRender                   // must the input be rendered?
			,  [MarshalAs(UnmanagedType.Bool)] bool  bOutputNeeded             // need at least one output pin
			,   Int32 cOutputTypes              // Number of output types to match
			// Any match is OK
			,  [MarshalAs(UnmanagedType.LPArray)] Guid[] pOutputTypes // output major+subtype pair array
			,   IntPtr pMedOut     // output medium
			,   IntPtr pPinCategoryOut    // output pin category
			);


	}



} // namespace DShowNET.Device
