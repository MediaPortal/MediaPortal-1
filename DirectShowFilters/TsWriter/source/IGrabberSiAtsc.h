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


// {9769a108-9b39-403a-9fe8-60991095231f}
DEFINE_GUID(IID_IGRABBER_SI_ATSC,
            0x9769a108, 0x9b39, 0x403a, 0x9f, 0xe8, 0x60, 0x99, 0x10, 0x95, 0x23, 0x1f);

DECLARE_INTERFACE_(IGrabberSiAtsc, IGrabber)
{
  // IGrabber
  STDMETHOD_(void, SetCallBack)(THIS_ ICallBackGrabber* callBack)PURE;


  // IGrabberSiAtsc
  STDMETHOD_(bool, IsSeenLvct)(THIS)PURE;
  STDMETHOD_(bool, IsReadyLvct)(THIS)PURE;
  STDMETHOD_(unsigned short, GetLvctChannelCount)(THIS)PURE;
  STDMETHOD_(bool, GetLvctChannel)(THIS_ unsigned short index,
                                    unsigned char* tableId,
                                    unsigned short* sectionTransportStreamId,
                                    unsigned short* mapId,
                                    char* shortName,
                                    unsigned short* shortNameBufferSize,
                                    unsigned char* longNameCount,
                                    unsigned short* majorChannelNumber,
                                    unsigned short* minorChannelNumber,
                                    unsigned char* modulationMode,
                                    unsigned long* carrierFrequency,
                                    unsigned short* transportStreamId,
                                    unsigned short* programNumber,
                                    unsigned char* etmLocation,
                                    bool* accessControlled,
                                    bool* hidden,
                                    bool* pathSelect,
                                    bool* outOfBand,
                                    bool* hideGuide,
                                    unsigned char* serviceType,
                                    unsigned short* sourceId,
                                    unsigned char* streamCountVideo,
                                    unsigned char* streamCountAudio,
                                    bool* isThreeDimensional,
                                    unsigned long* audioLanguages,
                                    unsigned char* audioLanguageCount,
                                    unsigned long* captionsLanguages,
                                    unsigned char* captionsLanguageCount)PURE;
  STDMETHOD_(bool, GetLvctChannelLongNameByIndex)(THIS_ unsigned short channelIndex,
                                                  unsigned char nameIndex,
                                                  unsigned long* language,
                                                  char* name,
                                                  unsigned short* nameBufferSize)PURE;
  STDMETHOD_(bool, GetLvctChannelLongNameByLanguage)(THIS_ unsigned short channelIndex,
                                                      unsigned long language,
                                                      char* name,
                                                      unsigned short* nameBufferSize)PURE;

  STDMETHOD_(bool, IsSeenSvct)(THIS)PURE;
  STDMETHOD_(bool, IsReadySvct)(THIS)PURE;
  STDMETHOD_(unsigned short, GetSvctChannelCount)(THIS)PURE;
  STDMETHOD_(bool, GetSvctChannel)(THIS_ unsigned short index,
                                    unsigned char* transmissionMedium,
                                    unsigned short* vctId,
                                    unsigned long* mapNameLanguage,
                                    char* mapName,
                                    unsigned short* mapNameBufferSize,
                                    bool* splice,
                                    unsigned long* activationTime,
                                    bool* hdtvChannel,
                                    bool* preferredSource,
                                    bool* applicationVirtualChannel,
                                    unsigned short* majorChannelNumber,
                                    unsigned short* minorChannelNumber,
                                    unsigned short* sourceId,
                                    unsigned long* sourceNameLanguage,
                                    char* sourceName,
                                    unsigned short* sourceNameBufferSize,
                                    bool* accessControlled,
                                    bool* hideGuide,
                                    unsigned char* serviceType,
                                    bool* outOfBand,
                                    unsigned char* bitstreamSelect,
                                    unsigned char* pathSelect,
                                    unsigned char* channelType,
                                    unsigned short* nvodChannelBase,
                                    unsigned char* transportType, 
                                    bool* wideBandwidthVideo,
                                    unsigned char* waveformStandard,
                                    unsigned char* videoStandard,
                                    bool* wideBandwidthAudio,
                                    bool* compandedAudio,
                                    unsigned char* matrixMode,
                                    unsigned short* subcarrier2Offset,
                                    unsigned short* subcarrier1Offset,
                                    bool* suppressVideo,
                                    unsigned char* audioSelection,
                                    unsigned short* programNumber,
                                    unsigned short* transportStreamId, 
                                    unsigned char* satelliteId,
                                    unsigned long* satelliteNameLanguage,
                                    char* satelliteReferenceName,
                                    unsigned short* satelliteReferenceNameBufferSize,
                                    char* satelliteFullName,
                                    unsigned short* satelliteFullNameBufferSize,
                                    unsigned char* hemisphere,
                                    unsigned short* orbitalPosition,
                                    bool* youAreHere,
                                    unsigned char* frequencyBand,
                                    bool* outOfService,
                                    unsigned char* polarisationType,
                                    unsigned char* transponderNumber,
                                    unsigned long* transponderNameLanguage,
                                    char* transponderName,
                                    unsigned short* transponderNameBufferSize,
                                    bool* rootTransponder,
                                    unsigned char* toneSelect,
                                    unsigned char* polarisation,
                                    unsigned long* frequency,
                                    unsigned long* symbolRate,
                                    unsigned char* transmissionSystem,
                                    unsigned char* innerCodingMode,
                                    bool* splitBitstreamMode,
                                    unsigned char* modulationFormat)PURE;
};