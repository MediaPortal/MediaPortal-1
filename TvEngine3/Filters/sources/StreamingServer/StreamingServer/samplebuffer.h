/**
*  SampleBuffer.h
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

#ifndef SAMPLEBUFFER_H
#define SAMPLEBUFFER_H

#include <vector>
#include "Global.h"

/**********************************************
 *
 *  CSampleBuffer Class
 *
 **********************************************/

class CSampleBuffer
{
public:
	CSampleBuffer();
	virtual ~CSampleBuffer();

	void Clear();
	long Count();
	HRESULT LoadMediaSample(IMediaSample *pSample);
	HRESULT ReadSampleBuffer(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes);
	HRESULT Require(long nBytes, BOOL bIgnoreDelay = FALSE);

	HRESULT DequeFromBuffer(BYTE *pbData, long lDataLength);
	HRESULT ReadFromBuffer(BYTE *pbData, long lDataLength, long lOffset);
	void PrintLongLong(LPCTSTR lstring, __int64 value);

protected:
	std::vector<CBufferInfo *> m_Array;
	long m_lItemOffset;
	CCritSec m_BufferLock;

	long m_lSampleBufferItemSize;
	int debugcount;
	long m_lbuflen;
};

#endif
