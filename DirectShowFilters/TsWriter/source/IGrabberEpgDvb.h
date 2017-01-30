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


// {0fa842df-9c1b-4164-96ac-47c0a44241de}
DEFINE_GUID(IID_IGRABBER_EPG_DVB,
            0xfa842df, 0x9c1b, 0x4164, 0x96, 0xac, 0x47, 0xc0, 0xa4, 0x42, 0x41, 0xde);

DECLARE_INTERFACE_(IGrabberEpgDvb, IGrabber)
{
  // IGrabber
  STDMETHOD_(void, SetCallBack)(THIS_ ICallBackGrabber* callBack)PURE;


  // IGrabberEpgEit
  STDMETHOD_(void, SetProtocols)(THIS_ bool grabDvbEit,
                                  bool grabBellTv,
                                  bool grabDish,
                                  bool grabFreesat,
                                  bool grabMultiChoice,
                                  bool grabOrbitShowtimeNetwork,
                                  bool grabPremiere,
                                  bool grabViasatSweden)PURE;

  STDMETHOD_(bool, IsSeen)(THIS)PURE;
  STDMETHOD_(bool, IsReady)(THIS)PURE;

  STDMETHOD_(unsigned short, GetServiceCount)(THIS)PURE;
  STDMETHOD_(bool, GetService)(THIS_ unsigned short index,
                                unsigned short* originalNetworkId,
                                unsigned short* transportStreamId,
                                unsigned short* serviceId,
                                unsigned short* eventCount)PURE;
  STDMETHOD_(bool, GetEvent)(THIS_ unsigned short serviceIndex,
                              unsigned short eventIndex,
                              unsigned long long* eventId,
                              unsigned long long* startDateTime,
                              unsigned short* duration,
                              unsigned char* runningStatus,
                              bool* freeCaMode,
                              unsigned short* referenceServiceId,
                              unsigned long long* referenceEventId,
                              char* seriesId,
                              unsigned short* seriesIdBufferSize,
                              char* episodeId,
                              unsigned short* episodeIdBufferSize,
                              bool* isHighDefinition,
                              bool* isStandardDefinition,
                              bool* isThreeDimensional,
                              bool* isPreviouslyShown,
                              unsigned long* audioLanguages,
                              unsigned char* audioLanguageCount,
                              unsigned long* subtitlesLanguages,
                              unsigned char* subtitlesLanguageCount,
                              unsigned short* dvbContentTypeIds,
                              unsigned char* dvbContentTypeIdCount,
                              unsigned long* dvbParentalRatingCountryCodes,
                              unsigned char* dvbParentalRatings,
                              unsigned char* dvbParentalRatingCount,
                              unsigned char* starRating,
                              unsigned char* mpaaClassification,
                              unsigned short* dishBevAdvisories,
                              unsigned char* vchipRating,
                              unsigned char* textCount)PURE;
  STDMETHOD_(bool, GetEventText)(THIS_ unsigned short serviceIndex,
                                  unsigned short eventIndex,
                                  unsigned char textIndex,
                                  unsigned long* language,
                                  char* title,
                                  unsigned short* titleBufferSize,
                                  char* shortDescription,
                                  unsigned short* shortDescriptionBufferSize,
                                  char* extendedDescription,
                                  unsigned short* extendedDescriptionBufferSize,
                                  unsigned char* descriptionItemCount)PURE;
  STDMETHOD_(bool, GetEventDescriptionItem)(THIS_ unsigned short serviceIndex,
                                            unsigned short eventIndex,
                                            unsigned char textIndex,
                                            unsigned char itemIndex,
                                            char* description,
                                            unsigned short* descriptionBufferSize,
                                            char* text,
                                            unsigned short* textBufferSize)PURE;
};