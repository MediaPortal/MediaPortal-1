/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#include <windows.h>
#include "VideoAudioScrambledAnalyzer.h"
#include "..\..\shared\packetsync.h"

extern void LogDebug(const char *fmt, ...) ;

CVideoAudioScrambledAnalyzer::CVideoAudioScrambledAnalyzer()
{
  m_videoPid=0;
  m_audioPid=0;
	Reset();
}

CVideoAudioScrambledAnalyzer::~CVideoAudioScrambledAnalyzer()
{
}

void CVideoAudioScrambledAnalyzer::SetVideoPid(int pid)
{
	LogDebug("analyzer: set video pid:%x", pid);
	m_videoPid=pid;
}

int  CVideoAudioScrambledAnalyzer::GetVideoPid()
{
	return m_videoPid;
}

void CVideoAudioScrambledAnalyzer::SetAudioPid(int pid)
{
	LogDebug("analyzer: set audio pid:%x", pid);
	m_audioPid=pid;
}

int  CVideoAudioScrambledAnalyzer::GetAudioPid()
{
	return m_audioPid;
}

bool CVideoAudioScrambledAnalyzer::IsAudioScrambled()
{
	return (m_bAudioEncrypted==Scrambled);
}

bool CVideoAudioScrambledAnalyzer::IsVideoScrambled()
{
	return (m_bVideoEncrypted==Scrambled);
}


void CVideoAudioScrambledAnalyzer::Reset()
{
	LogDebug("analyzer: reset");
	m_bVideoEncrypted=Unknown;
	m_bAudioEncrypted=Unknown;
}

void CVideoAudioScrambledAnalyzer::OnTsPacket(byte* tsPacket)
{
  int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
  if (pid!=m_videoPid && pid!=m_audioPid) return;

	m_tsheader.Decode(tsPacket);
	if (m_tsheader.SyncByte != TS_PACKET_SYNC) return;
	if (m_tsheader.TransportError==true) return;
	BOOL scrambled= (m_tsheader.TScrambling!=0);
	if (m_tsheader.Pid==m_audioPid && m_audioPid > 0x10) 
	{
    enum ScrambleState newState;
    if (scrambled)
      newState=Scrambled;
    else
      newState=UnScrambled;
    if (newState!=m_bAudioEncrypted)
    {
      m_bAudioEncrypted=newState;
		  Dump(true,false);
    }
	}

	if (m_tsheader.Pid==m_videoPid && m_videoPid > 0x10) 
	{
    enum ScrambleState newState;
    if (scrambled)
      newState=Scrambled;
    else
      newState=UnScrambled;
    if (newState!=m_bVideoEncrypted)
    {
      m_bVideoEncrypted=newState;
		  Dump(false,true);
    }
	}
}

void CVideoAudioScrambledAnalyzer::Dump(bool audio, bool video)
{
  return;
  if (audio)
  {
    switch (m_bAudioEncrypted)
    {
      case Unknown: 
        LogDebug("analyzer: audio unknown");
      break;
      case Scrambled: 
        LogDebug("analyzer: audio Scrambled");
      break;
      case UnScrambled: 
        LogDebug("analyzer: audio UnScrambled");
      break;
    }
  }
  if (video)
  {
    switch (m_bVideoEncrypted)
    {
      case Unknown: 
        LogDebug("analyzer: video unknown");
      break;
      case Scrambled: 
        LogDebug("analyzer: video Scrambled");
      break;
      case UnScrambled: 
        LogDebug("analyzer: video UnScrambled");
      break;
    }
  }
}
