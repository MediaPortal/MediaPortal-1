/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2014, 2016 see Authors.txt
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

#include "stdafx.h"
#include <urlmon.h>

#pragma comment(lib, "urlmon.lib")
#include <algorithm>
#include <filesystem>
#include <ios>
#include "TextFile.h"
#include "Utf8.h"

#define TEXTFILE_BUFFER_SIZE (64 * 1024)

CTextFile::CTextFile(enc e)
    : m_encoding(e)
    , m_defaultencoding(e)
    , m_fallbackencoding(DEFAULT_ENCODING)
    , m_offset(0)
    , m_posInFile(0)
    , m_posInBuffer(0)
    , m_nInBuffer(0)
{
    m_buffer.reset(new char[TEXTFILE_BUFFER_SIZE]);
    m_wbuffer.reset(new WCHAR[TEXTFILE_BUFFER_SIZE]);
}

bool CTextFile::OpenStream(LPCTSTR lpszFileName, StreamMode mode)
{
    static const wchar_t* const modes[] = { L"rb", L"rt", L"wb", L"wt" };
    // like MFC: files that are read are shared with everybody, files that are written with readers
    m_pStream = OpenFile(lpszFileName, modes[mode], mode == READ_BINARY || mode == READ_TEXT ? _SH_DENYNO : _SH_DENYWR);
    if (!m_pStream) {
        m_strFileName.clear();
        return false;
    }
    m_strFileName = lpszFileName;
    return true;
}

void CTextFile::CloseStream()
{
    m_pStream.reset();
}

void CTextFile::Flush()
{
    if (m_pStream) {
        fflush(m_pStream.get());
    }
}

void CTextFile::Close()
{
    CloseStream();
}

UINT CTextFile::StreamRead(void* lpBuf, UINT nCount)
{
    if (nCount == 0) {
        return 0;
    }
    const UINT nRead = static_cast<UINT>(fread(lpBuf, 1, nCount, m_pStream.get()));
    if (ferror(m_pStream.get())) {
        clearerr(m_pStream.get());
        throw std::ios_base::failure("CTextFile: read error");
    }
    return nRead;
}

void CTextFile::StreamWrite(const void* lpBuf, UINT nCount)
{
    if (nCount && fwrite(lpBuf, 1, nCount, m_pStream.get()) != nCount) {
        throw std::ios_base::failure("CTextFile: write error");
    }
}

ULONGLONG CTextFile::StreamSeek(LONGLONG lOff, UINT nFrom)
{
    if (_fseeki64(m_pStream.get(), lOff, static_cast<int>(nFrom)) != 0) {
        throw std::ios_base::failure("CTextFile: seek error");
    }
    return static_cast<ULONGLONG>(_ftelli64(m_pStream.get()));
}

ULONGLONG CTextFile::StreamPosition() const
{
    const long long position = _ftelli64(m_pStream.get());
    if (position == -1) {
        throw std::ios_base::failure("CTextFile: position error");
    }
    return static_cast<ULONGLONG>(position);
}

ULONGLONG CTextFile::StreamLength() const
{
    const long long position = _ftelli64(m_pStream.get());
    if (position == -1 || _fseeki64(m_pStream.get(), 0, SEEK_END) != 0) {
        throw std::ios_base::failure("CTextFile: length error");
    }
    const long long length = _ftelli64(m_pStream.get());
    if (length == -1 || _fseeki64(m_pStream.get(), position, SEEK_SET) != 0) {
        throw std::ios_base::failure("CTextFile: length error");
    }
    return static_cast<ULONGLONG>(length);
}

// One line of the text, without the line end; FALSE at the end of the file
BOOL CTextFile::StreamReadLine(tstring& str)
{
    str.clear();
    TCHAR buff[128];
    bool any = false;
    for (;;) {
        if (!_fgetts(buff, 128, m_pStream.get())) {
            if (!feof(m_pStream.get())) {
                clearerr(m_pStream.get());
                throw std::ios_base::failure("CTextFile: read error");
            }
            break;
        }
        any = true;
        const int n = static_cast<int>(_tcslen(buff));
        if (n > 0 && buff[n - 1] == _T('\n')) {
            buff[n - 1] = 0;
            str += buff;
            return TRUE;
        }
        str += buff;
    }
    return any;
}

void CTextFile::StreamWriteString(LPCTSTR lpsz)
{
    if (_fputts(lpsz, m_pStream.get()) == _TEOF) {
        throw std::ios_base::failure("CTextFile: write error");
    }
}

bool CTextFile::Open(LPCTSTR lpszFileName)
{
    if (!OpenStream(lpszFileName, READ_BINARY)) {
        return false;
    }

    m_encoding = m_defaultencoding;
    m_offset = 0;
    m_nInBuffer = m_posInBuffer = 0;

    try {
        if (StreamLength() >= 2) {
            WORD w;
            if (sizeof(w) != StreamRead(&w, sizeof(w))) {
                return CloseStream(), false;
            }

            if (w == 0xfeff) {
                m_encoding = LE16;
                m_offset = 2;
            } else if (w == 0xfffe) {
                m_encoding = BE16;
                m_offset = 2;
            } else if (w == 0xbbef && StreamLength() >= 3) {
                BYTE b;
                if (sizeof(b) != StreamRead(&b, sizeof(b))) {
                    return CloseStream(), false;
                }

                if (b == 0xbf) {
                    m_encoding = UTF8;
                    m_offset = 3;
                }
            }
        }

        if (m_encoding == DEFAULT_ENCODING) {
            if (!ReopenAsText()) {
                return false;
            }
        } else if (m_offset == 0) { // No BOM detected, ensure the file is read from the beginning
            Seek(0, begin);
        } else {
            m_posInFile = StreamPosition();
        }
    } catch (const std::ios_base::failure&) {
        return false;
    }

    return true;
}

bool CTextFile::ReopenAsText()
{
    tstring strFileName = m_strFileName;

    CloseStream(); // CWebTextFile::Close() would delete the temp file if we called it...

    return !!OpenStream(strFileName.c_str(), READ_TEXT);
}

bool CTextFile::Save(LPCTSTR lpszFileName, enc e)
{
    if (!OpenStream(lpszFileName, e == DEFAULT_ENCODING ? WRITE_TEXT : WRITE_BINARY)) {
        return false;
    }

    if (e == UTF8) {
        BYTE b[3] = {0xef, 0xbb, 0xbf};
        StreamWrite(b, sizeof(b));
    } else if (e == LE16) {
        BYTE b[2] = {0xff, 0xfe};
        StreamWrite(b, sizeof(b));
    } else if (e == BE16) {
        BYTE b[2] = {0xfe, 0xff};
        StreamWrite(b, sizeof(b));
    }

    m_encoding = e;

    return true;
}

void CTextFile::SetEncoding(enc e)
{
    m_encoding = e;
}

void CTextFile::SetFallbackEncoding(enc e)
{
    m_fallbackencoding = e;
}

CTextFile::enc CTextFile::GetEncoding()
{
    return m_encoding;
}

bool CTextFile::IsUnicode()
{
    return m_encoding == UTF8 || m_encoding == LE16 || m_encoding == BE16;
}

tstring CTextFile::GetFilePath() const
{
    return m_strFileName;
}

ULONGLONG CTextFile::GetPosition() const
{
    return (StreamPosition() - m_offset - (m_nInBuffer - m_posInBuffer));
}

ULONGLONG CTextFile::GetLength() const
{
    return (StreamLength() - m_offset);
}

ULONGLONG CTextFile::Seek(LONGLONG lOff, UINT nFrom)
{
    ULONGLONG newPos;

    // Try to reuse the buffer if any
    if (m_nInBuffer > 0) {
        ULONGLONG pos = GetPosition();
        ULONGLONG len = GetLength();

        switch (nFrom) {
            default:
            case begin:
                break;
            case current:
                lOff = pos + lOff;
                break;
            case end:
                lOff = len - lOff;
                break;
        }

        lOff = std::max((LONGLONG)std::min((ULONGLONG)lOff, len), 0ll);

        m_posInBuffer += LONGLONG(ULONGLONG(lOff) - pos);
        if (m_posInBuffer < 0 || m_posInBuffer >= m_nInBuffer) {
            // If we would have to end up out of the buffer, we just reset it and seek normally
            m_nInBuffer = m_posInBuffer = 0;
            newPos = StreamSeek(lOff + m_offset, begin) - m_offset;
        } else { // If we can reuse the buffer, we have nothing special to do
            newPos = ULONGLONG(lOff);
        }
    } else { // No buffer, we can use the base implementation
        if (nFrom == begin) {
            lOff += m_offset;
        }
        newPos = StreamSeek(lOff, nFrom) - m_offset;
    }

    m_posInFile = newPos + m_offset + (m_nInBuffer - m_posInBuffer);

    return newPos;
}

void CTextFile::WriteString(LPCSTR lpsz/*std::string str*/)
{
    std::string str(lpsz);

    if (m_encoding == DEFAULT_ENCODING) {
        StreamWriteString(AToT(str).c_str());
    } else if (m_encoding == ANSI) {
        StringReplace(str, "\n", "\r\n");
        StreamWrite(str.c_str(), int(str.size()));
    } else if (m_encoding == UTF8) {
        WriteString(AToW(str));
    } else if (m_encoding == LE16) {
        WriteString(AToW(str));
    } else if (m_encoding == BE16) {
        WriteString(AToW(str));
    }
}

std::string ConvertUnicodeToUTF8(const std::wstring& input)
{
    if (input.empty()) {
        return "";
    }
    std::string utf8;
    int cc = 0;
    if ((cc = WideCharToMultiByte(CP_UTF8, 0, input.c_str(), -1, NULL, 0, 0, 0) - 1) > 0)
    {
        char* buf = StringBuffer(utf8, cc);
        if (buf) {
            WideCharToMultiByte(CP_UTF8, 0, input.c_str(), -1, buf, cc, 0, 0);
        }
        StringRelease(utf8);
    }
    return utf8;
}

void CTextFile::WriteString(LPCWSTR lpsz/*std::wstring str*/)
{
    std::wstring str(lpsz);

    if (m_encoding == DEFAULT_ENCODING) {
        StreamWriteString(WToT(str).c_str());
    } else if (m_encoding == ANSI) {
        StringReplace(str, L"\n", L"\r\n");
        std::string stra = StringNarrow(str.c_str()); // TODO: codepage
        StreamWrite(stra.c_str(), int(stra.size()));
    } else if (m_encoding == UTF8) {
        StringReplace(str, L"\n", L"\r\n");
        std::string utf8data = ConvertUnicodeToUTF8(str);
        StreamWrite(utf8data.c_str(), int(utf8data.size()));
    } else if (m_encoding == LE16) {
        StringReplace(str, L"\n", L"\r\n");
        StreamWrite(str.c_str(), int(str.size()) * 2);
    } else if (m_encoding == BE16) {
        StringReplace(str, L"\n", L"\r\n");
        for (unsigned int i = 0, l = int(str.size()); i < l; i++) {
            str[i] = ((str[i] >> 8) & 0x00ff) | ((str[i] << 8) & 0xff00);
        }
        StreamWrite(str.c_str(), int(str.size()) * 2);
    }
}

bool CTextFile::FillBuffer()
{
    if (m_posInBuffer < m_nInBuffer) {
        m_nInBuffer -= m_posInBuffer;
        memcpy(m_buffer.get(), &m_buffer[m_posInBuffer], (size_t)m_nInBuffer * sizeof(char));
    } else {
        m_nInBuffer = 0;
    }
    m_posInBuffer = 0;

    UINT nBytesRead;
    try {
        nBytesRead = StreamRead(&m_buffer[m_nInBuffer], UINT(TEXTFILE_BUFFER_SIZE - m_nInBuffer) * sizeof(char));
    } catch (...) {
        return true; // signal EOF in case of exception
    }
    if (nBytesRead) {
        m_nInBuffer += nBytesRead;
    }

    // Workaround for buggy text files that contain a duplicate UTF BOM
    if (m_posInFile == m_offset && m_offset >= 2 && m_nInBuffer > 3) {
        if (m_buffer[0] == '\xEF' && m_buffer[1] == '\xBB' && m_buffer[2] == '\xBF') {
            m_posInBuffer = 3;
        } else if (m_buffer[0] == '\xFE' && m_buffer[1] == '\xFF' || m_buffer[0] == '\xFF' && m_buffer[1] == '\xEF') {
            m_posInBuffer = 2;
        }
    }

    m_posInFile = StreamPosition();

    return !nBytesRead;
}

ULONGLONG CTextFile::GetPositionFastBuffered() const
{
    return (m_posInFile - m_offset - (m_nInBuffer - m_posInBuffer));
}

BOOL CTextFile::ReadString(std::string& str)
{
    bool fEOF = true;

    StringTruncate(str, 0);

    if (m_encoding == DEFAULT_ENCODING) {
        tstring s;
        fEOF = !StreamReadLine(s);
        str = TToA(s);
        // For consistency with other encodings, we continue reading
        // the file even when a NUL char is encountered.
        char c;
        while (fEOF && (StreamRead(&c, sizeof(c)) == sizeof(c))) {
            str += c;
            fEOF = !StreamReadLine(s);
            str += TToA(s);
        }
    } else if (m_encoding == ANSI) {
        bool bLineEndFound = false;
        fEOF = false;

        do {
            int nCharsRead;

            for (nCharsRead = 0; m_posInBuffer + nCharsRead < m_nInBuffer; nCharsRead++) {
                if (m_buffer[m_posInBuffer + nCharsRead] == '\n') {
                    break;
                } else if (m_buffer[m_posInBuffer + nCharsRead] == '\r') {
                    break;
                }
            }

            str.append(&m_buffer[m_posInBuffer], nCharsRead);

            m_posInBuffer += nCharsRead;
            while (m_posInBuffer < m_nInBuffer && m_buffer[m_posInBuffer] == '\r') {
                m_posInBuffer++;
            }
            if (m_posInBuffer < m_nInBuffer && m_buffer[m_posInBuffer] == '\n') {
                bLineEndFound = true; // Stop at end of line
                m_posInBuffer++;
            }

            if (!bLineEndFound) {
                bLineEndFound = FillBuffer();
                if (!nCharsRead) {
                    fEOF = bLineEndFound;
                }
            }
        } while (!bLineEndFound);
    } else if (m_encoding == UTF8) {
        ULONGLONG lineStartPos = GetPositionFastBuffered();
        bool bValid = true;
        bool bLineEndFound = false;
        fEOF = false;

        do {
            int nCharsRead;
            char* abuffer = (char*)(WCHAR*)m_wbuffer.get();

            for (nCharsRead = 0; m_posInBuffer < m_nInBuffer; m_posInBuffer++, nCharsRead++) {
                if (Utf8::isSingleByte(m_buffer[m_posInBuffer])) { // 0xxxxxxx
                    abuffer[nCharsRead] = m_buffer[m_posInBuffer] & 0x7f;
                } else if (Utf8::isFirstOfMultibyte(m_buffer[m_posInBuffer])) {
                    int nContinuationBytes = Utf8::continuationBytes(m_buffer[m_posInBuffer]);
                    ASSERT(bValid);

                    if (m_posInBuffer + nContinuationBytes >= m_nInBuffer) {
                        // If we are at the end of the file, the buffer won't be full
                        // and we won't be able to read any more continuation bytes.
                        bValid = (m_nInBuffer == TEXTFILE_BUFFER_SIZE);
                        break;
                    } else {
                        for (int j = 1; j <= nContinuationBytes; j++) {
                            if (!Utf8::isContinuation(m_buffer[m_posInBuffer + j])) {
                                bValid = false;
                            }
                        }

                        switch (nContinuationBytes) {
                            case 0: // 0xxxxxxx
                                abuffer[nCharsRead] = m_buffer[m_posInBuffer] & 0x7f;
                                break;
                            case 1: // 110xxxxx 10xxxxxx
                            case 2: // 1110xxxx 10xxxxxx 10xxxxxx
                            case 3: // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
                                // Unsupported for non unicode strings
                                abuffer[nCharsRead] = '?';
                                break;
                        }
                        m_posInBuffer += nContinuationBytes;
                    }
                } else {
                    bValid = false;
                    TRACE(_T("Invalid UTF8 character found\n"));
                }

                if (!bValid) {
                    abuffer[nCharsRead] = '?';
                    m_posInBuffer++;
                    nCharsRead++;
                    break;
                } else if (abuffer[nCharsRead] == '\n') {
                    bLineEndFound = true; // Stop at end of line
                    m_posInBuffer++;
                    break;
                } else if (abuffer[nCharsRead] == '\r') {
                    nCharsRead--; // Skip \r
                }
            }

            if (bValid || m_offset) {
                str.append(abuffer, nCharsRead);

                if (!bLineEndFound) {
                    bLineEndFound = FillBuffer();
                    if (!nCharsRead) {
                        fEOF = bLineEndFound;
                    }
                }
            } else {
                // Switch to text and read again
                m_encoding = m_fallbackencoding;
                // Stop using the buffer
                m_posInBuffer = m_nInBuffer = 0;

                fEOF = !ReopenAsText();

                if (!fEOF) {
                    // Seek back to the beginning of the line where we stopped
                    Seek(lineStartPos, begin);

                    fEOF = !ReadString(str);
                }
            }
        } while (bValid && !bLineEndFound);
    } else if (m_encoding == LE16) {
        bool bLineEndFound = false;
        fEOF = false;

        do {
            int nCharsRead;
            WCHAR* wbuffer = (WCHAR*)&m_buffer[m_posInBuffer];
            char* abuffer = (char*)(WCHAR*)m_wbuffer.get();

            for (nCharsRead = 0; m_posInBuffer + 1 < m_nInBuffer; nCharsRead++, m_posInBuffer += sizeof(WCHAR)) {
                if (wbuffer[nCharsRead] == L'\n') {
                    break; // Stop at end of line
                } else if (wbuffer[nCharsRead] == L'\r') {
                    break; // Skip \r
                } else if (!(wbuffer[nCharsRead] & 0xff00)) {
                    abuffer[nCharsRead] = char(wbuffer[nCharsRead] & 0xff);
                } else {
                    abuffer[nCharsRead] = '?';
                }
            }

            str.append(abuffer, nCharsRead);

            while (m_posInBuffer + 1 < m_nInBuffer && wbuffer[nCharsRead] == L'\r') {
                nCharsRead++;
                m_posInBuffer += sizeof(WCHAR);
            }
            if (m_posInBuffer + 1 < m_nInBuffer && wbuffer[nCharsRead] == L'\n') {
                bLineEndFound = true; // Stop at end of line
                nCharsRead++;
                m_posInBuffer += sizeof(WCHAR);
            }

            if (!bLineEndFound) {
                bLineEndFound = FillBuffer();
                if (!nCharsRead) {
                    fEOF = bLineEndFound;
                }
            }
        } while (!bLineEndFound);
    } else if (m_encoding == BE16) {
        bool bLineEndFound = false;
        fEOF = false;

        do {
            int nCharsRead;
            char* abuffer = (char*)(WCHAR*)m_wbuffer.get();

            for (nCharsRead = 0; m_posInBuffer + 1 < m_nInBuffer; nCharsRead++, m_posInBuffer += sizeof(WCHAR)) {
                if (!m_buffer[m_posInBuffer]) {
                    abuffer[nCharsRead] = m_buffer[m_posInBuffer + 1];
                } else {
                    abuffer[nCharsRead] = '?';
                }

                if (abuffer[nCharsRead] == '\n') {
                    bLineEndFound = true; // Stop at end of line
                    m_posInBuffer += sizeof(WCHAR);
                    break;
                } else if (abuffer[nCharsRead] == L'\r') {
                    nCharsRead--; // Skip \r
                }
            }

            str.append(abuffer, nCharsRead);

            if (!bLineEndFound) {
                bLineEndFound = FillBuffer();
                if (!nCharsRead) {
                    fEOF = bLineEndFound;
                }
            }
        } while (!bLineEndFound);
    }

    return !fEOF;
}

BOOL CTextFile::ReadString(std::wstring& str)
{
    bool fEOF = true;

    StringTruncate(str, 0);

    if (m_encoding == DEFAULT_ENCODING) {
        tstring s;
        fEOF = !StreamReadLine(s);
        str = TToW(s);
        // For consistency with other encodings, we continue reading
        // the file even when a NUL char is encountered.
        char c;
        while (fEOF && (StreamRead(&c, sizeof(c)) == sizeof(c))) {
            str += c;
            fEOF = !StreamReadLine(s);
            str += TToW(s);
        }
    } else if (m_encoding == ANSI) {
        bool bLineEndFound = false;
        fEOF = false;

        do {
            int nCharsRead;

            for (nCharsRead = 0; m_posInBuffer + nCharsRead < m_nInBuffer; nCharsRead++) {
                if (m_buffer[m_posInBuffer + nCharsRead] == '\n') {
                    break;
                } else if (m_buffer[m_posInBuffer + nCharsRead] == '\r') {
                    break;
                }
            }

            // TODO: codepage
            str.append(StringWiden(std::string(&m_buffer[m_posInBuffer], nCharsRead).c_str()));

            m_posInBuffer += nCharsRead;
            while (m_posInBuffer < m_nInBuffer && m_buffer[m_posInBuffer] == '\r') {
                m_posInBuffer++;
            }
            if (m_posInBuffer < m_nInBuffer && m_buffer[m_posInBuffer] == '\n') {
                bLineEndFound = true; // Stop at end of line
                m_posInBuffer++;
            }

            if (!bLineEndFound) {
                bLineEndFound = FillBuffer();
                if (!nCharsRead) {
                    fEOF = bLineEndFound;
                }
            }
        } while (!bLineEndFound);
    } else if (m_encoding == UTF8) {
        ULONGLONG lineStartPos = GetPositionFastBuffered();
        bool bValid = true;
        bool bLineEndFound = false;
        fEOF = false;

        do {
            int nCharsRead;

            for (nCharsRead = 0; m_posInBuffer < m_nInBuffer; m_posInBuffer++, nCharsRead++) {
                if (Utf8::isSingleByte(m_buffer[m_posInBuffer])) { // 0xxxxxxx
                    m_wbuffer[nCharsRead] = m_buffer[m_posInBuffer] & 0x7f;
                } else if (Utf8::isFirstOfMultibyte(m_buffer[m_posInBuffer])) {
                    int nContinuationBytes = Utf8::continuationBytes(m_buffer[m_posInBuffer]);
                    if (m_posInBuffer + nContinuationBytes >= m_nInBuffer) {
                        // If we are at the end of the file, the buffer won't be full
                        // and we won't be able to read any more continuation bytes.
                        bValid = (m_nInBuffer == TEXTFILE_BUFFER_SIZE);
                        break;
                    } else {
                        for (int j = 1; j <= nContinuationBytes; j++) {
                            if (!Utf8::isContinuation(m_buffer[m_posInBuffer + j])) { //maybe redundant since MultiByteToWideChar should return zero if bad utf-8?
                                bValid = false;
                            }
                        }
                        if (bValid) {
                            int nWchars = MultiByteToWideChar(CP_UTF8, 0, &m_buffer[m_posInBuffer], nContinuationBytes + 1, nullptr, 0);
                            if (nWchars > 0) {
                                MultiByteToWideChar(CP_UTF8, 0, &m_buffer[m_posInBuffer], nContinuationBytes + 1, &m_wbuffer[nCharsRead], nWchars);
                                nCharsRead += nWchars - 1; //subtract one for loop increment
                            } else {
                                bValid = false;
                            }
                        }
                        m_posInBuffer += nContinuationBytes;
                    }
                } else {
                    bValid = false;
                    TRACE(_T("Invalid UTF8 character found\n"));
                }

                if (!bValid) {
                    m_wbuffer[nCharsRead] = L'?';
                    m_posInBuffer++;
                    nCharsRead++;
                    break;
                } else if (m_wbuffer[nCharsRead] == L'\n') {
                    bLineEndFound = true; // Stop at end of line
                    m_posInBuffer++;
                    break;
                } else if (m_wbuffer[nCharsRead] == L'\r') {
                    // check if it is followed by a '\n'
                    if (m_posInBuffer + 1 >= m_nInBuffer) {
                        bLineEndFound = FillBuffer();
                    }
                    if (!bLineEndFound && Utf8::isSingleByte(m_buffer[m_posInBuffer+1]) && ((m_buffer[m_posInBuffer+1] & 0x7f) == L'\n')) {
                        nCharsRead--; // Skip '\r'
                    } else {
                        // Add the missing '\n'
                        nCharsRead++;
                        m_wbuffer[nCharsRead] = L'\n';
                        bLineEndFound = true;
                        m_posInBuffer++;
                        break;
                    }
                }
            }

            if (bValid || m_offset) {
                if (nCharsRead > 0) {
                    str.append(m_wbuffer.get(), nCharsRead);
                }
                if (!bLineEndFound) {
                    bLineEndFound = FillBuffer();
                    if (!nCharsRead) {
                        fEOF = bLineEndFound;
                    }
                }
            } else {
                // Switch to text and read again
                m_encoding = m_fallbackencoding;
                // Stop using the buffer
                m_posInBuffer = m_nInBuffer = 0;

                fEOF = !ReopenAsText();

                if (!fEOF) {
                    // Seek back to the beginning of the line where we stopped
                    Seek(lineStartPos, begin);

                    fEOF = !ReadString(str);
                }
            }
        } while (bValid && !bLineEndFound);
    } else if (m_encoding == LE16) {
        bool bLineEndFound = false;
        fEOF = false;

        do {
            int nCharsRead;
            WCHAR* wbuffer = (WCHAR*)&m_buffer[m_posInBuffer];

            for (nCharsRead = 0; m_posInBuffer + 1 < m_nInBuffer; nCharsRead++, m_posInBuffer += sizeof(WCHAR)) {
                if (wbuffer[nCharsRead] == L'\n') {
                    break; // Stop at end of line
                } else if (wbuffer[nCharsRead] == L'\r') {
                    break; // Skip \r
                }
            }

            str.append(wbuffer, nCharsRead);

            while (m_posInBuffer + 1 < m_nInBuffer && wbuffer[nCharsRead] == L'\r') {
                nCharsRead++;
                m_posInBuffer += sizeof(WCHAR);
            }
            if (m_posInBuffer + 1 < m_nInBuffer && wbuffer[nCharsRead] == L'\n') {
                bLineEndFound = true; // Stop at end of line
                nCharsRead++;
                m_posInBuffer += sizeof(WCHAR);
            }

            if (!bLineEndFound) {
                bLineEndFound = FillBuffer();
                if (!nCharsRead) {
                    fEOF = bLineEndFound;
                }
            }
        } while (!bLineEndFound);
    } else if (m_encoding == BE16) {
        bool bLineEndFound = false;
        fEOF = false;

        do {
            int nCharsRead;

            for (nCharsRead = 0; m_posInBuffer + 1 < m_nInBuffer; nCharsRead++, m_posInBuffer += sizeof(WCHAR)) {
                m_wbuffer[nCharsRead] = ((WCHAR(m_buffer[m_posInBuffer]) << 8) & 0xff00) | (WCHAR(m_buffer[m_posInBuffer + 1]) & 0x00ff);
                if (m_wbuffer[nCharsRead] == L'\n') {
                    bLineEndFound = true; // Stop at end of line
                    m_posInBuffer += sizeof(WCHAR);
                    break;
                } else if (m_wbuffer[nCharsRead] == L'\r') {
                    nCharsRead--; // Skip \r
                }
            }

            str.append(m_wbuffer.get(), nCharsRead);

            if (!bLineEndFound) {
                bLineEndFound = FillBuffer();
                if (!nCharsRead) {
                    fEOF = bLineEndFound;
                }
            }
        } while (!bLineEndFound);
    }

    return !fEOF;
}

//
// CWebTextFile
//

CWebTextFile::CWebTextFile(CTextFile::enc e, LONGLONG llMaxSize)
    : CTextFile(e)
    , m_llMaxSize(llMaxSize)
{
}

bool CWebTextFile::Open(LPCTSTR lpszFileName)
{
    tstring fn(lpszFileName);

    if (StringFind(fn, _T("://")) == -1) {
        return CTextFile::Open(lpszFileName);
    }

    TCHAR path[MAX_PATH];
    GetTempPath(MAX_PATH, path);

    fn = path + StringMid(fn, StringRFind(fn, '/') + 1);
    int i = StringFind(fn, _T("?"));
    if (i > 0) {
        fn = StringLeft(fn, i);
    }

    if (FAILED(URLDownloadToFile(nullptr, lpszFileName, fn.c_str(), 0, nullptr))) {
        return false;
    }

    if (m_llMaxSize > 0) {
        // the download is not stopped at the maximum size: the file is cut
        std::error_code ec;
        if (std::filesystem::file_size(fn.c_str(), ec) > ULONGLONG(m_llMaxSize)) {
            std::filesystem::resize_file(fn.c_str(), ULONGLONG(m_llMaxSize), ec);
        }
    }

    m_tempfn = fn;

    return CTextFile::Open(m_tempfn.c_str());
}

bool CWebTextFile::Save(LPCTSTR lpszFileName, enc e)
{
    // CWebTextFile is read-only...
    ASSERT(0);
    return false;
}

void CWebTextFile::Close()
{
    CTextFile::Close();

    if (!m_tempfn.empty()) {
        _tremove(m_tempfn.c_str());
        m_tempfn.clear();
    }
}

///////////////////////////////////////////////////////////////

std::wstring AToW(std::string str)
{
    std::wstring ret;
    for (int i = 0, j = int(str.size()); i < j; i++) {
        ret += (WCHAR)(BYTE)str[i];
    }
    return ret;
}

std::string WToA(std::wstring str)
{
    std::string ret;
    for (int i = 0, j = int(str.size()); i < j; i++) {
        ret += (CHAR)(WORD)str[i];
    }
    return ret;
}

tstring AToT(std::string str)
{
#ifdef UNICODE
    tstring ret;
    for (int i = 0, j = int(str.size()); i < j; i++) {
        ret += (TCHAR)(BYTE)str[i];
    }
    return ret;
#else
    return str;
#endif
}

tstring WToT(std::wstring str)
{
#ifdef UNICODE
    return str;
#else
    tstring ret;
    for (int i = 0, j = str.GetLength(); i < j; i++) {
        ret += (TCHAR)(WORD)str[i];
    }
    return ret;
#endif
}

std::string TToA(tstring str)
{
#ifdef UNICODE
    std::string ret;
    for (int i = 0, j = int(str.size()); i < j; i++) {
        ret += (CHAR)(BYTE)str[i];
    }
    return ret;
#else
    return str;
#endif
}

std::wstring TToW(tstring str)
{
#ifdef UNICODE
    return str;
#else
    std::wstring ret;
    for (size_t i = 0, j = str.GetLength(); i < j; i++) {
        ret += (WCHAR)(BYTE)str[i];
    }
    return ret;
#endif
}
