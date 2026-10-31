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
#include <winioctl.h>
#include <algorithm>
#include "TextFile.h"
#include "VobSubFile.h"
#include "mpc-hc_config.h"

#if !USE_STATIC_UNRAR
#include <unrar.h> // vcpkg: rarlab's dll.hpp
#else
#include "unrar/dll.hpp"
#endif
#include "RTS.h"
#include "../DSUtil/PathUtils.h"
#include "../DSUtil/ISOLang.h"

//

struct lang_type {
    unsigned short id;
    LPCSTR lang_long;
} static constexpr lang_tbl[] = {
    {'--', "(Not detected)"},
    {'cc', "Closed Caption"},
    {'aa', "Afar"},
    {'ab', "Abkhazian"},
    {'af', "Afrikaans"},
    {'am', "Amharic"},
    {'ar', "Arabic"},
    {'as', "Assamese"},
    {'ay', "Aymara"},
    {'az', "Azerbaijani"},
    {'ba', "Bashkir"},
    {'be', "Byelorussian"},
    {'bg', "Bulgarian"},
    {'bh', "Bihari"},
    {'bi', "Bislama"},
    {'bn', "Bengali; Bangla"},
    {'bo', "Tibetan"},
    {'br', "Breton"},
    {'ca', "Catalan"},
    {'co', "Corsican"},
    {'cs', "Czech"},
    {'cy', "Welsh"},
    {'da', "Dansk"},
    {'de', "Deutsch"},
    {'dz', "Bhutani"},
    {'el', "Greek"},
    {'en', "English"},
    {'eo', "Esperanto"},
    {'es', "Espanol"},
    {'et', "Estonian"},
    {'eu', "Basque"},
    {'fa', "Persian"},
    {'fi', "Finnish"},
    {'fj', "Fiji"},
    {'fo', "Faroese"},
    {'fr', "Francais"},
    {'fy', "Frisian"},
    {'ga', "Irish"},
    {'gd', "Scots Gaelic"},
    {'gl', "Galician"},
    {'gn', "Guarani"},
    {'gu', "Gujarati"},
    {'ha', "Hausa"},
    {'he', "Hebrew"},
    {'hi', "Hindi"},
    {'hr', "Hrvatski"},
    {'hu', "Hungarian"},
    {'hy', "Armenian"},
    {'ia', "Interlingua"},
    {'id', "Indonesian"},
    {'ie', "Interlingue"},
    {'ik', "Inupiak"},
    {'in', "Indonesian"},
    {'is', "Islenska"},
    {'it', "Italiano"},
    {'iu', "Inuktitut"},
    {'iw', "Hebrew"},
    {'ja', "Japanese"},
    {'ji', "Yiddish"},
    {'jw', "Javanese"},
    {'ka', "Georgian"},
    {'kk', "Kazakh"},
    {'kl', "Greenlandic"},
    {'km', "Cambodian"},
    {'kn', "Kannada"},
    {'ko', "Korean"},
    {'ks', "Kashmiri"},
    {'ku', "Kurdish"},
    {'ky', "Kirghiz"},
    {'la', "Latin"},
    {'ln', "Lingala"},
    {'lo', "Laothian"},
    {'lt', "Lithuanian"},
    {'lv', "Latvian, Lettish"},
    {'mg', "Malagasy"},
    {'mi', "Maori"},
    {'mk', "Macedonian"},
    {'ml', "Malayalam"},
    {'mn', "Mongolian"},
    {'mo', "Moldavian"},
    {'mr', "Marathi"},
    {'ms', "Malay"},
    {'mt', "Maltese"},
    {'my', "Burmese"},
    {'na', "Nauru"},
    {'ne', "Nepali"},
    {'nl', "Nederlands"},
    {'no', "Norsk"},
    {'oc', "Occitan"},
    {'om', "(Afan) Oromo"},
    {'or', "Oriya"},
    {'pa', "Punjabi"},
    {'pl', "Polish"},
    {'ps', "Pashto, Pushto"},
    {'pt', "Portugues"},
    {'qu', "Quechua"},
    {'rm', "Rhaeto-Romance"},
    {'rn', "Kirundi"},
    {'ro', "Romanian"},
    {'ru', "Russian"},
    {'rw', "Kinyarwanda"},
    {'sa', "Sanskrit"},
    {'sd', "Sindhi"},
    {'sg', "Sangho"},
    {'sh', "Serbo-Croatian"},
    {'si', "Sinhalese"},
    {'sk', "Slovak"},
    {'sl', "Slovenian"},
    {'sm', "Samoan"},
    {'sn', "Shona"},
    {'so', "Somali"},
    {'sq', "Albanian"},
    {'sr', "Serbian"},
    {'ss', "Siswati"},
    {'st', "Sesotho"},
    {'su', "Sundanese"},
    {'sv', "Svenska"},
    {'sw', "Swahili"},
    {'ta', "Tamil"},
    {'te', "Telugu"},
    {'tg', "Tajik"},
    {'th', "Thai"},
    {'ti', "Tigrinya"},
    {'tk', "Turkmen"},
    {'tl', "Tagalog"},
    {'tn', "Setswana"},
    {'to', "Tonga"},
    {'tr', "Turkish"},
    {'ts', "Tsonga"},
    {'tt', "Tatar"},
    {'tw', "Twi"},
    {'ug', "Uighur"},
    {'uk', "Ukrainian"},
    {'ur', "Urdu"},
    {'uz', "Uzbek"},
    {'vi', "Vietnamese"},
    {'vo', "Volapuk"},
    {'wo', "Wolof"},
    {'xh', "Xhosa"},
    {'yi', "Yiddish"},              // formerly ji
    {'yo', "Yoruba"},
    {'za', "Zhuang"},
    {'zh', "Chinese"},
    {'zu', "Zulu"},
};

int find_lang(unsigned short id)
{
    int lo = 0, hi = _countof(lang_tbl) - 1;

    while (lo < hi) {
        int mid = (lo + hi) >> 1;

        if (id < lang_tbl[mid].id) {
            hi = mid;
        } else if (id > lang_tbl[mid].id) {
            lo = mid + 1;
        } else {
            return mid;
        }
    }

    return (id == lang_tbl[lo].id ? lo : 0);
}

tstring FindLangFromId(WORD id)
{
    return StringToT(lang_tbl[find_lang(id)].lang_long);
}

//
// CVobSubFile
//

CVobSubFile::CVobSubFile(CCritSec* pLock)
    : CSubPicProviderImpl(pLock)
    , m_nLang(0)
{
}

CVobSubFile::~CVobSubFile()
{
}

//

bool CVobSubFile::Copy(CVobSubFile& vsf)
{
    Close();

    *(CVobSubSettings*)this = *(CVobSubSettings*)&vsf;
    m_title = vsf.m_title;
    m_nLang = vsf.m_nLang;

    m_sub.SetLength(vsf.m_sub.GetLength());
    m_sub.SeekToBegin();

    for (size_t i = 0; i < m_langs.size(); i++) {
        SubLang& src = vsf.m_langs[i];
        SubLang& dst = m_langs[i];

        dst.id = src.id;
        dst.name = src.name;
        dst.alt = src.alt;

        for (size_t j = 0; j < src.subpos.size(); j++) {
            SubPos& sp = src.subpos[j];
            if (!sp.bValid) {
                continue;
            }

            if (sp.filepos != (__int64)vsf.m_sub.Seek(sp.filepos, SEEK_SET)) {
                continue;
            }

            sp.filepos = m_sub.GetPosition();

            BYTE buff[2048];
            UINT uRead = vsf.m_sub.Read(buff, 2048);
            m_sub.Write(buff, uRead);

            WORD packetsize = (buff[buff[0x16] + 0x18] << 8) | buff[buff[0x16] + 0x19];

            for (int k = 0, size, sizeleft = packetsize - 4;
                    k < packetsize - 4;
                    k += size, sizeleft -= size) {
                int hsize = buff[0x16] + 0x18 + ((buff[0x15] & 0x80) ? 4 : 0);
                size = std::min(sizeleft, 2048 - hsize);

                if (size != sizeleft) {
                    while ((uRead = vsf.m_sub.Read(buff, 2048)) > 0) {
                        if (!(buff[0x15] & 0x80) && buff[buff[0x16] + 0x17] == (i | 0x20)) {
                            break;
                        }
                    }

                    m_sub.Write(buff, uRead);
                }
            }

            dst.subpos.push_back(sp);
        }
    }

    m_sub.SetLength(m_sub.GetPosition());

    return true;
}

//

void CVobSubFile::TrimExtension(tstring& fn)
{
    int i = StringRFind(fn, '.');
    if (i > 0) {
        tstring ext = StringLowered(StringMid(fn, i));
        if (ext == _T(".ifo") || ext == _T(".idx") || ext == _T(".sub")
                || ext == _T(".sst") || ext == _T(".son") || ext == _T(".rar")) {
            fn = StringLeft(fn, i);
        }
    }
}

bool CVobSubFile::Open(tstring fn)
{
    m_path = fn;
    TrimExtension(fn);

    do {
        Close();

        int ver;
        if (!ReadIdx(fn + _T(".idx"), ver)) {
            break;
        }

        if (ver < 6 && !ReadIfo(fn + _T(".ifo"))) {
            break;
        }

        if (!ReadSub(fn + _T(".sub")) && !ReadRar(fn + _T(".rar"))) {
            break;
        }

        m_title = fn;

        for (size_t i = 0; i < m_langs.size(); i++) {
            std::vector<SubPos>& sp = m_langs[i].subpos;

            for (size_t j = 0; j < sp.size(); j++) {
                sp[j].stop = sp[j].start;
                sp[j].bForced = false;

                size_t packetSize = 0, dataSize = 0;
                BYTE* buff = GetPacket(j, packetSize, dataSize, i);
                if (!buff) {
                    sp[j].bValid = false;
                    continue;
                }

                m_img.delay = j + 1 < sp.size() ? sp[j + 1].start - sp[j].start : 3000;
                m_img.GetPacketInfo(buff, packetSize, dataSize);
                if (j + 1 < sp.size()) {
                    m_img.delay = std::min(m_img.delay, sp[j + 1].start - sp[j].start);
                }

                sp[j].stop = sp[j].start + m_img.delay;
                sp[j].bForced = m_img.bForced;
                sp[j].bAnimated = m_img.bAnimated;

                if (j > 0 && sp[j - 1].stop > sp[j].start) {
                    sp[j - 1].stop = sp[j].start;
                }

                delete [] buff;
            }
        }

        return true;
    } while (false);

    Close();

    return false;
}

bool CVobSubFile::Save(tstring fn, int delay, SubFormat sf)
{
    TrimExtension(fn);

    CVobSubFile vsf(nullptr);
    if (!vsf.Copy(*this)) {
        return false;
    }

    switch (sf) {
        case VobSub:
            return vsf.SaveVobSub(fn, delay);
        case WinSubMux:
            return vsf.SaveWinSubMux(fn, delay);
        case Scenarist:
            return vsf.SaveScenarist(fn, delay);
        case Maestro:
            return vsf.SaveMaestro(fn, delay);
        default:
            break;
    }

    return false;
}

void CVobSubFile::Close()
{
    InitSettings();
    m_title.clear();
    m_sub.SetLength(0);
    m_img.Invalidate();
    m_nLang = SIZE_T_ERROR;
    for (auto& sl : m_langs) {
        sl.id = 0;
        sl.name.clear();
        sl.alt.clear();
        sl.subpos.clear();
    }
}

//

bool CVobSubFile::ReadIdx(tstring fn, int& ver)
{
    CWebTextFile f;
    if (!f.Open(fn.c_str())) {
        return false;
    }

    bool bError = false;

    int id = -1, delay = 0, vobid = -1, cellid = -1;
    __int64 celltimestamp = 0;

    tstring str;
    for (ptrdiff_t line = 0; !bError && f.ReadString(str); line++) {
        StringTrim(str);

        if (line == 0) {
            TCHAR buff[] = _T("VobSub index file, v");

            const TCHAR* s = str.c_str();

            int i = StringFind(str, buff);
            if (i < 0 || _stscanf_s(&s[i + _tcslen(buff)], _T("%d"), &ver) != 1
                    || ver > VOBSUBIDXVER) {
                TRACE(_T("[CVobSubFile::ReadIdx] Wrong file version!\n"));
                bError = true;
                continue;
            }
        } else if (!int(str.size())) {
            continue;
        } else if (str[0] == _T('#')) {
            TCHAR buff[] = _T("Vob/Cell ID:");

            const TCHAR* s = str.c_str();

            int i = StringFind(str, buff);
            if (i >= 0) {
                _stscanf_s(&s[i + _tcslen(buff)], _T("%d, %d (PTS: %I64d)"), &vobid, &cellid, &celltimestamp);
            }

            continue;
        }

        int i = StringFind(str, ':');
        if (i <= 0) {
            continue;
        }

        tstring entry = StringLowered(StringLeft(str, i));

        str = StringMid(str, i + 1);
        StringTrim(str);
        if (str.empty()) {
            continue;
        }

        if (entry == _T("size")) {
            int x, y;
            if (_stscanf_s(str.c_str(), _T("%dx%d"), &x, &y) != 2) {
                bError = true;
            }
            m_size.cx = x;
            m_size.cy = y;
        } else if (entry == _T("org")) {
            if (_stscanf_s(str.c_str(), _T("%d,%d"), &m_x, &m_y) != 2) {
                bError = true;
            } else {
                m_org = MakePoint(m_x, m_y);
            }
        } else if (entry == _T("scale")) {
            if (ver < 5) {
                int scale = 100;
                if (_stscanf_s(str.c_str(), _T("%d%%"), &scale) != 1) {
                    bError = true;
                }
                m_scale_x = m_scale_y = scale;
            } else {
                if (_stscanf_s(str.c_str(), _T("%d%%,%d%%"), &m_scale_x, &m_scale_y) != 2) {
                    bError = true;
                }
            }
        } else if (entry == _T("alpha")) {
            if (_stscanf_s(str.c_str(), _T("%d"), &m_alpha) != 1) {
                bError = true;
            }
        } else if (entry == _T("smooth")) {
            StringLower(str);

            if (StringFind(str, _T("old")) >= 0 || StringFind(str, _T("2")) >= 0) {
                m_iSmooth = 2;
            } else if (StringFind(str, _T("on")) >= 0 || StringFind(str, _T("1")) >= 0) {
                m_iSmooth = 1;
            } else if (StringFind(str, _T("off")) >= 0 || StringFind(str, _T("0")) >= 0) {
                m_iSmooth = 0;
            } else {
                bError = true;
            }
        } else if (entry == _T("fadein/out")) {
            if (_stscanf_s(str.c_str(), _T("%d,%d"), &m_fadein, &m_fadeout) != 2) {
                bError = true;
            }
        } else if (entry == _T("align")) {
            StringLower(str);

            int k = 0;
            tstring token;
            for (int j = 0; j < 3; j++) {
                token = StringTokenize(str, _T(" "), k);
                if (token.empty()) {
                    break;
                }

                if (j == 0) {
                    if (token == _T("on") || token == _T("1")) {
                        m_bAlign = true;
                    } else if (token == _T("off") || token == _T("0")) {
                        m_bAlign = false;
                    } else {
                        bError = true;
                        break;
                    }
                } else if (j == 1) {
                    if (token == _T("at")) {
                        j--;
                        continue;
                    }

                    if (token == _T("left")) {
                        m_alignhor = 0;
                    } else if (token == _T("center")) {
                        m_alignhor = 1;
                    } else if (token == _T("right")) {
                        m_alignhor = 2;
                    } else {
                        bError = true;
                        break;
                    }
                } else if (j == 2) {
                    if (token == _T("top")) {
                        m_alignver = 0;
                    } else if (token == _T("center")) {
                        m_alignver = 1;
                    } else if (token == _T("bottom")) {
                        m_alignver = 2;
                    } else {
                        bError = true;
                        break;
                    }
                }
            }
        } else if (entry == _T("time offset")) {
            bool bNegative = false;
            if (str[0] == '-') {
                bNegative = true;
            }
            StringTrimLeft(str, _T("+-"));

            TCHAR c;
            int hh, mm, ss, ms;
            int n = _stscanf_s(str.c_str(), _T("%d%c%d%c%d%c%d"), &hh, &c, 1, &mm, &c, 1, &ss, &c, 1, &ms);

            switch (n) {
                case 1: // We have read only one integer, interpret it as an offset expressed in milliseconds
                    m_toff = hh * (bNegative ? -1 : 1);
                    break;
                case 7: // We have read 4 integers + 3 separators, interpret them as hh:mm:ss.ms
                    m_toff = (hh * 60 * 60 * 1000 + mm * 60 * 1000 + ss * 1000 + ms) * (bNegative ? -1 : 1);
                    break;
                default:
                    bError = true;
                    m_toff = 0;
                    break;
            }
        } else if (entry == _T("forced subs")) {
            StringLower(str);

            if (StringFind(str, _T("on")) >= 0 || StringFind(str, _T("1")) >= 0) {
                m_bOnlyShowForcedSubs = true;
            } else if (StringFind(str, _T("off")) >= 0 || StringFind(str, _T("0")) >= 0) {
                m_bOnlyShowForcedSubs = false;
            } else {
                bError = true;
            }
        } else if (entry == _T("langidx")) {
            int iLang = -1;
            if (_stscanf_s(str.c_str(), _T("%d"), &iLang) != 1) {
                bError = true;
            }
            m_nLang = (iLang < 0 && size_t(iLang) >= m_langs.size()) ? SIZE_T_ERROR : size_t(iLang);
        } else if (entry == _T("palette")) {
            // The assert guarantees that the shortcut we use will work as expected
            static_assert(sizeof(RGBQUAD) == 4, "Packing error");
#pragma warning(push)
#pragma warning(disable: 4477)
            if (_stscanf_s(str.c_str(), _T("%x,%x,%x,%x,%x,%x,%x,%x,%x,%x,%x,%x,%x,%x,%x,%x"),
                           &m_orgpal[0], &m_orgpal[1], &m_orgpal[2], &m_orgpal[3],
                           &m_orgpal[4], &m_orgpal[5], &m_orgpal[6], &m_orgpal[7],
                           &m_orgpal[8], &m_orgpal[9], &m_orgpal[10], &m_orgpal[11],
                           &m_orgpal[12], &m_orgpal[13], &m_orgpal[14], &m_orgpal[15]
                          ) != 16) {
#pragma warning(pop)
                bError = true;
            }
        } else if (entry == _T("custom colors")) {
            StringLower(str);

            if (StringFind(str, _T("on")) == 0 || StringFind(str, _T("1")) == 0) {
                m_bCustomPal = true;
            } else if (StringFind(str, _T("off")) == 0 || StringFind(str, _T("0")) == 0) {
                m_bCustomPal = false;
            } else {
                bError = true;
            }

            i = StringFind(str, _T("tridx:"));
            if (i < 0) {
                bError = true;
                continue;
            }
            str = StringMid(str, i + (int)_tcslen(_T("tridx:")));

            int tridx;
            if (_stscanf_s(str.c_str(), _T("%x"), &tridx) != 1) {
                bError = true;
                continue;
            }
            tridx = ((tridx & 0x1000) >> 12) | ((tridx & 0x100) >> 7) | ((tridx & 0x10) >> 2) | ((tridx & 1) << 3);

            i = StringFind(str, _T("colors:"));
            if (i < 0) {
                bError = true;
                continue;
            }
            str = StringMid(str, i + (int)_tcslen(_T("colors:")));

            RGBQUAD pal[4];
#pragma warning(push)
#pragma warning(disable: 4477)
            if (_stscanf_s(str.c_str(), _T("%x,%x,%x,%x"), &pal[0], &pal[1], &pal[2], &pal[3]) != 4) {
#pragma warning(pop)
                bError = true;
                continue;
            }

            SetCustomPal(pal, tridx);
        } else if (entry == _T("id")) {
            StringLower(str);

            WORD langid = ((str[0] & 0xff) << 8) | (str[1] & 0xff);

            i = StringFind(str, _T("index:"));
            if (i < 0) {
                bError = true;
                continue;
            }
            str = StringMid(str, i + (int)_tcslen(_T("index:")));

            if (_stscanf_s(str.c_str(), _T("%d"), &id) != 1 || id < 0 || size_t(id) >= m_langs.size()) {
                bError = true;
                continue;
            }
            if (m_nLang == SIZE_T_ERROR) {
                m_nLang = size_t(id);
            }

            m_langs[id].id = langid;
            m_langs[id].name = StringToT(lang_tbl[find_lang(langid)].lang_long);
            m_langs[id].alt = StringToT(lang_tbl[find_lang(langid)].lang_long);

            delay = 0;
        } else if (id >= 0 && entry == _T("alt")) {
            m_langs[id].alt = str;
        } else if (id >= 0 && entry == _T("delay")) {
            bool bNegative = false;
            if (str[0] == '-') {
                bNegative = true;
            }
            StringTrimLeft(str, _T("+-"));

            TCHAR c;
            int hh, mm, ss, ms;
            if (_stscanf_s(str.c_str(), _T("%d%c%d%c%d%c%d"), &hh, &c, 1, &mm, &c, 1, &ss, &c, 1, &ms) != 4 + 3) {
                bError = true;
                continue;
            }

            delay += (hh * 60 * 60 * 1000 + mm * 60 * 1000 + ss * 1000 + ms) * (bNegative ? -1 : 1);
        } else if (id >= 0 && entry == _T("timestamp")) {
            SubPos sb;

            sb.vobid = (char)vobid;
            sb.cellid = (char)cellid;
            sb.celltimestamp = celltimestamp;
            sb.bValid = true;

            bool bNegative = false;
            if (str[0] == '-') {
                bNegative = true;
            }
            StringTrimLeft(str, _T("+-"));

            TCHAR c;
            int hh, mm, ss, ms;
            if (_stscanf_s(str.c_str(), _T("%d%c%d%c%d%c%d"), &hh, &c, 1, &mm, &c, 1, &ss, &c, 1, &ms) != 4 + 3) {
                bError = true;
                continue;
            }

            sb.start = (hh * 60 * 60 * 1000 + mm * 60 * 1000 + ss * 1000 + ms) * (bNegative ? -1 : 1) + delay;

            i = StringFind(str, _T("filepos:"));
            if (i < 0) {
                bError = true;
                continue;
            }
            str = StringMid(str, i + (int)_tcslen(_T("filepos:")));

            if (_stscanf_s(str.c_str(), _T("%I64x"), &sb.filepos) != 1) {
                bError = true;
                continue;
            }

            if (delay < 0 && !m_langs[id].subpos.empty()) {
                __int64 ts = m_langs[id].subpos[m_langs[id].subpos.size() - 1].start;

                if (sb.start < ts) {
                    delay += (int)(ts - sb.start);
                    sb.start = ts;
                }
            }

            m_langs[id].subpos.push_back(sb);
        } else {
            bError = true;
        }
    }

    return !bError;
}

bool CVobSubFile::ReadSub(tstring fn)
{
    FilePtr f = OpenFile(fn, L"rb");
    if (!f) {
        return false;
    }

    size_t len;
    BYTE buff[2048];
    m_sub.SetLength(FileLength(f.get()));
    m_sub.SeekToBegin();
    while ((len = fread(buff, 1, sizeof(buff), f.get())) > 0 && *(DWORD*)buff == 0xba010000) {
        m_sub.Write(buff, static_cast<UINT>(len));
    }

    return !ferror(f.get());
}

static unsigned char* RARbuff = nullptr;
static unsigned int RARpos = 0;
static unsigned int RARsize = 0;

static int CALLBACK MyCallbackProc(UINT msg, LPARAM UserData, LPARAM P1, LPARAM P2)
{
    if (msg == UCM_PROCESSDATA) {
        ASSERT(RARbuff);

        // The buffer is sized from the header's UnpSize, which the archive can lie about:
        // abort the extraction instead of writing past the end of the buffer.
        if (!RARbuff || (size_t)P2 > RARsize - RARpos) {
            return -1;
        }

        memcpy(&RARbuff[RARpos], (char*)P1, (size_t)P2);
        RARpos += (unsigned int)P2;
    }

    return 1;
}

bool CVobSubFile::ReadRar(tstring fn)
{
#if !USE_STATIC_UNRAR
    // Same name for x86 and x64 (vcpkg port). Only look next to the application and in
    // System32, never in the current directory (DLL planting).
    HMODULE h = LoadLibraryEx(_T("unrar.dll"), nullptr, LOAD_LIBRARY_SEARCH_APPLICATION_DIR | LOAD_LIBRARY_SEARCH_SYSTEM32);
    if (!h) {
        return false;
    }

    auto OpenArchiveEx = reinterpret_cast<decltype(&::RAROpenArchiveEx)>(GetProcAddress(h, "RAROpenArchiveEx"));
    auto CloseArchive  = reinterpret_cast<decltype(&::RARCloseArchive)>(GetProcAddress(h, "RARCloseArchive"));
    auto ReadHeaderEx  = reinterpret_cast<decltype(&::RARReadHeaderEx)>(GetProcAddress(h, "RARReadHeaderEx"));
    auto ProcessFile   = reinterpret_cast<decltype(&::RARProcessFile)>(GetProcAddress(h, "RARProcessFile"));
    auto SetCallback   = reinterpret_cast<decltype(&::RARSetCallback)>(GetProcAddress(h, "RARSetCallback"));

    if (!(OpenArchiveEx && CloseArchive && ReadHeaderEx && ProcessFile && SetCallback)) {
        FreeLibrary(h);
        return false;
    }

#else

#define OpenArchiveEx      RAROpenArchiveEx
#define CloseArchive       RARCloseArchive
#define ReadHeaderEx       RARReadHeaderEx
#define ProcessFile        RARProcessFile
#define SetCallback        RARSetCallback
#endif /* USE_STATIC_UNRAR */

    RAROpenArchiveDataEx OpenArchiveData;
    ZeroMemory(&OpenArchiveData, sizeof(OpenArchiveData));

    OpenArchiveData.ArcNameW = (LPTSTR)fn.c_str();
    char fnA[MAX_PATH];
    size_t size;
    if (wcstombs_s(&size, fnA, fn.c_str(), int(fn.size()))) {
        fnA[0] = 0;
    }
    OpenArchiveData.ArcName = fnA;
    OpenArchiveData.OpenMode = RAR_OM_EXTRACT;
    OpenArchiveData.CmtBuf = 0;
    OpenArchiveData.Callback = MyCallbackProc;
    HANDLE hArcData = OpenArchiveEx(&OpenArchiveData);
    if (!hArcData) {
#if !USE_STATIC_UNRAR
        FreeLibrary(h);
#endif
        return false;
    }

    RARHeaderDataEx HeaderDataEx;
    HeaderDataEx.CmtBuf = nullptr;

    while (ReadHeaderEx(hArcData, &HeaderDataEx) == 0) {
        tstring subfn(HeaderDataEx.FileNameW);

        if (!StringICompare(StringRight(subfn, 4), _T(".sub"))) {
            std::unique_ptr<BYTE[]> buff;
            if (HeaderDataEx.UnpSizeHigh == 0) {
                buff.reset(new(std::nothrow) BYTE[HeaderDataEx.UnpSize]);
            }
            if (!buff) {
                CloseArchive(hArcData);
#if !USE_STATIC_UNRAR
                FreeLibrary(h);
#endif
                return false;
            }

            RARbuff = buff.get();
            RARpos = 0;
            RARsize = HeaderDataEx.UnpSize;

            if (ProcessFile(hArcData, RAR_TEST, nullptr, nullptr)) {
                RARbuff = nullptr;
                CloseArchive(hArcData);
#if !USE_STATIC_UNRAR
                FreeLibrary(h);
#endif

                return false;
            }

            // Only what was actually extracted is initialized
            m_sub.SetLength(RARpos);
            m_sub.SeekToBegin();
            m_sub.Write(buff.get(), RARpos);
            m_sub.SeekToBegin();

            RARbuff = nullptr;
            RARpos = 0;

            break;
        }

        ProcessFile(hArcData, RAR_SKIP, nullptr, nullptr);
    }

    CloseArchive(hArcData);
#if !USE_STATIC_UNRAR
    FreeLibrary(h);
#endif

    return true;
}

bool CVobSubFile::ReadIfo(tstring fn)
{
    FilePtr f = OpenFile(fn, L"rb");
    if (!f) {
        return false;
    }

    /* PGC1 */

    _fseeki64(f.get(), 0xc0 + 0x0c, SEEK_SET);

    DWORD pos;
    ReadBEdw(pos);

    _fseeki64(f.get(), static_cast<long long>(pos) * 0x800 + 0x0c, SEEK_SET);

    DWORD offset;
    ReadBEdw(offset);

    /* Subpic palette */

    _fseeki64(f.get(), static_cast<long long>(pos) * 0x800 + offset + 0xa4, SEEK_SET);

    for (size_t i = 0; i < 16; i++) {
        BYTE y, u, v, tmp;

        fread(&tmp, 1, 1, f.get());
        fread(&y, 1, 1, f.get());
        fread(&u, 1, 1, f.get());
        fread(&v, 1, 1, f.get());

        y = (y - 16) * 255 / 219;

        m_orgpal[i].rgbRed = std::min<BYTE>(std::max<BYTE>(BYTE(1.0 * y + 1.4022 * (u - 128)), 0u), 255u);
        m_orgpal[i].rgbGreen = std::min<BYTE>(std::max<BYTE>(BYTE(1.0 * y - 0.3456 * (u - 128) - 0.7145 * (v - 128)), 0u), 255u);
        m_orgpal[i].rgbBlue = std::min<BYTE>(std::max<BYTE>(BYTE(1.0 * y + 1.7710 * (v - 128)), 0u), 255u);
    }

    return true;
}

bool CVobSubFile::WriteIdx(tstring fn, int delay)
{
    CTextFile f;
    if (!f.Save(fn.c_str(), CTextFile::DEFAULT_ENCODING)) {
        return false;
    }

    tstring str;
    str = ::Format(_T("# VobSub index file, v%d (do not modify this line!)\n"), VOBSUBIDXVER);

    f.WriteString(str);
    f.WriteString(_T("# \n"));
    f.WriteString(_T("# To repair desyncronization, you can insert gaps this way:\n"));
    f.WriteString(_T("# (it usually happens after vob id changes)\n"));
    f.WriteString(_T("# \n"));
    f.WriteString(_T("#\t delay: [sign]hh:mm:ss:ms\n"));
    f.WriteString(_T("# \n"));
    f.WriteString(_T("# Where:\n"));
    f.WriteString(_T("#\t [sign]: +, - (optional)\n"));
    f.WriteString(_T("#\t hh: hours (0 <= hh)\n"));
    f.WriteString(_T("#\t mm/ss: minutes/seconds (0 <= mm/ss <= 59)\n"));
    f.WriteString(_T("#\t ms: milliseconds (0 <= ms <= 999)\n"));
    f.WriteString(_T("# \n"));
    f.WriteString(_T("#\t Note: You can't position a sub before the previous with a negative value.\n"));
    f.WriteString(_T("# \n"));
    f.WriteString(_T("# You can also modify timestamps or delete a few subs you don't like.\n"));
    f.WriteString(_T("# Just make sure they stay in increasing order.\n"));
    f.WriteString(_T("\n"));
    f.WriteString(_T("\n"));

    // Settings

    f.WriteString(_T("# Settings\n\n"));

    f.WriteString(_T("# Original frame size\n"));
    str = ::Format(_T("size: %ldx%ld\n\n"), m_size.cx, m_size.cy);
    f.WriteString(str);

    f.WriteString(_T("# Origin, relative to the upper-left corner, can be overloaded by aligment\n"));
    str = ::Format(_T("org: %d, %d\n\n"), m_x, m_y);
    f.WriteString(str);

    f.WriteString(_T("# Image scaling (hor,ver), origin is at the upper-left corner or at the alignment coord (x, y)\n"));
    str = ::Format(_T("scale: %d%%, %d%%\n\n"), m_scale_x, m_scale_y);
    f.WriteString(str);

    f.WriteString(_T("# Alpha blending\n"));
    str = ::Format(_T("alpha: %d%%\n\n"), m_alpha);
    f.WriteString(str);

    f.WriteString(_T("# Smoothing for very blocky images (use OLD for no filtering)\n"));
    str = ::Format(_T("smooth: %s\n\n"), m_iSmooth == 0 ? _T("OFF") : m_iSmooth == 1 ? _T("ON") : _T("OLD"));
    f.WriteString(str);

    f.WriteString(_T("# In millisecs\n"));
    str = ::Format(_T("fadein/out: %d, %d\n\n"), m_fadein, m_fadeout);
    f.WriteString(str);

    f.WriteString(_T("# Force subtitle placement relative to (org.x, org.y)\n"));
    str = ::Format(_T("align: %s %s %s\n\n"), m_bAlign ? _T("ON at") : _T("OFF at"), m_alignhor == 0 ? _T("LEFT") : m_alignhor == 1 ? _T("CENTER") : m_alignhor == 2 ? _T("RIGHT") : _T(""), m_alignver == 0 ? _T("TOP") : m_alignver == 1 ? _T("CENTER") : m_alignver == 2 ? _T("BOTTOM") : _T(""));
    f.WriteString(str);

    f.WriteString(_T("# For correcting non-progressive desync. (in millisecs or hh:mm:ss:ms)\n"));
    f.WriteString(_T("# Note: Not effective in VSFilter, use \"delay: ... \" instead.\n"));
    str = ::Format(_T("time offset: %u\n\n"), m_toff);
    f.WriteString(str);

    f.WriteString(_T("# ON: displays only forced subtitles, OFF: shows everything\n"));
    str = ::Format(_T("forced subs: %s\n\n"), m_bOnlyShowForcedSubs ? _T("ON") : _T("OFF"));
    f.WriteString(str);

    f.WriteString(_T("# The original palette of the DVD\n"));
    str = ::Format(_T("palette: %06x, %06x, %06x, %06x, %06x, %06x, %06x, %06x, %06x, %06x, %06x, %06x, %06x, %06x, %06x, %06x\n\n"), *((unsigned int*)&m_orgpal[0]) & 0xffffff, *((unsigned int*)&m_orgpal[1]) & 0xffffff, *((unsigned int*)&m_orgpal[2]) & 0xffffff, *((unsigned int*)&m_orgpal[3]) & 0xffffff, *((unsigned int*)&m_orgpal[4]) & 0xffffff, *((unsigned int*)&m_orgpal[5]) & 0xffffff, *((unsigned int*)&m_orgpal[6]) & 0xffffff, *((unsigned int*)&m_orgpal[7]) & 0xffffff, *((unsigned int*)&m_orgpal[8]) & 0xffffff, *((unsigned int*)&m_orgpal[9]) & 0xffffff, *((unsigned int*)&m_orgpal[10]) & 0xffffff, *((unsigned int*)&m_orgpal[11]) & 0xffffff, *((unsigned int*)&m_orgpal[12]) & 0xffffff, *((unsigned int*)&m_orgpal[13]) & 0xffffff, *((unsigned int*)&m_orgpal[14]) & 0xffffff, *((unsigned int*)&m_orgpal[15]) & 0xffffff);
    f.WriteString(str);

    int tridx = (!!(m_tridx & 1)) * 0x1000 + (!!(m_tridx & 2)) * 0x100 + (!!(m_tridx & 4)) * 0x10 + (!!(m_tridx & 8));

    f.WriteString(_T("# Custom colors (transp idxs and the four colors)\n"));
    str = ::Format(_T("custom colors: %s, tridx: %04x, colors: %06x, %06x, %06x, %06x\n\n"), m_bCustomPal ? _T("ON") : _T("OFF"), tridx, *((unsigned int*)&m_cuspal[0]) & 0xffffff, *((unsigned int*)&m_cuspal[1]) & 0xffffff, *((unsigned int*)&m_cuspal[2]) & 0xffffff, *((unsigned int*)&m_cuspal[3]) & 0xffffff);
    f.WriteString(str);

    f.WriteString(_T("# Language index in use\n"));
    str = ::Format(_T("langidx: %Iu\n\n"), m_nLang);
    f.WriteString(str);

    if (delay) {
        str = ::Format(_T("delay: %s%02d:%02d:%02d:%03d\n\n"), delay < 0 ? _T("-") : _T(""), abs(delay / 1000 / 60 / 60) % 60, abs(delay / 1000 / 60) % 60, abs(delay / 1000) % 60, abs(delay % 1000));
        f.WriteString(str);
    }

    // Subs

    for (size_t i = 0; i < m_langs.size(); i++) {
        SubLang& sl = m_langs[i];

        std::vector<SubPos>& sp = sl.subpos;
        if (sp.empty() && !sl.id) {
            continue;
        }

        str = ::Format(_T("# %s\n"), sl.name.c_str());
        f.WriteString(str);

        ASSERT(sl.id);
        if (!sl.id) {
            sl.id = '--';
        }
        str = ::Format(_T("id: %c%c, index: %Iu\n"), sl.id >> 8, sl.id & 0xff, i);
        f.WriteString(str);

        str = _T("# Uncomment next line to activate alternative name in VSFilter / Windows Media Player 6.x\n");
        f.WriteString(str);
        str = ::Format(_T("alt: %s\n"), sl.alt.c_str());
        if (sl.name == sl.alt) {
            str = _T("# ") + str;
        }
        f.WriteString(str);

        char vobid = -1, cellid = -1;

        for (size_t j = 0; j < sp.size(); j++) {
            if (!sp[j].bValid) {
                continue;
            }

            if (sp[j].vobid != vobid || sp[j].cellid != cellid) {
                str = ::Format(_T("# Vob/Cell ID: %d, %d (PTS: %I64d)\n"), sp[j].vobid, sp[j].cellid, sp[j].celltimestamp);
                f.WriteString(str);
                vobid = sp[j].vobid;
                cellid = sp[j].cellid;
            }

            str = ::Format(_T("timestamp: %s%02d:%02d:%02d:%03d, filepos: %09I64x\n"), sp[j].start < 0 ? _T("-") : _T(""), abs(int((sp[j].start / 1000 / 60 / 60) % 60)), abs(int((sp[j].start / 1000 / 60) % 60)), abs(int((sp[j].start / 1000) % 60)), abs(int((sp[j].start) % 1000)), sp[j].filepos);
            f.WriteString(str);
        }

        f.WriteString(_T("\n"));
    }

    return true;
}

bool CVobSubFile::WriteSub(tstring fn)
{
    FilePtr f = OpenFile(fn, L"wb", _SH_DENYWR);
    if (!f) {
        return false;
    }

    if (m_sub.GetLength() == 0) {
        return true;    // nothing to do...
    }

    m_sub.SeekToBegin();

    UINT len;
    BYTE buff[2048];
    while ((len = m_sub.Read(buff, sizeof(buff))) > 0 && *(DWORD*)buff == 0xba010000) {
        fwrite(buff, 1, len, f.get());
    }

    return true;
}

//

BYTE* CVobSubFile::GetPacket(size_t idx, size_t& packetSize, size_t& dataSize, size_t nLang /*= SIZE_T_ERROR*/)
{
    BYTE* ret = nullptr;

    if (nLang >= m_langs.size()) {
        nLang = m_nLang;
    }
    std::vector<SubPos>& sp = m_langs[nLang].subpos;

    do {
        if (idx >= sp.size()) {
            break;
        }

        if ((__int64)m_sub.Seek(sp[idx].filepos, SEEK_SET) != sp[idx].filepos) {
            break;
        }

        BYTE buff[0x800];
        if (sizeof(buff) != m_sub.Read(buff, sizeof(buff))) {
            break;
        }

        ASSERT(nLang < BYTE_MAX);

        // let's check a few things to make sure...
        if (*(DWORD*)&buff[0x00] != 0xba010000
                || *(DWORD*)&buff[0x0e] != 0xbd010000
                || !(buff[0x15] & 0x80)
                || (buff[0x17] & 0xf0) != 0x20
                || (buff[buff[0x16] + 0x17] & 0xe0) != 0x20
                || (buff[buff[0x16] + 0x17] & 0x1f) != (BYTE)nLang) {
            break;
        }

        packetSize = (buff[buff[0x16] + 0x18] << 8) + buff[buff[0x16] + 0x19];
        dataSize = (buff[buff[0x16] + 0x1a] << 8) + buff[buff[0x16] + 0x1b];

        try {
            ret = new BYTE[packetSize];
        } catch (const std::bad_alloc&) {
            ASSERT(FALSE);
            break;
        }

        size_t i = 0, sizeLeft = packetSize;
        for (size_t size; i < packetSize; i += size, sizeLeft -= size) {
            size_t hsize = 0x18 + buff[0x16];
            size = std::min(sizeLeft, 0x800 - hsize);
            memcpy(&ret[i], &buff[hsize], size);

            if (size != sizeLeft) {
                while (m_sub.Read(buff, sizeof(buff))) {
                    if (/*!(buff[0x15] & 0x80) &&*/ buff[buff[0x16] + 0x17] == (nLang | 0x20)) {
                        break;
                    }
                }
            }
        }

        if (i != packetSize || sizeLeft > 0) {
            delete [] ret;
            ret = nullptr;
        }
    } while (false);

    return ret;
}

const CVobSubFile::SubPos* CVobSubFile::GetFrameInfo(size_t idx, size_t nLang /*= SIZE_T_ERROR*/) const
{
    if (nLang >= m_langs.size()) {
        nLang = m_nLang;
    }
    const std::vector<SubPos>& sp = m_langs[nLang].subpos;

    if (idx >= sp.size()
            || !sp[idx].bValid
            || (m_bOnlyShowForcedSubs && !sp[idx].bForced)) {
        return nullptr;
    }

    return &sp[idx];
}

bool CVobSubFile::GetFrame(size_t idx, size_t nLang /*= SIZE_T_ERROR*/, REFERENCE_TIME rt /*= -1*/)
{
    if (nLang >= m_langs.size()) {
        nLang = m_nLang;
    }
    std::vector<SubPos>& sp = m_langs[nLang].subpos;

    if (idx >= sp.size()) {
        return false;
    }

    if (m_img.nLang != nLang || m_img.nIdx != idx
            || (sp[idx].bAnimated && sp[idx].start + m_img.tCurrent <= rt)) {
        size_t packetSize = 0, dataSize = 0;
        std::unique_ptr<BYTE[]> buff;
        buff.reset(GetPacket(idx, packetSize, dataSize, nLang));
        if (!buff || packetSize == 0 || dataSize == 0) {
            return false;
        }

        m_img.start = sp[idx].start;

        bool ret = m_img.Decode(buff.get(), packetSize, dataSize, rt >= 0 ? int(rt - sp[idx].start) : INT_MAX,
                                m_bCustomPal, m_tridx, m_orgpal, m_cuspal, true);

        m_img.delay = sp[idx].stop - sp[idx].start;

        if (!ret) {
            return false;
        }

        m_img.nIdx = idx;
        m_img.nLang = nLang;
    }

    return (m_bOnlyShowForcedSubs ? m_img.bForced : true);
}

bool CVobSubFile::GetFrameByTimeStamp(__int64 time)
{
    return GetFrame(GetFrameIdxByTimeStamp(time));
}

size_t CVobSubFile::GetFrameIdxByTimeStamp(__int64 time)
{
    if (m_nLang >= m_langs.size() || m_langs[m_nLang].subpos.empty()) {
        return SIZE_T_ERROR;
    }

    std::vector<SubPos>& sp = m_langs[m_nLang].subpos;

    size_t i = 0, j = sp.size() - 1, ret = SIZE_T_ERROR;

    if (time >= sp[j].start) {
        return j;
    }

    while (i < j) {
        size_t mid = (i + j) >> 1;
        __int64 midstart = sp[mid].start;

        if (time == midstart) {
            ret = mid;
            break;
        } else if (time < midstart) {
            ret = SIZE_T_ERROR;
            if (j == mid) {
                mid--;
            }
            j = mid;
        } else if (time > midstart) {
            ret = mid;
            if (i == mid) {
                mid++;
            }
            i = mid;
        }
    }

    return ret;
}

//

STDMETHODIMP CVobSubFile::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
    CheckPointer(ppv, E_POINTER);
    *ppv = nullptr;

    return
        QI(IPersist)
        QI(ISubStream)
        QI(ISubPicProvider)
        __super::NonDelegatingQueryInterface(riid, ppv);
}

// ISubPicProvider

STDMETHODIMP_(POSITION) CVobSubFile::GetStartPosition(REFERENCE_TIME rt, double fps)
{
    rt /= 10000;

    size_t i = GetFrameIdxByTimeStamp(rt);

    const SubPos* sp = GetFrameInfo(i);
    if (!sp) {
        return nullptr;
    }

    if (rt >= sp->stop) {
        if (!GetFrameInfo(++i)) {
            return nullptr;
        }
    }

    return (POSITION)(i + 1);
}

tstring CVobSubFile::GetPath() {
    return m_path;
}

STDMETHODIMP_(POSITION) CVobSubFile::GetNext(POSITION pos)
{
    size_t i = (size_t)pos;
    return (GetFrameInfo(i) ? (POSITION)(i + 1) : nullptr);
}

STDMETHODIMP_(REFERENCE_TIME) CVobSubFile::GetStart(POSITION pos, double fps)
{
    size_t i = (size_t)pos - 1;
    const SubPos* sp = GetFrameInfo(i);
    return (sp ? 10000i64 * sp->start : 0);
}

STDMETHODIMP_(REFERENCE_TIME) CVobSubFile::GetStop(POSITION pos, double fps)
{
    size_t i = (size_t)pos - 1;
    const SubPos* sp = GetFrameInfo(i);
    return (sp ? 10000i64 * sp->stop : 0);
}

STDMETHODIMP_(bool) CVobSubFile::IsAnimated(POSITION pos)
{
    size_t i = (size_t)pos - 1;
    const SubPos* sp = GetFrameInfo(i);
    return (sp ? sp->bAnimated : false);
}

STDMETHODIMP CVobSubFile::Render(SubPicDesc& spd, REFERENCE_TIME rt, double fps, RECT& bbox)
{
    if (spd.bpp != 32) {
        return E_INVALIDARG;
    }

    rt /= 10000;

    if (!GetFrame(GetFrameIdxByTimeStamp(rt), SIZE_T_ERROR, rt)) {
        return E_FAIL;
    }

    if (rt >= (m_img.start + m_img.delay)) {
        return E_FAIL;
    }

    return __super::Render(spd, bbox);
}

STDMETHODIMP CVobSubFile::GetRelativeTo(POSITION pos, RelativeTo& relativeTo)
{
    relativeTo = VIDEO;
    return S_OK;
}

// IPersist

STDMETHODIMP CVobSubFile::GetClassID(CLSID* pClassID)
{
    return pClassID ? *pClassID = __uuidof(this), S_OK : E_POINTER;
}

// ISubStream

STDMETHODIMP_(int) CVobSubFile::GetStreamCount()
{
    int iStreamCount = 0;
    for (const auto& sl : m_langs) {
        if (sl.subpos.size()) {
            iStreamCount++;
        }
    }
    return iStreamCount;
}

DWORD LangIDToLCID(WORD langid)
{
    unsigned short id = lang_tbl[find_lang(langid)].id;
    CHAR tmp[3];
    tmp[0] = id / 256;
    tmp[1] = id & 0xFF;
    tmp[2] = 0;
    return ISOLang::ISO6391ToLcid(tmp);
}

STDMETHODIMP CVobSubFile::GetStreamInfo(int iStream, WCHAR** ppName, LCID* pLCID)
{
    for (const auto& sl : m_langs) {
        if (sl.subpos.empty() || iStream-- > 0) {
            continue;
        }

        if (ppName) {
            *ppName = (WCHAR*)CoTaskMemAlloc((int(sl.alt.size()) + 1) * sizeof(WCHAR));
            if (!(*ppName)) {
                return E_OUTOFMEMORY;
            }

            wcscpy_s(*ppName, int(sl.alt.size()) + 1, std::wstring(sl.alt).c_str());
        }

        if (pLCID) {
            *pLCID = LangIDToLCID(sl.id);
        }

        return S_OK;
    }

    return E_FAIL;
}

STDMETHODIMP_(int) CVobSubFile::GetStream()
{
    int iStream = 0;

    if (m_nLang < m_langs.size()) {
        for (size_t i = 0; i < m_nLang; i++) {
            if (!m_langs[i].subpos.empty()) {
                iStream++;
            }
        }
    }

    return iStream;
}

STDMETHODIMP CVobSubFile::SetStream(int iStream)
{
    for (size_t i = 0; i < m_langs.size(); i++) {
        const std::vector<SubPos>& sp = m_langs[i].subpos;

        if (sp.empty() || iStream-- > 0) {
            continue;
        }

        m_nLang = i;

        m_img.Invalidate();

        break;
    }

    return iStream < 0 ? S_OK : E_FAIL;
}

STDMETHODIMP CVobSubFile::Reload()
{
    if (!PathUtils::Exists((m_title + _T(".idx")).c_str())) {
        return E_FAIL;
    }
    return !m_title.empty() && Open(m_title) ? S_OK : E_FAIL;
}

// StretchBlt

static void PixelAtBiLinear(RGBQUAD& c, int x, int y, CVobSubImage& src)
{
    int w = RectWidth(src.rect),
        h = RectHeight(src.rect);

    int x1 = (x >> 16), y1 = (y >> 16) * w,
        x2 = std::min(x1 + 1, w - 1), y2 = std::min(y1 + w, (h - 1) * w);

    RGBQUAD* ptr = src.lpPixels;

    RGBQUAD c11 = ptr[y1 + x1], c12 = ptr[y1 + x2],
            c21 = ptr[y2 + x1], c22 = ptr[y2 + x2];

    __int64 u2 = x & 0xffff,
            v2 = y & 0xffff,
            u1 = 0x10000 - u2,
            v1 = 0x10000 - v2;

    int v1u1 = int(v1 * u1 >> 16) * c11.rgbReserved,
        v1u2 = int(v1 * u2 >> 16) * c12.rgbReserved,
        v2u1 = int(v2 * u1 >> 16) * c21.rgbReserved,
        v2u2 = int(v2 * u2 >> 16) * c22.rgbReserved;

    c.rgbRed = (c11.rgbRed * v1u1 + c12.rgbRed * v1u2
                + c21.rgbRed * v2u1 + c22.rgbRed * v2u2) >> 24;
    c.rgbGreen = (c11.rgbGreen * v1u1 + c12.rgbGreen * v1u2
                  + c21.rgbGreen * v2u1 + c22.rgbGreen * v2u2) >> 24;
    c.rgbBlue = (c11.rgbBlue * v1u1 + c12.rgbBlue * v1u2
                 + c21.rgbBlue * v2u1 + c22.rgbBlue * v2u2) >> 24;
    c.rgbReserved = BYTE((v1u1 + v1u2
                          + v2u1 + v2u2) >> 16);
}

static void StretchBlt(SubPicDesc& spd, RECT dstrect, CVobSubImage& src)
{
    if (::IsRectEmpty(&dstrect)) {
        return;
    }

    if (RectIsEmpty(RectIntersect(dstrect, MakeRect(0, 0, spd.w, spd.h)))) {
        return;
    }

    int sw = RectWidth(src.rect),
        sh = RectHeight(src.rect),
        dw = RectWidth(dstrect),
        dh = RectHeight(dstrect);

    int srcx = 0,
        srcy = 0,
        srcdx = (sw << 16) / dw >> 1,
        srcdy = (sh << 16) / dh >> 1;

    if (dstrect.left < 0) {
        srcx = -dstrect.left * (srcdx << 1);
        dstrect.left = 0;
    }
    if (dstrect.top < 0) {
        srcy = -dstrect.top * (srcdy << 1);
        dstrect.top = 0;
    }
    if (dstrect.right > spd.w) {
        dstrect.right = spd.w;
    }
    if (dstrect.bottom > spd.h) {
        dstrect.bottom = spd.h;
    }

    if (RectIsEmpty(RectIntersect(dstrect, MakeRect(0, 0, spd.w, spd.h)))) {
        return;
    }

    dw = RectWidth(dstrect);
    dh = RectHeight(dstrect);

    for (int y = dstrect.top; y < dstrect.bottom; y++, srcy += (srcdy << 1)) {
        RGBQUAD* ptr = (RGBQUAD*) & ((BYTE*)spd.bits)[y * spd.pitch] + dstrect.left;
        RGBQUAD* endptr = ptr + dw;

        for (int sx = srcx; ptr < endptr; sx += (srcdx << 1), ptr++) {
            //          PixelAtBiLinear(*ptr, sx, srcy, src);
            ////
            RGBQUAD cc[4];

            PixelAtBiLinear(cc[0], sx,         srcy,         src);
            PixelAtBiLinear(cc[1], sx + srcdx, srcy,         src);
            PixelAtBiLinear(cc[2], sx,         srcy + srcdy, src);
            PixelAtBiLinear(cc[3], sx + srcdx, srcy + srcdy, src);

            ptr->rgbRed = (cc[0].rgbRed + cc[1].rgbRed + cc[2].rgbRed + cc[3].rgbRed) >> 2;
            ptr->rgbGreen = (cc[0].rgbGreen + cc[1].rgbGreen + cc[2].rgbGreen + cc[3].rgbGreen) >> 2;
            ptr->rgbBlue = (cc[0].rgbBlue + cc[1].rgbBlue + cc[2].rgbBlue + cc[3].rgbBlue) >> 2;
            ptr->rgbReserved = (cc[0].rgbReserved + cc[1].rgbReserved + cc[2].rgbReserved + cc[3].rgbReserved) >> 2;
            ////
            ptr->rgbRed = ptr->rgbRed * ptr->rgbReserved >> 8;
            ptr->rgbGreen = ptr->rgbGreen * ptr->rgbReserved >> 8;
            ptr->rgbBlue = ptr->rgbBlue * ptr->rgbReserved >> 8;
            ptr->rgbReserved = ~ptr->rgbReserved;

        }
    }
}

//
// CVobSubSettings
//

void CVobSubSettings::InitSettings()
{
    m_size = MakeSize(720, 480);
    m_toff = m_x = m_y = 0;
    m_org = MakePoint(0, 0);
    m_scale_x = m_scale_y = m_alpha = 100;
    m_fadein = m_fadeout = 50;
    m_iSmooth = 0;
    m_bAlign = false;
    m_alignhor = m_alignver = 0;
    m_bOnlyShowForcedSubs = false;
    m_bCustomPal = false;
    m_tridx = 0;
    ZeroMemory(m_orgpal, sizeof(m_orgpal));
    ZeroMemory(m_cuspal, sizeof(m_cuspal));
}

bool CVobSubSettings::GetCustomPal(RGBQUAD* cuspal, int& tridx)
{
    memcpy(cuspal, m_cuspal, sizeof(RGBQUAD) * 4);
    tridx = m_tridx;
    return m_bCustomPal;
}

void CVobSubSettings::SetCustomPal(const RGBQUAD* cuspal, int tridx)
{
    memcpy(m_cuspal, cuspal, sizeof(RGBQUAD) * 4);
    m_tridx = tridx & 0xf;
    for (int i = 0; i < 4; i++) {
        m_cuspal[i].rgbReserved = (tridx & (1 << i)) ? 0 : 0xff;
    }
    m_img.Invalidate();
}

void CVobSubSettings::GetDestrect(RECT& r)
{
    int w = MulDiv(RectWidth(m_img.rect), m_scale_x, 100);
    int h = MulDiv(RectHeight(m_img.rect), m_scale_y, 100);

    if (!m_bAlign) {
        r.left = MulDiv(m_img.rect.left, m_scale_x, 100);
        r.right = MulDiv(m_img.rect.right, m_scale_x, 100);
        r.top = MulDiv(m_img.rect.top, m_scale_y, 100);
        r.bottom = MulDiv(m_img.rect.bottom, m_scale_y, 100);
    } else {
        switch (m_alignhor) {
            case 0:
                r.left = 0;
                r.right = w;
                break; // left
            case 1:
                r.left = -(w >> 1);
                r.right = -(w >> 1) + w;
                break; // center
            case 2:
                r.left = -w;
                r.right = 0;
                break; // right
            default:
                r.left = MulDiv(m_img.rect.left, m_scale_x, 100);
                r.right = MulDiv(m_img.rect.right, m_scale_x, 100);
                break;
        }

        switch (m_alignver) {
            case 0:
                r.top = 0;
                r.bottom = h;
                break; // top
            case 1:
                r.top = -(h >> 1);
                r.bottom = -(h >> 1) + h;
                break; // center
            case 2:
                r.top = -h;
                r.bottom = 0;
                break; // bottom
            default:
                r.top = MulDiv(m_img.rect.top, m_scale_y, 100);
                r.bottom = MulDiv(m_img.rect.bottom, m_scale_y, 100);
                break;
        }
    }

    ::OffsetRect(&r, m_org.x, m_org.y);
}

void CVobSubSettings::GetDestrect(RECT& r, int w, int h)
{
    GetDestrect(r);

    r.left = MulDiv(r.left, w, m_size.cx);
    r.right = MulDiv(r.right, w, m_size.cx);
    r.top = MulDiv(r.top, h, m_size.cy);
    r.bottom = MulDiv(r.bottom, h, m_size.cy);
}

void CVobSubSettings::SetAlignment(bool bAlign, int x, int y, int hor /*= 1*/, int ver /*= 1*/)
{
    m_bAlign = bAlign;
    if (bAlign) {
        m_org.x = MulDiv(m_size.cx, x, 100);
        m_org.y = MulDiv(m_size.cy, y, 100);
        m_alignhor = std::min(std::max(hor, 0), 2);
        m_alignver = std::min(std::max(ver, 0), 2);
    } else {
        m_org.x = m_x;
        m_org.y = m_y;
    }
}

HRESULT CVobSubSettings::Render(SubPicDesc& spd, RECT& bbox)
{
    RECT r = {};
    GetDestrect(r, spd.w, spd.h);
    StretchBlt(spd, r, m_img);
    /*
        CRenderedTextSubtitle rts(nullptr);
        rts.CreateDefaultStyle(DEFAULT_CHARSET);
        rts.m_storageRes = MakeSize(m_size.cx, m_size.cy);
        std::wstring assstr;
        m_img.Polygonize(assstr, false);
        REFERENCE_TIME rtStart = 10000i64*m_img.start, rtStop = 10000i64*(m_img.start+m_img.delay);
        rts.Add(assstr, true, rtStart, rtStop);
        rts.Render(spd, (rtStart+rtStop)/2, 25, r);
    */
    r = RectIntersect(r, MakeRect(MakePoint(0, 0), MakeSize(spd.w, spd.h)));
    bbox = r;
    return !::IsRectEmpty(&r) ? S_OK : S_FALSE;
}

/////////////////////////////////////////////////////////

static bool CompressFile(tstring fn)
{
    BOOL b = FALSE;

    HANDLE h = CreateFile(fn.c_str(), GENERIC_WRITE | GENERIC_READ, FILE_SHARE_READ, 0, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, 0);
    if (h != INVALID_HANDLE_VALUE) {
        unsigned short us = COMPRESSION_FORMAT_DEFAULT;
        DWORD nBytesReturned;
        b = DeviceIoControl(h, FSCTL_SET_COMPRESSION, (LPVOID)&us, 2, nullptr, 0, (LPDWORD)&nBytesReturned, nullptr);
        CloseHandle(h);
    }

    return !!b;
}

bool CVobSubFile::SaveVobSub(tstring fn, int delay)
{
    if (!WriteIdx(fn + _T(".idx"), delay) || !WriteSub(fn + _T(".sub"))) {
        return false;
    }
    m_path = fn + _T(".idx");
    return true;
}

bool CVobSubFile::SaveWinSubMux(tstring fn, int delay)
{
    TrimExtension(fn);

    FilePtr f = OpenFile(fn + _T(".sub"), L"wt", _SH_DENYWR);
    if (!f) {
        return false;
    }

    m_img.Invalidate();

    std::unique_ptr<BYTE[]> p4bpp(new(std::nothrow) BYTE[720 * 576 / 2]);
    if (!p4bpp) {
        return false;
    }

    std::vector<SubPos>& sp = m_langs[m_nLang].subpos;
    for (size_t i = 0; i < sp.size(); i++) {
        if (!GetFrame(i)) {
            continue;
        }

        int pal[4] = {0, 1, 2, 3};

        for (int j = 0; j < 5; j++) {
            if (j == 4 || !m_img.pal[j].tr) {
                j &= 3;
                memset(p4bpp.get(), (j << 4) | j, 720 * 576 / 2);
                pal[j] ^= pal[0], pal[0] ^= pal[j], pal[j] ^= pal[0];
                break;
            }
        }

        int tr[4] = {m_img.pal[pal[0]].tr, m_img.pal[pal[1]].tr, m_img.pal[pal[2]].tr, m_img.pal[pal[3]].tr};

        DWORD uipal[4 + 12];

        if (!m_bCustomPal) {
            uipal[0] = *((DWORD*)&m_img.orgpal[m_img.pal[pal[0]].pal]);
            uipal[1] = *((DWORD*)&m_img.orgpal[m_img.pal[pal[1]].pal]);
            uipal[2] = *((DWORD*)&m_img.orgpal[m_img.pal[pal[2]].pal]);
            uipal[3] = *((DWORD*)&m_img.orgpal[m_img.pal[pal[3]].pal]);
        } else {
            uipal[0] = *((DWORD*)&m_img.cuspal[pal[0]]) & 0xffffff;
            uipal[1] = *((DWORD*)&m_img.cuspal[pal[1]]) & 0xffffff;
            uipal[2] = *((DWORD*)&m_img.cuspal[pal[2]]) & 0xffffff;
            uipal[3] = *((DWORD*)&m_img.cuspal[pal[3]]) & 0xffffff;
        }

        std::map<DWORD, BYTE> palmap;
        palmap[uipal[0]] = 0;
        palmap[uipal[1]] = 1;
        palmap[uipal[2]] = 2;
        palmap[uipal[3]] = 3;

        uipal[0] = 0xff; // blue background

        int w = RectWidth(m_img.rect) - 2;
        int h = RectHeight(m_img.rect) - 2;
        int pitch = (((w + 1) >> 1) + 3) & ~3;

        for (ptrdiff_t y = 0; y < h; y++) {
            DWORD* p = (DWORD*)&m_img.lpPixels[(y + 1) * (w + 2) + 1];

            for (ptrdiff_t x = 0; x < w; x++, p++) {
                BYTE c = 0;

                if (*p & 0xff000000) {
                    DWORD uic = *p & 0xffffff;
                    Lookup(palmap, uic, c);
                }

                BYTE& c4bpp = p4bpp[(h - y - 1) * pitch + (x >> 1)];
                c4bpp = (x & 1) ? ((c4bpp & 0xf0) | c) : ((c4bpp & 0x0f) | (c << 4));
            }
        }

        int t1 = (int)m_img.start + delay;
        int t2 = t1 + (int)m_img.delay /*+ (m_size.cy==480?(1000/29.97+1):(1000/25))*/;

        ASSERT(t2 > t1);

        if (t2 <= 0) {
            continue;
        }
        if (t1 < 0) {
            t1 = 0;
        }

        tstring bmpfn;
        bmpfn = ::Format(_T("%s_%06Iu.bmp"), fn.c_str(), i + 1);

        tstring str;
        str = ::Format(_T("%s\t%02d:%02d:%02d:%02d %02d:%02d:%02d:%02d\t%03d %03d %03d %03d %d %d %d %d\n"), bmpfn.c_str(), t1 / 1000 / 60 / 60, (t1 / 1000 / 60) % 60, (t1 / 1000) % 60, (t1 % 1000) / 10, t2 / 1000 / 60 / 60, (t2 / 1000 / 60) % 60, (t2 / 1000) % 60, (t2 % 1000) / 10, RectWidth(m_img.rect), RectHeight(m_img.rect), m_img.rect.left, m_img.rect.top, (tr[0] << 4) | tr[0], (tr[1] << 4) | tr[1], (tr[2] << 4) | tr[2], (tr[3] << 4) | tr[3]);
        _fputts(str.c_str(), f.get());

        BITMAPFILEHEADER fhdr = {
            0x4d42,
            sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + 16 * sizeof(RGBQUAD) + pitch * h,
            0, 0,
            sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + 16 * sizeof(RGBQUAD)
        };

        BITMAPINFOHEADER ihdr = {
            sizeof(BITMAPINFOHEADER),
            w, h, 1, 4, 0,
            0,
            pitch * h, 0,
            16, 4
        };

        if (FilePtr bmp = OpenFile(bmpfn, L"wb")) {
            fwrite(&fhdr, 1, sizeof(fhdr), bmp.get());
            fwrite(&ihdr, 1, sizeof(ihdr), bmp.get());
            fwrite(uipal, 1, sizeof(RGBQUAD) * 16, bmp.get());
            fwrite(p4bpp.get(), 1, pitch * h, bmp.get());
            bmp.reset();

            CompressFile(bmpfn);
        }
    }

    m_path = fn + _T(".sub");

    return true;
}

bool CVobSubFile::SaveScenarist(tstring fn, int delay)
{
    TrimExtension(fn);

    FilePtr f = OpenFile(fn + _T(".sst"), L"wt", _SH_DENYWR);
    if (!f) {
        return false;
    }

    m_img.Invalidate();

    StringReplace(fn, '\\', '/');
    tstring title = StringMid(fn, StringRFind(fn, '/') + 1);

    TCHAR buff[MAX_PATH], * pFilePart = buff;
    if (GetFullPathName(fn.c_str(), MAX_PATH, buff, &pFilePart) == 0) {
        return false;
    }

    tstring fullpath = StringLeft(tstring(buff), int(pFilePart - buff));
    StringTrimRight(fullpath, _T("\\/"));
    if (fullpath.empty()) {
        return false;
    }

    tstring str, str2;
    str += _T("st_format\t2\n");
    str += _T("Display_Start\t%s\n");
    str += _T("TV_Type\t\t%s\n");
    str += _T("Tape_Type\tNON_DROP\n");
    str += _T("Pixel_Area\t(0 %d)\n");
    str += _T("Directory\t%s\n");
    str += _T("Subtitle\t%s\n");
    str += _T("Display_Area\t(0 2 719 %d)\n");
    str += _T("Contrast\t(15 15 15 0)\n");
    str += _T("\n");
    str += _T("PA\t(0 0 255 - - - )\n");
    str += _T("E1\t(255 0 0 - - - )\n");
    str += _T("E2\t(0 0 0 - - - )\n");
    str += _T("BG\t(255 255 255 - - - )\n");
    str += _T("\n");
    str += _T("SP_NUMBER\tSTART\tEND\tFILE_NAME\n");
    str2 = ::Format(str, !m_bOnlyShowForcedSubs ? _T("non_forced") : _T("forced"), m_size.cy == 480 ? _T("NTSC") : _T("PAL"), m_size.cy - 3, fullpath.c_str(), title.c_str(), m_size.cy == 480 ? 479 : 574);

    _fputts(str2.c_str(), f.get());

    fflush(f.get());

    RGBQUAD pal[16] = {
        {255, 0, 0, 0},
        {0, 0, 255, 0},
        {0, 0, 0, 0},
        {255, 255, 255, 0},
        {0, 255, 0, 0},
        {255, 0, 255, 0},
        {0, 255, 255, 0},
        {125, 125, 0, 0},
        {125, 125, 125, 0},
        {225, 225, 225, 0},
        {0, 0, 125, 0},
        {0, 125, 0, 0},
        {125, 0, 0, 0},
        {255, 0, 222, 0},
        {0, 125, 222, 0},
        {125, 0, 125, 0},
    };

    BITMAPFILEHEADER fhdr = {
        0x4d42,
        sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + 16 * sizeof(RGBQUAD) + 360 * (m_size.cy - 2),
        0, 0,
        sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + 16 * sizeof(RGBQUAD)
    };

    BITMAPINFOHEADER ihdr = {
        sizeof(BITMAPINFOHEADER),
        720, m_size.cy - 2, 1, 4, 0,
        DWORD(360 * (m_size.cy - 2)),
        0, 0,
        16, 4
    };

    bool bCustomPal = m_bCustomPal;
    m_bCustomPal = true;
    RGBQUAD tempCusPal[4], newCusPal[4 + 12] = {{255, 0, 0, 0}, {0, 0, 255, 0}, {0, 0, 0, 0}, {255, 255, 255, 0}};
    memcpy(tempCusPal, m_cuspal, sizeof(tempCusPal));
    memcpy(m_cuspal, newCusPal, sizeof(m_cuspal));

    std::unique_ptr<BYTE[]> p4bpp(new(std::nothrow) BYTE[(m_size.cy - 2) * 360]);
    if (!p4bpp) {
        return false;
    }

    BYTE colormap[16];

    for (size_t i = 0; i < 16; i++) {
        BYTE idx = 0;
        int maxdif = 255 * 255 * 3 + 1;

        for (size_t j = 0; j < 16 && maxdif; j++) {
            int rdif = pal[j].rgbRed - m_orgpal[i].rgbRed;
            int gdif = pal[j].rgbGreen - m_orgpal[i].rgbGreen;
            int bdif = pal[j].rgbBlue - m_orgpal[i].rgbBlue;

            int dif = rdif * rdif + gdif * gdif + bdif * bdif;
            if (dif < maxdif) {
                maxdif = dif;
                idx = (BYTE)j;
            }
        }

        colormap[i] = idx + 1;
    }

    int pc[4] = {1, 1, 1, 1}, pa[4] = {15, 15, 15, 0};

    std::vector<SubPos>& sp = m_langs[m_nLang].subpos;
    for (size_t i = 0, k = 0; i < sp.size(); i++) {
        if (!GetFrame(i)) {
            continue;
        }

        for (int j = 0; j < 5; j++) {
            if (j == 4 || !m_img.pal[j].tr) {
                j &= 3;
                memset(p4bpp.get(), (j << 4) | j, (m_size.cy - 2) * 360);
                break;
            }
        }

        for (ptrdiff_t y = std::max(m_img.rect.top + 1, 2l); y < m_img.rect.bottom - 1; y++) {
            ASSERT(m_size.cy - y - 1 >= 0);
            if (m_size.cy - y - 1 < 0) {
                break;
            }

            DWORD* p = (DWORD*)&m_img.lpPixels[(y - m_img.rect.top) * RectWidth(m_img.rect) + 1];

            for (ptrdiff_t x = m_img.rect.left + 1; x < m_img.rect.right - 1; x++, p++) {
                DWORD rgb = *p & 0xffffff;
                BYTE c = rgb == 0x0000ff ? 0 : rgb == 0xff0000 ? 1 : rgb == 0x000000 ? 2 : 3;
                BYTE& c4bpp = p4bpp[(m_size.cy - y - 1) * 360 + (x >> 1)];
                c4bpp = (x & 1) ? ((c4bpp & 0xf0) | c) : ((c4bpp & 0x0f) | (c << 4));
            }
        }

        tstring bmpfn;
        bmpfn = ::Format(_T("%s_%04Iu.bmp"), fn.c_str(), i + 1);
        title = StringMid(bmpfn, StringRFind(bmpfn, '/') + 1);

        // E1, E2, P, Bg
        int c[4] = {colormap[m_img.pal[1].pal], colormap[m_img.pal[2].pal], colormap[m_img.pal[0].pal], colormap[m_img.pal[3].pal]};
        c[0] ^= c[1], c[1] ^= c[0], c[0] ^= c[1];

        if (memcmp(pc, c, sizeof(c))) {
            memcpy(pc, c, sizeof(c));
            str = ::Format(_T("Color\t (%d %d %d %d)\n"), c[0], c[1], c[2], c[3]);
            _fputts(str.c_str(), f.get());
        }

        // E1, E2, P, Bg
        int a[4] = {m_img.pal[1].tr, m_img.pal[2].tr, m_img.pal[0].tr, m_img.pal[3].tr};
        a[0] ^= a[1], a[1] ^= a[0], a[0] ^= a[1];

        if (memcmp(pa, a, sizeof(a))) {
            memcpy(pa, a, sizeof(a));
            str = ::Format(_T("Contrast (%d %d %d %d)\n"), a[0], a[1], a[2], a[3]);
            _fputts(str.c_str(), f.get());
        }

        int t1 = (int)sp[i].start + delay;
        int h1 = t1 / 1000 / 60 / 60, m1 = (t1 / 1000 / 60) % 60, s1 = (t1 / 1000) % 60;
        int f1 = (int)((m_size.cy == 480 ? 29.97 : 25) * (t1 % 1000) / 1000);

        int t2 = (int)sp[i].stop + delay;
        int h2 = t2 / 1000 / 60 / 60, m2 = (t2 / 1000 / 60) % 60, s2 = (t2 / 1000) % 60;
        int f2 = (int)((m_size.cy == 480 ? 29.97 : 25) * (t2 % 1000) / 1000);

        if (t2 <= 0) {
            continue;
        }
        if (t1 < 0) {
            t1 = 0;
        }

        if (h1 == h2 && m1 == m2 && s1 == s2 && f1 == f2) {
            f2++;
            if (f2 == (m_size.cy == 480 ? 30 : 25)) {
                f2 = 0;
                s2++;
                if (s2 == 60) {
                    s2 = 0;
                    m2++;
                    if (m2 == 60) {
                        m2 = 0;
                        h2++;
                    }
                }
            }
        }

        if (i + 1 < sp.size()) {
            int t3 = (int)sp[i + 1].start + delay;
            int h3 = t3 / 1000 / 60 / 60, m3 = (t3 / 1000 / 60) % 60, s3 = (t3 / 1000) % 60;
            int f3 = (int)((m_size.cy == 480 ? 29.97 : 25) * (t3 % 1000) / 1000);

            if (h3 == h2 && m3 == m2 && s3 == s2 && f3 == f2) {
                f2--;
                if (f2 == -1) {
                    f2 = (m_size.cy == 480 ? 29 : 24);
                    s2--;
                    if (s2 == -1) {
                        s2 = 59;
                        m2--;
                        if (m2 == -1) {
                            m2 = 59;
                            if (h2 > 0) {
                                h2--;
                            }
                        }
                    }
                }
            }
        }

        if (h1 == h2 && m1 == m2 && s1 == s2 && f1 == f2) {
            continue;
        }

        str = ::Format(_T("%04u\t%02d:%02d:%02d:%02d\t%02d:%02d:%02d:%02d\t%s\n"), ++k, h1, m1, s1, f1, h2, m2, s2, f2, title.c_str());
        _fputts(str.c_str(), f.get());

        if (FilePtr bmp = OpenFile(bmpfn, L"wb")) {
            fwrite(&fhdr, 1, sizeof(fhdr), bmp.get());
            fwrite(&ihdr, 1, sizeof(ihdr), bmp.get());
            fwrite(newCusPal, 1, sizeof(RGBQUAD) * 16, bmp.get());
            fwrite(p4bpp.get(), 1, 360 * (m_size.cy - 2), bmp.get());
            bmp.reset();

            CompressFile(bmpfn);
        }
    }

    m_bCustomPal = bCustomPal;
    memcpy(m_cuspal, tempCusPal, sizeof(m_cuspal));

    m_path = fn + _T(".sst");

    return true;
}

bool CVobSubFile::SaveMaestro(tstring fn, int delay)
{
    TrimExtension(fn);

    FilePtr f = OpenFile(fn + _T(".son"), L"wt", _SH_DENYWR);
    if (!f) {
        return false;
    }

    m_img.Invalidate();

    StringReplace(fn, '\\', '/');
    tstring title = StringMid(fn, StringRFind(fn, '/') + 1);

    TCHAR buff[MAX_PATH], * pFilePart = buff;
    if (GetFullPathName(fn.c_str(), MAX_PATH, buff, &pFilePart) == 0) {
        return false;
    }

    tstring fullpath = StringLeft(tstring(buff), int(pFilePart - buff));
    StringTrimRight(fullpath, _T("\\/"));
    if (fullpath.empty()) {
        return false;
    }

    tstring str, str2;
    str += _T("st_format\t2\n");
    str += _T("Display_Start\t%s\n");
    str += _T("TV_Type\t\t%s\n");
    str += _T("Tape_Type\tNON_DROP\n");
    str += _T("Pixel_Area\t(0 %d)\n");
    str += _T("Directory\t%s\n");
    str += _T("Subtitle\t%s\n");
    str += _T("Display_Area\t(0 2 719 %d)\n");
    str += _T("Contrast\t(15 15 15 0)\n");
    str += _T("\n");
    str += _T("SP_NUMBER\tSTART\tEND\tFILE_NAME\n");
    str2 = ::Format(str, !m_bOnlyShowForcedSubs ? _T("non_forced") : _T("forced"), m_size.cy == 480 ? _T("NTSC") : _T("PAL"), m_size.cy - 3, fullpath.c_str(), title.c_str(), m_size.cy == 480 ? 479 : 574);

    _fputts(str2.c_str(), f.get());

    fflush(f.get());

    BITMAPFILEHEADER fhdr = {
        0x4d42,
        sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + 16 * sizeof(RGBQUAD) + 360 * (m_size.cy - 2),
        0, 0,
        sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + 16 * sizeof(RGBQUAD)
    };

    BITMAPINFOHEADER ihdr = {
        sizeof(BITMAPINFOHEADER),
        720, m_size.cy - 2, 1, 4, 0,
        DWORD(360 * (m_size.cy - 2)),
        0, 0,
        16, 4
    };

    bool bCustomPal = m_bCustomPal;
    m_bCustomPal = true;
    RGBQUAD tempCusPal[4], newCusPal[4 + 12] = {{255, 0, 0, 0}, {0, 0, 255, 0}, {0, 0, 0, 0}, {255, 255, 255, 0}};
    memcpy(tempCusPal, m_cuspal, sizeof(tempCusPal));
    memcpy(m_cuspal, newCusPal, sizeof(m_cuspal));

    std::unique_ptr<BYTE[]> p4bpp(new(std::nothrow) BYTE[(m_size.cy - 2) * 360]);
    if (!p4bpp) {
        return false;
    }

    BYTE colormap[16];
    for (BYTE i = 0; i < 16; i++) {
        colormap[i] = i;
    }

    if (FilePtr spf = OpenFile(fn + _T(".spf"), L"wb")) {
        for (size_t i = 0; i < 16; i++) {
            COLORREF c = (m_orgpal[i].rgbBlue << 16) | (m_orgpal[i].rgbGreen << 8) | m_orgpal[i].rgbRed;
            fwrite(&c, 1, sizeof(COLORREF), spf.get());
        }

        spf.reset();
    }

    int pc[4] = {1, 1, 1, 1}, pa[4] = {15, 15, 15, 0};

    std::vector<SubPos>& sp = m_langs[m_nLang].subpos;
    for (size_t i = 0, k = 0; i < sp.size(); i++) {
        if (!GetFrame(i)) {
            continue;
        }

        for (int j = 0; j < 5; j++) {
            if (j == 4 || !m_img.pal[j].tr) {
                j &= 3;
                memset(p4bpp.get(), (j << 4) | j, (m_size.cy - 2) * 360);
                break;
            }
        }

        for (ptrdiff_t y = std::max(m_img.rect.top + 1, 2l); y < m_img.rect.bottom - 1; y++) {
            ASSERT(m_size.cy - y - 1 >= 0);
            if (m_size.cy - y - 1 < 0) {
                break;
            }

            DWORD* p = (DWORD*)&m_img.lpPixels[(y - m_img.rect.top) * RectWidth(m_img.rect) + 1];

            for (ptrdiff_t x = m_img.rect.left + 1; x < m_img.rect.right - 1; x++, p++) {
                DWORD rgb = *p & 0xffffff;
                BYTE c = rgb == 0x0000ff ? 0 : rgb == 0xff0000 ? 1 : rgb == 0x000000 ? 2 : 3;
                BYTE& c4bpp = p4bpp[(m_size.cy - y - 1) * 360 + (x >> 1)];
                c4bpp = (x & 1) ? ((c4bpp & 0xf0) | c) : ((c4bpp & 0x0f) | (c << 4));
            }
        }

        tstring bmpfn;
        bmpfn = ::Format(_T("%s_%04Iu.bmp"), fn.c_str(), i + 1);
        title = StringMid(bmpfn, StringRFind(bmpfn, '/') + 1);

        // E1, E2, P, Bg
        int c[4] = {colormap[m_img.pal[1].pal], colormap[m_img.pal[2].pal], colormap[m_img.pal[0].pal], colormap[m_img.pal[3].pal]};

        if (memcmp(pc, c, sizeof(c))) {
            memcpy(pc, c, sizeof(c));
            str = ::Format(_T("Color\t (%d %d %d %d)\n"), c[0], c[1], c[2], c[3]);
            _fputts(str.c_str(), f.get());
        }

        // E1, E2, P, Bg
        int a[4] = {m_img.pal[1].tr, m_img.pal[2].tr, m_img.pal[0].tr, m_img.pal[3].tr};

        if (memcmp(pa, a, sizeof(a))) {
            memcpy(pa, a, sizeof(a));
            str = ::Format(_T("Contrast (%d %d %d %d)\n"), a[0], a[1], a[2], a[3]);
            _fputts(str.c_str(), f.get());
        }

        int t1 = (int)sp[i].start + delay;
        int h1 = t1 / 1000 / 60 / 60, m1 = (t1 / 1000 / 60) % 60, s1 = (t1 / 1000) % 60;
        int f1 = (int)((m_size.cy == 480 ? 29.97 : 25) * (t1 % 1000) / 1000);

        int t2 = (int)sp[i].stop + delay;
        int h2 = t2 / 1000 / 60 / 60, m2 = (t2 / 1000 / 60) % 60, s2 = (t2 / 1000) % 60;
        int f2 = (int)((m_size.cy == 480 ? 29.97 : 25) * (t2 % 1000) / 1000);

        if (t2 <= 0) {
            continue;
        }
        if (t1 < 0) {
            t1 = 0;
        }

        if (h1 == h2 && m1 == m2 && s1 == s2 && f1 == f2) {
            f2++;
            if (f2 == (m_size.cy == 480 ? 30 : 25)) {
                f2 = 0;
                s2++;
                if (s2 == 60) {
                    s2 = 0;
                    m2++;
                    if (m2 == 60) {
                        m2 = 0;
                        h2++;
                    }
                }
            }
        }

        if (i < sp.size() - 1) {
            int t3 = (int)sp[i + 1].start + delay;
            int h3 = t3 / 1000 / 60 / 60, m3 = (t3 / 1000 / 60) % 60, s3 = (t3 / 1000) % 60;
            int f3 = (int)((m_size.cy == 480 ? 29.97 : 25) * (t3 % 1000) / 1000);

            if (h3 == h2 && m3 == m2 && s3 == s2 && f3 == f2) {
                f2--;
                if (f2 == -1) {
                    f2 = (m_size.cy == 480 ? 29 : 24);
                    s2--;
                    if (s2 == -1) {
                        s2 = 59;
                        m2--;
                        if (m2 == -1) {
                            m2 = 59;
                            if (h2 > 0) {
                                h2--;
                            }
                        }
                    }
                }
            }
        }

        if (h1 == h2 && m1 == m2 && s1 == s2 && f1 == f2) {
            continue;
        }

        str = ::Format(_T("%04u\t%02d:%02d:%02d:%02d\t%02d:%02d:%02d:%02d\t%s\n"), ++k, h1, m1, s1, f1, h2, m2, s2, f2, title.c_str());
        _fputts(str.c_str(), f.get());

        if (FilePtr bmp = OpenFile(bmpfn, L"wb")) {
            fwrite(&fhdr, 1, sizeof(fhdr), bmp.get());
            fwrite(&ihdr, 1, sizeof(ihdr), bmp.get());
            fwrite(newCusPal, 1, sizeof(RGBQUAD) * 16, bmp.get());
            fwrite(p4bpp.get(), 1, 360 * (m_size.cy - 2), bmp.get());
            bmp.reset();

            CompressFile(bmpfn);
        }
    }

    m_bCustomPal = bCustomPal;
    memcpy(m_cuspal, tempCusPal, sizeof(m_cuspal));

    m_path = fn + _T(".son");

    return true;
}

//
// CVobSubStream
//

CVobSubStream::CVobSubStream(CCritSec* pLock)
    : CSubPicProviderImpl(pLock)
{
}

CVobSubStream::~CVobSubStream()
{
}

static tstring PopFront(std::list<tstring>& list)
{
    tstring head = std::move(list.front());
    list.pop_front();
    return head;
}

void CVobSubStream::Open(tstring name, BYTE* pData, int len)
{
    CAutoLock cAutoLock(&m_csSubPics);

    m_name = name;

    std::list<tstring> lines;
    Explode(StringToT(std::string((CHAR*)pData, len)), lines, _T('\n'));
    while (lines.size()) {
        std::list<tstring> sl;
        Explode(PopFront(lines), sl, ':', 2);
        if (sl.size() != 2) {
            continue;
        }
        tstring key = sl.front();
        tstring value = sl.back();
        if (key == _T("size")) {
            _stscanf_s(value.c_str(), _T("%dx %d"), &m_size.cx, &m_size.cy);
        } else if (key == _T("org")) {
            _stscanf_s(value.c_str(), _T("%d, %d"), &m_org.x, &m_org.y);
        } else if (key == _T("scale")) {
            _stscanf_s(value.c_str(), _T("%d%%, %d%%"), &m_scale_x, &m_scale_y);
        } else if (key == _T("alpha")) {
            _stscanf_s(value.c_str(), _T("%d%%"), &m_alpha);
        } else if (key == _T("smooth"))
            m_iSmooth =
                value == _T("0") || value == _T("OFF") ? 0 :
                value == _T("1") || value == _T("ON") ? 1 :
                value == _T("2") || value == _T("OLD") ? 2 :
                0;
        else if (key == _T("align")) {
            Explode(value, sl, ' ');
            if (sl.size() == 4) {
                sl.erase(std::next(sl.begin()));
            }
            if (sl.size() == 3) {
                m_bAlign = PopFront(sl) == _T("ON");
                tstring hor = sl.front(), ver = sl.back();
                m_alignhor = hor == _T("LEFT") ? 0 : hor == _T("CENTER") ? 1 : hor == _T("RIGHT") ? 2 : 1;
                m_alignver = ver == _T("TOP") ? 0 : ver == _T("CENTER") ? 1 : /*ver == _T("BOTTOM") ? 2 :*/ 2;
            }
        } else if (key == _T("fade in/out")) {
            _stscanf_s(value.c_str(), _T("%d, %d"), &m_fadein, &m_fadeout);
        } else if (key == _T("time offset")) {
            m_toff = _tcstol(value.c_str(), nullptr, 10);
        } else if (key == _T("forced subs")) {
            m_bOnlyShowForcedSubs = value == _T("1") || value == _T("ON");
        } else if (key == _T("palette")) {
            Explode(value, sl, ',', 16);
            for (size_t i = 0; i < 16 && sl.size(); i++) {
                *(DWORD*)&m_orgpal[i] = _tcstol(PopFront(sl).c_str(), nullptr, 16);
            }
        } else if (key == _T("custom colors")) {
            m_bCustomPal = Explode(value, sl, ',', 3) == _T("ON");
            if (sl.size() == 3) {
                sl.pop_front();
                std::list<tstring> tridx, colors;

                Explode(PopFront(sl), tridx, ':', 2);
                int _tridx = 1;
                if (PopFront(tridx) == _T("tridx")) {
                    TCHAR tr[4];
                    _stscanf_s(PopFront(tridx).c_str(), _T("%c%c%c%c"), &tr[0], 1, &tr[1], 1, &tr[2], 1, &tr[3], 1);
                    for (size_t i = 0; i < 4; i++) {
                        _tridx |= ((tr[i] == '1') ? 1 : 0) << i;
                    }
                }

                Explode(PopFront(sl), colors, ':', 2);
                if (PopFront(colors) == _T("colors")) {
                    Explode(PopFront(colors), colors, ',', 4);

                    RGBQUAD pal[4];
                    for (size_t i = 0; i < 4 && colors.size(); i++) {
                        *(DWORD*)&pal[i] = _tcstol(PopFront(colors).c_str(), nullptr, 16);
                    }

                    SetCustomPal(pal, _tridx);
                }
            }
        }
    }
}

void CVobSubStream::Add(REFERENCE_TIME tStart, REFERENCE_TIME tStop, BYTE* pData, int len)
{
    if (len <= 4 || ((pData[0] << 8) | pData[1]) != len) {
        return;
    }

    CVobSubImage vsi;
    vsi.GetPacketInfo(pData, (pData[0] << 8) | pData[1], (pData[2] << 8) | pData[3]);

    std::unique_ptr<SubPic> p(new SubPic());
    p->tStart = tStart;
    p->tStop = vsi.delay > 0 ? (tStart + 10000i64 * vsi.delay) : tStop;
    p->bAnimated = vsi.bAnimated;
    p->pData.resize(len);
    memcpy(p->pData.data(), pData, p->pData.size());

    CAutoLock cAutoLock(&m_csSubPics);
    while (m_subpics.size() && m_subpics.back()->tStart >= tStart) {
        m_subpics.pop_back();
        m_img.nIdx = SIZE_T_ERROR;
    }

    // We can only render one subpicture at a time, thus if there is overlap
    // we have to fix it. tStop = tStart seems to work.
    if (m_subpics.size() && m_subpics.back()->tStop > p->tStart) {
        TRACE(_T("[CVobSubStream::Add] Vobsub timestamp overlap detected! ")
              _T("Subpicture #%d, StopTime %I64d > %I64d (Next StartTime), making them equal!\n"),
              m_subpics.size(), m_subpics.back()->tStop, p->tStart);
        m_subpics.back()->tStop = p->tStart;
    }

    m_subpics.push_back(std::move(p));
}

void CVobSubStream::RemoveAll()
{
    CAutoLock cAutoLock(&m_csSubPics);
    m_subpics.clear();
    m_img.nIdx = SIZE_T_ERROR;
}

STDMETHODIMP CVobSubStream::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
    CheckPointer(ppv, E_POINTER);
    *ppv = nullptr;

    return
        QI(IPersist)
        QI(ISubStream)
        QI(ISubPicProvider)
        __super::NonDelegatingQueryInterface(riid, ppv);
}

// ISubPicProvider

STDMETHODIMP_(POSITION) CVobSubStream::GetStartPosition(REFERENCE_TIME rt, double fps)
{
    CAutoLock cAutoLock(&m_csSubPics);
    // the position is the subpicture itself
    for (auto it = m_subpics.rbegin(); it != m_subpics.rend(); ++it) {
        SubPic* sp = it->get();
        if (sp->tStart <= rt) {
            if (sp->tStop <= rt) {
                // the next one in the list, which is the previous one in the reversed order
                return it == m_subpics.rbegin() ? nullptr : reinterpret_cast<POSITION>(std::prev(it)->get());
            }
            return reinterpret_cast<POSITION>(sp);
        }
    }
    return nullptr;
}

STDMETHODIMP_(POSITION) CVobSubStream::GetNext(POSITION pos)
{
    CAutoLock cAutoLock(&m_csSubPics);
    for (auto it = m_subpics.cbegin(); it != m_subpics.cend(); ++it) {
        if (reinterpret_cast<POSITION>(it->get()) == pos) {
            ++it;
            return it == m_subpics.cend() ? nullptr : reinterpret_cast<POSITION>(it->get());
        }
    }
    return nullptr;
}

STDMETHODIMP_(REFERENCE_TIME) CVobSubStream::GetStart(POSITION pos, double fps)
{
    CAutoLock cAutoLock(&m_csSubPics);
    return reinterpret_cast<const SubPic*>(pos)->tStart;
}

STDMETHODIMP_(REFERENCE_TIME) CVobSubStream::GetStop(POSITION pos, double fps)
{
    CAutoLock cAutoLock(&m_csSubPics);
    return reinterpret_cast<const SubPic*>(pos)->tStop;
}

STDMETHODIMP_(bool) CVobSubStream::IsAnimated(POSITION pos)
{
    CAutoLock cAutoLock(&m_csSubPics);
    return reinterpret_cast<const SubPic*>(pos)->bAnimated;
}

STDMETHODIMP CVobSubStream::Render(SubPicDesc& spd, REFERENCE_TIME rt, double fps, RECT& bbox)
{
    if (spd.bpp != 32) {
        return E_INVALIDARG;
    }

    for (auto it = m_subpics.rbegin(); it != m_subpics.rend(); ++it) {
        SubPic* sp = it->get();
        if (sp->tStart <= rt && rt < sp->tStop) {
            if (m_img.nIdx != (size_t)sp || (sp->bAnimated && sp->tStart + m_img.tCurrent * 10000i64 <= rt)) {
                BYTE* pData = sp->pData.data();
                m_img.Decode(
                    pData, (pData[0] << 8) | pData[1], (pData[2] << 8) | pData[3], int((rt - sp->tStart) / 10000i64),
                    m_bCustomPal, m_tridx, m_orgpal, m_cuspal, true);
                m_img.nIdx = (size_t)sp;
            }

            return __super::Render(spd, bbox);
        }
    }

    return E_FAIL;
}

STDMETHODIMP CVobSubStream::GetRelativeTo(POSITION pos, RelativeTo& relativeTo)
{
    relativeTo = VIDEO;
    return S_OK;
}

// IPersist

STDMETHODIMP CVobSubStream::GetClassID(CLSID* pClassID)
{
    return pClassID ? *pClassID = __uuidof(this), S_OK : E_POINTER;
}

// ISubStream

STDMETHODIMP_(int) CVobSubStream::GetStreamCount()
{
    return 1;
}

STDMETHODIMP CVobSubStream::GetStreamInfo(int i, WCHAR** ppName, LCID* pLCID)
{
    CAutoLock cAutoLock(&m_csSubPics);

    if (ppName) {
        *ppName = (WCHAR*)CoTaskMemAlloc((int(m_name.size()) + 1) * sizeof(WCHAR));
        if (!(*ppName)) {
            return E_OUTOFMEMORY;
        }
        wcscpy_s(*ppName, int(m_name.size()) + 1, std::wstring(m_name).c_str());
    }

    if (pLCID) {
        *pLCID = 0; // TODO
    }

    return S_OK;
}

STDMETHODIMP_(int) CVobSubStream::GetStream()
{
    return 0;
}

STDMETHODIMP CVobSubStream::SetStream(int iStream)
{
    return iStream == 0 ? S_OK : E_FAIL;
}
