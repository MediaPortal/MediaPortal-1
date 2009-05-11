/**
*  TSFileSinkPin.h
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

#ifndef TSFILESINKPIN_H
#define TSFILESINKPIN_H

#include <vector>
#include "TSThread.h"	
#include "Global.h"

/**********************************************
 *
 *  CTSFileSinkPin Class
 *
 **********************************************/

class CTSFileSinkPin : public CRenderedInputPin, public TSThread
{
public:
	CTSFileSinkPin(CTSFileSink *pTSFileSink, LPUNKNOWN pUnk, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr);
	~CTSFileSinkPin();

	STDMETHODIMP Receive(IMediaSample *pSample);
    STDMETHODIMP EndOfStream(void);
    STDMETHODIMP ReceiveCanBlock();

    HRESULT CheckMediaType(const CMediaType *);

	__int64 getNumbErrorPackets(void);
	void setNumbErrorPackets(__int64 lpllErrors);

    HRESULT BreakConnect();
	HRESULT Run(REFERENCE_TIME tStart);

    STDMETHODIMP NewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate);
	virtual void ThreadProc();
	void Clear();

private:
	CTSFileSink * const m_pTSFileSink;
    REFERENCE_TIME m_tLast;             // Last sample receive time

    CCritSec m_ReceiveLock;

	std::vector<CBufferInfo*> m_Array;
	__int64 m_writeBufferSize;
	CCritSec m_BufferLock;

//Changes by Frodo
	HRESULT Filter(byte* rawData,long len);
	HRESULT WriteBufferSample(byte* pbData,long sampleLen);

	BYTE  m_restBuffer[4096];
	long  m_restBufferLen;
	long  m_writeBufferLen;
	BYTE*  m_writeBuffer;
	__int64 m_PacketErrors;

	long  m_WriteSampleSize;
	long  m_WriteBufferSize;
	void PrintLongLong(LPCTSTR lstring, __int64 value);
	BOOL m_WriteThreadActive;
	int debugcount;

};

#endif
