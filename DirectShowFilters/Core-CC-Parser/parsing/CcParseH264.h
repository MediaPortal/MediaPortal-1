/*
 *  Copyright (C) 2015 Team MediaPortal
 *  http://www.team-mediaportal.com
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
 
//  Derived from 'CCExtractor' code, credits below:
//  ===============================================
//  ccextractor, 0.75
//  -----------------
//  Authors: Carlos Fernández (cfsmp3), Volker Quetschke.
//  Maintainer: cfsmp3
//  
//  Lots of credit goes to other people, though:
//  McPoodle (author of the original SCC_RIP), Neuron2, 
//  and others (see source code).
//  
//  Home: http://www.ccextractor.org
//  
//  Google Summer of Code 2014 students
//  - Willem van iseghem
//  - Ruslan KuchumoV
//  - Anshul Maheshwari
//  -----------------------------------------------

#ifndef __CcParseH264_H
#define __CcParseH264_H


// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

using namespace std;

// Functions to parse an AVC/H.264 data stream to extract Closed Caption data, see ISO/IEC 14496-10

class CcParseH264
{
public :
 
CcParseH264();
virtual ~CcParseH264();

void  sei_rbsp (unsigned char *seibuf, int Length);
void  do_NAL (unsigned char *NALstart, int Length);
DWORD  parseAVC1sample (const BYTE* pData, DWORD sampleLength, DWORD dwFlags);
BYTE* get_cc_buffer_pointer();

private :

int           ccblocks_in_avc_total=0;
int           ccblocks_in_avc_lost=0;
long          num_unexpected_sei_length=0;
WORD          cc_count = 0;
DWORD         cc_bytes_in_buffer = 0; //Max buffer size is 65536

// buffer to hold cc data
unsigned char *cc_data = NULL;
DWORD         cc_databufsize = 1024;

// local functions
unsigned char   *sei_message (unsigned char *seibuf, unsigned char *seiend);
void            user_data_registered_itu_t_t35 (unsigned char *userbuf, unsigned char *userend);
void            copy_ccdata_to_buffer (unsigned char *source, int new_cc_count);
int             EBSPtoRBSP(unsigned char *streamBuffer, int end_bytepos, int begin_bytepos);
//unsigned char   *remove_03emu(unsigned char *from, unsigned char *to);

};

#endif
