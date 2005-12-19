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
using System.Runtime.InteropServices;

namespace DShowNET.BDA
{
	/*
	public struct MPEG_STREAM_BUFFER
	{
		int hr;
		uint   dwDataBufferSize;
		uint   dwSizeOfDataRead;
		byte[]  pDataBuffer;
	} ;
*/

	public enum MPEG_REQUEST_TYPE:int
	{
		MPEG_RQST_UNKNOWN = 0, 
		MPEG_RQST_GET_SECTION,
		MPEG_RQST_GET_SECTION_ASYNC,
		MPEG_RQST_GET_TABLE,
		MPEG_RQST_GET_TABLE_ASYNC,
		MPEG_RQST_GET_SECTIONS_STREAM,
		MPEG_RQST_GET_PES_STREAM,
		MPEG_RQST_GET_TS_STREAM,
		MPEG_RQST_START_MPE_STREAM,
	} ;

	[StructLayout(LayoutKind.Sequential),  ComVisible(false)]
	public struct ATSC_FILTER_OPTIONS
	{
		bool fSpecifyEtmId;          // If true, EtmId should be set to desired value
		uint EtmId;
	} ;
	// 8 BYTES
 

	[StructLayout(LayoutKind.Sequential),  ComVisible(false)]
	public  struct DSMCC_FILTER_OPTIONS
	{
		bool  fSpecifyProtocol;       // If true, Protocol should be set to desired value
		byte  Protocol;
		bool  fSpecifyType;           // If true, Type should be set to desired value
		byte  Type;
		bool  fSpecifyMessageId;      // If true, MessageId should be set to desired value
		ushort  MessageId;
		bool  fSpecifyTransactionId;  // If true, TransactionId (or DownloadId for DDB msgs) should be set to desired value
		bool  fUseTrxIdMessageIdMask; // If false, TransactionId is filtered as is.
		// If true, TransactionId is masked to look
		// for any version of message with associated
		// message identifier. See DVB - Data
		// Broadcasting Guidlines 4.6.5. (Assignment
		// and use of transactionId values).
		uint TransactionId;
		bool  fSpecifyModuleVersion;  // If true, ModuleVersion should be set to the desired value
		byte  ModuleVersion;
		bool  fSpecifyBlockNumber;    // If true, BlockNumber should be set to desired value
		ushort  BlockNumber;
		bool  fGetModuleCall;         // If true, NumberOfBlocksInModule should be set
		ushort  NumberOfBlocksInModule; 
	} ;
	// 45 BYTES
/*
	[StructLayout(LayoutKind.Sequential),  ComVisible(false)]
	public struct MPEG2_FILTER
	{
		byte  bVersionNumber;           // Must be set to 1 or more to match filter definition
		ushort  wFilterSize;              // Size of total filter structure. Version 1 filter is 73 bytes.
		bool  fUseRawFilteringBits;     // If true, Filter and Mask fields should be set to desired value, all other
		// fields with be ignored.
		byte[16]  Filter ;               // Bits with values to compare against for a match.
		byte[16]  Mask ;                 // Bits set to 0 are bits that are compared to those in the filter, those
		// bits set to 1 are ignored.
		bool  fSpecifyTableIdExtension; // If true, TableIdExtension should be set to desired value (false = don't care)
		ushort  TableIdExtension;
		bool  fSpecifyVersion;          // If true, Version should be set to desired value (false = don't care)
		byte  Version;
		bool  fSpecifySectionNumber;    // If true, SectionNumber should be set to desired value (false = don't care)
		byte  SectionNumber;
		bool  fSpecifyCurrentNext;      // If true, fNext should be set to desired value (false = don't care)
		bool  fNext;                    // If true, next table is queried. Else, current
		bool  fSpecifyDsmccOptions;     // If true, Dsmcc should be set with desired filter options
		DSMCC_FILTER_OPTIONS Dsmcc;
		bool  fSpecifyAtscOptions;      // If true, Atsc should be set with desired filter options
		ATSC_FILTER_OPTIONS Atsc;
	} */
	/// <summary>
	/// KSProperty Set Structure Definitions for BDA
	/// </summary>
	public class KSPropertyStructs 
	{
//		public struct KSP_BDA_NODE_PIN 
//		{
//			public KSPROPERTY	Property;
//			public ulong		ulNodeType;
//			public ulong		ulInputPinId;
//			public ulong		ulOutputPinId;
//		}
//
//		[ StructLayout( LayoutKind.Explicit )]
//		public struct KSM_BDA_PIN
//		{
//			[FieldOffset(0)] public KSMETHOD	Method;
//			[FieldOffset(0)] public ULONG       PinId;
//			[FieldOffset(0)] public ULONG       PinType;
//			[FieldOffset()] public ULONG       Reserved;
//		}
//
//
//	typedef struct _KSM_BDA_PIN_PAIR
//			{
//				KSMETHOD    Method;
//				union
//				{
//				ULONG       InputPinId;
//				ULONG       InputPinType;
//			};
//			union
//		{
//			ULONG       OutputPinId;
//			ULONG       OutputPinType;
//		};
//		} KSM_BDA_PIN_PAIR, * PKSM_BDA_PIN_PAIR;
//
//
//		typedef struct 
//				{
//					KSP_NODE        Property;
//					ULONG           EsPid;
//				} KSP_NODE_ESPID, *PKSP_NODE_ESPID;
	}
	/// <summary>
	/// These are sent by the IBroadcastEvent service on the graph.
	/// </summary>
	[ComVisible(false)]
	public class EventID
	{
		//===========================================================================
		//  BDA Event Guids
		//
		//      These are sent by the IBroadcastEvent service on the graph.
		//      To receive,
		//          0) Implement IBroadcastEvent in your receiving object - this has one Method on it: Fire() 
		//          1) QI the graphs service provider for SID_SBroadcastEventService
		//                 for the IID_IBroadcastEvent object
		//          2) OR create the event service (CLSID_BroadcastEventService) if not already there
		//                 and register it
		//          3) QI that object for it's IConnectionPoint interface (*pCP)
		//          4) Advise your object on *pCP  (e.g. pCP->Advise(static_cast<IBroadCastEvent*>(this), &dwCookie)
		//          5) Unadvise when done..
		//          6) Implement IBroadcastEvent::Fire(GUID gEventID)
		//             Check for relevant event below and deal with it appropriatly...
		//===========================================================================

		public static Guid EVENTID_TuningChanged			= new Guid(0x9d7e6235, 0x4b7d, 0x425d, 0xa6, 0xd1, 0xd7, 0x17, 0xc3, 0x3b, 0x9c, 0x4c);

		public static Guid EVENTID_CADenialCountChanged		= new Guid(0x2a65c528, 0x2249, 0x4070, 0xac, 0x16, 0x00, 0x39, 0x0c, 0xdf, 0xb2, 0xdd);

		public static Guid EVENTID_SignalStatusChanged		= new Guid(0x6d9cfaf2, 0x702d, 0x4b01, 0x8d, 0xff, 0x68, 0x92, 0xad, 0x20, 0xd1, 0x91);

	}

	/// <summary>
	/// BDA Stream Format GUIDs
	/// </summary>
	[ComVisible(false)]
	public class KSDataFormat 
	{
		public static Guid KSDATAFORMAT_TYPE_BDA_ANTENNA				= new Guid(0x71985f41, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

		public static Guid KSDATAFORMAT_SUBTYPE_BDA_MPEG2_TRANSPORT		= new Guid(0xf4aeb342, 0x0329, 0x4fdd, 0xa8, 0xfd, 0x4a, 0xff, 0x49, 0x26, 0xc9, 0x78);

		public static Guid KSDATAFORMAT_SPECIFIER_BDA_TRANSPORT			= new Guid(0x8deda6fd, 0xac5f, 0x4334, 0x8e, 0xcf, 0xa4, 0xba, 0x8f, 0xa7, 0xd0, 0xf0);

		public static Guid KSDATAFORMAT_TYPE_BDA_IF_SIGNAL				= new Guid(0x61be0b47, 0xa5eb, 0x499b, 0x9a, 0x85, 0x5b, 0x16, 0xc0, 0x7f, 0x12, 0x58);

		public static Guid KSDATAFORMAT_TYPE_MPEG2_SECTIONS				= new Guid(0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);

		public static Guid KSDATAFORMAT_SUBTYPE_ATSC_SI					= new Guid(0xb3c7397c, 0xd303, 0x414d, 0xb3, 0x3c, 0x4e, 0xd2, 0xc9, 0xd2, 0x97, 0x33);

		public static Guid KSDATAFORMAT_SUBTYPE_DVB_SI					= new Guid(0xe9dd31a3, 0x221d, 0x4adb, 0x85, 0x32, 0x9a, 0xf3, 0x09, 0xc1, 0xa4, 0x08);

		public static Guid KSDATAFORMAT_SUBTYPE_BDA_OPENCABLE_PSIP		= new Guid(0x762e3f66, 0x336f, 0x48d1, 0xbf, 0x83, 0x2b, 0x00, 0x35, 0x2c, 0x11, 0xf0);

		public static Guid KSDATAFORMAT_SUBTYPE_BDA_OPENCABLE_OOB_PSIP	= new Guid(0x951727db, 0xd2ce, 0x4528, 0x96, 0xf6, 0x33, 0x01, 0xfa, 0xbb, 0x2d, 0xe0);

	}

	/// <summary>
	/// KSPinName Definitions for BDA
	/// </summary>
	[ComVisible(false)]	
	public class KSPinName 
	{
		/// <summary>
		/// Pin name for a BDA transport pin
		/// </summary>
		public static Guid PINNAME_BDA_TRANSPORT			= new Guid(0x78216a81, 0xcfa8, 0x493e, 0x97, 0x11, 0x36, 0xa6, 0x1c, 0x08, 0xbd, 0x9d);

		/// <summary>
		/// Pin name for a BDA analog video pin
		/// </summary>
		public static Guid PINNAME_BDA_ANALOG_VIDEO			= new Guid(0x5c0c8281, 0x5667, 0x486c, 0x84, 0x82, 0x63, 0xe3, 0x1f, 0x01, 0xa6, 0xe9);

		/// <summary>
		/// Pin name for a BDA analog audio pin
		/// </summary>
		public static Guid PINNAME_BDA_ANALOG_AUDIO			= new Guid(0xd28a580a, 0x9b1f, 0x4b0c, 0x9c, 0x33, 0x9b, 0xf0, 0xa8, 0xea, 0x63, 0x6b);

		/// <summary>
		/// Pin name for a BDA FM Radio pin
		/// </summary>
		public static Guid PINNAME_BDA_FM_RADIO				= new Guid(0xd2855fed, 0xb2d3, 0x4eeb, 0x9b, 0xd0, 0x19, 0x34, 0x36, 0xa2, 0xf8, 0x90);

		/// <summary>
		/// Pin name for a BDA Intermediate Frequency pin
		/// </summary>
		public static Guid PINNAME_BDA_IF_PIN				= new Guid(0x1a9d4a42, 0xf3cd, 0x48a1, 0x9a, 0xea, 0x71, 0xde, 0x13, 0x3c, 0xbe, 0x14);

		/// <summary>
		/// Pin name for a BDA Open Cable PSIP pin
		/// </summary>
		public static Guid PINNAME_BDA_OPENCABLE_PSIP_PIN	= new Guid(0x297bb104, 0xe5c9, 0x4ace, 0xb1, 0x23, 0x95, 0xc3, 0xcb, 0xb2, 0x4d, 0x4f);

	}

	/// <summary>
	/// BDA Filter Categories
	/// </summary>
	[ComVisible(false)]	
	public class FilterCategories 
	{
		public static Guid KSCATEGORY_BDA_RECEIVER_COMPONENT		= new Guid(0xFD0A5AF4, 0xB41D, 0x11d2, 0x9c, 0x95, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

		public static Guid KSCATEGORY_BDA_NETWORK_TUNER				= new Guid(0x71985f48, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

		public static Guid KSCATEGORY_BDA_NETWORK_EPG				= new Guid(0x71985f49, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

		public static Guid KSCATEGORY_BDA_IP_SINK					= new Guid(0x71985f4a, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

		public static Guid KSCATEGORY_BDA_NETWORK_PROVIDER			= new Guid(0x71985f4b, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

		public static Guid KSCATEGORY_BDA_TRANSPORT_INFORMATION		= new Guid(0xa2e3074f, 0x6c3d, 0x11d3, 0xb6, 0x53, 0x00, 0xc0, 0x4f, 0x79, 0x49, 0x8e);

	}

	/// <summary>
	/// BDA Node Categories
	/// </summary>
	[ComVisible(false)]	
	public class NodeCategories 
	{
		public static Guid KSNODE_BDA_RF_TUNER				= new Guid(0x71985f4c, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

		public static Guid KSNODE_BDA_QAM_DEMODULATOR		= new Guid(0x71985f4d, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

		public static Guid KSNODE_BDA_QPSK_DEMODULATOR		= new Guid(0x6390c905, 0x27c1, 0x4d67, 0xbd, 0xb7, 0x77, 0xc5, 0x0d, 0x07, 0x93, 0x00);

		public static Guid KSNODE_BDA_8VSB_DEMODULATOR		= new Guid(0x71985f4f, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

		public static Guid KSNODE_BDA_COFDM_DEMODULATOR		= new Guid(0x2dac6e05, 0xedbe, 0x4b9c, 0xb3, 0x87, 0x1b, 0x6f, 0xad, 0x7d, 0x64, 0x95);

		public static Guid KSNODE_BDA_OPENCABLE_POD			= new Guid(0xd83ef8fc, 0xf3b8, 0x45ab, 0x8b, 0x71, 0xec, 0xf7, 0xc3, 0x39, 0xde, 0xb4);

		public static Guid KSNODE_BDA_COMMON_CA_POD			= new Guid(0xd83ef8fc, 0xf3b8, 0x45ab, 0x8b, 0x71, 0xec, 0xf7, 0xc3, 0x39, 0xde, 0xb4);

		public static Guid KSNODE_BDA_PID_FILTER			= new Guid(0xf5412789, 0xb0a0, 0x44e1, 0xae, 0x4f, 0xee, 0x99, 0x9b, 0x1b, 0x7f, 0xbe);

		public static Guid KSNODE_BDA_IP_SINK				= new Guid(0x71985f4e, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);

	}
	
	/// <summary>
	/// IPSink PINNAME GUID
	/// </summary>
	[ComVisible(false)]	
	public class IPSinkPinName 
	{
		public static Guid PINNAME_IPSINK_INPUT	= new Guid(0x3fdffa70, 0xac9a, 0x11d2, 0x8f, 0x17, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe2);
	}

	/// <summary>
	/// BDA IPSink Categories/Types
	/// </summary>
	[ComVisible(false)]	
	public class IPSinkCategories 
	{
		public static Guid KSDATAFORMAT_TYPE_BDA_IP				= new Guid(0xe25f7b8e, 0xcccc, 0x11d2, 0x8f, 0x25, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe2);

		public static Guid KSDATAFORMAT_SUBTYPE_BDA_IP			= new Guid(0x5a9a213c, 0xdb08, 0x11d2, 0x8f, 0x32, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe2);

		public static Guid KSDATAFORMAT_SPECIFIER_BDA_IP		= new Guid(0x6b891420, 0xdb09, 0x11d2, 0x8f, 0x32, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe2);

		public static Guid KSDATAFORMAT_TYPE_BDA_IP_CONTROL		= new Guid(0xdadd5799, 0x7d5b, 0x4b63, 0x80, 0xfb, 0xd1, 0x44, 0x2f, 0x26, 0xb6, 0x21);

		public static Guid KSDATAFORMAT_SUBTYPE_BDA_IP_CONTROL	= new Guid(0x499856e8, 0xe85b, 0x48ed, 0x9b, 0xea, 0x41, 0x0d, 0x0d, 0xd4, 0xef, 0x81);

	}

	/// <summary>
	/// MPE PINNAME GUID
	/// </summary>
	[ComVisible(false)]	
	public class MPEPinName 
	{
		public static Guid PINNAME_MPE	= new Guid(0xc1b06d73, 0x1dbb, 0x11d3, 0x8f, 0x46, 0x00, 0xC0, 0x4f, 0x79, 0x71, 0xE2);
	}

	/// <summary>
	/// BDA MPE Categories/Types
	/// </summary>
	[ComVisible(false)]	
	public class MPECategories 
	{
		public static Guid KSDATAFORMAT_TYPE_MPE	= new Guid(0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
	}

	/// <summary>
	/// BDA Network Provider GUIDS
	/// </summary>
	[ComVisible(false)]
	public class NetworkProviders 
	{
		public static Guid CLSID_ATSCNetworkProvider		= new Guid(0x0dad2fdd, 0x5fd7, 0x11d3, 0x8f, 0x50, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe2);

		public static Guid CLSID_ATSCNetworkPropertyPage	= new Guid(0xe3444d16, 0x5ac4, 0x4386, 0x88, 0xdf, 0x13, 0xfd, 0x23, 0x0e, 0x1d, 0xda);

		public static Guid CLSID_DVBSNetworkProvider		= new Guid(0xfa4b375a, 0x45b4, 0x4d45, 0x84, 0x40, 0x26, 0x39, 0x57, 0xb1, 0x16, 0x23);

		public static Guid CLSID_DVBTNetworkProvider		= new Guid(0x216c62df, 0x6d7f, 0x4e9a, 0x85, 0x71, 0x05, 0xf1, 0x4e, 0xdb, 0x76, 0x6a);

		public static Guid CLSID_DVBCNetworkProvider		= new Guid(0xdc0c0fe7, 0x0485, 0x4266, 0xb9, 0x3f, 0x68, 0xfb, 0xf8, 0x0e, 0xd8, 0x34);
	}

	/// <summary>
	/// Tuning Space GUIDS
	/// </summary>
	[ComVisible(false)]
	public class TuningSpaces 
	{
		public static Guid CLSID_SystemTuningSpaces		= new Guid("D02AAC50-027E-11d3-9D8E-00C04F72D980");

		public static Guid CLSID_TuningSpace			= new Guid("5FFDC5E6-B83A-4b55-B6E8-C69E765FE9DB");

		public static Guid CLSID_ATSCTuningSpace		= new Guid("A2E30750-6C3D-11d3-B653-00C04F79498E");

		public static Guid CLSID_DVBTuningSpace			= new Guid("C6B14B32-76AA-4a86-A7AC-5C79AAF58DA7");

		public static Guid CLSID_DVBSTuningSpace		= new Guid("B64016F3-C9A2-4066-96F0-BD9563314726");
	}

	/// <summary>
	/// Locator GUIDS
	/// </summary>
	[ComVisible(false)]
	public class Locators 
	{
		public static Guid CLSID_Locator		= new Guid("0888C883-AC4F-4943-B516-2C38D9B34562");

		public static Guid CLSID_ATSCLocator	= new Guid("8872FF1B-98FA-4d7a-8D93-C9F1055F85BB");

		public static Guid CLSID_DVBTLocator	= new Guid("9CD64701-BDF3-4d14-8E03-F12983D86664");

		public static Guid CLSID_DVBSLocator	= new Guid("1DF7D126-4050-47f0-A7CF-4C4CA9241333");
		
		public static Guid CLSID_DVBCLocator	= new Guid("C531D9FD-9685-4028-8B68-6E1232079F1E");
	}

}
