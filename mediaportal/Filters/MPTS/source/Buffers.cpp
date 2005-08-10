/*
	MediaPortal TS-SourceFilter by Agree

	this files was taken in its original from OpenSource TSSourceFilter by nate & bear & bisswanger

*/


#include <streams.h>
#include "Buffers.h"
#include "Sections.h"
#include <crtdbg.h>



extern void LogDebug(const char *fmt, ...) ;

CBuffers::CBuffers(FileReader *pFileReader, StreamPids *pPids, long bufferSize)
{
	m_pFileReader = pFileReader;
	m_pPids = pPids;
	
	m_lBytesInBuffer=0;
	m_lBuffersItemSize = bufferSize;
}

CBuffers::~CBuffers()
{
	Clear();
}

void CBuffers::Clear()
{
	itArray it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		if (it->pData!=NULL)
			delete [] it->pData;
	}
	m_Array.clear();
	
	m_lBytesInBuffer=0;
}

long CBuffers::Count()
{
	return m_lBytesInBuffer;
}

HRESULT CBuffers::Require(long nBytesRequired, bool& endOfFile)
{
	endOfFile=false;
	if (nBytesRequired < Count()) return S_OK;

	BUFFER newBuffer;
	newBuffer.iDataLen=0;
	newBuffer.pos=0;
	newBuffer.pData= new BYTE[m_lBuffersItemSize];
	ULONG ulBytesRead = 0;

	__int64 fileSize     = 0;
	__int64 currPosition = m_pFileReader->GetFilePointer();
	m_pFileReader->GetFileSize(&fileSize);
	//LogDebug("buffers: read from:%x", (DWORD)currPosition);

	ULONG dwBytesRead = 0;				
	HRESULT hr = m_pFileReader->Read(newBuffer.pData, m_lBuffersItemSize, &dwBytesRead);
	if (FAILED(hr))
	{	
		LogDebug("buffers: failed to read %d bytes hr:%x", m_lBuffersItemSize,hr);
		m_pFileReader->SetFilePointer(currPosition, FILE_BEGIN);
		delete newBuffer.pData;
		return hr;
	}
	if (dwBytesRead==0)
	{
		if (m_pFileReader->m_hInfoFile==INVALID_HANDLE_VALUE)
		{
			endOfFile=true;
			LogDebug("buffers: end of file reached");
			delete newBuffer.pData;
			return E_FAIL;
		}

		if (fileSize> MAX_FILE_LENGTH)
		{
			endOfFile=true;
			delete newBuffer.pData;
			currPosition=0;
			LogDebug("buffers: end of file reached, seek to begin");
			m_pFileReader->SetFilePointer(currPosition, FILE_BEGIN);
		}
	}
	if (dwBytesRead>0)
	{
		newBuffer.iDataLen=dwBytesRead;
		m_Array.push_back(newBuffer);
		m_lBytesInBuffer += dwBytesRead;
	}

	if (m_pFileReader->m_hInfoFile!=INVALID_HANDLE_VALUE)
	{
		if (Count() < nBytesRequired) 
		{
			//currPosition = m_pFileReader->GetFilePointer();
			Sleep(100);
		}
	}
	//currPosition = m_pFileReader->GetFilePointer();
	//LogDebug("buffers: read %d bytes buffer:%d/%d pos:%x filesize:%x",dwBytesRead,m_lBytesInBuffer,nBytesRequired,(DWORD)currPosition,(DWORD)fileSize);
	
	return S_OK;
}


HRESULT CBuffers::DequeFromBuffer(BYTE *pbData, long lDataLength)
{

	if (Count() < lDataLength) 
	{
		LogDebug("buffers:DequeFromBuffer() not enough data available:%d/%d",lDataLength,Count());
		return E_FAIL;
	}

	
//	LogDebug("buffers:DequeFromBuffer() get:%d/%d",lDataLength,Count());
	long bytesWritten = 0;
	while (bytesWritten < lDataLength)
	{
		BUFFER& buffer= m_Array.at(0);

		long copyLen = buffer.iDataLen-buffer.pos;
		
		if (bytesWritten+copyLen > lDataLength)
			copyLen=(lDataLength-bytesWritten);

//		LogDebug("buffers: pos:%d datalen:%d copy:%d written:%d", buffer.pos,buffer.iDataLen,copyLen,bytesWritten);
		memcpy(pbData + bytesWritten, &buffer.pData[buffer.pos], copyLen);

		bytesWritten += copyLen;
		buffer.pos += copyLen;
		m_lBytesInBuffer -= copyLen;

		if (buffer.pos>=buffer.iDataLen)
		{
			
//			LogDebug("buffers: delete buffer");
			delete[] buffer.pData;
			m_Array.erase(m_Array.begin());
		}
	}
	

	return S_OK;
}

