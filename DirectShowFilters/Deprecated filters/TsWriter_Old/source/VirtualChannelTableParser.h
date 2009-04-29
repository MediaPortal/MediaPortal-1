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
#include "channelinfo.h"
#include "pidtable.h"
#include "tsHeader.h"
#include <vector>
using namespace std;

class IAtscCallback
{
public:
	virtual void OnChannel(const CChannelInfo& info)=0;
};

#define PID_VCT 0x1ffb

class CVirtualChannelTableParser : ISectionCallback
{
public:
  CVirtualChannelTableParser(void);
  virtual ~CVirtualChannelTableParser(void);

  void  Reset();
	void OnNewSection(int pid, int tableId, CSection& section);

  int   Count();
  bool  GetChannelInfo(int serviceId,CChannelInfo& info);
	bool  GetChannel(int index,CChannelInfo& info);
  void  OnTsPacket(byte* tsPacket);
  void SetCallback(IAtscCallback* callback);
private:
  void DecodeServiceLocationDescriptor( byte* buf,int start,CChannelInfo& channelInfo);
  void DecodeExtendedChannelNameDescriptor( byte* buf,int start,CChannelInfo& channelInfo, int maxLen);
  char* DecodeMultipleStrings(byte* buf, int offset, int maxLen);
  char* DecodeString(byte* buf, int offset, int compression_type, int mode, int number_of_bytes);
  vector<CChannelInfo> m_vecChannels;
  int m_iVctVersionC8;
	int m_iVctVersionC9;
  CSectionDecoder* m_decoder[2];
  IAtscCallback* m_pCallback;
  CTsHeader             m_tsHeader;
};
