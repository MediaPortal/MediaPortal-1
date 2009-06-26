#pragma once
#include "Subresync.h"

class CRenderedTextSubtitle;

extern STSStyle g_style;
extern BOOL g_overrideUserStyles;
extern int g_subPicsBufferAhead;
extern CSize g_textureSize;
extern bool g_pow2tex;
extern BOOL g_disableAnim;

class CSubManager
{
public:
	CSubManager(IDirect3DDevice9* d3DDev, SIZE size, HRESULT& hr);
	~CSubManager(void);

	void LoadSubtitlesForFile(const wchar_t* fn, IGraphBuilder* pGB);

	int GetCount();
	BSTR GetLanguage(int i);
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
private:
	friend class CTextPassThruInputPin;
	friend class CTextPassThruFilter;
	//ReplaceSubtitle, InvalidateSubtitle are called from CTextPassThruInputPin
	void ReplaceSubtitle(ISubStream* pSubStreamOld, ISubStream* pSubStreamNew);
	void InvalidateSubtitle(DWORD_PTR nSubtitleId, REFERENCE_TIME rtInvalidate);

	//load internal subtitles through TextPassThruFilter
	void LoadInternalSubtitles(IGraphBuilder* pGB);
	void LoadExternalSubtitles(const wchar_t* fn);

	void UpdateSubtitle();
	void ApplyStyle(CRenderedTextSubtitle* pRTS);
	void ApplyStyleSubStream(ISubStream* pSubStream);
	void SetSubPicProvider(ISubStream* pSubStream);

	void InitInternalSubs(IBaseFilter* pBF);
	void SelectStream(int i);
	int GetExtCount();

	CComPtr<IDirect3DDevice9> m_d3DDev;
	CComQIPtr<ISubPicQueue> m_pSubPicQueue;
	bool m_isSetTime;
	CCritSec m_csSubLock; 
	CInterfaceList<ISubStream> m_pSubStreams; //external subs
	int m_iSubtitleSel; // if(m_iSubtitleSel&(1<<31)): disabled
	REFERENCE_TIME m_rtNow; //current time
	double m_fps;
	REFERENCE_TIME m_delay; 
	CComPtr<ISubStream> m_pSubStream; //current sub stream
	CString m_movieFile;
	CSize m_lastSize;
	CComQIPtr<ISubPicAllocator> m_pAllocator;

	CComQIPtr<IAMStreamSelect> m_pSS; //graph filter with subtitles
	CAtlArray<int> m_intSubs; //internal sub indexes on IAMStreamSelect
	CAtlArray<CString> m_intNames; //internal sub names
	CComQIPtr<ISubStream> m_intSubStream; //current internal sub stream

	CSubresync m_subresync;
};
