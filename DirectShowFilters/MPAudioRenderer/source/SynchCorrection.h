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

#include "stdafx.h"

#include "ClockAdjuster.h"
#include "AudioClockTracker.h"

// TODO use enum?
#define DIRUP 1 // speeding up the clock as calculated bias is too high
#define DIRDOWN -1 // slowing downthe clock as calculated bias is too low

class SynchCorrection
{
public:
  SynchCorrection(void);
  ~SynchCorrection(void);
  // suggested adjustment - this can be ignored if you want
  // I'll add in some logic to keep it at 1.0 if the material is near 1:1
  // sample length is not currently used - but it could be useful
  double SuggestedAudioMultiplier(INT64 sampleLength);
  // call after resampling to indicate what was resampled
  void AudioResampled(double sourceLength, double resampleLength,double driftFactor);
  // estimate of the current drift
  double GetCurrentDrift();
  // The current AV Drift (mult) value
  // I still don't have a picture of what >1 means so I still might have it wrong
  void SetAVMult(double mult);
  double GetAVMult();
  // used for the adjustment - it also corrects bias
  void SetAdjustment(double adjustment);
  void SetBias(double bias);
  // recalculation of the delta value for the reference clock
  INT64 GetCorrectedTimeDelta(INT64 time);
  double GetAdjustment();
  double GetBias();
  char* DebugData();

private:
  INT64 GetAudioTime();
  double GetRequiredAdjustment(long sampleTime, double biasMultiplier, double AVMult, INT64 biasAdjustment, INT64 adjustmentAdjustment, INT64 totalAudioProcessed, INT64 totalAudioAfterSampling, INT64 totalBaseTime);
  INT64 GetResampledAudioTime();
  double TotalAudioDrift(double AVMult,double biasMultiplier,INT64 biasAdjustment, INT64 adjustmentAdjustment, INT64 totalAudioProcessed, INT64 totalAudioAfterSampling, INT64 totalBaseTime);

  double m_dBiasCorrection;
  double m_dlastAdjustment;
  int m_dBiasDir;
  double m_dAVmult;
  INT64 m_ullTotalTime;
  ClockAdjuster m_Bias;
  ClockAdjuster m_Adjustment;
  AudioClockTracker m_AVTracker;
  char* m_pDebugLine;
  INT64 m_ullClockError;
};
