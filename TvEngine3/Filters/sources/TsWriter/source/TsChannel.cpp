
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include "TsChannel.h"

extern void LogDebug(const char *fmt, ...) ;

CTsChannel::CTsChannel(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown(NAME("CTsChannel"), pUnk)
{
	m_pVideoAnalyzer = new CVideoAnalyzer(GetOwner(),phr);
	m_pPmtGrabber = new CPmtGrabber(GetOwner(),phr);
	m_pRecorder = new CRecorder(GetOwner(),phr);
	m_pTimeShifting= new CTimeShifting(GetOwner(),phr);
	m_pTeletextGrabber= new CTeletextGrabber(GetOwner(),phr);
  m_pCaGrabber= new CCaGrabber(GetOwner(),phr);
}

CTsChannel::~CTsChannel(void)
{
	delete m_pVideoAnalyzer;
	delete m_pPmtGrabber;
	delete m_pRecorder;
	delete m_pTimeShifting;
	delete m_pTeletextGrabber;
  delete m_pCaGrabber;
}


STDMETHODIMP CTsChannel::Test()
{
	LogDebug("test");
	return S_OK;
}

STDMETHODIMP CTsChannel::QueryInterface(REFIID riid, void **ppv)
{      
	//LogDebug("tschannel:QueryInterface");
	if (SUCCEEDED(NonDelegatingQueryInterface(riid,ppv))) return S_OK;
  return GetOwner()->QueryInterface(riid,ppv);            
}                           

STDMETHODIMP CTsChannel::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
	//LogDebug("tschannel:NonDelegatingQueryInterface");
  CheckPointer(ppv,E_POINTER);

    // Do we have this interface
	if (riid == IID_TSChannel)
	{
		//LogDebug("tschannel:NonDelegatingQueryInterface IID_TSChannel");
		return GetInterface((ITSChannel*)this, ppv);
	}
	if (riid == IID_ITSVideoAnalyzer)
	{
		//LogDebug("tschannel:NonDelegatingQueryInterface IID_ITSVideoAnalyzer");
		return GetInterface((ITsVideoAnalyzer*)m_pVideoAnalyzer, ppv);
	}
	else if (riid == IID_IPmtGrabber)
	{
		//LogDebug("tschannel:NonDelegatingQueryInterface IID_IPmtGrabber");
		return GetInterface((IPmtGrabber*)m_pPmtGrabber, ppv);
	}
	else if (riid == IID_ITsRecorder)
	{
		//LogDebug("tschannel:NonDelegatingQueryInterface IID_ITsRecorder");
		return GetInterface((ITsRecorder*)m_pRecorder, ppv);
	}
	else if (riid == IID_ITsTimeshifting)
	{
		//LogDebug("tschannel:NonDelegatingQueryInterface IID_ITsTimeshifting");
		return GetInterface((ITsTimeshifting*)m_pTimeShifting, ppv);
	}
	else if (riid == IID_ITeletextGrabber)
	{
		//LogDebug("tschannel:NonDelegatingQueryInterface IID_ITeletextGrabber");
		return GetInterface((ITeletextGrabber*)m_pTeletextGrabber, ppv);
	}
	else if (riid == IID_ICaGrabber)
	{
		//LogDebug("tschannel:NonDelegatingQueryInterface IID_ICaGrabber");
		return GetInterface((ICaGrabber*)m_pCaGrabber, ppv);
	}
		//LogDebug("tschannel:NonDelegatingQueryInterface unknown");
  return CUnknown::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface


void CTsChannel::OnTsPacket(byte* tsPacket)
{
	try
	{
		m_pVideoAnalyzer->OnTsPacket(tsPacket);
		m_pPmtGrabber->OnTsPacket(tsPacket);
		m_pRecorder->OnTsPacket(tsPacket);
		m_pTimeShifting->OnTsPacket(tsPacket);
		m_pTeletextGrabber->OnTsPacket(tsPacket);
    m_pCaGrabber->OnTsPacket(tsPacket);
	}
	catch(...)
	{
		LogDebug("exception in AnalyzeTsPacket");
	}
}