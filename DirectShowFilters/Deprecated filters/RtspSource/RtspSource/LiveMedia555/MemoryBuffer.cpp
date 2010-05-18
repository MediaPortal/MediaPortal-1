
#include <streams.h>
#include "MemoryBuffer.h"

#define MAX_MEMORY_BUFFER_SIZE (1024L*1024L*4L)

CMemoryBuffer::CMemoryBuffer(void)
{
}

CMemoryBuffer::~CMemoryBuffer()
{
  Clear();
}
void CMemoryBuffer::Clear()
{
	CAutoLock BufferLock(&m_BufferLock);
	std::vector<BUFFERITEM *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
    BUFFERITEM *item = *it;
    delete[] item->data;
		delete item;
	}
	m_Array.clear();
}

DWORD CMemoryBuffer::ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset)
{	
  long bytesWritten = 0;
	CAutoLock BufferLock(&m_BufferLock);
	while (bytesWritten < lDataLength)
	{
		if(!m_Array.size() || m_Array.size() <= 0)
			return E_FAIL;

		BUFFERITEM *item = m_Array.at(0);
    
		long copyLength = min(item->nDataLength - item->nOffset, lDataLength-bytesWritten);
		memcpy(pbData + bytesWritten, &item->data[item->nOffset], copyLength);

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

HRESULT CMemoryBuffer::PutBuffer(BYTE *pbData, long lDataLength, long lOffset)
{
  if (lDataLength<=0) return E_FAIL;
  if (lOffset<0) return E_FAIL;
  if (pbData==NULL) return E_FAIL;

  BUFFERITEM* item = new BUFFERITEM();
  item->nOffset=0;
  item->nDataLength=(lDataLength-lOffset);
  item->data = new byte[item->nDataLength];
  memcpy(item->data, &pbData[lOffset], item->nDataLength);

	CAutoLock BufferLock(&m_BufferLock);
  m_Array.push_back(item);
  m_BytesInBuffer+=item->nDataLength;

  while (m_BytesInBuffer > MAX_MEMORY_BUFFER_SIZE)
  {
		BUFFERITEM *item = m_Array.at(0);
    int copyLength=item->nDataLength - item->nOffset;

    m_BytesInBuffer-=copyLength;
		m_Array.erase(m_Array.begin());
    delete[] item->data;
		delete item;
  }
	return S_OK;
}