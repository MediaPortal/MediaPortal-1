/*
 *  Copyright (C) 2006-2008 Team MediaPortal
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
#include <InitGuid.h>   // DEFINE_GUID()
#include "IGrabber.h"


// {615ab76a-7a85-45f8-abf8-5bbab30aa26d}
DEFINE_GUID(IID_IGRABBER_EPG_ATSC,
            0x615ab76a, 0x7a85, 0x45f8, 0xab, 0xf8, 0x5b, 0xba, 0xb3, 0xa, 0xa2, 0x6d);

DECLARE_INTERFACE_(IGrabberEpgAtsc, IGrabber)
{
  // IGrabber
  STDMETHOD_(void, SetCallBack)(THIS_ ICallBackGrabber* callBack)PURE;


  // IGrabberEpgAtsc
  STDMETHOD_(void, Start)(THIS)PURE;
  STDMETHOD_(void, Stop)(THIS)PURE;

  STDMETHOD_(bool, IsSeen)(THIS)PURE;
  STDMETHOD_(bool, IsReady)(THIS)PURE;

  STDMETHOD_(unsigned long, GetEventCount)(THIS)PURE;
  STDMETHOD_(bool, GetEvent)(THIS_ unsigned long index,
                              unsigned short* sourceId,
                              unsigned short* eventId,
                              unsigned long long* startDateTime,
                              unsigned short* duration,
                              unsigned char* textCount,
                              unsigned long* audioLanguages,
                              unsigned char* audioLanguageCount,
                              unsigned long* captionsLanguages,
                              unsigned char* captionsLanguageCount,
                              unsigned char* genreIds,
                              unsigned char* genreIdCount,
                              unsigned char* vchipRating,
                              unsigned char* mpaaClassification,
                              unsigned short* advisories)PURE;
  STDMETHOD_(bool, GetEventTextByIndex)(THIS_ unsigned long eventIndex,
                                        unsigned char textIndex,
                                        unsigned long* language,
                                        char* title,
                                        unsigned short* titleBufferSize,
                                        char* text,
                                        unsigned short* textBufferSize)PURE;
  STDMETHOD_(bool, GetEventTextByLanguage)(THIS_ unsigned long eventIndex,
                                            unsigned long language,
                                            char* title,
                                            unsigned short* titleBufferSize,
                                            char* text,
                                            unsigned short* textBufferSize)PURE;
};