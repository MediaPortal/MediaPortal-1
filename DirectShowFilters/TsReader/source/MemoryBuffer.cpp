#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "MemoryBuffer.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define MAX_MEMORY_BUFFER_SIZE (1024L*1024L*32L)
#define OVERFLOW_BUFFER_SIZE (MAX_MEMORY_BUFFER_SIZE - (1024L*1024L*4L))

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
	CAutoLock ClearLock(&m_ClearLock);
	CAutoLock BufferLock(&m_BufferLock);
	ivecBuffers it = m_Array.begin();
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

long CMemoryBuffer::Size()
{
  return (m_bRunning ? m_BytesInBuffer : -1);
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
    		
	//Log("get..%d/%d",lDataLength,m_BytesInBuffer);
  long bytesWritten = 0;
	CAutoLock ClearLock(&m_ClearLock);
	while (bytesWritten < lDataLength)
	{
	  BUFFERITEM* item;
	  { //Context for CAutoLock
  	  CAutoLock BufferLock(&m_BufferLock);
  		if(m_BytesInBuffer <= 0 || !m_Array.size() || m_Array.size() <= 0)
      {
	      return (DWORD)bytesWritten;
      }
	    ivecBuffers it = m_Array.begin();
  		item = *it;  		
    }
    
		long copyLength = min(item->nDataLength - item->nOffset, lDataLength-bytesWritten);
		memcpy(&pbData[bytesWritten], &item->data[item->nOffset], copyLength);

		bytesWritten += copyLength;
		item->nOffset += copyLength;
    m_BytesInBuffer-=copyLength;

		if (item->nOffset >= item->nDataLength)
		{
      delete[] item->data;
			delete item;
  	  CAutoLock BufferLock(&m_BufferLock);
			m_Array.erase(m_Array.begin());
		}
	}
	return (DWORD)bytesWritten;
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
    if (m_BytesInBuffer > MAX_MEMORY_BUFFER_SIZE)
    {
  	  //Log("add..%d/%d",lDataLength,m_BytesInBuffer);
  	  //Overflow - discard a reasonable chunk of the current buffer
      while (m_BytesInBuffer > OVERFLOW_BUFFER_SIZE)
      {
        sleep=true;
  		  LogDebug("memorybuffer:put full buffer (%d)",m_BytesInBuffer);
  	    CAutoLock ClearLock(&m_ClearLock);
    	  BUFFERITEM *itemc = m_Array.back();		
        int copyLength=itemc->nDataLength - itemc->nOffset;  
        m_BytesInBuffer-=copyLength;
        delete[] itemc->data;
  		  delete itemc;
  		  m_Array.pop_back();
      }
    }
  
	  CAutoLock BufferLock(&m_BufferLock);
    m_Array.push_back(item);
    m_BytesInBuffer+=lDataLength;
  }
  if (m_pcallback)
  {
    m_pcallback->OnRawDataReceived(pbData,lDataLength);
  }
  if (sleep)
  {
    Sleep(1);
  }
	return S_OK;
}


void CMemoryBuffer::SetCallback(IMemoryCallback* callback)
{
  m_pcallback=callback;
}