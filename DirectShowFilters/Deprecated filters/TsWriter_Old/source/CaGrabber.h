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
#include "sectiondecoder.h"
#include "section.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "TsHeader.h"

using namespace Mediaportal;

// {F9AA3910-7818-452a-94D1-72E039DF50EF}
DEFINE_GUID(IID_ICaGrabber,0xf9aa3910, 0x7818, 0x452a, 0x94, 0xd1, 0x72, 0xe0, 0x39, 0xdf, 0x50, 0xef);

DECLARE_INTERFACE_(ICACallback, IUnknown)
{
	STDMETHOD(OnCaReceived)()PURE;
};


DECLARE_INTERFACE_(ICaGrabber, IUnknown)
{
	STDMETHOD(SetCallBack)(THIS_ ICACallback* callback)PURE;
	STDMETHOD(GetCaData) (THIS_ BYTE *caData)PURE;
	STDMETHOD(Reset)()PURE;
};

class CCaGrabber: public CUnknown, public CSectionDecoder, public ICaGrabber
{
public:
	CCaGrabber(LPUNKNOWN pUnk, HRESULT *phr);
	~CCaGrabber(void);

  DECLARE_IUNKNOWN
	STDMETHODIMP SetCallBack( ICACallback* callback);
	STDMETHODIMP GetCaData(BYTE *caData);
	STDMETHODIMP Reset();

	void OnTsPacket(byte* tsPacket);
  virtual void OnNewSection(CSection& section);
private:
	ICACallback* m_pCallback;
  byte					m_caData[MAX_SECTION_LENGTH];
	byte					m_caPrevData[MAX_SECTION_LENGTH];
	int						m_iCaVersion;
	int						m_iCaLength;
  CTsHeader     m_tsHeader;
	CCriticalSection m_section;
};
