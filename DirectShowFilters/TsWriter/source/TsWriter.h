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
#include <streams.h>    // AMovieDllRegisterServer2(), CCritSec, CFactoryTemplate, CUnknown (IUnknown, LPUNKNOWN)
#include <vector>
#include <WinError.h>   // HRESULT
#include "..\..\shared\DebugSettings.h"
#include "..\..\shared\Section.h"
#include "EncryptionAnalyser.h"
#include "GrabberEpgAtsc.h"
#include "GrabberSiAtscScte.h"
#include "GrabberSiDvb.h"
#include "GrabberSiMpeg.h"
#include "ICallBackEncryptionAnalyser.h"
#include "ICallBackPidConsumer.h"
#include "ICallBackSiAtscScte.h"
#include "ICallBackSiDvb.h"
#include "ICallBackSiMpeg.h"
#include "IChannelObserver.h"
#include "IDefaultAuthorityProvider.h"
#include "IObserver.h"
#include "ITsAnalyser.h"
#include "ITsWriter.h"
#include "ParserAet.h"
#include "ParserEitDvb.h"
#include "ParserMhw.h"
#include "ParserOpenTv.h"
#include "PidUsage.h"
#include "TsChannel.h"
#include "TsWriterFilter.h"

using namespace std;


DEFINE_TVE_DEBUG_SETTING(TsWriterDisableCrcCheck)
DEFINE_TVE_DEBUG_SETTING(TsWriterDisableTsBufferReservation)
DEFINE_TVE_DEBUG_SETTING(TsWriterDumpInput)


class CTsWriter
  : public CUnknown, ICallBackEncryptionAnalyser, ICallBackPidConsumer,
    ICallBackSiAtscScte, ICallBackSiDvb, ICallBackSiMpeg,
    IDefaultAuthorityProvider, public ITsAnalyser, public ITsWriter
{
  public:
    CTsWriter(LPUNKNOWN unk, HRESULT* hr);
    ~CTsWriter();

    static CUnknown* WINAPI CreateInstance(LPUNKNOWN unk, HRESULT* hr);

    DECLARE_IUNKNOWN

    void AnalyseOobSiSection(const CSection& section);
    void AnalyseTsPacket(const unsigned char* tsPacket);

    STDMETHODIMP ConfigureLogging(wchar_t* path);
    STDMETHODIMP_(void) DumpInput(bool enableTs, bool enableOobSi);
    STDMETHODIMP_(void) CheckSectionCrcs(bool enable);
    STDMETHODIMP_(void) SetObserver(IObserver* observer);

    STDMETHODIMP_(void) Start();
    STDMETHODIMP_(void) Stop();

    STDMETHODIMP AddChannel(IChannelObserver* observer, long* handle);
    STDMETHODIMP_(void) GetPidState(unsigned short pid, unsigned long* state);
    STDMETHODIMP_(void) DeleteChannel(long handle);
    STDMETHODIMP_(void) DeleteAllChannels();

    STDMETHODIMP RecorderSetFileName(long handle, wchar_t* fileName);
    STDMETHODIMP RecorderSetPmt(long handle,
                                unsigned char* pmt,
                                unsigned short pmtSize,
                                bool isDynamicPmtChange);
    STDMETHODIMP RecorderStart(long handle);
    STDMETHODIMP RecorderPause(long handle, bool isPause);
    STDMETHODIMP RecorderGetStreamQuality(long handle,
                                          unsigned long long* countTsPackets,
                                          unsigned long long* countDiscontinuities,
                                          unsigned long long* countDroppedBytes);
    STDMETHODIMP RecorderStop(long handle);

    STDMETHODIMP TimeShifterSetFileName(long handle, wchar_t* fileName);
    STDMETHODIMP TimeShifterSetParameters(long handle,
                                          unsigned long fileCountMinimum,
                                          unsigned long fileCountMaximum,
                                          unsigned long long fileSizeBytes);
    STDMETHODIMP TimeShifterSetPmt(long handle,
                                    unsigned char* pmt,
                                    unsigned short pmtSize,
                                    bool isDynamicPmtChange);
    STDMETHODIMP TimeShifterStart(long handle);
    STDMETHODIMP TimeShifterPause(long handle, bool isPause);
    STDMETHODIMP TimeShifterGetStreamQuality(long handle,
                                              unsigned long long* countTsPackets,
                                              unsigned long long* countDiscontinuities,
                                              unsigned long long* countDroppedBytes);
    STDMETHODIMP TimeShifterGetCurrentFilePosition(long handle,
                                                    unsigned long long* position,
                                                    unsigned long* bufferId);
    STDMETHODIMP TimeShifterStop(long handle);

  private:
    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);
    CTsChannel* GetChannel(long handle);

    bool GetDefaultAuthority(unsigned short originalNetworkId,
                              unsigned short transportStreamId,
                              unsigned short serviceId,
                              char* defaultAuthority,
                              unsigned short& defaultAuthorityBufferSize) const;

    void OnTableSeen(unsigned char tableId);
    void OnTableComplete(unsigned char tableId);
    void OnTableChange(unsigned char tableId);

    void OnEncryptionStateChange(unsigned short pid,
                                  EncryptionState statePrevious,
                                  EncryptionState stateNew);

    void OnPidsRequired(unsigned short* pids, unsigned char pidCount, PidUsage usage);
    void OnPidsNotRequired(unsigned short* pids, unsigned char pidCount, PidUsage usage);

    void OnFreesatPids(unsigned short pidEitSchedule,
                        unsigned short pidEitPresentFollowing,
                        unsigned short pidSdt,
                        unsigned short pidBat,
                        unsigned short pidTdt,
                        unsigned short pidTot,
                        unsigned short pidNit);

    void OnSdtRunningStatus(unsigned short serviceId, unsigned char runningStatus);
    void OnOpenTvEpgService(unsigned short serviceId, unsigned short originalNetworkId);

    void OnCatReceived(const unsigned char* table, unsigned short tableSize);
    void OnCatChanged(const unsigned char* table, unsigned short tableSize);

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

    void OnPatProgramReceived(unsigned short programNumber, unsigned short pmtPid);
    void OnPatProgramChanged(unsigned short programNumber, unsigned short pmtPid);
    void OnPatProgramRemoved(unsigned short programNumber, unsigned short pmtPid);

    void OnPatTsidChanged(unsigned short oldTransportStreamId,
                          unsigned short newTransportStreamId);
    void OnPatNetworkPidChanged(unsigned short oldNetworkPid, unsigned short newNetworkPid);

    void OnPmtReceived(unsigned short programNumber,
                        unsigned short pid,
                        const unsigned char* table,
                        unsigned short tableSize);
    void OnPmtChanged(unsigned short programNumber,
                        unsigned short pid,
                        const unsigned char* table,
                        unsigned short tableSize);
    void OnPmtRemoved(unsigned short programNumber, unsigned short pid);

    CTsWriterFilter* m_filter;
    CCritSec m_filterLock;                  // filter control lock
    CCritSec m_receiveLock;                 // sample receive lock

    // electronic programme guide grabbers
    CGrabberEpgAtsc* m_grabberEpgAtsc;
    CParserEitDvb* m_grabberEpgDvb;
    CParserMhw* m_grabberEpgMhw;
    CParserOpenTv* m_grabberEpgOpenTv;
    CParserAet* m_grabberEpgScte;

    // service information grabbers
    CGrabberSiAtscScte* m_grabberSiAtsc;
    CGrabberSiDvb* m_grabberSiDvb;
    CGrabberSiDvb* m_grabberSiFreesat;       // DVB on different PIDs
    CGrabberSiMpeg* m_grabberSiMpeg;
    CGrabberSiAtscScte* m_grabberSiScte;

    vector<CTsChannel*> m_channels;
    unsigned long m_nextChannelId;
    CCritSec m_channelLock;

    unsigned short m_openTvEpgServiceId;
    bool m_isOpenTvEpgServiceRunning;
    unsigned short m_openTvEpgPmtPid;
    vector<unsigned short> m_openTvEpgPidsEvent;
    vector<unsigned short> m_openTvEpgPidsDescription;

    vector<unsigned short> m_atscEpgPidsEit;
    vector<unsigned short> m_atscEpgPidsEtt;
    vector<unsigned short> m_scteEpgPids;

    bool m_checkedIsFreesatTransportStream;
    bool m_isFreesatTransportStream;
    unsigned short m_freesatPmtPid;

    bool m_isRunning;
    bool m_checkSectionCrcs;
    CEncryptionAnalyser m_encryptionAnalyser;
    IObserver* m_observer;
};