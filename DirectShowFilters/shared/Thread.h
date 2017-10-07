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
#include "Windows.h"  // HANDLE


class CThread
{
  public:
    CThread();
    ~CThread();

    bool Start(unsigned long frequency, bool (*function)(void*), void* context);
    bool Wake();
    void Stop();

  private:
    static void __cdecl ThreadFunction(void* arg);

    HANDLE m_thread;
    HANDLE m_wakeEvent;
    unsigned long m_wakeCount;
    bool m_stopSignal;
    unsigned long m_frequency;    // unit = ms
    bool (*m_function)(void* context);
    void* m_context;
};