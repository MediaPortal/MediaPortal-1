/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#include "epgparser.h"
#include "mhwparser.h"
#include "criticalsection.h"
#include "entercriticalsection.h"

using namespace Mediaportal;

// {5CDAC655-D9FB-4c71-8119-DD07FE86A9CE}
DEFINE_GUID(IID_ITsEpgScanner, 0x5cdac655, 0xd9fb, 0x4c71, 0x81, 0x19, 0xdd, 0x7, 0xfe, 0x86, 0xa9, 0xce);

// video anayzer interface
DECLARE_INTERFACE_(ITsEpgScanner, IUnknown)
{
	//epg
	STDMETHOD(GrabEPG)(THIS_)PURE;
	STDMETHOD(IsEPGReady) (THIS_ BOOL* yesNo)PURE;
	STDMETHOD(GetEPGChannelCount) (THIS_ ULONG* channelCount)PURE;
	STDMETHOD(GetEPGEventCount) (THIS_ ULONG channel, ULONG* eventCount)PURE;
	STDMETHOD(GetEPGChannel) (THIS_ ULONG channel, WORD* networkId, WORD* transportid,WORD* service_id  )PURE;
	STDMETHOD(GetEPGEvent) (THIS_ ULONG channel, ULONG eventid,ULONG* language,ULONG* dateMJD,ULONG* timeUTC,ULONG* duration,char** genre    )PURE;
	STDMETHOD(GetEPGLanguage) (THIS_ ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription    )PURE;

	//mhw
	STDMETHOD(GrabMHW)()PURE;
	STDMETHOD(IsMHWReady) (THIS_ BOOL* yesNo)PURE;
	STDMETHOD(GetMHWTitleCount)(THIS_ WORD* count)PURE;
	STDMETHOD(GetMHWTitle)(THIS_ WORD program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)PURE;
	STDMETHOD(GetMHWChannel)(THIS_ WORD channelNr, WORD* channelId, WORD* networkId, WORD* transportId, char** channelName)PURE;
	STDMETHOD(GetMHWSummary)(THIS_ WORD programId, char** summary)PURE;
	STDMETHOD(GetMHWTheme)(THIS_ WORD themeId, char** theme)PURE;
	STDMETHOD(Reset)(THIS_)PURE;
};

class CEpgScanner: public CUnknown, public ITsEpgScanner
{
public:
	CEpgScanner(LPUNKNOWN pUnk, HRESULT *phr);
	~CEpgScanner(void);

  DECLARE_IUNKNOWN
//EPG
	STDMETHODIMP GrabEPG();
	STDMETHODIMP IsEPGReady(BOOL* yesNo);
	STDMETHODIMP GetEPGChannelCount( ULONG* channelCount);
	STDMETHODIMP GetEPGEventCount( ULONG channel,  ULONG* eventCount);
	STDMETHODIMP GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  );
	STDMETHODIMP GetEPGEvent( ULONG channel,  ULONG eventid,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre    );
	STDMETHODIMP GetEPGLanguage(THIS_ ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription    );
	//MHW
	STDMETHODIMP GrabMHW();
	STDMETHODIMP IsMHWReady(BOOL* yesNo);

	STDMETHODIMP GetMHWTitleCount(WORD* count);
	STDMETHODIMP GetMHWTitle(WORD program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName);
	STDMETHODIMP GetMHWChannel(WORD channelNr, WORD* channelId, WORD* networkId, WORD* transportId, char** channelName);
	STDMETHODIMP GetMHWSummary(WORD programId, char** summary);
	STDMETHODIMP GetMHWTheme(WORD themeId, char** theme);
	STDMETHODIMP Reset();

	void OnTsPacket(byte* tsPacket);
protected:
	CEpgParser m_epgParser;
	CMhwParser m_mhwParser;
private:
	bool m_bGrabbing;
	CCriticalSection m_section;
};
