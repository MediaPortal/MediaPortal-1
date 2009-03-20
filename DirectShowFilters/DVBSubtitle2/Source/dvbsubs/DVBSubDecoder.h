/*
   dvbsubs - a program for decoding DVB subtitles (ETS 300 743)

   File: dvbsubs.h

   Copyright (C) Dave Chapman 2002
  
   This program is free software; you can redistribute it and/or
   modify it under the terms of the GNU General Public License
   as published by the Free Software Foundation; either version 2
   of the License, or (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program; if not, write to the Free Software
   Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
   Or, point your browser to http://www.gnu.org/copyleft/gpl.html
*/

#pragma once
#define MAX_REGIONS 32
  
#include "windows.h"
#include <vector>
#include "subtitle.h"
#include "..\SubdecoderObserver.h"

typedef unsigned __int64 uint64_t;
typedef unsigned __int16 uint16_t;
typedef unsigned __int8 uint8_t;

typedef struct 
{
  int x,y;
  unsigned char is_visible;
} visible_region_t;


typedef struct 
{
  int acquired;
  int page_time_out;
  int page_version_number;
  int page_state;
  visible_region_t regions[MAX_REGIONS];
} page_t;

typedef struct 
{
  int width,height;
  int depth;
  int win;
  int CLUT_id;
  int objects_start,objects_end;
  unsigned int object_pos[65536];
  unsigned char img[720*576];
} region_t;

class CSubtitle;

class CDVBSubDecoder
{
public:

  CDVBSubDecoder();
  ~CDVBSubDecoder();

  int ProcessPES( const unsigned char* data, int length, int pid );
  BITMAP* GetSubtitle();
  long GetSubtitleId();

  CSubtitle* GetSubtitle( unsigned int place );
  CSubtitle* GetLatestSubtitle();
  int GetSubtitleCount();
  void ReleaseOldestSubtitle();

  void Reset();
  void SetObserver( MSubdecoderObserver* pObserver );
  void RemoveObserver( MSubdecoderObserver* pObserver );

private:

  void Init_data(); 
  void Create_region( int region_id, int region_width, int region_height, int region_depth );
  void Do_plot( int r,int x, int y, unsigned char pixel );
  void Plot( int r, int run_length, unsigned char pixel );
  unsigned char Next_nibble();
  void Set_clut( int CLUT_id,int CLUT_entry_id,int Y, int Cr, int Cb, int T_value );
  void Decode_4bit_pixel_code_string( int r, int object_id, int ofs, int n );
  void Process_pixel_data_sub_block( int r, int o, int ofs, int n );
  void Process_page_composition_segment();
  void Process_region_composition_segment();
  void Process_CLUT_definition_segment();
  void Process_object_data_segment();
  void Process_display_definition_segment();
  void Compose_subtitle();
  char* Pts2hmsu( uint64_t pts, char sep );
  uint64_t Get_pes_pts ( unsigned char* buf );

  // Member data
  unsigned char m_Buffer[1920*1080]; // should be dynamically allocated

  int m_ScreenWidth;
  int m_ScreenHeight;

  CSubtitle*	m_CurrentSubtitle;
  MSubdecoderObserver* m_pObserver;
  std::vector<CSubtitle*> m_RenderedSubtitles;
};

