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
extern void LogRotate();

int gAllowedAC3bitrates[9] = {192, 224, 256, 320, 384, 448, 512, 576, 640};

AudioRendererSettings::AudioRendererSettings() :
  m_bLogSampleTimes(false),
  m_bHWBasedRefClock(true),
  m_bEnableSyncAdjustment(true),
  m_bUseWASAPI(true),
  m_WASAPIUseEventMode(true),
  m_bUseTimeStretching(false),
  m_bEnableAC3Encoding(false),
  m_hnsPeriod(0),
  m_AC3bitrate(448), 
  m_dMaxBias(1.1),
  m_dMinBias(0.9),
  m_lAudioDelay(0),
  m_dwChannelMaskOverride_5_1(0),
  m_dwChannelMaskOverride_7_1(0),
  m_WASAPIShareMode(AUDCLNT_SHAREMODE_EXCLUSIVE),
  m_wWASAPIPreferredDeviceId(NULL)
{
  LogRotate();
  Log("MP Audio Renderer - v0.994");

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
  LPCTSTR WASAPIUseEventMode = TEXT("WASAPIUseEventMode");
  LPCTSTR devicePeriod = TEXT("DevicePeriod");
  LPCTSTR enableAC3Encoding = TEXT("EnableAC3Encoding");
  LPCTSTR AC3bitrate = TEXT("AC3bitrate");
  LPCTSTR maxBias = TEXT("MaxBias");
  LPCTSTR minBias = TEXT("MinBias");
  LPCTSTR audioDelay = TEXT("AudioDelay");
  LPCTSTR channelMaskOverride_5_1 = TEXT("ChannelMaskOverride_5_1");
  LPCTSTR channelMaskOverride_7_1 = TEXT("ChannelMaskOverride_7_1");
  LPCTSTR logSampleTimes = TEXT("LogSampleTimes");
  LPCTSTR WASAPIPreferredDevice = TEXT("WASAPIPreferredDevice");
  LPCTSTR HWBasedRefClock = TEXT("HWBasedRefClock");
  LPCTSTR enableSyncAdjustment = TEXT("EnableSyncAdjustment");
  
  // Default values for the settings in registry
  DWORD forceDirectSoundData = 0;
  DWORD enableTimestretchingData = 1;
  DWORD WASAPIExclusiveData = 1;
  DWORD WASAPIUseEventModeData = 1;
  DWORD devicePeriodData = 500000;  // 50 ms
  DWORD enableAC3EncodingData = 0;
  DWORD AC3bitrateData = 448;       // maximum based on the DVD spec
  DWORD maxBiasData = 11000;        // divide with 10000 to get real double value
  DWORD minBiasData = 9000;         // divide with 10000 to get real double value
  DWORD audioDelayData = 0;         // in ms
  DWORD channelMaskOverride_5_1Data = 0;
  DWORD channelMaskOverride_7_1Data = 0;
  DWORD logSampleTimesData = 0;
  DWORD HWBasedRefClockData = 1;
  DWORD enableSyncAdjustmentData = 1;
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
    ReadRegistryKeyDword(hKey, WASAPIUseEventMode, WASAPIUseEventModeData);
    ReadRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
    ReadRegistryKeyDword(hKey, enableAC3Encoding, enableAC3EncodingData);
    ReadRegistryKeyDword(hKey, AC3bitrate, AC3bitrateData);
    ReadRegistryKeyDword(hKey, maxBias, maxBiasData);
    ReadRegistryKeyDword(hKey, minBias, minBiasData);
    ReadRegistryKeyDword(hKey, audioDelay, audioDelayData);
    ReadRegistryKeyDword(hKey, channelMaskOverride_5_1, channelMaskOverride_5_1Data);
    ReadRegistryKeyDword(hKey, channelMaskOverride_7_1, channelMaskOverride_7_1Data);
    ReadRegistryKeyDword(hKey, logSampleTimes, logSampleTimesData);
    ReadRegistryKeyDword(hKey, HWBasedRefClock, HWBasedRefClockData);
    ReadRegistryKeyDword(hKey, enableSyncAdjustment, enableSyncAdjustmentData);
    ReadRegistryKeyString(hKey, WASAPIPreferredDevice, WASAPIPreferredDeviceData);

    Log("   ForceDirectSound:        %d", forceDirectSoundData);
    Log("   EnableTimestrecthing:    %d", enableTimestretchingData);
    Log("   WASAPIExclusive:         %d", WASAPIExclusiveData);
    Log("   WASAPIUseEventMode:      %d", WASAPIUseEventModeData);
    Log("   EnableAC3Encoding:       %d", enableAC3EncodingData);
    Log("   AC3bitrate:              %d", AC3bitrateData);
    Log("   MaxBias:                 %d", maxBiasData);
    Log("   MinBias:                 %d", minBiasData);
    Log("   AudioDelay:              %d", audioDelayData);
    Log("   ChannelMaskOverride_5_1: %d", channelMaskOverride_5_1Data);
    Log("   ChannelMaskOverride_7_1: %d", channelMaskOverride_7_1Data);
    Log("   LogSampleTimes:          %d", logSampleTimesData);
    Log("   HWBasedRefClock:         %d", HWBasedRefClockData);
    Log("   EnableSyncAdjustment:    %d", enableSyncAdjustmentData);
    Log("   DevicePeriod:            %d (1 == minimal, 0 == driver default, other user defined)", devicePeriodData);
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

    if (WASAPIUseEventModeData > 0)
      m_WASAPIUseEventMode = true;
    else
      m_WASAPIUseEventMode = false;

    if (enableAC3EncodingData > 0)
      m_bEnableAC3Encoding = true;
    else
      m_bEnableAC3Encoding = false;

    m_dMaxBias = (double)maxBiasData / 10000.0;
    m_dMinBias = (double)minBiasData / 10000.0;

    m_lAudioDelay = audioDelayData;

    if (channelMaskOverride_5_1Data > 0)
      m_dwChannelMaskOverride_5_1 = channelMaskOverride_5_1Data;
    else
      m_dwChannelMaskOverride_5_1 = 0;

    if (channelMaskOverride_7_1Data > 0)
      m_dwChannelMaskOverride_7_1 = channelMaskOverride_7_1Data;
    else
      m_dwChannelMaskOverride_7_1 = 0;

    if (logSampleTimesData > 0)
      m_bLogSampleTimes = true;
    else
      m_bLogSampleTimes = false;

    if (HWBasedRefClockData > 0)
      m_bHWBasedRefClock = true;
    else
      m_bHWBasedRefClock = false;

    if (enableSyncAdjustmentData > 0)
      m_bEnableSyncAdjustment = true;
    else
      m_bEnableSyncAdjustment = false;

    m_hnsPeriod = devicePeriodData;

    bool rateOk = false;
    for(int i = 0; i < sizeof(gAllowedAC3bitrates) / sizeof(int); i++)
    {
      if (gAllowedAC3bitrates[i] == AC3bitrateData)
      {
        rateOk = true;
        break;
      }
    }

    if (rateOk)
      m_AC3bitrate = AC3bitrateData * 1000;
    else
    {
      m_AC3bitrate = 448000;
      Log("   invalid AC3 bitrate, using 448");
    }

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
      WriteRegistryKeyDword(hKey, WASAPIUseEventMode, WASAPIUseEventModeData);
      WriteRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
      WriteRegistryKeyDword(hKey, enableAC3Encoding, enableAC3EncodingData);
      WriteRegistryKeyDword(hKey, AC3bitrate, AC3bitrateData);
      WriteRegistryKeyDword(hKey, maxBias, maxBiasData);
      WriteRegistryKeyDword(hKey, minBias, minBiasData);
      WriteRegistryKeyDword(hKey, audioDelay, audioDelayData);
      WriteRegistryKeyDword(hKey, channelMaskOverride_5_1, channelMaskOverride_5_1Data);
      WriteRegistryKeyDword(hKey, channelMaskOverride_7_1, channelMaskOverride_7_1Data);
      WriteRegistryKeyDword(hKey, logSampleTimes, logSampleTimesData);
      WriteRegistryKeyDword(hKey, HWBasedRefClock, HWBasedRefClockData);
      WriteRegistryKeyDword(hKey, enableSyncAdjustment, enableSyncAdjustmentData);
      ReadRegistryKeyString(hKey, WASAPIPreferredDevice, WASAPIPreferredDeviceData);
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