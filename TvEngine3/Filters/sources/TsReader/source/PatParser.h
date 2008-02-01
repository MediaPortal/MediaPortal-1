/* 
 *	Copyright (C) 2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#pragma once

#include "..\..\shared\sectiondecoder.h"
#include "PmtParser.h"
#include "channelinfo.h"
#include <vector>
using namespace std;

class IPatParserCallback
{
public:
	virtual void OnNewChannel(CChannelInfo& info)=0;
};

class CPatParser : public CSectionDecoder
{
public:
	enum PatState
	{
		Idle,
		Parsing,
	};
  CPatParser(void);
  virtual ~CPatParser(void);
  void        SkipPacketsAtStart(__int64 packets);
	void	      OnTsPacket(byte* tsPacket);
  void        Reset();
	void        OnNewSection(CSection& section);
  int         Count();
  bool        GetChannel(int index, CChannelInfo& info);
  void        Dump();
	void        SetCallBack(IPatParserCallback* callback);
private:
  void        CleanUp();
	IPatParserCallback* m_pCallback;
  vector<CPmtParser*> m_pmtParsers;
  __int64     m_packetsReceived;
  __int64     m_packetsToSkip;
	int					m_iPatTableVersion;
	PatState		m_iState;
};
