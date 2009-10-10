/**
*  SampleBuffer.cpp
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
#include "SampleBuffer.h"
#include <crtdbg.h>
#include <math.h>
#include "global.h"

CSampleBuffer::CSampleBuffer()
{
	debugcount = 0;
	m_lItemOffset = 0;
	m_lSampleBufferItemSize = 5000000;
}

CSampleBuffer::~CSampleBuffer()
{
	Clear();
}

void CSampleBuffer::Clear()
{
	CAutoLock BufferLock(&m_BufferLock);
	std::vector<CBufferInfo *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		CBufferInfo *bufferInfo = *it;
		delete[] bufferInfo->sample;
		delete bufferInfo;
	}
	m_Array.clear();
	m_lItemOffset = 0;
}

long CSampleBuffer::Count()
{
	long bytesAvailable = 0;

	CAutoLock BufferLock(&m_BufferLock);
	std::vector<CBufferInfo *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		CBufferInfo *bufferInfo = *it;
		bytesAvailable += bufferInfo->size;
	}

	if (bytesAvailable >= m_lItemOffset)
		bytesAvailable -= m_lItemOffset;

	return bytesAvailable;
}

HRESULT CSampleBuffer::LoadMediaSample(IMediaSample *pSample)
{
	CheckPointer(pSample, E_POINTER);

	PBYTE pData = 0;
	HRESULT hr = pSample->GetPointer(&pData);
	if (FAILED(hr))
	{
		return hr;
	}

//::OutputDebugString(TEXT("CSampleBuffer::LoadMediaSample().\n"));
	LONG lDataLength = pSample->GetActualDataLength();
	CBufferInfo *bufferInfo = new CBufferInfo();
	bufferInfo->sample = new BYTE[lDataLength];
	if (!bufferInfo->sample)
	{
		delete bufferInfo;
		return E_FAIL;
	}

	memcpy(bufferInfo->sample, pData, lDataLength);
	bufferInfo->size = lDataLength;
	CAutoLock BufferLock(&m_BufferLock);
	m_Array.push_back(bufferInfo);
	while (Count() > m_lSampleBufferItemSize)
	{
		std::vector<CBufferInfo *>::iterator it = m_Array.begin();
		CBufferInfo *bufferInfo = *it;
		delete[] bufferInfo->sample;
		delete bufferInfo;
		m_Array.erase(it);
	}

	return S_OK;
}

HRESULT CSampleBuffer::ReadSampleBuffer(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	CheckPointer(pbData, E_POINTER);

	CAutoLock BufferLock(&m_BufferLock);
	ULONG bytesAvailable = Count();
	if (!bytesAvailable)
		return E_FAIL;

	*dwReadBytes = 0;
	int i = 0;
	while (i < (int)m_Array.size() && *dwReadBytes < bytesAvailable && *dwReadBytes < lDataLength)
	{
		CBufferInfo *bufferInfo = m_Array.at(i);
		ULONG length = bufferInfo->size;
		if ((*dwReadBytes + length) > lDataLength)
			length = lDataLength - *dwReadBytes;

		memcpy(pbData+(*dwReadBytes), bufferInfo->sample, length);
		*dwReadBytes = *dwReadBytes + length;
		i++;
	};
	return S_OK;
}

HRESULT CSampleBuffer::Require(long nBytes, BOOL bIgnoreDelay)
{
	CAutoLock BufferLock(&m_BufferLock);
	long bytesAvailable = Count();
	if (nBytes <= bytesAvailable)
		return S_OK;

::OutputDebugString(TEXT("SampleBuffer::Require() Waiting for file to grow.\n"));

return E_FAIL;
	while (nBytes > bytesAvailable)
	{
		CBufferInfo *newItem = new CBufferInfo();
		newItem->sample = new BYTE[m_lSampleBufferItemSize];
		newItem->size = m_lSampleBufferItemSize;

		ULONG ulBytesRead = 0;

//		__int64 currPosition = m_pFileReader->GetFilePointer();
//		HRESULT hr = m_pFileReader->Read(newItem, m_lSampleBufferItemSize, &ulBytesRead);
//		if (FAILED(hr)){

//			delete[] newItem;
//			m_lbuflen = 0;
//			return hr;
//		}

		if (ulBytesRead < (ULONG)m_lSampleBufferItemSize) 
		{
			WORD wReadOnly = 0;
//			m_pFileReader->get_ReadOnly(&wReadOnly);
			if (wReadOnly && !bIgnoreDelay)
			{
				int count = 20; // 2 second max delay
				while (ulBytesRead < (ULONG)m_lSampleBufferItemSize && count) 
				{
					::OutputDebugString(TEXT("SampleBuffer::Require() Waiting for file to grow.\n"));

					WORD bDelay = 0;
//					m_pFileReader->get_DelayMode(&bDelay);
					count--;

					if (bDelay > 0)
					{
						Sleep(2000);
						count = 0;
					}
					else
					{
						if (!count)
						{
							delete[] newItem;
							m_lbuflen = 0;
//							return hr;
						}
						Sleep(100);
					}

					ULONG ulNextBytesRead = 0;				
//					m_pFileReader->SetFilePointer(currPosition, FILE_BEGIN);
//					HRESULT hr = m_pFileReader->Read(newItem, m_lSampleBufferItemSize, &ulNextBytesRead);
//					if (FAILED(hr) && !count){

						delete[] newItem;
						m_lbuflen = 0;
//						return hr;
//					}

					if (((ulNextBytesRead == 0) | (ulNextBytesRead == ulBytesRead)) && !count){

						delete[] newItem;
						m_lbuflen = 0;
						return E_FAIL;
					}

					ulBytesRead = ulNextBytesRead;
				}
			}
			else
			{
				delete[] newItem;
				m_lbuflen = 0;
				return E_FAIL;
			}

			m_lbuflen--;
		}
		else if (m_lbuflen < 4)
			m_lbuflen++;

		m_lbuflen = max(0, m_lbuflen);

		CAutoLock BufferLock(&m_BufferLock);
		m_Array.push_back(newItem);
		bytesAvailable += m_lSampleBufferItemSize;
	}
	return S_OK;
}

HRESULT CSampleBuffer::DequeFromBuffer(BYTE *pbData, long lDataLength)
{
	HRESULT hr = Require(lDataLength);
	if (FAILED(hr))
		return hr;

	long bytesWritten = 0;
	CAutoLock BufferLock(&m_BufferLock);
	while (bytesWritten < lDataLength)
	{
		if(!m_Array.size() || m_Array.size() <= 0)
			return E_FAIL;

		CBufferInfo *item = m_Array.at(0);

		long copyLength = min(item->size-m_lItemOffset, lDataLength-bytesWritten);
		memcpy(pbData + bytesWritten, item->sample + m_lItemOffset, copyLength);

		bytesWritten += copyLength;
		m_lItemOffset += copyLength;

		if (m_lItemOffset >= item->size)
		{
			m_Array.erase(m_Array.begin());
			m_lItemOffset -= item->size;	//should result in zero
			delete[] item->sample;
			delete item;
		}
	}
	return S_OK;
}

HRESULT CSampleBuffer::ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset)
{
	CAutoLock BufferLock(&m_BufferLock);
	HRESULT hr = Require(lOffset + lDataLength);
	if (FAILED(hr))
		return hr;

	long bytesWritten = 0;
	long itemIndex = 0;
	lOffset += m_lItemOffset;

	while (bytesWritten < lDataLength)
	{
		while (lOffset >= m_Array.at(itemIndex)->size)
		{
			lOffset -= m_Array.at(itemIndex)->size;
			itemIndex++;

			if((m_Array.size() == 0) || ((long)m_Array.size() <= itemIndex))
				return E_FAIL;
		}

		if((m_Array.size() == 0) || ((long)m_Array.size() <= itemIndex))
			return E_FAIL;

		CBufferInfo *item = m_Array.at(itemIndex);

		long copyLength = min(item->size-lOffset, lDataLength-bytesWritten);
		{
			memcpy(pbData + bytesWritten, item->sample + lOffset, copyLength);

			bytesWritten += copyLength;
			lOffset += copyLength;
		}
	}

	return S_OK;
}

void CSampleBuffer::PrintLongLong(LPCTSTR lstring, __int64 value)
{
	TCHAR sz[100];
	double dVal = (double)value;
	double len = log10(dVal);
	int pos = (int)len;
	sz[pos+1] = '\0';
	while (pos >= 0)
	{
		int val = (int)(value % 10);
		sz[pos] = '0' + val;
		value /= 10;
		pos--;
	}
	TCHAR szout[100];
	wsprintf(szout, TEXT("%05i - %s %s\n"), debugcount, lstring, sz);
	::OutputDebugString(szout);
	debugcount++;
}
