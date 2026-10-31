// String conversions between ANSI, wide and TCHAR strings (replacement for the ATL macros
// T2CA / W2CT / CT2CW). They return std strings, so use .c_str() on the result; the
// temporary lives until the end of the full expression, the same as the ATL macros.
// Works for both Unicode and MBCS builds: a conversion to the type that is already used
// is just a copy.
#pragma once

#include <string>
#include <tchar.h>
#include <windows.h>

typedef std::basic_string<TCHAR> tstring;

inline std::string ToAnsi(const char* s) { return s ? s : ""; }

inline std::string ToAnsi(const wchar_t* s)
{
  if (!s || !*s) return std::string();
  int n = WideCharToMultiByte(CP_ACP, 0, s, -1, NULL, 0, NULL, NULL);
  if (n <= 1) return std::string();
  std::string out(n - 1, '\0');
  WideCharToMultiByte(CP_ACP, 0, s, -1, &out[0], n, NULL, NULL);
  return out;
}

inline std::wstring ToWide(const wchar_t* s) { return s ? s : L""; }

inline std::wstring ToWide(const char* s)
{
  if (!s || !*s) return std::wstring();
  int n = MultiByteToWideChar(CP_ACP, 0, s, -1, NULL, 0);
  if (n <= 1) return std::wstring();
  std::wstring out(n - 1, L'\0');
  MultiByteToWideChar(CP_ACP, 0, s, -1, &out[0], n);
  return out;
}

#ifdef _UNICODE
inline tstring ToTchar(const wchar_t* s) { return ToWide(s); }
#else
inline tstring ToTchar(const wchar_t* s) { return ToAnsi(s); }
#endif
