using System;

namespace MediaPortal.Video.Database
{
	/// <summary>
	/// Summary description for IMDBActor.
	/// </summary>
	public class IMDBActor
	{
		string _Name;
		string _thumbnailUrl;

		public IMDBActor()
		{
		}

		public string Name
		{
			get { return _Name;}
			set { _Name=value;}
		}

		public string ThumbnailUrl
		{
			get { return _thumbnailUrl;}
			set { _thumbnailUrl=value;}
		}
	}
}
