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
#include "..\..\shared\sectiondecoder.h"
#include "..\..\shared\section.h"
#include "criticalsection.h"
#include "PmtParser.h"
#include "PatParser.h"
#include "SdtParser.h"
#include "VctParser.h"
#include "entercriticalsection.h"

using namespace Mediaportal;

// {6E714740-803D-4175-BEF6-67246BDF1855}
DEFINE_GUID(IID_IPmtGrabber, 0x6e714740, 0x803d, 0x4175, 0xbe, 0xf6, 0x67, 0x24, 0x6b, 0xdf, 0x18, 0x55);

DECLARE_INTERFACE_(IPmtCallBack, IUnknown)
{
  STDMETHOD(OnPmtReceived)(THIS_ int pmtPid, int serviceId, int isServiceRunning)PURE;
};

DECLARE_INTERFACE_(IPmtGrabber, IUnknown)
{
  STDMETHOD(SetPmtPid)(THIS_ int pmtPid, int serviceId)PURE;
  STDMETHOD(SetCallBack)(THIS_ IPmtCallBack* callback)PURE;
  STDMETHOD(GetPmtData)(THIS_ BYTE* pmtData)PURE;
};

class CPmtGrabber : public CUnknown, public CSectionDecoder, public IPmtGrabber, IPatCallBack, ISdtCallBack, IVctCallBack
{
  public:
    CPmtGrabber(LPUNKNOWN pUnk, HRESULT *phr);
    ~CPmtGrabber(void);

    DECLARE_IUNKNOWN
    STDMETHODIMP SetPmtPid(int pmtPid, int serviceId);
    STDMETHODIMP SetCallBack(IPmtCallBack* callBack);
    STDMETHODIMP GetPmtData(BYTE* pmtData);

    void OnTsPacket(byte* tsPacket);
    virtual void OnNewSection(CSection& section);
    void OnPatReceived(int serviceId, int pmtPid);
    void OnSdtReceived(const CChannelInfo& sdtInfo);
    void OnVctReceived(const CChannelInfo& vctInfo);

  private:
    IPmtCallBack* m_pCallBack;
    byte m_pmtData[MAX_SECTION_LENGTH];
    int m_iPmtVersion;
    int m_iPmtLength;
    int m_iOriginalServiceId;   // The service ID passed to us.
    int m_iCurrentServiceId;    // The service ID that we're actually monitoring.
    CSection m_pmtPrevSection;
    CPatParser m_patParser;
    CSdtParser m_sdtParser;
    CVctParser m_vctParser;
    CCriticalSection m_section;
};