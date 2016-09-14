/**
*  FileWriterThreaded.h
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

#ifndef FILEWRITERTHREADED
#define FILEWRITERTHREADED
#include <vector>

//Variable size buffers are used - CDiskBuffWT::CDiskBuffWT(int size)
#define FULL_BUFFERS 256
#define NOT_FULL_BUFFERS 192

//Number of retries allowed when writing buffer files
#define FILE_WRITE_RETRIES 19

//System timer resolution in ms
#define SYS_TIMER_RES 5

class CDiskBuffWT
{
public:
  CDiskBuffWT(int size);
  ~CDiskBuffWT(void);
  int    Length();
  byte*  Data();
  int    Add(byte* data, int len);

private:
  byte* m_pBuffer;
  int   m_iLength;
  int   m_iSize;
};


class FileWriterThreaded
{
public:

	FileWriterThreaded();
	virtual ~FileWriterThreaded();

	HRESULT GetFileName(LPWSTR *lpszFileName);
	HRESULT SetFileName(LPCWSTR pszFileName);
  
	HRESULT AddToBuffer(byte* pbData, int len, int newBuffSize);
	HRESULT DiscardBuffer();


protected:
	HANDLE m_hFile;
	LPWSTR m_pFileName;
	
	int    m_iPart;

	BOOL m_bWriteFailed;
	CDiskBuffWT* m_pDiskBuffer;
	
	UINT m_maxBuffersUsed;
	BOOL m_bDiskFull;
	BOOL m_bBufferFull;
	
	HRESULT PushBuffer();
	HRESULT NewBuffer(int size);
	void ClearBuffers();
	HRESULT OpenFile();
	HRESULT CloseFile();
  HRESULT WriteWithRetry(PBYTE pbData, ULONG lDataLength, int retries);
	DWORD SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
	__int64 GetFilePointer();

	CCritSec m_Lock;
	CCritSec m_qLock;	
	
  std::vector<CDiskBuffWT*> m_writeQueue;
  typedef std::vector<CDiskBuffWT*>::iterator ivecDiskBuff;

  BOOL m_bThreadRunning;
  HANDLE m_hThreadProc;
  CAMEvent m_WakeThreadEvent;

	static unsigned __stdcall thread_function(void* p);
  unsigned __stdcall ThreadProc();
  void StartThread();
  void StopThread();
  DWORD m_dwTimerResolution;   //Timer resolution variable
};

#endif
