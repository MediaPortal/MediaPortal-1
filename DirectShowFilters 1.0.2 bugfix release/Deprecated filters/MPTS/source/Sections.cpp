
/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#include <streams.h>
#include <aviriff.h>
#include "Sections.h"
#include <streams.h>

void LogDebug(const char *fmt, ...) 
{
//#ifndef DEBUG
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("mpts.log","a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		OutputDebugString(buffer);
		OutputDebugString("\n");
		fclose(fp);
	}
//#endif
};

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

Sections::Sections(FileReader *pFileReader)
{
	m_pFileReader=pFileReader;
	pids.StartPTS=0;
	pids.EndPTS=0;
	pids.Duration=0;
	pids.DurTime=0;
}

Sections::~Sections()
{
	//delete m_pFileReader;
}

HRESULT Sections::ParseFromFile()
{
	if (m_pFileReader->m_hInfoFile!=INVALID_HANDLE_VALUE)
	{	
		//LogDebug("sections::ParseFromFile() using live.info");
		DWORD			dwReadBytes;
		ULONGLONG		ptsStart;
		ULONGLONG		ptsNow;
		LARGE_INTEGER	li,writepos;
		li.QuadPart = 0;
		int ttxPid,subtitlePid;
		::SetFilePointer(m_pFileReader->m_hInfoFile, li.LowPart, &li.HighPart, FILE_BEGIN);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&writepos, 8, &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ptsStart, sizeof(ptsStart), &dwReadBytes, NULL) ;
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ptsNow, sizeof(ptsNow), &dwReadBytes, NULL) ;
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.AC3, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.AudioPid1, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.AudioPid2, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.VideoPid, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ttxPid, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.PMTPid, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&subtitlePid, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.PCRPid, sizeof(int), &dwReadBytes, NULL);
		if (pids.PCRPid==0) pids.PCRPid=pids.VideoPid;

		if (ptsStart>ptsNow) ptsStart=1;
		pids.CurrentAudioPid=pids.AudioPid1;
		pids.StartPTS=(__int64)ptsStart;
		pids.EndPTS=(__int64)ptsNow;
		pids.MPEG4=false;
		pids.Duration=ptsNow-ptsStart;
		pids.DurTime=pids.Duration;
		PTSToRefTime(ptsNow-ptsStart,pids.Duration);

		if (ptsNow==ptsStart)
			pids.Duration=10000000;
		__int64 filePointer=0;
		m_pFileReader->SetFilePointer(filePointer,FILE_BEGIN);
		m_pFileReader->SetOffset(0);

		return S_OK;
	}

	
	LogDebug("sections::ParseFromFile() parse pat/pmt");
	__int64 filePointer=0;
	HRESULT hr = S_OK;
	__int64 len;
	m_pFileReader->GetFileSize(&len);
	filePointer=m_pFileReader->GetFilePointer(); // to restore after reading
	FindPATPMT();
	CheckStream();
	if(pids.Duration<1600000000)
		pids.Duration=1600000000;
	m_pFileReader->SetFilePointer(filePointer,FILE_BEGIN);
	return hr;
}
HRESULT Sections::CheckStream()
{
	LogDebug("Sections::CheckStream()");
	__int64 fileSize;
	m_pFileReader->GetFileSize(&fileSize);
	__int64 fileEndOffset=fileSize-(1024LL*1024LL);
	fileEndOffset=(fileEndOffset/188)*188;
	if(m_pFileReader->OpenFile()==S_FALSE)
		return S_FALSE;
	HRESULT hr;
	BYTE pData[188];
	bool finished=false;
	ULONG countBytesRead;
	m_pFileReader->SetFilePointer(0,FILE_BEGIN);
	unsigned long offset=0;
	ULONGLONG filePosCounter=0;
	ULONGLONG pts=0;
	int pid=0;
	__int64 fpos=0;
	while(finished!=true)
	{
		hr=m_pFileReader->Read(pData,188,&countBytesRead);
		if (countBytesRead==0) 
		{
			finished=true;
			hr=E_FAIL;
		}
		if (hr!=S_OK) 
		{
			finished=true;
		}
//		m_pFileReader->SetFilePointer(countBytesRead,FILE_CURRENT);
		
		int apid;
		ULONGLONG currentPts;
		if (CurrentPTS(pData,&currentPts,&apid) == S_OK)
		{					
			if (pid==0) pid=apid;
			if (pid==apid)
			{
				pts=currentPts;
				if(pids.StartPTS==0)
				{
					pids.StartPTS=(__int64)pts; // first pts
					m_pFileReader->SetFilePointer(fileEndOffset,FILE_BEGIN);// sets to file-end - 1MB
				}
			}
		}
		fpos++;
	}
	// set end pts
	pids.EndPTS=(__int64)pts;
	pids.Duration=(pids.EndPTS-pids.StartPTS);
	pids.DurTime=pids.Duration;
	// calc duration in 100 ns

	PTSToRefTime(pids.EndPTS-pids.StartPTS,pids.Duration);
	//
	m_pFileReader->SetFilePointer(0,FILE_BEGIN);// sets to file begin
	LogDebug("Start PTS:%x  end PTS:%x", (DWORD)pids.StartPTS, (DWORD)pids.EndPTS);
	return S_OK;
}
HRESULT Sections::GetAdaptionHeader(BYTE *data,AdaptionHeader *header)
{
	header->Len=data[0];
	header->DiscontinuityIndicator=(data[1] & 0x80)>0?true:false;
	header->RandomAccessIndicator=(data[1] & 0x40)>0?true:false;
	header->ElementaryStreamPriorityIndicator=(data[1] & 0x20)>0?true:false;
	header->PCRFlag=(data[1] & 0x10)>0?true:false;
	header->OPCRFlag=(data[1] & 0x08)>0?true:false;
	header->SplicingPointFlag=(data[1] & 0x04)>0?true:false;
	header->TransportPrivateData=(data[1] & 0x02)>0?true:false;
	header->AdaptationHeaderExtension=(data[1] & 0x01)>0?true:false;
	if(header->PCRFlag==true)
	{

		__int64 pcr_H=(__int64)((data[2]& 0x080)>>7);
		__int64 pcr_L =(__int64)((data[2]&0x7f));pcr_L<<=25;
		pcr_L |= (__int64)(data[3]<<17); 
		pcr_L |= (__int64) (data[4]<<9); 
		pcr_L |= (__int64) ((data[5])<<1);
		pcr_L |= (__int64)((data[6]&0x80)>>7);
		__int64 ull=ull = (pcr_H << 32) + pcr_L;
		header->PCRValue=ull;
		//GetPTS(&data[2],&(header->PCRValue));
		header->PCRCounter=((data[6] & 0x01)*256)+data[7];
	}
	return S_OK;
}
HRESULT Sections::GetTSHeader(BYTE *data,TSHeader *header)
{
	header->SyncByte=data[0];
	header->TransportError=(data[1] & 0x80)>0?true:false;
	header->PayloadUnitStart=(data[1] & 0x40)>0?true:false;
	header->TransportPriority=(data[1] & 0x20)>0?true:false;
	header->Pid=((data[1] & 0x1F) <<8)+data[2];
	header->TScrambling=data[3] & 0xC0;
	header->AdaptionControl=(data[3]>>4) & 0x3;
	header->ContinuityCounter=data[3] & 0x0F;
	return S_OK;
}
HRESULT Sections::GetPESHeader(BYTE *data,PESHeader *header)
{
	header->Reserved=(data[0] & 0xC0)>>6;
	header->ScramblingControl=(data[0] &0x30)>>4;
	header->Priority=(data[0] & 0x08)>>3;
	header->dataAlignmentIndicator=(data[0] & 0x04)>>2;
	header->Copyright=(data[0] & 0x02)>>1;
	header->Original=data[0] & 0x01;
	header->PTSFlags=(data[1] & 0xC0)>>6;
	header->ESCRFlag=(data[1] & 0x20)>>5;
	header->ESRateFlag=(data[1] & 0x10)>>4;
	header->DSMTrickModeFlag=(data[1] & 0x08)>>3;
	header->AdditionalCopyInfoFlag=(data[1] & 0x04)>>2;
	header->PESCRCFlag=(data[1] & 0x02)>>1;
	header->PESExtensionFlag=data[1] & 0x01;
	header->PESHeaderDataLength=data[2];
	return S_OK;
}
void Sections::GetPTS(BYTE *data,ULONGLONG *pts)
{
	//High:1 Low:32 reserved:6 extension:9
	ULONGLONG ptsVal;
	bool ptsFlag=false;
	ptsVal= 0xFFFFFFFFL & ( (6&data[0])<<29 | (255&data[1])<<22 | (254&data[2])<<14 | (255&data[3])<<7 | (((254&data[4])>>1)& 0x7F));
	if ((ptsVal & 0xFF000000L)==0xFF000000L) 
		ptsFlag=true;
	if (ptsFlag && ptsVal<0xF0000000L) 
		ptsVal |=0x100000000L;
	*pts=ptsVal;
}
void Sections::PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime)
{
	ULONGLONG  _90khz = (ULONGLONG)(pts/90LL);
	ptsTime->h=(_90khz/(1000LL*60LL*60LL));
	ptsTime->m=(_90khz/(1000LL*60LL))-(ptsTime->h*60LL);
	ptsTime->s=(_90khz/1000LL)-(ptsTime->h*3600LL)-(ptsTime->m*60LL);
	ptsTime->u=_90khz-(ptsTime->h*1000LL*60LL*60LL)-(ptsTime->m*1000LL*60LL)-(ptsTime->s*1000LL);
}
void Sections::PTSToRefTime(ULONGLONG pts,REFERENCE_TIME& refTime)
{
	PTSTime time;
	PTSToPTSTime(pts,&time);
	refTime=MILLISECONDS_TO_100NS_UNITS(time.u);
	refTime+=MILLISECONDS_TO_100NS_UNITS(time.s*1000);
	refTime+=MILLISECONDS_TO_100NS_UNITS(time.m*1000LL*60L);
	refTime+=MILLISECONDS_TO_100NS_UNITS(time.h*1000LL*60L*60L);

}

HRESULT Sections::CurrentPTS(BYTE *pData,ULONGLONG *ptsValue,int* pid)
{
	HRESULT hr=S_FALSE;
	*ptsValue=0;
	TSHeader header;
	GetTSHeader(pData,&header);
	int offset=4;
	bool found=false;

	*pid=header.Pid;
	if (header.Pid<1) return S_FALSE;
	if (header.SyncByte!=0x47) return S_FALSE;
	//if (header.PayloadUnitStart==0) return S_FALSE;
	if(header.Pid!=pids.CurrentAudioPid &&
	   header.Pid != pids.VideoPid &&
	   header.Pid != pids.PCRPid)
		return S_FALSE;

	//          0  1  2  3    4  5    6
	//  0000:  [47 82 00 f3] [cc 5b] [38 2a  0e ad e4 52 12 75 a0 46   G....[8*...R.u.F
	//        program_clock_reference:
    //        baseH: 0 (0x00)
    //        baseL: 1884560731 (0x70541d5b)
    //        reserved: 50 (0x32)
    //        extension: 82 (0x0052)
    //         ==> program_clock_reference: 565368219382 (0x83a29266f6)  [= PCR-Timestamp: 5:48:59.563680]
    //    original_program_clock_reference:
    //        baseH: 0 (0x00)
    //        baseL: 619397261 (0x24eb408d)
    //        reserved: 8 (0x08)
    //        extension: 11 (0x000b)
    //         ==> original_program_clock_reference: 185819178311 (0x2b43afa547)  [= PCR-Timestamp: 1:54:42.191789]

	if(header.AdaptionControl==3 || header.AdaptionControl==2)
	{
		AdaptionHeader ah;
		GetAdaptionHeader(&pData[4],&ah);
		if(ah.PCRFlag)
		{
			PTSTime ptsTime;
			PTSToPTSTime(ah.PCRValue,&ptsTime);
			*ptsValue=ah.PCRValue;
			hr=S_OK;
		}
	}

	//if (offset< 0 || offset+14>=188) return S_FALSE;
	//if(header.SyncByte==0x47 && pData[offset]==0 && pData[offset+1]==0 && pData[offset+2]==1)
	//{
	//	int code=pData[offset+3]| 0x100;
	//	if (!((code >= 0x1c0 && code <= 0x1df) ||
 //             (code >= 0x1e0 && code <= 0x1ef) ||
 //             (code == 0x1bd)))
	//	{
	//		return hr;
	//	}
	//	GetPESHeader(&pData[offset+6],&pes);
	//	if(pes.Reserved==0x02) // valid header
	//	{
	//		if(pes.PTSFlags==0x02 || pes.PTSFlags==0x03)
	//		{
	//			// pes packet found
	//			GetPTS(&pData[offset+9],ptsValue);
	//			//char buffer[128];
	//			//sprintf(buffer,"pid: %x pes:%x\n", header.Pid,(DWORD) (*ptsValue));
	//			//OutputDebugString(buffer);

	//			if (pes.ESCRFlag==0 && pes.ESRateFlag==0 && pes.DSMTrickModeFlag==0 && pes.AdditionalCopyInfoFlag==0 && pes.PESCRCFlag==0 && pes.PESExtensionFlag==0)
	//			{
	//				GetPTS(&pData[offset+9],ptsValue);
	//				hr=S_OK;
	//			}
	//		}
	//	}	
	//}
	return hr;
}
void Sections::FindPATPMT()
{
	if(m_pFileReader->OpenFile()==S_FALSE)
		return ;

	LogDebug("Sections::FindPATPMT()");
	HRESULT hr;
	BYTE pData[188];
	bool finished=false;
	ULONG countBytesRead;
	int actPid;
	TSHeader header;
	DWORD offset;
	bool ready=false;
	int pmt[255];
	int pmtcount=0;
	// first get all pmt pids by decoding the pat

	__int64 offsetInFile=0;
	while(true)
	{
		m_pFileReader->SetFilePointer(offsetInFile,FILE_BEGIN);
		hr=m_pFileReader->Read(pData,188,&countBytesRead);
		if(hr==S_OK)
		{
			GetTSHeader(pData,&header);
			if(header.SyncByte==0x47 && header.TransportError==FALSE)
			{
				hr=m_pFileReader->Read(pData,188,&countBytesRead);
				if(hr==S_OK)
				{
					GetTSHeader(pData,&header);
					if(header.SyncByte==0x47 && header.TransportError==FALSE)
					{
						LogDebug("Sections::Found start packet() at 0x%x",offsetInFile);
						break;
					}
				}
			}
		}
		else break;
		offsetInFile++;
	}

	m_pFileReader->SetOffset(offsetInFile);
	finished=false;
	m_pFileReader->SetFilePointer(0,FILE_BEGIN);
	pmtcount=0;
	long pmtSectionLen=0;
	while(finished!=true)
	{
		hr=m_pFileReader->Read(pData,188,&countBytesRead);
		if(hr!=S_OK|| countBytesRead!=188)
			finished=true;
		//m_pFileReader->SetFilePointer(countBytesRead,FILE_CURRENT);
		if(hr==S_OK)
		{
			GetTSHeader(pData,&header);
			if(header.SyncByte==0x47 && header.Pid==0 && header.TransportError==FALSE)// pat
			{
				if(header.PayloadUnitStart==true)
					offset=4+pData[4]+1;
				else
					offset=4;
				if(pData[offset]==0x00 && pData[offset+1]==0x00 && pData[offset+2]==0x01) continue;//PES
				int table_id = pData[offset+0];
				if (table_id!=0) continue;
				int section_syntax_indicator = (pData[offset+1]>>7) & 1;
				int section_length = ((pData[offset+1]& 0xF)<<8) + pData[offset+2];
				int transport_stream_id = (pData[offset+3]<<8)+pData[offset+4];
				int version_number = ((pData[offset+5]>>1)&0x1F);
				int current_next_indicator = pData[offset+5] & 1;
				int section_number = pData[offset+6];
				int last_section_number = pData[offset+7];

				LogDebug("Sections::FindPATPMT() pid:0x%x tsid:0x%x %d/%d len:%d", header.Pid,transport_stream_id,section_number,last_section_number, section_length);
				//if (section_number!=0||last_section_number!=0) continue;
				
	
				LogDebug("Sections::decodePMT() found PAT");
				int loop =(section_length - 9) / 4;
				int offset1=0;
				pmtcount=loop;
				for(int i=0;i<loop;i++)
				{
					offset1=offset+(8 +(i * 4));
					pmt[i]=((pData[offset1+2] & 0x1F)<<8)+pData[offset1+3];
					LogDebug("Sections::FindPATPMT() PMT:%i pid:%x",i,pmt[i]);
				}
				finished=true;
			}
		}
	}
	if(pmtcount==0)
	{
		LogDebug("Sections::FindPATPMT() no PAT found");
		pids.Clear();
		return ;
	}
	
	LogDebug("Sections::FindPATPMT() Get PMT");
	
	// get PMT
	m_pFileReader->SetFilePointer(0,FILE_BEGIN);
	finished=false;
	ULONG fpos=0;
	while(finished!=true)
	{
		//m_pFileReader->SetFilePointer(fpos,FILE_CURRENT);
		hr=m_pFileReader->Read(pData,188,&countBytesRead);
		if(hr!=S_OK|| countBytesRead!=188)
			finished=true;
		//fpos+=countBytesRead;
		if(hr==S_OK)
		{
			GetTSHeader(pData,&header);
			if (header.SyncByte!=0x47 || header.TransportError==TRUE) continue;
			for(int i=0;i<pmtcount;i++)
			{
				actPid=pmt[i];
				if(header.Pid==actPid && header.Pid>0)
				{
					if(header.PayloadUnitStart==true)
						offset=4+pData[4]+1;
					else
						offset=4;
					if(pData[offset]==0x02 && ((pData[offset+1]>>7) & 1)==1 && (pData[offset+5] & 1)==1 && pids.PMTPid==0)
					{					
						pids.PCRPid=((pData[offset+8]& 0x1F)<<8)+pData[offset+9];
						pids.PMTPid=actPid;
						pids.ProgramNumber=(pData[offset+3]<<8)+pData[offset+4];
						LogDebug("Sections::FindPATPMT() found PMT:%x pcr:%x",pids.PCRPid,pids.PMTPid);
						__int64 fpos=m_pFileReader->GetFilePointer();
						if (DecodePMT(&pData[offset]) )
						{
							LogDebug("Sections::FindPatPMT() PMT decoded");
							if (pids.VideoPid!=0) 
							{
								if (true||FindVideo())
								{
									LogDebug("Sections::FindPatPMT() got video");
									finished=true;
									ready=true;
									break;
								}
							}
							else
							{
								//radio
								break;
							}
						}
					}
				}
			}
		}
	}
}
bool Sections::DecodePMT(byte* PMT)
{
	LogDebug("sections::DecodePMT");
	bool finished;
	int hr;
	ULONG countBytesRead;
	int pmtSectionLen=((PMT[1]& 0xF)<<8) + PMT[2];
	LogDebug("Sections::decodePMT() decode PMT");
	BYTE buf[1200];
	byte pData[1200];
	memset(buf,0,1000);
	memcpy(buf,PMT,188);
	pmtSectionLen-=188;
	DWORD len=0;
	TSHeader header;
	GetTSHeader(PMT,&header);
	int contCounter=header.ContinuityCounter;
	int offset=0;
	int pos=188;
	while(pmtSectionLen>0)
	{
		hr=m_pFileReader->Read(pData,188,&countBytesRead);
		if(hr!=S_OK|| countBytesRead!=188)
			finished=true;
		if(hr==S_OK)
		{
			GetTSHeader(pData,&header);
			if(header.Pid==pids.PMTPid && contCounter+1==header.ContinuityCounter)
			{
				if(header.PayloadUnitStart==true)
					offset=4+pData[4]+1;
				else
					offset=4;
				len=188-offset;
				if(len>(DWORD)pmtSectionLen)
					len=pmtSectionLen;
				memcpy(&buf[pos],pData+offset,len);
				pos+=len;
				pmtSectionLen-=len;
				contCounter=header.ContinuityCounter;
				if(pmtSectionLen<1)
				{
					LogDebug("Sections::decodePMT() got PMT");
					finished=true;
				}
			}
		}
	}
					
	LogDebug("Sections::decodePMT() decode PMT");
	// pmt should now in the pmtData array
	int table_id = buf[0];
	int section_syntax_indicator = (buf[1]>>7) & 1;
	int section_length = ((buf[1]& 0xF)<<8) + buf[2];
	int program_number = (buf[3]<<8)+buf[4];
	int version_number = ((buf[5]>>1)&0x1F);
	int current_next_indicator = buf[5] & 1;
	int section_number = buf[6];
	int last_section_number = buf[7];
	int program_info_length = ((buf[10] & 0xF)<<8)+buf[11];
	int pcr_pid=((buf[8]& 0x1F)<<8)+buf[9];
	int len2 = program_info_length;
	int pointer = 12;
	int len1 = section_length - pointer;
	int x;
	// loop 1
	while (len2 > 0)
	{
		int indicator=buf[pointer];
		x = 0;
		x = buf[pointer + 1] + 2;
		len2 -= x;
		pointer += x;
		len1 -= x;
	}
	// loop 2
	int stream_type=0;
	int elementary_PID=0;
	int ES_info_length=0;
	pids.PCRPid=pids.CurrentAudioPid=pids.AudioPid1=pids.AudioPid2=pids.AudioPid3=pids.VideoPid=pids.AC3=0;
	pids.PCRPid=pcr_pid;

	while (len1 > 4)
	{
		stream_type = buf[pointer];
		elementary_PID = ((buf[pointer+1]&0x1F)<<8)+buf[pointer+2];
		ES_info_length = ((buf[pointer+3] & 0xF)<<8)+buf[pointer+4];
		if(stream_type==1 || stream_type==2 || stream_type==0x1b)
		{
			pids.VideoPid=elementary_PID;
			if (pids.PCRPid<=0)
				pids.PCRPid=pids.VideoPid;
			LogDebug("Sections::decodePMT() videopid:0x%x",pids.VideoPid);
			if(stream_type==0x1b)
			{
				pids.idMPEG4=(GUID)FOURCCMap(FCC('VSSH'));
				pids.MPEG4=true;
			}
			else
				pids.MPEG4=false;
		}
		if(stream_type==3 || stream_type==4)
		{
			if(pids.AudioPid1==0)
			{
				pids.CurrentAudioPid=elementary_PID;
				pids.AudioPid1=elementary_PID;
				LogDebug("Sections::decodePMT() AudioPid:0x%x",pids.AudioPid1);
			}
			else if (pids.AudioPid2==0)
			{
				pids.AudioPid2=elementary_PID;
				LogDebug("Sections::decodePMT() AudioPid2:0x%x",pids.AudioPid2);
			}
			else 
			{
				pids.AudioPid3=elementary_PID;
				LogDebug("Sections::decodePMT() AudioPid3:0x%x",pids.AudioPid3);
			}
		}
		if(stream_type==0x81)
		{
			if(pids.AC3==0)
			{
				pids.AC3=elementary_PID;
				LogDebug("Sections::decodePMT() AC3Pid:0x%x",pids.AC3);
			}
		}
		pointer += 5;
		len1 -= 5;
		len2 = ES_info_length;
		while (len2 > 0)
		{
			x = 0;
			int indicator=buf[pointer];
			x = buf[pointer + 1] + 2;
			if(indicator==0x6A)
				pids.AC3=elementary_PID;
			if (indicator==0x0a)
			{
				//decode language...
				string language=DVB_GetMPEGISO639Lang((byte*)&buf[pointer]);
				if (elementary_PID==pids.AudioPid1) pids.AudioLanguage1=language;
				if (elementary_PID==pids.AudioPid2) pids.AudioLanguage2=language;
				if (elementary_PID==pids.AudioPid3) pids.AudioLanguage3=language;
				if (elementary_PID==pids.AC3) pids.AC3Language=language;
			}
			len2 -= x;
			len1 -= x;
			pointer += x;

		}
	}
	return true;
}
//
string Sections::DVB_GetMPEGISO639Lang (byte* b)

{

	int		descriptor_tag;
	int     descriptor_length;		
	string  ISO_639_language_code="";
	int     audio_type;
	int     len;

	descriptor_tag= b[0];
	descriptor_length= b[1];
	if(descriptor_tag==0xa)
	{
		len = descriptor_length;
		char* bytes=new char[len+1];

		int pointer= 2;

		while ( len > 0) 
		{
			memcpy(bytes,&b[pointer],len);
			ISO_639_language_code=(string)bytes;
			if(descriptor_length>4)
				audio_type = bytes[3];
			pointer += 4;
			len -= 4;
		}
	}
	
	return ISO_639_language_code;
}
bool Sections::FindVideo()
{
	byte pData[200];
	m_pFileReader->SetFilePointer(0,FILE_BEGIN);
	bool finished=false;
	while(finished!=true)
	{
		ULONG countBytesRead;
		int hr=m_pFileReader->Read(pData,188,&countBytesRead);
		if(hr!=S_OK|| countBytesRead!=188)
			finished=true;
		if (m_pFileReader->GetFilePointer() > (2LL*1024LL*1024LL)) return false;
		
		TSHeader header;
		GetTSHeader(pData,&header);
		if (header.SyncByte==0x47 || header.Pid==pids.VideoPid) 
		{
			int offset=0;
			if(header.AdaptionControl==1 || header.AdaptionControl==3)
				offset+=pData[4];
			if (offset>= 0 && offset <188) 
			{
//				if(pData[offset]==0 && pData[offset+1]==0 && pData[offset+2]==1)
					return true;
			}
		}
	}
	return false;
}
