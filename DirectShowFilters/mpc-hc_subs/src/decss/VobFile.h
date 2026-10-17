#pragma once

#include "winddk/ntddcdvd.h"

class CDVDSession
{
protected:
    HANDLE m_hDrive;

    DVD_SESSION_ID m_session;
    bool BeginSession();
    void EndSession();

    BYTE m_SessionKey[5];
    bool Authenticate();

    BYTE m_DiscKey[6], m_TitleKey[6];
    bool GetDiscKey();
    bool GetTitleKey(int lba, BYTE* pKey);

public:
    CDVDSession();
    virtual ~CDVDSession();

    bool Open(LPCTSTR path);
    void Close();

    operator HANDLE() const { return m_hDrive; }
    operator DVD_SESSION_ID() const { return m_session; }

    bool SendKey(DVD_KEY_TYPE KeyType, BYTE* pKeyData);
    bool ReadKey(DVD_KEY_TYPE KeyType, BYTE* pKeyData, int lba = 0);
};

class CLBAFile
{
    HANDLE m_hFile = INVALID_HANDLE_VALUE;

public:
    CLBAFile();
    virtual ~CLBAFile();

    bool IsOpen() const;

    bool Open(LPCTSTR path);
    void Close();

    int GetLengthLBA() const;
    int GetPositionLBA() const;
    int Seek(int lba);
    bool Read(BYTE* buff);
};

class CVobFile : public CDVDSession
{
    // all files
    struct file_t {
        tstring fn;
        int size;
    };

    std::vector<file_t> m_files;
    int m_iFile;
    int m_pos, m_size, m_offset, m_offsetAuth;

    // currently opened file
    CLBAFile m_file;

    // attribs
    bool m_fDVD, m_fHasDiscKey, m_fHasTitleKey;

    std::map<DWORD, tstring> m_pStream_Lang;

    int m_iChaptersCount;
    std::map<BYTE, LONGLONG> m_pChapters;
public:
    CVobFile();
    virtual ~CVobFile();

    static bool GetTitleInfo(LPCTSTR fn, ULONG nTitleNum, ULONG& VTSN /* out */, ULONG& TTN /* out */); // video_ts.ifo

    bool IsDVD() const;
    bool HasDiscKey(BYTE* key) const;
    bool HasTitleKey(BYTE* key) const;

    bool Open(tstring fn, std::list<tstring>& files /* out */, ULONG nProgNum = 1, bool bAuthenticate = true); // vts ifo
    bool Open(const std::list<tstring>& files, int offset = -1, bool bAuthenticate = true); // vts vobs, video vob offset in lba
    bool Authenticate();
    void Close();

    int GetLength() const;
    int GetPosition() const;
    int Seek(int pos);
    bool Read(BYTE* buff);

    BSTR GetTrackName(UINT aTrackIdx) const;

    int GetChaptersCount() const { return m_iChaptersCount; }
    LONGLONG GetChapterOffset(UINT chapterNumber) const;

private:
    HANDLE  m_ifoFile = INVALID_HANDLE_VALUE;
    DWORD   ReadDword();
    short   ReadShort();
    BYTE    ReadByte();
    void    ReadBuffer(BYTE* pBuff, DWORD nLen);
};
