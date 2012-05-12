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

#include "ClockAdjuster.h"
#include "AudioClockTracker.h"

// TODO use enum?
#define DIRUP 1 // speeding up the clock as calculated bias is too high
#define DIRDOWN -1 // slowing downthe clock as calculated bias is too low
#define ALLOWED_DRIFT 10000.0 //allow drift to go to 1 ms before correction
#define CORRECTION_RATE 1.005 //apply this extra correction when the ALLOWED_DRIFT is breached 0.5%
#define QUALITY_DRIFT_LIMIT 80000.0 //drift allowed before correction
#define QUALITY_CORRECTION_LIMIT 60000.0 //correction limit
#define QUALITY_CORRECTION_MULTIPLIER 1.02 //correction rate
#define QUALITY_BIAS_LIMIT 0.01 // if the bias is within this of one we use quality mode

class SynchCorrection
{
public:
  SynchCorrection();
  ~SynchCorrection();
  
  // Call reset when a discontinuity happens in the audio stream (drifting resets to zero etc)
  void Reset();
  void Reset(double dBias);
  
  // Suggested adjustment - this can be ignored if you want
  double SuggestedAudioMultiplier(double sampleLength, double bias, double adjustment);
  // call after resampling to indicate what was resampled
  void AudioResampled(double sourceLength, double resampleLength,double bias, double adjustment, double driftFactor);
  // estimate of the current drift
  double GetCurrentDrift();

  // The current AV Drift (mult) value
  // This is the difference in hardware clocks
  void SetAVMult(double mult);
  double GetAVMult() const;

  // Used for the adjustment - it also corrects bias
  void SetAdjustment(double adjustment);
  double GetAdjustment();

  void SetBias(double bias);
  double GetBias();

  // gets and sets the audio delay in 10 shake units
  void SetAudioDelay(INT64 delay);
  INT64 GetAudioDelay() const;

  // gets and sets the audio delay required by the EVR presenter in 10 shake units
  void SetPresenterInducedAudioDelay(INT64 delay);
  INT64 GetPresenterInducedAudioDelay() const;

  // Recalculation of the delta value for the reference clock
  INT64 GetCorrectedTimeDelta(INT64 time);
  // This is used for degugging
  char* DebugData();

private:
  double GetRequiredAdjustment(double sampleTime, double AVMult, double bias, double adjustment);
  double TotalAudioDrift(double AVMult);

  double m_dBiasCorrection;
  double m_dlastAdjustment;

  double m_dAudioDelay;
  double m_dEVRAudioDelay;
  double m_dBiasAdjustmentDelay;

  int m_iBiasDir;
  double m_dAVmult;
  INT64 m_ullTotalTime;
  ClockAdjuster m_Bias;
  ClockAdjuster m_Adjustment;
  AudioClockTracker m_AVTracker;
  char* m_pDebugLine;
  bool m_bDriftCorrectionEnabled;
  bool m_bBiasCorrectionEnabled;
  bool m_bAdjustmentCorrectionEnabled;

  bool m_bQualityMode;
  bool m_bQualityCorrectionOn;
  int m_iQualityDir;

};
