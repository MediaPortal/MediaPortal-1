// prefs.h

class CPrefs {
public:
    HMMIO m_hFile;
    IMMDevice *m_pMMDevice;
    WAVEFORMATEX *m_pWfx;
    UINT32 m_nBytes;
    UINT32 m_nFrames;
    bool pDetailedInfo;

    // set hr to S_FALSE to abort but return success
    CPrefs(int argc, LPCWSTR argv[], HRESULT &hr);
    ~CPrefs();

};
