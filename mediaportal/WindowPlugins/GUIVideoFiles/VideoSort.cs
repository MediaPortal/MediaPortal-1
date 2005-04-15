using System;
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


			switch (CurrentSortMethod)
			{
				case SortMethod.Year : 
				{
					if (CurrentSortAsc)
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
					if (CurrentSortAsc)
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
          
					if (CurrentSortAsc)
					{
						return String.Compare(item1.Label, item2.Label, true);
					}
					else
					{
						return String.Compare(item2.Label, item1.Label, true);
					}
        
				case SortMethod.Label : 
					if (CurrentSortAsc)
					{
						return String.Compare(item1.DVDLabel, item2.DVDLabel, true);
					}
					else
					{
						return String.Compare(item2.DVDLabel, item1.DVDLabel, true);
					}

			} 
			return 0;
		}

	}
}
