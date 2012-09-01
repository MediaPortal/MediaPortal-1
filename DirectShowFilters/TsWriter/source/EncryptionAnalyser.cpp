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
#include <streams.h>
#include <initguid.h>

#include "EncryptionAnalyser.h"
#include "..\..\shared\packetsync.h"

extern void LogDebug(const char *fmt, ...) ;

CEncryptionAnalyser::CEncryptionAnalyser(LPUNKNOWN pUnk, HRESULT *phr) 
  : CUnknown( NAME ("MpTsEncryptionAnalyser"), pUnk)
{
  m_mPids.clear();
}

CEncryptionAnalyser::~CEncryptionAnalyser(void)
{
  m_mPids.clear();
}
  
STDMETHODIMP CEncryptionAnalyser::AddPid(int pid)
{
  try
  {
    LogDebug("EncryptionAnalyser: add PID 0x%x", pid);
    map<int, EncryptionState>::iterator it = m_mPids.find(pid);
    if (it != m_mPids.end())
    {
      LogDebug("EncryptionAnalyser: PID already being monitored, current state = %d", it->second);
    }
    else
    {
      m_mPids[pid] = EncryptionStateNotSet;
    }
    return S_OK;
  }
  catch (...)
  {
    LogDebug("EncryptionAnalyser: unhandled exception in AddPid()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::RemovePid(int pid)
{
  try
  {
    LogDebug("EncryptionAnalyser: remove PID 0x%x", pid);
    map<int, EncryptionState>::iterator it = m_mPids.find(pid);
    if (it == m_mPids.end())
    {
      LogDebug("EncryptionAnalyser: PID not being monitored");
    }
    else
    {
      m_mPids.erase(it);
    }
    return S_OK;
  }
  catch (...)
  {
    LogDebug("EncryptionAnalyser: unhandled exception in RemovePid()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::GetPidCount(int* pidCount)
{
  try
  {
    *pidCount = m_mPids.size();
    LogDebug("EncryptionAnalyser: get PID count, count = %d", pidCount);
    return S_OK;
  }
  catch (...)
  {
    LogDebug("EncryptionAnalyser: unhandled exception in GetPidCount()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::GetPid(int pidIdx, int* pid, EncryptionState* encryptionState)
{
  try
  {
    LogDebug("EncryptionAnalyser: get PID %d", pidIdx);
    *pid = -1;
    *encryptionState = EncryptionStateNotSet;
    if (pidIdx >= 0 && pidIdx < (int)m_mPids.size())
    {
      int count = 0;
      map<int, EncryptionState>::iterator it = m_mPids.begin();
      while (count < pidIdx)
      {
        it++;
        count++;
      }
      *pid = it->first;
      *encryptionState = it->second;
    }
    return S_OK;
  }
  catch (...)
  {
    LogDebug("EncryptionAnalyser: unhandled exception in GetPid()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::SetCallBack(IEncryptionStateChangeCallBack* callBack)
{
  try
  {
    LogDebug("EncryptionAnalyser: set callback 0x%x", callBack);
    m_pCallBack = callBack;
    return S_OK;
  }
  catch (...)
  {
    LogDebug("EncryptionAnalyser: unhandled exception in SetCallBack()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::Reset()
{
  try
  {
    LogDebug("EncryptionAnalyser: reset");
    m_mPids.clear();
    return S_OK;
  }
  catch (...)
  {
    LogDebug("EncryptionAnalyser: unhandled exception in Reset()");
  }
  return S_FALSE;
}

void CEncryptionAnalyser::OnTsPacket(byte* tsPacket)
{
  try
  {
    if (m_pCallBack == NULL)
    {
      return;
    }

    // Does the packet look okay?
    m_tsHeader.Decode(tsPacket);
    if (m_tsHeader.SyncByte != TS_PACKET_SYNC || m_tsHeader.TransportError)
    {
      return;
    }

    // Is this a packet from elementary stream that we've been asked to monitor?
    int pid = ((tsPacket[1] & 0x1f) << 8) + tsPacket[2];
    map<int, EncryptionState>::iterator it = m_mPids.find(pid);
    if (it == m_mPids.end())
    {
      return;
    }

    // Check the transport scrambling control field in the packet header.
    bool packetScrambled = (m_tsHeader.TScrambling != 0);
    if (packetScrambled && it->second != Encrypted)
    {
      LogDebug("EncryptionAnalyser: PID 0x%x state change %d -> %d (TS header)", it->first, it->second, m_tsHeader.TScrambling);
      it->second = Encrypted;
      if (m_pCallBack != NULL)
      {
        m_pCallBack->OnEncryptionStateChange(it->first, it->second);
      }
      // No need to bother checking the PES header.
      return;
    }

    // Does this packet contain the start of a PES packet?
    if (!m_tsHeader.PayloadUnitStart)
    {
      return;
    }
    // Does the packet include the PES scrambling control field.
    if (m_tsHeader.HasAdaptionField && (m_tsHeader.PayLoadStart == 4 || m_tsHeader.PayLoadStart > 181))
    {
      return;
    }
    // Does the PES packet look okay?
    int offset = m_tsHeader.PayLoadStart;
    if (tsPacket[offset] != 0 || tsPacket[offset + 1] != 0 || tsPacket[offset + 2] != 1 || (tsPacket[offset + 6] & 0xc0) != 0x80)
    {
      return;
    }

    // Check the scrambling control bits.
    int pes_scrambling_control = (tsPacket[offset + 6] & 0x30) >> 4;
    bool pesScrambled = (pes_scrambling_control != 0);
    if ((pesScrambled && it->second != Encrypted) || (!pesScrambled && it->second != Clear))
    {
      LogDebug("EncryptionAnalyser: PID 0x%x state change %d -> %d (PES header)", it->first, it->second, pes_scrambling_control);
      it->second = pesScrambled ? Encrypted : Clear;
      if (m_pCallBack != NULL)
      {
        m_pCallBack->OnEncryptionStateChange(it->first, it->second);
      }
    }
  }
  catch (...)
  {
    LogDebug("EncryptionAnalyser: unhandled exception in OnTsPacket()");
  }
}
