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

#region Usings
using System;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;

#endregion

namespace GUIRecipies
{
	/// <summary>
	/// Summary description for GUIRecipies.
	/// </summary>
	public class GUIRecipies : GUIWindow
	{
		#region Private Enumerations
		enum Controls
		{
			CONTROL_BACKBUTTON	= 2,
			CONTROL_SEARCH_TYP	= 3,
			CONTROL_SEARCH_TOG	= 4,
			CONTROL_SEARCH		= 5,
			CONTROL_FAVOR		= 6,
			CONTROL_DELETE		= 7,
			CONTROL_PRINT		= 8,
			CONTROL_SPIN		= 9,
			CONTROL_LIST        = 10,
			CONTROL_TEXTBOX		= 11
		};
		#endregion

		#region Base variabeles

		Recipie rec = new Recipie();
		RecipiePrinter rp = new RecipiePrinter();
		string subcatstr = "";// contains actual subcategorie
		string catstr = "";   // contains actual categorie
		string titstr = "";   // contains actual title of recipie
		string seastr = "";   // contains actual search string
		bool search = false;  // was search mode the last menu?
		bool online = false;  // online recipie update?
		bool subcat = false;  // show subcategories ?

		enum States
		{
			STATE_MAIN = 0,
			STATE_CATEGORY = 1,
			STATE_RECIPIE  = 2,
			STATE_SUB = 3,
			STATE_FAVORITES = 4
		};

		enum Search_Types
		{
			SEARCH_TITLE = 0,
			SEARCH_RECIPIE = 1
		};

		private States currentState = States.STATE_MAIN;
		private Search_Types currentSearch = Search_Types.SEARCH_TITLE;

		#endregion

		#region Constructor
		public GUIRecipies() {
			//
			// TODO: Add constructor logic here
			//
		}

		#endregion

		#region Overides
		/// <summary>
		/// Return the id of this window
		/// </summary>
		public override int GetID {
			get { return 750; }
			set { base.GetID = value; }
		}

		/// <summary>
		/// Gets called by the runtime when a new window has been created
		/// Every window window should override this method and load itself by calling
		/// the Load() method
		/// </summary>
		/// <returns></returns>
		public override bool Init() {
			LoadSettings();
			return Load (GUIGraphicsContext.Skin+@"\myrecipies.xml");
		}

		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) {
				GUIWindowManager.ShowPreviousWindow();
				return;
			}
			if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)	{
				if(action.m_key.KeyChar == 89 || action.m_key.KeyChar == 121 ) 
				{
					GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST );
					if (item != null)
					{
						if (item.Label.Length>1) RecipieDatabase.GetInstance().AddFavorite(item.Label);
					}
				}
				return;
			}
			if (action.wID == Action.ActionType.ACTION_QUEUE_ITEM) // add recipie to favorites
			{
				if( currentState == States.STATE_RECIPIE  )	
				{
					GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST );
					if (item != null)
					{
						if (item.Label.Length>1) RecipieDatabase.GetInstance().AddFavorite(item.Label);
					}
				}
				return;
			}
			if (action.wID == Action.ActionType.ACTION_DELETE_ITEM) 
			{
				GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST );
				if (item != null)
				{
					GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
					if (null!=dlgYesNo)
					{
						titstr=item.Label;
						dlgYesNo.SetLine(1, "");
						dlgYesNo.SetLine(2, "");						
						if( currentState == States.STATE_RECIPIE || currentState == States.STATE_CATEGORY ) 
						{ 
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(2049)); 
							dlgYesNo.SetLine(1,titstr);
						} 
						if( currentState == States.STATE_MAIN )
						{
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(2050)); 
							dlgYesNo.SetLine(1,titstr);
						}
						if( currentState == States.STATE_FAVORITES ) 
						{
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(933)); 
							dlgYesNo.SetLine(1,titstr);
						}
						dlgYesNo.DoModal(GetID);

						if (dlgYesNo.IsConfirmed)
						{
							switch (currentState)
							{
								case States.STATE_FAVORITES:
								{
									RecipieDatabase.GetInstance().DeleteFavorite(titstr);
									ArrayList recipies = RecipieDatabase.GetInstance().GetRecipiesForFavorites();
									UpDateList(recipies);
									GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BACKBUTTON);
									currentState = States.STATE_FAVORITES;
									UpdateButtons();
								}
									break;
								case States.STATE_CATEGORY:
								case States.STATE_RECIPIE:
								{
									RecipieDatabase.GetInstance().DeleteRecipie(titstr);
									currentState = States.STATE_CATEGORY;
									UpdateButtons();
									ArrayList recipies = RecipieDatabase.GetInstance().GetRecipiesForCategory( catstr );
									UpDateList(recipies);
									GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BACKBUTTON);
								}
									break;
								case States.STATE_MAIN:
								{
								}
									break;
							}
						}
					}
				}
			}
			base.OnAction(action);
		}

		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{  
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
					base.OnMessage(message);
					GUISpinControl cntlYieldInterval=GetControl((int)Controls.CONTROL_SPIN) as GUISpinControl;
					if (cntlYieldInterval!=null) {
						for (int i=1; i <= 24; i++) cntlYieldInterval.AddLabel("",i);	
						cntlYieldInterval.Value=1;
					}
					LoadAllCategories();
					currentState = States.STATE_MAIN;
					UpdateButtons();
					return true;
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_SPIN) {		// Yield Calculator
						if( currentState == States.STATE_RECIPIE ) {	
							GUISpinControl cntlYieldInt=GetControl((int)Controls.CONTROL_SPIN) as GUISpinControl;
							int iInterval=(cntlYieldInt.Value)+1;
							rec.CYield=iInterval;
							ShowDetails(rec);
							UpdateButtons();
						}
					} 
					else if (iControl==(int)Controls.CONTROL_SEARCH_TYP) {  // Select type of search
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						switch (currentSearch) {
							case Search_Types.SEARCH_RECIPIE :				// search by title
								currentSearch = Search_Types.SEARCH_TITLE;
								break;
							case Search_Types.SEARCH_TITLE:					// search by recipie
								currentSearch = Search_Types.SEARCH_RECIPIE;
								break;
						}
						UpdateButtons();
						GUIControl.FocusControl(GetID,iControl);
					}
					else if ( iControl==(int)Controls.CONTROL_LIST )		// Click on Control_List ?
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						int iItem=(int)msg.Param1;
						int iAction=(int)message.Param1;
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM && iItem!=-1) 
						{
							GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST );
							if (currentState == States.STATE_CATEGORY || currentState == States.STATE_FAVORITES)	// show recipie details
							{
								if (item.Label!=GUILocalizeStrings.Get(2054)) { 
									rec = RecipieDatabase.GetInstance().GetRecipie( item.Label );
									titstr=item.Label;
									GUISpinControl cntlYieldInt=GetControl((int)Controls.CONTROL_SPIN) as GUISpinControl;
									cntlYieldInt.Value=rec.CYield;
									ShowDetails(rec);
									GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BACKBUTTON);
									currentState = States.STATE_RECIPIE;
									UpdateButtons();
								}
							}
							else if ( currentState == States.STATE_MAIN ) {		// show category
								// show list of items
								ArrayList recipies;
								if (subcat==true) {
									recipies = RecipieDatabase.GetInstance().GetSubsForCategory( item.Label );
									currentState = States.STATE_SUB;
									subcatstr=item.Label;
								} else {
									recipies = RecipieDatabase.GetInstance().GetRecipiesForCategory( item.Label );
									currentState = States.STATE_CATEGORY;
									catstr=item.Label;
								}
								UpDateList(recipies);
								GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BACKBUTTON);
								UpdateButtons();
							}
							else if ( currentState == States.STATE_SUB ) {		// show category
								// show list of items
								ArrayList recipies = RecipieDatabase.GetInstance().GetRecipiesForCategory( item.Label );
								currentState = States.STATE_CATEGORY;
								catstr=item.Label;
								UpDateList(recipies);
								GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BACKBUTTON);
								UpdateButtons();
							}
						}
					}
					else if( iControl == (int) Controls.CONTROL_BACKBUTTON ) { // click on Backbutton
						if(currentState == States.STATE_RECIPIE) { // back from recipie detail
							currentState = States.STATE_CATEGORY;
							UpdateButtons();
							if (search==true) {
								byte styp=0;
								if (currentSearch == Search_Types.SEARCH_RECIPIE) styp=0;
								if (currentSearch == Search_Types.SEARCH_TITLE) styp=1;
								ArrayList recipies = RecipieDatabase.GetInstance().SearchRecipies(seastr,styp);
								UpDateList(recipies);
							} else {
								ArrayList recipies = RecipieDatabase.GetInstance().GetRecipiesForCategory( catstr );
								UpDateList(recipies);
							}
							GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BACKBUTTON);
						} else if(currentState == States.STATE_CATEGORY || currentState == States.STATE_FAVORITES) {
							search = false;
							if (subcat==true) {
								currentState = States.STATE_SUB;
								// show list of items
								ArrayList recipies = RecipieDatabase.GetInstance().GetSubsForCategory( subcatstr);
								UpDateList(recipies);
								GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BACKBUTTON);
								UpdateButtons();
							} else {
								currentState = States.STATE_MAIN;
								LoadAllCategories();
							}
							UpdateButtons();
							GUIControl.FocusControl(GetID, (int)Controls.CONTROL_LIST);
						} else if(currentState == States.STATE_SUB) {
							currentState = States.STATE_MAIN;
							LoadAllCategories();
							UpdateButtons();
							GUIControl.FocusControl(GetID, (int)Controls.CONTROL_LIST);
						}
					}
					else if( iControl == (int) Controls.CONTROL_SEARCH )		// click on Search Button
					{
						int activeWindow=(int)GUIWindowManager.ActiveWindow;
						VirtualSearchKeyboard keyBoard=(VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);
						keyBoard.Text = "";
						keyBoard.Reset();
						keyBoard.TextChanged+=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged); // add the event handler
						keyBoard.DoModal(activeWindow); // show it...
						keyBoard.TextChanged-=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged);	// remove the handler			
						seastr = keyBoard.Text;
						GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST );
						byte styp=0;
						if (currentSearch == Search_Types.SEARCH_RECIPIE) styp=0;
						if (currentSearch == Search_Types.SEARCH_TITLE) styp=1;
						search = true;
						ArrayList recipies = RecipieDatabase.GetInstance().SearchRecipies(seastr,styp);
						UpDateList(recipies);
						currentState = States.STATE_CATEGORY;
						UpdateButtons();
					}
					else if( iControl == (int) Controls.CONTROL_FAVOR )			// click on Favorites
					{
						// show list of items
						GUIListItem item = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST );
						ArrayList recipies = RecipieDatabase.GetInstance().GetRecipiesForFavorites();
						UpDateList(recipies);
						GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BACKBUTTON);
						currentState = States.STATE_FAVORITES;
						UpdateButtons();
					}
					else if( iControl == (int) Controls.CONTROL_DELETE )		// click on delete button
					{
						Action action = new Action(Action.ActionType.ACTION_DELETE_ITEM, 0, 0);
						OnAction(action);
					}					
					else if( iControl == (int) Controls.CONTROL_PRINT ) {		// click on Print button
						if( currentState == States.STATE_RECIPIE ) {
							rp.printRecipie(rec,catstr,titstr);
							GUIControl.FocusControl(GetID, (int)Controls.CONTROL_BACKBUTTON);
						}
					}
					break;
			}
			return base.OnMessage (message);
		}
		#endregion

		#region Private Methods
		//loads list control with new values
		void UpDateList(ArrayList recipies) {
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST );
			if (recipies.Count>0) {
				foreach( Recipie r in recipies ) {
					GUIListItem gli = new GUIListItem( r.Title );
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST, gli );
				}
			} else {
				GUIListItem gli = new GUIListItem(GUILocalizeStrings.Get(2054));
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST, gli );
			}
			string strObjects=String.Format("{0} {1}", recipies.Count, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
		}


		void UpdateButtons()
		{
		    string strLine="";
			switch (currentSearch)
			{
				case Search_Types.SEARCH_RECIPIE:
					strLine=GUILocalizeStrings.Get(2052);
					break;
				case Search_Types.SEARCH_TITLE:
					strLine=GUILocalizeStrings.Get(2051);
					break;
			}
			GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_SEARCH_TYP,strLine);
			switch (currentState)
			{
				case States.STATE_MAIN :
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_TEXTBOX );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_SPIN );
					GUIControl.HideControl( GetID, (int) Controls.CONTROL_TEXTBOX );
					GUIControl.HideControl( GetID, (int) Controls.CONTROL_SPIN);
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_LIST );
					GUIControl.ShowControl( GetID, (int) Controls.CONTROL_LIST );
					GUIControl.FocusControl(GetID, (int) Controls.CONTROL_LIST );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_DELETE );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_BACKBUTTON);
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_PRINT);
					break;
				case States.STATE_FAVORITES:
				case States.STATE_CATEGORY :
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_TEXTBOX );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_SPIN );
					GUIControl.HideControl( GetID, (int) Controls.CONTROL_TEXTBOX );
					GUIControl.HideControl( GetID, (int) Controls.CONTROL_SPIN);
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_LIST );
					GUIControl.ShowControl( GetID, (int) Controls.CONTROL_LIST );
					GUIControl.FocusControl(GetID, (int) Controls.CONTROL_LIST );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_DELETE );
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_BACKBUTTON );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_PRINT);
					break;
				case States.STATE_SUB :
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_TEXTBOX );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_SPIN );
					GUIControl.HideControl( GetID, (int) Controls.CONTROL_TEXTBOX );
					GUIControl.HideControl( GetID, (int) Controls.CONTROL_SPIN);
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_LIST );
					GUIControl.ShowControl( GetID, (int) Controls.CONTROL_LIST );
					GUIControl.FocusControl(GetID, (int) Controls.CONTROL_LIST );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_DELETE );
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_BACKBUTTON );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_PRINT);
					break;
				case States.STATE_RECIPIE :
					GUIControl.HideControl( GetID, (int) Controls.CONTROL_LIST );
					GUIControl.DisableControl( GetID, (int) Controls.CONTROL_LIST );
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_BACKBUTTON );
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_TEXTBOX );
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_SPIN );
					GUIControl.ShowControl( GetID, (int) Controls.CONTROL_TEXTBOX );
					GUIControl.ShowControl( GetID, (int) Controls.CONTROL_SPIN );			
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_DELETE );
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_BACKBUTTON );
					GUIControl.EnableControl( GetID, (int) Controls.CONTROL_PRINT);
					break;
			}
		}

		/// <summary>
		/// Loads my status settings from the profile xml.
		/// </summary>
		/// 
		private void LoadSettings() {
			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml")) {
				subcat = xmlreader.GetValueAsBool("recipie","subcats",false);
				online = xmlreader.GetValueAsBool("recipie","online",false);
			}
		}

		private void ShowDetails(Recipie rec) // show recipie directions
		{
			GUITextControl control = (GUITextControl)this.GetControl((int)Controls.CONTROL_TEXTBOX);
			control.OnMessage(new GUIMessage( GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID, 0, (int)Controls.CONTROL_TEXTBOX, 0, 0, null ) ); 

			GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_TEXTBOX, rec.ToString() );
			GUIPropertyManager.SetProperty("#itemcount"," ");
		}
		
		void keyboard_TextChanged(int kindOfSearch,string data)
		{
			//
		}
		
		private void LoadAllCategories()	// show all categories 
		{
			ArrayList recipies;
			GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST ); 
			if (subcat==true) {
				recipies = RecipieDatabase.GetInstance().GetMainCategories();
			} else {
				recipies = RecipieDatabase.GetInstance().GetCategories();
			}
			foreach( string cat in recipies )
			{
				GUIListItem gli = new GUIListItem( cat );
				GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST, gli );
			}
            string strObjects=String.Format("{0} {1}", recipies.Count, GUILocalizeStrings.Get(632));
            GUIPropertyManager.SetProperty("#itemcount",strObjects);
		}
		#endregion

	}
}
