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
	m_buffer.iDataLen = 0;
	m_buffer.pos      = 0;
	m_buffer.pData    = new byte[200000];
	m_pFileReader	  = pFileReader;
	m_pPids			  = pPids;
}

CBuffers::~CBuffers()
{
	delete[] m_buffer.pData;
}

void CBuffers::Clear()
{	
	m_buffer.pos=0;
	m_buffer.iDataLen=0;
}

long CBuffers::Count()
{
	return (m_buffer.iDataLen-m_buffer.pos);
}

HRESULT CBuffers::Require(long nBytesRequired, bool& endOfFile)
{
	
	endOfFile=false;
	if (nBytesRequired < Count()) return S_OK;

	ULONG ulBytesRead = 0;

	__int64 fileSize     = 0;
	__int64 currPosition = m_pFileReader->GetFilePointer();
	m_pFileReader->GetFileSize(&fileSize);
	//LogDebug("buffers: read from:%x", (DWORD)currPosition);

	ULONG dwBytesRead = 0;				
	HRESULT hr = m_pFileReader->Read(&m_buffer.pData[m_buffer.iDataLen], nBytesRequired, &dwBytesRead);
	if (FAILED(hr))
	{	
		LogDebug("buffers: failed to read %d bytes hr:%x", nBytesRequired,hr);
		m_pFileReader->SetFilePointer(currPosition, FILE_BEGIN);
		return hr;
	}
	if (dwBytesRead==0)
	{
		if (m_pFileReader->m_hInfoFile==INVALID_HANDLE_VALUE)
		{
			endOfFile=true;
			LogDebug("buffers: end of file reached");
			return E_FAIL;
		}

		if (fileSize> MAX_FILE_LENGTH)
		{
			endOfFile=true;
			currPosition=0;
			LogDebug("buffers: end of file reached, seek to begin");
			m_pFileReader->SetFilePointer(currPosition, FILE_BEGIN);
		}
	}
	if (dwBytesRead>0)
	{
		m_buffer.iDataLen+=dwBytesRead;
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
		long copyLen = m_buffer.iDataLen-m_buffer.pos;
		
		if (bytesWritten+copyLen > lDataLength)
			copyLen=(lDataLength-bytesWritten);

//		LogDebug("buffers: pos:%d datalen:%d copy:%d written:%d", buffer.pos,buffer.iDataLen,copyLen,bytesWritten);
		memcpy(pbData + bytesWritten, &m_buffer.pData[m_buffer.pos], copyLen);

		bytesWritten += copyLen;
		m_buffer.pos += copyLen;

		if (m_buffer.pos>=m_buffer.iDataLen)
		{
			
//			LogDebug("buffers: delete buffer");
			m_buffer.pos=0;
			m_buffer.iDataLen=0;
		}
	}
	
	return S_OK;
}

