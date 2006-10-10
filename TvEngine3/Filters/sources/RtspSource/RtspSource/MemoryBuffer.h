#pragma once
#include "WaitEvent.h"
#include <vector>
using namespace std;
class CMemoryBuffer
{
public:
  struct BufferItem
  {
    BYTE* data;
    int   nDataLength;
    int   nOffset;
  };
  typedef struct BufferItem BUFFERITEM;

  CMemoryBuffer(void);
  virtual ~CMemoryBuffer(void);
	DWORD ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset);
	HRESULT PutBuffer(BYTE *pbData, long lDataLength, long lOffset);
	void Clear();
  DWORD Size();
protected:
	vector<BUFFERITEM *> m_Array;
	CCritSec m_BufferLock;
  DWORD    m_BytesInBuffer;
  CWaitEvent m_event;
};
