using System;

namespace MediaPortal.Configuration
{
	public class RadioStation
	{
		public string Type;
		public string Name;
		public int Channel;
		public string Genre;
		public int Bitrate;
		public string URL;
		public Frequency Frequency;

		public RadioStation()
		{
		}

		public RadioStation(int channel, long frequency)
		{
			this.Channel = channel;
			this.Frequency = frequency;
		}

		public override string ToString()
		{
			return String.Format("Channel: {0}, Frequency: {1}", Channel, Frequency.ToString(Frequency.Format.Herz));
		}
	}
}
