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
#include "..\..\shared\ISectionCallback.h"
#include "..\..\shared\ISectionDispatcher.h"
#include "..\..\shared\Section.h"
#include "..\..\shared\SectionDecoder.h"
#include "..\..\shared\TsHeader.h"
#include "ICallBackEam.h"
#include "ICallBackGrabber.h"
#include "ICallBackLvct.h"
#include "ICallBackMgt.h"
#include "ICallBackNitAtsc.h"
#include "ICallBackNtt.h"
#include "ICallBackSiAtscScte.h"
#include "ICallBackStt.h"
#include "ICallBackSvct.h"
#include "IGrabberSiAtsc.h"
#include "IGrabberSiScte.h"
#include "ISystemTimeInfoProviderAtscScte.h"
#include "ParserEam.h"
#include "ParserLvct.h"
#include "ParserMgt.h"
#include "ParserNtt.h"
#include "ParserNitAtsc.h"
#include "ParserSttAtsc.h"
#include "ParserSttScte.h"
#include "ParserSvct.h"

using namespace MediaPortal;
using namespace std;


#define PID_ATSC_BASE 0x1ffb
#define PID_SCTE_BASE 0x1ffc


class CGrabberSiAtscScte
  : public CUnknown, ICallBackEam, ICallBackLvct, ICallBackMgt,
    ICallBackNitAtsc, ICallBackNtt, ICallBackStt, ICallBackSvct,
    public IGrabberSiScte, public ISectionCallback,
    public ISystemTimeInfoProviderAtscScte
{
  public:
    CGrabberSiAtscScte(unsigned short pid,
                        ISectionDispatcher* sectionDispatcher,
                        ICallBackSiAtscScte* callBack,
                        LPUNKNOWN unk,
                        HRESULT* hr);
    virtual ~CGrabberSiAtscScte();

    DECLARE_IUNKNOWN

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    void Reset(bool enableCrcCheck);
    STDMETHODIMP_(void) SetCallBack(ICallBackGrabber* callBack);
    bool OnTsPacket(const CTsHeader& header, const unsigned char* tsPacket);
    void OnNewSection(unsigned short pid, unsigned char tableId, const CSection& section);
    void OnNewSection(unsigned short pid,
                      unsigned char tableId,
                      const CSection& section,
                      bool isOutOfBandSection);

    STDMETHODIMP_(bool) IsSeenLvct();
    bool IsSeenMgt() const;
    STDMETHODIMP_(bool) IsSeenSvct();
    STDMETHODIMP_(bool) IsReadyLvct();
    bool IsReadyMgt() const;
    STDMETHODIMP_(bool) IsReadySvct();

    bool GetLatestEmergencyAlertMessage(unsigned short& id,
                                        unsigned long& originatorCode,
                                        char* eventCode,
                                        unsigned short& eventCodeBufferSize,
                                        unsigned char& alertMessageTimeRemaining,
                                        unsigned long& eventStartTime,
                                        unsigned short& eventDuration,
                                        unsigned char& alertPriority,
                                        unsigned short& detailsOobSourceId,
                                        unsigned short& detailsMajorChannelNumber,
                                        unsigned short& detailsMinorChannelNumber,
                                        unsigned char& detailsRfChannel,
                                        unsigned short& detailsProgramNumber,
                                        unsigned short& audioOobSourceId,
                                        unsigned char& textCount,
                                        unsigned long* locationCodes,
                                        unsigned char& locationCodeCount,
                                        unsigned long* exceptions,
                                        unsigned char& exceptionCount,
                                        unsigned long* alternativeExceptions,
                                        unsigned char& alternativeExceptionCount) const;
    bool GetLatestEmergencyAlertMessageTextByIndex(unsigned char index,
                                                    unsigned long& language,
                                                    char* alertText,
                                                    unsigned short& alertTextBufferSize,
                                                    char* natureOfActivationText,
                                                    unsigned short& natureOfActivationTextBufferSize) const;
    bool GetLatestEmergencyAlertMessageTextByLanguage(unsigned long language,
                                                      char* alertText,
                                                      unsigned short& alertTextBufferSize,
                                                      char* natureOfActivationText,
                                                      unsigned short& natureOfActivationTextBufferSize) const;


    STDMETHODIMP_(unsigned short) GetLvctChannelCount();
    STDMETHODIMP_(bool) GetLvctChannel(unsigned short index,
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
                                        unsigned char* pathSelect,
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
                                        unsigned char* captionsLanguageCount);
    STDMETHODIMP_(bool) GetLvctChannelLongNameByIndex(unsigned short channelIndex,
                                                      unsigned char nameIndex,
                                                      unsigned long* language,
                                                      char* name,
                                                      unsigned short* nameBufferSize);
    STDMETHODIMP_(bool) GetLvctChannelLongNameByLanguage(unsigned short channelIndex,
                                                          unsigned long language,
                                                          char* name,
                                                          unsigned short* nameBufferSize);


    unsigned short GetMasterGuideTableCount() const;
    bool GetMasterGuideTable(unsigned short index,
                              unsigned short& tableType,
                              unsigned short& pid,
                              unsigned char& versionNumber,
                              unsigned long& numberBytes) const;


    bool GetSystemTimeDetail(unsigned long& systemTime,
                              unsigned char& gpsUtcOffset,
                              bool& isDaylightSavingStateKnown,
                              bool& isDaylightSaving,
                              unsigned char& daylightSavingDayOfMonth,
                              unsigned char& daylightSavingHour) const;


    STDMETHODIMP_(unsigned short) GetSvctVirtualChannelCount();
    STDMETHODIMP_(bool) GetSvctVirtualChannel(unsigned short index,
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
                                              unsigned char* modulationFormat);
    STDMETHODIMP_(unsigned short) GetSvctDefinedChannelCount();
    STDMETHODIMP_(bool) GetSvctDefinedChannel(unsigned short index,
                                              unsigned char* transmissionMedium,
                                              unsigned short* vctId,
                                              unsigned short* virtualChannelNumber);

    STDMETHODIMP_(void) OnOutOfBandSectionReceived(unsigned char* sectionData,
                                                    unsigned short sectionDataBufferSize);

  private:
    void OnTableSeen(unsigned char tableId);
    void OnTableComplete(unsigned char tableId);
    void OnTableChange(unsigned char tableId);

    void OnEamReceived(unsigned short id,
                        unsigned long originatorCode,
                        const char* eventCode,
                        const map<unsigned long, char*>& NatureOfActivationTexts,
                        unsigned char alertMessageTimeRemaining,
                        unsigned long eventStartTime,
                        unsigned short eventDuration,
                        unsigned char alertPriority,
                        unsigned short detailsOobSourceId,
                        unsigned short detailsMajorChannelNumber,
                        unsigned short detailsMinorChannelNumber,
                        unsigned char detailsRfChannel,
                        unsigned short detailsProgramNumber,
                        unsigned short audioOobSourceId,
                        const map<unsigned long, char*>& alertTexts,
                        const vector<unsigned long>& locationCodes,
                        const vector<unsigned long>& exceptions,
                        const vector<unsigned long>& alternativeExceptions);
    void OnEamChanged(unsigned short id,
                        unsigned long originatorCode,
                        const char* eventCode,
                        const map<unsigned long, char*>& NatureOfActivationTexts,
                        unsigned char alertMessageTimeRemaining,
                        unsigned long eventStartTime,
                        unsigned short eventDuration,
                        unsigned char alertPriority,
                        unsigned short detailsOobSourceId,
                        unsigned short detailsMajorChannelNumber,
                        unsigned short detailsMinorChannelNumber,
                        unsigned char detailsRfChannel,
                        unsigned short detailsProgramNumber,
                        unsigned short audioOobSourceId,
                        const map<unsigned long, char*>& alertTexts,
                        const vector<unsigned long>& locationCodes,
                        const vector<unsigned long>& exceptions,
                        const vector<unsigned long>& alternativeExceptions);
    void OnEamRemoved(unsigned short id,
                        unsigned long originatorCode,
                        const char* eventCode,
                        const map<unsigned long, char*>& NatureOfActivationTexts,
                        unsigned char alertMessageTimeRemaining,
                        unsigned long eventStartTime,
                        unsigned short eventDuration,
                        unsigned char alertPriority,
                        unsigned short detailsOobSourceId,
                        unsigned short detailsMajorChannelNumber,
                        unsigned short detailsMinorChannelNumber,
                        unsigned char detailsRfChannel,
                        unsigned short detailsProgramNumber,
                        unsigned short audioOobSourceId,
                        const map<unsigned long, char*>& alertTexts,
                        const vector<unsigned long>& locationCodes,
                        const vector<unsigned long>& exceptions,
                        const vector<unsigned long>& alternativeExceptions);

    void OnMgtReceived(unsigned short tableType,
                        unsigned short pid,
                        unsigned char versionNumber,
                        unsigned long numberBytes);
    void OnMgtChanged(unsigned short tableType,
                        unsigned short pid,
                        unsigned char versionNumber,
                        unsigned long numberBytes);
    void OnMgtRemoved(unsigned short tableType,
                        unsigned short pid,
                        unsigned char versionNumber,
                        unsigned long numberBytes);

    CCriticalSection m_section;
    CSectionDecoder m_sectionDecoder;
    CParserEam m_parserEam;
    CParserLvct m_parserLvct;
    CParserMgt m_parserMgt;
    CParserNitAtsc m_parserNit;
    CParserNtt m_parserNtt;
    CParserSttAtsc m_parserSttAtsc;
    CParserSttScte m_parserSttScte;
    CParserSvct m_parserSvct;
    ICallBackGrabber* m_callBackGrabber;
    ICallBackSiAtscScte* m_callBackSiAtscScte;
    bool m_enableCrcCheck;
};