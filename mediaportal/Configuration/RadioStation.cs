using System;

namespace MediaPortal.Configuration
{
	public class RadioStation
	{
		public string Type;
		public string Name;
		public string Genre;
		public int Bitrate;
		public string URL;
		public Frequency Frequency;

		public RadioStation()
		{
		}

		public RadioStation(int channel, long frequency)
		{
			this.Frequency = frequency;
		}

		public override string ToString()
		{
			return String.Format("Frequency: {0}", Frequency.ToString(Frequency.Format.Herz));
		}
	}
}
