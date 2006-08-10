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
#include "multifilereader.h"
#include "pcrdecoder.h"
#include "demultiplexer.h"
#include <map>
using namespace std;

class CAudioPin;
class CVideoPin;
class CTsReader;
class CTsReaderFilter;

DEFINE_GUID(CLSID_TSReader, 0xb9559486, 0xe1bb, 0x45d3, 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x3f);


class CTsReaderFilter : public CSource,public IFileSourceFilter, public IAMFilterMiscFlags
{
public:
		DECLARE_IUNKNOWN
		static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

private:
		CTsReaderFilter(IUnknown *pUnk, HRESULT *phr);
		~CTsReaderFilter();
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

public:
	// IFileSourceFilter
	STDMETHODIMP Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
	STDMETHODIMP GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
	STDMETHODIMP GetDuration(REFERENCE_TIME *dur);
	double		GetStartTime();
	CAudioPin* GetAudioPin();
	CDeMultiplexer& GetDemultiplexer();
	void Seek(CRefTime& seekTime);
private:
	double UpdateDuration();
	CAudioPin*	m_pAudioPin;;
	CVideoPin*	m_pVideoPin;
	WCHAR m_fileName[1024];
	CCritSec m_section;
	MultiFileReader m_fileReader;
	CPcrDecoder m_pcrDecoder;
	CDeMultiplexer m_demultiplexer;
	double m_endTime;
	double m_startTime;
};

