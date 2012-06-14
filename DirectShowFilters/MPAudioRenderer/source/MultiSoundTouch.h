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

#include <dsound.h>
#include <MMReg.h>  //must be before other Wasapi headers
#include <ks.h>
#include <ksmedia.h>
#include <vector>

#include "../SoundTouch/Include/SoundTouch.h"
#include "../AC3_encoder/ac3enc.h"
#include "SoundTouchEx.h"
#include "SyncClock.h"

typedef signed __int64 int64_t;

class CMultiSoundTouch
{
public:
  CMultiSoundTouch(bool pEnableAC3Encoding, int AC3bitrate, CSyncClock* pClock);
  ~CMultiSoundTouch();

  /// Sets new rate control value. Normal rate = 1.0, smaller values
  /// represent slower rate, larger faster rates.
  void setRate(double newRate);

  /// Sets new tempo control value. Normal tempo = 1.0, smaller values
  /// represent slower tempo, larger faster tempo.
  void setTempo(double newTempo, double newAdjustment);

  /// Sets new rate control value as a difference in percents compared
  /// to the original rate (-50 .. +100 %)
  void setRateChange(double newRate);

  /// Sets new tempo control value as a difference in percents compared
  /// to the original tempo (-50 .. +100 %)
  void setTempoChange(double newTempo);

  /// Sets pitch change in octaves compared to the original pitch  
  /// (-1.00 .. +1.00)
  void setPitchOctaves(double newPitch);

  /// Sets pitch change in semi-tones compared to the original pitch
  /// (-12 .. +12)
  void setPitchSemiTones(int newPitch);
  void setPitchSemiTones(double newPitch);

  /// Sets sample rate.
  void setSampleRate(uint srate);

  /// Flushes the last samples from the processing pipeline to the output.
  /// Clears also the internal processing buffers.
  //
  /// Note: This function is meant for extracting the last samples of a sound
  /// stream. This function may introduce additional blank samples in the end
  /// of the sound stream, and thus it's not recommended to call this function
  /// in the middle of a sound stream.
  void flush();

  /// Clears all the samples.
  void clear();

  /// Returns number of samples currently unprocessed.
  uint numUnprocessedSamples() const;

  /// Returns number of samples currently available.
  uint numSamples() const;

  /// Returns nonzero if there aren't any samples available for outputting.
  int isEmpty() const;

  // set the number of channels to process
  // internally enough SoundTouch processors will be 
  // created to process the requested number of channels
  // Any samples already in que will be lost!
  //void setChannels(int channels);

  // Changes a setting controlling the processing system behaviour. See the
  // 'SETTING_...' defines for available setting ID's.
  // 
  // \return 'TRUE' if the setting was succesfully changed
  BOOL setSetting(int settingId, int value);

  HRESULT CheckFormat(WAVEFORMATEX *pwf);
  HRESULT CheckFormat(WAVEFORMATEXTENSIBLE *pwfe);
  HRESULT SetFormat(WAVEFORMATEX *pwf);
  HRESULT SetFormat(WAVEFORMATEXTENSIBLE *pwfe);

  bool putSamples(const short *inBuffer, long inSamples);
  uint receiveSamples(short **outBuffer, uint maxSamples);

  bool ProcessSamples(const short *inBuffer, long inSamples, short *outBuffer, long *outSamples, long maxOutSamples);
  bool processSample(IMediaSample *pMediaSample);

  HRESULT GetNextSample(IMediaSample** pSample, bool pReleaseOnly);
  HRESULT QueueSample(IMediaSample* pSample);

  void BeginFlush();
  void EndFlush();

  void StopResamplingThread();

  // these needs to be private (pass somehow to the thread...)
  bool putSamplesInternal(const short *inBuffer, long inSamples);
  uint receiveSamplesInternal(short *outBuffer, uint maxSamples);

protected:
  HRESULT ToWaveFormatExtensible(WAVEFORMATEXTENSIBLE *pwfe, WAVEFORMATEX *pwf);
  void setTempoInternal(double newTempo, double newAdjustment);

private:

  static const uint SAMPLE_LEN = 0x40000;
  std::vector<CSoundTouchEx *> *m_Streams;
  WAVEFORMATEXTENSIBLE *m_pWaveFormat;
  double m_fCurrentTempo;
  double m_fCurrentAdjustment;
  double m_fNewTempo;
  double m_fNewAdjustment;

  INT32 m_nFrameCorr;
  INT32 m_nPrevFrameCorr;

  soundtouch::SAMPLETYPE m_temp[2*SAMPLE_LEN];
  HANDLE m_hThread;

  HANDLE m_hSampleArrivedEvent;
  HANDLE m_hStopThreadEvent;
  HANDLE m_hWaitThreadToExitEvent;
  IMemAllocator *m_pMemAllocator;

  bool m_bFlushSamples;

  std::vector<IMediaSample*> m_sampleQueue;
  std::vector<IMediaSample*> m_sampleOutQueue;
  CCritSec m_sampleQueueLock;
  CCritSec m_sampleOutQueueLock;

  CCritSec m_allocatorLock;

  IMediaSample* m_pPreviousSample;

  // Reference clock, not owned
  CSyncClock* m_pClock;

  // Threading 
  static DWORD WINAPI ResampleThreadEntryPoint(LPVOID lpParameter);
  DWORD ResampleThread();
  bool InitializeAllocator();
  DWORD m_threadId;

  // AC3 Encoding
  HRESULT OpenAC3Encoder(unsigned int bitrate, unsigned int channels, unsigned int sampleRate);
  HRESULT CloseAC3Encoder();
  long CreateAC3Bitstream(void *buf, size_t size, BYTE *pDataOut);

  AC3CodecContext* m_pEncoder;
  bool m_bEnableAC3Encoding;
  int m_dAC3bitrate;
};
