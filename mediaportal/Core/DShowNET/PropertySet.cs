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
using System.Collections;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace DShowNET
{
	public class DigitalEverywhere : IksPropertyUtils
	{
		[StructLayout(LayoutKind.Explicit,Size=56), ComVisible(true)]
		struct FIRESAT_SELECT_PIDS_DVBT
		{
			[FieldOffset(0)] public bool			bCurrentTransponder;//Set TRUE
			[FieldOffset(4)] public bool			bFullTransponder;   //Set FALSE when selecting PIDs
			[FieldOffset(8)] public uint			uFrequency;    // kHz 47.000-860.000
			[FieldOffset(12)] public byte			uBandwidth;    // BANDWIDTH_8_MHZ, BANDWIDTH_7_MHZ, BANDWIDTH_6_MHZ
			[FieldOffset(13)] public byte			uConstellation;// CONSTELLATION_DVB_T_QPSK,CONSTELLATION_QAM_16,CONSTELLATION_QAM_64,OFDM_AUTO
			[FieldOffset(14)] public byte			uCodeRateHP;   // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO
			[FieldOffset(15)] public byte			uCodeRateLP;   // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO
			[FieldOffset(16)] public byte			uGuardInterval;// GUARD_INTERVAL_1_32,GUARD_INTERVAL_1_16,GUARD_INTERVAL_1_8,GUARD_INTERVAL_1_4,OFDM_AUTO
			[FieldOffset(17)] public byte			uTransmissionMode;// TRANSMISSION_MODE_2K, TRANSMISSION_MODE_8K, OFDM_AUTO
			[FieldOffset(18)] public byte			uHierarchyInfo;// HIERARCHY_NONE,HIERARCHY_1,HIERARCHY_2,HIERARCHY_4,OFDM_AUTO
			[FieldOffset(19)] public byte			dummy; // 
			[FieldOffset(20)] public byte			uNumberOfValidPids; // 1-16
			[FieldOffset(21)] public byte			dummy2; // 
			[FieldOffset(22)] public ushort		uPid1 ;
			[FieldOffset(24)] public ushort		uPid2 ;
			[FieldOffset(26)] public ushort		uPid3 ;
			[FieldOffset(28)] public ushort		uPid4 ;
			[FieldOffset(30)] public ushort		uPid5 ;
			[FieldOffset(32)] public ushort		uPid6 ;
			[FieldOffset(34)] public ushort		uPid7 ;
			[FieldOffset(36)] public ushort		uPid8 ;
			[FieldOffset(38)] public ushort		uPid9 ;
			[FieldOffset(40)] public ushort		uPid10 ;
			[FieldOffset(42)] public ushort		uPid11 ;
			[FieldOffset(44)] public ushort		uPid12 ;
			[FieldOffset(46)] public ushort		uPid13 ;
			[FieldOffset(48)] public ushort		uPid14 ;
			[FieldOffset(50)] public ushort		uPid15 ;
			[FieldOffset(52)] public ushort		uPid16 ;
			[FieldOffset(54)] public ushort		dummy3;
		}
		static public readonly Guid KSPROPSETID_Firesat = new Guid( 0xab132414, 0xd060, 0x11d0,  0x85, 0x83, 0x00, 0xc0, 0x4f, 0xd9, 0xba,0xf3  );
		const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C=8;
		const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T=6;
		const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S=2;

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

			//Log.Write("SendPMTToFireDTV pmt:{0}", pmtLength);
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
				Log.Write("SendPMTToFireDTV() SendPMT is not supported");
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
			string log=String.Format("pmt len:{0} data:",pmtLength);
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
				Log.WriteFile(Log.LogType.Log,true,"FireDTV:SetPMT() failed 0x{0:X} offs:{1}",hr, offs);
				return false;
			}
			return true;
		}//public bool SendPMTToFireDTV(byte[] PMT)

		public bool SetPIDS(bool isDvbc, bool isDvbT, bool isDvbS, bool isAtsc, ArrayList pids)
		{
			if (!isDvbT) return false;
			IKsPropertySet propertySet= captureFilter as IKsPropertySet;
			Guid propertyGuid=KSPROPSETID_Firesat;
			uint IsTypeSupported=0;
			int hr=propertySet.QuerySupported( ref propertyGuid, (uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T, out IsTypeSupported);
			if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Set)==0) 
			{
				Log.Write("SendPMTToFireDTV() SetPIDS is not supported");
				return true;
			}

			FIRESAT_SELECT_PIDS_DVBT dvbtStruct = new FIRESAT_SELECT_PIDS_DVBT();
			dvbtStruct.bCurrentTransponder=true;
			dvbtStruct.bFullTransponder=true;
			if (pids.Count>0)
			{
				dvbtStruct.bFullTransponder=false;
				dvbtStruct.uNumberOfValidPids=(byte)pids.Count;
				if (pids.Count >=1) dvbtStruct.uPid1=(ushort)pids[0];
				if (pids.Count >=2) dvbtStruct.uPid2=(ushort)pids[1];
				if (pids.Count >=3) dvbtStruct.uPid3=(ushort)pids[2];
				if (pids.Count >=4) dvbtStruct.uPid4=(ushort)pids[3];
				if (pids.Count >=5) dvbtStruct.uPid5=(ushort)pids[4];
				if (pids.Count >=6) dvbtStruct.uPid6=(ushort)pids[5];
				if (pids.Count >=7) dvbtStruct.uPid7=(ushort)pids[6];
				if (pids.Count >=8) dvbtStruct.uPid8=(ushort)pids[7];
				if (pids.Count >=9) dvbtStruct.uPid9=(ushort)pids[8];
				if (pids.Count >=10) dvbtStruct.uPid10=(ushort)pids[9];
				if (pids.Count >=11) dvbtStruct.uPid11=(ushort)pids[10];
				if (pids.Count >=12) dvbtStruct.uPid12=(ushort)pids[11];
				if (pids.Count >=13) dvbtStruct.uPid13=(ushort)pids[12];
				if (pids.Count >=14) dvbtStruct.uPid14=(ushort)pids[13];
				if (pids.Count >=15) dvbtStruct.uPid15=(ushort)pids[14];
				if (pids.Count >=16) dvbtStruct.uPid16=(ushort)pids[15];

			}
			
			int len=Marshal.SizeOf(dvbtStruct) ;
			
			IntPtr pDataInstance = Marshal.AllocCoTaskMem( len);
			IntPtr pDataReturned = Marshal.AllocCoTaskMem(len);
			Marshal.StructureToPtr(dvbtStruct,pDataInstance,true);
			Marshal.StructureToPtr(dvbtStruct,pDataReturned,true);

			Log.WriteFile(Log.LogType.Log,true,"FireDTV:SetPIDS() count:{0} len:{1}",pids.Count,Marshal.SizeOf(dvbtStruct));

			string txt="";
			for (int i=0; i < len; ++i)
				txt += String.Format("0x{0:X} ",Marshal.ReadByte(pDataInstance,i));
			Log.Write("data:{0}",txt);
			hr=propertySet.RemoteSet(ref propertyGuid,
																	(uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T,
																  pDataInstance,(uint)len, 
																	pDataReturned,(uint)len );
			Marshal.FreeCoTaskMem(pDataReturned);
			Marshal.FreeCoTaskMem(pDataInstance);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Log,true,"FireDTV:SetPIDS() failed 0x{0:X}",hr);
				return false;
			}

			return true;
		}
	}
}
