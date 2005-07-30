/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Runtime.InteropServices;
using System.Threading;
using DShowNET;

namespace DShowNET.BDA
{
////////////////////////////////////////////////////////////////////////////////
	public enum MPEG_CONTEXT_TYPE
	{
		MPEG_CONTEXT_BCS_DEMUX,
		MPEG_CONTEXT_WINSOCK
	};
////////////////////////////////////////////////////////////////////////////////
	public struct HeaderUnion
	{
		public MPEG_HEADER_BITS_MIDL S;
		public ushort                W;
	}
	public struct UUnion
	{
		public MPEG_BCS_DEMUX      Demux;
		public MPEG_WINSOCK        Winsock;
	}
	public struct MPEG_BCS_DEMUX
	{
		public uint AVMGraphId;
	}

	public struct MPEG_WINSOCK
	{
		public uint AVMGraphId;
	}
	public struct MPEG_HEADER_BITS_MIDL
	{
		public ushort Bits;
	}
	public struct SECTION
	{
		public Byte				TableId;
		public HeaderUnion	Header;
		public Byte[]			SectionData;
	}
	public struct MPEG_PACKET_LIST
	{
		public ushort							wPacketCount;
		public MPEG_RQST_PACKET[]	PacketList;    // Array size is wPacketCount;
	}
	public struct MPEG_RQST_PACKET
	{
		public uint    dwLength;
		public SECTION pSection;
	}
	public struct MPEG_CONTEXT
	{
		public MPEG_CONTEXT_TYPE	Type;
		public UUnion							U;
	}
	public struct MPEG2_FILTER
	{
		public Byte									bVersionNumber;           // Must be set to 1 or more to match filter definition
		public ushort								wFilterSize;              // Size of total filter structure. Version 1 filter is 73 bytes.
		public bool									fUseRawFilteringBits;     // If true, Filter and Mask fields should be set to desired value, all other fields with be ignored.
		public Byte[]								Filter;										// Bits with values to compare against for a match.
		public Byte[]								Mask;											// Bits set to 0 are bits that are compared to those in the filter, those bits set to 1 are ignored.
		public bool									fSpecifyTableIdExtension; // If true, TableIdExtension should be set to desired value (false = don't care)
		public ushort								TableIdExtension;
		public bool									fSpecifyVersion;          // If true, Version should be set to desired value (false = don't care)
		public Byte									Version;
		public bool									fSpecifySectionNumber;    // If true, SectionNumber should be set to desired value (false = don't care)
		public Byte									SectionNumber;
		public bool									fSpecifyCurrentNext;      // If true, fNext should be set to desired value (false = don't care)
		public bool									fNext;                    // If true, next table is queried. Else, current
		public bool									fSpecifyDsmccOptions;     // If true, Dsmcc should be set with desired filter options
		public DSMCC_FILTER_OPTIONS	Dsmcc;
		public bool									fSpecifyAtscOptions;      // If true, Atsc should be set with desired filter options
		public ATSC_FILTER_OPTIONS		Atsc;
	}
//	public struct DSMCC_FILTER_OPTIONS
//	{
//		public bool		fSpecifyProtocol;       // If true, Protocol should be set to desired value
//		public Byte		Protocol;
//		public bool		fSpecifyType;           // If true, Type should be set to desired value
//		public Byte		Type;
//		public bool		fSpecifyMessageId;      // If true, MessageId should be set to desired value
//		public ushort	MessageId;
//		public bool		fSpecifyTransactionId;  // If true, TransactionId (or DownloadId for DDB msgs) should be set to desired value
//		public bool		fUseTrxIdMessageIdMask; // If false, TransactionId is filtered as is.
//		// If true, TransactionId is masked to look
//		// for any version of message with associated
//		// message identifier. See DVB - Data
//		// Broadcasting Guidlines 4.6.5. (Assignment
//		// and use of transactionId values).
//		public uint		TransactionId;
//		public bool		fSpecifyModuleVersion;  // If true, ModuleVersion should be set to the desired value
//		public Byte		ModuleVersion;
//		public bool		fSpecifyBlockNumber;    // If true, BlockNumber should be set to desired value
//		public ushort	BlockNumber;
//		public bool		fGetModuleCall;         // If true, NumberOfBlocksInModule should be set
//		public ushort	NumberOfBlocksInModule; 
//	}
//	public struct ATSC_FILTER_OPTIONS
//	{
//		public bool fSpecifyEtmId;          // If true, EtmId should be set to desired value
//		public uint EtmId;
//	}
//	public struct MPEG_PACKET_LIST
//	{
//		ushort							wPacketCount;
//		MPEG_RQST_PACKET[]	PacketList;    // Array size is wPacketCount;
//	}
//////////////////////////////////////////////////////////////////////////////

	[ComImport,
	Guid("AFEC1EB5-2A64-46c6-BF4B-AE3CCB6AFDB0"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface ISectionList 
	{
		int Initialize([In]  MPEG_REQUEST_TYPE	requestType,
											[In]  IMpeg2Data			pMpeg2Data,
											[In]  MPEG_CONTEXT		pContext,
											[In]  ushort					pid,
											[In]  Byte						tid,
											[In]  MPEG2_FILTER		pFilter,                   // OPTIONAL
											[In]  uint						timeout,
											[In]  IntPtr          hDoneEvent);               // OPTIONAL

		int InitializeWithRawSections([In]  MPEG_PACKET_LIST pmplSections);

		int CancelPendingRequest();

		int GetNumberOfSections([Out] out ushort pCount);

		int GetSectionData([In]  ushort      sectionNumber,
												[Out] out uint       pdwRawPacketLength,
												[Out] out SECTION    ppSection);

		int GetProgramIdentifier(ushort pPid);

		int GetTableIdentifier(Byte pTableId);
	};

	[ComImport,
	Guid("400CC286-32A0-4ce4-9041-39571125A635"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IMpeg2Stream 
	{
		[PreserveSig]
		int  Initialize([In]  MPEG_REQUEST_TYPE         requestType,
											 [In]  ref IMpeg2Data						 pMpeg2Data,
											 [In]  ref MPEG2_FILTER          pContext,
											 [In]  ushort                    pid,
											 [In]  byte                      tid,
											 [In]  ref MPEG2_FILTER          pFilter,           // OPTIONAL
											 [In]  int                    hDataReadyEvent);

		//[PreserveSig]
		//int  SupplyDataBuffer([In]  ref  MPEG_STREAM_BUFFER pStreamBuffer);
	};

	[ComImport,
	Guid("9B396D40-F380-4e3c-A514-1A82BF6EBFE6"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IMpeg2Data 
	{
		[PreserveSig]
		int GetSection([In]  ushort                   pid,
									 [In]  byte                     tid,
									 [In]  ref MPEG2_FILTER         pFilter,            // OPTIONAL
									 [In]  uint                     dwTimeout,
									 [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ISectionList[]          ppSectionList);

		[PreserveSig]
		int GetTable([In]  ushort                     pid,
								 [In]  byte                       tid,
								 [In]  ref MPEG2_FILTER           pFilter,            // OPTIONAL
								 [In]  uint                       dwTimeout,
								 [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]  ISectionList []            ppSectionList);

		[PreserveSig]
		int GetStreamOfSections([In]  ushort						 pid,
														[In]  byte							 tid,
														[In]  ref MPEG2_FILTER   pFilter,            // OPTIONAL
														[In]  int								 hDataReadyEvent,
														[Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] IMpeg2Stream[]  ppMpegStream);
	};

	[ComImport,
	Guid("992CF102-49F9-4719-A664-C4F23E2408F4"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IBDA_LNBInfo
	{
		[PreserveSig]
		int put_LocalOscilatorFrequencyLowBand ([In] uint          ulLOFLow);

		[PreserveSig]
		int get_LocalOscilatorFrequencyLowBand ( [In, Out] ref uint   pulLOFLow);

		[PreserveSig]
		int put_LocalOscilatorFrequencyHighBand ([In] uint          ulLOFHigh);

		[PreserveSig]
		int get_LocalOscilatorFrequencyHighBand ([In, Out] ref uint   pulLOFHigh);

		[PreserveSig]
		int put_HighLowSwitchFrequency ([In] uint          ulSwitchFrequency);

		[PreserveSig]
		int get_HighLowSwitchFrequency ([In, Out] ref uint   pulSwitchFrequency);
	}

	[ComImport,
	Guid("1347D106-CF3A-428a-A5CB-AC0D9A2A4338"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IBDA_SignalStatistics
	{
		void put_SignalStrength ([In] uint lDbStrength);
    
		void get_SignalStrength ([In, Out] ref uint plDbStrength );
    
		void put_SignalQuality ([In] uint lPercentQuality);
    
		void get_SignalQuality ([In, Out] ref uint plPercentQuality);
    
		void put_SignalPresent ([In] bool fPresent);
    
		void get_SignalPresent ([In, Out] ref bool pfPresent);
    
		void put_SignalLocked ([In] bool fLocked);
    
		void get_SignalLocked ([In, Out] ref bool pfLocked);
    
		void put_SampleTime ([In] long lmsSampleTime);
    
		void get_SampleTime ([In, Out] ref long plmsSampleTime);
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
