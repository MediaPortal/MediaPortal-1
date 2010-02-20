/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "..\..\shared\AdaptionField.h"
#include "recorder.h"
#define ERROR_FILE_TOO_LARGE 223
#define RECORD_BUFFER_SIZE 256000
#define IGNORE_AFTER_TUNE 75        // how many TS packets to ignore after tuning

extern void LogDebug(const char *fmt, ...) ;

//FILE* fpOut=NULL;
CRecorder::CRecorder(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsRecorder"), pUnk)
{
	strcpy(m_szFileName,"");
	m_timeShiftMode=ProgramStream;
	m_bRecording=false;
  m_hFile=INVALID_HANDLE_VALUE;
  m_pWriteBuffer = new byte[RECORD_BUFFER_SIZE];
  m_iWriteBufferPos=0;
  m_iServiceId=-1;
  m_iPmtPid=-1;
  m_pmtVersion=-1;
	m_multiPlexer.SetFileWriterCallBack(this);
  m_TsPacketCount=0;
	m_FakeTsPacketCount=0;
  m_pPmtParser=new CPmtParser();
  if (m_pPmtParser)
    m_pPmtParser->SetPmtCallBack2(this);
}
CRecorder::~CRecorder(void)
{
	CEnterCriticalSection enter(m_section);
  if (m_hFile!=INVALID_HANDLE_VALUE)
  {
	  CloseHandle(m_hFile);
	  m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file
  }
  delete [] m_pWriteBuffer;
	m_pPmtParser->Reset();
  delete m_pPmtParser;
}

void CRecorder::OnTsPacket(byte* tsPacket)
{	if (m_bRecording)
	{
	  m_tsHeader.Decode(tsPacket);
    if (m_tsHeader.SyncByte!=0x47) return;
	  if (m_tsHeader.TransportError) return;
	  CEnterCriticalSection enter(m_section);
    
    if (m_tsHeader.Pid==m_iPmtPid && m_pPmtParser && m_pmtVersion==-1 )
    {
      m_pPmtParser->OnTsPacket(tsPacket);
    }
    
    if (m_timeShiftMode==ProgramStream)
    {
		  m_multiPlexer.OnTsPacket(tsPacket);
    }
    else
    {
      WriteTs(tsPacket);
    }
	}
}


STDMETHODIMP CRecorder::SetMode(int mode) 
{
  m_timeShiftMode=(TimeShiftingMode)mode;
  if (mode==ProgramStream)
			LogDebug("Recorder:program stream mode");
  else
      LogDebug("Recorder:transport stream mode");
	return S_OK;
}

STDMETHODIMP CRecorder::GetMode(int *mode) 
{
  *mode=(int)m_timeShiftMode;
	return S_OK;
}

STDMETHODIMP CRecorder::SetPmtPid(int pmtPid, int serviceId )
{
	CEnterCriticalSection enter(m_section);
	m_iPmtPid=pmtPid;
  m_iServiceId=serviceId;
	LogDebug("Recorder:pmt pid:%x sid:%x",m_iPmtPid,serviceId);
	m_pPmtParser->SetFilter(pmtPid,serviceId);
	return S_OK;
}

STDMETHODIMP CRecorder::SetRecordingFileName(char* pszFileName)
{
	CEnterCriticalSection enter(m_section);
  m_vecPids.clear();
	m_multiPlexer.Reset();
	strcpy(m_szFileName,pszFileName);
	return S_OK;
}
STDMETHODIMP CRecorder::StartRecord()
{
	CEnterCriticalSection enter(m_section);
	if (strlen(m_szFileName)==0) return E_FAIL;
	::DeleteFile((LPCTSTR) m_szFileName);
  m_iPart=2;
	if (m_hFile!=INVALID_HANDLE_VALUE)
	{
    CloseHandle(m_hFile);
    m_hFile=INVALID_HANDLE_VALUE;
  }
	m_hFile = CreateFile(m_szFileName,      // The filename
						 (DWORD) GENERIC_WRITE,         // File access
						 (DWORD) FILE_SHARE_READ,       // Share access
						 NULL,                  // Security
						 (DWORD) OPEN_ALWAYS,           // Open flags
//						 (DWORD) FILE_FLAG_RANDOM_ACCESS,
//						 (DWORD) FILE_FLAG_WRITE_THROUGH,             // More flags
						 (DWORD) 0,             // More flags
						 NULL);                 // Template
	if (m_hFile == INVALID_HANDLE_VALUE)
	{
    LogDebug("Recorder:unable to create file:'%s' %d",m_szFileName, GetLastError());
		return E_FAIL;
	}
	LogDebug("Recorder:Start Recording:'%s'",m_szFileName);
  m_iWriteBufferPos=0;
	m_bRecording=true;
	//::DeleteFile("out.ts");
	//fpOut =fopen("out.ts","wb+");
	m_iPmtContinuityCounter=-1;
	m_iPatContinuityCounter=-1;
	return S_OK;
}
STDMETHODIMP CRecorder::StopRecord()
{
	CEnterCriticalSection enter(m_section);
  if (m_bRecording)
	  LogDebug("Recorder:Stop Recording:'%s'",m_szFileName);
	m_bRecording=false;
	m_multiPlexer.Reset();
	if (m_hFile!=INVALID_HANDLE_VALUE)
	{
    if (m_iWriteBufferPos>0)
    {
	    DWORD written = 0;
	    WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL);
      m_iWriteBufferPos=0;
    }
		CloseHandle(m_hFile);
		m_hFile=INVALID_HANDLE_VALUE;
	}
  m_iPmtPid=-1;
	return S_OK;
}



void CRecorder::SetPcrPid(int pcrPid)
{
	LogDebug("Recorder:pcr pid:%x",pcrPid);
  m_TsPacketCount=0;
	m_pcrPid = pcrPid;
  
	m_multiPlexer.SetPcrPid(pcrPid);
}

bool CRecorder::IsStreamWanted(int stream_type)
{
	return (stream_type==SERVICE_TYPE_VIDEO_MPEG1 || 
					stream_type==SERVICE_TYPE_VIDEO_MPEG2 || 
					stream_type==SERVICE_TYPE_VIDEO_MPEG4 || 
					stream_type==SERVICE_TYPE_VIDEO_H264 ||
					stream_type==SERVICE_TYPE_AUDIO_MPEG1 || 
					stream_type==SERVICE_TYPE_AUDIO_MPEG2 || 
					stream_type==SERVICE_TYPE_AUDIO_AC3 ||
					stream_type==SERVICE_TYPE_DVB_SUBTITLES2 ||
					stream_type==DESCRIPTOR_DVB_TELETEXT
					);
}

void CRecorder::AddStream(PidInfo2 pidInfo)
{
  ivecPidInfo2 it = m_vecPids.begin();
  while (it!=m_vecPids.end())
  {
		PidInfo2 info=*it;
		if (info.elementaryPid==pidInfo.elementaryPid)
			return;
    ++it;
  }

	if (IsStreamWanted(pidInfo.logicalStreamType))
	{
		LogDebug("Recorder: add stream pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);
		PidInfo2 pi;
		pi.seenStart=false;
		pi.fakePid=-1;
		pi.elementaryPid=pidInfo.elementaryPid;
		pi.streamType=pidInfo.streamType;
		pi.rawDescriptorSize=pidInfo.rawDescriptorSize;
		memset(pi.rawDescriptorData,0xFF,pi.rawDescriptorSize);
		memcpy(pi.rawDescriptorData,pidInfo.rawDescriptorData,pi.rawDescriptorSize);
		m_vecPids.push_back(pi);
		m_multiPlexer.AddPesStream(pi);
	}
	else
		LogDebug("Recorder: stream rejected - pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);
}

void CRecorder::Write(byte* buffer, int len)
{
	CEnterCriticalSection enter(m_section);
	if (!m_bRecording) return;
  if (buffer==NULL) return;
  if (len <=0) return;
  if (len + m_iWriteBufferPos >= RECORD_BUFFER_SIZE)
  {
	  try
	  {
      if (m_iWriteBufferPos > 0)
      {
		    if (m_hFile != INVALID_HANDLE_VALUE)
		    {
	        DWORD written = 0;
	        if (FALSE == WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL))
          {
            //On fat16/fat32 we can only create files of max. 2gb/4gb
            if (ERROR_FILE_TOO_LARGE == GetLastError())
            {
              LogDebug("Recorder:Maximum filesize reached for file:'%s' %d",m_szFileName);
                //close the file...
		          CloseHandle(m_hFile);
              m_hFile=INVALID_HANDLE_VALUE;

              //create a new file
              char ext[MAX_PATH];
              char fileName[MAX_PATH];
              char part[100];
						  int len=strlen(m_szFileName)-1;
						  int pos=len-1;
						  while (pos>0)
						  {
							  if (m_szFileName[pos]=='.') break;
							  pos--;
						  }
              strcpy(ext, &m_szFileName[pos]);
              strncpy(fileName, m_szFileName, pos);
              fileName[pos]=0;
              sprintf(part,"_p%d",m_iPart);
              char newFileName[MAX_PATH];
              sprintf(newFileName,"%s%s%s",fileName,part,ext);

						  LogDebug("Recorder:Create new  file:'%s' %d",newFileName);
	            m_hFile = CreateFile(newFileName,      // The filename
						             (DWORD) GENERIC_WRITE,         // File access
						             (DWORD) FILE_SHARE_READ,       // Share access
						             NULL,                  // Security
						             (DWORD) OPEN_ALWAYS,           // Open flags
						             (DWORD) 0,             // More flags
						             NULL);                 // Template
	            if (m_hFile == INVALID_HANDLE_VALUE)
	            {
                LogDebug("Recorder:unable to create file:'%s' %d",newFileName, GetLastError());
                m_iWriteBufferPos=0;
		            return ;
	            }
              m_iPart++;
              WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL);
            }//of if (ERROR_FILE_TOO_LARGE == GetLastError())
						else
						{				 
							LogDebug("Recorder:unable to write file:'%s' %d %d %x",m_szFileName, GetLastError(),m_iWriteBufferPos,m_hFile);
						}
          }//of if (FALSE == WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL))
		    }//if (m_hFile!=INVALID_HANDLE_VALUE)
      }//if (m_iWriteBufferPos>0)
      m_iWriteBufferPos=0;
	  }
	  catch(...)
	  {
		  LogDebug("Timeshifter:Write exception");
	  }
  }// of if (len + m_iWriteBufferPos >= RECORD_BUFFER_SIZE)

  if ( (m_iWriteBufferPos+len) < RECORD_BUFFER_SIZE && len > 0)
  {
    memcpy(&m_pWriteBuffer[m_iWriteBufferPos],buffer,len);
    m_iWriteBufferPos+=len;
  }
}

void CRecorder::WriteTs(byte* tsPacket)
{
	if (!m_bRecording) return;
  m_TsPacketCount++;
	m_FakeTsPacketCount++;
  if( m_TsPacketCount < IGNORE_AFTER_TUNE ) return;
	if (m_pcrPid<0 || m_vecPids.size()==0 || m_iPmtPid<0) return;
	
	if (m_FakeTsPacketCount>=100)
	{
		WriteFakePAT();
		WriteFakePMT();
		m_FakeTsPacketCount=0;
	}
  bool writePid = false;

  int PayLoadUnitStart=0;
  if (m_tsHeader.PayloadUnitStart) PayLoadUnitStart=1;

  ivecPidInfo2 it = m_vecPids.begin();
  while (it!=m_vecPids.end())
  {
		PidInfo2 info=*it;
		if (m_tsHeader.Pid==info.elementaryPid)
    {
      writePid = true;
    }
    ++it;
  }

  // Patch PCR & PTS/DTS to start from beginning & fill PCR "holes"
  if( writePid )
  {
    CAdaptionField field;
    field.Decode(m_tsHeader,tsPacket);

	  byte pkt[200];
	  memcpy(pkt,tsPacket,188);
	  int pid=m_tsHeader.Pid;
	  pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
	  pkt[2]=(pid&0xff);
	  if (m_tsHeader.Pid==m_pcrPid)  PatchPcr(pkt,m_tsHeader);
	  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
	  {
	    if (PayLoadUnitStart) PatchPtsDts(pkt,m_tsHeader,m_startPcr);
		  //info.ContintuityCounter=m_tsHeader.ContinuityCounter;
      //m_iPacketCounter++;
	  }
    Write(pkt,188);
  }
}

//*******************************************************************
//* WriteFakePAT()
//* Constructs a PAT table and writes it to the timeshifting file
//*******************************************************************
void CRecorder::WriteFakePAT()
{
	int pid=0;
	int tableId=0;
	int patVersion=0;
	int transportId=m_iServiceId;
	int pmtPid=m_iPmtPid;
	int sectionLenght=9+4;
	int current_next_indicator=1;
	int section_number = 0;
	int last_section_number = 0;

	int PayLoadUnitStart=1;
	int AdaptionControl=1;
	m_iPatContinuityCounter++;
	if (m_iPatContinuityCounter>0xf) m_iPatContinuityCounter=0;

	BYTE pat[200];
	memset(pat,0xFF,sizeof(pat));
	pat[0]=0x47;
	pat[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
	pat[2]=(pid&0xff);
	pat[3]=(AdaptionControl<<4) +m_iPatContinuityCounter;		//0x10
	pat[4]=0;																								//0

	pat[5]=tableId;//table id																//0
	pat[6]=0xb0+((sectionLenght>>8)&0xf);										//0xb0
	pat[7]=sectionLenght&0xff;
	pat[8]=(transportId>>8)&0xff;
	pat[9]=(transportId)&0xff;
	pat[10]=((patVersion&0x1f)<<1)+current_next_indicator;
	pat[11]=section_number;
	pat[12]=last_section_number;
	pat[13]=(m_iServiceId>>8)&0xff;
	pat[14]=(m_iServiceId)&0xff;
	pat[15]=((pmtPid>>8)&0xff)|0xe0;
	pat[16]=(pmtPid)&0xff;

	int len=17;
	DWORD crc= crc32((char*)&pat[5],len-5);
	pat[len]=(byte)((crc>>24)&0xff);
	pat[len+1]=(byte)((crc>>16)&0xff);
	pat[len+2]=(byte)((crc>>8)&0xff);
	pat[len+3]=(byte)((crc)&0xff);
	Write(pat,188);
}

//*******************************************************************
//* WriteFakePMT()
//* Constructs a PMT table and writes it to the timeshifting file
//*******************************************************************
void CRecorder::WriteFakePMT()
{
	int program_info_length=0;
	int sectionLenght=9+2*5+5;

	int current_next_indicator=1;
	int section_number = 0;
	int last_section_number = 0;
	int transportId=m_iServiceId;

	int tableId=2;
	int pid=m_iPmtPid;
	int PayLoadUnitStart=1;
	int AdaptionControl=1;

	m_iPmtContinuityCounter++;
	if (m_iPmtContinuityCounter>0xf) m_iPmtContinuityCounter=0;

	BYTE pmt[256]; 

	memset(pmt,0xff,sizeof(pmt));
	pmt[0]=0x47;
	pmt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
	pmt[2]=(pid&0xff);
	pmt[3]=(AdaptionControl<<4) +m_iPmtContinuityCounter;
	pmt[4]=0;
	byte* pmtPtr=&pmt[4];
	pmt[5]=tableId;//table id
	pmt[6]=0;
	pmt[7]=0;
	pmt[8]=(m_iServiceId>>8)&0xff;
	pmt[9]=(m_iServiceId)&0xff;
	pmt[10]=((m_pmtVersion&0x1f)<<1)+current_next_indicator;
	pmt[11]=section_number;
	pmt[12]=last_section_number;
	pmt[13]=(m_pcrPid>>8)&0xff;
	pmt[14]=(m_pcrPid)&0xff;
	pmt[15]=(program_info_length>>8)&0xff;
	pmt[16]=(program_info_length)&0xff;

	int pmtLength=9+4;
	int offset=17;
	ivecPidInfo2 it=m_vecPids.begin();
	while (it!=m_vecPids.end())
	{
		PidInfo2 info=*it;
		pmt[offset++]=info.streamType;
		pmt[offset++]=0xe0+((info.elementaryPid>>8)&0x1F); // reserved; elementary_pid (high)
		pmt[offset++]=(info.elementaryPid)&0xff; // elementary_pid (low)
		pmt[offset++]=0; // es_length (high)
		pmt[offset++]=0; // es_length (low)
		pmtLength+=5;
		if (info.rawDescriptorData!=NULL)
		{
			pmt[offset-1]=info.rawDescriptorSize;
			memcpy(&pmt[offset],info.rawDescriptorData,info.rawDescriptorSize);
			offset += info.rawDescriptorSize;
			pmtLength += info.rawDescriptorSize;
		}
		++it;
	}
	unsigned section_length = (pmtLength);
	pmt[6]=0xb0+((section_length>>8)&0xf);
	pmt[7]=section_length&0xff;

	DWORD crc= crc32((char*)&pmt[5],offset-5);
	pmt[offset++]=(byte)((crc>>24)&0xff);
	pmt[offset++]=(byte)((crc>>16)&0xff);
	pmt[offset++]=(byte)((crc>>8)&0xff);
	pmt[offset++]=(byte)((crc)&0xff);

	if(pmtLength > 188) LogDebug("ERROR: Pmt length : %i ( >188 )!!!!",pmtLength);

	Write(pmt,188);
}

void CRecorder::OnPmtReceived2(int pid,int serviceId,int pcrPid,vector<PidInfo2> pidInfos)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("Recorder: PMT version changed from %d to %d - ServiceId %x", m_pmtVersion, m_pPmtParser->GetPmtVersion(), m_iServiceId );

	m_pmtVersion=m_pPmtParser->GetPmtVersion();
  LogDebug("Recorder: clear PIDs vector" );
  m_vecPids.clear();
  m_pmtVersion=m_pPmtParser->GetPmtVersion();

  SetPcrPid(pcrPid);
	ivecPidInfo2 it=pidInfos.begin();
	while (it!=pidInfos.end())
	{
		PidInfo2 info=*it;
		AddStream(info);
		++it;
	}
}

//*******************************************************************
//* PatchPcr()
//* Patches the PCR so the start of the .ts file always starts
//* with a PCR of 0. 
//*******************************************************************
void CRecorder::PatchPcr(byte* tsPacket,CTsHeader& header)
{
  if (header.PayLoadOnly()) return;
  m_adaptionField.Decode(header,tsPacket);
  if (m_adaptionField.PcrFlag==false) return;
  CPcr pcrNew=m_adaptionField.Pcr;
	bool logNextPcr = false;  
  if (m_bStartPcrFound)
	{
		if (m_bDetermineNewStartPcr )
		{
			m_bDetermineNewStartPcr=false;
      m_startPcr=pcrNew; 
			m_highestPcr.Reset();
		}
	}
  
	if (m_bStartPcrFound==false)
	{
		m_bDetermineNewStartPcr=false;
		m_bStartPcrFound=true;
		m_startPcr  = pcrNew;
    m_highestPcr= pcrNew;
    LogDebug("Pcr new start pcr :%s - pid:%x", m_startPcr.ToString() , header.Pid);
	} 

  CPcr pcrHi=pcrNew;
  CPcr diff;

	if (pcrNew > m_highestPcr)
	{
		m_highestPcr = pcrNew;
	}

  if (pcrNew > m_prevPcr)
  {
    diff = pcrNew - m_prevPcr;
  }
  else
  {
    diff = m_prevPcr - pcrNew;
  }

  if(diff.ToClock() > 10L && m_prevPcr.ToClock() > 0)
  {
    bool logNextPcr = true;
    if( diff.ToClock() > 95443L ) // Max PCR value 95443.71768
    {
      m_bPCRRollover = true;
      m_pcrDuration = m_prevPcr;
      m_pcrDuration -= m_startPcr;
      m_pcrHole.Reset();
      m_backwardsPcrHole.Reset();
      m_startPcr.Reset();
      m_highestPcr.Reset();

      LogDebug( "PCR rollover detected!" );
    }
    else
    {      
      CPcr step;
      step.FromClock(0.02); // an estimated PCR step
      
      if (pcrNew > m_prevPcr)
      {
        m_pcrHole += diff;
        m_pcrHole -= step; 
        LogDebug( "Jump forward in PCR detected!" );
      }
      else
      {
				m_backwardsPcrHole += diff;
				m_backwardsPcrHole += step;
        LogDebug( "Jump backward in PCR detected!" );
      }
    }
  }

  /*pcrHi -= m_startPcr;
  pcrHi += m_backwardsPcrHole;
  pcrHi -= m_pcrHole;*/

  double result = pcrHi.ToClock() - m_startPcr.ToClock() + m_backwardsPcrHole.ToClock() - m_pcrHole.ToClock();
  pcrHi.FromClock(result);

  if( m_bPCRRollover )
  {
    pcrHi += m_pcrDuration;
  }

  if( logNextPcr )
  {
    LogDebug("PCR: %s new: %s prev: %s start: %s diff: %s hole: %s holeB: %s  - pid:%x", pcrHi.ToString(), pcrNew.ToString(), m_prevPcr.ToString(), m_startPcr.ToString(), diff.ToString(), m_pcrHole.ToString(), m_backwardsPcrHole.ToString(), header.Pid );
  }
  tsPacket[6] = (byte)(((pcrHi.PcrReferenceBase>>25)&0xff));
  tsPacket[7] = (byte)(((pcrHi.PcrReferenceBase>>17)&0xff));
  tsPacket[8] = (byte)(((pcrHi.PcrReferenceBase>>9)&0xff));
  tsPacket[9] = (byte)(((pcrHi.PcrReferenceBase>>1)&0xff));
  tsPacket[10]=	(byte)(((pcrHi.PcrReferenceBase&0x1)<<7) + 0x7e+ ((pcrHi.PcrReferenceExtension>>8)&0x1));
  tsPacket[11]= (byte)(pcrHi.PcrReferenceExtension&0xff);

  m_prevPcr = pcrNew;
}

//*******************************************************************
//* PatchPtsDts()
//* Patches the PTS/DTS timestamps in a audio/video transport packet
//* so the start of the timeshifting file always starts
//* with a PTS/DTS of 0. 
//*******************************************************************
void CRecorder::PatchPtsDts(byte* tsPacket,CTsHeader& header,CPcr& startPcr)
{
  if (false==header.PayloadUnitStart) return;

  int start=header.PayLoadStart;
  if (tsPacket[start] !=0 || tsPacket[start+1] !=0  || tsPacket[start+2] !=1) return; 

  byte* pesHeader=&tsPacket[start];
	CPcr pts;
	CPcr dts;
  if (!CPcr::DecodeFromPesHeader(pesHeader,pts,dts))
  {
		return ;
	}
	if (pts.IsValid)
	{
    CPcr ptsorg=pts;
		pts -= startPcr;
    pts -= m_pcrHole;

    if( m_bPCRRollover )
    {
      pts += m_pcrDuration;
    }
		// 9       10        11        12      13
		//76543210 76543210 76543210 76543210 76543210
		//0011pppM pppppppp pppppppM pppppppp pppppppM 
		//LogDebug("pts: org:%s new:%s start:%s - pid:%x", ptsorg.ToString(),pts.ToString(),startPcr.ToString(), header.Pid); 
		byte marker=0x21;
		if (dts.PcrReferenceBase!=0) marker=0x31;
		pesHeader[13]=(byte)((( (pts.PcrReferenceBase&0x7f)<<1)+1));   pts.PcrReferenceBase>>=7;
		pesHeader[12]=(byte)(   (pts.PcrReferenceBase&0xff));				   pts.PcrReferenceBase>>=8;
		pesHeader[11]=(byte)((( (pts.PcrReferenceBase&0x7f)<<1)+1));   pts.PcrReferenceBase>>=7;
		pesHeader[10]=(byte)(   (pts.PcrReferenceBase&0xff));					 pts.PcrReferenceBase>>=8;
		pesHeader[9] =(byte)( (((pts.PcrReferenceBase&7)<<1)+marker)); 
    
		if (dts.IsValid)
		{
			CPcr dtsorg=dts;
			dts -= startPcr;
      dts -= m_pcrHole;

      if( m_bPCRRollover )
      {
        dts += m_pcrDuration;
      }

			// 14       15        16        17      18
			//76543210 76543210 76543210 76543210 76543210
			//0001pppM pppppppp pppppppM pppppppp pppppppM 
	 		//LogDebug("dts: org:%s new:%s start:%s - pid:%x", dtsorg.ToString(),dts.ToString(),startPcr.ToString(), header.Pid); 
			pesHeader[18]=(byte)( (((dts.PcrReferenceBase&0x7f)<<1)+1));  dts.PcrReferenceBase>>=7;
			pesHeader[17]=(byte)(   (dts.PcrReferenceBase&0xff));				  dts.PcrReferenceBase>>=8;
			pesHeader[16]=(byte)( (((dts.PcrReferenceBase&0x7f)<<1)+1));  dts.PcrReferenceBase>>=7;
			pesHeader[15]=(byte)(   (dts.PcrReferenceBase&0xff));					dts.PcrReferenceBase>>=8;
			pesHeader[14]=(byte)( (((dts.PcrReferenceBase&7)<<1)+0x11)); 
		}
	}
}


