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


// {dfbe4e16-fb7b-47d2-a1c0-6f6d9fb61e9a}
DEFINE_GUID(IID_IGRABBER_SI_DVB,
            0xdfbe4e16, 0xfb7b, 0x47d2, 0xa1, 0xc0, 0x6f, 0x6d, 0x9f, 0xb6, 0x1e, 0x9a);

DECLARE_INTERFACE_(IGrabberSiDvb, IGrabber)
{
  BEGIN_INTERFACE


  // IUnknown
  STDMETHOD(QueryInterface)(THIS_ REFIID riid, void** ppv)PURE;
  STDMETHOD_(unsigned long, AddRef)(THIS)PURE;
  STDMETHOD_(unsigned long, Release)(THIS)PURE;


  // IGrabber
  STDMETHOD_(void, SetCallBack)(THIS_ ICallBackGrabber* callBack)PURE;


  // IGrabberSiDvb
  STDMETHOD_(bool, IsSeenBat)(THIS)PURE;
  STDMETHOD_(bool, IsSeenNitActual)(THIS)PURE;
  STDMETHOD_(bool, IsSeenNitOther)(THIS)PURE;
  STDMETHOD_(bool, IsSeenSdtActual)(THIS)PURE;
  STDMETHOD_(bool, IsSeenSdtOther)(THIS)PURE;

  STDMETHOD_(bool, IsReadyBat)(THIS)PURE;
  STDMETHOD_(bool, IsReadyNitActual)(THIS)PURE;
  STDMETHOD_(bool, IsReadyNitOther)(THIS)PURE;
  STDMETHOD_(bool, IsReadySdtActual)(THIS)PURE;
  STDMETHOD_(bool, IsReadySdtOther)(THIS)PURE;

  STDMETHOD_(void, GetServiceCount)(THIS_ unsigned short* actualOriginalNetworkId,
                                    unsigned short* serviceCount)PURE;
  STDMETHOD_(bool, GetService)(THIS_ unsigned short index,
                                unsigned char* tableId,
                                unsigned short* originalNetworkId,
                                unsigned short* transportStreamId,
                                unsigned short* serviceId,
                                unsigned short* referenceServiceId,
                                unsigned short* freesatChannelId,
                                unsigned short* openTvChannelId,
                                unsigned long long* logicalChannelNumbers,
                                unsigned short* logicalChannelNumberCount,
                                unsigned char* dishSubChannelNumber,
                                bool* eitScheduleFlag,
                                bool* eitPresentFollowingFlag,
                                unsigned char* runningStatus,
                                bool* freeCaMode,
                                unsigned char* serviceType,
                                unsigned char* serviceNameCount,
                                bool* visibleInGuide,
                                unsigned short* streamCountVideo,
                                unsigned short* streamCountAudio,
                                bool* isHighDefinition,
                                bool* isStandardDefinition,
                                bool* isThreeDimensional,
                                unsigned long* audioLanguages,
                                unsigned char* audioLanguageCount,
                                unsigned long* subtitlesLanguages,
                                unsigned char* subtitlesLanguageCount,
                                unsigned short* networkIds,
                                unsigned char* networkIdCount,
                                unsigned short* bouquetIds,
                                unsigned char* bouquetIdCount,
                                unsigned long* availableInCountries,
                                unsigned char* availableInCountryCount,
                                unsigned long* unavailableInCountries,
                                unsigned char* unavailableInCountryCount,
                                unsigned long* availableInCells,
                                unsigned char* availableInCellCount,
                                unsigned long* unavailableInCells,
                                unsigned char* unavailableInCellCount,
                                unsigned long long* targetRegionIds,
                                unsigned char* targetRegionIdCount,
                                unsigned long* freesatRegionIds,
                                unsigned char* freesatRegionIdCount,
                                unsigned long* openTvRegionIds,
                                unsigned char* openTvRegionIdCount,
                                unsigned char* cyfrowyPolsatChannelCategoryId,
                                unsigned long* freesatChannelCategoryIds,
                                unsigned char* freesatChannelCategoryIdCount,
                                unsigned short* mediaHighwayChannelCategoryIds,
                                unsigned char* mediaHighwayChannelCategoryIdCount,
                                unsigned char* openTvChannelCategoryIds,
                                unsigned char* openTvChannelCategoryIdCount,
                                unsigned char* virginMediaChannelCategoryId,
                                unsigned short* dishMarketId,
                                unsigned long long* norDigChannelListIds,
                                unsigned char* norDigChannelListIdCount,
                                unsigned short* previousOriginalNetworkId,
                                unsigned short* previousTransportStreamId,
                                unsigned short* previousServiceId,
                                unsigned short* epgOriginalNetworkId,
                                unsigned short* epgTransportStreamId,
                                unsigned short* epgServiceId)PURE;
  STDMETHOD_(bool, GetServiceNameByIndex)(THIS_ unsigned short serviceIndex,
                                          unsigned char nameIndex,
                                          unsigned long* language,
                                          char* providerName,
                                          unsigned short* providerNameBufferSize,
                                          char* serviceName,
                                          unsigned short* serviceNameBufferSize)PURE;
  STDMETHOD_(bool, GetServiceNameByLanguage)(THIS_ unsigned short serviceIndex,
                                              unsigned long language,
                                              char* providerName,
                                              unsigned short* providerNameBufferSize,
                                              char* serviceName,
                                              unsigned short* serviceNameBufferSize)PURE;

  STDMETHOD_(unsigned char, GetNetworkNameCount)(THIS_ unsigned short networkId)PURE;
  STDMETHOD_(bool, GetNetworkNameByIndex)(THIS_ unsigned short networkId,
                                          unsigned char index,
                                          unsigned long* language,
                                          char* name,
                                          unsigned short* nameBufferSize)PURE;
  STDMETHOD_(bool, GetNetworkNameByLanguage)(THIS_ unsigned short networkId,
                                              unsigned long language,
                                              char* name,
                                              unsigned short* nameBufferSize)PURE;

  STDMETHOD_(unsigned char, GetBouquetNameCount)(THIS_ unsigned short bouquetId)PURE;
  STDMETHOD_(bool, GetBouquetNameByIndex)(THIS_ unsigned short bouquetId,
                                          unsigned char index,
                                          unsigned long* language,
                                          char* name,
                                          unsigned short* nameBufferSize)PURE;
  STDMETHOD_(bool, GetBouquetNameByLanguage)(THIS_ unsigned short bouquetId,
                                              unsigned long language,
                                              char* name,
                                              unsigned short* nameBufferSize)PURE;

  STDMETHOD_(unsigned char, GetTargetRegionNameCount)(THIS_ unsigned long long regionId)PURE;
  STDMETHOD_(bool, GetTargetRegionNameByIndex)(THIS_ unsigned long long regionId,
                                                unsigned char index,
                                                unsigned long* language,
                                                char* name,
                                                unsigned short* nameBufferSize)PURE;
  STDMETHOD_(bool, GetTargetRegionNameByLanguage)(THIS_ unsigned long long regionId,
                                                  unsigned long language,
                                                  char* name,
                                                  unsigned short* nameBufferSize)PURE;

  STDMETHOD_(unsigned char, GetCyfrowyPolsatChannelCategoryNameCount)(THIS_ unsigned char categoryId)PURE;
  STDMETHOD_(bool, GetCyfrowyPolsatChannelCategoryNameByIndex)(THIS_ unsigned char categoryId,
                                                                unsigned char index,
                                                                unsigned long* language,
                                                                char* name,
                                                                unsigned short* nameBufferSize)PURE;
  STDMETHOD_(bool, GetCyfrowyPolsatChannelCategoryNameByLanguage)(THIS_ unsigned char categoryId,
                                                                  unsigned long language,
                                                                  char* name,
                                                                  unsigned short* nameBufferSize)PURE;

  STDMETHOD_(unsigned char, GetFreesatRegionNameCount)(THIS_ unsigned long regionId)PURE;
  STDMETHOD_(bool, GetFreesatRegionNameByIndex)(THIS_ unsigned long regionId,
                                                unsigned char index,
                                                unsigned long* language,
                                                char* name,
                                                unsigned short* nameBufferSize)PURE;
  STDMETHOD_(bool, GetFreesatRegionNameByLanguage)(THIS_ unsigned long regionId,
                                                    unsigned long language,
                                                    char* name,
                                                    unsigned short* nameBufferSize)PURE;

  STDMETHOD_(unsigned char, GetFreesatChannelCategoryNameCount)(THIS_ unsigned long categoryId)PURE;
  STDMETHOD_(bool, GetFreesatChannelCategoryNameByIndex)(THIS_ unsigned long categoryId,
                                                          unsigned char index,
                                                          unsigned long* language,
                                                          char* name,
                                                          unsigned short* nameBufferSize)PURE;
  STDMETHOD_(bool, GetFreesatChannelCategoryNameByLanguage)(THIS_ unsigned long categoryId,
                                                            unsigned long language,
                                                            char* name,
                                                            unsigned short* nameBufferSize)PURE;

  STDMETHOD_(bool, GetMediaHighwayChannelCategoryName)(THIS_ unsigned short categoryId,
                                                        char* name,
                                                        unsigned short* nameBufferSize)PURE;

  STDMETHOD_(unsigned char, GetNorDigChannelListNameCount)(THIS_ unsigned long long channelListId)PURE;
  STDMETHOD_(bool, GetNorDigChannelListNameByIndex)(THIS_ unsigned long long channelListId,
                                                    unsigned char index,
                                                    unsigned long* language,
                                                    char* name,
                                                    unsigned short* nameBufferSize)PURE;
  STDMETHOD_(bool, GetNorDigChannelListNameByLanguage)(THIS_ unsigned long long channelListId,
                                                        unsigned long language,
                                                        char* name,
                                                        unsigned short* nameBufferSize)PURE;

  STDMETHOD_(unsigned short, GetTransmitterCount)(THIS)PURE;
  STDMETHOD_(bool, GetTransmitter)(THIS_ unsigned short index,
                                    unsigned char* tableId,
                                    unsigned short* networkId,
                                    unsigned short* originalNetworkId,
                                    unsigned short* transportStreamId,
                                    bool* isHomeTransmitter,
                                    unsigned long* broadcastStandard,
                                    unsigned long* frequencies,
                                    unsigned char* frequencyCount,
                                    unsigned char* polarisation,
                                    unsigned char* modulation,
                                    unsigned long* symbolRate,
                                    unsigned short* bandwidth,
                                    unsigned char* innerFecRate,
                                    unsigned char* rollOffFactor,
                                    short* longitude,
                                    unsigned short* cellId,
                                    unsigned char* cellIdExtension,
                                    bool* isMultipleInputStream,
                                    unsigned char* plpId)PURE;


  END_INTERFACE
};