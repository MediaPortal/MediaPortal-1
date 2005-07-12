using System;

namespace MediaPortal.Mixer
{
	internal enum MixerControlUnits : uint
	{
		Boolean						= 0x00010000,
		Custom						= 0x00000000,
		Decibels					= 0x00040000,
		Mask						= 0x00FF0000,
		Percent						= 0x00050000,
		Signed						= 0x00020000,
		Unsigned					= 0x00030000,
	}
}
