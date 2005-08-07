/*
	MediaPortal TS-SourceFilter by Agree

	
*/


#ifndef __Buffers
#define __Buffers

#include <vector>
using namespace std;
#include "FileReader.h"
#include "StreamPids.h"

#define MAX_BYTES_IN_BUFFER = (5*1024*1024) // 5 megs

class CBuffers
{
public:
	struct stBuffer
	{
		BYTE* pData;
		LONG   iDataLen;
		LONG   pos;
	};
	typedef struct stBuffer BUFFER;

	CBuffers(FileReader *pFileReader, StreamPids *pPids, long bufferSize);
	virtual ~CBuffers();

	void Clear();
	long Count();
	HRESULT Require(long nBytes);

	HRESULT DequeFromBuffer(BYTE *pbData, long lDataLength);
	//HRESULT ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset);

protected:
	FileReader *m_pFileReader;
	StreamPids *m_pPids;
	vector<BUFFER> m_Array;
	typedef vector<BUFFER>::iterator itArray;
	long m_lBuffersItemSize;
	long m_lBytesInBuffer;
};

#endif
