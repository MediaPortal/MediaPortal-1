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
#include <streams.h>    // IUnknown


DECLARE_INTERFACE_(IObserver, IUnknown)
{
  BEGIN_INTERFACE


  // IUnknown
  STDMETHOD(QueryInterface)(THIS_ REFIID riid, void** ppv)PURE;
  STDMETHOD_(unsigned long, AddRef)(THIS)PURE;
  STDMETHOD_(unsigned long, Release)(THIS)PURE;


  // IObserver
  STDMETHOD_(void, OnProgramAssociationTable)(THIS_ unsigned short transportStreamId,
                                              unsigned short networkPid,
                                              unsigned short programCount)PURE;
  STDMETHOD_(void, OnConditionalAccessTable)(THIS_ const unsigned char* cat,
                                              unsigned short catBufferSize)PURE;
  STDMETHOD_(void, OnProgramDetail)(THIS_ unsigned short programNumber,
                                    unsigned short pmtPid,
                                    bool isRunning,
                                    const unsigned char* pmt,
                                    unsigned short pmtBufferSize)PURE;
  STDMETHOD_(void, OnPidEncryptionStateChange)(THIS_ unsigned short pid,
                                                unsigned long state)PURE;
  STDMETHOD_(void, OnPidsRequired)(THIS_ unsigned short* pids,
                                    unsigned char pidCount,
                                    unsigned long usage)PURE;
  STDMETHOD_(void, OnPidsNotRequired)(THIS_ unsigned short* pids,
                                      unsigned char pidCount,
                                      unsigned long usage)PURE;


  END_INTERFACE
};