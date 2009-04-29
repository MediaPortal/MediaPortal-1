/**
*  TSFileSinkPin.cpp
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
#include "TSFileSink.h"
#include "TSFileSinkGuids.h"
#include "BDATYPES.H"
#include "KS.H"
#include "KSMEDIA.H"
#include "BDAMedia.h"
#include "global.h"

//////////////////////////////////////////////////////////////////////
// CTSFileSinkPin
//////////////////////////////////////////////////////////////////////
CTSFileSinkPin::CTSFileSinkPin(CTSFileSink *pTSFileSink, LPUNKNOWN pUnk, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr) :
	CRenderedInputPin(NAME("Input Pin"), pFilter, pLock, phr, L"In"),
	m_pTSFileSink(pTSFileSink),
    m_tLast(0)
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

}

CTSFileSinkPin::~CTSFileSinkPin()
{
	Clear();
	delete[] m_writeBuffer;
}

void CTSFileSinkPin::Clear()
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

HRESULT CTSFileSinkPin::CheckMediaType(const CMediaType *)
{
    return S_OK;
}

HRESULT CTSFileSinkPin::BreakConnect()
{
	Clear();

    if (m_pTSFileSink->m_pPosition != NULL) {
        m_pTSFileSink->m_pPosition->ForceRefresh();
    }
//Frodo code changes
    m_restBufferLen=0;
//Frodo code changes
	m_writeBufferSize = sizeof(m_writeBuffer);
    m_writeBufferLen = 0;

    return CRenderedInputPin::BreakConnect();
}

HRESULT CTSFileSinkPin::Run(REFERENCE_TIME tStart)
{
	if (!m_WriteThreadActive && IsConnected())
		StartThread();

	return CBaseInputPin::Run(tStart);
}

STDMETHODIMP CTSFileSinkPin::ReceiveCanBlock()
{
    return S_FALSE;
}

//Frodo code changes
HRESULT CTSFileSinkPin::Filter(byte* pbData,long sampleLen)
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

void CTSFileSinkPin::ThreadProc()
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
			HRESULT hr = m_pTSFileSink->Write(item, sampleLen);
			delete[] item;
			if (FAILED(hr))
			{
				CAutoLock BufferLock(&m_BufferLock);
				::OutputDebugString(TEXT("TSFileSinkPin::ThreadProc:Write Fail."));
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

HRESULT CTSFileSinkPin::WriteBufferSample(byte* pbData,long sampleLen)
{
	HRESULT hr;
	long bufferLen = 32768;// /2;
	//
	//Only start buffering if the buffer thread is active
	//
	if (!m_WriteThreadActive)
	{
		BoostThread Boost;

		hr = m_pTSFileSink->Write(pbData, sampleLen);
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
				::OutputDebugString(TEXT("TSFileSinkPin::WriteBufferSample:Out of Memory."));
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
					::OutputDebugString(TEXT("TSFileSinkPin::WriteBufferSample:Out of Memory."));
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
	::OutputDebugString(TEXT("TSFileSinkPin::WriteBufferSample:Buffer Full error."));
	CAutoLock BufferLock(&m_BufferLock);
	::OutputDebugString(TEXT("TSFileSinkPin::ThreadProc:Write Fail."));
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

STDMETHODIMP CTSFileSinkPin::Receive(IMediaSample *pSample)
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

//	return m_pTSFileSink->Write(pbData, pSample->GetActualDataLength());
	return WriteBufferSample(pbData,pSample->GetActualDataLength()); 

//Frodo code changes
//	return m_pTSFileSink->Write(pbData, pSample->GetActualDataLength());

//	return Filter(pbData,pSample->GetActualDataLength()); 
	
//Frodo code changes

}

STDMETHODIMP CTSFileSinkPin::EndOfStream(void)
{
	Clear();
    CAutoLock lock(&m_ReceiveLock);
    return CRenderedInputPin::EndOfStream();

}

STDMETHODIMP CTSFileSinkPin::NewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
	Clear();
	m_PacketErrors = 0;
	m_restBufferLen=0;
	m_writeBufferSize = sizeof(m_writeBuffer);
    m_writeBufferLen = 0;
	m_tLast = 0;
    return S_OK;

}

__int64 CTSFileSinkPin::getNumbErrorPackets()
{
    return m_PacketErrors;
}

void CTSFileSinkPin::setNumbErrorPackets(__int64 lpllErrors)
{
    m_PacketErrors = lpllErrors;
}

void CTSFileSinkPin::PrintLongLong(LPCTSTR lstring, __int64 value)
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
