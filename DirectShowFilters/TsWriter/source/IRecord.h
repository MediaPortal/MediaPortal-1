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


class IRecord
{
  public:
    IRecord()
    {
      LastSeen = clock();
    }

    virtual ~IRecord()
    {
    }

    virtual bool Equals(const IRecord* record) const = 0;

    bool operator == (const IRecord& record)
    {
      return Equals(&record);
    }

    bool operator != (const IRecord& record)
    {
      return !Equals(&record);
    }

    virtual unsigned long long GetKey() const = 0;

    virtual unsigned long long GetExpiryKey() const = 0;

    virtual void Debug(const wchar_t* situation) const
    {
    }

    virtual void OnReceived(void* callBack) const
    {
    }

    virtual void OnChanged(void* callBack) const
    {
    }

    virtual void OnRemoved(void* callBack) const
    {
    }

    clock_t LastSeen;
};