/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#ifndef __DEMUXER_UTILS_DEFINED
#define __DEMUXER_UTILS_DEFINED

#define LCID_NOSUBTITLES			                                        -1

#define AV_DISPOSITION_SUB_STREAM                                     0x10000
#define AV_DISPOSITION_SECONDARY_AUDIO                                0x20000

class CDemuxerUtils
{
public:

  static int GetBitRate(AVCodecContext *codecContext);

  static int GetBitsPerSample(AVCodecContext *codecContext, bool raw);

  static wchar_t *GetStreamLanguage(const AVStream *stream);

  static wchar_t *GetCodecName(AVCodecContext *codecContext);

  static wchar_t *GetStreamDescription(AVStream *stream);

  static wchar_t *GetFormatFlags(int flags);

  /* language methods */

  // get language name from ISO6391 code
  // @param code : ISO6391 language code to get language name
  // @return : language name or NULL if not found
  static wchar_t *ISO6391ToLanguage(const wchar_t *code);

  // get language name from ISO6392 code
  // @param code : ISO6392 language code to get language name
  // @return : language name or NULL if not found
  static wchar_t *ISO6392ToLanguage(const wchar_t *code);

  // probes language code for language name
  // @param code : ISO6391 or ISO6392 language code to probe for language name
  // @return : language name or NULL if not found
  static wchar_t *ProbeForLanguage(const wchar_t *code);

  // checks ISO6392 language code
  // @param code : ISO6392 language code to check
  // @return : ISO6392 language code or NULL if not found
  static const wchar_t *ISO6392Check(const wchar_t *code);

  // gets ISO6392 language code for language name
  // @param language : the language to get ISO6392 language code
  // @return : ISO6392 language code or NULL if not found
  static const wchar_t *LanguageToISO6392(const wchar_t *language);

  // probes for ISO6392 language code for language name
  // @param language : the language name to get ISO6392 language code
  // @return : ISO6392 language code or NULL if not found
  static const wchar_t *ProbeForISO6392(const wchar_t *language);

  // gets LCID for ISO6391 language code
  // @param code : the ISO6391 language code to get LCID
  // @return : LCID of ISO6391 language code or 0 if not found
  static LCID ISO6391ToLcid(const wchar_t *code);

  // gets LCID for ISO6392 language code
  // @param code : the ISO6392 language code to get LCID
  // @return : LCID of ISO6392 language code or 0 if not found
  static LCID ISO6392ToLcid(const wchar_t *code);

  // gets ISO6392 language code from ISO6391 language code
  // @param code : ISO6391 language code to get ISO6392 language code
  // @return : ISO6392 language code or NULL if not found
  static const wchar_t *ISO6391To6392(const wchar_t *code);

  // gets ISO6391 language code from ISO6392 language code
  // @param code : ISO6392 language code to get ISO6391 language code
  // @return : ISO6391 language code or NULL if not found
  static const wchar_t *ISO6392To6391(const wchar_t *code);

  // probes ISO6391 or ISO6392 language code for LCID
  // @param : ISO6391 or ISO6392 language code to get LCID
  // @return : LCID of language code or 0 if not found
  static LCID ProbeForLCID(const wchar_t *code);
};

#endif