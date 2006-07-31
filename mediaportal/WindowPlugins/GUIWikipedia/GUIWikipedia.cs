#region Copyright (C) 2006 Team MediaPortal

/* 
 *      Copyright (C) 2006 Team MediaPortal
 *      http://www.team-mediaportal.com
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

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.Utils.Services;

namespace Wikipedia
{
	/// <summary>
	/// Windowplugin to search in Wikipedia and display articles using the MP Wikipedia Classes.
	/// </summary>
	public class GUIWikipedia : GUIWindow, ISetupForm
	{
		[SkinControlAttribute(10)]			protected GUIButtonControl buttonSearch=null;
		[SkinControlAttribute(11)]			protected GUIButtonControl buttonLocal=null;

		[SkinControlAttribute(12)]			protected GUIButtonControl buttonLinks=null;
		[SkinControlAttribute(13)]			protected GUIButtonControl buttonImages=null;

		[SkinControlAttribute(4)]			  protected GUILabelControl  searchtermLabel=null;
		[SkinControlAttribute(20)]			protected GUITextControl   txtArticle=null;

    private string language = "Default";
    private string articletext = string.Empty;
    private ArrayList linkArray = new ArrayList();
    private ArrayList imageArray = new ArrayList();

    private ILog _wikilog;

    public GUIWikipedia()
		{
      ServiceProvider services = GlobalServiceProvider.Instance;
      _wikilog = services.Get<ILog>();
		}
		#region ISetupForm Members

		// Returns the name of the plugin which is shown in the plugin menu
		public string PluginName()
		{
			return "Wikipedia";
		}

		// Returns the description of the plugin is shown in the plugin menu
		public string Description()
		{
			return "A Plugin to search in Wikipedia";
		}

		// Returns the author of the plugin which is shown in the plugin menu
		public string Author()      
		{
			return "Maschine";
		}	
		
		// show the setup dialog
		public void   ShowPlugin()  
		{
			MessageBox.Show("Nothing to configure yet...");
		}	

		// Indicates whether plugin can be enabled/disabled
		public bool   CanEnable()   
		{
			return true;
		}	

		// get ID of windowplugin belonging to this setup
		public int    GetWindowId() 
		{
			return 4711;
		}	
		
		// Indicates if plugin is enabled by default;
		public bool   DefaultEnabled()
		{
			return false;
		}	

		// indicates if a plugin has its own setup screen
		public bool   HasSetup()    
		{
			return false;
		}    
	
		/// <summary>
		/// If the plugin should have its own button on the main menu of Media Portal then it
		/// should return true to this method, otherwise if it should not be on home
		/// it should return false
		/// </summary>
		/// <param name="strButtonText">text the button should have</param>
		/// <param name="strButtonImage">image for the button, or empty for default</param>
		/// <param name="strButtonImageFocus">image for the button, or empty for default</param>
		/// <param name="strPictureImage">subpicture for the button or empty for none</param>
		/// <returns>true  : plugin needs its own button on home
		///          false : plugin does not need its own button on home</returns>
		public bool   GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage) 
		{
			strButtonText = PluginName();
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "";
			return true;
		}
		#endregion
		
		public override int GetID
		{
			get
			{
				return 4711;
			}
			set
			{
			}
		}

		public override bool Init()
		{
			// extract our assets to the current skin folder if they don't already exist
			if(File.Exists(GUIGraphicsContext.Skin + @"\wikipedia.xml") == false)
				Utils.ExportEmbeddedResource("Wikipedia.Assets.wikipedia.xml", GUIGraphicsContext.Skin + @"\wikipedia.xml");

			if(File.Exists(GUIGraphicsContext.Skin + @"\media\wikipedia_logo.png") == false)
				Utils.ExportEmbeddedResource("Wikipedia.Assets.wikipedia_logo.png", GUIGraphicsContext.Skin + @"\media\wikipedia_logo.png");

			return Load(GUIGraphicsContext.Skin+@"\wikipedia.xml");
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			// we don't want the user to start another search while one is already active
			if(_workerCompleted == false)
				return;

			// Here we want to open the OSD Keyboard to enter the searchstring
			if (control==buttonSearch) 
			{	
				// If the search Button was clicked we need to bring up the search keyboard.
				VirtualSearchKeyboard keyBoard=(VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);
        string searchterm = string.Empty;    
				keyBoard.Reset();
				keyBoard.Text = "";
				keyBoard.DoModal(GetID); // show it...

        _wikilog.Info("Wikipedia: OSD keyboard loaded!");

				// If input is finished, the string is saved to the searchterm var.
				if (keyBoard.IsConfirmed)
					searchterm = keyBoard.Text;
				
				// If there was a string entered try getting the article.
        if (searchterm != "")
        {
          _wikilog.Info("Wikipedia: Searchterm gotten from OSD keyboard: {0}", searchterm);
          GetAndDisplayArticle(searchterm);
        }
        // Else display an error dialog.
        else
        {
          GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
          dlg.SetHeading("Error");
          dlg.SetLine(1, GUILocalizeStrings.Get(2500)); // No searchterm entered!
          dlg.SetLine(2, String.Empty);
          dlg.SetLine(3, GUILocalizeStrings.Get(2501)); // Please enter a valid searchterm!
          dlg.DoModal(GUIWindowManager.ActiveWindow);
        }
			}
			// This is the control to select the local Wikipedia site.
			if (control==buttonLocal)
			{
				// Create a new selection dialog.
				GUIDialogMenu pDlgOK = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (pDlgOK!=null)
				{	
					pDlgOK.Reset();
					pDlgOK.SetHeading(GUILocalizeStrings.Get(2502)); //Select your local Wikipedia:
					
					// Add all the local sites we want to be displayed starting with int 0.
          for (int i = 0; i <= 5; i++)
          {
            pDlgOK.Add(GUILocalizeStrings.Get(2600 + i)); //English, German, French ...
          }

					pDlgOK.DoModal(GetID);
					if (pDlgOK.SelectedLabel>=0)
					{
						SelectLocalWikipedia(pDlgOK.SelectedLabel);
					}
				}
			}
			// The Button holding the Links to other articles
			if (control==buttonLinks)
			{
        if (linkArray.Count > 0)
        {
          // Create a new selection dialog.
          GUIDialogMenu pDlgOK = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (pDlgOK != null)
          {
            pDlgOK.Reset();
            pDlgOK.SetHeading(GUILocalizeStrings.Get(2505)); //Links to other articles:

            // Add all the links from the linkarray.
            foreach (string link in linkArray)
            {
              pDlgOK.Add(link);
            }
            pDlgOK.DoModal(GetID);
            if (pDlgOK.SelectedLabel >= 0)
            {
              _wikilog.Info("Wikipedia: new search from the links array: {0}", pDlgOK.SelectedLabelText);
              GetAndDisplayArticle(pDlgOK.SelectedLabelText);
            }
          }
        }
        else
        {
          GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
          dlg.SetHeading("Error");
          dlg.SetLine(1, GUILocalizeStrings.Get(2506)); // No Links from this article.
          dlg.DoModal(GUIWindowManager.ActiveWindow);
        }

			}
			// The Button containing a list of all images from the article
      if (control == buttonImages)
      {
        if (imageArray.Count > 0)
        {
          // Create a new selection dialog.
          GUIDialogMenu pDlgOK = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (pDlgOK != null)
          {
            pDlgOK.Reset();
            pDlgOK.SetHeading(GUILocalizeStrings.Get(2508)); //Images from this article

            // Add all the images from the imagearray.
            foreach (string image in imageArray)
            {
              pDlgOK.Add(image);
            }
            pDlgOK.DoModal(GetID);
            if (pDlgOK.SelectedLabel >= 0)
            {
              _wikilog.Info("Wikipedia: new search from the image array: {0}", pDlgOK.SelectedLabelText);
              //TODO: get images
              //GetAndDisplayArticle(pDlgOK.SelectedLabelText);
            }
          }
        }
        else
        {
          GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
          dlg.SetHeading("Error");
          dlg.SetLine(1, GUILocalizeStrings.Get(2508)); // No Images referenced in this article.
          dlg.DoModal(GUIWindowManager.ActiveWindow);
        }
      }
			base.OnClicked (controlId, control, actionType);
		}

		// Depending on which Entry was selected from the listbox we chose the language here.
		private void SelectLocalWikipedia(int labelnumber)
		{
			switch (labelnumber)
			{
				case 0: //English
          language = "English";
          break;
				case 1: //German
          language = "German";
					break;
				case 2: //French
          language = "French";
          break;
        case 3: //Dutch
          language = "Dutch";
          break;
        case 4: //Norwegian
          language = "Norwegian";
          break;
        case 5: //Swedish
          language = "Swedish";
          break;
      }
      if (searchtermLabel.Label != string.Empty)
      {
        _wikilog.Info("Wikipedia: language changed to {0}. Display article {1} again.", language, searchtermLabel.Label);
        GetAndDisplayArticle(searchtermLabel.Label);
      }
    }
		
		// The main function.
		void GetAndDisplayArticle(string searchterm)
		{	
			BackgroundWorker worker = new BackgroundWorker();

			worker.DoWork += new DoWorkEventHandler(DownloadWorker);
			worker.RunWorkerAsync(searchterm);

			while(_workerCompleted == false)
				GUIWindowManager.Process();
		}

    // All kind of stuff because of the wait cursor ;-)
		void DownloadWorker(object sender, DoWorkEventArgs e)
		{
			_workerCompleted = false;

			using(WaitCursor cursor = new WaitCursor())
			lock(this)
			{
				linkArray.Clear();
				imageArray.Clear();
				searchtermLabel.Label = e.Argument.ToString();
        WikipediaArticle article = new WikipediaArticle(e.Argument.ToString(), language);
        articletext = article.GetArticleText();
        linkArray = article.GetLinkArray();
        imageArray = article.GetImageArray();

        if(articletext == "REDIRECT")
          txtArticle.Label = GUILocalizeStrings.Get(2509) + "\n" + GUILocalizeStrings.Get(2510); //This page is only a redirect. Please chose the redirect aim from the link list.
        else if (articletext == string.Empty)
          txtArticle.Label = GUILocalizeStrings.Get(2504); //Sorry, no Article was found for your searchterm...
        else
          txtArticle.Label = articletext;
			}

			_workerCompleted = true;
		}

		volatile bool _workerCompleted = true;
	}
}
