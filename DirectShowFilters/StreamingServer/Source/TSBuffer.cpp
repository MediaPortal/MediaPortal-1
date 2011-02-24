/**
*  TSBuffer.cpp
*  Copyright (C) 2005      nate
*  Copyright (C) 2006      bear
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "TSBuffer.h"
#include <crtdbg.h>
#include <math.h>
#include "global.h"
#include "entercriticalsection.h"

#define TV_BUFFER_ITEM_SIZE	32336
#define RADIO_BUFFER_ITEM_SIZE	1880

extern void LogDebug(const char *fmt, ...) ;
CTSBuffer::CTSBuffer()
{
	m_pFileReader = NULL;
	m_lItemOffset = 0;
	m_lTSBufferItemSize = TV_BUFFER_ITEM_SIZE; 

	//round to nearest byte boundary.

	m_ParserLock = FALSE;
	m_loopCount = 20;
	debugcount = 0;
}

CTSBuffer::~CTSBuffer()
{
	Clear();
}

void CTSBuffer::SetFileReader(FileReader *pFileReader)
{
	if (!pFileReader)
		return;

  Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	m_pFileReader = pFileReader;
}

void CTSBuffer::Clear()
{
  Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	std::vector<BYTE *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		delete[] *it;
	}
	m_Array.clear();

	m_lItemOffset = 0;
	m_loopCount = 2;
}

long CTSBuffer::Count()
{
	Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	long bytesAvailable = 0;
	long itemCount = m_Array.size();

	if (itemCount > 0)
	{
		bytesAvailable += m_lTSBufferItemSize - m_lItemOffset;
		bytesAvailable += m_lTSBufferItemSize * (itemCount - 1);
	}
	return bytesAvailable;
}

void CTSBuffer::SetChannelType(int channelType)
{
	Mediaportal::CEnterCriticalSection lock(m_BufferLock);

	Clear();

	//	Tv
	if(channelType == 0)
	{
		LogDebug("CTSBuffer::SetChannelType() - Buffer size set to TV (%d)", TV_BUFFER_ITEM_SIZE);
		m_lTSBufferItemSize = TV_BUFFER_ITEM_SIZE;
		m_eChannelType = TV;
	}

	//	Radio
	else
	{
		LogDebug("CTSBuffer::SetChannelType() - Buffer size set to radio (%d)", RADIO_BUFFER_ITEM_SIZE);
		m_lTSBufferItemSize = RADIO_BUFFER_ITEM_SIZE;
		m_eChannelType = Radio;
	}
}

HRESULT CTSBuffer::Require(long nBytes, BOOL bIgnoreDelay)
{
	if (!m_pFileReader)
		return E_POINTER;

	Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	long bytesAvailable = Count();
	if (nBytes <= bytesAvailable)
		return S_OK;

	//	Calculate how many data items we need
	UINT bytesRequired = nBytes - bytesAvailable;
	UINT dataItemsRequired = bytesRequired / m_lTSBufferItemSize;

	if((bytesRequired % m_lTSBufferItemSize) > 0)
		dataItemsRequired++;

	//	Work out the total number of bytes we need to read and allocate a buffer
	UINT bytesToRead = dataItemsRequired * m_lTSBufferItemSize;
	BYTE* readBuffer = new BYTE[bytesToRead];

	UINT totalBytesRead = 0;
	UINT iteration = 0;

	do
	{
		if(iteration > 0)
		{
			//	Sleep for 4ms per iteration (TV)
			int sleepPerIteration = 4;

			//	If radio set to 20ms
			if(m_eChannelType == Radio)
				sleepPerIteration = 20;

			Sleep(iteration * sleepPerIteration);
		}

		ULONG bytesRead = 0;
		HRESULT hr = m_pFileReader->Read(readBuffer + totalBytesRead, bytesToRead - totalBytesRead, &bytesRead);

		if(FAILED(hr) || iteration >= 20)
		{
			LogDebug("TSBuffer::Require() - Failed to read buffer file");
			
			delete[] readBuffer;

			return hr;
		}

		totalBytesRead += bytesRead;
		iteration++;
	}
	while(totalBytesRead < bytesToRead);


	//	Success! Copy all bytes to data items
	for(int i = 0; i < dataItemsRequired; i++)
	{
		BYTE* newDataItem = new BYTE[m_lTSBufferItemSize];
		memcpy(newDataItem, readBuffer + (i * m_lTSBufferItemSize), m_lTSBufferItemSize);

		m_Array.push_back(newDataItem);
		bytesAvailable += m_lTSBufferItemSize;
	}

	delete[] readBuffer;

	return S_OK;
}

HRESULT CTSBuffer::DequeFromBuffer(BYTE *pbData, long lDataLength)
{
	Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	HRESULT hr = Require(lDataLength);
	if (FAILED(hr))
		return hr;

	long bytesWritten = 0;
	while (bytesWritten < lDataLength)
	{
		if(!m_Array.size() || m_Array.size() <= 0)
			return E_FAIL;

		BYTE *item = m_Array.at(0);

		long copyLength = min(m_lTSBufferItemSize-m_lItemOffset, lDataLength-bytesWritten);
		memcpy(pbData + bytesWritten, item + m_lItemOffset, copyLength);

		bytesWritten += copyLength;
		m_lItemOffset += copyLength;

		if (m_lItemOffset >= m_lTSBufferItemSize)
		{
			m_Array.erase(m_Array.begin());
			delete[] item;
			m_lItemOffset -= m_lTSBufferItemSize;	//should result in zero
		}
	}
	return S_OK;
}

HRESULT CTSBuffer::ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset)
{
	if (!m_pFileReader)
		return E_POINTER;

	Mediaportal::CEnterCriticalSection lock(m_BufferLock);
	HRESULT hr = Require(lOffset + lDataLength);
	if (FAILED(hr))
		return hr;

	long bytesWritten = 0;
	long itemIndex = 0;
	lOffset += m_lItemOffset;

	while (bytesWritten < lDataLength)
	{
		while (lOffset >= m_lTSBufferItemSize)
		{
			lOffset -= m_lTSBufferItemSize;

			itemIndex++;
			if((m_Array.size() == 0) || ((long)m_Array.size() <= itemIndex))
				return E_FAIL;
		}

		if((m_Array.size() == 0) || ((long)m_Array.size() <= itemIndex))
			return E_FAIL;

		BYTE *item = m_Array.at(itemIndex);

		long copyLength = min(m_lTSBufferItemSize-lOffset, lDataLength-bytesWritten);
		{
			memcpy(pbData + bytesWritten, item + lOffset, copyLength);

			bytesWritten += copyLength;
			lOffset += copyLength;
		}
	}

	return S_OK;
}