// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;

namespace id3
{
	/// <summary>
	/// Summary description for FrameModel.
	/// </summary>
	public class FrameModel
	{
		public FrameModel()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private Header _tagHeader  = new Header();
		private ExtendedHeader _tagExtendedHeader  = new ExtendedHeader();

		public Header Header
		{
			get{ return _tagHeader;}
			set{ _tagHeader = value;}
		}

	}
}
