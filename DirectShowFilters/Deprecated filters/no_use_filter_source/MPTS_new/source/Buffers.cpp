/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#include <streams.h>
#include "Buffers.h"
#include "Sections.h"
#include <crtdbg.h>



extern void LogDebug(const char *fmt, ...) ;

CBuffers::CBuffers(FileReader *pFileReader, StreamPids *pPids, long bufferSize)
{
	LogDebug("Buffers:ctor");
	m_buffer.iDataLen = 0;
	m_buffer.pos      = 0;
	m_buffer.pData    = new byte[200000];
	m_pFileReader	  = pFileReader;
	m_pPids			  = pPids;
}

CBuffers::~CBuffers()
{
	LogDebug("Buffers:dtor");
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
//	LogDebug("buffers: read from:%x into:%x size:%x", (DWORD)currPosition,m_buffer.iDataLen,(DWORD)fileSize);

	ULONG dwBytesRead = 0;				
	HRESULT hr = m_pFileReader->Read(&m_buffer.pData[m_buffer.iDataLen], nBytesRequired-Count(), &dwBytesRead);
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
//		LogDebug("buffers: read %x/%x bytes buffer:%x/%x ",dwBytesRead,nBytesRequired, m_buffer.pos,m_buffer.iDataLen);
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
	
	return S_OK;
}


HRESULT CBuffers::DequeFromBuffer(BYTE *pbData, long lDataLength)
{
	if (Count() < lDataLength) 
	{
		LogDebug("buffers:DequeFromBuffer() not enough data available:%d/%d",lDataLength,Count());
		return E_FAIL;
	}

	
//	LogDebug("buffers:DequeFromBuffer() get:%x/%x",lDataLength,Count());
	long bytesWritten = 0;
	while (bytesWritten < lDataLength)
	{
		long copyLen = m_buffer.iDataLen-m_buffer.pos;
		
		if (bytesWritten+copyLen > lDataLength)
			copyLen=(lDataLength-bytesWritten);

//		LogDebug("buffers: pos:%x datalen:%x copy:%x written:%x", m_buffer.pos,m_buffer.iDataLen,copyLen,bytesWritten);
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

