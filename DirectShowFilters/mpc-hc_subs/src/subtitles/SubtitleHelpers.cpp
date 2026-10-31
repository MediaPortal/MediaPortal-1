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
#include "SubtitleHelpers.h"
#include "TextFile.h"
#include "../DSUtil/PathUtils.h"
#include "../DSUtil/ISOLang.h"

namespace
{
    constexpr std::array<LPCTSTR, 13> subTypesExt = {
        _T("srt"), _T("sub"), _T("smi"), _T("psb"),
        _T("ssa"), _T("ass"), _T("idx"), _T("usf"),
        _T("xss"), _T("txt"), _T("rt"), _T("sup"),
        _T("vtt")
    };

    LPCTSTR separators = _T(".\\-_");
    LPCTSTR extListVid = _T("(avi)|(mkv)|(mp4)|(mov)|(webm)|(wmv)|(flv)|(ts)|(m2ts)");
}

LPCTSTR Subtitle::GetSubtitleFileExt(SubType type)
{
    return (type >= 0 && size_t(type) < subTypesExt.size()) ? subTypesExt[type] : nullptr;
}

bool Subtitle::IsTextSubtitleFileName(tstring fileName)
{
    auto fileExt = StringTrimLeft(PathUtils::FileExt(fileName.c_str()), '.');
    return std::any_of(subTypesExt.cbegin(), subTypesExt.cend(), [&](LPCTSTR ext) {
        return fileExt == ext;
    });
}

void Subtitle::GetSubFileNames(tstring fn, const std::vector<tstring>& paths, std::vector<SubFile>& ret)
{
    ret.clear();

    if (StringFind(fn, _T("://")) > 1) {
        return;
    }

    StringReplace(fn, '/', '\\');

    ExtendMaxPathLengthIfNeeded(fn, MAX_PATH);

    int l  = StringRFind(fn, '\\') + 1;
    int l2 = StringRFind(fn, '.');
    if (l2 < l) { // no extension, read to the end
        l2 = int(fn.size());
    }

    tstring orgpath = StringLeft(fn, l);
    tstring title = StringMid(fn, l, l2 - l);
    int titleLength = int(title.size());

    WIN32_FIND_DATA wfd;

    tstring extListSub, regExpSub, regExpVid;
    for (size_t i = 0; i < subTypesExt.size(); i++) {
        extListSub += ::Format(_T("(%s)"), subTypesExt[i]);
        if (i < subTypesExt.size() - 1) {
            extListSub.push_back(_T('|'));
        }
    }
    regExpSub = ::Format(_T("([%s]+.+)?\\.(%s)$"), separators, extListSub.c_str());
    regExpVid = ::Format(_T(".+\\.(%s)$"), extListVid);

    const std::wregex::flag_type reFlags = std::wregex::icase | std::wregex::optimize;
    std::wregex reSub(regExpSub, reFlags), reVid(regExpVid, reFlags);

    for (size_t k = 0; k < paths.size(); k++) {
        tstring path = paths[k];
        StringReplace(path, '/', '\\');

        l = int(path.size());
        if (l > 0 && path[l - 1] != '\\') {
            path += _T('\\');
        }

        if (StringFind(path, ':') == -1 && StringFind(path, _T("\\\\")) != 0) {
            path = orgpath + path;
            StringReplace(path, _T("\\.\\"), _T("\\"));
            ExtendMaxPathLengthIfNeeded(path, MAX_PATH);
        }

        std::list<tstring> subs, vids;

        HANDLE hFile = FindFirstFile((path + title + _T("*")).c_str(), &wfd);
        if (hFile != INVALID_HANDLE_VALUE) {
            do {
                tstring fn2 = path + wfd.cFileName;
                if (std::regex_match(&wfd.cFileName[titleLength], reSub)) {
                    subs.push_back(fn2);
                } else if (std::regex_match(&wfd.cFileName[titleLength], reVid)) {
                    // Convert to lower-case and cut the extension for easier matching
                    vids.push_back(StringLowered(StringLeft(fn2, StringRFind(fn2, _T('.')))));
                }
            } while (FindNextFile(hFile, &wfd));

            FindClose(hFile);
        }

        for (tstring&fn2 : subs) {

            bool bMatchAnotherVid = false;
            if (!vids.empty()) {
                // Check if there is an exact match for another video file
                tstring fnlower = fn2;
                StringLower(fnlower);
                for (const tstring& vid : vids) {
                    if (StringFind(fnlower, vid) == 0) {
                        bMatchAnotherVid = true;
                        break;
                    }
                }
            }

            if (!bMatchAnotherVid) {
                SubFile f;
                f.fn = fn2;
                ret.push_back(f);
            }
        }
    }

    if (ret.empty()) {
        // Load all subs from folder .\Subs\FILENAME_WITHOUT_EXT
        tstring path = orgpath + L"Subs\\" + title + L"\\";
        ExtendMaxPathLengthIfNeeded(path, MAX_PATH);

        HANDLE hFile = FindFirstFile((path + L"*").c_str(), &wfd);
        if (hFile != INVALID_HANDLE_VALUE) {
            do {
                tstring fn2 = path + wfd.cFileName;
                tstring ext = PathFindExtension(fn2.c_str());
                if (ext == L".srt" || ext == L".ass" || ext == L".ssa" || ext == L".webvtt" || ext == L".vtt") {
                    SubFile f;
                    f.fn = fn2;
                    ret.push_back(f);
                }
            } while (FindNextFile(hFile, &wfd));
            FindClose(hFile);
        }
    }

    // sort files, this way the user can define the order (movie.00.English.srt, movie.01.Hungarian.srt, etc)
    std::sort(ret.data(), ret.data() + ret.size());
}

void Subtitle::GetLCIDAndLangName(std::wstring subName, LCID& lcid, tstring& langname, HearingImpairedType& hi, std::wregex re) {
    std::wcmatch mc;
    if (std::regex_search(subName.c_str(), mc, re)) {
        ASSERT(mc.size() == 4);
        ASSERT(mc[2].matched);

        std::wstring langFull(mc[1].str().c_str());
        std::string langSimple = StringNarrow(mc[2].str().c_str());

        LCID lcidFull = 0;
        if (StringFind(langFull, L"-") != -1) { //only use LocaleNameToLCID when we have the longer ISO codes
            lcidFull = LocaleNameToLCID(langFull.c_str(), 0);
        }
        if (lcidFull) {
            lcid = lcidFull;
            langname = langFull;
        } else {
            langname = ISOLang::ISO639XToLanguage(langSimple.c_str(), true);

            if (!langname.empty()) {
                size_t len = mc[1].str().size();
                if (len == 3) {
                    lcid = ISOLang::ISO6392ToLcid(langSimple.c_str());
                } else if (len == 2) {
                    lcid = ISOLang::ISO6391ToLcid(langSimple.c_str());
                }
            }
        }
        if (!langname.empty() && StringICompare(langSimple, "hi") == 0) {
            hi = HI_YES;
        }
    }
}

tstring Subtitle::GuessSubtitleName(const tstring& fn, tstring videoName, LCID& lcid, tstring& langname, HearingImpairedType& hi)
{
    tstring name;

    // The filename of the subtitle file
    int iExtStart = StringRFind(fn, '.');
    if (iExtStart < 0) {
        iExtStart = int(fn.size());
    }
    tstring subName = StringMid(StringLeft(fn, iExtStart), StringRFind(fn, '\\') + 1);

    if (!videoName.empty()) {
        // The filename of the video file
        iExtStart = StringRFind(videoName, '.');
        if (iExtStart < 0) {
            iExtStart = int(videoName.size());
        }
        tstring videoExt = StringLowered(StringMid(videoName, iExtStart + 1));
        videoName = StringMid(StringLeft(videoName, iExtStart), StringRFind(videoName, '\\') + 1);

        tstring subNameNoCase = StringLowered(tstring(subName));
        tstring videoNameNoCase = StringLowered(tstring(videoName));

        // Check if the subtitle filename starts with the video filename
        // so that we can try to find a language info right after it
        if (StringFind(subNameNoCase, videoNameNoCase) == 0) {
            int iVideoNameEnd = int(videoName.size());
            // Get ride of the video extension if it's in the subtitle filename
            if (StringFind(subNameNoCase, videoExt, iVideoNameEnd) == iVideoNameEnd + 1) {
                iVideoNameEnd += 1 + int(videoExt.size());
            }
            subName = StringMid(subName, iVideoNameEnd);

            std::wregex re(_T("^[.\\-_ ]+(([^.\\-_ ]+)(?:[.\\-_ ]+([^.\\-_ ]+))?)"), std::wregex::icase);
            GetLCIDAndLangName(subName, lcid, langname, hi, re);
        }
    }

    // If we couldn't find any info yet, we try to find the language at the end of the filename
    if (langname.empty()) {
        std::wregex re(_T(".*?[.\\-_ ]+(([^.\\-_ ]+)(?:[.\\-_ ]+([^.\\-_ ]+))?)$"), std::wregex::icase);
        GetLCIDAndLangName(subName, lcid, langname, hi, re);
    }

    name = StringMid(fn, StringRFind(fn, '\\') + 1);
    if (int(name.size()) > 100) { // Cut some part of the filename if it's too long
        name = ::Format(_T("%s...%s"), StringTrimRight(StringLeft(name, 50), _T(".-_ ")).c_str(), StringTrimLeft(StringRight(name, 50), _T(".-_ ")).c_str());
    }

    return name;
}
