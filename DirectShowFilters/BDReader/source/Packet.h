/*
 *  Copyright (C) 2005-2011 Team MediaPortal
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

#include <atlcoll.h>
#include <Dshow.h>

class Packet : public CAtlArray<BYTE>
{
public:

  Packet() ;
  virtual ~Packet();
  virtual int GetDataSize();
  void SetData(const void* ptr, DWORD len);
  void CopyProperties(Packet& pSrc, bool pValidateStartTime = false);
  void TransferProperties(Packet& pSrc, bool pValidateStartTime, bool pResetClipInfo);
  void ResetProperties(bool pResetClipInfo);

  INT32 nClipNumber;
  INT32 nPlaylist;
  bool bDiscontinuity, bSyncPoint, bNewClip, bResuming;  
  static const REFERENCE_TIME INVALID_TIME = _I64_MIN;  
  REFERENCE_TIME rtStart, rtStop, rtOffset, rtPlaylistTime, rtClipStartTime, rtTitleDuration;
  AM_MEDIA_TYPE* pmt;
};
