// Copyright (C) 2005-2014 Team MediaPortal
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

// Available audio renderer settings that are available thru IMPAudioSettings interface
enum MPARSetting
{
  AC3_ENCODING = 0,           // enum (MPARAC3EncodingMode)
  LOG_SAMPLE_TIMES = 1,       // bool
  ENABLE_SYNC_ADJUSTMENT = 2, // bool
  WASAPI_MODE = 3,            // enum (MPARWASAPIMode)
  WASAPI_EVENT_DRIVEN = 4,    // bool
  ENABLE_TIME_STRETCHING = 5, // bool
  EXPAND_MONO_TO_STEREO = 6,  // bool
  AC3_BITRATE = 7,            // enum (MPARAC3Bitrate)
  SPEAKER_CONFIG = 8,         // enum (MPARSpeakerConfig)
  FORCE_CHANNEL_MIXING = 9,   // bool
  AUDIO_DELAY = 10,           // int (audio delay in ms)
  OUTPUT_BUFFER_LENGTH = 11,  // int (output buffer in ms)
  SAMPLE_RATE = 12,           // enum (MPARSampleRate)
  BIT_DEPTH = 13,             // enum (MPARBitDepth)
  LIB_RESAMPLE_QUALITY = 14,  // enum (MPARLibResampleQuality)
  USE_FILTERS = 15,           // int (flags from MPARUseFilters enum)
  SETTING_AUDIO_DEVICE = 16,  // wchar
  ALLOW_BITSTREAMING = 17     // bool
};

enum MPARWASAPIMode
{
  SHARED = 0,
  EXCLUSIVE = 1
};

enum MPARAC3Bitrate
{
  RATE_192 = 192,
  RATE_224 = 224,
  RATE_256 = 256,
  RATE_320 = 320,
  RATE_384 = 384,
  RATE_448 = 448,
  RATE_512 = 512,
  RATE_576 = 576,
  RATE_640 = 640
};

enum MPARAC3EncodingMode
{ 
  DISABLED = 0,
  AUTO,
  FORCED
};

enum MPARSpeakerConfig
{
  SPEAKER_MONO = 4,
  SPEAKER_STEREO = 3,
  SPEAKER_QUAD = 51,
  SPEAKER_SURROUND = 263,
  SPEAKER_5POINT1 = 63,
  SPEAKER_5POINT1_SURROUND = 1551,
  SPEAKER_7POINT1_SURROUND = 1599
};

enum MPARSampleRate
{
  RATE_22050 = 22050,
  RATE_32000 = 32000,
  RATE_44100 = 44100,
  RATE_48000 = 48000,
  RATE_88200 = 88200,
  RATE_96000 = 96000,
  RATE_192000 = 192000
};

enum MPARBitDepth
{
  DEPTH_8 = 8,
  DEPTH_16 = 16,
  DEPTH_24 = 24,
  DEPTH_32 = 32
};

enum MPARLibResampleQuality
{
  BEST_QUALITY = 0,
  MEDIUM_QUALITY = 1,
  FASTEST = 2,
  ZERO_ORDER_HOLD = 2,
  LINEAR = 4
};

enum MPARUseFilters
{
  USE_FILTERS_WASAPI = 0,
  USE_FILTERS_AC3ENCODER = 1,
  USE_FILTERS_BIT_DEPTH_IN = 2,
  USE_FILTERS_BIT_DEPTH_OUT = 4,
  USE_FILTERS_TIME_STRETCH = 8,
  USE_FILTERS_SAMPLE_RATE_CONVERTER = 16,
  USE_FILTERS_CHANNEL_MIXER = 32,
  USE_FILTERS_COMPACT = USE_FILTERS_WASAPI | USE_FILTERS_CHANNEL_MIXER | USE_FILTERS_BIT_DEPTH_OUT | USE_FILTERS_BIT_DEPTH_IN | USE_FILTERS_SAMPLE_RATE_CONVERTER,
  USE_FILTERS_MID = USE_FILTERS_WASAPI | USE_FILTERS_BIT_DEPTH_IN |USE_FILTERS_BIT_DEPTH_OUT | USE_FILTERS_TIME_STRETCH | USE_FILTERS_SAMPLE_RATE_CONVERTER | USE_FILTERS_CHANNEL_MIXER,
  USE_FILTERS_ALL = USE_FILTERS_WASAPI | USE_FILTERS_AC3ENCODER | USE_FILTERS_BIT_DEPTH_IN | USE_FILTERS_BIT_DEPTH_OUT | USE_FILTERS_TIME_STRETCH | USE_FILTERS_SAMPLE_RATE_CONVERTER | USE_FILTERS_CHANNEL_MIXER
};

// {CA0CDCD8-D26B-4F8F-B23C-D8D949B14297}
static const GUID IID_IMPAudioSettings = 
{ 0xca0cdcd8, 0xd26b, 0x4f8f, { 0xb2, 0x3c, 0xd8, 0xd9, 0x49, 0xb1, 0x42, 0x97 } };

DEFINE_GUID(CLSID_IMPAudioRendererConfig, 
  0xca0cdcd8, 0xd26b, 0x4f8f, 0xb2, 0x3c, 0xd8, 0xd9, 0x49, 0xb1, 0x42, 0x97);

MIDL_INTERFACE("CA0CDCD8-D26B-4F8F-B23C-D8D949B14297")
IMPAudioRendererConfig: public IUnknown
{
  /* 
    Failure codes for IMPAudioSettings methods.

    E_NOTIMPL when the accessed setting is not available
    E_INVALIDARG when the passed parameter is invalid
  */

  virtual HRESULT STDMETHODCALLTYPE GetBool(MPARSetting setting, bool* pValue) = 0;
  virtual HRESULT STDMETHODCALLTYPE SetBool(MPARSetting setting, bool value) = 0;

  virtual HRESULT STDMETHODCALLTYPE GetInt(MPARSetting setting, int* pValue) = 0;
  virtual HRESULT STDMETHODCALLTYPE SetInt(MPARSetting setting, int value) = 0;

  virtual HRESULT STDMETHODCALLTYPE GetString(MPARSetting setting, LPWSTR* ppValue) = 0;
  virtual HRESULT STDMETHODCALLTYPE SetString(MPARSetting setting, LPWSTR pValue) = 0;
};