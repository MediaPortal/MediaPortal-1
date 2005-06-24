using System;
using System.Collections;

namespace MediaPortal.Video.Database
{
	/// <summary>
	/// Summary description for IMDBActor.
	/// </summary>
	public class IMDBActor
	{
		public class IMDBActorMovie
		{
			public string MovieTitle;
			public string Role;
			public int Year;
		};
		string _Name=String.Empty;
		string _thumbnailUrl=String.Empty;
		string _placeOfBirth=String.Empty;
		string _dateOfBirth=String.Empty;
		string _miniBiography=String.Empty;
		string _biography=String.Empty;
		ArrayList movies=new ArrayList(); 

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
		public string DateOfBirth
		{
			get { return _dateOfBirth;}
			set { _dateOfBirth=value;}
		}
		public string PlaceOfBirth
		{
			get { return _placeOfBirth;}
			set { _placeOfBirth=value;}
		}
		public string MiniBiography
		{
			get { return _miniBiography;}
			set { _miniBiography=value;}
		}
		public string Biography
		{
			get { return _biography;}
			set { _biography=value;}
		}
		public int Count
		{
			get { return movies.Count;}
		}
		public IMDBActorMovie this[int index]
		{
			get { return (IMDBActorMovie)movies[index];}
			set { movies[index]=value;}
		}
		public void Add(IMDBActorMovie movie)
		{
			movies.Add(movie);
		}
	}
}
