using System;

namespace MediaPortal.Configuration
{
	public class RadioStation
	{
		public bool Scrambled=false;
		public int ID=-1;
		public string Type="";
		public string Name="";
		public string Genre="";
		public int Bitrate=0;
		public string URL="";
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
