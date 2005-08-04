/*
	MediaPortal TS-SourceFilter by Agree

	this files was taken in its original from OpenSource TSSourceFilter by nate & bear & bisswanger

*/


#include <streams.h>
#include "Buffers.h"
#include "Sections.h"
#include <crtdbg.h>

CBuffers::CBuffers(FileReader *pFileReader, StreamPids *pPids)
{
	m_pFileReader = pFileReader;
	m_pPids = pPids;
	m_lItemOffset = 0;
	m_lBuffersItemSize = 188000;
}

CBuffers::~CBuffers()
{
	Clear();
}

void CBuffers::Clear()
{
	std::vector<BYTE *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		delete *it;
	}
	m_Array.clear();
	m_lItemOffset = 0;
}

long CBuffers::Count()
{
	long bytesAvailable = 0;
	long itemCount = m_Array.size();

	if (itemCount > 0)
	{
		bytesAvailable += m_lBuffersItemSize - m_lItemOffset;
		bytesAvailable += m_lBuffersItemSize * (itemCount - 1);
	}
	return bytesAvailable;
}

HRESULT CBuffers::Require(long nBytes)
{

	long bytesAvailable = Count();

	while (nBytes > bytesAvailable)
	{
		BYTE *newItem = new BYTE[m_lBuffersItemSize];
		ULONG ulBytesRead = 0;

		__int64 currPosition = m_pFileReader->GetFilePointer();
		HRESULT hr = m_pFileReader->Read(newItem, m_lBuffersItemSize, &ulBytesRead);
		if (FAILED(hr))
			return hr;

		if (ulBytesRead < m_lBuffersItemSize) 
		{
			while (ulBytesRead < m_lBuffersItemSize) 
			{
				Sleep(100);

				ULONG ulNextBytesRead = 0;				
				m_pFileReader->SetFilePointer(currPosition, FILE_BEGIN);
				HRESULT hr = m_pFileReader->Read(newItem, m_lBuffersItemSize, &ulNextBytesRead);
				if (FAILED(hr))
					return hr;

				if ((ulNextBytesRead == 0) || (ulNextBytesRead == ulBytesRead))
					return E_FAIL;

				ulBytesRead = ulNextBytesRead;
			}
		}

		m_Array.push_back(newItem);
		bytesAvailable += m_lBuffersItemSize;
	}
	return S_OK;
}


HRESULT CBuffers::DequeFromBuffer(BYTE *pbData, long lDataLength)
{
	HRESULT hr = Require(lDataLength);
	if (FAILED(hr))
		return hr;

	long bytesWritten = 0;
	while (bytesWritten < lDataLength)
	{
		BYTE *item = m_Array.at(0);

		long copyLength = min(m_lBuffersItemSize-m_lItemOffset, lDataLength-bytesWritten);
		memcpy(pbData + bytesWritten, item + m_lItemOffset, copyLength);

		bytesWritten += copyLength;
		m_lItemOffset += copyLength;

		if (m_lItemOffset >= m_lBuffersItemSize)
		{
			m_Array.erase(m_Array.begin());
			delete[] item;
			m_lItemOffset -= m_lBuffersItemSize;	//should result in zero
		}
	}

	return S_OK;
}

HRESULT CBuffers::ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset)
{
	HRESULT hr = Require(lOffset + lDataLength);
	if (FAILED(hr))
		return hr;

	long bytesWritten = 0;
	long itemIndex = 0;
	lOffset += m_lItemOffset;

	while (bytesWritten < lDataLength)
	{
		while (lOffset >= m_lBuffersItemSize)
		{
			itemIndex++;
			lOffset -= m_lBuffersItemSize;
		}


		BYTE *item = m_Array.at(itemIndex);

		long copyLength = min(m_lBuffersItemSize-lOffset, lDataLength-bytesWritten);
		memcpy(pbData + bytesWritten, item + lOffset, copyLength);

		bytesWritten += copyLength;
		lOffset += copyLength;
	}

	return S_OK;
}

