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
#include <map>
#include <vector>
#include <streams.h>    // CUnknown, LPUNKNOWN
#include <WinError.h>   // HRESULT
#include "..\..\shared\TsHeader.h"
#include "CriticalSection.h"
#include "ICallBackGrabber.h"
#include "ICallBackNitDvb.h"
#include "ICallBackSdt.h"
#include "ICallBackSiDvb.h"
#include "IDefaultAuthorityProvider.h"
#include "IGrabberSiFreesat.h"
#include "ParserBat.h"
#include "ParserNitDvb.h"
#include "ParserSdt.h"

using namespace MediaPortal;
using namespace std;


class CGrabberSiDvb
  : public CUnknown, ICallBackNitDvb, ICallBackSdt,
    public IDefaultAuthorityProvider, public IGrabberSiFreesat
{
  public:
    CGrabberSiDvb(ICallBackSiDvb* callBack, LPUNKNOWN unk, HRESULT* hr);
    virtual ~CGrabberSiDvb(void);

    DECLARE_IUNKNOWN

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    void SetPids(unsigned short pidBat, unsigned short pidNit, unsigned short pidSdt);
    void Reset(bool enableCrcCheck);
    STDMETHODIMP_(void) SetCallBack(ICallBackGrabber* callBack);
    bool OnTsPacket(CTsHeader& header, unsigned char* tsPacket);

    STDMETHODIMP_(bool) IsSeenBat();
    STDMETHODIMP_(bool) IsSeenNitActual();
    STDMETHODIMP_(bool) IsSeenNitOther();
    STDMETHODIMP_(bool) IsSeenSdtActual();
    STDMETHODIMP_(bool) IsSeenSdtOther();
    STDMETHODIMP_(bool) IsReadyBat();
    STDMETHODIMP_(bool) IsReadyNitActual();
    STDMETHODIMP_(bool) IsReadyNitOther();
    STDMETHODIMP_(bool) IsReadySdtActual();
    STDMETHODIMP_(bool) IsReadySdtOther();

    STDMETHODIMP_(unsigned short) GetServiceCount();
    STDMETHODIMP_(bool) GetService(unsigned short index,
                                    unsigned short preferredLogicalChannelNumberBouquetId,
                                    unsigned short preferredLogicalChannelNumberRegionId,
                                    unsigned char* tableId,
                                    unsigned short* originalNetworkId,
                                    unsigned short* transportStreamId,
                                    unsigned short* serviceId,
                                    unsigned short* referenceServiceId,
                                    unsigned short* freesatChannelId,
                                    unsigned short* openTvChannelId,
                                    unsigned short* logicalChannelNumber,
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
                                    unsigned short* freesatRegionIds,
                                    unsigned char* freesatRegionIdCount,
                                    unsigned short* openTvRegionIds,
                                    unsigned char* openTvRegionIdCount,
                                    unsigned short* freesatChannelCategoryIds,
                                    unsigned char* freesatChannelCategoryIdCount,
                                    unsigned char* openTvChannelCategoryId,
                                    unsigned char* virginMediaChannelCategoryId,
                                    unsigned short* dishMarketId,
                                    unsigned char* norDigChannelListIds,
                                    unsigned char* norDigChannelListIdCount,
                                    unsigned short* previousOriginalNetworkId,
                                    unsigned short* previousTransportStreamId,
                                    unsigned short* previousServiceId,
                                    unsigned short* epgOriginalNetworkId,
                                    unsigned short* epgTransportStreamId,
                                    unsigned short* epgServiceId);
    STDMETHODIMP_(bool) GetServiceNameByIndex(unsigned short serviceIndex,
                                              unsigned char nameIndex,
                                              unsigned long* language,
                                              char* providerName,
                                              unsigned short* providerNameBufferSize,
                                              char* serviceName,
                                              unsigned short* serviceNameBufferSize);
    STDMETHODIMP_(bool) GetServiceNameByLanguage(unsigned short serviceIndex,
                                                  unsigned long language,
                                                  char* providerName,
                                                  unsigned short* providerNameBufferSize,
                                                  char* serviceName,
                                                  unsigned short* serviceNameBufferSize);
    bool GetDefaultAuthority(unsigned short originalNetworkId,
                              unsigned short transportStreamId,
                              unsigned short serviceId,
                              char* defaultAuthority,
                              unsigned short& defaultAuthorityBufferSize) const;

    STDMETHODIMP_(unsigned char) GetNetworkNameCount(unsigned short networkId);
    STDMETHODIMP_(bool) GetNetworkNameByIndex(unsigned short networkId,
                                              unsigned char index,
                                              unsigned long* language,
                                              char* name,
                                              unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetNetworkNameByLanguage(unsigned short networkId,
                                                  unsigned long language,
                                                  char* name,
                                                  unsigned short* nameBufferSize);

    STDMETHODIMP_(unsigned char) GetBouquetNameCount(unsigned short bouquetId);
    STDMETHODIMP_(bool) GetBouquetNameByIndex(unsigned short bouquetId,
                                              unsigned char index,
                                              unsigned long* language,
                                              char* name,
                                              unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetBouquetNameByLanguage(unsigned short bouquetId,
                                                  unsigned long language,
                                                  char* name,
                                                  unsigned short* nameBufferSize);

    STDMETHODIMP_(unsigned char) GetTargetRegionNameCount(unsigned long long regionId);
    STDMETHODIMP_(bool) GetTargetRegionNameByIndex(unsigned long long regionId,
                                                    unsigned char index,
                                                    unsigned long* language,
                                                    char* name,
                                                    unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetTargetRegionNameByLanguage(unsigned long long regionId,
                                                      unsigned long language,
                                                      char* name,
                                                      unsigned short* nameBufferSize);

    STDMETHODIMP_(unsigned char) GetFreesatRegionNameCount(unsigned short regionId);
    STDMETHODIMP_(bool) GetFreesatRegionNameByIndex(unsigned short regionId,
                                                    unsigned char index,
                                                    unsigned long* language,
                                                    char* name,
                                                    unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetFreesatRegionNameByLanguage(unsigned short regionId,
                                                        unsigned long language,
                                                        char* name,
                                                        unsigned short* nameBufferSize);

    STDMETHODIMP_(unsigned char) GetFreesatChannelCategoryNameCount(unsigned short categoryId);
    STDMETHODIMP_(bool) GetFreesatChannelCategoryNameByIndex(unsigned short categoryId,
                                                              unsigned char index,
                                                              unsigned long* language,
                                                              char* name,
                                                              unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetFreesatChannelCategoryNameByLanguage(unsigned short categoryId,
                                                                unsigned long language,
                                                                char* name,
                                                                unsigned short* nameBufferSize);

    STDMETHODIMP_(unsigned char) GetNorDigChannelListNameCount(unsigned char channelListId);
    STDMETHODIMP_(bool) GetNorDigChannelListNameByIndex(unsigned char channelListId,
                                                        unsigned char index,
                                                        unsigned long* language,
                                                        char* name,
                                                        unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetNorDigChannelListNameByLanguage(unsigned char channelListId,
                                                            unsigned long language,
                                                            char* name,
                                                            unsigned short* nameBufferSize);

    STDMETHODIMP_(unsigned short) GetTransmitterCount();
    STDMETHODIMP_(bool) GetTransmitter(unsigned short index,
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
                                        unsigned char* plpId);

  private:
    void OnTableSeen(unsigned char tableId);
    void OnTableComplete(unsigned char tableId);
    void OnTableChange(unsigned char tableId);

    void OnSdtReceived(unsigned char tableId,
                        unsigned short originalNetworkId,
                        unsigned short transportStreamId,
                        unsigned short serviceId,
                        bool eitScheduleFlag,
                        bool eitPresentFollowingFlag,
                        unsigned char runningStatus,
                        bool freeCaMode,
                        unsigned char serviceType,
                        const map<unsigned long, char*>& providerNames,
                        const map<unsigned long, char*>& serviceNames,
                        unsigned short logicalChannelNumber,
                        unsigned char dishSubChannelNumber,
                        bool visibleInGuide,
                        unsigned short referenceServiceId,
                        bool isHighDefinition,
                        bool isStandardDefinition,
                        bool isThreeDimensional,
                        unsigned short streamCountVideo,
                        unsigned short streamCountAudio,
                        const vector<unsigned long>& audioLanguages,
                        const vector<unsigned long>& subtitlesLanguages,
                        unsigned char openTvCategoryId,
                        unsigned char virginMediaCategoryId,
                        unsigned short dishMarketId,
                        const vector<unsigned long>& availableInCountries,
                        const vector<unsigned long>& unavailableInCountries,
                        const vector<unsigned long>& availableInCells,
                        const vector<unsigned long>& unavailableInCells,
                        const vector<unsigned long long>& targetRegionIds,
                        unsigned short previousOriginalNetworkId,
                        unsigned short previousTransportStreamId,
                        unsigned short previousServiceId,
                        unsigned short epgOriginalNetworkId,
                        unsigned short epgTransportStreamId,
                        unsigned short epgServiceId,
                        const char* defaultAuthority);
    void OnSdtChanged(unsigned char tableId,
                        unsigned short originalNetworkId,
                        unsigned short transportStreamId,
                        unsigned short serviceId,
                        bool eitScheduleFlag,
                        bool eitPresentFollowingFlag,
                        unsigned char runningStatus,
                        bool freeCaMode,
                        unsigned char serviceType,
                        const map<unsigned long, char*>& providerNames,
                        const map<unsigned long, char*>& serviceNames,
                        unsigned short logicalChannelNumber,
                        unsigned char dishSubChannelNumber,
                        bool visibleInGuide,
                        unsigned short referenceServiceId,
                        bool isHighDefinition,
                        bool isStandardDefinition,
                        bool isThreeDimensional,
                        unsigned short streamCountVideo,
                        unsigned short streamCountAudio,
                        const vector<unsigned long>& audioLanguages,
                        const vector<unsigned long>& subtitlesLanguages,
                        unsigned char openTvCategoryId,
                        unsigned char virginMediaCategoryId,
                        unsigned short dishMarketId,
                        const vector<unsigned long>& availableInCountries,
                        const vector<unsigned long>& unavailableInCountries,
                        const vector<unsigned long>& availableInCells,
                        const vector<unsigned long>& unavailableInCells,
                        const vector<unsigned long long>& targetRegionIds,
                        unsigned short previousOriginalNetworkId,
                        unsigned short previousTransportStreamId,
                        unsigned short previousServiceId,
                        unsigned short epgOriginalNetworkId,
                        unsigned short epgTransportStreamId,
                        unsigned short epgServiceId,
                        const char* defaultAuthority);
    void OnSdtRemoved(unsigned char tableId,
                        unsigned short originalNetworkId,
                        unsigned short transportStreamId,
                        unsigned short serviceId,
                        bool eitScheduleFlag,
                        bool eitPresentFollowingFlag,
                        unsigned char runningStatus,
                        bool freeCaMode,
                        unsigned char serviceType,
                        const map<unsigned long, char*>& providerNames,
                        const map<unsigned long, char*>& serviceNames,
                        unsigned short logicalChannelNumber,
                        unsigned char dishSubChannelNumber,
                        bool visibleInGuide,
                        unsigned short referenceServiceId,
                        bool isHighDefinition,
                        bool isStandardDefinition,
                        bool isThreeDimensional,
                        unsigned short streamCountVideo,
                        unsigned short streamCountAudio,
                        const vector<unsigned long>& audioLanguages,
                        const vector<unsigned long>& subtitlesLanguages,
                        unsigned char openTvCategoryId,
                        unsigned char virginMediaCategoryId,
                        unsigned short dishMarketId,
                        const vector<unsigned long>& availableInCountries,
                        const vector<unsigned long>& unavailableInCountries,
                        const vector<unsigned long>& availableInCells,
                        const vector<unsigned long>& unavailableInCells,
                        const vector<unsigned long long>& targetRegionIds,
                        unsigned short previousOriginalNetworkId,
                        unsigned short previousTransportStreamId,
                        unsigned short previousServiceId,
                        unsigned short epgOriginalNetworkId,
                        unsigned short epgTransportStreamId,
                        unsigned short epgServiceId,
                        const char* defaultAuthority);

    CCriticalSection m_section;
    CParserBat m_parserBat;
    CParserNitDvb m_parserNit;
    CParserSdt m_parserSdt;
    ICallBackGrabber* m_callBackGrabber;
    ICallBackSiDvb* m_callBackSiDvb;
    bool m_enableCrcCheck;
    bool m_isNitExpected;
};