/* 
 *  Copyright (C) 2005-2013 Team MediaPortal
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
#include <streams.h>
#include "..\..\shared\FileWriter.h"


const AMOVIESETUP_MEDIATYPE OUTPUT_MEDIA_TYPES[] =
{
  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG2_TRANSPORT }
};
const int OUTPUT_MEDIA_TYPE_COUNT = 1;

class CTsOutputPin : public CBaseOutputPin
{
  public:    
    CTsOutputPin(CBaseFilter* filter, CCritSec* filterLock, HRESULT* hr);
    virtual ~CTsOutputPin(void);

    HRESULT CheckMediaType(const CMediaType* mediaType);
    HRESULT DecideBufferSize(IMemAllocator* allocator, ALLOCATOR_PROPERTIES* properties);
    HRESULT Deliver(PBYTE data, long dataLength);
    HRESULT DeliverEndOfStream();
    HRESULT GetMediaType(int position, CMediaType* mediaType);

    HRESULT StartDumping(wchar_t* fileName);
    HRESULT StopDumping();

  private:
    bool m_isDumpEnabled;
    FileWriter m_dumpFileWriter;
    CCritSec m_dumpLock;
};