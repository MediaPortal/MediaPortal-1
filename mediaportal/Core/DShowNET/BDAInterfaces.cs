using System;
using System.Runtime.InteropServices;
using System.Threading;
using DShowNET;

namespace DShowNET.BDA
{
	[ComImport,
	Guid("1347D106-CF3A-428a-A5CB-AC0D9A2A4338"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IBDA_SignalStatistics 
	{
		[PreserveSig]
		int put_SignalStrength( /* [in] */ int  lDbStrength) ;
        
		[PreserveSig]
		int get_SignalStrength( /* [out][in] */ out int  plDbStrength) ;
        
		[PreserveSig]
		int put_SignalQuality( /* [in] */ int  lPercentQuality) ;
        
		[PreserveSig]
		int get_SignalQuality( /* [out][in] */ out int  plPercentQuality) ;
        
		[PreserveSig]
		int put_SignalPresent( /* [in] */ bool fPresent) ;
        
		[PreserveSig]
		int get_SignalPresent( /* [out][in] */ out bool pfPresent) ;
        
		[PreserveSig]
		int put_SignalLocked( /* [in] */ bool fLocked) ;
        
		[PreserveSig]
		int get_SignalLocked( /* [out][in] */ out bool pfLocked) ;
        
		[PreserveSig]
		int put_SampleTime( /* [in] */ int  lmsSampleTime) ;
        
		[PreserveSig]
		int get_SampleTime( /* [out][in] */ out int  plmsSampleTime) ;
        
	}	

	[ComImport,
	Guid("79B56888-7FEA-4690-B45D-38FD3C7849BE"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public	interface IBDA_Topology 
	{
		[PreserveSig]
		int GetNodeTypes (
									[In, Out] ref int                         pulcNodeTypes,
									[In]  int                                 ulcNodeTypesMax,
									[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int [] rgulNodeTypes);
        
		[PreserveSig]
		int GetNodeDescriptors (
													[In, Out] ref int    ulcNodeDescriptors,
													[In]  int         ulcNodeDescriptorsMax,
													[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] BDANODE_DESCRIPTOR[]   rgNodeDescriptors);

		[PreserveSig]
		int GetNodeInterfaces (
													[In]  int                                     ulNodeType,
													[In, Out] ref int                               pulcInterfaces,
													[In]  int                                     ulcInterfacesMax,
													[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] Guid[]      rgguidInterfaces);

		[PreserveSig]
		int GetPinTypes ([In, Out] ref int                           pulcPinTypes,
										 [In]  int                                 ulcPinTypesMax,
										 [In, Out,MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int[]   rgulPinTypes);

		[PreserveSig]
		int  GetTemplateConnections ([In, Out] ref int                       pulcConnections,
																 [In]  int                             ulcConnectionsMax,
																 [In, Out,MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] BDA_TEMPLATE_CONNECTION[]    rgConnections);

		[PreserveSig]
		int  CreatePin ([In]  int         ulPinType,
										[In, Out] ref int   pulPinId);

		[PreserveSig]
		int  DeletePin ([In] int      ulPinId);

		[PreserveSig]
		int  SetMediaType ([In]  int             ulPinId,
											 [In]  ref		AMMediaType   pMediaType);

		[PreserveSig]
		int  SetMedium ([In] int          ulPinId,
										[In] ref object  pMedium);

		[PreserveSig]
		int CreateTopology ([In] int ulInputPinId,
												[In] int ulOutputPinId);

	  [PreserveSig]
		int GetControlNode ([In] int              ulInputPinId,
												[In] int              ulOutputPinId,
												[In] int              ulNodeType,
												[Out, MarshalAs(UnmanagedType.IUnknown) ] out object ppControlNode);
	}

	[ComImport,
	Guid("00020404-0000-0000-C000-000000000046"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IEnumVARIANT 
	{
		[PreserveSig]
		int Next( [In] int celt,
						  [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]	 object[] rgVar, //, size_is(celt), length_is(*pCeltFetched)] VARIANT * rgVar,
							[Out] out int pCeltFetched);

		[PreserveSig]
		int RemoteNext([In] int celt,
									 [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]	 object[] rgVar, //size_is(celt), length_is(*pCeltFetched)] VARIANT * rgVar,
									 [Out] out int pCeltFetched);

		[PreserveSig]
		int Skip([In] int celt);

		[PreserveSig]
		int Reset();

		[PreserveSig]
		int Clone([Out] out IEnumVARIANT ppEnum);
	}


	[ComImport,
	Guid("88EC5E58-BB73-41d6-99CE-66C524B8B591"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IGuideDataProperty 
	{
		[PreserveSig]
		int Name([Out] out string pbstrName);
		[PreserveSig]
		int Language([Out] out int idLang);
		[PreserveSig]
		int Value([Out] out object pvar);
	}

	[ComImport,
	Guid("AE44423B-4571-475c-AD2C-F40A771D80EF"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IEnumGuideDataProperties 
	{
		[PreserveSig]
		int Next([In] int celt, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]  IGuideDataProperty[] ppprop, [Out] out int pcelt);
		[PreserveSig]
		int Skip([In] int celt);
		[PreserveSig]
		int Reset();
		[PreserveSig]
		int Clone([Out] out IEnumGuideDataProperties ppenum);
	}

	[ComImport,
	Guid("1993299C-CED6-4788-87A3-420067DCE0C7"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IEnumTuneRequests 
	{
		[PreserveSig]
		int Next([In] int celt, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] TunerLib.ITuneRequest[] ppprop, [Out] out int pcelt);
		[PreserveSig]
		int Skip([In] int celt);
		[PreserveSig]
		int Reset();
		[PreserveSig]
		int Clone([Out] out IEnumTuneRequests ppenum);
	}

	[ComImport,
	Guid("61571138-5B01-43cd-AEAF-60B784A0BF93"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IGuideData 
	{
		[PreserveSig]
		int GetServices ([Out]  out IEnumTuneRequests ppEnumTuneRequests);
		
		[PreserveSig]
		int GetServiceProperties ([In]       TunerLib.ITuneRequest							pTuneRequest,
														  [Out]  out IEnumGuideDataProperties ppEnumProperties);
		
		[PreserveSig]
		int GetGuideProgramIDs ([Out]  out IEnumVARIANT pEnumPrograms);

		
		[PreserveSig]
		int GetProgramProperties ([In]   object varProgramDescriptionID,
															[Out]  out IEnumGuideDataProperties ppEnumProperties);
		
		[PreserveSig]
		int GetScheduleEntryIDs ( [Out]  out IEnumVARIANT pEnumScheduleEntries);
		
		[PreserveSig]
		int GetScheduleEntryProperties ( [In]   object varScheduleEntryDescriptionID,
																		 [Out]  out IEnumGuideDataProperties ppEnumProperties);
	}
	
	
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
		[PreserveSig]
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
		static public Mutex mutexProgramChanged = new Mutex();
		static public Mutex mutexServiceChanged = new Mutex();
		static public Mutex mutexScheduleEntryChanged = new Mutex();
		static public Mutex mutexProgramDeleted= new Mutex();
		static public Mutex mutexServiceDeleted= new Mutex();
		static public Mutex mutexScheduleDeleted= new Mutex();
		public GuideDataEvent()
		{
		}

		public int GuideDataAcquired()
		{
			DirectShowUtil.DebugWrite("GuideDataAcquired()");
			return 0;
		}

		public int ProgramChanged([In] object    varProgramDescriptionID)
		{
				DirectShowUtil.DebugWrite("ProgramChanged():");
				mutexProgramChanged.ReleaseMutex();
				return 0;
		}

		public int ServiceChanged([In] object    varServiceDescriptionID)
		{
			DirectShowUtil.DebugWrite("ServiceChanged()");
			mutexServiceChanged .ReleaseMutex();
		
			return 0;
		}

		public int ScheduleEntryChanged([In] object    varScheduleEntryDescriptionID)
		{
			DirectShowUtil.DebugWrite("ScheduleEntryChanged()");
			mutexScheduleEntryChanged .ReleaseMutex();
			return 0;
		}

		public int ProgramDeleted([In] object    varProgramDescriptionID)
		{
			DirectShowUtil.DebugWrite("ProgramDeleted()");
			mutexProgramDeleted.ReleaseMutex();
			return 0;
		}

		public int ServiceDeleted([In] object    varServiceDescriptionID)
		{
			DirectShowUtil.DebugWrite("ServiceDeleted()");
			mutexServiceDeleted.ReleaseMutex();
			return 0;
		}

		public int ScheduleDeleted([In] object    varScheduleEntryDescriptionID)
		{
			DirectShowUtil.DebugWrite("ScheduleDeleted()");
			mutexScheduleDeleted.ReleaseMutex();
			return 0;
		}
	}
}
