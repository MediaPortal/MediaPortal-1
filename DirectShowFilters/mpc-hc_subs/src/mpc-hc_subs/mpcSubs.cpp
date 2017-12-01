#include "stdafx.h"
#include <d3d9.h>
#include "mpcSubs.h"
#include "..\subtitles\RTS.h"
#include "SubManager.h"

static CAutoPtr<CSubManager> g_subManager;

// SetDevice stores device pointer here in case the sub manager is not yet instantiated
static IDirect3DDevice9* g_d3DDev;

void SetDefaultStyle(const SubtitleStyle* s, BOOL overrideUserStyles)
{
	g_style.fontName = s->fontName;
	g_style.colors[0] = s->fontColor;
	g_style.fontWeight = (s->fontIsBold ? 700 : 400);
	g_style.fontSize = s->fontSize; 
	g_style.charSet = s->fontCharset;
	g_style.shadowDepthX  = g_style.shadowDepthY = s->shadow;
	g_style.outlineWidthX = g_style.outlineWidthY = s->borderWidth;
	g_style.borderStyle = (s->isBorderOutline ? 0 : 1); // 0: outline, 1: opaque box
	g_overrideUserStyles = overrideUserStyles;
}

void SetAdvancedOptions(int subPicsBufferAhead, SIZE textureSize, BOOL pow2tex, BOOL disableAnim)
{
	g_subPicsBufferAhead = subPicsBufferAhead;
	g_textureSize = textureSize;
	g_pow2tex = pow2tex != 0;
	g_disableAnim = disableAnim;
}

void SetShowForcedOnly(BOOL onlyShowForcedSubs)
{
	if (g_subManager)	{
	 g_subManager->ToggleForcedOnly(onlyShowForcedSubs != 0);
	}
	else {
	 g_onlyShowForcedSubs = onlyShowForcedSubs;
	}
}

BOOL SetDevice(IDirect3DDevice9* d3DDev)
{
  if (g_subManager)
  {
    g_subManager->SetDevice(d3DDev);

    if (d3DDev)
      g_subManager->SetCurrent(g_subManager->GetCurrent());
  }

  if (d3DDev)
    g_d3DDev = d3DDev;

  return TRUE;
}

BOOL LoadSubtitles(IDirect3DDevice9* d3DDev, SIZE size, const wchar_t* fn, IGraphBuilder* pGB, const wchar_t* paths, LCID lcidci)
{
	g_subManager.Free();
	HRESULT hr = S_OK;
  CAutoPtr<CSubManager> subManager(new CSubManager(g_d3DDev ? g_d3DDev : d3DDev, size, hr));
	if (FAILED(hr))
	{
		return FALSE;
	}
	subManager->LoadSubtitlesForFile(fn, pGB, paths, lcidci);
	g_subManager.Attach(subManager.Detach());
	return TRUE;
}

void SetTime(REFERENCE_TIME nsSampleTime)
{
	if (g_subManager)
	{
		g_subManager->SetTime(nsSampleTime);
	}
}

void Render(int x, int y, int width, int height, int xOffsetInPixels)
{
	if (g_subManager)
	{
		g_subManager->Render(x, y, width, height, xOffsetInPixels);
	}
}

void RenderEx(RECT viewportRect, RECT croppedVideoRect, int xOffsetInPixels, bool posRelativeToFrame)
{
  if (g_subManager)
  {
    g_subManager->RenderEx(viewportRect, croppedVideoRect, xOffsetInPixels, posRelativeToFrame);
  }
}

int GetCount()
{
	return (g_subManager ? g_subManager->GetCount() : 0);
}

BSTR GetLanguage(int i)
{
	return (g_subManager ? g_subManager->GetLanguage(i) : NULL);
}

BSTR GetTrackName(int i)
{
	return (g_subManager ? g_subManager->GetTrackName(i) : NULL);
}

int GetCurrent()
{
	return (g_subManager ? g_subManager->GetCurrent() : -1);
}

void SetCurrent(int current)
{
	if (g_subManager)
		g_subManager->SetCurrent(current);
}

void SetCurrent3DSubtitle(int current)
{
  if (g_subManager)
    g_subManager->SetCurrent3DSubtitle(current);
}

BOOL GetEnable()
{
	return (g_subManager ? g_subManager->GetEnable() : FALSE);
}

void SetEnable(BOOL enable)
{
	if (g_subManager)
		g_subManager->SetEnable(enable);
}

int GetDelay()
{
	return (g_subManager ? g_subManager->GetDelay() : 0);
}

void SetDelay(int delay)
{
	if (g_subManager)
		g_subManager->SetDelay(delay);
}

void SaveToDisk()
{
	if (g_subManager)
		g_subManager->SaveToDisk();
}

BOOL IsModified()
{
	return (g_subManager ? (g_subManager->IsModified() ? TRUE : FALSE) : FALSE);
}

void FreeSubtitles()
{
  try
  {
    g_d3DDev = NULL;
    if (g_subManager)
    {
      g_subManager->SetDevice(nullptr);
      g_subManager.Free();
    }
  }
  catch (...)
  {}
}
