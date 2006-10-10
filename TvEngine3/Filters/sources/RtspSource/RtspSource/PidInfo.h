/**
*  PidInfo.h
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


#ifndef PIDINFO_H
#define PIDINFO_H

#include <vector>

class PidInfo
{
public:
	PidInfo();
	virtual ~PidInfo();

	void Clear();
	void CopyFrom(PidInfo *pidInfo);
	void CopyTo(PidInfo *pidInfo);

	int vid;
	int h264;
	int mpeg4;
	int aud;
	int aud2;
	int aac;
	int aac2;
	int dts;
	int dts2;
	int ac3;
	int ac3_2;
	int chnumb;
	int txt;
	int sub;
	int sid;
	int pmt;
	int pcr;
	int opcr;
	long bitrate;
	__int64 start;
	__int64 end;
	__int64 dur;
	unsigned char chname[128];
	unsigned char onetname[128];
	unsigned char sdesc[128];
	unsigned char edesc[600];
	unsigned char sndesc[128];
	unsigned char endesc[600];
	unsigned long TsArray[16];
};

class PidInfoArray
{
	
public:
	PidInfoArray();
	virtual ~PidInfoArray();

	void Clear();
	void Add(PidInfo *newPidInfo);
	void RemoveAt(int nPosition);

	PidInfo &operator[](int nPosition);
	int Count();

private:
	std::vector<PidInfo *> m_Array;

	CCritSec m_ArrayLock;

};

#endif
