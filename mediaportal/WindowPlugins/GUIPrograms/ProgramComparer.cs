using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace GUIPrograms
{
	/// <summary>
	/// Summary description for DirBrowseComparer.
	/// </summary>
	public class DirBrowseComparer: IComparer
	{

		private enum SortMethod
		{
			SORT_NAME=0,
			SORT_DATE=1,
			SORT_SIZE=2
		}

		private SortMethod  currentSortMethod=SortMethod.SORT_NAME;
		public bool bAsc = true;
		

		public DirBrowseComparer()
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
				case DirBrowseComparer.SortMethod.SORT_NAME:
					strLine=GUILocalizeStrings.Get(103);
					break;
				case DirBrowseComparer.SortMethod.SORT_DATE:
					strLine=GUILocalizeStrings.Get(104);
					break;
				case DirBrowseComparer.SortMethod.SORT_SIZE:
					strLine=GUILocalizeStrings.Get(105);
					break;
			}
			return strLine;
		}

		public void updateState()
		{
			switch( currentSortMethod )
			{
				case SortMethod.SORT_NAME:
					currentSortMethod = SortMethod.SORT_DATE;
					bAsc = true;
					break;
				case SortMethod.SORT_DATE:
					currentSortMethod = SortMethod.SORT_SIZE;
					bAsc = true;
					break;
				case SortMethod.SORT_SIZE:
					currentSortMethod = SortMethod.SORT_NAME;
					bAsc = true;
					break;
			}
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

			switch (method)
			{
				case SortMethod.SORT_NAME:
					item1.Label2="";
					item2.Label2="";

					if (bAsc)
					{
						return String.Compare(item1.Label ,item2.Label,true);
					}
					else
					{
						return String.Compare(item2.Label ,item1.Label,true);
					}
        

				case SortMethod.SORT_DATE:
					if (item1.FileInfo==null) return -1;
					if (item2.FileInfo==null) return -1;
          
					item1.Label2 =item1.FileInfo.CreationTime.ToShortDateString() + " "+item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					item2.Label2 =item2.FileInfo.CreationTime.ToShortDateString() + " "+item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					if (bAsc)
					{
						return DateTime.Compare(item1.FileInfo.CreationTime,item2.FileInfo.CreationTime);
					}
					else
					{
						return DateTime.Compare(item2.FileInfo.CreationTime,item1.FileInfo.CreationTime);
					}

				case SortMethod.SORT_SIZE:
					if (item1.FileInfo==null) return -1;
					if (item2.FileInfo==null) return -1;
					item1.Label2=strSize1;
					item2.Label2=strSize2;
					if (bAsc)
					{
						return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
					}
					else
					{
						return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
					}
			} 
			return 0;
		}
	}
}
