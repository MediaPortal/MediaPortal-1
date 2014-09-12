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
#include "Settings.h"

#include <queue>

// TODO use enum?
#define DIRUP 1 // speeding up the clock as calculated bias is too high
#define DIRDOWN -1 // slowing downthe clock as calculated bias is too low
#define ALLOWED_DRIFT 1000.0 //allow drift to go to 0.1 ms before correction
#define IGNORED_DRIFT 20000.0 //allow drift to go to 0.1 ms before correction
#define CORRECTION_RATE 1.005  //apply this extra correction when the ALLOWED_DRIFT is breached 0.1%
#define QUALITY_DRIFT_LIMIT 180000.0 //drift allowed before correction
#define QUALITY_CORRECTION_LIMIT 60000.0 //correction limit
#define QUALITY_CORRECTION_MULTIPLIER 1.02 //correction rate
#define QUALITY_BIAS_LIMIT 0.01 // if the bias is within this of one we use quality mode

typedef struct stSampleTimeData 
{
  INT64 rtOriginalSampleStart;
  INT64 rtAdjustedSampleStart;
  INT64 rtOriginalSampleEnd;
  INT64 rtAdjustedSampleEnd;
} SampleTimeData;

class SynchCorrection
{
public:
  SynchCorrection(AudioRendererSettings* pSettings);
  ~SynchCorrection();

  void Flush();
  // Call reset when a discontinuity happens in the audio stream (drifting resets to zero etc)
  void Reset(bool soft);
  void Reset(double dBias);
  void Reset(double dBias, REFERENCE_TIME tStart);
  
  // Suggested adjustment - this can be ignored if you want
  double SuggestedAudioMultiplier(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime, double bias, double adjustment);
  // estimate of the current drift
  double GetCurrentDrift(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime);

  // Used for the adjustment - it also corrects bias
  void SetAdjustment(double adjustment);
  double GetAdjustment();

  void SetBias(double bias);
  double GetBias();

  // gets and sets the audio delay in 10 shake units
  void SetAudioDelay(INT64 delay);
  INT64 GetAudioDelay();

  // gets and sets the audio delay required by the EVR presenter in 10 shake units
  void SetPresenterInducedAudioDelay(INT64 delay);
  INT64 GetPresenterInducedAudioDelay();

  // Recalculation of the delta value for the reference clock
  INT64 GetCorrectedTimeDelta(INT64 time, REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime);

  UINT64 CalculateReferenceClock(UINT64 rtCurrentPlaybackTime, UINT64 rtPlaybackQpc, UINT64 rtCurrentQpc, UINT64 delta, UINT64 lastRefernceTime, UINT64 rtHwStart);

  void AddSample(INT64 rtOriginalStart, INT64 rtAdjustedStart, INT64 rtOriginalEnd, INT64 rtAdjustedEnd);

  REFERENCE_TIME GetReferenceTimeFromAudioSamples(REFERENCE_TIME rtAHwtime);

  INT64 CalculateDrift(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime);

private:
  double GetRequiredAdjustment(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime, double bias, double adjustment);
  double TotalAudioDrift(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime);

  double m_dBiasCorrection;
  double m_dlastAdjustment;

  double m_dAudioDelay;
  double m_dEVRAudioDelay;

  int m_iBiasDir;
  double m_Bias;
  double m_Adjustment;
  double m_dDeltaError;

  bool m_bQualityCorrectionOn;
  int m_iQualityDir;

  std::queue<SampleTimeData*> m_qSampleTimes;
  REFERENCE_TIME m_rtQueueDuration;
  REFERENCE_TIME m_rtQueueAdjustedDuration;

  CCritSec m_csSampleQueueLock;
  CCritSec m_csBiasLock;
  CCritSec m_csAdjustmentLock;
  CCritSec m_csDeltaLock;
  
  SampleTimeData * GetMatchingSampleForTime(REFERENCE_TIME time);

  REFERENCE_TIME m_rtStart;
  REFERENCE_TIME m_rtAHwStart;
  
  bool m_rtAHwStartSet;

  AudioRendererSettings* m_pSettings;
};
