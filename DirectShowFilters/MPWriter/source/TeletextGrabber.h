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

// {14639355-4BA4-471c-BA91-8B4AF51F3A0D}
DEFINE_GUID(IID_IAnalogTeletextCallBack, 0x14639355, 0x4ba4, 0x471c, 0xba, 0x91, 0x8b, 0x4a, 0xf5, 0x1f, 0x3a, 0xd);

DECLARE_INTERFACE_(IAnalogTeletextCallBack, IUnknown)
{
	STDMETHOD(OnTeletextReceived)(THIS_ BYTE* data, int packetCounts)PURE;
};

const int TS_PACKET_LENGTH = 188;
const int COLLECT_TS_PACKETS = 25;
const int TSHEADER_BYTE1_OFFSET = 0;
const int TSHEADER_BYTE1 = 0x47;
const int TSHEADER_BYTE2_OFFSET = 1;
const int TSHEADER_BYTE2 = 0x70;
const int TSHEADER_BYTE3_OFFSET = 2;
const int TSHEADER_BYTE3 = 0x00;
const int TSHEADER_BYTE4_OFFSET = 3;
const int TSHEADER_BYTE4 = 0x00;
const int NUMBER_OF_TXT_LINES_IN_TS = 4;
const int DATAPACKET_LENGTH = 46;
const int DATA_UNIT_OFFSET = 4;
const int DATA_UNIT = 0x02;
const int DATA_UNIT_LENGTH_OFFSET = 5;
const int DATA_UNIT_LENGTH = 0x2C;
const int DATAFIELD_HEADER_OFFSET = 6;
const int DATAFIELD_HEADER = 0x00;
const int FRAMING_CODE_OFFSET = 7;
const int FRAMING_CODE = 0xE4;
const int VBI_LINE_LENGTH = 43;
const int TXT_LINE_OFFSET = 8;
const int TXT_LINE_LENGTH = 42;


class CTeletextGrabber
{
public:
	CTeletextGrabber();
	~CTeletextGrabber(void);

	void Start( );
	void Stop( );
	void SetCallBack( IAnalogTeletextCallBack* callback);

	void OnSampleReceived(byte* sampleData, int sampleLen);
private:
	IAnalogTeletextCallBack*	m_pCallback;
	bool						m_bRunning;
	byte*						m_pInvData;
	byte*						m_pBuffer;
	byte*						m_pBufferTemp;
	int							m_iBufferPos;
	byte*						m_pCurrentTsPacket;
	int							m_iCurrentTsPacketCounter;
	byte*						m_pTsResultBuffer;
	int							m_iTsPacketCount;
};

