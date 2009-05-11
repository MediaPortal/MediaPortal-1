/**
*  DvbFormats.h
*  Copyright (C) 2004-2006 bear
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bear can be reached on the forums at
*    http://forums.dvbowners.com/
*/
#ifndef DVBFORMATS_H
#define DVBFORMATS_H

static BYTE	PatPacket [] = {
	0x47,								//Sync Byte 47
	0x40,0x00,							// 0x40 & table_id	0							
	0x10,								//section_syntax_indicator	0x10		
	0x00,								// must be 0
//CRC begin here	
	0x00,								//reserved
//6
	0xB0, 0x11,							// 0xB0 & section_length 17bytes from next inc crc's
//8
	0x04, 0xD2, 						//transport_stream_id = 1234
	
	0xC5, // or 0xEB, 					//reserved	
	0x00,								//version_number		
	0x00,								//current_next_indicator 0
	0x00, 0x00,							//section_number	
	0xE0, 0x10,							//must be 0xE0 & EIT Pid 
//17	
	0x04, 0x01,							//program_number 1025
//19
	0xE1, 0x01,							//reserved must be 0xE0 & sid 257
//21
	0xFF, 0xFF, 0xFF, 0xFF              // CRC
//25
} ;

static BYTE	PcrPacket [] = {
	0x47,								//Sync Byte 47
	0x00, 0x82,							// 0x00 & table_id	130							
	0x20,
	0xB7, 0x10,
//6
	0x43, 0xCA,	0x61, 0x0A, 0x7E,		// PCR & reserved
	0x18								// PCR Extension
//12
} ;

static BYTE	PmtPacket [] = {
	0x47,								//Sync Byte 47
	0x41, 0x01,							// 0x00 & table_id	257							
	0x10,								//section_syntax_indicator	
	0x00,								// must be 0
	0x02,								//reserved	2
//6
	0xB0, 0x27, 						// 0xB0 & section_length 27bytes from inc here inc crc's
	0x04, 0x01,							//program_number 1025	
	0xCD,//C9//C3//DF								//reserved	bit 0 = 4 zeros next instead of 2
	0x00,								//version_number
	0x00,								//current_next_indicator
//13	
	0xE0, 0x82,							// 0xE0 & PCR_PID	
	0xF0, 0x00,							//program descriptor length 0 bytes	
//17
	0x02,								//Video stream
	0xE2, 0x01,							//pid 513 & E0
	0xF0, 0x00,					//descriptor length 0 bytes
//22
	0x04,								//Audio
	0xE2, 0x94,							//pid 660
	0xF0, 0x00,					//descriptor length 0 bytes
//27	
	0x06,								//Private data
	0xE2, 0x95,							//pid 661
	0xF0, 0x00,							//descriptor length 0 bytes
//32	
	0x06,								//Private data teletext
	0xE2, 0x41,							//pid 577
	0xF0, 0x00,							//descriptor length 0 bytes
//37
	0x04,								//Audio 2
	0xE2, 0x94,							//pid 660
	0xF0, 0x00,							//descriptor length 0 bytes
//42
	0x06,								//Private data 2
	0xE2, 0x95,							//pid 661
	0xF0, 0x00,							//descriptor length 0 bytes
//47
	0xFF, 0xFF, 0xFF, 0xFF				// CRC
//51
} ;

static const DWORD CRCTable[256] = {
	0x00000000, 0x04C11DB7, 0x09823B6E, 0x0D4326D9,
	0x130476DC, 0x17C56B6B,	0x1A864DB2, 0x1E475005,
	0x2608EDB8, 0x22C9F00F, 0x2F8AD6D6, 0x2B4BCB61,
	0x350C9B64, 0x31CD86D3, 0x3C8EA00A, 0x384FBDBD,
	0x4C11DB70, 0x48D0C6C7,	0x4593E01E, 0x4152FDA9,
	0x5F15ADAC, 0x5BD4B01B, 0x569796C2, 0x52568B75,
	0x6A1936C8, 0x6ED82B7F, 0x639B0DA6, 0x675A1011,
	0x791D4014, 0x7DDC5DA3,	0x709F7B7A, 0x745E66CD,
	0x9823B6E0, 0x9CE2AB57, 0x91A18D8E, 0x95609039,
	0x8B27C03C, 0x8FE6DD8B, 0x82A5FB52, 0x8664E6E5,
	0xBE2B5B58, 0xBAEA46EF,	0xB7A96036, 0xB3687D81,
	0xAD2F2D84, 0xA9EE3033, 0xA4AD16EA, 0xA06C0B5D,
	0xD4326D90, 0xD0F37027, 0xDDB056FE, 0xD9714B49,
	0xC7361B4C, 0xC3F706FB,	0xCEB42022, 0xCA753D95,
	0xF23A8028, 0xF6FB9D9F, 0xFBB8BB46, 0xFF79A6F1,
	0xE13EF6F4, 0xE5FFEB43, 0xE8BCCD9A, 0xEC7DD02D,
	0x34867077, 0x30476DC0,	0x3D044B19, 0x39C556AE,
	0x278206AB, 0x23431B1C, 0x2E003DC5, 0x2AC12072,
	0x128E9DCF, 0x164F8078, 0x1B0CA6A1, 0x1FCDBB16,
	0x018AEB13, 0x054BF6A4,	0x0808D07D, 0x0CC9CDCA,
	0x7897AB07, 0x7C56B6B0, 0x71159069, 0x75D48DDE,
	0x6B93DDDB, 0x6F52C06C, 0x6211E6B5, 0x66D0FB02,
	0x5E9F46BF, 0x5A5E5B08,	0x571D7DD1, 0x53DC6066,
	0x4D9B3063, 0x495A2DD4, 0x44190B0D, 0x40D816BA,
	0xACA5C697, 0xA864DB20, 0xA527FDF9, 0xA1E6E04E,
	0xBFA1B04B, 0xBB60ADFC,	0xB6238B25, 0xB2E29692,
	0x8AAD2B2F, 0x8E6C3698, 0x832F1041, 0x87EE0DF6,
	0x99A95DF3, 0x9D684044, 0x902B669D, 0x94EA7B2A,
	0xE0B41DE7, 0xE4750050,	0xE9362689, 0xEDF73B3E,
	0xF3B06B3B, 0xF771768C, 0xFA325055, 0xFEF34DE2,
	0xC6BCF05F, 0xC27DEDE8, 0xCF3ECB31, 0xCBFFD686,
	0xD5B88683, 0xD1799B34,	0xDC3ABDED, 0xD8FBA05A,
	0x690CE0EE, 0x6DCDFD59, 0x608EDB80, 0x644FC637,
	0x7A089632, 0x7EC98B85, 0x738AAD5C, 0x774BB0EB,
	0x4F040D56, 0x4BC510E1,	0x46863638, 0x42472B8F,
	0x5C007B8A, 0x58C1663D, 0x558240E4, 0x51435D53,
	0x251D3B9E, 0x21DC2629, 0x2C9F00F0, 0x285E1D47,
	0x36194D42, 0x32D850F5,	0x3F9B762C, 0x3B5A6B9B,
	0x0315D626, 0x07D4CB91, 0x0A97ED48, 0x0E56F0FF,
	0x1011A0FA, 0x14D0BD4D, 0x19939B94, 0x1D528623,
	0xF12F560E, 0xF5EE4BB9,	0xF8AD6D60, 0xFC6C70D7,
	0xE22B20D2, 0xE6EA3D65, 0xEBA91BBC, 0xEF68060B,
	0xD727BBB6, 0xD3E6A601, 0xDEA580D8, 0xDA649D6F,
	0xC423CD6A, 0xC0E2D0DD,	0xCDA1F604, 0xC960EBB3,
	0xBD3E8D7E, 0xB9FF90C9, 0xB4BCB610, 0xB07DABA7,
	0xAE3AFBA2, 0xAAFBE615, 0xA7B8C0CC, 0xA379DD7B,
	0x9B3660C6, 0x9FF77D71,	0x92B45BA8, 0x9675461F,
	0x8832161A, 0x8CF30BAD, 0x81B02D74, 0x857130C3,
	0x5D8A9099, 0x594B8D2E, 0x5408ABF7, 0x50C9B640,
	0x4E8EE645, 0x4A4FFBF2,	0x470CDD2B, 0x43CDC09C,
	0x7B827D21, 0x7F436096, 0x7200464F, 0x76C15BF8,
	0x68860BFD, 0x6C47164A, 0x61043093, 0x65C52D24,
	0x119B4BE9, 0x155A565E,	0x18197087, 0x1CD86D30,
	0x029F3D35, 0x065E2082, 0x0B1D065B, 0x0FDC1BEC,
	0x3793A651, 0x3352BBE6, 0x3E119D3F, 0x3AD08088,
	0x2497D08D, 0x2056CD3A,	0x2D15EBE3, 0x29D4F654,
	0xC5A92679, 0xC1683BCE, 0xCC2B1D17, 0xC8EA00A0,
	0xD6AD50A5, 0xD26C4D12, 0xDF2F6BCB, 0xDBEE767C,
	0xE3A1CBC1, 0xE760D676,	0xEA23F0AF, 0xEEE2ED18,
	0xF0A5BD1D, 0xF464A0AA, 0xF9278673, 0xFDE69BC4,
	0x89B8FD09, 0x8D79E0BE, 0x803AC667, 0x84FBDBD0,
	0x9ABC8BD5, 0x9E7D9662,	0x933EB0BB, 0x97FFAD0C,
	0xAFB010B1, 0xAB710D06, 0xA6322BDF, 0xA2F33668,
	0xBCB4666D, 0xB8757BDA,	0xB5365D03, 0xB1F740B4
};

class DVBFormat
{

public:

	static void LoadPATPacket(PBYTE pData, USHORT tsid, USHORT sid, USHORT pmt)
	{
		//Fill our packet with nulls
		memset(pData, 0xFF, 188);

		//Copy our blank pat accross
		memcpy(pData, &PatPacket, 25);

		//Set our tsid numb if we have one
		if (tsid) {
			pData[8] = (BYTE)(tsid >> 8);
			pData[9] = (BYTE)(tsid & 0xFF);
		}

		//Set our sid numb if we have one
		if (sid) {
			pData[17] = (BYTE)(sid >> 8);
			pData[18] = (BYTE)(sid & 0xFF);
		}

		//Set our pmt pid numb if we have one
		if (pmt) {
			pData[19] = (BYTE)(((pmt>>8)&0x1F) | 0xE0);
			pData[20] = (BYTE)(pmt & 0xFF);
		}

		//Get the length from the Table
		int len = ((pData[6] & 0xF) | (pData[7] & 0xFF)) + 3 + 5;

		//Calculate our crc value
		DWORD dwCRC32 = GetCRC32(pData + 5, len - 4 - 5);

		//Set our crc value
		pData[len - 4] = (BYTE)((dwCRC32 >> 24) & 0xff);
		pData[len - 3] = (BYTE)((dwCRC32 >> 16) & 0xff);
		pData[len - 2] = (BYTE)((dwCRC32 >> 8) & 0xff);
		pData[len - 1] = (BYTE)((dwCRC32) & 0xff);


	}

	static void LoadPCRPacket(PBYTE pData, USHORT pcr, REFERENCE_TIME pcrtime)
	{
		//Fill our packet with nulls
		memset(pData, 0xFF, 188);

		//Copy our blank pcr accross
		memcpy(pData, &PcrPacket, 12);

		//Set our pcr pid numb if we have one
		if (pcr) {
			pData[1] = (BYTE)((pcr >> 8) & 0x1F);
			pData[2] = (BYTE)(pcr & 0xFF);
		}

		//Set our pcr value if we have one
		if (pcrtime) {
			pData[6] = (BYTE)((pcrtime >> 25) & 0xFF);
			pData[7] = (BYTE)((pcrtime >> 17) & 0xFF);
			pData[8] = (BYTE)((pcrtime >> 9) & 0xFF);
			pData[9] = (BYTE)((pcrtime >> 1) & 0xFF);
			pData[10] = (BYTE)(((pcrtime << 7) & 0x80) | 0x7E);
		}
	}

	static void LoadPMTPacket(PBYTE pData,
							  USHORT pcr,
							  USHORT vid,
							  USHORT aud,
							  USHORT aud2,
							  USHORT ac3,
							  USHORT ac3_2,
							  USHORT txt)
	{
		//Fill our packet with nulls
		memset(pData, 0xFF, 188);

		//Copy our blank pat accross
		memcpy(pData, &PmtPacket, 51);

		//Set our pcr pid numb if we have one
		if (pcr) {
			pData[13] = (BYTE)(((pcr >> 8)&0x1F) | 0xE0);
			pData[14] = (BYTE)(pcr & 0xFF);
		}

		//Set our vid pid numb if we have one
		if (vid) {
			pData[18] = (BYTE)(((vid >> 8)&0x1F) | 0xE0);
			pData[19] = (BYTE)(vid & 0xFF);
		}

		//Set our audio pid numb if we have one
		if (aud) {
			pData[23] = (BYTE)(((aud >> 8)&0x1F) | 0xE0);
			pData[24] = (BYTE)(aud & 0xFF);
		}

		//Set our ac3 pid numb if we have one
		if (ac3) {
			pData[28] = (BYTE)(((ac3 >> 8)&0x1F) | 0xE0);
			pData[29] =(BYTE)( ac3 & 0xFF);
		}

		//Set our teletext pid numb if we have one
		if (txt) {
			pData[33] = (BYTE)(((txt >> 8)&0x1F) | 0xE0);
			pData[34] = (BYTE)(txt & 0xFF);
		}

		//Set our audio2 pid numb if we have one
		if (aud2) {
			pData[38] = (BYTE)(((aud >> 8)&0x1F) | 0xE0);
			pData[39] = (BYTE)(aud & 0xFF);
		}

		//Set our ac3 2 pid numb if we have one
		if (ac3_2) {
			pData[43] = (BYTE)(((ac3 >> 8)&0x1F) | 0xE0);
			pData[44] = (BYTE)(ac3 & 0xFF);
		}

		//Get the length from the Table
		int len = ((pData[6] & 0xF) | (pData[7] & 0xFF)) + 3 + 5;

		//Calculate our crc value
		DWORD dwCRC32 = GetCRC32(pData + 5, len - 4 - 5);

		//Set our crc value
		pData[len - 4] = (BYTE)((dwCRC32 >> 24) & 0xff);
		pData[len - 3] = (BYTE)((dwCRC32 >> 16) & 0xff);
		pData[len - 2] = (BYTE)((dwCRC32 >> 8) & 0xff);
		pData[len - 1] = (BYTE)((dwCRC32) & 0xff);
	}

	static DWORD GetCRC32(PBYTE pData, int length)
	{
		DWORD dwCRC = MAXDWORD; 
    
		for (int i=0; i < length; i++)
			dwCRC = (dwCRC << 8) ^ CRCTable[((dwCRC >> 24) ^ *pData++) & 0xFF];
    
		return dwCRC;
	}
};


#endif





