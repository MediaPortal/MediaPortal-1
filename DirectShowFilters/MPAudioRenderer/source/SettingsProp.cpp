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

#include "SettingsProp.h"
#include "resource.h"

#include "alloctracing.h"

extern unsigned int gAllowedAC3bitrates[9];
extern unsigned int gAllowedSampleRates[7];
extern unsigned int gAllowedBitDepths[4];
extern unsigned int gAllowedResamplingQualities[5];
extern unsigned int speakerConfigs[7];

CUnknown* WINAPI CSettingsProp::CreateInstance(LPUNKNOWN pUnk, HRESULT* pHr)
{
  CSettingsProp* pNewObject = new CSettingsProp(pUnk);
  if (!pNewObject) 
    *pHr = E_OUTOFMEMORY;
  return pNewObject;
}

CSettingsProp::CSettingsProp(LPUNKNOWN lpunk) :
  CBasePropertyPage(NAME("SettingsProp"), lpunk, IDD_PROPPAGE, IDS_PROPPAGE_TITLE),
  m_pSettings(NULL)
{
}

CSettingsProp::~CSettingsProp()
{
}

HRESULT CSettingsProp::OnConnect(IUnknown* pUnknown)
{
  if (!pUnknown)
    return E_POINTER;

  ASSERT(!m_pSettings);
  return pUnknown->QueryInterface(IID_IMPARSettings, reinterpret_cast<void**>(&m_pSettings));
}

HRESULT CSettingsProp::OnDisconnect()
{
  if (m_pSettings)
    m_pSettings->Release();

  return S_OK;
}

HRESULT CSettingsProp::OnActivate()
{
  INITCOMMONCONTROLSEX icc;
  icc.dwSize = sizeof(INITCOMMONCONTROLSEX);
  icc.dwICC = ICC_BAR_CLASSES;

  if (InitCommonControlsEx(&icc) == FALSE)
    return E_FAIL;

  // Log sample times
  SendDlgItemMessage(m_Dlg, IDC_LOG_SAMPLE_TIMES, BM_SETCHECK, m_pSettings->GetLogSampleTimes(), 0);

  // Enable V-sync adjustment
  SendDlgItemMessage(m_Dlg, IDC_ENABLE_VSYNC, BM_SETCHECK, m_pSettings->GetEnableSyncAdjustment(), 0);

  // Use exclusive WASAPI mode
  SendDlgItemMessage(m_Dlg, IDC_USE_EXCLUSIVE, BM_SETCHECK, m_pSettings->GetWASAPIMode() == AUDCLNT_SHAREMODE_EXCLUSIVE, 0);

  // Use event driven WASAPI mode
  SendDlgItemMessage(m_Dlg, IDC_USE_EVENT_DRIVEN, BM_SETCHECK, m_pSettings->GetUseWASAPIEventMode() , 0);

  // Use timestretching
  SendDlgItemMessage(m_Dlg, IDC_ENABLE_TIMESTRETCH, BM_SETCHECK, m_pSettings->GetUseTimeStretching() , 0);

  // Force channel mixing
  SendDlgItemMessage(m_Dlg, IDC_FORCE_MIXING, BM_SETCHECK, m_pSettings->GetForceChannelMixing() , 0);

  // Expand mono to stereo
  SendDlgItemMessage(m_Dlg, IDC_EXPAND_MONO, BM_SETCHECK, m_pSettings->GetExpandMonoToStereo() , 0);

  CHAR settingString[256] = "";

  CHAR stringAuto[] = "Auto";
  CHAR stringDisabled[] = "Disabled";
  CHAR stringForced[] = "Forced";

  // AC3 encoding mode
  SendDlgItemMessage(m_Dlg, IDC_AC3_MODE, CB_RESETCONTENT, 0, 0);
  SendDlgItemMessage(m_Dlg, IDC_AC3_MODE, CB_ADDSTRING, 0, (LPARAM)stringDisabled);
  SendDlgItemMessage(m_Dlg, IDC_AC3_MODE, CB_ADDSTRING, 0, (LPARAM)stringAuto);
  SendDlgItemMessage(m_Dlg, IDC_AC3_MODE, CB_ADDSTRING, 0, (LPARAM)stringForced);
  SendDlgItemMessage(m_Dlg, IDC_AC3_MODE, CB_SETCURSEL, m_pSettings->GetAC3EncodingMode(), 0);

  // AC3 bitrate
  SendDlgItemMessage(m_Dlg, IDC_AC3_BITRATE, CB_RESETCONTENT, 0, 0);
  int numOfBitrates = sizeof(gAllowedAC3bitrates) / sizeof(int);
  unsigned int AC3Bitrate = m_pSettings->GetAC3Bitrate();

  for (unsigned int i = 0; i < numOfBitrates; i++) 
  {
    sprintf_s(settingString, "%d", gAllowedAC3bitrates[i]);
    SendDlgItemMessage(m_Dlg, IDC_AC3_BITRATE, CB_ADDSTRING, 0, (LPARAM)settingString);
    if (AC3Bitrate == gAllowedAC3bitrates[i])
      SendDlgItemMessage(m_Dlg, IDC_AC3_BITRATE, CB_SETCURSEL, i, 0);
  }

  // Audio delay
  sprintf_s(settingString, "%d", m_pSettings->GetAudioDelay());
  SendDlgItemMessage(m_Dlg, IDC_AUDIO_DELAY, WM_SETTEXT, 0, (LPARAM)settingString);

  // Bit depth
  CHAR string16[] = "16";
  CHAR string24[] = "24";
  CHAR string32[] = "32";
  
  SendDlgItemMessage(m_Dlg, IDC_BITDEPTH, CB_RESETCONTENT, 0, 0);
  SendDlgItemMessage(m_Dlg, IDC_BITDEPTH, CB_ADDSTRING, 0, (LPARAM)stringAuto);
  SendDlgItemMessage(m_Dlg, IDC_BITDEPTH, CB_ADDSTRING, 0, (LPARAM)string16);
  SendDlgItemMessage(m_Dlg, IDC_BITDEPTH, CB_ADDSTRING, 0, (LPARAM)string24);
  SendDlgItemMessage(m_Dlg, IDC_BITDEPTH, CB_ADDSTRING, 0, (LPARAM)string32);
  
  int nBitDepth = m_pSettings->GetBitDepth();
  int itemToFocus = 0;

  switch (nBitDepth)
  {
    case 16:
      itemToFocus = 1;
      break;
    case 24:
      itemToFocus = 2;
      break;
    case 32:
      itemToFocus = 3;
      break;
    case 0:
    default:
      itemToFocus = 0;
  }

  SendDlgItemMessage(m_Dlg, IDC_BITDEPTH, CB_SETCURSEL, itemToFocus, 0);

  // Sample rate
  SendDlgItemMessage(m_Dlg, IDC_SAMPLERATE, CB_RESETCONTENT, 0, 0);
  SendDlgItemMessage(m_Dlg, IDC_SAMPLERATE, CB_ADDSTRING, 0, (LPARAM)stringAuto);
  
  unsigned int nNumOfSampleRates = sizeof(gAllowedSampleRates) / sizeof(int);
  unsigned int nSampleRate = m_pSettings->GetSampleRate();

  for (unsigned int i = 0; i < nNumOfSampleRates; i++)
  {
    sprintf_s(settingString, "%d", gAllowedSampleRates[i]);
    SendDlgItemMessage(m_Dlg, IDC_SAMPLERATE, CB_ADDSTRING, 0, (LPARAM)settingString);
    if (nSampleRate == gAllowedSampleRates[i])
      SendDlgItemMessage(m_Dlg, IDC_SAMPLERATE, CB_SETCURSEL, i + 1, 0);
  }

  if (nSampleRate == 0) // Auto
    SendDlgItemMessage(m_Dlg, IDC_SAMPLERATE, CB_SETCURSEL, 0, 0);

  // Resampling quality
  SendDlgItemMessage(m_Dlg, IDC_RESAMPLING_QUALITY, CB_RESETCONTENT, 0, 0);
  
  int nResamplingQuality = m_pSettings->GetResamplingQuality();

  for (int i = 0; i < 5; i++)
  {
    SendDlgItemMessage(m_Dlg, IDC_RESAMPLING_QUALITY, CB_ADDSTRING, 0, (LPARAM)m_pSettings->ResamplingQualityAsString(i));
    if (nResamplingQuality == i)
      SendDlgItemMessage(m_Dlg, IDC_RESAMPLING_QUALITY, CB_SETCURSEL, i, 0);
  }

  // Speaker config
  SendDlgItemMessage(m_Dlg, IDC_SPEAKER_SETUP, CB_RESETCONTENT, 0, 0);
  
  int nNumOfSpeakerConfigs = sizeof(speakerConfigs) / sizeof(int);
  unsigned int nSpeakerSetup = m_pSettings->GetSpeakerConfig();

  for (unsigned int i = 0; i < nNumOfSpeakerConfigs; i++)
  {
    int nConfig = speakerConfigs[i];
    sprintf_s(settingString, "%d", nConfig);
    SendDlgItemMessage(m_Dlg, IDC_SPEAKER_SETUP, CB_ADDSTRING, 0, (LPARAM)m_pSettings->SpeakerConfigAsString(nConfig));
    if (nSpeakerSetup == speakerConfigs[i])
      SendDlgItemMessage(m_Dlg, IDC_SPEAKER_SETUP, CB_SETCURSEL, i, 0);
  }

  CComPtr<IMMDeviceEnumerator> enumerator;
  IMMDeviceCollection* devices = NULL;
  HRESULT hr = enumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));

  if (SUCCEEDED(hr))
    m_pSettings->GetAvailableAudioDevices(&devices, m_Dlg, false);
  
  return S_OK;
}

HRESULT CSettingsProp::OnApplyChanges()
{
  bool bFlag = false;
  int nValue = 0;
  
  // Log sample times
  bFlag = (bool)SendDlgItemMessage(m_Dlg, IDC_LOG_SAMPLE_TIMES, BM_GETCHECK, 0, 0);
  m_pSettings->SetLogSampleTimes(bFlag);

  // Enable V-sync adjustment
  bFlag = (bool)SendDlgItemMessage(m_Dlg, IDC_ENABLE_VSYNC, BM_GETCHECK, 0, 0);
  m_pSettings->SetEnableSyncAdjustment(bFlag);

  // Use exclusive WASAPI mode
  bFlag = (bool)SendDlgItemMessage(m_Dlg, IDC_USE_EXCLUSIVE, BM_GETCHECK, 0, 0);
  if (bFlag)
    m_pSettings->SetWASAPIMode(AUDCLNT_SHAREMODE_EXCLUSIVE);
  else
    m_pSettings->SetWASAPIMode(AUDCLNT_SHAREMODE_SHARED);

  // Use event driven WASAPI mode
  bFlag = (bool)SendDlgItemMessage(m_Dlg, IDC_USE_EVENT_DRIVEN, BM_GETCHECK, 0, 0);
  m_pSettings->SetUseWASAPIEventMode(bFlag);

  // Use timestretching
  bFlag = (bool)SendDlgItemMessage(m_Dlg, IDC_ENABLE_TIMESTRETCH, BM_GETCHECK, 0, 0);
  m_pSettings->SetUseTimeStretching(bFlag);

  // Force channel mixing
  bFlag = (bool)SendDlgItemMessage(m_Dlg, IDC_FORCE_MIXING, BM_GETCHECK, 0, 0);
  m_pSettings->SetForceChannelMixing(bFlag);

  // Expand mono to stereo
  bFlag = (bool)SendDlgItemMessage(m_Dlg, IDC_EXPAND_MONO, BM_GETCHECK, 0, 0);
  m_pSettings->SetExpandMonoToStereo(bFlag);

  // AC3 encoding mode
  nValue = (int)SendDlgItemMessage(m_Dlg, IDC_AC3_MODE, CB_GETCURSEL, 0, 0);
  m_pSettings->SetAC3EncodingMode(nValue);

  // AC3 bitrate
  nValue = (int)SendDlgItemMessage(m_Dlg, IDC_AC3_BITRATE, CB_GETCURSEL, 0, 0);
  m_pSettings->SetAC3Bitrate(gAllowedAC3bitrates[nValue]);
   
  // Audio delay
  CHAR delay[50];
  SendDlgItemMessage(m_Dlg, IDC_AUDIO_DELAY, WM_GETTEXT, 50, (LPARAM)&delay);
  m_pSettings->SetAudioDelay(atoi(delay));

  nValue = (int)SendDlgItemMessage(m_Dlg, IDC_BITDEPTH, CB_GETCURSEL, 0, 0);

  int nBitDepth = 0;

  switch (nValue)
  {
    case 1:
      nBitDepth = 16;
      break;
    case 2:
      nBitDepth = 24;
      break;
    case 3:
      nBitDepth = 32;
      break;
    case 0:
    default:
      nBitDepth = 0;
  }
  m_pSettings->SetBitDepth(nBitDepth);

  // Sample rate
  nValue = (int)SendDlgItemMessage(m_Dlg, IDC_SAMPLERATE, CB_GETCURSEL, 0, 0);
  if (nValue == 0)
    m_pSettings->SetSampleRate(0);
  else
    m_pSettings->SetSampleRate(gAllowedSampleRates[nValue - 1]);

  // Resampling quality
  nValue = (int)SendDlgItemMessage(m_Dlg, IDC_RESAMPLING_QUALITY, CB_GETCURSEL, 0, 0);
  m_pSettings->SetResamplingQuality(nValue);

  // Speaker config
  nValue = (int)SendDlgItemMessage(m_Dlg, IDC_SPEAKER_SETUP, CB_GETCURSEL, 0, 0);
  m_pSettings->SetSpeakerConfig(speakerConfigs[nValue]);

  // WASAPI device
  nValue = (int)SendDlgItemMessage(m_Dlg, IDC_AUDIO_DEVICE, CB_GETCURSEL, 0, 0);
  m_pSettings->SetAudioDevice(nValue);

  m_pSettings->SaveSettingsToRegistry(NULL);

  return S_OK;
} 

INT_PTR CSettingsProp::OnReceiveMessage(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
  switch (uMsg)
  {
    case WM_COMMAND:
    {
      if ((LOWORD(wParam) == IDC_AC3_MODE ||
            LOWORD(wParam) == IDC_AC3_BITRATE ||
            LOWORD(wParam) == IDC_USE_EXCLUSIVE ||
            LOWORD(wParam) == IDC_USE_EVENT_DRIVEN ||
            LOWORD(wParam) == IDC_AUDIO_DEVICE ||
            LOWORD(wParam) == IDC_EXPAND_MONO ||
            LOWORD(wParam) == IDC_ENABLE_VSYNC ||
            LOWORD(wParam) == IDC_ENABLE_TIMESTRETCH ||
            LOWORD(wParam) == IDC_SPEAKER_SETUP ||
            LOWORD(wParam) == IDC_FORCE_MIXING ||
            LOWORD(wParam) == IDC_SAMPLERATE ||
            LOWORD(wParam) == IDC_RESAMPLING_QUALITY ||
            LOWORD(wParam) == IDC_BITDEPTH ||
            LOWORD(wParam) == IDC_LOG_SAMPLE_TIMES) && 
            HIWORD(wParam) == BN_CLICKED || HIWORD(wParam) == CBN_SELCHANGE)
      {
        // TODO: check if any of the setting has really changed
        m_bDirty = TRUE;
        if (m_pPageSite)
          m_pPageSite->OnStatusChange(PROPPAGESTATUS_DIRTY);
      }
      else if (LOWORD(wParam) == IDC_AUDIO_DELAY && HIWORD(wParam) == EN_CHANGE)
      {
        CHAR delay[50];
        SendDlgItemMessage(m_Dlg, IDC_AUDIO_DELAY, WM_GETTEXT, 50, (LPARAM)&delay);
        if (atoi(delay) != m_pSettings->GetAudioDelay())
        {  
          m_bDirty = TRUE;
          if (m_pPageSite)
            m_pPageSite->OnStatusChange(PROPPAGESTATUS_DIRTY);
        }
      }
    }
  }

  return __super::OnReceiveMessage(hwnd, uMsg, wParam, lParam);
}
