using System;

namespace MediaPortal.Mixer
{
	internal enum MixerError : uint
	{
		None						= 0,
		Error						= 1,
		BadDeviceId					= 2,
		NotEnabled					= 3,
		Allocated					= 4,
		InvalidHandle				= 5,
		NoDriver					= 6,
		NoMemory					= 7,
		NotSupported				= 8,
		BadErrorNumber				= 9,
		InvalidFlag					= 10,
		InvalidParameter			= 11,
		Busy						= 12,

		InvalidLine					= 1024,
		InvalidControl				= 1025,
		InvalidValue				= 1026,
	}
}
