/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2014 see Authors.txt
 *
 * This file is part of MPC-HC.
 *
 * MPC-HC is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPC-HC is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#pragma once

#include "../dsutil/StdHelpers.h"

class CTextFile
{
public:
    enum SeekFrom {
        begin = SEEK_SET,
        current = SEEK_CUR,
        end = SEEK_END
    };

    enum enc {
        DEFAULT_ENCODING,
        UTF8,
        LE16,
        BE16,
        ANSI
    };

private:
    enc m_encoding, m_defaultencoding, m_fallbackencoding;
    int m_offset;
    ULONGLONG m_posInFile;
    std::unique_ptr<char[]> m_buffer;
    std::unique_ptr<WCHAR[]> m_wbuffer;
    LONGLONG m_posInBuffer, m_nInBuffer;

    FilePtr m_pStream;
    tstring m_strFileName;

public:
    CTextFile(enc e = DEFAULT_ENCODING);
    virtual ~CTextFile() {}

    void Flush();
    void Close();

    virtual bool Open(LPCTSTR lpszFileName);
    virtual bool Save(LPCTSTR lpszFileName, enc e /*= DEFAULT_ENCODING*/);

    void SetEncoding(enc e);
    void SetFallbackEncoding(enc e);
    enc GetEncoding();
    bool IsUnicode();

    tstring GetFilePath() const;

    ULONGLONG GetPosition() const;
    ULONGLONG GetLength() const;
    ULONGLONG Seek(LONGLONG lOff, UINT nFrom);

    void WriteString(LPCSTR lpsz/*std::string str*/);
    void WriteString(LPCWSTR lpsz/*std::wstring str*/);
    void WriteString(const std::string& str) { WriteString(str.c_str()); }
    void WriteString(const std::wstring& str) { WriteString(str.c_str()); }
    BOOL ReadString(std::string& str);
    BOOL ReadString(std::wstring& str);

protected:
    // The file as a C stream. The text modes translate the line ends and the code page like the CRT does.
    enum StreamMode { READ_BINARY, READ_TEXT, WRITE_BINARY, WRITE_TEXT };
    bool OpenStream(LPCTSTR lpszFileName, StreamMode mode);
    void CloseStream();
    UINT StreamRead(void* lpBuf, UINT nCount);
    void StreamWrite(const void* lpBuf, UINT nCount);
    ULONGLONG StreamSeek(LONGLONG lOff, UINT nFrom);
    ULONGLONG StreamPosition() const;
    ULONGLONG StreamLength() const;
    BOOL StreamReadLine(tstring& str);
    void StreamWriteString(LPCTSTR lpsz);

    virtual bool ReopenAsText();
    bool FillBuffer();
    ULONGLONG GetPositionFastBuffered() const;
};

class CWebTextFile : public CTextFile
{
    LONGLONG m_llMaxSize;
    tstring m_tempfn;

public:
    CWebTextFile(CTextFile::enc e = DEFAULT_ENCODING, LONGLONG llMaxSize = 64 * 1024 * 1024);

    bool Open(LPCTSTR lpszFileName);
    bool Save(LPCTSTR lpszFileName, enc e /*= DEFAULT_ENCODING*/);
    void Close();
};

extern std::wstring AToW(std::string str);
extern std::string WToA(std::wstring str);
extern tstring  AToT(std::string str);
extern tstring  WToT(std::wstring str);
extern std::string TToA(tstring  str);
extern std::wstring TToW(tstring  str);
