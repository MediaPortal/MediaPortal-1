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

#pragma once

#include <dsound.h>
#include <MMReg.h>  //must be before other Wasapi headers
#include <strsafe.h>
#include <mmdeviceapi.h>
#include <Avrt.h>
#include <audioclient.h>

#define MAX_REG_LENGTH 256

class AudioRendererSettings
{
public:  
  AudioRendererSettings();
  ~AudioRendererSettings();

public:
  bool m_bLogSampleTimes;
  bool m_bHWBasedRefClock;
  bool m_bEnableSyncAdjustment;
  bool m_bUseWASAPI;
  bool m_WASAPIUseEventMode;
  bool m_bUseTimeStretching;
  bool m_bEnableAC3Encoding;
  
  int m_AC3bitrate;
  double m_dMaxBias;
  double m_dMinBias;

  int m_lAudioDelay;

  DWORD m_dwChannelMaskOverride_5_1;
  DWORD m_dwChannelMaskOverride_7_1;

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
};
