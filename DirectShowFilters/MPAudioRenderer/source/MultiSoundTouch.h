
#pragma once

#include <dsound.h>
#include <ks.h>
#include <ksmedia.h>
#include <vector>

#include <MMReg.h>  //must be before other Wasapi headers

#include "../SoundTouch/Include/SoundTouch.h"

class CMultiSoundTouch;

class CMultiSoundTouch
{
public:
  CMultiSoundTouch(bool pUseThreads);
  ~CMultiSoundTouch();

  /// Sets new rate control value. Normal rate = 1.0, smaller values
  /// represent slower rate, larger faster rates.
  void setRate(float newRate);

  /// Sets new tempo control value. Normal tempo = 1.0, smaller values
  /// represent slower tempo, larger faster tempo.
  void setTempo(float newTempo);

  /// Sets new rate control value as a difference in percents compared
  /// to the original rate (-50 .. +100 %)
  void setRateChange(float newRate);

  /// Sets new tempo control value as a difference in percents compared
  /// to the original tempo (-50 .. +100 %)
  void setTempoChange(float newTempo);

  /// Sets pitch change in octaves compared to the original pitch  
  /// (-1.00 .. +1.00)
  void setPitchOctaves(float newPitch);

  /// Sets pitch change in semi-tones compared to the original pitch
  /// (-12 .. +12)
  void setPitchSemiTones(int newPitch);
  void setPitchSemiTones(float newPitch);

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
  void setChannels(int channels);

  // Changes a setting controlling the processing system behaviour. See the
  // 'SETTING_...' defines for available setting ID's.
  // 
  // \return 'TRUE' if the setting was succesfully changed
  BOOL setSetting(int settingId, int value);

  bool putSamples(const short *inBuffer, long inSamples);
  uint receiveSamples(short **outBuffer, uint maxSamples);

  bool ProcessSamples(const short *inBuffer, long inSamples, short *outBuffer, long *outSamples, long maxOutSamples);
  bool processSample(IMediaSample *pMediaSample);

  HRESULT GetNextSample(IMediaSample** pSample, bool pReleaseOnly);
  HRESULT QueueSample(IMediaSample* pSample);

  void FlushQueues();

  // these needs to be private (pass somehow to the thread...)
  bool putSamplesInternal(const short *inBuffer, long inSamples);
  uint receiveSamplesInternal(short *outBuffer, uint maxSamples);

private:
  

  static const uint SAMPLE_LEN = 0x10000;
  typedef struct 
  {
    soundtouch::SoundTouch *processor;
    int channels;
  } StreamProcessor;

  int m_nChannels;
  
  int m_nStreamCount;
  StreamProcessor *m_Streams;
  soundtouch::SAMPLETYPE m_temp[2*SAMPLE_LEN];

  // internal functions to separate/merge streams out of sample buffers
  void StereoDeInterleave(const short *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count);
  void StereoInterleave(const soundtouch::SAMPLETYPE *inBuffer, short *outBuffer, uint count);
  void MonoDeInterleave(const short *inBuffer, soundtouch::SAMPLETYPE *outBuffer, uint count);
  void MonoInterleave(const soundtouch::SAMPLETYPE *inBuffer, short *outBuffer, uint count);

  HANDLE m_hThread;

  HANDLE m_hSampleArrivedEvent;
  HANDLE m_hStopThreadEvent;
  HANDLE m_hWaitThreadToExitEvent;
  IMemAllocator *m_pMemAllocator;

  bool m_bUseThreads;
  bool m_bFlushSamples;

  std::vector<IMediaSample*> m_sampleQueue;
  std::vector<IMediaSample*> m_sampleOutQueue;
  CCritSec m_sampleQueueLock;
  CCritSec m_sampleOutQueueLock;

  CCritSec m_allocatorLock;

  IMediaSample* m_pPreviousSample;

  // Threading 
  static DWORD WINAPI ResampleThreadEntryPoint(LPVOID lpParameter);
  DWORD ResampleThread();
  bool InitializeAllocator();
  DWORD m_threadId;
};
