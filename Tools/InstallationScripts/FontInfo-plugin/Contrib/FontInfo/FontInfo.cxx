/*****************************************************************************\
**                                                                           **
**  FontInfo, Copyright (C) Anders Kjersem.                                  **
**                                                                           **
**  Licensed under the GNU General Public License v3.0 (the "License").      **
**  You may not use this file except in compliance with the License.         **
**                                                                           **
**  You can obtain a copy of the License at http://gnu.org/licenses/gpl-3.0  **
**                                                                           **
\*****************************************************************************/

/*
//docs.fileformat.com/font/ttf/
//developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6name.html
//www.microsoft.com/typography/otspec/#//learn.microsoft.com/en-us/typography/opentype/spec/
//thegeekpage.com/how-to-delete-the-font-cache-on-windows-10/

TODO:
https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6head.html & https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6bhed.html
https://docs.fileformat.com/font/fon/
	https://jeffpar.github.io/kbarchive/kb/065/Q65123/
	https://web.archive.org/web/20220510163159/http://www.csn.ul.ie/~caolan/publink/winresdump/winresdump/doc/resfmt.txt
*/

#ifdef _WIN64
#define _INTEGRAL_MAX_BITS 64
#endif
#include <windows.h>
#include <stdlib.h>
#ifdef TESTAPP
#include <stdio.h>
#endif
#ifndef UNICODE
#	include <tchar.h>
#	undef _stprintf
#	define _stprintf wsprintfA
#elif defined(PREFERNTDLL) && !defined(_WIN64) // 64-bit NTDLL has _wtoi64 but not wcstoll
# ifndef TCHAR
#		define TCHAR WCHAR
#	endif
#	ifndef _T
#		define _T TEXT
#	endif
#	undef _tcstol
#	define _tcstol wcstol
#	undef _stprintf
#	define _stprintf swprintf
#else
#	include <tchar.h>
#	include <wchar.h>
#endif
#ifndef _tcstoll
#	define _tcstoll _wcstoi64
#endif

typedef struct { UINT32 Version; UINT16 Num, SearchRange, EntrySelector, RangeShift; } TTTBLDIR;
typedef struct { UINT32 Tag, CheckSum, Offset, Size; } TTRECTBL;
typedef struct { UINT16 Ver, Num, Offset; } TTNAMTBL;
typedef struct { UINT16 PID, EID, LID, NID, Size, Offset; } TTNAMREC;
typedef struct { UINT32 Tag; UINT16 Maj, Min; UINT32 Num; } TTTTC;
#define TTMAKETAG(a,b,c,d) MAKELONG(MAKEWORD((a), (b)), MAKEWORD((c), (d)))

template<class T, class B, class O> T MkPtr(B b, O o) { return (T) ( ((char*)(b)) + (o) ); }
static void MFree(LPCVOID p) { if (sizeof(void*) > 4 || p) GlobalFree((HGLOBAL) p); }
template<class T> static T MAlloc(SIZE_T cb) { return (T) GlobalAlloc(GMEM_FIXED, cb); }
#define U16SWAP(v) MAKEWORD(HIBYTE(v), LOBYTE(v))
#define U16BE2HE(v) MAKEWORD(HIBYTE(v), LOBYTE(v))
#define U32BE2HE(v) MAKELONG(U16BE2HE(HIWORD(v)), U16BE2HE(LOWORD(v)))
#define IsWinNT() ( sizeof(TCHAR) > 1 || CharNextW(L"") != 0 )
template<class T> static FARPROC GetSysProc(T Mod, LPCSTR FN) { return GetProcAddress(LoadLibraryA(Mod), FN); }
static INT_PTR StrToIPtr(LPCTSTR Str)
{
	LPTSTR end;
	if (sizeof(*Str) == 1 && sizeof(void*) < 8)
	{
		FARPROC f = GetSysProc("MSVCRT", "strtol");
		if (!f) f = GetSysProc("CRTDLL", "strtol");
		return ((long(__cdecl*)(LPCTSTR, LPTSTR*, int))f)(Str, &end, 0);
	}
	else
	{
#ifdef _WIN64
		return _tcstoll(Str, &end, 0);
#else
		return _tcstol(Str, &end, 0);
#endif
	}
}

#define NPPUBEXPORT EXTERN_C __declspec(dllexport) void STDMETHODVCALLTYPE
#define NsisFree MFree
#ifndef NSISPIAPIVER_1_0
typedef struct { int autoclose, ctx, err, abort, bootflag, reqedboot, old, apiver, silent, direrr, rtl, errlvl, regsam, status; } exec_flags_t;
typedef struct {
	exec_flags_t*ef;
	int (WINAPI*ExecuteCodeSegment)(int, HWND);
	void (WINAPI*validate_filename)(LPTSTR);
	int (WINAPI*RegisterPluginCallback)(HMODULE, UINT_PTR(__cdecl*f)(int));
} extra_parameters;
typedef struct _stack_t { struct _stack_t *next; TCHAR text[1]; } stack_t;
#endif
static int NsisExecute(extra_parameters*pXP, UINT Addr) { return Addr != 0 ? pXP->ExecuteCodeSegment(Addr - 1, 0) : 0; }
static LPTSTR NsisGetVar(UINT cchNsis, LPTSTR Vars, UINT VId) { return MkPtr<LPTSTR>(Vars, cchNsis * VId * sizeof(*Vars)); }
static LPTSTR NsisSetVar(UINT cchNsis, LPTSTR Vars, UINT VId, LPCTSTR Str) { return lstrcpy(NsisGetVar(cchNsis, Vars, VId), Str); }
static stack_t*NsisStackPop(stack_t**pST)
{
	stack_t*p = *pST;
	return p ? (*pST = p->next, p) : p;
}
static INT_PTR NsisStackPopIntPtr(stack_t**pST)
{
	INT_PTR v;
	stack_t*p = NsisStackPop(pST);
	return p ? (v = StrToIPtr(p->text), NsisFree(p), v) : (SIZE_T) p;
}
static int NsisStackPopInt(stack_t**pST)
{
	return (int)(UINT) NsisStackPopIntPtr(pST);
}

static int GetUTF16BEString(BYTE*Start, UINT Size, LPWSTR Dst, UINT Cap)
{
	UINT cch = min(Size / 2, Cap);
	for (SIZE_T i = 0; i < cch; ++i) Dst[i] = U16BE2HE(MkPtr<UINT16*>(Start, 0)[i]);
	if (cch < Cap)
	{
		Dst[cch] = L'\0';
		return ++cch;
	}
	if (cch) Dst[cch - 1] = L'\0'; // Truncate
	return MAKELONG((Size / 2) + 1, ERROR_INSUFFICIENT_BUFFER * -1);
}

static int GetUTF16BEString(BYTE*Start, UINT Size, LPSTR Dst, UINT Cap)
{
	WCHAR *widebuf = MAlloc<WCHAR*>(Size + sizeof(WCHAR));
	if (!widebuf) return MAKELONG(0, ERROR_OUTOFMEMORY * -1);
	int code = GetUTF16BEString(Start, Size, widebuf, (Size + sizeof(WCHAR)) / 2);
	if (code >= 0)
	{
		UINT req = WideCharToMultiByte(CP_ACP, 0, widebuf, -1, 0, 0, 0, 0);
		code = WideCharToMultiByte(CP_ACP, 0, widebuf, -1, Dst, Cap, 0, 0);
		if (req != code)
		{
			if (Cap) Dst[min(Cap - 1, req)] = '\0'; // Truncate
			code = MAKELONG(req, ERROR_INSUFFICIENT_BUFFER * -1);
		}
	}
	else // Always true in the current implementation: if (HIWORD(code) == HIWORD(MAKELONG(0, ERROR_INSUFFICIENT_BUFFER * -1)))
	{
		UINT part = LOWORD(code);
		WideCharToMultiByte(CP_ACP, 0, widebuf, part ? part - 1 : part, Dst, Cap, 0, 0);
		code = MAKELONG(LOWORD(code) * 2, ERROR_INSUFFICIENT_BUFFER * -1); // Assume everything is MBCS
	}
	MFree(widebuf);
	return code;
}

static int GetCodepageString(BYTE*Start, UINT Size, LPWSTR Dst, UINT Cap, UINT CP)
{
	UINT cch = MultiByteToWideChar(CP, 0, (char*) Start, Size, Dst, Cap), trunc;
	if (!cch)
	{
		cch = MultiByteToWideChar(CP, 0, (char*) Start, Size, NULL, 0);
		return !Size ? 1 : MAKELONG(cch, ERROR_NO_UNICODE_TRANSLATION * -1);
	}
	if (cch < Cap)
	{
		Dst[cch] = L'\0';
		return ++cch;
	}
	trunc = min(cch, Cap);
	if (trunc) Dst[cch - 1] = L'\0'; // Truncate
	return MAKELONG(cch + 1, ERROR_INSUFFICIENT_BUFFER * -1);
}

static int GetCodepageString(BYTE*Start, UINT Size, LPSTR Dst, UINT Cap, UINT CP)
{
	if (Cap) Dst[0] = '\0';
	int code = GetCodepageString(Start, Size, (LPWSTR) 0, 0, CP);
	if (HIWORD(code) != HIWORD(MAKELONG(0, ERROR_INSUFFICIENT_BUFFER * -1))) return code;
	UINT widecch = LOWORD(code);
	WCHAR *widebuf = MAlloc<WCHAR*>(widecch * sizeof(WCHAR));
	if (!widebuf) return MAKELONG(0, ERROR_OUTOFMEMORY * -1);
	code = GetCodepageString(Start, Size, widebuf, widecch, CP);
	if (code > 0)
	{
		for (UINT i = 0; i < widecch; ++i) widebuf[i] = U16SWAP(widebuf[i]);
		code = GetUTF16BEString((BYTE*) widebuf, --code * 2, Dst, Cap);
	}
	MFree(widebuf);
	return code;
}

static int GetString(const TTNAMREC*NameRec, UINT32 StorageOffset, LPTSTR Dst, UINT Cap)
{
	BYTE*storage = MkPtr<BYTE*>(NameRec, StorageOffset);
	BYTE*start = storage + U16BE2HE(NameRec->Offset);
	UINT16 pid = U16BE2HE(NameRec->PID), eid = U16BE2HE(NameRec->EID), siz = U16BE2HE(NameRec->Size);
	if (pid == 0) // Unicode
	{
		if (eid <= 4) getutf16le:
		{
			return GetUTF16BEString(start, siz, Dst, Cap);
		}
	}
	if (pid == 3) // Windows
	{
		// TODO: How are we supposed to handle 0 AKA symbols encoding?!
		if (eid >= 2 && eid <= 6)
		{
			// TODO: Somehow guess if this is UTF-16 or (incorrectly) MBCS.
		}
		goto getutf16le;
	}
	if (pid == 1) // Macintosh
	{
		int code = GetCodepageString(start, siz, Dst, Cap, 10000 + eid), retry = -1;
		if (code <= 0 && sizeof(void*) < 8)
		{
			switch(eid)
			{
			case 0: retry = 1252; break; // Old Windows does not support the Mac. codepage. Using 1252 as Mac. Roman should at least somewhat work.
			}
			if (retry >= 0) retry = GetCodepageString(start, siz, Dst, Cap, retry); 
			if (retry >= 0) code = retry;
		}
		return code;
	}
	return MAKELONG(0, ERROR_CAN_NOT_COMPLETE * -1);
}

static const BYTE* MapFile(LPCTSTR File)
{
	HANDLE hFile = CreateFile(File, GENERIC_READ, IsWinNT() ? 7 : FILE_SHARE_READ|FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL|FILE_FLAG_SEQUENTIAL_SCAN, NULL);
	if (hFile == INVALID_HANDLE_VALUE) return 0;
	HANDLE hMap =  CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
	CloseHandle(hFile);
	if (!hMap) return 0;
	BYTE *pView = (BYTE*) MapViewOfFile(hMap, FILE_MAP_READ, 0, 0, 0);
	CloseHandle(hMap);
	return pView;
}

static void* FindTableByTag(const BYTE*RelBase, UINT32 Tag, UINT32&Size, const BYTE*FileBase = 0)
{
	const TTTBLDIR *pTD = MkPtr<TTTBLDIR*>(RelBase, 0);
	UINT32 ver = U32BE2HE(pTD->Version), num = U16BE2HE(pTD->Num);
	TTRECTBL *pR = MkPtr<TTRECTBL*>(pTD, sizeof(TTTBLDIR));
	if (ver == 0x00010000)
	{
		for (SIZE_T i = 0; i < num; ++i)
		{
			if (pR[i].Tag == Tag)
			{
				Size = U32BE2HE(pR[i].Size);
				return MkPtr<void*>(FileBase ? FileBase : RelBase, U32BE2HE(pR[i].Offset));
			}
		}
	}

	const TTTTC *pTTC = MkPtr<TTTTC*>(RelBase, 0);
	if (!FileBase && pTTC->Tag == TTMAKETAG('t','t','c','f'))
	{
		num = U32BE2HE(pTTC->Num);
		if (U16BE2HE(pTTC->Maj) - 1 <= 2 - 1) // v1..2
		{
			UINT32 *pOffsets = MkPtr<UINT32*>(pTTC, 4+2+2+4);
			for (SIZE_T i = 0; i < num; ++i)
			{
				void *ret = FindTableByTag(MkPtr<BYTE*>(RelBase, U32BE2HE(pOffsets[i])), Tag, Size, RelBase);
				if (ret) return ret;
			}
		}
	}

	//TODO: 'OTTO', 'true' and 'typ1'
	return 0;
}

enum { ANYPID = -1, ANYLID = -1 };
static TTNAMREC* FindNameRecord(const BYTE*Base, UINT32&StorageOffset, UINT16 NID, int LID, int PID)
{
	UINT32 ntsiz;
	TTNAMTBL*pNT = (TTNAMTBL*) FindTableByTag(Base, TTMAKETAG('n','a','m','e'), ntsiz);
	if (!pNT || ntsiz < sizeof(TTNAMTBL)) return 0;
	UINT16 ntver = U16BE2HE(pNT->Ver), num = U16BE2HE(pNT->Num), offset = U16BE2HE(pNT->Offset);
	BYTE *pStorage = MkPtr<BYTE*>(pNT, offset);
	TTNAMREC *pNR = MkPtr<TTNAMREC*>(pNT, FIELD_OFFSET(TTNAMTBL, Offset)+sizeof(UINT16));
	if (ntver <= 1)
	{
		for (SIZE_T i = 0; i < num; ++i)
		{
			if (MkPtr<SIZE_T>(&pNR[i], sizeof(TTNAMREC)) > MkPtr<SIZE_T>(pNT, ntsiz)) break;
			if (U16BE2HE(pNR[i].NID) == NID && (PID == ANYPID || U16BE2HE(pNR[i].PID) == PID))
			{
				if (LID == ANYLID || U16BE2HE(pNR[i].LID) == LID)
				{
					StorageOffset = UINT32((SIZE_T) pStorage - (SIZE_T) &pNR[i]);
					return &pNR[i];
				}
			}
		}
	}
	return 0;
}

static TTNAMREC* FindNameRecord(const BYTE*Base, UINT32&StorageOffset, UINT16 NID, int LID = ANYLID)
{
	static const UINT16 pids[] = { 0, 3, 1 };
	for (SIZE_T i = 0; i < ARRAYSIZE(pids); ++i)
	{
		TTNAMREC*p = FindNameRecord(Base, StorageOffset, NID, LID, pids[i]);
		if (p) return p;
	}
	return 0;
}

static void EnumNameRecords(const BYTE*Base, UINT cchNsis, PTSTR Vars, extra_parameters*pXP, UINT Callback)
{
	UINT32 ntsiz;
	TTNAMTBL*pNT = (TTNAMTBL*) FindTableByTag(Base, TTMAKETAG('n','a','m','e'), ntsiz);
	if (!pNT || ntsiz < sizeof(TTNAMTBL)) return ;
	UINT16 ntver = U16BE2HE(pNT->Ver), num = U16BE2HE(pNT->Num), offset = U16BE2HE(pNT->Offset);
	BYTE *pStorage = MkPtr<BYTE*>(pNT, offset);
	TTNAMREC *pNR = MkPtr<TTNAMREC*>(pNT, FIELD_OFFSET(TTNAMTBL, Offset)+sizeof(UINT16));
	LPTSTR v0 = NsisGetVar(cchNsis, Vars, 0);
	LPTSTR v1 = NsisGetVar(cchNsis, Vars, 1);
	LPTSTR v2 = NsisGetVar(cchNsis, Vars, 2);
	if (ntver <= 1)
	{
		for (SIZE_T i = 0; i < num; ++i)
		{
			if (MkPtr<SIZE_T>(&pNR[i], sizeof(TTNAMREC)) > MkPtr<SIZE_T>(pNT, ntsiz)) break;
			UINT strstorofs = UINT32((SIZE_T) pStorage - (SIZE_T) &pNR[i]);
			UINT pid = U16BE2HE(pNR[i].PID), eid = U16BE2HE(pNR[i].EID), lid = U16BE2HE(pNR[i].LID), nid = U16BE2HE(pNR[i].NID);
			_stprintf(v0, _T("%u"), nid);
			_stprintf(v1, _T("%.4x%.4x%.4x"), pid, eid, lid); // TODO: Append the lang tag string for TTNAMTBL v1

			*v2 = '\0';
			if (pid <= 3 && pid != 2) GetString(&pNR[i], strstorofs, v2, cchNsis);
			
			NsisExecute(pXP, Callback);
			if (!*v0) break; // Abort enum requested
		}
	}
}

NPPUBEXPORT EnumNameStrings(HWND hWndNsis, UINT cchNsis, PTSTR Vars, stack_t **ppST, extra_parameters*pXP, ...)
{
	stack_t*path = NsisStackPop(ppST);
	UINT callbackfunc = NsisStackPopInt(ppST);
	const BYTE *pView = MapFile(path->text), *failed = pView;
	if (pView)
	{
		EnumNameRecords(pView, cchNsis, Vars, pXP, callbackfunc);
		UnmapViewOfFile(pView);
	}
	NsisFree(path);
}

NPPUBEXPORT GetFontName(HWND hWndNsis, UINT cchNsis, PTSTR Vars, stack_t **ppST, extra_parameters*pXP, ...)
{
	stack_t*path = NsisStackPop(ppST);
	const BYTE *pView = MapFile(path->text), *failed = pView;
	LPTSTR v0 = NsisGetVar(cchNsis, Vars, 0);
	if (pView)
	{
		UINT32 offset;
		TTNAMREC*pNR = FindNameRecord(pView, offset, 4);
		if (pNR)
		{
			int code = GetString(pNR, offset, v0, cchNsis);
			if (code >= 0) failed = false;
		}
		UnmapViewOfFile(pView);
	}
	if (failed)
	{
		pXP->ef->err++;
		*v0 = '\0';
	}
	NsisFree(path);
}

#ifdef TESTAPP
static int Test()
{
	TCHAR path[MAX_PATH];
	wsprintf(&path[GetWindowsDirectory(path, ARRAYSIZE(path))], TEXT("\\Fonts\\Arial.ttf")); // CascadiaCodePL.ttf Arial.ttf cambria.ttc
	const BYTE *pView = MapFile(path);
	if (!pView) return printf("MapFile failed %d\n", GetLastError());

	UINT32 offset;
	TTNAMREC*pNR = FindNameRecord(pView, offset, 4);
	if (pNR)
	{
		TCHAR buf[1024];
		int cch = GetString(pNR, offset, buf, ARRAYSIZE(buf));
		wprintf(sizeof(TCHAR) > 1 ? L"%d|%ls| %d\n" : L"%d|%hs| %d\n", cch, buf, LOWORD(cch));
	}

	return UnmapViewOfFile(pView);
}

EXTERN_C DECLSPEC_NORETURN void __cdecl mainCRTStartup()
{
	exit(Test());
}
#else
EXTERN_C BOOL WINAPI _DllMainCRTStartup(HMODULE hMod, DWORD, void*)
{
	return true;
}
#endif