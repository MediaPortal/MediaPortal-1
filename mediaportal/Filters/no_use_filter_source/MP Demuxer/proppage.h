
#ifndef __PROPPAGE_
#define __PROPPAGE_
#pragma warning(disable: 4511 4512 4995)
#include "stdafx.h"
#include "TSMuxSplitter.h"
#include "Demuxer.h"

// clsid for the proppage
// 
// {B3221EE2-6BF8-45e4-A513-3D606497E706}
class TSMuxSplitter;

class DECLSPEC_UUID("B3221EE2-6BF8-45e4-A513-3D606497E706")
MPDSTProperties : public CBasePropertyPage
{
public:

    static CUnknown * WINAPI CreateInstance(LPUNKNOWN lpunk, HRESULT *phr);
    DECLARE_IUNKNOWN;

private:

    BOOL OnReceiveMessage(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam);

    HRESULT OnConnect(IUnknown *pUnknown);
    HRESULT OnDisconnect();
    HRESULT OnActivate();
    HRESULT OnApplyChanges();

    void SetDirty();
	void ReadValues();

    MPDSTProperties(LPUNKNOWN lpunk, HRESULT *phr);

 
    int         m_nIndex ;       // Index of the selected media type
    IPin        *m_pPin ;        // The upstream output pin connected to us

	// video elemensts
	HWND	m_videoWidth;
	HWND	m_videoHeigth;
	HWND	m_videoAR;
	HWND	m_videoBitrate;

	// audio elements
	HWND	m_audioBitRate;
	HWND	m_audioSampFreq;
	HWND	m_audioChannels;
	HWND	m_audioAC3Check;

	// pins
	HWND	m_audioPid;
	HWND	m_videoPid;

	//
	HWND	m_picButton;
	HWND	m_picture;



	TSMuxSplitter		*m_pFilter;
	Demux::VideoHeader	m_videoHeader;
	Demux::AudioHeader	m_audioHeader;


};  // class MPDSTProperties

#endif