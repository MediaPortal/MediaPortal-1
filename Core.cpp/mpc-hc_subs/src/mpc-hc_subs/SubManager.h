#pragma once
#include "Subresync.h"

class CRenderedTextSubtitle;

extern STSStyle g_style;
extern BOOL g_overrideUserStyles;
extern int g_subPicsBufferAhead;
extern CSize g_textureSize;
extern bool g_pow2tex;
extern BOOL g_disableAnim;
extern BOOL g_onlyShowForcedSubs;

class CSubManager
{
public:
	CSubManager(IDirect3DDevice9* d3DDev, SIZE size, HRESULT& hr);
	~CSubManager(void);

	void LoadSubtitlesForFile(const wchar_t* fn, IGraphBuilder* pGB, const wchar_t* paths);

	int GetCount();
	BSTR GetLanguage(int i);
	BSTR GetTrackName(int i);
	int GetCurrent();
	void SetCurrent(int current);
	BOOL GetEnable();
	void SetEnable(BOOL enable);
	void SetTime(REFERENCE_TIME nsSampleTime);
	void Render(int x, int y, int width, int height);
	int GetDelay(); 
	void SetDelay(int delay);
	bool IsModified() { return m_subresync.IsModified(); };
	void SaveToDisk();
	void ToggleForcedOnly(bool onlyShowForcedSubs);
private:
	friend class CTextPassThruInputPin;
	friend class CTextPassThruFilter;
	//ReplaceSubtitle, InvalidateSubtitle are called from CTextPassThruInputPin
	void ReplaceSubtitle(ISubStream* pSubStreamOld, ISubStream* pSubStreamNew);
	void InvalidateSubtitle(ISubStream* pSubStream, REFERENCE_TIME rtInvalidate);

	//load internal subtitles through TextPassThruFilter
	void LoadInternalSubtitles(IGraphBuilder* pGB, bool onlyShowForcedSubs);
	void LoadExternalSubtitles(const wchar_t* fn, const wchar_t* paths, bool onlyShowForcedSubs);

	void UpdateSubtitle();
	void ApplyStyle(CRenderedTextSubtitle* pRTS);
	void ApplyStyleSubStream(ISubStream* pSubStream);
	void SetSubPicProvider(ISubStream* pSubStream);

	void InitInternalSubs(IBaseFilter* pBF);
	bool SelectStream(int i);
	int GetExtCount();
	BSTR GetLanguageHelper(int i, bool useTrackName);

	CComPtr<IDirect3DDevice9> m_d3DDev;
	CComQIPtr<ISubPicQueue> m_pSubPicQueue;
	bool m_isSetTime;
	CCritSec m_csSubLock; 
	
	//list of subs (that are not coming from IAMStreamSelect filter, e.g. external sub files)
	CInterfaceList<ISubStream> m_pSubStreams; 

	int m_forcedSubIndex; //index of forced sub in m_pSubStreams

	int m_iSubtitleSel;
	bool m_enabled;
	REFERENCE_TIME m_rtNow; //current time
	double m_fps;
	REFERENCE_TIME m_delay; 
	CComPtr<ISubStream> m_pSubStream; //current sub stream
	CString m_movieFile;
	CSize m_lastSize;
	CComQIPtr<ISubPicAllocator> m_pAllocator;

	bool m_isIntSubStreamSelected;
	CComQIPtr<IAMStreamSelect> m_pSS; //graph filter with subtitles
	CAtlArray<int> m_intSubs; //internal sub indexes on IAMStreamSelect
	CAtlArray<CString> m_intNames; //internal sub names
	CAtlArray<CString> m_intTrackNames; //internal track names
	CComQIPtr<ISubStream> m_intSubStream; //current internal sub stream

	CSubresync m_subresync;
};
