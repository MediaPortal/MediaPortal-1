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
#include "PidTable.h"
#include "..\..\shared\tsheader.h"
#include "pidtable.h"
#include <map>
#include <vector>
using namespace std;

#define SERVICE_TYPE_VIDEO_MPEG1		0x1
#define SERVICE_TYPE_VIDEO_MPEG2		0x2
#define SERVICE_TYPE_VIDEO_MPEG2_DCII	0x80
#define SERVICE_TYPE_VIDEO_MPEG4		0x10
#define SERVICE_TYPE_VIDEO_H264		  0x1b
#define SERVICE_TYPE_AUDIO_MPEG1		0x3
#define SERVICE_TYPE_AUDIO_MPEG2		0x4
#define SERVICE_TYPE_AUDIO_AC3			0x81 //fake
#define SERVICE_TYPE_AUDIO_E_AC3		0x84 //fake
#define SERVICE_TYPE_AUDIO_AAC			0x0f
#define SERVICE_TYPE_AUDIO_LATM_AAC	0x11 //LATM AAC audio

#define SERVICE_TYPE_DVB_SUBTITLES1		0x5
#define SERVICE_TYPE_DVB_SUBTITLES2		0x6
//#define SERVICE_TYPE_TELETEXT			0x6; // ?

#define DESCRIPTOR_DVB_AC3				0x6a
#define DESCRIPTOR_DVB_E_AC3			0x7a
#define DESCRIPTOR_DVB_TELETEXT			0x56
#define DESCRIPTOR_DVB_SUBTITLING		0x59
#define DESCRIPTOR_MPEG_ISO639_Lang		0x0a
#define DESCRIPTOR_STREAM_IDENTIFIER	0x52

typedef struct stPidInfo2
{
public:
	int elementaryPid;
	int fakePid;
	int streamType;
	int logicalStreamType;
	byte rawDescriptorData[400];
	int rawDescriptorSize;
	bool seenStart;
	byte TsPktQ[4][188] ;
	int  NPktQ ;
	CTsHeader TsHeaderQ[4] ;
	byte PesHeader[19] ;
	int  PesHeaderLength ;
	byte ccPrev ;
	byte m_Pkt[188] ;
}PidInfo2;

typedef vector<stPidInfo2>::iterator ivecPidInfo2;

class IPmtCallBack2
{
public:
	virtual void OnPmtReceived2(int pid, int serviceId, int pcrPid,vector<PidInfo2> info)=0;
};

class CPmtParser: public  CSectionDecoder
{
public:
  CPmtParser(void);
  virtual ~CPmtParser(void);
	void    OnTsPacket(byte* tsPacket);
	bool		DecodePmt(CSection sections, int& pcr_pid, vector<PidInfo2>& pidInfos);
	void		OnNewSection(CSection& sections);
	void    SetPmtCallBack2(IPmtCallBack2* callback);
	void	  Reset();
  bool    IsReady();
	void		SetFilter(int pid,int serviceId);
	void		GetFilter(int &pid,int &serviceId);
private:
  int				m_pmtPid;
	int				m_serviceId;
	bool			_isFound;
	IPmtCallBack2* m_pmtCallback2;
  CTsHeader             m_tsHeader;
	vector<PidInfo2> m_pidInfos2;
};
