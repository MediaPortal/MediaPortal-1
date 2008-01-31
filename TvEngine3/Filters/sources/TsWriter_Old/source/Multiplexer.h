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
	void AddPesStream(int pid, bool isAc3,bool isAudio, bool isVideo);
	void RemovePesStream(int pid);
	void OnTsPacket(byte* tsPacket);
	void Reset();
	void ClearStreams();
	void SetFileWriterCallBack(IFileWriter* callback);
	int OnNewPesPacket(CPesDecoder* decoder);

private:
	int  WritePackHeader(byte* buf, CPcr& pcr);
  int  WriteSystemHeader(byte* buf);
  int  WritePaddingHeader(byte* buf, int full_padding_size);
  int  WritePaddingPacket(byte* buf,int packet_bytes);
  int  get_system_header_size();
  int  get_packet_payload_size(CPesPacket& packet);
  int  mpeg_mux_write_packet(CPesPacket& packet, int streamId);
  void flush_packet(CPesPacket& packet, int streamId);
	void PatchPtsDts(byte* pesPacket,CPcr& startPcr);
	vector<CPesDecoder*> m_pesDecoders;
	typedef vector<CPesDecoder*>::iterator ivecPesDecoders;
  
  vector<CPesPacket*> m_packets;
	typedef vector<CPesPacket*>::iterator ivecPackets;

	IFileWriter* m_pCallback;
	CAdaptionField m_adaptionField;
	CTsHeader m_header;
  int m_system_header_size;
	int m_pcrPid;
	CPcr m_pcr;
	CPcr m_pcrStart;
	bool m_bVideoStartFound;
	bool m_bFirstPacket;
};
