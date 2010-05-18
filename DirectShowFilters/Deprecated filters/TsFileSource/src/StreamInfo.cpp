/**
*  StreamInfo.cpp
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

#include "stdafx.h"
#include "StreamInfo.h"
#include <crtdbg.h>

StreamInfo::StreamInfo()
{
	Clear();
}

StreamInfo::~StreamInfo()
{
}

void StreamInfo::Clear()
{
	Vid   = false;
	H264  = false;
	Mpeg4 = false;
	Aud   = false;
	Aud2  = false;
	AC3   = false;
	AAC   = false;
	DTS   = false;
	Pid   = 0;
	ZeroMemory(&media, sizeof(AM_MEDIA_TYPE));
	flags = 0;
	lcid  = 0;
	group = 0;
	ZeroMemory(name, sizeof(name));
	object = 0;
	unk   = 0;
}

void StreamInfo::CopyFrom(StreamInfo *StreamInfo)
{
	Vid   = StreamInfo->Vid;
	H264  = StreamInfo->H264;
	Mpeg4   = StreamInfo->Mpeg4;
	Aud   = StreamInfo->Aud;
	Aud2  = StreamInfo->Aud2;
	AC3   = StreamInfo->AC3;
	AAC   = StreamInfo->AAC;
	DTS   = StreamInfo->DTS;
	Pid   = StreamInfo->Pid;
	memcpy(&media, &StreamInfo->media, sizeof(AM_MEDIA_TYPE));
	flags   = StreamInfo->flags;
	lcid  = StreamInfo->lcid;
	group   = StreamInfo->group;
	memcpy(name, StreamInfo->name, sizeof(StreamInfo->name));
	object	= StreamInfo->object;
	unk   = StreamInfo->unk;
}

void StreamInfo::CopyTo(StreamInfo *StreamInfo)
{
}

StreamInfoArray::StreamInfoArray()
{
}

StreamInfoArray::~StreamInfoArray()
{
	Clear();
}

void StreamInfoArray::Clear()
{
//	CAutoLock StreamInfoLock(&m_StreamInfoLock);
	std::vector<StreamInfo *>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		delete *it;
	}
	m_Array.clear();
}

void StreamInfoArray::Add(StreamInfo *newStreamInfo)
{
//	CAutoLock StreamInfoLock(&m_StreamInfoLock);
	m_Array.push_back(newStreamInfo);
}

void StreamInfoArray::RemoveAt(int nPosition)
{
//	CAutoLock StreamInfoLock(&m_StreamInfoLock);
	if ((nPosition >= 0) && (nPosition < (int)m_Array.size()))
	{
		m_Array.erase(m_Array.begin() + nPosition);
	}
}

StreamInfo &StreamInfoArray::operator[](int nPosition)
{
//	CAutoLock StreamInfoLock(&m_StreamInfoLock);
	int size = m_Array.size();
	_ASSERT(nPosition >= 0);
	_ASSERT(nPosition < size);

	return *m_Array.at(nPosition);
}

int StreamInfoArray::Count()
{
//	CAutoLock StreamInfoLock(&m_StreamInfoLock);
	return m_Array.size();
}

