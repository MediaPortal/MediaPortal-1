/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#include "rtspclient.h"
#include "MemoryBuffer.h"
// {DF5ACC0A-5612-44ba-963B-C757298F4030}
DEFINE_GUID(CLSID_RtspSource,0xdf5acc0a, 0x5612, 0x44ba, 0x96, 0x3b, 0xc7, 0x57, 0x29, 0x8f, 0x40, 0x30);
class COutputPin;
class CRtspSourceFilter: public CSource,public IFileSourceFilter, public IAMFilterMiscFlags
{
public:
		DECLARE_IUNKNOWN
		static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);
    CRtspSourceFilter(IUnknown *pUnk, HRESULT *phr);
    ~CRtspSourceFilter(void);
		STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

    // Pin enumeration
    CBasePin * GetPin(int n);
    int GetPinCount();

    // Open and close the file as necessary
    STDMETHODIMP Run(REFERENCE_TIME tStart);
    STDMETHODIMP Pause();
    STDMETHODIMP Stop();
	// IAMFilterMiscFlags
		virtual ULONG STDMETHODCALLTYPE		GetMiscFlags();
    
	// IFileSourceFilter
	STDMETHODIMP    Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
	STDMETHODIMP    GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
	STDMETHODIMP    GetDuration(REFERENCE_TIME *dur);
  LONG GetData(BYTE* pData, long size);
	void GetStartStop(CRefTime &m_rtStart, CRefTime &m_rtStop);
	void Seek(float start);
private:
	CCritSec        m_section;
  COutputPin* m_pOutputPin;
  WCHAR m_fileName[1024];
  CRTSPClient m_client;
  CMemoryBuffer m_buffer;
};
