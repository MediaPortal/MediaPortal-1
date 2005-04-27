using System;
using System.Globalization;
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for VideoSort.
	/// </summary>
	public class VideoSort : IComparer
	{
		public enum SortMethod
		{
			Name=0,
			Date=1,
			Size=2,
			Year=3,
			Rating=4,
			Label=5,
		}
		protected SortMethod currentSortMethod;
		protected bool sortAscending;

		public VideoSort(SortMethod sortMethod, bool ascending)
		{
			currentSortMethod=sortMethod;
			sortAscending=ascending;
		}
		
		public int Compare(object x, object y)
		{
			if (x == y) return 0;
			GUIListItem item1 = (GUIListItem)x;
			GUIListItem item2 = (GUIListItem)y;
			if (item1 == null) return - 1;
			if (item2 == null) return - 1;
			if (item1.IsFolder && item1.Label == "..") return - 1;
			if (item2.IsFolder && item2.Label == "..") return - 1;
			if (item1.IsFolder && !item2.IsFolder) return - 1;
			else if (!item1.IsFolder && item2.IsFolder) return 1;


			switch (currentSortMethod)
			{
				case SortMethod.Year : 
				{
					if (sortAscending)
					{
						if (item1.Year > item2.Year) return 1;
						if (item1.Year < item2.Year) return - 1;
					}
					else
					{
						if (item1.Year > item2.Year) return - 1;
						if (item1.Year < item2.Year) return 1;
					}
					return 0;
				}
				case SortMethod.Rating : 
				{
					if (sortAscending)
					{
						if (item1.Rating > item2.Rating) return 1;
						if (item1.Rating < item2.Rating) return - 1;
					}
					else
					{
						if (item1.Rating > item2.Rating) return - 1;
						if (item1.Rating < item2.Rating) return 1;
					}
					return 0;
				}

				case SortMethod.Name: 
          
					if (sortAscending)
					{
						return String.Compare(item1.Label, item2.Label, true);
					}
					else
					{
						return String.Compare(item2.Label, item1.Label, true);
					}
        
				case SortMethod.Label : 
					if (sortAscending)
					{
						return String.Compare(item1.DVDLabel, item2.DVDLabel, true);
					}
					else
					{
						return String.Compare(item2.DVDLabel, item1.DVDLabel, true);
					}
				case SortMethod.Size:
					if (item1.FileInfo==null) return -1;
					if (item2.FileInfo==null) return -1;
					if (sortAscending)
					{
						return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
					}
					else
					{
						return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
					}

        

				case SortMethod.Date:
					if (item1.FileInfo==null) return -1;
					if (item2.FileInfo==null) return -1;
          
					item1.Label2 =item1.FileInfo.CreationTime.ToShortDateString() + " "+item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					item2.Label2 =item2.FileInfo.CreationTime.ToShortDateString() + " "+item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					if (sortAscending)
					{
						return DateTime.Compare(item1.FileInfo.CreationTime,item2.FileInfo.CreationTime);
					}
					else
					{
						return DateTime.Compare(item2.FileInfo.CreationTime,item1.FileInfo.CreationTime);
					}


			} 
			return 0;
		}

	}
}
