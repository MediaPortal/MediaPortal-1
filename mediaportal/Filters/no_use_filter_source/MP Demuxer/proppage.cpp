/* 
 *	Copyright (C) 2005 Media Portal
 *  Author: Agree
 *	http://mediaportal.sourceforge.net
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

#pragma warning(disable: 4511 4512)
#include <streams.h>
#include <windowsx.h>
#include <commctrl.h>
#include <olectl.h>
#include <memory.h>
#include <stdlib.h>
#include <stdio.h>
#include <tchar.h>

#include "resource.h"    // ids used in the dialog
#include "proppage.h"    // our own class


//
CUnknown * WINAPI MPDSTProperties::CreateInstance(LPUNKNOWN lpunk, HRESULT *phr)
{
    ASSERT(phr);
    
    CUnknown *punk = new MPDSTProperties(lpunk, phr);

    if (punk == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
    }

    return punk;
}


//
// MPDSTProperties::Constructor
//
MPDSTProperties::MPDSTProperties(LPUNKNOWN pUnk, HRESULT *phr)
: CBasePropertyPage(NAME("Splitter Property Page\0"),pUnk, IDD_PROPPAGE_MEDIUM, IDS_TITLE)
    , m_pPin(NULL)
    , m_nIndex(0)
	, m_pFilter(NULL)
	
{
    ASSERT(phr);

} // (constructor) MPDSTProperties



void MPDSTProperties::SetDirty()
{
    if (m_pPageSite)
        m_pPageSite->OnStatusChange(PROPPAGESTATUS_DIRTY);

} // SetDirty



BOOL MPDSTProperties::OnReceiveMessage(HWND hwnd,
                                        UINT uMsg,
                                        WPARAM wParam,
                                        LPARAM lParam)
{
    switch (uMsg)
    {
        case WM_INITDIALOG:
        {
            // video
            m_videoWidth = GetDlgItem (hwnd, IDC_EDIT1 ) ;
            m_videoHeigth = GetDlgItem (hwnd, IDC_EDIT2) ;
            m_videoAR = GetDlgItem (hwnd, IDC_EDIT4) ;
            m_videoBitrate = GetDlgItem (hwnd, IDC_EDIT3) ;
			// audio
			m_audioBitRate=GetDlgItem (hwnd, IDC_EDIT5);
			m_audioSampFreq=GetDlgItem (hwnd, IDC_EDIT6);
			m_audioChannels=GetDlgItem (hwnd, IDC_EDIT7);
			m_audioAC3Check=GetDlgItem (hwnd, IDC_CHECK1);
			// pins
			m_audioPid=GetDlgItem (hwnd, IDC_EDIT8);
			m_videoPid=GetDlgItem (hwnd, IDC_EDIT9);

			//

            ReadValues();

            return (LRESULT) 1;
        }

        case WM_COMMAND:
        {
           DWORD hiparam=HIWORD(wParam);
		   DWORD loparam=LOWORD(wParam);

		   if(loparam==IDC_BUTTON3)
		   {
			   int a=0;
		   }
        }

    }

    return CBasePropertyPage::OnReceiveMessage(hwnd,uMsg,wParam,lParam);

} // OnReceiveMessage

void MPDSTProperties::ReadValues()
{
	
	char buffer[32];
	if(m_pFilter==NULL)
		return;

	m_pFilter->GetVideoHeader(&m_videoHeader);
	m_pFilter->GetAudioHeader(&m_audioHeader);

	if(m_pFilter->GetVideoPID()>0)
	{
		SendMessage( m_videoAR, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
		SendMessage( m_videoAR, EM_REPLACESEL, 0, (LPARAM) m_videoHeader.vAspectRatio ); 

		_i64toa((__int64)m_videoHeader.vWidth,buffer,10);
		SendMessage( m_videoWidth, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
		SendMessage( m_videoWidth, EM_REPLACESEL, 0, (LPARAM) buffer ); 

		_i64toa((__int64)m_videoHeader.vHeight,buffer,10);
		SendMessage( m_videoHeigth, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
		SendMessage( m_videoHeigth, EM_REPLACESEL, 0, (LPARAM) buffer ); 

		_i64toa((__int64)m_videoHeader.vBitrate,buffer,10);
		SendMessage( m_videoBitrate, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
		SendMessage( m_videoBitrate, EM_REPLACESEL, 0, (LPARAM) buffer ); 
		
		_i64toa((__int64)m_pFilter->GetVideoPID(),buffer,10);
		SendMessage( m_videoPid, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
		SendMessage( m_videoPid, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	}
	if(m_pFilter->GetAudioPID()>0)
	{
		_i64toa((__int64)m_audioHeader.SamplingFreq,buffer,10);
		SendMessage( m_audioSampFreq, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
		SendMessage( m_audioSampFreq, EM_REPLACESEL, 0, (LPARAM) buffer ); 

		_i64toa((__int64)m_audioHeader.Channel,buffer,10);
		SendMessage( m_audioChannels, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
		SendMessage( m_audioChannels, EM_REPLACESEL, 0, (LPARAM) buffer ); 
		
		_i64toa((__int64)m_audioHeader.Bitrate,buffer,10);
		SendMessage( m_audioBitRate, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
		SendMessage( m_audioBitRate, EM_REPLACESEL, 0, (LPARAM) buffer ); 

		SendMessage(m_audioAC3Check,BM_SETCHECK,m_pFilter->IsAC3Audio(),0);
		
		_i64toa((__int64)m_pFilter->GetAudioPID(),buffer,10);
		SendMessage( m_audioPid, EM_SETSEL, 0, MAKELONG(-1,-1) ); 
		SendMessage( m_audioPid, EM_REPLACESEL, 0, (LPARAM) buffer ); 
	
	}

}

HRESULT MPDSTProperties::OnConnect(IUnknown *pUnknown)
{

	IBaseFilter *pFilter;
	HRESULT hr=pUnknown->QueryInterface(IID_IBaseFilter,(void**)&pFilter);
	if(hr==S_OK)
	{
		m_pFilter=(TSMuxSplitter*)pFilter;
		pFilter->Release();
	}
	return hr;

} // OnConnect



HRESULT MPDSTProperties::OnDisconnect()
{
	return NOERROR;
} // OnDisconnect



HRESULT MPDSTProperties::OnActivate()
{
    return NOERROR;
} // Activate



HRESULT MPDSTProperties::OnApplyChanges()
{
    
	return NOERROR;

} // OnApplyChanges