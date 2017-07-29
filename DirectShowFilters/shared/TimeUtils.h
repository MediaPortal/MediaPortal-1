/*
 *  Copyright (C) 2006-2008 Team MediaPortal
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
#pragma once
#include <ctime>

using namespace std;


#define SECONDS_PER_YEAR  31536000
#define SECONDS_PER_DAY   86400
#define SECONDS_PER_HOUR  3600


class CTimeUtils
{
  public:
    static unsigned long ElapsedMillis(clock_t start)
    {
      return (clock() - start) * 1000 / CLOCKS_PER_SEC;
    }

    static unsigned long long DecodeMjDateBcdTime(unsigned short dateMjd, unsigned long timeBcd)
    {
      if (dateMjd == 0xffff && timeBcd == 0xffffff)
      {
        // NVOD reference service event.
        return 0;
      }

      // Calculate the corresponding epoch/Unix/POSIX time.
      unsigned short year = (unsigned short)((dateMjd - 15078.2) / 365.25);
      unsigned long temp1 = (unsigned long)((unsigned long)year * 365.25);
      unsigned char month = (unsigned char)((dateMjd - 14956.1 - temp1) / 30.6001);
      unsigned char day = (unsigned char)(dateMjd - 14956 - temp1 - (unsigned short)((unsigned short)month * 30.6001));
      unsigned char adjustment = (month == 14 || month == 15) ? 1 : 0;
      year += adjustment + 1900;
      month = month - 1 - adjustment * 12;

      unsigned long hour = ((timeBcd >> 20) * 10) + ((timeBcd >> 16) & 0x0f);
      unsigned long minute = (((timeBcd >> 12) & 0x0f) * 10) + ((timeBcd >> 8) & 0x0f);
      unsigned long second = (((timeBcd >> 4) & 0x0f) * 10) + (timeBcd & 0x0f);

      static const unsigned short CUMULATIVE_DAYS[12] = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
      unsigned long long result = (((unsigned long long)year - 1970) * SECONDS_PER_YEAR) + ((CUMULATIVE_DAYS[month - 1] + day - 1) * SECONDS_PER_DAY) + (hour * SECONDS_PER_HOUR) + (minute * 60) + second;

      // This leap year adjustment only supports a limited date range, but that
      // shouldn't be a problem.
      if (month < 3)
      {
        year--;
      }
      return result + ((unsigned long long)((year - 1968) / 4) * SECONDS_PER_DAY);
    }
};