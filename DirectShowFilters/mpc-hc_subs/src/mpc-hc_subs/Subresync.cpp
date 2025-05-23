#include "stdafx.h"
#include "..\subpic\ISubPic.h"
#include "..\subtitles\VobSubFile.h"
#include "..\subtitles\RTS.h"

#include "Subresync.h"

CSubresync::CSubresync(void)
{
}

CSubresync::~CSubresync(void)
{
}

void CSubresync::AddShift(REFERENCE_TIME time, REFERENCE_TIME val)
{
	m_delayTime.Add(time);
	m_delayVal.Add(val);
}

void CSubresync::RemoveAll()
{
	m_delayTime.RemoveAll();
	m_delayVal.RemoveAll();
}

void CSubresync::SetSubtitle(ISubStream* pSubStream, double fps)
{
	m_mode = NONE;
	m_sts.Empty();

	if(!pSubStream) return;

	CLSID clsid;
	pSubStream->GetClassID(&clsid);

	if(clsid == __uuidof(CVobSubFile))
	{
		CVobSubFile* pVSF = (CVobSubFile*)(ISubStream*)pSubStream;

		m_mode = VOBSUB;

		ASSERT(pVSF->m_nLang >= 0);
		CAtlArray<CVobSubFile::SubPos>& sp = pVSF->m_langs[pVSF->m_nLang].subpos;

		for(int i = 0, j = sp.GetCount(); i < j; i++)
		{
			CString str;
			str.Format(_T("%d,%d,%d,%d"), sp[i].vobid, sp[i].cellid, sp[i].bForced, i);
			m_sts.Add(TToW(str), false, sp[i].start, sp[i].stop);
		}

		m_sts.CreateDefaultStyle(DEFAULT_CHARSET);

		pVSF->m_bOnlyShowForcedSubs = false;
	}
	else if(clsid == __uuidof(CRenderedTextSubtitle))
	{
		CRenderedTextSubtitle* pRTS = (CRenderedTextSubtitle*)(ISubStream*)pSubStream;

		m_mode = TEXTSUB;

		m_sts.Copy(*pRTS);
		m_sts.ConvertToTimeBased(fps);
		m_sts.Sort(true); /*!!m_fUnlink*/
	}

}


int CSubresync::FindNearestSub(REFERENCE_TIME rtPos)
{
	if (m_sts.GetCount() == 0) return -1;

	REFERENCE_TIME rtCurTime = rtPos - 10000;

	if (rtCurTime < m_sts[0].start)
	{
		return 0;
	}

	for(int i = 1, j = m_sts.GetCount(); i < j; i++)
	{
		if ((rtCurTime >= m_sts[i-1].start) && (rtCurTime < m_sts[i].start))
		{
			return i-1;
		}
	}

	return m_sts.GetCount() - 1;
}


void CSubresync::ShiftSubtitle(int nItem, REFERENCE_TIME rtValue)
{
	while (nItem > 0 && (m_sts[nItem-1].end > m_sts[nItem].start + rtValue))
		--nItem;

	for (size_t i = nItem; i<m_sts.GetCount(); i++)
	{
		m_sts[i].start += rtValue;
		m_sts[i].end   += rtValue;
	}
}

bool CSubresync::SaveToDisk(ISubStream* pSubStream, double fps, const CString & movieName)
{
	if (m_delayTime.IsEmpty())
		return false;
	SetSubtitle(pSubStream, fps);
	for (size_t i=0; i<m_delayTime.GetCount(); i++)
	{
		int nItem = FindNearestSub(m_delayTime[i]);
		if (nItem >= 0)	ShiftSubtitle(nItem, m_delayVal[i]);
	}

	m_sts.CreateSegments();

	m_sts.m_bUsingPlayerDefaultStyle = true; //prevent .style file
	if (m_sts.m_path.IsEmpty())
	{//for embedded subtitles
		int k = movieName.ReverseFind('.');
		m_sts.m_path = (k < 0 ? movieName : movieName.Left(k));
		if (!m_sts.m_name.IsEmpty())
			m_sts.m_path += L"." + m_sts.m_name;
		m_sts.m_subtitleType = (m_mode == VOBSUB ? Subtitle::IDX : Subtitle::SRT);
	}
	return m_sts.SaveAs(m_sts.m_path, m_sts.m_subtitleType);
}

