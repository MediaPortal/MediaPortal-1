// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.Text;

namespace id3
{
	/// <summary>
	/// Summary description for BaseFrame.
	/// </summary>
	public abstract class FrameBase
	{
		string _tag;

		public FrameBase(string tag)
		{
			_tag = tag;
		}

		public abstract void Parse(byte[] frame);
		public abstract byte[] Make();

		public string Tag
		{
			get{return _tag;}
		}
	}
}
