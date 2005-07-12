using System;

namespace MediaPortal.Mixer
{
	internal enum MixerControlClass : uint
	{
		Mask						= 0xF0000000,
		Custom						= 0x00000000,
		Meter						= 0x10000000,
		Switch						= 0x20000000,
		Number						= 0x30000000,
		Slider						= 0x40000000,
		Fader						= 0x50000000,
		Time						= 0x60000000,
		List						= 0x70000000,
	}
}
