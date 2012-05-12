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
#include "Globals.h"

#include "alloctracing.h"

// List of speakers that should be handled as a pair
SpeakerPair PairedSpeakers[] = {
  {SPEAKER_FRONT_LEFT, SPEAKER_FRONT_RIGHT},
  {SPEAKER_BACK_LEFT, SPEAKER_BACK_RIGHT},
  {SPEAKER_FRONT_LEFT_OF_CENTER, SPEAKER_FRONT_RIGHT_OF_CENTER},
  {SPEAKER_FRONT_CENTER, SPEAKER_BACK_CENTER}, // not sure about this one
  {SPEAKER_SIDE_LEFT, SPEAKER_SIDE_RIGHT},
  {SPEAKER_TOP_FRONT_LEFT, SPEAKER_TOP_FRONT_RIGHT},
  {SPEAKER_TOP_BACK_LEFT, SPEAKER_TOP_BACK_RIGHT},
  {SPEAKER_TOP_FRONT_CENTER, SPEAKER_TOP_BACK_CENTER},  // not sure about this one
  {NULL, NULL} // end marker
};

DWORD gdwDefaultChannelMask[] = {
  0, // no channels - invalid
  KSAUDIO_SPEAKER_MONO,
  KSAUDIO_SPEAKER_STEREO,
  KSAUDIO_SPEAKER_STEREO | KSAUDIO_SPEAKER_GROUND_FRONT_CENTER,
  KSAUDIO_SPEAKER_QUAD,
  0, // 5 channels?
  KSAUDIO_SPEAKER_5POINT1,
  0, // 7 channels?
  KSAUDIO_SPEAKER_7POINT1_SURROUND
};

DWORD gdwAC3SpeakerOrder[] = {
  SPEAKER_FRONT_LEFT, 
  SPEAKER_FRONT_CENTER,
  SPEAKER_FRONT_RIGHT,
  SPEAKER_BACK_LEFT, 
  SPEAKER_BACK_RIGHT,
  SPEAKER_LOW_FREQUENCY,
};
