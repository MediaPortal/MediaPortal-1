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
    IMMDevice *pMMDevice
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
        pArgs->pMMDevice
    );

    CoUninitialize();

    return 0;
}

HRESULT Play(
    HMMIO hFile,
    LPCWAVEFORMATEX pWfx,
    UINT32 nFramesInFile,
    UINT32 nBytesInFile,
    IMMDevice *pMMDevice
) {

    if (nFramesInFile == 0) {
        printf("No frames in file.\n");
        return E_INVALIDARG;
    }
    
    if (nFramesInFile * pWfx->nBlockAlign != nBytesInFile) {
        printf(
            "Unexpected number of bytes in the file (%u) - expected %u.\n",
            nBytesInFile,
            nFramesInFile * pWfx->nBlockAlign
        );
        return E_INVALIDARG;
    }

    HRESULT hr;

    // activate an IAudioClient
    IAudioClient *pAudioClient;
    hr = pMMDevice->Activate(
        __uuidof(IAudioClient),
        CLSCTX_ALL, NULL,
        (void**)&pAudioClient
    );
    if (FAILED(hr)) {
        printf("IMMDevice::Activate(IAudioClient) failed: hr = 0x%08x\n", hr);
        return hr;
    }

    // check to see if the format is supported
    hr = pAudioClient->IsFormatSupported(
        AUDCLNT_SHAREMODE_EXCLUSIVE,
        pWfx,
        NULL // can't suggest a "closest match" in exclusive mode
    );


    printf("%d %d %d %d %d %d %d",
    pWfx->wFormatTag,
    pWfx->wBitsPerSample,
    pWfx->cbSize,
    pWfx->nAvgBytesPerSec,
    pWfx->nChannels,
    pWfx->nSamplesPerSec,
    pWfx->nBlockAlign);

    if (AUDCLNT_E_UNSUPPORTED_FORMAT == hr) {
        printf("Audio device does not support the requested format.\n");
        pAudioClient->Release();
        return hr;
    } else if (FAILED(hr)) {
        printf("IAudioClient::IsFormatSupported failed: hr = 0x%08x.\n", hr);
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
        printf("IAudioClient::GetDevicePeriod failed: hr = 0x%08x.\n", hr);
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


    printf("The default period for this device is %I64u hundred-nanoseconds, or %u frames.\n", hnsPeriod, nFramesInBuffer);

    // call IAudioClient::Initialize the first time
    // this may very well fail
    // if the device period is unaligned
    hr = pAudioClient->Initialize(
        AUDCLNT_SHAREMODE_EXCLUSIVE,
        AUDCLNT_STREAMFLAGS_EVENTCALLBACK,
        hnsPeriod, hnsPeriod, pWfx, NULL
    );
    // if you get a compilation error on AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED,
    // uncomment the #define below
    #define AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED      AUDCLNT_ERR(0x019)
    if (AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED == hr) {

        // if the buffer size was not aligned, need to do the alignment dance
        printf("Buffer size not aligned - doing the alignment dance.\n");
        
        // get the buffer size, which will be aligned
        hr = pAudioClient->GetBufferSize(&nFramesInBuffer);
        if (FAILED(hr)) {
            printf("IAudioClient::GetBufferSize failed: hr = 0x%08x\n", hr);
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
            printf("IMMDevice::Activate(IAudioClient) failed: hr = 0x%08x\n", hr);
            return hr;
        }

        // try initialize again
        printf("Trying again with periodicity of %I64u hundred-nanoseconds, or %u frames.\n", hnsPeriod, nFramesInBuffer);
        hr = pAudioClient->Initialize(
            AUDCLNT_SHAREMODE_EXCLUSIVE,
            AUDCLNT_STREAMFLAGS_EVENTCALLBACK,
            hnsPeriod, hnsPeriod, pWfx, NULL
        );

        if (FAILED(hr)) {
            printf("IAudioClient::Initialize failed, even with an aligned buffer: hr = 0x%08x\n", hr);
            pAudioClient->Release();
            return hr;
        }
    } else if (FAILED(hr)) {
        printf("IAudioClient::Initialize failed: hr = 0x%08x\n", hr);
        pAudioClient->Release();
        return hr;
    }

    // OK, IAudioClient::Initialize succeeded
    // let's see what buffer size we actually ended up with
    hr = pAudioClient->GetBufferSize(&nFramesInBuffer);
    if (FAILED(hr)) {
        printf("IAudioClient::GetBufferSize failed: hr = 0x%08x\n", hr);
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
    
    printf("We ended up with a period of %I64u hns or %u frames.\n", hnsPeriod, nFramesInBuffer);

    // make an event
    HANDLE hNeedDataEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
    if (NULL == hNeedDataEvent) {
        DWORD dwErr = GetLastError();
        printf("CreateEvent failed: GetLastError = %u\n", dwErr);
        pAudioClient->Release();
        return HRESULT_FROM_WIN32(dwErr);
    }

    // set it as the event handle
    hr = pAudioClient->SetEventHandle(hNeedDataEvent);
    if (FAILED(hr)) {
        printf("IAudioClient::SetEventHandle failed: hr = 0x%08x\n", hr);
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }
    
    // activate an IAudioRenderClient
    IAudioRenderClient *pAudioRenderClient;
    hr = pAudioClient->GetService(
        __uuidof(IAudioRenderClient),
        (void**)&pAudioRenderClient
    );
    if (FAILED(hr)) {
        printf("IAudioClient::GetService(IAudioRenderClient) failed: hr 0x%08x\n", hr);
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }
    
    // pre-roll a buffer of silence
    BYTE *pData;
    hr = pAudioRenderClient->GetBuffer(nFramesInBuffer, &pData); // just a "ping" buffer
    if (FAILED(hr)) {
        printf("IAudioRenderClient::GetBuffer failed trying to pre-roll silence: hr = 0x%08x\n", hr);
        pAudioRenderClient->Release();        
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }

    hr = pAudioRenderClient->ReleaseBuffer(nFramesInBuffer, AUDCLNT_BUFFERFLAGS_SILENT);
    if (FAILED(hr)) {
        printf("IAudioRenderClient::ReleaseBuffer failed trying to pre-roll silence: hr = 0x%08x\n", hr);
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
        printf("AvSetMmThreadCharacteristics failed: last error = %u\n", dwErr);
        pAudioRenderClient->Release();        
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return HRESULT_FROM_WIN32(dwErr);
    }

    // call IAudioClient::Start
    hr = pAudioClient->Start();
    if (FAILED(hr)) {
        printf("IAudioClient::Start failed: hr = 0x%08x", hr);
        AvRevertMmThreadCharacteristics(hTask);
        pAudioRenderClient->Release();
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
    }

    // render loop
    for (
        UINT32 nFramesPlayed = 0,
            nFramesThisPass = nFramesInBuffer;
        nFramesPlayed < nFramesInFile;
        nFramesPlayed += nFramesThisPass
    ) {
        // in a production app there would be a timeout here
        WaitForSingleObject(hNeedDataEvent, INFINITE);

        // need data
        hr = pAudioRenderClient->GetBuffer(nFramesInBuffer, &pData);
        if (FAILED(hr)) {
            printf("IAudioRenderClient::GetBuffer failed: hr = 0x%08x\n", hr);
            pAudioClient->Stop();
            AvRevertMmThreadCharacteristics(hTask);
            pAudioRenderClient->Release();        
            CloseHandle(hNeedDataEvent);
            pAudioClient->Release();
            return hr;
        }

        // is there a full buffer's worth of data left in the file?
        if (nFramesPlayed + nFramesInBuffer > nFramesInFile) {
            // nope - this is the last buffer
            nFramesThisPass = nFramesInFile - nFramesPlayed;
        }
        UINT32 nBytesThisPass = nFramesThisPass * pWfx->nBlockAlign;

        LONG nBytesGotten = mmioRead(hFile, (HPSTR)pData, nBytesThisPass);
        if (0 == nBytesGotten) {
            printf("Unexpectedly reached the end of the file.\n");
            pAudioClient->Stop();
            AvRevertMmThreadCharacteristics(hTask);
            pAudioRenderClient->Release();        
            CloseHandle(hNeedDataEvent);
            pAudioClient->Release();
            return E_UNEXPECTED;            
        } else if (-1 == nBytesGotten) {
            printf("Error reading from the file.\n");
            pAudioClient->Stop();
            AvRevertMmThreadCharacteristics(hTask);
            pAudioRenderClient->Release();        
            CloseHandle(hNeedDataEvent);
            pAudioClient->Release();
            return E_UNEXPECTED;            
        } else if (nBytesGotten != (LONG)nBytesThisPass) {
            printf("mmioRead got %d bytes instead of %u\n", nBytesGotten, nBytesThisPass);
            pAudioClient->Stop();
            AvRevertMmThreadCharacteristics(hTask);
            pAudioRenderClient->Release();        
            CloseHandle(hNeedDataEvent);
            pAudioClient->Release();
            return E_UNEXPECTED;
        }

        // if there's leftover buffer space, zero it out
        // it would be much better if we could intelligently fill this with silence
        // ah well, c'est la vie
        if (nFramesThisPass < nFramesInBuffer) {
            UINT32 nBytesToZero = (nFramesInBuffer * pWfx->nBlockAlign) - nBytesThisPass;
            ZeroMemory(pData + nBytesGotten, nBytesToZero);
        }

        hr = pAudioRenderClient->ReleaseBuffer(nFramesInBuffer, 0); // no flags
        if (FAILED(hr)) {
            printf("IAudioRenderClient::ReleaseBuffer failed: hr = 0x%08x\n", hr);
            pAudioClient->Stop();
            AvRevertMmThreadCharacteristics(hTask);
            pAudioRenderClient->Release();        
            CloseHandle(hNeedDataEvent);
            pAudioClient->Release();
            return hr;
        }
    } // render loop

    // add a buffer of silence for good measure
    WaitForSingleObject(hNeedDataEvent, INFINITE);
    hr = pAudioRenderClient->GetBuffer(nFramesInBuffer, &pData);
    if (FAILED(hr)) {
        printf("IAudioRenderClient::GetBuffer failed trying to post-roll silence: hr = 0x%08x\n", hr);
        pAudioClient->Stop();
        AvRevertMmThreadCharacteristics(hTask);
        pAudioRenderClient->Release();        
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }

    hr = pAudioRenderClient->ReleaseBuffer(nFramesInBuffer, AUDCLNT_BUFFERFLAGS_SILENT);
    if (FAILED(hr)) {
        printf("IAudioRenderClient::ReleaseBuffer failed trying to post-roll silence: hr = 0x%08x\n", hr);
        pAudioClient->Stop();
        AvRevertMmThreadCharacteristics(hTask);
        pAudioRenderClient->Release();        
        CloseHandle(hNeedDataEvent);
        pAudioClient->Release();
        return hr;
    }

    printf("Successfully played all %u frames.\n", nFramesInFile);

    pAudioClient->Stop();
    AvRevertMmThreadCharacteristics(hTask);
    pAudioRenderClient->Release();
    CloseHandle(hNeedDataEvent);
    pAudioClient->Release();
    
    return S_OK;
}
