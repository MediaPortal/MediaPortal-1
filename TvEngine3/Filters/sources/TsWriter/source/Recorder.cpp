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

#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "recorder.h"

#define RECORD_BUFFER_SIZE 256000
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
  m_iPmtPid=-1;
	m_multiPlexer.SetFileWriterCallBack(this);
  
}
CRecorder::~CRecorder(void)
{
  if (m_hFile!=INVALID_HANDLE_VALUE)
  {
	  CloseHandle(m_hFile);
	  m_hFile = INVALID_HANDLE_VALUE; // Invalidate the file
  }
  delete [] m_pWriteBuffer;
}

void CRecorder::OnTsPacket(byte* tsPacket)
{
	if (m_bRecording)
	{
	  m_tsHeader.Decode(tsPacket);
    if (m_tsHeader.SyncByte!=0x47) return;
	  if (m_tsHeader.TransportError) return;
	  CEnterCriticalSection enter(m_section);
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

STDMETHODIMP CRecorder::SetPcrPid(int pcrPid)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("Recorder:pcr pid:%x",pcrPid);
	m_multiPlexer.SetPcrPid(pcrPid);
	return S_OK;
}

STDMETHODIMP CRecorder::SetPmtPid(int pmtPid)
{
	CEnterCriticalSection enter(m_section);
	m_iPmtPid=pmtPid;
	LogDebug("Recorder:pmt pid:%x",m_iPmtPid);
	return S_OK;
}

STDMETHODIMP CRecorder::AddStream(int pid,bool isAudio,bool isVideo)
{
	CEnterCriticalSection enter(m_section);
	if (isAudio)
		LogDebug("Recorder:add audio stream pid:%x",pid);
	else if (isVideo)
		LogDebug("Recorder:add video stream pid:%x",pid);
	else 
		LogDebug("Recorder:add private stream pid:%x",pid);

  m_vecPids.push_back(pid);
	m_multiPlexer.AddPesStream(pid,isAudio,isVideo);
	return S_OK;
}

STDMETHODIMP CRecorder::RemoveStream(int pid)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("Recorder:remove pes stream pid:%x",pid);
	m_multiPlexer.RemovePesStream(pid);
  itvecPids it = m_vecPids.begin();
  while (it!=m_vecPids.end())
  {
    if (*it==pid)
    {
      it=m_vecPids.erase(it);
    }
    else
    {
      ++it;
    }
  }
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


void CRecorder::Write(byte* buffer, int len)
{
	if (!m_bRecording) return;
  if (buffer==NULL) return;
  if (len <=0) return;
	CEnterCriticalSection enter(m_section);
  if (len + m_iWriteBufferPos >= RECORD_BUFFER_SIZE)
  {
	  try
	  {
		  if (m_hFile!=INVALID_HANDLE_VALUE)
		  {
	      DWORD written = 0;
	      WriteFile(m_hFile, (PVOID)m_pWriteBuffer, (DWORD)m_iWriteBufferPos, &written, NULL);
        m_iWriteBufferPos=0;
		  }
	  }
	  catch(...)
	  {
		  LogDebug("Timeshifter:Write exception");
	  }
  }
  memcpy(&m_pWriteBuffer[m_iWriteBufferPos],buffer,len);
  m_iWriteBufferPos+=len;
}

void CRecorder::WriteTs(byte* tsPacket)
{
	if (!m_bRecording) return;
  if (m_tsHeader.Pid==0 ||m_tsHeader.Pid==0x11 || m_tsHeader.Pid==m_multiPlexer.GetPcrPid() || m_tsHeader.Pid==m_iPmtPid)
  {
    //PAT/PCR/PMT/SDT
    Write(tsPacket,188);
    return;
  }

  itvecPids it = m_vecPids.begin();
  while (it!=m_vecPids.end())
  {
    if (m_tsHeader.Pid==*it)
    {
      Write(tsPacket,188);
      return;
    }
    ++it;
  }
}