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
#pragma once
#include "buffer.h"
#include "MultiFileReader.h"
#include "pcrdecoder.h"
#include <vector>
using namespace std;

class CDeMultiplexer
{
public:
	CDeMultiplexer(MultiFileReader& reader);
	virtual ~CDeMultiplexer(void);

	CBuffer* GetAudio();
	CBuffer* GetVideo();
	int VideoPacketCount();
	int AudioPacketCount();
	void Require();
	bool Parse();
	void Reset();
private:
  CCritSec m_section;
	vector<CBuffer*> m_vecAudioBuffers;
	vector<CBuffer*> m_vecVideoBuffers;
	typedef vector<CBuffer*>::iterator ivecBuffer;
	MultiFileReader& m_reader;
	byte		 Next(int len);
	void		 Advance(int len);
	int      BufferLength();
	void		 Copy(int len, byte* destination);
	double	 m_pcrTime;
	double	 m_ptsTime;
	double	 m_dtsTime;
	byte*		 m_pBuffer;
	int			 m_iBufferPosWrite;
	int			 m_iBufferPosRead;
	int			 m_iBytesInBuffer;
	int      m_streamId;
	CPcrDecoder m_pcrDecoder;
  CBuffer* m_pVideoBuffer;
  CBuffer* m_pAudioBuffer;
};
