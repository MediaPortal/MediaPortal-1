/**
*  MediaFormats.h
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

#ifndef MEDIAFORMATS_H
#define MEDIAFORMATS_H

static BYTE AC3AudioFormat [] = {
	0x00, 0x20,				//wFormatTag
	0x06, 0x00,				//nChannels
	0x80, 0xBB, 0x00, 0x00, //nSamplesPerSec
	0xC0, 0x5D, 0x00, 0x00, //nAvgBytesPerSec
	0x00, 0x03,				//nBlockAlign
	0x00, 0x00,				//wBitsPerSample
	0x00, 0x00				//cbSize
};

/*
static GUID MEDIASUBTYPE_ARCSOFT_MLP =    {0x4288b843, 0x610b, 0x4e15 ,0xa5, 0x3b, 0x43, 0x00, 0x7f, 0xcf, 0xf6, 0x14};
static GUID MEDIASUBTYPE_NERO_MLP =       {0x1e889be7, 0xb276, 0x4064 ,0x9a, 0x39, 0x16, 0x0a, 0x06, 0x89, 0x5b, 0x52};
static GUID MEDIASUBTYPE_SONIC_MLP =      {0x4094a857, 0x7891, 0x44ac, 0x92, 0xb5, 0xc1, 0xcf, 0xf3, 0x7a, 0xf2, 0xe7};
static GUID MEDIASUBTYPE_ARCSOFT_DTSHD =  {0xf6498f57, 0xb399, 0x4a43, 0xa6, 0xfa, 0xf6, 0x94, 0xad, 0x42, 0xb9, 0xbe};
*/

static GUID  WAVE_FORMAT_MLP = FOURCCMap(MAKEFOURCC('M','L','P',' ')) ;

struct WAVEFORMATEX_HDMV_LPCM : public WAVEFORMATEX
{
  BYTE channel_conf;

	struct WAVEFORMATEX_HDMV_LPCM()
	{
		memset(this, 0, sizeof(*this)); 
		cbSize = sizeof(WAVEFORMATEX_HDMV_LPCM) - sizeof(WAVEFORMATEX);
	}
};

struct WAVEFORMATEXPS2 : public WAVEFORMATEX
{
    DWORD dwInterleave;

	struct WAVEFORMATEXPS2()
	{
		memset(this, 0, sizeof(*this)); 
		cbSize = sizeof(WAVEFORMATEXPS2) - sizeof(WAVEFORMATEX);
	}
};

#pragma pack(push, 1)
typedef struct {
	DWORD dwOffset;	
	CHAR IsoLang[4]; // three letter lang code + terminating zero
	WCHAR TrackName[256]; // 256 chars ought to be enough for everyone :)
} SUBTITLEINFO;
#pragma pack(pop)

#define WAVE_FORMAT_MP3 0x0055
#define WAVE_FORMAT_AAC 0x00FF
#define WAVE_FORMAT_DOLBY_AC3 0x2000
#define WAVE_FORMAT_DVD_DTS 0x2001
#define WAVE_FORMAT_PS2_PCM 0xF521
#define WAVE_FORMAT_PS2_ADPCM 0xF522

static GUID MEDIASUBTYPE_AAC = {0x00000ff, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71};
static GUID MEDIASUBTYPE_HDMV_LPCM_AUDIO = {0x949f97fd, 0x56f6, 0x4527, 0xb4, 0xae, 0xdd, 0xeb, 0x37, 0x5a, 0xb8, 0xf};
static GUID MEDIASUBTYPE_HDMVSUB = {0x4eba53e, 0x9330, 0x436c, 0x91, 0x33, 0x55, 0x3e, 0xc8, 0x70, 0x31, 0xdc};
static GUID MEDIASUBTYPE_SVCD_SUBPICTURE = {0xda5b82ee, 0x6bd2, 0x426f, 0xbf, 0x1e, 0x30, 0x11, 0x2d, 0xa7, 0x8a, 0xe1};
static GUID MEDIASUBTYPE_CVD_SUBPICTURE = {0x7b57308f, 0x5154, 0x4c36, 0xb9, 0x3, 0x52, 0xfe, 0x76, 0xe1, 0x84, 0xfc};
static GUID MEDIATYPE_Subtitle = {0xe487eb08, 0x6b26, 0x4be9, 0x9d, 0xd3, 0x99, 0x34, 0x34, 0xd3, 0x13, 0xfd};
static GUID MEDIASUBTYPE_PS2_SUB = {0x4f3d3d21, 0x6d7c, 0x4f73, 0xaa, 0x5, 0xe3, 0x97, 0xb5, 0xea, 0xe0, 0xaa};
static GUID H264_SubType = {0x8D2D71CB, 0x243F, 0x45E3, {0xB2, 0xD8, 0x5F, 0xD7, 0x96, 0x7E, 0xC0, 0x9B}};
static GUID MPG4_SubType = FOURCCMap(MAKEFOURCC('A','V','C','1'));
static GUID VC1_SubType = FOURCCMap('1CVW');
static GUID AVC1_SubType = {0x31435641, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71};
static GUID avc1_SubType = {0x31637661, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71};
static GUID WVC1_SubType = {0x31435657, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71};
static GUID MEDIASUBTYPE_BD_LPCM_AUDIO = {0xa23eb7fc, 0x510b, 0x466f, 0x9f, 0xbf, 0x5f, 0x87, 0x8f, 0x69, 0x34, 0x7c};

#endif
