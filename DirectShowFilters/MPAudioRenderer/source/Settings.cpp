// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#include "stdafx.h"
#include "Settings.h"

extern void Log(const char *fmt, ...);

AudioRendererSettings::AudioRendererSettings() : 
  m_bLogSampleTimes(false),
  m_bUseWASAPI(true),
  m_bUseTimeStretching(false),
  m_bEnableAC3Encoding(false),
  m_hnsPeriod(0),
  m_WASAPIShareMode(AUDCLNT_SHAREMODE_EXCLUSIVE),
  m_wWASAPIPreferredDeviceId(NULL)
{
  LoadSettingsFromRegistry();
}

AudioRendererSettings::~AudioRendererSettings()
{
  delete[] m_wWASAPIPreferredDeviceId;
}

void AudioRendererSettings::LoadSettingsFromRegistry()
{
  USES_CONVERSION; // this is required for T2W macro
  
  Log("Loading settings from registry");

  LPCTSTR folder = TEXT("Software\\Team MediaPortal\\Audio Renderer");

  HKEY hKey;
  char* lpData = new char[MAX_REG_LENGTH];

  // Registry setting names
  LPCTSTR forceDirectSound = TEXT("ForceDirectSound");
  LPCTSTR enableTimestretching = TEXT("EnableTimestretching");
  LPCTSTR WASAPIExclusive = TEXT("WASAPIExclusive");
  LPCTSTR devicePeriod = TEXT("DevicePeriod");
  LPCTSTR enableAC3Encoding = TEXT("EnableAC3Encoding");
  LPCTSTR logSampleTimes = TEXT("LogSampleTimes");
  LPCTSTR WASAPIPreferredDevice = TEXT("WASAPIPreferredDevice");
  
  // Default values for the settings in registry
  DWORD forceDirectSoundData = 0;
  DWORD enableTimestretchingData = 1;
  DWORD WASAPIExclusiveData = 1;
  DWORD devicePeriodData = 500000; // 50 ms
  DWORD enableAC3EncodingData = 0;
  DWORD logSampleTimesData = 0;
  LPCTSTR WASAPIPreferredDeviceData = new TCHAR[MAX_REG_LENGTH];

  ZeroMemory((void*)WASAPIPreferredDeviceData, MAX_REG_LENGTH);

  // Try to access the setting root "Software\Team MediaPortal\Audio Renderer"
  RegOpenKeyEx(HKEY_CURRENT_USER, folder, NULL, KEY_ALL_ACCESS, &hKey);

  if (hKey)
  {
    // Read settings from registry
    ReadRegistryKeyDword(hKey, forceDirectSound, forceDirectSoundData);
    ReadRegistryKeyDword(hKey, enableTimestretching, enableTimestretchingData);
    ReadRegistryKeyDword(hKey, WASAPIExclusive, WASAPIExclusiveData);
    ReadRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
    ReadRegistryKeyDword(hKey, enableAC3Encoding, enableAC3EncodingData);
    ReadRegistryKeyDword(hKey, logSampleTimes, logSampleTimesData);
    ReadRegistryKeyString(hKey, WASAPIPreferredDevice, WASAPIPreferredDeviceData);

    Log("   ForceDirectSound:        %d", forceDirectSoundData);
    Log("   EnableTimestrecthing:    %d", enableTimestretchingData);
    Log("   WASAPIExclusive:         %d", WASAPIExclusiveData);
    Log("   EnableAC3Encoding:       %d", enableAC3EncodingData);
    Log("   LogSampleTimes:          %d", logSampleTimesData);
    Log("   DevicePeriod:            %d (1 == minimal, 0 == default, other user defined)", devicePeriodData);
    Log("   WASAPIPreferredDevice:   %s", WASAPIPreferredDeviceData);

    if (forceDirectSoundData > 0)
      m_bUseWASAPI = false;
    else
      m_bUseWASAPI = true;

    if (enableTimestretchingData > 0)
      m_bUseTimeStretching = true;
    else
      m_bUseTimeStretching = false;

    if (WASAPIExclusiveData > 0)
      m_WASAPIShareMode = AUDCLNT_SHAREMODE_EXCLUSIVE;
    else
      m_WASAPIShareMode = AUDCLNT_SHAREMODE_SHARED;

   if (enableAC3EncodingData > 0)
      m_bEnableAC3Encoding = true;
    else
      m_bEnableAC3Encoding = false;

    if (logSampleTimesData > 0)
      m_bLogSampleTimes = true;
    else
      m_bLogSampleTimes = false;

    m_hnsPeriod = devicePeriodData;

    delete[] m_wWASAPIPreferredDeviceId;
    m_wWASAPIPreferredDeviceId = new WCHAR[MAX_REG_LENGTH];
    
    wcsncpy(m_wWASAPIPreferredDeviceId, T2W(WASAPIPreferredDeviceData), MAX_REG_LENGTH);

    delete[] WASAPIPreferredDeviceData;
  }

  else // no settings in registry, create default values
  {
    Log("Failed to open %s", folder);
    Log("Initializing registry with default settings");

    LONG result = RegCreateKeyEx(HKEY_CURRENT_USER, folder, 0, NULL, REG_OPTION_NON_VOLATILE,
                                  KEY_ALL_ACCESS, NULL, &hKey, NULL);

    if (result == ERROR_SUCCESS) 
    {
      Log("Success creating master key");
      WriteRegistryKeyDword(hKey, forceDirectSound, forceDirectSoundData);
      WriteRegistryKeyDword(hKey, enableTimestretching, enableTimestretchingData);
      WriteRegistryKeyDword(hKey, WASAPIExclusive, WASAPIExclusiveData);
      WriteRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
      WriteRegistryKeyDword(hKey, enableAC3Encoding, enableAC3EncodingData);
      WriteRegistryKeyDword(hKey, logSampleTimes, logSampleTimesData);
    } 
    else 
    {
      Log("Error creating master key %d", result);
    }
  }
  
  delete[] lpData;
  RegCloseKey (hKey);
}

void AudioRendererSettings::ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{
  DWORD dwSize = sizeof(DWORD);
  DWORD dwType = REG_DWORD;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)&data, &dwSize);
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      Log("   create default value for %s", lpSubKey);
      WriteRegistryKeyDword(hKey, lpSubKey, data);
    }
    else
    {
      Log("   faíled to create default value for %s", lpSubKey);
    }
  }
}

void AudioRendererSettings::WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{  
  DWORD dwSize = sizeof(DWORD);
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_DWORD, (LPBYTE)&data, dwSize);
  if (result == ERROR_SUCCESS) 
  {
    Log("Success writing to Registry: %s", lpSubKey);
  } 
  else 
  {
    Log("Error writing to Registry - subkey: %s error: %d", lpSubKey, result);
  }
}

void AudioRendererSettings::ReadRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{
  DWORD dwSize = MAX_REG_LENGTH;
  DWORD dwType = REG_SZ;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)data, &dwSize);
  
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      Log("   create default value for %s", lpSubKey);
      WriteRegistryKeyString(hKey, lpSubKey, data);
    }
    else if (error == ERROR_MORE_DATA)
    {
      Log("   too much data, corrupted registry setting(?):  %s", lpSubKey);      
    }
    else
    {
      Log("   error: %d subkey: %s", error, lpSubKey);       
    }
  }
}

void AudioRendererSettings::WriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{  
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_SZ, (LPBYTE)data, strlen(data)+1);
  if (result == ERROR_SUCCESS) 
  {
    Log("Success writing to Registry: %s", lpSubKey);
  } 
  else 
  {
    Log("Error writing to Registry - subkey: %s error: %d", lpSubKey, result);
  }
}