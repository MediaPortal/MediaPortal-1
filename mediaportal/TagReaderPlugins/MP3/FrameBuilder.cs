// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;

namespace id3
{
	/// <summary>
	/// Build the adecuate frame parser
	/// </summary>
	public class FrameBuilder
	{
		/// <summary>
		/// Builds a frame base, based on the tag type
		/// </summary>
		public static FrameBase Build(string tag)
		{
			
			switch(tag)
			{
				case "USLT":
				case "COMM":
				{
					return new FrameLCText(tag);
				}
				case "APIC":
				{
					return new FrameAPIC(tag);
				}
				case "GEOB":
				{
					return new FrameGEOB(tag);
				}
			}
			if(tag[0] == 'T') //(TXXX) Decault Text tag
			{
				return new FrameText(tag);	//default
			}
			if(tag[0] == 'W') //(WXXX) Decault Text tag
			{
				return new FrameURL(tag);	//default
			}
			
			throw new Exception("Tag not impemented.");//TODO: Change this to a null FrameBase handler
		}
	}
}
