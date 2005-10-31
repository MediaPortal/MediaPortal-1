#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

namespace System.Windows.Input
{
	public sealed class NavigationCommands
	{
		#region Constructors

		static NavigationCommands()
		{
			BrowseBack = new UICommand("BrowseBack", typeof(NavigationCommands));
			BrowseForward = new UICommand("BrowseForward", typeof(NavigationCommands));
			BrowseHome = new UICommand("BrowseHome", typeof(NavigationCommands));
			BrowseStop = new UICommand("BrowseStop", typeof(NavigationCommands));
			DecreaseZoom = new UICommand("DecreaseZoom", typeof(NavigationCommands));
			Favorites = new UICommand("Favorites", typeof(NavigationCommands));
			FirstPage = new UICommand("FirstPage", typeof(NavigationCommands));
			GoToPage = new UICommand("GoToPage", typeof(NavigationCommands));
			IncreaseZoom = new UICommand("IncreaseZoom", typeof(NavigationCommands));
			LastPage = new UICommand("LastPage", typeof(NavigationCommands));
			NextPage = new UICommand("NextPage", typeof(NavigationCommands));
			PreviousPage = new UICommand("PreviousPage", typeof(NavigationCommands));
			Refresh = new UICommand("Refresh", typeof(NavigationCommands));
			Search = new UICommand("Search", typeof(NavigationCommands));
			Zoom = new UICommand("Zoom", typeof(NavigationCommands));
		}

		private NavigationCommands()
		{
		}

		#endregion Constructors

		#region Fields
		
		public static readonly UICommand BrowseBack;
		public static readonly UICommand BrowseForward;
		public static readonly UICommand BrowseHome;
		public static readonly UICommand BrowseStop;
		public static readonly UICommand DecreaseZoom;
		public static readonly UICommand Favorites;
		public static readonly UICommand FirstPage;
		public static readonly UICommand GoToPage;
		public static readonly UICommand IncreaseZoom;
		public static readonly UICommand LastPage;
		public static readonly UICommand NextPage;
		public static readonly UICommand PreviousPage;
		public static readonly UICommand Refresh;
		public static readonly UICommand Search;
		public static readonly UICommand Zoom;

		#endregion Fields
	}
}
