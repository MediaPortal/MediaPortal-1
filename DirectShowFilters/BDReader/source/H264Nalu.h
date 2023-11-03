/*
 * $Id: H264Nalu.h 3581 2011-08-05 16:26:10Z underground78 $
 *
 * (C) 2006-2011 see AUTHORS
 *
 * This file is part of mplayerc.
 *
 * Mplayerc is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * Mplayerc is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#pragma once


typedef enum {
	NALU_TYPE_SLICE    = 1,
	NALU_TYPE_DPA      = 2,
	NALU_TYPE_DPB      = 3,
	NALU_TYPE_DPC      = 4,
	NALU_TYPE_IDR      = 5,
	NALU_TYPE_SEI      = 6,
	NALU_TYPE_SPS      = 7,
	NALU_TYPE_PPS      = 8,
	NALU_TYPE_AUD      = 9,
	NALU_TYPE_EOSEQ    = 10,
	NALU_TYPE_EOSTREAM = 11,
	NALU_TYPE_FILL     = 12
} NALU_TYPE;

typedef enum {
    HEVC_NAL_TRAIL_N = 0,
    HEVC_NAL_TRAIL_R = 1,
    HEVC_NAL_TSA_N = 2,
    HEVC_NAL_TSA_R = 3,
    HEVC_NAL_STSA_N = 4,
    HEVC_NAL_STSA_R = 5,
    HEVC_NAL_RADL_N = 6,
    HEVC_NAL_RADL_R = 7,
    HEVC_NAL_RASL_N = 8,
    HEVC_NAL_RASL_R = 9,
    HEVC_NAL_VCL_N10 = 10,
    HEVC_NAL_VCL_R11 = 11,
    HEVC_NAL_VCL_N12 = 12,
    HEVC_NAL_VCL_R13 = 13,
    HEVC_NAL_VCL_N14 = 14,
    HEVC_NAL_VCL_R15 = 15,
    HEVC_NAL_BLA_W_LP = 16,
    HEVC_NAL_BLA_W_RADL = 17,
    HEVC_NAL_BLA_N_LP = 18,
    HEVC_NAL_IDR_W_RADL = 19,
    HEVC_NAL_IDR_N_LP = 20,
    HEVC_NAL_CRA_NUT = 21,
    HEVC_NAL_RSV_IRAP_VCL22 = 22,
    HEVC_NAL_RSV_IRAP_VCL23 = 23,
    HEVC_NAL_RSV_VCL24 = 24,
    HEVC_NAL_RSV_VCL25 = 25,
    HEVC_NAL_RSV_VCL26 = 26,
    HEVC_NAL_RSV_VCL27 = 27,
    HEVC_NAL_RSV_VCL28 = 28,
    HEVC_NAL_RSV_VCL29 = 29,
    HEVC_NAL_RSV_VCL30 = 30,
    HEVC_NAL_RSV_VCL31 = 31,
    HEVC_NAL_VPS = 32,
    HEVC_NAL_SPS = 33,
    HEVC_NAL_PPS = 34,
    HEVC_NAL_AUD = 35,
    HEVC_NAL_EOS_NUT = 36,
    HEVC_NAL_EOB_NUT = 37,
    HEVC_NAL_FD_NUT = 38,
    HEVC_NAL_SEI_PREFIX = 39,
    HEVC_NAL_SEI_SUFFIX = 40,
    HEVC_NAL_RSV_NVCL41 = 41,
    HEVC_NAL_RSV_NVCL42 = 42,
    HEVC_NAL_RSV_NVCL43 = 43,
    HEVC_NAL_RSV_NVCL44 = 44,
    HEVC_NAL_RSV_NVCL45 = 45,
    HEVC_NAL_RSV_NVCL46 = 46,
    HEVC_NAL_RSV_NVCL47 = 47,
    HEVC_NAL_UNSPEC48 = 48,
    HEVC_NAL_UNSPEC49 = 49,
    HEVC_NAL_UNSPEC50 = 50,
    HEVC_NAL_UNSPEC51 = 51,
    HEVC_NAL_UNSPEC52 = 52,
    HEVC_NAL_UNSPEC53 = 53,
    HEVC_NAL_UNSPEC54 = 54,
    HEVC_NAL_UNSPEC55 = 55,
    HEVC_NAL_UNSPEC56 = 56,
    HEVC_NAL_UNSPEC57 = 57,
    HEVC_NAL_UNSPEC58 = 58,
    HEVC_NAL_UNSPEC59 = 59,
    HEVC_NAL_UNSPEC60 = 60,
    HEVC_NAL_UNSPEC61 = 61,
    HEVC_NAL_UNSPEC62 = 62,
    HEVC_NAL_UNSPEC63 = 63,
} HEVC_NALU_TYPE;


class CH264Nalu
{
private :
	//int			forbidden_bit;		//! should be always FALSE
	//int			nal_reference_idc;	//! NALU_PRIORITY_xxxx
	//NALU_TYPE	nal_unit_type;		//! NALU_TYPE_xxxx

	int			m_nNALStartPos;		//! NALU start (including startcode / size)
	int			m_nNALDataPos;		//! Useful part
	unsigned	m_nDataLen;			//! Length of the NAL unit (Excluding the start code, which does not belong to the NALU)

	BYTE*		m_pBuffer;
	int			m_nCurPos;
	int			m_nNextRTP;
	int			m_nSize;
	int			m_nNALSize;

	bool		MoveToNextAnnexBStartcode();
	bool		MoveToNextRTPStartcode();

public :
	CH264Nalu();

	//NALU_TYPE GetType() const {
	//	return nal_unit_type;
	//};
	//bool IsRefFrame() const {
	//	return (nal_reference_idc != 0);
	//};

	int GetDataLength() const {
		return m_nCurPos - m_nNALDataPos;
	};
	BYTE* GetDataBuffer() {
		return m_pBuffer + m_nNALDataPos;
	};
	int GetRoundedDataLength() const {
		int nSize = m_nCurPos - m_nNALDataPos;
		return nSize + 128 - (nSize %128);
	}

	int GetLength() const {
		return m_nCurPos - m_nNALStartPos;
	};
	BYTE* GetNALBuffer() {
		return m_pBuffer + m_nNALStartPos;
	};
	bool IsEOF() const {
		return m_nCurPos >= m_nSize;
	};

	void SetBuffer (BYTE* pBuffer, int nSize, int nNALSize);
	bool ReadNext();
};
