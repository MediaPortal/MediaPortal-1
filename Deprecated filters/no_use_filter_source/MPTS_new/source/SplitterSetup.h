/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#ifndef __Demux
#define __Demux

#include "Sections.h"
#include "Control.h"
	

// media types
	static BYTE	Mpeg2ProgramVideo [] = 
	{
					0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
					0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
					0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
					0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

					//0x051736=333667-> 10000000/333667 = 29.97fps
					//0x061A80=400000-> 10000000/400000 = 25fps
					0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
					0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
					0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
					0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
					0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
					0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
					0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
					0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
					0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
					0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
					0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
					0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
					0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
					0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
					0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
					//0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
					0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
					0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
					0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
					
					//  .dwSequenceHeader [1]
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00
	};
	
	static	BYTE	MPEG1AudioFormat [] = 
	{
		0x50, 0x00,
		0x02, 0x00,
		0x80, 0xBB,	0x00, 0x00, 
		0x00, 0x7D,	0x00, 0x00, 
		0x00, 0x03,				
		0x00, 0x00,				
		0x16, 0x00,				
		0x02, 0x00,				
		0x00, 0xE8,				
		0x03, 0x00,				
		0x01, 0x00,	0x01,0x00,  
		0x01, 0x00,	0x1C, 0x00, 0x00, 0x00,	0x00, 0x00, 0x00, 0x00, 0x00, 0x00
	};
	//
		static BYTE g_Mpeg2ProgramVideo [] = 
		{
			0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
			0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
			0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
		0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
		0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
		0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
		0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
		0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
		0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

		//0x051736=333667-> 10000000/333667 = 29.97fps
		//0x061A80=400000-> 10000000/400000 = 25fps
		0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
		0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
		0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
		0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
		0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
		0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
		0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
		0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
		0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
		0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
		0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
		0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
		0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
		0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
		0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
		0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
		0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
				//0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
		0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
		0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
		0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
					
				//  .dwSequenceHeader [1]
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00
	};


	static BYTE g_MPEG1AudioFormat [] = 
	{
		0x50, 0x00,				//wFormatTag
		0x02, 0x00,				//nChannels
		0x80, 0xbb, 0x00, 0x00, //nSamplesPerSec
		0x00, 0x7d, 0x00, 0x00, //nAvgBytesPerSec
		0x01, 0x00,				//nBlockAlign
		0x00, 0x00,				//wBitsPerSample
		0x16, 0x00,				//cbSize
		0x02, 0x00,				//wValidBitsPerSample
		0x00, 0xe8,				//wSamplesPerBlock
		0x03, 0x00,				//wReserved
		0x01, 0x00, 0x01, 0x00, //dwChannelMask
		0x01, 0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
	};
class SplitterSetup
{
public:

	SplitterSetup(Sections *pSections);
	virtual ~SplitterSetup();
	STDMETHODIMP SetDemuxPins(IFilterGraph *pGraph);
	HRESULT SetupPids();


protected:
	HRESULT	GetAC3Media(AM_MEDIA_TYPE *pintype);
	HRESULT	GetMP2Media(AM_MEDIA_TYPE *pintype);
	HRESULT GetAudioPayload(AM_MEDIA_TYPE *pintype);
	HRESULT	GetMP1Media(AM_MEDIA_TYPE *pintype);
	HRESULT	GetVideoMedia(AM_MEDIA_TYPE *pintype);
	HRESULT SetupDemuxer(IBaseFilter *p);

protected:
	Sections *m_pSections;
	BOOL m_demuxSetupComplete;
	IPin* m_pAudio;
	IPin* m_pVideo;

};

#endif
