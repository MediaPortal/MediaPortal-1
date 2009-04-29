/**
*  TSParserInputPin.cpp
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
*  authors can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include <math.h>
#include <crtdbg.h>
#include "stdafx.h"
#include "TSParserSource.h"
#include "TSParserSourceGuids.h"
#include "BDATYPES.H"
#include "KS.H"
#include "KSMEDIA.H"
#include "BDAMedia.h"
#include "global.h"

//////////////////////////////////////////////////////////////////////
// CTSParserInputPin
//////////////////////////////////////////////////////////////////////
CTSParserInputPin::CTSParserInputPin(CTSParserSourceFilter *pParserFilter, LPUNKNOWN pUnk, CCritSec *pLock, HRESULT *phr) :
	CRenderedInputPin(NAME("Input Pin"), pParserFilter, pLock, phr, L"In"),
	m_pTSParserSourceFilter(pParserFilter),
    m_tLast(0),
	m_pFileName(0),
	m_pRegFileName(NULL),
	m_pFileWriter(NULL),
	m_pSharedMemory(NULL)
{
//Frodo code changes
    m_restBufferLen = 0;
	m_PacketErrors = 0;
//Frodo code changes

	m_writeBufferSize = 4096*32;
	m_writeBuffer = new BYTE[(UINT)m_writeBufferSize];
    m_writeBufferLen = 0;

	m_WriteSampleSize = 0;
	m_WriteBufferSize = 0;
	m_WriteThreadActive = FALSE;







	MultiMemWriterParam writerParams;
	writerParams.chunkSize = (__int64)((__int64)1048576 *(__int64)8); //250MB;
	writerParams.maxFiles = 8;
	writerParams.maxSize = (__int64)((__int64)1048576 *(__int64)8); //250MB
	writerParams.minFiles = 4;

	m_pSharedMemory = new SharedMemory(writerParams.maxSize);
	m_pFileWriter = new MultiMemWriter(m_pSharedMemory, &writerParams);

	m_pRegFileName = new char[MAX_PATH];
	if (m_pRegFileName != 0)
		sprintf(m_pRegFileName, "MyBufferFile");//"G:\\Capture\\MyBufferFile");
	
	if(m_pRegFileName && strlen(m_pRegFileName) > 0 && strlen(m_pRegFileName) <= MAX_PATH)
	{
		if (m_pFileName)
			delete[] m_pFileName;

		long length = lstrlen(m_pRegFileName);

		// Check that the filename ends with .tsbuffer. If it doesn't we'll add it
		if ((length < 9) || (stricmp(m_pRegFileName+length-9, ".tsbuffer") != 0))
		{
			m_pFileName = new wchar_t[1+length+9];
			if (m_pFileName != 0)
				swprintf(m_pFileName, L"%S.tsbuffer", m_pRegFileName);
		}
		else
		{
			m_pFileName = new WCHAR[1+length];
			if (m_pFileName != 0)
				swprintf(m_pFileName, L"%S", m_pRegFileName);
		}
	}













}

CTSParserInputPin::~CTSParserInputPin()
{
	CloseFile();
	Clear();
	if (m_pFileWriter) delete m_pFileWriter;
	if (m_pSharedMemory) delete m_pSharedMemory;
	if (m_pFileName) delete[] m_pFileName;
	if (m_pRegFileName) delete[] m_pRegFileName;
	delete[] m_writeBuffer;
}

STDMETHODIMP CTSParserInputPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
//	CAutoLock cAutoLock(m_pFilter->pStateLock());
    CAutoLock lock(&m_ReceiveLock);

    if (riid == IID_IFileSinkFilter)
	{
        return GetInterface((IFileSinkFilter *) this, ppv);
    } 
	if (riid == IID_ITSParserSource)
	{
		return GetInterface((ITSParserSource*)m_pTSParserSourceFilter, ppv);
	}
	if (riid == IID_IFileSourceFilter)
	{
		return GetInterface((IFileSourceFilter*)m_pTSParserSourceFilter, ppv);
	}
    if (riid == IID_IAMFilterMiscFlags)
    {
		return GetInterface((IAMFilterMiscFlags*)m_pTSParserSourceFilter, ppv);
    }
	if (riid == IID_IAMStreamSelect && m_pTSParserSourceFilter->get_AutoMode())
	{
		 GetInterface((IAMStreamSelect*)m_pTSParserSourceFilter, ppv);
	}
	if (riid == IID_IAsyncReader)
	{
		if ((!m_pTSParserSourceFilter->m_pPidParser->pids.pcr
			&& !m_pTSParserSourceFilter->get_AutoMode()
			&& m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode())
			&& m_pTSParserSourceFilter->m_pPidParser->get_AsyncMode())
		{
			return GetInterface((IAsyncReader*)m_pTSParserSourceFilter, ppv);
		}
	}
	if (riid == IID_IMediaSeeking)
    {
		return GetInterface((IMediaSeeking*)m_pTSParserSourceFilter, ppv);
    }
    return CRenderedInputPin::NonDelegatingQueryInterface( riid, ppv );
}

void CTSParserInputPin::Clear()
{
	StopThread(0);
	CAutoLock BufferLock(&m_BufferLock);
	std::vector<CBufferInfo*>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		CBufferInfo *bufferInfo = *it;
		delete[] bufferInfo->sample;
		delete bufferInfo;
	}
	m_Array.clear();

	m_WriteSampleSize = 0;
	m_WriteBufferSize = 0;
	m_WriteThreadActive = FALSE;
}

HRESULT CTSParserInputPin::CheckMediaType(const CMediaType *pType)
{
    CheckPointer(pType, E_POINTER);
    CAutoLock lock(&m_ReceiveLock);
//	CAutoLock cAutoLock(m_pFilter->pStateLock());

	if(MEDIATYPE_Stream == pType->majortype)
	{
		//Are we in Capture mode
		if (KSDATAFORMAT_SUBTYPE_BDA_MPEG2_TRANSPORT == pType->subtype)
			return S_OK;

		//Are we in Transport mode
		if (MEDIASUBTYPE_MPEG2_TRANSPORT == pType->subtype)
			return S_OK;

		//Are we in Program mode
		else if (MEDIASUBTYPE_MPEG2_PROGRAM == pType->subtype)
			return S_OK;

	}
		//Are we connecting to a network provider
	else if(KSDATAFORMAT_TYPE_BDA_ANTENNA == pType->majortype)
	{
		if (MEDIASUBTYPE_None  == pType->subtype)
			return S_OK;
	}

    return S_FALSE;
}

STDMETHODIMP  CTSParserInputPin::Connect(IPin * pReceivePin, const AM_MEDIA_TYPE *pmt)
{
	return CBasePin::Connect(pReceivePin, pmt);
}

HRESULT CTSParserInputPin::BreakConnect()
{
	Clear();

    if (m_pFileWriter)
        CloseFile();


//////////////////    if (m_pTSParserSourceFilter->m_pPosition != NULL) {
/////////////////        m_pTSParserSourceFilter->m_pPosition->ForceRefresh();
//////////////////// /   }
//Frodo code changes
    m_restBufferLen=0;
//Frodo code changes
	m_writeBufferSize = sizeof(m_writeBuffer);
    m_writeBufferLen = 0;

    return CRenderedInputPin::BreakConnect();
}

HRESULT CTSParserInputPin::CompleteConnect(IPin *pReceivePin)
{
    HRESULT hr =  CBasePin::CompleteConnect(pReceivePin);

    return hr;
}

HRESULT CTSParserInputPin::Run(REFERENCE_TIME tStart)
{
	if (!m_WriteThreadActive && IsConnected())
		StartThread();

	return CRenderedInputPin::Run(tStart);
}

HRESULT CTSParserInputPin::Load()
{
	if (!m_WriteThreadActive && IsConnected())
	{
		Clear();

	    if (m_pFileWriter)
			CloseFile();

	    if (m_pFileWriter)
    		OpenFile();

		StartThread();

		__int64 lpllsize = 0;
		int count = 0;
//		Sleep(200);
		while (lpllsize < 200000 && count < 100)
		{
			m_pFileWriter->GetFileSize(&lpllsize);
			Sleep(20);
			count++;
		}

		LPWSTR wsz = new WCHAR[1+lstrlenW(m_pFileName)];
		if (wsz == NULL)
			return E_OUTOFMEMORY;

		wcscpy(wsz, m_pFileName);
		m_pTSParserSourceFilter->ReLoad(wsz, NULL);

		REFERENCE_TIME stop, start = (__int64)max(0,(__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.dur - RT_2_SECOND));
		IMediaSeeking *pMediaSeeking;
		if(m_pTSParserSourceFilter->GetFilterGraph() && SUCCEEDED(m_pTSParserSourceFilter->GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
		{
			pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_AbsolutePositioning);
			pMediaSeeking->Release();
		}

		delete[] wsz;

		return CRenderedInputPin::Run(start);
	}
	else if (IsConnected() && m_WriteThreadActive)
	{
	}

	return 0;
}

STDMETHODIMP CTSParserInputPin::ReceiveCanBlock()
{
    return S_FALSE;
}

//Frodo code changes
HRESULT CTSParserInputPin::Filter(byte* pbData,long sampleLen)
{
	HRESULT hr;
	int packet = 0;
	BOOL bProg = FALSE;
	_AMMediaType *mtype = &m_mt;
	if (mtype->majortype == MEDIATYPE_Stream &&
		((mtype->subtype == MEDIASUBTYPE_MPEG2_TRANSPORT) | (mtype->subtype == KSDATAFORMAT_SUBTYPE_BDA_MPEG2_TRANSPORT)))
	{
		//filter transport Packets
		packet = 188;
	}
	else if (mtype->majortype == MEDIATYPE_Stream &&
		mtype->subtype == MEDIASUBTYPE_MPEG2_PROGRAM)
	{
		//filter mpeg2 es Packets
		bProg = TRUE;
		packet = 2048;
	}
	else
	{
		//Write raw data method
		return WriteBufferSample(pbData, sampleLen);
	}

	int off=-1;

	// did last media sample we received contain a incomplete transport packet at the end? 
	if (m_restBufferLen>0) 
	{ 
       //yep then we copy the remaining bytes of the packet first 
		int len=packet-m_restBufferLen;

		//remaining bytes of packet  
		if (len>0 && len < packet)  
		{         
			if (m_restBufferLen>=0 && m_restBufferLen+len < packet+2)    
			{      
				memcpy(&m_restBuffer[m_restBufferLen], pbData, len);

				if(!bProg)
				{
					//check if this is indeed a transport packet  
					if(m_restBuffer[0]==0x47)   
					{     
						if FAILED(hr = WriteBufferSample(m_restBuffer,packet))
							return hr;
					}

					//set offset ...   
					if (pbData[len]==0x47 && pbData[len+packet]==0x47 && pbData[len+2*packet]==0x47)    
					{    
						off=len;   
					}      
					else             
					{                 
						m_restBufferLen=0;      
					}  
				}
				else
				{
					if (((0xFF&pbData[0])<<24
					| (0xFF&pbData[1])<<16
					| (0xFF&pbData[2])<<8
					| (0xFF&pbData[3])) == 0x1BA)
					{
						if FAILED(hr = WriteBufferSample(m_restBuffer,packet))   
							return hr;
					}

					//set offset ...   
					if (((0xFF&pbData[len])<<24
						| (0xFF&pbData[len+1])<<16
						| (0xFF&pbData[len+2])<<8
						| (0xFF&pbData[len+3])) == 0x1BA &&
						((0xFF&pbData[len+packet])<<24
						| (0xFF&pbData[len+packet+1])<<16
						| (0xFF&pbData[len+packet+2])<<8
						| (0xFF&pbData[len+packet+3])) == 0x1BA &&
						((0xFF&pbData[len+2*packet])<<24
						| (0xFF&pbData[len+2*packet+1])<<16
						| (0xFF&pbData[len+2*packet+2])<<8
						| (0xFF&pbData[len+2*packet+3])) == 0x1BA)    
					{    
						off=len;   
					}      
					else             
					{                 
						m_restBufferLen=0;      
					}  
				}
			}       
			else    
			{       
				m_restBufferLen=0;   
			}   
		}      
		else     
		{      
			m_restBufferLen=0;   
		}  
	}

	// is offset set ?   
	if (off==-1)  
	{     
		//no then find first 3 transport packets in mediasample  
		for (int i=0; i < sampleLen-2*packet;++i)  
		{         
			if(!bProg)
			{
				if (pbData[i]==0x47 && pbData[i+packet]==0x47 && pbData[i+2*packet]==0x47) 
				{     
					//found first 3 ts packets 
					//set the offset     
					off=i;    
					break;     
				} 
			}
			else
			{
				if (((0xFF&pbData[i])<<24
					| (0xFF&pbData[i+1])<<16
					| (0xFF&pbData[i+2])<<8
					| (0xFF&pbData[i+3])) == 0x1BA &&
					((0xFF&pbData[i+packet])<<24
					| (0xFF&pbData[i+packet+1])<<16
					| (0xFF&pbData[i+packet+2])<<8
					| (0xFF&pbData[i+packet+3])) == 0x1BA &&
					((0xFF&pbData[i+2*packet])<<24
					| (0xFF&pbData[i+2*packet+1])<<16
					| (0xFF&pbData[i+2*packet+2])<<8
					| (0xFF&pbData[i+2*packet+3])) == 0x1BA)    
				{     
					//found first 3 mpeg2 es packets
					//set the offset     
					off=i;    
					break;     
				} 
			}
		}   
	} 
	
	if (off<0)   
	{       
		off=0; 
    }

	DWORD t;
	PBYTE pData = new BYTE[sampleLen];
	DWORD pos = 0;
    //loop through all transport packets in the media sample   
	for(t=off;t<(DWORD)sampleLen;t+=packet)   
	{
        //sanity check 
		if ((int)t+packet > sampleLen)
			break;
		
		if(!bProg)
		{
			//is this a transport packet   
			if(pbData[t]==0x47)     
			{     
				memcpy(&pData[pos], &pbData[t], packet);
				pos += packet;
			} 
			else
				m_PacketErrors++;
		}
		else
		{
			//is this a mpeg2 es packet   
			if (((0xFF&pbData[t])<<24
				| (0xFF&pbData[t+1])<<16
				| (0xFF&pbData[t+2])<<8
				| (0xFF&pbData[t+3])) == 0x1BA)    
			{     
				memcpy(&pData[pos], &pbData[t], packet);
				pos += packet;
			}
			else
				m_PacketErrors++;

		}
	};

	if (pos)
		if FAILED(hr = WriteBufferSample(&pData[0], pos))
		{
			delete [] pData;
			return hr;
		}

	delete [] pData;
//	off = t;

    //calculate if there's a incomplete transport packet at end of media sample   
	m_restBufferLen=(sampleLen-off); 
	if (m_restBufferLen>0) 
	{       
		m_restBufferLen/=packet;    
		m_restBufferLen *=packet;   
		m_restBufferLen=(sampleLen-off)-m_restBufferLen;   
		if (m_restBufferLen>0 && m_restBufferLen < packet)  
		{      
			//copy the incomplete packet in the rest buffer      
			memcpy(m_restBuffer,&pbData[sampleLen-m_restBufferLen],m_restBufferLen); 
		}  
	}
	return S_OK;
}//Frodo code changes

void CTSParserInputPin::ThreadProc()
{
	m_WriteThreadActive = TRUE;

	BoostThread Boost;
	
	while (!ThreadIsStopping(0))
	{
		BYTE *item = NULL;
		long sampleLen = 0;
		long size = 0;
		{
			CAutoLock BufferLock(&m_BufferLock);
			size = m_Array.size();
		}

		if (size)
		{
			CAutoLock BufferLock(&m_BufferLock);
			std::vector<CBufferInfo*>::iterator it = m_Array.begin();
			CBufferInfo *bufferInfo = *it;
			item = bufferInfo->sample;
			sampleLen = bufferInfo->size;
			m_Array.erase(it);
			delete bufferInfo;
			m_WriteBufferSize -= sampleLen;
		}
		else
		{
			Sleep(1);
			continue;
		}

		if (item)
		{
			HRESULT hr = Write(item, sampleLen);
			delete[] item;
			if (FAILED(hr))
			{
				CAutoLock BufferLock(&m_BufferLock);
				::OutputDebugString(TEXT("TSParserInputPin::ThreadProc:Write Fail."));
				std::vector<CBufferInfo*>::iterator it = m_Array.begin();
				for ( ; it != m_Array.end() ; it++ )
				{
					CBufferInfo *bufferInfo = *it;
					delete[] bufferInfo->sample;
					delete bufferInfo;
				}
				m_Array.clear();
				m_WriteBufferSize = 0;
			}
		}
//		Sleep(1);
	}
	Clear();
	return;
}

HRESULT CTSParserInputPin::WriteBufferSample(byte* pbData,long sampleLen)
{
	HRESULT hr;
	long bufferLen = 32768;// /2;
	//
	//Only start buffering if the buffer thread is active
	//
	if (!m_WriteThreadActive)
	{
		BoostThread Boost;

		hr = Write(pbData, sampleLen);
		return hr;
	}

	//
	//If buffer thread is active and the buffer is not full
	//
	if(m_WriteThreadActive && m_WriteBufferSize + sampleLen < 64000000)
	{
		//use the sample packet size for the buffer
		if(sampleLen <= bufferLen)
		{
			CBufferInfo *newItem = new CBufferInfo();
			newItem->sample = new BYTE[sampleLen];
			//Return if we are out of memory
			if (!newItem->sample)
			{
				::OutputDebugString(TEXT("TSParserInputPin::WriteBufferSample:Out of Memory."));
				return S_OK;
			}
			//store the sample in the temp buffer
			memcpy((void*)newItem->sample, &pbData[0], sampleLen);
			newItem->size = sampleLen;
			CAutoLock BufferLock(&m_BufferLock);
			m_Array.push_back(newItem);
			m_WriteBufferSize += sampleLen;
		}
		else
		{
			long pos = 0;
			//break up the sample into smaller packets
			for (long i = sampleLen; i > 0; i -= bufferLen)
			{
				long size = ((i/bufferLen) != 0)*bufferLen + ((i/bufferLen) == 0)*i;
				CBufferInfo *newItem = new CBufferInfo();
				newItem->sample = new BYTE[size];
				//Return if we are out of memory
				if (!newItem->sample)
				{
					::OutputDebugString(TEXT("TSParserInputPin::WriteBufferSample:Out of Memory."));
					return S_OK;
				}
				//store the sample in the temp buffer
				memcpy((void*)newItem->sample, &pbData[pos], size);
				newItem->size = size;
				CAutoLock BufferLock(&m_BufferLock);
				m_Array.push_back(newItem);
				m_WriteBufferSize += size;
				pos += size;
			}
		}
		return S_OK;
	}
	//else clear the buffer
	::OutputDebugString(TEXT("TSParserInputPin::WriteBufferSample:Buffer Full error."));
	CAutoLock BufferLock(&m_BufferLock);
	::OutputDebugString(TEXT("TSParserInputPin::ThreadProc:Write Fail."));
	std::vector<CBufferInfo*>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		CBufferInfo *bufferInfo = *it;
		delete[] bufferInfo->sample;
		delete bufferInfo;
	}
	m_Array.clear();
	m_WriteBufferSize = 0;
	return S_OK;
}

STDMETHODIMP CTSParserInputPin::Receive(IMediaSample *pSample)
{
    CheckPointer(pSample,E_POINTER);

    CAutoLock lock(&m_ReceiveLock);
    PBYTE pbData;

    REFERENCE_TIME tStart, tStop;
    pSample->GetTime(&tStart, &tStop);

    DbgLog((LOG_TRACE, 1, TEXT("tStart(%s), tStop(%s), Diff(%d ms), Bytes(%d)"),
           (LPCTSTR) CDisp(tStart),
           (LPCTSTR) CDisp(tStop),
           (LONG)((tStart - m_tLast) / 10000),
           pSample->GetActualDataLength()));

    m_tLast = tStart;

    HRESULT hr = pSample->GetPointer(&pbData);
    if (FAILED(hr)) {
        return hr;
    }

//	return m_pTSParserSource->Write(pbData, pSample->GetActualDataLength());
	return WriteBufferSample(pbData,pSample->GetActualDataLength()); 

//Frodo code changes
//	return m_pTSParserSource->Write(pbData, pSample->GetActualDataLength());

//	return Filter(pbData,pSample->GetActualDataLength()); 
	
//Frodo code changes

}

STDMETHODIMP CTSParserInputPin::EndOfStream(void)
{
	Clear();
    CAutoLock lock(&m_ReceiveLock);
    return CRenderedInputPin::EndOfStream();

}

STDMETHODIMP CTSParserInputPin::NewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
	Clear();
	m_PacketErrors = 0;
	m_restBufferLen=0;
	m_writeBufferSize = sizeof(m_writeBuffer);
    m_writeBufferLen = 0;
	m_tLast = 0;
    return S_OK;

}

__int64 CTSParserInputPin::getNumbErrorPackets()
{
    return m_PacketErrors;
}

void CTSParserInputPin::setNumbErrorPackets(__int64 lpllErrors)
{
    m_PacketErrors = lpllErrors;
}

void CTSParserInputPin::PrintLongLong(LPCTSTR lstring, __int64 value)
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

HRESULT CTSParserInputPin::OpenFile()
{
//    CAutoLock lock(&m_Lock);

    // Has a filename been set yet
    if (m_pFileName == NULL)
	{
        return ERROR_INVALID_NAME;
    }

	HRESULT hr = m_pFileWriter->OpenFile(m_pFileName);

    return S_OK;
}

HRESULT CTSParserInputPin::CloseFile()
{
    // Must lock this section to prevent problems related to
    // closing the file while still receiving data in Receive()
//    CAutoLock lock(&m_Lock);
    CAutoLock lock(&m_ReceiveLock);

	m_pFileWriter->CloseFile();

    return NOERROR;
}

HRESULT CTSParserInputPin::Write(PBYTE pbData, LONG lDataLength)
{
//    CAutoLock lock(&m_Lock);

	HRESULT hr = m_pFileWriter->Write(pbData, lDataLength);

    return S_OK;
}

STDMETHODIMP CTSParserInputPin::SetFileName(LPCWSTR pszFileName, const AM_MEDIA_TYPE *pmt)
{
    CheckPointer(pszFileName,E_POINTER);

    if(wcslen(pszFileName) == 0 && strlen(m_pRegFileName) > 0)
	{
		if(strlen(m_pRegFileName) > MAX_PATH)
			return ERROR_FILENAME_EXCED_RANGE;

		if (m_pFileName)
			delete[] m_pFileName;

		long length = lstrlen(m_pRegFileName);

		// Check that the filename ends with .tsbuffer. If it doesn't we'll add it
		if ((length < 9) || (stricmp(m_pRegFileName+length-9, ".tsbuffer") != 0))
		{
			m_pFileName = new wchar_t[1+length+9];
			if (m_pFileName == 0)
				return E_OUTOFMEMORY;

			swprintf(m_pFileName, L"%S.tsbuffer", m_pRegFileName);
		}
		else
		{
			m_pFileName = new WCHAR[1+length];
			if (m_pFileName == 0)
				return E_OUTOFMEMORY;

			swprintf(m_pFileName, L"%S", m_pRegFileName);
		}
	}
	else
	{

		if(wcslen(pszFileName) > MAX_PATH)
			return ERROR_FILENAME_EXCED_RANGE;

		if (m_pFileName)
			delete[] m_pFileName;

		long length = lstrlenW(pszFileName);

		// Check that the filename ends with .tsbuffer. If it doesn't we'll add it
		if ((length < 9) || (_wcsicmp(pszFileName+length-9, L".tsbuffer") != 0))
		{
			m_pFileName = new wchar_t[1+length+9];
			if (m_pFileName == 0)
				return E_OUTOFMEMORY;

			swprintf(m_pFileName, L"%s.tsbuffer", pszFileName);
		}
		else
		{
			m_pFileName = new WCHAR[1+length];
			if (m_pFileName == 0)
				return E_OUTOFMEMORY;

			wcscpy(m_pFileName, pszFileName);
		}
	}
    return S_OK;
}

STDMETHODIMP CTSParserInputPin::GetCurFile(LPOLESTR * ppszFileName, AM_MEDIA_TYPE *pmt)
{
    CheckPointer(ppszFileName, E_POINTER);
    *ppszFileName = NULL;

    if (m_pFileName != NULL) 
    {
		//QzTask = CoTask
        *ppszFileName = (LPOLESTR)CoTaskMemAlloc(sizeof(WCHAR) * (1+lstrlenW(m_pFileName)));

        if (*ppszFileName != NULL)
        {
            wcscpy(*ppszFileName, m_pFileName);
        }
    }

    if(pmt)
    {
        ZeroMemory(pmt, sizeof(*pmt));
        pmt->majortype = MEDIATYPE_NULL;
        pmt->subtype = MEDIASUBTYPE_NULL;
    }

    return S_OK;

}
/*
	{
		StartThread();
		Sleep(2000);
		LPWSTR wsz = new WCHAR[1+lstrlenW(m_pFileName)];
		if (wsz == NULL)
			return E_OUTOFMEMORY;

		wcscpy(wsz, m_pFileName);
		m_pTSParserSourceFilter->Load(wsz, NULL);
		delete[] wsz;
	}

*/