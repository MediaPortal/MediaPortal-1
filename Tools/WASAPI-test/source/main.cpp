// main.cpp

#include <windows.h>
#include <stdio.h>
#include <mmsystem.h>
#include <mmdeviceapi.h>
#include <streams.h>
#include <MMReg.h>  //must be before other Wasapi headers
#include <ks.h>
#include <ksmedia.h>

#include "play.h"
#include "prefs.h"

int do_everything(int argc, LPCWSTR argv[]);
void TestFormats(CPrefs* pPrefs, bool pExclusive, bool pEventDriven);
void TestFormatsAC3(CPrefs* pPrefs, bool pExclusive, bool pEventDriven);
void ToWaveFormatExtensible(WAVEFORMATEXTENSIBLE *pwfe, WAVEFORMATEX *pwf);

int float_bitDepth = 256;

int gAllowedBitDephts[4] = {16, 24, 32, float_bitDepth}; // 256 == float 32
int gAllowedChannels[8] = {1, 2, 3, 4, 5, 6, 7, 8};
int gAllowedSampleRates[7] = {22050, 32000, 44100, 48000, 88200, 96000, 192000};
int gAllowedSampleRatesAC3[3] = {32000, 44100, 48000};

int _cdecl wmain(int argc, LPCWSTR argv[]) 
{
    HRESULT hr = S_OK;

    hr = CoInitialize(NULL);
    if (FAILED(hr)) 
    {
        printf("CoInitialize failed: hr = 0x%08x", hr);
        return __LINE__;
    }

    int result = do_everything(argc, argv);
    
    CoUninitialize();
    return result;
}

int do_everything(int argc, LPCWSTR argv[]) 
{
    HRESULT hr = S_OK;

    // parse command line
    CPrefs prefs(argc, argv, hr);
    if (FAILED(hr)) 
    {
        printf("CPrefs::CPrefs constructor failed: hr = 0x%08x\n", hr);
        return __LINE__;
    }
    if (S_FALSE == hr) 
    {
        return 0;
    }

    printf("\nEXCLUSIVE: off    EVENT DRIVEN: off");
    printf("\n-------------------------------------------------------------------------\n");
    TestFormats(&prefs, false, false);

    printf("\nEXCLUSIVE: on    EVENT DRIVEN: off");
    printf("\n-------------------------------------------------------------------------\n");
    TestFormats(&prefs, true, false);
    
    printf("\nEXCLUSIVE: off    EVENT DRIVEN: on");
    printf("\n-------------------------------------------------------------------------\n");
    TestFormats(&prefs, false, true);

    printf("\nEXCLUSIVE: on     EVENT DRIVEN: on");
    printf("\n-------------------------------------------------------------------------\n");
    TestFormats(&prefs, true, true);

    printf("\nAC3 - EXCLUSIVE: on     EVENT DRIVEN: off");
    printf("\n-------------------------------------------------------------------------\n");
    TestFormatsAC3(&prefs, true, false);

    printf("\nAC3 - EXCLUSIVE: on     EVENT DRIVEN: on");
    printf("\n-------------------------------------------------------------------------\n");
    TestFormatsAC3(&prefs, true, true);

    return 0;
}

void TestFormats(CPrefs* pPrefs, bool pExclusive, bool pEventDriven)
{
  WAVEFORMATEX format;
  WAVEFORMATEXTENSIBLE formatEx;

  int sampleRateCount = sizeof(gAllowedSampleRates) / sizeof(int);
  int channelCount = sizeof(gAllowedChannels) / sizeof(int);
  int bitDepthCount = sizeof(gAllowedBitDephts) / sizeof(int);
    
  for (int sr = 0 ; sr < sampleRateCount ; sr++)
  {
    for (int c = 0 ; c < channelCount ; c++)
    {
      for (int bd = 0 ; bd < bitDepthCount ; bd++)
      {
        for (int i = 0; i < 2 ; i++)
        {
          format.wBitsPerSample = gAllowedBitDephts[bd];
          format.wFormatTag = WAVE_FORMAT_PCM;
          
          // handle float with an ugly way
          if (format.wBitsPerSample == float_bitDepth)
          {
            format.wBitsPerSample = 32;
            format.wFormatTag = 0x3; //WAVE_FORMAT_IEEE_FLOAT
          }

          format.cbSize = 0;
          format.nChannels = gAllowedChannels[c];
          format.nSamplesPerSec = gAllowedSampleRates[sr];
          format.nBlockAlign = format.wBitsPerSample / 8 * format.nChannels;
          format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;
          
          PlayThreadArgs pta = {0};
          pta.pMMDevice = pPrefs->m_pMMDevice;
          pta.pExclusive = pExclusive;
          pta.pEventDriven = pEventDriven;
          pta.hr = E_UNEXPECTED;

          // try WAVEFORMATEXTENSIBLE since some drivers are weird 
          if (i == 1) 
          {
            if (gAllowedBitDephts[bd] == float_bitDepth)
              continue;

            ToWaveFormatExtensible(&formatEx, &format);
            pta.pWfx = (WAVEFORMATEX*)&formatEx;
          }
          else
          {
            pta.pWfx = &format;
          }

          HANDLE hThread = CreateThread(NULL, 0, PlayThreadFunction, &pta, 0, NULL);

          if (NULL == hThread) 
          {
            printf("CreateThread failed: GetLastError = %u\n", GetLastError());
            return;
          }

          WaitForSingleObject(hThread, INFINITE);

          if (FAILED(pta.hr)) 
          {
            //printf("Thread returned failing HRESULT 0x%08x", pta.hr);
            CloseHandle(hThread);
          }
          else
          {
            CloseHandle(hThread);
          }
        }
      }
    }
  }
}

void TestFormatsAC3(CPrefs* pPrefs, bool pExclusive, bool pEventDriven)
{
  WAVEFORMATEX format;
  int sampleRateCount = sizeof(gAllowedSampleRatesAC3) / sizeof(int);
  
  for (int sr = 0 ; sr < sampleRateCount ; sr++)
  {
    format.cbSize = 0;
    format.nChannels = 2;
    format.nSamplesPerSec = gAllowedSampleRatesAC3[sr];
    format.wBitsPerSample = 16;
    format.nBlockAlign = 4;
    format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;
    format.wFormatTag = 146;

    PlayThreadArgs pta = {0};
    pta.pWfx = &format;
    pta.pMMDevice = pPrefs->m_pMMDevice;
    pta.pExclusive = pExclusive;
    pta.pEventDriven = pEventDriven;
    pta.hr = E_UNEXPECTED;

    HANDLE hThread = CreateThread(NULL, 0, PlayThreadFunction, &pta, 0, NULL);

    if (NULL == hThread) 
    {
        printf("CreateThread failed: GetLastError = %u\n", GetLastError());
        return;
    }

    WaitForSingleObject(hThread, INFINITE);

    if (FAILED(pta.hr)) 
    {
        //printf("Thread returned failing HRESULT 0x%08x", pta.hr);
        CloseHandle(hThread);
    }
    else
    {
      CloseHandle(hThread);
    }
  }
}

static DWORD gdwDefaultChannelMask[] = {
  0, // no channels - invalid
  KSAUDIO_SPEAKER_MONO,
  KSAUDIO_SPEAKER_STEREO,
  KSAUDIO_SPEAKER_STEREO | KSAUDIO_SPEAKER_GROUND_FRONT_CENTER,
  KSAUDIO_SPEAKER_QUAD,
  0, // 5 channels?
  KSAUDIO_SPEAKER_5POINT1_SURROUND,
  0, // 7 channels?
  KSAUDIO_SPEAKER_7POINT1_SURROUND
};

void ToWaveFormatExtensible(WAVEFORMATEXTENSIBLE *pwfe, WAVEFORMATEX *pwf)
{
  //ASSERT(pwf->cbSize <= sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX));
  memcpy(pwfe, pwf, sizeof(WAVEFORMATEX)/* + pwf->cbSize*/);
  pwfe->Format.cbSize = sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX);
  switch(pwfe->Format.wFormatTag)
  {
  case WAVE_FORMAT_PCM:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_PCM;
    break;
  case WAVE_FORMAT_IEEE_FLOAT:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;
    break;
  }
  if (pwfe->Format.nChannels >= 1 && pwfe->Format.nChannels <= 8)
  {
    pwfe->dwChannelMask = gdwDefaultChannelMask[pwfe->Format.nChannels];
  }

  pwfe->Samples.wValidBitsPerSample = pwfe->Format.wBitsPerSample;
  pwfe->Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
}
