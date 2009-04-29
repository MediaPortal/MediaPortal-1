#pragma once
#include "WaitEvent.h"
#include <vector>
using namespace std;

class IMemoryCallback
{
public:
	virtual void OnRawDataReceived(BYTE *pbData, long lDataLength)=0;
};

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
  void  SetCallback(IMemoryCallback* callback);
	DWORD ReadFromBuffer(BYTE *pbData, long lDataLength);
	HRESULT PutBuffer(BYTE *pbData, long lDataLength);
	void Clear();
  DWORD Size();
  void Run(bool onOff);
protected:
	vector<BUFFERITEM *> m_Array;
	CCritSec m_BufferLock;
  DWORD    m_BytesInBuffer;
  CWaitEvent m_event;
  IMemoryCallback* m_pcallback;
  bool m_bRunning;
};
