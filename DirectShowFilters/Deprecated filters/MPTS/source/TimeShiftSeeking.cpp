/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#include <streams.h>
#include ".\timeshiftseeking.h"
extern void LogDebug(const char *fmt, ...) ;

// -- CTimeShiftSeeking implementation ------------

CTimeShiftSeeking::CTimeShiftSeeking(
    const TCHAR * pName,
    LPUNKNOWN pUnk,
    HRESULT* phr,
    CCritSec * pLock) :
        CUnknown(pName, pUnk),
        m_pLock(pLock),
        m_rtStart((long)0)
{
	m_rtCurrent=0;
    m_rtStop = _I64_MAX / 2;
    m_rtDuration = m_rtStop;
    m_dRateSeeking = 1.0;

    m_dwSeekingCaps = AM_SEEKING_CanSeekForwards
        | AM_SEEKING_CanSeekBackwards
        | AM_SEEKING_CanSeekAbsolute
        | AM_SEEKING_CanGetStopPos
        | AM_SEEKING_CanGetDuration
		| AM_SEEKING_AbsolutePositioning
		| AM_SEEKING_CanGetCurrentPos;
}

HRESULT CTimeShiftSeeking::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
    if(riid == IID_IMediaSeeking) {
        CheckPointer(ppv, E_POINTER);
        return GetInterface(static_cast<IMediaSeeking *>(this), ppv);
    }
    else {
        return CUnknown::NonDelegatingQueryInterface(riid, ppv);
    }
}


HRESULT CTimeShiftSeeking::IsFormatSupported(const GUID * pFormat)
{
    CheckPointer(pFormat, E_POINTER);
    // only seeking in time (REFERENCE_TIME units) is supported
    return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

HRESULT CTimeShiftSeeking::QueryPreferredFormat(GUID *pFormat)
{
    CheckPointer(pFormat, E_POINTER);
    *pFormat = TIME_FORMAT_MEDIA_TIME;
    return S_OK;
}

HRESULT CTimeShiftSeeking::SetTimeFormat(const GUID * pFormat)
{
    CheckPointer(pFormat, E_POINTER);

    // nothing to set; just check that it's TIME_FORMAT_TIME
    return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : E_INVALIDARG;
}

HRESULT CTimeShiftSeeking::IsUsingTimeFormat(const GUID * pFormat)
{
    CheckPointer(pFormat, E_POINTER);
    return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

HRESULT CTimeShiftSeeking::GetTimeFormat(GUID *pFormat)
{
    CheckPointer(pFormat, E_POINTER);
    *pFormat = TIME_FORMAT_MEDIA_TIME;
    return S_OK;
}

HRESULT CTimeShiftSeeking::GetDuration(LONGLONG *pDuration)
{
    CheckPointer(pDuration, E_POINTER);
    //CAutoLock lock(m_pLock);
    *pDuration = m_rtDuration;
    return S_OK;
}

HRESULT CTimeShiftSeeking::GetStopPosition(LONGLONG *pStop)
{
    CheckPointer(pStop, E_POINTER);
    //CAutoLock lock(m_pLock);
    *pStop = m_rtStop;
    return S_OK;
}

HRESULT CTimeShiftSeeking::GetCurrentPosition(LONGLONG *pCurrent)
{
	*pCurrent=m_rtStart;
    return *pCurrent;
}

HRESULT CTimeShiftSeeking::GetCapabilities( DWORD * pCapabilities )
{
    CheckPointer(pCapabilities, E_POINTER);
    *pCapabilities = m_dwSeekingCaps;
    return S_OK;
}

HRESULT CTimeShiftSeeking::CheckCapabilities( DWORD * pCapabilities )
{
    CheckPointer(pCapabilities, E_POINTER);

    // make sure all requested capabilities are in our mask
    return (~m_dwSeekingCaps & *pCapabilities) ? S_FALSE : S_OK;
}

HRESULT CTimeShiftSeeking::ConvertTimeFormat( LONGLONG * pTarget, const GUID * pTargetFormat,
                           LONGLONG    Source, const GUID * pSourceFormat )
{
    CheckPointer(pTarget, E_POINTER);
    // format guids can be null to indicate current format

    // since we only support TIME_FORMAT_MEDIA_TIME, we don't really
    // offer any conversions.
    if(pTargetFormat == 0 || *pTargetFormat == TIME_FORMAT_MEDIA_TIME)
    {
        if(pSourceFormat == 0 || *pSourceFormat == TIME_FORMAT_MEDIA_TIME)
        {
            *pTarget = Source;
            return S_OK;
        }
    }

    return E_INVALIDARG;
}


HRESULT CTimeShiftSeeking::SetPositions( LONGLONG * pCurrent,  DWORD CurrentFlags
                      , LONGLONG * pStop,  DWORD StopFlags )
{
    DWORD StopPosBits = StopFlags & AM_SEEKING_PositioningBitsMask;
    DWORD StartPosBits = CurrentFlags & AM_SEEKING_PositioningBitsMask;

    if(StopFlags) {
        CheckPointer(pStop, E_POINTER);

        // accept only relative, incremental, or absolute positioning
        if(StopPosBits != StopFlags) {
            return E_INVALIDARG;
        }
    }

    if(CurrentFlags) {
        CheckPointer(pCurrent, E_POINTER);
        if(StartPosBits != AM_SEEKING_AbsolutePositioning &&
           StartPosBits != AM_SEEKING_RelativePositioning) {
            return E_INVALIDARG;
        }
    }


    // scope for autolock
    {
        //CAutoLock lock(m_pLock);

        // set start position
        if(StartPosBits == AM_SEEKING_AbsolutePositioning)
        {
            m_rtStart = *pCurrent;
			double dStart=(double)((ULONGLONG)m_rtStart);
			dStart/=10000000.0;
			LogDebug("setpositions start:%f", m_rtStart);
        }
        else if(StartPosBits == AM_SEEKING_RelativePositioning)
        {
            m_rtStart += *pCurrent;
			double dStart=(double)((ULONGLONG)m_rtStart);
			dStart/=10000000.0;
			LogDebug("setpositions startrel:%f", dStart);
        }

        // set stop position
        if(StopPosBits == AM_SEEKING_AbsolutePositioning)
        {
            m_rtStop = *pStop;
			double dStop=(double)((ULONGLONG)m_rtStop);
			dStop/=10000000.0;
			LogDebug("setpositions stop:%f", dStop);
        }
        else if(StopPosBits == AM_SEEKING_IncrementalPositioning)
        {
            m_rtStop = m_rtStart + *pStop;
        }
        else if(StopPosBits == AM_SEEKING_RelativePositioning)
        {
            m_rtStop = m_rtStop + *pStop;
			double dStop=(double)((ULONGLONG)m_rtStop);
			dStop/=10000000.0;
			LogDebug("setpositions relstop:%f", dStop);
        }
    }


    HRESULT hr = S_OK;
    if(SUCCEEDED(hr) && StopPosBits) {
        hr = ChangeStop();
    }
    if(StartPosBits) {
        hr = ChangeStart();
    }

    return hr;
}


HRESULT CTimeShiftSeeking::GetPositions( LONGLONG * pCurrent, LONGLONG * pStop )
{
    if(pCurrent) {
        *pCurrent = m_rtStart;
    }
    if(pStop) {
        *pStop = m_rtStop;
    }

    return S_OK;;
}


HRESULT CTimeShiftSeeking::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
    if(pEarliest) {
        *pEarliest = m_rtStart;
    }
    if(pLatest) {
        //CAutoLock lock(m_pLock);
        *pLatest = m_rtStop;
    }
    return S_OK;
}

HRESULT CTimeShiftSeeking::SetRate( double dRate)
{
    {
        CAutoLock lock(m_pLock);
        m_dRateSeeking = dRate;
    }
    return ChangeRate();
}

HRESULT CTimeShiftSeeking::GetRate( double * pdRate)
{
    CheckPointer(pdRate, E_POINTER);
    //CAutoLock lock(m_pLock);
    *pdRate = m_dRateSeeking;
    return S_OK;
}

HRESULT CTimeShiftSeeking::GetPreroll(LONGLONG *pPreroll)
{
    CheckPointer(pPreroll, E_POINTER);
    *pPreroll = 0;
    return S_OK;
}



