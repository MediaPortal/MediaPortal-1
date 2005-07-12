using System;

namespace MediaPortal.Mixer
{
	internal enum MixerControlType : uint
	{
		Bass						= (MixerControlClass.Fader | MixerControlUnits.Unsigned) + MixerControlClass.Fader + 2,
		Equalizer					= (MixerControlClass.Fader | MixerControlUnits.Unsigned) + MixerControlClass.Fader + 4,
		Mute						= (MixerControlClass.Switch | MixerControlUnits.Boolean) + 2,
		Treble						= (MixerControlClass.Fader | MixerControlUnits.Unsigned) + MixerControlClass.Fader + 3,
		Volume						= (MixerControlClass.Fader | MixerControlUnits.Unsigned) + 1,
	}
}
