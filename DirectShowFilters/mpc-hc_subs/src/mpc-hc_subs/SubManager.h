#pragma once
#include "Subresync.h"
#include "../filters/renderer/VideoRenderers/RenderersSettings.h"
#include "..\dsutil\RegexUtil.h"

class CRenderedTextSubtitle;

extern STSStyle g_style;
extern BOOL g_overrideUserStyles;
extern int g_subPicsBufferAhead;
extern SIZE g_textureSize;
extern bool g_pow2tex;
extern BOOL g_disableAnim;
extern BOOL g_onlyShowForcedSubs;
extern CRenderersSettings g_rendSet;

class CSubManager
{
public:
	CSubManager(IDirect3DDevice9* d3DDev, SIZE size, HRESULT& hr);
	~CSubManager(void);

	void LoadSubtitlesForFile(const wchar_t* fn, IGraphBuilder* pGB, const wchar_t* paths, LCID lcidci);

	int GetCount();
	BSTR GetLanguage(int i);
	BSTR GetTrackName(int i);
	int GetCurrent();
	void SetCurrent(int current);
  void SetCurrent3DSubtitle(int current);
	BOOL GetEnable();
	void SetEnable(BOOL enable);
	void SetTime(REFERENCE_TIME nsSampleTime);
	void Render(int x, int y, int width, int height, int xOffsetInPixels);
  void RenderEx(RECT viewportRect, RECT croppedVideoRect, int xOffsetInPixels, bool posRelativeToFrame);
	int GetDelay(); 
	void SetDelay(int delay);
	bool IsModified() { return m_subresync.IsModified(); };
	void SaveToDisk();
	void ToggleForcedOnly(bool onlyShowForcedSubs);
  void SetDevice(IDirect3DDevice9* d3DDev);
private:
	friend class CTextPassThruInputPin;
	friend class CTextPassThruFilter;
	//ReplaceSubtitle, InvalidateSubtitle are called from CTextPassThruInputPin
	void ReplaceSubtitle(ISubStream* pSubStreamOld, ISubStream* pSubStreamNew);
	void InvalidateSubtitle(ISubStream* pSubStream, REFERENCE_TIME rtInvalidate);

	//load internal subtitles through TextPassThruFilter
	void LoadInternalSubtitles(IGraphBuilder* pGB, bool onlyShowForcedSubs, LCID lcidci);
	void LoadExternalSubtitles(const wchar_t* fn, const wchar_t* paths, bool onlyShowForcedSubs);

	void UpdateSubtitle();
	void ApplyStyle(CRenderedTextSubtitle* pRTS);
	void ApplyStyleSubStream(ISubStream* pSubStream);
	void SetSubPicProvider(ISubStream* pSubStream);

	void InitInternalSubs(IBaseFilter* pBF, LCID lcidci);
	bool SelectStream(int i);
	int GetExtCount();
	BSTR GetLanguageHelper(int i, bool useTrackName);

	ComPtr<IDirect3DDevice9> m_d3DDev;
	ComPtr<ISubPicQueue> m_pSubPicQueue;
	bool m_isSetTime;
  bool m_bIsMadVR;
	CCritSec m_csSubLock; 
	
	//list of subs (that are not coming from IAMStreamSelect filter, e.g. external sub files)
	std::list<ComPtr<ISubStream>> m_pSubStreams; 

	int m_forcedSubIndex; //index of forced sub in m_pSubStreams

	int m_iSubtitleSel;
	bool m_enabled;
	REFERENCE_TIME m_rtNow; //current time
	double m_fps;
	REFERENCE_TIME m_delay; 
	ComPtr<ISubStream> m_pSubStream; //current sub stream
	tstring m_movieFile;
	SIZE m_lastSize = {};
	ComPtr<ISubPicAllocator> m_pAllocator;

	bool m_isIntSubStreamSelected;
	ComPtr<IAMStreamSelect> m_pSS; //graph filter with subtitles
	std::vector<int> m_intSubs; //internal sub indexes on IAMStreamSelect
	std::vector<tstring> m_intNames; //internal sub names
	std::vector<tstring> m_intTrackNames; //internal track names
	ComPtr<ISubStream> m_intSubStream; //current internal sub stream
  int m_iSubpicStereoMode;

	CSubresync m_subresync;

  enum {
    SUBPIC_STEREO_NONE = 0,
    SUBPIC_STEREO_SIDEBYSIDE,
    SUBPIC_STEREO_TOPANDBOTTOM,
  };

#define DefaultStereoOffsetInPixels 4
};
