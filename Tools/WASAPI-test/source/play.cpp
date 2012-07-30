// play.cpp

#include <windows.h>
#include <stdio.h>
#include <mmsystem.h>
#include <mmdeviceapi.h>
#include <audioclient.h>
#include <avrt.h>

#include "play.h"

HRESULT Play(
    HMMIO hFile,
    LPCWAVEFORMATEX pWfx,
    UINT32 nFrames,
    UINT32 nBytes,
    IMMDevice *pMMDevice,
    bool bDetailedInfo,
    bool pExclusive,
    bool pEventDriven,
    bool* pFormatOk
);

DWORD WINAPI PlayThreadFunction(LPVOID pContext) {
    PlayThreadArgs *pArgs = (PlayThreadArgs*)pContext;

    pArgs->hr = CoInitialize(NULL);
    if (FAILED(pArgs->hr)) {
        return 0;
    }

    pArgs->hr = Play(
        pArgs->hFile,
        pArgs->pWfx,
        pArgs->nFrames,
        pArgs->nBytes,
        pArgs->pMMDevice,
        pArgs->pDetailedInfo,
        pArgs->pExclusive,
        pArgs->pEventDriven,
        &pArgs->formatOk
    );

    CoUninitialize();

    return 0;
}

HRESULT Play(
    HMMIO hFile,
    LPCWAVEFORMATEX pWfx,
    UINT32 nFramesInFile,
    UINT32 nBytesInFile,
    IMMDevice *pMMDevice,
    bool pDetailedInfo,
    bool pExclusive,
    bool pEventDriven,
    bool* pFormatOk
) {
    HRESULT hr;
    (*pFormatOk) = false;

    // activate an IAudioClient
    IAudioClient *pAudioClient;
    hr = pMMDevice->Activate(
        __uuidof(IAudioClient),
        CLSCTX_ALL, NULL,
        (void**)&pAudioClient
    );
    if (FAILED(hr)) {
        printf("IMMDevice::Activate(IAudioClient) failed:0x%08x\n", hr);
        return hr;
    }

    _AUDCLNT_SHAREMODE shareMode;
    DWORD streamFlags;

    if (pExclusive)
      shareMode = AUDCLNT_SHAREMODE_EXCLUSIVE;
    else
      shareMode = AUDCLNT_SHAREMODE_SHARED;  

    if (pEventDriven)
      streamFlags = AUDCLNT_STREAMFLAGS_EVENTCALLBACK;
    else
      streamFlags = 0;
      
    WAVEFORMATEX *pwfxCM = NULL;

    // check to see if the format is supported
    hr = pAudioClient->IsFormatSupported(
        shareMode,
        pWfx,
        &pwfxCM
    );

    if (pDetailedInfo)
    {
	    char cSampleType = (pWfx->wFormatTag == WAVE_FORMAT_IEEE_FLOAT ||
                        (pWfx->cbSize == 22 &&
                         (((WAVEFORMATEXTENSIBLE*)pWfx)->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)))? 'f' : 'i';
      printf("%6d %2d%c %2d %8d %2d %5d %2d",
      pWfx->nSamplesPerSec,
      pWfx->wBitsPerSample,
      cSampleType,
      pWfx->nChannels,
      pWfx->nAvgBytesPerSec,
      pWfx->nBlockAlign,
      pWfx->wFormatTag, 
      pWfx->cbSize);

      if(pWfx->cbSize == 22)
      {
        WAVEFORMATEXTENSIBLE* ex = (WAVEFORMATEXTENSIBLE*)pWfx;
        printf(" %2d %4d", ex->Samples.wValidBitsPerSample, ex->dwChannelMask);
      }
      else
        printf("        ");
    }

    if (AUDCLNT_E_UNSUPPORTED_FORMAT == hr) {
        if (pDetailedInfo)
          printf(" - not supported\n");
        pAudioClient->Release();
        return hr;
    } else if (FAILED(hr)) {
        if (pDetailedInfo)
          printf(" - IAudioClient::IsFormatSupported:0x%08x.\n", hr);
        pAudioClient->Release();
        return hr;
    }

    // get the periodicity of the device
    REFERENCE_TIME hnsPeriod;
    hr = pAudioClient->GetDevicePeriod(
        NULL, // don't care about the engine period
        &hnsPeriod // only the device period
    );
    if (FAILED(hr)) {
        printf("IAudioClient::GetDevicePeriod:0x%08x.\n", hr);
        pAudioClient->Release();
        return hr;
    }

    // need to know how many frames that is
    UINT32 nFramesInBuffer = (UINT32)( // frames =
        1.0 * hnsPeriod * // hns *
        pWfx->nSamplesPerSec / // (frames / s) /
        1000 / // (ms / s) /
        10000 // (hns / s) /
        + 0.5 // rounding
    );


    // printf("The default period for this device is %I64u hundred-nanoseconds, or %u frames.\n", hnsPeriod, nFramesInBuffer);

    //printf("\n");

    // call IAudioClient::Initialize the first time
    // this may very well fail
    // if the device period is unaligned
    hr = pAudioClient->Initialize(
        shareMode,
        streamFlags,
        hnsPeriod, hnsPeriod, pWfx, NULL
    );
    // if you get a compilation error on AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED,
    // uncomment the #define below
    #define AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED      AUDCLNT_ERR(0x019)
    if (AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED == hr) {

        // if the buffer size was not aligned, need to do the alignment dance
        if(pDetailedInfo)
          printf("   Buffer size not aligned\n");
        
        // get the buffer size, which will be aligned
        hr = pAudioClient->GetBufferSize(&nFramesInBuffer);
        if (FAILED(hr)) {
            printf("   IAudioClient::GetBufferSize:0x%08x\n", hr);
            return hr;
        }
        
        // throw away this IAudioClient
        pAudioClient->Release();

        // calculate the new aligned periodicity
        hnsPeriod = // hns =
            (REFERENCE_TIME)(
                10000.0 * // (hns / ms) *
                1000 * // (ms / s) *
                nFramesInBuffer / // frames /
                pWfx->nSamplesPerSec  // (frames / s)
                + 0.5 // rounding
            );

        // activate a new IAudioClient
        hr = pMMDevice->Activate(
            __uuidof(IAudioClient),
            CLSCTX_ALL, NULL,
            (void**)&pAudioClient
        );
        if (FAILED(hr)) {
            printf("   IMMDevice::Activate(IAudioClient):0x%08x\n", hr);
            return hr;
        }

        // try initialize again
        if(pDetailedInfo)
          printf("   Trying again with periodicity of %I64u hundred-nanoseconds, or %u frames.\n", hnsPeriod, nFramesInBuffer);
        
        hr = pAudioClient->Initialize(
            shareMode,
            streamFlags,
            hnsPeriod, hnsPeriod, pWfx, NULL
        );

        if (FAILED(hr)) {
            printf("   IAudioClient::Initialize failed, even with an aligned buffer: hr = 0x%08x\n", hr);
            pAudioClient->Release();
            return hr;
        }
    } else if (FAILED(hr)) {
        if (hr == AUDCLNT_E_UNSUPPORTED_FORMAT)
        {
          if (pDetailedInfo)
            printf(" - not supported\n");
        }
        else
        {
          if (pDetailedInfo)
            printf("   IAudioClient::Initialize:0x%08x\n", hr);
        }
        pAudioClient->Release();
        return hr;
    }

    // OK, IAudioClient::Initialize succeeded
    // let's see what buffer size we actually ended up with
    hr = pAudioClient->GetBufferSize(&nFramesInBuffer);
    if (FAILED(hr)) {
        printf("   IAudioClient::GetBufferSize:0x%08x\n", hr);
        pAudioClient->Release();
        return hr;
    }

    // calculate the new period
    hnsPeriod = // hns =
        (REFERENCE_TIME)(
            10000.0 * // (hns / ms) *
            1000 * // (ms / s) *
            nFramesInBuffer / // frames /
            pWfx->nSamplesPerSec  // (frames / s)
            + 0.5 // rounding
        );
    
    if(pDetailedInfo)
      printf("   We ended up with a period of %I64u hns or %u frames.\n", hnsPeriod, nFramesInBuffer);

    // make an event
    HANDLE hNeedDataEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
    if (NULL == hNeedDataEvent) {
        DWORD dwErr = GetLastError();
        printf("   CreateEvent failed:%u\n", dwErr);
        pAudioClient->Release();
        return HRESULT_FROM_WIN32(dwErr);
    }

    if (pEventDriven)
    {
      // set it as the event handle
      hr = pAudioClient->SetEventHandle(hNeedDataEvent);
      if (FAILED(hr)) {
          printf("   IAudioClient::SetEventHandle:0x%08x\n", hr);
          CloseHandle(hNeedDataEvent);
          pAudioClient->Release();
          return hr;
      }
    }
    
    // activate an IAudioRenderClient
    IAudioRenderClient *pAudioRenderClient;
    hr = pAudioClient->GetService(
        __uuidof(IAudioRenderClient),
        (void**)&pAudioRenderClient
    );
    if (FAILED(hr)) {
        printf("   IAudioClient::GetService(IAudioRenderClient):0x%08x\n", hr);
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }
    
    // pre-roll a buffer of silence
    BYTE *pData;
    hr = pAudioRenderClient->GetBuffer(nFramesInBuffer, &pData); // just a "ping" buffer
    if (FAILED(hr)) {
        printf("   IAudioRenderClient::GetBuffer failed trying to pre-roll silence: hr = 0x%08x\n", hr);
        pAudioRenderClient->Release();        
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }

    hr = pAudioRenderClient->ReleaseBuffer(nFramesInBuffer, AUDCLNT_BUFFERFLAGS_SILENT);
    if (FAILED(hr)) {
        printf("   IAudioRenderClient::ReleaseBuffer failed trying to pre-roll silence: hr = 0x%08x\n", hr);
        pAudioRenderClient->Release();        
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }

    // register with MMCSS
    DWORD nTaskIndex = 0;
    HANDLE hTask = AvSetMmThreadCharacteristics(L"Playback", &nTaskIndex);
    if (NULL == hTask) {
        DWORD dwErr = GetLastError();
        printf("   AvSetMmThreadCharacteristics failed: last error = %u\n", dwErr);
        pAudioRenderClient->Release();        
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return HRESULT_FROM_WIN32(dwErr);
    }

    // call IAudioClient::Start
    hr = pAudioClient->Start();
    if (FAILED(hr)) {
        printf("   IAudioClient::Start failed:0x%08x", hr);
        AvRevertMmThreadCharacteristics(hTask);
        pAudioRenderClient->Release();
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
    }

    DWORD res = 0;
    if (pEventDriven)
    {
      res = WaitForSingleObject(hNeedDataEvent, INFINITE);
    }

    UINT32 padding(0);
    if(!pExclusive || !pEventDriven)
      pAudioClient->GetCurrentPadding(&padding);

    hr = pAudioRenderClient->GetBuffer(nFramesInBuffer - padding, &pData);
    if (FAILED(hr)) {
        printf("   IAudioRenderClient::GetBuffer failed to post-roll silence:0x%08x\n", hr);
        pAudioClient->Stop();
        AvRevertMmThreadCharacteristics(hTask);
        pAudioRenderClient->Release();        
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }

    hr = pAudioRenderClient->ReleaseBuffer(nFramesInBuffer - padding, AUDCLNT_BUFFERFLAGS_SILENT);
    if (FAILED(hr)) {
      printf("   IAudioRenderClient::ReleaseBuffer failed trying to post-roll silence:0x%08x\n", hr);
        pAudioClient->Stop();
        AvRevertMmThreadCharacteristics(hTask);
        pAudioRenderClient->Release();        
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }

    (*pFormatOk) = true;

    if (!pDetailedInfo)
    {
      printf("%6d %2d %2d %8d %2d %5d %2d",
        pWfx->nSamplesPerSec,
        pWfx->wBitsPerSample,
        pWfx->nChannels,
        pWfx->nAvgBytesPerSec,
        pWfx->nBlockAlign,
        pWfx->wFormatTag, 
        pWfx->cbSize);

      if(pWfx->cbSize == 22)
      {
        WAVEFORMATEXTENSIBLE* ex = (WAVEFORMATEXTENSIBLE*)pWfx;
        printf(" %2d %4d", ex->Samples.wValidBitsPerSample, ex->dwChannelMask);
      }
      else
        printf("        ");
    }

    printf(" - Format works ok\n", nFramesInFile);

    pAudioClient->Stop();
    AvRevertMmThreadCharacteristics(hTask);
    pAudioRenderClient->Release();
    CloseHandle(hNeedDataEvent);
    pAudioClient->Reset();
    pAudioClient->Release();
    
    return S_OK;
}
