using System;

namespace MediaPortal.Mixer
{
	internal enum MixerLineStatusFlags : uint
	{
		Active						= 0x00000001,
		Disconnected				= 0x00008000,
		Source						= 0x80000000,
	}
}
