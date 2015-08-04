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


// {65e51de0-4a4e-43e0-8b13-c809e5be8a93}
DEFINE_GUID(IID_IGRABBER_EPG_OPENTV,
            0x65e51de0, 0x4a4e, 0x43e0, 0x8b, 0x13, 0xc8, 0x9, 0xe5, 0xbe, 0x8a, 0x93);

DECLARE_INTERFACE_(IGrabberEpgOpenTv, IGrabber)
{
  // IGrabber
  STDMETHOD_(void, SetCallBack)(THIS_ ICallBackGrabber* callBack)PURE;


  // IGrabberEpgOpenTv
  STDMETHOD_(void, Start)(THIS)PURE;
  STDMETHOD_(void, Stop)(THIS)PURE;

  STDMETHOD_(bool, IsSeen)(THIS)PURE;
  STDMETHOD_(bool, IsReady)(THIS)PURE;

  STDMETHOD_(unsigned long, GetEventCount)(THIS)PURE;
  STDMETHOD_(bool, GetEvent)(THIS_ unsigned long index,
                              unsigned short* channelId,
                              unsigned short* eventId,
                              unsigned long long* startDateTime,
                              unsigned short* duration,
                              char* title,
                              unsigned short* titleBufferSize,
                              char* shortDescription,
                              unsigned short* shortDescriptionBufferSize,
                              char* extendedDescription,
                              unsigned short* extendedDescriptionBufferSize,
                              unsigned char* categoryId,
                              unsigned char* subCategoryId,
                              bool* isHighDefinition,
                              bool* hasSubtitles,
                              unsigned char* parentalRating,
                              unsigned short* seriesLinkId)PURE;
};