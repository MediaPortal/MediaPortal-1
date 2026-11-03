#include "stdafx.h"
#include <d3d9.h>
#include <memory.h>
#include "..\subpic\ISubPic.h"
#include "..\subpic\DX9SubPic.h"
#include "..\subpic\SubPicQueueImpl.h"
#include <moreuuids.h>
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
SIZE g_textureSize = MakeSize(800, 600);
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
		m_pSubPicQueue = new CSubPicQueue(SubPicQueueSettings(g_subPicsBufferAhead, 2560, 1440, g_disableAnim, 50, 100, true), m_pAllocator.Get(), &hr);
	else
		m_pSubPicQueue = new CSubPicQueueNoThread(SubPicQueueSettings(0, 2560, 1440, g_disableAnim, 50, 100, true), m_pAllocator.Get(), &hr);
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
			m_d3DDev.Reset();
		}

		if (m_pAllocator)
		{
			m_pAllocator.Reset();
			m_pAllocator.Detach();
			m_pAllocator = nullptr;
		}

		if (m_pSubPicQueue)
		{
			m_pSubPicQueue.Reset();
			m_pSubPicQueue.Detach();
			m_pSubPicQueue = nullptr;
		}
		return;
	}

	// Release ressource
	if (m_d3DDev)
	{
		m_d3DDev.Reset();
	}

	m_d3DDev = d3DDev;

	if (m_pAllocator)
	{
		m_pAllocator.Reset();
		m_pAllocator.Detach();
		m_pAllocator = nullptr;
	}

	if (m_pSubPicQueue)
	{
		m_pSubPicQueue.Reset();
		m_pSubPicQueue.Detach();
		m_pSubPicQueue = nullptr;
	}

	m_pAllocator = new CDX9SubPicAllocator(d3DDev, g_textureSize, false);
	HRESULT hr = S_OK;

	if (g_subPicsBufferAhead > 0)
		m_pSubPicQueue = new CSubPicQueue(SubPicQueueSettings(g_subPicsBufferAhead, 2560, 1440, g_disableAnim, 50, 100, true), m_pAllocator.Get(), &hr);
	else
		m_pSubPicQueue = new CSubPicQueueNoThread(SubPicQueueSettings(0, 2560, 1440, g_disableAnim, 50, 100, true), m_pAllocator.Get(), &hr);

	if (FAILED(hr))
	{
		ATLTRACE("CSubPicQueue creation error: %x", hr);
	}
}

void CSubManager::ApplyStyle(CRenderedTextSubtitle* pRTS) {
	if (g_overrideUserStyles)
	{
		if (pRTS->m_styles.size() > 1)
		{ //remove all styles besides Default
			tstring def = _T("Default");
			STSStyle* defStyle;
			if (Lookup(pRTS->m_styles, def, defStyle))
			{
				pRTS->m_styles.erase(def);
			}
			pRTS->m_styles.Free();
			pRTS->m_styles[def] = defStyle;
		}
		//m_playRes defined by PlayResX, PlayResY params of ass
		//need to set to MPC default which is 384x288 (CSimpleTextSubtitle::Open)
		pRTS->m_playRes = MakeSize(384, 288);
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
			CVobSubFile* pVSF = (CVobSubFile*)m_pSubStream.Get();
			pVSF->m_bOnlyShowForcedSubs = onlyShowForcedSubs;
		}
	}
}

void CSubManager::SetSubPicProvider(ISubStream* pSubStream)
{
	m_pSubStream = pSubStream;
	m_pSubPicQueue->SetSubPicProvider(ComQuery<ISubPicProvider>(pSubStream).Get());
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
	if (m_pSubStream.Get() == pSubStream && m_iSubtitleSel >= 0)
	{
		ATLTRACE("InvalidateSubtitle!");
		if (m_pSubPicQueue)
			m_pSubPicQueue->Invalidate(rtInvalidate);
	}
}

bool CSubManager::SelectStream(int i)
{
	if (i >= 0 && i < (int)m_intSubs.size())
	{
		int index = m_intSubs[i];
		m_isIntSubStreamSelected = true;
		SetSubPicProvider(m_intSubStream.Get());
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

	for (const ComPtr<ISubStream>& pSubStream : m_pSubStreams)
	{
		if (i < 0)
		{
			break;
		}
		if (i < pSubStream->GetStreamCount())
		{
			CAutoLock cAutoLock(&m_csSubLock);
			m_isIntSubStreamSelected = false;
			pSubStream->SetStream(i);
			SetSubPicProvider(pSubStream.Get());
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
	for (const ComPtr<ISubStream>& pSubStream : m_pSubStreams) cnt += pSubStream->GetStreamCount();
	return cnt;
}

int CSubManager::GetCount()
{
	return GetExtCount() + m_intSubs.size();
}

BSTR CSubManager::GetLanguageHelper(int i, bool useTrackName)
{
	for (const ComPtr<ISubStream>& pSubStream : m_pSubStreams)
	{
		if (i < 0)
		{
			break;
		}

		if (i < pSubStream->GetStreamCount())
		{
			WCHAR* pName = NULL;
			if (SUCCEEDED(pSubStream->GetStreamInfo(i, &pName, NULL)))
			{
				BSTR res = SysAllocString(pName);
				CoTaskMemFree(pName);
				return res;
			}
			return 0;
		}
		i -= pSubStream->GetStreamCount();
	}

	return (i >= 0 && i < (int)m_intNames.size()) ?
		(useTrackName ? StringAllocSys(m_intTrackNames[i]) : StringAllocSys(m_intNames[i])) : NULL;
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

	SIZE size = MakeSize(width, height);
	if (m_lastSize != size && width > 0 && height > 0)
	{ //adjust texture size
		ATLTRACE("Size change from %dx%d to %dx%d", m_lastSize.cx, m_lastSize.cy, size.cx, size.cy);
		m_pAllocator->ChangeDevice(m_d3DDev.Get());
		//m_pAllocator->SetMaxTextureSize(g_textureSize);
		m_pAllocator->SetCurSize(size);
		m_pAllocator->SetCurVidRect(MakeRect(MakePoint(0, 0), size));
		m_pSubPicQueue->Invalidate(m_rtNow + 1000000);
		m_lastSize = size;
	}

	RECT windowRect, videoRect;
	windowRect = MakeRect(MakePoint(0, 0), size);
	videoRect = (windowRect);

	ComPtr<ISubPic> pSubPic;
	if (m_pSubPicQueue->LookupSubPic(m_rtNow, pSubPic))
	{
		RECT rcSource, rcDest;
		if (SUCCEEDED(pSubPic->GetSourceAndDest(windowRect, videoRect, &rcSource, &rcDest, 1.0, xOffsetInPixels, 0))) {
			//ATLTRACE("m_rtNow %d", (long)(m_rtNow/10000000));
			//ATLTRACE("src: (%d,%d) - (%d,%d)", rcSource.left, rcSource.top, rcSource.right, rcSource.bottom);
			//ATLTRACE("dst: (%d,%d) - (%d,%d)\n", rcDest.left, rcDest.top, rcDest.right, rcDest.bottom);
			::OffsetRect(&rcDest, x, y);
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

			pSubPic->AlphaBlt(&rcSource, &rcDest, NULL/*pTarget*/);

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

void AlphaBlt(const RECT& windowRect, const RECT& videoRect, ISubPic* pSubPic, SubPicDesc* pTarget, int xOffsetInPixels/* = 0*/, int yOffsetInPixels/* = 0*/)
{
	RECT rcSource, rcDest;
	if (SUCCEEDED(pSubPic->GetSourceAndDest(windowRect, videoRect, &rcSource, &rcDest, 1.0, xOffsetInPixels, yOffsetInPixels))) {
		pSubPic->AlphaBlt(&rcSource, &rcDest, pTarget);
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

	RECT windowRect, videoRect;
	windowRect = viewportRect;
	videoRect = croppedVideoRect;

	int width = croppedVideoRect.right - croppedVideoRect.left;
	int height = croppedVideoRect.bottom - croppedVideoRect.top;
	int x = croppedVideoRect.right + croppedVideoRect.left;
	int y = croppedVideoRect.bottom + croppedVideoRect.top;

	SIZE size = MakeSize(width, height);
	if (m_lastSize != size && width > 0 && height > 0)
	{ //adjust texture size
		ATLTRACE("Size change from %dx%d to %dx%d", m_lastSize.cx, m_lastSize.cy, size.cx, size.cy);
		m_pAllocator->ChangeDevice(m_d3DDev.Get());
		//m_pAllocator->SetMaxTextureSize(g_textureSize);
		m_pAllocator->SetCurSize(size);
		m_pAllocator->SetCurVidRect(MakeRect(MakePoint(0, 0), size));
		m_pSubPicQueue->Invalidate(m_rtNow + 1000000);
		m_lastSize = size;
	}

	ComPtr<ISubPic> pSubPic;
	if (m_pSubPicQueue->LookupSubPic(m_rtNow, pSubPic))
	{
		RECT rcWindow = windowRect;
		RECT rcVideo = videoRect;

		pSubPic->SetRelativeTo(posRelativeToFrame ? RelativeTo::VIDEO : RelativeTo::WINDOW);

		if (m_iSubpicStereoMode == SUBPIC_STEREO_SIDEBYSIDE)
		{
			RECT rcTempWindow = windowRect;
			rcTempWindow.right -= RectWidth(rcTempWindow) / 2;
			RECT rcTempVideo = videoRect;
			rcTempVideo.right -= RectWidth(rcTempVideo) / 2;

			xOffsetInPixels = -DefaultStereoOffsetInPixels;
			DWORD fvf, alphaTest, colorOp, samplerAddressU, samplerAddressV;
			m_d3DDev->GetFVF(&fvf);
			m_d3DDev->GetRenderState(D3DRS_ALPHATESTENABLE, &alphaTest);
			m_d3DDev->GetTextureStageState(0, D3DTSS_COLOROP, &colorOp); //change to it causes "white" osd artifact  
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSU, &samplerAddressU);
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSV, &samplerAddressV);

			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, FALSE);

			AlphaBlt(rcTempWindow, rcTempVideo, pSubPic.Get(), NULL, xOffsetInPixels, 0);

			m_d3DDev->SetFVF(fvf);
			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, alphaTest);
			m_d3DDev->SetTextureStageState(0, D3DTSS_COLOROP, colorOp);

			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSU, samplerAddressU);
			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSV, samplerAddressV);

			rcWindow.left += RectWidth(rcWindow) / 2;
			rcVideo.left += RectWidth(rcVideo) / 2;
		}
		else if (m_iSubpicStereoMode == SUBPIC_STEREO_TOPANDBOTTOM)
		{
			RECT rcTempWindow = windowRect;
			rcTempWindow.bottom -= RectHeight(rcTempWindow) / 2;
			RECT rcTempVideo = videoRect;
			rcTempVideo.bottom -= RectHeight(rcTempVideo) / 2;

			xOffsetInPixels = -DefaultStereoOffsetInPixels;

			DWORD fvf, alphaTest, colorOp, samplerAddressU, samplerAddressV;
			m_d3DDev->GetFVF(&fvf);
			m_d3DDev->GetRenderState(D3DRS_ALPHATESTENABLE, &alphaTest);
			m_d3DDev->GetTextureStageState(0, D3DTSS_COLOROP, &colorOp); //change to it causes "white" osd artifact  
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSU, &samplerAddressU);
			m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSV, &samplerAddressV);

			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, FALSE);

			AlphaBlt(rcTempWindow, rcTempVideo, pSubPic.Get(), NULL, xOffsetInPixels, 0);

			m_d3DDev->SetFVF(fvf);
			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, alphaTest);
			m_d3DDev->SetTextureStageState(0, D3DTSS_COLOROP, colorOp);

			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSU, samplerAddressU);
			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSV, samplerAddressV);

			rcWindow.top += RectHeight(rcWindow) / 2;
			rcVideo.top += RectHeight(rcVideo) / 2;
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

			AlphaBlt(rcWindow, rcVideo, pSubPic.Get(), NULL, xOffsetInPixels, 0);

			m_d3DDev->SetFVF(fvf);
			m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, alphaTest);
			m_d3DDev->SetTextureStageState(0, D3DTSS_COLOROP, colorOp);

			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSU, samplerAddressU);
			m_d3DDev->SetSamplerState(0, D3DSAMP_ADDRESSV, samplerAddressV);
		}
		//RECT rcSource, rcDest;
		//if (SUCCEEDED(pSubPic->GetSourceAndDest(&size, rcSource, rcDest, xOffsetInPixels)))
		//{
		//  //ATLTRACE("m_rtNow %d", (long)(m_rtNow/10000000));
		//  //ATLTRACE("src: (%d,%d) - (%d,%d)", rcSource.left, rcSource.top, rcSource.right, rcSource.bottom);
		//  //ATLTRACE("dst: (%d,%d) - (%d,%d)\n", rcDest.left, rcDest.top, rcDest.right, rcDest.bottom);
		//  ::OffsetRect(&rcDest, x, y);
		//  DWORD fvf, alphaTest, colorOp, samplerAddressU, samplerAddressV;
		//  m_d3DDev->GetFVF(&fvf);
		//  m_d3DDev->GetRenderState(D3DRS_ALPHATESTENABLE, &alphaTest);
		//  m_d3DDev->GetTextureStageState(0, D3DTSS_COLOROP, &colorOp); //change to it causes "white" osd artifact  
		//  m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSU, &samplerAddressU);
		//  m_d3DDev->GetSamplerState(0, D3DSAMP_ADDRESSV, &samplerAddressV);

		//  m_d3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, FALSE, posRelativeToFrame);

		//  pSubPic->AlphaBlt(&rcSource, &rcDest, NULL/*pTarget*/);

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
		ATLTRACE(L"Processing filter: %s", GetFilterName(pBF.Get()).c_str());
		if (!IsSplitter(pBF.Get())) continue;
		ATLTRACE("Is splitter!");
		if (m_pSS && ComQuery<IAMStreamSelect>(pBF))
		{
			ATLTRACE("Skipping - IAMStreamSelect filter already set!");
			continue;
		}

		auto pTrackInfo = ComQuery<ITrackInfo>(pBF);
		int index = -1;

		BeginEnumPins(pBF, pEP, pPin)
		{
			PIN_DIRECTION pindir;
			pPin->QueryDirection(&pindir);
			if (pindir != PINDIR_OUTPUT)
				continue;
			index++;
			ComPtr<IPin> pPinTo;
			pPin->ConnectedTo(&pPinTo);
			if (pPinTo)
			{
				if (!isTextConnection(pPin.Get()))
					continue;
				pGB->Disconnect(pPin.Get());
				pGB->Disconnect(pPinTo.Get());
			}
			else if (!IsTextPin(pPin.Get()))
				continue;

			auto pTPTF = ComQuery<IBaseFilter>(static_cast<IBaseFilter*>(new CTextPassThruFilter(this)));
			std::wstring name;
			name = ::Format(L"TextPassThru%08x", pTPTF.Get());
			if (FAILED(pGB->AddFilter(pTPTF.Get(), name.c_str())))
				continue;

			ComPtr<ISubStream> pSubStream;
			HRESULT hr;
			do
			{
				if (FAILED(hr = pGB->ConnectDirect(pPin.Get(), GetFirstPin(pTPTF.Get(), PINDIR_INPUT), NULL)))
				{
					ATLTRACE("connection to TextPassThruFilter failed: %x", hr);
					break;
				}
				auto pNTR = ComQuery<IBaseFilter>(static_cast<IBaseFilter*>(new CNullTextRenderer(NULL, &hr)));
				if (FAILED(hr) || FAILED(pGB->AddFilter(pNTR.Get(), NULL)))
					break;

				if FAILED(hr = pGB->ConnectDirect(GetFirstPin(pTPTF.Get(), PINDIR_OUTPUT), GetFirstPin(pNTR.Get(), PINDIR_INPUT), NULL))
					break;
				pSubStream = ComQuery<ISubStream>(pTPTF);
			} while (0);

			if (pSubStream)
			{
				ApplyStyleSubStream(pSubStream.Get());
				if (!m_pSS && (m_pSS = ComQuery<IAMStreamSelect>(pBF)))
				{
					InitInternalSubs(pBF.Get(), lcidci);
					m_intSubStream = pSubStream;
				}
				else
				{
					m_pSubStreams.push_back(pSubStream);
					if (pTrackInfo) {
						TrackElement trackElement;
						memset(&trackElement, 0, sizeof(trackElement));
						pTrackInfo->GetTrackInfo(index, &trackElement);
						if (trackElement.FlagForced) {
							m_forcedSubIndex = m_pSubStreams.size() - 1;
							ATLTRACE("subtitle track %d is forced", index);
						}
					}
				}
			}
			else
			{
				ATLTRACE("TextPassThruFilter removed");
				pGB->RemoveFilter(pTPTF.Get());
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
		auto pTrackInfo = ComQuery<ITrackInfo>(pBF);
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
				tstring lang, langForced, track(pszName);
				if (lcid == 0)
				{
					lang = pszName;
					if (StringFind(lang, L"No ") >= 0)
						lang.clear();
				}
				else
				{
					int len = GetLocaleInfo(lcid, LOCALE_SENGLANGUAGE, StringBuffer(lang, 64), 64);
					StringRelease(lang, std::max(len - 1, 0));
				}
				if (!lang.empty())
				{
					ATLTRACE(L"InitInternalSubs: %d, %s", i, lang.c_str());
					m_intSubs.push_back(i);
					m_intNames.push_back(lang);
					m_intTrackNames.push_back(track);
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
							if (m_forcedSubIndex <= -1 && StringFind(langForced, L"Forced Subtitles (auto)") >= 0)
							{
								m_forcedSubIndex = m_intSubs.size() - 1;
							}
							else if (((lcid == lcidci) || (lcid == lcidciTrad)) && StringFind(langForced, L"Forced Subtitles (auto)") <= 0)
							{
								m_forcedSubIndex = m_intSubs.size() - 1;
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
	std::vector<tstring> paths;

	tstring allpaths = subpaths;
	tstring path;
	int start = 0;
	int prev = 0;

	while (start != -1)
	{
		start = StringFind(allpaths, ',', start);
		if (start > 0)
		{
			path = StringMid(allpaths, prev, start);
			paths.push_back(path);
			int end = StringFind(allpaths, ',', start + 1);
			if (end > start)
			{
				path = StringMid(allpaths, start + 1, end - start - 1);
				paths.push_back(path);
				prev = StringFind(allpaths, ',', end + 1);
				if (prev > 0)
				{
					start++;
					prev = start;
				}
				else
				{
					path = StringRight(allpaths, int(allpaths.size()) - end - 1);
					paths.push_back(path);
					start = -1;
				}
			}
			else
			{
				path = StringRight(allpaths, int(allpaths.size()) - start - 1);
				paths.push_back(path);
				start = -1;
			}
		}
		else if (int(allpaths.size()) > 0)
		{
			paths.push_back(allpaths);
			start = -1;
		}
	}

	if (paths.size() <= 0)
	{
		paths.push_back(_T("."));
		paths.push_back(_T(".\\subtitles"));
		paths.push_back(_T("c:\\subtitles"));
	}

	std::vector<Subtitle::SubFile> ret;
	Subtitle::GetSubFileNames(m_movieFile, paths, ret);

	for (size_t i = 0; i < ret.size(); i++)
	{
		// TMP: maybe this will catch something for those who get a runtime error dialog when opening subtitles from cds
		try
		{
			ComPtr<ISubStream> pSubStream;

			//if(!pSubStream)
			{
				std::unique_ptr<CVobSubFile> pVSF(new CVobSubFile(&m_csSubLock));
				if (StringLowered(tstring(PathFindExtension(ret[i].fn.c_str()))) == _T(".idx") && pVSF && pVSF->Open(ret[i].fn) && pVSF->GetStreamCount() > 0) {
					pVSF->m_bOnlyShowForcedSubs = onlyShowForcedSubs;
					pSubStream = pVSF.release();
				}
			}

			if (!pSubStream)
			{
				std::unique_ptr<CRenderedTextSubtitle> pRTS(new CRenderedTextSubtitle(&m_csSubLock));
				if (StringLowered(tstring(PathFindExtension(ret[i].fn.c_str()))) != _T(".sub"))
				{
					if (pRTS && pRTS->Open(ret[i].fn, DEFAULT_CHARSET, _T(""), m_movieFile) && pRTS->GetStreamCount() > 0) {
						ApplyStyle(pRTS.get());
						pSubStream = pRTS.release();
					}
				}
			}
			if (pSubStream)
			{
				m_pSubStreams.push_back(pSubStream);
			}
		}
		catch (const std::exception&)
		{
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
		m_subresync.SaveToDisk(m_pSubStream.Get(), m_fps, m_movieFile);
	}
}

void CSubManager::LoadSubtitlesForFile(const wchar_t* fn, IGraphBuilder* pGB, const wchar_t* paths, LCID lcidci)
{
	{//hook vmr
		ComPtr<IBaseFilter> vmr;
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
		ComPtr<IPin> pPin = GetFirstPin(vmr.Get());

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

		auto pMemInputPin = ComQuery<IMemInputPin>(pPin);
		HookNewSegmentAndReceive((IPinC*)(IPin*)pPin.Get(), (IMemInputPinC*)(IMemInputPin*)pMemInputPin.Get());
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