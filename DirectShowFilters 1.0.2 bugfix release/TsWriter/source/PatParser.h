/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
#include "sdtParser.h"
#include "NitDecoder.h"
#include "channelinfo.h"
#include "VirtualChannelTableParser.h"
#include "..\..\shared\tsheader.h"
//#include "conditionalAccess.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include <vector>
#include <map>
using namespace std;
using namespace Mediaportal;


DECLARE_INTERFACE_(IChannelScanCallback, IUnknown)
{
	STDMETHOD(OnScannerDone)()PURE;
};

#define PID_PAT 0x0

class CPatParser : public CSectionDecoder, public ISdtCallBack, public IAtscCallback, public IPmtCallBack2
{
public:
  CPatParser(void);
  virtual ~CPatParser(void);

	void	OnTsPacket(byte* tsPacket);
  void  Reset(IChannelScanCallback* callback,bool waitForVCT);
	void  OnNewSection(CSection& section);

  BOOL        IsReady();
  int         Count();
  bool        GetChannel(int index, CChannelInfo& info);
  void        Dump();
	void        OnSdtReceived(const CChannelInfo& sdtInfo);
	void				OnPmtReceived2(int pid,int serviceId, int pcrPid,vector<PidInfo2> pidInfo);
  void        OnChannel(const CChannelInfo& info);

private:
	CCriticalSection m_section;
  CVirtualChannelTableParser m_vctParser;
  CSdtParser                 m_sdtParser;
	CNITDecoder                m_nitDecoder;

  void                       CleanUp();
	void											 AnalyzePidInfo(vector<PidInfo2> pidInfo,int &hasVideo, int &hasAudio);
	bool											 PmtParserExists(int pid,int serviceId);
	
	vector<CPmtParser*>	m_pmtParsers;
  typedef vector<CPmtParser*> ::iterator itPmtParser;

  map<int,CChannelInfo> m_mapChannels;
  typedef map<int,CChannelInfo> ::iterator itChannels;
  bool m_bDumped;
	IChannelScanCallback* m_pCallback;
  DWORD                 m_tickCount;
  CTsHeader             m_tsHeader;
	bool									m_finished;
	bool									m_waitForVCT;
};
