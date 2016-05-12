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
#include "StdAfx.h"
#include "TSBuffer.h"
#include "StreamingDefs.h"

extern void LogDebug(const char *fmt, ...) ;
CTSBuffer::CTSBuffer()
{
	m_pFileReader = NULL;
	m_lItemOffset = 0;
	m_lTSBufferItemSize = PREFERRED_FRAME_SIZE; 
	m_lTSItemsPerRead = TV_BUFFER_ITEMS;

	//round to nearest byte boundary.

	m_maxReadIterations = 0;
	m_lastGoodReadTime = timeGetTime();
	m_lastEmptyReadTime = m_lastGoodReadTime;
	m_bWasEmpty = false;
	
	m_eChannelType = TV;

	m_maxReqSize = 0;
	m_minReqSize = LONG_MAX;
	m_maxReadSize = 0;
	m_minReadSize = LONG_MAX;

  LogDebug("CTSBuffer::ctor");
}

CTSBuffer::~CTSBuffer()
{
	Clear();
  LogDebug("CTSBuffer::dtor");
}

void CTSBuffer::SetFileReader(FileReader *pFileReader)
{
	if (!pFileReader)
		return;

  CAutoLock lock (&m_BufferLock);
	m_pFileReader = pFileReader;
}

void CTSBuffer::Clear()
{
  CAutoLock lock (&m_BufferLock);
	std::vector<BYTE *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		delete[] *it;
	}
	m_Array.clear();

	m_lItemOffset = 0;
	
	m_lastGoodReadTime = timeGetTime();
	m_lastEmptyReadTime = m_lastGoodReadTime;
	m_bWasEmpty = false;
	
	m_maxReqSize = 0;
	m_minReqSize = LONG_MAX;
	m_maxReadSize = 0;
	m_minReadSize = LONG_MAX;
}

long CTSBuffer::Count()
{
	CAutoLock lock (&m_BufferLock);
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
	CAutoLock lock (&m_BufferLock);

	Clear();

	//	Tv
	if(channelType == 0)
	{
		m_lTSItemsPerRead = TV_BUFFER_ITEMS;
		m_eChannelType = TV;
		LogDebug("CTSBuffer::SetChannelType() - Read size set to TV (%d)", m_lTSBufferItemSize * m_lTSItemsPerRead);
	}	
	else //	Radio
	{
		m_lTSItemsPerRead = RADIO_BUFFER_ITEMS;
		m_eChannelType = Radio;
		LogDebug("CTSBuffer::SetChannelType() - Read size set to Radio (%d)", m_lTSBufferItemSize * m_lTSItemsPerRead);
	}
}

HRESULT CTSBuffer::Require(long nBytes, long *lReadBytes)
{
	if (!m_pFileReader)
		return E_POINTER;

	CAutoLock lock (&m_BufferLock);
	long bytesAvailable = Count();
	if (nBytes <= bytesAvailable)
	{
	  *lReadBytes = nBytes;
		return S_OK;
	}

	*lReadBytes = bytesAvailable;

	//	Calculate how many data items we need
	UINT bytesRequired = nBytes - bytesAvailable;	
	UINT dataItemsRequired = bytesRequired / m_lTSBufferItemSize;

	if((bytesRequired % m_lTSBufferItemSize) > 0)
		dataItemsRequired++;
		
  dataItemsRequired = max(m_lTSItemsPerRead, dataItemsRequired);

	//	Work out the total number of bytes we need to read and allocate a buffer
	UINT bytesToRead = dataItemsRequired * m_lTSBufferItemSize;
	BYTE* readBuffer = new BYTE[bytesToRead];

	UINT totalBytesRead = 0;
	UINT iteration = 0;

  __int64 filePointer = m_pFileReader->GetFilePointer(); //store current pointer

	do
	{
		ULONG bytesRead = 0;
		HRESULT hr = m_pFileReader->Read(readBuffer + totalBytesRead, bytesToRead - totalBytesRead, &bytesRead);
		
		totalBytesRead += bytesRead;
		iteration++;

		if(FAILED(hr))
		{
			LogDebug("CTSBuffer::Require() - Failed to read buffer file, iteration %d", iteration);
			m_maxReadIterations = 0;			
      m_pFileReader->SetFilePointer(filePointer,FILE_BEGIN); //Restore file read position
			delete[] readBuffer;
			return hr;
		}

	  dataItemsRequired = (totalBytesRead / m_lTSBufferItemSize);
	  if (dataItemsRequired > 0)
	  { 
	    //Check if enough delay time since last 'empty' read - need at least MIN_FILE_BUFFER_TIME (ms)
	    //of data buffered in file to avoid stutter problems on restart of good reads.
      DWORD readTime = timeGetTime();
      if (m_bWasEmpty && ((readTime - m_lastEmptyReadTime) < MIN_FILE_BUFFER_TIME))
      {
        m_pFileReader->SetFilePointer(filePointer,FILE_BEGIN); //Restore file read position
  			delete[] readBuffer;
			  return E_FAIL;
      }
		  if (m_bWasEmpty)
		  {
			  LogDebug("CTSBuffer::Require() - First read after EOF, total stall: %d ms, bytes: %d", readTime - m_lastEmptyReadTime, totalBytesRead);
			}
			m_bWasEmpty = false;
	    m_lastGoodReadTime = readTime;

	    //We have at least one frame of data, so just add all complete frames onto the queue and correct the file pointer position
	    UINT discardBytes = (totalBytesRead % m_lTSBufferItemSize); //Keep complete frames only - discard partial frame data
	    if (discardBytes > 0)
	    {
  	    totalBytesRead -= discardBytes;
        m_pFileReader->SetFilePointer(filePointer+totalBytesRead, FILE_BEGIN);
      }
	    break;
	  }

		if(iteration >= 20)
		{
		  if (!m_bWasEmpty)
		  {
			  LogDebug("CTSBuffer::Require() - End of file, bytes available %d, bytes requested %d", (bytesAvailable + totalBytesRead), nBytes);
			}
			m_maxReadIterations = 0;			
      m_pFileReader->SetFilePointer(filePointer,FILE_BEGIN); //Restore file read position
			delete[] readBuffer;
			m_lastEmptyReadTime = timeGetTime();
			m_bWasEmpty = true;
			return E_FAIL; //'Empty' read - not enough data to satisfy request
		}
		
		Sleep(5); //Sleep for 5ms per iteration
	}
	while(true);

  if (iteration > m_maxReadIterations) 
  {
    m_maxReadIterations = iteration;	
	  LogDebug("CTSBuffer::Require() - m_maxReadIterations: %d", m_maxReadIterations);
  }		

	// Copy all bytes to data items
	for(UINT i = 0; i < dataItemsRequired; i++)
	{
		BYTE* newDataItem = new BYTE[m_lTSBufferItemSize];
		memcpy(newDataItem, readBuffer + (i * m_lTSBufferItemSize), m_lTSBufferItemSize);

		m_Array.push_back(newDataItem);
	}

	delete[] readBuffer;

  //Return the data amount requested, or less if full amount is not available
	*lReadBytes = min(nBytes, bytesAvailable + (long)totalBytesRead);

	return S_OK;
}

HRESULT CTSBuffer::DequeFromBuffer(BYTE *pbData, long lDataLength, long *lReadBytes)
{
	CAutoLock lock (&m_BufferLock);     
		
	//Debug logging
	if (lDataLength > m_maxReqSize) {m_maxReqSize = lDataLength; LogDebug("CTSBuffer - m_maxReqSize: %d)", m_maxReqSize);}
	if (lDataLength < m_minReqSize) {m_minReqSize = lDataLength; LogDebug("CTSBuffer - m_minReqSize: %d)", m_minReqSize);}
	
	HRESULT hr = Require(lDataLength, lReadBytes);
	if (FAILED(hr))
		return hr;
		
	if (*lReadBytes<=0)
		return S_FALSE;

	long bytesWritten = 0;
	while (bytesWritten < *lReadBytes)
	{
		if(!m_Array.size() || m_Array.size() <= 0)
	  {
	    *lReadBytes = 0;
			return S_FALSE;
		}

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
	
	*lReadBytes = bytesWritten;
	
	if (bytesWritten != lDataLength) {LogDebug("CTSBuffer - DequeFromBuffer() length mismatch, request: %d, return: %d)", lDataLength, bytesWritten);}

	return S_OK;
}

//HRESULT CTSBuffer::ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset)
//{
//	if (!m_pFileReader)
//		return E_POINTER;
//
//	CAutoLock lock (&m_BufferLock);
//	long lReadBytes = 0;
//	HRESULT hr = Require(lOffset + lDataLength, &lReadBytes);
//	if (FAILED(hr))
//		return hr;
//
//	long bytesWritten = 0;
//	long itemIndex = 0;
//	lOffset += m_lItemOffset;
//
//	while (bytesWritten < lDataLength)
//	{
//		while (lOffset >= m_lTSBufferItemSize)
//		{
//			lOffset -= m_lTSBufferItemSize;
//
//			itemIndex++;
//			if((m_Array.size() == 0) || ((long)m_Array.size() <= itemIndex))
//				return E_FAIL;
//		}
//
//		if((m_Array.size() == 0) || ((long)m_Array.size() <= itemIndex))
//			return E_FAIL;
//
//		BYTE *item = m_Array.at(itemIndex);
//
//		long copyLength = min(m_lTSBufferItemSize-lOffset, lDataLength-bytesWritten);
//		{
//			memcpy(pbData + bytesWritten, item + lOffset, copyLength);
//
//			bytesWritten += copyLength;
//			lOffset += copyLength;
//		}
//	}
//
//	return S_OK;
//}

HRESULT CTSBuffer::GetNullTsBuffer(BYTE *pbData, long lDataLength, long *lReadBytes)
{
  if ((timeGetTime() - m_lastGoodReadTime) > NULL_TS_TIMEOUT)
  {
    //Limit contiguous null packet sending to NULL_TS_TIMEOUT time
    *lReadBytes = 0;
    return E_FAIL;
  }

  //Fill up a buffer with NULL transport stream packets...
	BYTE* nullTsPacket = new BYTE[TRANSPORT_PACKET_SIZE];
  ZeroMemory((void*)nullTsPacket, TRANSPORT_PACKET_SIZE);
  //Add TS header (for null packet)
  nullTsPacket[0] = TRANSPORT_SYNC_BYTE; //Sync Byte
  nullTsPacket[1] = 0x1F; //NULL packet PID
  nullTsPacket[2] = 0xFF; //NULL packet PID
  nullTsPacket[3] = 0x10; //not scrambled, payload only, continuity zero
  
	lDataLength -= (lDataLength % TRANSPORT_PACKET_SIZE); //Adjust to be an integral number of TS packets in size
	long bytesWritten = 0;
	while (bytesWritten < lDataLength)
	{
		long copyLength = min(TRANSPORT_PACKET_SIZE, lDataLength-bytesWritten);
		memcpy(pbData + bytesWritten, nullTsPacket, copyLength);

		bytesWritten += copyLength;
	}
	  
	*lReadBytes = bytesWritten;
	
	delete[] nullTsPacket;
	return S_OK;
}
