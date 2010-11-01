#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "MemoryBuffer.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define MAX_MEMORY_BUFFER_SIZE (1024L*1024L*12L)

extern void LogDebug(const char *fmt, ...) ;

CMemoryBuffer::CMemoryBuffer(void)
:m_event(NULL,FALSE,FALSE,NULL)
{
  LogDebug("CMemoryBuffer::ctor");
  m_bRunning=true;
  m_BytesInBuffer=0;
  m_pcallback=NULL;
}

CMemoryBuffer::~CMemoryBuffer()
{
  LogDebug("CMemoryBuffer::dtor");
  Clear();
}

bool CMemoryBuffer::IsRunning()
{
  return m_bRunning;
}
void CMemoryBuffer::Clear()
{
  LogDebug("memorybuffer: Clear() %d",m_Array.size());
	CAutoLock BufferLock(&m_BufferLock);
	std::vector<BUFFERITEM *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
    BUFFERITEM *item = *it;
    delete[] item->data;
		delete item;
	}
	m_Array.clear();
  m_BytesInBuffer=0;
	LogDebug("memorybuffer: Clear() done");
}

DWORD CMemoryBuffer::Size()
{
  return m_BytesInBuffer;
}
void CMemoryBuffer::Run(bool onOff)
{
	LogDebug("memorybuffer: run:%d %d", onOff, m_bRunning);
  if (m_bRunning!=onOff)
  {
    m_bRunning=onOff;
	  if (m_bRunning==false) 
	  {
		  Clear();
	  }
  }
	LogDebug("memorybuffer: running:%d", onOff);
}

DWORD CMemoryBuffer::ReadFromBuffer(BYTE *pbData, long lDataLength)
{	
	if (pbData==NULL) return 0;
	if (lDataLength<=0) return 0;
  if (!m_bRunning) return 0;
  while (m_BytesInBuffer < lDataLength)
  {	
    if (!m_bRunning) return 0;
    m_event.ResetEvent();
    m_event.Wait();
    if (!m_bRunning) return 0;
  }
		
	//Log("get..%d/%d",lDataLength,m_BytesInBuffer);
  long bytesWritten = 0;
	CAutoLock BufferLock(&m_BufferLock);
	while (bytesWritten < lDataLength)
	{
		if(!m_Array.size() || m_Array.size() <= 0)
    {
			LogDebug("memorybuffer: read:empty buffer\n");
			return 0;
    }
		BUFFERITEM *item = m_Array.at(0);
    
		long copyLength = min(item->nDataLength - item->nOffset, lDataLength-bytesWritten);
		memcpy(&pbData[bytesWritten], &item->data[item->nOffset], copyLength);

		bytesWritten += copyLength;
		item->nOffset += copyLength;
    m_BytesInBuffer-=copyLength;

		if (item->nOffset >= item->nDataLength)
		{
			m_Array.erase(m_Array.begin());
      delete[] item->data;
			delete item;
		}
	}
	return bytesWritten;
}

HRESULT CMemoryBuffer::PutBuffer(BYTE *pbData, long lDataLength)
{
  if (lDataLength<=0) return E_FAIL;
  if (pbData==NULL) return E_FAIL;

  BUFFERITEM* item = new BUFFERITEM();
  item->nOffset=0;
  item->nDataLength=lDataLength;
  item->data = new byte[lDataLength];
  memcpy(item->data, pbData, lDataLength);
  bool sleep=false;
  {
	  CAutoLock BufferLock(&m_BufferLock);
    m_Array.push_back(item);
    m_BytesInBuffer+=lDataLength;

	  //Log("add..%d/%d",lDataLength,m_BytesInBuffer);
    while (m_BytesInBuffer > MAX_MEMORY_BUFFER_SIZE)
    {
      sleep=true;
		  LogDebug("memorybuffer:put full buffer (%d)",m_BytesInBuffer);
		  BUFFERITEM *item = m_Array.at(0);
      int copyLength=item->nDataLength - item->nOffset;

      m_BytesInBuffer-=copyLength;
		  m_Array.erase(m_Array.begin());
      delete[] item->data;
		  delete item;
    }
    if (m_BytesInBuffer>0)
    {
      m_event.SetEvent();
    }
  }
  if (m_pcallback)
  {
    m_pcallback->OnRawDataReceived(pbData,lDataLength);
  }
  if (sleep)
  {
    Sleep(10);
  }
	return S_OK;
}


void CMemoryBuffer::SetCallback(IMemoryCallback* callback)
{
  m_pcallback=callback;
}