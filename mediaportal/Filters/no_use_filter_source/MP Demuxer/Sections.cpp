#include <streams.h>
#include <aviriff.h>
#include "Sections.h"

// MPEG Audio Values
	const ULONG AudioFrequencies[3][4]={{22050,24000,16000,0},{44100,48000,32000,0},{11025,12000,8000,0}};

	const ULONG AudioBitrates[3][3][16] = {
		{{-1,8000,16000,24000,32000,40000,48000,56000,64000,80000,96000,112000,128000,144000,160000,0 },		
		{-1,8000,16000,24000,32000,40000,48000,56000,64000,80000,96000,112000,128000,144000,160000,0 },		
		{-1,32000,48000,56000,64000,80000,96000,112000,128000,144000,160000,176000,192000,224000,256000,0 }	}
		,
		{{-1,32000,40000,48000,56000,64000,80000,96000,112000,128000,160000,192000,224000,256000,320000, 0 },	
		{-1,32000,48000,56000,64000,80000,96000,112000,128000,160000,192000,224000,256000,320000,384000, 0 },	
		{-1,32000,64000,96000,128000,160000,192000,224000,256000,288000,320000,352000,384000,416000,448000,0 }}
		,
		{{-1, 6000, 8000, 10000, 12000, 16000, 20000, 24000,28000, 320000, 40000, 48000, 56000, 64000, 80000, 0 },
		{-1, 6000, 8000, 10000, 12000, 16000, 20000, 24000,28000, 320000, 40000, 48000, 56000, 64000, 80000, 0 },
		{-1, 8000, 12000, 16000, 20000, 24000, 32000, 40000,48000, 560000, 64000, 80000, 96000, 112000, 128000, 0}
	}};

	const double AudioTimes[4] = { 0.0,103680000.0,103680000.0,34560000.0 };

Sections::Sections(void)
{
}

Sections::~Sections()
{
}

HRESULT Sections::GetAdaptionHeader(BYTE *data,AdaptionHeader *header)
{
	header->Len=data[0];
	header->DiscontinuityIndicator=(data[1] & 0x80)>0?true:false;
	header->RandomAccessIndicator=(data[1] & 0x40)>0?true:false;
	header->ElementaryStreamPriorityIndicator=(data[1] & 0x20)>0?true:false;
	header->PCRFlag=(data[1] & 0x10)>0?true:false;
	header->OPCRFlag=(data[1] & 0x08)>0?true:false;
	header->SplicingPointFlag=(data[1] & 0x04)>0?true:false;
	header->TransportPrivateData=(data[1] & 0x02)>0?true:false;
	header->AdaptationHeaderExtension=(data[1] & 0x01)>0?true:false;
	if(header->PCRFlag==true)
	{
		__int64 pcr_H=(__int64)((data[2]& 0x080)>>7);
		__int64 pcr_L =(__int64)((data[2]&0x7f));pcr_L<<=25;
		pcr_L |= (__int64)(data[3]<<17); 
		pcr_L |= (__int64) (data[4]<<9); 
		pcr_L |= (__int64) ((data[5])<<1);
		pcr_L |= (__int64)((data[6]&0x80)>>7);
		__int64 ull=ull = (pcr_H << 32) + pcr_L;
		header->PCRValue=ull;
		//GetPTS(&data[2],&(header->PCRValue));
		header->PCRCounter=((data[6] & 0x01)*256)+data[7];
	}
	return S_OK;
}
HRESULT Sections::GetTSHeader(BYTE *data,TSHeader *header)
{
	header->SyncByte=data[0];
	header->TransportError=(data[1] & 0x80)>0?true:false;
	header->PayloadUnitStart=(data[1] & 0x40)>0?true:false;
	header->TransportPriority=(data[1] & 0x20)>0?true:false;
	header->Pid=((data[1] & 0x1F) <<8)+data[2];
	header->TScrambling=data[3] & 0xC0;
	header->AdaptionControl=(data[3]>>4) & 0x3;
	header->ContinuityCounter=data[3] & 0x0F;
	return S_OK;
}
HRESULT Sections::GetPESHeader(BYTE *data,PESHeader *header)
{
	header->Reserved=(data[0] & 0xC0)>>6;
	header->ScramblingControl=(data[0] &0x30)>>4;
	header->Priority=(data[0] & 0x08)>>3;
	header->dataAlignmentIndicator=(data[0] & 0x04)>>2;
	header->Copyright=(data[0] & 0x02)>>1;
	header->Original=data[0] & 0x01;
	header->PTSFlags=(data[1] & 0xC0)>>6;
	header->ESCRFlag=(data[1] & 0x20)>>5;
	header->ESRateFlag=(data[1] & 0x10)>>4;
	header->DSMTrickModeFlag=(data[1] & 0x08)>>3;
	header->AdditionalCopyInfoFlag=(data[1] & 0x04)>>2;
	header->PESCRCFlag=(data[1] & 0x02)>>1;
	header->PESExtensionFlag=data[1] & 0x01;
	header->PESHeaderDataLength=data[2];
	return S_OK;
}
void Sections::GetPTS(BYTE *data,ULONGLONG *pts)
{
	//High:1 Low:32 reserved:6 extension:9
	ULONGLONG ptsVal;
	bool ptsFlag=false;
	ptsVal= (ULONGLONG((data[0] & 0xE0)) << 29) +
                    (data[1] << 22) +
                    ((data[2] & 0xFE) << 14) +
                    (data[3] << 7) +
                    ((data[4] & 0xFE) >> 1);

	*pts=ptsVal;
}
void Sections::PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime)
{
	ULONG  _90khz = (ULONG)(pts/90);
	ptsTime->h=(_90khz/(1000*60*60));
	ptsTime->m=(_90khz/(1000*60))-(ptsTime->h*60);
	ptsTime->s=(_90khz/1000)-(ptsTime->h*3600)-(ptsTime->m*60);
	ptsTime->u=_90khz-(ptsTime->h*1000*60*60)-(ptsTime->m*1000*60)-(ptsTime->s*1000);

}


HRESULT Sections::ParseAudioHeader(BYTE *data,AudioHeader *head)
{
    AudioHeader header;
	int limit = 32;

	if ((data[0] & 0xFF) != 0xFF || (data[1] & 0xF0) != 0xF0)
		return S_FALSE;

	header.ID = ((data[1] >> 3) &0x01) ;
	header.Emphasis = data[3] & 0x03;

	if (header.ID == 1 && header.Emphasis == 2)
		header.ID = 2;
	header.Layer = ((data[1] >>1) &0x03);

	if (header.Layer < 1)
		return S_FALSE;

	header.ProtectionBit = (data[1] & 0x01) ^ 1;
	header.Bitrate = AudioBitrates[header.ID][header.Layer-1][((data[2] >>4)& 0x0F)];
	if (header.Bitrate < 1)
		return S_FALSE;
	header.SamplingFreq = AudioFrequencies[header.ID][((data[2] >>2)& 0x03)];
	if (header.SamplingFreq == 0)
		return S_FALSE;

	header.PaddingBit = ((data[2] >>1)& 0x01) ;
	header.PrivateBit = data[2] & 0x01;

	header.Mode = ((data[3] >>6)& 0x03) & 0x03;
	header.ModeExtension = ((data[3] >>4)& 0x03) ;
	if (header.Mode == 0)
		header.ModeExtension = 0;

	header.Bound = (header.Mode == 1) ? ((header.ModeExtension + 1) << 2) : limit;
	header.Channel = (header.Mode == 3) ? 1 : 2;
	header.Copyright = ((data[3]>>3) & 0x01);
	header.Original = ((data[3] >>2)& 0x01) ;
	header.TimeLength = (int)(AudioTimes[header.Layer] / header.SamplingFreq);

	if (header.ID == 1 && header.Layer == 2)
	{	

		if (header.Bitrate / header.Channel < 32000)
			return S_FALSE;
		if (header.Bitrate / header.Channel > 192000)
			return S_FALSE;

		if (header.Bitrate < 56000)
		{
			if (header.SamplingFreq == 32000)
				limit = 12;
			else
				limit = 8;
		}
		else 
			if (header.Bitrate < 96000)
                    limit = 27;
			else
			{
				if (header.SamplingFreq == 48000)
					limit = 27;
				else
					limit = 30;
			}
			if (header.Bound > limit)
				header.Bound = limit;
	}
	else 
	if (header.Layer == 2)  // MPEG-2
	{
		limit = 30;
	}

	if (header.Layer < 3)
	{
		if (header.Bound > limit)
			header.Bound = limit;
		header.Size = (header.SizeBase = 144 * header.Bitrate / header.SamplingFreq) + header.PaddingBit;
		memcpy(head,&header,sizeof(struct staudioHeader));
		return S_OK;
	}
	else
	{
		limit = 32;
		header.Size = (header.SizeBase = (12 * header.Bitrate / header.SamplingFreq) * 4) + (4 * header.PaddingBit);
		memcpy(head,&header,sizeof(struct staudioHeader));
		return S_OK;
	}

}