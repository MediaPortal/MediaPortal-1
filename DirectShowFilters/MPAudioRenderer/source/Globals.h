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

#define MAX_SAMPLE_TIME_ERROR 20000 // 2 ms

typedef struct tagSpeakerPair {
  DWORD dwLeft, dwRight;
  __inline DWORD PairMask()  { return dwLeft | dwRight; };
} SpeakerPair;

extern SpeakerPair PairedSpeakers[];
extern DWORD gdwDefaultChannelMask[];
extern DWORD gdwAC3SpeakerOrder[];

#define cAC3SpeakerOrder  (sizeof(gdwAC3SpeakerOrder)/sizeof(DWORD))
#define SPEAKER_AC3_VALID_POSITIONS ( \
  SPEAKER_FRONT_LEFT | \
  SPEAKER_FRONT_CENTER | \
  SPEAKER_FRONT_RIGHT | \
  SPEAKER_BACK_LEFT | \
  SPEAKER_BACK_RIGHT | \
  SPEAKER_LOW_FREQUENCY )

#define IS_WAVEFORMATEXTENSIBLE(pwfx)   (pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE && \
                                         pwfx->cbSize >= sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX))

#define IS_WAVEFORMAT_FLOAT(pwfx)       (pwfx->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)

extern void Log(const char *fmt, ...);
extern void LogWaveFormat(const WAVEFORMATEXTENSIBLE* pwfx, const char *text);

extern HRESULT CopyWaveFormatEx(WAVEFORMATEXTENSIBLE** dst, const WAVEFORMATEXTENSIBLE* src);
extern HRESULT ToWaveFormatExtensible(WAVEFORMATEXTENSIBLE** dst, WAVEFORMATEX* src);

extern void SetThreadName(DWORD dwThreadID, char* threadName);

#define MPAR_S_THREAD_STOPPING        ((HRESULT)0x00040200)
#define MPAR_S_OOB_COMMAND_AVAILABLE  ((HRESULT)0x00040201)

#define MPAR_S_NEED_DATA              ((HRESULT)0x00040301)
#define MPAR_S_DROP_SAMPLE            ((HRESULT)0x00040302)
#define MPAR_S_RENDER_SAMPLE          ((HRESULT)0x00040303)
#define MPAR_S_WAIT_RENDER_TIME       ((HRESULT)0x00040304)

#define MPAR_S_WAIT_TIMED_OUT         ((HRESULT)0x00040400)