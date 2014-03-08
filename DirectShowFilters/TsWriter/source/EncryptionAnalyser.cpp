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


// We have a threshold to avoid call back jitter.
#define STATE_CHANGE_THRESHOLD 3

// Refer to ISO/IEC 13818 part 1 table 2-22.
#define STREAM_ID_PROGRAM_STREAM_MAP 0xbc
#define STREAM_ID_PADDING 0xbe
#define STREAM_ID_PRIVATE_2 0xbf
#define STREAM_ID_ECM 0xf0
#define STREAM_ID_EMM 0xf1
#define STREAM_ID_DSMCC 0xf2
#define STREAM_ID_H222_TYPE_E 0xf8
#define STREAM_ID_PROGRAM_STREAM_DIRECTORY 0xff

extern void LogDebug(const char* fmt, ...);

CEncryptionAnalyser::CEncryptionAnalyser(LPUNKNOWN unk, HRESULT* hr) 
  : CUnknown(NAME("TsEncryptionAnalyser"), unk)
{
  Reset();
  m_callBack = NULL;
}

CEncryptionAnalyser::~CEncryptionAnalyser(void)
{
  CleanUp();
}

void CEncryptionAnalyser::CleanUp()
{
  map<int, PidState*>::iterator it = m_pids.begin();
  while (it != m_pids.end())
  {
    if (it->second != NULL)
    {
      delete it->second;
      it->second = NULL;
    }
    it++;
  }
  m_pids.clear();
}
  
STDMETHODIMP CEncryptionAnalyser::AddPid(int pid)
{
  try
  {
    LogDebug("encryption: add PID %d", pid);
    map<int, PidState*>::iterator it = m_pids.find(pid);
    if (it != m_pids.end())
    {
      LogDebug("encryption: PID already being monitored, current state = %d", it->second->State);
    }
    else
    {
      PidState* state = new PidState();
      state->Pid = pid;
      state->State = EncryptionStateNotSet;
      state->PacketCount = 0;
      m_pids[pid] = state;
    }
    return S_OK;
  }
  catch (...)
  {
    LogDebug("encryption: unhandled exception in AddPid()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::RemovePid(int pid)
{
  try
  {
    LogDebug("encryption: remove PID %d", pid);
    map<int, PidState*>::iterator it = m_pids.find(pid);
    if (it == m_pids.end())
    {
      LogDebug("encryption: PID not being monitored");
    }
    else
    {
      if (it->second != NULL)
      {
        delete it->second;
        it->second = NULL;
      }
      m_pids.erase(it);
    }
    return S_OK;
  }
  catch (...)
  {
    LogDebug("encryption: unhandled exception in RemovePid()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::GetPidCount(int* pidCount)
{
  try
  {
    *pidCount = m_pids.size();
    LogDebug("encryption: get PID count, count = %d", pidCount);
    return S_OK;
  }
  catch (...)
  {
    LogDebug("encryption: unhandled exception in GetPidCount()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::GetPidByIndex(int pidIdx, int* pid, EncryptionState* encryptionState)
{
  try
  {
    LogDebug("encryption: get PID by index %d", pidIdx);
    *pid = -1;
    *encryptionState = EncryptionStateNotSet;
    if (pidIdx >= 0 && pidIdx < (int)m_pids.size())
    {
      int count = 0;
      map<int, PidState*>::iterator it = m_pids.begin();
      while (count < pidIdx)
      {
        it++;
        count++;
      }
      *pid = it->first;
      *encryptionState = it->second->State;
    }
    else
    {
      LogDebug("encryption: index out of bounds, count = %d", m_pids.size());
    }
    return S_OK;
  }
  catch (...)
  {
    LogDebug("encryption: unhandled exception in GetPidByIndex()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::GetPid(int pid, EncryptionState* encryptionState)
{
  try
  {
    LogDebug("encryption: get PID %d", pid);
    *encryptionState = EncryptionStateNotSet;
    map<int, PidState*>::iterator it = m_pids.find(pid);
    if (it == m_pids.end())
    {
      LogDebug("encryption: PID not being monitored");
    }
    else
    {
      *encryptionState = it->second->State;
    }
    return S_OK;
  }
  catch (...)
  {
    LogDebug("encryption: unhandled exception in GetPidByIndex()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::SetCallBack(IEncryptionStateChangeCallBack* callBack)
{
  try
  {
    LogDebug("encryption: set call back 0x%x", callBack);
    m_callBack = callBack;
    return S_OK;
  }
  catch (...)
  {
    LogDebug("encryption: unhandled exception in SetCallBack()");
  }
  return S_FALSE;
}

STDMETHODIMP CEncryptionAnalyser::Reset()
{
  try
  {
    LogDebug("encryption: reset");
    CleanUp();
    return S_OK;
  }
  catch (...)
  {
    LogDebug("encryption: unhandled exception in Reset()");
  }
  return S_FALSE;
}

// Return true if the packet is scrambled, otherwise false.
bool CEncryptionAnalyser::OnTsPacket(byte* tsPacket)
{
  try
  {
    if (m_callBack == NULL)
    {
      return false;
    }

    // Does the TS packet look okay, and does it contain payload?
    // Note packets that don't carry any payload are considered to never be
    // scrambled.
    m_tsHeader.Decode(tsPacket);
    if (m_tsHeader.SyncByte != TS_PACKET_SYNC || m_tsHeader.TransportError || m_tsHeader.AdaptionFieldOnly())
    {
      // No. Assume not scrambled.
      return false;
    }

    // Is this a packet from elementary stream that we've been asked to monitor?
    map<int, PidState*>::iterator it = m_pids.find(m_tsHeader.Pid);
    if (it == m_pids.end())
    {
      // No. Assume not scrambled.
      return false;
    }
    PidState* state = it->second;

    // Check the transport scrambling control field in the packet header. Note
    // this is only enough to determine that the packet is scrambled, not that
    // it is not scrambled.
    bool tsPacketScrambled = (m_tsHeader.TScrambling != 0);
    if (tsPacketScrambled && state->State != Encrypted)
    {
      state->PacketCount++;
      if (state->PacketCount >= STATE_CHANGE_THRESHOLD)
      {
        LogDebug("encryption: PID %d state change %d -> %d (TS header)", state->Pid, state->State, tsPacketScrambled);
        state->State = Encrypted;
        state->PacketCount = 0;
        if (m_callBack != NULL)
        {
          m_callBack->OnEncryptionStateChange(state->Pid, state->State);
        }
      }
      // No need to bother checking the PES header.
      return true;
    }

    // Does this TS packet contain the start of a PES packet?
    if (!m_tsHeader.PayloadUnitStart)
    {
      // No. Assume status quo.
      return (state->State == Encrypted);
    }

    // Does the packet include the PES scrambling control field? Note according
    // to ETR 289 the header of a scrambled PES packet must not span multiple
    // TS packets.
    bool pesPacketScrambled = false;
    if (!m_tsHeader.HasAdaptionField || (m_tsHeader.PayLoadStart != 4 && m_tsHeader.PayLoadStart <= 181))
    {
      // Yes. Does it look okay?
      int offset = m_tsHeader.PayLoadStart;
      if (tsPacket[offset] != 0 || tsPacket[offset + 1] != 0 || tsPacket[offset + 2] != 1)
      {
        // No. Assume status quo.
        pesPacketScrambled = (state->State == Encrypted);
      }
      else
      {
        // Yes. Does the stream have scrambling control bits?
        byte streamId = tsPacket[offset + 3];
        if (streamId != STREAM_ID_PROGRAM_STREAM_MAP &&
          streamId != STREAM_ID_PADDING &&
          streamId != STREAM_ID_PRIVATE_2 &&
          streamId != STREAM_ID_ECM &&
          streamId != STREAM_ID_EMM &&
          streamId != STREAM_ID_DSMCC &&
          streamId != STREAM_ID_H222_TYPE_E &&
          streamId != STREAM_ID_PROGRAM_STREAM_DIRECTORY)
        {
          // Yes. The packet might be scrambled.
          int pesScramblingControl = (tsPacket[offset + 6] & 0x30) >> 4;
          pesPacketScrambled = (pesScramblingControl != 0);
        }
      }
    }

    if ((pesPacketScrambled && state->State != Encrypted) || (!pesPacketScrambled && state->State != Clear))
    {
      state->PacketCount++;
      if (state->PacketCount >= STATE_CHANGE_THRESHOLD)
      {
        LogDebug("encryption: PID %d state change %d -> %d (PES header)", state->Pid, it->second, pesPacketScrambled);
        state->State = pesPacketScrambled ? Encrypted : Clear;
        state->PacketCount = 0;
        if (m_callBack != NULL)
        {
          m_callBack->OnEncryptionStateChange(state->Pid, state->State);
        }
      }
    }
    return (state->State == Encrypted);
  }
  catch (...)
  {
    LogDebug("encryption: unhandled exception in OnTsPacket()");
  }
  return false;
}