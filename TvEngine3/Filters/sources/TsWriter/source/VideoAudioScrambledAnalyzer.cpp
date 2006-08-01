/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#include "TsHeader.h"
#include "packetsync.h"

extern void LogDebug(const char *fmt, ...) ;

CVideoAudioScrambledAnalyzer::CVideoAudioScrambledAnalyzer()
{
	Reset();
}

CVideoAudioScrambledAnalyzer::~CVideoAudioScrambledAnalyzer()
{
}

void CVideoAudioScrambledAnalyzer::SetVideoPid(int pid)
{
	m_videoPid=pid;
}

int  CVideoAudioScrambledAnalyzer::GetVideoPid()
{
	return m_videoPid;
}

void CVideoAudioScrambledAnalyzer::SetAudioPid(int pid)
{
	m_audioPid=pid;
}

int  CVideoAudioScrambledAnalyzer::GetAudioPid()
{
	return m_audioPid;
}

bool CVideoAudioScrambledAnalyzer::IsAudioScrambled()
{
	return m_bAudioEncrypted;
}

bool CVideoAudioScrambledAnalyzer::IsVideoScrambled()
{
	return m_bVideoEncrypted;
}


void CVideoAudioScrambledAnalyzer::Reset()
{
		m_bInitAudio=TRUE;
		m_bInitVideo=TRUE;
		m_bVideoEncrypted=TRUE;
		m_bAudioEncrypted=TRUE;
		m_audioTimer=GetTickCount();
		m_videoTimer=GetTickCount();
}

void CVideoAudioScrambledAnalyzer::OnTsPacket(byte* tsPacket)
{
	CTsHeader  header(tsPacket);
	if (header.SyncByte != TS_PACKET_SYNC) return;
	if (header.TransportError==true) return;
	if (header.ContinuityCounter!=0)  return;
	BOOL scrambled= (header.TScrambling!=0);
	if (header.Pid==m_audioPid) 
	{
		//LogDebug("audio:%x",header.TScrambling);
		if (TRUE==scrambled)
		{
			m_audioTimer=GetTickCount();
		}

		if (scrambled != m_bAudioEncrypted || m_bInitAudio)
		{
			m_bInitAudio=FALSE;
			if (FALSE == scrambled)
			{
				DWORD timeSpan=GetTickCount()-m_audioTimer;
				if (timeSpan > 150)
				{
					LogDebug("audio pid %x unscrambled", m_audioPid);
					m_bAudioEncrypted=scrambled;
					//LogHeader(header);
				}
			}
			else
			{
					LogDebug("audio pid %x scrambled", m_audioPid);
				//LogHeader(header);
				m_bAudioEncrypted=scrambled;
			}
		}
	}

	if (header.Pid==m_videoPid) 
	{
		if (TRUE==scrambled)
		{
			m_videoTimer=GetTickCount();
		}

		if (scrambled != m_bVideoEncrypted || m_bInitVideo)
		{
			m_bInitVideo=FALSE;
			if (FALSE == scrambled)
			{
				DWORD timeSpan=GetTickCount()-m_videoTimer;
				if (timeSpan > 150)
				{
					LogDebug("video pid %x unscrambled", m_videoPid);
					//LogHeader(header);
					m_bVideoEncrypted=scrambled;
				}
			}
			else
			{
				LogDebug("video pid %x scrambled", m_videoPid);
				//LogHeader(header);
				m_bVideoEncrypted=scrambled;
			}
		}
	}
}