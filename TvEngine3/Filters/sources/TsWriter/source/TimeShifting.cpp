/* 
 *	Copyright (C) 2006 Team MediaPortal
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

#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "timeshifting.h"
#include "tsheader.h"

#define PID_PAT   0
#define TABLE_ID_PAT 0
#define TABLE_ID_SDT 0x42


int FAKE_NETWORK_ID   = 0x456;
int FAKE_TRANSPORT_ID = 0x4;
int FAKE_SERVICE_ID   = 0x89;
int FAKE_PMT_PID      = 0x20;
int FAKE_PCR_PID      = 0x21;
int FAKE_VIDEO_PID    = 0x30;
int FAKE_AUDIO_PID    = 0x31;

extern void LogDebug(const char *fmt, ...) ;

CTimeShifting::CTimeShifting(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsTimeshifting"), pUnk)
{

	m_params.chunkSize=1024*1024*256;
	m_params.maxFiles=20;
	m_params.maxSize=1024*1024*256;
	m_params.minFiles=6;
  
  m_pmtPid=-1;
  m_pcrPid=-1;
  m_audioPid=-1;
  m_videoPid=-1;
  m_timeShiftMode=ProgramStream;
	m_bTimeShifting=false;
	m_pTimeShiftFile=NULL;
	m_multiPlexer.SetFileWriterCallBack(this);
}
CTimeShifting::~CTimeShifting(void)
{
}

void CTimeShifting::OnTsPacket(byte* tsPacket)
{
	CEnterCriticalSection enter(m_section);
	if (m_bTimeShifting)
	{
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


STDMETHODIMP CTimeShifting::SetPcrPid(int pcrPid)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:pcr pid:%x",pcrPid); 
		m_multiPlexer.ClearStreams();
		m_multiPlexer.SetPcrPid(pcrPid);
    m_pcrPid=pcrPid;
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetPcrPid exception");
	}
	return S_OK;
}

STDMETHODIMP CTimeShifting::SetPmtPid(int pmtPid)
{
  CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:pmt pid:%x",pmtPid);
    m_pmtPid=pmtPid;
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetPmtPid exception");
	}
	return S_OK;
}

STDMETHODIMP CTimeShifting::SetMode(int mode) 
{
  m_timeShiftMode=(TimeShiftingMode)mode;
  if (mode==ProgramStream)
			LogDebug("Timeshifter:program stream mode");
  else
      LogDebug("Timeshifter:transport stream mode");
	return S_OK;
}

STDMETHODIMP CTimeShifting::GetMode(int *mode) 
{
  *mode=(int)m_timeShiftMode;
	return S_OK;
}

STDMETHODIMP CTimeShifting::AddPesStream(int pid, bool isAudio, bool isVideo)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		if (isAudio)
    {
			LogDebug("Timeshifter:add audio pes stream pid:%x",pid);
      m_audioPid=pid;
    }
		else if (isVideo)
    {
			LogDebug("Timeshifter:add video pes stream pid:%x",pid);
      m_videoPid=pid;
    }
		else 
    {
			LogDebug("Timeshifter:add private pes stream pid:%x",pid); 
    }
		m_multiPlexer.AddPesStream(pid,isAudio,isVideo);
	}
	catch(...)
	{
		LogDebug("Timeshifter:AddPesStream exception");
	}
	return S_OK;
}
STDMETHODIMP CTimeShifting::RemovePesStream(int pid)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:remove pes stream pid:%x",pid);
		m_multiPlexer.RemovePesStream(pid);
	}
	catch(...)
	{
		LogDebug("Timeshifter:RemovePesStream exception");
	}
	return S_OK;
}

STDMETHODIMP CTimeShifting::SetTimeShiftingFileName(char* pszFileName)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		m_multiPlexer.Reset();
		strcpy(m_szFileName,pszFileName);
		strcat(m_szFileName,".tsbuffer");
		LogDebug("Timeshifter:set filename:%s",m_szFileName);
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetTimeShiftingFileName exception");
	}
	return S_OK;
}

STDMETHODIMP CTimeShifting::Start()
{
	CEnterCriticalSection enter(m_section);
	try
	{

    if (m_pcrPid == m_videoPid)
    {
      FAKE_PCR_PID      = 0x30;
      FAKE_VIDEO_PID    = 0x30;
    }
    else
    {
      FAKE_PCR_PID      = 0x21;
      FAKE_VIDEO_PID    = 0x30;
    }
		if (strlen(m_szFileName)==0) return E_FAIL;
		::DeleteFile((LPCTSTR) m_szFileName);
		WCHAR wstrFileName[2048];
		MultiByteToWideChar(CP_ACP,0,m_szFileName,-1,wstrFileName,1+strlen(m_szFileName));

		m_pTimeShiftFile = new MultiFileWriter(&m_params);
		if (FAILED(m_pTimeShiftFile->OpenFile(wstrFileName))) 
		{
			LogDebug("Timeshifter:failed to open filename:%s %d",m_szFileName,GetLastError());
			m_pTimeShiftFile->CloseFile();
			delete m_pTimeShiftFile;
			m_pTimeShiftFile=NULL;
			return E_FAIL;
		}

		LogDebug("Timeshifter:Start timeshifting:'%s'",m_szFileName);
    WriteFakePAT();
    WriteFakePMT();
		m_bTimeShifting=true;
	}
	catch(...)
	{
		LogDebug("Timeshifter:Start timeshifting exception");
	}
	return S_OK;
}
STDMETHODIMP CTimeShifting::Reset()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:Reset");
    m_pmtPid=-1;
    m_pcrPid=-1;
    m_audioPid=-1;
    m_videoPid=-1;
		m_multiPlexer.Reset();
	}
	catch(...)
	{
		LogDebug("Timeshifter:Reset timeshifting exception");
	}
	return S_OK;
}

STDMETHODIMP CTimeShifting::Stop()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:Stop timeshifting:'%s'",m_szFileName);
		m_bTimeShifting=false;
		m_multiPlexer.Reset();
		if (m_pTimeShiftFile!=NULL)
		{
			m_pTimeShiftFile->CloseFile();
			delete m_pTimeShiftFile;
			m_pTimeShiftFile=NULL;
		}
	}
	catch(...)
	{
		LogDebug("Timeshifter:Stop timeshifting exception");
	}
	return S_OK;
}


void CTimeShifting::Write(byte* buffer, int len)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		if (!m_bTimeShifting) return;
    if (buffer==NULL) return;
    if (len <=0) return;
		if (m_pTimeShiftFile!=NULL)
		{
			m_pTimeShiftFile->Write(buffer,len);
		}
	}
	catch(...)
	{
		LogDebug("Timeshifter:Write exception");
	}
}

STDMETHODIMP CTimeShifting::GetBufferSize(long *size)
{
	CheckPointer(size, E_POINTER);
	*size = 0;
	return S_OK;
}

STDMETHODIMP CTimeShifting::GetNumbFilesAdded(WORD *numbAdd)
{
    CheckPointer(numbAdd, E_POINTER);
	*numbAdd = (WORD)m_pTimeShiftFile->getNumbFilesAdded();
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetNumbFilesRemoved(WORD *numbRem)
{
    CheckPointer(numbRem, E_POINTER);
	*numbRem = (WORD)m_pTimeShiftFile->getNumbFilesRemoved();
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetCurrentFileId(WORD *fileID)
{
    CheckPointer(fileID, E_POINTER);
	*fileID = (WORD)m_pTimeShiftFile->getCurrentFileId();
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetMinTSFiles(WORD *minFiles)
{
    CheckPointer(minFiles, E_POINTER);
	*minFiles = (WORD) m_params.minFiles;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::SetMinTSFiles(WORD minFiles)
{
	m_params.minFiles=(long)minFiles;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetMaxTSFiles(WORD *maxFiles)
{
    CheckPointer(maxFiles, E_POINTER);
	*maxFiles = (WORD) m_params.maxFiles;
	return NOERROR;
}

STDMETHODIMP CTimeShifting::SetMaxTSFiles(WORD maxFiles)
{
	m_params.maxFiles=(long)maxFiles;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetMaxTSFileSize(__int64 *maxSize)
{
    CheckPointer(maxSize, E_POINTER);
	*maxSize = m_params.maxSize;
	return NOERROR;
}

STDMETHODIMP CTimeShifting::SetMaxTSFileSize(__int64 maxSize)
{
	m_params.maxSize=maxSize;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetChunkReserve(__int64 *chunkSize)
{
  CheckPointer(chunkSize, E_POINTER);
	*chunkSize = m_params.chunkSize;
	return NOERROR;
}

STDMETHODIMP CTimeShifting::SetChunkReserve(__int64 chunkSize)
{
  m_params.chunkSize=chunkSize;
    return NOERROR;
}

STDMETHODIMP CTimeShifting::GetFileBufferSize(__int64 *lpllsize)
{
    CheckPointer(lpllsize, E_POINTER);
	m_pTimeShiftFile->GetFileSize(lpllsize);
	return NOERROR;
}


void CTimeShifting::WriteTs(byte* tsPacket)
{
  if (m_pcrPid<0 || m_audioPid<0 || m_pmtPid<0) return;

	CTsHeader header(tsPacket);
  if (header.Pid==0)
  {
    //PAT
    if (header.PayloadUnitStart)
    {
      WriteFakePAT();
    }
    return;
  }

  if (header.Pid==m_pmtPid)
  {
    //PMT
    if (header.PayloadUnitStart)
    {
      WriteFakePMT();
    }
    return;
  }

  int PayLoadUnitStart=0;
  if (header.PayloadUnitStart) PayLoadUnitStart=1;

  if (header.Pid==m_pcrPid)
  {
      byte pkt[200];
      memcpy(pkt,tsPacket,188);
      int pid=FAKE_PCR_PID;
      pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
      pkt[2]=(pid&0xff);
      Write(pkt,188);
      return;
  }

  if (header.Pid==m_videoPid)
  {
    byte pkt[200];
    memcpy(pkt,tsPacket,188);
    int pid=FAKE_VIDEO_PID;
    pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
    pkt[2]=(pid&0xff);
    Write(pkt,188);
    return;
  }

  if (header.Pid==m_audioPid)
  {
    byte pkt[200];
    memcpy(pkt,tsPacket,188);
    int pid=FAKE_AUDIO_PID;
    pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
    pkt[2]=(pid&0xff);
    Write(pkt,188);
    return;
  }
}

void CTimeShifting::WriteFakePAT()
{
  int tableId=TABLE_ID_PAT;
  int transportId=FAKE_TRANSPORT_ID;
  int pmtPid=FAKE_PMT_PID;
  int sectionLenght=9+4;
  int version_number=0;
  int current_next_indicator=1;
  int section_number = 0;
  int last_section_number = 0;

  int pid=PID_PAT;
  int PayLoadUnitStart=1;
  int AdaptionControl=1;
  int ContinuityCounter=0;

  BYTE pat[200];
  memset(pat,0,sizeof(pat));
  pat[0]=0x47;
  pat[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
  pat[2]=(pid&0xff);
  pat[3]=(AdaptionControl<<4) +ContinuityCounter;
  pat[4]=0;

  pat[5]=tableId;//table id
  pat[6]=0x80+((sectionLenght>>8)&0xf);
  pat[7]=sectionLenght&0xff;
  pat[8]=(transportId>>8)&0xff;
  pat[9]=(transportId)&0xff;
  pat[10]=((version_number&0x1f)<<1)+current_next_indicator;
  pat[11]=section_number;
  pat[12]=last_section_number;
  pat[13]=(FAKE_NETWORK_ID>>8)&0xff;
  pat[14]=(FAKE_NETWORK_ID)&0xff;
  pat[15]=(pmtPid>>8)&0xff;
  pat[16]=(pmtPid)&0xff;
  Write(pat,188);
}
void CTimeShifting::WriteFakePMT()
{
  int program_info_length=0;
  int sectionLenght=9+2*5+3;
  int version_number=0;
  int current_next_indicator=1;
  int section_number = 0;
  int last_section_number = 0;
  int transportId=FAKE_TRANSPORT_ID;

  int tableId=2;
  int pid=FAKE_PMT_PID;
  int PayLoadUnitStart=1;
  int AdaptionControl=1;
  int ContinuityCounter=0;

  BYTE pmt[200];
  memset(pmt,0,sizeof(pmt));
  pmt[0]=0x47;
  pmt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
  pmt[2]=(pid&0xff);
  pmt[3]=(AdaptionControl<<4) +ContinuityCounter;
  pmt[4]=0;
  pmt[5]=tableId;//table id
  pmt[6]=0x80+((sectionLenght>>8)&0xf);
  pmt[7]=sectionLenght&0xff;
  pmt[8]=(transportId>>8)&0xff;
  pmt[9]=(transportId)&0xff;
  pmt[10]=((version_number&0x1f)<<1)+current_next_indicator;
  pmt[11]=section_number;
  pmt[12]=last_section_number;
  pmt[13]=(FAKE_PCR_PID>>8)&0xff;
  pmt[14]=(FAKE_PCR_PID)&0xff;
  pmt[15]=(program_info_length>>8)&0xff;
  pmt[16]=(program_info_length)&0xff;
  
  pmt[17]=2;//video stream_type
  pmt[18]=(FAKE_VIDEO_PID>>8)&0x1F;
  pmt[19]=(FAKE_VIDEO_PID)&0xffF;
  pmt[20]=0;
  pmt[21]=0;


  pmt[22]=3;//audio stream_type
  pmt[23]=(FAKE_AUDIO_PID>>8)&0x1F;
  pmt[24]=(FAKE_AUDIO_PID)&0xffF;
  pmt[25]=0;
  pmt[26]=0;

  Write(pmt,188);
}