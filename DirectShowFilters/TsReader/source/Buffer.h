/*
 *  Copyright (C) 2005 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#define MAX_BUFFER_SIZE 0x10000
class CBuffer
{
public:
  CBuffer(void);
  CBuffer(unsigned long size);
  ~CBuffer(void);
  int    Length();
  byte*  Data();
  void   Add(CBuffer* pBuffer);
  void   Add(byte* data, int len);
  void   SetPcr(CPcr& firstPcr,CPcr& maxPcr);
  void   SetPts(CPcr& pts);
  void   SetLength(int len);
  CPcr&  Pcr();
  bool   MediaTime(CRefTime &reftime);
  void   SetDiscontinuity();
  bool   GetDiscontinuity();
  void   SetVideoServiceType(int type);
  int    GetVideoServiceType();
  void   SetFrameType(char type) { m_frameType = type ; } ;
  char   GetFrameType() { return m_frameType ; }
  void   SetFrameCount(int count) { m_frameCount = count ; };
  int    GetFrameCount() { return m_frameCount ; };

private:
  bool  m_bDiscontinuity;
  int   m_iVideoServiceType;
  CPcr  m_currentPcr;
  CPcr  m_pts;
  CPcr  m_firstPcr;
  CPcr  m_maxPcr;
  byte* m_pBuffer;
  int   m_iLength;
  int   m_frameType ;
  int   m_frameCount ;
  unsigned int m_iSize;
};
