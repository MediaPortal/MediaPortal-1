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

#include "stdafx.h"
#include "SampleBuffer.h"
#include <crtdbg.h>
#include <math.h>
#include "global.h"

CSampleBuffer::CSampleBuffer()
{
	debugcount = 0;
	m_lItemOffset = 0;
	m_lSampleBufferItemSize = 5000000;
	m_TStreamID = 0;
	m_PATVersion = 0;
	m_NitPid = 0;
	m_PacketSize = 188;
	m_ProgPinMode = FALSE;
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

	ULONG pos = 0;
	hr = S_OK;
	while (hr == S_OK)
	{
		//search at the head of the file
		hr = FindSyncByte(pData, lDataLength-m_PacketSize, &pos, 1);
		if (hr == S_OK)
		{
			//parse next packet for the PAT
			if (ParsePAT(pData, lDataLength-m_PacketSize, pos) == S_OK)
				return S_OK;
		}
		pos += m_PacketSize;
	};
	return S_OK;
}

HRESULT CSampleBuffer::ReadSampleBuffer(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	CheckPointer(pbData, E_POINTER);

	CAutoLock BufferLock(&m_BufferLock);
	ULONG bytesAvailable = Count();
	if (bytesAvailable < lDataLength)
		return E_FAIL;

//	BoostThread Boost;

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

BOOL CSampleBuffer::CheckUpdateParser(int ver)
{
	if (m_PATVersion && ver && m_PATVersion != ver)
		return TRUE;

	return FALSE;
}

HRESULT CSampleBuffer::ParsePAT(PBYTE pData, ULONG lDataLength, long pos)
{
	HRESULT hr = S_FALSE;

	if ((WORD)(((0x1F & pData[pos+1])<<8) | (0xFF & pData[pos+2])) != 0) //must be pid 0 for PAT
		return S_FALSE;

	PBYTE pSection = ParseExtendedPacket(0, pData, lDataLength, pos);
	if (pSection == NULL)
		return S_FALSE;

	pos = 0;

	if ((0x20&pSection[pos+3]) == 0x20)
		pos += pSection[pos+4] + 1;

	int sectionLen = (WORD)(((0x0F & pSection[pos + 6]) << 8) | (0xFF & pSection[pos + 7]));
	m_TStreamID = ((0xFF & pSection[pos + 8]) << 8) | (0xFF & pSection[pos + 9]); //Get TSID Pid
	m_PATVersion = (((0xFF & pSection[pos+7 + sectionLen - 4])<<24) |
		((0xFF & pSection[pos+7 + sectionLen - 3])<<16) |
		((0xFF & pSection[pos+7 + sectionLen - 2])<<8) |
		(0xFF & pSection[pos+7 + sectionLen - 1])); //Get Version

	for (long b = pos + 7 + 5 ; b < pos + sectionLen + 7 - 4 ; b = b + 4) //less 4 crc bytes
	{
		//Get Program value and skip if nit pid
		if ((((0xFF & pSection[b + 1]) << 8) | (0xFF & pSection[b + 2])) == 0)
		{
			m_NitPid = (WORD)(0x1F & pSection[b + 3]) << 8 | (0xFF & pSection[b + 4]);
		}
	}
	delete[] pSection;
	return hr;
}

PBYTE CSampleBuffer::ParseExtendedPacket(int tableID, PBYTE pData, ULONG ulDataLength, ULONG pos)
{
	HRESULT hr = S_OK;

	if ((0x40&pData[pos+1])!=0x40 || (0x10&pData[pos+3])!=0x10)
		return NULL;

	int pos_save = pos;
	WORD pid = (WORD)(0x1F & pData[pos+1])<<8 | (0xFF & pData[pos+2]);

	if ((0x30&pData[pos+3]) == 0x30)	//adaptation field + payload
			pos += pData[pos+4] + 1;

	if (pData[pos+4] != 0x0 || pData[pos+5] != tableID || (0xf0&pData[pos+6])!=0xb0)
		return NULL;

	int sectionLen	= min(4096, (WORD)((0x0F & pData[pos+6])<<8 | (0xFF & pData[pos+7])));
	sectionLen -= m_PacketSize - ((pos+7)-pos_save);

	PBYTE pSectionData = new BYTE[4096];
	PBYTE pSectionDataSave = pSectionData;
	memcpy(pSectionData, pData+pos, 188); //save first pmt data packet
	pSectionData += m_PacketSize;
	pos +=m_PacketSize;

	while (hr == S_OK && sectionLen > 0)
	{
		//search at the head of the file
		hr = FindSyncByte(pData, ulDataLength, &pos, 1);
		if (hr == S_OK)
		{
			//parse next packet for the section
			if ((0x40&pData[pos+1])==0x00 && (0x10&pData[pos+3])==0x10 &&
				pid == (WORD)(((0x1F&pData[pos+1])<<8)|(0xFF&pData[pos+2])))
			{
				memcpy(pSectionData, pData+pos+4, 184); //save first section data packet
				pSectionData +=184;
				pos +=184;
				sectionLen -= 184;
			}
			
		}
		pos += m_PacketSize;
	}

	if (sectionLen > 0)
	{
		delete[] pSectionDataSave;
		return NULL;
	}

	return pSectionDataSave;
}

HRESULT CSampleBuffer::ParsePMT(PBYTE pData, ULONG ulDataLength, long pos)
{
	return S_OK;
}

HRESULT CSampleBuffer::FindSyncByte(PBYTE pbData, ULONG ulDataLength, ULONG* a, int step)
{
	//look for Program Pin Mode
	if (m_ProgPinMode)
	{
		//Look for Mpeg Program Pack headers
		while ((*a >= 0) && (*a < ulDataLength))
		{
			if ((ULONG)((0xFF&pbData[*a])<<24
					| (0xFF&pbData[*a+1])<<16
					| (0xFF&pbData[*a+2])<<8
					| (0xFF&pbData[*a+3])) == 0x1BA)
			{
				if (*a+m_PacketSize < ulDataLength)
				{
					if ((ULONG)((0xFF&pbData[*a + m_PacketSize])<<24
						| (0xFF&pbData[*a+1 + m_PacketSize])<<16
						| (0xFF&pbData[*a+2 + m_PacketSize])<<8
						| (0xFF&pbData[*a+3 + m_PacketSize])) == 0x1BA)
						return S_OK;
				}
				else
				{
					if (step > 0)
						return E_FAIL;

					if (*a-m_PacketSize > 0)
					{
						if ((ULONG)((0xFF&pbData[*a - m_PacketSize])<<24
						| (0xFF&pbData[*a+1 - m_PacketSize])<<16
						| (0xFF&pbData[*a+2 - m_PacketSize])<<8
						| (0xFF&pbData[*a+3 - m_PacketSize])) == 0x1BA)
							return S_OK;
					}
				}
			}
			*a += step;
		}
		return E_FAIL;
	}

	//Set for Transport Pin Mode
	while ((*a >= 0) && (*a < ulDataLength))
	{
		if (pbData[*a] == 0x47 && (pbData[*a+1]&0x80) == 0)
		{
			if (*a+m_PacketSize < ulDataLength)
			{
				if (pbData[*a+m_PacketSize] == 0x47 && (pbData[*a+1+m_PacketSize]&0x80) == 0)
					return S_OK;
			}
			else
			{
				if (step > 0)
					return E_FAIL;

				if (*a-m_PacketSize > 0)
				{
					if (pbData[*a-m_PacketSize] == 0x47 && (pbData[*a+1-m_PacketSize]&0x80) == 0)
						return S_OK;
				}
			}
		}
		*a += step;
	}
	return E_FAIL;
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
