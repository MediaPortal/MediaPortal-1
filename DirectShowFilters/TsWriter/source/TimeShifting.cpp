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
#include <cassert>

#include "timeshifting.h"
#include "pmtparser.h"
#include "..\..\shared\dvbutil.h"

#define WRITE_BUFFER_SIZE 32900
#define IGNORE_AFTER_TUNE 25                        // how many TS packets to ignore after tuning
#define TS_QUEUE_SIZE 50                           // how many TS packets fits in TS buffer

#define PID_PAT                               0     // PID for PAT table
#define TABLE_ID_PAT                          0     // TABLE ID for PAT
#define TABLE_ID_SDT                        0x42    // TABLE ID for SDT

#define ADAPTION_FIELD_LENGTH_OFFSET        0x4     // offset in TS header to the adaption field length
#define PCR_FLAG_OFFSET                     0x5     // offset in TS header to the PCR 
#define DISCONTINUITY_FLAG_BIT              0x80    // bitmask for the DISCONTINUITY flag
#define RANDOM_ACCESS_FLAG_BIT              0x40    // bitmask for the RANDOM_ACCESS_FLAG flag
#define ES_PRIORITY_FLAG_BIT                0x20    // bitmask for the ES_PRIORITY_FLAG flag
#define PCR_FLAG_BIT                        0x10    // bitmask for the PCR flag
#define OPCR_FLAG_BIT                       0x8     // bitmask for the OPCR flag
#define SPLICING_FLAG_BIT                   0x4     // bitmask for the SPLICING flag
#define TRANSPORT_PRIVATE_DATA_FLAG_BIT     0x2     // bitmask for the TRANSPORT_PRIVATE_DATA flag
#define ADAPTION_FIELD_EXTENSION_FLAG_BIT   0x1     // bitmask for the DAPTION_FIELD_EXTENSION flag

int FAKE_NETWORK_ID   = 0x456;                // network id we use in our PAT
int FAKE_TRANSPORT_ID = 0x4;                  // transport id we use in our PAT
int FAKE_SERVICE_ID   = 0x89;                 // service id we use in our PAT
int FAKE_PMT_PID      = 0x20;                 // pid we use for our PMT
int FAKE_PCR_PID      = 0x30;//0x21;          // pid we use for our PCR
int FAKE_VIDEO_PID    = 0x30;                 // pid we use for the video stream
int FAKE_AUDIO_PID    = 0x40;                 // pid we use for the audio strean
int FAKE_SUBTITLE_PID = 0x50;                 // pid we use for subtitles
int FAKE_TELETEXT_PID = 0x60; // Ziphnor

double MAX_ALLOWED_PCR_DIFF = 10.0; // (clock value) if pcr/pts/dts difference between last and current value is higher than
								    //			   this value a hole/jump is assumed

extern void LogDebug(const char *fmt, ...) ;

//*******************************************************************
//* ctor
//*******************************************************************
CTimeShifting::CTimeShifting(LPUNKNOWN pUnk, HRESULT *phr) 
	:CUnknown( NAME ("MpTsTimeshifting"), pUnk)
{
	m_bPaused=FALSE;
	m_params.chunkSize=268435424;
	m_params.maxFiles=20;
	m_params.maxSize=268435424;
	m_params.minFiles=6;

	m_iPmtPid=-1;
	m_pcrPid=-1;
	m_iServiceId=-1;
	m_timeShiftMode=ProgramStream;
	m_bTimeShifting=false;
	m_pTimeShiftFile=NULL;

	m_bStartPcrFound=false;
	m_startPcr.Reset();
	m_highestPcr.Reset();
	m_bDetermineNewStartPcr=false;
	m_iPatVersion=0;
	m_iPmtVersion=0;
	m_pWriteBuffer = new byte[WRITE_BUFFER_SIZE];
	m_iWriteBufferPos=0;
  m_TsPacketCount=0;
	m_fDump=NULL;
	m_bIgnoreNextPcrJump=false;
  m_bClearTsQueue=false;
	rclock=new CPcrRefClock();
	m_pPmtParser=new CPmtParser();
  m_pPmtParser->SetPmtCallBack2(this);
	m_mapLastPtsDts.clear();
}
//*******************************************************************
//* dtor
//*******************************************************************
CTimeShifting::~CTimeShifting(void)
{
	delete [] m_pWriteBuffer;
  for (int i=0; i < m_tsQueue.size();++i)
  {
    delete[] m_tsQueue[i];
  }    
  m_tsQueue.clear();
	m_pPmtParser->Reset();
	delete m_pPmtParser;
}

//*******************************************************************
//* OnTsPacket gets called when a new mpeg-2 transport packet has been received
//* Method will check if we are timeshifting and ifso write the packet to
//* the timeshift file if pid is correct
//*******************************************************************
void CTimeShifting::OnTsPacket(byte* tsPacket)
{
	CTsHeader header(tsPacket);
	if (header.Pid==m_iPmtPid)
      m_pPmtParser->OnTsPacket(tsPacket);
	if (m_bPaused) return;
	if (m_bTimeShifting)
	{
		if (header.Pid==0x1fff) return;

		if (header.SyncByte!=0x47) return;
		if (header.TransportError) return;

		CEnterCriticalSection enter(m_section);

		WriteTs(tsPacket);
	}
}


//*******************************************************************
//* Pause or continue timeshifting
//* This method allows the client to pause and continue timeshifting
//* Normally client pauses the timeshifting when updating the pids
//* and continues timeshifting when all pids have been set
//* onOff : 0 = continue, 1=pause
//*******************************************************************
STDMETHODIMP CTimeShifting::Pause( BYTE onOff) 
{
	CEnterCriticalSection enter(m_section);
  if (onOff!=0)
  {
		m_bClearTsQueue=true;
    m_bPaused=TRUE;
  }
	else
  {
		m_bPaused=FALSE;
  }
	if (m_bPaused)
  {
		LogDebug("Timeshifter:paused:yes"); 
  }
	else
	{
		LogDebug("Timeshifter:paused:no"); 
		Flush();
		//m_iPacketCounter=200;//write fake pat/pmt
	}
	return S_OK;
}

//*******************************************************************
//* Sets the video / audio observer
//*******************************************************************
STDMETHODIMP CTimeShifting::SetVideoAudioObserver (IVideoAudioObserver* callback)
{
  if( callback )
  {
    LogDebug("Timeshifter:SetVideoAudioObserver observer ok");
    m_pVideoAudioObserver = callback;
    return S_OK;
  }
  else
  {
    return S_FALSE;
    LogDebug("Timeshifter:SetVideoAudioObserver observer was null");  
  }
}

//*******************************************************************
//* Sets the PMT pid to timeshift
//* pmtPid = the PMT pid
//*******************************************************************
STDMETHODIMP CTimeShifting::SetPmtPid(int pmtPid,int serviceId)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("Timeshifter:pmt pid:0x%x serviceId: 0x%x",pmtPid,serviceId);
  m_iPmtPid=pmtPid;
	m_iServiceId=serviceId;
	m_pPmtParser->SetFilter(pmtPid,serviceId);
	return S_OK;
}

//*******************************************************************
//* Sets the timeshifting mode
//* This can be mpeg-2 program stream or mpeg-2 transport stream
//* mode 0 = ProgramStream, 1= transport stream
//*******************************************************************
STDMETHODIMP CTimeShifting::SetMode(int mode) 
{
	m_timeShiftMode=(TimeShiftingMode)mode;
	if (mode==ProgramStream)
		LogDebug("Timeshifter:program stream mode");
	else
		LogDebug("Timeshifter:transport stream mode");
	return S_OK;
}

//*******************************************************************
//* Returns the timeshifting mode
//* This can be mpeg-2 program stream or mpeg-2 transport stream
//* mode 0 = ProgramStream, 1= transport stream
//*******************************************************************
STDMETHODIMP CTimeShifting::GetMode(int *mode) 
{
	*mode=(int)m_timeShiftMode;
	return S_OK;
}

//*******************************************************************
//* Sets the pcr pid to timeshift
//* pcrPid = the PCR pid
//*******************************************************************
void CTimeShifting::SetPcrPid(int pcrPid)
{
		CEnterCriticalSection enter(m_section);
		LogDebug("Timeshifter:pcr pid:0x%x",pcrPid); 
    LogDebug("Timeshifter:SetPcrPid clear old PIDs");
    m_vecPids.clear();
    m_bClearTsQueue=true;
    m_TsPacketCount=0;

		if (m_bTimeShifting)
		{
			LogDebug("Timeshifter:determine new start pcr"); 
			m_bDetermineNewStartPcr=true;
			m_bIgnoreNextPcrJump=true;
		}
		m_pcrPid=pcrPid;
		m_vecPids.clear();
		FAKE_NETWORK_ID   = 0x456;
		FAKE_TRANSPORT_ID = 0x4;
		FAKE_SERVICE_ID   = 0x89;
		FAKE_PMT_PID      = 0x20;
		FAKE_PCR_PID      = 0x30;//0x21;
		FAKE_VIDEO_PID    = 0x30;
		FAKE_AUDIO_PID    = 0x40;
		FAKE_SUBTITLE_PID = 0x50;
		m_iPatVersion++;
		if (m_iPatVersion>15) 
			m_iPatVersion=0;
		m_iPmtVersion++;
		if (m_iPmtVersion>15) 
			m_iPmtVersion=0;
}

bool CTimeShifting::IsStreamWanted(int stream_type)
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

void CTimeShifting::AddStream(PidInfo2 pidInfo)
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
		PidInfo2 pi;
		pi.seenStart=false;
		pi.fakePid=-1;
		pi.elementaryPid=pidInfo.elementaryPid;
		pi.streamType=pidInfo.streamType;
		pi.rawDescriptorSize=pidInfo.rawDescriptorSize;
		memset(pi.rawDescriptorData,0xFF,pi.rawDescriptorSize);
		memcpy(pi.rawDescriptorData,pidInfo.rawDescriptorData,pi.rawDescriptorSize);
		if (pidInfo.logicalStreamType==SERVICE_TYPE_AUDIO_AC3 || pidInfo.streamType==SERVICE_TYPE_AUDIO_MPEG1 || pidInfo.streamType==SERVICE_TYPE_AUDIO_MPEG2)
		{
			pi.fakePid=FAKE_AUDIO_PID;
			FAKE_AUDIO_PID++;
		}
		else if (pidInfo.streamType==SERVICE_TYPE_VIDEO_MPEG1 || pidInfo.streamType==SERVICE_TYPE_VIDEO_MPEG2 || pidInfo.streamType==SERVICE_TYPE_AUDIO_MPEG1 || pidInfo.streamType==SERVICE_TYPE_VIDEO_MPEG4 || pidInfo.streamType==SERVICE_TYPE_AUDIO_MPEG1 || pidInfo.streamType==SERVICE_TYPE_VIDEO_H264)
		{
			pi.fakePid=FAKE_VIDEO_PID;
			FAKE_VIDEO_PID++;
		}
		else if (pidInfo.logicalStreamType==DESCRIPTOR_DVB_TELETEXT)
		{
			pi.fakePid=FAKE_TELETEXT_PID;
			FAKE_TELETEXT_PID++;
		}
		else if (pidInfo.logicalStreamType==SERVICE_TYPE_DVB_SUBTITLES1 || pidInfo.logicalStreamType==SERVICE_TYPE_DVB_SUBTITLES2)
		{
			pi.fakePid=FAKE_SUBTITLE_PID;
			FAKE_SUBTITLE_PID++;
		}
		if (pi.elementaryPid==m_pcrPid)
			FAKE_PCR_PID=pi.fakePid;
		LogDebug("Timeshifting: add stream pid: 0x%x fake pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pi.fakePid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);
		m_vecPids.push_back(pi);
	}
	else
		LogDebug("TimeShifting: stream rejected - pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);
}

void CTimeShifting::OnPmtReceived2(int pid,int serviceId,int pcrPid,vector<PidInfo2> pidInfos)
{
		CEnterCriticalSection enter(m_section);

    LogDebug("Timeshifter: PMT version changed from %d to %d - ServiceId %x", m_iPmtVersion, m_pPmtParser->GetPmtVersion(), m_iServiceId );
		m_iPmtVersion=m_pPmtParser->GetPmtVersion();
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
//* Sets the filename for the timeshift file
//* pszFileName : full path and filename
//*******************************************************************
STDMETHODIMP CTimeShifting::SetTimeShiftingFileName(char* pszFileName)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:set filename:%s",pszFileName);
		m_iPacketCounter=0;
		m_iPmtPid=-1;
		m_pcrPid=-1;
		m_vecPids.clear();
		m_startPcr.Reset();
		m_bStartPcrFound=false;
		m_highestPcr.Reset();
		m_bDetermineNewStartPcr=false;
		strcpy(m_szFileName,pszFileName);
		strcat(m_szFileName,".tsbuffer");
	}
	catch(...)
	{
		LogDebug("Timeshifter:SetTimeShiftingFileName exception");
	}
	return S_OK;
}

//*******************************************************************
//* Starts timeshifting
//*******************************************************************
STDMETHODIMP CTimeShifting::Start()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		/*m_fDump=fopen("c:\\dump.txt","r");
		if (m_fDump!=NULL)
		{
		fclose(m_fDump);
		::DeleteFile("c:\\dump.ts");
		m_fDump = fopen("c:\\dump.ts","wb+");
		}*/
		if (strlen(m_szFileName)==0) return E_FAIL;
		::DeleteFile((LPCTSTR) m_szFileName);
		WCHAR wstrFileName[2048];
		MultiByteToWideChar(CP_ACP,0,m_szFileName,-1,wstrFileName,1+strlen(m_szFileName));

		//fTsFile = fopen("c:\\users\\public\\test.ts","wb+");
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
		m_startPcr.Reset();
		m_bStartPcrFound=false;
		m_highestPcr.Reset();
		m_mapLastPtsDts.clear();
		m_iPacketCounter=0;
		m_iWriteBufferPos=0;
		LogDebug("Timeshifter:Start timeshifting:'%s'",m_szFileName);
		m_bTimeShifting=true;
		m_bPaused=FALSE;
	}
	catch(...)
	{
		LogDebug("Timeshifter:Start timeshifting exception");
	}
	return S_OK;
}

//*******************************************************************
//* Resets the timeshifter
//*******************************************************************
STDMETHODIMP CTimeShifting::Reset()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("Timeshifter:Reset");
		m_iPmtPid=-1;
		m_pcrPid=-1;
		m_bDetermineNewStartPcr=false;
		m_startPcr.Reset();
		m_bStartPcrFound=false;
		m_highestPcr.Reset();
		m_vecPids.clear();
		m_pPmtParser->Reset();
    m_tsQueue.clear();
    m_pcrHole.Reset();
    m_backwardsPcrHole.Reset();
		FAKE_NETWORK_ID   = 0x456;
		FAKE_TRANSPORT_ID = 0x4;
		FAKE_SERVICE_ID   = 0x89;
		FAKE_PMT_PID      = 0x20;
		FAKE_PCR_PID      = 0x30;//0x21;
		FAKE_VIDEO_PID    = 0x30;
		FAKE_AUDIO_PID    = 0x40;
		FAKE_SUBTITLE_PID = 0x50;
		m_iPacketCounter=0;
		m_bPaused=FALSE;
		m_mapLastPtsDts.clear();
	}
	catch(...)
	{
		LogDebug("Timeshifter:Reset timeshifting exception");
	}
	return S_OK;
}


//*******************************************************************
//* Stops timeshifting
//*******************************************************************
STDMETHODIMP CTimeShifting::Stop()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//fclose(fTsFile );

		LogDebug("Timeshifter:Stop timeshifting:'%s'",m_szFileName);
		m_bTimeShifting=false;
		m_pPmtParser->Reset();
		if (m_pTimeShiftFile!=NULL)
		{
			m_pTimeShiftFile->CloseFile();
			delete m_pTimeShiftFile;
			m_pTimeShiftFile=NULL;
		}
		if (m_fDump!=NULL)
		{
			fclose(m_fDump);
			m_fDump=NULL;
		}
		Reset();
	}
	catch(...)
	{
		LogDebug("Timeshifter:Stop timeshifting exception");
	}
	return S_OK;
}


//*******************************************************************
//* Flush i/o buffer to timeshifting file
//*******************************************************************
void CTimeShifting::Flush()
{
	try
	{
		if (m_iWriteBufferPos>0)
		{
			if (m_pTimeShiftFile!=NULL)
			{
				m_pTimeShiftFile->Write(m_pWriteBuffer,m_iWriteBufferPos);
				m_iWriteBufferPos=0;
			}
		}
	}
	catch(...)
	{
		LogDebug("Timeshifter:Flush exception");
	}
}


//*******************************************************************
//* Write a datablock to i/o buffer
//* When the i/o buffer is full, it is flushed to the timeshifting file
//* Uses internal TS packet queue to allow "rollback" of TS packets
//* on a channel change
//*
//* buffer: block of data
//* len   : length of buffer
//*******************************************************************
void CTimeShifting::Write(byte* buffer, int len)
{
  if (!m_bTimeShifting) return;
  if (buffer == NULL) return;
  if (len != 188) return; //sanity check
  if (m_pWriteBuffer == NULL) return; //sanity check
  if (m_bPaused || m_bClearTsQueue) 
  {
     try
     {
        LogDebug("Timeshifter: clear TS packet queue"); 
        for (int i=0; i < m_tsQueue.size();++i)
        {
          delete[] m_tsQueue[i];
        }
        m_tsQueue.clear();
        m_bClearTsQueue = false;
        ZeroMemory(m_pWriteBuffer, WRITE_BUFFER_SIZE);
        m_iWriteBufferPos = 0;
      }
      catch(...)
      {
        LogDebug("Timeshifter:Write exception - 1");
      }
      return;
    }
    CEnterCriticalSection enter(m_section);

    if (m_tsQueue.size() < TS_QUEUE_SIZE)
    {
      // Put all TS packets to the queue so we can drop some TS packets
      // when tuning to new channel
      try
      {
        char* tmp = new char[len];
        if (tmp!=NULL)
        {
          memcpy(tmp,buffer,len);
          m_tsQueue.push_back(tmp);
        }
      }
      catch(...)
      {
        LogDebug("Timeshifter:Write exception - 2");
        return;
      }
    }

    if (m_tsQueue.size() >= TS_QUEUE_SIZE)
    {
      try  
      {      
        // Copy first TS packet from the queue to the I/O buffer
        if (m_iWriteBufferPos >= 0 && m_iWriteBufferPos+188 <= WRITE_BUFFER_SIZE)
        {
          vector<char*>::iterator it = m_tsQueue.begin();
          char* tmp = *it;
          m_tsQueue.erase(it);
          memcpy(&m_pWriteBuffer[m_iWriteBufferPos], tmp,188);
          delete[] tmp;
          m_iWriteBufferPos+=188;
        }
        else
        {
           LogDebug("Timeshifter:Write m_iWriteBufferPos overflow!");
           m_iWriteBufferPos=0;
        }
      }
      catch(...)
      {
        LogDebug("Timeshifter:Write exception - 3");
        return;
      }

      if (m_iWriteBufferPos >= WRITE_BUFFER_SIZE)
      {
        Flush();
      }
    }  
  }

//*******************************************************************
//* returns the buffer size
//*******************************************************************
STDMETHODIMP CTimeShifting::GetBufferSize(long *size)
{
	CheckPointer(size, E_POINTER);
	*size = 0;
	return S_OK;
}

//*******************************************************************
//* returns the number of timeshifting files in use
//*******************************************************************
STDMETHODIMP CTimeShifting::GetNumbFilesAdded(WORD *numbAdd)
{
	CheckPointer(numbAdd, E_POINTER);
	*numbAdd = (WORD)m_pTimeShiftFile->getNumbFilesAdded();
	return NOERROR;
}

//*******************************************************************
//* returns the number of timeshifting files removed since start
//*******************************************************************
STDMETHODIMP CTimeShifting::GetNumbFilesRemoved(WORD *numbRem)
{
	CheckPointer(numbRem, E_POINTER);
	*numbRem = (WORD)m_pTimeShiftFile->getNumbFilesRemoved();
	return NOERROR;
}

//*******************************************************************
//* returns the current timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::GetCurrentFileId(WORD *fileID)
{
	CheckPointer(fileID, E_POINTER);
	*fileID = (WORD)m_pTimeShiftFile->getCurrentFileId();
	return NOERROR;
}

//*******************************************************************
//* returns the minimum amount of timeshifting files
//*******************************************************************
STDMETHODIMP CTimeShifting::GetMinTSFiles(WORD *minFiles)
{
	CheckPointer(minFiles, E_POINTER);
	*minFiles = (WORD) m_params.minFiles;
	return NOERROR;
}

//*******************************************************************
//* Sets the minimum amount of timeshifting files
//*******************************************************************
STDMETHODIMP CTimeShifting::SetMinTSFiles(WORD minFiles)
{
	m_params.minFiles=(long)minFiles;
	return NOERROR;
}

//*******************************************************************
//* returns the maxmimum amount of timeshifting files
//*******************************************************************
STDMETHODIMP CTimeShifting::GetMaxTSFiles(WORD *maxFiles)
{
	CheckPointer(maxFiles, E_POINTER);
	*maxFiles = (WORD) m_params.maxFiles;
	return NOERROR;
}

//*******************************************************************
//* sets the maxmimum amount of timeshifting files
//*******************************************************************
STDMETHODIMP CTimeShifting::SetMaxTSFiles(WORD maxFiles)
{
	m_params.maxFiles=(long)maxFiles;
	return NOERROR;
}

//*******************************************************************
//* returns the maxmimum filesize for a timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::GetMaxTSFileSize(__int64 *maxSize)
{
	CheckPointer(maxSize, E_POINTER);
	*maxSize = m_params.maxSize;
	return NOERROR;
}

//*******************************************************************
//* sets the maxmimum filesize for a timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::SetMaxTSFileSize(__int64 maxSize)
{
	m_params.maxSize=maxSize;
	return NOERROR;
}

//*******************************************************************
//* returns the initial file length for a timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::GetChunkReserve(__int64 *chunkSize)
{
	CheckPointer(chunkSize, E_POINTER);
	*chunkSize = m_params.chunkSize;
	return NOERROR;
}

//*******************************************************************
//* sets the initial file length for a timeshifting file
//*******************************************************************
STDMETHODIMP CTimeShifting::SetChunkReserve(__int64 chunkSize)
{
	m_params.chunkSize=chunkSize;
	return NOERROR;
}

//*******************************************************************
//* returns the current filesize of all timeshift files
//*******************************************************************
STDMETHODIMP CTimeShifting::GetFileBufferSize(__int64 *lpllsize)
{
	CheckPointer(lpllsize, E_POINTER);
	m_pTimeShiftFile->GetFileSize(lpllsize);
	return NOERROR;
}

//*******************************************************************
//* WriteTs()
//* this method checks if the pid of the tspacket needs to be written to
//* the timeshift file. Ifso.. the pid is written to the timeshifting file
//* This method also inserts the pat/pmt packets in the stream
//* and adjust the PCR/PTS timestamps
//*
//* tsPacket : mpeg-2 transport stream packet of 188 bytes
//*******************************************************************
void CTimeShifting::WriteTs(byte* tsPacket)
{	  	  
	CEnterCriticalSection enter(m_section);
	m_TsPacketCount++;
  if( m_TsPacketCount < IGNORE_AFTER_TUNE ) return;
  if (m_pcrPid<0 || m_vecPids.size()==0 || m_iPmtPid<0) return;

	m_tsHeader.Decode(tsPacket);
	if (m_tsHeader.TransportError) 
	{
	  LogDebug("  m_tsHeader.TransportError - IGNORE TS PACKET!");
	  return;
	}

	bool writeTS = true;
	int start=0;

	if (m_iPacketCounter>=100)
	{
		WriteFakePAT();
		WriteFakePMT();
		m_iPacketCounter=0;
	}

	int PayLoadUnitStart=0;
	if (m_tsHeader.PayloadUnitStart) PayLoadUnitStart=1;

	ivecPidInfo2 it=m_vecPids.begin();
	ivecPidInfo2 itPcr=m_vecPids.end();
	while (it!=m_vecPids.end())
	{
		PidInfo2 &info=*it;

		if (m_tsHeader.Pid==info.elementaryPid)
		{
			// writeTS determines if a TS packet gets written to the timeshifting file or not.
			// invalid headers are skipped, such as scrambled packets.				
			writeTS = true; 
			if (PayLoadUnitStart)
			{
			  byte pkt[200];
			  memcpy(pkt,tsPacket,188);
			  PatchPtsDts(pkt,m_tsHeader,m_startPcr);					  					  
			  start=m_tsHeader.PayLoadStart;
			  if (tsPacket[start] !=0 || tsPacket[start+1] !=0  || tsPacket[start+2] !=1) writeTS = false; 
			}
			if (writeTS)
			{
				if (info.streamType==SERVICE_TYPE_VIDEO_MPEG1 || info.streamType==SERVICE_TYPE_VIDEO_MPEG2||info.streamType==SERVICE_TYPE_VIDEO_MPEG4||info.streamType==SERVICE_TYPE_VIDEO_H264)
			  {
				  //        PatchPcr(tsPacket,m_tsHeader);
				  //video
				  if (!info.seenStart) 
				  {
					  if (PayLoadUnitStart)
					  {
						  info.seenStart=true;
						  LogDebug("timeshift: start of video detected");
              if(m_pVideoAudioObserver)
								m_pVideoAudioObserver->OnNotify(PidType::Video);
					  }
				  }
				  if (!info.seenStart) return;
				  //LogDebug("vid:%x->%x %x %x", info.realPid,info.fakePid,m_tsHeader.ContinuityCounter,m_tsHeader.AdaptionControl);
				  byte pkt[200];
				  memcpy(pkt,tsPacket,188);
				  int pid=info.fakePid;
				  pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
				  pkt[2]=(pid&0xff);
				  if (m_tsHeader.Pid==m_pcrPid)  
						PatchPcr(pkt,m_tsHeader);

				  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
				  {						
					  if (PayLoadUnitStart) PatchPtsDts(pkt,m_tsHeader,m_startPcr);					  					  												
					  Write(pkt,188);
					  m_iPacketCounter++;
				  }
				  return;
			  }

			  if (info.streamType==SERVICE_TYPE_AUDIO_MPEG1 || info.streamType==SERVICE_TYPE_AUDIO_MPEG2|| info.logicalStreamType==SERVICE_TYPE_AUDIO_AC3)
			  {
				  //audio
				  if (!info.seenStart)
				  {
					  if (PayLoadUnitStart)
					  {
						  info.seenStart=true;
						  LogDebug("timeshift: start of audio detected");
              if(m_pVideoAudioObserver)
								m_pVideoAudioObserver->OnNotify(PidType::Audio);
					  }
				  }
				  if (!info.seenStart) return;

				  byte pkt[200];
				  memcpy(pkt,tsPacket,188);
				  int pid=info.fakePid;
				  pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
				  pkt[2]=(pid&0xff);
				  if (m_tsHeader.Pid==m_pcrPid) PatchPcr(pkt,m_tsHeader);

				  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
				  {						  
					  if (PayLoadUnitStart) PatchPtsDts(pkt,m_tsHeader,m_startPcr);					  					  							
					  Write(pkt,188);
					  m_iPacketCounter++;
				  }
				  return;
			  }

			  if (info.logicalStreamType==SERVICE_TYPE_DVB_SUBTITLES1 || info.streamType==SERVICE_TYPE_DVB_SUBTITLES2)
			  {
				  //subtitle pid...
				  byte pkt[200];
				  memcpy(pkt,tsPacket,188);
				  int pid=info.fakePid;
				  pkt[1]=(PayLoadUnitStart<<6) + ( (pid>>8) & 0x1f);
				  pkt[2]=(pid&0xff);
				  if (m_tsHeader.Pid==m_pcrPid) PatchPcr(pkt,m_tsHeader);

				  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
				  {						  						  
					  if (PayLoadUnitStart) PatchPtsDts(pkt,m_tsHeader,m_startPcr);					  					  												
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
			  if (m_tsHeader.Pid==m_pcrPid) PatchPcr(pkt,m_tsHeader);

			  if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
			  {							  
				  Write(pkt,188);
				  m_iPacketCounter++;
			  }
			}
			return;
		}
		++it;
	}

	if (m_tsHeader.Pid==m_pcrPid)
	{
		byte pkt[200];
		memcpy(pkt,tsPacket,188);
		int pid=FAKE_PCR_PID;
		PatchPcr(pkt,m_tsHeader);
		pkt[1]=( (pid>>8) & 0x1f);
		pkt[2]=(pid&0xff);
		pkt[3]=(2<<4);// Adaption Field Control==adaptation field only, no payload
		pkt[4]=0xb7;

		if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
		{						
			Write(pkt,188);
			m_iPacketCounter++;
		}
		return;
	}
}


//*******************************************************************
//* WriteFakePAT()
//* Constructs a PAT table and writes it to the timeshifting file
//*******************************************************************
void CTimeShifting::WriteFakePAT()
{
	int tableId=TABLE_ID_PAT;
	int transportId=FAKE_TRANSPORT_ID;
	int pmtPid=FAKE_PMT_PID;
	int sectionLenght=9+4;
	int current_next_indicator=1;
	int section_number = 0;
	int last_section_number = 0;

	int pid=PID_PAT;
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
	pat[10]=((m_iPatVersion&0x1f)<<1)+current_next_indicator;
	pat[11]=section_number;
	pat[12]=last_section_number;
	pat[13]=(FAKE_SERVICE_ID>>8)&0xff;
	pat[14]=(FAKE_SERVICE_ID)&0xff;
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
void CTimeShifting::WriteFakePMT()
{
	int program_info_length=0;
	int sectionLenght=9+2*5+5;

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

	// Ziphnor: is this enough (and we only write 188 bytes of it at the end)?
	// a single descriptor can contain up to 257 bytes (up to 255 bytes after descriptor_length)
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
	pmt[10]=((m_iPmtVersion&0x1f)<<1)+current_next_indicator;
	pmt[11]=section_number;
	pmt[12]=last_section_number;
	pmt[13]=(FAKE_PCR_PID>>8)&0xff;
	pmt[14]=(FAKE_PCR_PID)&0xff;
	pmt[15]=(program_info_length>>8)&0xff;
	pmt[16]=(program_info_length)&0xff;

	int pmtLength=9+4;
	int offset=17;
	ivecPidInfo2 it=m_vecPids.begin();
	while (it!=m_vecPids.end())
	{
		PidInfo2 info=*it;
		pmt[offset++]=info.streamType;
		pmt[offset++]=0xe0+((info.fakePid>>8)&0x1F); // reserved; elementary_pid (high)
		pmt[offset++]=(info.fakePid)&0xff; // elementary_pid (low)
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


//*******************************************************************
//* PatchPcr()
//* Patches the PCR so the start of the timeshifting file always starts
//* with a PCR of 0. 
//* When zapping from channel A<-> channel B it will also make sure
//* that the PCR is continuious
//*******************************************************************
void CTimeShifting::PatchPcr(byte* tsPacket,CTsHeader& header)
{
	CPcr myPcr=rclock->GetAsPCR();
	//LogDebug("pcr pid:%x  head:%02.2x %02.2x %02.2x %02.2x %02.2x %02.2x",header.Pid, tsPacket[0],tsPacket[1],tsPacket[2],tsPacket[3],tsPacket[4],tsPacket[5]);
	if (header.PayLoadOnly()) return;
	//LogDebug(" pcrflag:%x", (tsPacket[5]&0x10));
	//LogDebug(" opcrflag:%x", (tsPacket[5]&0x8));
	m_adaptionField.Decode(header,tsPacket);
	if (m_adaptionField.PcrFlag==false) return;
	CPcr pcrNew=m_adaptionField.Pcr;
	bool logNextPcr = false;
  if (m_bStartPcrFound)
	{
		if (m_bDetermineNewStartPcr )
		{
			m_bDetermineNewStartPcr=false;

			CPcr duration=m_highestPcr ;
			duration-=m_startPcr;
			CPcr newStartPcr=pcrNew;
			newStartPcr-=duration ;
			LogDebug("Pcr start    :%s",m_startPcr.ToString());
			LogDebug("Pcr high     :%s",m_highestPcr.ToString());
			LogDebug("Pcr duration :%s",duration.ToString());
			LogDebug("Pcr current  :%s",pcrNew.ToString());
			LogDebug("Pcr newstart :%s",newStartPcr.ToString());

			m_startPcr  = newStartPcr;
			m_highestPcr= newStartPcr;
			m_prevPcr   = newStartPcr;
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

  if (m_bDetermineNewStartPcr)
  {
    LogDebug( "not allowed to patch PCRs yet - m_bDetermineNewStartPcr = true" );
  }

	// PCR value has jumped too much in the stream
  if (diff.ToClock() > MAX_ALLOWED_PCR_DIFF && m_prevPcr.ToClock() > 0 && !m_bDetermineNewStartPcr )
	{
    logNextPcr = true;
    if (m_bIgnoreNextPcrJump)
		{
			LogDebug( "Ignoring first PCR jump after channel change" );
			m_bIgnoreNextPcrJump=false;
		}
		else
		{
			if (diff.ToClock() > 95443L) // Max PCR value 95443.71768
			{
				m_bPCRRollover = true;
				m_pcrDuration = m_prevPcr;
				m_pcrDuration -= m_startPcr;
				m_pcrHole.Reset();
        m_backwardsPcrHole.Reset();
				m_startPcr.Reset();
				m_highestPcr.Reset();

				LogDebug( "PCR rollover detected" );
			}
			else
			{      
				logNextPcr = true;
        CPcr step;
				step.FromClock(0.02); // an estimated PCR step, to be fixed with some average from the stream...

				if (pcrNew > m_prevPcr)
				{
					m_pcrHole += diff;
					m_pcrHole -= step;
					LogDebug( "Jump forward in PCR detected" );
				}
				else
				{
					m_backwardsPcrHole += diff;
					m_backwardsPcrHole += step;
          LogDebug( "Jump backward in PCR detected" );
				}
			}
		}
	}

  /*pcrHi -= m_startPcr;
  pcrHi += m_backwardsPcrHole;
  pcrHi -= m_pcrHole;*/

  double result = pcrHi.ToClock() - m_startPcr.ToClock() + m_backwardsPcrHole.ToClock() - m_pcrHole.ToClock();
  pcrHi.FromClock(result);

	if (m_bPCRRollover)
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
	CPcr tmpPcr=m_startPcr+myPcr;
	//LogDebug("myPcr: %s tsPCR: %s",tmpPcr.ToString(),m_prevPcr.ToString());
}

//*******************************************************************
//* PatchPtsDts()
//* Patches the PTS/DTS timestamps in a audio/video transport packet
//* so the start of the timeshifting file always starts
//* with a PTS/DTS of 0. 
//* When zapping from channel A<-> channel B it will also make sure
//* that the PTS/TS is continuious
//*******************************************************************
void CTimeShifting::PatchPtsDts(byte* tsPacket,CTsHeader& header,CPcr& startPcr)
{
	if (false==header.PayloadUnitStart) return;

	int start=header.PayLoadStart;
	if (start>=188) 
	{
		LogDebug("ERROR: payload start>188. payloadStart=%d adaptionControl=%d adaptionFieldLength=%d",start,header.AdaptionControl,header.AdaptionFieldLength);
		//return;
	}
	if (tsPacket[start] !=0 || tsPacket[start+1] !=0  || tsPacket[start+2] !=1) return; 

	byte* pesHeader=&tsPacket[start];
	CPcr pts;
	CPcr dts;
	if (!CPcr::DecodeFromPesHeader(pesHeader,pts,dts))
	{
		return ;
	}
	imapLastPtsDts it=m_mapLastPtsDts.find(header.Pid);
	if (it==m_mapLastPtsDts.end())
	{
		LastPtsDtsRecord lastRec;
		lastRec.pts.Reset();
		lastRec.dts.Reset();
		m_mapLastPtsDts[header.Pid]=lastRec;
		it=m_mapLastPtsDts.find(header.Pid);
	}
	LastPtsDtsRecord &lastPtsDts=it->second;
	if (pts.IsValid)
	{
		CPcr ptsorg=pts;
		pts -= startPcr;
		//pts -= m_pcrHole;
		// GEMX: code is currently being tested
		if (lastPtsDts.pts.IsValid)
		{
			double diff=0;
			if (pts.ToClock()>lastPtsDts.pts.ToClock())
				diff=pts.ToClock()-lastPtsDts.pts.ToClock();
			else
				diff=lastPtsDts.pts.ToClock()-pts.ToClock();
			// Only apply pcr hole patching if pts also jumped
			lastPtsDts.pts=pts;
			if (diff>MAX_ALLOWED_PCR_DIFF && m_pcrHole.IsValid)
					pts -= m_pcrHole;
		}
		else
		{
			lastPtsDts.pts=pts;
			pts-=m_pcrHole;
		}
		if( m_bPCRRollover )
			pts += m_pcrDuration;		

		// 9       10        11        12      13
		//76543210 76543210 76543210 76543210 76543210
		//0011pppM pppppppp pppppppM pppppppp pppppppM 
		//LogDebug("pts: org:%s new:%s start:%s", ptsorg.ToString(),pts.ToString(),startPcr.ToString()); 
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
			//dts -= m_pcrHole;

			// GEMX: deactivated. code is currently being tested
			if (lastPtsDts.dts.IsValid)
			{
				double diff=0;
				if (dts.ToClock()>lastPtsDts.dts.ToClock())
					diff=dts.ToClock()-lastPtsDts.dts.ToClock();
				else
					diff=lastPtsDts.dts.ToClock()-dts.ToClock();
				// Only apply pcr hole patching if dts also jumped
				lastPtsDts.dts=dts;
				if (diff>MAX_ALLOWED_PCR_DIFF && m_pcrHole.IsValid)
					dts -= m_pcrHole;
			}
			else
			{
				lastPtsDts.dts=dts;
				dts-=m_pcrHole;
			}
			if( m_bPCRRollover )
				dts += m_pcrDuration;			

			// 14       15        16        17      18
			//76543210 76543210 76543210 76543210 76543210
			//0001pppM pppppppp pppppppM pppppppp pppppppM 
			//LogDebug("dts: org:%s new:%s start:%s", dtsorg.ToString(),dts.ToString(),startPcr.ToString()); 
			pesHeader[18]=(byte)( (((dts.PcrReferenceBase&0x7f)<<1)+1));  dts.PcrReferenceBase>>=7;
			pesHeader[17]=(byte)(   (dts.PcrReferenceBase&0xff));				  dts.PcrReferenceBase>>=8;
			pesHeader[16]=(byte)( (((dts.PcrReferenceBase&0x7f)<<1)+1));  dts.PcrReferenceBase>>=7;
			pesHeader[15]=(byte)(   (dts.PcrReferenceBase&0xff));					dts.PcrReferenceBase>>=8;
			pesHeader[14]=(byte)( (((dts.PcrReferenceBase&7)<<1)+0x11)); 
		}

		//pts.Reset();
		//dts.Reset();
		//if (CPcr::DecodeFromPesHeader(pesHeader,pts,dts))
		//{
		//	LogDebug("pts:%s dts:%s", pts.ToString(),dts.ToString());
		//}
	}
}
