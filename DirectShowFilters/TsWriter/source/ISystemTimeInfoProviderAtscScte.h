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


// Number of non-leap seconds between the start of epoch/Unix/POSIX time (1
// January 1970 00:00:00 UTC) and the start of GPS time (6 January 1980
// 00:00:00 UTC):
// = ((10 years x 365 days per year) + 2 leap year days + 5 days from 1980) x 24 hours per day x 60 minutes per hour x 60 seconds per minute
#define GPS_TIME_START_OFFSET 315964800


class ISystemTimeInfoProviderAtscScte
{
  public:
    virtual ~ISystemTimeInfoProviderAtscScte() {}

    virtual bool GetSystemTimeDetail(unsigned long& systemTime,
                                      unsigned char& gpsUtcOffset,
                                      bool& isDaylightSavingStateKnown,
                                      bool& isDaylightSaving,
                                      unsigned char& daylightSavingDayOfMonth,
                                      unsigned char& daylightSavingHour) const = 0;
};