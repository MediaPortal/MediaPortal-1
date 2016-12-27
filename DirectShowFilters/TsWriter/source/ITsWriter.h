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
#include <InitGuid.h>   // DEFINE_GUID()
#include <streams.h>    // IUnknown
#include "IChannelObserver.h"
#include "IObserver.h"


// {5eb9f392-e7fd-4071-8e44-3590e5e767ba}
DEFINE_GUID(IID_ITS_WRITER,
            0x5eb9f392, 0xe7fd, 0x4071, 0x8e, 0x44, 0x35, 0x90, 0xe5, 0xe7, 0x67, 0xba);

DECLARE_INTERFACE_(ITsWriter, IUnknown)
{
  STDMETHOD(ConfigureLogging)(THIS_ wchar_t* path)PURE;
  STDMETHOD_(void, DumpInput)(THIS_ bool enableTs, bool enableOobSi)PURE;
  STDMETHOD_(void, CheckSectionCrcs)(THIS_ bool enable)PURE;
  STDMETHOD_(void, SetObserver)(THIS_ IObserver* observer)PURE;

  STDMETHOD_(void, Start)(THIS)PURE;
  STDMETHOD_(void, Stop)(THIS)PURE;

  STDMETHOD(AddChannel)(THIS_ IChannelObserver* observer, long* handle)PURE;
  STDMETHOD_(void, GetPidState)(THIS_ unsigned short pid, unsigned long* state)PURE;
  STDMETHOD_(void, DeleteChannel)(THIS_ long handle)PURE;
  STDMETHOD_(void, DeleteAllChannels)(THIS)PURE;

  STDMETHOD(RecorderSetFileName)(THIS_ long handle, wchar_t* fileName)PURE;
  STDMETHOD(RecorderSetPmt)(THIS_ long handle,
                            unsigned char* pmt,
                            unsigned short pmtSize,
                            bool isDynamicPmtChange)PURE;
  STDMETHOD(RecorderStart)(THIS_ long handle)PURE;
  STDMETHOD(RecorderPause)(THIS_ long handle, bool isPause)PURE;
  STDMETHOD(RecorderGetStreamQuality)(THIS_ long handle,
                                      unsigned long long* countTsPackets,
                                      unsigned long long* countDiscontinuities,
                                      unsigned long long* countDroppedBytes)PURE;
  STDMETHOD(RecorderStop)(THIS_ long handle)PURE;

  STDMETHOD(TimeShifterSetFileName)(THIS_ long handle, wchar_t* fileName)PURE;
  STDMETHOD(TimeShifterSetParameters)(THIS_ long handle,
                                      unsigned long fileCountMinimum,
                                      unsigned long fileCountMaximum,
                                      unsigned long long fileSizeBytes)PURE;
  STDMETHOD(TimeShifterSetPmt)(THIS_ long handle,
                                unsigned char* pmt,
                                unsigned short pmtSize,
                                bool isDynamicPmtChange)PURE;
  STDMETHOD(TimeShifterStart)(THIS_ long handle)PURE;
  STDMETHOD(TimeShifterPause)(THIS_ long handle, bool isPause)PURE;
  STDMETHOD(TimeShifterGetStreamQuality)(THIS_ long handle,
                                          unsigned long long* countTsPackets,
                                          unsigned long long* countDiscontinuities,
                                          unsigned long long* countDroppedBytes)PURE;
  STDMETHOD(TimeShifterGetCurrentFilePosition)(THIS_ long handle,
                                                unsigned long long* position,
                                                unsigned long* bufferId)PURE;
  STDMETHOD(TimeShifterStop)(THIS_ long handle)PURE;
};