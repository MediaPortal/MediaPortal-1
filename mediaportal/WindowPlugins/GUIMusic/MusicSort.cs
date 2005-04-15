using System;
using System.Collections;
using System.Globalization;
using MediaPortal.TagReader;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.GUI.Music
{
	/// <summary>
	/// Summary description for MusicSort.
	/// </summary>
	public class MusicSort : IComparer
	{
		SortMethod currentSortMethod;
		bool sortAscending=true;
		public MusicSort(SortMethod method, bool ascending)
		{
			currentSortMethod=method;
			sortAscending=ascending;
		}

		public enum SortMethod
		{
			Name=0,
			Date=1,
			Size=2,
			Track=3,
			Duration=4,
			Title=5,
			Artist=6,
			Album=7,
			Filename=8,
			Rating=9
		}
		public int Compare(object x, object y)
		{
			if (x==y) return 0;
			GUIListItem item1=(GUIListItem)x;
			GUIListItem item2=(GUIListItem)y;
			if (item1==null) return -1;
			if (item2==null) return -1;
			if (item1.IsFolder && item1.Label=="..") return -1;
			if (item2.IsFolder && item2.Label=="..") return -1;
			if (item1.IsFolder && !item2.IsFolder) return -1;
			else if (!item1.IsFolder && item2.IsFolder) return 1; 

			string strSize1="";
			string strSize2="";
			if (item1.FileInfo!=null) strSize1=Utils.GetSize(item1.FileInfo.Length);
			if (item2.FileInfo!=null) strSize2=Utils.GetSize(item2.FileInfo.Length);

			SortMethod method=currentSortMethod;
			bool bAscending=sortAscending;

			switch (method)
			{
				case SortMethod.Name:
					if (bAscending)
					{
						return String.Compare(item1.Label ,item2.Label,true);
					}
					else
					{
						return String.Compare(item2.Label ,item1.Label,true);
					}
        

				case SortMethod.Date:
					if (item1.FileInfo==null) return -1;
					if (item2.FileInfo==null) return -1;
          
					item1.Label2 =item1.FileInfo.CreationTime.ToShortDateString() + " "+item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					item2.Label2 =item2.FileInfo.CreationTime.ToShortDateString() + " "+item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					if (bAscending)
					{
						return DateTime.Compare(item1.FileInfo.CreationTime,item2.FileInfo.CreationTime);
					}
					else
					{
						return DateTime.Compare(item2.FileInfo.CreationTime,item1.FileInfo.CreationTime);
					}

				case SortMethod.Rating:
					int iRating1 = 0;
					int iRating2 = 0;
					if (item1.MusicTag != null) iRating1 = ((MusicTag)item1.MusicTag).Rating;
					if (item2.MusicTag != null) iRating2 = ((MusicTag)item2.MusicTag).Rating;
					if (bAscending)
					{
						return (int)(iRating1 - iRating2);
					}
					else
					{
						return (int)(iRating2 - iRating1);
					}

				case SortMethod.Size:
					if (item1.FileInfo==null) return -1;
					if (item2.FileInfo==null) return -1;
					if (bAscending)
					{
						return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
					}
					else
					{
						return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
					}

				case SortMethod.Track:
					int iTrack1=0;
					int iTrack2=0;
					if (item1.MusicTag!=null) iTrack1=((MusicTag)item1.MusicTag).Track;
					if (item2.MusicTag!=null) iTrack2=((MusicTag)item2.MusicTag).Track;
					if (bAscending)
					{
						return (int)(iTrack1 - iTrack2);
					}
					else
					{
						return (int)(iTrack2 - iTrack1);
					}
          
				case SortMethod.Duration:
					int iDuration1=0;
					int iDuration2=0;
					if (item1.MusicTag!=null) iDuration1=((MusicTag)item1.MusicTag).Duration;
					if (item2.MusicTag!=null) iDuration2=((MusicTag)item2.MusicTag).Duration;
					if (bAscending)
					{
						return (int)(iDuration1 - iDuration2);
					}
					else
					{
						return (int)(iDuration2 - iDuration1);
					}
          
				case SortMethod.Title:
					string strTitle1=item1.Label;
					string strTitle2=item2.Label;
					if (item1.MusicTag!=null) strTitle1=((MusicTag)item1.MusicTag).Title;
					if (item2.MusicTag!=null) strTitle2=((MusicTag)item2.MusicTag).Title;
					if (bAscending)
					{
						return String.Compare(strTitle1 ,strTitle2,true);
					}
					else
					{
						return String.Compare(strTitle2 ,strTitle1,true);
					}
        
				case SortMethod.Artist:
					string strArtist1="";
					string strArtist2="";
					if (item1.MusicTag!=null) strArtist1=((MusicTag)item1.MusicTag).Artist;
					if (item2.MusicTag!=null) strArtist2=((MusicTag)item2.MusicTag).Artist;
					if (bAscending)
					{
						return String.Compare(strArtist1 ,strArtist2,true);
					}
					else
					{
						return String.Compare(strArtist2 ,strArtist1,true);
					}
        
				case SortMethod.Album:
					string strAlbum1="";
					string strAlbum2="";
					if (item1.MusicTag!=null) strAlbum1=((MusicTag)item1.MusicTag).Album;
					if (item2.MusicTag!=null) strAlbum2=((MusicTag)item2.MusicTag).Album;
					if (bAscending)
					{
						return String.Compare(strAlbum1 ,strAlbum2,true);
					}
					else
					{
						return String.Compare(strAlbum2 ,strAlbum1,true);
					}
          

				case SortMethod.Filename:
					string strFile1=System.IO.Path.GetFileName(item1.Path);
					string strFile2=System.IO.Path.GetFileName(item2.Path);
					if (bAscending)
					{
						return String.Compare(strFile1 ,strFile2,true);
					}
					else
					{
						return String.Compare(strFile2 ,strFile1,true);
					}
          
			} 
			return 0;
		}

	}
}
