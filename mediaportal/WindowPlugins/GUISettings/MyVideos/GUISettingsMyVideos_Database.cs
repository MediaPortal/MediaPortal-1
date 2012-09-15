#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using CSScriptLibrary;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  public class GUISettingsMoviesDatabase : GUIInternalWindow, IMDB.IProgress
  {
    // Grabbers
    [SkinControl(2)] protected GUIButtonControl btnGrabber= null; 
    [SkinControl(3)] protected GUIButtonControl btnUpdateGrabbers = null; 
    // Scan options
    [SkinControl(4)] protected GUICheckButton btnNearestmatch = null; 
    [SkinControl(5)] protected GUICheckButton btnUsefanart = null; 
    [SkinControl(7)] protected GUIButtonControl btnActorsFetchSize = null; 
    [SkinControl(8)] protected GUICheckButton btnFoldernamefortitle= null;
    [SkinControl(9)] protected GUICheckButton btnPrefervideofilename = null;
    
    [SkinControl(10)] protected GUIListControl lcFolders = null;

    [SkinControl(11)] protected GUICheckButton btnSkipalreadyexisting = null; 
    [SkinControl(12)] protected GUICheckButton btnRefreshexistingonly = null; 
    [SkinControl(13)] protected GUICheckButton btnStripprefix= null;
    // Scan
    [SkinControl(14)] protected GUIButtonControl btnScandatabase = null;
    [SkinControl(15)] protected GUIButtonControl btnResetdatabase= null;
    [SkinControl(16)] protected GUICheckButton btnUseSortTitle = null;
    [SkinControl(17)] protected GUICheckButton btnUseNfoScraper = null;

    private String _defaultShare;
    private bool _rememberLastFolder;
    private bool _addOpticalDiskDrives ;
    private bool _autoSwitchRemovableDrives;

    private enum Controls
    {
      CONTROL_FANARTCOUNT= 6
    } ;
    

    // grabber index holds information/urls of available grabbers to download
    private string _grabberIndexFile = Config.GetFile(Config.Dir.Config, "MovieInfoGrabber.xml");
    private string _grabberIndexUrl = @"http://install.team-mediaportal.com/MP1/MovieInfoGrabber.xml";
    private Dictionary<string, IIMDBScriptGrabber> _grabberList;

    private int m_iCount = 1;
    private string _prefixes = string.Empty;
    private int _scanShare = 0;
    private ArrayList _conflictFiles = new ArrayList();
    private bool _scanning = false;
    private int _scanningFileNumber = 1;
    private int _scanningFileTotal = 1;
    private string _actorsFetchSize = "Short";

    private ShareData FolderInfo(GUIListItem item)
    {
      ShareData folderInfo = item.AlbumInfoTag as ShareData;
      return folderInfo;
    }

    public GUISettingsMoviesDatabase()
    {
      GetID = (int)Window.WINDOW_SETTINGS_VIDEODATABASE; //1010
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_MyVideos_Database.xml"));
    }

    // Need change for 1.3.0
    #region Serialization

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        // Automatch
        btnNearestmatch.Selected = xmlreader.GetValueAsBool("movies", "fuzzyMatching", true);
        
        string configDir;
        FanArt.GetFanArtFolder(out configDir);
        
        if (Directory.Exists(configDir))
        {
          // FanArt setting
          btnUsefanart.Selected = xmlreader.GetValueAsBool("moviedatabase", "usefanart", false);

          if (btnUsefanart.Selected)
          {
            GUIControl.EnableControl(GetID, (int)Controls.CONTROL_FANARTCOUNT);
          }
          else
          {
            GUIControl.DisableControl(GetID, (int)Controls.CONTROL_FANARTCOUNT);
          }
        }
        else
        {
          btnUsefanart.IsEnabled = false;
          btnUsefanart.Selected = false;
          GUIControl.DisableControl(GetID, (int)Controls.CONTROL_FANARTCOUNT);
        }
        
        m_iCount = xmlreader.GetValueAsInt("moviedatabase", "fanartnumber", 0);
        _actorsFetchSize = xmlreader.GetValueAsString("moviedatabase", "actorslistsize", "Short");
        
        // Folder names as title
        btnFoldernamefortitle.Selected = xmlreader.GetValueAsBool("moviedatabase", "usefolderastitle", false);
        if (btnFoldernamefortitle.Selected)
        {
          btnPrefervideofilename.IsEnabled = true;
        }
        else
        {
          btnPrefervideofilename.IsEnabled = false;
        }
        btnPrefervideofilename.Selected= xmlreader.GetValueAsBool("moviedatabase", "preferfilenameforsearch", false);
        // Skip existing
        btnSkipalreadyexisting.Selected =  xmlreader.GetValueAsBool("moviedatabase", "scanskipexisting", true);
        // Prefixes
        btnStripprefix.Selected = xmlreader.GetValueAsBool("moviedatabase", "striptitleprefixes", false);
        _prefixes = xmlreader.GetValueAsString("moviedatabase", "titleprefixes", "The, Les, Die");
        // Load share settings
        lcFolders.Clear();
        _scanShare = 0;
        SettingsSharesHelper settingsSharesHelper = new SettingsSharesHelper();
        
        settingsSharesHelper.LoadSettings("movies");

        foreach (GUIListItem item in settingsSharesHelper.ShareListControl)
        {
          string driveLetter = FolderInfo(item).Folder.Substring(0, 3).ToUpper();
          
          if (Util.Utils.getDriveType(driveLetter) == 3 ||
              Util.Utils.getDriveType(driveLetter) == 4)
          {
            item.IsPlayed = false;
            
            if (FolderInfo(item).ScanShare)
            {
              item.IsPlayed = true;
              item.Label2 = GUILocalizeStrings.Get(193);
              _scanShare++;
            }
            item.OnItemSelected += OnItemSelected;
            item.Label = FolderInfo(item).Folder;

            item.Path = FolderInfo(item).Folder;
            lcFolders.Add(item);
          }
        }
        _defaultShare = xmlreader.GetValueAsString("movies", "default", "");
        _rememberLastFolder = xmlreader.GetValueAsBool("movies", "rememberlastfolder", false);
        _addOpticalDiskDrives = xmlreader.GetValueAsBool("movies", "AddOpticalDiskDrives", true);
        _autoSwitchRemovableDrives = xmlreader.GetValueAsBool("movies", "SwitchRemovableDrives", true);

        btnUseSortTitle.Selected = xmlreader.GetValueAsBool("moviedatabase", "usesorttitle", false);
        btnUseNfoScraper.Selected = xmlreader.GetValueAsBool("moviedatabase", "useonlynfoscraper", false);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("movies", "fuzzyMatching", btnNearestmatch.Selected);
        // FanArt
        xmlwriter.SetValueAsBool("moviedatabase", "usefanart", btnUsefanart.Selected);
        xmlwriter.SetValue("moviedatabase", "fanartnumber", m_iCount);
        
        // Folder movie title
        xmlwriter.SetValueAsBool("moviedatabase", "usefolderastitle", btnFoldernamefortitle.Selected);
        xmlwriter.SetValueAsBool("moviedatabase", "preferfilenameforsearch", btnPrefervideofilename.Selected);

        // Strip movie title prefix
        xmlwriter.SetValueAsBool("moviedatabase", "striptitleprefixes", btnStripprefix.Selected);
        xmlwriter.SetValue("moviedatabase", "titleprefixes", _prefixes);

        // Database
        xmlwriter.SetValueAsBool("moviedatabase", "scanskipexisting", btnSkipalreadyexisting.Selected);
        // Actors fetch size
        xmlwriter.SetValue("moviedatabase", "actorslistsize", _actorsFetchSize);
        // SortTitle
        xmlwriter.SetValueAsBool("moviedatabase", "usesorttitle", btnUseSortTitle.Selected);
        // nfo scraper only
        xmlwriter.SetValueAsBool("moviedatabase", "useonlynfoscraper", btnUseNfoScraper.Selected);
        
        SettingsSharesHelper settingsSharesHelper = new SettingsSharesHelper();
        settingsSharesHelper.ShareListControl = lcFolders.ListItems;
        
        settingsSharesHelper.RememberLastFolder = _rememberLastFolder;
        settingsSharesHelper.AddOpticalDiskDrives = _addOpticalDiskDrives;
        settingsSharesHelper.SwitchRemovableDrives = _autoSwitchRemovableDrives;
        settingsSharesHelper.DefaultShare = _defaultShare;
        
        settingsSharesHelper.SaveSettings("movies");
      }
    }

    #endregion

    #region Overrides

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            LoadSettings();
            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_FANARTCOUNT);
            for (int i = 1; i <= 5; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_FANARTCOUNT, i.ToString());
            }

            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_FANARTCOUNT, m_iCount - 1);

            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.CONTROL_FANARTCOUNT)
            {
              string strLabel = message.Label;
              m_iCount = Int32.Parse(strLabel);
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101010));
      LoadSettings();
      SetProperties();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == lcFolders)
      {
        if (lcFolders.SelectedListItem.IsPlayed)
        {
          lcFolders.SelectedListItem.Label2 = "";
          lcFolders.SelectedListItem.IsPlayed = false;
          FolderInfo(lcFolders.SelectedListItem).ScanShare = false;
          _scanShare--;
        }
        else
        {
          lcFolders.SelectedListItem.Label2 = GUILocalizeStrings.Get(193);
          lcFolders.SelectedListItem.IsPlayed = true;
          FolderInfo(lcFolders.SelectedListItem).ScanShare = true;
          _scanShare++;
        }
      }

      if (control == btnGrabber)
      {
        OnSetDefaultGrabber();
      }

      if (control == btnUpdateGrabbers)
      {
        OnUpdateGrabberScripts();
      }

      if (control == btnScandatabase)
      {
        if (_scanShare == 0)
        {
          GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(GUILocalizeStrings.Get(1020)); //Information
          dlgOk.SetLine(1, GUILocalizeStrings.Get(300004)); //Nothing to scan
          dlgOk.SetLine(2, GUILocalizeStrings.Get(300005)); //please select folder(s) to scan
          dlgOk.DoModal(GetID);
          return;
        }
        OnScanDatabase();
      }

      if (control == btnUsefanart)
      {
        if (btnUsefanart.Selected)
        {
          //btnUsefanartonshare.IsEnabled = true;
          GUIControl.EnableControl(GetID, (int)Controls.CONTROL_FANARTCOUNT);
        }
        else
        {
          //btnUsefanartonshare.IsEnabled = false;
          GUIControl.DisableControl(GetID, (int)Controls.CONTROL_FANARTCOUNT);
        }
      }

      if (control == btnActorsFetchSize)
      {
        OnActorsFetchSize();
      }

      if (control == btnFoldernamefortitle)
      {
        if (btnFoldernamefortitle.Selected)
        {
          btnPrefervideofilename.IsEnabled = true;
        }
        else
        {
          btnPrefervideofilename.IsEnabled = false;
        }
      }

      if (control == btnSkipalreadyexisting)
      {
        if (btnSkipalreadyexisting.Selected)
        {
          btnRefreshexistingonly.IsEnabled = true;
        }
        else
        {
          btnRefreshexistingonly.IsEnabled = false;
        }
      }

      if (control == btnStripprefix)
      {
        if (btnStripprefix.Selected)
        {
          OnStripPrefixes();
        }
      }

      if (control == btnResetdatabase)
      {
        OnResetDatabase();
      }

    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    #endregion

    private void OnUpdateGrabberScripts()
    {
      // Check Internet connection
      if (!Win32API.IsConnectedToInternet())
      {
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        dlgOk.SetHeading(257);
        dlgOk.SetLine(1, GUILocalizeStrings.Get(703));
        dlgOk.DoModal(GetID);
        return;
      }

      // Initialize progress bar
      GUIDialogProgress progressDialog =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      progressDialog.Reset();
      progressDialog.SetHeading(GUILocalizeStrings.Get(300030)); // Updating grabbers
      progressDialog.ShowProgressBar(true);
      progressDialog.SetLine(1, GUILocalizeStrings.Get(300031)); //Downloading index file
      progressDialog.SetLine(2, GUILocalizeStrings.Get(300032)); //Downloading
      progressDialog.SetPercentage(100);
      progressDialog.StartModal(GetID);

      if (DownloadFile(_grabberIndexFile, _grabberIndexUrl) == false)
      {
        progressDialog.Close();
        return;
      }

      string parserIndexFile = Config.GetFile(Config.Dir.Config, "scripts\\VDBParserStrings.xml");
      string parserIndexUrl = @"http://install.team-mediaportal.com/MP1/VDBParserStrings.xml";
      string internalGrabberScriptFile = Config.GetFile(Config.Dir.Config, "scripts\\InternalActorMoviesGrabber.csscript");
      string internalGrabberScriptUrl = @"http://install.team-mediaportal.com/MP1/InternalGrabber/InternalActorMoviesGrabber.csscript";

      // VDB parser update
      progressDialog.SetHeading("Updating VDBparser file......");
      progressDialog.ShowProgressBar(true);
      progressDialog.SetLine(1, "Downloading VDBparser file...");
      progressDialog.SetLine(2, "Downloading...");
      progressDialog.SetPercentage(75);
      progressDialog.StartModal(GUIWindowManager.ActiveWindow);

      if (DownloadFile(parserIndexFile, parserIndexUrl) == false)
      {
        progressDialog.Close();
        return;
      }

      // Internal grabber script update
      progressDialog.SetHeading("Updating InternalGrabberScript file......");
      progressDialog.ShowProgressBar(true);
      progressDialog.SetLine(1, "Downloading InternalGrabberScript file...");
      progressDialog.SetLine(2, "Downloading...");
      progressDialog.SetPercentage(100);
      progressDialog.StartModal(GUIWindowManager.ActiveWindow);

      if (DownloadFile(internalGrabberScriptFile, internalGrabberScriptUrl) == false)
      {
        progressDialog.Close();
        return;
      }

      // For 1.3.0B
      IMDB.InternalMovieInfoScraper internalGrabber = new IMDB.InternalMovieInfoScraper();
      internalGrabber.LoadScript();

      // read index file
      if (!File.Exists(_grabberIndexFile))
      {
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        dlgOk.SetHeading(257);
        dlgOk.SetLine(1, GUILocalizeStrings.Get(300033)); // No grabber index found
        dlgOk.DoModal(GetID);
        progressDialog.Close();
        return;
      }
      XmlDocument doc = new XmlDocument();
      doc.Load(_grabberIndexFile);
      XmlNodeList sectionNodes = doc.SelectNodes("MovieInfoGrabber/grabber");

      // download all grabbers
      int percent = 0;
      if (sectionNodes != null)
      {
        for (int i = 0; i < sectionNodes.Count; i++)
        {
          if (progressDialog.IsCanceled)
          {
            break;
          }

          string url = sectionNodes[i].Attributes["url"].Value;
          string id = Path.GetFileName(url);
          progressDialog.SetLine(1, GUILocalizeStrings.Get(300034) + id); //Downloading grabber:
          progressDialog.SetLine(2, GUILocalizeStrings.Get(300035)); // Processing grabbers
          progressDialog.SetPercentage(percent);
          //progressDialog.Count = i + 1;
          percent += 100 / (sectionNodes.Count - 1);
          progressDialog.Progress();

          if (DownloadFile(IMDB.ScriptDirectory + @"\" + id, url) == false)
          {
            progressDialog.Close();
            return;
          }
        }
      }
      progressDialog.Close();
    }

    private bool DownloadFile(string filepath, string url)
    {
      string grabberTempFile = Path.GetTempFileName();

      try
      {
        if (File.Exists(grabberTempFile))
        {
          File.Delete(grabberTempFile);
        }

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // request.Proxy = WebProxy.GetDefaultProxy();
          request.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) { }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          using (Stream resStream = response.GetResponseStream())
          {
            using (TextReader tin = new StreamReader(resStream, Encoding.Default))
            {
              using (TextWriter tout = File.CreateText(grabberTempFile))
              {
                while (true)
                {
                  string line = tin.ReadLine();
                  if (line == null)
                  {
                    break;
                  }
                  tout.WriteLine(line);
                }
              }
            }
          }
        }

        File.Delete(filepath);
        File.Move(grabberTempFile, filepath);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("EXCEPTION in DownloadFile | {0}\r\n{1}", ex.Message, ex.Source);
        return false;
      }
    }
    
    private void OnSetDefaultGrabber()
    {
      // read index file
      if (!File.Exists(_grabberIndexFile))
      {
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        dlgOk.SetHeading(257);
        dlgOk.SetLine(1, GUILocalizeStrings.Get(300033)); // No grabber index found
        dlgOk.DoModal(GetID);
        return;
      }

      GUIWaitCursor.Show();
      GetGrabbers();

      string defaultDatabase = string.Empty;
      int defaultIndex = 0;
      int dbNumber;

      using (MediaPortal.Profile.Settings xmlreader = new MPSettings())
      {
        defaultDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + 0, "IMDB");
        dbNumber = xmlreader.GetValueAsInt("moviedatabase", "number", 0);
      }

      // Dialog menu with grabbers
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(300036); // Set default Grabber script

      foreach (KeyValuePair<string, IIMDBScriptGrabber> grabber in _grabberList)
      {
        dlg.Add(grabber.Value.GetName() + " - " + grabber.Value.GetLanguage());

        if (defaultDatabase == grabber.Key)
        {
          dlg.SelectedLabel = defaultIndex;
        }
        else
        {
          defaultIndex++;
        }
      }

      GUIWaitCursor.Hide();

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }

      using (MediaPortal.Profile.Settings xmlwriter = new MPSettings())
      {
        KeyValuePair<string, IIMDBScriptGrabber> grabber = _grabberList.ElementAt(dlg.SelectedLabel);


        if (grabber.Key != "IMDB")
        {
          if (dbNumber == 0)
          {
            dbNumber = 1;
          }
          xmlwriter.SetValue("moviedatabase", "number", dbNumber);
          xmlwriter.SetValue("moviedatabase", "database" + 0, grabber.Key);
          xmlwriter.SetValue("moviedatabase", "title" + 0, grabber.Value.GetName());
          xmlwriter.SetValue("moviedatabase", "language" + 0, grabber.Value.GetLanguage());
          xmlwriter.SetValue("moviedatabase", "limit" + 0, 25);
        }
        else
        {
          for (int i = 0; i < 4; i++)
          {
            xmlwriter.SetValue("moviedatabase", "number", 0);
            xmlwriter.RemoveEntry("moviedatabase", "database" + i);
            xmlwriter.RemoveEntry("moviedatabase", "title" + i);
            xmlwriter.RemoveEntry("moviedatabase", "language" + i);
            xmlwriter.RemoveEntry("moviedatabase", "limit" + i);
          }
        }
      }
    }

    private void GetGrabbers()
    {
      // Initialize progress bar
      GUIDialogProgress progressDialog =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      progressDialog.Reset();
      progressDialog.SetHeading(GUILocalizeStrings.Get(300037)); // Compiling..
      progressDialog.ShowProgressBar(true);
      progressDialog.SetLine(1, "");
      progressDialog.SetPercentage(0);
      progressDialog.StartModal(GetID);

      int percent = 0;

      _grabberList = new Dictionary<string, IIMDBScriptGrabber>();

      Directory.CreateDirectory(IMDB.ScriptDirectory);
      DirectoryInfo di = new DirectoryInfo(IMDB.ScriptDirectory);

      FileInfo[] fileList = di.GetFiles("*.csscript", SearchOption.AllDirectories);

      foreach (FileInfo f in fileList)
      {
        try
        {
          progressDialog.SetLine(1, f.Name);
          progressDialog.SetPercentage(percent);
          Application.DoEvents();

          AsmHelper script = new AsmHelper(CSScript.Load(f.FullName, null, false));
          IIMDBScriptGrabber grabber = (IIMDBScriptGrabber)script.CreateObject("Grabber");

          percent += 100 / (fileList.Count() - 1);
          progressDialog.Progress();
          Application.DoEvents();

          _grabberList.Add(Path.GetFileNameWithoutExtension(f.FullName), grabber);
        }
        catch (Exception ex)
        {
          Log.Error("Script grabber error file: {0}, message : {1}", f.FullName, ex.Message);
        }
      }

      progressDialog.Close();
    }

    private void OnStripPrefixes()
    {
      string txt = _prefixes;
      GetStringFromKeyboard(ref txt);

      if (!string.IsNullOrEmpty(txt))
      {
        _prefixes = txt;
      }
      SetProperties();
    }

    private void OnActorsFetchSize()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu

        dlg.AddLocalizedString(300209); // Short
        dlg.AddLocalizedString(300210); // Long

        if (_actorsFetchSize == "Short")
        {
          dlg.SelectedLabel = 0;
        }
        else
        {
          dlg.SelectedLabel = 1;
        }

        dlg.DoModal(GetID);

        if (dlg.SelectedId == -1)
        {
          return;
        }

        if (dlg.SelectedLabel == 0)
        {
          _actorsFetchSize = "Short";
        }
        else
        {
          _actorsFetchSize = "Long";
        }
      }

    }

    // need change for 1.3.0
    private void OnScanDatabase()
    {
      SaveSettings();
      
      ArrayList availablePaths = new ArrayList();
      ArrayList scanShares = new ArrayList();

      foreach (GUIListItem item in lcFolders.ListItems)
      {
        if (item.IsPlayed)
        {
          scanShares.Add(item);
        }
      }

      for (int index = 0; index < _scanShare; index++)
      {
        GUIListItem item = (GUIListItem)scanShares[index];
        string path = item.Path;
        availablePaths.Add(path);
      }
      
      // Clean covers and fanarts (only if refreshexisting cb is checked)
      if (btnRefreshexistingonly.Selected)
      {
        ArrayList movies = new ArrayList();
        VideoDatabase.GetMovies(ref movies);

        foreach (IMDBMovie movie in movies)
        {
          string strFilenameAndPath = string.Empty;

          ArrayList files = new ArrayList();
          VideoDatabase.GetFilesForMovie(movie.ID, ref files);
          strFilenameAndPath = files[0].ToString();
          
          // Delete covers
          FanArt.DeleteCovers(movie.Title, movie.ID);
          // Delete fanarts
          FanArt.DeleteFanarts(movie.ID);
        }
      }
      _conflictFiles = new ArrayList();

      if (!btnUseNfoScraper.Selected)
      {
        IMDBFetcher.ScanIMDB(this, availablePaths, btnNearestmatch.Selected, btnSkipalreadyexisting.Selected, false,
                             btnRefreshexistingonly.Selected);
      }
      else
      {
        ArrayList nfoFiles = new ArrayList();

        foreach (string availablePath in availablePaths)
        {
          GetNfoFiles(availablePath, ref nfoFiles);
        }

        IMDBFetcher fetcher = new IMDBFetcher(this);
        fetcher.FetchNfo(nfoFiles, btnSkipalreadyexisting.Selected, btnRefreshexistingonly.Selected);
      }
    }

    private void OnResetDatabase()
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(927));
      dlgYesNo.SetLine(1, GUILocalizeStrings.Get(300038)); // Are you sure you want to delete....
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }
      string database = Config.GetFile(Config.Dir.Database, "VideoDatabaseV5.db3");
      if (File.Exists(database))
      {
        VideoDatabase.Dispose();
        try
        {
          File.Delete(database);
          // FanArt delete all files
          string configDir;
          FanArt.GetFanArtFolder(out configDir);
          if (btnUsefanart.Selected)
          {
            dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo)
            {
              return;
            }
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(927));
            dlgYesNo.SetLine(1, GUILocalizeStrings.Get(300039)); // Delete all fanarts...
            dlgYesNo.DoModal(GetID);

            if (!dlgYesNo.IsConfirmed)
            {
              return;
            }
            string files = @"*.jpg"; // Only delete jpg files
            string[] fileList = Directory.GetFiles(configDir, files);
            foreach (string file in fileList)
            {
              File.Delete(file);
            }
          }
        }
        catch (Exception)
        {
          GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(GUILocalizeStrings.Get(257));
          dlgOk.SetLine(1, GUILocalizeStrings.Get(300040)); // VDB can't be cleared
          dlgOk.DoModal(GUIWindowManager.ActiveWindow);
          return;
        }
        finally
        {
          VideoDatabase.ReOpen();
        }
      }
        
      GUIDialogNotify dlgNotify =
      (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (null != dlgNotify)
      {
        dlgNotify.SetHeading(GUILocalizeStrings.Get(1020)); // Information
        dlgNotify.SetText(GUILocalizeStrings.Get(300041)); // VDB cleared successful...
        dlgNotify.DoModal(GetID);
      }
    }

    private bool GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.Reset();
      keyboard.Text = strLine;

      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
      return true;
    }

    private void SetProperties()
    {
      GUIPropertyManager.SetProperty("#prefixes", _prefixes);
    }
    
    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item != null)
      {

      }
    }

    private void GetNfoFiles(string path, ref ArrayList nfoFiles)
    {
      string[] files = Directory.GetFiles(path, "*.nfo", SearchOption.AllDirectories);
      var sortedFiles = files.OrderBy(f => f);

      foreach (string file in sortedFiles)
      {
        nfoFiles.Add(file);
      }
    }

    // Need change fore 1.3.0
    #region IMDB.IProgress

    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if (pDlgProgress.IsInstance(fetcher))
      {
        pDlgProgress.DisableCancel(true);
      }
      return true;
    }

    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      if (!GUIWindowManager.IsRouted)
      {
        return;
      }
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.ShowProgressBar(true);
      pDlgProgress.SetLine(1, line1);
      pDlgProgress.SetLine(2, line2);
      if (percent > 0)
      {
        pDlgProgress.SetPercentage(percent);
      }
      pDlgProgress.Progress();
    }

    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're busy querying www.imdb.com
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(197), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(197);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnSearchStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnSearchEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        return false;
      }
      else
      {
        // show dialog...
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        pDlgOK.SetHeading(195);
        pDlgOK.SetLine(1, fetcher.MovieName);
        pDlgOK.SetLine(2, string.Empty);
        pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
        return true;
      }
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the movie info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(198), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(198);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      //pDlgProgress.SetLine(0, strMovieName);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnDetailsEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the actor info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(986), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(986);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnActorInfoStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the actor info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(986), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(1302);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      //pDlgProgress.SetLine(0, strMovieName);
      pDlgProgress.SetLine(1, fetcher.ActorName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnActorsEnd(IMDBFetcher fetcher)
    {
      return true;
    }

    public bool OnDetailsNotFound(IMDBFetcher fetcher)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        return false;
      }
      else
      {
        // show dialog...
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        // show dialog...
        pDlgOK.SetHeading(195);
        pDlgOK.SetLine(1, fetcher.MovieName);
        pDlgOK.SetLine(2, string.Empty);
        pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
        return false;
      }
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        movieName = string.Empty;
        return false;
      }
      else
      {
        movieName = fetcher.MovieName;
        if (GetStringFromKeyboard(ref movieName))
        {
          if (movieName == string.Empty)
          {
            return false;
          }
          return true;
        }
        movieName = string.Empty;
        return false;
      }
    }

    public bool OnSelectMovie(IMDBFetcher fetcher, out int selectedMovie)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        selectedMovie = -1;
        return false;
      }
      else
      {
        GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
        // more then 1 movie found
        // ask user to select 1
        pDlgSelect.Reset();
        pDlgSelect.SetHeading(196); //select movie
        for (int i = 0; i < fetcher.Count; ++i)
        {
          pDlgSelect.Add(fetcher[i].Title);
        }
        pDlgSelect.EnableButton(true);
        pDlgSelect.SetButtonLabel(413); // manual
        pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

        // and wait till user selects one
        selectedMovie = pDlgSelect.SelectedLabel;
        if (pDlgSelect.IsButtonPressed)
        {
          return true;
        }
        if (selectedMovie == -1)
        {
          return false;
        }
        else
        {
          return true;
        }
      }
    }

    public bool OnSelectActor(IMDBFetcher fetcher, out int selectedActor)
    {
      GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      // more then 1 actor found
      // ask user to select 1
      pDlgSelect.SetHeading("Select actor:"); //select actor
      pDlgSelect.Reset();
      for (int i = 0; i < fetcher.Count; ++i)
      {
        pDlgSelect.Add(fetcher[i].Title);
      }
      pDlgSelect.EnableButton(false);
      pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

      // and wait till user selects one
      selectedActor = pDlgSelect.SelectedLabel;
      if (selectedActor != -1)
      {
        return true;
      }
      return false;
    }

    public bool OnScanStart(int total)
    {
      _scanning = true;
      _conflictFiles.Clear();
      _scanningFileTotal = total;
      _scanningFileNumber = 1;
      return true;
    }

    public bool OnScanEnd()
    {
      _scanning = false;
      if (_conflictFiles.Count > 0)
      {
        GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
        // more than 1 movie found
        // ask user to select 1
        do
        {
          pDlgSelect.Reset();
          pDlgSelect.SetHeading(892); //select movie
          for (int i = 0; i < _conflictFiles.Count; ++i)
          {
            IMDBMovie currentMovie = (IMDBMovie)_conflictFiles[i];
            string strFileName = string.Empty;
            string path = currentMovie.Path;
            string filename = currentMovie.File;
            if (path != string.Empty)
            {
              if (path.EndsWith(@"\"))
              {
                path = path.Substring(0, path.Length - 1);
                currentMovie.Path = path;
              }
              if (filename.StartsWith(@"\"))
              {
                filename = filename.Substring(1);
                currentMovie.File = filename;
              }
              strFileName = path + @"\" + filename;
            }
            else
            {
              strFileName = filename;
            }
            pDlgSelect.Add(strFileName);
          }
          pDlgSelect.EnableButton(true);
          pDlgSelect.SetButtonLabel(4517); // manual
          pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

          // and wait till user selects one
          int selectedMovie = pDlgSelect.SelectedLabel;
          if (pDlgSelect.IsButtonPressed)
          {
            break;
          }
          if (selectedMovie == -1)
          {
            break;
          }
          IMDBMovie movieDetails = (IMDBMovie)_conflictFiles[selectedMovie];
          string searchText = movieDetails.Title;
          if (searchText == string.Empty)
          {
            searchText = movieDetails.SearchString;
          }
          if (GetStringFromKeyboard(ref searchText))
          {
            if (searchText != string.Empty)
            {
              movieDetails.SearchString = searchText;
              if (IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, false, false))
              {
                if (movieDetails != null)
                {
                  _conflictFiles.RemoveAt(selectedMovie);
                }
              }
            }
          }
        } while (_conflictFiles.Count > 0);
      }
      return true;
    }

    public bool OnScanIterating(int count)
    {
      _scanningFileNumber = count;
      return true;
    }

    public bool OnScanIterated(int count)
    {
      _scanningFileNumber = count;
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    #endregion
  }
}
