using System;
using System.Runtime.InteropServices;
namespace DShowNET
{
	public class IVac : IksPropertyUtils
	{

		static readonly Guid IvacGuid=new Guid(0xd2185a40, 0x0398, 0x11d3, 0xa5, 0x3e, 0x0, 0xa0, 0xc9, 0xef, 0x50, 0x6a );
		enum PropertyId
		{
			IVAC_OUTPUT_TYPE			= 0,		// Get & Set
			IVAC_BITRATE				= 1,		// Get & Set
			IVAC_VIDEO_RESOLUTION		= 3,		// Get & Set
			IVAC_TV_ENCODE_FORMAT		= 4,		// Get & Set
			IVAC_GOP_SIZE				= 6,		// Get & Set
			IVAC_CLOSED_GOP				= 7,		// Get & Set
			IVAC_VERSION_INFO			= 27,		// Get only
			IVAC_INVERSE_TELECINE		= 50,		// Get & Set
		} ;
		enum eBitRateMode:int
		{
			Cbr    = 0x00,
			Vbr    = 0x01
		};
    
		[StructLayout(LayoutKind.Sequential,Pack=1), ComVisible(true)]
			struct videoBitRate
		{
			public eBitRateMode    bEncodingMode;  // Variable or Constant bit rate
			public ushort          wBitrate;       // Actual bitrate in 1/400 mbits/sec
			public uint          dwPeak;         // Peak/400
		} 

		[StructLayout(LayoutKind.Sequential), ComVisible(true)]
			struct versionInfo
		{
			string DriverVersion; //xx.yy.zzz
			string FWVersion; //xx.yy.zzz
		} 


		enum eStreamOutput:int
		{
			PROGRAM       = 0,
			TRANSPORT     = 1,
			MPEG1         = 2,
			PES_AV        = 3,
			PES_Video     = 5,
			PES_Audio     = 7,
			DVD           = 10,
			VCD           = 11
		};

		enum eVideoFormat:byte
		{
			NTSC=0,
			PAL=1
		};

		enum eVideoResolution: byte
		{
			Resolution_720x480        = 0,
			Resolution_720x576        = 0, // For PAL
			Resolution_480x480        = 1,
			Resolution_480x576        = 1, // For PAL
			Resolution_352x480        = 2,
			Resolution_352x576        = 2, // For PAL
			Resolution_352x240        = 2, // For NTSC MPEG1
			Resolution_352x288        = 2,  // For PAL MPEG1
			Resolution_320x240        = 3  // For NTSC MPEG1
		};

		public IVac(IBaseFilter filter) 
			:base(filter)
		{
		}

		
		public bool IsIVAC
		{
			get 
			{
				IKsPropertySet propertySet= captureFilter as IKsPropertySet;
				if (propertySet==null) return false;
				Guid propertyGuid=IVac.IvacGuid;
				uint IsTypeSupported=0;
				int hr=propertySet.QuerySupported( ref propertyGuid, (uint)PropertyId.IVAC_VERSION_INFO, out IsTypeSupported);
				if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
				{
					return false;
				}
				return true;
			}
		}

		public void GetVideoBitRate(out int minKbps, out int maxKbps,out bool isVBR)
		{
			videoBitRate bitrate=new videoBitRate();
			object obj =GetStructure(IvacGuid,(uint)PropertyId.IVAC_BITRATE, typeof(videoBitRate)) ;
			try
			{ 
				bitrate = (videoBitRate)obj ;
			}
			catch (Exception){}
			isVBR = (bitrate.bEncodingMode==eBitRateMode.Vbr);
			minKbps=(int)((bitrate.wBitrate*400)/1000);
			maxKbps=(int)((bitrate.dwPeak*400)/1000);
		}
		
		public void SetVideoBitRate(int minKbps, int maxKbps,bool isVBR)
		{
			videoBitRate bitrate=new videoBitRate();
			if (isVBR) bitrate.bEncodingMode=eBitRateMode.Vbr;
			else bitrate.bEncodingMode=eBitRateMode.Cbr;

			bitrate.wBitrate=(ushort)((minKbps*1000)/400);
			bitrate.dwPeak=(uint)((maxKbps*1000)/400);
			SetStructure(IvacGuid,(uint)PropertyId.IVAC_BITRATE, typeof(videoBitRate), (object)bitrate) ;
		}

		public string VersionInfo
		{
			get
			{
				string version=GetString(IvacGuid,(uint)PropertyId.IVAC_VERSION_INFO);
				
				return version;
			}
		}
	}
}