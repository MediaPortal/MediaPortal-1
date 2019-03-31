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
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\ISectionDispatcher.h"
#include "..\..\shared\TsHeader.h"
#include "ICallBackGrabber.h"
#include "ICallBackNitDvb.h"
#include "ICallBackSdt.h"
#include "ICallBackSiDvb.h"
#include "ICallBackTot.h"
#include "IDefaultAuthorityProvider.h"
#include "IGrabberSiFreesat.h"
#include "IMhwChannelInfoProvider.h"
#include "ISystemTimeInfoProviderDvb.h"
#include "ParserBat.h"
#include "ParserNitDvb.h"
#include "ParserSdt.h"
#include "ParserTot.h"

using namespace MediaPortal;
using namespace std;


class CGrabberSiDvb
  : public CUnknown, ICallBackNitDvb, ICallBackSdt, ICallBackTot,
    public IDefaultAuthorityProvider, public IGrabberSiFreesat,
    public ISystemTimeInfoProviderDvb
{
  public:
    CGrabberSiDvb(ISectionDispatcher* sectionDispatcher,
                  ICallBackSiDvb* callBack,
                  LPUNKNOWN unk,
                  HRESULT* hr);
    virtual ~CGrabberSiDvb();

    DECLARE_IUNKNOWN

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    void SetMediaHighwayChannelInfoProvider(IMhwChannelInfoProvider* mhwChannelInfoProvider);
    void SetPids(unsigned short pidBat,
                  unsigned short pidNit,
                  unsigned short pidSdt,
                  unsigned short pidTot);
    void Reset(bool enableCrcCheck);
    STDMETHODIMP_(void) SetCallBack(ICallBackGrabber* callBack);
    bool OnTsPacket(const CTsHeader& header, const unsigned char* tsPacket);

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

    STDMETHODIMP_(void) GetServiceCount(unsigned short* actualOriginalNetworkId,
                                        unsigned short* serviceCount);
    STDMETHODIMP_(bool) GetService(unsigned short index,
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

    STDMETHODIMP_(unsigned char) GetCyfrowyPolsatChannelCategoryNameCount(unsigned char categoryId);
    STDMETHODIMP_(bool) GetCyfrowyPolsatChannelCategoryNameByIndex(unsigned char categoryId,
                                                                    unsigned char index,
                                                                    unsigned long* language,
                                                                    char* name,
                                                                    unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetCyfrowyPolsatChannelCategoryNameByLanguage(unsigned char categoryId,
                                                                      unsigned long language,
                                                                      char* name,
                                                                      unsigned short* nameBufferSize);

    STDMETHODIMP_(unsigned char) GetFreesatRegionNameCount(unsigned long regionId);
    STDMETHODIMP_(bool) GetFreesatRegionNameByIndex(unsigned long regionId,
                                                    unsigned char index,
                                                    unsigned long* language,
                                                    char* name,
                                                    unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetFreesatRegionNameByLanguage(unsigned long regionId,
                                                        unsigned long language,
                                                        char* name,
                                                        unsigned short* nameBufferSize);

    STDMETHODIMP_(unsigned char) GetFreesatChannelCategoryNameCount(unsigned long categoryId);
    STDMETHODIMP_(bool) GetFreesatChannelCategoryNameByIndex(unsigned long categoryId,
                                                              unsigned char index,
                                                              unsigned long* language,
                                                              char* name,
                                                              unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetFreesatChannelCategoryNameByLanguage(unsigned long categoryId,
                                                                unsigned long language,
                                                                char* name,
                                                                unsigned short* nameBufferSize);

    STDMETHODIMP_(bool) GetMediaHighwayChannelCategoryName(unsigned short categoryId,
                                                            char* name,
                                                            unsigned short* nameBufferSize);

    STDMETHODIMP_(unsigned char) GetNorDigChannelListNameCount(unsigned long long channelListId);
    STDMETHODIMP_(bool) GetNorDigChannelListNameByIndex(unsigned long long channelListId,
                                                        unsigned char index,
                                                        unsigned long* language,
                                                        char* name,
                                                        unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetNorDigChannelListNameByLanguage(unsigned long long channelListId,
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
                                        bool* isMultipleInputStream,
                                        unsigned char* plpId);

    bool GetSystemTimeDetail(unsigned long long& systemTime,
                              unsigned char& localTimeOffsetCount) const;
    bool GetLocalTimeOffsetByIndex(unsigned char index,
                                    unsigned long& countryCode,
                                    unsigned char& countryRegionId,
                                    long& localTimeOffsetCurrent,
                                    unsigned long long& localTimeOffsetNextChangeDateTime,
                                    long& localTimeOffsetNext) const;
    bool GetLocalTimeOffsetByCountryAndRegion(unsigned long countryCode,
                                              unsigned char countryRegionId,
                                              long& localTimeOffsetCurrent,
                                              unsigned long long& localTimeOffsetNextChangeDateTime,
                                              long& localTimeOffsetNext) const;

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
                        unsigned char cyfrowyPolsatChannelCategoryId,
                        const vector<unsigned char>& openTvChannelCategoryIds,
                        unsigned char virginMediaChannelCategoryId,
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
                        unsigned char cyfrowyPolsatChannelCategoryId,
                        const vector<unsigned char>& openTvChannelCategoryIds,
                        unsigned char virginMediaChannelCategoryId,
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
                        unsigned char cyfrowyPolsatChannelCategoryId,
                        const vector<unsigned char>& openTvChannelCategoryIds,
                        unsigned char virginMediaChannelCategoryId,
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
    CParserTot m_parserTot;

    ICallBackGrabber* m_callBackGrabber;
    ICallBackSiDvb* m_callBackSiDvb;
    IMhwChannelInfoProvider* m_mhwChannelInfoProvider;
    bool m_enableCrcCheck;
    bool m_isNitExpected;
};