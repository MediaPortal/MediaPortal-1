/**
*  StreamInfo.h
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


#ifndef STREAMINFO_H
#define STREAMINFO_H

#include <vector>

class StreamInfo
{
public:
	StreamInfo();
	virtual ~StreamInfo();

	void Clear();
	void CopyFrom(StreamInfo *StreamInfo);
	void CopyTo(StreamInfo *StreamInfo);

	bool Vid;
	bool H264;
	bool Mpeg4;
	bool Aud;
	bool Aud2;
	bool AC3;
	bool AAC;
	bool DTS;
	int  Pid;
	AM_MEDIA_TYPE media;
	DWORD flags;
	LCID lcid;
	DWORD group;
	WCHAR name[256];
	DWORD object;
	DWORD unk;
};

class StreamInfoArray
{
public:
	StreamInfoArray();
	virtual ~StreamInfoArray();

	void Clear();
	void Add(StreamInfo *newStreamInfo);
	void RemoveAt(int nPosition);

	StreamInfo &operator[](int nPosition);
	int Count();

private:
	CCritSec m_StreamInfoLock;
	std::vector<StreamInfo *> m_Array;

};

#endif
