//-----------------------------------------------------------------------------
//
//	MONOGRAM AC3 Encoder
//
//	Based on libavcodec AC-3 encoder
//
//	Author : Igor Janos
//
//-----------------------------------------------------------------------------
#include "..\source\stdafx.h"
#include "ac3enc.h"
#include "bits.h"

#define _USE_MATH_DEFINES
#include <math.h>

typedef UINT8 uint8;
typedef UINT16 uint16;
typedef UINT32 uint32;


/*
 * The simplest AC-3 encoder
 * Copyright (c) 2000 Fabrice Bellard.
 *
 * This file is part of FFmpeg.
 *
 * FFmpeg is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * FFmpeg is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with FFmpeg; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

#define FFMAX(a,b) ((a) > (b) ? (a) : (b))
#define FFMAX3(a,b,c) FFMAX(FFMAX(a,b),c)
#define FFMIN(a,b) ((a) > (b) ? (b) : (a))
#define FFMIN3(a,b,c) FFMIN(FFMIN(a,b),c)
#define FFABS(a) ((a) >= 0 ? (a) : (-(a)))
#define FFSIGN(a) ((a) > 0 ? 1 : -1)

#define FFSWAP(type,a,b) do{type SWAP_tmp= b; b= a; a= SWAP_tmp;}while(0)
#define FF_ARRAY_ELEMS(a) (sizeof(a) / sizeof((a)[0]))

typedef uint32 AVCRC;

typedef enum {
    AV_CRC_8_ATM,
    AV_CRC_16_ANSI,
    AV_CRC_16_CCITT,
    AV_CRC_32_IEEE,
    AV_CRC_32_IEEE_LE,  /*< reversed bitorder version of AV_CRC_32_IEEE */
    AV_CRC_MAX,         /*< not part of public API! don't use outside lavu */
} AVCRCId;

static const AVCRC av_crc_table[AV_CRC_MAX][257] = {
    {
        0x00, 0x07, 0x0E, 0x09, 0x1C, 0x1B, 0x12, 0x15, 0x38, 0x3F, 0x36, 0x31,
        0x24, 0x23, 0x2A, 0x2D, 0x70, 0x77, 0x7E, 0x79, 0x6C, 0x6B, 0x62, 0x65,
        0x48, 0x4F, 0x46, 0x41, 0x54, 0x53, 0x5A, 0x5D, 0xE0, 0xE7, 0xEE, 0xE9,
        0xFC, 0xFB, 0xF2, 0xF5, 0xD8, 0xDF, 0xD6, 0xD1, 0xC4, 0xC3, 0xCA, 0xCD,
        0x90, 0x97, 0x9E, 0x99, 0x8C, 0x8B, 0x82, 0x85, 0xA8, 0xAF, 0xA6, 0xA1,
        0xB4, 0xB3, 0xBA, 0xBD, 0xC7, 0xC0, 0xC9, 0xCE, 0xDB, 0xDC, 0xD5, 0xD2,
        0xFF, 0xF8, 0xF1, 0xF6, 0xE3, 0xE4, 0xED, 0xEA, 0xB7, 0xB0, 0xB9, 0xBE,
        0xAB, 0xAC, 0xA5, 0xA2, 0x8F, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9D, 0x9A,
        0x27, 0x20, 0x29, 0x2E, 0x3B, 0x3C, 0x35, 0x32, 0x1F, 0x18, 0x11, 0x16,
        0x03, 0x04, 0x0D, 0x0A, 0x57, 0x50, 0x59, 0x5E, 0x4B, 0x4C, 0x45, 0x42,
        0x6F, 0x68, 0x61, 0x66, 0x73, 0x74, 0x7D, 0x7A, 0x89, 0x8E, 0x87, 0x80,
        0x95, 0x92, 0x9B, 0x9C, 0xB1, 0xB6, 0xBF, 0xB8, 0xAD, 0xAA, 0xA3, 0xA4,
        0xF9, 0xFE, 0xF7, 0xF0, 0xE5, 0xE2, 0xEB, 0xEC, 0xC1, 0xC6, 0xCF, 0xC8,
        0xDD, 0xDA, 0xD3, 0xD4, 0x69, 0x6E, 0x67, 0x60, 0x75, 0x72, 0x7B, 0x7C,
        0x51, 0x56, 0x5F, 0x58, 0x4D, 0x4A, 0x43, 0x44, 0x19, 0x1E, 0x17, 0x10,
        0x05, 0x02, 0x0B, 0x0C, 0x21, 0x26, 0x2F, 0x28, 0x3D, 0x3A, 0x33, 0x34,
        0x4E, 0x49, 0x40, 0x47, 0x52, 0x55, 0x5C, 0x5B, 0x76, 0x71, 0x78, 0x7F,
        0x6A, 0x6D, 0x64, 0x63, 0x3E, 0x39, 0x30, 0x37, 0x22, 0x25, 0x2C, 0x2B,
        0x06, 0x01, 0x08, 0x0F, 0x1A, 0x1D, 0x14, 0x13, 0xAE, 0xA9, 0xA0, 0xA7,
        0xB2, 0xB5, 0xBC, 0xBB, 0x96, 0x91, 0x98, 0x9F, 0x8A, 0x8D, 0x84, 0x83,
        0xDE, 0xD9, 0xD0, 0xD7, 0xC2, 0xC5, 0xCC, 0xCB, 0xE6, 0xE1, 0xE8, 0xEF,
        0xFA, 0xFD, 0xF4, 0xF3, 0x01
    },
    {
        0x0000, 0x0580, 0x0F80, 0x0A00, 0x1B80, 0x1E00, 0x1400, 0x1180,
        0x3380, 0x3600, 0x3C00, 0x3980, 0x2800, 0x2D80, 0x2780, 0x2200,
        0x6380, 0x6600, 0x6C00, 0x6980, 0x7800, 0x7D80, 0x7780, 0x7200,
        0x5000, 0x5580, 0x5F80, 0x5A00, 0x4B80, 0x4E00, 0x4400, 0x4180,
        0xC380, 0xC600, 0xCC00, 0xC980, 0xD800, 0xDD80, 0xD780, 0xD200,
        0xF000, 0xF580, 0xFF80, 0xFA00, 0xEB80, 0xEE00, 0xE400, 0xE180,
        0xA000, 0xA580, 0xAF80, 0xAA00, 0xBB80, 0xBE00, 0xB400, 0xB180,
        0x9380, 0x9600, 0x9C00, 0x9980, 0x8800, 0x8D80, 0x8780, 0x8200,
        0x8381, 0x8601, 0x8C01, 0x8981, 0x9801, 0x9D81, 0x9781, 0x9201,
        0xB001, 0xB581, 0xBF81, 0xBA01, 0xAB81, 0xAE01, 0xA401, 0xA181,
        0xE001, 0xE581, 0xEF81, 0xEA01, 0xFB81, 0xFE01, 0xF401, 0xF181,
        0xD381, 0xD601, 0xDC01, 0xD981, 0xC801, 0xCD81, 0xC781, 0xC201,
        0x4001, 0x4581, 0x4F81, 0x4A01, 0x5B81, 0x5E01, 0x5401, 0x5181,
        0x7381, 0x7601, 0x7C01, 0x7981, 0x6801, 0x6D81, 0x6781, 0x6201,
        0x2381, 0x2601, 0x2C01, 0x2981, 0x3801, 0x3D81, 0x3781, 0x3201,
        0x1001, 0x1581, 0x1F81, 0x1A01, 0x0B81, 0x0E01, 0x0401, 0x0181,
        0x0383, 0x0603, 0x0C03, 0x0983, 0x1803, 0x1D83, 0x1783, 0x1203,
        0x3003, 0x3583, 0x3F83, 0x3A03, 0x2B83, 0x2E03, 0x2403, 0x2183,
        0x6003, 0x6583, 0x6F83, 0x6A03, 0x7B83, 0x7E03, 0x7403, 0x7183,
        0x5383, 0x5603, 0x5C03, 0x5983, 0x4803, 0x4D83, 0x4783, 0x4203,
        0xC003, 0xC583, 0xCF83, 0xCA03, 0xDB83, 0xDE03, 0xD403, 0xD183,
        0xF383, 0xF603, 0xFC03, 0xF983, 0xE803, 0xED83, 0xE783, 0xE203,
        0xA383, 0xA603, 0xAC03, 0xA983, 0xB803, 0xBD83, 0xB783, 0xB203,
        0x9003, 0x9583, 0x9F83, 0x9A03, 0x8B83, 0x8E03, 0x8403, 0x8183,
        0x8002, 0x8582, 0x8F82, 0x8A02, 0x9B82, 0x9E02, 0x9402, 0x9182,
        0xB382, 0xB602, 0xBC02, 0xB982, 0xA802, 0xAD82, 0xA782, 0xA202,
        0xE382, 0xE602, 0xEC02, 0xE982, 0xF802, 0xFD82, 0xF782, 0xF202,
        0xD002, 0xD582, 0xDF82, 0xDA02, 0xCB82, 0xCE02, 0xC402, 0xC182,
        0x4382, 0x4602, 0x4C02, 0x4982, 0x5802, 0x5D82, 0x5782, 0x5202,
        0x7002, 0x7582, 0x7F82, 0x7A02, 0x6B82, 0x6E02, 0x6402, 0x6182,
        0x2002, 0x2582, 0x2F82, 0x2A02, 0x3B82, 0x3E02, 0x3402, 0x3182,
        0x1382, 0x1602, 0x1C02, 0x1982, 0x0802, 0x0D82, 0x0782, 0x0202,
        0x0001
    },
    {
        0x0000, 0x2110, 0x4220, 0x6330, 0x8440, 0xA550, 0xC660, 0xE770,
        0x0881, 0x2991, 0x4AA1, 0x6BB1, 0x8CC1, 0xADD1, 0xCEE1, 0xEFF1,
        0x3112, 0x1002, 0x7332, 0x5222, 0xB552, 0x9442, 0xF772, 0xD662,
        0x3993, 0x1883, 0x7BB3, 0x5AA3, 0xBDD3, 0x9CC3, 0xFFF3, 0xDEE3,
        0x6224, 0x4334, 0x2004, 0x0114, 0xE664, 0xC774, 0xA444, 0x8554,
        0x6AA5, 0x4BB5, 0x2885, 0x0995, 0xEEE5, 0xCFF5, 0xACC5, 0x8DD5,
        0x5336, 0x7226, 0x1116, 0x3006, 0xD776, 0xF666, 0x9556, 0xB446,
        0x5BB7, 0x7AA7, 0x1997, 0x3887, 0xDFF7, 0xFEE7, 0x9DD7, 0xBCC7,
        0xC448, 0xE558, 0x8668, 0xA778, 0x4008, 0x6118, 0x0228, 0x2338,
        0xCCC9, 0xEDD9, 0x8EE9, 0xAFF9, 0x4889, 0x6999, 0x0AA9, 0x2BB9,
        0xF55A, 0xD44A, 0xB77A, 0x966A, 0x711A, 0x500A, 0x333A, 0x122A,
        0xFDDB, 0xDCCB, 0xBFFB, 0x9EEB, 0x799B, 0x588B, 0x3BBB, 0x1AAB,
        0xA66C, 0x877C, 0xE44C, 0xC55C, 0x222C, 0x033C, 0x600C, 0x411C,
        0xAEED, 0x8FFD, 0xECCD, 0xCDDD, 0x2AAD, 0x0BBD, 0x688D, 0x499D,
        0x977E, 0xB66E, 0xD55E, 0xF44E, 0x133E, 0x322E, 0x511E, 0x700E,
        0x9FFF, 0xBEEF, 0xDDDF, 0xFCCF, 0x1BBF, 0x3AAF, 0x599F, 0x788F,
        0x8891, 0xA981, 0xCAB1, 0xEBA1, 0x0CD1, 0x2DC1, 0x4EF1, 0x6FE1,
        0x8010, 0xA100, 0xC230, 0xE320, 0x0450, 0x2540, 0x4670, 0x6760,
        0xB983, 0x9893, 0xFBA3, 0xDAB3, 0x3DC3, 0x1CD3, 0x7FE3, 0x5EF3,
        0xB102, 0x9012, 0xF322, 0xD232, 0x3542, 0x1452, 0x7762, 0x5672,
        0xEAB5, 0xCBA5, 0xA895, 0x8985, 0x6EF5, 0x4FE5, 0x2CD5, 0x0DC5,
        0xE234, 0xC324, 0xA014, 0x8104, 0x6674, 0x4764, 0x2454, 0x0544,
        0xDBA7, 0xFAB7, 0x9987, 0xB897, 0x5FE7, 0x7EF7, 0x1DC7, 0x3CD7,
        0xD326, 0xF236, 0x9106, 0xB016, 0x5766, 0x7676, 0x1546, 0x3456,
        0x4CD9, 0x6DC9, 0x0EF9, 0x2FE9, 0xC899, 0xE989, 0x8AB9, 0xABA9,
        0x4458, 0x6548, 0x0678, 0x2768, 0xC018, 0xE108, 0x8238, 0xA328,
        0x7DCB, 0x5CDB, 0x3FEB, 0x1EFB, 0xF98B, 0xD89B, 0xBBAB, 0x9ABB,
        0x754A, 0x545A, 0x376A, 0x167A, 0xF10A, 0xD01A, 0xB32A, 0x923A,
        0x2EFD, 0x0FED, 0x6CDD, 0x4DCD, 0xAABD, 0x8BAD, 0xE89D, 0xC98D,
        0x267C, 0x076C, 0x645C, 0x454C, 0xA23C, 0x832C, 0xE01C, 0xC10C,
        0x1FEF, 0x3EFF, 0x5DCF, 0x7CDF, 0x9BAF, 0xBABF, 0xD98F, 0xF89F,
        0x176E, 0x367E, 0x554E, 0x745E, 0x932E, 0xB23E, 0xD10E, 0xF01E,
        0x0001
    },
    {
        0x00000000, 0xB71DC104, 0x6E3B8209, 0xD926430D, 0xDC760413, 0x6B6BC517,
        0xB24D861A, 0x0550471E, 0xB8ED0826, 0x0FF0C922, 0xD6D68A2F, 0x61CB4B2B,
        0x649B0C35, 0xD386CD31, 0x0AA08E3C, 0xBDBD4F38, 0x70DB114C, 0xC7C6D048,
        0x1EE09345, 0xA9FD5241, 0xACAD155F, 0x1BB0D45B, 0xC2969756, 0x758B5652,
        0xC836196A, 0x7F2BD86E, 0xA60D9B63, 0x11105A67, 0x14401D79, 0xA35DDC7D,
        0x7A7B9F70, 0xCD665E74, 0xE0B62398, 0x57ABE29C, 0x8E8DA191, 0x39906095,
        0x3CC0278B, 0x8BDDE68F, 0x52FBA582, 0xE5E66486, 0x585B2BBE, 0xEF46EABA,
        0x3660A9B7, 0x817D68B3, 0x842D2FAD, 0x3330EEA9, 0xEA16ADA4, 0x5D0B6CA0,
        0x906D32D4, 0x2770F3D0, 0xFE56B0DD, 0x494B71D9, 0x4C1B36C7, 0xFB06F7C3,
        0x2220B4CE, 0x953D75CA, 0x28803AF2, 0x9F9DFBF6, 0x46BBB8FB, 0xF1A679FF,
        0xF4F63EE1, 0x43EBFFE5, 0x9ACDBCE8, 0x2DD07DEC, 0x77708634, 0xC06D4730,
        0x194B043D, 0xAE56C539, 0xAB068227, 0x1C1B4323, 0xC53D002E, 0x7220C12A,
        0xCF9D8E12, 0x78804F16, 0xA1A60C1B, 0x16BBCD1F, 0x13EB8A01, 0xA4F64B05,
        0x7DD00808, 0xCACDC90C, 0x07AB9778, 0xB0B6567C, 0x69901571, 0xDE8DD475,
        0xDBDD936B, 0x6CC0526F, 0xB5E61162, 0x02FBD066, 0xBF469F5E, 0x085B5E5A,
        0xD17D1D57, 0x6660DC53, 0x63309B4D, 0xD42D5A49, 0x0D0B1944, 0xBA16D840,
        0x97C6A5AC, 0x20DB64A8, 0xF9FD27A5, 0x4EE0E6A1, 0x4BB0A1BF, 0xFCAD60BB,
        0x258B23B6, 0x9296E2B2, 0x2F2BAD8A, 0x98366C8E, 0x41102F83, 0xF60DEE87,
        0xF35DA999, 0x4440689D, 0x9D662B90, 0x2A7BEA94, 0xE71DB4E0, 0x500075E4,
        0x892636E9, 0x3E3BF7ED, 0x3B6BB0F3, 0x8C7671F7, 0x555032FA, 0xE24DF3FE,
        0x5FF0BCC6, 0xE8ED7DC2, 0x31CB3ECF, 0x86D6FFCB, 0x8386B8D5, 0x349B79D1,
        0xEDBD3ADC, 0x5AA0FBD8, 0xEEE00C69, 0x59FDCD6D, 0x80DB8E60, 0x37C64F64,
        0x3296087A, 0x858BC97E, 0x5CAD8A73, 0xEBB04B77, 0x560D044F, 0xE110C54B,
        0x38368646, 0x8F2B4742, 0x8A7B005C, 0x3D66C158, 0xE4408255, 0x535D4351,
        0x9E3B1D25, 0x2926DC21, 0xF0009F2C, 0x471D5E28, 0x424D1936, 0xF550D832,
        0x2C769B3F, 0x9B6B5A3B, 0x26D61503, 0x91CBD407, 0x48ED970A, 0xFFF0560E,
        0xFAA01110, 0x4DBDD014, 0x949B9319, 0x2386521D, 0x0E562FF1, 0xB94BEEF5,
        0x606DADF8, 0xD7706CFC, 0xD2202BE2, 0x653DEAE6, 0xBC1BA9EB, 0x0B0668EF,
        0xB6BB27D7, 0x01A6E6D3, 0xD880A5DE, 0x6F9D64DA, 0x6ACD23C4, 0xDDD0E2C0,
        0x04F6A1CD, 0xB3EB60C9, 0x7E8D3EBD, 0xC990FFB9, 0x10B6BCB4, 0xA7AB7DB0,
        0xA2FB3AAE, 0x15E6FBAA, 0xCCC0B8A7, 0x7BDD79A3, 0xC660369B, 0x717DF79F,
        0xA85BB492, 0x1F467596, 0x1A163288, 0xAD0BF38C, 0x742DB081, 0xC3307185,
        0x99908A5D, 0x2E8D4B59, 0xF7AB0854, 0x40B6C950, 0x45E68E4E, 0xF2FB4F4A,
        0x2BDD0C47, 0x9CC0CD43, 0x217D827B, 0x9660437F, 0x4F460072, 0xF85BC176,
        0xFD0B8668, 0x4A16476C, 0x93300461, 0x242DC565, 0xE94B9B11, 0x5E565A15,
        0x87701918, 0x306DD81C, 0x353D9F02, 0x82205E06, 0x5B061D0B, 0xEC1BDC0F,
        0x51A69337, 0xE6BB5233, 0x3F9D113E, 0x8880D03A, 0x8DD09724, 0x3ACD5620,
        0xE3EB152D, 0x54F6D429, 0x7926A9C5, 0xCE3B68C1, 0x171D2BCC, 0xA000EAC8,
        0xA550ADD6, 0x124D6CD2, 0xCB6B2FDF, 0x7C76EEDB, 0xC1CBA1E3, 0x76D660E7,
        0xAFF023EA, 0x18EDE2EE, 0x1DBDA5F0, 0xAAA064F4, 0x738627F9, 0xC49BE6FD,
        0x09FDB889, 0xBEE0798D, 0x67C63A80, 0xD0DBFB84, 0xD58BBC9A, 0x62967D9E,
        0xBBB03E93, 0x0CADFF97, 0xB110B0AF, 0x060D71AB, 0xDF2B32A6, 0x6836F3A2,
        0x6D66B4BC, 0xDA7B75B8, 0x035D36B5, 0xB440F7B1, 0x00000001
    },
    {
        0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F,
        0xE963A535, 0x9E6495A3, 0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988,
        0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 0x1DB71064, 0x6AB020F2,
        0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7,
        0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9,
        0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
        0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C,
        0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59,
        0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423,
        0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924,
        0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D, 0x76DC4190, 0x01DB7106,
        0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
        0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D,
        0x91646C97, 0xE6635C01, 0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E,
        0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950,
        0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
        0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541, 0x3DD895D7,
        0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
        0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA,
        0xBE0B1010, 0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F,
        0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81,
        0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A,
        0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683, 0xE3630B12, 0x94643B84,
        0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
        0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB,
        0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC,
        0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8, 0xA1D1937E,
        0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B,
        0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55,
        0x316E8EEF, 0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
        0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28,
        0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D,
        0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F,
        0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38,
        0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242,
        0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
        0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69,
        0x616BFFD3, 0x166CCF45, 0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2,
        0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC,
        0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
        0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693,
        0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
        0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D, 0x00000001
    }
};

const AVCRC *av_crc_get_table(AVCRCId crc_id)
{
    return av_crc_table[crc_id];
}

uint32 av_crc(const AVCRC *ctx, uint32 crc, const uint8 *buffer, int length)
{
    const uint8 *end= buffer+length;
    while(buffer<end)
        crc = ctx[((uint8)crc) ^ *buffer++] ^ (crc >> 8);
    return crc;
}



const uint8		ff_log2_tab[256]={
        0,0,1,1,2,2,2,2,3,3,3,3,3,3,3,3,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,
        5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
        6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
        6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
        7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
        7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
        7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
        7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7
};


const uint8 ff_reverse[256] = {
	0x00,0x80,0x40,0xC0,0x20,0xA0,0x60,0xE0,0x10,0x90,0x50,0xD0,0x30,0xB0,0x70,0xF0,
	0x08,0x88,0x48,0xC8,0x28,0xA8,0x68,0xE8,0x18,0x98,0x58,0xD8,0x38,0xB8,0x78,0xF8,
	0x04,0x84,0x44,0xC4,0x24,0xA4,0x64,0xE4,0x14,0x94,0x54,0xD4,0x34,0xB4,0x74,0xF4,
	0x0C,0x8C,0x4C,0xCC,0x2C,0xAC,0x6C,0xEC,0x1C,0x9C,0x5C,0xDC,0x3C,0xBC,0x7C,0xFC,
	0x02,0x82,0x42,0xC2,0x22,0xA2,0x62,0xE2,0x12,0x92,0x52,0xD2,0x32,0xB2,0x72,0xF2,
	0x0A,0x8A,0x4A,0xCA,0x2A,0xAA,0x6A,0xEA,0x1A,0x9A,0x5A,0xDA,0x3A,0xBA,0x7A,0xFA,
	0x06,0x86,0x46,0xC6,0x26,0xA6,0x66,0xE6,0x16,0x96,0x56,0xD6,0x36,0xB6,0x76,0xF6,
	0x0E,0x8E,0x4E,0xCE,0x2E,0xAE,0x6E,0xEE,0x1E,0x9E,0x5E,0xDE,0x3E,0xBE,0x7E,0xFE,
	0x01,0x81,0x41,0xC1,0x21,0xA1,0x61,0xE1,0x11,0x91,0x51,0xD1,0x31,0xB1,0x71,0xF1,
	0x09,0x89,0x49,0xC9,0x29,0xA9,0x69,0xE9,0x19,0x99,0x59,0xD9,0x39,0xB9,0x79,0xF9,
	0x05,0x85,0x45,0xC5,0x25,0xA5,0x65,0xE5,0x15,0x95,0x55,0xD5,0x35,0xB5,0x75,0xF5,
	0x0D,0x8D,0x4D,0xCD,0x2D,0xAD,0x6D,0xED,0x1D,0x9D,0x5D,0xDD,0x3D,0xBD,0x7D,0xFD,
	0x03,0x83,0x43,0xC3,0x23,0xA3,0x63,0xE3,0x13,0x93,0x53,0xD3,0x33,0xB3,0x73,0xF3,
	0x0B,0x8B,0x4B,0xCB,0x2B,0xAB,0x6B,0xEB,0x1B,0x9B,0x5B,0xDB,0x3B,0xBB,0x7B,0xFB,
	0x07,0x87,0x47,0xC7,0x27,0xA7,0x67,0xE7,0x17,0x97,0x57,0xD7,0x37,0xB7,0x77,0xF7,
	0x0F,0x8F,0x4F,0xCF,0x2F,0xAF,0x6F,0xEF,0x1F,0x9F,0x5F,0xDF,0x3F,0xBF,0x7F,0xFF,
};

/**
 * Possible frame sizes.
 * from ATSC A/52 Table 5.18 Frame Size Code Table.
 */
const uint16 ff_ac3_frame_size_tab[38][3] = {
    { 64,   69,   96   },
    { 64,   70,   96   },
    { 80,   87,   120  },
    { 80,   88,   120  },
    { 96,   104,  144  },
    { 96,   105,  144  },
    { 112,  121,  168  },
    { 112,  122,  168  },
    { 128,  139,  192  },
    { 128,  140,  192  },
    { 160,  174,  240  },
    { 160,  175,  240  },
    { 192,  208,  288  },
    { 192,  209,  288  },
    { 224,  243,  336  },
    { 224,  244,  336  },
    { 256,  278,  384  },
    { 256,  279,  384  },
    { 320,  348,  480  },
    { 320,  349,  480  },
    { 384,  417,  576  },
    { 384,  418,  576  },
    { 448,  487,  672  },
    { 448,  488,  672  },
    { 512,  557,  768  },
    { 512,  558,  768  },
    { 640,  696,  960  },
    { 640,  697,  960  },
    { 768,  835,  1152 },
    { 768,  836,  1152 },
    { 896,  975,  1344 },
    { 896,  976,  1344 },
    { 1024, 1114, 1536 },
    { 1024, 1115, 1536 },
    { 1152, 1253, 1728 },
    { 1152, 1254, 1728 },
    { 1280, 1393, 1920 },
    { 1280, 1394, 1920 },
};

/**
 * Maps audio coding mode (acmod) to number of full-bandwidth channels.
 * from ATSC A/52 Table 5.8 Audio Coding Mode
 */
const uint8 ff_ac3_channels_tab[8] = {
    2, 1, 2, 3, 3, 4, 4, 5
};

/* possible frequencies */
const uint16 ff_ac3_sample_rate_tab[3] = { 48000, 44100, 32000 };

/* possible bitrates */
const uint16 ff_ac3_bitrate_tab[19] = {
    32, 40, 48, 56, 64, 80, 96, 112, 128,
    160, 192, 224, 256, 320, 384, 448, 512, 576, 640
};

/* AC-3 MDCT window */

/* MDCT window */
const int16 ff_ac3_window[256] = {
    4,    7,   12,   16,   21,   28,   34,   42,
   51,   61,   72,   84,   97,  111,  127,  145,
  164,  184,  207,  231,  257,  285,  315,  347,
  382,  419,  458,  500,  544,  591,  641,  694,
  750,  810,  872,  937, 1007, 1079, 1155, 1235,
 1318, 1406, 1497, 1593, 1692, 1796, 1903, 2016,
 2132, 2253, 2379, 2509, 2644, 2783, 2927, 3076,
 3230, 3389, 3552, 3721, 3894, 4072, 4255, 4444,
 4637, 4835, 5038, 5246, 5459, 5677, 5899, 6127,
 6359, 6596, 6837, 7083, 7334, 7589, 7848, 8112,
 8380, 8652, 8927, 9207, 9491, 9778,10069,10363,
10660,10960,11264,11570,11879,12190,12504,12820,
13138,13458,13780,14103,14427,14753,15079,15407,
15735,16063,16392,16720,17049,17377,17705,18032,
18358,18683,19007,19330,19651,19970,20287,20602,
20914,21225,21532,21837,22139,22438,22733,23025,
23314,23599,23880,24157,24430,24699,24964,25225,
25481,25732,25979,26221,26459,26691,26919,27142,
27359,27572,27780,27983,28180,28373,28560,28742,
28919,29091,29258,29420,29577,29729,29876,30018,
30155,30288,30415,30538,30657,30771,30880,30985,
31086,31182,31274,31363,31447,31528,31605,31678,
31747,31814,31877,31936,31993,32046,32097,32145,
32190,32232,32272,32310,32345,32378,32409,32438,
32465,32490,32513,32535,32556,32574,32592,32608,
32623,32636,32649,32661,32671,32681,32690,32698,
32705,32712,32718,32724,32729,32733,32737,32741,
32744,32747,32750,32752,32754,32756,32757,32759,
32760,32761,32762,32763,32764,32764,32765,32765,
32766,32766,32766,32766,32767,32767,32767,32767,
32767,32767,32767,32767,32767,32767,32767,32767,
32767,32767,32767,32767,32767,32767,32767,32767,
};

const uint8 ff_ac3_log_add_tab[260]= {
0x40,0x3f,0x3e,0x3d,0x3c,0x3b,0x3a,0x39,0x38,0x37,
0x36,0x35,0x34,0x34,0x33,0x32,0x31,0x30,0x2f,0x2f,
0x2e,0x2d,0x2c,0x2c,0x2b,0x2a,0x29,0x29,0x28,0x27,
0x26,0x26,0x25,0x24,0x24,0x23,0x23,0x22,0x21,0x21,
0x20,0x20,0x1f,0x1e,0x1e,0x1d,0x1d,0x1c,0x1c,0x1b,
0x1b,0x1a,0x1a,0x19,0x19,0x18,0x18,0x17,0x17,0x16,
0x16,0x15,0x15,0x15,0x14,0x14,0x13,0x13,0x13,0x12,
0x12,0x12,0x11,0x11,0x11,0x10,0x10,0x10,0x0f,0x0f,
0x0f,0x0e,0x0e,0x0e,0x0d,0x0d,0x0d,0x0d,0x0c,0x0c,
0x0c,0x0c,0x0b,0x0b,0x0b,0x0b,0x0a,0x0a,0x0a,0x0a,
0x0a,0x09,0x09,0x09,0x09,0x09,0x08,0x08,0x08,0x08,
0x08,0x08,0x07,0x07,0x07,0x07,0x07,0x07,0x06,0x06,
0x06,0x06,0x06,0x06,0x06,0x06,0x05,0x05,0x05,0x05,
0x05,0x05,0x05,0x05,0x04,0x04,0x04,0x04,0x04,0x04,
0x04,0x04,0x04,0x04,0x04,0x03,0x03,0x03,0x03,0x03,
0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x02,
0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,
0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x02,0x01,0x01,
0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,0x01,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
};

const uint16 ff_ac3_hearing_threshold_tab[50][3]= {
{ 0x04d0,0x04f0,0x0580 },
{ 0x04d0,0x04f0,0x0580 },
{ 0x0440,0x0460,0x04b0 },
{ 0x0400,0x0410,0x0450 },
{ 0x03e0,0x03e0,0x0420 },
{ 0x03c0,0x03d0,0x03f0 },
{ 0x03b0,0x03c0,0x03e0 },
{ 0x03b0,0x03b0,0x03d0 },
{ 0x03a0,0x03b0,0x03c0 },
{ 0x03a0,0x03a0,0x03b0 },
{ 0x03a0,0x03a0,0x03b0 },
{ 0x03a0,0x03a0,0x03b0 },
{ 0x03a0,0x03a0,0x03a0 },
{ 0x0390,0x03a0,0x03a0 },
{ 0x0390,0x0390,0x03a0 },
{ 0x0390,0x0390,0x03a0 },
{ 0x0380,0x0390,0x03a0 },
{ 0x0380,0x0380,0x03a0 },
{ 0x0370,0x0380,0x03a0 },
{ 0x0370,0x0380,0x03a0 },
{ 0x0360,0x0370,0x0390 },
{ 0x0360,0x0370,0x0390 },
{ 0x0350,0x0360,0x0390 },
{ 0x0350,0x0360,0x0390 },
{ 0x0340,0x0350,0x0380 },
{ 0x0340,0x0350,0x0380 },
{ 0x0330,0x0340,0x0380 },
{ 0x0320,0x0340,0x0370 },
{ 0x0310,0x0320,0x0360 },
{ 0x0300,0x0310,0x0350 },
{ 0x02f0,0x0300,0x0340 },
{ 0x02f0,0x02f0,0x0330 },
{ 0x02f0,0x02f0,0x0320 },
{ 0x02f0,0x02f0,0x0310 },
{ 0x0300,0x02f0,0x0300 },
{ 0x0310,0x0300,0x02f0 },
{ 0x0340,0x0320,0x02f0 },
{ 0x0390,0x0350,0x02f0 },
{ 0x03e0,0x0390,0x0300 },
{ 0x0420,0x03e0,0x0310 },
{ 0x0460,0x0420,0x0330 },
{ 0x0490,0x0450,0x0350 },
{ 0x04a0,0x04a0,0x03c0 },
{ 0x0460,0x0490,0x0410 },
{ 0x0440,0x0460,0x0470 },
{ 0x0440,0x0440,0x04a0 },
{ 0x0520,0x0480,0x0460 },
{ 0x0800,0x0630,0x0440 },
{ 0x0840,0x0840,0x0450 },
{ 0x0840,0x0840,0x04e0 },
};

const uint8 ff_ac3_bap_tab[64]= {
    0, 1, 1, 1, 1, 1, 2, 2, 3, 3,
    3, 4, 4, 5, 5, 6, 6, 6, 6, 7,
    7, 7, 7, 8, 8, 8, 8, 9, 9, 9,
    9, 10, 10, 10, 10, 11, 11, 11, 11, 12,
    12, 12, 12, 13, 13, 13, 13, 14, 14, 14,
    14, 14, 14, 14, 14, 15, 15, 15, 15, 15,
    15, 15, 15, 15,
};

const uint8 ff_ac3_slow_decay_tab[4]={
    0x0f, 0x11, 0x13, 0x15,
};

const uint8 ff_ac3_fast_decay_tab[4]={
    0x3f, 0x53, 0x67, 0x7b,
};

const uint16 ff_ac3_slow_gain_tab[4]= {
    0x540, 0x4d8, 0x478, 0x410,
};

const uint16 ff_ac3_db_per_bit_tab[4]= {
    0x000, 0x700, 0x900, 0xb00,
};

const uint16 ff_ac3_floor_tab[8]= {
    0x2f0, 0x2b0, 0x270, 0x230, 0x1f0, 0x170, 0x0f0, 0xf800,
};

const uint16 ff_ac3_fast_gain_tab[8]= {
    0x080, 0x100, 0x180, 0x200, 0x280, 0x300, 0x380, 0x400,
};

const uint8 ff_ac3_critical_band_size_tab[50]={
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 3, 3, 3, 3,
    3, 6, 6, 6, 6, 6, 6, 12, 12, 12, 12, 24, 24, 24, 24, 24
};

static inline uint16 bswap_16(uint16 x)
{
    x= (x>>8) | (x<<8);
    return x;
}


typedef struct AC3BitAllocParameters {
    int sr_code;
    int sr_shift;
    int slow_gain, slow_decay, fast_decay, db_per_bit, floor;
    int cpl_fast_leak, cpl_slow_leak;
} AC3BitAllocParameters;

typedef struct AC3EncodeContext {

	Bitstream		pb;			// piseme po bitoch

    int nb_channels;
    int nb_all_channels;
    int lfe_channel;
    int bit_rate;
    unsigned int sample_rate;
    unsigned int bitstream_id;
    unsigned int frame_size_min; /* minimum frame size in case rounding is necessary */
    unsigned int frame_size; /* current frame size in words */
    unsigned int bits_written;
    unsigned int samples_written;
    int sr_shift;
    unsigned int frame_size_code;
    unsigned int sr_code; /* frequency */
    unsigned int channel_mode;
    int lfe;
    unsigned int bitstream_mode;
    short last_samples[AC3_MAX_CHANNELS][256];
    unsigned int chbwcod[AC3_MAX_CHANNELS];
    int nb_coefs[AC3_MAX_CHANNELS];

    /* bitrate allocation control */
    int slow_gain_code, slow_decay_code, fast_decay_code, db_per_bit_code, floor_code;
    AC3BitAllocParameters bit_alloc;
    int coarse_snr_offset;
    int fast_gain_code[AC3_MAX_CHANNELS];
    int fine_snr_offset[AC3_MAX_CHANNELS];
    /* mantissa encoding */
    int mant1_cnt, mant2_cnt, mant4_cnt;


} AC3EncodeContext;


static uint8		band_start_tab[51];
static uint8		bin_to_band_tab[253];
static int16		costab[64];
static int16		sintab[64];
static int16		xcos1[128];
static int16		xsin1[128];

#define MDCT_NBITS 9
#define N         (1 << MDCT_NBITS)

typedef struct IComplex 
{
    short	re,im;
} IComplex;

/* new exponents are sent if their Norm 1 exceed this number */
#define EXP_DIFF_THRESHOLD 1000


static inline int av_clip(int a, int amin, int amax)
{
    if      (a < amin) return amin;
    else if (a > amax) return amax;
    else               return a;
}

static inline int16 fix15(float a)
{
    int v;
    v = (int)(a * (float)(1 << 15));
	if (v < -32767) {	v = -32767;	} else 
	if (v > 32767)  {   v = 32767; }
    return v;
}

static void fft_init(int ln)
{
    int i, n;
    float alpha;

    n = 1 << ln;

    for(i=0;i<(n/2);i++) {
        alpha = 2 * M_PI * (float)i / (float)n;
        costab[i] = fix15(cos(alpha));
        sintab[i] = fix15(sin(alpha));
    }
}

/* butter fly op */
#define BF(pre, pim, qre, qim, pre1, pim1, qre1, qim1) \
{\
  int ax, ay, bx, by;\
  bx=pre1;\
  by=pim1;\
  ax=qre1;\
  ay=qim1;\
  pre = (bx + ax) >> 1;\
  pim = (by + ay) >> 1;\
  qre = (bx - ax) >> 1;\
  qim = (by - ay) >> 1;\
}

#define MUL16(a,b) ((a) * (b))

#define CMUL(pre, pim, are, aim, bre, bim) \
{\
   pre = (MUL16(are, bre) - MUL16(aim, bim)) >> 15;\
   pim = (MUL16(are, bim) + MUL16(bre, aim)) >> 15;\
}



/* do a 2^n point complex fft on 2^ln points. */
static void fft(IComplex *z, int ln)
{
    int			j, l, np, np2;
    int			nblocks, nloops;
    IComplex	*p,*q;
    int			tmp_re, tmp_im;

    np = 1 << ln;

    /* reverse */
    for(j=0;j<np;j++) {
        int k = ff_reverse[j] >> (8 - ln);
        if (k < j)
            FFSWAP(IComplex, z[k], z[j]);
    }

    /* pass 0 */

    p=&z[0];
    j=(np >> 1);
    do {
        BF(p[0].re, p[0].im, p[1].re, p[1].im,
           p[0].re, p[0].im, p[1].re, p[1].im);
        p+=2;
    } while (--j != 0);

    /* pass 1 */

    p=&z[0];
    j=np >> 2;
    do {
        BF(p[0].re, p[0].im, p[2].re, p[2].im,
           p[0].re, p[0].im, p[2].re, p[2].im);
        BF(p[1].re, p[1].im, p[3].re, p[3].im,
           p[1].re, p[1].im, p[3].im, -p[3].re);
        p+=4;
    } while (--j != 0);

    /* pass 2 .. ln-1 */

    nblocks = np >> 3;
    nloops = 1 << 2;
    np2 = np >> 1;
    do {
        p = z;
        q = z + nloops;
        for (j = 0; j < nblocks; ++j) {

            BF(p->re, p->im, q->re, q->im,
               p->re, p->im, q->re, q->im);

            p++;
            q++;
            for(l = nblocks; l < np2; l += nblocks) {
                CMUL(tmp_re, tmp_im, costab[l], -sintab[l], q->re, q->im);
                BF(p->re, p->im, q->re, q->im,
                   p->re, p->im, tmp_re, tmp_im);
                p++;
                q++;
            }
            p += nloops;
            q += nloops;
        }
        nblocks = nblocks >> 1;
        nloops = nloops << 1;
    } while (nblocks != 0);
}

/* do a 512 point mdct */
static void mdct512(int32 *out, int16 *in)
{
    int			i, re, im, re1, im1;
    int16		rot[N];
    IComplex	x[N/4];

    /* shift to simplify computations */
    for(i=0;i<N/4;i++)
        rot[i] = -in[i + 3*N/4];
    for(i=N/4;i<N;i++)
        rot[i] = in[i - N/4];

    /* pre rotation */
    for(i=0;i<N/4;i++) {
        re = ((int)rot[2*i] - (int)rot[N-1-2*i]) >> 1;
        im = -((int)rot[N/2+2*i] - (int)rot[N/2-1-2*i]) >> 1;
        CMUL(x[i].re, x[i].im, re, im, -xcos1[i], xsin1[i]);
    }

    fft(x, MDCT_NBITS - 2);

    /* post rotation */
    for(i=0;i<N/4;i++) {
        re = x[i].re;
        im = x[i].im;
        CMUL(re1, im1, re, im, xsin1[i], xcos1[i]);
        out[2*i] = im1;
        out[N/2-1-2*i] = re1;
    }
}

/* XXX: use another norm ? */
static int calc_exp_diff(uint8 *exp1, uint8 *exp2, int n)
{
    int sum, i;
    sum = 0;
    for(i=0;i<n;i++) {
        sum += abs(exp1[i] - exp2[i]);
    }
    return sum;
}

static void compute_exp_strategy(uint8 exp_strategy[NB_BLOCKS][AC3_MAX_CHANNELS],
                                 uint8 exp[NB_BLOCKS][AC3_MAX_CHANNELS][N/2],
                                 int ch, int is_lfe)
{
    int i, j;
    int exp_diff;

    /* estimate if the exponent variation & decide if they should be
       reused in the next frame */
    exp_strategy[0][ch] = EXP_NEW;
    for(i=1;i<NB_BLOCKS;i++) {
        exp_diff = calc_exp_diff(exp[i][ch], exp[i-1][ch], N/2);
        if (exp_diff > EXP_DIFF_THRESHOLD)
            exp_strategy[i][ch] = EXP_NEW;
        else
            exp_strategy[i][ch] = EXP_REUSE;
    }
    if (is_lfe)
        return;

    /* now select the encoding strategy type : if exponents are often
       recoded, we use a coarse encoding */
    i = 0;
    while (i < NB_BLOCKS) {
        j = i + 1;
        while (j < NB_BLOCKS && exp_strategy[j][ch] == EXP_REUSE)
            j++;
        switch(j - i) {
        case 1:
            exp_strategy[i][ch] = EXP_D45;
            break;
        case 2:
        case 3:
            exp_strategy[i][ch] = EXP_D25;
            break;
        default:
            exp_strategy[i][ch] = EXP_D15;
            break;
        }
        i = j;
    }
}

/* set exp[i] to min(exp[i], exp1[i]) */
static void exponent_min(uint8 exp[N/2], uint8 exp1[N/2], int n)
{
    int i;

    for(i=0;i<n;i++) {
        if (exp1[i] < exp[i])
            exp[i] = exp1[i];
    }
}

/* update the exponents so that they are the ones the decoder will
   decode. Return the number of bits used to code the exponents */
static int encode_exp(uint8 encoded_exp[N/2],
                      uint8 exp[N/2],
                      int nb_exps,
                      int exp_strategy)
{
    int			group_size, nb_groups, i, j, k, exp_min;
    uint8		exp1[N/2];

    switch(exp_strategy) {
    case EXP_D15:
        group_size = 1;
        break;
    case EXP_D25:
        group_size = 2;
        break;
    default:
    case EXP_D45:
        group_size = 4;
        break;
    }
    nb_groups = ((nb_exps + (group_size * 3) - 4) / (3 * group_size)) * 3;

    /* for each group, compute the minimum exponent */
    exp1[0] = exp[0]; /* DC exponent is handled separately */
    k = 1;
    for(i=1;i<=nb_groups;i++) {
        exp_min = exp[k];
        for(j=1;j<group_size;j++) {
            if (exp[k+j] < exp_min)
                exp_min = exp[k+j];
        }
        exp1[i] = exp_min;
        k += group_size;
    }

    /* constraint for DC exponent */
    if (exp1[0] > 15)
        exp1[0] = 15;

    /* Decrease the delta between each groups to within 2
     * so that they can be differentially encoded */
    for (i=1;i<=nb_groups;i++)
        exp1[i] = FFMIN(exp1[i], exp1[i-1] + 2);
    for (i=nb_groups-1;i>=0;i--)
        exp1[i] = FFMIN(exp1[i], exp1[i+1] + 2);

    /* now we have the exponent values the decoder will see */
    encoded_exp[0] = exp1[0];
    k = 1;
    for(i=1;i<=nb_groups;i++) {
        for(j=0;j<group_size;j++) {
            encoded_exp[k+j] = exp1[i];
        }
        k += group_size;
    }

    return 4 + (nb_groups / 3) * 7;
}

/* symetric quantization on 'levels' levels */
static inline int sym_quant(int c, int e, int levels)
{
    int v;

    if (c >= 0) {
        v = (levels * (c << e)) >> 24;
        v = (v + 1) >> 1;
        v = (levels >> 1) + v;
    } else {
        v = (levels * ((-c) << e)) >> 24;
        v = (v + 1) >> 1;
        v = (levels >> 1) - v;
    }
    return v;
}

/* asymetric quantization on 2^qbits levels */
static inline int asym_quant(int c, int e, int qbits)
{
    int lshift, m, v;

    lshift = e + qbits - 24;
    if (lshift >= 0)
        v = c << lshift;
    else
        v = c >> (-lshift);
    /* rounding */
    v = (v + 1) >> 1;
    m = (1 << (qbits-1));
    if (v >= m)
        v = m - 1;
    return v & ((1 << qbits)-1);
}

#define CRC16_POLY ((1 << 0) | (1 << 2) | (1 << 15) | (1 << 16))

static unsigned int mul_poly(unsigned int a, unsigned int b, unsigned int poly)
{
    unsigned int c;

    c = 0;
    while (a) {
        if (a & 1)
            c ^= b;
        a = a >> 1;
        b = b << 1;
        if (b & (1 << 16))
            b ^= poly;
    }
    return c;
}

static unsigned int pow_poly(unsigned int a, unsigned int n, unsigned int poly)
{
    unsigned int r;
    r = 1;
    while (n) {
        if (n & 1)
            r = mul_poly(r, a, poly);
        a = mul_poly(a, a, poly);
        n >>= 1;
    }
    return r;
}

static inline int av_log2(unsigned int v)
{
    int n = 0;
    if (v & 0xffff0000) {
        v >>= 16;
        n += 16;
    }
    if (v & 0xff00) {
        v >>= 8;
        n += 8;
    }
    n += ff_log2_tab[v];

    return n;
}

/* compute log2(max(abs(tab[]))) */
static int log2_tab(int16 *tab, int n)
{
    int i, v;

    v = 0;
    for(i=0;i<n;i++) {
        v |= abs(tab[i]);
    }
    return av_log2(v);
}

static void lshift_tab(int16 *tab, int n, int lshift)
{
    int i;

    if (lshift > 0) {
        for(i=0;i<n;i++) {
            tab[i] <<= lshift;
        }
    } else if (lshift < 0) {
        lshift = -lshift;
        for(i=0;i<n;i++) {
            tab[i] >>= lshift;
        }
    }
}

static inline int calc_lowcomp1(int a, int b0, int b1, int c)
{
    if ((b0 + 256) == b1) {
        a = c;
    } else if (b0 > b1) {
        a = FFMAX(a - 64, 0);
    }
    return a;
}

static inline int calc_lowcomp(int a, int b0, int b1, int bin)
{
    if (bin < 7) {
        return calc_lowcomp1(a, b0, b1, 384);
    } else if (bin < 20) {
        return calc_lowcomp1(a, b0, b1, 320);
    } else {
        return FFMAX(a - 128, 0);
    }
}


/* return the size in bits taken by the mantissa */
static int compute_mantissa_size(AC3EncodeContext *s, uint8 *m, int nb_coefs)
{
    int bits, mant, i;

    bits = 0;
    for(i=0;i<nb_coefs;i++) {
        mant = m[i];
        switch(mant) {
        case 0:
            /* nothing */
            break;
        case 1:
            /* 3 mantissa in 5 bits */
            if (s->mant1_cnt == 0)
                bits += 5;
            if (++s->mant1_cnt == 3)
                s->mant1_cnt = 0;
            break;
        case 2:
            /* 3 mantissa in 7 bits */
            if (s->mant2_cnt == 0)
                bits += 7;
            if (++s->mant2_cnt == 3)
                s->mant2_cnt = 0;
            break;
        case 3:
            bits += 3;
            break;
        case 4:
            /* 2 mantissa in 7 bits */
            if (s->mant4_cnt == 0)
                bits += 7;
            if (++s->mant4_cnt == 2)
                s->mant4_cnt = 0;
            break;
        case 14:
            bits += 14;
            break;
        case 15:
            bits += 16;
            break;
        default:
            bits += mant - 1;
            break;
        }
    }
    return bits;
}

void ff_ac3_bit_alloc_calc_psd(uint8 *exp, int start, int end, int16 *psd, int16 *band_psd)
{
    int bin, i, j, k, end1, v;

    /* exponent mapping to PSD */
    for(bin=start;bin<end;bin++) {
        psd[bin]=(3072 - (exp[bin] << 7));
    }

    /* PSD integration */
    j=start;
    k=bin_to_band_tab[start];
    do {
        v=psd[j];
        j++;
        end1 = FFMIN(band_start_tab[k+1], end);
        for(i=j;i<end1;i++) {
            /* logadd */
            int adr = FFMIN(FFABS(v - psd[j]) >> 1, 255);
            v = FFMAX(v, psd[j]) + ff_ac3_log_add_tab[adr];
            j++;
        }
        band_psd[k]=v;
        k++;
    } while (end > band_start_tab[k]);
}

void ff_ac3_bit_alloc_calc_mask(AC3BitAllocParameters *s, int16 *band_psd,
                                int start, int end, int fast_gain, int is_lfe,
                                int dba_mode, int dba_nsegs, uint8 *dba_offsets,
                                uint8 *dba_lengths, uint8 *dba_values,
                                int16 *mask)
{
    int16 excite[50]; /* excitation */
    int bin, k;
    int bndstrt, bndend, begin, end1, tmp;
    int lowcomp, fastleak, slowleak;

    /* excitation function */
    bndstrt = bin_to_band_tab[start];
    bndend = bin_to_band_tab[end-1] + 1;

    if (bndstrt == 0) {
        lowcomp = 0;
        lowcomp = calc_lowcomp1(lowcomp, band_psd[0], band_psd[1], 384);
        excite[0] = band_psd[0] - fast_gain - lowcomp;
        lowcomp = calc_lowcomp1(lowcomp, band_psd[1], band_psd[2], 384);
        excite[1] = band_psd[1] - fast_gain - lowcomp;
        begin = 7;
        for (bin = 2; bin < 7; bin++) {
            if (!(is_lfe && bin == 6))
                lowcomp = calc_lowcomp1(lowcomp, band_psd[bin], band_psd[bin+1], 384);
            fastleak = band_psd[bin] - fast_gain;
            slowleak = band_psd[bin] - s->slow_gain;
            excite[bin] = fastleak - lowcomp;
            if (!(is_lfe && bin == 6)) {
                if (band_psd[bin] <= band_psd[bin+1]) {
                    begin = bin + 1;
                    break;
                }
            }
        }

        end1=bndend;
        if (end1 > 22) end1=22;

        for (bin = begin; bin < end1; bin++) {
            if (!(is_lfe && bin == 6))
                lowcomp = calc_lowcomp(lowcomp, band_psd[bin], band_psd[bin+1], bin);

            fastleak = FFMAX(fastleak - s->fast_decay, band_psd[bin] - fast_gain);
            slowleak = FFMAX(slowleak - s->slow_decay, band_psd[bin] - s->slow_gain);
            excite[bin] = FFMAX(fastleak - lowcomp, slowleak);
        }
        begin = 22;
    } else {
        /* coupling channel */
        begin = bndstrt;

        fastleak = (s->cpl_fast_leak << 8) + 768;
        slowleak = (s->cpl_slow_leak << 8) + 768;
    }

    for (bin = begin; bin < bndend; bin++) {
        fastleak = FFMAX(fastleak - s->fast_decay, band_psd[bin] - fast_gain);
        slowleak = FFMAX(slowleak - s->slow_decay, band_psd[bin] - s->slow_gain);
        excite[bin] = FFMAX(fastleak, slowleak);
    }

    /* compute masking curve */

    for (bin = bndstrt; bin < bndend; bin++) {
        tmp = s->db_per_bit - band_psd[bin];
        if (tmp > 0) {
            excite[bin] += tmp >> 2;
        }
        mask[bin] = FFMAX(ff_ac3_hearing_threshold_tab[bin >> s->sr_shift][s->sr_code], excite[bin]);
    }

    /* delta bit allocation */

    if (dba_mode == DBA_REUSE || dba_mode == DBA_NEW) {
        int band, seg, delta;
        band = 0;
        for (seg = 0; seg < FFMIN(8, dba_nsegs); seg++) {
            band = FFMIN(49, band + dba_offsets[seg]);
            if (dba_values[seg] >= 4) {
                delta = (dba_values[seg] - 3) << 7;
            } else {
                delta = (dba_values[seg] - 4) << 7;
            }
            for (k = 0; k < dba_lengths[seg]; k++) {
                mask[band] += delta;
                band++;
            }
        }
    }
}

void ff_ac3_bit_alloc_calc_bap(int16 *mask, int16 *psd, int start, int end,
                               int snr_offset, int floor,
                               const uint8 *bap_tab, uint8 *bap)
{
    int i, j, k, end1, v, address;

    /* special case, if snr offset is -960, set all bap's to zero */
    if(snr_offset == -960) {
        memset(bap, 0, 256);
        return;
    }

    i = start;
    j = bin_to_band_tab[start];
    do {
        v = (FFMAX(mask[j] - snr_offset - floor, 0) & 0x1FE0) + floor;
        end1 = FFMIN(band_start_tab[j] + ff_ac3_critical_band_size_tab[j], end);
        for (k = i; k < end1; k++) {
            address = av_clip((psd[i] - v) >> 5, 0, 63);
            bap[i] = bap_tab[address];
            i++;
        }
    } while (end > band_start_tab[j++]);
}

/* AC-3 bit allocation. The algorithm is the one described in the AC-3
   spec. */
void ac3_parametric_bit_allocation(AC3BitAllocParameters *s, uint8 *bap,
                                   uint8 *exp, int start, int end,
                                   int snr_offset, int fast_gain, int is_lfe,
                                   int dba_mode, int dba_nsegs,
                                   uint8 *dba_offsets, uint8 *dba_lengths,
                                   uint8 *dba_values)
{
    int16 psd[256];   /* scaled exponents */
    int16 band_psd[50]; /* interpolated exponents */
    int16 mask[50];   /* masking value */

    ff_ac3_bit_alloc_calc_psd(exp, start, end, psd, band_psd);

    ff_ac3_bit_alloc_calc_mask(s, band_psd, start, end, fast_gain, is_lfe,
                               dba_mode, dba_nsegs, dba_offsets, dba_lengths, dba_values,
                               mask);

    ff_ac3_bit_alloc_calc_bap(mask, psd, start, end, snr_offset, s->floor,
                              ff_ac3_bap_tab, bap);
}

/**
 * Initializes some tables.
 * note: This function must remain thread safe because it is called by the
 *       AVParser init code.
 */
void ac3_common_init(void)
{
    int i, j, k, l, v;
    /* compute bndtab and masktab from bandsz */
    k = 0;
    l = 0;
    for(i=0;i<50;i++) {
        band_start_tab[i] = l;
        v = ff_ac3_critical_band_size_tab[i];
        for(j=0;j<v;j++) bin_to_band_tab[k++]=i;
        l += v;
    }
    band_start_tab[50] = l;
}


static void bit_alloc_masking(AC3EncodeContext *s,
                              uint8 encoded_exp[NB_BLOCKS][AC3_MAX_CHANNELS][N/2],
                              uint8 exp_strategy[NB_BLOCKS][AC3_MAX_CHANNELS],
                              int16 psd[NB_BLOCKS][AC3_MAX_CHANNELS][N/2],
                              int16 mask[NB_BLOCKS][AC3_MAX_CHANNELS][50])
{
    int blk, ch;
    int16 band_psd[NB_BLOCKS][AC3_MAX_CHANNELS][50];

    for(blk=0; blk<NB_BLOCKS; blk++) {
        for(ch=0;ch<s->nb_all_channels;ch++) {
            if(exp_strategy[blk][ch] == EXP_REUSE) {
                memcpy(psd[blk][ch], psd[blk-1][ch], (N/2)*sizeof(int16));
                memcpy(mask[blk][ch], mask[blk-1][ch], 50*sizeof(int16));
            } else {
                ff_ac3_bit_alloc_calc_psd(encoded_exp[blk][ch], 0,
                                          s->nb_coefs[ch],
                                          psd[blk][ch], band_psd[blk][ch]);
                ff_ac3_bit_alloc_calc_mask(&s->bit_alloc, band_psd[blk][ch],
                                           0, s->nb_coefs[ch],
                                           ff_ac3_fast_gain_tab[s->fast_gain_code[ch]],
                                           ch == s->lfe_channel,
                                           DBA_NONE, 0, NULL, NULL, NULL,
                                           mask[blk][ch]);
            }
        }
    }
}

static int bit_alloc(AC3EncodeContext *s,
                     int16 mask[NB_BLOCKS][AC3_MAX_CHANNELS][50],
                     int16 psd[NB_BLOCKS][AC3_MAX_CHANNELS][N/2],
                     uint8 bap[NB_BLOCKS][AC3_MAX_CHANNELS][N/2],
                     int frame_bits, int coarse_snr_offset, int fine_snr_offset)
{
    int i, ch;
    int snr_offset;

    snr_offset = (((coarse_snr_offset - 15) << 4) + fine_snr_offset) << 2;

    /* compute size */
    for(i=0;i<NB_BLOCKS;i++) {
        s->mant1_cnt = 0;
        s->mant2_cnt = 0;
        s->mant4_cnt = 0;
        for(ch=0;ch<s->nb_all_channels;ch++) {
            ff_ac3_bit_alloc_calc_bap(mask[i][ch], psd[i][ch], 0,
                                      s->nb_coefs[ch], snr_offset,
                                      s->bit_alloc.floor, ff_ac3_bap_tab,
                                      bap[i][ch]);
            frame_bits += compute_mantissa_size(s, bap[i][ch],
                                                 s->nb_coefs[ch]);
        }
    }
    return 16 * s->frame_size - frame_bits;
}

#define SNR_INC1 4

static int compute_bit_allocation(AC3EncodeContext *s,
                                  uint8 bap[NB_BLOCKS][AC3_MAX_CHANNELS][N/2],
                                  uint8 encoded_exp[NB_BLOCKS][AC3_MAX_CHANNELS][N/2],
                                  uint8 exp_strategy[NB_BLOCKS][AC3_MAX_CHANNELS],
                                  int frame_bits)
{
    int i, ch;
    int coarse_snr_offset, fine_snr_offset;
    uint8 bap1[NB_BLOCKS][AC3_MAX_CHANNELS][N/2];
    int16 psd[NB_BLOCKS][AC3_MAX_CHANNELS][N/2];
    int16 mask[NB_BLOCKS][AC3_MAX_CHANNELS][50];
    static const int frame_bits_inc[8] = { 0, 0, 2, 2, 2, 4, 2, 4 };

    /* init default parameters */
    s->slow_decay_code = 2;
    s->fast_decay_code = 1;
    s->slow_gain_code = 1;
    s->db_per_bit_code = 2;
    s->floor_code = 4;
    for(ch=0;ch<s->nb_all_channels;ch++)
        s->fast_gain_code[ch] = 4;

    /* compute real values */
    s->bit_alloc.sr_code = s->sr_code;
    s->bit_alloc.sr_shift = s->sr_shift;
    s->bit_alloc.slow_decay = ff_ac3_slow_decay_tab[s->slow_decay_code] >> s->sr_shift;
    s->bit_alloc.fast_decay = ff_ac3_fast_decay_tab[s->fast_decay_code] >> s->sr_shift;
    s->bit_alloc.slow_gain = ff_ac3_slow_gain_tab[s->slow_gain_code];
    s->bit_alloc.db_per_bit = ff_ac3_db_per_bit_tab[s->db_per_bit_code];
    s->bit_alloc.floor = ff_ac3_floor_tab[s->floor_code];

    /* header size */
    frame_bits += 65;
    // if (s->channel_mode == 2)
    //    frame_bits += 2;
    frame_bits += frame_bits_inc[s->channel_mode];

    /* audio blocks */
    for(i=0;i<NB_BLOCKS;i++) {
        frame_bits += s->nb_channels * 2 + 2; /* blksw * c, dithflag * c, dynrnge, cplstre */
        if (s->channel_mode == AC3_CHMODE_STEREO) {
            frame_bits++; /* rematstr */
            if(i==0) frame_bits += 4;
        }
        frame_bits += 2 * s->nb_channels; /* chexpstr[2] * c */
        if (s->lfe)
            frame_bits++; /* lfeexpstr */
        for(ch=0;ch<s->nb_channels;ch++) {
            if (exp_strategy[i][ch] != EXP_REUSE)
                frame_bits += 6 + 2; /* chbwcod[6], gainrng[2] */
        }
        frame_bits++; /* baie */
        frame_bits++; /* snr */
        frame_bits += 2; /* delta / skip */
    }
    frame_bits++; /* cplinu for block 0 */
    /* bit alloc info */
    /* sdcycod[2], fdcycod[2], sgaincod[2], dbpbcod[2], floorcod[3] */
    /* csnroffset[6] */
    /* (fsnoffset[4] + fgaincod[4]) * c */
    frame_bits += 2*4 + 3 + 6 + s->nb_all_channels * (4 + 3);

    /* auxdatae, crcrsv */
    frame_bits += 2;

    /* CRC */
    frame_bits += 16;

    /* calculate psd and masking curve before doing bit allocation */
    bit_alloc_masking(s, encoded_exp, exp_strategy, psd, mask);

    /* now the big work begins : do the bit allocation. Modify the snr
       offset until we can pack everything in the requested frame size */

    coarse_snr_offset = s->coarse_snr_offset;
    while (coarse_snr_offset >= 0 &&
           bit_alloc(s, mask, psd, bap, frame_bits, coarse_snr_offset, 0) < 0)
        coarse_snr_offset -= SNR_INC1;
    if (coarse_snr_offset < 0) {
        return -1;
    }
    while ((coarse_snr_offset + SNR_INC1) <= 63 &&
           bit_alloc(s, mask, psd, bap1, frame_bits,
                     coarse_snr_offset + SNR_INC1, 0) >= 0) {
        coarse_snr_offset += SNR_INC1;
        memcpy(bap, bap1, sizeof(bap1));
    }
    while ((coarse_snr_offset + 1) <= 63 &&
           bit_alloc(s, mask, psd, bap1, frame_bits, coarse_snr_offset + 1, 0) >= 0) {
        coarse_snr_offset++;
        memcpy(bap, bap1, sizeof(bap1));
    }

    fine_snr_offset = 0;
    while ((fine_snr_offset + SNR_INC1) <= 15 &&
           bit_alloc(s, mask, psd, bap1, frame_bits,
                     coarse_snr_offset, fine_snr_offset + SNR_INC1) >= 0) {
        fine_snr_offset += SNR_INC1;
        memcpy(bap, bap1, sizeof(bap1));
    }
    while ((fine_snr_offset + 1) <= 15 &&
           bit_alloc(s, mask, psd, bap1, frame_bits,
                     coarse_snr_offset, fine_snr_offset + 1) >= 0) {
        fine_snr_offset++;
        memcpy(bap, bap1, sizeof(bap1));
    }

    s->coarse_snr_offset = coarse_snr_offset;
    for(ch=0;ch<s->nb_all_channels;ch++)
        s->fine_snr_offset[ch] = fine_snr_offset;
    return 0;
}

static int AC3_encode_init(AC3CodecContext *avctx)
{
    int freq = avctx->sample_rate;
    int bitrate = avctx->bit_rate;
    int channels = avctx->channels;
    AC3EncodeContext *s = (AC3EncodeContext*)avctx->priv_data;
    int i, j, ch;
    float alpha;
    int bw_code;
    static const uint8 channel_mode_defs[6] = {
        0x01, /* C */
        0x02, /* L R */
        0x03, /* L C R */
        0x06, /* L R SL SR */
        0x07, /* L C R SL SR */
        0x07, /* L C R SL SR (+LFE) */
    };

    avctx->frame_size = AC3_FRAME_SIZE;

    ac3_common_init();

    /* number of channels */
    if (channels < 1 || channels > 6)
        return -1;
    s->channel_mode = channel_mode_defs[channels - 1];
    s->lfe = (channels == 6) ? 1 : 0;
    s->nb_all_channels = channels;
    s->nb_channels = channels > 5 ? 5 : channels;
    s->lfe_channel = s->lfe ? 5 : -1;

    /* frequency */
    for(i=0;i<3;i++) {
        for(j=0;j<3;j++)
            if ((ff_ac3_sample_rate_tab[j] >> i) == freq)
                goto found;
    }
    return -1;
 found:
    s->sample_rate = freq;
    s->sr_shift = i;
    s->sr_code = j;
    s->bitstream_id = 8 + s->sr_shift;
    s->bitstream_mode = 0; /* complete main audio service */

    /* bitrate & frame size */
    for(i=0;i<19;i++) {
        if ((ff_ac3_bitrate_tab[i] >> s->sr_shift)*1000 == bitrate)
            break;
    }
    if (i == 19)
        return -1;
    s->bit_rate = bitrate;
    s->frame_size_code = i << 1;
    s->frame_size_min = ff_ac3_frame_size_tab[s->frame_size_code][s->sr_code];
    s->bits_written = 0;
    s->samples_written = 0;
    s->frame_size = s->frame_size_min;

    /* bit allocation init */
    if(avctx->cutoff) {
        /* calculate bandwidth based on user-specified cutoff frequency */
        int cutoff = av_clip(avctx->cutoff, 1, s->sample_rate >> 1);
        int fbw_coeffs = cutoff * 512 / s->sample_rate;
        bw_code = av_clip((fbw_coeffs - 73) / 3, 0, 60);
    } else {
        /* use default bandwidth setting */
        /* XXX: should compute the bandwidth according to the frame
           size, so that we avoid annoying high frequency artifacts */
        bw_code = 50;
    }
    for(ch=0;ch<s->nb_channels;ch++) {
        /* bandwidth for each channel */
        s->chbwcod[ch] = bw_code;
        s->nb_coefs[ch] = bw_code * 3 + 73;
    }
    if (s->lfe) {
        s->nb_coefs[s->lfe_channel] = 7; /* fixed */
    }
    /* initial snr offset */
    s->coarse_snr_offset = 40;

    /* mdct init */
    fft_init(MDCT_NBITS - 2);
    for(i=0;i<N/4;i++) {
        alpha = 2 * M_PI * (i + 1.0 / 8.0) / (float)N;
        xcos1[i] = fix15(-cos(alpha));
        xsin1[i] = fix15(-sin(alpha));
    }

    return 0;
}

/* output the AC-3 frame header */
static void output_frame_header(AC3EncodeContext *s, unsigned char *frame)
{
	// TODO: nieco rozumne
	s->pb.Init(frame);
	s->pb.start = frame;

	s->pb.PutBits(0x0b77, 16);		// frame header
	s->pb.PutBits(0, 16);			// crc1: will be filled later
	s->pb.PutBits(s->sr_code, 2);
	s->pb.PutBits(s->frame_size_code + (s->frame_size - s->frame_size_min) , 6);
	s->pb.PutBits(s->bitstream_id, 5);
	s->pb.PutBits(s->bitstream_mode, 3);
	s->pb.PutBits(s->channel_mode, 3);
    if ((s->channel_mode & 0x01) && s->channel_mode != AC3_CHMODE_MONO)
		s->pb.PutBits(1, 2);	/* XXX -4.5 dB */
    if (s->channel_mode & 0x04)
		s->pb.PutBits(1, 2);	/* XXX -6 dB */
    if (s->channel_mode == AC3_CHMODE_STEREO)
		s->pb.PutBits(0, 2);	/* surround not indicated */
	s->pb.PutBits(s->lfe, 1);	// LFE
	s->pb.PutBits(31, 5);		// dialog norm: -31 db
	s->pb.PutBits(0, 1);		// no compression control word
	s->pb.PutBits(0, 1);		// no lang code
	s->pb.PutBits(0, 1);		// no audio production info
	s->pb.PutBits(0, 1);		// no copyright
	s->pb.PutBits(1, 1);		// original bitstream
	s->pb.PutBits(0, 1);		// no time code 1
	s->pb.PutBits(0, 1);		// no time code 2
	s->pb.PutBits(0, 1);		// no additional bit stream info
}

/* Output one audio block. There are NB_BLOCKS audio blocks in one AC-3
   frame */
static void output_audio_block(AC3EncodeContext *s,
                               uint8 exp_strategy[AC3_MAX_CHANNELS],
                               uint8 encoded_exp[AC3_MAX_CHANNELS][N/2],
                               uint8 bap[AC3_MAX_CHANNELS][N/2],
                               int32 mdct_coefs[AC3_MAX_CHANNELS][N/2],
                               int8 global_exp[AC3_MAX_CHANNELS],
                               int block_num)
{
    int			ch, nb_groups, group_size, i, baie, rbnd;
    uint8		*p;
    uint16		qmant[AC3_MAX_CHANNELS][N/2];
    int			exp0, exp1;
    int			mant1_cnt, mant2_cnt, mant4_cnt;
    uint16		*qmant1_ptr, *qmant2_ptr, *qmant4_ptr;
    int			delta0, delta1, delta2;

    for(ch=0;ch<s->nb_channels;ch++)
		s->pb.PutBits(0, 1); /* 512 point MDCT */
    for(ch=0;ch<s->nb_channels;ch++)
		s->pb.PutBits(1, 1); /* no dither */
	s->pb.PutBits(0, 1);	 /* no dynamic range */
    if (block_num == 0) {
        /* for block 0, even if no coupling, we must say it. This is a
           waste of bit :-) */
		s->pb.PutBits(1, 1);	/* coupling strategy present */
        s->pb.PutBits(0, 1);	/* no coupling strategy */
    } else {
		s->pb.PutBits(0, 1);	/* no new coupling strategy */
    }

    if (s->channel_mode == AC3_CHMODE_STEREO)
      {
        if(block_num==0)
          {
            /* first block must define rematrixing (rematstr)  */
		    s->pb.PutBits(1, 1);

            /* dummy rematrixing rematflg(1:4)=0 */
            for (rbnd=0;rbnd<4;rbnd++)
			  s->pb.PutBits(0, 1);
          }
        else
          {
            /* no matrixing (but should be used in the future) */
			s->pb.PutBits(0, 1);
          }
      }

    /* exponent strategy */
    for(ch=0;ch<s->nb_channels;ch++) {
		s->pb.PutBits(exp_strategy[ch], 2);
    }

    if (s->lfe) {
		s->pb.PutBits(exp_strategy[s->lfe_channel], 1);
    }

    for(ch=0;ch<s->nb_channels;ch++) {
        if (exp_strategy[ch] != EXP_REUSE)
			s->pb.PutBits(s->chbwcod[ch], 6);
    }

    /* exponents */
    for (ch = 0; ch < s->nb_all_channels; ch++) {
        switch(exp_strategy[ch]) {
        case EXP_REUSE:
            continue;
        case EXP_D15:
            group_size = 1;
            break;
        case EXP_D25:
            group_size = 2;
            break;
        default:
        case EXP_D45:
            group_size = 4;
            break;
        }
        nb_groups = (s->nb_coefs[ch] + (group_size * 3) - 4) / (3 * group_size);
        p = encoded_exp[ch];

        /* first exponent */
        exp1 = *p++;
		s->pb.PutBits(exp1, 4);

        /* next ones are delta encoded */
        for(i=0;i<nb_groups;i++) {
            /* merge three delta in one code */
            exp0 = exp1;
            exp1 = p[0];
            p += group_size;
            delta0 = exp1 - exp0 + 2;

            exp0 = exp1;
            exp1 = p[0];
            p += group_size;
            delta1 = exp1 - exp0 + 2;

            exp0 = exp1;
            exp1 = p[0];
            p += group_size;
            delta2 = exp1 - exp0 + 2;

			s->pb.PutBits(((delta0 * 5 + delta1) * 5) + delta2, 7);
        }

        if (ch != s->lfe_channel)
			s->pb.PutBits(0, 2);	/* no gain range info */
    }

    /* bit allocation info */
    baie = (block_num == 0);
	s->pb.PutBits(baie, 1);
    if (baie) {
		s->pb.PutBits(s->slow_decay_code, 2);
		s->pb.PutBits(s->fast_decay_code, 2);
		s->pb.PutBits(s->slow_gain_code, 2);
		s->pb.PutBits(s->db_per_bit_code, 2);
		s->pb.PutBits(s->floor_code, 3);
    }

    /* snr offset */
	s->pb.PutBits(baie, 1);	/* always present with bai */
    if (baie) {
		s->pb.PutBits(s->coarse_snr_offset, 6);
        for(ch=0;ch<s->nb_all_channels;ch++) {
			s->pb.PutBits(s->fine_snr_offset[ch], 4);
			s->pb.PutBits(s->fast_gain_code[ch], 3);
        }
    }

	s->pb.PutBits(0, 1);	/* no delta bit allocation */
	s->pb.PutBits(0, 1);	/* no data to skip */

    /* mantissa encoding : we use two passes to handle the grouping. A
       one pass method may be faster, but it would necessitate to
       modify the output stream. */

    /* first pass: quantize */
    mant1_cnt = mant2_cnt = mant4_cnt = 0;
    qmant1_ptr = qmant2_ptr = qmant4_ptr = NULL;

    for (ch = 0; ch < s->nb_all_channels; ch++) {
        int b, c, e, v;

        for(i=0;i<s->nb_coefs[ch];i++) {
            c = mdct_coefs[ch][i];
            e = encoded_exp[ch][i] - global_exp[ch];
            b = bap[ch][i];
            switch(b) {
            case 0:
                v = 0;
                break;
            case 1:
                v = sym_quant(c, e, 3);
                switch(mant1_cnt) {
                case 0:
                    qmant1_ptr = &qmant[ch][i];
                    v = 9 * v;
                    mant1_cnt = 1;
                    break;
                case 1:
                    *qmant1_ptr += 3 * v;
                    mant1_cnt = 2;
                    v = 128;
                    break;
                default:
                    *qmant1_ptr += v;
                    mant1_cnt = 0;
                    v = 128;
                    break;
                }
                break;
            case 2:
                v = sym_quant(c, e, 5);
                switch(mant2_cnt) {
                case 0:
                    qmant2_ptr = &qmant[ch][i];
                    v = 25 * v;
                    mant2_cnt = 1;
                    break;
                case 1:
                    *qmant2_ptr += 5 * v;
                    mant2_cnt = 2;
                    v = 128;
                    break;
                default:
                    *qmant2_ptr += v;
                    mant2_cnt = 0;
                    v = 128;
                    break;
                }
                break;
            case 3:
                v = sym_quant(c, e, 7);
                break;
            case 4:
                v = sym_quant(c, e, 11);
                switch(mant4_cnt) {
                case 0:
                    qmant4_ptr = &qmant[ch][i];
                    v = 11 * v;
                    mant4_cnt = 1;
                    break;
                default:
                    *qmant4_ptr += v;
                    mant4_cnt = 0;
                    v = 128;
                    break;
                }
                break;
            case 5:
                v = sym_quant(c, e, 15);
                break;
            case 14:
                v = asym_quant(c, e, 14);
                break;
            case 15:
                v = asym_quant(c, e, 16);
                break;
            default:
                v = asym_quant(c, e, b - 1);
                break;
            }
            qmant[ch][i] = v;
        }
    }

    /* second pass : output the values */
    for (ch = 0; ch < s->nb_all_channels; ch++) {
        int b, q;

        for(i=0;i<s->nb_coefs[ch];i++) {
            q = qmant[ch][i];
            b = bap[ch][i];
            switch(b) {
            case 0:
                break;
            case 1:
                if (q != 128)
					s->pb.PutBits(q, 5);
                break;
            case 2:
                if (q != 128)
					s->pb.PutBits(q, 7);
                break;
            case 3:
				s->pb.PutBits(q, 3);
                break;
            case 4:
                if (q != 128)
					s->pb.PutBits(q, 7);
                break;
            case 14:
				s->pb.PutBits(q, 14);
                break;
            case 15:
				s->pb.PutBits(q, 16);
                break;
            default:
				s->pb.PutBits(q, b-1);
                break;
            }
        }
    }
}


/* fill the end of the frame and compute the two crcs */
static int output_frame_end(AC3EncodeContext *s)
{
    int		frame_size, frame_size_58, n, crc1, crc2, crc_inv;
    uint8	*frame;

    frame_size = s->frame_size; /* frame size in words */
    
	/* align to 8 bits */
	s->pb.Put_ByteAlign_Zero();
	s->pb.WriteBits();

    /* add zero bytes to reach the frame size */
	int	written = s->pb.buf - s->pb.start;
    frame = s->pb.start;
    n = 2 * s->frame_size - (written) - 2;
    if(n>0)
      memset(s->pb.buf, 0, n);

    /* Now we must compute both crcs : this is not so easy for crc1
       because it is at the beginning of the data... */
    frame_size_58 = (frame_size >> 1) + (frame_size >> 3);
    crc1 = bswap_16(av_crc(av_crc_get_table(AV_CRC_16_ANSI), 0, frame + 4, 2 * frame_size_58 - 4));

    /* XXX: could precompute crc_inv */
    crc_inv = pow_poly((CRC16_POLY >> 1), (16 * frame_size_58) - 16, CRC16_POLY);
    crc1 = mul_poly(crc_inv, crc1, CRC16_POLY);
	frame[2] = (crc1 >> 8) & 0xff;
	frame[3] = (crc1 >> 0) & 0xff;

    crc2 = bswap_16(av_crc(av_crc_get_table(AV_CRC_16_ANSI), 0, frame + 2 * frame_size_58, (frame_size - frame_size_58) * 2 - 2));

	frame[2*frame_size-2+0] = (crc2 >> 8) & 0xff;
	frame[2*frame_size-2+1] = (crc2 >> 0) & 0xff;

    return frame_size * 2;
}

static int AC3_encode_frame(AC3CodecContext *avctx,
                            unsigned char *frame, int buf_size, void *data)
{
    AC3EncodeContext *s = (AC3EncodeContext*)avctx->priv_data;
    int16 *samples = (int16*)data;
    int i, j, k, v, ch;
    int16 input_samples[N];
    int32 mdct_coef[NB_BLOCKS][AC3_MAX_CHANNELS][N/2];
    uint8 exp[NB_BLOCKS][AC3_MAX_CHANNELS][N/2];
    uint8 exp_strategy[NB_BLOCKS][AC3_MAX_CHANNELS];
    uint8 encoded_exp[NB_BLOCKS][AC3_MAX_CHANNELS][N/2];
    uint8 bap[NB_BLOCKS][AC3_MAX_CHANNELS][N/2];
    int8 exp_samples[NB_BLOCKS][AC3_MAX_CHANNELS];
    int frame_bits;

    frame_bits = 0;
    for(ch=0;ch<s->nb_all_channels;ch++) {
        /* fixed mdct to the six sub blocks & exponent computation */
        for(i=0;i<NB_BLOCKS;i++) {
            int16 *sptr;
            int sinc;

            /* compute input samples */
            memcpy(input_samples, s->last_samples[ch], N/2 * sizeof(int16));
            sinc = s->nb_all_channels;
            sptr = samples + (sinc * (N/2) * i) + ch;
            for(j=0;j<N/2;j++) {
                v = *sptr;
                input_samples[j + N/2] = v;
                s->last_samples[ch][j] = v;
                sptr += sinc;
            }

            /* apply the MDCT window */
            for(j=0;j<N/2;j++) {
                input_samples[j] = MUL16(input_samples[j],
                                         ff_ac3_window[j]) >> 15;
                input_samples[N-j-1] = MUL16(input_samples[N-j-1],
                                             ff_ac3_window[j]) >> 15;
            }

            /* Normalize the samples to use the maximum available
               precision */
            v = 14 - log2_tab(input_samples, N);
            if (v < 0)
                v = 0;
            exp_samples[i][ch] = v - 9;
            lshift_tab(input_samples, N, v);

            /* do the MDCT */
            mdct512(mdct_coef[i][ch], input_samples);

            /* compute "exponents". We take into account the
               normalization there */
            for(j=0;j<N/2;j++) {
                int e;
                v = abs(mdct_coef[i][ch][j]);
                if (v == 0)
                    e = 24;
                else {
                    e = 23 - av_log2(v) + exp_samples[i][ch];
                    if (e >= 24) {
                        e = 24;
                        mdct_coef[i][ch][j] = 0;
                    }
                }
                exp[i][ch][j] = e;
            }
        }

        compute_exp_strategy(exp_strategy, exp, ch, ch == s->lfe_channel);

        /* compute the exponents as the decoder will see them. The
           EXP_REUSE case must be handled carefully : we select the
           min of the exponents */
        i = 0;
        while (i < NB_BLOCKS) {
            j = i + 1;
            while (j < NB_BLOCKS && exp_strategy[j][ch] == EXP_REUSE) {
                exponent_min(exp[i][ch], exp[j][ch], s->nb_coefs[ch]);
                j++;
            }
            frame_bits += encode_exp(encoded_exp[i][ch],
                                     exp[i][ch], s->nb_coefs[ch],
                                     exp_strategy[i][ch]);
            /* copy encoded exponents for reuse case */
            for(k=i+1;k<j;k++) {
                memcpy(encoded_exp[k][ch], encoded_exp[i][ch],
                       s->nb_coefs[ch] * sizeof(uint8));
            }
            i = j;
        }
    }

    /* adjust for fractional frame sizes */
    while(s->bits_written >= s->bit_rate && s->samples_written >= s->sample_rate) {
        s->bits_written -= s->bit_rate;
        s->samples_written -= s->sample_rate;
    }
    s->frame_size = s->frame_size_min + (s->bits_written * s->sample_rate < s->samples_written * s->bit_rate);
    s->bits_written += s->frame_size * 16;
    s->samples_written += AC3_FRAME_SIZE;

    compute_bit_allocation(s, bap, encoded_exp, exp_strategy, frame_bits);
    /* everything is known... let's output the frame */
    output_frame_header(s, frame);

    for(i=0;i<NB_BLOCKS;i++) {
        output_audio_block(s, exp_strategy[i], encoded_exp[i],
                           bap[i], mdct_coef[i], exp_samples[i], i);
    }
    return output_frame_end(s);
}

/******************************************************************************
**
**	AC-3 Encoder API
**
*******************************************************************************/

AC3CodecContext	*ac3_encoder_open()
{
	// encoder-wrapper structure
	AC3CodecContext	*enc = (AC3CodecContext*)malloc(sizeof(AC3CodecContext));
	if (!enc) return NULL;

	// encoder context
	AC3EncodeContext	*encc = (AC3EncodeContext*)malloc(sizeof(AC3EncodeContext));
	if (!encc) {
		free(enc);
		return NULL;
	}
	enc->priv_data = (void*)encc;

	// set default values
	enc->bit_rate		= 192000;
	enc->sample_rate	= 0;
	enc->channels		= 0;
	enc->frame_size		= 0;
	enc->cutoff			= 0;

	return enc;
}

void ac3_encoder_close(AC3CodecContext *encoder)
{
	if (!encoder) return ;

	AC3EncodeContext	*encc = (AC3EncodeContext*)encoder->priv_data;
	if (encc) {
		free(encc);
	}

	// done with the encoder
	free(encoder);
}

int ac3_encoder_init(AC3CodecContext *encoder)
{
	if (!encoder) return -1;

	AC3EncodeContext	*encc = (AC3EncodeContext*)encoder->priv_data;
	if (!encc) return -1;

	int ret = AC3_encode_init(encoder);
	return ret;
}

int ac3_encoder_frame(AC3CodecContext *encoder, short *pcm_samples, uint8 *buf, int buf_size)
{
	if (!encoder) return -1;

	int ret = AC3_encode_frame(encoder, buf, buf_size, (void*)pcm_samples);
	return ret;
}






