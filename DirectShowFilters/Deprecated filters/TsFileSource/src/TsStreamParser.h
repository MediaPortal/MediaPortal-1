/**
*  StreamParser.h
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
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
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#ifndef TSSTREAMPARSER_H
#define TSSTREAMPARSER_H

#include "StreamInfo.h"
#include "PidParser.h"
#include "PidInfo.h"
#include "Demux.h"
#include "NetInfo.h"


class TsStreamParser
{
public:

	TsStreamParser(PidParser *pPidParser, Demux * pDemux, NetInfoArray *pNetArray);
	virtual ~TsStreamParser();

	HRESULT ParsePidArray();
	void SetStreamActive(int group);
	bool IsStreamActive(int index);
	void set_StreamIndex(WORD streamIndex);
	WORD get_StreamIndex();
	void set_StreamArray(int streamIndex);

	StreamInfo streams;
	StreamInfoArray StreamArray;	//Currently selected streams

protected:

	CCritSec m_StreamLock;

	void LoadStreamArray(int cnt);
	void AddStreamArray();
	void SetStreamArray(int n);

	int	m_StreamIndex;

	PidParser *m_pPidParser;
	Demux *	m_pDemux;
	NetInfoArray *m_pNetArray;

};

#endif
