/*
   dvbsubs - a program for decoding DVB subtitles (ETS 300 743)

   File: dvbsubs.c

   Copyright (C) Dave Chapman 2002,2004
  
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

#pragma warning( disable: 4995 4996 )

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <time.h>
#include <ctype.h>
#include <errno.h>
#include "dvbsubs.h"
#include "subtitle.h"
#include "pes.h"

page_t page;
region_t regions[MAX_REGIONS];
uint8_t colours[256*3];
uint8_t trans[256];
uint16_t apid;
int y=0;
int x=0;
int ext;
int acquired=0;
uint64_t PTS;
uint64_t first_PTS;
unsigned int curr_obj;
unsigned int curr_reg[64];
uint8_t buf[720*576];  
int i=0;
int nibble_flag=0;
int in_scanline=0;
uint64_t video_pts,first_video_pts,audio_pts,first_audio_pts;
int audio_pts_wrap=0;

//struct timeval start_tv;

extern void Log(const char *fmt, ...);

CDVBSubDecoder::~CDVBSubDecoder()
{
	if( m_CurrentSubtitle )
		delete m_CurrentSubtitle;

	// Release all rendered subtitles
	for( unsigned int i = 0 ; i < m_RenderedSubtitles.size() ; i++ )
	{
		m_RenderedSubtitles.erase( m_RenderedSubtitles.begin() );
	}
}

CDVBSubDecoder::CDVBSubDecoder() :
	m_CurrentSubtitle( NULL ),	
	m_pObserver( NULL )	
{
}

void CDVBSubDecoder::Init_data() 
{
	int i;

	for( i = 0 ; i < MAX_REGIONS ; i++ ) 
	{
		page.regions[i].is_visible = 0;
		regions[i].win = -1;
	}
}

void CDVBSubDecoder::Create_region(int region_id,int region_width,int region_height,int region_depth) 
{
  regions[region_id].win=1;
  regions[region_id].width=region_width;
  regions[region_id].height=region_height;

  memset(regions[region_id].img,15,sizeof(regions[region_id].img));
}

void CDVBSubDecoder::Do_plot( int r,int x, int y, unsigned char pixel ) 
{
	int i;
	if ( ( y >= 0 ) && ( y < regions[r].height ) ) 
	{
		i = ( y * regions[r].width ) + x;
		regions[r].img[i] = pixel;
	} 
	else 
	{
		Log("DVBsubs: plot out of region: x=%d, y=%d - r=%d, height=%d",x,y,r,regions[r].height);
	}
}

void CDVBSubDecoder::Plot( int r, int run_length, unsigned char pixel ) 
{
	int x2 = x + run_length;

	//  Log("DVBsubs: plot: x=%d,y=%d,length=%d,pixel=%d", x, y, run_length,pixel );
	while ( x < x2 ) 
	{
		Do_plot( r, x , y , pixel );
		x++;
	}
}

unsigned char CDVBSubDecoder::Next_nibble() 
{
	unsigned char x;

	if ( nibble_flag == 0 ) 
	{
		x=( buf[i]&0xf0 ) >> 4;
		nibble_flag = 1;
	} 
	else 
	{
		x = ( buf[i++]&0x0f );
		nibble_flag = 0;
	}
	return( x );
}

/* function taken from "dvd2sub.c" in the svcdsubs packages in the
   vcdimager contribs directory.  Author unknown, but released under GPL2.
*/
void CDVBSubDecoder::Set_clut( int CLUT_id,int CLUT_entry_id,int Y_value, int Cr_value, int Cb_value, int T_value ) 
{
	// No need for YUV -> RGB conversion
	
/*	int Y,Cr,Cb,R,G,B;

	Y = Y_value;
	Cr = Cr_value;
	Cb = Cb_value;

	B = 1.164*(Y - 16)                    + 2.018*(Cb - 128);
	G = 1.164*(Y - 16) - 0.813*(Cr - 128) - 0.391*(Cb - 128);
	R = 1.164*(Y - 16) + 1.596*(Cr - 128);
	if (B<0) B=0; if (B>255) B=255;
	if (G<0) G=0; if (G>255) G=255;
	if (R<0) R=0; if (R>255) R=255; */

//	Log("DVBsubs: Setting colour for CLUT_id=%d, CLUT_entry_id=%d",CLUT_id,CLUT_entry_id);

	if ((CLUT_id > 15) || (CLUT_entry_id > 15)) 
	{
		Log("DVBsubs: ERROR: CLUT_id=%d, CLUT_entry_id=%d",CLUT_id,CLUT_entry_id);
		exit(1);
	}

	colours[(CLUT_id*48)+(CLUT_entry_id*3)+0] = Y_value;	//R;
	colours[(CLUT_id*48)+(CLUT_entry_id*3)+1] = Cr_value;	//G;
	colours[(CLUT_id*48)+(CLUT_entry_id*3)+2] = Cb_value;	//B;
	
	if( Y_value == 0 ) 
	{
		trans[(CLUT_id*16)+CLUT_entry_id] = 0;
	} 
	else 
	{
		trans[(CLUT_id*16)+CLUT_entry_id] = 255;
	}
}

void CDVBSubDecoder::Decode_4bit_pixel_code_string( int r, int object_id, int ofs, int n ) 
{
	int next_bits,
		switch_1,
		switch_2,
		switch_3,
		run_length,
		pixel_code;

	int bits;
	unsigned int data;
	int j;

	if ( in_scanline == 0 ) 
	{
		in_scanline=1;
	}
	nibble_flag = 0;
	j = i + n;
	while(i < j) 
	{
		//Log("DVBsubs: start of loop, i=%d, nibble-flag=%d", i, nibble_flag );
		//Log("DVBsubs: buf=%02x %02x %02x %02x", buf[i], buf[i+1], buf[i+2], buf[i+3] );

		bits = 0;
		pixel_code = 0;
		next_bits = Next_nibble();

		if( next_bits !=0 ) 
		{
			pixel_code = next_bits;
			Plot( r, 1, pixel_code );
			bits += 4;
		} 
		else 
		{
			bits+=4;
			data=Next_nibble();
			switch_1=(data&0x08)>>3;
			bits++;
			
			if ( switch_1 == 0 ) 
			{
				run_length=(data&0x07);
				bits+=3;
				if (run_length!=0) 
				{
					Plot(r,run_length+2,pixel_code);
				} 
				else 
				{
					//Log("DVBsubs: end_of_string - run_length=%d", run_length );
					break;
				}
			} 
			else 
			{
				switch_2 = (data&0x04) >> 2;
				bits++;
				if( switch_2 == 0 ) 
				{
					run_length=(data&0x03);
					bits+=2;
					pixel_code=Next_nibble();
					bits+=4;
					Plot(r,run_length+4,pixel_code);
				} 
				else 
				{
					switch_3 = (data&0x03);
					bits+=2;
					switch (switch_3) 
					{
						case 0: Plot(r,1,pixel_code);
							break;
						case 1: Plot(r,2,pixel_code);
							break;
						case 2: run_length=Next_nibble();
							bits+=4;
							pixel_code=Next_nibble();
							bits+=4;
							Plot(r,run_length+9,pixel_code);
							break;
						case 3: run_length=Next_nibble();
							run_length=(run_length<<4)|Next_nibble();
							bits+=8;
							pixel_code=Next_nibble();
							bits+=4;
							Plot(r,run_length+25,pixel_code);
					}
				}
			}
		}

	}
	
	if ( nibble_flag == 1 ) 
	{
		i++;
		nibble_flag = 0;
	}
}

void CDVBSubDecoder::Process_pixel_data_sub_block( int r, int o, int ofs, int n ) 
{
	int data_type;
	int j;
 
	j = i + n;

	x = ( regions[r].object_pos[o] ) >> 16;
	y = ( ( regions[r].object_pos[o] ) & 0xffff ) + ofs;
//	Log("DVBsubs: process_pixel_data_sub_block: r=%d, x=%d, y=%d, o=%d, ofs=%d, n=%d",r,x,y,o,ofs);
//	Log("DVBsubs: process_pixel_data: %02x %02x %02x %02x %02x %02x",buf[i],buf[i+1],buf[i+2],buf[i+3],buf[i+4],buf[i+5]);

	while ( i < j ) 
	{
		data_type = buf[i++];

		switch( data_type ) 
		{
			case 0: 
				i++;
			case 0x11: 
				Decode_4bit_pixel_code_string( r, o, ofs, n-1 );
				break;
			case 0xf0: 
				in_scanline = 0;
				x = ( regions[r].object_pos[o] ) >> 16;
				y += 2;
				break;
			default: 
				Log("DVBsubs: unimplemented data_type %02x in pixel_data_sub_block", data_type );
		}
  }
  i = j;
}
void CDVBSubDecoder::Process_page_composition_segment() 
{
	int page_id;
	int segment_length;
	int page_time_out;
	int page_version_number;
	int page_state;
	int region_id,region_x,region_y;
	int j;
	int r;

	page_id = (buf[i]<<8)|buf[i+1]; i += 2;
	segment_length = (buf[i]<<8)|buf[i+1]; i += 2;

	j = i + segment_length;

	page_time_out=buf[i++];
	page_version_number=(buf[i]&0xf0)>>4;
	page_state=(buf[i]&0x0c)>>2;
	i++;
	//Log("DVBsubs: "PAGE_COMPOSITION_SEGMENT: page_id=%04x, page_time_out=%d, page_version=%d,page_state=%d",page_id, page_time_out, page_version_number, page_state );
	//Log("DVBsubs: page_state=%d", page_state );
  
	if ((acquired==0) && (page_state!=2) && (page_state!=1)) 
	{
		//Log("DVBsubs: waiting for mode_change");
		return;
	} 
	else 
	{
		acquired=1;
	}

	for ( r=0 ; r < MAX_REGIONS ; r++ ) 
	{
		page.regions[r].is_visible = 0;
	}
	while ( i < j ) 
	{
		region_id=buf[i++];
		i++; // reserved
		region_x = (buf[i]<<8)|buf[i+1]; i += 2;
		region_y = (buf[i]<<8)|buf[i+1]; i += 2;

		page.regions[region_id].x = region_x;
		page.regions[region_id].y = region_y;
		page.regions[region_id].is_visible = 1;
	}  
}

void CDVBSubDecoder::Process_region_composition_segment() 
{
	int page_id,
		segment_length,
		region_id,
		region_version_number,
		region_fill_flag,
		region_width,
		region_height,
		region_level_of_compatibility,
		region_depth,
		CLUT_id,
		region_8_bit_pixel_code,
		region_4_bit_pixel_code,
		region_2_bit_pixel_code;
	int object_id,
		object_type,
		object_provider_flag,
		object_x,
		object_y,
		foreground_pixel_code,
		background_pixel_code;
	int j;
	int o;

	page_id = (buf[i]<<8)|buf[i+1]; i+=2;
	segment_length = (buf[i]<<8)|buf[i+1]; i+=2;

	j = i + segment_length;

	region_id=buf[i++];
	region_version_number=(buf[i]&0xf0)>>4;
	region_fill_flag=(buf[i]&0x08)>>3;
	i++;

	region_width=(buf[i]<<8)|buf[i+1]; i+=2;
	region_height=(buf[i]<<8)|buf[i+1]; i+=2;
	region_level_of_compatibility=(buf[i]&0xe0)>>5;
	region_depth=(buf[i]&0x1c)>>2;
	i++;

	CLUT_id=buf[i++];
	region_8_bit_pixel_code=buf[i++];
	region_4_bit_pixel_code=(buf[i]&0xf0)>>4;
	region_2_bit_pixel_code=(buf[i]&0x0c)>>2;
	i++;

	if ( regions[region_id].win < 0 ) 
	{
		// If the region doesn't exist, then open it.
		Create_region( region_id, region_width, region_height, region_depth );
		regions[region_id].CLUT_id = CLUT_id;
	}

	regions[region_id].width = region_width;
	regions[region_id].height = region_height;

	if (region_fill_flag==1) 
	{
		//Log("DVBsubs: "filling region %d with %d", region_id, region_4_bit_pixel_code );
		memset(regions[region_id].img,region_4_bit_pixel_code, sizeof( regions[region_id].img ) );
	}

	regions[region_id].objects_start = i;  
	regions[region_id].objects_end = j;  

	for ( o = 0 ;o < 65536 ; o++ ) 
	{
		regions[region_id].object_pos[o]=0xffffffff;
	}

	while ( i < j ) 
	{
		object_id = (buf[i]<<8)|buf[i+1]; i+=2;
		object_type = (buf[i]&0xc0)>>6;
		object_provider_flag = (buf[i]&0x30)>>4;
		object_x = ((buf[i]&0x0f)<<8)|buf[i+1]; i+=2;
		object_y = ((buf[i]&0x0f)<<8)|buf[i+1]; i+=2;

		regions[region_id].object_pos[object_id]=(object_x<<16)|object_y;
      
		if ((object_type==0x01) || (object_type==0x02)) 
		{
			foreground_pixel_code=buf[i++];
			background_pixel_code=buf[i++];
		}
	}
}

void CDVBSubDecoder::Process_CLUT_definition_segment() 
{
	int page_id,
		segment_length,
		CLUT_id,
		CLUT_version_number;

	int CLUT_entry_id,
		CLUT_flag_8_bit,
		CLUT_flag_4_bit,
		CLUT_flag_2_bit,
		full_range_flag,
		Y_value,
		Cr_value,
		Cb_value,
		T_value;

	int j;

	page_id = (buf[i]<<8)|buf[i+1]; i+=2;
	segment_length = (buf[i]<<8)|buf[i+1]; i+=2;
	j = i + segment_length;

	CLUT_id = buf[i++];
	CLUT_version_number = (buf[i]&0xf0)>>4;
	i++;

	while ( i < j ) 
	{
		CLUT_entry_id=buf[i++];

		CLUT_flag_2_bit=(buf[i]&0x80)>>7;
		CLUT_flag_4_bit=(buf[i]&0x40)>>6;
		CLUT_flag_8_bit=(buf[i]&0x20)>>5;
		full_range_flag=buf[i]&1;
		i++;
	
		if (full_range_flag==1) 
		{
			Y_value=buf[i++];
			Cr_value=buf[i++];
			Cb_value=buf[i++];
			T_value=buf[i++];
		} 
		else 
		{
			Y_value = (buf[i]&0xfc)>>2;
			Cr_value = (buf[i]&0x2<<2)|((buf[i+1]&0xc0)>>6);
			Cb_value = (buf[i+1]&0x2c)>>2;
			T_value = buf[i+1]&2;
			i += 2;
		}
	Set_clut(CLUT_id,CLUT_entry_id,Y_value,Cr_value,Cb_value,T_value);
  }
}

void CDVBSubDecoder::Process_object_data_segment() 
{
	int page_id,
		segment_length,
		object_id,
		object_version_number,
		object_coding_method,
		non_modifying_colour_flag;
      
	int top_field_data_block_length,
		bottom_field_data_block_length;
      
	int j;
	int old_i;
	int r;

	page_id = (buf[i]<<8)|buf[i+1]; i+=2;
	segment_length = (buf[i]<<8)|buf[i+1]; i+=2;
	j = i + segment_length;

	object_id = (buf[i]<<8)|buf[i+1]; i+=2;
	curr_obj = object_id;
	object_version_number = (buf[i]&0xf0)>>4;
	object_coding_method = (buf[i]&0x0c)>>2;
	non_modifying_colour_flag = (buf[i]&0x02)>>1;
	i++;

	//  Log("DVBsubs: "decoding object %d", object_id );
	
	old_i = i;
	for (r=0;r<MAX_REGIONS;r++) 
	{
		// If this object is in this region...
		if (regions[r].win >= 0) 
		{
			//Log("DVBsubs: "testing region %d, object_pos=%08x", r, regions[r].object_pos[object_id] );
			if (regions[r].object_pos[object_id]!=0xffffffff) 
			{
				//Log("DVBsubs: "rendering object %d into region %d", object_id, r );
				i=old_i;
				
				if (object_coding_method==0) 
				{
					top_field_data_block_length=(buf[i]<<8)|buf[i+1]; i+=2;
					bottom_field_data_block_length=(buf[i]<<8)|buf[i+1]; i+=2;
					Process_pixel_data_sub_block(r,object_id,0,top_field_data_block_length);
					Process_pixel_data_sub_block(r,object_id,1,bottom_field_data_block_length);
				}
			}
		}
	}
}

void CDVBSubDecoder::Save_png(char* filename) 
{
	int r;
	int x, y, out_y;
	int v;
	int count;

	out_y = 0;  

	count = 0;
	for ( r = 0; r < MAX_REGIONS ; r++ ) 
	{
		if (regions[r].win >= 0) 
		{
			if (page.regions[r].is_visible) 
			{
				//Log("DVBsubs: bitmap displaying region %d at %d,%d width=%d,height=%d\n",r,page.regions[r].x,page.regions[r].y,regions[r].width,regions[r].height);
				count++;

				out_y=page.regions[r].y*720;
				
				for ( y = 0 ; y < regions[r].height ; y++ ) 
				{
					for ( x = 0 ; x < regions[r].width ; x++ ) 
					{
						v = regions[r].img[(y*regions[r].width)+x];
						m_Buffer[out_y+x+page.regions[r].x]=v+16*regions[r].CLUT_id;
					}
				
				out_y+=720;
				}
			}
		}
	}
	m_CurrentSubtitle->RenderBitmap( m_Buffer, filename, colours, trans, 256 );
	

	m_RenderedSubtitles.resize( m_RenderedSubtitles.size() + 1 );
	m_RenderedSubtitles[m_RenderedSubtitles.size() - 1] = m_CurrentSubtitle;
	m_CurrentSubtitle = NULL; // ownership is transfered

	Log("New subtitle ready - subtitle cache count = %d", m_RenderedSubtitles.size() );
}

int CDVBSubDecoder::ProcessPES( const unsigned char* data, int length, int pid ) 
{
	if( m_CurrentSubtitle )
		delete m_CurrentSubtitle;

	m_CurrentSubtitle = new CSubtitle( 720, 576 );
	memset( m_Buffer, 0x00, sizeof( m_Buffer ) );
	
	int n;
	int vdrmode = 0;
	char filename[20];
	int page_id;
	int new_i;

	int PES_packet_length;
	int PES_header_data_length;

	uint64_t PTS;
	int r; 
	int data_identifier,subtitle_stream_id;

	int segment_length;
	int segment_type;
  
	Init_data();

	PES_packet_length = length;
	memcpy(buf,data,PES_packet_length);

	while (PES_packet_length >= 0) 
	{
		PTS = Get_pes_pts(buf);// / 90;
		m_CurrentSubtitle->SetPTS( PTS );

		if (first_PTS == 0) 
		{ 
			first_PTS=PTS; 
		}

		Log("%s\r", Pts2hmsu(PTS-first_PTS,'.'));

		PES_header_data_length=buf[8];
		i = 9 + PES_header_data_length;

		data_identifier = buf[i++];
		subtitle_stream_id = buf[i++];

		if( data_identifier != 0x20 ) 
		{
			Log("DVBsubs: ERROR: PES data_identifier != 0x20 (%02x), aborting\n",data_identifier);
			return 1;
		}
		
		if( subtitle_stream_id != 0 ) 
		{
			Log("DVBsubs: ERROR: subtitle_stream_id != 0 (%02x), aborting\n",subtitle_stream_id);
			return 1;
		}

		while(i < (PES_packet_length-1)) 
		{
			/* SUBTITLING SEGMENT */
			if( buf[i] != 0x0f ) 
			{ 
				Log("DVBsubs: ERROR: sync byte not present, skipping rest of PES packet - next PNG is sub%05d.png",fileno);
				i=PES_packet_length;
				continue;
			}
			
			i++;
			segment_type=buf[i++];
			//Log("Processing segment_type 0x%02x",segment_type);

			page_id=(buf[i]<<8)|buf[i+1]; 
			segment_length=(buf[i+2]<<8)|buf[i+3];

			new_i=i+segment_length+4;

			/* SEGMENT_DATA_FIELD */
			switch(segment_type) {
				case 0x10: 
					Process_page_composition_segment(); 
					break;
				case 0x11: 
					Process_region_composition_segment();
					break;
				case 0x12: 
					Process_CLUT_definition_segment();
					break;
				case 0x13: 
					Process_object_data_segment();
					break;
				case 0x80: 
					// IMPLEMENTATION IS OPTIONAL - dvbsubs ignores it.
					// end_of_display_set_segment(); 
					break;
				default:
					Log("DVBsubs: ERROR: Unknown segment %02x, length %d, data=%02x %02x %02x %02x",segment_type,segment_length,buf[i+4],buf[i+5],buf[i+6],buf[i+7]);
					exit(1);
			}
		i = new_i;
	}   

		if (acquired) 
		{
			acquired=0;
			n=0;
			for (r=0;r<MAX_REGIONS;r++) 
			{
				if (regions[r].win >= 0) 
				{
					if (page.regions[r].is_visible) 
					{
						n++;
					}
				}
			}
			
			if( n )
			{
				sprintf(filename,"sub%05d.bmp",ext++);
				Log("spu start=\"%s\" image=\"%s\"", Pts2hmsu(PTS-first_PTS,'.'),filename);
				Save_png( filename );
				
				if( m_pObserver )
				{
					m_pObserver->Notify();
				}

			}
		}
		Log("DVBsubs: END OF INPUT PES DATA.");
		return 0;
	}

	return 0;
}

char pts_text[30];
char* CDVBSubDecoder::Pts2hmsu( uint64_t pts, char sep ) 
{
	int h,m,s,u;

	pts /= 90; // Convert to milliseconds
	h = int( ( pts / ( 1000*60*60 ) ) );
	m = int( ( pts / ( 1000*60 ) ) - ( h*60 ) );
	s = int( ( pts/1000 ) - ( h*3600 ) - ( m*60 ) );
	u = int( pts - ( h*1000*60*60 ) - ( m*1000*60 ) - ( s*1000 ) );

	sprintf( pts_text,"%d:%02d:%02d%c%03d",h,m,s,sep,u );
	return( pts_text );
}

uint64_t CDVBSubDecoder::Get_pes_pts (unsigned char* buf) 
{
	UINT64 pts=0LL;
	UINT64 dts=0LL;
	UINT64 k = 0LL;
	//int PTS_DTS_flags;
	//uint64_t p0,p1,p2,p3,p4;
	bool PTS_available=false;
	bool DTS_available=false;

	if ( (buf[7]&0x80)!=0) PTS_available=true;
	if ( (buf[7]&0x40)!=0) DTS_available=true;
	//PTS_DTS_flags=(buf[7]&0xb0)>>6;
	
	//if ((PTS_DTS_flags&0x02)==0x02) 
	if (PTS_available)
	{
		// PTS is in bytes 9,10,11,12,13
		//p0=(buf[13]&0xfe)>>1|((buf[12]&1)<<7);
		//p1=(buf[12]&0xfe)>>1|((buf[11]&2)<<6);
		//p2=(buf[11]&0xfc)>>2|((buf[10]&3)<<6);
		//p3=(buf[10]&0xfc)>>2|((buf[9]&6)<<5);
		//p4=(buf[9]&0x08)>>3;

		//PTS=p0|(p1<<8)|(p2<<16)|(p3<<24)|(p4<<32);

		/* Hobbit code to replace earlier futile attempts to solve these numbers */

		pts+= ((buf[13]>>1)&0x7f);				// 7bits	7
		pts+=(buf[12]<<7);								// 8bits	15
		pts+=((buf[11]>>1)<<15);					// 7bits	22
		pts+=((buf[10])<<22);							// 8bits	30
		k=((buf[9]>>1)&0x7);
		k <<=30LL;
		pts+=k;			// 3bits
		pts &= 0x1FFFFFFFFLL;
		
	}
	if (DTS_available) 
	{
		dts= (buf[18]>>1);								// 7bits	7
		dts+=(buf[17]<<7);								// 8bits	15
		dts+=((buf[16]>>1)<<15);					// 7bits	22
		dts+=((buf[15])<<22);							// 8bits	30
		k=((buf[14]>>1)&0x7);
		k <<=30LL;
		dts+=k;			// 3bits
		dts &= 0x1FFFFFFFFLL;
	}
	Log("Decoder PTS: %lld, DTS: %lld",pts,dts);
	return( pts );
}

BITMAP* CDVBSubDecoder::GetSubtitle()
{
	return m_CurrentSubtitle->GetBitmap();
}

long CDVBSubDecoder::GetSubtitleId()
{
	return ext;
}

void CDVBSubDecoder::SetObserver( MSubdecoderObserver* pObserver )
{
	m_pObserver = pObserver;
}

void CDVBSubDecoder::RemoveObserver( MSubdecoderObserver* pObserver )
{
	if( m_pObserver == pObserver )
	{
		m_pObserver = NULL;	
	}
}

CSubtitle* CDVBSubDecoder::GetSubtitle( unsigned int place )
{
	if( m_RenderedSubtitles.size() > place )
	{
		return m_RenderedSubtitles[place];
	}
	else
	{
		return NULL;
	}
}


CSubtitle* CDVBSubDecoder::GetLatestSubtitle()
{
	int size = m_RenderedSubtitles.size();

	if( size > 0 )
	{
		return m_RenderedSubtitles[size - 1];
	}
	else
	{
		return NULL;
	}
}

void CDVBSubDecoder::ReleaseOldestSubtitle()
{
	m_RenderedSubtitles.erase( m_RenderedSubtitles.begin() );		
}

void CDVBSubDecoder::Reset()
{
	page.page_time_out = 0;
	page.acquired = 0;
	page.page_state = 0;
	page.page_version_number = 0;
	page.page_state = 0;

//	region_t regions[MAX_REGIONS];
	ZeroMemory( colours, 256*3 );
	ZeroMemory( trans, 256 );
	
	apid = 0;
	y = 0;
	x = 0;
	
	// do not reset ( causes debug files to be over written ) 
	//ext = 0;

	acquired = 0;
	PTS = 0;
	first_PTS = 0;

	curr_obj = 0;
	
	ZeroMemory( curr_reg, 64 );
	ZeroMemory( buf, 720*576 );  
	
	i = 0;
	
	nibble_flag = 0;
	in_scanline = 0;
	video_pts = 0;
	first_video_pts = 0;
	audio_pts = 0;
	first_audio_pts = 0;
	audio_pts_wrap = 0;

	for( unsigned int i( 0 ) ; i < m_RenderedSubtitles.size() ; i++ )
	{
		m_RenderedSubtitles.erase( m_RenderedSubtitles.begin() );	
	}
}