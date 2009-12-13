/* 
 *      Copyright (C) 2005-2009 Team MediaPortal
 *      http://www.team-mediaportal.com
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
 *  cMulDiv64 based on Richard van der Wal's ASM version (R.vdWal@xs4all.nl)
 */

#include <windows.h>
#include <streams.h>  // CAutolock

#define ABS64(num) (num >=0 ? num : -num)
#define LowDW(num) ((unsigned __int64)(unsigned long)(num & 0xFFFFFFFFUL))
#define HighDW(num) ((unsigned __int64)(num >> 32))

static BOOL           g_bTimerInitializer = false;
static BOOL           g_bQPCAvail;
static LARGE_INTEGER  g_lPerfFrequency;

static CCritSec lock;  // lock for timer initialization (multiple threads are using the timer during startup)


#pragma warning(disable: 4723)
__int64 _stdcall cMulDiv64(__int64 operant, __int64 multiplier, __int64 divider)
{
	// Declare 128bit storage
	union {
		unsigned long DW[4];
    struct {
      unsigned __int64 LowQW;
      unsigned __int64 HighQW;
    };
	} var128, quotient;
	// Change semantics for intermediate results for Full Div by renaming the vars
	#define REMAINDER quotient
	#define QUOTIENT var128

  bool negative = ((operant ^ multiplier ^ divider) & 0x8000000000000000LL) != 0;

	// Take absolute values because algorithm is for unsigned only
	operant		 = ABS64(operant);
	multiplier = ABS64(multiplier);
	divider		 = ABS64(divider);

  // integer division by zero needs to be handled in the calling method
  if (divider == 0)
  {
    return 1/divider;
    #pragma warning(default: 4723)
  }
  
  // Multiply
  if (multiplier == 0)
  {
    return 0;
  }

  var128.HighQW = 0;

  if (multiplier == 1)
  {
    var128.LowQW = operant;
  }
  else if (((multiplier | operant) & 0xFFFFFFFF00000000LL) == 0)
  {
    // 32*32 multiply
    var128.LowQW = operant * multiplier;
  }
  else
  {
    // Full multiply: var128 = operant * multiplier
    var128.LowQW = LowDW(operant) * LowDW(multiplier);
    unsigned __int64 tmp = var128.DW[1] + LowDW(operant) * HighDW(multiplier);
    unsigned __int64 tmp2 = tmp + HighDW(operant) * LowDW(multiplier);
    if(tmp2 < tmp)
    {
      var128.DW[3]++;
    }
    var128.DW[1] = LowDW(tmp2);
    var128.DW[2] = HighDW(tmp2);
    var128.HighQW += HighDW(operant) * HighDW(multiplier);
  }
  
  // Divide
  if (HighDW(divider) == 0)
  {
    if (divider != 1)
    {
      // 32 bit divisor, do 128:32
      quotient.DW[3] = (unsigned long)(var128.DW[3] / divider);
      unsigned __int64 tmp = ((var128.DW[3] % divider) << 32) | var128.DW[2];
      quotient.DW[2] = (unsigned long)(tmp / divider);
      tmp = ((tmp % divider) << 32) | var128.DW[1];
      quotient.DW[1] = (unsigned long)(tmp / divider);
      tmp = ((tmp % divider) << 32) |  var128.DW[0];
      quotient.DW[0] = (unsigned long)(tmp / divider);
      var128 = quotient;
    }
  }
  else
  {
    // 64 bit divisor, do full division (128:64)
    int c = 128;
    quotient.LowQW = 0;
    quotient.HighQW = 0;
    do
    {
      REMAINDER.HighQW = (REMAINDER.HighQW << 1) | (REMAINDER.DW[1] >> 31);
      REMAINDER.LowQW = (REMAINDER.LowQW << 1) | (QUOTIENT.DW[3] >> 31);
      QUOTIENT.HighQW = (QUOTIENT.HighQW << 1) | (QUOTIENT.DW[1] >> 31);
      QUOTIENT.LowQW = (QUOTIENT.LowQW << 1);
      if (REMAINDER.HighQW > 0 || REMAINDER.LowQW >= (unsigned __int64)divider)
      {
        if (REMAINDER.LowQW < (unsigned __int64)divider)
        {
          REMAINDER.LowQW -= divider;
          REMAINDER.HighQW--;
        }
        else
        {
          REMAINDER.LowQW -= divider;
        }
        if (++QUOTIENT.LowQW == 0)
        {
          QUOTIENT.HighQW++;
        }
      }
    } while(--c > 0);
  }

  // Apply Sign
  if (negative)
  {
	  return -(__int64)var128.LowQW;
  }
  else
  {
    return (__int64)var128.LowQW;
  }
}


LONGLONG GetCurrentTimestamp()
{
  LONGLONG result;
  if (!g_bTimerInitializer)
  {
    CAutoLock lock(&lock);
    DWORD_PTR oldmask = SetThreadAffinityMask(GetCurrentThread(), 0);
    g_bQPCAvail = QueryPerformanceFrequency((LARGE_INTEGER*)&g_lPerfFrequency);
    SetThreadAffinityMask(GetCurrentThread(), oldmask);
    g_bTimerInitializer = true;
    if( g_lPerfFrequency.QuadPart == 0)
    {
      // Bug in HW? Frequency cannot be zero
      g_bQPCAvail = false;
    }
  }
  if (g_bQPCAvail)
  {
    // http://msdn.microsoft.com/en-us/library/ms644904(VS.85).aspx
    // Use always the same CPU core (should help with broken BIOS and/or HAL)
    DWORD_PTR oldmask = SetThreadAffinityMask(GetCurrentThread(), 1);
    ULARGE_INTEGER tics;
    QueryPerformanceCounter((LARGE_INTEGER*)&tics);
    SetThreadAffinityMask(GetCurrentThread(), oldmask);
    result = cMulDiv64(tics.QuadPart, 10000000, g_lPerfFrequency.QuadPart); // to keep accuracy
  }
  else
  {
    result = timeGetTime() * 10000; // ms to 100ns units
  }
  return result;
}

