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

class AudioClockTracker
{
public:

  AudioClockTracker();
  ~AudioClockTracker();
  void Reset();

  // informs code that a resample has been done
  void ResampleComplete(double sampleTime, double resampleTime, double bias, double adjustment, double AVMult);
  // debug information on amount processed (in non-corrected time)
  INT64 GetAudioProcessed() const;
  // debug information on amount processed through resampler
  INT64 GetAudioResampled() const;
  // the current uncorrected drift
  double GetCurrentDrift(double AVMult) const;

protected:

  double m_dAudioProcessedError;
  double m_dUndriftedAudioProcessed;
  double m_dDriftedAudioProcessed;
  double m_dAudioResampledError;
  INT64 m_llAudioProcessed;
  INT64 m_llAudioResampled;
};
