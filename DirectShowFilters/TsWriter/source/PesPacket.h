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
#include "..\..\shared\pcr.h"

class CBuffer
{
  public:
    CBuffer();
    virtual ~CBuffer();
    int  Write(byte* data, int len, bool isStart,CPcr& pcr);
    int  Read(byte* data, int len);
    int  Size();
    void HasPtsDts(bool& pts, bool &dts);
		bool HasSequenceHeader();
    bool IsStart();
    void Reset();
    CPcr& Pcr();
    CPcr& Pts();
    CPcr& Dts();

  private:
    BYTE* m_pData;
    int   m_iReadPtr;
    int   m_iSize;
    bool  m_bIsStart;
		bool  m_bSequenceHeader;
    CPcr  m_pcr;
    CPcr  m_dts;
    CPcr  m_pts;
};
#define MAX_BUFFERS 6000 //doubled to test Mantis #1053
class CPesPacket
{
  public:
    CPesPacket();
    virtual ~CPesPacket();
    void Reset();

    void Write(byte* data, int len, bool isStart,CPcr& pcr);
    int  Read(byte* data, int len);
    bool IsAvailable(int size);
    bool IsStart();
    void NextPacketHasPtsDts(bool& pts, bool &dts);
		void Skip();
    CPcr& Pcr();
    CPcr& Pts();
    CPcr& Dts();
    int   InUse();
		bool  HasSequenceHeader();
    ULONG packet_number;

  private:
    CBuffer m_buffers[MAX_BUFFERS];
    int    m_iCurrentWriteBuffer;
    int    m_iCurrentReadBuffer;
    UINT64 m_totalSize;
    int    m_inUse;
    int    m_maxInUse;
    
};
