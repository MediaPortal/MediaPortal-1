//-----------------------------------------------------------------------------
//
//	MONOGRAM AC3 Encoder
//
//	Based on libavcodec AC-3 encoder
//
//	Author : Igor Janos
//
//-----------------------------------------------------------------------------
#pragma once

#include "bits.h"

#define AC3_MAX_CHANNELS 6						/* including LFE channel */
#define NB_BLOCKS 6								/* number of PCM blocks inside an AC-3 frame */
#define AC3_FRAME_SIZE (NB_BLOCKS * 256)

/* exponent encoding strategy */
#define EXP_REUSE 0
#define EXP_NEW   1

#define EXP_D15   1
#define EXP_D25   2
#define EXP_D45   3

/** Delta bit allocation strategy */
typedef enum {
    DBA_REUSE = 0,
    DBA_NEW,
    DBA_NONE,
    DBA_RESERVED
} AC3DeltaStrategy;

/** Channel mode (audio coding mode) */
typedef enum {
    AC3_CHMODE_DUALMONO = 0,
    AC3_CHMODE_MONO,
    AC3_CHMODE_STEREO,
    AC3_CHMODE_3F,
    AC3_CHMODE_2F1R,
    AC3_CHMODE_3F1R,
    AC3_CHMODE_2F2R,
    AC3_CHMODE_3F2R
} AC3ChannelMode;

struct AC3CodecContext {

	int		sample_rate;		
	int		bit_rate;			
	int		channels;
	int		frame_size;		
	int		cutoff;

	void	*priv_data;				// encode context
};


/*
	API na AC-3 encoder
*/

AC3CodecContext	*ac3_encoder_open();
void ac3_encoder_close(AC3CodecContext *encoder);
int ac3_encoder_init(AC3CodecContext *encoder);
int ac3_encoder_frame(AC3CodecContext *encoder, short *pcm_samples, uint8 *buf, int buf_size);
