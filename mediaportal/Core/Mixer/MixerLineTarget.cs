using System;

namespace MediaPortal.Mixer
{
	internal enum MixerLineTargetType
	{
		None						= 0,
		WaveOut						= 1,
		WaveIn						= 2,
		MidiOut						= 3,
		MidiIn						= 4,
		Auxiliary					= 5,
	}
}
