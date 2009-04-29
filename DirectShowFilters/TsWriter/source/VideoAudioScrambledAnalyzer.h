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
#pragma once

#include "..\..\shared\TsHeader.h"
class CVideoAudioScrambledAnalyzer
{
public:
  enum ScrambleState
  {
    Unknown,
    Scrambled,
    UnScrambled
  };
	CVideoAudioScrambledAnalyzer(void);
	~CVideoAudioScrambledAnalyzer(void);

	void SetVideoPid(int pid);
	int  GetVideoPid();
	
	void SetAudioPid(int pid);
	int  GetAudioPid();

	bool IsAudioScrambled();
	bool IsVideoScrambled();

	void Reset();
	void OnTsPacket(byte* tsPacket);

private:
  void Dump(bool audio, bool video);
	int m_videoPid;
	int m_audioPid;
	enum ScrambleState m_bAudioEncrypted;
	enum ScrambleState m_bVideoEncrypted;
  CTsHeader  m_tsheader;
};
