/* 
 *	Copyright (C) 2006-2018 Team MediaPortal
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

#include "RegistryUtil.h"

extern void LogDebug(const char *fmt, ...) ;

//Initialise the 'static' shared variables
bool CRegistryUtil::m_bPassThruISO6937 = false;
bool CRegistryUtil::m_bNoGeneralInGenre = false;

CRegistryUtil::CRegistryUtil(void)
{
}

CRegistryUtil::~CRegistryUtil(void)
{
}

void CRegistryUtil::ReadSettingsFromReg()
{
  //Read (and create if needed) debug registry settings
  //Note that HKEY_CURRENT_USER is mapped to 'HKEY_USERS\.DEFAULT' for the LocalSystem Account (which TVServer normally runs under)
  HKEY key;
  if (ERROR_SUCCESS==RegCreateKeyEx(HKEY_CURRENT_USER, _T("Software\\Team MediaPortal\\TsWriter"), 0, NULL, 
                                    REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &key, NULL))
  {
    DWORD keyValue = 0;
    LPCTSTR passThruISO6937 = _T("PassThruISO6937");
    ReadRegistryKeyDword(key, passThruISO6937, keyValue);
    if (keyValue)
    {
      m_bPassThruISO6937 = true;
    }
    else
    {
      m_bPassThruISO6937 = false;
    }
    keyValue = 0;
    LPCTSTR noGeneralInGenre = _T("NoGeneralInGenre");
    ReadRegistryKeyDword(key, noGeneralInGenre, keyValue);
    if (keyValue)
    {
      m_bNoGeneralInGenre = true;
    }
    else
    {
      m_bNoGeneralInGenre = false;
    }     
    RegCloseKey(key);
    LogDebug("CRegistryUtil::ReadSettingsFromReg(), PassThruISO6937 = %d, NoGeneralInGenre = %d", m_bPassThruISO6937, m_bNoGeneralInGenre);
  }
  else
  {
    LogDebug("CRegistryUtil::ReadSettingsFromReg() **FAILED**, PassThruISO6937 = %d, NoGeneralInGenre = %d", m_bPassThruISO6937, m_bNoGeneralInGenre);
  }
}

void CRegistryUtil::ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{
  USES_CONVERSION;
  DWORD dwSize = sizeof(DWORD);
  DWORD dwType = REG_DWORD;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)&data, &dwSize);
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      LogDebug("Create default value for: %s", T2A(lpSubKey));
      WriteRegistryKeyDword(hKey, lpSubKey, data);
    }
    else
    {
      LogDebug("Faíled to create default value for: %s", T2A(lpSubKey));
    }
  }
}

void CRegistryUtil::WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{  
  USES_CONVERSION;
  DWORD dwSize = sizeof(DWORD);
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_DWORD, (LPBYTE)&data, dwSize);
  if (result == ERROR_SUCCESS) 
  {
    LogDebug("Success writing to Registry: %s", T2A(lpSubKey));
  } 
  else 
  {
    LogDebug("Error writing to Registry - subkey: %s error: %d", T2A(lpSubKey), result);
  }
}

LONG CRegistryUtil::ReadOnlyRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{
  USES_CONVERSION;
  DWORD dwSize = sizeof(DWORD);
  DWORD dwType = REG_DWORD;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)&data, &dwSize);
  return error;
}

void CRegistryUtil::ReadRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{
  USES_CONVERSION;

  DWORD dwSize = MAX_REG_LENGTH;
  DWORD dwType = REG_SZ;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)data, &dwSize);
  
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      LogDebug("   create default value for %s", T2A(lpSubKey));
      WriteRegistryKeyString(hKey, lpSubKey, data);
    }
    else if (error == ERROR_MORE_DATA)
      LogDebug("   too much data, corrupted registry setting(?):  %s", T2A(lpSubKey));
    else
      LogDebug("   error: %d subkey: %s", error, T2A(lpSubKey));
  }
}

void CRegistryUtil::WriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{  
  USES_CONVERSION;

  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_SZ, (LPBYTE)data, _tcslen(data) * sizeof(TCHAR));
  if (result == ERROR_SUCCESS) 
    LogDebug("Success writing to Registry: %s", T2A(lpSubKey));
  else 
    LogDebug("Error writing to Registry - subkey: %s error: %d", T2A(lpSubKey), result);
}

