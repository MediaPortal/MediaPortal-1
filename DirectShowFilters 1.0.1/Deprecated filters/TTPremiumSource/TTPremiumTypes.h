#pragma once

enum NetworkType
{
	NT_UNKNOWN,
	NT_DVB_S,		
	NT_DVB_C,				
	NT_DVB_T,
};

enum ModType
{
	QAM_16   =  0,
	QAM_32   =  1,
	QAM_64   =  2,
	QAM_128  =  3,
	QAM_256  =  4,
};

enum BandwidthType
{
	BW_6MHz  = 0,
	BW_7MHz  = 1,
	BW_8MHz  = 2,
	BW_NONE  = 4
};

