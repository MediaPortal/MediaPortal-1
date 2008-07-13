/*****************************************************************
 *                     XML NSIS plugin setup                     *
 *                                                               *
 * 2006 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)  *
 *****************************************************************/

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shlobj.h>
#include "Res\Resource.h"

#define PROGRAM_NAME        "XML"
#define PROGRAM_CAPTION     PROGRAM_NAME " Plugin Setup"
#define PROGRAM_REGROOT     HKEY_LOCAL_MACHINE
#define PROGRAM_REGKEY      "SOFTWARE\\NSIS"
#define PROGRAM_FILE        "\\makensis.exe"
#define PROGRAM_BROWSETEXT  "Choose NSIS directory:"


#ifndef BIF_NONEWFOLDERBUTTON
  #define BIF_NONEWFOLDERBUTTON 0x200
#endif
#ifndef BIF_NEWDIALOGSTYLE
  #define BIF_NEWDIALOGSTYLE 0x0040
#endif

HINSTANCE hInstance;
LPSTR lpCmdLine;
int nCmdShow;
char szExeDir[MAX_PATH];

BOOL CALLBACK SelectPath(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam);
int CALLBACK BrowseCallbackProc(HWND hWnd, UINT uMsg, LPARAM lParam, LPARAM lpData);
BOOL CopyFileWithMessages(HWND hWnd, char *pSourcePath, char *pSourceFile, char *pTargetPath, char *pTargetFile);
char* GetCommandLineParamsA();
BOOL FileExists(char *fname);
void TrimBackslash(char *szPath);

void _WinMain()
{
  STARTUPINFO lpStartupInfo;
  HWND hDlg;
  MSG msg;
  int i;

  //Get standart WinMain info, because of own entry point.
  hInstance=GetModuleHandle(NULL);

  lpCmdLine=GetCommandLineParamsA();

  lpStartupInfo.cb=sizeof(STARTUPINFO);
  GetStartupInfo(&lpStartupInfo);
  nCmdShow=lpStartupInfo.wShowWindow;
  if (!nCmdShow) nCmdShow=SW_SHOWNORMAL;

  //Get program directory
  i=GetModuleFileName(NULL, szExeDir, MAX_PATH);
  while (i > 0 && szExeDir[i] != '\\') --i;
  szExeDir[i]='\0';

  if (hDlg=CreateDialog(hInstance, MAKEINTRESOURCE(IDD_PATH), 0, (DLGPROC)SelectPath))
    ShowWindow(hDlg, SW_SHOW);

  while (GetMessage(&msg, 0, 0, 0)) 
  {
    if (!IsDialogMessage(hDlg, &msg))
    {
      TranslateMessage(&msg);
      DispatchMessage(&msg);
    }
  }
  ExitProcess(0);
}

BOOL CALLBACK SelectPath(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
  char szBuf[MAX_PATH];
  HKEY hKey;
  DWORD dwType;
  DWORD dwSize;
  static HWND hOK;
  static HWND hPath;

  if (uMsg == WM_INITDIALOG)
  {
    szBuf[0]='\0';
    hOK=GetDlgItem(hWnd, IDOK);
    hPath=GetDlgItem(hWnd, IDC_PATH);

    if (RegOpenKeyEx(PROGRAM_REGROOT, PROGRAM_REGKEY, 0, KEY_READ, &hKey) == ERROR_SUCCESS)
    {
      dwSize=MAX_PATH;
      RegQueryValueEx(hKey, "", NULL, &dwType, (unsigned char *)&szBuf, &dwSize);
      RegCloseKey(hKey);
    }

    SetWindowText(hWnd, PROGRAM_CAPTION);
    SendMessage(hWnd, WM_SETICON, (WPARAM)ICON_BIG, (LPARAM)LoadIcon(hInstance, MAKEINTRESOURCE(IDI_ICON)));
    SendMessage(hPath, EM_LIMITTEXT, MAX_PATH, 0);
    SetWindowText(hPath, szBuf);
  }
  else if (uMsg == WM_COMMAND)
  {
    if (LOWORD(wParam) == IDC_PATH)
    {
      GetWindowText(hPath, szBuf, MAX_PATH);
      TrimBackslash(szBuf);
      lstrcat(szBuf, PROGRAM_FILE);
      EnableWindow(hOK, FileExists(szBuf));
      return TRUE;
    }
    else if (LOWORD(wParam) == IDC_BROWSE)
    {
      BROWSEINFO bi={0};
      LPITEMIDLIST pIdList;
      LPMALLOC pMalloc;

      GetWindowText(hPath, szBuf, MAX_PATH);
      bi.hwndOwner=hWnd;
      bi.pszDisplayName=szBuf;
      bi.lpszTitle=PROGRAM_BROWSETEXT;
      bi.ulFlags=BIF_RETURNONLYFSDIRS|BIF_NONEWFOLDERBUTTON|BIF_NEWDIALOGSTYLE;
      bi.lpfn=BrowseCallbackProc;
      bi.lParam=(LPARAM)szBuf;

      if (pIdList=SHBrowseForFolder(&bi))
      {
        SHGetPathFromIDList(pIdList, szBuf);

        if (SHGetMalloc(&pMalloc))
        {
          pMalloc->Free(pIdList);
          pMalloc->Release();
        }
        SetWindowText(hPath, szBuf);
      }
      return TRUE;
    }
    else if (LOWORD(wParam) == IDOK)
    {
      char szSourcePath[MAX_PATH];
      char szTargetPath[MAX_PATH];

      GetWindowText(hPath, szBuf, MAX_PATH);

      //Plugin installation
      wsprintf(szTargetPath, "%s\\Docs\\%s", szBuf, PROGRAM_NAME);
      if (!CopyFileWithMessages(hWnd, szExeDir, "Readme.html", szTargetPath, NULL))
        goto Quit;
      wsprintf(szSourcePath, "%s\\Example", szExeDir);
      wsprintf(szTargetPath, "%s\\Examples\\%s", szBuf, PROGRAM_NAME);
      if (!CopyFileWithMessages(hWnd, szSourcePath, PROGRAM_NAME "Test.nsi", szTargetPath, NULL))
        goto Quit;
      wsprintf(szSourcePath, "%s\\Include", szExeDir);
      wsprintf(szTargetPath, "%s\\Include", szBuf);
      if (!CopyFileWithMessages(hWnd, szSourcePath, PROGRAM_NAME ".nsh", szTargetPath, NULL))
        goto Quit;
      wsprintf(szSourcePath, "%s\\Plugin", szExeDir);
      wsprintf(szTargetPath, "%s\\Plugins", szBuf);
      if (!CopyFileWithMessages(hWnd, szSourcePath, PROGRAM_NAME ".dll", szTargetPath, NULL))
        goto Quit;

      DestroyWindow(hWnd);
	MessageBox(NULL, "Installation complite", PROGRAM_CAPTION, MB_OK|MB_ICONINFORMATION);
      PostQuitMessage(0);
      return TRUE;
    }
    else if (LOWORD(wParam) == IDCANCEL)
    {
      Quit:
      DestroyWindow(hWnd);
      PostQuitMessage(0);
      return TRUE;
    }
  }
  return FALSE;
}

int CALLBACK BrowseCallbackProc(HWND hWnd, UINT uMsg, LPARAM lParam, LPARAM lpData)
{
  char szPath[MAX_PATH];
  BOOL bEnable=FALSE;

  if (uMsg == BFFM_INITIALIZED || uMsg == BFFM_SELCHANGED)
  {
    if (uMsg == BFFM_INITIALIZED)
    {
      SendMessage(hWnd, BFFM_SETSELECTION, 1, lpData);
      lstrcpy(szPath, (char *)lpData);
    }
    else if (uMsg == BFFM_SELCHANGED)
    {
      SHGetPathFromIDList((LPITEMIDLIST)lParam, szPath);
    }

    if (*szPath)
    {
      TrimBackslash(szPath);
      lstrcat(szPath, PROGRAM_FILE);
      bEnable=FileExists(szPath);
    }
    SendMessage(hWnd, BFFM_ENABLEOK, 0, bEnable);
  }
  return 0;
}

BOOL CopyFileWithMessages(HWND hWnd, char *pSourcePath, char *pSourceFile, char *pTargetPath, char *pTargetFile)
{
  char szSource[MAX_PATH];
  char szTarget[MAX_PATH];
  char szTmp[MAX_PATH+32];
  int nChoice;

  wsprintf(szSource, "%s\\%s", pSourcePath, pSourceFile);

  if (!FileExists(szSource))
  {
    wsprintf(szTmp, "%s\n\nFile does not exists. Continue?", szSource);
    if (MessageBox(hWnd, szTmp, PROGRAM_CAPTION, MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2) == IDNO)
      return FALSE;
    return TRUE;
  }

  wsprintf(szTmp, "%s\\*.*", pTargetPath);

  if (!FileExists(szTmp))
  {
    CreateDirectory(pTargetPath, NULL);
  }

  wsprintf(szTarget, "%s\\%s", pTargetPath, pTargetFile?pTargetFile:pSourceFile);

  if (FileExists(szTarget))
  {
    wsprintf(szTmp, "%s\n\nFile already exists. Replace it?", szTarget);

    nChoice=MessageBox(hWnd, szTmp, PROGRAM_CAPTION, MB_YESNOCANCEL|MB_ICONQUESTION);

    if (nChoice == IDNO)
    {
      return TRUE;
    }
    else if (nChoice == IDCANCEL)
    {
      MessageBox(hWnd, "Installation aborted", PROGRAM_CAPTION, MB_OK|MB_ICONEXCLAMATION);
      return FALSE;
    }
  }
  CopyFile(szSource, szTarget, FALSE);

  return TRUE;
}

char* GetCommandLineParamsA()
{
  char *lpCmdLine=GetCommandLine();

  if (*lpCmdLine++ == '\"')
    while (*lpCmdLine != '\"' && *lpCmdLine != '\0') ++lpCmdLine;
  else
    while (*lpCmdLine != ' ' && *lpCmdLine != '\0') ++lpCmdLine;
  if (*lpCmdLine != '\0')
    while (*++lpCmdLine == ' ');

  return lpCmdLine;
}

BOOL FileExists(char *fname)
{
  WIN32_FIND_DATA wfd;
  HANDLE hFind;

  if ((hFind=FindFirstFile(fname, &wfd)) == INVALID_HANDLE_VALUE)
    return FALSE;

  FindClose(hFind);
  return TRUE;
}

void TrimBackslash(char *szPath)
{
  char *pPath=szPath + lstrlen(szPath) - 1;

  while (pPath >= szPath && *pPath == '\\') *pPath--='\0';
}
