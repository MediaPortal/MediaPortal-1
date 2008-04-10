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

#define MAX_PES_PACKET 0x80000
class CPesCallback
{
public:
	virtual int OnNewPesPacket(int streamId,byte* header, int headerlen,byte* data, int len, bool isStart)=0;
};

class CPesDecoder
{
public:
	CPesDecoder(CPesCallback* callback);
	virtual ~CPesDecoder(void);
	void					SetMaxLength(int len);
	void					SetPid(int pid);
	int						GetPid();
	bool					OnTsPacket(byte* tsPacket);
	void					Reset();
	int						GetStreamId();
	bool					IsAudio();
	bool					IsVideo();
	void					SetStreamId(int streamId);
private:
  bool          m_bStart;
  int           m_iPesHeaderLen;
  byte          m_pesHeader[256];
	CPesCallback* m_pCallback;
	unsigned long m_packets;
	int					  m_pid;
	byte*					m_pesBuffer;
	int						m_iWritePos;
	int						m_iMaxLength;
	int						m_iStreamId;
};
