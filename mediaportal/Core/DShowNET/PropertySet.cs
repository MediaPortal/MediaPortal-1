using System;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace DShowNET
{
	public class DigitalEverywhere : IksPropertyUtils
	{
		static public readonly Guid KSPROPSETID_Firesat = new Guid( 0xab132414, 0xd060, 0x11d0,  0x85, 0x83, 0x00, 0xc0, 0x4f, 0xd9, 0xba,0xf3  );
		public DigitalEverywhere(IBaseFilter filter) 
			:base(filter)
		{
		}

		public bool IsDigitalEverywhere
		{
			get 
			{
				IKsPropertySet propertySet= captureFilter as IKsPropertySet;
				if (propertySet==null) return false;
				Guid propertyGuid=KSPROPSETID_Firesat;
				uint IsTypeSupported=0;
				int hr=propertySet.QuerySupported( ref propertyGuid, 22, out IsTypeSupported);
				if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
				{
					return false;
				}
				return true;
			}
		}
		/// <summary>
		/// This function sends the PMT (Program Map Table) to the FireDTV DVB-T/DVB-C/DVB-S card
		/// This allows the integrated CI & CAM module inside the FireDTv device to decrypt the current TV channel
		/// (provided that offcourse a smartcard with the correct subscription and its inserted in the CAM)
		/// </summary>
		/// <param name="PMT">Program Map Table received from digital transport stream</param>
		/// <remarks>
		/// 1. first byte in PMT is 0x02=tableId for PMT
		/// 2. This function is vender specific. It will only work on the FireDTV devices
		/// </remarks>
		/// <preconditions>
		/// 1. FireDTV device should be tuned to a digital DVB-C/S/T TV channel 
		/// 2. PMT should have been received 
		/// </preconditions>
		public bool SendPMTToFireDTV(byte[] PMT, int pmtLength)
		{
			if (PMT==null) return false;
			if (pmtLength==0) return false;

			Log.Write("SendPMTToFireDTV pmt:{0}", pmtLength);
			Guid propertyGuid=KSPROPSETID_Firesat;
			int propId=22;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			uint IsTypeSupported=0;
			if (propertySet==null) 
			{
				Log.Write("SendPMTToFireDTV() properySet=null");
				return true;
			}

			int hr=propertySet.QuerySupported( ref propertyGuid, (uint)propId, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
			{
				Log.Write("SendPMTToFireDTV() GetStructure is not supported");
				return true;
			}

			int iSize=12+2+pmtLength;
			IntPtr pDataInstance = Marshal.AllocCoTaskMem(1036);
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(1036);
			int offs=0;

			//example data:0x0 0x2 0x0 0x0 0x0 0x0 0x0 0x0 0x14 0x0 0x3 0x1 | 0x2 0xB0 0x12 0x1 0x3D 0xC1 0x0 0x0 0xFF 0xFF 0xF0 0x0 0x3 0xEC 0x8C 0xF0 0x0 0xD3 
			byte[] byData = new byte[1036];
			uint uLength=(uint)(2+pmtLength);
			byData[offs]= 0; offs++;//slot
			byData[offs]= 2; offs++;//utag

			byData[offs]= 0; offs++;//padding
			byData[offs]= 0; offs++;//padding

			byData[offs]= 0; offs++;//bmore
			
			byData[offs]= 0; offs++;//padding
			byData[offs]= 0; offs++;//padding
			byData[offs]= 0; offs++;//padding
			
			byData[offs]= (byte)(uLength%256); offs++;		//ulength lo
			byData[offs]= (byte)(uLength/256); offs++;		//ulength hi
			
			//byData[offs]= 0; offs++;
			//byData[offs]= 0; offs++;
			
			byData[offs]= 3; offs++;// List Management = ONLY
			byData[offs]= 1; offs++;// pmt_cmd = OK DESCRAMBLING		
			for (int i=0; i < pmtLength;++i)
			{
				byData[offs]=PMT[i];
				offs++;
			}
			string log="data:";
			for (int i=0; i < offs;++i)
			{
				Marshal.WriteByte(pDataInstance,i,byData[i]);
				Marshal.WriteByte(pDataReturned,i,byData[i]);
				log += String.Format("0x{0:X} ",byData[i]);
			}

			Log.Write(log);
			hr=propertySet.RemoteSet(ref propertyGuid,(uint)propId,pDataInstance,(uint)1036, pDataReturned,(uint)1036 );
			Marshal.FreeCoTaskMem(pDataReturned);
			Marshal.FreeCoTaskMem(pDataInstance);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Log,true,"FireDTV:SetStructure() failed 0x{0:X} offs:{1}",hr, offs);
				return false;
			}
			return true;
		}//public bool SendPMTToFireDTV(byte[] PMT)
	}
}
