//
// Copyright (c) 1998-2001 B2C2, Inc.  All Rights Reserved.
//

// IB2C2MPEG2AVCtrl.h

// Note: Contains interfaces IB2C2MPEG2AVCtrl and IB2C2MPEG2AVCtrl2


#ifndef _IB2C2MPEG2AVCTRL_H_
#define _IB2C2MPEG2AVCTRL_H_

#if defined __linux__	// Class implementation for Linux

typedef unsigned short WORD;
typedef unsigned char BYTE;

class CAVSrcFilter;

///////////////////////////////////////////////////////////////////////////////
//
// IB2C2MPEG2AVCtrl
//

class IB2C2MPEG2AVCtrl
{
protected: // Data

	CAVSrcFilter * m_pFilter;

public:	// Constructor
	IB2C2MPEG2AVCtrl (CAVSrcFilter *);

#else 					// COM implementation for Windows

#ifdef __cplusplus
extern "C" {
#endif

// Interface: IB2C2MPEG2AVCtrl

DECLARE_INTERFACE_(IB2C2MPEG2AVCtrl, IUnknown) {

#endif //defined __linux__

	// Argument 1: Audio PID
	// Argument 2: Video PID

	STDMETHOD(SetAudioVideoPIDs) (THIS_
				long,
				long
			 ) PURE;
};

// Video window aspect ratios

#define ASPECT_RATIO_INVALID		0
#define ASPECT_RATIO_SQUARE			1
#define ASPECT_RATIO_4x3			2
#define ASPECT_RATIO_16x9			3
#define USER_DEFINED_ASPECT_RATIO	4

// Frame rate values

#define FRAME_RATE_FORBIDDEN		0
#define FRAME_RATE_23_97			1	// I.e. 23.97
#define FRAME_RATE_24				2
#define FRAME_RATE_25				3
#define FRAME_RATE_29_97			4
#define FRAME_RATE_30				5
#define FRAME_RATE_50				6
#define FRAME_RATE_59_94			7
#define FRAME_RATE_60				8

// Interface: IB2C2MPEG2AVCtrl2

// Struct describing video data; this structure will be populated and
// passed to the user function described below.

typedef struct _VIDEO_INFO
{
	WORD	wHSize;			// video data horizontal size in pixels
	WORD	wVSize;			// video data vertical size in pixels
	BYTE	bAspectRatio;
	BYTE	bFrameRate;
} MPEG2_VIDEO_INFO;

// The user function that will be passed to SetCallbackForVideoMode should
// have the following prototype:
//
//    UINT __stdcall UserFunc(MPEG2_VIDEO_INFO *);
//
// The return value from the user function is currently ignored.
// To cancel the callback, call SetCallbackForVideoMode with a NULL argument.
// "UserFunc" should return control as soon as possible as this is a
// synchronized callback and blocks continuation in the calling thread, which
// in this case is the B2C2 MPEG2 filter.

#if defined __linux__	// Class implementation for Linux

///////////////////////////////////////////////////////////////////////////////
//
// IB2C2MPEG2AVCtrl2
//

class IB2C2MPEG2AVCtrl2 : public IB2C2MPEG2AVCtrl
{


public:	// Constructor
	IB2C2MPEG2AVCtrl2 (CAVSrcFilter *);

#else 					// COM implementation for Windows

// Interface: IB2C2MPEG2AVCtrl2

DECLARE_INTERFACE_(IB2C2MPEG2AVCtrl2, IB2C2MPEG2AVCtrl) {

#endif //defined __linux__

#if defined WIN32
	// Argument : Function pointer to user function; see comments above for
	//			  prototype.

	STDMETHOD(SetCallbackForVideoMode) (THIS_
				PVOID
			 ) PURE;
#endif //defined WIN32

	STDMETHOD(DeleteAudioVideoPIDs) (THIS_
				long,
				long
			 ) PURE;

	STDMETHOD(GetAudioVideoState) (THIS_
				long *,
				long *,
				long *,
				long *,
				long *,
				long *
			 ) PURE;
};


#if defined WIN32

#ifdef __cplusplus
}

#endif

#endif //defined WIN32

#endif // _IB2C2MPEG2AVCTRL_H_
