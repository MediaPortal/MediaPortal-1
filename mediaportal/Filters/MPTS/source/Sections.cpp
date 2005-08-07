/*
	MediaPortal TS-SourceFilter by Agree

	
*/


#include <streams.h>
#include "Sections.h"
void LogDebug(const char *fmt, ...) 
{
#ifdef DEBUG
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("MPTS.log","a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetSystemTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		fclose(fp);
	}
#endif
};

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

Sections::Sections(FileReader *pFileReader)
{
	m_pFileReader=pFileReader;
}

Sections::~Sections()
{
	//delete m_pFileReader;
}

HRESULT Sections::ParseFromFile()
{

	if (m_pFileReader->m_hInfoFile!=INVALID_HANDLE_VALUE)
	{

		LogDebug("Get pids from info file");
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
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.AudioPid, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.AudioPid2, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.VideoPid, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&ttxPid, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.PMTPid, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&subtitlePid, sizeof(int), &dwReadBytes, NULL);
		::ReadFile(m_pFileReader->m_hInfoFile, (PVOID)&pids.PCRPid, sizeof(int), &dwReadBytes, NULL);
		if (pids.PCRPid==0) pids.PCRPid=pids.VideoPid;

		pids.StartPTS=(__int64)ptsStart;
		pids.EndPTS=(__int64)ptsNow;
		Sections::PTSTime time;
		pids.Duration=ptsNow-ptsStart;
		pids.DurTime=pids.Duration;
		PTSToPTSTime(pids.Duration,&time);
		pids.Duration=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);

		pids.DurTime=0;
		pids.Duration=600000000;
		pids.EndPTS=0;
		pids.StartPTS=0;

		__int64 filePointer=0;
		m_pFileReader->SetFilePointer(filePointer,FILE_BEGIN);
		return S_OK;
	}

	
	__int64 filePointer=0;
	HRESULT hr = S_OK;
	__int64 len;
	m_pFileReader->GetFileSize(&len);
	filePointer=m_pFileReader->GetFilePointer(); // to restore after reading
	decodePMT();
	CheckStream();
	if(pids.Duration<600000000)
		pids.Duration=600000000;
	m_pFileReader->SetFilePointer(filePointer,FILE_BEGIN);
	return hr;
}
HRESULT Sections::CheckStream(void)
{
	LogDebug("Sections::CheckStream()");
	__int64 fileSize;
	m_pFileReader->GetFileSize(&fileSize);
	__int64 fileEndOffset=fileSize-((fileSize*20)/100);
	fileEndOffset=(fileEndOffset/188)*188;
	if(m_pFileReader->OpenFile()==S_FALSE)
		return S_FALSE;
	HRESULT hr;
	BYTE pData[188];
	bool finished=false;
	ULONG countBytesRead;
	m_pFileReader->SetFilePointer(0,FILE_BEGIN);
	TSHeader header;
	PESHeader pes;
	unsigned long offset=0;
	ULONGLONG filePosCounter=0;
	ULONGLONG pts=0;
	int pidToCheck=(pids.AudioPid!=0?pids.AudioPid:pids.AC3);
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
		m_pFileReader->SetFilePointer(countBytesRead,FILE_CURRENT);
		
		GetTSHeader(pData,&header);
		
		if(hr==S_OK && header.SyncByte==0x47)
		{
			for(offset=4;offset<181;offset++)
			{
				if(pData[offset]==0 && pData[offset+1]==0 && pData[offset+2]==1)// pes
				{
					BYTE streamID=(pData[offset+3]>>5) & 0x07;
					WORD pesLen=(pData[offset+4]<<8)+pData[offset+5];
					GetPESHeader(&pData[offset+6],&pes);
					BYTE pesHeaderLen=pData[offset+8];
					if(pes.Reserved==0x02 && pidToCheck==header.Pid) // valid header
					{
						if(pes.PTSFlags==0x02)
						{
							// audio pes found
							GetPTS(&pData[offset+9],&pts);
							if(pids.StartPTS==0)
							{
								LogDebug("Sections::CheckStream() got PTS");
								pids.StartPTS=(__int64)pts; // first pts
								m_pFileReader->SetFilePointer(fileEndOffset,FILE_BEGIN);// sets to file-end - 20%
							}
							
						}
					}
					break;
				}
			}
		}else 
			finished=true;

	}
	// set end pts
	pids.EndPTS=(__int64)pts;
	pids.Duration=(pids.EndPTS-pids.StartPTS);
	pids.DurTime=pids.Duration;
	// calc duration in 100 ns
	PTSTime time;
	PTSToPTSTime(pids.Duration,&time);
	pids.Duration=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);
	//
	m_pFileReader->SetFilePointer(0,FILE_BEGIN);// sets to file-end - 20%
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
	*pts= 0xFFFFFFFFL & ( (6&data[0])<<29 | (255&data[1])<<22 | (254&data[2])<<14 | (255&data[3])<<7 | (((254&data[4])>>1)& 0x7F));
}
void Sections::PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime)
{
	PTSTime time;
	ULONG  _90khz = (ULONG)(pts/90);
	time.h=(_90khz/(1000*60*60));
	time.m=(_90khz/(1000*60))-(time.h*60);
	time.s=(_90khz/1000)-(time.h*3600)-(time.m*60);
	time.u=_90khz-(time.h*1000*60*60)-(time.m*1000*60)-(time.s*1000);
	*ptsTime=time;
}
HRESULT Sections::CurrentPTS(BYTE *pData,ULONGLONG *ptsValue,int *streamType)
{
	HRESULT hr=S_FALSE;
	*ptsValue=-1;
	TSHeader header;
	PESHeader pes;
	GetTSHeader(pData,&header);
	int offset=4;
	bool found=false;

	if(header.Pid!=pids.AudioPid)
		return S_FALSE;

	if(header.AdaptionControl==1 || header.AdaptionControl==3)
		offset+=pData[4];
	
	if(header.SyncByte==0x47 && pData[offset]==0 && pData[offset+1]==0 && pData[offset+2]==1)
	{
		*streamType=(int)((pData[offset+3]>>5) & 0x07);
		WORD pesLen=(pData[offset+4]<<8)+pData[offset+5];
		GetPESHeader(&pData[offset+6],&pes);
		BYTE pesHeaderLen=pData[offset+8];
		if(pes.Reserved==0x02 && pids.AudioPid==header.Pid) // valid header
		{
			if(pes.PTSFlags==0x02)
			{
				// audio pes found
				GetPTS(&pData[offset+9],ptsValue);
				hr=S_OK;
			}
		}	
	}
	return hr;
}
void Sections::decodePMT()
{
	if(m_pFileReader->OpenFile()==S_FALSE)
		return;

	LogDebug("Sections::decodePMT()");
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
			if(header.SyncByte==0x47 && header.Pid==0)// pat
			{
				if(header.PayloadUnitStart==true)
					offset=4+pData[4]+1;
				else
					offset=4;
				if(pData[offset]==0x00 && pData[offset+1]==0x00 && pData[offset+2]==0x01) continue;//PES
				int table_id = pData[offset+0];
				int section_syntax_indicator = (pData[offset+1]>>7) & 1;
				int section_length = ((pData[offset+1]& 0xF)<<8) + pData[offset+2];
				int transport_stream_id = (pData[offset+3]<<8)+pData[offset+4];
				int version_number = ((pData[offset+5]>>1)&0x1F);
				int current_next_indicator = pData[offset+5] & 1;
				int section_number = pData[offset+6];
				int last_section_number = pData[offset+7];

				LogDebug("Sections::decodePMT() pid:0x%x tsid:0x%x %d/%d len:%d", header.Pid,transport_stream_id,section_number,last_section_number, section_length);
				//if (section_number!=0||last_section_number!=0) continue;
				
	
				LogDebug("Sections::decodePMT() found PAT");
				int loop =(section_length - 9) / 4;
				int offset1=0;
				pmtcount=loop;
				for(int i=0;i<loop;i++)
				{
					offset1=offset+(8 +(i * 4));
					pmt[i]=((pData[offset1+2] & 0x1F)<<8)+pData[offset1+3];
					LogDebug("Sections::decodePMT() PMT:%i pid:%x",i,pmt[i]);
				}
				finished=true;
			}
		}
	}
	if(pmtcount==0)
	{
		LogDebug("Sections::decodePMT() no PAT found");
		pids.Clear();
		return;
	}
	
	LogDebug("Sections::decodePMT() Get PMT");
	// get pcr, audio, video, etc.
	m_pFileReader->SetFilePointer(0,FILE_BEGIN);
	finished=false;
	while(finished!=true)
	{
		hr=m_pFileReader->Read(pData,188,&countBytesRead);
		if(hr!=S_OK|| countBytesRead!=188)
			finished=true;
		m_pFileReader->SetFilePointer(countBytesRead,FILE_CURRENT);
		if(hr==S_OK)
		{
			GetTSHeader(pData,&header);
			for(int i=0;i<pmtcount;i++)
			{
				actPid=pmt[i];
				if(header.Pid==actPid && header.Pid>0x11)
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
						pmtSectionLen=((pData[offset+1]& 0xF)<<8) + pData[offset+2];
						finished=true;
						ready=true;
						LogDebug("Sections::decodePMT() found PMT:%x pcr:%x",pids.PCRPid,pids.PMTPid);
						break;
					}
				}
			}
		}
	}
	// decode the pmt
	if(pids.PMTPid==0)
	{
		LogDebug("Sections::decodePMT() no PMT found");
		pids.Clear(); // invaild file
		return;
	}
	
	LogDebug("Sections::decodePMT() get PMT");
	int contCounter=header.ContinuityCounter;
	if(pmtSectionLen<183)
		pmtSectionLen=183;

	BYTE *buf=new BYTE[pmtSectionLen];
	memset(buf,0,pmtSectionLen);
	memcpy(buf,pData+offset,188-offset);
	pmtSectionLen-=188-offset;
	DWORD len=0;
	while(pmtSectionLen>0)
	{
		hr=m_pFileReader->Read(pData,188,&countBytesRead);
		if(hr!=S_OK|| countBytesRead!=188)
			finished=true;
		m_pFileReader->SetFilePointer(countBytesRead,FILE_CURRENT);
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
				memcpy(buf,pData+offset,len);
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
	pids.AudioPid=pids.AudioPid2=pids.VideoPid=pids.AC3=0;

	while (len1 > 4)
	{
		stream_type = buf[pointer];
		elementary_PID = ((buf[pointer+1]&0x1F)<<8)+buf[pointer+2];
		ES_info_length = ((buf[pointer+3] & 0xF)<<8)+buf[pointer+4];
		if(stream_type==1 || stream_type==2)
		{
			pids.VideoPid=elementary_PID;
			if (pids.PCRPid<=0)
				pids.PCRPid=pids.VideoPid;
			LogDebug("Sections::decodePMT() videopid:0x%x",pids.VideoPid);
		}
		if(stream_type==3 || stream_type==4)
		{
			if(pids.AudioPid==0)
			{
				pids.AudioPid=elementary_PID;
				LogDebug("Sections::decodePMT() AudioPid:0x%x",pids.AudioPid);
			}
			else
			{
				pids.AudioPid2=elementary_PID;
				LogDebug("Sections::decodePMT() AudioPid2:0x%x",pids.AudioPid2);
			}
		}
		if(stream_type==0x81 || stream_type==6)
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
			len2 -= x;
			len1 -= x;
			pointer += x;

		}
	}
	delete buf;
}