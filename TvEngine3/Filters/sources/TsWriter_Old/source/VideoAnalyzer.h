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
#include "VideoAudioScrambledAnalyzer.h"

#pragma once
DEFINE_GUID(IID_ITSVideoAnalyzer,0x59f8d617, 0x92fd, 0x48d5, 0x8f, 0x6d, 0xa9, 0x7b, 0xfd, 0x95, 0xc4, 0x48);

// video anayzer interface
DECLARE_INTERFACE_(ITsVideoAnalyzer, IUnknown)
{
	STDMETHOD(SetVideoPid)(THIS_ int videoPid)PURE;
	STDMETHOD(GetVideoPid)(THIS_ int* videoPid)PURE;
	
	STDMETHOD(SetAudioPid)(THIS_ int audioPid)PURE;
	STDMETHOD(GetAudioPid)(THIS_ int* audioPid)PURE;
	
	STDMETHOD(IsVideoEncrypted)(THIS_ int* yesNo)PURE;
	STDMETHOD(IsAudioEncrypted)(THIS_ int* yesNo)PURE;

	STDMETHOD(Reset)(THIS_)PURE;
};

class CVideoAnalyzer: public CUnknown, public ITsVideoAnalyzer
{
public:
	CVideoAnalyzer(LPUNKNOWN pUnk, HRESULT *phr);
	~CVideoAnalyzer(void);

  DECLARE_IUNKNOWN

	STDMETHODIMP SetVideoPid( int videoPid);
	STDMETHODIMP GetVideoPid( int* videoPid);
	
	STDMETHODIMP SetAudioPid( int audioPid);
	STDMETHODIMP GetAudioPid( int* audioPid);
	
	STDMETHODIMP IsVideoEncrypted( int* yesNo);
	STDMETHODIMP IsAudioEncrypted( int* yesNo);
	STDMETHODIMP Reset();
	void OnTsPacket(byte* tsPacket);
protected:
	CVideoAudioScrambledAnalyzer m_videoAudioAnalyzer;
};
