/**
*  TSFileSinkProp.cpp
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
#include "TSFileSinkProp.h"
#include "resource.h"
#include "global.h"


CUnknown * WINAPI CTSFileSinkProp::CreateInstance(LPUNKNOWN pUnk, HRESULT *pHr)
{
	CTSFileSinkProp *pNewObject = new CTSFileSinkProp(pUnk);
	if (pNewObject == NULL)
	{
		*pHr = E_OUTOFMEMORY;
	}
	return pNewObject;
}


CTSFileSinkProp::CTSFileSinkProp(IUnknown *pUnk) :
	CBasePropertyPage(NAME("CTSFileSinkProp"), pUnk, IDD_SINKPROPPAGE, IDS_SINKPROPPAGE_TITLE),
	m_pProgram(0)
{
}

CTSFileSinkProp::~CTSFileSinkProp(void)
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

DWORD CTSFileSinkProp::ThreadProc(void)
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

    } while(cmd != CMD_EXIT);

	m_bThreadRunning = FALSE;
    DbgLog((LOG_TRACE, 1, TEXT("Worker thread exiting")));
    return 0;
}

//
// DoProcessingLoop
//
HRESULT CTSFileSinkProp::DoProcessingLoop(void)
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

			Sleep(50);
        }

        // For all commands sent to us there must be a Reply call!
        if(com == CMD_RUN || com == CMD_PAUSE)
        {
            Reply(NOERROR);
        }
        else if(com != CMD_STOP)
        {
            Reply((DWORD) E_UNEXPECTED);
            DbgLog((LOG_ERROR, 1, TEXT("Unexpected command!!!")));
        }
    } while(com != CMD_STOP);

    return S_FALSE;
}

BOOL CTSFileSinkProp::ThreadRunning(void)
{ 
	return m_bThreadRunning;
}

HRESULT CTSFileSinkProp::OnConnect(IUnknown *pUnk)
{
	if (pUnk == NULL)
	{
		return E_POINTER;
	}
	ASSERT(m_pProgram == NULL);

	HRESULT hr = pUnk->QueryInterface(IID_ITSFileSink, (void**)(&m_pProgram));

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

HRESULT CTSFileSinkProp::OnDisconnect(void)
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

HRESULT CTSFileSinkProp::OnActivate(void)
{
	ASSERT(m_pProgram != NULL);
	PopulateDialog();
	HRESULT hr = S_OK;

	CAMThread::CallWorker(CMD_RUN);

	return hr;
}

void CTSFileSinkProp::SetDirty()
{
	m_bDirty = TRUE;

	if(m_pPageSite)
	{
		m_pPageSite->OnStatusChange(PROPPAGESTATUS_DIRTY);
	}
}

void CTSFileSinkProp::SetClean()
{
	m_bDirty = FALSE;

	if(m_pPageSite)
	{
		m_pPageSite->OnStatusChange(PROPPAGESTATUS_CLEAN);
	}
}

BOOL CTSFileSinkProp::OnRefreshProgram()
{
	PopulateDialog();
	return TRUE;
}

BOOL CTSFileSinkProp::PopulateDialog()
{
	ASSERT(m_pProgram != NULL);

	TCHAR sz[MAX_PATH];
	WORD intVal = 0x00;
	__int64 lLongVal;

	m_pProgram->GetChunkReserve(&lLongVal);
	wsprintf(sz, TEXT("%lu"), (__int64)(lLongVal/(__int64)1048576));
	Edit_SetText(GetDlgItem(m_hwnd, IDC_CHKRES), sz);

	m_pProgram->GetMinTSFiles(&intVal);
	wsprintf(sz, TEXT("%u"), intVal);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_MINNUMB), sz);

	m_pProgram->GetMaxTSFiles(&intVal);
	wsprintf(sz, TEXT("%u"), intVal);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_MAXNUMB), sz);

	m_pProgram->GetMaxTSFileSize(&lLongVal);
	wsprintf(sz, TEXT("%lu"), (__int64)(lLongVal/(__int64)1048576));
	Edit_SetText(GetDlgItem(m_hwnd, IDC_MAXSIZE), sz);

	TCHAR szDefFileName[MAX_PATH];
	m_pProgram->GetRegFileName((char*)&szDefFileName);
	sprintf(sz, TEXT("%s"), szDefFileName);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_DEFNAME), sz);
	
	WCHAR curFileName[MAX_PATH];
	m_pProgram->GetBufferFileName((LPWSTR)&curFileName);
	wsprintf(sz, TEXT("%S"), curFileName);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_CURNAME), sz);

	return RefreshDialog();
}

BOOL CTSFileSinkProp::RefreshDialog()
{

	ASSERT(m_pProgram != NULL);

	TCHAR sz[MAX_PATH];
	WORD intVal = 0x00;
	__int64 lLongVal;

	m_pProgram->GetCurrentFileId(&intVal);
	wsprintf(sz, TEXT("%u"), intVal);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_CURNUMB), sz);

	m_pProgram->GetNumbFilesAdded(&intVal);
	wsprintf(sz, TEXT("%u"), intVal);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_NUMBADD), sz);
	int saveAdded = intVal;

	m_pProgram->GetNumbFilesRemoved(&intVal);
	wsprintf(sz, TEXT("%u"), intVal);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_NUMBREM), sz);

	wsprintf(sz, TEXT("%u"), saveAdded - intVal);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_TOTALNUMB), sz);

	m_pProgram->GetFileBufferSize(&lLongVal);
	wsprintf(sz, TEXT("%lu"), (__int64)(lLongVal/(__int64)1048576));
	Edit_SetText(GetDlgItem(m_hwnd, IDC_CURSIZE), sz);

	m_pProgram->GetNumbErrorPackets(&lLongVal);
	wsprintf(sz, TEXT("%lu"), lLongVal);
	Edit_SetText(GetDlgItem(m_hwnd, IDC_PKTERRORS), sz);
	return TRUE;
}

BOOL CTSFileSinkProp::OnReceiveMessage(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	BOOL    bRet = FALSE;
	TCHAR sz[10] = "";

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

		case WM_COMMAND:
		{
			BOOL checked = FALSE;
			switch (LOWORD (wParam))
			{
				case IDCANCEL :
				{
					OnRefreshProgram ();
					break ;
				}

				case IDC_REFRESH:
				{
					OnRefreshProgram ();
					break ;
				}

				case IDC_SAVE :
				{
					// Save Registry settings
					m_pProgram->SetRegSettings();
					SetClean();
					OnRefreshProgram ();
					break ;
				}

				case IDC_RESET:
				{
					m_pProgram->SetNumbErrorPackets((__int64)0);
					OnRefreshProgram ();
					SetDirty();
					break;
				}
				case IDC_CHKRESCHG:
				{
					m_pProgram->SetChunkReserve((__int64) ((__int64)1048576 *(__int64) GetDlgItemInt(hwnd, IDC_CHKRES, &bRet, TRUE)));
					OnRefreshProgram ();
					SetDirty();
					break;
				}

				case IDC_MAXSIZECHG:
				{
					m_pProgram->SetMaxTSFileSize((__int64)((__int64)1048576 *(__int64) GetDlgItemInt(hwnd, IDC_MAXSIZE, &bRet, TRUE)));
					OnRefreshProgram ();
					SetDirty();
					break;
				}

				case IDC_MAXNUMBCHG:
				{
					m_pProgram->SetMaxTSFiles((WORD) GetDlgItemInt(hwnd, IDC_MAXNUMB, &bRet, TRUE));
					OnRefreshProgram ();
					SetDirty();
					break;
				}

				case IDC_MINNUMBCHG:
				{
					m_pProgram->SetMinTSFiles((WORD) GetDlgItemInt(hwnd, IDC_MINNUMB, &bRet, TRUE));
					OnRefreshProgram ();
					SetDirty();
					break;
				}

				case IDC_DEFNAMECHG:
				{
					TCHAR szString[MAX_PATH];
					if (GetDlgItemText(hwnd, IDC_DEFNAME, (char *)&szString, MAX_PATH))
					{
						m_pProgram->SetRegFileName((char *)&szString);
						OnRefreshProgram ();
						SetDirty();
					}
					break;
				}
				
				case IDC_CURNAMECHG:
				{
					WCHAR wString[MAX_PATH];
					if (GetDlgItemTextW(hwnd, IDC_CURNAME, (LPWSTR)&wString, MAX_PATH))
					{
						m_pProgram->SetBufferFileName((LPWSTR)&wString);
						OnRefreshProgram ();
						SetDirty();
					}
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

HRESULT CTSFileSinkProp::OnApplyChanges(void)
{

	TCHAR sz[100];
	sprintf(sz, "%S", L"Do you wish to save these Filter Settings");
	if (MessageBox(NULL, sz, TEXT("TS File Sink Filter Settings"), MB_YESNO) == IDYES)
	{
		m_pProgram->SetRegSettings();
		OnRefreshProgram ();
	}
	return NOERROR;
}

