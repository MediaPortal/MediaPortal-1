/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2017 see Authors.txt
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
#include <filesystem>
#include <algorithm>
#include "VobSubFile.h"
#include "VobSubFileRipper.h"
#include "../DeCSS/VobDec.h"
#include "CCDecoder.h"

//
// CVobSubFileRipper
//

CVobSubFileRipper::CVobSubFileRipper()
    : CVobSubFile(nullptr)
    , m_bThreadActive(false)
    , m_bBreakThread(false)
    , m_bIndexing(false)
{
    m_rd.Reset();
    CAMThread::Create();
}

CVobSubFileRipper::~CVobSubFileRipper()
{
    CAMThread::CallWorker(CMD_EXIT);
    CAMThread::Close();
}

STDMETHODIMP CVobSubFileRipper::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
    return
        QI(IVSFRipper)
        __super::NonDelegatingQueryInterface(riid, ppv);
}

void CVobSubFileRipper::Log(log_t type, LPCTSTR lpszFormat, ...)
{
    CAutoLock cAutoLock(&m_csCallback);
    if (!m_pCallback) {
        return;
    }

    TCHAR buff[1024];

    va_list args;
    va_start(args, lpszFormat);
    _vstprintf_s(buff, _countof(buff), lpszFormat, args);
    va_end(args);

    tstring msg;
    switch (type) {
        default:
        case LOG_INFO:
            msg = _T("");
            break;
        case LOG_WARNING:
            msg = _T("WARNING: ");
            break;
        case LOG_ERROR:
            msg = _T("ERROR: ");
            break;
    }

    msg += buff;

    m_pCallback->OnMessage(msg.c_str());
}

void CVobSubFileRipper::Progress(double progress)
{
    CAutoLock cAutoLock(&m_csCallback);
    if (!m_pCallback) {
        return;
    }

    m_pCallback->OnProgress(progress);
}

void CVobSubFileRipper::Finished(bool bSucceeded)
{
    CAutoLock cAutoLock(&m_csCallback);
    if (!m_pCallback) {
        return;
    }

    m_pCallback->OnFinished(bSucceeded);
}

bool CVobSubFileRipper::LoadIfo(tstring fn)
{
    std::error_code fileSizeError;
    if (std::filesystem::file_size(fn.c_str(), fileSizeError) == 0 || fileSizeError) {
        Log(LOG_ERROR, _T("Invalid ifo"));
        return false;
    }

    FilePtr f = OpenFile(fn, L"rb");
    if (!f) {
        Log(LOG_ERROR, _T("Cannot open ifo"));
        return false;
    }

    Log(LOG_INFO, _T("Opening ifo OK"));

    char hdr[13];
    fread(hdr, 1, 12, f.get());
    hdr[12] = 0;
    if (strcmp(hdr, "DVDVIDEO-VTS")) {
        Log(LOG_ERROR, _T("Not a Video Title Set IFO file!"));
        return false;
    }

    // lang ids

    _fseeki64(f.get(), 0x254, SEEK_SET);

    WORD ids[32];
    ZeroMemory(ids, sizeof(ids));

    int len = 0;
    ReadBEw(len);

    for (ptrdiff_t i = 0; i < len; i++) {
        _fseeki64(f.get(), 2, SEEK_CUR); // 01 00 ?
        ReadBEw(ids[i]);
        if (ids[i] == 0) {
            ids[i] = '--';
        }
        _fseeki64(f.get(), 2, SEEK_CUR); // 00 00 ?
    }

    /* Video info */

    _fseeki64(f.get(), 0x200, SEEK_SET);
    fread(&m_rd.vidinfo, 1, 2, f.get());

    SIZE res[4][2] = {
        {{720, 480}, {720, 576}},
        {{704, 480}, {704, 576}},
        {{352, 480}, {352, 576}},
        {{352, 240}, {352, 288}}
    };

    m_rd.vidsize = res[m_rd.vidinfo.source_res][m_rd.vidinfo.system & 1];

    double rate = (m_rd.vidinfo.system == 0) ? 30.0 / 29.97 : 1.0;

    /* PGCs */

    {
        DWORD offset;

        DWORD pgcpos;
        _fseeki64(f.get(), 0xc0 + 0x0c, SEEK_SET);
        ReadBEdw(pgcpos);
        pgcpos *= 0x800;

        WORD nPGC;
        _fseeki64(f.get(), pgcpos, SEEK_SET);
        ReadBEw(nPGC);

        m_rd.pgcs.clear();
        m_rd.pgcs.resize(nPGC);

        for (size_t i = 0; i < nPGC; i++) {
            PGC& pgc = m_rd.pgcs[i];

            _fseeki64(f.get(), pgcpos + 8 + i * 8 + 4, SEEK_SET);
            ReadBEdw(offset);
            offset += pgcpos;

            BYTE nProgs, nCells;
            _fseeki64(f.get(), offset + 2, SEEK_SET);
            ReadBEb(nProgs);
            ReadBEb(nCells);

            //

            memcpy(pgc.ids, ids, sizeof(ids));

            struct splanginfo {
                BYTE res1, id1, id2, res2;
            };
            splanginfo splinfo[32];

            _fseeki64(f.get(), offset + 0x1c, SEEK_SET);
            fread(splinfo, 1, 32 * 4, f.get());

            for (size_t j = 0; j < 32; j++) {
                if (splinfo[j].id1 || splinfo[i].id2) {

                    for (j = 0; j < 32; j++) {
                        if (!(splinfo[j].res1 & 0x80)) {
                            break;
                        }

                        pgc.ids[splinfo[j].id1] = ids[j];
                        pgc.ids[splinfo[j].id2] = ids[j];
                    }

                    break;
                }
            }

            //

            _fseeki64(f.get(), offset + 0xa4, SEEK_SET);

            for (size_t j = 0; j < 16; j++) {
                BYTE y, u, v, tmp;

                fread(&tmp, 1, 1, f.get());
                fread(&y, 1, 1, f.get());
                fread(&u, 1, 1, f.get());
                fread(&v, 1, 1, f.get());

                y = (y - 16) * 255 / 219;

                pgc.pal[j].rgbRed = std::min<BYTE>(std::max<BYTE>(BYTE(1.0 * y + 1.4022 * (u - 128)), 0u), 255u);
                pgc.pal[j].rgbGreen = std::min<BYTE>(std::max<BYTE>(BYTE(1.0 * y - 0.3456 * (u - 128) - 0.7145 * (v - 128)), 0u), 255u);
                pgc.pal[j].rgbBlue = std::min<BYTE>(std::max<BYTE>(BYTE(1.0 * y + 1.7710 * (v - 128)), 0u), 255u);
            }

            //

            WORD progoff, celladdroff, vobcelloff;
            _fseeki64(f.get(), offset + 0xe6, SEEK_SET);
            ReadBEw(progoff);
            _fseeki64(f.get(), offset + 0xe8, SEEK_SET);
            ReadBEw(celladdroff);
            _fseeki64(f.get(), offset + 0xea, SEEK_SET);
            ReadBEw(vobcelloff);

            //

            std::vector<BYTE> progs;
            progs.resize(nProgs);
            _fseeki64(f.get(), offset + progoff, SEEK_SET);
            fread(progs.data(), 1, nProgs, f.get());

            //

            pgc.angles[0].resize(nCells);
            pgc.iSelAngle = 0;

            //

            _fseeki64(f.get(), offset + vobcelloff, SEEK_SET);
            for (size_t j = 0; j < nCells; j++) {
                ReadBEw(pgc.angles[0][j].vob);
                ReadBEw(pgc.angles[0][j].cell);
            }

            //

            DWORD tOffset = 0, tTotal = 0;

            int iAngle = 0;

            pgc.nAngles = 0;

            _fseeki64(f.get(), offset + celladdroff, SEEK_SET);
            for (size_t j = 0; j < nCells; j++) {
                BYTE b;
                ReadBEb(b);
                switch (b >> 6) {
                    case 0:
                        iAngle = 0;
                        break; // normal
                    case 1:
                        iAngle = 1;
                        break; // first angle block
                    case 2:
                        iAngle++;
                        break; // middle angle block
                    case 3:
                        iAngle++;
                        break; // last angle block (no more should follow)
                }
                pgc.angles[0][j].iAngle = iAngle;
                pgc.nAngles = std::max(pgc.nAngles, iAngle);

                _fseeki64(f.get(), 3, SEEK_CUR);
                ReadBEdw(pgc.angles[0][j].tTime);
                ReadBEdw(pgc.angles[0][j].start);
                _fseeki64(f.get(), 8, SEEK_CUR);
                ReadBEdw(pgc.angles[0][j].end);

                float fps;
                switch ((pgc.angles[0][j].tTime >> 6) & 0x3) {
                    default:
                    case 3:
                        fps = 30;
                        break;
                    case 1:
                        fps = 25;
                        break;
                }

                int t = pgc.angles[0][j].tTime;
                int hh = ((t >> 28) & 0xf) * 10 + ((t >> 24) & 0xf);
                int mm = ((t >> 20) & 0xf) * 10 + ((t >> 16) & 0xf);
                int ss = ((t >> 12) & 0xf) * 10 + ((t >> 8) & 0xf);
                int ms = (int)(1000.0 * (((t >> 4) & 0x3) * 10 + ((t >> 0) & 0xf)) / fps);
                pgc.angles[0][j].tTime = (DWORD)((((hh * 60 + mm) * 60 + ss) * 1000 + ms) * rate);

                // time discontinuity
                if (b & 0x02) {
                    tOffset = tTotal;
                }
                pgc.angles[0][j].bDiscontinuity = !!(b & 0x02);

                pgc.angles[0][j].tTotal = tTotal;
                pgc.angles[0][j].tOffset = tOffset;

                tTotal += pgc.angles[0][j].tTime;
            }

            for (iAngle = 1; iAngle <= 9; iAngle++) {
                tOffset = tTotal = 0;

                for (size_t j = 0, k = 0; j < nCells; j++) {
                    if (pgc.angles[0][j].iAngle != 0
                            && pgc.angles[0][j].iAngle != iAngle) {
                        continue;
                    }

                    pgc.angles[iAngle].push_back(pgc.angles[0][j]);

                    if (pgc.angles[iAngle][k].bDiscontinuity) {
                        tOffset = tTotal;
                    }

                    pgc.angles[iAngle][k].tTotal = tTotal;
                    pgc.angles[iAngle][k].tOffset = tOffset;

                    tTotal += pgc.angles[iAngle][k].tTime;

                    k++;
                }
            }
        }
    }

    Log(LOG_INFO, _T("Parsing ifo OK"));

    return true;
}

bool CVobSubFileRipper::LoadVob(tstring fn)
{
    Log(LOG_INFO, _T("Searching vobs..."));

    std::list<tstring> vobs;
    if (!m_vob.Open(fn, vobs/*m_vobs*/)) {
        Log(LOG_ERROR, _T("Cannot open vob sequence"));
        return false;
    }

    if (vobs.size() <= 0) {
        Log(LOG_ERROR, _T("Nothing found! (%s*.vob)"), fn.c_str());
        return false;
    }

    for (const tstring& vob : vobs) {
        Log(LOG_INFO, (_T("Found ") + vob).c_str());
    }

    if (m_vob.IsDVD()) {
        Log(LOG_INFO, _T("DVD detected..."));

        BYTE key[5];

        if (m_vob.HasDiscKey(key)) {
            Log(LOG_INFO, _T("Disc key: %02x%02x%02x%02x%02x"), key[0], key[1], key[2], key[3], key[4]);
        } else {
            Log(LOG_WARNING, _T("Couldn't get the disc key"));
        }

        if (m_vob.HasTitleKey(key)) {
            Log(LOG_INFO, _T("Title key: %02x%02x%02x%02x%02x"), key[0], key[1], key[2], key[3], key[4]);
        } else {
            Log(LOG_WARNING, _T("Couldn't get the title key"));
        }

        BYTE buff[2048];

        m_vob.Seek(0);
        if (!m_vob.Read(buff)) {
            Log(LOG_ERROR, _T("Can't read vob, please unlock it with a software player!"));
            return false;
        }
        m_vob.Seek(0);
    }

    return true;
}

#pragma warning(push)
#pragma warning(disable: 4702)
DWORD CVobSubFileRipper::ThreadProc()
{
    SetThreadPriority(m_hThread, THREAD_PRIORITY_BELOW_NORMAL);

    for (;;) {
        DWORD cmd = GetRequest();

        m_bThreadActive = true;

        switch (cmd) {
            case CMD_EXIT:
                Reply(S_OK);
                return 0;

            case CMD_INDEX:
                Reply(S_OK);
                {
                    m_bIndexing = true;
                    bool bSucceeded = Create();
                    m_bIndexing = false;
                    Finished(bSucceeded);
                }
                break;

            default:
                Reply((DWORD)E_FAIL);
                return DWORD_ERROR;
        }

        m_bBreakThread = false;
        m_bThreadActive = false;
    }
    UNREACHABLE_CODE(); // we should only exit via CMD_EXIT
#pragma warning(pop)
}

bool CVobSubFileRipper::Create()
{
    CAutoLock cAutoLock(&m_csAccessLock);

    if (m_rd.iSelPGC < 0 || (size_t)m_rd.iSelPGC >= m_rd.pgcs.size()) {
        Log(LOG_ERROR, _T("Invalid program chain number (%d)!"), m_rd.iSelPGC);
        return false;
    }

    PGC& pgc = m_rd.pgcs[m_rd.iSelPGC];

    if (pgc.iSelAngle < 0 || pgc.iSelAngle > 9 || pgc.angles[pgc.iSelAngle].empty()) {
        Log(LOG_ERROR, _T("Invalid angle number (%d)!"), pgc.iSelAngle);
        return false;
    }

    std::vector<vc_t>& angle = pgc.angles[pgc.iSelAngle];

    if (m_rd.selids.empty() && !m_rd.bClosedCaption) {
        Log(LOG_ERROR, _T("No valid stream set to be extacted!"));
        return false;
    }

    if (m_rd.selvcs.empty()) {
        Log(LOG_ERROR, _T("No valid vob/cell id set to be extacted!"));
        return false;
    }

    Log(LOG_INFO, _T("Indexing..."));

    // initalize CVobSubFile
    CVobSubFile::Close();
    InitSettings();
    m_title = m_outfn;
    m_size = m_rd.vidsize;
    TrimExtension(m_title);
    memcpy(m_orgpal, pgc.pal, sizeof(m_orgpal));
    m_sub.SetLength(0);

    CCDecoder ccdec(m_title + _T(".cc.srt"), m_title + _T(".cc.raw"));

    CVobDec vd;

    __int64 PTS = 0, tOffset = 0, tPrevOffset = 0, tTotal = 0, tStart = 0;
    int vob = 0, cell = 0;
    bool bDiscontinuity = false, bDiscontinuityFixApplied = false, bNavpackFound = false;

    int PTSframeoffset = 0, minPTSframeoffset = 0;

    if (m_rd.bResetTime) {
        for (size_t i = 0; i < angle.size() && (UINT)((angle[i].vob << 16) | angle[i].cell) != m_rd.selvcs[0]; i++) {
            tStart += angle[i].tTime;
        }

        Log(LOG_INFO, _T("Counting timestamps from %I64dms (v%02dc%02d)"),
            tStart, m_rd.selvcs[0] >> 16, m_rd.selvcs[0] & 0xffff);
    }

    std::map<DWORD, int> selvcmap;
    selvcmap.clear();
    for (size_t i = 0; i < m_rd.selvcs.size(); i++) {
        selvcmap[m_rd.selvcs[i]] = 90000;
    }

    std::vector<vcchunk> chunks, foundchunks, loadedchunks;

    if (m_vob.IsDVD()) {
        Log(LOG_INFO, _T("Indexing mode: DVD"));

        for (size_t i = 0; i < angle.size(); i++) {
            DWORD vc = (angle[i].vob << 16) | angle[i].cell;
            if (!(selvcmap.find(vc) != selvcmap.end())) {
                continue;
            }

            vcchunk c = {2048i64 * angle[i].start, 2048i64 * angle[i].end + 2048, vc};
            chunks.push_back(c);

            Log(LOG_INFO, _T("Adding: 0x%x - 0x%x (lba) for vob %d cell %d"),
                angle[i].start, angle[i].end, angle[i].vob, angle[i].cell);
        }
    } else if (LoadChunks(loadedchunks)) {
        Log(LOG_INFO, _T("Indexing mode: File"));

        for (size_t i = 0; i < loadedchunks.size(); i++) {
            DWORD vcid = loadedchunks[i].vc;
            if (!(selvcmap.find(vcid) != selvcmap.end())) {
                continue;
            }

            chunks.push_back(loadedchunks[i]);
        }

        Log(LOG_INFO, _T(".chunk file loaded"));
    } else {
        Log(LOG_INFO, _T("Indexing mode: File"));

        chunks.clear();
        vcchunk c = {0, 2048i64 * m_vob.GetLength(), 0};
        chunks.push_back(c);
    }

    __int64 sizedone = 0, sizetotal = 0;
    for (size_t i = 0; i < chunks.size(); i++) {
        sizetotal += chunks[i].end - chunks[i].start;
    }

    for (size_t i = 0; !m_bBreakThread && i < chunks.size(); i++) {
        __int64 curpos = chunks[i].start, endpos = chunks[i].end;

        vcchunk curchunk = {curpos, curpos, chunks[i].vc};

        for (m_vob.Seek((int)(curpos / 2048)); !m_bBreakThread && curpos < endpos; curpos += 2048, sizedone += 2048) {
            if (!(curpos & 0x7ffff)) {
                Progress(1.0 * sizedone / sizetotal);
            }

            static BYTE buff[2048];

            if (!m_vob.Read(buff)) {
                Log(LOG_ERROR, _T("Cannot read, either locked dvd or truncated/missing files!"));
                return false;
            }

            curchunk.end = curpos;

            if (buff[0x14] & 0x30) {
                if (!vd.m_fFoundKey) {
                    Log(LOG_INFO, _T("Encrypted sector found, searching key..."));

                    __int64 savepos = curpos;

                    m_vob.Seek(0);
                    for (__int64 pos = 0; !m_bBreakThread && pos < endpos; pos += 2048) {
                        if (!m_vob.Read(buff)) {
                            Log(LOG_ERROR, _T("Cannot read, either locked dvd or truncated/missing files!"));
                            return false;
                        }

                        if (vd.FindKey(buff)) {
                            break;
                        }
                    }

                    if (m_bBreakThread) {
                        break;
                    }

                    if (!vd.m_fFoundKey) {
                        Log(LOG_ERROR, _T("Key not found, can't decrypt!"));
                        return false;
                    }

                    Log(LOG_INFO, _T("Key found, continuing extraction..."));

                    m_vob.Seek((int)((curpos = savepos) / 2048));
                    m_vob.Read(buff);
                }

                vd.Decrypt(buff);
            }

            if (*((DWORD*)&buff[0]) != 0xba010000) {
                Log(LOG_WARNING, _T("Bad sector header at block %08d!"), (int)(curpos / 2048));

                if (MessageBox(GetActiveWindow(), _T("Bad packet header found, do you want to continue?"), nullptr, MB_YESNO) == IDNO) {
                    Log(LOG_ERROR, _T("Terminated!"));
                    return false;
                }
            }

            /*__int64 SCR = (__int64(buff[0x04] & 0x38) >> 3) << 30
                  | __int64(buff[0x04] & 0x03) << 28
                  | __int64(buff[0x05]) << 20
                  | (__int64(buff[0x06] & 0xf8) >> 3) << 15
                  | __int64(buff[0x06] & 0x03) << 13
                  | __int64(buff[0x07]) << 5
                  | (__int64(buff[0x08] & 0xf8) >> 3) << 0;*/

            bool hasPTS = false;

            if ((*(DWORD*)&buff[0x0e] == 0xe0010000 || *(DWORD*)&buff[0x0e] == 0xbd010000)
                    && buff[0x15] & 0x80) {
                PTS = (__int64)(buff[0x17] & 0x0e) << 29            // 32-30 (+marker)
                      | ((__int64)(buff[0x18]) << 22)               // 29-22
                      | ((__int64)(buff[0x19] & 0xfe) << 14)        // 21-15 (+marker)
                      | ((__int64)(buff[0x1a]) << 7)                // 14-07
                      | ((__int64)(buff[0x1b]) >> 1);               // 06-00 (+marker)

                hasPTS = true;
            }

            if (*((DWORD*)&buff[0x0e]) == 0xbb010000) {
                bNavpackFound = true;

                if (vob == buff[0x420] && cell == buff[0x422]) {
                    continue;
                }

                vob = buff[0x420];
                cell = buff[0x422];

                tOffset = tTotal = 0;

                for (size_t j = 0; j < angle.size(); j++) {
                    if (angle[j].vob == vob && angle[j].cell == cell) {
                        tPrevOffset = tOffset;
                        tOffset = (__int64)angle[j].tOffset;
                        tTotal = (__int64)angle[j].tTotal;
                        bDiscontinuity = angle[j].bDiscontinuity;
                        bDiscontinuityFixApplied = false;
                        break;
                    }
                }

                if (curchunk.vc != (DWORD)((vob << 16) | cell)) {
                    if (curchunk.vc != 0) {
                        foundchunks.push_back(curchunk);
                    }
                    curchunk.start = curchunk.end = curpos;
                    curchunk.vc = (vob << 16) | cell;
                }

                tstring str, str2;
                str = ::Format(_T("v%02d c%02d lba%08d"), vob, cell, (int)(curpos / 2048));
                UINT vcid = (vob << 16) | cell;
                if (!Lookup(selvcmap, vcid, minPTSframeoffset)) {
                    str2 = _T(", skipping");
                } else str2 = ::Format(_T(", total=%I64dms, off=%I64dms, corr=%I64dms, discont.:%d"), tTotal, tOffset, -tStart, (int)bDiscontinuity);
                Log(LOG_INFO, (str + str2).c_str());
            }

            DWORD vcid = (vob << 16) | cell;
            if (!Lookup(selvcmap, vcid, minPTSframeoffset)) {
                continue;
            }

            if (hasPTS && bDiscontinuity && !bDiscontinuityFixApplied) {
                __int64 tDiff = tOffset - tPrevOffset;
                if (tDiff > 0 && tDiff < (PTS / 90 + 1000)) {
                    tstring str;
                    str = ::Format(_T("False discontinuity detected, correcting time by %I64dms"), -tDiff);
                    Log(LOG_INFO, str.c_str());

                    tStart += tDiff;
                }
                bDiscontinuityFixApplied = true;
            }

            if (*(DWORD*)&buff[0x0e] == 0xe0010000) {
                if (bDiscontinuity) {
                    if (PTS < minPTSframeoffset) {
                        selvcmap[vcid] = PTSframeoffset = (int)PTS;
                    }

                    bDiscontinuity = false;
                }

                if (m_rd.bClosedCaption) {
                    ccdec.ExtractCC(buff, 2048, tOffset + ((PTS - PTSframeoffset) / 90) - tStart);
                }
            } else if (*(DWORD*)&buff[0x0e] == 0xbd010000) {
                BYTE id = buff[0x17 + buff[0x16]], iLang = id & 0x1f;

                if ((id & 0xe0) == 0x20 && (m_rd.selids.find(iLang) != m_rd.selids.end())) {
                    if (hasPTS) {
                        SubPos sb;
                        sb.filepos = m_sub.GetPosition();
                        sb.start = tOffset + ((PTS - PTSframeoffset) / 90) - tStart;
                        sb.vobid = (char)vob;
                        sb.cellid = (char)cell;
                        sb.celltimestamp = tTotal;
                        sb.bValid = true;
                        m_langs[iLang].subpos.push_back(sb);
                    }

                    m_sub.Write(buff, 2048);
                }
            }
        }

        if (curchunk.vc != (DWORD)((vob << 16) | cell)) {
            if (curchunk.vc != 0) {
                foundchunks.push_back(curchunk);
            }
            curchunk.start = curchunk.end = curpos;
            curchunk.vc = (vob << 16) | cell;
        }
    }

    if (sizedone < sizetotal) {
        Log(LOG_ERROR, _T("Indexing terminated before reaching the end!"));
        Progress(0);
        return false;
    }

    if (!bNavpackFound) {
        Log(LOG_ERROR, _T("Could not find any system header start code! (0x000001bb)"));
        if (!m_vob.IsDVD()) {
            Log(LOG_ERROR, _T("Make sure the ripper doesn't strip these packets."));
        }
        Progress(0);
        return false;
    }

    Log(LOG_INFO, _T("Indexing finished"));
    Progress(1);

    for (size_t i = 0; i < m_langs.size(); i++) {
        if (m_nLang == SIZE_T_ERROR && !m_langs[i].subpos.empty()) {
            m_nLang = i;
        }
        m_langs[i].id = pgc.ids[i];
        m_langs[i].name = m_langs[i].alt = FindLangFromId(m_langs[i].id);

        std::vector<SubPos>& sp = m_langs[i].subpos;
        std::sort(sp.data(), sp.data() + sp.size());

        if (m_rd.bForcedOnly) {
            Log(LOG_INFO, _T("Searching for forced subs..."));
            Progress(0);

            for (size_t j = 0, len = sp.size(); j < len; j++) {
                Progress(1.0 * j / len);

                sp[j].bValid = false;
                size_t packetSize = 0, dataSize = 0;
                if (BYTE* buff = GetPacket(j, packetSize, dataSize, i)) {
                    m_img.GetPacketInfo(buff, packetSize, dataSize);
                    sp[j].bValid = m_img.bForced;
                    delete [] buff;
                }
            }

            Progress(1);
        }
    }

    Log(LOG_INFO, _T("Saving files..."));

    if (m_nLang != SIZE_T_ERROR) {
        if (!Save(m_title)) {
            Log(LOG_ERROR, _T("Could not save output files!"));
            return false;
        }
    }

    Log(LOG_INFO, _T("Subtitles saved"));

    if (!m_vob.IsDVD() && loadedchunks.empty()) {
        if (SaveChunks(foundchunks)) {
            Log(LOG_INFO, _T(".chunk file saved"));
        }
    }

    Log(LOG_INFO, _T("Done!"));

    return true;
}

static const DWORD s_version = 1;

bool CVobSubFileRipper::LoadChunks(std::vector<vcchunk>& chunks)
{
    tstring fn = m_infn;
    TrimExtension(fn);
    fn += _T(".chunks");

    DWORD chksum = 0, chunklen, version;
    __int64 voblen = 0;

    {
        FilePtr f = OpenFile(fn, L"rb");
        if (!f) {
            return false;
        }
        fread(&version, 1, sizeof(version), f.get());
        if (version == 1) {
            fread(&chksum, 1, sizeof(chksum), f.get());
            fread(&voblen, 1, sizeof(voblen), f.get());
            fread(&chunklen, 1, sizeof(chunklen), f.get());
            chunks.resize(chunklen);
            fread(chunks.data(), 1, sizeof(vcchunk) * chunks.size(), f.get());
        }
    }

    if (voblen != m_vob.GetLength()) {
        chunks.clear();
        return false;
    }

    DWORD chksum2 = 0;
    {
        FilePtr f = OpenFile(m_infn, L"rb");
        if (!f) {
            return false;
        }
        DWORD dw;
        while (fread(&dw, 1, sizeof(dw), f.get()) == sizeof(dw)) {
            chksum2 += dw;
        }
    }

    if (chksum != chksum2) {
        chunks.clear();
        return false;
    }

    return true;
}

bool CVobSubFileRipper::SaveChunks(std::vector<vcchunk>& chunks)
{
    tstring fn = m_infn;
    TrimExtension(fn);
    fn += _T(".chunks");

    DWORD chksum = 0, chunklen = (DWORD)chunks.size();
    __int64 voblen = m_vob.GetLength();

    {
        FilePtr f = OpenFile(m_infn, L"rb");
        if (!f) {
            return false;
        }
        DWORD dw;
        while (fread(&dw, 1, sizeof(dw), f.get()) == sizeof(dw)) {
            chksum += dw;
        }
    }

    FilePtr f = OpenFile(fn, L"wb", _SH_DENYWR);
    if (!f) {
        return false;
    }
    fwrite(&s_version, 1, sizeof(s_version), f.get());
    fwrite(&chksum, 1, sizeof(chksum), f.get());
    fwrite(&voblen, 1, sizeof(voblen), f.get());
    fwrite(&chunklen, 1, sizeof(chunklen), f.get());
    fwrite(chunks.data(), 1, sizeof(vcchunk) * chunklen, f.get());

    return true;
}

// IVSFRipper

STDMETHODIMP CVobSubFileRipper::SetCallBack(IVSFRipperCallback* pCallback)
{
    CAutoLock cAutoLock(&m_csCallback);
    m_pCallback = pCallback;
    return S_OK;
}

STDMETHODIMP CVobSubFileRipper::LoadParamFile(tstring fn)
{
    CAutoLock cAutoLock(&m_csAccessLock);

    m_rd.Reset();

    CTextFile f;
    if (!f.Open(fn.c_str())) {
        return E_FAIL;
    }

    TCHAR langid[256];

    enum {
        P_INPUT,
        P_OUTPUT,
        P_PGC,
        P_ANGLE,
        P_LANGS,
        P_OPTIONS
    };
    int phase = P_INPUT;

    tstring line;
    while (f.ReadString(line)) {
        if (StringTrim(line).empty() || line[0] == '#') {
            continue;
        }

        if (phase == P_INPUT) {
            if (S_OK != SetInput(line)) {
                break;
            }
            phase = P_OUTPUT;
        } else if (phase == P_OUTPUT) {
            if (S_OK != SetOutput(line)) {
                break;
            }
            phase = P_PGC;
        } else if (phase == P_PGC) {
            m_rd.iSelPGC = _tcstol(line.c_str(), nullptr, 10) - 1;
            if (m_rd.iSelPGC < 0 || (size_t)m_rd.iSelPGC >= m_rd.pgcs.size()) {
                break;
            }
            phase = P_ANGLE;
        } else if (phase == 3) {
            PGC& pgc = m_rd.pgcs[m_rd.iSelPGC];

            pgc.iSelAngle = _tcstol(line.c_str(), nullptr, 10);
            if (pgc.iSelAngle < 0 || pgc.iSelAngle > std::max(1, pgc.nAngles) || pgc.iSelAngle > 9) {
                break;
            }

            std::vector<vc_t>& angle = pgc.angles[pgc.iSelAngle];

            if (StringFind(line, 'v') >= 0) {
                int vob = 0, cell = 0;

                line += _T(' ');

                TCHAR* s = (LPTSTR)line.c_str();
                TCHAR* e = s + int(line.size());
                while (s < e) {
                    if (*s == 'v' || s == e - 1) {
                        s++;
                        if (vob != 0 && cell == 0) {
                            for (size_t i = 0; i < angle.size(); i++) {
                                if (angle[i].vob == vob) {
                                    m_rd.selvcs.push_back((angle[i].vob << 16) | angle[i].cell);
                                }
                            }
                        }

                        vob = _tcstol(s, &s, 10);
                        cell = 0;
                    } else if (*s == 'c' && vob > 0) {
                        s++;
                        cell = _tcstol(s, &s, 10);

                        for (size_t i = 0; i < angle.size(); i++) {
                            if (angle[i].vob == vob && angle[i].cell == cell) {
                                m_rd.selvcs.push_back((vob << 16) | cell);
                                break;
                            }
                        }
                    } else {
                        s++;
                    }
                }
            } else {
                for (size_t i = 0; i < angle.size(); i++) {
                    m_rd.selvcs.push_back((angle[i].vob << 16) | angle[i].cell);
                }
            }

            phase = P_LANGS;
        } else if (phase == 4) {
            if (!StringICompare(line, _T("ALL"))) {
                for (BYTE i = 0; i < 32; i++) {
                    m_rd.selids[i] = true;
                }
                m_rd.bClosedCaption = true;
                phase = P_OPTIONS;
            } else {
                line += _T(' ');

                while (!line.empty()) {
                    int n = StringFind(line, _T(" "));

                    tstring lang = StringLeft(line, n);

                    line = StringMid(line, n);
                    StringTrimLeft(line);

                    n = 0;
                    if (_istdigit(lang[0])) {
                        int langnum;
                        n = _stscanf_s(lang.c_str(), _T("%d"), &langnum);
                        if (n != 1) {
                            break;
                        }

                        m_rd.selids[(BYTE)langnum] = true;
                    } else if (_istalpha(lang[0])) {
                        n = _stscanf_s(lang.c_str(), _T("%s"), langid, UINT(_countof(langid)));
                        if (n != 1) {
                            break;
                        }

                        int id = (langid[0] << 8) + langid[1];

                        if (id == 'cc') {
                            m_rd.bClosedCaption = true;
                        } else {
                            ASSERT(id <= BYTE_MAX);
                            m_rd.selids[(BYTE)id] = true;
                        }
                    } else {
                        break;
                    }
                }

                if ((!m_rd.selids.empty() || m_rd.bClosedCaption) && line.empty()) {
                    phase = P_OPTIONS;
                }
            }
        } else if (phase == 5 && !StringICompare(line, _T("CLOSE"))) {
            m_rd.bClose = true;
        } else if (phase == 5 && !StringICompare(line, _T("BEEP"))) {
            m_rd.bBeep = true;
        } else if (phase == 5 && !StringICompare(line, _T("RESETTIME"))) {
            m_rd.bResetTime = true;
        } else if (phase == 5 && !StringICompare(line, _T("FORCEDONLY"))) {
            m_rd.bForcedOnly = true;
        } else if (phase == 5 && !StringICompare(line, _T("CLOSEIGNOREERRORS"))) {
            m_rd.bCloseIgnoreError = true;
        }

    }

    m_rd.bAuto = true;

    return phase == P_OPTIONS ? S_OK : E_FAIL;
}

STDMETHODIMP CVobSubFileRipper::SetInput(tstring infn)
{
    CAutoLock cAutoLock(&m_csAccessLock);

    m_rd.Reset();

    if (!LoadIfo(infn) || !LoadVob(infn)) {
        return E_INVALIDARG;
    }

    m_infn = infn;

    return S_OK;
}

STDMETHODIMP CVobSubFileRipper::SetOutput(tstring outfn)
{
    CAutoLock cAutoLock(&m_csAccessLock);
    m_outfn = outfn;
    return S_OK;
}

STDMETHODIMP CVobSubFileRipper::GetRipperData(VSFRipperData& rd)
{
    CAutoLock cAutoLock(&m_csAccessLock);
    rd.Copy(m_rd);
    return S_OK;
}

STDMETHODIMP CVobSubFileRipper::UpdateRipperData(VSFRipperData& rd)
{
    CAutoLock cAutoLock(&m_csAccessLock);
    m_rd.Copy(rd);
    return S_OK;
}

STDMETHODIMP CVobSubFileRipper::Index()
{
    if (m_bIndexing) {
        return E_FAIL;
    }
    CAMThread::CallWorker(CMD_INDEX);
    return S_OK;
}


STDMETHODIMP CVobSubFileRipper::IsIndexing()
{
    return m_bIndexing ? S_OK : S_FALSE;
}

STDMETHODIMP CVobSubFileRipper::Abort(bool bSavePartial)
{
    m_bBreakThread = true;
    return S_OK;
}

//

void VSFRipperData::Reset()
{
    vidsize = MakeSize(0, 0);
    ZeroMemory(&vidinfo, sizeof(vidinfo));
    pgcs.clear();
    iSelPGC = -1;
    bResetTime = bClosedCaption = true;
    bForcedOnly = false;
    bClose = bBeep = bAuto = false;
    bCloseIgnoreError = false;

    selvcs.clear();
    selids.clear();
}

void VSFRipperData::Copy(VSFRipperData& rd)
{
    Reset();

    vidsize = rd.vidsize;
    vidinfo = rd.vidinfo;
    if (size_t len = rd.pgcs.size()) {
        pgcs.resize(len);
        for (size_t i = 0; i < len; i++) {
            PGC& src = rd.pgcs[i];
            PGC& dst = pgcs[i];
            dst.nAngles = src.nAngles;
            for (size_t j = 0; j < _countof(dst.angles); j++) {
                dst.angles[j] = src.angles[j];
            }
            dst.iSelAngle = src.iSelAngle;
            memcpy(dst.pal, src.pal, sizeof(src.pal));
            memcpy(dst.ids, src.ids, sizeof(src.ids));
        }
    }
    iSelPGC = rd.iSelPGC;
    bResetTime = rd.bResetTime;
    bClosedCaption = rd.bClosedCaption;
    bForcedOnly = rd.bForcedOnly;
    bClose = rd.bClose;
    bBeep = rd.bBeep;
    bAuto = rd.bAuto;
    bCloseIgnoreError = rd.bCloseIgnoreError;
    selvcs = rd.selvcs;
    selids = rd.selids;
}
