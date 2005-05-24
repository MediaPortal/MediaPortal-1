using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Summary description for ATSCSections.
	/// </summary>
	public class ATSCSections
	{

		#region imports

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionPtr(int section,ref IntPtr dataPointer,ref int len,ref int header,ref int tableExtId,ref int version,ref int secNum,ref int lastSecNum);
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool ReleaseSectionsBuffer();
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionData(DShowNET.IBaseFilter filter,int pid,int tid,ref int sectionCount,int tableSection,int timeout);
		// globals
		#endregion

		ArrayList							m_sectionsList;	
		int									m_timeoutMS=1000; // the timeout in milliseconds
		public ATSCSections()
		{
		}
		
		//
		public int Timeout
		{
			get{return m_timeoutMS;}
			set{m_timeoutMS=value;}
		}

		public DVBSections.Transponder Scan(DShowNET.IBaseFilter filter)
		{
			m_sectionsList=new ArrayList();	
			DVBSections.Transponder transponder = new DVBSections.Transponder();

			transponder.channels = new ArrayList();
			transponder.PMTTable = new ArrayList();

			//get Master Guide table (pid=0x1FFB)
			GetStreamData(filter,0x1ffb, 0xc7,0,Timeout);
			if (m_sectionsList.Count==0) return transponder;
			foreach(byte[] arr in m_sectionsList)
				DecodeMasterGuideTable(arr);
			return transponder;
		}

		void DecodeMasterGuideTable(byte[] buf)
		{
			// tid   
			//  8       112-12-- -------- 16------ -------- 2-5----1 8------- 8------- 8------- 16------ --------
      // 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
			//    0        1        2         3        4       5       6         7        8       9        10
			int table_id = buf[0];
			int section_syntax_indicator = (buf[1]>>7) & 1;
			int private_indicator = (buf[1]>>6) & 1;
			int section_length = ((buf[1]& 0xF)<<8) + buf[2];
			int transport_stream_id = (buf[3]<<8)+buf[4];
			int version_number = ((buf[5]>>1)&0x1F);
			int current_next_indicator = buf[5] & 1;
			int section_number = buf[6];
			int last_section_number = buf[7];
			int protocol_version = buf[8];
			int tables_defined = (buf[9]<<8) + buf[10];

			//decode tables...
			int start=11;
			// 16------ -------- 3--13--- -------- 3--5---- 32------ -------- -------- -------- 4---12-- --------
			// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
			//    0        1        2         3        4       5       6         7        8       9        10
			for (int i=0; i < tables_defined; ++i)
			{
				int table_type								 =  (buf[start]<<8) + (buf[start+1]);
				int table_type_PID						 = ((buf[start+2]&0xf)<<8) + (buf[start+3]);
				int table_type_version				 =   buf[start+4] & 0x1f;
				int number_of_bytes					   =  (buf[start+5]<<24) + (buf[start+6]<<16) + (buf[start+7]<<8)+ buf[start+8];
				int table_type_descriptors_len = ((buf[start+9]&0xf)<<8) + buf[start+10];
				int pos=0;
				int ofs=start+11;
				while (pos < table_type_descriptors_len)
				{
					int descriptor_tag = buf[ofs];
					int descriptor_len = buf[ofs+1];
					switch (descriptor_tag)
					{
						case 0x80: //stuffing
							break;
						case 0x81: //AC3 audio descriptor
							break;
						case 0x86: //caption service descriptor
							break;
						case 0x87: //content advisory descriptor
							break;
						case 0xa0: //extended channel name descriptor
							break;
						case 0xa1: //service location descriptor
							break;
						case 0xa2: //time-shifted service descriptor
							break;
						case 0xa3: //component name descriptor
							break;
						case 0xa8: //DCC departing request descriptor
							break;
						case 0xa9: //DCC arriving request descriptor
							break;
						case 0xaa: //redistribution control descriptor
							break;
					}
					pos += (2+descriptor_len);
					ofs += (2+descriptor_len);
				}
				start= start + 11 + table_type_descriptors_len;
			}
			//todo decoder other descriptors
		}

		private bool GetStreamData(DShowNET.IBaseFilter filter,int pid, int tid,int tableSection,int timeout)
		{
			bool flag;
			int dataLen=0;
			int header=0;
			int tableExt=0;
			int sectNum=0;
			int sectLast=0;
			int version=0;
			byte[] arr = new byte[1];
			IntPtr sectionBuffer=IntPtr.Zero;

			m_sectionsList=new ArrayList();
			flag = GetSectionData(filter,pid, tid,ref sectLast,tableSection,timeout);
			if(flag==false)
			{
				Log.WriteFile(Log.LogType.Capture,"ATSCSections:GetStreamdata() failed for pid:{0:X} tid:{1:X} section:{2} timeout:{3}", pid,tid,tableSection,timeout);
				return false;
			}
			if (sectLast<=0)
			{
				Log.WriteFile(Log.LogType.Capture,"ATSCSections:Sections:GetStreamdata() timeout for pid:{0:X} tid:{1:X} section:{2} timeout:{3}", pid,tid,tableSection,timeout);
			}
			for(int n=0;n<sectLast;n++)
			{
				flag=GetSectionPtr(n,ref sectionBuffer,ref dataLen,ref header, ref tableExt, ref version,ref sectNum, ref sectLast);
				if(flag)
				{
					if (tableExt != - 1)
					{
						arr = new byte[dataLen+8 + 1];
						try
						{
							Marshal.Copy(sectionBuffer, arr, 8, dataLen);
						}
						catch
						{
							Log.WriteFile(Log.LogType.Capture,true,"ATSCSections: error on copy data. address={0}, length ={1}",sectionBuffer,dataLen);
							m_sectionsList.Clear();
							break;
						}
						arr[0] = (byte)tid;
						arr[1] = (byte)((header >> 8) & 255);
						arr[2] =(byte) (header & 255);
						arr[3] = (byte)((tableExt >> 8) & 255);
						arr[4] = (byte)(tableExt & 255);
						arr[5] =(byte) version;
						arr[6] = (byte)sectNum;
						arr[7] = (byte)sectLast;
						m_sectionsList.Add(arr);
						if(tableSection!=0)
							break;
					}
					else
					{
						arr = new byte[dataLen+3 + 1];
						try
						{
							Marshal.Copy(sectionBuffer, arr, 3, dataLen);
						}
						catch
						{
							Log.WriteFile(Log.LogType.Capture,true,"ATSCSections: error on copy data. address={0}, length ={1}",sectionBuffer,dataLen);
							m_sectionsList.Clear();
							break;
						}
						arr[0] = System.Convert.ToByte(tid);
						arr[1] = System.Convert.ToByte((header >> 8) & 255);
						arr[2] = System.Convert.ToByte(header & 255);
						m_sectionsList.Add(arr);
						if(tableSection!=0)
							break;
					}// else

				}// if(flag)
			}//for
			ReleaseSectionsBuffer();
			return true;
		}
		
	}
}
