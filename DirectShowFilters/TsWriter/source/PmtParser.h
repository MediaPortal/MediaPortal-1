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

#include "..\..\shared\BasePmtParser.h"

using namespace std;

typedef struct stPidInfo2
{
public:
	int elementaryPid;
	int fakePid;
	int streamType;
	int logicalStreamType;
	byte rawDescriptorData[1024];
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
	virtual void OnPmtReceived2(int pid, int serviceId, int pcrPid, bool hasCaDescriptor, vector<PidInfo2> info)=0;
};

class CPmtParser: public CBasePmtParser
{
public:
  CPmtParser(void);
  virtual ~CPmtParser(void);
	bool		DecodePmt(CSection sections, int& pcr_pid, bool& hasCaDescriptor, vector<PidInfo2>& pidInfos);
	void		OnNewSection(CSection& sections);
	void    SetPmtCallBack2(IPmtCallBack2* callback);
	void	  Reset();

private:
	IPmtCallBack2* m_pmtCallback2;
  //FIXME: this older code version is only for backward compatibility with dependent classes.
  //       proper fix is to change code of all classes that depend on PidInfo2 in favour of CPidTable!
	vector<PidInfo2> m_pidInfos2;
};
