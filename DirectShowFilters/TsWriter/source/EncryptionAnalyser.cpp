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
#include "EncryptionAnalyser.h"
#include "..\..\shared\EnterCriticalSection.h"
#include "..\..\shared\PacketSync.h"


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


extern void LogDebug(const wchar_t* fmt, ...);

CEncryptionAnalyser::CEncryptionAnalyser()
{
  SetCallBack(NULL);
}

CEncryptionAnalyser::~CEncryptionAnalyser()
{
  SetCallBack(NULL);
  CleanUp();
}

void CEncryptionAnalyser::Reset()
{
  LogDebug(L"encryption: reset");
  CleanUp();
  LogDebug(L"encryption: reset done");
}

void CEncryptionAnalyser::SetCallBack(ICallBackEncryptionAnalyser* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

bool CEncryptionAnalyser::OnTsPacket(CTsHeader& header, unsigned char* tsPacket)
{
  try
  {
    // Does the TS packet look okay?
    if (header.SyncByte != TS_PACKET_SYNC || header.TransportError)
    {
      // No. Assume not scrambled.
      return false;
    }

    // Does the packet contain payload?
    // Note packets that don't carry any payload are considered to never be
    // scrambled.
    if (header.AdaptionFieldOnly())
    {
      // Ensure the packet is marked as not scrambled. This fixes compatibility
      // with certain streams.
      tsPacket[3] = tsPacket[3] & 0x3f;
      return false;
    }

    // Get or create the PID state.
    CEnterCriticalSection lock(m_section);
    PidState* state = NULL;
    map<unsigned short, PidState*>::const_iterator it = m_pids.find(header.Pid);
    if (it == m_pids.end() || it->second == NULL)
    {
      state = new PidState();
      if (state == NULL)
      {
        LogDebug(L"encryption: failed to allocate PID state, PID = %hu",
                  header.Pid);
        return false;
      }

      state->Pid = header.Pid;
      state->State = EncryptionStateNotSet;
      state->PacketCount = 0;
      m_pids[header.Pid] = state;
    }
    else
    {
      state = it->second;
    }

    // Check the transport scrambling control field in the packet header. Note
    // this is only enough to determine that the packet is scrambled, not that
    // it is not scrambled.
    bool tsPacketScrambled = header.TScrambling != 0;
    if (tsPacketScrambled)
    {
      if (state->State == Encrypted)
      {
        state->PacketCount = 0;
        return true;
      }

      state->PacketCount++;
      if (state->State == EncryptionStateNotSet || state->PacketCount >= STATE_CHANGE_THRESHOLD)
      {
        if (state->State == EncryptionStateNotSet)
        {
          LogDebug(L"encryption: PID %hu state determined %d (TS header)",
                    state->Pid, tsPacketScrambled);
        }
        else
        {
          LogDebug(L"encryption: PID %hu state change %d -> %d (TS header)",
                    state->Pid, state->State, tsPacketScrambled);
        }

        if (m_callBack != NULL)
        {
          m_callBack->OnEncryptionStateChange(state->Pid, state->State, Encrypted);
        }
        state->State = Encrypted;
        state->PacketCount = 0;
      }
      return true;
    }

    // Does this TS packet contain the start of a PES packet?
    if (!header.PayloadUnitStart)
    {
      // No. Assume status quo.
      return state->State != Clear;
    }

    // Does the packet include the PES scrambling control field? Note according
    // to ETR 289 the header of a scrambled PES packet must not span multiple
    // TS packets.
    bool pesPacketScrambled = false;
    if (!header.HasAdaptionField || header.PayLoadStart <= 181)
    {
      // Yes. Does the PES packet header look okay?
      unsigned char offset = header.PayLoadStart;
      if (tsPacket[offset] != 0 || tsPacket[offset + 1] != 0 || tsPacket[offset + 2] != 1)
      {
        // No. It is odd for the TS packet to be marked as not scrambled, but
        // for the PES packet to appear scrambled. Perhaps there's some kind of
        // decryption error.
        pesPacketScrambled = true;
      }
      else
      {
        // Yes. Does the stream have scrambling control bits?
        unsigned char streamId = tsPacket[offset + 3];
        if (
          streamId != STREAM_ID_PROGRAM_STREAM_MAP &&
          streamId != STREAM_ID_PADDING &&
          streamId != STREAM_ID_PRIVATE_2 &&
          streamId != STREAM_ID_ECM &&
          streamId != STREAM_ID_EMM &&
          streamId != STREAM_ID_DSMCC &&
          streamId != STREAM_ID_H222_TYPE_E &&
          streamId != STREAM_ID_PROGRAM_STREAM_DIRECTORY
        )
        {
          // Yes. The packet might be scrambled.
          unsigned char pesScramblingControl = (tsPacket[offset + 6] & 0x30) >> 4;
          pesPacketScrambled = pesScramblingControl != 0;
        }
      }
    }

    if (
      (pesPacketScrambled && state->State != Encrypted) ||
      (!pesPacketScrambled && state->State != Clear)
    )
    {
      state->PacketCount++;
      if (state->State == EncryptionStateNotSet || state->PacketCount >= STATE_CHANGE_THRESHOLD)
      {
        if (state->State == EncryptionStateNotSet)
        {
          LogDebug(L"encryption: PID %hu state determined %d (PES header)",
                    state->Pid, pesPacketScrambled);
        }
        else
        {
          LogDebug(L"encryption: PID %hu state change %d -> %d (PES header)",
                    state->Pid, state->State, pesPacketScrambled);
        }

        EncryptionState newState = pesPacketScrambled ? Encrypted : Clear;
        if (m_callBack != NULL)
        {
          m_callBack->OnEncryptionStateChange(state->Pid, state->State, newState);
        }
        state->State = newState;
        state->PacketCount = 0;
      }
    }
    else
    {
      state->PacketCount = 0;
    }
    return pesPacketScrambled;
  }
  catch (...)
  {
    LogDebug(L"encryption: unhandled exception in OnTsPacket()");
  }
  return false;
}

EncryptionState CEncryptionAnalyser::GetPidState(unsigned short pid)
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, PidState*>::const_iterator it = m_pids.find(pid);
  if (it == m_pids.end())
  {
    return EncryptionStateNotSet;
  }
  return it->second->State;
}

void CEncryptionAnalyser::CleanUp()
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, PidState*>::iterator it = m_pids.begin();
  for ( ; it != m_pids.end(); it++)
  {
    if (it->second != NULL)
    {
      delete it->second;
      it->second = NULL;
    }
  }
  m_pids.clear();
}