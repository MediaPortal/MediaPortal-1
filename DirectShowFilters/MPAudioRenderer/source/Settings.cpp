// Copyright (C) 2005-2012 Team MediaPortal
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
#include "resource.h"
#include "..\libresample\src\samplerate.h"

#include <FunctionDiscoveryKeys_devpkey.h>

#include "alloctracing.h"

extern void Log(const char *fmt, ...);

unsigned int gAllowedAC3bitrates[9]         = {192, 224, 256, 320, 384, 448, 512, 576, 640};
unsigned int gAllowedSampleRates[7]         = {22050, 32000, 44100, 48000, 88200, 96000, 192000};
unsigned int gAllowedBitDepths[4]           = {8, 16, 24, 32};
unsigned int gAllowedResamplingQualities[5] = {0, 1, 2, 3, 4};
unsigned int speakerConfigs[7]              = {4, 3, 51, 263, 63, 1551, 1599};

LPCTSTR folder = TEXT("Software\\Team MediaPortal\\Audio Renderer");

#define DEFAULT_AC3_BITRATE 448000
#define DEFAULT_OUTPUT_BUFFER 500
#define MAX_OUTPUT_BUFFER 5000
#define MIN_OUTPUT_BUFFER 100
#define MAX_AUDIO_DELAY 2000
#define MIN_AUDIO_DELAY 0

// Registry setting names
LPCTSTR enableTimestretching = TEXT("EnableTimestretching");
LPCTSTR WASAPIExclusive = TEXT("WASAPIExclusive");
LPCTSTR WASAPIUseEventMode = TEXT("WASAPIUseEventMode");
LPCTSTR devicePeriod = TEXT("DevicePeriod");
LPCTSTR outputBufferSize = TEXT("OutputBufferSize");
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
LPCTSTR expandMonoToStereo = TEXT("ExpandMonoToStereo");
LPCTSTR allowBitStreaming = TEXT("AllowBitStreaming");

// Default values for the settings in registry
DWORD enableTimestretchingData = 1;
DWORD WASAPIExclusiveData = 1;
DWORD WASAPIUseEventModeData = 1;
DWORD devicePeriodData = 500000;      // 50 ms
DWORD outputBufferSizeData = 500;     // 500 ms
DWORD AC3EncodingData = 0;            // 0 = disabled, 1 = auto, 2 = forced
DWORD AC3bitrateData = 448;           // maximum based on the DVD spec
DWORD maxBiasData = 11000;            // divide with 10000 to get real double value
DWORD minBiasData = 9000;             // divide with 10000 to get real double value
DWORD audioDelayData = 0;             // in ms
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
DWORD expandMonoToStereoData = 1;
DWORD quality_USE_QUICKSEEKData = 0;
DWORD quality_USE_AA_FILTERData = 0;
DWORD quality_AA_FILTER_LENGTHData = 32;  // in ms (same as soundtouch default)
DWORD quality_SEQUENCE_MSData = 82;       // in ms (same as soundtouch default)
DWORD quality_SEEKWINDOW_MSData = 28;     // in ms (same as soundtouch default)
DWORD quality_OVERLAP_MSData = 28;        // in ms (same as soundtouch default)
DWORD allowBitStreamingData = 1;

AudioRendererSettings::AudioRendererSettings() :
  CUnknown(_T("MPAR_Settings"), NULL),
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
  m_hnsPeriod(devicePeriodData),
  m_msOutputBuffer(outputBufferSizeData),
  m_AC3bitrate(DEFAULT_AC3_BITRATE),
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
  m_bReleaseDeviceOnStop(false),
  m_bExpandMonoToStereo(true),
  m_nUseFilters(USE_FILTERS_ALL),
  m_bAllowBitStreaming(true)
{
  LoadSettingsFromRegistry();
}

AudioRendererSettings::~AudioRendererSettings()
{
  delete[] m_wWASAPIPreferredDeviceId;
}

void AudioRendererSettings::LoadSettingsFromRegistry()
{
  Log("Loading settings from registry");

  HKEY hKey;
  char* lpData = new char[MAX_REG_LENGTH];

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
    ReadRegistryKeyDword(hKey, enableTimestretching, enableTimestretchingData);
    ReadRegistryKeyDword(hKey, WASAPIExclusive, WASAPIExclusiveData);
    ReadRegistryKeyDword(hKey, WASAPIUseEventMode, WASAPIUseEventModeData);
    ReadRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
    ReadRegistryKeyDword(hKey, outputBufferSize, outputBufferSizeData);
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
    ReadRegistryKeyDword(hKey, expandMonoToStereo, expandMonoToStereoData);
    ReadRegistryKeyDword(hKey, allowBitStreaming, allowBitStreamingData);

    // SoundTouch quality settings
    ReadRegistryKeyDword(hKey, quality_USE_QUICKSEEK, quality_USE_QUICKSEEKData);
    ReadRegistryKeyDword(hKey, quality_USE_AA_FILTER, quality_USE_AA_FILTERData);
    ReadRegistryKeyDword(hKey, quality_AA_FILTER_LENGTH, quality_AA_FILTER_LENGTHData);
    ReadRegistryKeyDword(hKey, quality_SEQUENCE_MS, quality_SEQUENCE_MSData);
    ReadRegistryKeyDword(hKey, quality_SEEKWINDOW_MS, quality_SEEKWINDOW_MSData);
    ReadRegistryKeyDword(hKey, quality_OVERLAP_MS, quality_OVERLAP_MSData);

    ReadRegistryKeyString(hKey, WASAPIPreferredDevice, WASAPIPreferredDeviceData);

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
    Log("   ReleaseDeviceOnStop:      %d", releaseDeviceOnStopData);
    Log("   ExpandMonoToStereo:       %d", expandMonoToStereoData);
    Log("   AllowBitStreaming:        %d", allowBitStreamingData);
    Log("   quality_USE_QUICKSEEK:    %d", quality_USE_QUICKSEEKData);
    Log("   quality_USE_AA_FILTER:    %d", quality_USE_AA_FILTERData);
    Log("   quality_AA_FILTER_LENGTH: %d", quality_AA_FILTER_LENGTHData);
    Log("   quality_SEQUENCE_MS:      %d", quality_SEQUENCE_MSData);
    Log("   quality_SEEKWINDOW_MS:    %d", quality_SEEKWINDOW_MSData);
    Log("   quality_OVERLAP_MS:       %d", quality_OVERLAP_MSData);
    Log("   Output buffer (ms):       %d", outputBufferSizeData);
    Log("   DevicePeriod:             %d (1 = minimal, 0 = driver default, other user defined)", devicePeriodData);
    Log("   WASAPIPreferredDevice:    %s", WASAPIPreferredDeviceData);

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

    if (expandMonoToStereoData > 0)
      m_bExpandMonoToStereo = true;
    else
      m_bExpandMonoToStereo = false;

    if (allowBitStreamingData > 0)
      m_bAllowBitStreaming = true;
    else
      m_bAllowBitStreaming = false;

    if (IsValidAC3EncodingMode(AC3EncodingData))
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
    bool sampleRateAllowed = IsValidSampleRate(forceSamplingRateData);
    bool bitDepthAllowed = IsValidBitDepth(forceBitDepthData);

    if (AC3EncodingForced)
    {
      if (sampleRateAllowed && (forceSamplingRateData != 48000 && forceSamplingRateData != 44100))
      {
        Log("   Warning: AC3 encoding forced and sampling rate set to non-matching!");
        sampleRateAllowed = false;
      }
    }
    else if (AC3EncodingData == AUTO && (forceSamplingRateData != 48000 && forceSamplingRateData != 44100 && forceSamplingRateData != 0))
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

    if (IsValidResamplingQuality(resamplingQualityData))
      m_nResamplingQuality = resamplingQualityData;
    else
    {
      m_nResamplingQuality = SRC_LINEAR;
      Log("   invalid resampling quality setting, using 4 (SRC_LINEAR)");
    }

    if (IsValidAC3Bitrate(AC3bitrateData))
      m_AC3bitrate = AC3bitrateData * 1000;
    else
    {
      m_AC3bitrate = DEFAULT_AC3_BITRATE;
      Log("   invalid AC3 bitrate, using 448");
    }

    m_hnsPeriod = devicePeriodData;
    
    if (devicePeriodData == 0 || devicePeriodData == 1)
    {
      m_msOutputBuffer = DEFAULT_OUTPUT_BUFFER;
      Log("   devicePeriodData: %d - using default (%d ms) output buffer", devicePeriodData, outputBufferSizeData);
    }
    else
    {
      if ((outputBufferSizeData * 10000) < (devicePeriodData * 2))
      {
        m_msOutputBuffer = max((devicePeriodData / 10000) * 2, MIN_OUTPUT_BUFFER);
        Log("   too small output buffer - devicePeriodData: %d - using (%d ms) output buffer", devicePeriodData, outputBufferSizeData);
      }
      else if (outputBufferSizeData > MAX_OUTPUT_BUFFER)
      {
        m_msOutputBuffer = MAX_OUTPUT_BUFFER;
        Log("   outputBufferSize: %d - using (%d ms) output buffer", outputBufferSizeData, outputBufferSizeData);
      }
      else if (outputBufferSizeData < MIN_OUTPUT_BUFFER)
      {
        m_msOutputBuffer = MIN_OUTPUT_BUFFER;
        Log("   outputBufferSize: %d - using (%d ms) output buffer", outputBufferSizeData, outputBufferSizeData);
      }
      else
        m_msOutputBuffer = outputBufferSizeData;
    }

    if (forceChannelMixingData > 0)
      m_bForceChannelMixing = true;
    else
      m_bForceChannelMixing = false;

    if (IsValidSpeakerConfig(speakerConfigData))
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
    
    _tcsncpy(m_wWASAPIPreferredDeviceId, WASAPIPreferredDeviceData, MAX_REG_LENGTH);

    delete[] WASAPIPreferredDeviceData;
  }
  else // no settings in registry, create default values
  {
    USES_CONVERSION;

    Log("Failed to open %s", T2A(folder));
    Log("Initializing registry with default settings");

    LONG result = RegCreateKeyEx(HKEY_CURRENT_USER, folder, 0, NULL, REG_OPTION_NON_VOLATILE,
                                  KEY_ALL_ACCESS, NULL, &hKey, NULL);

    if (result == ERROR_SUCCESS) 
    {
      Log("Success creating master key");
      WriteRegistryKeyDword(hKey, enableTimestretching, enableTimestretchingData);
      WriteRegistryKeyDword(hKey, WASAPIExclusive, WASAPIExclusiveData);
      WriteRegistryKeyDword(hKey, WASAPIUseEventMode, WASAPIUseEventModeData);
      WriteRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
      WriteRegistryKeyDword(hKey, outputBufferSize, outputBufferSizeData);
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
      WriteRegistryKeyDword(hKey, expandMonoToStereo, expandMonoToStereoData);
      WriteRegistryKeyDword(hKey, allowBitStreaming, allowBitStreamingData);
      WriteRegistryKeyDword(hKey, quality_USE_QUICKSEEK, quality_USE_QUICKSEEKData);
      WriteRegistryKeyDword(hKey, quality_USE_AA_FILTER, quality_USE_AA_FILTERData);
      WriteRegistryKeyDword(hKey, quality_AA_FILTER_LENGTH, quality_AA_FILTER_LENGTHData);
      WriteRegistryKeyDword(hKey, quality_SEQUENCE_MS, quality_SEQUENCE_MSData);
      WriteRegistryKeyDword(hKey, quality_SEEKWINDOW_MS, quality_SEEKWINDOW_MSData);
      WriteRegistryKeyDword(hKey, quality_OVERLAP_MS, quality_OVERLAP_MSData);

      delete[] m_wWASAPIPreferredDeviceId;
      m_wWASAPIPreferredDeviceId = new WCHAR[MAX_REG_LENGTH];
      _tcsncpy(m_wWASAPIPreferredDeviceId, WASAPIPreferredDeviceData, MAX_REG_LENGTH);

      WriteRegistryKeyString(hKey, WASAPIPreferredDevice, WASAPIPreferredDeviceData);
    } 
    else 
      Log("Error creating master key %d", result);
  }
  
  delete[] lpData;
  RegCloseKey (hKey);
}

void AudioRendererSettings::SaveSettingsToRegistry(HKEY hKey)
{
  USES_CONVERSION;

  if (!hKey)
  {
    // Try to access the setting root "Software\Team MediaPortal\Audio Renderer"
    RegOpenKeyEx(HKEY_CURRENT_USER, folder, NULL, KEY_ALL_ACCESS, &hKey);
  }

  enableTimestretchingData = m_bUseTimeStretching ? 1 : 0;
  WASAPIExclusiveData = m_WASAPIShareMode == AUDCLNT_SHAREMODE_EXCLUSIVE ? 1 : 0;
  WASAPIUseEventModeData = m_bWASAPIUseEventMode ? 1 : 0;
  //devicePeriodData = m_hnsPeriod;
  AC3EncodingData = m_lAC3Encoding;
  AC3bitrateData = m_AC3bitrate / 1000;
  maxBiasData = m_dMaxBias * 10000;
  minBiasData = m_dMinBias * 10000;
  audioDelayData = m_lAudioDelay;
  outputBufferSizeData = m_msOutputBuffer;
  logSampleTimesData = m_bLogSampleTimes ? 1 : 0;
  logDebugData = m_bLogDebug ? 1: 0;
  HWBasedRefClockData = m_bHWBasedRefClock ? 1 : 0;
  enableSyncAdjustmentData = m_bEnableSyncAdjustment ? 1 : 0;
  forceSamplingRateData = m_nForceSamplingRate;
  forceBitDepthData = m_nForceBitDepth;
  resamplingQualityData = m_nResamplingQuality;
  speakerConfigData = m_lSpeakerConfig;
  forceChannelMixingData = m_bForceChannelMixing ? 1 : 0;
  releaseDeviceOnStopData = m_bReleaseDeviceOnStop ? 1 : 0;
  expandMonoToStereoData = m_bExpandMonoToStereo ? 1 : 0;
  allowBitStreamingData = m_bAllowBitStreaming ? 1 : 0;
  quality_USE_QUICKSEEKData = m_bQuality_USE_QUICKSEEK;
  quality_USE_AA_FILTERData = m_bQuality_USE_AA_FILTER;
  quality_AA_FILTER_LENGTHData = m_lQuality_AA_FILTER_LENGTH;
  quality_SEQUENCE_MSData = m_lQuality_SEQUENCE_MS;
  quality_SEEKWINDOW_MSData = m_lQuality_SEEKWINDOW_MS;
  quality_OVERLAP_MSData = m_lQuality_OVERLAP_MS;
  
  LPCTSTR WASAPIPreferredDeviceData = W2T(m_wWASAPIPreferredDeviceId);

  WriteRegistryKeyDword(hKey, enableTimestretching, enableTimestretchingData);
  WriteRegistryKeyDword(hKey, WASAPIExclusive, WASAPIExclusiveData);
  WriteRegistryKeyDword(hKey, WASAPIUseEventMode, WASAPIUseEventModeData);
  //WriteRegistryKeyDword(hKey, devicePeriod, devicePeriodData);
  WriteRegistryKeyDword(hKey, outputBufferSize, outputBufferSizeData);
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
  WriteRegistryKeyDword(hKey, expandMonoToStereo, expandMonoToStereoData);
  WriteRegistryKeyDword(hKey, allowBitStreaming, allowBitStreamingData);
  WriteRegistryKeyDword(hKey, quality_USE_QUICKSEEK, quality_USE_QUICKSEEKData);
  WriteRegistryKeyDword(hKey, quality_USE_AA_FILTER, quality_USE_AA_FILTERData);
  WriteRegistryKeyDword(hKey, quality_AA_FILTER_LENGTH, quality_AA_FILTER_LENGTHData);
  WriteRegistryKeyDword(hKey, quality_SEQUENCE_MS, quality_SEQUENCE_MSData);
  WriteRegistryKeyDword(hKey, quality_SEEKWINDOW_MS, quality_SEEKWINDOW_MSData);
  WriteRegistryKeyDword(hKey, quality_OVERLAP_MS, quality_OVERLAP_MSData);

  WriteRegistryKeyString(hKey, WASAPIPreferredDevice, WASAPIPreferredDeviceData);
}

void AudioRendererSettings::ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{
  USES_CONVERSION;

  DWORD dwSize = sizeof(DWORD);
  DWORD dwType = REG_DWORD;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)&data, &dwSize);
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      Log("   create default value for %s", T2A(lpSubKey));
      WriteRegistryKeyDword(hKey, lpSubKey, data);
    }
    else
      Log("   faíled to create default value for %s", T2A(lpSubKey));
  }
}

void AudioRendererSettings::WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{
  USES_CONVERSION;

  DWORD dwSize = sizeof(DWORD);
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_DWORD, (LPBYTE)&data, dwSize);
  if (result == ERROR_SUCCESS) 
    Log("Success writing to Registry: %s", T2A(lpSubKey));
  else 
    Log("Error writing to Registry - subkey: %s error: %d", T2A(lpSubKey), result);
}

void AudioRendererSettings::ReadRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{
  USES_CONVERSION;

  DWORD dwSize = MAX_REG_LENGTH;
  DWORD dwType = REG_SZ;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)data, &dwSize);
  
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      Log("   create default value for %s", T2A(lpSubKey));
      WriteRegistryKeyString(hKey, lpSubKey, data);
    }
    else if (error == ERROR_MORE_DATA)
      Log("   too much data, corrupted registry setting(?):  %s", T2A(lpSubKey));
    else
      Log("   error: %d subkey: %s", error, T2A(lpSubKey));
  }
}

void AudioRendererSettings::WriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{  
  USES_CONVERSION;

  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_SZ, (LPBYTE)data, _tcslen(data) * sizeof(TCHAR));
  if (result == ERROR_SUCCESS) 
    Log("Success writing to Registry: %s", T2A(lpSubKey));
  else 
    Log("Error writing to Registry - subkey: %s error: %d", T2A(lpSubKey), result);
}

bool AudioRendererSettings::AllowedValue(unsigned int allowedValues[], unsigned int size, unsigned int value)
{
  bool valueOk = false;
  for (unsigned int i = 0; i < size; i++)
  {
    if (allowedValues[i] == value)
    {
      valueOk = true;
      break;
    }
  }

  return valueOk;
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

LPCTSTR AudioRendererSettings::SpeakerConfigAsString(int setting)
{
  switch (setting)
  {
    case 4:
      return _T("Mono");
    case 3:
      return _T("Stereo");
    case 51:
      return _T("Quad");
    case 263:
      return _T("Surround");
    case 63:
      return _T("5.1");
    case 1551:
      return _T("5.1 Surround");
    case 1599:
      return _T("7.1 Surround");
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

HRESULT AudioRendererSettings::GetAudioDevice(IMMDevice** ppMMDevice)
{
  Log("CWASAPIRenderFilter::GetAudioDevice");

  CComPtr<IMMDeviceEnumerator> enumerator;
  IMMDeviceCollection* devices = NULL;
  HRESULT hr = enumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));

  if (FAILED(hr))
  {
    Log("  failed to create MMDeviceEnumerator!");
    return hr;
  }

  Log("Target end point: %S", m_wWASAPIPreferredDeviceId);

  if (GetAvailableAudioDevices(&devices, NULL, false) == S_OK)
  {
    UINT count = 0;
    hr = devices->GetCount(&count);
    if (FAILED(hr))
    {
      Log("  devices->GetCount failed: (0x%08x)", hr);
      return hr;
    }
    
    for (UINT i = 0; i < count; i++)
    {
      LPWSTR pwszID = NULL;
      IMMDevice* endpoint = NULL;
      hr = devices->Item(i, &endpoint);
      if (SUCCEEDED(hr))
      {
        hr = endpoint->GetId(&pwszID);
        if (SUCCEEDED(hr))
        {
          // Found the configured audio endpoint
          if (wcscmp(pwszID, m_wWASAPIPreferredDeviceId) == 0)
          {
            enumerator->GetDevice(m_wWASAPIPreferredDeviceId, ppMMDevice);
            SAFE_RELEASE(devices);
            *ppMMDevice = endpoint;
            CoTaskMemFree(pwszID);
            pwszID = NULL;
            return S_OK;
          }
          else
          {
            SAFE_RELEASE(endpoint);
            CoTaskMemFree(pwszID);
            pwszID = NULL;
          }
        }
        else
          Log("  devices->GetId failed: (0x%08x)", hr);
      }
      else
        Log("  devices->Item failed: (0x%08x)", hr);

      CoTaskMemFree(pwszID);
      pwszID = NULL;
    }
  }

  Log("Unable to find selected audio device, using the default end point!");
  hr = enumerator->GetDefaultAudioEndpoint(eRender, eConsole, ppMMDevice);

  IPropertyStore* pProps = NULL;

  if (SUCCEEDED((*ppMMDevice)->OpenPropertyStore(STGM_READ, &pProps)))
  {
    LPWSTR pwszID = NULL;
    
    PROPVARIANT varName;
    PropVariantInit(&varName);

    PROPVARIANT eventDriven;
    PropVariantInit(&eventDriven);

    PROPVARIANT speakerMask;
    PropVariantInit(&speakerMask);

    if (SUCCEEDED(pProps->GetValue(PKEY_Device_FriendlyName, &varName)) &&
        SUCCEEDED(pProps->GetValue(PKEY_AudioEndpoint_Supports_EventDriven_Mode, &eventDriven)) &&
        SUCCEEDED((*ppMMDevice)->GetId(&pwszID)))
    {
      pProps->GetValue(PKEY_AudioEndpoint_PhysicalSpeakers, &speakerMask);
      Log("Default audio endpoint: \"%S\" (%S) - pull mode: %d sprk mask: %d" ,varName.pwszVal, pwszID, eventDriven.intVal, speakerMask.uintVal);
    }

    CoTaskMemFree(pwszID);
    pwszID = NULL;
    PropVariantClear(&varName);
    PropVariantClear(&eventDriven);
    PropVariantClear(&speakerMask);
    SAFE_RELEASE(pProps)
  }

  SAFE_RELEASE(devices);

  return hr;
}

HRESULT AudioRendererSettings::GetAvailableAudioDevices(IMMDeviceCollection** ppMMDevices, HWND hDialog, bool pLog)
{
  USES_CONVERSION;

  HRESULT hr = S_OK;

  CComPtr<IMMDeviceEnumerator> enumerator;
  Log("CWASAPIRenderFilter::GetAvailableAudioDevices");
  hr = enumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));

  if (FAILED(hr))
  {
    Log("   failed to get MMDeviceEnumerator");
    return S_FALSE;
  }

  IMMDevice* pEndpoint = NULL;
  IPropertyStore* pProps = NULL;
  LPWSTR pwszID = NULL;

  enumerator->EnumAudioEndpoints(eRender, DEVICE_STATE_ACTIVE, ppMMDevices);
  UINT count = 0;
  hr = (*ppMMDevices)->GetCount(&count);

  if (pLog || hDialog)
  {
    if (hDialog)
    {
      TCHAR* pDefaultDevice = _T("<OS default audio device>");
      SendDlgItemMessage(hDialog, IDC_AUDIO_DEVICE, CB_ADDSTRING, 0, (LPARAM)pDefaultDevice);
      SendDlgItemMessage(hDialog, IDC_AUDIO_DEVICE, CB_SETCURSEL, 0, 0);
    }

    for (UINT i = 0; i < count; i++)
    {
      if ((*ppMMDevices)->Item(i, &pEndpoint) != S_OK)
        break;

      if (pEndpoint->GetId(&pwszID) != S_OK)
        break;

      if (pEndpoint->OpenPropertyStore(STGM_READ, &pProps) != S_OK)
        break;

      PROPVARIANT varName;
      PropVariantInit(&varName);

      PROPVARIANT eventDriven;
      PropVariantInit(&eventDriven);

      PROPVARIANT speakerMask;
      PropVariantInit(&speakerMask);

      if (pProps->GetValue(PKEY_Device_FriendlyName, &varName) != S_OK)
        break;
      
      if (hDialog)
      {
        SendDlgItemMessage(hDialog, IDC_AUDIO_DEVICE, CB_ADDSTRING, 0, (LPARAM)W2T(varName.pwszVal));
        if (m_wWASAPIPreferredDeviceId && wcscmp(pwszID, m_wWASAPIPreferredDeviceId) == 0)
          SendDlgItemMessage(hDialog, IDC_AUDIO_DEVICE, CB_SETCURSEL, i + 1, 0);
      }

      if (pLog)
      {
        Log(" ");
        Log("Audio endpoint %d:", i);
        Log("  %S", varName.pwszVal);
        Log("  %S",  pwszID);

        if (pProps->GetValue(PKEY_AudioEndpoint_Supports_EventDriven_Mode, &eventDriven) == S_OK)
          Log("  supports pull mode: %d", eventDriven.intVal);
        else
          Log("  pull mode query failed!");

        if (pProps->GetValue(PKEY_AudioEndpoint_PhysicalSpeakers, &speakerMask) == S_OK)
          Log("  speaker mask: %d", speakerMask.uintVal);
        else
          Log("  PhysicalSpeakers query failed!");
      }

      CoTaskMemFree(pwszID);
      pwszID = NULL;
      PropVariantClear(&varName);
      PropVariantClear(&eventDriven);
      PropVariantClear(&speakerMask);
      SAFE_RELEASE(pProps)
      SAFE_RELEASE(pEndpoint)
    }
    Log(" ");
  }

  return hr;
}

void AudioRendererSettings::SetAudioDevice(int setting)
{
  CAutoLock settingLock(&m_csSettings);
  IMMDeviceCollection* devices = NULL;
  HRESULT hr = S_OK;

  if (setting == 0) // default audio device
  {
    WCHAR empty[1] = {0};
    wcsncpy(m_wWASAPIPreferredDeviceId, empty, MAX_REG_LENGTH);
  }
  else if (GetAvailableAudioDevices(&devices, NULL, false) == S_OK)
  {
    UINT count = 0;
    hr = devices->GetCount(&count);

    IMMDevice* pEndpoint = NULL;
    LPWSTR pwszID = NULL;

    hr = devices->Item(setting - 1, &pEndpoint);
    if (SUCCEEDED(hr))
    {
      hr = pEndpoint->GetId(&pwszID);
      if (SUCCEEDED(hr))
      {
        // Found the selectde audio endpoint
        wcsncpy(m_wWASAPIPreferredDeviceId, pwszID, MAX_REG_LENGTH);

        SAFE_RELEASE(pEndpoint);
        CoTaskMemFree(pwszID);
        pwszID = NULL;
      }
      else
        Log("  devices->GetId failed: (0x%08x)", hr);
    }
  }

  SAFE_RELEASE(devices);
}

void AudioRendererSettings::SetAudioDevice(LPWSTR setting)
{
  CAutoLock settingLock(&m_csSettings);
  IMMDeviceCollection* devices = NULL;
  HRESULT hr = S_OK;

  if (FAILED(hr))
  {
    Log("SetAudioDevice failed (0x%08x)", hr);
    return;
  }

  if (!setting || wcscmp(setting, L"") == 0) // default audio device
  {
    WCHAR empty[1] = {0};
    wcsncpy(m_wWASAPIPreferredDeviceId, empty, MAX_REG_LENGTH);
  }
  else if (GetAvailableAudioDevices(&devices, NULL, false) == S_OK)
  {
    IMMDevice* pEndpoint = NULL;
    LPWSTR pwszID = NULL;

    UINT count = 0;
    hr = devices->GetCount(&count);
    bool done = false;

    for (UINT i = 0; i < count; i++)
    {
      if (devices->Item(i, &pEndpoint) != S_OK)
        break;

      if (pEndpoint->GetId(&pwszID) != S_OK)
        break;

      if (wcscmp(pwszID, setting) == 0)
      {
        // Found the selectd audio endpoint
        wcsncpy(m_wWASAPIPreferredDeviceId, pwszID, MAX_REG_LENGTH);
        done = true;
      }

      CoTaskMemFree(pwszID);
      pwszID = NULL;

      SAFE_RELEASE(pEndpoint)
      if (done)
        break;
    }
  }

  SAFE_RELEASE(devices);
}

HRESULT AudioRendererSettings::GetPages(CAUUID* pPages)
{
  if (!pPages) 
    return E_POINTER;

  pPages->cElems = 1;
  pPages->pElems = (GUID*)CoTaskMemAlloc(sizeof(GUID));

  if (!pPages->pElems) 
    return E_OUTOFMEMORY;

  pPages->pElems[0] = CLSID_MPARSettingsProp;

  return S_OK;
}

int AudioRendererSettings::GetAC3EncodingMode()
{
  CAutoLock settingLock(&m_csSettings);
  return m_lAC3Encoding;
}

void AudioRendererSettings::SetAC3EncodingMode(int mode)
{
  CAutoLock settingLock(&m_csSettings);
  m_lAC3Encoding = mode;
}

bool AudioRendererSettings::GetLogSampleTimes()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bLogSampleTimes;
}

void AudioRendererSettings::SetLogSampleTimes(bool setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_bLogSampleTimes = setting;
}

bool AudioRendererSettings::GetEnableSyncAdjustment()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bEnableSyncAdjustment;
}

void AudioRendererSettings::SetEnableSyncAdjustment(bool setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_bEnableSyncAdjustment = setting;
}

AUDCLNT_SHAREMODE AudioRendererSettings::GetWASAPIMode()
{
  CAutoLock settingLock(&m_csSettings);
  return m_WASAPIShareMode; 
}

void AudioRendererSettings::SetWASAPIMode(AUDCLNT_SHAREMODE setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_WASAPIShareMode = setting;
}

bool AudioRendererSettings::GetUseWASAPIEventMode()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bWASAPIUseEventMode;
}

void AudioRendererSettings::SetUseWASAPIEventMode(bool setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_bWASAPIUseEventMode = setting;
}

bool AudioRendererSettings::GetUseTimeStretching()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bUseTimeStretching;
}

void AudioRendererSettings::SetUseTimeStretching(bool setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_bUseTimeStretching = setting;
}

bool AudioRendererSettings::GetExpandMonoToStereo()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bExpandMonoToStereo;
}

void AudioRendererSettings::SetExpandMonoToStereo(bool setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_bExpandMonoToStereo = setting;
}

int AudioRendererSettings::GetAC3Bitrate()
{
  CAutoLock settingLock(&m_csSettings);
  return m_AC3bitrate / 1000;
}

void AudioRendererSettings::SetAC3Bitrate(int setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_AC3bitrate = setting * 1000;
}

int AudioRendererSettings::GetSpeakerCount()
{
  CAutoLock settingLock(&m_csSettings);
  return m_lSpeakerCount;
}

int AudioRendererSettings::GetSpeakerConfig()
{
  CAutoLock settingLock(&m_csSettings);
  return m_lSpeakerConfig;
}

void AudioRendererSettings::SetSpeakerConfig(int setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_lSpeakerConfig = setting;
  m_lSpeakerCount = ChannelCount(m_lSpeakerConfig);

  if ((m_lAC3Encoding == FORCED) && m_lSpeakerCount > 6 && m_bForceChannelMixing)
  {
    m_lSpeakerConfig = KSAUDIO_SPEAKER_5POINT1_SURROUND;
    m_lSpeakerCount = 6;
    Log("   Warning: incompatible settings. ForceChannelMixing + AC3 encoding forced + more than 6 channels");
  }
}

bool AudioRendererSettings::GetForceChannelMixing()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bForceChannelMixing;
}

void AudioRendererSettings::SetForceChannelMixing(bool setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_bForceChannelMixing = setting;
}

int AudioRendererSettings::GetAudioDelay()
{
  CAutoLock settingLock(&m_csSettings);
  return m_lAudioDelay;
}

void AudioRendererSettings::SetAudioDelay(int setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_lAudioDelay = setting;
}

int AudioRendererSettings::GetOutputBuffer()
{
  CAutoLock settingLock(&m_csSettings);
  return m_msOutputBuffer;
}

void AudioRendererSettings::SetOutputBuffer(int setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_msOutputBuffer = setting;
}

int AudioRendererSettings::GetSampleRate()
{
  CAutoLock settingLock(&m_csSettings);
  return m_nForceSamplingRate;
}

void AudioRendererSettings::SetSampleRate(int setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_nForceSamplingRate = setting;
}

int AudioRendererSettings::GetBitDepth()
{
  CAutoLock settingLock(&m_csSettings);
  return m_nForceBitDepth;
}

void AudioRendererSettings::SetBitDepth(int setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_nForceBitDepth = setting;
}

int AudioRendererSettings::GetResamplingQuality()
{
  CAutoLock settingLock(&m_csSettings);
  return m_nResamplingQuality;
}

void AudioRendererSettings::SetResamplingQuality(int setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_nResamplingQuality = setting;
}

bool AudioRendererSettings::GetReleaseDeviceOnStop()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bReleaseDeviceOnStop;
}

bool AudioRendererSettings::GetAllowBitStreaming()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bAllowBitStreaming;
}

double AudioRendererSettings::GetMinBias()
{
  CAutoLock settingLock(&m_csSettings);
  return m_dMinBias;
}

double AudioRendererSettings::GetMaxBias()
{
  CAutoLock settingLock(&m_csSettings);
  return m_dMaxBias;
}

bool AudioRendererSettings::GetLogDebug()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bLogDebug;
}

bool AudioRendererSettings::GetUseWASAPI()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bUseWASAPI;
}

void AudioRendererSettings::SetUseWASAPI(bool setting)
{
  CAutoLock settingLock(&m_csSettings);
  m_bUseWASAPI = setting;
}

int AudioRendererSettings::GetUseFilters()
{
  CAutoLock settingLock(&m_csSettings);
  return m_nUseFilters;
}

DWORD AudioRendererSettings::GetForceBitDepth()
{
  CAutoLock settingLock(&m_csSettings);
  return m_nForceBitDepth;
}

DWORD AudioRendererSettings::GetForceSamplingRate()
{
  CAutoLock settingLock(&m_csSettings);
  return m_nForceSamplingRate;
}

REFERENCE_TIME AudioRendererSettings::GetPeriod()
{
  CAutoLock settingLock(&m_csSettings);
  return m_hnsPeriod;
}

void AudioRendererSettings::SetPeriod(REFERENCE_TIME period)
{
  CAutoLock settingLock(&m_csSettings);
  m_hnsPeriod = period;
}

bool AudioRendererSettings::GetHWBasedRefClock()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bHWBasedRefClock;
}

bool AudioRendererSettings::GetQuality_USE_QUICKSEEK()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bQuality_USE_QUICKSEEK;
}

bool AudioRendererSettings::GetQuality_USE_AA_FILTER()
{
  CAutoLock settingLock(&m_csSettings);
  return m_bQuality_USE_QUICKSEEK;
}

int AudioRendererSettings::GetQuality_AA_FILTER_LENGTH()
{
  CAutoLock settingLock(&m_csSettings);
  return m_lQuality_AA_FILTER_LENGTH;
}

int AudioRendererSettings::GetQuality_SEQUENCE_MS()
{
  CAutoLock settingLock(&m_csSettings);
  return m_lQuality_SEQUENCE_MS;
}

int AudioRendererSettings::GetQuality_SEEKWINDOW_MS()
{
  CAutoLock settingLock(&m_csSettings);
  return m_lQuality_SEEKWINDOW_MS;
}

HRESULT AudioRendererSettings::GetBool(MPARSetting setting, bool* pValue)
{
  CheckPointer(pValue, E_POINTER);

  HRESULT hr = S_OK;
  CAutoLock settingLock(&m_csSettings);

  switch (setting)
  {
    case LOG_SAMPLE_TIMES:
      *pValue = m_bLogSampleTimes;
      break;

    case ENABLE_SYNC_ADJUSTMENT:
      *pValue = m_bEnableSyncAdjustment;
      break;

    case WASAPI_EVENT_DRIVEN:
      *pValue = m_bWASAPIUseEventMode;
      break;

    case ENABLE_TIME_STRETCHING:
      *pValue = m_bUseTimeStretching;
      break;

    case EXPAND_MONO_TO_STEREO:
      *pValue = m_bExpandMonoToStereo;
      break;

    case FORCE_CHANNEL_MIXING:
      *pValue = m_bForceChannelMixing;
      break;

    case ALLOW_BITSTREAMING:
      *pValue = m_bAllowBitStreaming;

    default:
      hr = E_NOTIMPL;
  }

  return hr;
}

HRESULT AudioRendererSettings::SetBool(MPARSetting setting, bool value)
{
  HRESULT hr = S_OK;
  CAutoLock settingLock(&m_csSettings);

  switch (setting)
  {
    case LOG_SAMPLE_TIMES:
      m_bLogSampleTimes = value;
      break;

    case ENABLE_SYNC_ADJUSTMENT:
      m_bEnableSyncAdjustment = value;
      break;

    case WASAPI_EVENT_DRIVEN:
      m_bWASAPIUseEventMode = value;
      break;

    case ENABLE_TIME_STRETCHING:
      m_bUseTimeStretching = value;
      break;

    case EXPAND_MONO_TO_STEREO:
      m_bExpandMonoToStereo = value;
      break;

    case FORCE_CHANNEL_MIXING:
      m_bForceChannelMixing = value;
      break;

    case ALLOW_BITSTREAMING:
      m_bAllowBitStreaming = value;

    default:
      hr = E_NOTIMPL;
  }

  return hr;
}

HRESULT AudioRendererSettings::GetInt(MPARSetting setting, int* pValue)
{
  CheckPointer(pValue, E_POINTER);

  HRESULT hr = S_OK;
  CAutoLock settingLock(&m_csSettings);

  switch (setting)
  {
    case AC3_ENCODING:
      *pValue = m_lAC3Encoding;
      break;
    
    case WASAPI_MODE:
      if (m_WASAPIShareMode == AUDCLNT_SHAREMODE_SHARED)
        *pValue = SHARED;
      else
        *pValue = EXCLUSIVE;
      break;

    case AC3_BITRATE:
      *pValue = m_AC3bitrate;
      break;

    case SPEAKER_CONFIG:
      *pValue = m_lSpeakerConfig;
      break;

    case AUDIO_DELAY:
      *pValue = m_lAudioDelay;
      break;

    case OUTPUT_BUFFER_LENGTH:
      *pValue = m_msOutputBuffer;
      break;

    case SAMPLE_RATE:
      *pValue = m_nForceSamplingRate;
      break;

    case BIT_DEPTH:
      *pValue = m_nForceBitDepth;
      break;

    case LIB_RESAMPLE_QUALITY:
      *pValue = m_nResamplingQuality;
      break;

    default:
      hr = E_NOTIMPL;
  }

  return hr;
}

HRESULT AudioRendererSettings::SetInt(MPARSetting setting, int value)
{
  HRESULT hr = S_OK;
  CAutoLock settingLock(&m_csSettings);

  switch (setting)
  {
    case AC3_ENCODING:
      if (IsValidAC3EncodingMode(value))
        m_lAC3Encoding = value;
      else
        hr = E_INVALIDARG;

      break;

    case WASAPI_MODE:
      if (IsValidWASAPIMode(value))
      {
        if (value == SHARED)
          m_WASAPIShareMode = AUDCLNT_SHAREMODE_SHARED;
        else 
          m_WASAPIShareMode = AUDCLNT_SHAREMODE_EXCLUSIVE;
      }
      else
        hr = E_INVALIDARG;

      break;

    case AC3_BITRATE:
      if (IsValidAC3Bitrate(value))
        m_AC3bitrate = value;
      else
        hr = E_INVALIDARG;

      break;

    case SPEAKER_CONFIG:
      if (IsValidSpeakerConfig(value))
        m_lSpeakerConfig = value;
      else
        hr = E_INVALIDARG;

      break;

    case AUDIO_DELAY:
      m_lAudioDelay = value;
      break;

    case OUTPUT_BUFFER_LENGTH:
      m_msOutputBuffer = value;
      break;

    case SAMPLE_RATE:
      if (IsValidSampleRate(value))
        m_nForceSamplingRate = value;
      else
        hr = E_INVALIDARG;

      break;

    case BIT_DEPTH:
      if (IsValidBitDepth(value))
        m_nForceBitDepth = value;
      else
        hr = E_INVALIDARG;

      break;

    case LIB_RESAMPLE_QUALITY:
      if (IsValidResamplingQuality(value))
        m_nResamplingQuality = value;
      else
        hr = E_INVALIDARG;

    case USE_FILTERS:
      if (IsValidUseFilters(value))
        m_nUseFilters = value;
      else
        hr = E_INVALIDARG;

      break;

    default:
      hr = E_NOTIMPL;
  }

  return hr;
}

STDMETHODIMP AudioRendererSettings::GetString(MPARSetting setting, LPWSTR* ppValue)
{
  CheckPointer(*ppValue, E_POINTER);
  return E_NOTIMPL;
}

STDMETHODIMP AudioRendererSettings::SetString(MPARSetting setting, LPWSTR pValue)
{
  CheckPointer(pValue, E_POINTER);

  HRESULT hr = S_OK;

  switch (setting)
  {
    case SETTING_AUDIO_DEVICE:
      SetAudioDevice(pValue);
      break;

    default:
      hr = E_NOTIMPL;
  }

  return hr;
}

bool AudioRendererSettings::IsValidSampleRate(int value)
{
  return AllowedValue(gAllowedSampleRates, sizeof(gAllowedSampleRates) / sizeof(int), value);
}

bool AudioRendererSettings::IsValidBitDepth(int value)
{
  return AllowedValue(gAllowedBitDepths, sizeof(gAllowedBitDepths) / sizeof(int), value);
}

bool AudioRendererSettings::IsValidResamplingQuality(int value)
{
  return AllowedValue(gAllowedResamplingQualities, sizeof(gAllowedResamplingQualities) / sizeof(int), value);
}

bool AudioRendererSettings::IsValidSpeakerConfig(int value)
{
  return AllowedValue(speakerConfigs, sizeof(speakerConfigs) / sizeof(int), value);
}

bool AudioRendererSettings::IsValidWASAPIMode(int value)
{
  return value == SHARED || value == EXCLUSIVE;
}

bool AudioRendererSettings::IsValidAC3Bitrate(int value)
{
  return AllowedValue(gAllowedAC3bitrates, sizeof(gAllowedAC3bitrates) / sizeof(int), value);
}

bool AudioRendererSettings::IsValidAC3EncodingMode(int value)
{
  return value == DISABLED || value == AUTO || value == FORCED;
}

bool AudioRendererSettings::IsValidOutputBuffer(int value)
{
  return MIN_OUTPUT_BUFFER <= value && value <= MAX_OUTPUT_BUFFER;
}

bool AudioRendererSettings::IsValidAudioDelay(int value)
{
  return MIN_AUDIO_DELAY <= value && value <= MAX_AUDIO_DELAY;
}

bool AudioRendererSettings::IsValidUseFilters(int value)
{
  value &= ~USE_FILTERS_AC3ENCODER;
  value &= ~USE_FILTERS_BIT_DEPTH_IN;
  value &= ~USE_FILTERS_BIT_DEPTH_OUT;
  value &= ~USE_FILTERS_TIME_STRETCH;
  value &= ~USE_FILTERS_SAMPLE_RATE_CONVERTER;
  value &= ~USE_FILTERS_CHANNEL_MIXER;

  if (value == 0)
    return true;
  else
    return false;
}
