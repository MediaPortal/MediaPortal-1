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
#include <streams.h>    // CUnknown, LPUNKNOWN
#include <vector>
#include <WinError.h>   // HRESULT
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\\TsHeader.h"
#include "GrabberCat.h"
#include "GrabberPmt.h"
#include "ICallBackCat.h"
#include "ICallBackGrabber.h"
#include "ICallBackPat.h"
#include "ICallBackPmt.h"
#include "ICallBackSiMpeg.h"
#include "IEncryptionAnalyser.h"
#include "IGrabberSiMpeg.h"
#include "ParserPat.h"

using namespace MediaPortal;
using namespace std;


class CGrabberSiMpeg
  : public CUnknown, ICallBackCat, ICallBackPat, ICallBackPmt,
    public IGrabberSiMpeg
{
  public:
    CGrabberSiMpeg(ICallBackSiMpeg* callBack,
                    IEncryptionAnalyser* analyser,
                    LPUNKNOWN unk,
                    HRESULT* hr);
    virtual ~CGrabberSiMpeg();

    DECLARE_IUNKNOWN

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    void Reset();
    STDMETHODIMP_(void) SetCallBack(ICallBackGrabber* callBack);
    bool OnTsPacket(const CTsHeader& header, const unsigned char* tsPacket);

    STDMETHODIMP_(bool) IsReadyPat();
    STDMETHODIMP_(bool) IsReadyCat();
    STDMETHODIMP_(bool) IsReadyPmt();

    STDMETHODIMP_(void) GetTransportStreamDetail(unsigned short* transportStreamId,
                                                  unsigned short* networkPid,
                                                  unsigned short* programCount);
    STDMETHODIMP_(bool) GetProgramByIndex(unsigned short index,
                                          unsigned short* programNumber,
                                          unsigned short* pmtPid,
                                          bool* isPmtReceived,
                                          unsigned short* streamCountVideo,
                                          unsigned short* streamCountAudio,
                                          bool* isEncrypted,
                                          bool* isEncryptionDetectionAccurate,
                                          bool* isThreeDimensional,
                                          unsigned long* audioLanguages,
                                          unsigned char* audioLanguageCount,
                                          unsigned long* subtitlesLanguages,
                                          unsigned char* subtitlesLanguageCount);
    STDMETHODIMP_(bool) GetProgramByNumber(unsigned short programNumber,
                                            unsigned short* pmtPid,
                                            bool* isPmtReceived,
                                            unsigned short* streamCountVideo,
                                            unsigned short* streamCountAudio,
                                            bool* isEncrypted,
                                            bool* isEncryptionDetectionAccurate,
                                            bool* isThreeDimensional,
                                            unsigned long* audioLanguages,
                                            unsigned char* audioLanguageCount,
                                            unsigned long* subtitlesLanguages,
                                            unsigned char* subtitlesLanguageCount);

    STDMETHODIMP_(bool) GetCat(unsigned char* table, unsigned short* tableBufferSize);
    STDMETHODIMP_(bool) GetPmt(unsigned short programNumber,
                                unsigned char* table,
                                unsigned short* tableBufferSize);

    bool GetFreesatPids(bool& isFreesatSiPresent,
                        unsigned short& pidEitSchedule,
                        unsigned short& pidEitPresentFollowing,
                        unsigned short& pidSdt,
                        unsigned short& pidBat,
                        unsigned short& pidTdt,
                        unsigned short& pidTot,
                        unsigned short& pidNit);
    bool GetOpenTvEpgPids(unsigned short programNumber,
                          unsigned short& pmtPid,
                          bool& isOpenTvEpgProgram,
                          vector<unsigned short>& pidsEvent,
                          vector<unsigned short>& pidsDescription);

  private:
    void OnTableSeen(unsigned char tableId);
    void OnTableComplete(unsigned char tableId);
    void OnTableChange(unsigned char tableId);

    void OnCatReceived(const unsigned char* table, unsigned short tableSize);
    void OnCatChanged(const unsigned char* table, unsigned short tableSize);

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

    CCriticalSection m_section;
    CParserPat m_patParser;
    CGrabberCat m_catGrabber;
    map<unsigned short, CGrabberPmt*> m_pmtGrabbers;    // key = program number
    unsigned short m_pmtReadyCount;
    ICallBackGrabber* m_callBackGrabber;
    ICallBackSiMpeg* m_callBackSiMpeg;
    IEncryptionAnalyser* m_encryptionAnalyser;
    bool m_isSeenPmt;
};