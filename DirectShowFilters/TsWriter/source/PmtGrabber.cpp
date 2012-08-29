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
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "pmtgrabber.h"
#include "PmtParser.h"

extern void LogDebug(const char *fmt, ...) ;


CPmtGrabber::CPmtGrabber(LPUNKNOWN pUnk, HRESULT *phr) 
  : CUnknown(NAME ("MpTsPmtGrabber"), pUnk)
{
  m_pCallBack = NULL;
  m_iPmtVersion = -1;
  m_iServiceId = -1;
  m_iCurrentServiceId = -1;
  memset(m_pmtPrevSection.Data, 0, sizeof(m_pmtPrevSection.Data));
}

CPmtGrabber::~CPmtGrabber(void)
{
}

STDMETHODIMP CPmtGrabber::SetPmtPid(int pmtPid, int serviceId)
{
  try
  {
    CEnterCriticalSection enter(m_section);
    CSectionDecoder::Reset();
    if (serviceId == 0)
    {
      LogDebug("PmtGrabber: grab first PMT");
      pmtPid = 0;       // Look for PAT sections.
    }
    else
    {
      if (pmtPid == 0)
      {
        LogDebug("PmtGrabber: grab PMT for service 0x%x", serviceId);
      }
      else
      {
        LogDebug("PmtGrabber: grab PMT PID from PID 0x%x for service 0x%x", pmtPid, serviceId);
      }
      m_sdtParser.Reset();
      m_sdtParser.SetCallBack(this);
      m_vctParser.Reset();
      m_vctParser.SetCallBack(this);
    }
    SetPid(pmtPid);   // Note: if the PMT PID is zero, we'll receive PAT sections.
    m_patParser.Reset();
    m_patParser.SetCallBack(this);
    m_iPmtVersion = -1;   // Indicates that we haven't seen PMT yet.
    m_iServiceId = serviceId;
    m_iCurrentServiceId = serviceId;
    memset(m_pmtPrevSection.Data, 0, sizeof(m_pmtPrevSection.Data));
  }
  catch (...)
  {
    LogDebug("PmtGrabber: exception in SetPmtPid()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CPmtGrabber::SetCallBack(IPmtCallBack* callBack)
{
  try
  {
    CEnterCriticalSection enter(m_section);
    LogDebug("PmtGrabber: set callback 0x%x", callBack);
    m_pCallBack = callBack;
  }
  catch (...)
  {
    LogDebug("PmtGrabber: exception in SetCallBack()");
    return S_FALSE;
  }
  return S_OK;
}

void CPmtGrabber::OnPatReceived(int serviceId, int pmtPid)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    LogDebug("PmtGrabber: PAT information for service 0x%x received, PMT PID = 0x%x", serviceId, pmtPid);
    if (m_iServiceId == 0)  // ([originally] searching for any service...)
    {
      if (GetPid() != 0)
      {
        LogDebug("PmtGrabber: previously monitoring service 0x%x (PMT PID 0x%x), switching to monitor new service...", m_iCurrentServiceId, GetPid());
      }
      SetPmtPid(pmtPid, serviceId);
      m_iServiceId = 0; // Important! Set back to zero so we keep monitoring for service ID changes.
    }
    else  // (searching for the PMT PID for a specific service...)
    {
      if (serviceId == m_iServiceId)
      {
        SetPmtPid(pmtPid, serviceId);
      }
    }
  }
  catch (...)
  {
    LogDebug("PmtGrabber: unhandled exception in OnPatReceived()");
  }
}

void CPmtGrabber::OnSdtReceived(const CChannelInfo& sdtInfo)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    // Do we have a service ID?
    if (m_iCurrentServiceId == 0)
    {
      return;
    }
    // Is this the service that we're interested in?
    if (sdtInfo.ServiceId != m_iCurrentServiceId)
    {
      return;
    }
    LogDebug("PmtGrabber: SDT information for service 0x%x received, is running = %d", sdtInfo.ServiceId, sdtInfo.IsRunning);
    if (!sdtInfo.IsRunning)
    {
      if (m_pCallBack != NULL)
      {
        LogDebug("PmtGrabber: do callback (SDT)...");
        m_pCallBack->OnPmtReceived(GetPid(), m_iCurrentServiceId, 0);
        m_sdtParser.SetCallBack(NULL);
        m_vctParser.SetCallBack(NULL);
      }
      else
      {
        LogDebug("PmtGrabber: callback is NULL");
      }
    }
  }
  catch (...)
  {
    LogDebug("PmtGrabber: unhandled exception in OnSdtReceived()");
  }
}

void CPmtGrabber::OnVctReceived(const CChannelInfo& vctInfo)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    // Do we have a service ID?
    if (m_iCurrentServiceId == 0)
    {
      return;
    }
    // Is this the service that we're interested in?
    if (vctInfo.ServiceId != m_iCurrentServiceId)
    {
      return;
    }
    LogDebug("PmtGrabber: VCT information for service 0x%x received, is running = %d", vctInfo.ServiceId, vctInfo.IsRunning);
    if (!vctInfo.IsRunning)
    {
      if (m_pCallBack != NULL)
      {
        LogDebug("PmtGrabber: do callback (VCT)...");
        m_pCallBack->OnPmtReceived(GetPid(), m_iCurrentServiceId, 0);
        m_sdtParser.SetCallBack(NULL);
        m_vctParser.SetCallBack(NULL);
      }
      else
      {
        LogDebug("PmtGrabber: callback is NULL");
      }
    }
  }
  catch (...)
  {
    LogDebug("PmtGrabber: unhandled exception in OnVctReceived()");
  }
}

void CPmtGrabber::OnTsPacket(byte* tsPacket)
{
  try
  {
    if (m_pCallBack == NULL)
    {
      return;
    }
    CEnterCriticalSection enter(m_section);
    if (m_iCurrentServiceId != 0)
    {
      m_sdtParser.OnTsPacket(tsPacket);
      m_vctParser.OnTsPacket(tsPacket);
    }
    if (GetPid() != 0)
    {
      // Only start monitoring PAT for PMT PID changes after we've found the
      // PMT PID for the first time.
      m_patParser.OnTsPacket(tsPacket);
    }
    CSectionDecoder::OnTsPacket(tsPacket);
  }
  catch (...)
  {
    LogDebug("PmtGrabber: exception in OnTsPacket()");
  }
}

void CPmtGrabber::OnNewSection(CSection& section)
{
  try
  {
    // If the current PID is zero, we've received a PAT section and we're meant to find the
    // current PMT PID for the service. We manually handle the first section to ensure that
    // the PAT contains at least one service.
    if (GetPid() == 0)
    {
      m_patParser.Reset();
      m_patParser.OnNewSection(section);
      // (Don't stop the PAT parser here - if the service ID passed to us
      // was not set then the service ID could potentially change mid-stream.)

      // Useful check: if the PAT doesn't list any services then it is pointless
      // to keep waiting for any PMT. Assume the service is not running.
      int serviceCount = m_patParser.GetServiceCount();
      if (serviceCount == 0)
      {
        LogDebug("PmtGrabber: PAT search failed, no services found");
        if (m_pCallBack != NULL)
        {
          LogDebug("PmtGrabber: do callback (PAT)...");
          m_pCallBack->OnPmtReceived(GetPid(), m_iServiceId, 0);
          m_sdtParser.SetCallBack(NULL);
          m_vctParser.SetCallBack(NULL);
        }
        else
        {
          LogDebug("PmtGrabber: callback is NULL");
        }
        return;
      }
      return;
    }

    if (section.table_id != 2)
    {
      // Ignore non-PMT sections.
      return;
    }

    CEnterCriticalSection enter(m_section);

    if (section.section_length < 0 || section.section_length >= MAX_SECTION_LENGTH)
    {
      return;
    }

    int serviceId = section.table_id_extension;
    if (serviceId != m_iCurrentServiceId) 
    {
      // This is a legitimate situation. Sometimes the PMT for multiple channels is carried on
      // one PID. The program number field within the PMT tells us which service each section
      // is associated with.
      return;
    }

    if (m_iPmtVersion < 0)
    {
      LogDebug("PmtGrabber: got PMT for service 0x%x from PID 0x%x", serviceId, GetPid());
    }

    // The + 3 is because the section length doesn't include the table ID, section
    // syntax indicator, and section length bytes that we want to pass back.
    m_iPmtLength = section.section_length + 3;
    memcpy(m_pmtData, section.Data, m_iPmtLength);

    // If the new section is not identical to the previous section...
    if (memcmp(section.Data, m_pmtPrevSection.Data, m_iPmtLength) != 0)
    {
      // Check section for corruption.
      CPmtParser currPmtParser;
      currPmtParser.SetPid(GetPid());
      if (!currPmtParser.DecodePmtSection(section))
      {
         LogDebug("PmtGrabber: error decoding PMT section for service 0x%x from PID 0x%x. Check your signal quality.", serviceId, GetPid());
         return;
      }

      // Decode the previous section so we can check if the service has different elementary streams.
      bool pidsChanged = false;
      CPmtParser prevPmtParser;
      prevPmtParser.SetPid(GetPid());
      prevPmtParser.DecodePmtSection(m_pmtPrevSection);

      if (!(prevPmtParser.GetPidInfo() == currPmtParser.GetPidInfo()))
      {
        LogDebug("PmtGrabber: PMT pids changed from:");
        prevPmtParser.GetPidInfo().LogPIDs();
        LogDebug("PmtGrabber: PMT pids changed to:");
        currPmtParser.GetPidInfo().LogPIDs();
        pidsChanged = true;
      }
      m_pmtPrevSection = section;

      // Only do a callback if it is absolutely necessary - see mantis #2886.
      if (m_pCallBack != NULL && m_iPmtVersion != section.version_number)
      {
        LogDebug("PmtGrabber: found new PMT version %d (old version %d) for service 0x%x from PID 0x%x", section.version_number, m_iPmtVersion, serviceId, GetPid());
        // If the elementary streams are different then a callback is required.
        if (pidsChanged)
        {
          LogDebug("PmtGrabber: do callback...");
          // If we receive PMT, assume the service is running.
          m_pCallBack->OnPmtReceived(GetPid(), serviceId, 1);
          // We're not interested in continually monitoring if the service is running.
          m_sdtParser.SetCallBack(NULL);
          m_vctParser.SetCallBack(NULL);
        }
        else 
        {
          LogDebug("PmtGrabber: callback not done because A/V PIDs haven't changed");
        }
      }
    }
     m_iPmtVersion = section.version_number;
  }
  catch (...)
  {
    LogDebug("PmtGrabber: exception in OnNewSection()");
  }
}

STDMETHODIMP CPmtGrabber::GetPmtData(BYTE *pmtData)
{
  try
  {
    CEnterCriticalSection enter(m_section);
    if (m_iPmtLength > 0)
    {
      memcpy(pmtData, m_pmtData, m_iPmtLength);
      return m_iPmtLength;
    }
  }
  catch (...)
  {
    LogDebug("PmtGrabber: exception in GetPmtData()");
  }
  return 0;
}