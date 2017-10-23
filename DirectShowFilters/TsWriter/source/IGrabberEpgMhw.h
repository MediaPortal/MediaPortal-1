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


// {83943f71-4a5a-4dde-9efc-d031fe77a85e}
DEFINE_GUID(IID_IGRABBER_EPG_MHW,
            0x83943f71, 0x4a5a, 0x4dde, 0x9e, 0xfc, 0xd0, 0x31, 0xfe, 0x77, 0xa8, 0x5e);

DECLARE_INTERFACE_(IGrabberEpgMhw, IGrabber)
{
  BEGIN_INTERFACE


  // IUnknown
  STDMETHOD(QueryInterface)(THIS_ REFIID riid, void** ppv)PURE;
  STDMETHOD_(unsigned long, AddRef)(THIS)PURE;
  STDMETHOD_(unsigned long, Release)(THIS)PURE;


  // IGrabber
  STDMETHOD_(void, SetCallBack)(THIS_ ICallBackGrabber* callBack)PURE;


  // IGrabberEpgMhw
  STDMETHOD_(void, SetProtocols)(THIS_ bool grabMhw1, bool grabMhw2)PURE;

  STDMETHOD_(bool, IsSeen)(THIS)PURE;
  STDMETHOD_(bool, IsReady)(THIS)PURE;

  STDMETHOD_(void, GetEventCount)(THIS_ unsigned long* eventCount,
                                  unsigned long* textLanguage)PURE;
  STDMETHOD_(bool, GetEvent)(THIS_ unsigned long index,
                              unsigned char* version,
                              unsigned long* eventId,
                              unsigned short* originalNetworkId,
                              unsigned short* transportStreamId,
                              unsigned short* serviceId,
                              char* serviceName,
                              unsigned short* serviceNameBufferSize,
                              unsigned long long* startDateTime,
                              unsigned short* duration,
                              char* title,
                              unsigned short* titleBufferSize,
                              char* description,
                              unsigned short* descriptionBufferSize,
                              unsigned char* descriptionLineCount,
                              unsigned long* seriesId,
                              char* seasonName,
                              unsigned short* seasonNameBufferSize,
                              unsigned long* episodeId,
                              unsigned short* episodeNumber,
                              char* episodeName,
                              unsigned short* episodeNameBufferSize,
                              char* themeName,
                              unsigned short* themeNameBufferSize,
                              char* subThemeName,
                              unsigned short* subThemeNameBufferSize,
                              unsigned char* classification,
                              bool* isHighDefinition,
                              bool* hasSubtitles,
                              bool* isRecommended,
                              bool* isPayPerView,
                              unsigned long* payPerViewId)PURE;
  STDMETHOD_(bool, GetDescriptionLine)(THIS_ unsigned long eventIndex,
                                        unsigned char lineIndex,
                                        char* line,
                                        unsigned short* lineBufferSize)PURE;


  END_INTERFACE
};