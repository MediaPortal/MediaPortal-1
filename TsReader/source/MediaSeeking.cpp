#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "mediaseeking.h"

extern void LogDebug(const char *fmt, ...) ;
// -- CMediaSeeking implementation ------------

CMediaSeeking::CMediaSeeking(
  const TCHAR * pName,
  LPUNKNOWN pUnk,
  HRESULT* phr,
  CCritSec * pLock) :
  CUnknown(pName, pUnk),
  m_pLock(pLock),
  m_rtStart((long)0)
{
  m_rtStop = _I64_MAX / 2;
  m_rtDuration = m_rtStop;
  m_dRateSeeking = 1.0;

  m_dwSeekingCaps = AM_SEEKING_CanSeekForwards
    | AM_SEEKING_CanSeekBackwards
    | AM_SEEKING_CanSeekAbsolute
    | AM_SEEKING_CanGetStopPos
    | AM_SEEKING_CanGetDuration;
}

HRESULT CMediaSeeking::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
if(riid == IID_IMediaSeeking) 
  {
    CheckPointer(ppv, E_POINTER);
    return GetInterface(static_cast<IMediaSeeking *>(this), ppv);
  }
  else 
  {
    return CUnknown::NonDelegatingQueryInterface(riid, ppv);
  }
}


HRESULT CMediaSeeking::IsFormatSupported(const GUID * pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  // only seeking in time (REFERENCE_TIME units) is supported
  return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

HRESULT CMediaSeeking::QueryPreferredFormat(GUID *pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  *pFormat = TIME_FORMAT_MEDIA_TIME;
  return S_OK;
}

HRESULT CMediaSeeking::SetTimeFormat(const GUID * pFormat)
{
  CheckPointer(pFormat, E_POINTER);

  // nothing to set; just check that it's TIME_FORMAT_TIME
  return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : E_INVALIDARG;
}

HRESULT CMediaSeeking::IsUsingTimeFormat(const GUID * pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  return *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

HRESULT CMediaSeeking::GetTimeFormat(GUID *pFormat)
{
  CheckPointer(pFormat, E_POINTER);
  *pFormat = TIME_FORMAT_MEDIA_TIME;
  return S_OK;
}

HRESULT CMediaSeeking::GetDuration(LONGLONG *pDuration)
{
  CheckPointer(pDuration, E_POINTER);
  //CAutoLock lock(m_pLock);
  *pDuration = m_rtDuration;
  return S_OK;
}

HRESULT CMediaSeeking::GetStopPosition(LONGLONG *pStop)
{
  CheckPointer(pStop, E_POINTER);
  //CAutoLock lock(m_pLock);
  *pStop = m_rtStop;
  return S_OK;
}

HRESULT CMediaSeeking::GetCurrentPosition(LONGLONG *pCurrent)
{
  // GetCurrentPosition is typically supported only in renderers and
  // not in source filters.
  return E_NOTIMPL;
}

HRESULT CMediaSeeking::GetCapabilities( DWORD * pCapabilities )
{
  CheckPointer(pCapabilities, E_POINTER);
  *pCapabilities = m_dwSeekingCaps;
  return S_OK;
}

HRESULT CMediaSeeking::CheckCapabilities( DWORD * pCapabilities )
{
  CheckPointer(pCapabilities, E_POINTER);

  // make sure all requested capabilities are in our mask
  return (~m_dwSeekingCaps & *pCapabilities) ? S_FALSE : S_OK;
}

HRESULT CMediaSeeking::ConvertTimeFormat( LONGLONG * pTarget, const GUID * pTargetFormat,
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


HRESULT CMediaSeeking::SetPositions( LONGLONG * pCurrent,  DWORD CurrentFlags
                      , LONGLONG * pStop,  DWORD StopFlags )
{
  LogDebug("CMediaSeeking::SetPositions");
  DWORD StopPosBits = StopFlags & AM_SEEKING_PositioningBitsMask;
  DWORD StartPosBits = CurrentFlags & AM_SEEKING_PositioningBitsMask;

  if(StopFlags) 
  {
    CheckPointer(pStop, E_POINTER);

    // accept only relative, incremental, or absolute positioning
    if(StopPosBits != StopFlags) 
    {
      return E_INVALIDARG;
    }
  }

  if(CurrentFlags) 
  {
    CheckPointer(pCurrent, E_POINTER);
    if(StartPosBits != AM_SEEKING_AbsolutePositioning &&
       StartPosBits != AM_SEEKING_RelativePositioning) 
    {
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
    }
    else if(StartPosBits == AM_SEEKING_RelativePositioning)
    {
      m_rtStart += *pCurrent;
    }

    // set stop position
    if(StopPosBits == AM_SEEKING_AbsolutePositioning)
    {
      m_rtStop = *pStop;
    }
    else if(StopPosBits == AM_SEEKING_IncrementalPositioning)
    {
      m_rtStop = m_rtStart + *pStop;
    }
    else if(StopPosBits == AM_SEEKING_RelativePositioning)
    {
      m_rtStop = m_rtStop + *pStop;
    }
  }

  HRESULT hr = S_OK;
  if(SUCCEEDED(hr) && StopPosBits) 
  {
    hr = ChangeStop();
  }
  if(StartPosBits) 
  {
    hr = ChangeStart();
  }
  return hr;
}


HRESULT CMediaSeeking::GetPositions( LONGLONG * pCurrent, LONGLONG * pStop )
{
  if(pCurrent) 
  {
    *pCurrent = m_rtStart;
  }
  if(pStop) 
  {
    *pStop = m_rtStop;
  }
  return S_OK;
}


HRESULT CMediaSeeking::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
  if(pEarliest) 
  {
    *pEarliest = 0;
  }
  if(pLatest) 
  {
    //CAutoLock lock(m_pLock);
    *pLatest = m_rtDuration;
  }
  return S_OK;
}

HRESULT CMediaSeeking::SetRate( double dRate)
{
  {
    //CAutoLock lock(m_pLock);
    m_dRateSeeking = dRate;
  }
  return ChangeRate();
}

HRESULT CMediaSeeking::GetRate( double * pdRate)
{
  CheckPointer(pdRate, E_POINTER);
  //CAutoLock lock(m_pLock);
  *pdRate = m_dRateSeeking;
  return S_OK;
}

HRESULT CMediaSeeking::GetPreroll(LONGLONG *pPreroll)
{
  CheckPointer(pPreroll, E_POINTER);
  *pPreroll = 0;
  return S_OK;
}
