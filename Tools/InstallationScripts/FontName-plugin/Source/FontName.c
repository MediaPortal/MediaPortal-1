///////////////////////////////////////////////////////////////////////////////
//
// NSIS Plugin Writen by Vytautas Krivickas based on exDll plugin and a modified
// version of GetFontProperties() function.
//
////////////////////////////////////////////////////////////////////////////////

#include <windows.h>
#include <stdio.h>
#include "FontName.h"
#include "XFont.h"


HINSTANCE g_hInstance;

HWND g_hwndParent;

LANGUAGE_STRINGS g_LanguageStrings;

void TranslateStrings();
void GetFontProperties(char * lpszFilePath);

void __declspec(dllexport) name(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  g_hwndParent=hwndParent;

  EXDLL_INIT();

  {
    TranslateStrings();

    char sFilePath[1024];
    popstring(sFilePath);
    GetFontProperties(sFilePath);
  }
}

void __declspec(dllexport) version(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  g_hwndParent=hwndParent;

  EXDLL_INIT();

  char msg[1024];

  {
    TranslateStrings();

    sprintf(msg, g_LanguageStrings.csVersion, "0.6");
    pushstring(msg);
  }
}

BOOL WINAPI _DllMainCRTStartup(HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
  g_hInstance=hInst;
	return TRUE;
}

void TranslateStrings()
{
  char sTemp[1024];

  popstring(sTemp);
  memcpy(g_LanguageStrings.csVersion,          sTemp, 1024);
  popstring(sTemp);
  memcpy(g_LanguageStrings.csErrorOpen,        sTemp, 1024);
  popstring(sTemp);
  memcpy(g_LanguageStrings.csErrorSize,        sTemp, 1024);
  popstring(sTemp);
  memcpy(g_LanguageStrings.csErrorMappedFile,  sTemp, 1024);
  popstring(sTemp);
  memcpy(g_LanguageStrings.csErrorMapAddress,  sTemp, 1024);
  popstring(sTemp);
  memcpy(g_LanguageStrings.csErrorFontVersion, sTemp, 1024);
}

///////////////////////////////////////////////////////////////////////////////
//
// GetFontProperties()
//
// Purpose:     Get font name from font file
//
// Parameters:  lpszFilePath - file path of font file
//
// Returns:     void
//
///////////////////////////////////////////////////////////////////////////////
void GetFontProperties(char * lpszFilePath) //LPCTSTR
{
	FONT_PROPERTIES_ANSI fp;
	FONT_PROPERTIES_ANSI * lpFontProps = &fp;
    char errormsg[1024];

	memset(lpFontProps, 0, sizeof(FONT_PROPERTIES_ANSI));

	HANDLE hFile = INVALID_HANDLE_VALUE;
	hFile = CreateFile(lpszFilePath,
	        		     GENERIC_READ,
						 0,
						 NULL,
						 OPEN_ALWAYS,
						 FILE_ATTRIBUTE_NORMAL  | FILE_FLAG_SEQUENTIAL_SCAN,
						 NULL);

	if (hFile == INVALID_HANDLE_VALUE)
	{
	    DWORD dw = GetLastError();
        sprintf(errormsg, g_LanguageStrings.csErrorOpen, dw);
	    pushstring(errormsg);
	    pushstring("*:*");
		return;
	}

	// get the file size
	DWORD dwFileSize = GetFileSize(hFile, NULL);

	if (dwFileSize == INVALID_FILE_SIZE)
	{
    	DWORD dw = GetLastError();
        sprintf(errormsg, g_LanguageStrings.csErrorSize, dw);
	    pushstring(errormsg);
	    pushstring("*:*");
		CloseHandle(hFile);
		return;
	}

	// Create a file mapping object that is the current size of the file
	HANDLE hMappedFile = NULL;
	hMappedFile = CreateFileMapping(hFile,
									  NULL,
									  PAGE_READONLY,
									  0,
									  dwFileSize,
									  NULL);

	if (hMappedFile == NULL)
	{
       	DWORD dw = GetLastError();
        sprintf(errormsg, g_LanguageStrings.csErrorMappedFile, dw);
	    pushstring(errormsg);
	    pushstring("*:*");
		CloseHandle(hFile);
		return;
	}

	LPBYTE lpMapAddress = (LPBYTE) MapViewOfFile(hMappedFile,		// handle to file-mapping object
											FILE_MAP_READ,			// access mode
											0,						// high-order DWORD of offset
											0,						// low-order DWORD of offset
											0);						// number of bytes to map

	if (lpMapAddress == NULL)
	{
	    DWORD dw = GetLastError();
        sprintf(errormsg, g_LanguageStrings.csErrorMapAddress, dw);
	    pushstring(errormsg);
	    pushstring("*:*");
		CloseHandle(hMappedFile);
		CloseHandle(hFile);
		return;
	}

	BOOL bRetVal = FALSE;
	int index = 0;

	TT_OFFSET_TABLE ttOffsetTable;
	memcpy(&ttOffsetTable, &lpMapAddress[index], sizeof(TT_OFFSET_TABLE));
	index += sizeof(TT_OFFSET_TABLE);

	ttOffsetTable.uNumOfTables  = SWAPWORD(ttOffsetTable.uNumOfTables);
	ttOffsetTable.uMajorVersion = SWAPWORD(ttOffsetTable.uMajorVersion);
	ttOffsetTable.uMinorVersion = SWAPWORD(ttOffsetTable.uMinorVersion);

	//check if this is a true type font and the version is 1.0
	if (ttOffsetTable.uMajorVersion != 1 || ttOffsetTable.uMinorVersion != 0)
	{
	    memcpy (errormsg, g_LanguageStrings.csErrorFontVersion, 1024);
	    pushstring(errormsg);
	    pushstring("*:*");
		return;
	}

	TT_TABLE_DIRECTORY tblDir;
	memset(&tblDir, 0, sizeof(TT_TABLE_DIRECTORY));
	BOOL bFound = FALSE;
	char szTemp[4096];
	memset(szTemp, 0, sizeof(szTemp));

    int i;

	for (i = 0; i < ttOffsetTable.uNumOfTables; i++)
	{
		memcpy(&tblDir, &lpMapAddress[index], sizeof(TT_TABLE_DIRECTORY));
		index += sizeof(TT_TABLE_DIRECTORY);

		if (_memicmp(tblDir.szTag, "name", 4) == 0)
		{
			bFound = TRUE;
			tblDir.uLength = SWAPLONG(tblDir.uLength);
			tblDir.uOffset = SWAPLONG(tblDir.uOffset);
			break;
		}
		else if (tblDir.szTag[0] == 0)
		{
			break;
		}
	}

	if (bFound)
	{
		index = tblDir.uOffset;

		TT_NAME_TABLE_HEADER ttNTHeader;
		memcpy(&ttNTHeader, &lpMapAddress[index], sizeof(TT_NAME_TABLE_HEADER));
		index += sizeof(TT_NAME_TABLE_HEADER);

		ttNTHeader.uNRCount = SWAPWORD(ttNTHeader.uNRCount);
		ttNTHeader.uStorageOffset = SWAPWORD(ttNTHeader.uStorageOffset);
		TT_NAME_RECORD ttRecord;
		bFound = FALSE;

		for (i = 0;
			 i < ttNTHeader.uNRCount &&
			 (lpFontProps->csName[0] == 0);
			 i++)
		{
			memcpy(&ttRecord, &lpMapAddress[index], sizeof(TT_NAME_RECORD));
			index += sizeof(TT_NAME_RECORD);

			ttRecord.uNameID = SWAPWORD(ttRecord.uNameID);
			ttRecord.uStringLength = SWAPWORD(ttRecord.uStringLength);
			ttRecord.uStringOffset = SWAPWORD(ttRecord.uStringOffset);

			if (ttRecord.uNameID == 4)
			{
				int nPos = index;

				index = tblDir.uOffset + ttRecord.uStringOffset + ttNTHeader.uStorageOffset;

				memset(szTemp, 0, sizeof(szTemp));

				memcpy(szTemp, &lpMapAddress[index], ttRecord.uStringLength);
				index += ttRecord.uStringLength;

				if (szTemp[0] != 0)
				{
					switch (ttRecord.uNameID)
					{
						case 4:
							if (lpFontProps->csName[0] == 0)
								memcpy(lpFontProps->csName, szTemp, 1024);
							break;

						default:
							break;
					}
				}
				index = nPos;
			}
		}
	}

	UnmapViewOfFile(lpMapAddress);
	CloseHandle(hMappedFile);
	CloseHandle(hFile);

    pushstring(lpFontProps->csName);
	return;
}
