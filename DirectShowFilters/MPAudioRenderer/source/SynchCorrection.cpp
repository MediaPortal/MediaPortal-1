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

#include "SynchCorrection.h"

#include "alloctracing.h"

SynchCorrection::SynchCorrection(AudioRendererSettings* pSettings, Logger* pLogger) :
  m_pSettings(pSettings),
  m_dAudioDelay(0.0), // audio delay is not reset on seek / pause
  m_pLogger(pLogger)
{
  Reset(false);
}

SynchCorrection::~SynchCorrection()
{
  Flush();
}

void SynchCorrection::Flush()
{
  CAutoLock lock(&m_csSampleQueueLock);

  Log("SynchCorrection::Flush");

  while (m_qSampleTimes.size() > 0)
  {
    SampleTimeData* oldSample = m_qSampleTimes.front();
    m_qSampleTimes.pop();
    delete oldSample;
  }

  m_rtQueueDuration = 0;
  m_rtQueueAdjustedDuration = 0;
}

void SynchCorrection::Reset(bool soft)
{
  Log("SynchCorrection::Reset %u", soft);
  m_dBiasCorrection = 0.0001;
  m_iBiasDir = 0;
  //m_dlastAdjustment = 1.0;
  
  if (!soft)
  {
    CAutoLock lock(&m_csSampleQueueLock);
    m_rtStart = 0;
    m_dEVRAudioDelay = 0.0;
    m_dlastAdjustment = 1.0;
    m_iQualityDir = 0;
    m_bQualityCorrectionOn = false;
    m_rtAHwStartSet = false;
    m_rtAHwStart = 0;
    m_dHwTimeLast = 0.0;
    m_dAudioDriftOffsetI = 0.0;
    m_dCurrentAdjustment = 1.0;
    m_dCurrentAdjustmentAvg = 1.0;
    m_dPhaseDiff = 0.0;
    m_dPhaseDiffAvg = 0.0;
    m_dPhaseBiasOffsetP = 0.0;
	m_dPhaseBiasOffsetI = 0.0;
    m_bMaintainSoundPitch = false;

	m_dAudioDriftAvg = 0.0;
	m_dAudioDriftLast = 0.0;
	m_iAudioDriftChangeCnt = 0;

    {
      CAutoLock dLock(&m_csDeltaLock);
      m_dDeltaError = 0.0;
    }

    SetBias(1.0);
    SetAdjustment(1.0);
    Flush();
  }
}

void SynchCorrection::Reset(double dBias)
{
  CAutoLock lock(&m_csBiasLock);
  Log("SynchCorrection::Reset with bias");
  //Reset();
  m_Bias = dBias;
  m_dPhaseBiasOffsetP = 0.0;
  m_dPhaseBiasOffsetI = 0.0;
}

void SynchCorrection::Reset(double dBias, REFERENCE_TIME tStart)
{
  Log("SynchCorrection::Reset with rtStart");
  //Reset();
  m_rtStart = tStart;
  m_rtAHwStartSet = false;
  m_rtAHwStart = 0;
  Reset(dBias);
}

double SynchCorrection::SuggestedAudioMultiplier(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime, double bias, double adjustment)
{
  return GetRequiredAdjustment(rtAHwTime,  rtRCTime, bias, adjustment);
}

double SynchCorrection::GetCurrentDrift(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime)
{
  return TotalAudioDrift(rtAHwTime, rtRCTime);
}

/// <summary>
/// Callback from EVR reporting current NST(next sample time) offset deviance
/// </summary>
/// <param name="dDiff">Current value</param>
/// <param name="dDiffAvg">Average value</param>
void SynchCorrection::SetCurrentPhaseDifference(double dDiff, double dDiffAvg)
{
	double dGain;
	if (!m_pSettings->GetMaintainSoundPitch() && m_pSettings->GetEnableSyncAdjustment())
	{
		CAutoLock lock(&m_csAdjustmentLock);

		//Nonlinear gain
		dGain = fabs(dDiffAvg) * 0.05 + 0.005;
		if (dGain < 0.01)
			dGain = 0.01;
		else if(dGain > 0.03)
			dGain = 0.03;

		m_dPhaseDiff = dDiff;
		m_dPhaseDiffAvg = round(dDiffAvg * 1000.0) / 1000.0; //round to 3 decimal places
		m_dPhaseBiasOffsetP = m_dPhaseDiffAvg * dGain; //0.01;  //Calculate bias offset for vsync adjustement (P component)
		//Log("SetCurrentPhaseDifference: %10.8f %10.8f", dDiffAvg, m_dPhaseDiffAvg);
	}
}

// used for the adjustment - it also corrects bias
void SynchCorrection::SetAdjustment(double adjustment)
{
  CAutoLock lock(&m_csAdjustmentLock);

  if (adjustment != m_dlastAdjustment)
  {
    m_dlastAdjustment = adjustment;
    if (m_iBiasDir != 0) // there has been an adjustment already
    {
      // if the direction is different we have overshot so half the correction
      if ((m_iBiasDir == DIRUP) && (adjustment < 1))
        m_dBiasCorrection /= 2;
      else if ((m_iBiasDir == DIRDOWN) && (adjustment > 1))
        m_dBiasCorrection /= 2;
    }

    if (adjustment > 1)
      m_iBiasDir = DIRUP;
    else if (adjustment < 1)
      m_iBiasDir = DIRDOWN;

    CAutoLock biasLock(&m_csBiasLock);
    m_Bias += m_dBiasCorrection * (double) m_iBiasDir;
  }

  m_Adjustment = adjustment;
}

double SynchCorrection::GetAdjustment()
{
  CAutoLock lock(&m_csAdjustmentLock);
  return m_Adjustment;
}

void SynchCorrection::SetBias(double bias)
{
  // handle intrastream bias change
  //double currentDrift = TotalAudioDrift(m_dAVmult);
  Reset(true);
  CAutoLock lock(&m_csBiasLock);
  m_Bias = bias;
  m_dPhaseBiasOffsetP = 0.0;
  m_dPhaseBiasOffsetI = 0.0;
}

double SynchCorrection::GetBias()
{
    CAutoLock lock(&m_csBiasLock);

    return m_Bias - m_dPhaseBiasOffsetP - m_dPhaseBiasOffsetI;
}

void SynchCorrection::SetAudioDelay(INT64 delay)
{
  m_dAudioDelay = (double)delay;
}

INT64 SynchCorrection::GetAudioDelay()
{
  return (INT64)m_dAudioDelay;
}

//EVR presenter requests a delay
void SynchCorrection::SetPresenterInducedAudioDelay(INT64 delay)
{
  m_dEVRAudioDelay = (double)delay;
}

INT64 SynchCorrection::GetPresenterInducedAudioDelay()
{
  return (INT64)m_dEVRAudioDelay;
}

// recalculation of the delta value for the reference clock
INT64 SynchCorrection::GetCorrectedTimeDelta(INT64 time, REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime)
{
  double deltaTime = 0;
  {
    CAutoLock lock(&m_csDeltaLock);
    deltaTime = time * GetAdjustment() * GetBias() + m_dDeltaError;
    m_dDeltaError = deltaTime - floor(deltaTime);
  }

  return (INT64) deltaTime;
}

// get the current drift
double SynchCorrection::TotalAudioDrift(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime)
{
  return CalculateDrift(rtAHwTime, rtRCTime - m_rtStart) + m_dAudioDelay + m_dEVRAudioDelay;
}

bool SynchCorrection::GetMaintainSoundPitch()
{
    return m_pSettings->GetMaintainSoundPitch() || m_bMaintainSoundPitch;
}

// get the adjustment required to match the hardware clocks
double SynchCorrection::GetRequiredAdjustment(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime, double bias, double adjustment)
{
	double ret;
	double dTotalAudioDrift;
	bool bQuality;
	double dHwTime, dHwTimeDelta, dPhaseDiff, dDiffP, dDiff, dGain;

	if (!m_pSettings->GetMaintainSoundPitch())
	{
		// RATE mode: sound pitch is not maintained
		// To avoid large change in speed(sound pitch), we use simple PI regulator to keep the target speed almost constant

		ret = 1.0;

		//Calculate deltaT
		dHwTime = rtAHwTime / 10000000.0;
		dHwTimeDelta = dHwTime - m_dHwTimeLast; //deltaT
		if (dHwTimeDelta <= 0.002)
			return m_dCurrentAdjustment;

		//Get current audio drift
		dTotalAudioDrift = CalculateDrift(rtAHwTime, rtRCTime - m_rtStart) + m_dAudioDelay + m_dEVRAudioDelay;

		//Remove spurious change in audio drift
		if (fabs(m_dAudioDriftLast - dTotalAudioDrift) > 150.0)
		{
			if (m_iAudioDriftChangeCnt == 0)
			{
				//New change
				m_dAudioDriftNew = dTotalAudioDrift;
				dTotalAudioDrift = m_dAudioDriftLast;
				m_iAudioDriftChangeCnt++;
			}
			else if (m_iAudioDriftChangeCnt > 0)
			{
				//Accept change
				m_iAudioDriftChangeCnt = 0;
				m_dAudioDriftLast = dTotalAudioDrift;
			}
		}
		else
		{
			if (m_iAudioDriftChangeCnt > 0 && m_pSettings->GetLogDebug())
				Log("SynchCorrection::GetRequiredAdjustment: Removed spurious AudioDrift change: current:%3.2f removed:%3.2f",
					dTotalAudioDrift, m_dAudioDriftNew);

			m_dAudioDriftLast = dTotalAudioDrift;
			m_iAudioDriftChangeCnt = 0;
		}
			
		//Filter: audio drift
		dDiff = dTotalAudioDrift - m_dAudioDriftAvg;
		dGain = fabs(dDiff) * 20 + 0.05;
		
		if (dGain < 0.25)
			dGain = 0.25;
		else if (dGain > 1.00)
			dGain = 1.00;

		dTotalAudioDrift = m_dAudioDriftAvg + (dDiff * dGain);
		m_dAudioDriftAvg = dTotalAudioDrift;
		

		//Log("SynchCorrection::GetRequiredAdjustment: drift:%10.8f rtAHwTime:%10.8f rtRCTime:%10.8f m_rtStart:%10.8f, bias:%10.8f adj:%10.8f",
		//	dTotalAudioDrift / 10000.0, rtAHwTime / 10000.0, rtRCTime / 10000.0, m_rtStart / 10000.0, bias, m_dCurrentAdjustment);

		//Fast correction
		if (m_iQualityDir != 0) //fast correction active
		{
			if (((m_iQualityDir == DIRUP) && (dTotalAudioDrift > QUALITY_CORRECTION_LIMIT)) ||
				((m_iQualityDir == DIRDOWN) && (dTotalAudioDrift < QUALITY_CORRECTION_LIMIT * -1.0)))
			{
				m_iQualityDir = 0;
			}
		}
		else
		{
			//check if we need fast correction

			if (dTotalAudioDrift > QUALITY_DRIFT_LIMIT)
				m_iQualityDir = DIRDOWN;
			else if (dTotalAudioDrift < QUALITY_DRIFT_LIMIT * -1.0)
				m_iQualityDir = DIRUP;
		}

		if (m_iQualityDir != 0)
		{
			//Fast correction active

			if (m_iQualityDir == DIRUP) //behind so stretch
				ret = QUALITY_CORRECTION_MULTIPLIER;
			else if (m_iQualityDir == DIRDOWN) // in front so slow
				ret = 1.0 / QUALITY_CORRECTION_MULTIPLIER;

			//Reset in case of fast correction
			m_bMaintainSoundPitch = true; //use TEMPO to keep sound pitch
			m_dAudioDriftOffsetI = 0.0;
			m_dCurrentAdjustment = ret;
			m_dHwTimeLast = dHwTime;
		}
		else
		{
			//I component
			if (dHwTimeDelta < 1.0)
			{
				if (dHwTimeDelta >= 0.100)
				{
					//We calculate the I component in period of 100ms: add 0.001% speed change per 1ms drift
					m_dAudioDriftOffsetI += dTotalAudioDrift / 10000.0 * 0.0001 * dHwTimeDelta;
					m_dHwTimeLast = dHwTime;

					//if (m_pSettings->GetLogDebug())
					//	Log("SynchCorrection::GetRequiredAdjustment: AudioDriftOffset:%10.8f HwTime:%10.8f", m_dAudioDriftOffsetI, dHwTime);

					//Calculate bias offset for vsync adjustement (I component)
					if (m_pSettings->GetEnableSyncAdjustment())
					{
						{
							CAutoLock lock(&m_csAdjustmentLock);
							dPhaseDiff = m_dPhaseDiffAvg;
						}
						{
							CAutoLock lock(&m_csBiasLock);
							m_dPhaseBiasOffsetI += dPhaseDiff * 0.001 * dHwTimeDelta;
						}
					}
				}
			}
			else //deltaT is too big; reset
			{
				if (m_pSettings->GetLogDebug())
					Log("SynchCorrection::GetRequiredAdjustment: HwTimeLast:%10.8f HwTime:%10.8f", m_dHwTimeLast, dHwTime);

				//reset
				m_dHwTimeLast = dHwTime;

				if (dTotalAudioDrift >= QUALITY_CORRECTION_LIMIT)
				{
					m_dAudioDriftOffsetI = 0.0;
					m_dCurrentAdjustment = 1.0;
					m_dCurrentAdjustmentAvg = 1.0;
				}
			}

			//Output: I component
			ret -= m_dAudioDriftOffsetI;

			//Output: P component: 0.5% speed change per 6ms audio drift
			dDiffP = dTotalAudioDrift / QUALITY_CORRECTION_LIMIT * 0.005;
			ret -= dDiffP;

			//Fast correction disable: we take P componnent as reference
			if (m_bMaintainSoundPitch)
			{
				if (fabs(dDiffP) < 0.005) //diff is back below 0.5%
					m_bMaintainSoundPitch = false;
			}

			//Output: Limit to max. 2% speed change
			if (ret >= 1.02)
				ret = 1.02;
			else if (ret <= 0.98)
				ret = 0.98;

			//Output: Filter
			dDiff = ret - m_dCurrentAdjustmentAvg;
			dGain = fabs(dDiff) * 5000;

			if (dGain < 0.25)
				dGain = 0.25;
			else if (dGain > 1.00)
				dGain = 1.00;

			ret = m_dCurrentAdjustmentAvg + (dDiff * dGain);
			m_dCurrentAdjustmentAvg = ret;


			//Output: Dead zone
			if (fabs(ret - m_dCurrentAdjustment) > 0.0000005)
				m_dCurrentAdjustment = ret;
			else
				ret = m_dCurrentAdjustment;
		}

		if (m_pSettings->GetLogDebug())
			Log("SynchCorrection::GetRequiredAdjustment: drift:%10.8f bias:%10.8f adj:%10.8f adjAvg:%10.8f  adjOffsI:%10.8f biasOffsP:%10.8f biasOffsI:%10.8f",
			dTotalAudioDrift / 10000.0, bias, ret, m_dCurrentAdjustmentAvg, m_dAudioDriftOffsetI, m_dPhaseBiasOffsetP, m_dPhaseBiasOffsetI);
	}
	else
	{
		ret = bias * adjustment;
		dTotalAudioDrift = CalculateDrift(rtAHwTime, rtRCTime - m_rtStart) + m_dAudioDelay + m_dEVRAudioDelay;
		bQuality = bias > 1.0 - QUALITY_BIAS_LIMIT && bias < 1.0 + QUALITY_BIAS_LIMIT;

		if (bQuality)
		{
			ret = 1.0; // 1 to 1 playback unless proved otherwise
			if (m_bQualityCorrectionOn) // we are correcting drift
			{
				if (((m_iQualityDir == DIRUP) && (dTotalAudioDrift > QUALITY_CORRECTION_LIMIT)) ||
					((m_iQualityDir == DIRDOWN) && (dTotalAudioDrift < QUALITY_CORRECTION_LIMIT * -1.0)))
				{
					//we've corrected enough
					m_bQualityCorrectionOn = false;
					m_iQualityDir = 0;
				}
				if (m_iQualityDir == DIRUP) //behind so stretch
					ret = QUALITY_CORRECTION_MULTIPLIER;
				else if (m_iQualityDir == DIRDOWN) // in front so slow
					ret = 1.0 / QUALITY_CORRECTION_MULTIPLIER;
			}
			else // not correcting now so check for breach
			{
				if (dTotalAudioDrift > QUALITY_DRIFT_LIMIT)
				{
					m_bQualityCorrectionOn = true;
					m_iQualityDir = DIRDOWN;
				}
				else if (dTotalAudioDrift < QUALITY_DRIFT_LIMIT * -1.0)
				{
					m_bQualityCorrectionOn = true;
					m_iQualityDir = DIRUP;
				}

			}
		}
		else if (dTotalAudioDrift > ALLOWED_DRIFT && (!bQuality || bias < 1.0))
		{ // we've stretched too much shift down for a while
			double msDrift = dTotalAudioDrift / 10000.0;
			double quickCorrection = 1.0;

			if (msDrift > 10.0)
				quickCorrection = log(msDrift);
			else
				quickCorrection = msDrift / 10.0;

			if (quickCorrection > 5.0) quickCorrection = 5.0;
			ret = ret * (1.0 / (1 + (CORRECTION_RATE - 1) * quickCorrection));
		}
		else if (dTotalAudioDrift < ALLOWED_DRIFT * -1.0 && (!bQuality || bias > 1.0))
		{ // haven't streched enough
			double msDrift = dTotalAudioDrift / -10000.0;
			double quickCorrection = 1.0;

			if (msDrift > 10.0)
				quickCorrection = log(msDrift);
			else
				quickCorrection = msDrift / 10.0;

			if (quickCorrection > 5.0)
				quickCorrection = 5.0;

			ret = ret * (1 + (CORRECTION_RATE - 1) * quickCorrection);
		}
	}

	return ret;
}

void SynchCorrection::AddSample(INT64 rtOriginalStart, INT64 rtAdjustedStart, INT64 rtOriginalEnd, INT64 rtAdjustedEnd)
{
  SampleTimeData* newSample = new SampleTimeData;
  newSample->rtOriginalSampleStart = rtOriginalStart;
  newSample->rtAdjustedSampleStart = rtAdjustedStart;
  newSample->rtOriginalSampleEnd = rtOriginalEnd;
  newSample->rtAdjustedSampleEnd = rtAdjustedEnd;

  if (m_pSettings->GetLogDebug())
    Log ("SynchCorrection::AddSample Size: %4u rtOriginalStart: %10.8f rtAdjustedStart: %10.8f rtOriginalEnd: %10.8f rtAdjustedEnd: %10.8f m_rtQueueDuration: %10.8f  m_rtAdjustedQueueDuration: %10.8f",
      m_qSampleTimes.size(),rtOriginalStart / 10000000.0, rtAdjustedStart / 10000000.0, rtOriginalEnd / 10000000.0, rtAdjustedEnd / 10000000.0, m_rtQueueDuration / 10000000.0, m_rtQueueAdjustedDuration/ 10000000.0);

  m_rtQueueDuration += newSample->rtOriginalSampleEnd - newSample->rtOriginalSampleStart;
  m_rtQueueAdjustedDuration += newSample->rtAdjustedSampleEnd - newSample->rtAdjustedSampleStart;

  CAutoLock lock(&m_csSampleQueueLock);
  m_qSampleTimes.push(newSample);
}

REFERENCE_TIME SynchCorrection::GetReferenceTimeFromAudioSamples(REFERENCE_TIME rtAHwtime)
{
  CAutoLock lock(&m_csSampleQueueLock);

  REFERENCE_TIME ret = rtAHwtime;
  SampleTimeData* sampleTime = GetMatchingSampleForTime(rtAHwtime);
  if (sampleTime)
  {
    INT64 duration = sampleTime->rtOriginalSampleEnd - sampleTime->rtOriginalSampleStart;
    INT64 adjustedDuration = sampleTime->rtAdjustedSampleEnd - sampleTime->rtAdjustedSampleStart;
    ret = sampleTime->rtOriginalSampleStart + ((rtAHwtime - sampleTime->rtAdjustedSampleStart) * duration) / adjustedDuration;
  }

  return ret;
}

SampleTimeData* SynchCorrection::GetMatchingSampleForTime(REFERENCE_TIME rtAHwtime)
{
  SampleTimeData* ret = NULL;

  while (m_qSampleTimes.size() > 1 && m_qSampleTimes.front()->rtAdjustedSampleEnd < rtAHwtime)
  {
    SampleTimeData* oldSample = m_qSampleTimes.front();
    m_qSampleTimes.pop();
    m_rtQueueDuration -= oldSample->rtOriginalSampleEnd - oldSample->rtOriginalSampleStart;
    m_rtQueueAdjustedDuration -= oldSample->rtAdjustedSampleEnd - oldSample->rtAdjustedSampleStart;
    delete oldSample;
  }

  if (m_qSampleTimes.size() == 1 && m_qSampleTimes.front()->rtOriginalSampleEnd < rtAHwtime)
    Log("SynchCorrection::GetMatchingSampleForTime: Not Enough Data");
  
  if (m_qSampleTimes.size() > 0)
    ret = m_qSampleTimes.front();

  return ret;
}

INT64 SynchCorrection::CalculateDrift(REFERENCE_TIME rtAHwTime, REFERENCE_TIME rtRCTime)
{
  rtAHwTime -= m_rtAHwStart;

  if (m_pSettings->GetLogDebug())
    Log ("SynchCorrection::CalculateDrift Size: %4u rtAHwTime: %10.8f rtRCTime: %10.8f m_rtStart: %10.8f",
      m_qSampleTimes.size(),rtAHwTime / 10000000.0, rtRCTime / 10000000.0, m_rtStart / 10000000.0);

  if (rtRCTime < 0)
    return 0;

  double bias = GetBias();
  if (!m_rtAHwStartSet)
  {
    m_rtAHwStart = rtAHwTime - rtRCTime / bias;
    m_rtAHwStartSet = true;
    rtAHwTime -= m_rtAHwStart;
  }

  REFERENCE_TIME preCalculatedTime = GetReferenceTimeFromAudioSamples(rtAHwTime);
  return preCalculatedTime - rtRCTime + (m_rtQueueDuration - m_rtQueueAdjustedDuration * bias);
}
