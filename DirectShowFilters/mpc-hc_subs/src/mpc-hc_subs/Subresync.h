#pragma once
#include "..\subtitles\STS.h"

class CSubresync
{
public:
	CSubresync(void);
	~CSubresync(void);
	
	void AddShift(REFERENCE_TIME time, REFERENCE_TIME val);
	void RemoveAll();
	bool IsModified() { return !m_delayTime.IsEmpty(); };
	bool SaveToDisk(ISubStream* pSubStream, double fps, const CString & movieName);
private:
	CAtlArray<REFERENCE_TIME> m_delayTime;
	CAtlArray<REFERENCE_TIME> m_delayVal;

	enum {NONE = 0, VOBSUB, TEXTSUB};
	int m_mode;

	CSimpleTextSubtitle m_sts;

	void SetSubtitle(ISubStream* pSubStream, double fps);
	int FindNearestSub(REFERENCE_TIME rtPos);
	void ShiftSubtitle(int nItem, REFERENCE_TIME rtValue);

};
