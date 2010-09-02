// prefs.cpp

#include <windows.h>
#include <stdio.h>
#include <mmsystem.h>
#include <mmdeviceapi.h>
#include <functiondiscoverykeys_devpkey.h>

#include "prefs.h"
#include "play.h"

void usage(LPCWSTR exe);
HRESULT list_devices();
HRESULT get_default_device(IMMDevice **ppMMDevice);
HRESULT get_specific_device(LPCWSTR szLongName, IMMDevice **ppMMDevice);
HRESULT open_file(
    LPCWSTR szFileName,
    HMMIO *phFile,
    WAVEFORMATEX **ppWfx,
    UINT32 *pnBytes,
    UINT32 *pnFrames
);

// parses command line
CPrefs::CPrefs(int argc, LPCWSTR argv[], HRESULT &hr)
: m_hFile(NULL)
, m_pMMDevice(NULL)
, m_pWfx(NULL)
, m_nBytes(0)
, m_nFrames(0)
{
    switch (argc) {
        // no arguments
        case 1:
            hr = S_FALSE;
            usage(argv[0]);
            break;

        // -?
        // /?
        // --list-devices
        case 2:
            if (0 == _wcsicmp(argv[1], L"-?") || 0 == _wcsicmp(argv[1], L"/?")) {
                // print usage but don't actually play
                hr = S_FALSE;
                usage(argv[0]);
                break;
            } else if (0 == _wcsicmp(argv[1], L"--list-devices")) {
                // list the devices but don't actually play
                hr = list_devices();

                // don't actually play
                if (S_OK == hr) {
                    hr = S_FALSE;
                }
            } else {
                printf("Unexpected argument %ls\n", argv[1]);
                hr = E_INVALIDARG;
                usage(argv[0]);
            }
            break;

         // --file foo.wav
         case 3:
            if (0 == _wcsicmp(argv[1], L"--file")) {
                HRESULT hrFile = open_file(argv[2], &m_hFile, &m_pWfx, &m_nBytes, &m_nFrames);
                HRESULT hrDevice = get_default_device(&m_pMMDevice);

                if (FAILED(hrFile)) { hr = hrFile; }
                if (FAILED(hrDevice)) { hr = hrDevice; }
            } else {
                printf("Unexpected argument %ls\n", argv[1]);
                hr = E_INVALIDARG;
                usage(argv[0]);
            }
            break;

        // --file foo.wav --device "some device"
        // --device "some device" --file foo.wav
        case 5:
            if (
                0 == _wcsicmp(argv[1], L"--file") &&
                0 == _wcsicmp(argv[3], L"--device")
            ) {
                HRESULT hrFile = open_file(argv[2], &m_hFile, &m_pWfx, &m_nBytes, &m_nFrames);
                HRESULT hrDevice = get_specific_device(argv[4], &m_pMMDevice);

                if (FAILED(hrFile)) { hr = hrFile; }
                if (FAILED(hrDevice)) { hr = hrDevice; }
            } else if (
                0 == _wcsicmp(argv[1], L"--device") &&
                0 == _wcsicmp(argv[3], L"--file")
            ) {
                HRESULT hrFile = open_file(argv[4], &m_hFile, &m_pWfx, &m_nBytes, &m_nFrames);
                HRESULT hrDevice = get_specific_device(argv[2], &m_pMMDevice);

                if (FAILED(hrFile)) { hr = hrFile; }
                if (FAILED(hrDevice)) { hr = hrDevice; }
            } else {
                printf("Unexpected arguments: %ls %ls %ls %ls\n", argv[1], argv[2], argv[3], argv[4]);
                hr = E_INVALIDARG;
                usage(argv[0]);
            }
            break;
        
        default:
            printf("Unexpected argument count %u\n", argc);
            hr = E_INVALIDARG;
            usage(argv[0]);
            break;
    }
}

// cleanup
CPrefs::~CPrefs() {
    if (NULL != m_pMMDevice) {
        m_pMMDevice->Release();
    }

    if (NULL != m_hFile) {
        mmioClose(m_hFile, 0);
    }

    if (NULL != m_pWfx) {
        CoTaskMemFree(m_pWfx);
    }
}

HRESULT get_default_device(IMMDevice **ppMMDevice) {
    HRESULT hr = S_OK;
    IMMDeviceEnumerator *pMMDeviceEnumerator;

    // activate a device enumerator
    hr = CoCreateInstance(
        __uuidof(MMDeviceEnumerator), NULL, CLSCTX_ALL, 
        __uuidof(IMMDeviceEnumerator),
        (void**)&pMMDeviceEnumerator
    );
    if (FAILED(hr)) {
        printf("CoCreateInstance(IMMDeviceEnumerator) failed: hr = 0x%08x\n", hr);
        return hr;
    }

    // get the default render endpoint
    hr = pMMDeviceEnumerator->GetDefaultAudioEndpoint(eRender, eConsole, ppMMDevice);
    pMMDeviceEnumerator->Release();
    if (FAILED(hr)) {
        printf("IMMDeviceEnumerator::GetDefaultAudioEndpoint failed: hr = 0x%08x\n", hr);
        return hr;
    }

    return S_OK;
}

HRESULT list_devices() {
    HRESULT hr = S_OK;

    // get an enumerator
    IMMDeviceEnumerator *pMMDeviceEnumerator;

    hr = CoCreateInstance(
        __uuidof(MMDeviceEnumerator), NULL, CLSCTX_ALL, 
        __uuidof(IMMDeviceEnumerator),
        (void**)&pMMDeviceEnumerator
    );
    if (FAILED(hr)) {
        printf("CoCreateInstance(IMMDeviceEnumerator) failed: hr = 0x%08x\n", hr);
        return hr;
    }

    IMMDeviceCollection *pMMDeviceCollection;

    // get all the active render endpoints
    hr = pMMDeviceEnumerator->EnumAudioEndpoints(
        eRender, DEVICE_STATE_ACTIVE, &pMMDeviceCollection
    );
    pMMDeviceEnumerator->Release();
    if (FAILED(hr)) {
        printf("IMMDeviceEnumerator::EnumAudioEndpoints failed: hr = 0x%08x\n", hr);
        return hr;
    }

    UINT count;
    hr = pMMDeviceCollection->GetCount(&count);
    if (FAILED(hr)) {
        pMMDeviceCollection->Release();
        printf("IMMDeviceCollection::GetCount failed: hr = 0x%08x\n", hr);
        return hr;
    }
    printf("Active render endpoints found: %u\n", count);

    for (UINT i = 0; i < count; i++) {
        IMMDevice *pMMDevice;

        // get the "n"th device
        hr = pMMDeviceCollection->Item(i, &pMMDevice);
        if (FAILED(hr)) {
            pMMDeviceCollection->Release();
            printf("IMMDeviceCollection::Item failed: hr = 0x%08x\n", hr);
            return hr;
        }

        // open the property store on that device
        IPropertyStore *pPropertyStore;
        hr = pMMDevice->OpenPropertyStore(STGM_READ, &pPropertyStore);
        pMMDevice->Release();
        if (FAILED(hr)) {
            pMMDeviceCollection->Release();
            printf("IMMDevice::OpenPropertyStore failed: hr = 0x%08x\n", hr);
            return hr;
        }

        // get the long name property
        PROPVARIANT pv; PropVariantInit(&pv);
        hr = pPropertyStore->GetValue(PKEY_Device_FriendlyName, &pv);
        pPropertyStore->Release();
        if (FAILED(hr)) {
            pMMDeviceCollection->Release();
            printf("IPropertyStore::GetValue failed: hr = 0x%08x\n", hr);
            return hr;
        }

        if (VT_LPWSTR != pv.vt) {
            printf("PKEY_Device_FriendlyName variant type is %u - expected VT_LPWSTR", pv.vt);

            PropVariantClear(&pv);
            pMMDeviceCollection->Release();
            return E_UNEXPECTED;
        }

        printf("    %ls\n", pv.pwszVal);
        
        PropVariantClear(&pv);
    }    
    pMMDeviceCollection->Release();
    
    return S_OK;
}

HRESULT get_specific_device(LPCWSTR szLongName, IMMDevice **ppMMDevice) {
    HRESULT hr = S_OK;

    *ppMMDevice = NULL;
    
    // get an enumerator
    IMMDeviceEnumerator *pMMDeviceEnumerator;

    hr = CoCreateInstance(
        __uuidof(MMDeviceEnumerator), NULL, CLSCTX_ALL, 
        __uuidof(IMMDeviceEnumerator),
        (void**)&pMMDeviceEnumerator
    );
    if (FAILED(hr)) {
        printf("CoCreateInstance(IMMDeviceEnumerator) failed: hr = 0x%08x\n", hr);
        return hr;
    }

    IMMDeviceCollection *pMMDeviceCollection;

    // get all the active render endpoints
    hr = pMMDeviceEnumerator->EnumAudioEndpoints(
        eRender, DEVICE_STATE_ACTIVE, &pMMDeviceCollection
    );
    pMMDeviceEnumerator->Release();
    if (FAILED(hr)) {
        printf("IMMDeviceEnumerator::EnumAudioEndpoints failed: hr = 0x%08x\n", hr);
        return hr;
    }

    UINT count;
    hr = pMMDeviceCollection->GetCount(&count);
    if (FAILED(hr)) {
        pMMDeviceCollection->Release();
        printf("IMMDeviceCollection::GetCount failed: hr = 0x%08x\n", hr);
        return hr;
    }

    for (UINT i = 0; i < count; i++) {
        IMMDevice *pMMDevice;

        // get the "n"th device
        hr = pMMDeviceCollection->Item(i, &pMMDevice);
        if (FAILED(hr)) {
            pMMDeviceCollection->Release();
            printf("IMMDeviceCollection::Item failed: hr = 0x%08x\n", hr);
            return hr;
        }

        // open the property store on that device
        IPropertyStore *pPropertyStore;
        hr = pMMDevice->OpenPropertyStore(STGM_READ, &pPropertyStore);
        if (FAILED(hr)) {
            pMMDevice->Release();
            pMMDeviceCollection->Release();
            printf("IMMDevice::OpenPropertyStore failed: hr = 0x%08x\n", hr);
            return hr;
        }

        // get the long name property
        PROPVARIANT pv; PropVariantInit(&pv);
        hr = pPropertyStore->GetValue(PKEY_Device_FriendlyName, &pv);
        pPropertyStore->Release();
        if (FAILED(hr)) {
            pMMDevice->Release();
            pMMDeviceCollection->Release();
            printf("IPropertyStore::GetValue failed: hr = 0x%08x\n", hr);
            return hr;
        }

        if (VT_LPWSTR != pv.vt) {
            printf("PKEY_Device_FriendlyName variant type is %u - expected VT_LPWSTR", pv.vt);

            PropVariantClear(&pv);
            pMMDevice->Release();
            pMMDeviceCollection->Release();
            return E_UNEXPECTED;
        }

        // is it a match?
        if (0 == _wcsicmp(pv.pwszVal, szLongName)) {
            // did we already find it?
            if (NULL == *ppMMDevice) {
                *ppMMDevice = pMMDevice;
                pMMDevice->AddRef();
            } else {
                printf("Found (at least) two devices named %ls\n", szLongName);
                PropVariantClear(&pv);
                pMMDevice->Release();
                pMMDeviceCollection->Release();
                return E_UNEXPECTED;
            }
        }
        
        pMMDevice->Release();
        PropVariantClear(&pv);
    }
    pMMDeviceCollection->Release();
    
    if (NULL == *ppMMDevice) {
        printf("Could not find a device named %ls\n", szLongName);
        return HRESULT_FROM_WIN32(ERROR_NOT_FOUND);
    }

    return S_OK;
}

HRESULT open_file(
    LPCWSTR szFileName,
    HMMIO *phFile,
    WAVEFORMATEX **ppWfx,
    UINT32 *pnBytes,
    UINT32 *pnFrames
) {

    MMIOINFO mi = {0};

    printf("Opening .wav file \"%ls\"...\n", szFileName);
    HMMIO hFile = mmioOpen(
        // some flags cause mmioOpen write to this buffer
        // but not any that we're using
        const_cast<LPWSTR>(szFileName),
        &mi,
        MMIO_READ
    );

    if (NULL == hFile) {
        printf("mmioOpen(\"%ls\", ...) failed. wErrorRet == %u\n", szFileName, mi.wErrorRet);
        return E_FAIL;
    }

    // parse file
    MMCKINFO ckRiff = {0};
    ckRiff.ckid = MAKEFOURCC('W', 'A', 'V', 'E');
    MMRESULT mmr = mmioDescend(
        hFile,
        &ckRiff,
        NULL, // no parent
        MMIO_FINDRIFF
    );

    if (MMSYSERR_NOERROR != mmr) {
        printf("Could not find a RIFF/WAVE chunk: mmr = 0x%08x\n", mmr);
        mmioClose(hFile, 0);
        return E_INVALIDARG;
    }

    // find the "fmt " chunk
    MMCKINFO ckFmt = {0};
    ckFmt.ckid = MAKEFOURCC('f', 'm', 't', ' ');
    mmr = mmioDescend(
        hFile,
        &ckFmt,
        &ckRiff,
        MMIO_FINDCHUNK
    );
    if (MMSYSERR_NOERROR != mmr) {
        printf("Could not find a \"fmt \" chunk in the RIFF/WAVE chunk: mmr = 0x%08x\n", mmr);
        mmioClose(hFile, 0);
        return HRESULT_FROM_WIN32(ERROR_NOT_FOUND);
    }

    // actually read the fmt data from the fmt chunk
    UINT nBytes = ckFmt.cksize;
    if (0 == nBytes) {
        printf("\"fmt \" chunk has size 0!\n");
        mmioClose(hFile, 0);
        return E_INVALIDARG;
    }
    if (sizeof(PCMWAVEFORMAT) > nBytes) {
        printf("\"fmt \" chunk has size %u which is less than sizeof(PCMWAVEFORMAT) (%u)\n",
            nBytes, (int)sizeof(PCMWAVEFORMAT)
        );
        mmioClose(hFile, 0);
        return E_INVALIDARG;
    }

    UINT32 nBytesToAllocate = nBytes;
    if (nBytes < sizeof(WAVEFORMATEX)) {
        nBytesToAllocate = sizeof(WAVEFORMATEX);
        printf("Wave format in file is smaller than a WAVEFORMATEX; will zero-pad the last %u bytes.\n", nBytesToAllocate - nBytes);
    }
    
    WAVEFORMATEX *pWfx = (WAVEFORMATEX*)CoTaskMemAlloc(nBytesToAllocate);
    if (NULL == pWfx) {
        printf("Could not allocate %u bytes for WAVEFORMATEX\n", nBytes);
        mmioClose(hFile, 0);
        return E_OUTOFMEMORY;
    }
    ZeroMemory(pWfx, nBytesToAllocate);

    LONG nBytesRead = mmioRead(hFile, (HPSTR)pWfx, nBytes);
    if (0 == nBytesRead) {
        printf("Unexpected EOF in fmt chunk - tried to read %u bytes\n", nBytes);
        CoTaskMemFree(pWfx);
        mmioClose(hFile, 0);
        return E_INVALIDARG;
    } else if (-1 == nBytesRead) {
        printf("Could not read from file when reading format data - tried to read %u bytes\n", nBytes);
        CoTaskMemFree(pWfx);
        mmioClose(hFile, 0);
        return E_INVALIDARG;
    } else if (nBytes != (UINT)nBytesRead) {
        printf("Tried to read %u bytes but read %u bytes\n", nBytes, nBytesRead);
        CoTaskMemFree(pWfx);
        mmioClose(hFile, 0);
        return E_INVALIDARG;
    }

    // do some sanity checking on the wave format
    if (sizeof(WAVEFORMATEX) > nBytes) {
        // well, it had better be a PCMWAVEFORMAT
        if (sizeof(PCMWAVEFORMAT) == nBytes && WAVE_FORMAT_PCM == pWfx->wFormatTag) {
            // fine
        } else {
            printf("Only WAVE_FORMAT_PCM formats are allowed to be smaller than sizeof(WAVEFORMATEX) - wFormatTag == %u, size == %u which is < sizeof(WAVEFORMATEX) (%u)\n",
                pWfx->wFormatTag, nBytes, (int)sizeof(WAVEFORMATEX)
            );
            CoTaskMemFree(pWfx);
            mmioClose(hFile, 0);
            return E_INVALIDARG;
        }
    } else {
        // cbSize had better be sane
        if (sizeof(WAVEFORMATEX) + pWfx->cbSize != nBytes) {
            printf("Format chunk size does not match format size - sizeof(WAVEFORMATEX) + cbSize == %u + %u = %u, chunk size == %u\n",
                (int)sizeof(WAVEFORMATEX), pWfx->cbSize, (int)sizeof(WAVEFORMATEX) + pWfx->cbSize, nBytes
            );
            CoTaskMemFree(pWfx);
            mmioClose(hFile, 0);
            return E_INVALIDARG;
        }
    }

    mmr = mmioAscend(hFile, &ckFmt, 0);
    if (MMSYSERR_NOERROR != mmr) {
        printf("Could not mmioAscend out of \"fmt \" chunk: mmr = 0x%08x\n", mmr);
        CoTaskMemFree(pWfx);
        mmioClose(hFile, 0);
        return E_INVALIDARG;
    }

    // unless the wave format is WAVE_FORMAT_PCM, find the "fact" chunk
    UINT nFrames = 0;
    if (WAVE_FORMAT_PCM != pWfx->wFormatTag) {
        MMCKINFO ckFact = {0};
        ckFact.ckid = MAKEFOURCC('f', 'a', 'c', 't');
        mmr = mmioDescend(
            hFile,
            &ckFact,
            &ckRiff,
            MMIO_FINDCHUNK
        );
        if (MMSYSERR_NOERROR != mmr) {
            printf("wFormatTag == %u but no \"fact\" chunk follows the \"fmt \" chunk - \"fact\" is required for wFormatTag != %u: mmr = 0x%08x\n",
                pWfx->wFormatTag, WAVE_FORMAT_PCM, mmr
            );
            CoTaskMemFree(pWfx);
            mmioClose(hFile, 0);
            return E_INVALIDARG;
        }
        nBytes = ckFact.cksize;
        if (sizeof(UINT32) != nBytes) {
            printf("\"fact\" chunk contains %u bytes - expected %u\n", nBytes, (int)sizeof(UINT32));
            CoTaskMemFree(pWfx);
            mmioClose(hFile, 0);
            return E_INVALIDARG;
        }

        nBytesRead = mmioRead(hFile, (HPSTR)&nFrames, nBytes);
        if (0 == nBytesRead) {
            printf("Unexpected EOF in fmt chunk - tried to read %u bytes\n", nBytes);
            CoTaskMemFree(pWfx);
            mmioClose(hFile, 0);
            return E_INVALIDARG;
         } else if (-1 == nBytesRead) {
            printf("Could not read from file when reading format data - tried to read %u bytes\n", nBytes);
            CoTaskMemFree(pWfx);
            mmioClose(hFile, 0);
            return E_INVALIDARG;
        } else if ((UINT)nBytesRead != nBytes) {
            printf("Tried to read %u bytes but read %u bytes\n", nBytes, nBytesRead);
            CoTaskMemFree(pWfx);
            mmioClose(hFile, 0);
            return E_INVALIDARG;
        }

        mmr = mmioAscend(hFile, &ckFact, 0);
        if (MMSYSERR_NOERROR != mmr) {
            printf("Could not mmioAscend out of \"fact\" chunk: mmr = 0x%08x\n", mmr);
            CoTaskMemFree(pWfx);
            mmioClose(hFile, 0);
            return E_INVALIDARG;
        }
    } // fact chunk
    
    // find the "data" chunk
    MMCKINFO ckData = {0};
    ckData.ckid = MAKEFOURCC('d', 'a', 't', 'a');
    mmr = mmioDescend(
        hFile,
        &ckData,
        &ckRiff,
        MMIO_FINDCHUNK
    );
    if (MMSYSERR_NOERROR != mmr) {
        printf("Could not find \"data\" after \"fmt \" (or \"fact\") chunk: mmr = 0x%08x\n", mmr);
        CoTaskMemFree(pWfx);
        mmioClose(hFile, 0);
        return E_INVALIDARG;
    }
    UINT nWaveDataBytes = ckData.cksize;

    // number of bytes of data should be a multiple of the frame size
    UINT32 nBytesPerFrame = pWfx->nBlockAlign;
    if (0 != nWaveDataBytes % nBytesPerFrame) {
        printf("\"data\" chunk contains %u bytes - format is %u bytes per frame; but %u %% %u == %u != 0!\n",
            nWaveDataBytes, nBytesPerFrame, nWaveDataBytes, nBytesPerFrame,
            nWaveDataBytes % nBytesPerFrame
        );
        CoTaskMemFree(pWfx);
        mmioClose(hFile, 0);
        return E_INVALIDARG;
    }

    // if there was a fact chunk, verify that the number of bytes is expected
    if (WAVE_FORMAT_PCM != pWfx->wFormatTag) {
        if (nFrames * nBytesPerFrame != nWaveDataBytes) {
            printf(
                "\"fact\" chunk reports %u frames\n"
                "format is %u bytes per frame\n"
                "data chunk has %u bytes\n"
                "but %u * %u == %u != %u\n",
                nFrames, nBytesPerFrame, nWaveDataBytes,
                nFrames, nBytesPerFrame, nFrames * nBytesPerFrame, nWaveDataBytes
            );
            CoTaskMemFree(pWfx);
            mmioClose(hFile, 0);
            return E_INVALIDARG;
        }
    } else {
        // set the number of frames by the number of bytes
        nFrames = nWaveDataBytes / nBytesPerFrame;
    }
    
    *phFile = hFile;
    *ppWfx = pWfx;
    *pnBytes = nWaveDataBytes;
    *pnFrames = nFrames;
 
    return S_OK;
}

void usage(LPCWSTR exe) {
    printf(
        "%ls -?\n"
        "%ls --list-devices\n"
        "%ls [--device \"Device long name\"] --file \"WAV file name\"\n"
        "\n"
        "    -? prints this message.\n"
        "    --list-devices displays the long names of all active playback devices.\n"
        "\n"
        "Plays the given file to the given device in WASAPI exclusive mode.\n"
        "If no device is specified, plays to the default console device.\n"
        ,
        exe, exe, exe
    );
}
