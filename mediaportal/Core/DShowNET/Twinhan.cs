using System;
using System.Runtime.InteropServices; 
using MediaPortal.GUI.Library;

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

		public Twinhan(IBaseFilter filter)
				:base(filter)
		{
		}
		public bool IsTwinhan
		{
			get
			{
				if (captureFilter==null) 
				{
					return false;
				}
				IPin pin=DirectShowUtil.FindPinNr(captureFilter,PinDirection.Input,0);
				if (pin==null) 
				{
					return false;
				}
				IKsPropertySet propertySet= pin as IKsPropertySet;
				if (propertySet==null) 
				{
					return false;
				}
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
			Log.Write("Twinham send PMT len:{0} video:0x{1:X} audio:0x{2:X}", pmtLen,videoPid,audioPid);
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

		
			int thbdaLen=0x28;
			IntPtr thbdaBuf = Marshal.AllocCoTaskMem(thbdaLen);
			Marshal.WriteInt32(thbdaBuf,0,0x255e0082);
			Marshal.WriteInt16(thbdaBuf,4,0x2017);
			Marshal.WriteInt16(thbdaBuf,6,0x4b03);
			Marshal.WriteByte(thbdaBuf,8,0x90);
			Marshal.WriteByte(thbdaBuf,9,0xf8);
			Marshal.WriteByte(thbdaBuf,10,0x85);
			Marshal.WriteByte(thbdaBuf,11,0x6a);
			Marshal.WriteByte(thbdaBuf,12,0x62);
			Marshal.WriteByte(thbdaBuf,13,0xcb);
			Marshal.WriteByte(thbdaBuf,14,0x3d);
			Marshal.WriteByte(thbdaBuf,15,0x67);
			Marshal.WriteInt32(thbdaBuf,16,(int)THBDA_IOCTL_CI_PARSER_PMT);
			Marshal.WriteInt32(thbdaBuf,20,ptrPARSERPMTINFO.ToInt32());
			Marshal.WriteInt32(thbdaBuf,24,lenParserPMTInfo);
			Marshal.WriteInt32(thbdaBuf,28,0);
			Marshal.WriteInt32(thbdaBuf,32,0);
			Marshal.WriteInt32(thbdaBuf,36,ptrDwBytesReturned.ToInt32());

			IPin pin=DirectShowUtil.FindPinNr(captureFilter,PinDirection.Input,0);
			if (pin!=null) 
			{
				IKsPropertySet propertySet= pin as IKsPropertySet;
				if (propertySet!=null) 
				{
					Guid propertyGuid=THBDA_TUNER;
					int hr=propertySet.RemoteSet(ref propertyGuid,0,IntPtr.Zero,0, thbdaBuf,(uint)thbdaLen );
					if (hr!=0)
					{
						Log.Write("SetStructure() failed 0x{0:X}",hr);
					}
					else
						Log.Write("SetStructure() returned ok 0x{0:X}",hr);
				}
			}


			Marshal.FreeCoTaskMem(thbdaBuf);
			Marshal.FreeCoTaskMem(ptrDwBytesReturned);
			Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
			Marshal.FreeCoTaskMem(ptrPMT);

		}

		protected override void SetStructure(Guid guidPropSet, uint propId, System.Type structureType, object structValue)
		{
			Guid propertyGuid=guidPropSet;
			IPin pin=DirectShowUtil.FindPinNr(captureFilter,PinDirection.Input,0);
			if (pin==null) return ;
			IKsPropertySet propertySet= pin as IKsPropertySet;
			if (propertySet==null) 
			{
				Log.Write("SetStructure() properySet=null");
				return ;
			}

			int iSize=Marshal.SizeOf(structureType);
			Log.Write("size:{0}",iSize);
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(iSize);
			Marshal.StructureToPtr(structValue,pDataReturned,true);
			int hr=propertySet.RemoteSet(ref propertyGuid,propId,IntPtr.Zero,0, pDataReturned,(uint)Marshal.SizeOf(structureType) );
			if (hr!=0)
			{
				Log.Write("SetStructure() failed 0x{0:X}",hr);
			}
			else
				Log.Write("SetStructure() returned ok 0x{0:X}",hr);
			Marshal.FreeCoTaskMem(pDataReturned);
		}

	}
}
