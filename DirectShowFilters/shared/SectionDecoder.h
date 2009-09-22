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
#include "ISectionCallback.h"
#include "dvbutil.h"
#include "Section.h"
#include "TsHeader.h"

#define MAX_SECTIONS 256

class CSectionDecoder : public CDvbUtil
{
public:
  CSectionDecoder(void);
  ~CSectionDecoder(void);
	void SetCallBack(ISectionCallback* callback);
	void OnTsPacket(byte* tsPacket);
	void OnTsPacket(byte* tsPacket, bool getAllSections);
	void OnTsPacket(CTsHeader& header,byte* tsPacket);
  void SetPid(int pid);
  int  GetPid();
	void Reset();
  void EnableLogging(bool onOff);
  void EnableCrcCheck(bool onOff);
  virtual void OnNewSection(CSection& section);
protected:
private:
	int StartNewSection(byte* tsPacket,int index,int sectionLen);
	int AppendSection(byte* tsPacket, int index, int sectionLen);
	int SnapshotSectionLength(byte* tsPacket,int start);

  bool        m_bLog;
  bool        m_bCrcCheck;
  int			    m_pid;
  CSection		m_section;
	int         m_iContinuityCounter;
	ISectionCallback* m_pCallback;
  CTsHeader m_header;
  CTsHeader m_headerSection;
	bool m_bGetAllSections;
};
