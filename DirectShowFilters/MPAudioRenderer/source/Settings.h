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

#pragma once

#include "stdafx.h"

#include <mmdeviceapi.h>
#include <Avrt.h>
#include <audioclient.h>

#define MAX_REG_LENGTH 256

enum AC3Encoding
{ 
  DISABLED = 0,
  AUTO,
  FORCED
};

// {101A150C-DD66-4230-BFD7-168CC184408C}
DEFINE_GUID(IID_IMPARSettings,
0x101a150c, 0xdd66, 0x4230, 0xbf, 0xd7, 0x16, 0x8c, 0xc1, 0x84, 0x40, 0x8c);

// {2BF166B2-3333-4CBF-9FA9-F0EEAB1F988D}
DEFINE_GUID(CLSID_MPARSettingsProp,
0x2bf166b2, 0x3333, 0x4cbf, 0x9f, 0xa9, 0xf0, 0xee, 0xab, 0x1f, 0x98, 0x8d);

interface IMPARSettings : public IUnknown
{
  virtual int  GetAC3EncodingMode() = 0;
  virtual void SetAC3EncodingMode(int setting) = 0;

  virtual bool GetLogSampleTimes() = 0;
  virtual void SetLogSampleTimes(bool setting) = 0;

  virtual bool GetEnableSyncAdjustment() = 0;
  virtual void SetEnableSyncAdjustment(bool setting) = 0;

  virtual AUDCLNT_SHAREMODE GetWASAPIMode() = 0;
  virtual void SetWASAPIMode(AUDCLNT_SHAREMODE setting) = 0;

  virtual bool GetUseWASAPIEventMode() = 0;
  virtual void SetUseWASAPIEventMode(bool setting) = 0;

  virtual bool GetUseTimeStretching() = 0;
  virtual void SetUseTimeStretching(bool setting) = 0;

  virtual bool GetExpandMonoToStereo() = 0;
  virtual void SetExpandMonoToStereo(bool setting) = 0;

  virtual int GetAC3Bitrate() = 0;
  virtual void SetAC3Bitrate(int setting) = 0;

  virtual int GetSpeakerConfig() = 0;
  virtual void SetSpeakerConfig(int setting) = 0;

  virtual bool GetForceChannelMixing() = 0;
  virtual void SetForceChannelMixing(bool setting) = 0;

  virtual int GetAudioDelay() = 0;
  virtual void SetAudioDelay(int setting) = 0;

  virtual int GetSampleRate() = 0;
  virtual void SetSampleRate(int setting) = 0;

  virtual int GetBitDepth() = 0;
  virtual void SetBitDepth(int setting) = 0;

  virtual int GetResamplingQuality() = 0;
  virtual void SetResamplingQuality(int setting) = 0;

  virtual LPCTSTR ResamplingQualityAsString(int setting) = 0;
  virtual LPCTSTR SpeakerConfigAsString(int setting) = 0;

  virtual void SaveSettingsToRegistry(HKEY hKey) = 0;

  virtual HRESULT GetAvailableAudioDevices(IMMDeviceCollection** ppMMDevices, HWND hDialog, bool pLog) = 0;
  virtual void SetAudioDevice(int setting) = 0;
};

class AudioRendererSettings : public CUnknown, public IMPARSettings, public ISpecifyPropertyPages
{
public:
  AudioRendererSettings();
  ~AudioRendererSettings();

public:
  DECLARE_IUNKNOWN;

  // ISpecifyPropertyPages
  STDMETHODIMP GetPages(CAUUID* pPages);

  HRESULT GetAudioDevice(IMMDevice** ppMMDevice);
  
  // IMPARSettings
  int  GetAC3EncodingMode();
  void SetAC3EncodingMode(int setting);

  bool GetLogSampleTimes();
  void SetLogSampleTimes(bool setting);

  bool GetEnableSyncAdjustment();
  void SetEnableSyncAdjustment(bool setting);

  AUDCLNT_SHAREMODE GetWASAPIMode();
  void SetWASAPIMode(AUDCLNT_SHAREMODE setting);

  bool GetUseWASAPIEventMode();
  void SetUseWASAPIEventMode(bool setting);

  bool GetUseTimeStretching();
  void SetUseTimeStretching(bool setting);

  bool GetExpandMonoToStereo();
  void SetExpandMonoToStereo(bool setting);

  int GetAC3Bitrate();
  void SetAC3Bitrate(int setting);

  int GetSpeakerConfig();
  void SetSpeakerConfig(int setting);

  bool GetForceChannelMixing();
  void SetForceChannelMixing(bool setting);

  int GetAudioDelay();
  void SetAudioDelay(int setting);

  int GetSampleRate();
  void SetSampleRate(int setting);

  int GetBitDepth();
  void SetBitDepth(int setting);

  int GetResamplingQuality();
  void SetResamplingQuality(int setting);

  void SaveSettingsToRegistry(HKEY hKey);

  LPCTSTR ResamplingQualityAsString(int setting);
  LPCTSTR SpeakerConfigAsString(int setting);

  HRESULT GetAvailableAudioDevices(IMMDeviceCollection** ppMMDevices, HWND hDialog, bool pLog);
  void SetAudioDevice(int setting);

  bool m_bLogSampleTimes;
  bool m_bLogDebug;
  bool m_bHWBasedRefClock;
  bool m_bEnableSyncAdjustment;
  bool m_bUseWASAPI;
  bool m_bWASAPIUseEventMode;
  bool m_bUseTimeStretching;
  bool m_bExpandMonoToStereo;
  int  m_lAC3Encoding;
  
  bool m_bQuality_USE_QUICKSEEK;
  bool m_bQuality_USE_AA_FILTER;
  
  int m_lQuality_AA_FILTER_LENGTH;
  int m_lQuality_SEQUENCE_MS;
  int m_lQuality_SEEKWINDOW_MS;
  int m_lQuality_OVERLAP_MS;

  int m_AC3bitrate;
  double m_dMaxBias;
  double m_dMinBias;

  int m_lSpeakerConfig; // channel mask
  int m_lSpeakerCount;
  bool m_bForceChannelMixing;

  int m_lAudioDelay;

  int m_nResamplingQuality;

  DWORD m_nForceSamplingRate;
  DWORD m_nForceBitDepth;

  bool m_bReleaseDeviceOnStop;
  
  REFERENCE_TIME m_hnsPeriod;

  AUDCLNT_SHAREMODE m_WASAPIShareMode;
  
  WCHAR* m_wWASAPIPreferredDeviceId;

private:
   // For accessing the registry
  void LoadSettingsFromRegistry();
  void ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);
  void WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);
  void ReadRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data);
  void WriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data);

  bool AllowedValue(unsigned int allowedRates[], unsigned int size, unsigned int rate);
  unsigned int ChannelCount(unsigned int channelMask);

  // TODO lock against this for dynamic setting changes
  // CCritSec  m_csSettings;
};
