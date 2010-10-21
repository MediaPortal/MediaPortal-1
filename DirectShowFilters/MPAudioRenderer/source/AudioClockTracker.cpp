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

#include "AudioClockTracker.h"

AudioClockTracker::AudioClockTracker() :
  m_dAudioProcessedError(0.0),
  m_dAudioResampledError(0.0),
  m_llAudioProcessed(0),
  m_llAudioResampled(0),
  m_dAudioDriftCorrected(0.0),
  m_dAudioDrift(0.0),
  m_dUndriftedAudioProcessed(0.0),
  m_dDriftedAudioProcessed(0.0)
{
}

AudioClockTracker::~AudioClockTracker()
{
}

void AudioClockTracker::ResampleComplete(double sampleTime, double resampleTime, double AVMult)
{
  m_dDriftedAudioProcessed += resampleTime / AVMult;
  m_dDriftedAudioProcessed += resampleTime;
  m_llAudioProcessed += (INT64)sampleTime;
  m_llAudioResampled += (INT64)resampleTime;
  m_dAudioProcessedError += sampleTime - (double)((INT64)sampleTime);
  m_dAudioResampledError += resampleTime - (double)((INT64)resampleTime);
  if (m_dAudioProcessedError > 1.0)
  {
    m_llAudioProcessed++;
    m_dAudioProcessedError -= 1.0;
  }
  if (m_dAudioResampledError > 1.0)
  {
    m_llAudioResampled++;
    m_dAudioResampledError -= 1.0;
  }
}

void AudioClockTracker::DriftCorrected(double correctionAmount)
{
  m_dAudioDriftCorrected += correctionAmount;
}

double AudioClockTracker::getCurrentDrift(double AVMult)
{
  return m_dDriftedAudioProcessed - m_dUndriftedAudioProcessed * AVMult;
}

INT64 AudioClockTracker::getAudioProcessed()
{
  return m_llAudioProcessed;
}
INT64 AudioClockTracker::getAudioResampled()
{
  return m_llAudioResampled;
}
