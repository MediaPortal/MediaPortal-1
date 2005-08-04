/*
	MediaPortal TS-SourceFilter by Agree

	
*/


#ifndef __Buffers
#define __Buffers

#include <vector>
#include "FileReader.h"
#include "StreamPids.h"

class CBuffers
{
public:

	CBuffers(FileReader *pFileReader, StreamPids *pPids);
	virtual ~CBuffers();

	void Clear();
	long Count();
	HRESULT Require(long nBytes);

	HRESULT DequeFromBuffer(BYTE *pbData, long lDataLength);
	HRESULT ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset);

protected:
	FileReader *m_pFileReader;
	StreamPids *m_pPids;
	std::vector<BYTE *> m_Array;
	long m_lItemOffset;
	long m_lBuffersItemSize;
};

#endif
