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
#include "ISectionCallback.h"
#include "dvbutil.h"
#include "Section.h"

#define MAX_SECTIONS 256

class CSectionDecoder : public CDvbUtil
{
public:
  CSectionDecoder(void);
  ~CSectionDecoder(void);
	void SetCallBack(ISectionCallback* callback);
	void OnTsPacket(byte* tsPacket);
  void SetPid(int pid);
  int  GetPid();
  void SetTableId(int tableId);
	void Reset();
  int  GetTableId();
  virtual void OnNewSection(CSection& section);
protected:
private:
  int			    m_pid;
  int			    m_tableId;
  CSection		m_section;
	int         m_iContinuityCounter;
	ISectionCallback* m_pCallback;
};
