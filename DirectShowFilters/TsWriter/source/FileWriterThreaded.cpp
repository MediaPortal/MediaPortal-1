// Copyright (C) 2006-2015 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

/**
*  FileWriterThreaded.cpp
*  Copyright (C) 2006-2007      nate
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#define _WIN32_WINNT 0x0502

#pragma warning(disable : 4995)
#include <streams.h>
#include "FileWriterThreaded.h"
#include <atlbase.h>

extern void LogDebug(const char *fmt, ...) ;
extern void LogDebug(const wchar_t *fmt, ...) ;


///*******************************************
/// FileWriter code 
///

FileWriterThreaded::FileWriterThreaded() :
  m_hFile(INVALID_HANDLE_VALUE),
  m_pFileName(NULL),
	m_hThreadProc(NULL),
  m_iPart(2),
  m_maxBuffersUsed(0),
  m_pDiskBuffer(NULL),
  m_bDiskFull(FALSE),
  m_bBufferFull(FALSE),
  m_bWriteFailed(FALSE),
  m_totalBuffers(0),
  m_totalWakes(0)
{
}

FileWriterThreaded::~FileWriterThreaded()
{
  if (m_pFileName != NULL)
  {
    Close();  
    LogDebug("FileWriterThreaded::Dtor() before Close()");
  }  
}

HRESULT FileWriterThreaded::Open(LPCWSTR pszFileName)
{  
  { //Context for CAutoLock
    CAutoLock lock(&m_Lock);
  
    // Are we already open?
    if (m_pFileName != NULL)
    {
      return E_FAIL;
    }
  
    // Is this a valid filename supplied
    CheckPointer(pszFileName,E_POINTER);
  
    long length = wcslen(pszFileName);
  
    if(length > MAX_PATH)
      return ERROR_FILENAME_EXCED_RANGE;
  
    // Take a copy of the filename
    m_pFileName = new wchar_t[length+1];
    if (m_pFileName == NULL)
      return E_OUTOFMEMORY;
  
    wcscpy(m_pFileName,pszFileName);
  
  	if (FAILED(StartThread()))
  	{
  		delete[] m_pFileName;
  		m_pFileName = NULL;
      return E_FAIL;
  	}
  }

  m_WakeThreadEvent.Set(); //Trigger thread to open file

  m_totalBuffers = 0;
  m_totalWakes = 0;

  return S_OK;
}

//
// OpenFile
//
HRESULT FileWriterThreaded::OpenFile()
{
  // Is the file already opened
  if (m_hFile != INVALID_HANDLE_VALUE)
  {
    return E_FAIL;
  }

  // Has a filename been set yet?
  if (m_pFileName == NULL)
  {
    return ERROR_INVALID_NAME;
  }

  // Check if the file is being read by another process.
  // (which should result in a 'sharing violation' error)
  m_hFile = CreateFileW(m_pFileName,          // The filename
             (DWORD) GENERIC_WRITE,           // File access
             (DWORD) NULL,                    // Share access
             NULL,                            // Security
             (DWORD) OPEN_ALWAYS,             // Open flags
             (DWORD) 0,                       // More flags
             NULL);                           // Template
  if (m_hFile == INVALID_HANDLE_VALUE)
  {
    DWORD dwErr = GetLastError();
    return HRESULT_FROM_WIN32(dwErr);
  }
  CloseHandle(m_hFile);

  ::DeleteFileW((LPCWSTR) m_pFileName);

  // Try to open the file in normal mode
  m_hFile = CreateFileW(m_pFileName,           // The filename
             (DWORD) GENERIC_WRITE,            // File access
             (DWORD) FILE_SHARE_READ,          // Share access
             NULL,                             // Security
             (DWORD) OPEN_ALWAYS,              // Open flags
             (DWORD) FILE_ATTRIBUTE_NORMAL,    // More flags
             NULL);                            // Template

  if (m_hFile == INVALID_HANDLE_VALUE)
  {
    DWORD dwErr = GetLastError();
    return HRESULT_FROM_WIN32(dwErr);
  }

  SetFilePointer(0, FILE_END);
  SetFilePointer(0, FILE_BEGIN);
  
  m_bWriteFailed = FALSE;
  m_maxBuffersUsed = 0;
  m_bDiskFull = FALSE;
  m_bBufferFull = FALSE; 
  m_iPart=2;

  LogDebug(L"FileWriterThreaded: OpenFile() succeeded, filename: %s", m_pFileName);

  return S_OK;
}

////
//// Close
////

HRESULT FileWriterThreaded::Close()
{  
  CAutoLock lock(&m_Lock);

  PushBuffer(); //Force temp buffer onto queue

  //Wait for all buffers to be written to disk
  m_WakeThreadEvent.Set();
  for (;;)
  { 
    if (m_bDiskFull || (m_hFile == INVALID_HANDLE_VALUE) || !m_hThreadProc)
    {
      //If we can't flush the buffers to disk, then just discard the data
      ClearBuffers();
    }
    
    {//Context for CAutoLock
      CAutoLock lock(&m_qLock);
      if (m_writeQueue.size() == 0)
      {
        break;
      }
    }
    Sleep(1);
  }

  StopThread();  
  ClearBuffers();

  CloseHandle(m_hFile);
  m_hFile = INVALID_HANDLE_VALUE;
  
  if (m_pFileName != NULL)
  {
    LogDebug(L"FileWriterThreaded: Close(), filename: %s, MaxBuff: %d, totBuff: %d, totWake: %d", m_pFileName, m_maxBuffersUsed, m_totalBuffers, m_totalWakes);
    delete m_pFileName;
    m_pFileName = NULL;
  }

  return S_OK;
}

DWORD FileWriterThreaded::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
  LARGE_INTEGER li;
  li.QuadPart = llDistanceToMove;
  return ::SetFilePointer(m_hFile, li.LowPart, &li.HighPart, dwMoveMethod);
}

__int64 FileWriterThreaded::GetFilePointer()
{
  LARGE_INTEGER li;
  li.QuadPart = 0;
  li.LowPart = ::SetFilePointer(m_hFile, 0, &li.HighPart, FILE_CURRENT);
  return li.QuadPart;
}

HRESULT FileWriterThreaded::WriteWithRetry(PBYTE pbData, ULONG lDataLength, int retries)
{
  HRESULT hr;
  
  // Is file open yet
  if (m_hFile == INVALID_HANDLE_VALUE)
  {
    return S_FALSE;
  }

  __int64 currentPosition = GetFilePointer();

  DWORD written = 0;
  
  for (int retryCnt = 0; retryCnt <= retries; retryCnt++)
  {
    written = 0;
    hr = WriteFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, &written, NULL);
    
    if (hr == FALSE) //WriteFile() failed
    {
      DWORD dwErr = GetLastError();
      if (ERROR_DISK_FULL == dwErr || ERROR_HANDLE_DISK_FULL == dwErr)
      {
        m_bDiskFull = TRUE;
        return S_FALSE;
      }

      //On fat16/fat32 we can only create files of max. 2gb/4gb
      if (ERROR_FILE_TOO_LARGE == dwErr)
      {
        LogDebug(L"FileWriterThreaded:Maximum filesize reached for file: '%s' %d", m_pFileName);
        //close the file...
        CloseHandle(m_hFile);
        m_hFile=INVALID_HANDLE_VALUE;
    
        //create a new file
        wchar_t ext[MAX_PATH];
        wchar_t fileName[MAX_PATH];
        wchar_t part[100];
        int len=wcslen(m_pFileName)-1;
        int pos=len-1;
        while (pos>0)
        {
          if (m_pFileName[pos]==L'.') break;
          pos--;
        }
        wcscpy(ext, &m_pFileName[pos]);
        wcsncpy(fileName, m_pFileName, pos);
        fileName[pos]=0;
        swprintf_s(part,L"_p%d",m_iPart);
        wchar_t newFileName[MAX_PATH];
        swprintf_s(newFileName,L"%s%s%s",fileName,part,ext);
        LogDebug(L"FileWriterThreaded:Create new file part:'%s' %d",newFileName);
        m_hFile = CreateFileW(newFileName,              // The filename
                    (DWORD) GENERIC_WRITE,              // File access
                    (DWORD) FILE_SHARE_READ,            // Share access
                    NULL,                               // Security
                    (DWORD) OPEN_ALWAYS,                // Open flags
                    (DWORD) FILE_ATTRIBUTE_NORMAL,      // More flags
                    NULL);                              // Template
        if (m_hFile == INVALID_HANDLE_VALUE)
        {
          LogDebug(L"FileWriterThreaded:unable to create new file part:'%s' %d",newFileName, GetLastError());
          return S_FALSE;
        }
        m_iPart++;
        hr = WriteFile(m_hFile, (PVOID)pbData, (DWORD)lDataLength, &written, NULL);
      }//end: if (ERROR_FILE_TOO_LARGE == GetLastError())
    }        
    
    if (hr && (written == (DWORD)lDataLength))
    {
      if (retryCnt > 0)
      {
        LogDebug(L"FileWriterThreaded: Write() retry, file %s: retries %d", m_pFileName, retryCnt);
      }
      m_bWriteFailed = FALSE;
      m_bDiskFull = FALSE;
      return S_OK;
    }
    else if (retries == 0)
    {
      break;
    }
    
    Sleep(50);
    DWORD dwPtr = SetFilePointer(currentPosition, FILE_BEGIN);
    if (dwPtr == INVALID_SET_FILE_POINTER)
    {
      LogDebug(L"FileWriterThreaded: Write() retry, SetFilePointer() error, file %s: pointer %d, hr: %d", m_pFileName, currentPosition, dwPtr);       
    }   
  }

  //Failed to write after retries
  if (!m_bWriteFailed) //Only log the first failure in a series...
  {
    LogDebug(L"FileWriterThreaded: Error writing to file %s: written %d of expected %d bytes, hr: %d", m_pFileName, written, lDataLength, hr);
  }    
  m_bWriteFailed = TRUE;
  
  return S_FALSE;
}


void FileWriterThreaded::ClearBuffers()
{
  CAutoLock qlock(&m_qLock);
  if (m_writeQueue.size()>0)
  {
    ivecDiskBuff it = m_writeQueue.begin();
    for ( ; it != m_writeQueue.end() ; it++ )
    {
      CDiskBuff* diskBuffer = *it;
      delete diskBuffer;
    }
    m_writeQueue.clear();
  }
  m_bBufferFull = FALSE;
}


////**************************************
//// Temporary Disk Buffer methods
////

HRESULT FileWriterThreaded::NewBuffer(int size)
{
  if (m_pDiskBuffer != NULL) return S_FALSE;
  	
  try 
  {
    m_pDiskBuffer = new CDiskBuff(size);    
  }
  catch(...)
  {
    m_pDiskBuffer = NULL;
    LogDebug("FileWriterThreaded::NewBuffer() buffer allocation exception, size = %d", size);
    return E_FAIL;
  }
  return S_OK;
}

HRESULT FileWriterThreaded::AddToBuffer(byte* pbData, int len, int newBuffSize)
{
  CAutoLock lock(&m_Lock);

  if (m_pDiskBuffer == NULL)
  {
    if (NewBuffer(newBuffSize) != S_OK)
    {
      return E_FAIL;    
    }
  }   
    
	if (m_pDiskBuffer->Add(pbData,len) > 0)
	{
	  //Not enough space to add data to current buffer
    PushBuffer(); //Push the current buffer onto the disk write queue
    if (NewBuffer(newBuffSize) != S_OK)
    {
      return E_FAIL;    
    }
  	if (m_pDiskBuffer->Add(pbData,len) != 0)
  	{
      return E_FAIL;
 	  }
    return S_FALSE; //We have rolled over into a new buffer
	}
	
  return S_OK;
}

HRESULT FileWriterThreaded::PushBuffer()
{
  // Has a filename been set yet?
  if (m_pFileName == NULL)
  {
		DiscardBuffer();
    return S_FALSE;
  }
  
  if (m_pDiskBuffer == NULL) return S_FALSE;
  UINT qsize;
  { //Context for CAutoLock
    CAutoLock lock(&m_qLock);
    qsize = m_writeQueue.size();
    m_writeQueue.push_back(m_pDiskBuffer);
    m_pDiskBuffer = NULL;   
  }	  
  if (qsize >= 2) //There is too much 'old' data in the buffer, so wake the thread (polling not frequent enough)
  {              
    m_WakeThreadEvent.Set(); 
    m_totalWakes++;   
  } 
  m_totalBuffers++;   
	return S_OK;
}

HRESULT FileWriterThreaded::DiscardBuffer()
{
  CAutoLock lock(&m_Lock);
  if (m_pDiskBuffer != NULL)
  {
    delete m_pDiskBuffer;
    m_pDiskBuffer = NULL;
  }
	return S_OK;
}

////**************************************
//// Write Thread methods
////

unsigned FileWriterThreaded::thread_function(void* p)
{
  FileWriterThreaded *thread = reinterpret_cast<FileWriterThreaded *>(p);
  thread->ThreadProc();
  _endthreadex(0);
  return 0;
}

unsigned __stdcall FileWriterThreaded::ThreadProc()
{
  //LogDebug("FileWriterThreaded::ThreadProc() started");
  CDiskBuff* diskBuffer = NULL;
  UINT qsize = 0;
  
  while (m_bThreadRunning)
  {    
    if (m_hFile == INVALID_HANDLE_VALUE) //Open the file
    {
      // Has a filename been set yet
      if (m_pFileName != NULL)
      {
        OpenFile();
      }
    }

    { //Context for CAutoLock
      CAutoLock lock(&m_qLock);
      qsize = m_writeQueue.size();
      if (qsize > 0) 
      {              
        //get the next buffer
        ivecDiskBuff it = m_writeQueue.begin();
        diskBuffer=*it;
        m_writeQueue.erase(it);
      }
      else
      {
        diskBuffer = NULL;   
      }
      
      if (qsize < NOT_FULL_BUFFERS)
      {
        m_bBufferFull = FALSE;
      }
    }
    
    if (diskBuffer != NULL)
    {
      WriteWithRetry(diskBuffer->Data(), diskBuffer->Length(), FILE_WRITE_RETRIES);  
      delete diskBuffer;
      diskBuffer = NULL;
    }

    if (qsize > m_maxBuffersUsed) 
    {     
      m_maxBuffersUsed = qsize;
      //LogDebug("FileWriterThreaded::ThreadProc(), Max buffers used = %d", m_maxBuffersUsed);
    }
    
    if (qsize < 2) //this is the pre 'pop' qsize value
    {
      //Sleep for 100ms, unless thread gets an event
      m_WakeThreadEvent.Wait(100);
    }
    //else there are more buffers to process, so go round again
  }
  
  if (diskBuffer != NULL)
  {
    delete diskBuffer;
  }
  //LogDebug("FileWriterThreaded::ThreadProc() finished");
  return 0;
}


HRESULT FileWriterThreaded::StartThread()
{
  m_bThreadRunning = TRUE;
  UINT id;
  m_hThreadProc = (HANDLE)_beginthreadex(NULL, 0, &FileWriterThreaded::thread_function, (void *) this, 0, &id);
  if (!m_hThreadProc)
  {
    return E_FAIL;
  }
  SetThreadPriority(m_hThreadProc, THREAD_PRIORITY_BELOW_NORMAL);
  
  // Set timer resolution to SYS_TIMER_RES (if possible)
  TIMECAPS tc; 
  m_dwTimerResolution = 0; 
  if (timeGetDevCaps(&tc, sizeof(TIMECAPS)) == MMSYSERR_NOERROR)
  {
    m_dwTimerResolution = min(max(tc.wPeriodMin, SYS_TIMER_RES), tc.wPeriodMax);
    if (m_dwTimerResolution)
    {
      timeBeginPeriod(m_dwTimerResolution);
    }
  }
  
  return S_OK;
}


void FileWriterThreaded::StopThread()
{
  if (m_hThreadProc)
  {
    //Make sure the thread runs soon so it can finish processing
    SetThreadPriority(m_hThreadProc, THREAD_PRIORITY_NORMAL);
    m_bThreadRunning = FALSE;
    m_WakeThreadEvent.Set();
    WaitForSingleObject(m_hThreadProc, INFINITE); 
    m_WakeThreadEvent.Reset();
    CloseHandle(m_hThreadProc);
    m_hThreadProc = NULL;
  }
  
  // Reset timer resolution (if we managed to set it originally)
  if (m_dwTimerResolution)
  {
    timeEndPeriod(m_dwTimerResolution);
    m_dwTimerResolution = 0;
  }
}

