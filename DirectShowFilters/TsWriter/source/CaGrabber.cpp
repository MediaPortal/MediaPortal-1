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
#include <streams.h>
#include "CaGrabber.h"

extern void LogDebug(const char *fmt, ...) ;

CCaGrabber::CCaGrabber(LPUNKNOWN pUnk, HRESULT *phr) 
  : CUnknown(NAME("MpTsCaGrabber"), pUnk)
{
  CSectionDecoder::SetPid(PID_CAT);
  m_pCallBack = NULL;
  Reset();
}

CCaGrabber::~CCaGrabber(void)
{
}

STDMETHODIMP CCaGrabber::Reset()
{
  try
  {
    LogDebug("CaGrabber: reset");
    CEnterCriticalSection enter(m_section);
    CSectionDecoder::Reset();
    m_iCaVersion = -1;
    LogDebug("CaGrabber: reset done");
  }
  catch (...)
  {
    LogDebug("CaGrabber: unhandled exception in Reset()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CCaGrabber::SetCallBack(ICaCallBack* callBack)
{
  try
  {
    CEnterCriticalSection enter(m_section);
    LogDebug("CaGrabber: set callback 0x%x", callBack);
    m_pCallBack = callBack;
  }
  catch (...)
  {
    LogDebug("CaGrabber: unhandled exception in SetCallBack()");
    return S_FALSE;
  }
  return S_OK;
}

void CCaGrabber::OnTsPacket(byte* tsPacket)
{
  try
  {
    CEnterCriticalSection enter(m_section);
    if (m_pCallBack == NULL)
    {
      return;
    }
    CSectionDecoder::OnTsPacket(tsPacket);
  }
  catch (...)
  {
    LogDebug("CaGrabber: unhandled exception in OnTsPacket()");
  }
}

void CCaGrabber::OnNewSection(CSection& section)
{
  try
  {
    if (section.table_id != 1)
    {
      // Ignore non-CAT sections.
      return;
    }

    CEnterCriticalSection enter(m_section);

    if (m_pCallBack == NULL || m_iCaVersion == section.version_number)
    {
      // CAT hasn't changed or we can't perform a callback. Save effort...
      return;
    }

    if (m_iCaVersion < 0)
    {
      LogDebug("CaGrabber: got CAT, version = %d", section.version_number);
    }
    else
    {
      LogDebug("CaGrabber: found new CAT version %d (old version %d)", section.version_number, m_iCaVersion);
    }

    // The + 3 is because the section length doesn't include the
    // table ID, section syntax indicator, and section length bytes
    // that we want to pass back.
    m_iCaLength = section.section_length + 3;
    m_iCaVersion = section.version_number;
    memcpy(m_caData, section.Data, m_iCaLength);

    LogDebug("CaGrabber: do callback...");
    m_pCallBack->OnCaReceived();

    // TODO: I don't think we really want to do this. What happens
    // if the CAT changes. Is that bad?
    m_pCallBack = NULL;
  }
  catch (...)
  {
    LogDebug("CaGrabber: unhandled exception in OnNewSection()");
  }
}

STDMETHODIMP CCaGrabber::GetCaData(BYTE *caData)
{
  try
  {
    CEnterCriticalSection enter(m_section);
    if (m_iCaLength > 0)
    {
      memcpy(caData, m_caData, m_iCaLength);
      return m_iCaLength;
    }
  }
  catch (...)
  {
    LogDebug("CaGrabber: unhandled exception in GetCaData()");
  }
  return 0;
}
