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
	DWORD ReadFromBuffer(BYTE *pbData, long lDataLength);
	HRESULT PutBuffer(BYTE *pbData, long lDataLength);
	void Clear();
  long Size();
  void Run(bool onOff);
  bool IsRunning();
protected:
	typedef vector<BUFFERITEM *>::iterator ivecBuffers;
	vector<BUFFERITEM *> m_Array;
	CCritSec m_BufferLock;
  long m_BytesInBuffer;
  bool m_bRunning;
};
