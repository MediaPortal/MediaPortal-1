#ifndef _PES_H
#define _PES_H

typedef unsigned __int64 uint64_t;
typedef unsigned __int16 uint16_t;
typedef unsigned __int8 uint8_t;

uint64_t get_pes_pts (unsigned char* buf);

#endif
