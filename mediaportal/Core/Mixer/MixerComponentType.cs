using System;

namespace MediaPortal.Mixer
{
	internal enum MixerComponentType : int
	{
		DestinationNone				= 0,
		DestinationDigital			= 1,
		DestinationLine				= 2,
		DestinationMonitor			= 3,
		DestinationSpeakers			= 4,
		DestinationHeadphones		= 5,
		DestinationTelephone		= 6,
		DestinationWave				= 7,
		DestinationVoice			= 8,

		SourceNone					= 4096,
		SourceDigital				= 4097,
		SourceLine					= 4098,
		SourceMicrophone			= 4099,
		SourceSynthesizer			= 4100,
		SourceCompactDisc			= 4101,
		SourceTelephone				= 4102,
		SourceSpeaker				= 4103,
		SourceWave					= 4104,
		SourceAuxiliary				= 4105,
		SourceAnalog				= 4106,
	}
}
