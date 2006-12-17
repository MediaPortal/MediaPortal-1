/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#include "pcrdecoder.h"
#include "pesdecoder.h"
#include "tsheader.h"
#include "adaptionfield.h"
#include "pcr.h"
//#include "patparser.h"
#include <vector>
#include <map>

using namespace std;

class IFileWriter
{
public:
	virtual void Write(byte* buffer, int len)=0;
};

class CMultiplexer : public CPesCallback
{
public:
	CMultiplexer(void);
	virtual ~CMultiplexer(void);
	void SetPcrPid(int pcrPid);
	int  GetPcrPid();
	void AddPesStream(int pid, bool isAudio, bool isVideo);
	void RemovePesStream(int pid);
	void OnTsPacket(byte* tsPacket);
	void Reset();
	void ClearStreams();
	void SetFileWriterCallBack(IFileWriter* callback);
	int OnNewPesPacket(int streamid,byte* header, int headerlen,byte* data, int len, bool isStart);

private:
  int  SplitPesPacket(int streamId,byte* header, int headerlen,byte* pesPacket, int nLen,bool isStart);
	int  WritePackHeader();
  
	vector<CPesDecoder*> m_pesDecoders;
	typedef vector<CPesDecoder*>::iterator ivecPesDecoders;
	//CPcrDecoder m_pcrDecoder;
	IFileWriter* m_pCallback;
	CAdaptionField m_adaptionField;
	CTsHeader m_header;
	int m_videoPacketCounter;
	int m_audioPacketCounter;
	int m_pcrPid;
	CPcr m_pcr;
	bool  m_streams[255];
  byte* m_pesBuffer;
};
