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
int FAKE_AUDIO_PID    = 0x40;
int FAKE_SUBTITLE_PID = 0x50;
static int pcrLogCount=0;
extern void LogDebug(const char *fmt, ...) ;

static DWORD crc_table[256] = {
	0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b,
	0x1a864db2, 0x1e475005, 0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61,
	0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd, 0x4c11db70, 0x48d0c6c7,
	0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
	0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3,
	0x709f7b7a, 0x745e66cd, 0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039,
	0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5, 0xbe2b5b58, 0xbaea46ef,
	0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
	0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb,
	0xceb42022, 0xca753d95, 0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1,
	0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d, 0x34867077, 0x30476dc0,
	0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
	0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4, 
	0x0808d07d, 0x0cc9cdca, 0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde,
	0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02, 0x5e9f46bf, 0x5a5e5b08,
	0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
	0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc,
	0xb6238b25, 0xb2e29692, 0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6,
	0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a, 0xe0b41de7, 0xe4750050,
	0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
	0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34,
	0xdc3abded, 0xd8fba05a, 0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637,
	0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb, 0x4f040d56, 0x4bc510e1,
	0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
	0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5,
	0x3f9b762c, 0x3b5a6b9b, 0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff,
	0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623, 0xf12f560e, 0xf5ee4bb9,
	0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
	0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd,
	0xcda1f604, 0xc960ebb3, 0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7,
	0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b, 0x9b3660c6, 0x9ff77d71,
	0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
	0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2,
	0x470cdd2b, 0x43cdc09c, 0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8,
	0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24, 0x119b4be9, 0x155a565e,
	0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
	0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a,
	0x2d15ebe3, 0x29d4f654, 0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0,
	0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c, 0xe3a1cbc1, 0xe760d676,
	0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
	0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662,
	0x933eb0bb, 0x97ffad0c, 0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668,
	0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4};

DWORD crc32 (char *data, int len)
{
	register int i;
	DWORD crc = 0xffffffff;

	for (i=0; i<len; i++)
		crc = (crc << 8) ^ crc_table[((crc >> 24) ^ *data++) & 0xff];

	return crc;
}

CTimeShifting::CTimeShifting(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsTimeshifting"), pUnk)
{

	m_params.chunkSize=1024*1024*256;
	m_params.maxFiles=20;
	m_params.maxSize=1024*1024*256;
	m_params.minFiles=6;
  
  m_pmtPid=-1;
  m_pcrPid=-1;
  m_timeShiftMode=ProgramStream;
	m_bTimeShifting=false;
	m_pTimeShiftFile=NULL;
	m_multiPlexer.SetFileWriterCallBack(this);
  
	m_bStartPcrFound=false;
  m_startPcr=0;
  m_highestPcr=0;
  m_bDetermineNewStartPcr=false;
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
		LogDebug("Timeshifter:pcr pid:0x%x",pcrPid); 
		pcrLogCount=0;
		m_multiPlexer.ClearStreams();
		m_multiPlexer.SetPcrPid(pcrPid);
		if (m_bTimeShifting)
		{
			LogDebug("Timeshifter:determine new start pcr"); 
			m_bDetermineNewStartPcr=true;
		}
    m_pcrPid=pcrPid;
		m_vecPids.clear();
		FAKE_NETWORK_ID   = 0x456;
		FAKE_TRANSPORT_ID = 0x4;
		FAKE_SERVICE_ID   = 0x89;
		FAKE_PMT_PID      = 0x20;
		FAKE_PCR_PID      = 0x21;
		FAKE_VIDEO_PID    = 0x30;
		FAKE_AUDIO_PID    = 0x40;
		FAKE_SUBTITLE_PID = 0x50;
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
		LogDebug("Timeshifter:pmt pid:0x%x",pmtPid);
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


STDMETHODIMP CTimeShifting::AddStream(int pid, int serviceType, char* language)
{
  if (pid==0) return S_OK;
	CEnterCriticalSection enter(m_section);
	itvecPids it=m_vecPids.begin();
	while (it!=m_vecPids.end())
	{
		PidInfo& info=*it;
    if (info.realPid==pid) return S_OK;
    ++it;
  }

	try
	{
		if (serviceType==3||serviceType==4||serviceType==0x81)
    {
			if (m_pcrPid == pid)
			{
				FAKE_PCR_PID = FAKE_AUDIO_PID;
			}
      PidInfo info;
			info.realPid=pid;
			info.fakePid=FAKE_AUDIO_PID;
			info.seenStart=false;
			info.serviceType=serviceType;
			strcpy(info.language,language);
			m_vecPids.push_back(info);
			
			LogDebug("Timeshifter:add audio stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
			FAKE_AUDIO_PID++;
			m_multiPlexer.AddPesStream(pid,true,false);
    }
		else if (serviceType==1||serviceType==2||serviceType==0x10||serviceType==0x1b)
    {
			if (m_pcrPid == pid)
			{
				FAKE_PCR_PID = FAKE_VIDEO_PID;
			}
			//LogDebug("Timeshifter:add video pes stream pid:%x",pid);
      PidInfo info;
			info.realPid=pid;
			info.fakePid=FAKE_VIDEO_PID;
			info.seenStart=false;
			info.serviceType=serviceType;
			strcpy(info.language,language);
			m_vecPids.push_back(info);
			LogDebug("Timeshifter:add video stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
			FAKE_VIDEO_PID++;
			m_multiPlexer.AddPesStream(pid,false,true);
    }
		else if (serviceType==5||serviceType==6)
		{
      PidInfo info;
			info.realPid=pid;
			info.fakePid=FAKE_SUBTITLE_PID;
			info.seenStart=false;
			info.serviceType=serviceType;
			strcpy(info.language,language);
			m_vecPids.push_back(info);
			LogDebug("Timeshifter:add subtitle stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
			FAKE_SUBTITLE_PID++;
			m_multiPlexer.AddPesStream(pid,false,true);
		}
		else 
    {
      PidInfo info;
			info.realPid=pid;
			info.fakePid=pid;
			info.serviceType=serviceType;
			info.seenStart=false;
			strcpy(info.language,language);
			LogDebug("Timeshifter:add stream real pid:0x%x fake pid:0x%x type:%x",info.realPid,info.fakePid,info.serviceType);
			m_vecPids.push_back(info);
    }
	}
	catch(...)
	{
		LogDebug("Timeshifter:AddPesStream exception");
	}
	return S_OK;
}

STDMETHODIMP CTimeShifting::RemoveStream(int pid)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("Timeshifter:remove pes stream pid:%x",pid);
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
		LogDebug("Timeshifter:set filename:%s",pszFileName);
    m_iPacketCounter=0;
    m_pmtPid=-1;
    m_pcrPid=-1;
    m_vecPids.clear();
	  m_startPcr=0;
		m_bStartPcrFound=false;
	  m_highestPcr=0;
    m_bDetermineNewStartPcr=false;
		m_multiPlexer.Reset();
		strcpy(m_szFileName,pszFileName);
		strcat(m_szFileName,".tsbuffer");
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

		m_iPmtContinuityCounter=-1;
		m_iPatContinuityCounter=-1;
    m_bDetermineNewStartPcr=false;
		m_startPcr=0;
		m_bStartPcrFound=false;
		m_highestPcr=0;
    m_iPacketCounter=0;
    LogDebug("Timeshifter:Start timeshifting:'%s'",m_szFileName);
    LogDebug("Timeshifter:real pcr:%x fake pcr:%x",m_pcrPid,FAKE_PCR_PID);
    LogDebug("Timeshifter:real pmt:%x fake pmt:%x",m_pmtPid,FAKE_PMT_PID);
    itvecPids it=m_vecPids.begin();
    while (it!=m_vecPids.end())
    {
	    PidInfo& info=*it;
      LogDebug("Timeshifter:real pid:%x fake pid:%x type:%x",info.realPid,info.fakePid,info.serviceType);
      ++it;
    }
		m_bTimeShifting=true;
		if (m_timeShiftMode==TransportStream)
		{
			for (int i=0; i < 20;++i)
			{
				WriteFakePAT();
				WriteFakePMT();
			}
		}
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
    m_bDetermineNewStartPcr=false;
	  m_startPcr=0;
		m_bStartPcrFound=false;
	  m_highestPcr=0;
		m_vecPids.clear();
		m_multiPlexer.Reset();
		FAKE_NETWORK_ID   = 0x456;
		FAKE_TRANSPORT_ID = 0x4;
		FAKE_SERVICE_ID   = 0x89;
		FAKE_PMT_PID      = 0x20;
		FAKE_PCR_PID      = 0x21;
		FAKE_VIDEO_PID    = 0x30;
		FAKE_AUDIO_PID    = 0x40;
		FAKE_SUBTITLE_PID = 0x50;
    m_iPacketCounter=0;
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
		Reset();
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
	if (m_pcrPid<0 || m_vecPids.size()==0|| m_pmtPid<0) return;

	CTsHeader header(tsPacket);
	if (header.TransportError) return;
	if (header.TScrambling!=0) return;
  if (m_iPacketCounter>=100)
  {
    WriteFakePAT();
    WriteFakePMT();
    m_iPacketCounter=0;
    return;
  }

  int PayLoadUnitStart=0;
  if (header.PayloadUnitStart) PayLoadUnitStart=1;

	itvecPids it=m_vecPids.begin();
	while (it!=m_vecPids.end())
	{
		PidInfo& info=*it;
		if (header.Pid==info.realPid)
		{
			if (info.serviceType==1 || info.serviceType==2||info.serviceType==0x10||info.serviceType==0x1b)
			{
				//video
				if (!info.seenStart) 
				{
					if (PayLoadUnitStart)
					{
						info.seenStart=true;
						LogDebug("timeshift: start of video detected");
					}
				}
				if (!info.seenStart) return;
				byte pkt[200];
				memcpy(pkt,tsPacket,188);
				int pid=info.fakePid;
				pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
				pkt[2]=(pid&0xff);
				if (header.Pid==m_pcrPid) PatchPcr(pkt,header);
				if (PayLoadUnitStart) PatchPtsDts(pkt,header,m_startPcr);
				if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
				{
					Write(pkt,188);
          m_iPacketCounter++;
				}
				return;
			}

			if (info.serviceType==3 || info.serviceType==4|| info.serviceType==0x81)
			{
				//audio
				if (!info.seenStart)
				{
					if (PayLoadUnitStart)
					{
						info.seenStart=true;
						LogDebug("timeshift: start of audio detected");
					}
				}
				if (!info.seenStart) return;
				byte pkt[200];
				memcpy(pkt,tsPacket,188);
				int pid=info.fakePid;
				pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
				pkt[2]=(pid&0xff);
				if (header.Pid==m_pcrPid) PatchPcr(pkt,header);
				if (PayLoadUnitStart)  PatchPtsDts(pkt,header,m_startPcr);
				if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
				{
					Write(pkt,188);
          m_iPacketCounter++;
				}
				return;
			}

			if (info.serviceType==5 || info.serviceType==6)
			{
				//subtitle pid...
				byte pkt[200];
				memcpy(pkt,tsPacket,188);
				int pid=info.fakePid;
				pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
				pkt[2]=(pid&0xff);
				if (header.Pid==m_pcrPid) PatchPcr(pkt,header);
				if (PayLoadUnitStart) PatchPtsDts(pkt,header,m_startPcr);
				if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
				{
					Write(pkt,188);
          m_iPacketCounter++;
				}
				return;
			}

			//private pid...
			byte pkt[200];
			memcpy(pkt,tsPacket,188);
			int pid=info.fakePid;
			pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
			pkt[2]=(pid&0xff);
			if (header.Pid==m_pcrPid) PatchPcr(pkt,header);
			if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
			{
				Write(pkt,188);
        m_iPacketCounter++;
			}
			return;
		}
		++it;
	}

  if (header.Pid==m_pcrPid)
  {
    byte pkt[200];
    memcpy(pkt,tsPacket,188);
    int pid=FAKE_PCR_PID;
    pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
    pkt[2]=(pid&0xff);
		PatchPcr(pkt,header);
    if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
		{
			Write(pkt,188);
      m_iPacketCounter++;
		}
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
  m_iPatContinuityCounter++;
  if (m_iPatContinuityCounter>0xf) m_iPatContinuityCounter=0;

  BYTE pat[200];
  memset(pat,0,sizeof(pat));
  pat[0]=0x47;
  pat[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
  pat[2]=(pid&0xff);
  pat[3]=(AdaptionControl<<4) +m_iPatContinuityCounter;
  pat[4]=0;

  pat[5]=tableId;//table id
  pat[6]=0x80+((sectionLenght>>8)&0xf);
  pat[7]=sectionLenght&0xff;
  pat[8]=(transportId>>8)&0xff;
  pat[9]=(transportId)&0xff;
  pat[10]=((version_number&0x1f)<<1)+current_next_indicator;
  pat[11]=section_number;
  pat[12]=last_section_number;
  pat[13]=(FAKE_SERVICE_ID>>8)&0xff;
  pat[14]=(FAKE_SERVICE_ID)&0xff;
  pat[15]=(pmtPid>>8)&0xff;
  pat[16]=(pmtPid)&0xff;
  
  int len=17;
  DWORD crc= crc32((char*)&pat[5],len-5);
  pat[len]=(crc>>24)&0xff;
  pat[len+1]=(crc>>16)&0xff;
  pat[len+2]=(crc>>8)&0xff;
  pat[len+3]=(crc)&0xff;
  Write(pat,188);
}

void CTimeShifting::WriteFakePMT()
{
  int program_info_length=0;
  int sectionLenght=9+2*5+5;
  int version_number=0;
  int current_next_indicator=1;
  int section_number = 0;
  int last_section_number = 0;
  int transportId=FAKE_TRANSPORT_ID;

  int tableId=2;
  int pid=FAKE_PMT_PID;
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
  pmt[8]=(FAKE_SERVICE_ID>>8)&0xff;
  pmt[9]=(FAKE_SERVICE_ID)&0xff;
  pmt[10]=((version_number&0x1f)<<1)+current_next_indicator;
  pmt[11]=section_number;
  pmt[12]=last_section_number;
  pmt[13]=(FAKE_PCR_PID>>8)&0xff;
  pmt[14]=(FAKE_PCR_PID)&0xff;
  pmt[15]=(program_info_length>>8)&0xff;
  pmt[16]=(program_info_length)&0xff;
  
	int pmtLength=9+4;
  int offset=17;
	itvecPids it=m_vecPids.begin();
	while (it!=m_vecPids.end())
	{
		PidInfo& info=*it;
    pmt[offset++]=info.serviceType;
    pmt[offset++]=(info.fakePid>>8)&0x1F; // reserved; elementary_pid (high)
    pmt[offset++]=(info.fakePid)&0xff; // elementary_pid (low)
    pmt[offset++]=0xF0;// reserved; ES_info_length (high)
		pmtLength+=4;
		if (info.serviceType==5||info.serviceType==6)
		{ 
			int esLen=strlen(info.language)+5;
			pmt[offset++]=esLen+2;   // ES_info_length (low)
			pmt[offset++]=0x59;   // descriptor indicator
			pmt[offset++]=esLen;
			pmtLength+=3;
			for (int i=0; i < 3;++i)
			{
				pmt[offset++]=info.language[i];
				pmtLength++;
			}
			pmt[offset++]=0x10;
			pmt[offset++]=0x00;
			pmt[offset++]=0x01;
			pmt[offset++]=0x00;
			pmt[offset++]=0x01;
      pmtLength+=5;
		}
    else
		{
			pmt[offset++]=0;   // ES_info_length (low)
			pmtLength++;
		}
    ++it;
  }


  unsigned section_length = (pmtLength );
  pmt[6]=0x80+((section_length>>8)&0xf);
  pmt[7]=section_length&0xff;

  DWORD crc= crc32((char*)&pmt[5],offset-5);
  pmt[offset++]=(crc>>24)&0xff;
  pmt[offset++]=(crc>>16)&0xff;
  pmt[offset++]=(crc>>8)&0xff;
  pmt[offset++]=(crc)&0xff;
  Write(pmt,188);
}

void CTimeShifting::PatchPcr(byte* tsPacket,CTsHeader& header)
{
  if (header.PayLoadOnly()) return;
  if (tsPacket[4]<7) return; //adaptation field length
  if (tsPacket[5]!=0x10) return;


/*
	char buf[1255];
	strcpy(buf,"");
	for (int i=0; i < 30;++i)
	{
		char tmp[200];
		sprintf(tmp,"%02.2x ", tsPacket[i]);
		strcat(buf,tmp);
	}
	LogDebug(buf);*/

  // There's a PCR.  Get it
	UINT64 pcrBaseHigh=0LL;
	UINT64 k=tsPacket[6]; k<<=25LL;pcrBaseHigh+=k;
	k=tsPacket[7]; k<<=17LL;pcrBaseHigh+=k;
	k=tsPacket[8]; k<<=9LL;pcrBaseHigh+=k;
	k=tsPacket[9]; k<<=1LL;pcrBaseHigh+=k;
	k=((tsPacket[10]>>7)&0x1); pcrBaseHigh +=k;


//  UINT64 pcrBaseHigh = (tsPacket[6]<<24)|(tsPacket[7]<<16)|(tsPacket[8]<<8)|tsPacket[9];
//	pcrBaseHigh<<=1LL;
//pcrBaseHigh += ((tsPacket[10]>>7)&0x1);
//  double clock = pcrBaseHigh/45000.0;
//  if ((tsPacket[10]&0x80) != 0) clock += 1/90000.0; // add in low-bit (if set)
//  unsigned short pcrExt = ((tsPacket[10]&0x01)<<8) | tsPacket[11];
//  clock += pcrExt/27000000.0;


  UINT64 pcrNew=pcrBaseHigh;
  if (m_bDetermineNewStartPcr )
  {
   if (pcrNew!=0LL) 
    {
      m_bDetermineNewStartPcr=false;
	    //correct pcr rollover
      UINT64 duration=m_highestPcr-m_startPcr;
    
      LogDebug("Pcr change detected from:%I64d to:%I64d  duration:%I64d ", m_highestPcr, pcrNew,duration);
	    UINT64 newStartPcr = pcrNew- (duration) ;
	    LogDebug("Pcr new start pcr from:%I64d  to %I64d  ", m_startPcr,newStartPcr);
      m_startPcr=newStartPcr;
      m_highestPcr=newStartPcr;
			pcrLogCount=0;
    }
  }
  
	if (m_bStartPcrFound==false)
	{
		m_bStartPcrFound=true;
		m_startPcr = pcrNew;
    m_highestPcr=pcrNew;
		LogDebug("Pcr new start pcr :%I64d", m_startPcr);
	} 

  if (pcrNew > m_highestPcr)
  {
	  m_highestPcr=pcrNew;
  }

	
  UINT64 pcrHi=pcrNew - m_startPcr;
  tsPacket[6] = ((pcrHi>>25)&0xff);
  tsPacket[7] = ((pcrHi>>17)&0xff);
  tsPacket[8] = ((pcrHi>>9)&0xff);
  tsPacket[9] = ((pcrHi>>1)&0xff);
  tsPacket[10]=	 (pcrHi&0x1);
  tsPacket[11]=0;
//	LogDebug("pcr: org:%x new:%x start:%x", (DWORD)pcrBaseHigh,(DWORD)pcrHi,(DWORD)m_startPcr);
	pcrLogCount++;
}

void CTimeShifting::PatchPtsDts(byte* tsPacket,CTsHeader& header,UINT64 startPcr)
{
  if (false==header.PayloadUnitStart) return;
  int start=header.PayLoadStart;
  if (tsPacket[start] !=0 || tsPacket[start+1] !=0  || tsPacket[start+2] !=1) return; 

  byte* pesHeader=&tsPacket[start];
	UINT64 pts=0LL;
	UINT64 dts=0LL;
	if (!GetPtsDts(pesHeader, pts, dts)) 
	{
		return ;
	}
	if (pts>0LL)
	{
		/*
		char buf[1255];
		strcpy(buf,"");
		for (int i=0; i < 30;++i)
		{
			char tmp[200];
			sprintf(tmp,"%02.2x ", tsPacket[i]);
			strcat(buf,tmp);
		}
		LogDebug(buf);
		*/
		UINT64 ptsorg=pts;
		if (pts > startPcr) 
			pts = (UINT64)( ((UINT64)pts) - ((UINT64)startPcr) );
		else pts=0LL;
	//	LogDebug("pts: org:%I64d new:%I64d start:%I64d pid:%x", ptsorg,pts,startPcr,header.Pid);
		
		byte marker=0x21;
		if (dts!=0) marker=0x31;
		pesHeader[13]=(((pts&0x7f)<<1)+1); pts>>=7;
		pesHeader[12]= (pts&0xff);				  pts>>=8;
		pesHeader[11]=(((pts&0x7f)<<1)+1); pts>>=7;
		pesHeader[10]=(pts&0xff);					pts>>=8;
		pesHeader[9]= (((pts&7)<<1)+marker); 
	}
	if (dts >0LL)
	{
		if (dts > startPcr) 
			dts = (UINT64)( ((UINT64)dts) - ((UINT64)startPcr) );
		else dts=0LL;
		pesHeader[18]=(((dts&0x7f)<<1)+1); dts>>=7;
		pesHeader[17]= (dts&0xff);				  dts>>=8;
		pesHeader[16]=(((dts&0x7f)<<1)+1); dts>>=7;
		pesHeader[15]=(dts&0xff);					dts>>=8;
		pesHeader[14]= (((dts&7)<<1)+0x11); 
	}
}


bool CTimeShifting::GetPtsDts(byte* pesHeader, UINT64& pts, UINT64& dts)
{
	pts=0LL;
	dts=0LL;
	bool ptsAvailable=false;
	bool dtsAvailable=false;
	if ( (pesHeader[7]&0x80)!=0) ptsAvailable=true;
	if ( (pesHeader[7]&0x40)!=0) dtsAvailable=true;
	if (ptsAvailable)
	{	
		pts+= ((pesHeader[13]>>1)&0x7f);				// 7bits	7
		pts+=(pesHeader[12]<<7);								// 8bits	15
		pts+=((pesHeader[11]>>1)<<15);					// 7bits	22
		pts+=((pesHeader[10])<<22);							// 8bits	30
    UINT64 k=((pesHeader[9]>>1)&0x7);
    k <<=30LL;
		pts+=k;			// 3bits
		pts &= 0x1FFFFFFFFLL;
	}
	if (dtsAvailable)
	{
		dts= (pesHeader[18]>>1);								// 7bits	7
		dts+=(pesHeader[17]<<7);								// 8bits	15
		dts+=((pesHeader[16]>>1)<<15);					// 7bits	22
		dts+=((pesHeader[15])<<22);							// 8bits	30
    UINT64 k=((pesHeader[14]>>1)&0x7);
    k <<=30LL;
		dts+=k;			// 3bits
		dts &= 0x1FFFFFFFFLL;
	
	}
	
	return (ptsAvailable||dtsAvailable);
}