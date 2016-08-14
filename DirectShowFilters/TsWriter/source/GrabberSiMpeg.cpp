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
#include "GrabberSiMpeg.h"
#include "EnterCriticalSection.h"


extern void LogDebug(const wchar_t* fmt, ...);

CGrabberSiMpeg::CGrabberSiMpeg(ICallBackSiMpeg* callBack,
                                IEncryptionAnalyser* analyser,
                                LPUNKNOWN unk,
                                HRESULT* hr)
  : CUnknown(NAME("MPEG SI Grabber"), unk)
{
  if (callBack == NULL)
  {
    LogDebug(L"SI MPEG: call back not supplied");
    *hr = E_INVALIDARG;
    return;
  }
  if (analyser == NULL)
  {
    LogDebug(L"SI MPEG: analyser not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_callBackGrabber = NULL;
  m_callBackSiMpeg = callBack;
  m_encryptionAnalyser = analyser;

  m_pmtReadyCount = 0;
  m_isSeenPmt = false;

  m_catGrabber.SetCallBack(this);
  m_patParser.SetCallBack(this);
}

CGrabberSiMpeg::~CGrabberSiMpeg()
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = NULL;
  m_callBackSiMpeg = NULL;
  m_encryptionAnalyser = NULL;

  map<unsigned short, CGrabberPmt*>::iterator it = m_pmtGrabbers.begin();
  for ( ; it != m_pmtGrabbers.end(); it++)
  {
    if (it->second != NULL)
    {
      delete it->second;
      it->second = NULL;
    }
  }
  m_pmtGrabbers.clear();
}

STDMETHODIMP CGrabberSiMpeg::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
  if (ppv == NULL)
  {
    return E_INVALIDARG;
  }

  if (iid == IID_IGRABBER)
  {
    return GetInterface((IGrabber*)this, ppv);
  }
  if (iid == IID_IGRABBER_SI_MPEG)
  {
    return GetInterface((IGrabberSiMpeg*)this, ppv);
  }
  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

void CGrabberSiMpeg::Reset()
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, CGrabberPmt*>::iterator it = m_pmtGrabbers.begin();
  for ( ; it != m_pmtGrabbers.end(); it++)
  {
    if (it->second != NULL)
    {
      delete it->second;
      it->second = NULL;
    }
  }
  m_pmtGrabbers.clear();
  m_pmtReadyCount = 0;

  m_catGrabber.Reset();
  m_patParser.Reset();
}

STDMETHODIMP_(void) CGrabberSiMpeg::SetCallBack(ICallBackGrabber* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = callBack;
  m_isSeenPmt = false;
}

bool CGrabberSiMpeg::OnTsPacket(CTsHeader& header, unsigned char* tsPacket)
{
  if (header.Pid == m_catGrabber.GetPid())
  {
    m_catGrabber.OnTsPacket(header, tsPacket);
    return true;
  }
  if (header.Pid == m_patParser.GetPid())
  {
    m_patParser.OnTsPacket(header, tsPacket);
    return true;
  }

  bool result = false;
  CEnterCriticalSection lock(m_section);
  map<unsigned short, CGrabberPmt*>::iterator it = m_pmtGrabbers.begin();
  for ( ; it != m_pmtGrabbers.end(); it++)
  {
    // Note: one PID may be used to carry PMT for more than one program.
    CGrabberPmt* grabber = it->second;
    if (grabber != NULL)
    {
      // We must pass packets to the grabber even if the PID doesn't match.
      grabber->OnTsPacket(header, tsPacket);
      if (header.Pid == grabber->GetPid())
      {
        result = true;
      }
    }
  }
  return result;
}

STDMETHODIMP_(bool) CGrabberSiMpeg::IsReadyPat()
{
  return m_patParser.IsReady();
}

STDMETHODIMP_(bool) CGrabberSiMpeg::IsReadyCat()
{
  return m_catGrabber.IsReady();
}

STDMETHODIMP_(bool) CGrabberSiMpeg::IsReadyPmt()
{
  return m_pmtReadyCount != 0 && m_pmtReadyCount == m_pmtGrabbers.size();
}

STDMETHODIMP_(void) CGrabberSiMpeg::GetTransportStreamDetail(unsigned short* transportStreamId,
                                                              unsigned short* networkPid,
                                                              unsigned short* programCount)
{
  m_patParser.GetTransportStreamDetail(*transportStreamId, *networkPid, *programCount);
}

STDMETHODIMP_(bool) CGrabberSiMpeg::GetProgramByIndex(unsigned short index,
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
                                                      unsigned char* subtitlesLanguageCount)
{
  CEnterCriticalSection lock(m_section);
  if (index >= m_pmtGrabbers.size())
  {
    LogDebug(L"SI MPEG: invalid program index, index = %hu, program count = %llu",
              index, (unsigned long long)m_pmtGrabbers.size());
    return false;
  }

  map<unsigned short, CGrabberPmt*>::const_iterator it = m_pmtGrabbers.begin();
  for ( ; it != m_pmtGrabbers.end(); it++)
  {
    if (index != 0)
    {
      index--;
      continue;
    }

    if (it->second == NULL)
    {
      LogDebug(L"SI MPEG: invalid PMT grabber, index = %hu, program number = %hu",
                index, it->first);
    }
    return it->second->GetProgramInformation(*pmtPid,
                                              *programNumber,
                                              *isPmtReceived,
                                              *streamCountVideo,
                                              *streamCountAudio,
                                              *isEncrypted,
                                              *isEncryptionDetectionAccurate,
                                              *isThreeDimensional,
                                              audioLanguages,
                                              *audioLanguageCount,
                                              subtitlesLanguages,
                                              *subtitlesLanguageCount);
  }
  return false;
}

STDMETHODIMP_(bool) CGrabberSiMpeg::GetProgramByNumber(unsigned short programNumber,
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
                                                        unsigned char* subtitlesLanguageCount)
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, CGrabberPmt*>::const_iterator it = m_pmtGrabbers.find(programNumber);
  if (it == m_pmtGrabbers.end() || it->second == NULL)
  {
    LogDebug(L"SI MPEG: invalid program number, program number = %hu",
              programNumber);
    return false;
  }

  return it->second->GetProgramInformation(*pmtPid,
                                            programNumber,
                                            *isPmtReceived,
                                            *streamCountVideo,
                                            *streamCountAudio,
                                            *isEncrypted,
                                            *isEncryptionDetectionAccurate,
                                            *isThreeDimensional,
                                            audioLanguages,
                                            *audioLanguageCount,
                                            subtitlesLanguages,
                                            *subtitlesLanguageCount);
}

STDMETHODIMP_(bool) CGrabberSiMpeg::GetCat(unsigned char* table,
                                            unsigned short* tableBufferSize)
{
  return m_catGrabber.GetTable(table, *tableBufferSize);
}

STDMETHODIMP_(bool) CGrabberSiMpeg::GetPmt(unsigned short programNumber,
                                            unsigned char* table,
                                            unsigned short* tableBufferSize)
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, CGrabberPmt*>::const_iterator it = m_pmtGrabbers.find(programNumber);
  if (it == m_pmtGrabbers.end() || it->second == NULL)
  {
    LogDebug(L"SI MPEG: invalid program number, program number = %hu",
              programNumber);
    return false;
  }

  return it->second->GetTable(table, *tableBufferSize);
}

bool CGrabberSiMpeg::GetFreesatPids(bool& isFreesatSiPresent,
                                    unsigned short& pidEitSchedule,
                                    unsigned short& pidEitPresentFollowing,
                                    unsigned short& pidSdt,
                                    unsigned short& pidBat,
                                    unsigned short& pidNit)
{
  CEnterCriticalSection lock(m_section);
  if (m_pmtGrabbers.size() == 0)
  {
    LogDebug(L"SI MPEG: PAT not yet received");
    return false;
  }
  if (m_pmtReadyCount == 0)
  {
    LogDebug(L"SI MPEG: Freesat PMT not yet received, program count = %llu",
              (unsigned long long)m_pmtGrabbers.size());
    return false;
  }

  map<unsigned short, CGrabberPmt*>::const_iterator it = m_pmtGrabbers.begin();
  for ( ; it != m_pmtGrabbers.end(); it++)
  {
    CGrabberPmt* grabber = it->second;
    if (grabber != NULL && grabber->IsReady())
    {
      return grabber->GetFreesatPids(isFreesatSiPresent,
                                      pidEitSchedule,
                                      pidEitPresentFollowing,
                                      pidSdt,
                                      pidBat,
                                      pidNit);
    }
  }
  return false;
}

bool CGrabberSiMpeg::GetOpenTvEpgPids(unsigned short programNumber,
                                      unsigned short& pmtPid,
                                      bool& isOpenTvEpgProgram,
                                      vector<unsigned short>& pidsEvent,
                                      vector<unsigned short>& pidsDescription)
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, CGrabberPmt*>::const_iterator it = m_pmtGrabbers.find(programNumber);
  if (it == m_pmtGrabbers.end() || it->second == NULL)
  {
    return false;
  }
  it->second->GetFilter(pmtPid, programNumber);
  return it->second->GetOpenTvEpgPids(isOpenTvEpgProgram, pidsEvent, pidsDescription);
}

void CGrabberSiMpeg::OnTableSeen(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableSeen(PID_PAT, TABLE_ID_PAT);
  }
  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnTableSeen(TABLE_ID_PAT);
  }
}

void CGrabberSiMpeg::OnTableComplete(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableComplete(PID_PAT, TABLE_ID_PAT);
  }
  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnTableComplete(TABLE_ID_PAT);
  }
}

void CGrabberSiMpeg::OnTableChange(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableChange(PID_PAT, TABLE_ID_PAT);
  }
  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnTableChange(TABLE_ID_PAT);
  }
}

void CGrabberSiMpeg::OnCatReceived(const unsigned char* table, unsigned short tableSize)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnCatReceived(table, tableSize);
  }
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableComplete(PID_CAT, TABLE_ID_CAT);
  }
}

void CGrabberSiMpeg::OnCatChanged(const unsigned char* table, unsigned short tableSize)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnCatChanged(table, tableSize);
  }
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableComplete(PID_CAT, TABLE_ID_CAT);
  }
}

void CGrabberSiMpeg::OnPatProgramReceived(unsigned short programNumber, unsigned short pmtPid)
{
  CEnterCriticalSection lock(m_section);
  if (m_pmtGrabbers.find(programNumber) != m_pmtGrabbers.end())
  {
    LogDebug(L"SI MPEG: received new PAT program for known program number, program number = %hu, PMT PID = %hu",
              programNumber, pmtPid);
    OnPatProgramChanged(programNumber, pmtPid);
    return;
  }

  CGrabberPmt* grabber = new CGrabberPmt(m_encryptionAnalyser);
  if (grabber == NULL)
  {
    LogDebug(L"SI MPEG: failed to allocate PMT grabber, program number = %hu, PMT PID = %hu",
              programNumber, pmtPid);
    return;
  }
  m_pmtGrabbers[programNumber] = grabber;
  grabber->SetCallBack(this);
  grabber->SetFilter(pmtPid, programNumber);
  grabber->Reset();

  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnPatProgramReceived(programNumber, pmtPid);
  }
}

void CGrabberSiMpeg::OnPatProgramChanged(unsigned short programNumber, unsigned short pmtPid)
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, CGrabberPmt*>::const_iterator it = m_pmtGrabbers.find(programNumber);
  if (it == m_pmtGrabbers.end())
  {
    LogDebug(L"SI MPEG: received PAT program change for unknown program number, program number = %hu, PMT PID = %hu",
              programNumber, pmtPid);
    OnPatProgramReceived(programNumber, pmtPid);
    return;
  }

  CGrabberPmt* grabber = it->second;
  if (grabber->IsReady())
  {
    m_pmtReadyCount--;
  }
  grabber->SetFilter(pmtPid, programNumber);
  grabber->Reset();

  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnPatProgramChanged(programNumber, pmtPid);
  }
}

void CGrabberSiMpeg::OnPatProgramRemoved(unsigned short programNumber, unsigned short pmtPid)
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, CGrabberPmt*>::iterator it = m_pmtGrabbers.find(programNumber);
  if (it == m_pmtGrabbers.end())
  {
    LogDebug(L"SI MPEG: received PAT program removal for unknown program number, program number = %hu, PMT PID = %hu",
              programNumber, pmtPid);
    return;
  }

  CGrabberPmt* grabber = it->second;
  if (grabber != NULL)
  {
    if (grabber->IsReady())
    {
      m_pmtReadyCount--;
    }

    delete grabber;
    it->second = NULL;
  }
  m_pmtGrabbers.erase(it);

  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnPatProgramRemoved(programNumber, pmtPid);
  }
}

void CGrabberSiMpeg::OnPatTsidChanged(unsigned short oldTransportStreamId,
                                      unsigned short newTransportStreamId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnPatTsidChanged(oldTransportStreamId, newTransportStreamId);
  }
}

void CGrabberSiMpeg::OnPatNetworkPidChanged(unsigned short oldNetworkPid,
                                            unsigned short newNetworkPid)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnPatNetworkPidChanged(oldNetworkPid, newNetworkPid);
  }
}

void CGrabberSiMpeg::OnPmtReceived(unsigned short programNumber,
                                    unsigned short pid,
                                    const unsigned char* table,
                                    unsigned short tableSize)
{
  CEnterCriticalSection lock(m_section);
  m_pmtReadyCount++;

  // Check for Freesat PIDs in the first PMT that we receive.
  if (m_pmtReadyCount == 1)
  {
    LogDebug(L"SI MPEG: received first PMT, checking for Freesat PIDs");
    map<unsigned short, CGrabberPmt*>::const_iterator it = m_pmtGrabbers.find(programNumber);
    if (it == m_pmtGrabbers.end() || it->second == NULL)
    {
      LogDebug(L"SI MPEG: received PMT for unknown grabber, program number = %hu, PID = %hu",
                programNumber, pid);
    }
    else
    {
      CGrabberPmt* grabber = it->second;
      if (grabber != NULL && grabber->IsReady())
      {
        bool isFreesatSiPresent;
        unsigned short pidEitSchedule;
        unsigned short pidEitPresentFollowing;
        unsigned short pidSdt;
        unsigned short pidBat;
        unsigned short pidNit;
        if (it->second->GetFreesatPids(isFreesatSiPresent,
                                        pidEitSchedule,
                                        pidEitPresentFollowing,
                                        pidSdt,
                                        pidBat,
                                        pidNit) && isFreesatSiPresent)
        {
          LogDebug(L"  Freesat PIDs, EIT schedule = %hu, EIT P/F = %hu, SDT = %hu, BAT = %hu, NIT = %hu",
                    pidEitSchedule, pidEitPresentFollowing, pidSdt, pidBat,
                    pidNit);
          if (m_callBackSiMpeg != NULL)
          {
            m_callBackSiMpeg->OnFreesatPids(pidEitSchedule,
                                            pidEitPresentFollowing,
                                            pidSdt,
                                            pidBat,
                                            pidNit);
          }
        }
      }
    }
  }

  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnPmtReceived(programNumber, pid, table, tableSize);
  }
  if (m_callBackGrabber != NULL && !m_isSeenPmt)
  {
    m_callBackGrabber->OnTableSeen(PID_PAT, TABLE_ID_PMT);
    m_isSeenPmt = true;
  }
  if (m_patParser.IsReady() && m_pmtReadyCount == m_pmtGrabbers.size())
  {
    LogDebug(L"SI MPEG: ready, program count = %hu", m_pmtReadyCount);
    if (m_callBackGrabber != NULL)
    {
      m_callBackGrabber->OnTableComplete(PID_PAT, TABLE_ID_PMT);
    }
  }
}

void CGrabberSiMpeg::OnPmtChanged(unsigned short programNumber,
                                  unsigned short pid,
                                  const unsigned char* table,
                                  unsigned short tableSize)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnPmtChanged(programNumber, pid, table, tableSize);
  }
  if (m_callBackGrabber != NULL && !m_isSeenPmt)
  {
    m_callBackGrabber->OnTableChange(PID_PAT, TABLE_ID_PMT);
    m_isSeenPmt = true;
  }
  if (m_patParser.IsReady() && m_pmtReadyCount == m_pmtGrabbers.size())
  {
    LogDebug(L"SI MPEG: ready, program count = %hu", m_pmtReadyCount);
    if (m_callBackGrabber != NULL)
    {
      m_callBackGrabber->OnTableComplete(PID_PAT, TABLE_ID_PMT);
    }
  }
}

void CGrabberSiMpeg::OnPmtRemoved(unsigned short programNumber, unsigned short pid)
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, CGrabberPmt*>::const_iterator it = m_pmtGrabbers.find(programNumber);
  if (it == m_pmtGrabbers.end() || it->second == NULL)
  {
    LogDebug(L"SI MPEG: received PMT removal for unknown grabber, program number = %hu, PID = %hu",
              programNumber, pid);
  }
  else if (it->second->IsReady())
  {
    m_pmtReadyCount--;
  }

  if (m_callBackSiMpeg != NULL)
  {
    m_callBackSiMpeg->OnPmtRemoved(programNumber, pid);
  }
}