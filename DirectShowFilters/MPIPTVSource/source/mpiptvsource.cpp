/* 
*	Copyright (C) 2006-2009 Team MediaPortal
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

// mpiptvsource.cpp : Defines the exported functions for the DLL application.
//
#pragma warning(disable : 4996)
#include <windows.h>
#include <stdio.h>
#include <shlobj.h>
#include "mpiptvsource.h"

//#define logging

#ifdef logging
static char logFile[MAX_PATH];
static WORD logFileParsed = -1;

void GetLogFile(char *pLog)
{
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  if(logFileParsed != systemTime.wDay)
  {
    TCHAR folder[MAX_PATH];
    ::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
    sprintf(logFile,"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\MPIPTVSource-%04.4d-%02.2d-%02.2d.Log",folder, systemTime.wYear, systemTime.wMonth, systemTime.wDay);
    logFileParsed=systemTime.wDay; // rec
  }
  strcpy(pLog, &logFile[0]);
}

void LogDebug(const char *fmt, ...)
{
  va_list ap;
  va_start(ap,fmt);

  char buffer[1000];
  int tmp;
  va_start(ap,fmt);
  tmp=vsprintf(buffer, fmt, ap);
  va_end(ap);
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);

  TCHAR filename[1024];
  GetLogFile(filename);
  FILE* fp = fopen(filename,"a+");

  if (fp!=NULL)
  {
    fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%x]%s\n",
      systemTime.wDay, systemTime.wMonth, systemTime.wYear,
      systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
      systemTime.wMilliseconds,
      GetCurrentThreadId(),
      buffer);
    fclose(fp);
  }
  char buf[1000];
  sprintf(buf,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
    systemTime.wDay, systemTime.wMonth, systemTime.wYear,
    systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
    buffer);
  ::OutputDebugString(buf);
};
#endif

CMPIptvSourceStream::CMPIptvSourceStream(HRESULT *phr, CSource *pFilter)
  : CSourceStream(NAME("MediaPortal IPTV Stream"), phr, pFilter, L"Out"),
  ip(NULL),
  localip(NULL),
  protocol(NULL),
  port(0) ,
  m_socket(-1),
  m_seqNumber(0),
  m_buffsize(0)
{
}

CMPIptvSourceStream::~CMPIptvSourceStream()
{
  Clear();
}

void CMPIptvSourceStream::Clear() 
{
  if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
  if(CAMThread::ThreadExists())
  {
    CAMThread::CallWorker(CMD_EXIT);
    CAMThread::Close();
  }
  if (ip) {
    CoTaskMemFree(ip);
    ip = NULL;
  }
  if (localip) {
    CoTaskMemFree(localip);
    localip = NULL;
  }
  if (protocol) {
    CoTaskMemFree(protocol);
    protocol = NULL;
  }
}

HRESULT CMPIptvSourceStream::FillBuffer(IMediaSample *pSamp)
{
#ifndef FILL_DIRECTLY_INTO_BUFFER
  BYTE *pData;
  long cbData;
#endif

  CheckPointer(pSamp, E_POINTER);
#ifndef FILL_DIRECTLY_INTO_BUFFER
  CAutoLock cAutoLock(m_pFilter->pStateLock());
// Access the sample's data buffer
  pSamp->GetPointer(&pData);
  cbData = pSamp->GetSize();

  memcpy(pData, m_buffer, min(m_buffsize, cbData));
#endif
#ifdef logging
#ifdef FILL_DIRECTLY_INTO_BUFFER
  LogDebug("Posting %d / %d bytes at seq # %d", m_buffsize, pSamp->GetSize(), m_seqNumber);
#else
  LogDebug("Posting %d / %d bytes at seq # %d", m_buffsize, cbData, m_seqNumber);
#endif
#endif

  REFERENCE_TIME rtStart = m_seqNumber;
  REFERENCE_TIME rtStop  = ++m_seqNumber;

  pSamp->SetTime(&rtStart, &rtStop);
#ifdef FILL_DIRECTLY_INTO_BUFFER
  pSamp->SetActualDataLength(m_buffsize);
  m_buffsize = 0; //We successfully posted the buffer so it can be cleared
#else
  pSamp->SetActualDataLength(min(m_buffsize, cbData));
  if (cbData < m_buffsize)
  { //The buffer was posted only partially, so we need to retain the remaining data
    memmove(m_buffer, m_buffer + cbData, m_buffsize - cbData);
    m_buffsize -= cbData;
  }
  else
    m_buffsize = 0; //We successfully posted the buffer so it can be cleared
#endif

  pSamp->SetSyncPoint(TRUE);

  return S_OK;
}

HRESULT CMPIptvSourceStream::GetMediaType(__inout CMediaType *pMediaType) 
{
  pMediaType->majortype = MEDIATYPE_Stream;
  pMediaType->subtype = MEDIASUBTYPE_MPEG2_TRANSPORT;

  return S_OK;
}

HRESULT CMPIptvSourceStream::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest) 
{
  HRESULT hr;
  CAutoLock cAutoLock(m_pFilter->pStateLock());

  CheckPointer(pAlloc, E_POINTER);
  CheckPointer(pRequest, E_POINTER);

  // If the bitmap file was not loaded, just fail here.

  // Ensure a minimum number of buffers
  if (pRequest->cBuffers == 0)
  {
    pRequest->cBuffers = 1;
  }
  pRequest->cbBuffer = IPTV_BUFFER_SIZE;

  ALLOCATOR_PROPERTIES Actual;
  hr = pAlloc->SetProperties(pRequest, &Actual);
  if (FAILED(hr)) 
  {
    return hr;
  }

  // Is this allocator unsuitable?
  if (Actual.cbBuffer < pRequest->cbBuffer) 
  {
    return E_FAIL;
  }

  return S_OK;
}

HRESULT CMPIptvSourceStream::DoBufferProcessingLoop(void) 
{
  Command com;

  OnThreadStartPlay();

  WSADATA wsaData;
  WSAStartup(MAKEWORD(2, 2), &wsaData);

#ifdef logging
  LogDebug("Starting grabber thread");
#endif
  sockaddr_in addr;
  memset(&addr, 0, sizeof(addr));
  addr.sin_family = AF_INET;
  if (localip) {
    addr.sin_addr.s_addr = inet_addr(localip);
  } else {
    addr.sin_addr.s_addr = htonl(INADDR_ANY);
  }
  addr.sin_port = htons((u_short)port);

  ip_mreq imr; 
  imr.imr_multiaddr.s_addr = inet_addr(ip);
  if (localip) {
    imr.imr_interface.s_addr = inet_addr(localip);
  } else {
    imr.imr_interface.s_addr = INADDR_ANY;
  }
  unsigned long nonblocking = 1;

  if((m_socket = socket(AF_INET, SOCK_DGRAM, 0)) >= 0)
  {
    /*		u_long argp = 1;
    ioctlsocket(m_socket, FIONBIO, &argp);
    */
    DWORD dw = TRUE;
    int dwLen = sizeof(dw);
    if(setsockopt(m_socket, SOL_SOCKET, SO_REUSEADDR, (const char*)&dw, sizeof(dw)) < 0)
    {
      closesocket(m_socket);
      m_socket = -1;
    }

    if(setsockopt(m_socket, SOL_SOCKET, SO_BROADCAST, (const char*)&dw, sizeof(dw)) < 0)
    {
      closesocket(m_socket);
      m_socket = -1;
    }

    getsockopt(m_socket, SOL_SOCKET, SO_RCVBUF, (char *)&dw, &dwLen);
#ifdef logging
    LogDebug("Socket receive buffer is: %d (%d)", dw, dwLen);

    LogDebug("Trying to set receive buffer to %d", IPTV_SOCKET_BUFFER_SIZE);
#endif
    dw = IPTV_SOCKET_BUFFER_SIZE;
    if(setsockopt(m_socket, SOL_SOCKET, SO_RCVBUF, (const char*)&dw, sizeof(dw)) < 0)
    {
      closesocket(m_socket);
      m_socket = -1;
    }

    dwLen = sizeof(dw);
    getsockopt(m_socket, SOL_SOCKET, SO_RCVBUF, (char *)&dw, &dwLen);
#ifdef logging
    LogDebug("New socket receive buffer is: %d (%d)", dw, dwLen);
#endif
    if (ioctlsocket(m_socket, FIONBIO, &nonblocking) != 0) {
      closesocket(m_socket);
      m_socket = -1;
    }

    if(bind(m_socket, (struct sockaddr*)&addr, sizeof(addr)) < 0)
    {
      closesocket(m_socket);
      m_socket = -1;
    }

    if(IN_MULTICAST(htonl(imr.imr_multiaddr.s_addr)))
    {
      int ret = setsockopt(m_socket, IPPROTO_IP, IP_ADD_MEMBERSHIP, (const char*)&imr, sizeof(imr));
      if(ret < 0) ret = ::WSAGetLastError();
      ret = ret;
    }
  }

  SetThreadPriority(m_hThread, THREAD_PRIORITY_TIME_CRITICAL);

  int fromlen = sizeof(addr);

  m_buffsize = 0;
  timeval tv; //Will be used for select() below
  tv.tv_sec = 0;
  tv.tv_usec = 100000; //100 msec
  do 
  {
    BOOL requestAvail;
    while ((requestAvail = CheckRequest(&com)) == FALSE) 
    {
      DWORD startRecvTime;
      startRecvTime = GetTickCount();
#ifdef FILL_DIRECTLY_INTO_BUFFER
      IMediaSample *pSample;
      char *pData;
      long cbData;

      HRESULT hr = GetDeliveryBuffer(&pSample,NULL,NULL,0);
      if (FAILED(hr))
        continue;	
      CheckPointer(pSample, E_POINTER);
    // Access the sample's data buffer
      pSample->GetPointer((BYTE **)&pData);
      cbData = pSample->GetSize();
#endif
      do 
      {
        //Try to read the complete remaining buffer size
        //But stop reading after 100ms have passed (slow streams like internet radio)
#ifdef FILL_DIRECTLY_INTO_BUFFER
        int len = recvfrom(m_socket, &pData[m_buffsize], cbData - m_buffsize, 0, (SOCKADDR*)&addr, &fromlen);
#else
        int len = recvfrom(m_socket, &m_buffer[m_buffsize], IPTV_BUFFER_SIZE - m_buffsize, 0, (SOCKADDR*)&addr, &fromlen);
#endif
        if(len <= 0)
        {
          //Wait until there's something in the receive buffer
          fd_set myFDsocket;
          myFDsocket.fd_count = 1;
          myFDsocket.fd_array[0] = m_socket;
          int selectRet = select(0, &myFDsocket, NULL, NULL, &tv);
#ifdef logging
          LogDebug("select return code: %d", selectRet);
#endif
          continue; //On error or nothing read just repeat the loop
        }
#ifdef logging
        LogDebug("Read %d bytes at pos %d of %d", len, m_buffsize, IPTV_BUFFER_SIZE); 
#endif
        m_buffsize += len;
#ifdef FILL_DIRECTLY_INTO_BUFFER
      } while ((requestAvail = CheckRequest(&com)) == FALSE && m_buffsize < (cbData           * 3 / 4) && abs((signed long)(GetTickCount() - startRecvTime)) < 100);
#else
      } while ((requestAvail = CheckRequest(&com)) == FALSE && m_buffsize < (IPTV_BUFFER_SIZE * 3 / 4) && abs((signed long)(GetTickCount() - startRecvTime)) < 100);
#endif
      if (requestAvail) break;
#ifndef FILL_DIRECTLY_INTO_BUFFER
      if (m_buffsize == 0) continue; //100ms passed but no buffer received
      IMediaSample *pSample;
      HRESULT hr = GetDeliveryBuffer(&pSample,NULL,NULL,0);
      if (FAILED(hr))
      {
        continue;	
        // go round again. Perhaps the error will go away
        // or the allocator is decommited & we will be asked to
        // exit soon.
      }
#endif
      // fill buffer
      hr = FillBuffer(pSample);

      if (hr == S_OK) 
      {
        hr = Deliver(pSample);
        pSample->Release();

        // downstream filter returns S_FALSE if it wants us to
        // stop or an error if it's reporting an error.
        if(hr != S_OK)
        {
#ifdef logging
          LogDebug("Deliver() returned %08x; stopping", hr);
#endif
          if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
          WSACleanup();
          return S_OK;
        }

      } else if (hr == S_FALSE) {
        // derived class wants us to stop pushing data
        pSample->Release();
        DeliverEndOfStream();
        if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
        WSACleanup();
        return S_OK;
      } else {
        // derived class encountered an error
        pSample->Release();
#ifdef logging
        LogDebug("Error %08lX from FillBuffer!!!", hr);
#endif
        DeliverEndOfStream();
        m_pFilter->NotifyEvent(EC_ERRORABORT, hr, 0);
        if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
        WSACleanup();
        return hr;
      }

      // all paths release the sample
    }

    // For all commands sent to us there must be a Reply call!

    if (com == CMD_RUN || com == CMD_PAUSE) {
      Reply(NOERROR);
    } else if (com != CMD_STOP) {
      Reply((DWORD) E_UNEXPECTED);
#ifdef logging
      LogDebug("Unexpected command %d!!!", com);
#endif
    }
  } while (com != CMD_STOP);
  if(m_socket >= 0) {closesocket(m_socket); m_socket = -1;}
  WSACleanup();
  return S_FALSE;
}

bool CMPIptvSourceStream::Load(const TCHAR* fn) 
{
  Clear();
  URL_COMPONENTS url;
  url.dwStructSize = sizeof(URL_COMPONENTS);
  url.lpszScheme = NULL;
  url.lpszExtraInfo = NULL;
  url.lpszHostName = NULL;
  url.lpszPassword = NULL;
  url.lpszUrlPath = NULL;
  url.lpszUserName = NULL;
  //	TCHAR *srcurl = "udp://192.168.2.197@233.1.1.1:1234";
  if (!InternetCrackUrl(fn, 0, 0, &url)) {
    return false;
  }
  if (!(ip = (TCHAR*) CoTaskMemAlloc((url.dwHostNameLength + 1) * sizeof(TCHAR)))) {
    return false;
  }
  memset(ip, 0, (url.dwHostNameLength + 1) * sizeof(TCHAR));
  strncat(ip, url.lpszHostName, url.dwHostNameLength);
  if (url.dwUserNameLength > 0) {
    if (!(localip = (TCHAR*) CoTaskMemAlloc((url.dwUserNameLength + 1) * sizeof(TCHAR)))) {
      return false;
    }
    memset(localip, 0, (url.dwUserNameLength + 1) * sizeof(TCHAR));
    strncat(localip, url.lpszUserName, url.dwUserNameLength);
  }
  if (!(protocol = (TCHAR*) CoTaskMemAlloc((url.dwSchemeLength + 1) * sizeof(TCHAR)))) {
    return false;
  }
  memset(protocol, 0, (url.dwSchemeLength + 1) * sizeof(TCHAR));
  strncat(protocol, url.lpszScheme, url.dwSchemeLength);
  port = url.nPort;

  return true;
}

// This is the constructor of a class that has been exported.
// see mpiptvsource.h for the class definition
CMPIptvSource::CMPIptvSource(IUnknown *pUnk, HRESULT *phr)
  : CSource(NAME("MediaPortal IPTV Source"), pUnk, CLSID_MPIptvSource)
{
  // The pin magically adds itself to our pin array.
  m_stream = new CMPIptvSourceStream(phr, this);

  if (phr)
  {
    if (m_stream == NULL)
      *phr = E_OUTOFMEMORY;
    else
      *phr = S_OK;
  } 

  m_fn = EMPTY_STRING;
}


CMPIptvSource::~CMPIptvSource()
{
  delete m_stream;
  if (m_fn != EMPTY_STRING) {
    CoTaskMemFree(m_fn);
    m_fn = EMPTY_STRING;
  }
}

STDMETHODIMP CMPIptvSource::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  CheckPointer(ppv, E_POINTER);

  return 
    QI(IFileSourceFilter)
    __super::NonDelegatingQueryInterface(riid, ppv);
}

STDMETHODIMP CMPIptvSource::Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE* pmt) 
{
  size_t length = wcstombs(NULL, pszFileName, 0);
  if(!(m_fn = (TCHAR*)CoTaskMemAlloc((length+1)*sizeof(TCHAR))))
    return E_OUTOFMEMORY;
  wcstombs(m_fn, pszFileName, length + 1);
  if(!m_stream->Load(m_fn))
    return E_FAIL;

  return S_OK;
}

STDMETHODIMP CMPIptvSource::GetCurFile(LPOLESTR* ppszFileName, AM_MEDIA_TYPE* pmt)
{
  if(!ppszFileName) return E_POINTER;

  if(!(*ppszFileName = (LPOLESTR)CoTaskMemAlloc((strlen(m_fn)+1)*sizeof(WCHAR))))
    return E_OUTOFMEMORY;

  mbstowcs(*ppszFileName, m_fn, strlen(m_fn) + 1);

  return S_OK;
}

CUnknown * WINAPI CMPIptvSource::CreateInstance(IUnknown *pUnk, HRESULT *phr)
{
  CMPIptvSource *pNewFilter = new CMPIptvSource(pUnk, phr );

  if (phr)
  {
    if (pNewFilter == NULL) 
      *phr = E_OUTOFMEMORY;
    else
      *phr = S_OK;
  }

  return pNewFilter;
}
