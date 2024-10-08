#include "stdafx.h"
#include <d3d9.h>
#include <memory.h>
#include "..\subpic\ISubPic.h"
#include "..\subpic\DX9SubPic.h"
#include "..\subpic\SubPicQueueImpl.h"
#include <moreuuids.h>
#include <atlpath.h>
#include "..\subtitles\VobSubFile.h"
#include "..\subtitles\RTS.h"
#include "..\DSUtil\NullRenderers.h"
#include "SubManager.h"
#include "TextPassThruFilter.h"
#include "IPinHook.h"
#include "ITrackInfo.h"
#include "../DSUtil/WinAPIUtils.h"
#include "../filters/renderer/VideoRenderers/RenderersSettings.h"

STSStyle g_style;
BOOL g_overrideUserStyles;
int g_subPicsBufferAhead(3);
CSize g_textureSize(800, 600);
bool g_pow2tex(true);
BOOL g_disableAnim(TRUE);
BOOL g_onlyShowForcedSubs;
CRenderersSettings g_rendSet;

CSubManager::CSubManager(IDirect3DDevice9* d3DDev, SIZE size, HRESULT& hr)
	: m_d3DDev(d3DDev),
	m_isSetTime(false),
	m_forcedSubIndex(-1),
	m_iSubtitleSel(-1),
	m_enabled(false),
	m_isIntSubStreamSelected(false),
	m_rtNow(-1),
	m_delay(0),
	m_lastSize(size),
	m_bIsMadVR(false)
{
	ATLTRACE("CSubManager constructor: texture size %dx%d, buffer ahead: %d", g_textureSize.cx, g_textureSize.cy, g_subPicsBufferAhead);
	m_pAllocator = new CDX9SubPicAllocator(d3DDev, g_textureSize, false);
	hr = S_OK;
	if (g_subPicsBufferAhead > 0)
		m_pSubPicQueue = new CSubPicQueue(SubPicQueueSettings(g_subPicsBufferAhead, 2560, 1440, g_disableAnim, 50, 100, true), m_pAllocator, &hr);
	else
		m_pSubPicQueue = new CSubPicQueueNoThread(SubPicQueueSettings(10, 2560, 1440, g_disableAnim, 50, 100, true), m_pAllocator, &hr);
	if (FAILED(hr))
	{
		ATLTRACE("CSubPicQueue creation error: %x", hr);
	}
}

CSubManager::~CSubManager(void)
{
	ATLTRACE("CSubManager destructor");
}

void CSubManager::SetDevice(IDirect3DDevice9* d3DDev)
{
	if (!d3DDev)
	{
		// Release ressource
		if (m_d3DDev)
		{
			m_d3DDev.Release();
		}

		if (m_pAllocator)
		{
			m_pAllocator.Release();
			m_pAllocator.Detach();
			m_pAllocator = nullptr;
		}

		if (m_pSubPicQueue)
		{
			m_pSubPicQueue.Release();
			m_pSubPicQueue.Detach();
			m_pSubPicQueue = nullptr;
		}
		return;
	}

	// Release ressource
	if (m_d3DDev)
	{
		m_d3DDev.Release();
	}

	m_d3DDev = d3DDev;

	if (m_pAllocator)
	{
		m_pAllocator.Release();
		m_pAllocator.Detach();
		m_pAllocator = nullptr;
	}

	if (m_pSubPicQueue)
	{
		m_pSubPicQueue.Release();
		m_pSubPicQueue.Detach();
		m_pSubPicQueue = nullptr;
	}

	m_pAllocator = new CDX9SubPicAllocator(d3DDev, g_textureSize, false);
	HRESULT hr = S_OK;

	if (g_subPicsBufferAhead > 0)
		m_pSubPicQueue = new CSubPicQueue(SubPicQueueSettings(g_subPicsBufferAhead, 2560, 1440, g_disableAnim, 50, 100, true), m_pAllocator, &hr);
	else
		m_pSubPicQueue = new CSubPicQueueNoThread(SubPicQueueSettings(10, 2560, 1440, g_disableAnim, 50, 100, true), m_pAllocator, &hr);

	if (FAILED(hr))
	{
		ATLTRACE("CSubPicQueue creation error: %x", hr);
	}
}

void CSubManager::ApplyStyle(CRenderedTextSubtitle* pRTS) {
	if (g_overrideUserStyles)
	{
		if (pRTS->m_styles.GetCount() > 1)
		{ //remove all styles besides Default
			CString def = _T("Default");
			STSStyle* defStyle;
			if (pRTS->m_styles.Lookup(def, defStyle))
			{
				pRTS->m_styles.RemoveKey(def);
			}
			pRTS->m_styles.Free();
			pRTS->m_styles[def] = defStyle;
		}
		//m_playRes defined by PlayResX, PlayResY params of ass
		//need to set to MPC default which is 384x288 (CSimpleTextSubtitle::Open)
		pRTS->m_playRes = CSize(384, 288);
		pRTS->SetDefaultStyle(g_style);
	}
	else if (pRTS->m_bUsingPlayerDefaultStyle)
	{
		pRTS->SetDefaultStyle(g_style);
	}
	pRTS->Deinit();
}

void CSubManager::ApplyStyleSubStream(ISubStream* pSubStream)
{
	CLSID clsid;
	if (FAILED(pSubStream->GetClassID(&clsid)))
		return;
	if (clsid == __uuidof(CRenderedTextSubtitle))
	{
		CRenderedTextSubtitle* pRTS = (CRenderedTextSubtitle*)pSubStream;
		ApplyStyle(pRTS);
	}
}

void CSubManager::ToggleForcedOnly(bool onlyShowForcedSubs)
{
	CAutoLock cAutoLock(&m_csSubLock);
	if (m_pSubStream) {
		CLSID clsid;
		if (FAILED(m_pSubStream->GetClassID(&clsid)))
			return;
		if (clsid == __uuidof(CVobSubFile))
		{
			CVobSubFile* pVSF = (CVobSubFile*)m_pSubStream.p;
			pVSF->m_bOnlyShowForcedSubs = onlyShowForcedSubs;
		}
	}
}

void CSubManager::SetSubPicProvider(ISubStream* pSubStream)
{
	m_pSubStream = pSubStream;
	m_pSubPicQueue->SetSubPicProvider(CComQIPtr<ISubPicProvider>(pSubStream));
	m_delay = 0;
	m_subresync.RemoveAll();
}

void CSubManager::ReplaceSubtitle(ISubStream* pSubStreamOld, ISubStream* pSubStreamNew)
{
	ApplyStyleSubStream(pSubStreamNew);
	m_intSubStream = pSubStreamNew;
	if (m_isIntSubStreamSelected)
	{
		SetSubPicProvider(pSubStreamNew);
	}
}

void CSubManager::InvalidateSubtitle(ISubStream* pSubStream, REFERENCE_TIME rtInvalidate)
{
	if (m_pSubStream == pSubStream && m_iSubtitleSel >= 0)
	{
		ATLTRACE("InvalidateSubtitle!");
		if (m_pSubPicQueue)
			m_pSubPicQueue->Invalidate(rtInvalidate);
	}
}

bool CSubManager::SelectStream(int i)
{
	if (i >= 0 && i < (int)m_intSubs.GetCount())
	{
		int index = m_intSubs[i];
		m_isIntSubStreamSelected = true;
		SetSubPicProvider(m_intSubStream);
		HRESULT hr = m_pSS->Enable(index, AMSTREAMSELECTENABLE_ENABLE);
		ATLTRACE("SelectStream %d, result: %s", i, hr == S_OK ? "success" : "failure");
		return hr == S_OK;
	}
	return false;
}

void CSubManager::UpdateSubtitle()
{
	ATLTRACE("UpdateSubtitle");
	int i = m_iSubtitleSel;

	POSITION pos = m_pSubStreams.GetHeadPosition();
	while (pos && i >= 0)
	{
		CComPtr<ISubStream> pSubStream = m_pSubStreams.GetNext(pos);
		if (i < pSubStream->GetStreamCount())
		{
			CAutoLock cAutoLock(&m_csSubLock);
			m_isIntSubStreamSelected = false;
			pSubStream->SetStream(i);
			SetSubPicProvider(pSubStream);
			return;
		}
		i -= pSubStream->GetStreamCount();
	}

	if (!SelectStream(i))
	{
		m_pSubPicQueue->SetSubPicProvider(NULL);
		m_isIntSubStreamSelected = false;
	}
}

int CSubManager::GetExtCount()
{
	int cnt = 0;
	POSITION pos = m_pSubStreams.GetHeadPosition();
	while (pos) cnt += m_pSubStreams.GetNext(pos)->GetStreamCount();
	return cnt;
}

int CSubManager::GetCount()
{
	return GetExtCount() + m_intSubs.GetCount();
}

BSTR CSubManager::GetLanguageHelper(int i, bool useTrackName)
{
	POSITION pos = m_pSubStreams.GetHeadPosition();
	while (pos && i >= 0)
	{
		CComPtr<ISubStream> pSubStream = m_pSubStreams.GetNext(pos);

		if (i < pSubStream->GetStreamCount())
		{
			WCHAR* pName = NULL;
			if (SUCCEEDED(pSubStream->GetStreamInfo(i, &pName, NULL)))
			{
				CComBSTR res(pName);
				CoTaskMemFree(pName);
				return res.Detach();
			}
			return 0;
		}
		i -= pSubStream->GetStreamCount();
	}

	return (i >= 0 && i < (int)m_intNames.GetCount()) ?
		(useTrackName ? m_intTrackNames[i].AllocSysString() : m_intNames[i].AllocSysString()) : NULL;
}

BSTR CSubManager::GetLanguage(int i)
{
	return GetLanguageHelper(i, false);
}

BSTR CSubManager::GetTrackName(int i)
{
	return GetLanguageHelper(i, true);
}

int CSubManager::GetCurrent()
{
	return m_iSubtitleSel;
}

void CSubManager::SetCurrent(int current)
{
	if (m_pAllocator && m_pSubPicQueue)
	{
		m_iSubtitleSel = current;
		UpdateSubtitle();
	}
}

void CSubManager::SetCurrent3DSubtitle(int current)
{
	if (m_pAllocator && m_pSubPicQueue)
	{
		m_iSubpicStereoMode = current;
	}
}

void CSubManager::SetEnable(BOOL enabled)
{
	m_enabled = enabled != 0;
}

BOOL CSubManager::GetEnable()
{
	return m_enabled ? TRUE : FALSE;
}

void CSubManager::SetTime(REFERENCE_TIME nsSampleTime)
{
	if (m_bIsMadVR)
		m_rtNow = nsSampleTime - m_delay;
	else
		m_rtNow = g_tSegmentStart + nsSampleTime - m_delay;

	m_pSubPicQueue->SetTime(m_rtNow);
	m_isSetTime = true;
}

void CSubManager::Render(int x, int y, int width, int height, int xOffsetInPixels)
{
	if (!m_enabled)
		return;

	if (!m_isSetTime)
	{
		if (m_bIsMadVR)
			m_rtNow = g_tSampleStart - m_delay;
		else
			m_rtNow = g_tSegmentStart + g_tSampleStart - m_delay;

		m_pSubPicQueue->SetTime(m_rtNow);
	}

	CSize size(width, height);
	if (m_lastSize != size && width > 0 && height > 0)
	{ //adjust texture size
		ATLTRACE("Size change from %dx%d to %dx%d", m_lastSize.cx, m_lastSize.cy, size.cx, size.cy);
		m_pAllocator->ChangeDevice(m_d3DDev);
		//m_pAllocator->SetMaxTextureSize(g_textureSize);
		m_pAllocator->SetCurSize(size);
		m_pAllocator->SetCurVidRect(CRect(CPoint(0, 0), size));
		m_pSubPicQueue->Invalidate(m_rtNow + 1000000);
		m_lastSize = size;
	}

	CRect windowRect, videoRect;
	windowRect = CRect(CPoint(0, 0), size);
	videoRect = CRect(windowRect);

	CComPtr<ISubPic> pSubPic;
	if (m_pSubPicQueue->LookupSubPic(m_rtNow, pSubPic))
	{
		CRect rcSource, rcDest;
		if (SUCCEEDED(pSubPic->GetSourceAndDest(windowRect, videoRect, rcSource, rcDest, 1.0, xOffsetInPixels, 0))) {
			//ATLTRACE("m_rtNow %d", (long)(m_rtNow/10000000));
			//ATLTRACE("src: (%d,%d) - (%d,%d)", rcSource.left, rcSource.top, rcSource.right, rcSource.bottom);
			//ATLTRACE("dst: (%d,%d) - (%d,%d)\n", rcDest.left, rcDest.top, rcDest.right, rcDest.bottom);
			rcDest.OffsetRect(x, y);
			DWORD fvf, alphaTest, colorOp, samplerAddressU, samplerAddressV, samplerFtMag, samplerFtMin, samplerFtMip;
			m_d3DDev->GetFVF(&fvf);
			m_d3DDev->GetRenderState(D3DRS_ALPHATESTENABLE, &alphaTest);
			m_d3DDev->GetTextureStageState(0, D3DTSS_COLOROP, &colorOp); //change to it causes "white" osd artifact  
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSU, &samplerAddressU);
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSV, &samplerAddressV);

			m_d3DDev->GetSamplerState(0, D3DSAMP_MAGFILTER, &samplerFtMag);
			m_d3DDev->GetSamplerState(0, D3DSAMP_MINFILTER, &samplerFtMin);
			m_d3DDev->GetSamplerState(0, D3DSAMP_MIPFILTER, &samplerFtMip);

			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, FALSE);

			pSubPic->AlphaBlt(rcSource, rcDest, NULL/*pTarget*/);

			m_d3DDev->SetFVF(fvf);
			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, alphaTest);
			m_d3DDev->SetTextureStageState(0, D3DTSS_COLOROP, colorOp);

			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSU, samplerAddressU);
			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSV, samplerAddressV);

			m_d3DDev->SetSamplerState(0, D3DSAMP_MAGFILTER, samplerFtMag);
			m_d3DDev->SetSamplerState(0, D3DSAMP_MINFILTER, samplerFtMin);
			m_d3DDev->SetSamplerState(0, D3DSAMP_MIPFILTER, samplerFtMip);
		}
	}
}

void AlphaBlt(const CRect& windowRect, const CRect& videoRect, ISubPic* pSubPic, SubPicDesc* pTarget, int xOffsetInPixels/* = 0*/, int yOffsetInPixels/* = 0*/)
{
	CRect rcSource, rcDest;
	if (SUCCEEDED(pSubPic->GetSourceAndDest(windowRect, videoRect, rcSource, rcDest, 1.0, xOffsetInPixels, yOffsetInPixels))) {
		pSubPic->AlphaBlt(rcSource, rcDest, pTarget);
	}
}

void CSubManager::RenderEx(RECT viewportRect, RECT croppedVideoRect, int xOffsetInPixels, bool posRelativeToFrame)
{
	if (!m_enabled)
		return;

	if (!m_isSetTime)
	{
		if (m_bIsMadVR)
			m_rtNow = g_tSampleStart - m_delay;
		else
			m_rtNow = g_tSegmentStart + g_tSampleStart - m_delay;

		m_pSubPicQueue->SetTime(m_rtNow);
	}

	CRect windowRect, videoRect;
	windowRect = viewportRect;
	videoRect = croppedVideoRect;

	int width = croppedVideoRect.right - croppedVideoRect.left;
	int height = croppedVideoRect.bottom - croppedVideoRect.top;
	int x = croppedVideoRect.right + croppedVideoRect.left;
	int y = croppedVideoRect.bottom + croppedVideoRect.top;

	CSize size(width, height);
	if (m_lastSize != size && width > 0 && height > 0)
	{ //adjust texture size
		ATLTRACE("Size change from %dx%d to %dx%d", m_lastSize.cx, m_lastSize.cy, size.cx, size.cy);
		m_pAllocator->ChangeDevice(m_d3DDev);
		//m_pAllocator->SetMaxTextureSize(g_textureSize);
		m_pAllocator->SetCurSize(size);
		m_pAllocator->SetCurVidRect(CRect(CPoint(0, 0), size));
		m_pSubPicQueue->Invalidate(m_rtNow + 1000000);
		m_lastSize = size;
	}

	CComPtr<ISubPic> pSubPic;
	if (m_pSubPicQueue->LookupSubPic(m_rtNow, pSubPic))
	{
		CRect rcWindow(windowRect);
		CRect rcVideo(videoRect);

		pSubPic->SetRelativeTo(posRelativeToFrame ? RelativeTo::VIDEO : RelativeTo::WINDOW);

		if (m_iSubpicStereoMode == SUBPIC_STEREO_SIDEBYSIDE)
		{
			CRect rcTempWindow(windowRect);
			rcTempWindow.right -= rcTempWindow.Width() / 2;
			CRect rcTempVideo(videoRect);
			rcTempVideo.right -= rcTempVideo.Width() / 2;

			xOffsetInPixels = -DefaultStereoOffsetInPixels;
			DWORD fvf, alphaTest, colorOp, samplerAddressU, samplerAddressV;
			m_d3DDev->GetFVF(&fvf);
			m_d3DDev->GetRenderState(D3DRS_ALPHATESTENABLE, &alphaTest);
			m_d3DDev->GetTextureStageState(0, D3DTSS_COLOROP, &colorOp); //change to it causes "white" osd artifact  
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSU, &samplerAddressU);
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSV, &samplerAddressV);

			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, FALSE);

			AlphaBlt(rcTempWindow, rcTempVideo, pSubPic, NULL, xOffsetInPixels, 0);

			m_d3DDev->SetFVF(fvf);
			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, alphaTest);
			m_d3DDev->SetTextureStageState(0, D3DTSS_COLOROP, colorOp);

			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSU, samplerAddressU);
			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSV, samplerAddressV);

			rcWindow.left += rcWindow.Width() / 2;
			rcVideo.left += rcVideo.Width() / 2;
		}
		else if (m_iSubpicStereoMode == SUBPIC_STEREO_TOPANDBOTTOM)
		{
			CRect rcTempWindow(windowRect);
			rcTempWindow.bottom -= rcTempWindow.Height() / 2;
			CRect rcTempVideo(videoRect);
			rcTempVideo.bottom -= rcTempVideo.Height() / 2;

			xOffsetInPixels = -DefaultStereoOffsetInPixels;

			DWORD fvf, alphaTest, colorOp, samplerAddressU, samplerAddressV;
			m_d3DDev->GetFVF(&fvf);
			m_d3DDev->GetRenderState(D3DRS_ALPHATESTENABLE, &alphaTest);
			m_d3DDev->GetTextureStageState(0, D3DTSS_COLOROP, &colorOp); //change to it causes "white" osd artifact  
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSU, &samplerAddressU);
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSV, &samplerAddressV);

			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, FALSE);

			AlphaBlt(rcTempWindow, rcTempVideo, pSubPic, NULL, xOffsetInPixels, 0);

			m_d3DDev->SetFVF(fvf);
			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, alphaTest);
			m_d3DDev->SetTextureStageState(0, D3DTSS_COLOROP, colorOp);

			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSU, samplerAddressU);
			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSV, samplerAddressV);

			rcWindow.top += rcWindow.Height() / 2;
			rcVideo.top += rcVideo.Height() / 2;
		}
		{
			xOffsetInPixels = DefaultStereoOffsetInPixels;

			DWORD fvf, alphaTest, colorOp, samplerAddressU, samplerAddressV;
			m_d3DDev->GetFVF(&fvf);
			m_d3DDev->GetRenderState(D3DRS_ALPHATESTENABLE, &alphaTest);
			m_d3DDev->GetTextureStageState(0, D3DTSS_COLOROP, &colorOp); //change to it causes "white" osd artifact  
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSU, &samplerAddressU);
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSV, &samplerAddressV);

			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, FALSE);

			AlphaBlt(rcWindow, rcVideo, pSubPic, NULL, xOffsetInPixels, 0);

			m_d3DDev->SetFVF(fvf);
			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, alphaTest);
			m_d3DDev->SetTextureStageState(0, D3DTSS_COLOROP, colorOp);

			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSU, samplerAddressU);
			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSV, samplerAddressV);
		}
		//CRect rcSource, rcDest;
		//if (SUCCEEDED(pSubPic->GetSourceAndDest(&size, rcSource, rcDest, xOffsetInPixels)))
		//{
		//  //ATLTRACE("m_rtNow %d", (long)(m_rtNow/10000000));
		//  //ATLTRACE("src: (%d,%d) - (%d,%d)", rcSource.left, rcSource.top, rcSource.right, rcSource.bottom);
		//  //ATLTRACE("dst: (%d,%d) - (%d,%d)\n", rcDest.left, rcDest.top, rcDest.right, rcDest.bottom);
		//  rcDest.OffsetRect(x, y);
		//  DWORD fvf, alphaTest, colorOp, samplerAddressU, samplerAddressV;
		//  m_d3DDev->GetFVF(&fvf);
		//  m_d3DDev->GetRenderState(D3DRS_ALPHATESTENABLE, &alphaTest);
		//  m_d3DDev->GetTextureStageState(0, D3DTSS_COLOROP, &colorOp); //change to it causes "white" osd artifact  
		//  m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSU, &samplerAddressU);
		//  m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSV, &samplerAddressV);

		//  m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, FALSE, posRelativeToFrame);

		//  pSubPic->AlphaBlt(rcSource, rcDest, NULL/*pTarget*/);

		//  m_d3DDev->SetFVF(fvf);
		//  m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, alphaTest);
		//  m_d3DDev->SetTextureStageState(0, D3DTSS_COLOROP, colorOp);

		//  m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSU, samplerAddressU);
		//  m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSV, samplerAddressV);
		//}
	}
}

static bool IsTextPin(IPin* pPin)
{
	bool isText = false;
	BeginEnumMediaTypes(pPin, pEMT, pmt)
	{
		if (pmt->majortype == MEDIATYPE_Text || pmt->majortype == MEDIATYPE_Subtitle)
		{
			isText = true;
			break;
		}
	}
	EndEnumMediaTypes(pmt)
		return isText;
}

static bool isTextConnection(IPin* pPin)
{
	AM_MEDIA_TYPE mt;
	if (FAILED(pPin->ConnectionMediaType(&mt)))
		return false;
	bool isText = (mt.majortype == MEDIATYPE_Text || mt.majortype == MEDIATYPE_Subtitle);
	FreeMediaType(mt);
	return isText;
}

//load internal subtitles through TextPassThruFilter
void CSubManager::LoadInternalSubtitles(IGraphBuilder* pGB, bool onlyShowForcedSubs, LCID lcidci)
{
	BeginEnumFilters(pGB, pEF, pBF)
	{
		ATLTRACE(L"Processing filter: %s", GetFilterName(pBF).GetString());
		if (!IsSplitter(pBF)) continue;
		ATLTRACE("Is splitter!");
		if (m_pSS != 0 && CComQIPtr<IAMStreamSelect>(pBF) != 0)
		{
			ATLTRACE("Skipping - IAMStreamSelect filter already set!");
			continue;
		}

		CComQIPtr<ITrackInfo> pTrackInfo(pBF);
		int index = -1;

		BeginEnumPins(pBF, pEP, pPin)
		{
			PIN_DIRECTION pindir;
			pPin->QueryDirection(&pindir);
			if (pindir != PINDIR_OUTPUT)
				continue;
			index++;
			CComPtr<IPin> pPinTo;
			pPin->ConnectedTo(&pPinTo);
			if (pPinTo)
			{
				if (!isTextConnection(pPin))
					continue;
				pGB->Disconnect(pPin);
				pGB->Disconnect(pPinTo);
			}
			else if (!IsTextPin(pPin))
				continue;

			CComQIPtr<IBaseFilter> pTPTF = new CTextPassThruFilter(this);
			CStringW name;
			name.Format(L"TextPassThru%08x", pTPTF);
			if (FAILED(pGB->AddFilter(pTPTF, name)))
				continue;

			CComQIPtr<ISubStream> pSubStream;
			HRESULT hr;
			do
			{
				if (FAILED(hr = pGB->ConnectDirect(pPin, GetFirstPin(pTPTF, PINDIR_INPUT), NULL)))
				{
					ATLTRACE("connection to TextPassThruFilter failed: %x", hr);
					break;
				}
				CComQIPtr<IBaseFilter> pNTR = new CNullTextRenderer(NULL, &hr);
				if (FAILED(hr) || FAILED(pGB->AddFilter(pNTR, NULL)))
					break;

				if FAILED(hr = pGB->ConnectDirect(GetFirstPin(pTPTF, PINDIR_OUTPUT), GetFirstPin(pNTR, PINDIR_INPUT), NULL))
					break;
				pSubStream = pTPTF;
			} while (0);

			if (pSubStream)
			{
				ApplyStyleSubStream(pSubStream);
				if (m_pSS == 0 && ((m_pSS = pBF) != 0))
				{
					InitInternalSubs(pBF, lcidci);
					m_intSubStream = pSubStream;
				}
				else
				{
					m_pSubStreams.AddTail(pSubStream);
					if (pTrackInfo) {
						TrackElement trackElement;
						memset(&trackElement, 0, sizeof(trackElement));
						pTrackInfo->GetTrackInfo(index, &trackElement);
						if (trackElement.FlagForced) {
							m_forcedSubIndex = m_pSubStreams.GetCount() - 1;
							ATLTRACE("subtitle track %d is forced", index);
						}
					}
				}
			}
			else
			{
				ATLTRACE("TextPassThruFilter removed");
				pGB->RemoveFilter(pTPTF);
			}
		}
		EndEnumPins
	}
	EndEnumFilters
}

void CSubManager::InitInternalSubs(IBaseFilter* pBF, LCID lcidci)
{
	if (!m_pSS) return;
	DWORD cStreams = 0;
	if (SUCCEEDED(m_pSS->Count(&cStreams)))
	{
		CComQIPtr<ITrackInfo> pTrackInfo(pBF);
		for (int i = 0; i < (int)cStreams; i++)
		{
			DWORD dwFlags = 0;
			LCID lcid = 0;
			DWORD dwGroup = 0;
			WCHAR* pszName = NULL;

			if (FAILED(m_pSS->Info(i, NULL, &dwFlags, &lcid, &dwGroup, &pszName, NULL, NULL)))
				continue;

			if (dwGroup == 2)
			{
				CString lang, langForced, track(pszName);
				if (lcid == 0)
				{
					lang = pszName;
					if (lang.Find(L"No ") >= 0)
						lang.Empty();
				}
				else
				{
					int len = GetLocaleInfo(lcid, LOCALE_SENGLANGUAGE, lang.GetBuffer(64), 64);
					lang.ReleaseBufferSetLength(std::max(len - 1, 0));
				}
				if (!lang.IsEmpty())
				{
					ATLTRACE(L"InitInternalSubs: %d, %s", i, lang);
					m_intSubs.Add(i);
					m_intNames.Add(lang);
					m_intTrackNames.Add(track);
					if (pTrackInfo)
					{
						TrackElement trackElement;
						memset(&trackElement, 0, sizeof(trackElement));
						int lcidciTrad = -1;
						int LCIDSpaInt = 3082;
						int LCIDSpaTrad = 1034;
						// Try to compare LCID for spanish international (Modern Sort) and traditional (Traditional Sort)
						if (lcidci == LCIDSpaInt)
						{
							lcidciTrad = LCIDSpaTrad;
						}
						pTrackInfo->GetTrackInfo(i, &trackElement);
						if (trackElement.FlagForced) {
							langForced = pszName;
							if (m_forcedSubIndex <= -1 && langForced.Find(L"Forced Subtitles (auto)") >= 0)
							{
								m_forcedSubIndex = m_intSubs.GetCount() - 1;
							}
							else if (((lcid == lcidci) || (lcid == lcidciTrad)) && langForced.Find(L"Forced Subtitles (auto)") <= 0)
							{
								m_forcedSubIndex = m_intSubs.GetCount() - 1;
								ATLTRACE("subtitle track %d is forced", i);
							}
						}
					}
				}
			}

			if (pszName) CoTaskMemFree(pszName);
		}
	}


}

void CSubManager::LoadExternalSubtitles(const wchar_t* filename, const wchar_t* subpaths, bool onlyShowForcedSubs)
{
	m_movieFile = filename;
	CAtlArray<CString> paths;

	CString allpaths = subpaths;
	CString path;
	int start = 0;
	int prev = 0;

	while (start != -1)
	{
		start = allpaths.Find(',', start);
		if (start > 0)
		{
			path = allpaths.Mid(prev, start);
			paths.Add(path);
			int end = allpaths.Find(',', start + 1);
			if (end > start)
			{
				path = allpaths.Mid(start + 1, end - start - 1);
				paths.Add(path);
				prev = allpaths.Find(',', end + 1);
				if (prev > 0)
				{
					start++;
					prev = start;
				}
				else
				{
					path = allpaths.Right(allpaths.GetLength() - end - 1);
					paths.Add(path);
					start = -1;
				}
			}
			else
			{
				path = allpaths.Right(allpaths.GetLength() - start - 1);
				paths.Add(path);
				start = -1;
			}
		}
		else if (allpaths.GetLength() > 0)
		{
			paths.Add(allpaths);
			start = -1;
		}
	}

	if (paths.GetCount() <= 0)
	{
		paths.Add(_T("."));
		paths.Add(_T(".\\subtitles"));
		paths.Add(_T("c:\\subtitles"));
	}

	CAtlArray<Subtitle::SubFile> ret;
	Subtitle::GetSubFileNames(m_movieFile, paths, ret);

	for (size_t i = 0; i < ret.GetCount(); i++)
	{
		// TMP: maybe this will catch something for those who get a runtime error dialog when opening subtitles from cds
		try
		{
			CComPtr<ISubStream> pSubStream;

			//if(!pSubStream)
			{
				CAutoPtr<CVobSubFile> pVSF(new CVobSubFile(&m_csSubLock));
				if (CString(CPath(ret[i].fn).GetExtension()).MakeLower() == _T(".idx") && pVSF && pVSF->Open(ret[i].fn) && pVSF->GetStreamCount() > 0) {
					pVSF->m_bOnlyShowForcedSubs = onlyShowForcedSubs;
					pSubStream = pVSF.Detach();
				}
			}

			if (!pSubStream)
			{
				CAutoPtr<CRenderedTextSubtitle> pRTS(new CRenderedTextSubtitle(&m_csSubLock));
				if (CString(CPath(ret[i].fn).GetExtension()).MakeLower() != _T(".sub"))
				{
					if (pRTS && pRTS->Open(ret[i].fn, DEFAULT_CHARSET, _T(""), m_movieFile) && pRTS->GetStreamCount() > 0) {
						ApplyStyle(pRTS);
						pSubStream = pRTS.Detach();
					}
				}
			}
			if (pSubStream)
			{
				m_pSubStreams.AddTail(pSubStream);
			}
		}
		catch (CException* e)
		{
			e->Delete();
		}
	}
}

int CSubManager::GetDelay()
{
	return (int)(m_delay / 10000);
}

void CSubManager::SetDelay(int delay_ms)
{
	REFERENCE_TIME rtDelay = delay_ms * 10000;
	m_subresync.AddShift(m_rtNow + m_delay, rtDelay - m_delay);
	m_delay = rtDelay;
}

void CSubManager::SaveToDisk()
{
	if (m_iSubtitleSel >= 0)
	{
		m_subresync.SaveToDisk(m_pSubStream, m_fps, m_movieFile);
	}
}

void CSubManager::LoadSubtitlesForFile(const wchar_t* fn, IGraphBuilder* pGB, const wchar_t* paths, LCID lcidci)
{
	{//hook vmr
		CComPtr<IBaseFilter> vmr;
		pGB->FindFilterByName(L"Enhanced Video Renderer", &vmr);
		if (!vmr)
		{
			pGB->FindFilterByName(L"Video Mixing Renderer 9", &vmr);
		}
		if (!vmr)
		{
			pGB->FindFilterByName(L"madVR", &vmr);
			m_bIsMadVR = true;
		}
		if (!vmr)
		{
			ATLTRACE(L"Failed to load subtitles: could not find video renderer");
			return;
		}
		CComPtr<IPin> pPin = GetFirstPin(vmr);

		//set fps
		REFERENCE_TIME rtTimePerFrame(0);
		if (pPin) {
			CMediaType mt;
			if (SUCCEEDED(pPin->ConnectionMediaType(&mt)))
			{
				ExtractAvgTimePerFrame(&mt, rtTimePerFrame);
				ATLTRACE(L"ExtractAvgTimePerFrame: %lu", rtTimePerFrame);
			}
		}
		// If framerate not set by Video Decoder choose 23.97...
		if (rtTimePerFrame == 0) rtTimePerFrame = 417166;
		m_fps = 10000000.0 / rtTimePerFrame;
		m_pSubPicQueue->SetFPS(m_fps);

		CComQIPtr<IMemInputPin> pMemInputPin = pPin;
		HookNewSegmentAndReceive((IPinC*)(IPin*)pPin, (IMemInputPinC*)(IMemInputPin*)pMemInputPin);
	}
	bool onlyShowForcedSubs = g_onlyShowForcedSubs ? true : false;
	LoadInternalSubtitles(pGB, onlyShowForcedSubs, lcidci);
	LoadExternalSubtitles(fn, paths, onlyShowForcedSubs);
	if (GetCount() > 0)
	{
		if (onlyShowForcedSubs && m_forcedSubIndex >= 0) {
			//if IAMStreamSelect available, m_forcedSubIndex is index of internal subs
			//(they are counted after external subs)
			SetCurrent(m_pSS != NULL ? GetExtCount() + m_forcedSubIndex : m_forcedSubIndex);
			m_enabled = true;
		}
		else {
			SetCurrent(0); //stream 0, disabled
		}
	}
}


CRenderersSettings& GetRenderersSettings()
{
	return g_rendSet;
}

void CRenderersSettings::CAdvRendererSettings::SetDefault()
{
	bVMR9AlterativeVSync = false;
	iVMR9VSyncOffset = 0;
	bVMR9VSyncAccurate = false;
	bVMR9FullscreenGUISupport = false;
	bVMR9VSync = false;
	bVMR9FullFloatingPointProcessing = false;
	bVMR9HalfFloatingPointProcessing = false;
	bVMR9ColorManagementEnable = false;
	iVMR9ColorManagementInput = VIDEO_SYSTEM_UNKNOWN;
	iVMR9ColorManagementAmbientLight = AMBIENT_LIGHT_BRIGHT;
	iVMR9ColorManagementIntent = COLOR_RENDERING_INTENT_PERCEPTUAL;
	bVMRDisableDesktopComposition = false;
	bVMRFlushGPUBeforeVSync = true;
	bVMRFlushGPUAfterPresent = true;
	bVMRFlushGPUWait = false;
	bEVRHighColorResolution = false;
	bEVRForceInputHighColorResolution = false;
	bEVREnableFrameTimeCorrection = false;
	iEVROutputRange = 0;
	bSynchronizeVideo = false;
	bSynchronizeDisplay = false;
	bSynchronizeNearest = true;
	iLineDelta = 0;
	iColumnDelta = 0;
	fCycleDelta = 0.0012;
	fTargetSyncOffset = 12.0;
	fControlLimit = 2.0;
	bCacheShaders = false;
	bDesktopSizeBackBuffer = false;
}

STSStyle GetAppDefaultStyle()
{
	return g_style;
}

size_t RegexUtil::wstringMatch(const std::wregex& pattern, const std::wstring& text, wregexResults& results) {
	results.clear();

	std::wstring data(text);
	std::wsmatch match_pieces;
	while (std::regex_search(data, match_pieces, pattern)) {
		wregexResult result;
		for (auto match = match_pieces.begin(); match != match_pieces.end(); ++match) {
			if (match != match_pieces.begin()) {
				result.push_back((*match).str());
			}
		}
		results.push_back(result);
		data = match_pieces.suffix().str();
	}
	return results.size();
}