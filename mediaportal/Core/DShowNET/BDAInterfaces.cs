using System;
using System.Runtime.InteropServices;


namespace DShowNET
{
/*	public struct CONNECTDATA
	{
		IUnknown *  pUnk;
		int       dwCookie;
	};
*/
	[ComImport,
	Guid("B196B287-BAB4-101A-B69C-00AA00341D07"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]

	public interface IEnumConnections 
	{
		[PreserveSig]
		int Next( [In]			int cConnections,
							[Out]	out object data, 
										//size_is(cConnections),
										//length_is(*lpcFetched)]				CONNECTDATA *   rgcd,
							[Out] out int      lpcFetched);

		[PreserveSig]
		int RemoteNext( [In] int cConnections,
										[Out] out object data,
												//size_is(cConnections),
												//length_is(*lpcFetched)]   CONNECTDATA *   rgcd,
										[Out]  out int lpcFetched);

		[PreserveSig]
		int Skip([In]    int cConnections);

		[PreserveSig]
		int Reset();

		[PreserveSig]
		int Clone([Out] out IEnumConnections ppEnum);
	}



	[ComImport,
	Guid("B196B286-BAB4-101A-B69C-00AA00341D07"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IConnectionPoint 
	{

		[PreserveSig]
		int GetConnectionInterface([Out] out Guid  piid);

		[PreserveSig]
		int GetConnectionPointContainer([Out] out IConnectionPointContainer  ppCPC);

		[PreserveSig]
		int Advise([In, MarshalAs(UnmanagedType.IUnknown)] object  pUnkSink,[Out]  out int    pdwCookie);

		[PreserveSig]
		int Unadvise([In]    int dwCookie);

		[PreserveSig]
		int EnumConnections([Out] out   IEnumConnections ppEnum);
	}


	[ComImport,
	Guid("B196B285-BAB4-101A-B69C-00AA00341D07"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]

	public interface IEnumConnectionPoints 
	{
		[PreserveSig]
		int Next(	[In] int	cConnections,
							[Out] out object data,
										//,	size_is(cConnections),
										//length_is(*lpcFetched)]    IConnectionPoint ** rgpcn,
							[Out] out int lpcFetched);

		[PreserveSig]
		int RemoteNext(	[In] int cConnections,
										[Out] out object data,
											//,	size_is(cConnections),
											//length_is(*lpcFetched)]    IConnectionPoint ** rgpcn,
										[Out]  out int lpcFetched);

		int Skip([In]  int cConnections);

		int Reset();

		int Clone([Out] out IEnumConnectionPoints    ppEnum);
	}

	[ComImport,
	Guid("B196B284-BAB4-101A-B69C-00AA00341D07"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IConnectionPointContainer 
	{
		[PreserveSig]
		int EnumConnectionPoints([Out]  out IEnumConnectionPoints  ppEnum);

		[PreserveSig]
		int FindConnectionPoint([In]    ref Guid riid, [Out]  out IConnectionPoint  ppCP);
	}


	[ComImport,
	Guid("EFDA0C80-F395-42c3-9B3C-56B37DEC7BB7"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IGuideDataEvent
	{
		[PreserveSig]
		int GuideDataAcquired();
		int ProgramChanged([In] object    varProgramDescriptionID);
		[PreserveSig]
		int ServiceChanged([In] object    varServiceDescriptionID);
		[PreserveSig]
		int ScheduleEntryChanged([In] object    varScheduleEntryDescriptionID);
		[PreserveSig]
		int ProgramDeleted([In] object    varProgramDescriptionID);
		[PreserveSig]
		int ServiceDeleted([In] object    varServiceDescriptionID);
		[PreserveSig]
		int ScheduleDeleted([In] object    varScheduleEntryDescriptionID);
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GuideDataEvent: IGuideDataEvent
	{
		public GuideDataEvent()
		{
		}

		public int GuideDataAcquired()
		{
			DirectShowUtil.DebugWrite("GuideDataAcquired()");
			return 0;
		}

		public int ProgramChanged(object    varProgramDescriptionID)
		{
				DirectShowUtil.DebugWrite("ProgramChanged()");
				return 0;
		}

		public int ServiceChanged(object    varServiceDescriptionID)
		{
			DirectShowUtil.DebugWrite("ServiceChanged()");
			return 0;
		}

		public int ScheduleEntryChanged(object    varScheduleEntryDescriptionID)
		{
			DirectShowUtil.DebugWrite("ScheduleEntryChanged()");
			return 0;
		}

		public int ProgramDeleted(object    varProgramDescriptionID)
		{
			DirectShowUtil.DebugWrite("ProgramDeleted()");
			return 0;
		}

		public int ServiceDeleted(object    varServiceDescriptionID)
		{
			DirectShowUtil.DebugWrite("ServiceDeleted()");
			return 0;
		}

		public int ScheduleDeleted(object    varScheduleEntryDescriptionID)
		{
			DirectShowUtil.DebugWrite("ScheduleDeleted()");
			return 0;
		}
	}
}
