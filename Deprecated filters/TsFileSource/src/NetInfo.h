/**
*  NetInfo.h
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


#ifndef NETINFO_H
#define NETINFO_H

#include <vector>
#include <objbase.h>
#include "NetRender.h"
#include <time.h>
#include <sys/types.h>
#include <sys/timeb.h>

class NetInfo
{

public:
	NetInfo();
	virtual ~NetInfo();

	void Clear();
	void CopyFrom(NetInfo *netInfo);
	void CopyTo(NetInfo *netInfo);

	ULONG userIP;
	USHORT userPort;
	ULONG  userNic;

	WCHAR fileName[MAX_PATH];
	WCHAR pathName[MAX_PATH];
	WCHAR strIP[15];
	WCHAR strPort[5];
	WCHAR strNic[15];
	WCHAR strTime[32];

	IGraphBuilder * pNetworkGraph;
	IBaseFilter *pNetSource;
	IBaseFilter *pFileSink;
	BOOL rotEnable;
    DWORD dwGraphRegister;
	BOOL playing;
	int retry;
	__int64 buffSize;
	__int64 flowRate;
	REFERENCE_TIME lastTime;
	time_t time;
	struct tm *tmTime;
	BOOL bParserSink;

};

class NetInfoArray
{
public:
	NetInfoArray();
	virtual ~NetInfoArray();

	void Clear();
	void Add(NetInfo *newNetInfo);
	void RemoveAt(int nPosition);

	NetInfo &operator[](int nPosition);
	int Count();

private:
	std::vector<NetInfo *> m_NetArray;

};

#endif
