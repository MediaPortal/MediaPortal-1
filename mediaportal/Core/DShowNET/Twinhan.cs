using System;
using System.Runtime.InteropServices; 

namespace DShowNET
{
	/// <summary>
	/// Summary description for Twinhan.
	/// </summary>
	public class Twinhan : IksPropertyUtils
	{
		readonly Guid THBDA_TUNER = new Guid( 0xE5644CC4, 0x17A1, 0x4eed,  0xBD, 0x90, 0x74, 0xFD, 0xA1, 0xD6, 0x54, 0x23);
		readonly Guid GUID_THBDA_CMD = new Guid( 0x255e0082, 0x2017, 0x4b03,  0x90, 0xf8, 0x85, 0x6a, 0x62, 0xcb, 0x3d, 0x67 );
		readonly uint THBDA_IOCTL_CI_PARSER_PMT=0xaa00033c;
		struct THBDACMD
		{
			public Guid    CmdGUID;            // Private Command GUID
			public uint    dwIoControlCode;    // operation
			public IntPtr  lpInBuffer;         // input data buffer
			public uint    nInBufferSize;      // size of input data buffer
			public IntPtr  lpOutBuffer;        // output data buffer
			public uint    nOutBufferSize;     // size of output data buffer
			public IntPtr lpBytesReturned;    // byte count
		};

		public Twinhan(IBaseFilter filter)
				:base(filter)
		{
		}
		public bool IsTwinhan
		{
			get
			{
				IKsPropertySet propertySet= captureFilter as IKsPropertySet;
				if (propertySet==null) return false;
				Guid propertyGuid=THBDA_TUNER;
				uint IsTypeSupported=0;
				int hr=propertySet.QuerySupported( ref propertyGuid, 0, out IsTypeSupported);
				if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
				{
					return false;
				}
				return true;
			}
		}

		public void SendPMT(uint videoPid, uint audioPid,byte[] PMT, int pmtLen)
		{
			IntPtr ptrPMT = Marshal.AllocCoTaskMem(pmtLen);
			for (int i=0; i < pmtLen;++i) Marshal.WriteByte(ptrPMT,i,PMT[i]);

			
			int lenParserPMTInfo=5*4;
			IntPtr ptrPARSERPMTINFO = Marshal.AllocCoTaskMem(lenParserPMTInfo);
			Marshal.WriteInt32(ptrPARSERPMTINFO,0,ptrPMT.ToInt32());
			Marshal.WriteInt32(ptrPARSERPMTINFO,4,PMT.Length);
			Marshal.WriteInt32(ptrPARSERPMTINFO,8,(int)videoPid);
			Marshal.WriteInt32(ptrPARSERPMTINFO,12,(int)audioPid);
			Marshal.WriteInt32(ptrPARSERPMTINFO,16,1);//default cam

			IntPtr ptrDwBytesReturned = Marshal.AllocCoTaskMem(4);

			THBDACMD bdaCmd = new THBDACMD();
			bdaCmd.CmdGUID=GUID_THBDA_CMD;
			bdaCmd.dwIoControlCode = THBDA_IOCTL_CI_PARSER_PMT;
			bdaCmd.lpInBuffer = ptrPARSERPMTINFO;
			bdaCmd.nInBufferSize = (uint)lenParserPMTInfo;
			bdaCmd.lpOutBuffer = IntPtr.Zero;
			bdaCmd.nOutBufferSize = 0;
			bdaCmd.lpBytesReturned = ptrDwBytesReturned;

			base.SetStructure(THBDA_TUNER,0,typeof(THBDACMD), bdaCmd);
			
			Marshal.FreeCoTaskMem(ptrDwBytesReturned);
			Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
			Marshal.FreeCoTaskMem(ptrPMT);

		}
	}
}
