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

#ifndef __STREAM_RECEIVE_DATA_DEFINED
#define __STREAM_RECEIVE_DATA_DEFINED

#include "SetTotalLength.h"
#include "MediaPacketCollection.h"
#include "EndOfStreamReached.h"

/*

For stream without container is neded to specify stream input format.

*/

// raw ADTS AAC (Advanced Audio Coding)
#define STREAM_INPUT_FORMAT_AAC                                       L"aac"
// raw AC-3
#define STREAM_INPUT_FORMAT_AC3                                       L"ac3"
// ACT Voice file format
#define STREAM_INPUT_FORMAT_ACT                                       L"act"
// Artworx Data Format
#define STREAM_INPUT_FORMAT_ADF                                       L"adf"
// CRI ADX
#define STREAM_INPUT_FORMAT_ADX                                       L"adx"
// MD STUDIO audio
#define STREAM_INPUT_FORMAT_AEA                                       L"aea"
// Audio IFF
#define STREAM_INPUT_FORMAT_AIFF                                      L"aiff"
// 3GPP AMR
#define STREAM_INPUT_FORMAT_AMR                                       L"amr"
// Deluxe Paint Animation
#define STREAM_INPUT_FORMAT_ANM                                       L"anm"
// CRYO APC
#define STREAM_INPUT_FORMAT_APC                                       L"apc"
// Monkey's Audio
#define STREAM_INPUT_FORMAT_APE                                       L"ape"
// ASF (Advanced / Active Streaming Format)
#define STREAM_INPUT_FORMAT_ASF                                       L"asf"
// SSA (SubStation Alpha) subtitle
#define STREAM_INPUT_FORMAT_ASS                                       L"ass"
// Sun AU
#define STREAM_INPUT_FORMAT_AU                                        L"au"
// AVI (Audio Video Interleaved)
#define STREAM_INPUT_FORMAT_AVI                                       L"avi"
// AVS
#define STREAM_INPUT_FORMAT_AVS                                       L"avs"
// Bethesda Softworks VID
#define STREAM_INPUT_FORMAT_BETHSOFTVID                               L"bethsoftvid"
// Brute Force & Ignorance
#define STREAM_INPUT_FORMAT_BFI                                       L"bfi"
// Binary text
#define STREAM_INPUT_FORMAT_BIN                                       L"bin"
// Bink
#define STREAM_INPUT_FORMAT_BINK                                      L"bink"
// G.729 BIT file format
#define STREAM_INPUT_FORMAT_BIT                                       L"bit"
// Discworld II BMV
#define STREAM_INPUT_FORMAT_BMV                                       L"bmv"
// Interplay C93
#define STREAM_INPUT_FORMAT_C93                                       L"c93"
// Apple CAF (Core Audio Format)
#define STREAM_INPUT_FORMAT_CAF                                       L"caf"
// raw Chinese AVS (Audio Video Standard)
#define STREAM_INPUT_FORMAT_CAVSVIDEO                                 L"cavsvideo"
// CD Graphics
#define STREAM_INPUT_FORMAT_CDG                                       L"cdg"
// Commodore CDXL video
#define STREAM_INPUT_FORMAT_CDXL                                      L"cdxl"
// D-Cinema audio
#define STREAM_INPUT_FORMAT_DAUD                                      L"daud"
// Chronomaster DFA
#define STREAM_INPUT_FORMAT_DFA                                       L"dfa"
// raw Dirac
#define STREAM_INPUT_FORMAT_DIRAC                                     L"dirac"
// raw DNxHD (SMPTE VC-3)
#define STREAM_INPUT_FORMAT_DNXHD                                     L"dnxhd"
// Delphine Software International CIN
#define STREAM_INPUT_FORMAT_DSICIN                                    L"dsicin"
// raw DTS
#define STREAM_INPUT_FORMAT_DTS                                       L"dts"
// DV (Digital Video)
#define STREAM_INPUT_FORMAT_DV                                        L"dv"
// DXA
#define STREAM_INPUT_FORMAT_DXA                                       L"dxa"
// Electronic Arts Multimedia
#define STREAM_INPUT_FORMAT_EA                                        L"ea"
// Electronic Arts cdata
#define STREAM_INPUT_FORMAT_EA_CDATA                                  L"ea_cdata"
// raw E-AC-3
#define STREAM_INPUT_FORMAT_EAC3                                      L"eac3"
// FFM (FFserver live feed)
#define STREAM_INPUT_FORMAT_FFM                                       L"ffm"
// FFmpeg metadata in text
#define STREAM_INPUT_FORMAT_FFMETADATA                                L"ffmetadata"
// Adobe Filmstrip
#define STREAM_INPUT_FORMAT_FILMSTRIP                                 L"filmstrip"
// raw FLAC
#define STREAM_INPUT_FORMAT_FLAC                                      L"flac"
// FLI/FLC/FLX animation
#define STREAM_INPUT_FORMAT_FLIC                                      L"flic"
// FLV (Flash Video)
#define STREAM_INPUT_FORMAT_FLV                                       L"flv"
// 4X Technologies
#define STREAM_INPUT_FORMAT_4XM                                       L"4xm"
// raw G.722
#define STREAM_INPUT_FORMAT_G722                                      L"g722"
// G.723.1
#define STREAM_INPUT_FORMAT_G723_1                                    L"g723_1"
// G.729 raw format demuxer
#define STREAM_INPUT_FORMAT_G729                                      L"g729"
// raw GSM
#define STREAM_INPUT_FORMAT_GSM                                       L"gsm"
// GXF (General eXchange Format)
#define STREAM_INPUT_FORMAT_GXF                                       L"gxf"
// raw H.261
#define STREAM_INPUT_FORMAT_H261                                      L"h261"
// raw H.263
#define STREAM_INPUT_FORMAT_H263                                      L"h263"
// raw H.264 video
#define STREAM_INPUT_FORMAT_H264                                      L"h264"
// Apple HTTP Live Streaming
#define STREAM_INPUT_FORMAT_HLS                                       L"hls"
#define STREAM_INPUT_FORMAT_APPLEHTTP                                 L"applehttp"
// Microsoft Windows ICO
#define STREAM_INPUT_FORMAT_ICO                                       L"ico"
// id Cinematic
#define STREAM_INPUT_FORMAT_IDCIN                                     L"idcin"
// iCE Draw File
#define STREAM_INPUT_FORMAT_IDF                                       L"idf"
// IFF (Interchange File Format)
#define STREAM_INPUT_FORMAT_IFF                                       L"iff"
// iLBC storage
#define STREAM_INPUT_FORMAT_ILBC                                      L"ilbc"
// image2 sequence
#define STREAM_INPUT_FORMAT_IMAGE2                                    L"image2"
// piped image2 sequence
#define STREAM_INPUT_FORMAT_IMAGE2PIPE                                L"image2pipe"
// raw Ingenient MJPEG
#define STREAM_INPUT_FORMAT_INGENIENT                                 L"ingenient"
// Interplay MVE
#define STREAM_INPUT_FORMAT_IPMOVIE                                   L"ipmovie"
// Funcom ISS
#define STREAM_INPUT_FORMAT_ISS                                       L"iss"
// IndigoVision 8000 video
#define STREAM_INPUT_FORMAT_IV8                                       L"iv8"
// On2 IVF
#define STREAM_INPUT_FORMAT_IVF                                       L"ivf"
// JACOsub subtitle format
#define STREAM_INPUT_FORMAT_JACOSUB                                   L"jacosub"
// Bitmap Brothers JV
#define STREAM_INPUT_FORMAT_JV                                        L"jv"
// raw LOAS/LATM
#define STREAM_INPUT_FORMAT_LATM                                      L"latm"
// raw lmlm4
#define STREAM_INPUT_FORMAT_LMLM4                                     L"lmlm4"
// LOAS AudioSyncStream
#define STREAM_INPUT_FORMAT_LOAS                                      L"loas"
// VR native stream (LXF)
#define STREAM_INPUT_FORMAT_LXF                                       L"lxf"
// raw MPEG-4 video
#define STREAM_INPUT_FORMAT_M4V                                       L"m4v"
// Matroska/WebM
#define STREAM_INPUT_FORMAT_MATROSKA                                  L"matroska"
// Metal Gear Solid: The Twin Snakes
#define STREAM_INPUT_FORMAT_MGSTS                                     L"mgsts"
// MicroDVD subtitle format
#define STREAM_INPUT_FORMAT_MICRODVD                                  L"microdvd"
// raw MJPEG video
#define STREAM_INPUT_FORMAT_MJPEG                                     L"mjpeg"
// raw MLP
#define STREAM_INPUT_FORMAT_MLP                                       L"mlp"
// American Laser Games MM
#define STREAM_INPUT_FORMAT_MM                                        L"mm"
// Yamaha SMAF
#define STREAM_INPUT_FORMAT_MMF                                       L"mmf"
// QuickTime / MOV
#define STREAM_INPUT_FORMAT_MOV                                       L"mov"
#define STREAM_INPUT_FORMAT_MP4                                       L"mp4"
#define STREAM_INPUT_FORMAT_M4A                                       L"m4a"
#define STREAM_INPUT_FORMAT_3GP                                       L"3gp"
#define STREAM_INPUT_FORMAT_3G2                                       L"3g2"
#define STREAM_INPUT_FORMAT_MJ2                                       L"mj2"
// MP2/3 (MPEG audio layer 2/3)
#define STREAM_INPUT_FORMAT_MP3                                       L"mp3"
// Musepack
#define STREAM_INPUT_FORMAT_MPC                                       L"mpc"
// Musepack SV8
#define STREAM_INPUT_FORMAT_MPC8                                      L"mpc8"
// MPEG-PS (MPEG-2 Program Stream)
#define STREAM_INPUT_FORMAT_MPEG                                      L"mpeg"
// MPEG-TS (MPEG-2 Transport Stream)
#define STREAM_INPUT_FORMAT_MPEGTS                                    L"mpegts"
// raw MPEG-TS (MPEG-2 Transport Stream)
#define STREAM_INPUT_FORMAT_MPEGTSRAW                                 L"mpegtsraw"
// raw MPEG video
#define STREAM_INPUT_FORMAT_MPEGVIDEO                                 L"mpegvideo"
// MSN TCP Webcam stream
#define STREAM_INPUT_FORMAT_MSNWCTCP                                  L"msnwctcp"
// MTV
#define STREAM_INPUT_FORMAT_MTV                                       L"mtv"
// Motion Pixels MVI
#define STREAM_INPUT_FORMAT_MVI                                       L"mvi"
// MXF (Material eXchange Format)
#define STREAM_INPUT_FORMAT_MXF                                       L"mxf"
// MxPEG clip
#define STREAM_INPUT_FORMAT_MXG                                       L"mxg"
// NC camera feed
#define STREAM_INPUT_FORMAT_NC                                        L"nc"
// Nullsoft Streaming Video
#define STREAM_INPUT_FORMAT_NSV                                       L"nsv"
// NUT
#define STREAM_INPUT_FORMAT_NUT                                       L"nut"
// NuppelVideo
#define STREAM_INPUT_FORMAT_NUV                                       L"nuv"
// Ogg
#define STREAM_INPUT_FORMAT_OGG                                       L"ogg"
// Sony OpenMG audio
#define STREAM_INPUT_FORMAT_OMA                                       L"oma"
// Amazing Studio Packed Animation File
#define STREAM_INPUT_FORMAT_PAF                                       L"paf"
// PCM A-law
#define STREAM_INPUT_FORMAT_ALAW                                      L"alaw"
// PCM mu-law
#define STREAM_INPUT_FORMAT_MULAW                                     L"mulaw"
// PCM 64-bit floating-point big-endian
#define STREAM_INPUT_FORMAT_F64BE                                     L"f64be"
// PCM 64-bit floating-point little-endian
#define STREAM_INPUT_FORMAT_F64LE                                     L"f64le"
// PCM 32-bit floating-point big-endian
#define STREAM_INPUT_FORMAT_F32BE                                     L"f32be"
// PCM 32-bit floating-point little-endian
#define STREAM_INPUT_FORMAT_F32LE                                     L"f32le"
// PCM signed 32-bit big-endian
#define STREAM_INPUT_FORMAT_S32BE                                     L"s32be"
// PCM signed 32-bit little-endian
#define STREAM_INPUT_FORMAT_S32LE                                     L"s32le"
// PCM signed 24-bit big-endian
#define STREAM_INPUT_FORMAT_S24BE                                     L"s24be"
// PCM signed 24-bit little-endian
#define STREAM_INPUT_FORMAT_S24LE                                     L"s24le"
// PCM signed 16-bit big-endian
#define STREAM_INPUT_FORMAT_S16BE                                     L"s16be"
// PCM signed 16-bit little-endian
#define STREAM_INPUT_FORMAT_S16LE                                     L"s16le"
// PCM signed 8-bit
#define STREAM_INPUT_FORMAT_S8                                        L"s8"
// PCM unsigned 32-bit big-endian
#define STREAM_INPUT_FORMAT_U32BE                                     L"u32be"
// PCM unsigned 32-bit little-endian
#define STREAM_INPUT_FORMAT_U32LE                                     L"u32le"
// PCM unsigned 24-bit big-endian
#define STREAM_INPUT_FORMAT_U24BE                                     L"u24be"
// PCM unsigned 24-bit little-endian
#define STREAM_INPUT_FORMAT_U24LE                                     L"u24le"
// PCM unsigned 16-bit big-endian
#define STREAM_INPUT_FORMAT_U16BE                                     L"u16be"
// PCM unsigned 16-bit little-endian
#define STREAM_INPUT_FORMAT_U16LE                                     L"u16le"
// PCM unsigned 8-bit
#define STREAM_INPUT_FORMAT_U8                                        L"u8"
// Playstation Portable PMP
#define STREAM_INPUT_FORMAT_PMP                                       L"pmp"
// TechnoTrend PVA
#define STREAM_INPUT_FORMAT_PVA                                       L"pva"
// QCP
#define STREAM_INPUT_FORMAT_QCP                                       L"qcp"
// REDCODE R3D
#define STREAM_INPUT_FORMAT_R3D                                       L"r3d"
// raw video
#define STREAM_INPUT_FORMAT_RAWVIDEO                                  L"rawvideo"
// RealText subtitle format
#define STREAM_INPUT_FORMAT_REALTEXT                                  L"realtext"
// RL2
#define STREAM_INPUT_FORMAT_RL2                                       L"rl2"
// RealMedia
#define STREAM_INPUT_FORMAT_RM                                        L"rm"
// id RoQ
#define STREAM_INPUT_FORMAT_ROQ                                       L"roq"
// RPL / ARMovie
#define STREAM_INPUT_FORMAT_RPL                                       L"rpl"
// Lego Mindstorms RSO
#define STREAM_INPUT_FORMAT_RSO                                       L"rso"
// SAMI subtitle format
#define STREAM_INPUT_FORMAT_SAMI                                      L"sami"
// SBaGen binaural beats script
#define STREAM_INPUT_FORMAT_SBG                                       L"sbg"
// Sega FILM / CPK
#define STREAM_INPUT_FORMAT_FILM_CPK                                  L"film_cpk"
// raw Shorten
#define STREAM_INPUT_FORMAT_SHN                                       L"shn"
// Beam Software SIFF
#define STREAM_INPUT_FORMAT_SIFF                                      L"siff"
// Smacker
#define STREAM_INPUT_FORMAT_SMK                                       L"smk"
// Loki SDL MJPEG
#define STREAM_INPUT_FORMAT_SMJPEG                                    L"smjpeg"
// LucasArts Smush
#define STREAM_INPUT_FORMAT_SMUSH                                     L"smush"
// Sierra SOL
#define STREAM_INPUT_FORMAT_SOL                                       L"sol"
// SoX native
#define STREAM_INPUT_FORMAT_SOX                                       L"sox"
// IEC 61937 (compressed data in S/PDIF)
#define STREAM_INPUT_FORMAT_SPDIF                                     L"spdif"
// SubRip subtitle
#define STREAM_INPUT_FORMAT_SRT                                       L"srt"
// Sony Playstation STR
#define STREAM_INPUT_FORMAT_PSXSTR                                    L"psxstr"
// SubViewer subtitle format
#define STREAM_INPUT_FORMAT_SUBVIEWER                                 L"subviewer"
// SWF (ShockWave Flash)
#define STREAM_INPUT_FORMAT_SWF                                       L"swf"
// THP
#define STREAM_INPUT_FORMAT_THP                                       L"thp"
// Tiertex Limited SEQ
#define STREAM_INPUT_FORMAT_TIERTEXSEQ                                L"tiertexseq"
// 8088flex TMV
#define STREAM_INPUT_FORMAT_TMV                                       L"tmv"
// raw TrueHD
#define STREAM_INPUT_FORMAT_TRUEHD                                    L"truehd"
// TTA (True Audio)
#define STREAM_INPUT_FORMAT_TTA                                       L"tta"
// Renderware TeXture Dictionary
#define STREAM_INPUT_FORMAT_TXD                                       L"txd"
// Tele-typewriter
#define STREAM_INPUT_FORMAT_TTY                                       L"tty"
// raw VC-1
#define STREAM_INPUT_FORMAT_VC1                                       L"vc1"
// VC-1 test bitstream
#define STREAM_INPUT_FORMAT_VC1TEST                                   L"vc1test"
// Sierra VMD
#define STREAM_INPUT_FORMAT_VMD                                       L"vmd"
// Creative Voice
#define STREAM_INPUT_FORMAT_VOC                                       L"voc"
// Nippon Telegraph and Telephone Corporation (NTT) TwinVQ
#define STREAM_INPUT_FORMAT_VQF                                       L"vqf"
// Sony Wave64
#define STREAM_INPUT_FORMAT_W64                                       L"w64"
// WAV / WAVE (Waveform Audio)
#define STREAM_INPUT_FORMAT_WAV                                       L"wav"
// Wing Commander III movie
#define STREAM_INPUT_FORMAT_WC3MOVIE                                  L"wc3movie"
// Westwood Studios audio
#define STREAM_INPUT_FORMAT_WSAUD                                     L"wsaud"
// Westwood Studios VQA
#define STREAM_INPUT_FORMAT_WSVQA                                     L"wsvqa"
// Windows Television (WTV)
#define STREAM_INPUT_FORMAT_WTV                                       L"wtv"
// WavPack
#define STREAM_INPUT_FORMAT_WV                                        L"wv"
// Maxis XA
#define STREAM_INPUT_FORMAT_XA                                        L"xa"
// eXtended BINary text (XBIN)
#define STREAM_INPUT_FORMAT_XBIN                                      L"xbin"
// Microsoft XMV
#define STREAM_INPUT_FORMAT_XMV                                       L"xmv"
// Microsoft xWMA
#define STREAM_INPUT_FORMAT_XWMA                                      L"xwma"
// Psygnosis YOP
#define STREAM_INPUT_FORMAT_YOP                                       L"yop"
// YUV4MPEG pipe
#define STREAM_INPUT_FORMAT_YUV4MPEGPIPE                              L"yuv4mpegpipe"

#define STREAM_RECEIVE_DATA_FLAG_NONE                                 0x00000000
// specifies that received data are container (e.g. avi, mkv, flv, ...)
#define STREAM_RECEIVE_DATA_FLAG_CONTAINER                            0x00000001
// specifies that received data are demuxed packets ready to output pins
#define STREAM_RECEIVE_DATA_FLAG_PACKETS                              0x00000002

class CStreamReceiveData
{
public:
  CStreamReceiveData(void);
  ~CStreamReceiveData(void);

  /* get methods */

  // gets stream input format (if specified)
  // @return : stream input format or NULL if not specified
  const wchar_t *GetStreamInputFormat(void);

  // gets total length
  // @return : total length
  CSetTotalLength *GetTotalLength(void);

  // gets received media packets
  // @return : media packet collection
  CMediaPacketCollection *GetMediaPacketCollection(void);

  // gets end of stream reached
  // @return : end of stream reached
  CEndOfStreamReached *GetEndOfStreamReached(void);

  // gets combination of set flags
  // @return : combination of set flags
  unsigned int GetFlags(void);

  /* set methods */

  // sets stream input format
  // @param streamInputFormat : stream input format to set
  // @return : true if successful, false otherwise
  bool SetStreamInputFormat(const wchar_t *streamInputFormat);

  // sets combination of flags
  // @param flags : the combination of flags to set
  void SetFlags(unsigned int flags);

  // sets if received data are in container
  // @param container : true if received data are in container, false otherwise
  void SetContainer(bool container);

  // sets if received data are in packets ready to output pins
  // @param packets : true if received data are in packets ready to output pins, false otherwise
  void SetPackets(bool packets);

  /* other methods */

  // tests if received data are in container
  // @return : true if received data are in container, false otherwise
  bool IsContainer(void);

  // tests if received data are in packets ready to output pins
  // @return : true if received data are in packets ready to output pins, false otherwise
  bool IsPackets(void);

  // tests if specific combination of flags is set
  // @param flags : the set of flags to test
  // @return : true if set of flags is set, false otherwise
  bool IsSetFlags(unsigned int flags);

  // clears current instance to default state
  void Clear(void);

private:

  // holds stream input format
  // can be NULL, but for stream without container is recommened to specify it - automatic guess in that case is not working properly
  wchar_t *streamInputFormat;

  // holds various flags
  unsigned int flags;

  // holds total length
  CSetTotalLength *totalLength;

  // holds received media packets
  CMediaPacketCollection *mediaPackets;

  // holds end of stream reached
  CEndOfStreamReached *endOfStreamReached;
};

#endif