using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
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


		public enum eStreamOutput:int
		{
			Program       = 0,
			Transport     = 1,
			Mpeg1         = 2,
			PES_AV        = 3,
			PES_Video     = 5,
			PES_Audio     = 7,
			DVD           = 10,
			VCD           = 11
		};

		public enum eVideoFormat:byte
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
			Log.Write("IVAC: setvideobitrate min:{0} max:{1} vbr:{2}",minKbps,maxKbps,isVBR);
			videoBitRate bitrate=new videoBitRate();
			if (isVBR) bitrate.bEncodingMode=eBitRateMode.Vbr;
			else bitrate.bEncodingMode=eBitRateMode.Cbr;

			bitrate.wBitrate=(ushort)((minKbps*1000)/400);
			bitrate.dwPeak=(uint)((maxKbps*1000)/400);
			SetStructure(IvacGuid,(uint)PropertyId.IVAC_BITRATE, typeof(videoBitRate), (object)bitrate) ;

			GetVideoBitRate(out minKbps, out maxKbps, out isVBR);
		}


		public eVideoFormat VideoFormat
		{
			get 
			{
				return (eVideoFormat)GetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_TV_ENCODE_FORMAT);
			}
			set
			{
				int byValue=(int)value;
				SetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_TV_ENCODE_FORMAT, byValue);
			}
		}

		public Size VideoResolution
		{
			get 
			{
				int videoRes= (GetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_VIDEO_RESOLUTION) );
				if (VideoFormat==eVideoFormat.NTSC)
				{
					switch (videoRes)
					{
						case (int)eVideoResolution.Resolution_720x480:
							return new Size(720,480);
						case (int)eVideoResolution.Resolution_480x480:
							return new Size(480,480);
						case (int)eVideoResolution.Resolution_352x480:
							return new Size(352,480);
							//case (int)eVideoResolution.Resolution_352x240:
							//  return new Size(352,240);
						case (int)eVideoResolution.Resolution_320x240:
							return new Size(320,240);
					}
				}
				else
				{
					switch (videoRes)
					{
						case (int)eVideoResolution.Resolution_720x576:
							return new Size(720,576);
						case (int)eVideoResolution.Resolution_480x576:
							return new Size(480,576);
						case (int)eVideoResolution.Resolution_352x288:
							return new Size(352,288);
					}
				}
				return new Size(0,0);
			}
			set
			{
				int byValue=0;
				if (value.Width==720 && value.Height==480) byValue=(int)eVideoResolution.Resolution_720x480;
				if (value.Width==480 && value.Height==480) byValue=(int)eVideoResolution.Resolution_480x480;
				if (value.Width==352 && value.Height==480) byValue=(int)eVideoResolution.Resolution_352x480;
				if (value.Width==352 && value.Height==240) byValue=(int)eVideoResolution.Resolution_352x240;
				if (value.Width==320 && value.Height==240) byValue=(int)eVideoResolution.Resolution_320x240;

				if (value.Width==720 && value.Height==576) byValue=(int)eVideoResolution.Resolution_720x576;
				if (value.Width==480 && value.Height==576) byValue=(int)eVideoResolution.Resolution_480x576;
				if (value.Width==352 && value.Height==576) byValue=(int)eVideoResolution.Resolution_352x576;
				if (value.Width==352 && value.Height==288) byValue=(int)eVideoResolution.Resolution_352x288;

				SetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_VIDEO_RESOLUTION, byValue);
			}
		}
    
		public string VersionInfo
		{
			get
			{
				string version=GetString(IvacGuid,(uint)PropertyId.IVAC_VERSION_INFO);
				StreamOutput=eStreamOutput.Program;
				Size res = VideoResolution;
				int minKbps, maxKbps;
				bool isVBR;
				GetVideoBitRate(out minKbps, out maxKbps,out isVBR);
				Log.Write("IVAC: version:{0} streamtype:{1} format:{2} resolution:{3}x{4}",version, StreamOutput.ToString(),VideoFormat.ToString(),res.Width,res.Height);
				Log.Write("IVAC: average bitrate:{0} KBPs peak:{1} KBPs vbr:{2}",minKbps,maxKbps,isVBR);

				return version;
			}
		}
		public eStreamOutput StreamOutput
		{
			get 
			{
				return (eStreamOutput)GetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_OUTPUT_TYPE);
			}
			set
			{
				int iValue=(int)value;
				SetIntValue(IVac.IvacGuid,(uint)IVac.PropertyId.IVAC_OUTPUT_TYPE, iValue);
			}
		}
	}
}