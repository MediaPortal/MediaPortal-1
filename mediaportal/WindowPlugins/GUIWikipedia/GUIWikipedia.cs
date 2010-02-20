#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace Wikipedia
{
  /// <summary>
  /// Windowplugin to search in Wikipedia and display articles using the MP Wikipedia Classes.
  /// </summary>
  [PluginIcons("WindowPlugins.GUIWikipedia.Wikipedia.gif", "WindowPlugins.GUIWikipedia.WikipediaDisabled.gif")]
  public class GUIWikipedia : GUIInternalWindow, ISetupForm, IShowPlugin
  {
    #region SkinControls

    [SkinControl(10)] protected GUIButtonControl buttonSearch = null;
    [SkinControl(11)] protected GUIButtonControl buttonLocal = null;
    [SkinControl(14)] protected GUIButtonControl buttonBack = null;
    [SkinControl(12)] protected GUIButtonControl buttonLinks = null;
    [SkinControl(13)] protected GUIButtonControl buttonImages = null;

    [SkinControl(4)] protected GUILabelControl searchtermLabel = null;
    [SkinControl(5)] protected GUILabelControl imagedescLabel = null;
    [SkinControl(20)] protected GUITextControl txtArticle = null;

    [SkinControl(25)] protected GUIImage imageControl = null;

    #endregion

    private string language = "Default";
    private string articletext = string.Empty;
    private ArrayList linkArray = new ArrayList();
    private ArrayList imagenameArray = new ArrayList();
    private ArrayList imagedescArray = new ArrayList();


    public GUIWikipedia()
    {
      GetID = (int)Window.WINDOW_WIKIPEDIA;
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
    public void ShowPlugin()
    {
      MessageBox.Show("Edit the wikipedia.xml file in MP's root directory to add new sites.");
    }

    // Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return true;
    }

    // get ID of windowplugin belonging to this setup
    public int GetWindowId()
    {
      return 4711;
    }

    // Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return false;
    }

    // indicates if a plugin has its own setup screen
    public bool HasSetup()
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
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(2516);
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = @"hover_wikipedia.png";
      return true;
    }

    #endregion

    #region IShowPlugin Member

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wikipedia.xml");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      // we don't want the user to start another search while one is already active
      if (_workerCompleted == false)
      {
        return;
      }

      // Here we want to open the OSD Keyboard to enter the searchstring
      if (control == buttonSearch)
      {
        // If the search Button was clicked we need to bring up the search keyboard.
        VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
        if (null == keyboard)
        {
          return;
        }
        string searchterm = string.Empty;
        //keyboard.IsSearchKeyboard = true;
        keyboard.Reset();
        keyboard.Text = "";
        keyboard.DoModal(GetID); // show it...

        Log.Info("Wikipedia: OSD keyboard loaded!");

        // If input is finished, the string is saved to the searchterm var.
        if (keyboard.IsConfirmed)
        {
          searchterm = keyboard.Text;
        }

        // If there was a string entered try getting the article.
        if (searchterm != "")
        {
          Log.Info("Wikipedia: Searchterm gotten from OSD keyboard: {0}", searchterm);
          GetAndDisplayArticle(searchterm);
        }
          // Else display an error dialog.
        else
        {
          GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlg.SetHeading(GUILocalizeStrings.Get(257)); // Error
          dlg.SetLine(1, GUILocalizeStrings.Get(2500)); // No searchterm entered!
          dlg.SetLine(2, string.Empty);
          dlg.SetLine(3, GUILocalizeStrings.Get(2501)); // Please enter a valid searchterm!
          dlg.DoModal(GUIWindowManager.ActiveWindow);
        }
      }
      // This is the control to select the local Wikipedia site.
      if (control == buttonLocal)
      {
        // Create a new selection dialog.
        GUIDialogMenu pDlgOK = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
        if (pDlgOK != null)
        {
          pDlgOK.Reset();
          pDlgOK.SetHeading(GUILocalizeStrings.Get(2502)); //Select your local Wikipedia:

          // Add all the local sites we want to be displayed starting with int 0.
          Settings langreader = new Settings(Config.GetFile(Config.Dir.Config, "wikipedia.xml"));
          String allsites = langreader.GetValueAsString("Allsites", "sitenames", "");
          Log.Info("Wikipedia: available sites: " + allsites);
          String[] siteArray = allsites.Split(',');
          for (int i = 0; i < siteArray.Length; i++)
          {
            int stringno = langreader.GetValueAsInt(siteArray[i], "string", 2006);
            pDlgOK.Add(GUILocalizeStrings.Get(stringno)); //English, German, French ...
          }

          pDlgOK.DoModal(GetID);
          if (pDlgOK.SelectedLabel >= 0)
          {
            SelectLocalWikipedia(pDlgOK.SelectedLabel, siteArray);
          }
        }
      }
      // The Button holding the Links to other articles
      if (control == buttonLinks)
      {
        if (linkArray.Count > 0)
        {
          // Create a new selection dialog.
          GUIDialogMenu pDlgOK = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
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
              Log.Info("Wikipedia: new search from the links array: {0}", pDlgOK.SelectedLabelText);
              GetAndDisplayArticle(pDlgOK.SelectedLabelText);
            }
          }
        }
        else
        {
          GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlg.SetHeading(GUILocalizeStrings.Get(257)); // Error
          dlg.SetLine(1, GUILocalizeStrings.Get(2506)); // No Links from this article.
          dlg.DoModal(GUIWindowManager.ActiveWindow);
        }
      }
      // The Button containing a list of all images from the article
      if (control == buttonImages)
      {
        if (imagedescArray.Count > 0)
        {
          // Create a new selection dialog.
          GUIDialogMenu pDlgOK = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
          if (pDlgOK != null)
          {
            pDlgOK.Reset();
            pDlgOK.SetHeading(GUILocalizeStrings.Get(2507)); //Images from this article

            // Add all the images from the imagearray.
            foreach (string image in imagedescArray)
            {
              pDlgOK.Add(image);
            }
            pDlgOK.DoModal(GetID);
            if (pDlgOK.SelectedLabel >= 0)
            {
              Log.Info("Wikipedia: new search from the image array: {0}", imagedescArray[pDlgOK.SelectedId - 1]);
              GetAndDisplayImage(imagenameArray[pDlgOK.SelectedId - 1].ToString(),
                                 imagedescArray[pDlgOK.SelectedId - 1].ToString());
            }
          }
        }
        else
        {
          GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlg.SetHeading(GUILocalizeStrings.Get(257)); // Error
          dlg.SetLine(1, GUILocalizeStrings.Get(2508)); // No Images referenced in this article.
          dlg.DoModal(GUIWindowManager.ActiveWindow);
        }
      }
      // Back to the text button to switch from image view
      if (control == buttonBack)
      {
        if (!txtArticle.IsVisible)
        {
          GUIControl.ShowControl(GetID, txtArticle.GetID);
        }
        if (imageControl.IsVisible)
        {
          GUIControl.HideControl(GetID, imageControl.GetID);
        }
        if (!searchtermLabel.IsVisible)
        {
          GUIControl.ShowControl(GetID, searchtermLabel.GetID);
        }
        if (imagedescLabel.IsVisible)
        {
          GUIControl.HideControl(GetID, imagedescLabel.GetID);
        }
        if (buttonBack.IsVisible)
        {
          GUIControl.HideControl(GetID, buttonBack.GetID);
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

    // Depending on which Entry was selected from the listbox we chose the language here.
    private void SelectLocalWikipedia(int labelnumber, String[] siteArray)
    {
      Settings langreader = new Settings(Config.GetFile(Config.Dir.Config, "wikipedia.xml"));
      language = siteArray[labelnumber];

      if (searchtermLabel.Label != string.Empty && searchtermLabel.Label != "Wikipedia")
      {
        Log.Info("Wikipedia: language changed to {0}. Display article {1} again.", language, searchtermLabel.Label);
        GetAndDisplayArticle(searchtermLabel.Label);
      }
    }

    private void GetAndDisplayImage(string imagename, string imagedesc)
    {
      WikipediaImage image = new WikipediaImage(imagename, language);
      string imagefilename = image.GetImageFilename();
      Log.Info("Wikipedia: Trying to display image file: {0}", imagefilename);

      if (imagefilename != string.Empty && File.Exists(imagefilename))
      {
        if (txtArticle.IsVisible)
        {
          GUIControl.HideControl(GetID, txtArticle.GetID);
        }
        if (!imageControl.IsVisible)
        {
          GUIControl.ShowControl(GetID, imageControl.GetID);
        }
        if (searchtermLabel.IsVisible)
        {
          GUIControl.HideControl(GetID, searchtermLabel.GetID);
        }
        if (!imagedescLabel.IsVisible)
        {
          GUIControl.ShowControl(GetID, imagedescLabel.GetID);
        }
        if (!buttonBack.IsVisible)
        {
          GUIControl.ShowControl(GetID, buttonBack.GetID);
        }
        imagedescLabel.Label = imagedesc;
        imageControl.SetFileName(imagefilename);
      }
      else
      {
        GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        dlg.SetHeading(GUILocalizeStrings.Get(257)); // Error
        dlg.SetLine(1, GUILocalizeStrings.Get(2512)); // Can't display image.
        dlg.SetLine(2, GUILocalizeStrings.Get(2513)); // Please have a look at the logfile.
        dlg.DoModal(GUIWindowManager.ActiveWindow);
      }
    }

    // The main function.
    private void GetAndDisplayArticle(string searchterm)
    {
      BackgroundWorker worker = new BackgroundWorker();

      worker.DoWork += new DoWorkEventHandler(DownloadWorker);
      worker.RunWorkerAsync(searchterm);

      while (_workerCompleted == false)
      {
        GUIWindowManager.Process();
      }
    }

    // All kind of stuff because of the wait cursor ;-)
    private void DownloadWorker(object sender, DoWorkEventArgs e)
    {
      Thread.CurrentThread.Name = "Wikipedia";
      _workerCompleted = false;

      using (WaitCursor cursor = new WaitCursor())
      {
        lock (this)
        {
          if (!txtArticle.IsVisible)
          {
            GUIControl.ShowControl(GetID, txtArticle.GetID);
          }
          if (imageControl.IsVisible)
          {
            GUIControl.HideControl(GetID, imageControl.GetID);
          }
          if (!searchtermLabel.IsVisible)
          {
            GUIControl.ShowControl(GetID, searchtermLabel.GetID);
          }
          if (imagedescLabel.IsVisible)
          {
            GUIControl.HideControl(GetID, imagedescLabel.GetID);
          }
          if (buttonBack.IsVisible)
          {
            GUIControl.HideControl(GetID, buttonBack.GetID);
          }
          linkArray.Clear();
          imagenameArray.Clear();
          imagedescArray.Clear();
          searchtermLabel.Label = e.Argument.ToString();
          WikipediaArticle article = new WikipediaArticle(e.Argument.ToString(), language);
          articletext = article.GetArticleText();
          linkArray = article.GetLinkArray();
          imagenameArray = article.GetImageArray();
          imagedescArray = article.GetImagedescArray();
          language = article.GetLanguage();

          if (articletext == "REDIRECT")
          {
            txtArticle.Label = GUILocalizeStrings.Get(2509) + "\n" + GUILocalizeStrings.Get(2510);
            //This page is only a redirect. Please chose the redirect aim from the link list.
          }
          else if (articletext == string.Empty)
          {
            txtArticle.Label = GUILocalizeStrings.Get(2504); //Sorry, no Article was found for your searchterm...
          }
          else
          {
            txtArticle.Label = articletext;
          }
        }
      }

      _workerCompleted = true;
    }

    private volatile bool _workerCompleted = true;
  }
}