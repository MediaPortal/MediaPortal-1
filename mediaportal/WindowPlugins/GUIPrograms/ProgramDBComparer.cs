using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using ProgramsDatabase;

namespace GUIPrograms
{
	/// <summary>
	/// Summary description for ProgramDBComparer.
	/// </summary>
	public class ProgramDBComparer: IComparer
	{
		enum SortMethod
		{
			SORT_NAME=0,
			SORT_LAUNCHES=1,
			SORT_RECENT=2,
			SORT_RATING=3
		}

		SortMethod  currentSortMethod=SortMethod.SORT_NAME;
		public bool bAsc = true;
		

		public ProgramDBComparer()
		{
		}

		public string currentSortMethodAsText
		{
			get {return GetCurrentSortMethodAsText();}
		}

		private string GetCurrentSortMethodAsText()
		{
			string strLine = "";
			SortMethod method = currentSortMethod;
			switch (method)
			{
				case SortMethod.SORT_NAME:
					strLine=GUILocalizeStrings.Get(103);
					break;
				case SortMethod.SORT_LAUNCHES:
					strLine = "Sort by: Launches";  // todo: localize
					break;
				case SortMethod.SORT_RECENT:
					strLine = "Sort by: Recent";  // todo: localize
					break;
				case SortMethod.SORT_RATING:
					strLine = "Sort by: Rating";  // todo: localize
					break;
			}
			return strLine;
		}

		public void updateState()
		{
			switch( currentSortMethod )
			{
					//			SORT_NAME=0,
					//			SORT_LAUNCHES=1,
					//			SORT_RECENT=2,
					//			SORT_RATING=3
				
				case SortMethod.SORT_NAME:
					currentSortMethod = SortMethod.SORT_LAUNCHES;
					bAsc = false;
					break;
				case SortMethod.SORT_LAUNCHES:
					currentSortMethod = SortMethod.SORT_RECENT;
					bAsc = false;
					break;
				case SortMethod.SORT_RECENT:
					currentSortMethod = SortMethod.SORT_RATING;
					bAsc = false;
					break;
				case SortMethod.SORT_RATING:
					currentSortMethod = SortMethod.SORT_NAME;
					bAsc = true;
					break;
			}
		}

		public int Compare(object x, object y)
		{
			FileItem curFile1 = null;
			FileItem curFile2 = null;
			if (x==y) return 0;
			GUIListItem item1=(GUIListItem)x;
			GUIListItem item2=(GUIListItem)y;
			if (item1==null) return -1;
			if (item2==null) return -1;
			if (item1.MusicTag == null) return -1;
			if (item2.MusicTag == null) return -1;
			if (item1.IsFolder && item1.Label=="..") return -1;
			if (item2.IsFolder && item2.Label=="..") return -1;
			if (item1.IsFolder && !item2.IsFolder) return -1;
			else if (!item1.IsFolder && item2.IsFolder) return 1; 


			if (currentSortMethod != SortMethod.SORT_NAME)
			{ 
				curFile1 = (FileItem)item1.MusicTag; 
				curFile2 = (FileItem)item2.MusicTag;
				if (curFile1==null) return -1;
				if (curFile2==null) return -1;
			}

			// ok let's start sorting :-)
			int nTemp;
			switch (currentSortMethod)
			{
				case SortMethod.SORT_NAME:
					item1.Label2 = "";
					item2.Label2 = "";
					if (bAsc)
					{
						return String.Compare(item1.Label ,item2.Label,true);
					}
					else
					{
						return String.Compare(item2.Label ,item1.Label,true);
					}
				case SortMethod.SORT_LAUNCHES:
					item1.Label2 = String.Format("{0}", curFile1.LaunchCount);
					item2.Label2 = String.Format("{0}", curFile2.LaunchCount);
					if (curFile1.LaunchCount == curFile2.LaunchCount)
					{
						// second sort always title ASC
						return String.Compare(item1.Label, item2.Label, true);
					}
					else
					{
						if (curFile1.LaunchCount < curFile2.LaunchCount)
						{
							nTemp = -1;
						}
						else
						{
							nTemp = 1;
						}
						if (bAsc)
						{
							return nTemp;
						}
						else
						{
							return -nTemp;
						}
					}

				case SortMethod.SORT_RATING:
					item1.Label2 = String.Format("{0}", curFile1.Rating);
					item2.Label2 = String.Format("{0}", curFile2.Rating);
					if (curFile1.Rating == curFile2.Rating)
					{
						// second sort always title ASC
						return String.Compare(item1.Label, item2.Label, true);
					}
					else
					{
						if (curFile1.Rating < curFile2.Rating)
						{
							nTemp = -1;
						}
						else
						{
							nTemp = 1;
						}
						if (bAsc)
						{
							return nTemp;
						}
						else
						{
							return -nTemp;
						}
					}

				case SortMethod.SORT_RECENT:
					if (curFile1.LastTimeLaunched > DateTime.MinValue)
					{
//					item1.Label2 = String.Format("{0}", curFile1.LastTimeLaunched);
					item1.Label2 = curFile1.LastTimeLaunched.ToShortDateString();
					}
					else
					{
						item1.Label2 = "";
					}
					if (curFile2.LastTimeLaunched > DateTime.MinValue)
					{
//						item2.Label2 = String.Format("{0}", curFile2.LastTimeLaunched);
						item2.Label2 = curFile1.LastTimeLaunched.ToShortDateString();
					}
					else
					{
						item2.Label2 = "";
					}

					if (curFile1.LastTimeLaunched == curFile2.LastTimeLaunched)
					{
						// second sort always title ASC
						return String.Compare(item1.Label, item2.Label, true);
					}
					else
					{
						if (curFile1.LastTimeLaunched < curFile2.LastTimeLaunched)
						{
							nTemp = -1;
						}
						else
						{
							nTemp = 1;
						}
						if (bAsc)
						{
							return nTemp;
						}
						else
						{
							return -nTemp;
						}
					}
			}



//			string strSize1="";
//			string strSize2="";
//			if (item1.FileInfo!=null) strSize1=Utils.GetSize(item1.FileInfo.Length);
//			if (item2.FileInfo!=null) strSize2=Utils.GetSize(item2.FileInfo.Length);
//
//			SortMethod method=currentSortMethod;
//
//			switch (method)
//			{
//				case SortMethod.SORT_NAME:
//					item1.Label2="";
//					item2.Label2="";
//
//					if (bAsc)
//					{
//						return String.Compare(item1.Label ,item2.Label,true);
//					}
//					else
//					{
//						return String.Compare(item2.Label ,item1.Label,true);
//					}
//        
//
//				case SortMethod.SORT_DATE:
//					if (item1.FileInfo==null) return -1;
//					if (item2.FileInfo==null) return -1;
//          
//					item1.Label2 =item1.FileInfo.CreationTime.ToShortDateString() + " "+item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
//					item2.Label2 =item2.FileInfo.CreationTime.ToShortDateString() + " "+item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
//					if (bAsc)
//					{
//						return DateTime.Compare(item1.FileInfo.CreationTime,item2.FileInfo.CreationTime);
//					}
//					else
//					{
//						return DateTime.Compare(item2.FileInfo.CreationTime,item1.FileInfo.CreationTime);
//					}
//
//				case SortMethod.SORT_SIZE:
//					if (item1.FileInfo==null) return -1;
//					if (item2.FileInfo==null) return -1;
//					item1.Label2=strSize1;
//					item2.Label2=strSize2;
//					if (bAsc)
//					{
//						return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
//					}
//					else
//					{
//						return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
//					}
//			} 
			return 0;
		}
		
	}
}
