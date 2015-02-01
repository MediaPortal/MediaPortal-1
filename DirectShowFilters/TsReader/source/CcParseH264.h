
#ifndef __CCparse_H
#define __CCparse_H

#include "StdAfx.h"

#include <atlbase.h>
#include <mmsystem.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

//#include "lib_ccx.h"
//#include "ccx_common_option.h"
//#include "utility.h"
//#include <math.h>

// Functions to parse a AVC/H.264 data stream, see ISO/IEC 14496-10

class CCparse
{
public :
 
CCparse();
virtual ~CCparse();

//void          sei_rbsp (unsigned char *seibuf, unsigned char *seiend);
void          sei_rbsp (unsigned char *seibuf, int Length);
//void          init_avc(void);

protected :

//#define ZEROBYTES_SHORTSTARTCODE 2
//typedef unsigned int u32;

int ccblocks_in_avc_total=0;
int ccblocks_in_avc_lost=0;
long num_unexpected_sei_length=0;

unsigned  char cc_count;
// buffer to hold cc data
unsigned char *cc_data = NULL;
long      cc_databufsize = 1024;
int       cc_buffer_saved=1; // Was the CC buffer saved after it was last updated?

//int       got_seq_para=0;
//unsigned  nal_ref_idc;
//LLONG     seq_parameter_set_id;
//int       log2_max_frame_num=0;
//int       pic_order_cnt_type;
//int       log2_max_pic_order_cnt_lsb=0;
//int       frame_mbs_only_flag;

// Use and throw stats for debug, remove this uglyness soon
//long num_nal_unit_type_7=0;
//long num_vcl_hrd=0;
//long num_nal_hrd=0;
//long num_jump_in_frames=0;

//double roundportable(double x) { return floor(x + 0.5); }

// local functions
unsigned char   *sei_message (unsigned char *seibuf, unsigned char *seiend);
void            user_data_registered_itu_t_t35 (unsigned char *userbuf, unsigned char *userend);
void            copy_ccdata_to_buffer (char *source, int new_cc_count);
int             EBSPtoRBSP(unsigned char *streamBuffer, int end_bytepos, int begin_bytepos);
unsigned char   *remove_03emu(unsigned char *from, unsigned char *to);

//u32           avc_remove_emulation_bytes(const unsigned char *buffer_src, unsigned char *buffer_dst, u32 nal_size) ;
//void          seq_parameter_set_rbsp (unsigned char *seqbuf, unsigned char *seqend);
//void          slice_header (struct lib_ccx_ctx *ctx, unsigned char *heabuf, unsigned char *heaend, int nal_unit_type, struct cc_subtitle *sub);
};

#endif
