/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
#ifndef __MPTsFilter
#define __MPTsFilter
class CMPTSFilter;

#include <objbase.h>
#include "FilterOutPin.h"

#include "FilterAudioOutPin.h"
class CFilterAudioPin;
#include "FilterVideoOutPin.h"
class CFilterVideoPin;

#include "StreamPids.h"
#include "Sections.h"
#include "FileReader.h"
#include "SplitterSetup.h"

#define MAX_PTS		(__int64)0xffffffff
// guids:

// filter
DEFINE_GUID(CLSID_MPTSFilter,0xA3556F1E, 0x787B, 0x12C4, 0x91, 0x00, 0x01, 0xAF, 0x31, 0x3A, 0xC9, 0x00);
// interface
DEFINE_GUID(IID_IMPTSControl,0xA3556F1E, 0x787B, 0x12C4, 0x91, 0x00, 0x01, 0xAF, 0x31, 0x3A, 0xC9, 0x10);

// interface
DECLARE_INTERFACE_(IMPTSControl, IUnknown)
{
	STDMETHOD(Refresh)()PURE;
	STDMETHOD(SetCurrentAudioPid)(THIS_ int audioPid)PURE;
	STDMETHOD(GetCurrentAudioPid)(THIS_ int* audioPid)PURE;
	STDMETHOD(GetAudioPid)(THIS_ int index, int* audioPid, BOOL* isAC3, char** language)PURE;
};

// filter
class CMPTSFilter;

class CMPTSFilter : public CSource,public IFileSourceFilter,public IMPTSControl
{
	//friend class CFilterOutPin;
public:
	DECLARE_IUNKNOWN
	static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

private:
	CMPTSFilter(IUnknown *pUnk, HRESULT *phr);
	~CMPTSFilter();
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

public:
	//CFilterOutPin*	m_pPin;
	CFilterAudioPin*	m_pAudioPin;
	CFilterVideoPin*	m_pVideoPin;

	// Pin enumeration
	CBasePin *			GetPin(int n);
	int					GetPinCount();
	STDMETHODIMP 		Run(REFERENCE_TIME tStart);
	STDMETHODIMP 		Pause();
	//STDMETHODIMP 		SetSyncClock(void);
	STDMETHODIMP 		Stop();
	HRESULT				OnConnect();
	STDMETHODIMP		Refresh();
	HRESULT				RefreshPids();
	HRESULT				RefreshDuration();
	HRESULT				GetFileSize(__int64 *pfilesize);
	HRESULT				SetFilePosition(REFERENCE_TIME seek);
	bool				UpdatePids();
	FileReader*			m_pFileReader;
	REFERENCE_TIME		StreamStartTime();
protected:
	// IFileSourceFilter
	STDMETHODIMP Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
	STDMETHODIMP GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
	STDMETHODIMP GetDuration(REFERENCE_TIME *dur);

	//audio stream selection
	STDMETHODIMP SetCurrentAudioPid(int audioPid);
	STDMETHODIMP GetCurrentAudioPid(int* audioPid);
	STDMETHODIMP GetAudioPid(int index, int* audioPid, BOOL* isAC3, char** language);

protected:
	Sections*		m_pSections;
	SplitterSetup*	m_pDemux;
	CCritSec		m_Lock;
	BOOL			m_setPosition;
	__int64			m_writePos;
};

#endif
