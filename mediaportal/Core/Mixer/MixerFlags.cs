using System;

namespace MediaPortal.Mixer
{
	internal enum MixerFlags : uint
	{
		Auxillary					= 0x50000000,
		CallbackDelegate			= 0x00030000,
		CallbackNone				= 0x00000000,		
		CallbackWindow				= 0x00010000,
		Handle						= 0x80000000,
		Mixer						= 0x00000000,
		MixerHandle					= Handle | Mixer,
		WaveOut						= 0x10000000,
		WaveOutHandle				= Handle | WaveOut,
		WaveIn						= 0x20000000,
		WaveInHandle				= Handle | WaveIn,
		MidiOut						= 0x30000000,
		MidiOutHandle				= Handle | MidiOut,
		MidiIn						= 0x40000000,
		MidiInHandle				= Handle | MidiIn,
	}
}
