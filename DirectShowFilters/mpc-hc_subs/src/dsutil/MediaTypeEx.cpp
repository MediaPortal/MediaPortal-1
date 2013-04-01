/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2012 see Authors.txt
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
#include "DSUtil.h"
#include "MediaTypeEx.h"

#include <MMReg.h>
#include <InitGuid.h>
#include "moreuuids.h"

#pragma pack(push, 1)
typedef struct {
    WAVEFORMATEX Format;
    BYTE bBigEndian;
    BYTE bsid;
    BYTE lfeon;
    BYTE copyrightb;
    BYTE nAuxBitsCode;  //  Aux bits per frame
} DOLBYAC3WAVEFORMAT;
#pragma pack(pop)

CMediaTypeEx::CMediaTypeEx()
{
}

CMediaTypeEx::~CMediaTypeEx()
{
}

CString CMediaTypeEx::ToString(IPin* pPin)
{
    CString packing, type, codec, dim, rate, dur;

    // TODO

    if (majortype == MEDIATYPE_DVD_ENCRYPTED_PACK) {
        packing = _T("Encrypted MPEG2 Pack");
    } else if (majortype == MEDIATYPE_MPEG2_PACK) {
        packing = _T("MPEG2 Pack");
    } else if (majortype == MEDIATYPE_MPEG2_PES) {
        packing = _T("MPEG2 PES");
    }

    if (majortype == MEDIATYPE_Video) {
        type = _T("Video");

        BITMAPINFOHEADER bih;
        bool fBIH = ExtractBIH(this, &bih);

        int w, h, arx, ary;
        bool fDim = ExtractDim(this, w, h, arx, ary);

        if (fBIH) {
            codec = GetVideoCodecName(subtype, bih.biCompression);
        }

        if (codec.IsEmpty()) {
            if (formattype == FORMAT_MPEGVideo) {
                codec = _T("MPEG1 Video");
            } else if (formattype == FORMAT_MPEG2_VIDEO) {
                codec = _T("MPEG2 Video");
            } else if (formattype == FORMAT_DiracVideoInfo) {
                codec = _T("Dirac Video");
            }
        }

        if (fDim) {
            dim.Format(_T("%dx%d"), w, h);
            if (w * ary != h * arx) {
                dim.AppendFormat(_T(" (%d:%d)"), arx, ary);
            }
        }

        if (formattype == FORMAT_VideoInfo || formattype == FORMAT_MPEGVideo) {
            VIDEOINFOHEADER* vih = (VIDEOINFOHEADER*)pbFormat;
            if (vih->AvgTimePerFrame) {
                rate.Format(_T("%0.3f"), 10000000.0f / vih->AvgTimePerFrame);
                rate.TrimRight(_T('0')); // remove trailing zeros
                rate.TrimRight(_T('.')); // remove the trailing dot
                rate += _T("fps ");
            }
            if (vih->dwBitRate) {
                rate.AppendFormat(_T("%dkbps"), vih->dwBitRate / 1000);
            }
        } else if (formattype == FORMAT_VideoInfo2 || formattype == FORMAT_MPEG2_VIDEO || formattype == FORMAT_DiracVideoInfo) {
            VIDEOINFOHEADER2* vih = (VIDEOINFOHEADER2*)pbFormat;
            if (vih->AvgTimePerFrame) {
                rate.Format(_T("%0.3f"), 10000000.0f / vih->AvgTimePerFrame);
                rate.TrimRight(_T('0')); // remove trailing zeros
                rate.TrimRight(_T('.')); // remove the trailing dot
                rate += _T("fps ");
            }
            if (vih->dwBitRate) {
                rate.AppendFormat(_T("%dkbps"), vih->dwBitRate / 1000);
            }
        }

        rate.TrimRight();

        if (subtype == MEDIASUBTYPE_DVD_SUBPICTURE) {
            type = _T("Subtitle");
            codec = _T("DVD Subpicture");
        }
    } else if (majortype == MEDIATYPE_Audio) {
        type = _T("Audio");

        if (formattype == FORMAT_WaveFormatEx) {
            WAVEFORMATEX* wfe = (WAVEFORMATEX*)Format();

            if (wfe->wFormatTag/* > WAVE_FORMAT_PCM && wfe->wFormatTag < WAVE_FORMAT_EXTENSIBLE
            && wfe->wFormatTag != WAVE_FORMAT_IEEE_FLOAT*/
                    || subtype != GUID_NULL) {
                codec = GetAudioCodecName(subtype, wfe->wFormatTag);
                dim.Format(_T("%dHz"), wfe->nSamplesPerSec);
                if (wfe->nChannels == 1) {
                    dim += _T(" mono");
                } else if (wfe->nChannels == 2) {
                    dim += _T(" stereo");
                } else {
                    dim.AppendFormat(_T(" %dch"), wfe->nChannels);
                }
                if (wfe->nAvgBytesPerSec) {
                    rate.Format(_T("%dkbps"), wfe->nAvgBytesPerSec * 8 / 1000);
                }
            }
        } else if (formattype == FORMAT_VorbisFormat) {
            VORBISFORMAT* vf = (VORBISFORMAT*)Format();

            codec = GetAudioCodecName(subtype, 0);
            dim.Format(_T("%dHz"), vf->nSamplesPerSec);
            if (vf->nChannels == 1) {
                dim += _T(" mono");
            } else if (vf->nChannels == 2) {
                dim += _T(" stereo");
            } else {
                dim.AppendFormat(_T(" %dch"), vf->nChannels);
            }
            if (vf->nAvgBitsPerSec) {
                rate.Format(_T("%dkbps"), vf->nAvgBitsPerSec / 1000);
            }
        } else if (formattype == FORMAT_VorbisFormat2) {
            VORBISFORMAT2* vf = (VORBISFORMAT2*)Format();

            codec = GetAudioCodecName(subtype, 0);
            dim.Format(_T("%dHz"), vf->SamplesPerSec);
            if (vf->Channels == 1) {
                dim += _T(" mono");
            } else if (vf->Channels == 2) {
                dim += _T(" stereo");
            } else {
                dim.AppendFormat(_T(" %dch"), vf->Channels);
            }
        }
    } else if (majortype == MEDIATYPE_Text) {
        type = _T("Text");
    } else if (majortype == MEDIATYPE_Subtitle) {
        type = _T("Subtitle");
        codec = GetSubtitleCodecName(subtype);
    } else {
        type = _T("Unknown");
    }

    if (CComQIPtr<IMediaSeeking> pMS = pPin) {
        REFERENCE_TIME rtDur = 0;
        if (SUCCEEDED(pMS->GetDuration(&rtDur)) && rtDur) {
            rtDur /= 10000000;
            int s = rtDur % 60;
            rtDur /= 60;
            int m = rtDur % 60;
            rtDur /= 60;
            int h = (int)rtDur;
            if (h) {
                dur.Format(_T("%d:%02d:%02d"), h, m, s);
            } else if (m) {
                dur.Format(_T("%02d:%02d"), m, s);
            } else if (s) {
                dur.Format(_T("%ds"), s);
            }
        }
    }

    CString str;
    if (!codec.IsEmpty()) {
        str += codec + _T(" ");
    }
    if (!dim.IsEmpty()) {
        str += dim + _T(" ");
    }
    if (!rate.IsEmpty()) {
        str += rate + _T(" ");
    }
    if (!dur.IsEmpty()) {
        str += dur + _T(" ");
    }
    str.Trim(_T(" ,"));

    if (!str.IsEmpty()) {
        str = type + _T(": ") + str;
    } else {
        str = type;
    }

    return str;
}

CString CMediaTypeEx::GetVideoCodecName(const GUID& subtype, DWORD biCompression)
{
    CString str = _T("");

    static CAtlMap<DWORD, CString> names;

    if (names.IsEmpty()) {
        names['WMV1'] = _T("Windows Media Video 7");
        names['WMV2'] = _T("Windows Media Video 8");
        names['WMV3'] = _T("Windows Media Video 9");
        names['DIV3'] = _T("DivX 3");
        names['MP43'] = _T("MSMPEG4v3");
        names['MP42'] = _T("MSMPEG4v2");
        names['MP41'] = _T("MSMPEG4v1");
        names['DX50'] = _T("DivX 5");
        names['DIVX'] = _T("DivX 6");
        names['XVID'] = _T("Xvid");
        names['MP4V'] = _T("MPEG4 Video");
        names['AVC1'] = _T("MPEG4 Video (H264)");
        names['H264'] = _T("MPEG4 Video (H264)");
        names['RV10'] = _T("RealVideo 1");
        names['RV20'] = _T("RealVideo 2");
        names['RV30'] = _T("RealVideo 3");
        names['RV40'] = _T("RealVideo 4");
        names['FLV1'] = _T("Flash Video 1");
        names['FLV4'] = _T("Flash Video 4");
        names['VP50'] = _T("On2 VP5");
        names['VP60'] = _T("On2 VP6");
        names['SVQ3'] = _T("SVQ3");
        names['SVQ1'] = _T("SVQ1");
        names['H263'] = _T("H263");
        // names[''] = _T("");
    }

    if (biCompression) {
        BYTE* b = (BYTE*)&biCompression;

        for (ptrdiff_t i = 0; i < 4; i++)
            if (b[i] >= 'a' && b[i] <= 'z') {
                b[i] = toupper(b[i]);
            }

        if (!names.Lookup(MAKEFOURCC(b[3], b[2], b[1], b[0]), str)) {
            if (subtype == MEDIASUBTYPE_DiracVideo) {
                str = _T("Dirac Video");
            }
            // else if (subtype == ) str = _T("");
            else if (biCompression < 256) {
                str.Format(_T("%d"), biCompression);
            } else {
                str.Format(_T("%4.4hs"), &biCompression);
            }
        }
    } else {
        if (subtype == MEDIASUBTYPE_RGB32) {
            str = _T("RGB32");
        } else if (subtype == MEDIASUBTYPE_RGB24) {
            str = _T("RGB24");
        } else if (subtype == MEDIASUBTYPE_RGB555) {
            str = _T("RGB555");
        } else if (subtype == MEDIASUBTYPE_RGB565) {
            str = _T("RGB565");
        }
    }

    return str;
}

CString CMediaTypeEx::GetAudioCodecName(const GUID& subtype, WORD wFormatTag)
{
    CString str;

    static CAtlMap<WORD, CString> names;

    if (names.IsEmpty()) {
        // MMReg.h
        names[WAVE_FORMAT_ADPCM]                 = _T("MS ADPCM");
        names[WAVE_FORMAT_IEEE_FLOAT]            = _T("IEEE Float");
        names[WAVE_FORMAT_ALAW]                  = _T("aLaw");
        names[WAVE_FORMAT_MULAW]                 = _T("muLaw");
        names[WAVE_FORMAT_DTS]                   = _T("DTS");
        names[WAVE_FORMAT_DRM]                   = _T("DRM");
        names[WAVE_FORMAT_WMAVOICE9]             = _T("WMA Voice");
        names[WAVE_FORMAT_WMAVOICE10]            = _T("WMA Voice");
        names[WAVE_FORMAT_OKI_ADPCM]             = _T("OKI ADPCM");
        names[WAVE_FORMAT_IMA_ADPCM]             = _T("IMA ADPCM");
        names[WAVE_FORMAT_MEDIASPACE_ADPCM]      = _T("Mediaspace ADPCM");
        names[WAVE_FORMAT_SIERRA_ADPCM]          = _T("Sierra ADPCM");
        names[WAVE_FORMAT_G723_ADPCM]            = _T("G723 ADPCM");
        names[WAVE_FORMAT_DIALOGIC_OKI_ADPCM]    = _T("Dialogic OKI ADPCM");
        names[WAVE_FORMAT_MEDIAVISION_ADPCM]     = _T("Media Vision ADPCM");
        names[WAVE_FORMAT_YAMAHA_ADPCM]          = _T("Yamaha ADPCM");
        names[WAVE_FORMAT_DSPGROUP_TRUESPEECH]   = _T("DSP Group Truespeech");
        names[WAVE_FORMAT_DOLBY_AC2]             = _T("Dolby AC2");
        names[WAVE_FORMAT_GSM610]                = _T("GSM610");
        names[WAVE_FORMAT_MSNAUDIO]              = _T("MSN Audio");
        names[WAVE_FORMAT_ANTEX_ADPCME]          = _T("Antex ADPCME");
        names[WAVE_FORMAT_CS_IMAADPCM]           = _T("Crystal Semiconductor IMA ADPCM");
        names[WAVE_FORMAT_ROCKWELL_ADPCM]        = _T("Rockwell ADPCM");
        names[WAVE_FORMAT_ROCKWELL_DIGITALK]     = _T("Rockwell Digitalk");
        names[WAVE_FORMAT_G721_ADPCM]            = _T("G721");
        names[WAVE_FORMAT_G728_CELP]             = _T("G728");
        names[WAVE_FORMAT_MSG723]                = _T("MSG723");
        names[WAVE_FORMAT_MPEG]                  = _T("MPEG Audio");
        names[WAVE_FORMAT_MPEGLAYER3]            = _T("MP3");
        names[WAVE_FORMAT_LUCENT_G723]           = _T("Lucent G723");
        names[WAVE_FORMAT_VOXWARE]               = _T("Voxware");
        names[WAVE_FORMAT_G726_ADPCM]            = _T("G726");
        names[WAVE_FORMAT_G722_ADPCM]            = _T("G722");
        names[WAVE_FORMAT_G729A]                 = _T("G729A");
        names[WAVE_FORMAT_MEDIASONIC_G723]       = _T("MediaSonic G723");
        names[WAVE_FORMAT_ZYXEL_ADPCM]           = _T("ZyXEL ADPCM");
        names[WAVE_FORMAT_RAW_AAC1]              = _T("AAC"); // = WAVE_FORMAT_AAC
        names[WAVE_FORMAT_RHETOREX_ADPCM]        = _T("Rhetorex ADPCM");
        names[WAVE_FORMAT_VIVO_G723]             = _T("Vivo G723");
        names[WAVE_FORMAT_VIVO_SIREN]            = _T("Vivo Siren");
        names[WAVE_FORMAT_DIGITAL_G723]          = _T("Digital G723");
        names[WAVE_FORMAT_SANYO_LD_ADPCM]        = _T("Sanyo LD ADPCM");
        names[WAVE_FORMAT_MSAUDIO1]              = _T("WMA 1");
        names[WAVE_FORMAT_WMAUDIO2]              = _T("WMA 2");
        names[WAVE_FORMAT_WMAUDIO3]              = _T("WMA Pro");
        names[WAVE_FORMAT_WMAUDIO_LOSSLESS]      = _T("WMA Lossless");
        names[WAVE_FORMAT_CREATIVE_ADPCM]        = _T("Creative ADPCM");
        names[WAVE_FORMAT_CREATIVE_FASTSPEECH8]  = _T("Creative Fastspeech 8");
        names[WAVE_FORMAT_CREATIVE_FASTSPEECH10] = _T("Creative Fastspeech 10");
        names[WAVE_FORMAT_UHER_ADPCM]            = _T("UHER ADPCM");
        names[WAVE_FORMAT_DTS2]                  = _T("DTS"); // = WAVE_FORMAT_DVD_DTS
        // other
        names[WAVE_FORMAT_DOLBY_AC3]             = _T("Dolby AC3");
        names[WAVE_FORMAT_LATM_AAC]              = _T("AAC(LATM)");
        names[WAVE_FORMAT_FLAC]                  = _T("FLAC");
        names[WAVE_FORMAT_TTA1]                  = _T("TTA");
        names[WAVE_FORMAT_WAVPACK4]              = _T("WavPack");
        names[WAVE_FORMAT_14_4]                  = _T("RealAudio 14.4");
        names[WAVE_FORMAT_28_8]                  = _T("RealAudio 28.8");
        names[WAVE_FORMAT_ATRC]                  = _T("RealAudio ATRC");
        names[WAVE_FORMAT_COOK]                  = _T("RealAudio COOK");
        names[WAVE_FORMAT_DNET]                  = _T("RealAudio DNET");
        names[WAVE_FORMAT_RAAC]                  = _T("RealAudio RAAC");
        names[WAVE_FORMAT_RACP]                  = _T("RealAudio RACP");
        names[WAVE_FORMAT_SIPR]                  = _T("RealAudio SIPR");
        names[WAVE_FORMAT_PS2_PCM]               = _T("PS2 PCM");
        names[WAVE_FORMAT_PS2_ADPCM]             = _T("PS2 ADPCM");
        // names[] = _T("");
    }

    if (!names.Lookup(wFormatTag, str)) {
        // for wFormatTag equal to WAVE_FORMAT_UNKNOWN, WAVE_FORMAT_PCM, WAVE_FORMAT_EXTENSIBLE and other.
        if (subtype == MEDIASUBTYPE_PCM) {
            str = _T("PCM");
        } else if (subtype == MEDIASUBTYPE_IEEE_FLOAT) {
            str = _T("IEEE Float");
        } else if (subtype == MEDIASUBTYPE_DVD_LPCM_AUDIO || subtype == MEDIASUBTYPE_HDMV_LPCM_AUDIO) {
            str = _T("LPCM");
        } else if (subtype == MEDIASUBTYPE_Vorbis) {
            str = _T("Vorbis (deprecated)");
        } else if (subtype == MEDIASUBTYPE_Vorbis2) {
            str = _T("Vorbis");
        } else if (subtype == MEDIASUBTYPE_MP4A) {
            str = _T("MPEG4 Audio");
        } else if (subtype == MEDIASUBTYPE_FLAC_FRAMED) {
            str = _T("FLAC (framed)");
        } else if (subtype == MEDIASUBTYPE_DOLBY_AC3) {
            str = _T("Dolby AC3");
        } else if (subtype == MEDIASUBTYPE_DOLBY_DDPLUS) {
            str = _T("DD+");
        } else if (subtype == MEDIASUBTYPE_DOLBY_TRUEHD) {
            str = _T("TrueHD");
        } else if (subtype == MEDIASUBTYPE_DTS) {
            str = _T("DTS");
        } else if (subtype == MEDIASUBTYPE_MLP) {
            str = _T("MLP");
        } else if (subtype == MEDIASUBTYPE_PCM_NONE || subtype == MEDIASUBTYPE_PCM_RAW ||
                   subtype == MEDIASUBTYPE_PCM_TWOS || subtype == MEDIASUBTYPE_PCM_SOWT ||
                   subtype == MEDIASUBTYPE_PCM_IN24 || subtype == MEDIASUBTYPE_PCM_IN32 ||
                   subtype == MEDIASUBTYPE_PCM_FL32 || subtype == MEDIASUBTYPE_PCM_FL64) {
            str = _T("QT PCM");
        } else if (subtype == MEDIASUBTYPE_IMA4      ||
                   subtype == MEDIASUBTYPE_ADPCM_SWF ||
                   subtype == MEDIASUBTYPE_ADPCM_AMV) {
            str = _T("ADPCM");
        } else if (subtype == MEDIASUBTYPE_ALAC) {
            str = _T("Alac");
        } else if (subtype == MEDIASUBTYPE_ALS) {
            str = _T("ALS");
        } else if (subtype == MEDIASUBTYPE_QDM2) {
            str = _T("QDM2");
        } else if (subtype == MEDIASUBTYPE_AMR  ||
                   subtype == MEDIASUBTYPE_SAMR ||
                   subtype == MEDIASUBTYPE_SAWB) {
            str = _T("AMR");
        } else {
            str.Format(_T("0x%04x"), wFormatTag);
        }
    }

    return str;
}

CString CMediaTypeEx::GetSubtitleCodecName(const GUID& subtype)
{
    CString str;

    static CAtlMap<GUID, CString> names;

    if (names.IsEmpty()) {
        names[MEDIASUBTYPE_UTF8] = _T("UTF-8");
        names[MEDIASUBTYPE_SSA] = _T("SubStation Alpha");
        names[MEDIASUBTYPE_ASS] = _T("Advanced SubStation Alpha");
        names[MEDIASUBTYPE_ASS2] = _T("Advanced SubStation Alpha");
        names[MEDIASUBTYPE_USF] = _T("Universal Subtitle Format");
        names[MEDIASUBTYPE_VOBSUB] = _T("VobSub");
        // names[''] = _T("");
    }

    if (names.Lookup(subtype, str)) {

    }

    return str;
}

void CMediaTypeEx::Dump(CAtlList<CString>& sl)
{
    CString str;

    sl.RemoveAll();

    int fmtsize = 0;

    CString major = CStringFromGUID(majortype);
    CString sub = CStringFromGUID(subtype);
    CString format = CStringFromGUID(formattype);

    sl.AddTail(ToString() + _T("\n"));

    sl.AddTail(_T("AM_MEDIA_TYPE: "));
    str.Format(_T("majortype: %s %s"), CString(GuidNames[majortype]), major);
    sl.AddTail(str);
    str.Format(_T("subtype: %s %s"), CString(GuidNames[subtype]), sub);
    sl.AddTail(str);
    str.Format(_T("formattype: %s %s"), CString(GuidNames[formattype]), format);
    sl.AddTail(str);
    str.Format(_T("bFixedSizeSamples: %d"), bFixedSizeSamples);
    sl.AddTail(str);
    str.Format(_T("bTemporalCompression: %d"), bTemporalCompression);
    sl.AddTail(str);
    str.Format(_T("lSampleSize: %d"), lSampleSize);
    sl.AddTail(str);
    str.Format(_T("cbFormat: %d"), cbFormat);
    sl.AddTail(str);

    sl.AddTail(_T(""));

    if (formattype == FORMAT_VideoInfo || formattype == FORMAT_VideoInfo2
            || formattype == FORMAT_MPEGVideo || formattype == FORMAT_MPEG2_VIDEO) {
        fmtsize =
            formattype == FORMAT_VideoInfo ? sizeof(VIDEOINFOHEADER) :
            formattype == FORMAT_VideoInfo2 ? sizeof(VIDEOINFOHEADER2) :
            formattype == FORMAT_MPEGVideo ? sizeof(MPEG1VIDEOINFO) - 1 :
            formattype == FORMAT_MPEG2_VIDEO ? sizeof(MPEG2VIDEOINFO) - 4 :
            0;

        VIDEOINFOHEADER& vih = *(VIDEOINFOHEADER*)pbFormat;
        BITMAPINFOHEADER* bih = &vih.bmiHeader;

        sl.AddTail(_T("VIDEOINFOHEADER:"));
        str.Format(_T("rcSource: (%d,%d)-(%d,%d)"), vih.rcSource.left, vih.rcSource.top, vih.rcSource.right, vih.rcSource.bottom);
        sl.AddTail(str);
        str.Format(_T("rcTarget: (%d,%d)-(%d,%d)"), vih.rcTarget.left, vih.rcTarget.top, vih.rcTarget.right, vih.rcTarget.bottom);
        sl.AddTail(str);
        str.Format(_T("dwBitRate: %d"), vih.dwBitRate);
        sl.AddTail(str);
        str.Format(_T("dwBitErrorRate: %d"), vih.dwBitErrorRate);
        sl.AddTail(str);
        str.Format(_T("AvgTimePerFrame: %I64d"), vih.AvgTimePerFrame);
        sl.AddTail(str);

        sl.AddTail(_T(""));

        if (formattype == FORMAT_VideoInfo2 || formattype == FORMAT_MPEG2_VIDEO) {
            VIDEOINFOHEADER2& vih2 = *(VIDEOINFOHEADER2*)pbFormat;
            bih = &vih2.bmiHeader;

            sl.AddTail(_T("VIDEOINFOHEADER2:"));
            str.Format(_T("dwInterlaceFlags: 0x%08x"), vih2.dwInterlaceFlags);
            sl.AddTail(str);
            str.Format(_T("dwCopyProtectFlags: 0x%08x"), vih2.dwCopyProtectFlags);
            sl.AddTail(str);
            str.Format(_T("dwPictAspectRatioX: %d"), vih2.dwPictAspectRatioX);
            sl.AddTail(str);
            str.Format(_T("dwPictAspectRatioY: %d"), vih2.dwPictAspectRatioY);
            sl.AddTail(str);
            str.Format(_T("dwControlFlags: 0x%08x"), vih2.dwControlFlags);
            sl.AddTail(str);
            str.Format(_T("dwReserved2: 0x%08x"), vih2.dwReserved2);
            sl.AddTail(str);

            sl.AddTail(_T(""));
        }

        if (formattype == FORMAT_MPEGVideo) {
            MPEG1VIDEOINFO& mvih = *(MPEG1VIDEOINFO*)pbFormat;

            sl.AddTail(_T("MPEG1VIDEOINFO:"));
            str.Format(_T("dwStartTimeCode: %d"), mvih.dwStartTimeCode);
            sl.AddTail(str);
            str.Format(_T("cbSequenceHeader: %d"), mvih.cbSequenceHeader);
            sl.AddTail(str);

            sl.AddTail(_T(""));
        } else if (formattype == FORMAT_MPEG2_VIDEO) {
            MPEG2VIDEOINFO& mvih = *(MPEG2VIDEOINFO*)pbFormat;

            sl.AddTail(_T("MPEG2VIDEOINFO:"));
            str.Format(_T("dwStartTimeCode: %d"), mvih.dwStartTimeCode);
            sl.AddTail(str);
            str.Format(_T("cbSequenceHeader: %d"), mvih.cbSequenceHeader);
            sl.AddTail(str);
            str.Format(_T("dwProfile: 0x%08x"), mvih.dwProfile);
            sl.AddTail(str);
            str.Format(_T("dwLevel: 0x%08x"), mvih.dwLevel);
            sl.AddTail(str);
            str.Format(_T("dwFlags: 0x%08x"), mvih.dwFlags);
            sl.AddTail(str);

            sl.AddTail(_T(""));
        }

        sl.AddTail(_T("BITMAPINFOHEADER:"));
        str.Format(_T("biSize: %d"), bih->biSize);
        sl.AddTail(str);
        str.Format(_T("biWidth: %d"), bih->biWidth);
        sl.AddTail(str);
        str.Format(_T("biHeight: %d"), bih->biHeight);
        sl.AddTail(str);
        str.Format(_T("biPlanes: %d"), bih->biPlanes);
        sl.AddTail(str);
        str.Format(_T("biBitCount: %d"), bih->biBitCount);
        sl.AddTail(str);
        if (bih->biCompression < 256) {
            str.Format(_T("biCompression: %d"), bih->biCompression);
        } else {
            str.Format(_T("biCompression: %4.4hs"), &bih->biCompression);
        }
        sl.AddTail(str);
        str.Format(_T("biSizeImage: %d"), bih->biSizeImage);
        sl.AddTail(str);
        str.Format(_T("biXPelsPerMeter: %d"), bih->biXPelsPerMeter);
        sl.AddTail(str);
        str.Format(_T("biYPelsPerMeter: %d"), bih->biYPelsPerMeter);
        sl.AddTail(str);
        str.Format(_T("biClrUsed: %d"), bih->biClrUsed);
        sl.AddTail(str);
        str.Format(_T("biClrImportant: %d"), bih->biClrImportant);
        sl.AddTail(str);

        sl.AddTail(_T(""));
    } else if (formattype == FORMAT_WaveFormatEx || formattype == FORMAT_WaveFormatExFFMPEG) {
        WAVEFORMATEX* pWfe = NULL;
        if (formattype == FORMAT_WaveFormatExFFMPEG) {
            fmtsize = sizeof(WAVEFORMATEXFFMPEG);

            WAVEFORMATEXFFMPEG* wfeff = (WAVEFORMATEXFFMPEG*)pbFormat;
            pWfe = &wfeff->wfex;

            sl.AddTail(_T("WAVEFORMATEXFFMPEG:"));
            str.Format(_T("nCodecId: 0x%04x"), wfeff->nCodecId);
            sl.AddTail(str);
            sl.AddTail(_T(""));
        } else {
            fmtsize = sizeof(WAVEFORMATEX);
            pWfe = (WAVEFORMATEX*)pbFormat;
        }

        WAVEFORMATEX& wfe = *pWfe;

        sl.AddTail(_T("WAVEFORMATEX:"));
        str.Format(_T("wFormatTag: 0x%04x"), wfe.wFormatTag);
        sl.AddTail(str);
        str.Format(_T("nChannels: %d"), wfe.nChannels);
        sl.AddTail(str);
        str.Format(_T("nSamplesPerSec: %d"), wfe.nSamplesPerSec);
        sl.AddTail(str);
        str.Format(_T("nAvgBytesPerSec: %d"), wfe.nAvgBytesPerSec);
        sl.AddTail(str);
        str.Format(_T("nBlockAlign: %d"), wfe.nBlockAlign);
        sl.AddTail(str);
        str.Format(_T("wBitsPerSample: %d"), wfe.wBitsPerSample);
        sl.AddTail(str);
        str.Format(_T("cbSize: %d (extra bytes)"), wfe.cbSize);
        sl.AddTail(str);

        sl.AddTail(_T(""));

        if (wfe.wFormatTag != WAVE_FORMAT_PCM && wfe.cbSize > 0 && formattype == FORMAT_WaveFormatEx) {
            if (wfe.wFormatTag == WAVE_FORMAT_EXTENSIBLE && wfe.cbSize == sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX)) {
                fmtsize = sizeof(WAVEFORMATEXTENSIBLE);

                WAVEFORMATEXTENSIBLE& wfe = *(WAVEFORMATEXTENSIBLE*)pbFormat;

                sl.AddTail(_T("WAVEFORMATEXTENSIBLE:"));
                if (wfe.Format.wBitsPerSample != 0) {
                    str.Format(_T("wValidBitsPerSample: %d"), wfe.Samples.wValidBitsPerSample);
                } else {
                    str.Format(_T("wSamplesPerBlock: %d"), wfe.Samples.wSamplesPerBlock);
                }
                sl.AddTail(str);
                str.Format(_T("dwChannelMask: 0x%08x"), wfe.dwChannelMask);
                sl.AddTail(str);
                str.Format(_T("SubFormat: %s"), CStringFromGUID(wfe.SubFormat));
                sl.AddTail(str);

                sl.AddTail(_T(""));
            } else if (wfe.wFormatTag == WAVE_FORMAT_DOLBY_AC3 && wfe.cbSize == sizeof(DOLBYAC3WAVEFORMAT) - sizeof(WAVEFORMATEX)) {
                fmtsize = sizeof(DOLBYAC3WAVEFORMAT);

                DOLBYAC3WAVEFORMAT& wfe = *(DOLBYAC3WAVEFORMAT*)pbFormat;

                sl.AddTail(_T("DOLBYAC3WAVEFORMAT:"));
                str.Format(_T("bBigEndian: %d"), wfe.bBigEndian);
                sl.AddTail(str);
                str.Format(_T("bsid: %d"), wfe.bsid);
                sl.AddTail(str);
                str.Format(_T("lfeon: %d"), wfe.lfeon);
                sl.AddTail(str);
                str.Format(_T("copyrightb: %d"), wfe.copyrightb);
                sl.AddTail(str);
                str.Format(_T("nAuxBitsCode: %d"), wfe.nAuxBitsCode);
                sl.AddTail(str);

                sl.AddTail(_T(""));
            }
        }
    } else if (formattype == FORMAT_VorbisFormat) {
        fmtsize = sizeof(VORBISFORMAT);

        VORBISFORMAT& vf = *(VORBISFORMAT*)pbFormat;

        sl.AddTail(_T("VORBISFORMAT:"));
        str.Format(_T("nChannels: %d"), vf.nChannels);
        sl.AddTail(str);
        str.Format(_T("nSamplesPerSec: %d"), vf.nSamplesPerSec);
        sl.AddTail(str);
        str.Format(_T("nMinBitsPerSec: %d"), vf.nMinBitsPerSec);
        sl.AddTail(str);
        str.Format(_T("nAvgBitsPerSec: %d"), vf.nAvgBitsPerSec);
        sl.AddTail(str);
        str.Format(_T("nMaxBitsPerSec: %d"), vf.nMaxBitsPerSec);
        sl.AddTail(str);
        str.Format(_T("fQuality: %.3f"), vf.fQuality);
        sl.AddTail(str);

        sl.AddTail(_T(""));
    } else if (formattype == FORMAT_VorbisFormat2) {
        fmtsize = sizeof(VORBISFORMAT2);

        VORBISFORMAT2& vf = *(VORBISFORMAT2*)pbFormat;

        sl.AddTail(_T("VORBISFORMAT:"));
        str.Format(_T("Channels: %d"), vf.Channels);
        sl.AddTail(str);
        str.Format(_T("SamplesPerSec: %d"), vf.SamplesPerSec);
        sl.AddTail(str);
        str.Format(_T("BitsPerSample: %d"), vf.BitsPerSample);
        sl.AddTail(str);
        str.Format(_T("HeaderSize: {%d, %d, %d}"), vf.HeaderSize[0], vf.HeaderSize[1], vf.HeaderSize[2]);
        sl.AddTail(str);

        sl.AddTail(_T(""));
    } else if (formattype == FORMAT_SubtitleInfo) {
        fmtsize = sizeof(SUBTITLEINFO);

        SUBTITLEINFO& si = *(SUBTITLEINFO*)pbFormat;

        sl.AddTail(_T("SUBTITLEINFO:"));
        str.Format(_T("dwOffset: %d"), si.dwOffset);
        sl.AddTail(str);
        str.Format(_T("IsoLang: %s"), CString(CStringA(si.IsoLang, sizeof(si.IsoLang) - 1)));
        sl.AddTail(str);
        str.Format(_T("TrackName: %s"), CString(CStringW(si.TrackName, sizeof(si.TrackName) - 1)));
        sl.AddTail(str);

        sl.AddTail(_T(""));
    }

    if (cbFormat > 0) {
        sl.AddTail(_T("pbFormat:"));

        for (ptrdiff_t i = 0, j = (cbFormat + 15) & ~15; i < j; i += 16) {
            str.Format(_T("%04x:"), i);

            for (ptrdiff_t k = i, l = min(i + 16, (int)cbFormat); k < l; k++) {
                CString byte;
                byte.Format(_T("%c%02x"), fmtsize > 0 && fmtsize == k ? '|' : ' ', pbFormat[k]);
                str += byte;
            }

            for (ptrdiff_t k = min(i + 16, (int)cbFormat), l = i + 16; k < l; k++) {
                str += _T("   ");
            }

            str += ' ';

            for (ptrdiff_t k = i, l = min(i + 16, (int)cbFormat); k < l; k++) {
                unsigned char c = (unsigned char)pbFormat[k];
                CStringA ch;
                ch.Format("%c", c >= 0x20 ? c : '.');
                str += ch;
            }

            sl.AddTail(str);
        }

        sl.AddTail(_T(""));
    }
}
