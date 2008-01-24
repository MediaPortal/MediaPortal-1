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
#include "sectiondecoder.h"
#include "isectioncallback.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "TsHeader.h"
#include <string>
#include "dvbutil.h"
#include <vector>
#include <map>
using namespace std;
using namespace Mediaportal;


#define PID_EPG 0x12

typedef struct stLinkedChannel
{
	int     network_id;
	int     transport_id;
	int     service_id;
	string name;
}LinkedChannel;

typedef struct stPortalChannel
{
	bool    allSectionsReceived;
	int     last_section_number;
	int     original_network_id;
	int     transport_id;
	int     service_id;

	vector<LinkedChannel> m_linkedChannels;

	typedef vector<LinkedChannel>::iterator ilinkedChannels;

	map<int,bool> mapSectionsReceived;
	typedef map<int,bool>::iterator imapSectionsReceived;
}PortalChannel;

class CChannelLinkageParser :  public ISectionCallback, public CDvbUtil
{
public:
  CChannelLinkageParser(void);
  virtual ~CChannelLinkageParser(void);

  void Start();
  void  Reset();
  bool IsScanningDone();

  ULONG GetChannelCount();
  void GetChannel (ULONG channelIndex, WORD* network_id, WORD* transport_id,WORD* service_id  );
  ULONG GetLinkedChannelsCount (ULONG channel);
  void GetLinkedChannel (ULONG channelIndex, ULONG linkIndex, WORD* network_id, WORD* transport_id,WORD* service_id, char** channelName  );

  void  OnTsPacket(CTsHeader& header,byte* tsPacket);
  void  OnNewSection(int pid, int tableId, CSection& section); 


private:
    vector<CSectionDecoder*> m_vecDecoders;
	CCriticalSection m_section;
    CTsHeader             m_tsHeader;

	bool	m_bScanning;
	bool	m_bScanningDone;
	time_t  m_scanTimeout;

	bool GetChannelByindex(ULONG channel, PortalChannel& portalChannel);
	void DecodeLinkage(byte* buf, int len);
	map<unsigned long,PortalChannel> m_mapChannels;
	typedef map<unsigned long,PortalChannel>::iterator imapChannels;
	long	   m_prevChannelIndex;
	long	   m_prevLinkIndex;
	PortalChannel m_prevChannel;
	LinkedChannel   m_prevLink;
};
