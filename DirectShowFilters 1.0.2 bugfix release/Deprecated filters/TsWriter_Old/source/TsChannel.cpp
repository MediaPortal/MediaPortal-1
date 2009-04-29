
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include "TsChannel.h"

extern void LogDebug(const char *fmt, ...) ;

CTsChannel::CTsChannel(LPUNKNOWN pUnk, HRESULT *phr,int id) 
{
	m_id=id;
	m_pVideoAnalyzer = new CVideoAnalyzer(pUnk,phr);
	m_pPmtGrabber = new CPmtGrabber(pUnk,phr);
	m_pRecorder = new CRecorder(pUnk,phr);
	m_pTimeShifting= new CTimeShifting(pUnk,phr);
	m_pTeletextGrabber= new CTeletextGrabber(pUnk,phr);
  m_pCaGrabber= new CCaGrabber(pUnk,phr);
}

CTsChannel::~CTsChannel(void)
{
	if (m_pVideoAnalyzer!=NULL)
	{
		LogDebug("del m_pVideoAnalyzer");
		delete m_pVideoAnalyzer;
		m_pVideoAnalyzer=NULL;
	}
	if (m_pPmtGrabber!=NULL)
	{
		LogDebug("del m_pPmtGrabber");
		delete m_pPmtGrabber;
		m_pPmtGrabber=NULL;
	}
	if (m_pRecorder!=NULL)
	{
		LogDebug("del m_pRecorder");
		delete m_pRecorder;
		m_pRecorder=NULL;
	}
	if (m_pTimeShifting!=NULL)
	{
		LogDebug("del m_pTimeShifting");
		delete m_pTimeShifting;
		m_pTimeShifting=NULL;
	}
	if (m_pTeletextGrabber!=NULL)
	{
		LogDebug("del m_pTeletextGrabber");
		delete m_pTeletextGrabber;
		m_pTeletextGrabber=NULL;
	}
	if (m_pCaGrabber!=NULL)
	{
		LogDebug("del m_pCaGrabber");
		delete m_pCaGrabber;
		m_pCaGrabber=NULL;
	}
	LogDebug("del done...");
}


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