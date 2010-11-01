/*********************************************************************************************************
 *
 *  Module Name:	debug.h
 *
 *  Abstract:		Debuging macroses
 *
 *  Author:		Vyacheslav I. Levtchenko (mail-to: sl@r-tt.com, sl@eltrast.ru)
 *
 *  Revision History:	20.10.2003	started
 *
 *  Classes, methods and structures:
 *
 *  TODO:
 *
 *********************************************************************************************************/

#ifndef __DEBUG_H__
#define __DEBUG_H__

#if (DEBUG)
#  ifndef __GNUC__
#    define brk() {_asm int 3}
#  else
#    define brk() { asm ("int $3"); }
#  endif
#else
#  ifndef __GNUC__
#    define brk() {/* nothing */;}
#  else
#    define brk() {/* nothing */;}
#  endif
#endif

#if (TOTALDEBUG)
# define tbrk()	brk()
#else
# define tbrk()
#endif

#endif
