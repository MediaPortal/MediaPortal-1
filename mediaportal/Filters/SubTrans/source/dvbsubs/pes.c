
#include <stdio.h>
#include <string.h>
#include "pes.h"

char pts_text[30];

uint64_t get_pes_pts( unsigned char* buf ) 
{
	uint64_t PTS;
	int PTS_DTS_flags;
	uint64_t p0,p1,p2,p3,p4;

	PTS_DTS_flags=(buf[7]&0xb0)>>6;
	
	if ((PTS_DTS_flags&0x02)==0x02) 
	{
		// PTS is in bytes 9,10,11,12,13
		p0 = (buf[13]&0xfe)>>1|((buf[12]&1)<<7);
		p1 = (buf[12]&0xfe)>>1|((buf[11]&2)<<6);
		p2 = (buf[11]&0xfc)>>2|((buf[10]&3)<<6);
		p3 = (buf[10]&0xfc)>>2|((buf[9]&6)<<5);
		p4 = (buf[9]&0x08)>>3;

		PTS = p0|(p1<<8)|(p2<<16)|(p3<<24)|(p4<<32);
	} 
	else 
	{
		PTS=0;
	}
	
	return(PTS);
}