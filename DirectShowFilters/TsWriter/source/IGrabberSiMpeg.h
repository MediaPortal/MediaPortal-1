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
#include "IGrabber.h"


// {e8f4ec8c-c4e0-44a3-a18c-00789a0e955e}
DEFINE_GUID(IID_IGRABBER_SI_MPEG,
            0xe8f4ec8c, 0xc4e0, 0x44a3, 0xa1, 0x8c, 0x0, 0x78, 0x9a, 0xe, 0x95, 0x5e);

DECLARE_INTERFACE_(IGrabberSiMpeg, IGrabber)
{
  BEGIN_INTERFACE


  // IUnknown
  STDMETHOD(QueryInterface)(THIS_ REFIID riid, void** ppv)PURE;
  STDMETHOD_(unsigned long, AddRef)(THIS)PURE;
  STDMETHOD_(unsigned long, Release)(THIS)PURE;


  // IGrabber
  STDMETHOD_(void, SetCallBack)(THIS_ ICallBackGrabber* callBack)PURE;


  // IGrabberSiMpeg
  STDMETHOD_(bool, IsReadyPat)(THIS)PURE;
  STDMETHOD_(bool, IsReadyCat)(THIS)PURE;
  STDMETHOD_(bool, IsReadyPmt)(THIS)PURE;

  STDMETHOD_(void, GetTransportStreamDetail)(THIS_ unsigned short* transportStreamId,
                                              unsigned short* networkPid,
                                              unsigned short* programCount)PURE;
  STDMETHOD_(bool, GetProgramByIndex)(THIS_ unsigned short index,
                                      unsigned short* programNumber,
                                      unsigned short* pmtPid,
                                      bool* isPmtReceived,
                                      unsigned short* streamCountVideo,
                                      unsigned short* streamCountAudio,
                                      bool* isEncrypted,
                                      bool* isEncryptionDetectionAccurate,
                                      bool* isThreeDimensional,
                                      unsigned long* audioLanguages,
                                      unsigned char* audioLanguageCount,
                                      unsigned long* subtitlesLanguages,
                                      unsigned char* subtitlesLanguageCount)PURE;
  STDMETHOD_(bool, GetProgramByNumber)(THIS_ unsigned short programNumber,
                                        unsigned short* pmtPid,
                                        bool* isPmtReceived,
                                        unsigned short* streamCountVideo,
                                        unsigned short* streamCountAudio,
                                        bool* isEncrypted,
                                        bool* isEncryptionDetectionAccurate,
                                        bool* isThreeDimensional,
                                        unsigned long* audioLanguages,
                                        unsigned char* audioLanguageCount,
                                        unsigned long* subtitlesLanguages,
                                        unsigned char* subtitlesLanguageCount)PURE;

  STDMETHOD_(bool, GetCat)(THIS_ unsigned char* table, unsigned short* tableBufferSize)PURE;
  STDMETHOD_(bool, GetPmt)(THIS_ unsigned short programNumber,
                            unsigned char* table,
                            unsigned short* tableBufferSize)PURE;


  END_INTERFACE
};