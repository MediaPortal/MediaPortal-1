/**
*  PidInfo.cpp
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

#include <streams.h>
#include "PidInfo.h"
#include <crtdbg.h>
#include <WXUtil.h>


PidInfo::PidInfo()
{
	Clear();
}

PidInfo::~PidInfo()
{
}

void PidInfo::Clear()
{
	vid   = 0;
	h264  = 0;
	mpeg4 = 0;
	aud   = 0;
	aud2  = 0;
	aac   = 0;
	aac2  = 0;
	dts   = 0;
	dts2  = 0;
	ac3   = 0;
	ac3_2   = 0;
	chnumb	= 0;
	ZeroMemory(onetname, 128);
	ZeroMemory(chname, 128);
	ZeroMemory(sdesc, 128);
	ZeroMemory(edesc, 600);
	ZeroMemory(sndesc, 128);
	ZeroMemory(endesc, 600);

	txt   = 0;
	sub   = 0;
	sid   = 0;
	pmt   = 0;
	pcr   = 0;
	opcr  = 0;
	start = 0;
	end   = 0;
	dur   = 0; //1000000;
	bitrate = 10000000; //Set reasonable value
	for (int i = 0 ; i < 16 ; i++ )
	{
		TsArray[i] = 0;
	}
}

void PidInfo::CopyFrom(PidInfo *pidInfo)
{
	vid   = pidInfo->vid;
	h264  = pidInfo->h264;
	mpeg4 = pidInfo->mpeg4;
	aud   = pidInfo->aud;
	aud2  = pidInfo->aud2;
	aac   = pidInfo->aac;
	aac2  = pidInfo->aac2;
	dts   = pidInfo->dts;
	dts2  = pidInfo->dts2;
	ac3   = pidInfo->ac3;
	ac3_2   = pidInfo->ac3_2;
	chnumb	= pidInfo->chnumb;
	memcpy(onetname, pidInfo->onetname, 128);
	memcpy(chname, pidInfo->chname, 128);
	memcpy(sdesc, pidInfo->sdesc, 128);
	memcpy(edesc, pidInfo->edesc, 600);
	memcpy(sndesc, pidInfo->sndesc, 128);
	memcpy(endesc, pidInfo->endesc, 600);

	txt   = pidInfo->txt;
	sub   = pidInfo->sub;
	sid   = pidInfo->sid;
	pmt   = pidInfo->pmt;
	pcr   = pidInfo->pcr;
	opcr   = pidInfo->opcr;
	start = pidInfo->start;
	end   = pidInfo->end;
	dur   = pidInfo->dur;

	bitrate = pidInfo->bitrate; 
	for (int i = 0 ; i < 16 ; i++ )
	{
		TsArray[i] = pidInfo->TsArray[i];
	}
}

void PidInfo::CopyTo(PidInfo *pidInfo)
{
	pidInfo->vid = vid;
	pidInfo->h264 = h264;
	pidInfo->mpeg4 = mpeg4;
	pidInfo->aud = aud;
	pidInfo->aud2 = aud2;
	pidInfo->aac = aac;
	pidInfo->aac2 = aac2;
	pidInfo->dts = dts;
	pidInfo->dts2 = dts2;
	pidInfo->ac3 = ac3;
	pidInfo->ac3_2 = ac3_2;
	pidInfo->chnumb = chnumb;
	memcpy(pidInfo->onetname, onetname, 128);
	memcpy(pidInfo->chname, chname, 128);
	memcpy(pidInfo->sdesc, sdesc, 128);
	memcpy(pidInfo->edesc, edesc, 600);
	memcpy(pidInfo->sndesc, sndesc, 128);
	memcpy(pidInfo->endesc, endesc, 600);

	pidInfo->txt = txt;
	pidInfo->sub = sub;
	pidInfo->sid = sid;
	pidInfo->pmt = pmt;
	pidInfo->pcr = pcr;
	pidInfo->opcr = opcr;
	pidInfo->start = start;
	pidInfo->end = end;
	pidInfo->dur = dur;
	pidInfo->bitrate = bitrate;

	for (int i = 0 ; i < 16 ; i++ )
	{
		pidInfo->TsArray[i] = TsArray[i];
	}
}

PidInfoArray::PidInfoArray()
{
}

PidInfoArray::~PidInfoArray()
{
//	CAutoLock arraylock(&m_ArrayLock);
	Clear();
}

void PidInfoArray::Clear()
{
//	CAutoLock arraylock(&m_ArrayLock);

	std::vector<PidInfo *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		delete *it;
	}
	m_Array.clear();
}

void PidInfoArray::Add(PidInfo *newPidInfo)
{
//	CAutoLock arraylock(&m_ArrayLock);
	m_Array.push_back(newPidInfo);
}

void PidInfoArray::RemoveAt(int nPosition)
{
//	CAutoLock arraylock(&m_ArrayLock);
	if ((nPosition >= 0) && (nPosition < (int)m_Array.size()))
	{
		m_Array.erase(m_Array.begin() + nPosition);
	}
}

PidInfo &PidInfoArray::operator[](int nPosition)
{
//	CAutoLock arraylock(&m_ArrayLock);
	int size = m_Array.size();
	_ASSERT(nPosition >= 0);
	_ASSERT(nPosition < size);

	return *m_Array.at(nPosition);
}

int PidInfoArray::Count()
{
//	CAutoLock arraylock(&m_ArrayLock);
	return m_Array.size();
}

