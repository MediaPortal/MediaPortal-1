/**
*  TSFileSourceProp.cpp
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include "stdafx.h"
#include "TSFileSourceProp.h"
#include "resource.h"
#include <string>
#include "global.h"


CUnknown * WINAPI CTSFileSourceProp::CreateInstance(LPUNKNOWN pUnk, HRESULT *pHr)
{
	CTSFileSourceProp *pNewObject = new CTSFileSourceProp(pUnk);
	if (pNewObject == NULL)
	{
		*pHr = E_OUTOFMEMORY;
	}
	return pNewObject;
}


CTSFileSourceProp::CTSFileSourceProp(IUnknown *pUnk) :
	CBasePropertyPage(NAME("TSFileSourceProp"), pUnk, IDD_PROPPAGE, IDS_PROPPAGE_TITLE),
//	CBasePropertyPage(NAME("TSFileSourceProp"), pUnk, IDD_INFO, IDS_INFORMATION_TITLE),
	m_pProgram(0)
{
	m_dRate = 1.0;
}

CTSFileSourceProp::~CTSFileSourceProp(void)
{
	//Make sure the worker thread is stopped before we exit.
	//Also closes the files.m_hThread
	if (CAMThread::ThreadExists())
	{
		CAMThread::CallWorker(CMD_STOP);
		CAMThread::CallWorker(CMD_EXIT);
		CAMThread::Close();
	}

	if (m_pProgram)
		m_pProgram->Release();
}

DWORD CTSFileSourceProp::ThreadProc(void)
{
	BrakeThread Brake;

    HRESULT hr;  // the return code from calls
    Command com;

    do
    {
        com = GetRequest();
        if(com != CMD_INIT)
        {
			m_bThreadRunning = FALSE;
            DbgLog((LOG_ERROR, 1, TEXT("Thread expected init command")));
            Reply((DWORD) E_UNEXPECTED);
        }
		Sleep(10);
    } while(com != CMD_INIT);

    DbgLog((LOG_TRACE, 1, TEXT("Worker thread initializing")));

	hr = S_OK;
	if (FAILED(hr))
    {
		m_bThreadRunning = FALSE;
		DbgLog((LOG_ERROR, 1, TEXT("ThreadCreate failed. Aborting thread.")));

        Reply(hr);  // send failed return code from ThreadCreate
        return 1;
    }

    // Initialisation suceeded
    Reply(NOERROR);

    Command cmd;
    do
    {
        cmd = GetRequest();

        switch(cmd)
        {
            case CMD_EXIT:
				m_bThreadRunning = FALSE;
                Reply(NOERROR);
                break;

            case CMD_RUN:
                DbgLog((LOG_ERROR, 1, TEXT("CMD_RUN received before a CMD_PAUSE???")));
                // !!! fall through

            case CMD_PAUSE:
				m_bThreadRunning = TRUE;
                Reply(NOERROR);
                DoProcessingLoop();
				m_bThreadRunning = FALSE;
                break;

            case CMD_STOP:
				m_bThreadRunning = FALSE;
                Reply(NOERROR);
                break;

            default:
                DbgLog((LOG_ERROR, 1, TEXT("Unknown command %d received!"), cmd));
                Reply((DWORD) E_NOTIMPL);
                break;
        }
		Sleep(10);

    } while(cmd != CMD_EXIT);

	m_bThreadRunning = FALSE;
    DbgLog((LOG_TRACE, 1, TEXT("Worker thread exiting")));
    return 0;
}

//
// DoProcessingLoop
//
HRESULT CTSFileSourceProp::DoProcessingLoop(void)
{
    Command com;

    do
    {
		int count = 0;
        while(!CheckRequest(&com))
        {
			HRESULT hr = S_OK;// if an error occurs.

			if (count > 10)
			{
				RefreshDialog();
				count = 0;
			}
			else
				count++;

			Sleep(100);
        }

        // For all commands sent to us there must be a Reply call!
        if(com == CMD_RUN || com == CMD_PAUSE)
        {
			m_bThreadRunning = TRUE;
            Reply(NOERROR);
        }
        else if(com != CMD_STOP)
        {
            Reply((DWORD) E_UNEXPECTED);
            DbgLog((LOG_ERROR, 1, TEXT("Unexpected command!!!")));
        }
 		Sleep(10);
   } while(com != CMD_STOP);

	m_bThreadRunning = FALSE;
    return S_FALSE;
}

BOOL CTSFileSourceProp::ThreadRunning(void)
{ 
	return m_bThreadRunning;
}

HRESULT CTSFileSourceProp::OnConnect(IUnknown *pUnk)
{
	if (pUnk == NULL)
	{
		return E_POINTER;
	}
	ASSERT(m_pProgram == NULL);

	HRESULT hr = pUnk->QueryInterface(IID_ITSFileSource, (void**)(&m_pProgram));

	if(FAILED(hr))
	{
		return E_NOINTERFACE;
	}
	ASSERT(m_pProgram);

	CAMThread::Create();			 //Create our update thread
	if (CAMThread::ThreadExists())
		CAMThread::CallWorker(CMD_INIT); //Initalize our update thread

	return NOERROR;
}

HRESULT CTSFileSourceProp::OnDisconnect(void)
{
	if (CAMThread::ThreadExists())
	{
		CAMThread::CallWorker(CMD_STOP);
		CAMThread::CallWorker(CMD_EXIT);
		CAMThread::Close();
	}

	if (m_pProgram)
	{
		m_pProgram->Release();
		m_pProgram = NULL;
	}
	return S_OK;
}

HRESULT CTSFileSourceProp::OnActivate(void)
{
	ASSERT(m_pProgram != NULL);
	PopulateDialog();
	HRESULT hr = S_OK;

	CAMThread::CallWorker(CMD_RUN);

	return hr;
}

void CTSFileSourceProp::SetDirty()
{
	m_bDirty = TRUE;

	if(m_pPageSite)
	{
		m_pPageSite->OnStatusChange(PROPPAGESTATUS_DIRTY);
	}
}

void CTSFileSourceProp::SetClean()
{
	m_bDirty = FALSE;

	if(m_pPageSite)
	{
		m_pPageSite->OnStatusChange(PROPPAGESTATUS_CLEAN);
	}
}

BOOL CTSFileSourceProp::OnRefreshProgram()
{
	PopulateDialog();
	return TRUE;
}

BOOL CTSFileSourceProp::PopulateDialog()
{
	ASSERT(m_pProgram != NULL);

	TCHAR sz[60];
	WORD PidNr = 0x00;
	m_pProgram->GetPgmNumb(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_PGM), sz);

	CComPtr <IMediaSeeking> pIMediaSeeking;
	if SUCCEEDED(m_pProgram->QueryInterface(&pIMediaSeeking))
	{
		TCHAR sz[32];
		pIMediaSeeking->GetRate(&m_dRate);
		sprintf(sz, "%4.2lf", m_dRate);
		Edit_SetText(GetDlgItem(m_hwnd, IDC_RATE), sz);
	}

	return 	RefreshDialog();
}

BOOL CTSFileSourceProp::RefreshDialog()
{
	ASSERT(m_pProgram != NULL);

	TCHAR sz[MAX_PATH];
	WORD PidNr = 0x00;
	REFERENCE_TIME dur;

	m_pProgram->GetVideoPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_VIDEO), sz);

	unsigned char videoPidType[128];
	m_pProgram->GetVideoPidType((unsigned char*)&videoPidType);
	wsprintf(sz, TEXT("%s"), videoPidType);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_VIDEOTYPE), sz);
	
	m_pProgram->GetAudioPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_AUDIO), sz);

	m_pProgram->GetAudio2Pid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_AUDIO2), sz);

	m_pProgram->GetAACPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_AAC), sz);

	m_pProgram->GetAAC2Pid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_AAC2), sz);

	m_pProgram->GetDTSPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_DTS), sz);

	m_pProgram->GetDTS2Pid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_DTS2), sz);

	m_pProgram->GetAC3Pid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_AC3), sz);

	m_pProgram->GetAC3_2Pid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_AC3_2), sz);

	m_pProgram->GetNIDPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_NID), sz);

	LPOLESTR filename;
	m_pProgram->GetCurFile(&filename, NULL);
	sprintf(sz, "%S",filename);
	SetWindowText(GetDlgItem(m_hwnd, IDC_FILE), sz);

	unsigned char netname[128] ="";
	m_pProgram->GetNetworkName((unsigned char*)&netname);
	sprintf(sz, "%s",netname);
	SetWindowText(GetDlgItem(m_hwnd, IDC_NETID), sz);

	unsigned char chnumb[128] ="";
	m_pProgram->GetChannelNumber((unsigned char*)&chnumb);
	m_pProgram->GetONIDPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_ONID), sz);

	unsigned char onetname[128] ="";
	unsigned char chname[128] ="";
	m_pProgram->GetONetworkName((unsigned char*)&onetname);
	m_pProgram->GetChannelName((unsigned char*)&chname);
	sprintf(sz, "%s",onetname);
	SetWindowText(GetDlgItem(m_hwnd, IDC_ONETID), sz);

	sprintf(sz, "Ch %s :- %s",chnumb, chname);
	SetWindowText(GetDlgItem(m_hwnd, IDC_CHID), sz);

	m_pProgram->GetTSIDPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_TSID), sz);

	m_pProgram->GetPMTPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_PMT), sz);

	m_pProgram->GetSIDPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_SID), sz);

	m_pProgram->GetPCRPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_PCR), sz);

	m_pProgram->GetDuration(&dur);
	LONG ms = (LONG)(dur/(LONGLONG)10000);
	LONG secs = ms / 1000;
	LONG mins = secs / 60;
	LONG hours = mins / 60;
	ms -= (secs*1000);
	secs -= (mins*60);
	mins -= (hours*60);
	wsprintf(sz, TEXT("%02i:%02i:%02i.%03i"), hours, mins, secs, ms);
	SetWindowText(GetDlgItem(m_hwnd, IDC_DURATION), sz);

	m_pProgram->GetPCRPosition(&dur);
	ms = (LONG)(dur/(LONGLONG)10000);
	secs = ms / 1000;
	mins = secs / 60;
	hours = mins / 60;
	ms -= (secs*1000);
	secs -= (mins*60);
	mins -= (hours*60);
	wsprintf(sz, TEXT("%02i:%02i:%02i.%03i"), hours, mins, secs, ms);
	SetWindowText(GetDlgItem(m_hwnd, IDC_CURR), sz);

	long rate;
	m_pProgram->GetBitRate(&rate);
    wsprintf(sz, TEXT("%lu"), rate);
    Edit_SetText(GetDlgItem(m_hwnd, IDC_DATARATE), sz);

	m_pProgram->GetTelexPid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_TXT), sz);

	m_pProgram->GetSubtitlePid(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_SUB), sz);

//	m_pProgram->GetPgmNumb(&PidNr);
//	wsprintf(sz, TEXT("%u"), PidNr);
//	Edit_SetText(GetDlgItem(m_hwnd, IDC_PGM), sz);

	m_pProgram->GetPgmCount(&PidNr);
	wsprintf(sz, TEXT("%u"), PidNr);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_CNT), sz);

	m_pProgram->GetFixedAspectRatio(&PidNr);
	CheckDlgButton(m_hwnd,IDC_FIXED_AR, PidNr);

	m_pProgram->GetCreateTSPinOnDemux(&PidNr);
	CheckDlgButton(m_hwnd,IDC_CREATETSPIN, PidNr);

	m_pProgram->GetCreateTxtPinOnDemux(&PidNr);
	CheckDlgButton(m_hwnd,IDC_CREATETXTPIN, PidNr);

	m_pProgram->GetCreateSubPinOnDemux(&PidNr);
	CheckDlgButton(m_hwnd,IDC_CREATESUBPIN, PidNr);

	m_pProgram->GetAC3Mode(&PidNr);
	CheckDlgButton(m_hwnd,IDC_AC3MODE,PidNr);

	m_pProgram->GetMP2Mode(&PidNr);
	CheckDlgButton(m_hwnd,IDC_MPEG1MODE,!PidNr);
	CheckDlgButton(m_hwnd,IDC_MPEG2MODE,PidNr);

	m_pProgram->GetAudio2Mode(&PidNr);
	CheckDlgButton(m_hwnd,IDC_AUDIO2MODE,PidNr);

	m_pProgram->GetAutoMode(&PidNr);
	CheckDlgButton(m_hwnd,IDC_AUTOMODE,PidNr);

	m_pProgram->GetNPControl(&PidNr);
	CheckDlgButton(m_hwnd,IDC_NPCTRL,PidNr);

	m_pProgram->GetNPSlave(&PidNr);
	CheckDlgButton(m_hwnd,IDC_NPSLAVE,PidNr);

	m_pProgram->GetSharedMode(&PidNr);
	CheckDlgButton(m_hwnd,IDC_SHAREDMODE,PidNr);

	m_pProgram->GetInjectMode(&PidNr);
	CheckDlgButton(m_hwnd,IDC_INJECTMODE,PidNr);

	m_pProgram->GetRateControlMode(&PidNr);
	CheckDlgButton(m_hwnd,IDC_RATECONTROL,PidNr);

	m_pProgram->GetReadOnly(&PidNr);
	wsprintf(sz, (PidNr==0?TEXT("Normal"):TEXT("ReadOnly")));
	SetWindowText(GetDlgItem(m_hwnd, IDC_FILEMODE), sz);
	EnableWindow(GetDlgItem(m_hwnd, IDC_DELAYMODE), PidNr);

	PidNr = 0;
	m_pProgram->GetROTMode(&PidNr);
	CheckDlgButton(m_hwnd,IDC_ROTMODE,PidNr);


	PidNr = 0;
	m_pProgram->GetClockMode(&PidNr);
	if (PidNr == 0)
		CheckDlgButton(m_hwnd,IDC_DEFCLOCK,TRUE);
	else if (PidNr == 1)
		CheckDlgButton(m_hwnd,IDC_TSFSCLOCK,TRUE);
	else if (PidNr == 2)
		CheckDlgButton(m_hwnd,IDC_DEMCLOCK,TRUE);
	else if (PidNr == 3)
		CheckDlgButton(m_hwnd,IDC_RENCLOCK,TRUE);
	else{
		CheckDlgButton(m_hwnd,IDC_DEFCLOCK,FALSE);
		CheckDlgButton(m_hwnd,IDC_TSFSCLOCK,FALSE);
		CheckDlgButton(m_hwnd,IDC_DEMCLOCK,FALSE);
		CheckDlgButton(m_hwnd,IDC_RENCLOCK,FALSE);
	}

	if (PidNr == 1)
	{
		EnableWindow(GetDlgItem(m_hwnd, IDC_RATE), TRUE);
		EnableWindow(GetDlgItem(m_hwnd, IDC_RATECHG), TRUE);
		EnableWindow(GetDlgItem(m_hwnd, IDC_RATESPIN), TRUE);
	}
	else
	{
		EnableWindow(GetDlgItem(m_hwnd, IDC_RATE), FALSE);
		EnableWindow(GetDlgItem(m_hwnd, IDC_RATECHG), FALSE);
		EnableWindow(GetDlgItem(m_hwnd, IDC_RATESPIN), FALSE);
	}

	m_pProgram->GetReadOnly(&PidNr);
	if (PidNr)
		m_pProgram->GetDelayMode(&PidNr);
	else
		PidNr = 0;
	CheckDlgButton(m_hwnd,IDC_DELAYMODE,PidNr);

	return TRUE;
}

BOOL CTSFileSourceProp::OnReceiveMessage(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	BOOL    bRet = FALSE;

	if (!m_bThreadRunning)
		return FALSE;

	switch(uMsg)
	{
		case WM_INITDIALOG:
		{
			PopulateDialog();
			return TRUE;
		}

		case WM_DESTROY:
		{
			DestroyWindow(m_hwnd);
			return TRUE;
		}

		case WM_RBUTTONUP:
		{
			if (m_bThreadRunning)
			{
				CAMThread::CallWorker(CMD_STOP);
				while (m_bThreadRunning){Sleep(100);};
				m_pProgram->ShowStreamMenu(hwnd);
				OnRefreshProgram () ;
				CAMThread::CallWorker(CMD_PAUSE);
			}
			return TRUE;
		}

		case WM_COMMAND:
		{
			BOOL checked = FALSE;
			switch (LOWORD (wParam))
			{
				case IDCANCEL :
				{
					OnRefreshProgram () ;
					break ;
				}

				case IDC_SAVE :
				{
					// Save Registry settings
					m_pProgram->SetRegSettings();
					m_pProgram->SetRegProgram();
					SetClean();
					OnRefreshProgram () ;
					break ;
				}

				case IDC_ENTER:
				{
					m_pProgram->SetPgmNumb((WORD) GetDlgItemInt(hwnd, IDC_PGM, &bRet, TRUE));
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_RATECHG:
				{
					CHAR *psz = new CHAR[MAX_PATH];
					if (GetDlgItemText(hwnd, IDC_RATE, psz, 5))
					{
						char * pEnd;
						m_dRate = strtod(psz, &pEnd);
						CComPtr <IBaseFilter> pIBaseFilter;
						if SUCCEEDED(m_pProgram->QueryInterface(&pIBaseFilter))
						{
							FILTER_INFO filterInfo;
							if SUCCEEDED(pIBaseFilter->QueryFilterInfo(&filterInfo) && filterInfo.pGraph)
							{
								CComPtr <IMediaSeeking> pIMediaSeeking;
								if SUCCEEDED(filterInfo.pGraph->QueryInterface(&pIMediaSeeking))
								{
									pIMediaSeeking->SetRate(m_dRate);
//	TCHAR sz[128];
//	sprintf(sz, "%u", pIMediaSeeking->SetRate(m_dRate));
//	MessageBox(NULL, sz,"test", NULL);

								}
								filterInfo.pGraph->Release();
							}
						}
						OnRefreshProgram () ;
						SetDirty();
						delete[]psz;
						break;
					}
					delete[]psz;
				}
/*
				case IDC_RATECHG:
				{
					CHAR *psz = new CHAR[MAX_PATH];
					if (GetDlgItemText(hwnd, IDC_RATE, psz, 5))
					{
						char * pEnd;
						m_dRate = strtod(psz, &pEnd);
						CComPtr <IMediaSeeking> pIMediaSeeking;
						if SUCCEEDED(m_pProgram->QueryInterface(&pIMediaSeeking))
						{
							pIMediaSeeking->SetRate(m_dRate);
						}
						OnRefreshProgram () ;
						SetDirty();
						delete[]psz;
						break;
					}
					delete[]psz;
				}
*/
				case IDC_NEXT:
				{
					m_pProgram->NextPgmNumb();
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_PREV:
				{
					m_pProgram->PrevPgmNumb();
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_CREATETSPIN:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd,IDC_CREATETSPIN);
					m_pProgram->SetCreateTSPinOnDemux(checked);
					OnRefreshProgram();
					SetDirty();
					break;
				}

				case IDC_FIXED_AR:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd,IDC_FIXED_AR);
					m_pProgram->SetFixedAspectRatio(checked);
					OnRefreshProgram();
					SetDirty();
					break;
				}

				case IDC_CREATETXTPIN:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd,IDC_CREATETXTPIN);
					m_pProgram->SetCreateTxtPinOnDemux(checked);
					OnRefreshProgram();
					SetDirty();
					break;
				}

				case IDC_CREATESUBPIN:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd,IDC_CREATESUBPIN);
					m_pProgram->SetCreateSubPinOnDemux(checked);
					OnRefreshProgram();
					SetDirty();
					break;
				}

				case IDC_AC3MODE:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd,IDC_AC3MODE);
					m_pProgram->SetAC3Mode(checked);
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_MPEG1MODE:
				{
					m_pProgram->SetMP2Mode(FALSE);
					CheckDlgButton(hwnd,IDC_MPEG1MODE,TRUE);
					CheckDlgButton(hwnd,IDC_MPEG2MODE,FALSE);
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_MPEG2MODE:
				{
					m_pProgram->SetMP2Mode(TRUE);
					CheckDlgButton(hwnd,IDC_MPEG1MODE,FALSE);
					CheckDlgButton(hwnd,IDC_MPEG2MODE,TRUE);
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_AUDIO2MODE:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd,IDC_AUDIO2MODE);
					m_pProgram->SetAudio2Mode(checked);
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_AUTOMODE:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd,IDC_AUTOMODE);
					m_pProgram->SetAutoMode(checked);
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_NPCTRL:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd,IDC_NPCTRL);
					m_pProgram->SetNPControl(checked);
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_NPSLAVE:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd,IDC_NPSLAVE);
					m_pProgram->SetNPSlave(checked);
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_EPGINFO:
				{
					if (m_pProgram->ShowEPGInfo() == S_OK)
						OnRefreshProgram () ;
					break;
				}

				case IDC_DELAYMODE:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd, IDC_DELAYMODE);
					m_pProgram->SetDelayMode(checked);
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_SHAREDMODE:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd, IDC_SHAREDMODE);
					m_pProgram->SetSharedMode(checked);
					OnRefreshProgram ();
					SetDirty();
					break;
				}

				case IDC_INJECTMODE:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd, IDC_INJECTMODE);
					m_pProgram->SetInjectMode(checked);
					OnRefreshProgram ();
					SetDirty();
					break;
				}

				case IDC_RATECONTROL:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd, IDC_RATECONTROL);
					m_pProgram->SetRateControlMode(checked);
					OnRefreshProgram ();
					SetDirty();
					break;
				}

				case IDC_REFRESH:
				{
					m_pProgram->Refresh();
					OnRefreshProgram();
					break;
				}

				case IDC_LOAD:
				{
					if (m_bThreadRunning)
					{
						CAMThread::CallWorker(CMD_STOP);
						while (m_bThreadRunning){Sleep(100);};
						m_pProgram->Load(L"", NULL);
						CAMThread::CallWorker(CMD_PAUSE);
					}
					break;
				}

				case IDC_ROTMODE:
				{
					checked = (BOOL)IsDlgButtonChecked(hwnd, IDC_ROTMODE);
					CheckDlgButton(hwnd,IDC_ROTMODE,checked);
					m_pProgram->SetROTMode(checked);
					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_DEFCLOCK:
				{
					CheckDlgButton(hwnd,IDC_TSFSCLOCK,FALSE);
					CheckDlgButton(hwnd,IDC_DEMCLOCK,FALSE);
					CheckDlgButton(hwnd,IDC_RENCLOCK,FALSE);
					if((BOOL)IsDlgButtonChecked(hwnd, IDC_DEFCLOCK))
					{
						WORD PidNr = 0;
						m_pProgram->GetClockMode(&PidNr);
						if (PidNr != 0){
							m_pProgram->SetClockMode(0);
							CheckDlgButton(hwnd,IDC_DEFCLOCK,TRUE);
						}
						else
						{
							CheckDlgButton(hwnd,IDC_DEFCLOCK,FALSE);
							m_pProgram->SetClockMode(0);
						}
					}

					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_TSFSCLOCK:
				{
					CheckDlgButton(hwnd,IDC_DEFCLOCK,FALSE);
					CheckDlgButton(hwnd,IDC_DEMCLOCK,FALSE);
					CheckDlgButton(hwnd,IDC_RENCLOCK,FALSE);
					if((BOOL)IsDlgButtonChecked(hwnd, IDC_TSFSCLOCK))
					{
						WORD PidNr = 0;
						m_pProgram->GetClockMode(&PidNr);
						if (PidNr != 1){
							m_pProgram->SetClockMode(1);
							CheckDlgButton(hwnd,IDC_TSFSCLOCK,TRUE);
						}
						else
						{
							CheckDlgButton(hwnd,IDC_TSFSCLOCK,FALSE);
							m_pProgram->SetClockMode(0);
						}
					}

					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_DEMCLOCK:
				{
					CheckDlgButton(hwnd,IDC_DEFCLOCK,FALSE);
					CheckDlgButton(hwnd,IDC_TSFSCLOCK,FALSE);
					CheckDlgButton(hwnd,IDC_RENCLOCK,FALSE);
					if((BOOL)IsDlgButtonChecked(hwnd, IDC_DEMCLOCK))
					{
						WORD PidNr = 0;
						m_pProgram->GetClockMode(&PidNr);
						if (PidNr != 2){
							m_pProgram->SetClockMode(2);
							CheckDlgButton(hwnd,IDC_DEMCLOCK,TRUE);
						}
						else
						{
							CheckDlgButton(hwnd,IDC_DEMCLOCK,FALSE);
							m_pProgram->SetClockMode(0);
						}
					}

					OnRefreshProgram () ;
					SetDirty();
					break;
				}

				case IDC_RENCLOCK:
				{
					CheckDlgButton(hwnd,IDC_DEFCLOCK,FALSE);
					CheckDlgButton(hwnd,IDC_TSFSCLOCK,FALSE);
					CheckDlgButton(hwnd,IDC_DEMCLOCK,FALSE);
					if((BOOL)IsDlgButtonChecked(hwnd, IDC_RENCLOCK))
					{
						WORD PidNr = 0;
						m_pProgram->GetClockMode(&PidNr);
						if (PidNr != 3){
							m_pProgram->SetClockMode(3);
							CheckDlgButton(hwnd,IDC_RENCLOCK,TRUE);
						}
						else
						{
							CheckDlgButton(hwnd,IDC_RENCLOCK,FALSE);
							m_pProgram->SetClockMode(0);
						}
					}

					OnRefreshProgram () ;
					SetDirty();
					break;
				}

			};
			return TRUE;
		}
		default:
			return FALSE;
	}
	return TRUE;
}

HRESULT CTSFileSourceProp::OnApplyChanges(void)
{

	TCHAR sz[100];
	sprintf(sz, "%S", L"Do you wish to save these Filter Settings");
	if (MessageBox(NULL, sz, TEXT("TSFileSource Filter Settings"), MB_YESNO) == IDYES)
	{
		m_pProgram->SetRegSettings();
		m_pProgram->SetRegProgram();
		OnRefreshProgram ();
	}
	return NOERROR;
}

