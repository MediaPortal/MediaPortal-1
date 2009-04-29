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

#include "tsHeader.h"
#include "pespacket.h"
#include "pcr.h"

#define MAX_PES_PACKET 0x80000
class CPesDecoder;

class CPesCallback
{
public:
	virtual int OnNewPesPacket(CPesDecoder* decoder)=0;
};

class CPesDecoder
{
public:
	CPesDecoder(CPesCallback* callback);
	virtual ~CPesDecoder(void);
	void					SetPid(int pid);
	int						GetPid();
	bool					OnTsPacket(byte* tsPacket,CPcr& pcr);
	void					Reset();
	bool					IsAudio();
	bool					IsAc3();
	bool					IsVideo();
	int						GetStreamId();
	void					SetStreamId(int streamId);

  CPesPacket    m_packet;
private:
	CPesCallback* m_pCallback;
	int					  m_pid;
	int						m_iStreamId;
  CTsHeader     m_tsHeader;
  bool          m_bStartFound;
};
