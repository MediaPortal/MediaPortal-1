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

#include "DiskRecorder.h"
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

#define ERROR_FILE_TOO_LARGE 223
#define RECORD_BUFFER_SIZE 256000

int DR_FAKE_NETWORK_ID   = 0x456;                // network id we use in our PAT
int DR_FAKE_TRANSPORT_ID = 0x4;                  // transport id we use in our PAT
int DR_FAKE_SERVICE_ID   = 0x89;                 // service id we use in our PAT
int DR_FAKE_PMT_PID      = 0x20;                 // pid we use for our PMT
int DR_FAKE_PCR_PID      = 0x30;//0x21;          // pid we use for our PCR
int DR_FAKE_VIDEO_PID    = 0x30;                 // pid we use for the video stream
int DR_FAKE_AUDIO_PID    = 0x40;                 // pid we use for the audio strean
int DR_FAKE_SUBTITLE_PID = 0x50;                 // pid we use for subtitles
int DR_FAKE_TELETEXT_PID = 0x60; // Ziphnor

double DR_MAX_ALLOWED_PCR_DIFF = 10.0; // (clock value) if pcr/pts/dts difference between last and current value is higher than
								    //			   this value a hole/jump is assumed

extern void LogDebug(const char *fmt, ...) ;

//*******************************************************************
//* ctor
//*******************************************************************
CDiskRecorder::CDiskRecorder(RecordingMode mode) 
{
	m_recordingMode=mode;
	m_hFile=INVALID_HANDLE_VALUE;
	m_bPaused=FALSE;
	m_params.chunkSize=268435424;
	m_params.maxFiles=20;
	m_params.maxSize=268435424;
	m_params.minFiles=6;

	m_iPmtPid=-1;
	m_pcrPid=-1;
	m_iServiceId=-1;
	m_streamMode=ProgramStream;

	m_bRunning=false;
	m_pTimeShiftFile=NULL;

	m_bStartPcrFound=false;
	m_startPcr.Reset();
	m_highestPcr.Reset();
	m_bDetermineNewStartPcr=false;
	m_iPatVersion=0;
	m_iPmtVersion=0;
	if (m_recordingMode==RecordingMode::TimeShift)
		m_pWriteBuffer = new byte[WRITE_BUFFER_SIZE];
	else
		m_pWriteBuffer = new byte[RECORD_BUFFER_SIZE];
	m_iWriteBufferPos=0;
  m_TsPacketCount=0;
	m_bIgnoreNextPcrJump=false;
  m_bClearTsQueue=false;
	rclock=new CPcrRefClock();
	m_pPmtParser=new CPmtParser();
	m_multiPlexer.SetFileWriterCallBack(this);
	m_pVideoAudioObserver=NULL;
	m_mapLastPtsDts.clear();
}
//*******************************************************************
//* dtor
//*******************************************************************
CDiskRecorder::~CDiskRecorder(void)
{
	CEnterCriticalSection enter(m_section);
  if (m_hFile!=INVALID_HANDLE_VALUE)
  {
	  CloseHandle(m_hFile);
	  m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file
  }
	delete [] m_pWriteBuffer;
  for (int i=0; i < m_tsQueue.size();++i)
  {
    delete[] m_tsQueue[i];
  }    
  m_tsQueue.clear();
	m_pPmtParser->Reset();
	delete m_pPmtParser;
}

void CDiskRecorder::SetFileName(char* pszFileName)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		WriteLog("set filename:%s",pszFileName);
		m_iPacketCounter=0;
		m_iPmtPid=-1;
		m_pcrPid=-1;
		m_vecPids.clear();
		m_startPcr.Reset();
		m_bStartPcrFound=false;
		m_highestPcr.Reset();
		m_bDetermineNewStartPcr=false;
		m_multiPlexer.Reset();
		strcpy(m_szFileName,pszFileName);
		if (m_recordingMode==RecordingMode::TimeShift)
			strcat(m_szFileName,".tsbuffer");
	}
	catch(...)
	{
		WriteLog("SetFilename exception");
	}
}

bool CDiskRecorder::Start()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		if (strlen(m_szFileName)==0) return false;
		::DeleteFile((LPCTSTR) m_szFileName);
		m_iPart=2;
		if (m_recordingMode==RecordingMode::TimeShift)
		{
			WCHAR wstrFileName[2048];
			MultiByteToWideChar(CP_ACP,0,m_szFileName,-1,wstrFileName,1+strlen(m_szFileName));
			m_pTimeShiftFile = new MultiFileWriter(&m_params);
			if (FAILED(m_pTimeShiftFile->OpenFile(wstrFileName))) 
			{
				WriteLog("failed to open filename:%s %d",m_szFileName,GetLastError());
				m_pTimeShiftFile->CloseFile();
				delete m_pTimeShiftFile;
				m_pTimeShiftFile=NULL;
				return false;
			}
		}
		else
		{
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
//				  						 (DWORD) FILE_FLAG_RANDOM_ACCESS,
//											 (DWORD) FILE_FLAG_WRITE_THROUGH,             // More flags
												 (DWORD) 0,             // More flags
													NULL);                 // Template
			if (m_hFile == INVALID_HANDLE_VALUE)
			{
				LogDebug("unable to create file:'%s' %d",m_szFileName, GetLastError());
				return false;
			}
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
		WriteLog("Start '%s'",m_szFileName);
		m_bRunning=true;
		m_bPaused=FALSE;
	}
	catch(...)
	{
		WriteLog("Start exception");
	}
	return true;
}

void CDiskRecorder::Stop()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		WriteLog("Stop '%s'",m_szFileName);
		m_bRunning=false;
		m_pPmtParser->Reset();
		m_multiPlexer.Reset();
		if (m_pTimeShiftFile!=NULL)
		{
			m_pTimeShiftFile->CloseFile();
			delete m_pTimeShiftFile;
			m_pTimeShiftFile=NULL;
		}
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
		Reset();
	}
	catch(...)
	{
		WriteLog("Stop  exception");
	}
}

void CDiskRecorder::Pause(BYTE onOff) 
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
		WriteLog("paused=yes"); 
  }
	else
	{
		WriteLog("paused=no"); 
		Flush();
		if (m_vecPids.size()==0 && m_streamMode==StreamMode::TransportStream)
			WriteLog("PANIC changed status to running but i have not a single stream to record !!!!");
	}
}

void CDiskRecorder::Reset()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		WriteLog("Reset");
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
		DR_FAKE_NETWORK_ID   = 0x456;
		DR_FAKE_TRANSPORT_ID = 0x4;
		DR_FAKE_SERVICE_ID   = 0x89;
		DR_FAKE_PMT_PID      = 0x20;
		DR_FAKE_PCR_PID      = 0x30;//0x21;
		DR_FAKE_VIDEO_PID    = 0x30;
		DR_FAKE_AUDIO_PID    = 0x40;
		DR_FAKE_SUBTITLE_PID = 0x50;
		m_iPacketCounter=0;
		m_bPaused=FALSE;
		m_mapLastPtsDts.clear();
	}
	catch(...)
	{
		WriteLog("Reset exception");
	}
}

void CDiskRecorder::GetRecordingMode(int *mode) 
{
	*mode=(int)m_recordingMode;
}

void CDiskRecorder::SetStreamMode(int mode) 
{
	m_streamMode=(StreamMode)mode;
	if (mode==ProgramStream)
		WriteLog("program stream mode");
	else
		WriteLog("transport stream mode");
}

void CDiskRecorder::GetStreamMode(int *mode) 
{
	*mode=(int)m_streamMode;
}

void CDiskRecorder::SetPmtPid(int pmtPid,int serviceId,byte* pmtData,int pmtLength)
{
	CEnterCriticalSection enter(m_section);
	WriteLog("Received from TvService: pmt pid:0x%x serviceId: 0x%x pmtlength:%d",pmtPid,serviceId,pmtLength);
  m_iPmtPid=pmtPid;
	m_iServiceId=serviceId;
	m_pPmtParser->Reset();
	m_pPmtParser->SetFilter(pmtPid,serviceId);
	CSection section;
	section.Data=new byte[MAX_SECTION_LENGTH*5];
	section.Reset();
	section.BufferPos=pmtLength;
	memcpy(section.Data,pmtData,pmtLength);
	section.DecodeHeader();
	m_vecPids.clear();
	WriteLog("Old pids cleared");
	WriteLog("got pmt - tableid: 0x%x section_length: %d sid: 0x%x",section.table_id,section.section_length,section.table_id_extension);
	int pcrPid; vector<PidInfo2> pidInfos;
	if (!m_pPmtParser->DecodePmt(section,pcrPid,pidInfos))
	{
		WriteLog("!!! PANIC - DecodePmt(...) returned FALSE !!!");
		delete section.Data;
		return;
	}
	WriteLog("PMT parsed  - Pid 0x%x ServiceId 0x%x stream count: %d",pmtPid, m_iServiceId,pidInfos.size());
	SetPcrPid(pcrPid);
	ivecPidInfo2 it=pidInfos.begin();
	while (it!=pidInfos.end())
	{
		PidInfo2 info=*it;
		AddStream(info);
		++it;
	}
	delete section.Data;
}

void CDiskRecorder::SetVideoAudioObserver (IVideoAudioObserver* callback)
{
  if(callback)
  {
    WriteLog("SetVideoAudioObserver observer ok");
    m_pVideoAudioObserver = callback;
  }
  else
  {
    WriteLog("SetVideoAudioObserver observer was null");  
		return;
  }
}

void CDiskRecorder::GetBufferSize(long *size)
{
	*size = 0;
}

void CDiskRecorder::GetNumbFilesAdded(WORD *numbAdd)
{
	*numbAdd = (WORD)m_pTimeShiftFile->getNumbFilesAdded();
}

void CDiskRecorder::GetNumbFilesRemoved(WORD *numbRem)
{
	*numbRem = (WORD)m_pTimeShiftFile->getNumbFilesRemoved();
}

void CDiskRecorder::GetCurrentFileId(WORD *fileID)
{
	*fileID = (WORD)m_pTimeShiftFile->getCurrentFileId();
}

void CDiskRecorder::GetMinTSFiles(WORD *minFiles)
{
	*minFiles = (WORD) m_params.minFiles;
}

void CDiskRecorder::SetMinTSFiles(WORD minFiles)
{
	m_params.minFiles=(long)minFiles;
}

void CDiskRecorder::GetMaxTSFiles(WORD *maxFiles)
{
	*maxFiles = (WORD) m_params.maxFiles;
}

void CDiskRecorder::SetMaxTSFiles(WORD maxFiles)
{
	m_params.maxFiles=(long)maxFiles;
}

void CDiskRecorder::GetMaxTSFileSize(__int64 *maxSize)
{
	*maxSize = m_params.maxSize;
}

void CDiskRecorder::SetMaxTSFileSize(__int64 maxSize)
{
	m_params.maxSize=maxSize;
}

void CDiskRecorder::GetChunkReserve(__int64 *chunkSize)
{
	*chunkSize = m_params.chunkSize;
}

void CDiskRecorder::SetChunkReserve(__int64 chunkSize)
{
	m_params.chunkSize=chunkSize;
}

void CDiskRecorder::GetFileBufferSize(__int64 *lpllsize)
{
	m_pTimeShiftFile->GetFileSize(lpllsize);
}



void CDiskRecorder::OnTsPacket(byte* tsPacket)
{
	if (m_bPaused) return;
	if (m_bRunning)
	{
		CEnterCriticalSection enter(m_section);		
		CTsHeader header(tsPacket);
		if (header.Pid==0x1fff) return;
		if (header.SyncByte!=0x47) return;
		if (header.TransportError) return;
		if (m_streamMode==StreamMode::TransportStream)
			WriteTs(tsPacket);
		else
			m_multiPlexer.OnTsPacket(tsPacket);
	}
}

void CDiskRecorder::Write(byte* buffer,int len)
{
	CEnterCriticalSection enter(m_section);
	if (m_recordingMode==RecordingMode::TimeShift)
		WriteToTimeshiftFile(buffer,len);
	else
		WriteToRecording(buffer,len);
}

void CDiskRecorder::WriteToRecording(byte* buffer, int len)
{
	CEnterCriticalSection enter(m_section);
	try{
	if (!m_bRunning) return;
  if (buffer==NULL) return;
  if (m_bPaused || m_bClearTsQueue) 
  {
     try
     {
        WriteLog("clear TS packet queue"); 
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
        WriteLog("Write exception - 1");
      }
      return;
  }
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
							LogDebug("Runable to write file:'%s' %d %d %x",m_szFileName, GetLastError(),m_iWriteBufferPos,m_hFile);
						}
          }//of if (FALSE == WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL))
		    }//if (m_hFile!=INVALID_HANDLE_VALUE)
      }//if (m_iWriteBufferPos>0)
      m_iWriteBufferPos=0;
	  }
	  catch(...)
	  {
		  LogDebug("Write exception");
	  }
  }// of if (len + m_iWriteBufferPos >= RECORD_BUFFER_SIZE)

  if ( (m_iWriteBufferPos+len) < RECORD_BUFFER_SIZE && len > 0)
  {
    memcpy(&m_pWriteBuffer[m_iWriteBufferPos],buffer,len);
    m_iWriteBufferPos+=len;
  }
	} catch (...) { WriteLog("Exception in writetorecording");}
}

void CDiskRecorder::WriteToTimeshiftFile(byte* buffer, int len)
{
  if (!m_bRunning) return;
  if (buffer == NULL) return;
  if (len != 188) return; //sanity check
  if (m_pWriteBuffer == NULL) return; //sanity check
  if (m_bPaused || m_bClearTsQueue) 
  {
     try
     {
        WriteLog("clear TS packet queue"); 
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
        WriteLog("Write exception - 1");
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
        WriteLog("Write exception - 2");
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
           WriteLog("Write m_iWriteBufferPos overflow!");
           m_iWriteBufferPos=0;
        }
      }
      catch(...)
      {
        WriteLog("Write exception - 3");
        return;
      }

      if (m_iWriteBufferPos >= WRITE_BUFFER_SIZE)
      {
        Flush();
      }
    }  
  }

void CDiskRecorder::WriteLog(const char* fmt,...)
{
	char logbuffer[2000]; 
	va_list ap;
	va_start(ap,fmt);

	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(logbuffer, fmt, ap);
	va_end(ap); 

	if (m_recordingMode==RecordingMode::TimeShift)
		LogDebug("DiskRecorder[TIMESHIFT] %s",logbuffer);
	else
		LogDebug("DiskRecorder[RECORD] %s",logbuffer);
}

void CDiskRecorder::SetPcrPid(int pcrPid)
{
		CEnterCriticalSection enter(m_section);
		WriteLog("pcr pid:0x%x",pcrPid); 
    WriteLog("SetPcrPid clear old PIDs");
    m_vecPids.clear();
    m_bClearTsQueue=true;
    m_TsPacketCount=0;

		if (m_bRunning)
		{
			WriteLog("determine new start pcr"); 
			m_bDetermineNewStartPcr=true;
			m_bIgnoreNextPcrJump=true;
		}
		m_pcrPid=pcrPid;
		m_vecPids.clear();
		DR_FAKE_NETWORK_ID   = 0x456;
		DR_FAKE_TRANSPORT_ID = 0x4;
		DR_FAKE_SERVICE_ID   = 0x89;
		DR_FAKE_PMT_PID      = 0x20;
		DR_FAKE_PCR_PID      = 0x30;//0x21;
		DR_FAKE_VIDEO_PID    = 0x30;
		DR_FAKE_AUDIO_PID    = 0x40;
		DR_FAKE_SUBTITLE_PID = 0x50;
		m_iPatVersion++;
		if (m_iPatVersion>15) 
			m_iPatVersion=0;
		m_iPmtVersion++;
		if (m_iPmtVersion>15) 
			m_iPmtVersion=0;
		if (m_streamMode==StreamMode::ProgramStream)
			m_multiPlexer.SetPcrPid(pcrPid);
}

bool CDiskRecorder::IsStreamWanted(int stream_type)
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

void CDiskRecorder::AddStream(PidInfo2 pidInfo)
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
			if (m_streamMode==StreamMode::TransportStream)
			{
				pi.fakePid=DR_FAKE_AUDIO_PID;
				DR_FAKE_AUDIO_PID++;
				m_vecPids.push_back(pi);
			}
			else
			{
				if (pidInfo.logicalStreamType==SERVICE_TYPE_AUDIO_AC3)
					m_multiPlexer.AddPesStream(pi.elementaryPid,true,false,false);
				else
					m_multiPlexer.AddPesStream(pi.elementaryPid,false,true,false);
			}
			WriteLog("add audio stream pid: 0x%x fake pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pi.fakePid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);
		}
		else if (pidInfo.streamType==SERVICE_TYPE_VIDEO_MPEG1 || pidInfo.streamType==SERVICE_TYPE_VIDEO_MPEG2 || pidInfo.streamType==SERVICE_TYPE_AUDIO_MPEG1 || pidInfo.streamType==SERVICE_TYPE_VIDEO_MPEG4 || pidInfo.streamType==SERVICE_TYPE_AUDIO_MPEG1 || pidInfo.streamType==SERVICE_TYPE_VIDEO_H264)
		{
			if (m_streamMode==StreamMode::TransportStream)
			{
				pi.fakePid=DR_FAKE_VIDEO_PID;
				DR_FAKE_VIDEO_PID++;
				m_vecPids.push_back(pi);
			}
			else
				m_multiPlexer.AddPesStream(pi.elementaryPid,false,false,true);
			WriteLog("add video stream pid: 0x%x fake pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pi.fakePid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);			
		}
		if (m_streamMode==StreamMode::TransportStream)
		{
			if (pidInfo.logicalStreamType==DESCRIPTOR_DVB_TELETEXT)
			{
				pi.fakePid=DR_FAKE_TELETEXT_PID;
				DR_FAKE_TELETEXT_PID++;
				m_vecPids.push_back(pi);
				WriteLog("add teletext stream pid: 0x%x fake pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pi.fakePid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);
			}
			else if (pidInfo.logicalStreamType==SERVICE_TYPE_DVB_SUBTITLES1 || pidInfo.logicalStreamType==SERVICE_TYPE_DVB_SUBTITLES2)
			{
				pi.fakePid=DR_FAKE_SUBTITLE_PID;
				DR_FAKE_SUBTITLE_PID++;
				m_vecPids.push_back(pi);
				WriteLog("add subtitle stream pid: 0x%x fake pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pi.fakePid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);
			}
		}
		else
			if (pidInfo.logicalStreamType==DESCRIPTOR_DVB_TELETEXT || pidInfo.logicalStreamType==SERVICE_TYPE_DVB_SUBTITLES1 || pidInfo.logicalStreamType==SERVICE_TYPE_DVB_SUBTITLES2)
			WriteLog("stream rejected - pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);

		if (pi.elementaryPid==m_pcrPid)
			DR_FAKE_PCR_PID=pi.fakePid;
	}
	else
		WriteLog("stream rejected - pid: 0x%x stream type: 0x%x logical type: 0x%x descriptor length: %d",pidInfo.elementaryPid,pidInfo.streamType,pidInfo.logicalStreamType,pidInfo.rawDescriptorSize);
}

void CDiskRecorder::Flush()
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
		WriteLog("Flush exception");
	}
}

void CDiskRecorder::WriteTs(byte* tsPacket)
{	  	  
	CEnterCriticalSection enter(m_section);
	try{
	m_TsPacketCount++;
  if( m_TsPacketCount < IGNORE_AFTER_TUNE ) return;
  if (m_pcrPid<0 || m_vecPids.size()==0 || m_iPmtPid<0) return;

	m_tsHeader.Decode(tsPacket);
	if (m_tsHeader.TransportError) 
		return;

	bool writeTS = true;
	int start=0;

	if (m_iPacketCounter>=100)
	{
		if (m_streamMode==StreamMode::TransportStream)
		{
			WriteFakePAT();
			WriteFakePMT();
		}
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
				  //video
				  if (!info.seenStart) 
				  {
					  if (PayLoadUnitStart)
					  {
						  info.seenStart=true;
						  WriteLog("start of video detected");
              if(m_pVideoAudioObserver)
								m_pVideoAudioObserver->OnNotify(PidType::Video);
					  }
				  }
					if (!info.seenStart) return;
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
						  WriteLog("start of audio detected");
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
		int pid=DR_FAKE_PCR_PID;
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
	} catch (...) { WriteLog("Exception in WriteTs");}
}

void CDiskRecorder::WriteFakePAT()
{
	int tableId=TABLE_ID_PAT;
	int transportId=DR_FAKE_TRANSPORT_ID;
	int pmtPid=DR_FAKE_PMT_PID;
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
	pat[13]=(DR_FAKE_SERVICE_ID>>8)&0xff;
	pat[14]=(DR_FAKE_SERVICE_ID)&0xff;
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

void CDiskRecorder::WriteFakePMT()
{
	int program_info_length=0;
	int sectionLenght=9+2*5+5;

	int current_next_indicator=1;
	int section_number = 0;
	int last_section_number = 0;
	int transportId=DR_FAKE_TRANSPORT_ID;

	int tableId=2;
	int pid=DR_FAKE_PMT_PID;
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
	pmt[8]=(DR_FAKE_SERVICE_ID>>8)&0xff;
	pmt[9]=(DR_FAKE_SERVICE_ID)&0xff;
	pmt[10]=((m_iPmtVersion&0x1f)<<1)+current_next_indicator;
	pmt[11]=section_number;
	pmt[12]=last_section_number;
	pmt[13]=(DR_FAKE_PCR_PID>>8)&0xff;
	pmt[14]=(DR_FAKE_PCR_PID)&0xff;
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

	//if(pmtLength > 188) WriteLog("ERROR: Pmt length : %i ( >188 )!!!!",pmtLength);

	Write(pmt,188);
	if (pmtLength>188)
	{
		int newLength=pmtLength-188;
		byte packet[188];
		memset(packet,0xff,188);
		packet[0]=0x47;
		packet[1]=(pid>>8) & 0x1f;
		packet[2]=(pid&0xff);
		m_iPmtContinuityCounter++;
		if (m_iPmtContinuityCounter>0xf) m_iPmtContinuityCounter=0;
		packet[3]=(AdaptionControl<<4) +m_iPmtContinuityCounter;
		memcpy(&packet[4],&pmt[188],newLength);
		Write(packet,188);
	}
}


void CDiskRecorder::PatchPcr(byte* tsPacket,CTsHeader& header)
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
			WriteLog("Pcr start    :%s",m_startPcr.ToString());
			WriteLog("Pcr high     :%s",m_highestPcr.ToString());
			WriteLog("Pcr duration :%s",duration.ToString());
			WriteLog("Pcr current  :%s",pcrNew.ToString());
			WriteLog("Pcr newstart :%s",newStartPcr.ToString());

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
		WriteLog("Pcr new start pcr :%s - pid:%x", m_startPcr.ToString() , header.Pid);
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
    WriteLog( "not allowed to patch PCRs yet - m_bDetermineNewStartPcr = true" );
  }

	// PCR value has jumped too much in the stream
  if (diff.ToClock() > DR_MAX_ALLOWED_PCR_DIFF && m_prevPcr.ToClock() > 0 && !m_bDetermineNewStartPcr )
	{
    logNextPcr = true;
    if (m_bIgnoreNextPcrJump)
		{
			WriteLog("Ignoring first PCR jump after channel change" );
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

				WriteLog( "PCR rollover detected" );
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
					WriteLog( "Jump forward in PCR detected" );
				}
				else
				{
					m_backwardsPcrHole += diff;
					m_backwardsPcrHole += step;
          WriteLog( "Jump backward in PCR detected" );
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
    WriteLog("PCR: %s new: %s prev: %s start: %s diff: %s hole: %s holeB: %s  - pid:%x", pcrHi.ToString(), pcrNew.ToString(), m_prevPcr.ToString(), m_startPcr.ToString(), diff.ToString(), m_pcrHole.ToString(), m_backwardsPcrHole.ToString(), header.Pid );
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

void CDiskRecorder::PatchPtsDts(byte* tsPacket,CTsHeader& header,CPcr& startPcr)
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
			if (diff>DR_MAX_ALLOWED_PCR_DIFF && m_pcrHole.IsValid)
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
				if (diff>DR_MAX_ALLOWED_PCR_DIFF && m_pcrHole.IsValid)
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
	}
}

