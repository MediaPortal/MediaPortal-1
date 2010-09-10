// play.h

// pass an address to this structure to PlayThreadFunction
struct PlayThreadArgs 
{
    HMMIO hFile;
    LPCWAVEFORMATEX pWfx;
    UINT32 nFrames;
    UINT32 nBytes;
    IMMDevice *pMMDevice;
    bool pDetailedInfo;
    bool pExclusive;
    bool pEventDriven;
    bool formatOk;
    HRESULT hr;
};

// will play the data in hFile,
// which is assumed to be in format pWfx,
// and to contain nFrames of data totaling nBytes,
// to the audio device pMMDevice
//
// sets hr according to success or failure
DWORD WINAPI PlayThreadFunction(LPVOID pContext);