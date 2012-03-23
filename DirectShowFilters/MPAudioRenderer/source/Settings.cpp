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
#include "..\libresample\src\samplerate.h"

#include "alloctracing.h"

extern void Log(const char *fmt, ...);
extern void LogRotate();

unsigned int gAllowedAC3bitrates[9]         = {192, 224, 256, 320, 384, 448, 512, 576, 640};
unsigned int gAllowedSampleRates[7]         = {22050, 32000, 44100, 48000, 88200, 96000, 192000};
unsigned int gAllowedBitDepths[4]           = {8, 16, 24, 32};
unsigned int gAllowedResamplingQualities[5] = {0, 1, 2, 3, 4};

AudioRendererSettings::AudioRendererSettings() :
  m_bLogSampleTimes(false),
  m_bLogDebug(false),
  m_bHWBasedRefClock(true),
  m_bEnableSyncAdjustment(true),
  m_bUseWASAPI(true),
  m_bWASAPIUseEventMode(true),
  m_bUseTimeStretching(false),
  m_lAC3Encoding(0),
  m_bQuality_USE_QUICKSEEK(false),
  m_bQuality_USE_AA_FILTER(false),
  m_lQuality_AA_FILTER_LENGTH(32),
  m_lQuality_SEQUENCE_MS(82),
  m_lQuality_SEEKWINDOW_MS(28),
  m_lQuality_OVERLAP_MS(28),
  m_hnsPeriod(0),
  m_AC3bitrate(448), 
  m_dMaxBias(1.1),
  m_dMinBias(0.9),
  m_lAudioDelay(0),
  m_WASAPIShareMode(AUDCLNT_SHAREMODE_EXCLUSIVE),
  m_wWASAPIPreferredDeviceId(NULL),
  m_nForceSamplingRate(0),
  m_nForceBitDepth(0),
  m_nResamplingQuality(4),
  m_lSpeakerCount(2),
  m_lSpeakerConfig(SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT),
  m_bForceChannelMixing(false),
  m_bReleaseDeviceOnStop(false)
{
  LogRotate();
  Log("MP Audio Renderer - v0.997");

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
  LPCTSTR AC3Encoding = TEXT("AC3Encoding");
  LPCTSTR AC3bitrate = TEXT("AC3bitrate");
  LPCTSTR maxBias = TEXT("MaxBias");
  LPCTSTR minBias = TEXT("MinBias");
  LPCTSTR audioDelay = TEXT("AudioDelay");
  LPCTSTR logSampleTimes = TEXT("LogSampleTimes");
  LPCTSTR logDebug = TEXT("LogDebug");
  LPCTSTR WASAPIPreferredDevice = TEXT("WASAPIPreferredDevice");
  LPCTSTR HWBasedRefClock = TEXT("HWBasedRefClock");
  LPCTSTR enableSyncAdjustment = TEXT("EnableSyncAdjustment");
  LPCTSTR forceSamplingRate = TEXT("ForceSamplingRate");
  LPCTSTR forceBitDepth = TEXT("ForceBitDepth");
  LPCTSTR resamplingQuality = TEXT("ResamplingQuality");
  LPCTSTR quality_USE_QUICKSEEK = TEXT("Quality_USE_QUICKSEEK");
  LPCTSTR quality_USE_AA_FILTER = TEXT("Quality_USE_AA_FILTER");
  LPCTSTR quality_AA_FILTER_LENGTH = TEXT("Quality_AA_FILTER_LENGTH");
  LPCTSTR quality_SEQUENCE_MS = TEXT("Quality_SEQUENCE_MS");
  LPCTSTR quality_SEEKWINDOW_MS = TEXT("Quality_SEEKWINDOW_MS");
  LPCTSTR quality_OVERLAP_MS = TEXT("Quality_OVERLAP_MS");
  LPCTSTR speakerConfig = TEXT("SpeakerConfig");
  LPCTSTR forceChannelMixing = TEXT("ForceChannelMixing");
  LPCTSTR releaseDeviceOnStop = TEXT("ReleaseDeviceOnStop");
  
  // Default values for the settings in registry
  DWORD forceDirectSoundData = 0;
  DWORD enableTimestretchingData = 1;
  DWORD WASAPIExclusiveData = 1;
  DWORD WASAPIUseEventModeData = 1;
  DWORD devicePeriodData = 500000;  // 50 ms
  DWORD AC3EncodingData = 0;        // 0 = disabled, 1 = auto, 2 = forced
  DWORD AC3bitrateData = 448;       // maximum based on the DVD spec
  DWORD maxBiasData = 11000;        // divide with 10000 to get real double value
  DWORD minBiasData = 9000;         // divide with 10000 to get real double value
  DWORD audioDelayData = 0;         // in ms
  DWORD logSampleTimesData = 0;
  DWORD logDebugData = 0;
  DWORD HWBasedRefClockData = 1;
  DWORD enableSyncAdjustmentData = 1;
  DWORD forceSamplingRateData = 0;
  DWORD forceBitDepthData = 0;
  DWORD resamplingQualityData = 4;
  DWORD speakerConfigData = SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT;
  DWORD forceChannelMixingData = 0;
  DWORD releaseDeviceOnStopData = 0;
  DWORD quality_USE_QUICKSEEKData = 0;
  DWORD quality_USE_AA_FILTERData = 0;
  DWORD quality_AA_FILTER_LENGTHData = 32;  // in ms (same as soundtouch default)
  DWORD quality_SEQUENCE_MSData = 82;       // in ms (same as soundtouch default)
  DWORD quality_SEEKWINDOW_MSData = 28;     // in ms (same as soundtouch default)
  DWORD quality_OVERLAP_MSData = 28;        // in ms (same as soundtouch default)
  
  // settings from Reclock - watch CPU usage when enabling these!
  /*bool usequickseek = false;
  bool useaafilter = false; //seems clearer without it
  int aafiltertaps = 56; //Def=32 doesnt matter coz its not used
  int seqms = 120; //reclock original is 82
  int seekwinms = 28; //reclock original is 28
  int overlapms = seekwinms; //reduces cutting sound if this is large
  int seqmslfe = 180; //larger value seems to preserve low frequencies better
  int seekwinmslfe = 42; //as percentage of seqms
  int overlapmslfe = seekwinmslfe; //reduces cutting sound if this is large
  */

  LPCTSTR WASAPIPreferredDeviceData = new TCHAR[MAX_REG_LENGTH];

  ZeroMemory((void*)WASAPIPreferredDeviceData, MAX_REG_LENGTH);

  // Try to access the setting root "Software\Team MediaPortal\Audio Renderer"
  RegOpenKeyEx(HKEY_CURRENT_USER, folder, NULL, KEY_ALL_ACCESS, &hKey);

  if (hKey)
  {
    ReadRegistryKeyDword(hKey, forceDirectSound, forceDirectSoundData);
    ReadRegistryKeyDword(hKey, enableTimestretching, enableTimestretchingData);
    ReadRegistryKeyDword(hKey, WASAPIExclusive, WASAPIExclusiveData);
    ReadRegistryKeyDword(hKey, WASAPIUseEventMode, WASAPIUseEventModeData);
    ReadRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
    ReadRegistryKeyDword(hKey, AC3Encoding, AC3EncodingData);
    ReadRegistryKeyDword(hKey, AC3bitrate, AC3bitrateData);
    ReadRegistryKeyDword(hKey, maxBias, maxBiasData);
    ReadRegistryKeyDword(hKey, minBias, minBiasData);
    ReadRegistryKeyDword(hKey, audioDelay, audioDelayData);
    ReadRegistryKeyDword(hKey, logSampleTimes, logSampleTimesData);
    ReadRegistryKeyDword(hKey, logDebug, logDebugData);
    ReadRegistryKeyDword(hKey, HWBasedRefClock, HWBasedRefClockData);
    ReadRegistryKeyDword(hKey, enableSyncAdjustment, enableSyncAdjustmentData);
    ReadRegistryKeyDword(hKey, forceSamplingRate, forceSamplingRateData);
    ReadRegistryKeyDword(hKey, forceBitDepth, forceBitDepthData);
    ReadRegistryKeyDword(hKey, resamplingQuality, resamplingQualityData);
    ReadRegistryKeyDword(hKey, speakerConfig, speakerConfigData);
    ReadRegistryKeyDword(hKey, forceChannelMixing, forceChannelMixingData);
    ReadRegistryKeyDword(hKey, releaseDeviceOnStop, releaseDeviceOnStopData);

    // SoundTouch quality settings
    ReadRegistryKeyDword(hKey, quality_USE_QUICKSEEK, quality_USE_QUICKSEEKData);
    ReadRegistryKeyDword(hKey, quality_USE_AA_FILTER, quality_USE_AA_FILTERData);
    ReadRegistryKeyDword(hKey, quality_AA_FILTER_LENGTH, quality_AA_FILTER_LENGTHData);
    ReadRegistryKeyDword(hKey, quality_SEQUENCE_MS, quality_SEQUENCE_MSData);
    ReadRegistryKeyDword(hKey, quality_SEEKWINDOW_MS, quality_SEEKWINDOW_MSData);
    ReadRegistryKeyDword(hKey, quality_OVERLAP_MS, quality_OVERLAP_MSData);

    ReadRegistryKeyString(hKey, WASAPIPreferredDevice, WASAPIPreferredDeviceData);

    Log("   ForceDirectSound:         %d", forceDirectSoundData);
    Log("   EnableTimestrecthing:     %d", enableTimestretchingData);
    Log("   WASAPIExclusive:          %d", WASAPIExclusiveData);
    Log("   WASAPIUseEventMode:       %d", WASAPIUseEventModeData);
    Log("   AC3Encoding:              %d (0 = disabled, 1 = auto, 2 = forced)", AC3EncodingData);
    Log("   AC3bitrate:               %d", AC3bitrateData);
    Log("   MaxBias:                  %d", maxBiasData);
    Log("   MinBias:                  %d", minBiasData);
    Log("   AudioDelay:               %d", audioDelayData);
    Log("   LogSampleTimes:           %d", logSampleTimesData);
    Log("   LogDebug:                 %d", logDebugData);
    Log("   HWBasedRefClock:          %d", HWBasedRefClockData);
    Log("   EnableSyncAdjustment:     %d", enableSyncAdjustmentData);
    Log("   ForceSamplingRate:        %d", forceSamplingRateData);
    Log("   ForceBitDepth:            %d", forceBitDepthData);
    Log("   ResamplingQuality:        %s", ResamplingQualityAsString(resamplingQualityData));
    Log("   SpeakerConfig:            %d", speakerConfigData);
    Log("   ForceChannelMixing:       %d", forceChannelMixingData);
    Log("   DoCleanupOnStop:          %d", releaseDeviceOnStopData);
    Log("   quality_USE_QUICKSEEK:    %d", quality_USE_QUICKSEEKData);
    Log("   quality_USE_AA_FILTER:    %d", quality_USE_AA_FILTERData);
    Log("   quality_AA_FILTER_LENGTH: %d", quality_AA_FILTER_LENGTHData);
    Log("   quality_SEQUENCE_MS:      %d", quality_SEQUENCE_MSData);
    Log("   quality_SEEKWINDOW_MS:    %d", quality_SEEKWINDOW_MSData);
    Log("   quality_OVERLAP_MS:       %d", quality_OVERLAP_MSData);
    Log("   DevicePeriod:             %d (1 = minimal, 0 = driver default, other user defined)", devicePeriodData);
    Log("   WASAPIPreferredDevice:    %s", WASAPIPreferredDeviceData);

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
      m_bWASAPIUseEventMode = true;
    else
      m_bWASAPIUseEventMode = false;

    if (AC3EncodingData == DISABLED || AC3EncodingData == AUTO || AC3EncodingData == FORCED)
      m_lAC3Encoding = AC3EncodingData;
    else
      m_lAC3Encoding = 0;

    m_dMaxBias = (double)maxBiasData / 10000.0;
    m_dMinBias = (double)minBiasData / 10000.0;

    m_lAudioDelay = audioDelayData;

    if (logSampleTimesData > 0)
      m_bLogSampleTimes = true;
    else
      m_bLogSampleTimes = false;
    
    if (logDebugData > 0)
      m_bLogDebug = true;
    else
      m_bLogDebug = false;

    if (HWBasedRefClockData > 0)
      m_bHWBasedRefClock = true;
    else
      m_bHWBasedRefClock = false;

    if (enableSyncAdjustmentData > 0)
      m_bEnableSyncAdjustment = true;
    else
      m_bEnableSyncAdjustment = false;

    bool AC3EncodingForced = AC3EncodingData == FORCED;
    bool sampleRateAllowed = AllowedValue(gAllowedSampleRates, sizeof(gAllowedSampleRates) / sizeof(int), forceSamplingRateData);
    bool bitDepthAllowed = AllowedValue(gAllowedBitDepths, sizeof(gAllowedBitDepths) / sizeof(int), forceBitDepthData);

    if (AC3EncodingForced)
    {
      if (sampleRateAllowed && (forceSamplingRateData != 48000 && forceSamplingRateData != 44100))
      {
        Log("   Warning: AC3 encoding forced and sampling rate set to non-matching!");
        sampleRateAllowed = false;
      }
    }
    else if (AC3EncodingData == AUTO && (forceSamplingRateData != 48000 && forceSamplingRateData != 44100))
      Log("   Warning: Using other than 48000 hz or 44100 hz sampling rates will disable AC3 encoding!");

    if (sampleRateAllowed || forceSamplingRateData == 0)
      m_nForceSamplingRate = forceSamplingRateData;
    else
    {
      m_nForceSamplingRate = 0;
      if (forceSamplingRateData != 0)
        Log("   invalid forced sample rate!");
    }

    if (bitDepthAllowed && !AC3EncodingForced || forceBitDepthData == 0)
      m_nForceBitDepth = forceBitDepthData;
    else
    {
      m_nForceBitDepth = 0;
      if (forceBitDepthData != 0)
        Log("   invalid forced bit depth!");
    }

    if (AllowedValue(gAllowedResamplingQualities, sizeof(gAllowedResamplingQualities) / sizeof(int), resamplingQualityData))
      m_nResamplingQuality = resamplingQualityData;
    else
    {
      m_nResamplingQuality = 4;
      Log("   invalid resampling quality setting, using 4 (SRC_LINEAR)");
    }

    if (AllowedValue(gAllowedAC3bitrates, sizeof(gAllowedAC3bitrates) / sizeof(int), AC3bitrateData))
      m_AC3bitrate = AC3bitrateData * 1000;
    else
    {
      m_AC3bitrate = 448000;
      Log("   invalid AC3 bitrate, using 448");
    }

    m_hnsPeriod = devicePeriodData;

    if (forceChannelMixingData > 0)
      m_bForceChannelMixing = true;
    else
      m_bForceChannelMixing = false;

    // TODO validate channel mask
    if (speakerConfigData > 0)
    {
      m_lSpeakerConfig = speakerConfigData;
      m_lSpeakerCount = ChannelCount(m_lSpeakerConfig);

      if (AC3EncodingForced && m_lSpeakerCount > 6 && m_bForceChannelMixing)
      {
        m_lSpeakerConfig = KSAUDIO_SPEAKER_5POINT1_SURROUND;
        m_lSpeakerCount = 6;
        Log("   Warning: incompatible settings. ForceChannelMixing + AC3 encoding forced + more than 6 channels");
      }
    }
    else
      m_lSpeakerConfig = SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT;

    if (releaseDeviceOnStopData > 0)
      m_bReleaseDeviceOnStop = true;
    else
      m_bReleaseDeviceOnStop = false;

    if (quality_USE_QUICKSEEKData > 0)
      m_bQuality_USE_QUICKSEEK = true;
    else
      m_bQuality_USE_QUICKSEEK = false;

    if (quality_USE_AA_FILTERData > 0)
      m_bQuality_USE_AA_FILTER = true;
    else
      m_bQuality_USE_AA_FILTER = false;

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
      WriteRegistryKeyDword(hKey, AC3Encoding, AC3EncodingData);
      WriteRegistryKeyDword(hKey, AC3bitrate, AC3bitrateData);
      WriteRegistryKeyDword(hKey, maxBias, maxBiasData);
      WriteRegistryKeyDword(hKey, minBias, minBiasData);
      WriteRegistryKeyDword(hKey, audioDelay, audioDelayData);
      WriteRegistryKeyDword(hKey, logSampleTimes, logSampleTimesData);
      WriteRegistryKeyDword(hKey, logDebug, logDebugData);
      WriteRegistryKeyDword(hKey, HWBasedRefClock, HWBasedRefClockData);
      WriteRegistryKeyDword(hKey, enableSyncAdjustment, enableSyncAdjustmentData);
      WriteRegistryKeyDword(hKey, forceSamplingRate, forceSamplingRateData);
      WriteRegistryKeyDword(hKey, forceBitDepth, forceBitDepthData);
      WriteRegistryKeyDword(hKey, resamplingQuality, resamplingQualityData);
      WriteRegistryKeyDword(hKey, speakerConfig, speakerConfigData);
      WriteRegistryKeyDword(hKey, forceChannelMixing, forceChannelMixingData);
      WriteRegistryKeyDword(hKey, releaseDeviceOnStop, releaseDeviceOnStopData);
      WriteRegistryKeyDword(hKey, quality_USE_QUICKSEEK, quality_USE_QUICKSEEKData);
      WriteRegistryKeyDword(hKey, quality_USE_AA_FILTER, quality_USE_AA_FILTERData);
      WriteRegistryKeyDword(hKey, quality_AA_FILTER_LENGTH, quality_AA_FILTER_LENGTHData);
      WriteRegistryKeyDword(hKey, quality_SEQUENCE_MS, quality_SEQUENCE_MSData);
      WriteRegistryKeyDword(hKey, quality_SEEKWINDOW_MS, quality_SEEKWINDOW_MSData);
      WriteRegistryKeyDword(hKey, quality_OVERLAP_MS, quality_OVERLAP_MSData);

      ReadRegistryKeyString(hKey, WASAPIPreferredDevice, WASAPIPreferredDeviceData);
    } 
    else 
      Log("Error creating master key %d", result);
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
      Log("   faíled to create default value for %s", lpSubKey);
  }
}

void AudioRendererSettings::WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{  
  DWORD dwSize = sizeof(DWORD);
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_DWORD, (LPBYTE)&data, dwSize);
  if (result == ERROR_SUCCESS) 
    Log("Success writing to Registry: %s", lpSubKey);
  else 
    Log("Error writing to Registry - subkey: %s error: %d", lpSubKey, result);
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
      Log("   too much data, corrupted registry setting(?):  %s", lpSubKey);      
    else
      Log("   error: %d subkey: %s", error, lpSubKey);       
  }
}

void AudioRendererSettings::WriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{  
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_SZ, (LPBYTE)data, strlen(data)+1);
  if (result == ERROR_SUCCESS) 
    Log("Success writing to Registry: %s", lpSubKey);
  else 
    Log("Error writing to Registry - subkey: %s error: %d", lpSubKey, result);
}

bool AudioRendererSettings::AllowedValue(unsigned int allowedRates[], unsigned int size, int rate)
{
  bool rateOk = false;
  for (int i = 0; i < size; i++)
  {
    if (allowedRates[i] == rate)
    {
      rateOk = true;
      break;
    }
  }

  return rateOk;
}

LPCTSTR AudioRendererSettings::ResamplingQualityAsString(int setting)
{
	switch (setting)
	{
	  case SRC_SINC_BEST_QUALITY:
		  return _T("SRC_SINC_BEST_QUALITY");
	  case SRC_SINC_MEDIUM_QUALITY:
		  return _T("SRC_SINC_MEDIUM_QUALITY");
	  case SRC_SINC_FASTEST:
		  return _T("SRC_SINC_FASTEST");
	  case SRC_ZERO_ORDER_HOLD:
		  return _T("SRC_ZERO_ORDER_HOLD");
	  case SRC_LINEAR:
		  return _T("SRC_LINEAR");
    default:
      return _T("UNKNOWN");
  }
}

unsigned int AudioRendererSettings::ChannelCount(unsigned int channelMask) 
{ 
  unsigned int channelCount = 0;
  for (channelCount = 0; channelMask; channelCount++) 
  { 
    channelMask &= channelMask - 1;
  } 

  return channelCount;
} 
