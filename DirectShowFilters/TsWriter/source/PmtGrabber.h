/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#include "entercriticalsection.h"
#include "..\..\shared\TsHeader.h"

using namespace Mediaportal;

// {6E714740-803D-4175-BEF6-67246BDF1855}
DEFINE_GUID(IID_IPmtGrabber,0x6e714740, 0x803d, 0x4175, 0xbe, 0xf6, 0x67, 0x24, 0x6b, 0xdf, 0x18, 0x55);

DECLARE_INTERFACE_(IPMTCallback, IUnknown)
{
	STDMETHOD(OnPMTReceived)(THIS_ int pmtPid)PURE;
};


DECLARE_INTERFACE_(IPmtGrabber, IUnknown)
{
	STDMETHOD(SetPmtPid)(THIS_ int pmtPid, long serviceId)PURE;
	STDMETHOD(SetCallBack)(THIS_ IPMTCallback* callback)PURE;
	STDMETHOD(GetPMTData) (THIS_ BYTE *pmtData)PURE;
};

class CPmtGrabber: public CUnknown, public CSectionDecoder, public IPmtGrabber
{
public:
	CPmtGrabber(LPUNKNOWN pUnk, HRESULT *phr);
	~CPmtGrabber(void);

  DECLARE_IUNKNOWN
	STDMETHODIMP SetPmtPid( int pmtPid, long serviceId);
	STDMETHODIMP SetCallBack( IPMTCallback* callback);
	STDMETHODIMP GetPMTData(BYTE *pmtData);

	void OnTsPacket(byte* tsPacket);
  virtual void OnNewSection(CSection& section);
private:
	IPMTCallback* m_pCallback;
  byte					m_pmtData[MAX_SECTION_LENGTH];
	byte					m_pmtPrevData[MAX_SECTION_LENGTH];
	int						m_iPmtVersion;
	int						m_iPmtLength;
	int						m_iServiceId;
  bool          GetPMTPid;
  CTsHeader     m_tsHeader;
  CPatParser  	m_patgrab;
	CCriticalSection m_section;
};
